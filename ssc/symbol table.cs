using arookas.Collections;
using arookas.IO.Binary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunSymbolTable : IEnumerable<sunSymbol> {
		List<sunSymbol> Symbols { get; set; }

		public int Count { get { return Symbols.Count; } }
		public int CallableCount { get { return Callables.Count(); } }
		public int BuiltinCount { get { return Builtins.Count(); } }
		public int FunctionCount { get { return Functions.Count(); } }
		public int StorableCount { get { return Storables.Count(); } }
		public int VariableCount { get { return Variables.Count(); } }
		public int ConstantCount { get { return Constants.Count(); } }

		public IEnumerable<sunCallableSymbol> Callables { get { return Symbols.OfType<sunCallableSymbol>(); } }
		public IEnumerable<sunBuiltinSymbol> Builtins { get { return Symbols.OfType<sunBuiltinSymbol>(); } }
		public IEnumerable<sunFunctionSymbol> Functions { get { return Symbols.OfType<sunFunctionSymbol>(); } }
		public IEnumerable<sunStorableSymbol> Storables { get { return Symbols.OfType<sunStorableSymbol>(); } }
		public IEnumerable<sunVariableSymbol> Variables { get { return Symbols.OfType<sunVariableSymbol>(); } }
		public IEnumerable<sunConstantSymbol> Constants { get { return Symbols.OfType<sunConstantSymbol>(); } }

		public sunSymbolTable() {
			Symbols = new List<sunSymbol>(10);
		}

		public void Add(sunSymbol symbol) { Symbols.Add(symbol); }
		public void Clear() { Symbols.Clear(); }

		public IEnumerator<sunSymbol> GetEnumerator() { return Symbols.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunSymbol {
		public string Name { get; private set; }

		// symbol table
		public abstract sunSymbolType Type { get; }
		public abstract uint Data { get; }

		protected sunSymbol(string name) {
			Name = name;
		}

		public abstract void Compile(sunCompiler compiler);
	}

	abstract class sunCallableSymbol : sunSymbol {
		public sunParameterInfo Parameters { get; private set; }
		protected List<sunPoint> CallSites { get; private set; }

		public bool HasCallSites { get { return CallSites.Count > 0; } }

		protected sunCallableSymbol(string name, sunParameterInfo parameterInfo)
			: base(name) {
			Parameters = parameterInfo;
			CallSites = new List<sunPoint>(10);
		}

		public abstract void OpenCallSite(sunCompiler compiler, int argumentCount);
		public abstract void CloseCallSites(sunCompiler compiler);
	}

	class sunBuiltinSymbol : sunCallableSymbol {
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Builtin; } }
		public override uint Data { get { return (uint)Index; } }

		public sunBuiltinSymbol(string name, int index)
			: base(name, null) {
			Index = index;
		}
		public sunBuiltinSymbol(string name, sunParameterInfo parameters, int index)
			: base(name, parameters) {
			Index = index;
		}

		public override void Compile(sunCompiler compiler) {
			// don't compile builtins
		}
		public override void OpenCallSite(sunCompiler compiler, int argumentCount) {
			compiler.Binary.WriteFUNC(Index, argumentCount);
		}
		public override void CloseCallSites(sunCompiler compiler) { }
	}

	class sunFunctionSymbol : sunCallableSymbol {
		sunNode Body { get; set; }
		public uint Offset { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Function; } }
		public override uint Data { get { return (uint)Offset; } }

		public sunFunctionSymbol(string name, sunParameterInfo parameters, sunNode body)
			: base(name, parameters) {
			Body = body;
		}

		public override void Compile(sunCompiler compiler) {
			Offset = compiler.Binary.Offset;
			compiler.Context.Scopes.Push(sunScopeType.Function);
			compiler.Context.Scopes.ResetLocalCount();
			foreach (var parameter in Parameters) {
				compiler.Context.Scopes.DeclareVariable(parameter); // since there is no AST node for these, they won't affect MaxLocalCount
			}
			compiler.Binary.WriteMKDS(1);
			compiler.Binary.WriteMKFR(Body.MaxLocalCount);
			Body.Compile(compiler);
			compiler.Binary.WriteRET0();
			compiler.Context.Scopes.Pop();
		}
		public override void OpenCallSite(sunCompiler compiler, int argumentCount) {
			var point = compiler.Binary.WriteCALL(argumentCount);
			CallSites.Add(point);
		}
		public override void CloseCallSites(sunCompiler compiler) {
			foreach (var callSite in CallSites) {
				compiler.Binary.ClosePoint(callSite, Offset);
			}
		}
	}

	class sunParameterInfo : IEnumerable<string> {
		string[] Parameters { get; set; }
		public int Minimum { get { return Parameters.Length; } }
		public bool IsVariadic { get; private set; }

		public sunParameterInfo(IEnumerable<sunIdentifier> parameters, bool variadic) {
			// validate parameter names
			var duplicate = parameters.FirstOrDefault(a => parameters.Count(b => a.Value == b.Value) > 1);
			if (duplicate != null) {
				throw new sunRedeclaredParameterException(duplicate);
			}
			Parameters = parameters.Select(i => i.Value).ToArray();
			IsVariadic = variadic;
		}
		public sunParameterInfo(IEnumerable<string> parameters, bool variadic) {
			// validate parameter names
			Parameters = parameters.ToArray();
			IsVariadic = variadic;
		}

		public bool ValidateArgumentCount(int count) {
			return IsVariadic ? count >= Minimum : count == Minimum;
		}

		public IEnumerator<string> GetEnumerator() { return Parameters.GetArrayEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunStorableSymbol : sunSymbol {
		protected sunStorableSymbol(string name)
			: base(name) { }

		public override void Compile(sunCompiler compiler) {
			CompileGet(compiler);
		}
		public abstract void CompileGet(sunCompiler compiler);
		public abstract void CompileSet(sunCompiler compiler);
		public virtual void CompileInc(sunCompiler compiler) {
			CompileGet(compiler);
			compiler.Binary.WriteINT(1);
			compiler.Binary.WriteADD();
		}
		public virtual void CompileDec(sunCompiler compiler) {
			CompileGet(compiler);
			compiler.Binary.WriteINT(1);
			compiler.Binary.WriteSUB();
		}
	}

	class sunVariableSymbol : sunStorableSymbol {
		public int Display { get; private set; }
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Variable; } }
		public override uint Data { get { return (uint)Index; } }

		public sunVariableSymbol(string name, int display, int index)
			: base(name) {
			Display = display;
			Index = index;
		}

		public override void CompileGet(sunCompiler compiler) { compiler.Binary.WriteVAR(Display, Index); }
		public override void CompileSet(sunCompiler compiler) { compiler.Binary.WriteASS(Display, Index); }
		public override void CompileInc(sunCompiler compiler) { compiler.Binary.WriteINC(Display, Index); }
		public override void CompileDec(sunCompiler compiler) { compiler.Binary.WriteDEC(Display, Index); }
	}

	class sunConstantSymbol : sunStorableSymbol {
		sunExpression Expression { get; set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Constant; } }
		public override uint Data { get { return 0; } }

		public sunConstantSymbol(string name, sunExpression expression)
			: base(name) {
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			Expression = expression;
		}

		public override void CompileGet(sunCompiler compiler) {
			Expression.Compile(compiler);
		}
		public override void CompileSet(sunCompiler compiler) {
			// checks against this have to be implemented at a higher level
			throw new InvalidOperationException();
		}
	}

	enum sunSymbolType {
		Builtin,
		Function,
		Variable,
		Constant,
	}
}
