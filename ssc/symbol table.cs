using arookas.Collections;
using arookas.IO.Binary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunSymbolTable : IEnumerable<sunSymbol>
	{
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

		public sunSymbolTable()
		{
			Symbols = new List<sunSymbol>(10);
		}

		public void Add(sunSymbol symbol) { Symbols.Add(symbol); }
		public void Clear() { Symbols.Clear(); }

		public void Write(aBinaryWriter writer)
		{
			int ofs = 0;
			foreach (var sym in this)
			{
				writer.WriteS32((int)sym.Type);
				writer.WriteS32(ofs);
				writer.Write32(sym.Data);

				// runtime fields
				writer.WriteS32(0);
				writer.WriteS32(0);

				ofs += writer.Encoding.GetByteCount(sym.Name) + 1; // include null terminator
			}
			foreach (var sym in this)
			{
				writer.WriteString(sym.Name, aBinaryStringFormat.NullTerminated);
			}
		}

		public IEnumerator<sunSymbol> GetEnumerator() { return Symbols.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunSymbol
	{
		public string Name { get; private set; }

		// symbol table
		public abstract sunSymbolType Type { get; }
		public abstract uint Data { get; }

		protected sunSymbol(string name)
		{
			Name = name;
		}

		public abstract void Compile(sunContext context);
	}

	abstract class sunCallableSymbol : sunSymbol
	{
		public sunParameterInfo Parameters { get; private set; }
		protected List<sunPoint> CallSites { get; private set; }

		public bool HasCallSites { get { return CallSites.Count > 0; } }

		protected sunCallableSymbol(string name, sunParameterInfo parameterInfo)
			: base(name)
		{
			Parameters = parameterInfo;
			CallSites = new List<sunPoint>(10);
		}

		public abstract void OpenCallSite(sunContext context, int argumentCount);
		public abstract void CloseCallSites(sunContext context);
	}

	class sunBuiltinSymbol : sunCallableSymbol
	{
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Builtin; } }
		public override uint Data { get { return (uint)Index; } }

		public sunBuiltinSymbol(string name, sunParameterInfo parameters, int index)
			: base(name, parameters)
		{
			Index = index;
		}

		public override void Compile(sunContext context)
		{
			throw new InvalidOperationException("Cannot compile builtins.");
		}
		public override void OpenCallSite(sunContext context, int argumentCount)
		{
			context.Text.CallBuiltin(Index, argumentCount);
		}
		public override void CloseCallSites(sunContext context)
		{
			// do nothing
		}
	}

	class sunFunctionSymbol : sunCallableSymbol
	{
		sunNode Body { get; set; }
		public uint Offset { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Function; } }
		public override uint Data { get { return (uint)Offset; } }

		public sunFunctionSymbol(string name, sunParameterInfo parameters, sunNode body)
			: base(name, parameters)
		{
			Body = body;
		}

		public override void Compile(sunContext context)
		{
			Offset = context.Text.Offset;
			context.Scopes.Push(sunScopeType.Function);
			context.Scopes.ResetLocalCount();
			foreach (var parameter in Parameters)
			{
				context.Scopes.DeclareVariable(parameter); // since there is no AST node for these, they won't affect MaxLocalCount
			}
			context.Text.StoreDisplay(1);
			context.Text.DeclareLocal(Body.MaxLocalCount);
			Body.Compile(context);
			context.Text.ReturnVoid();
			context.Scopes.Pop();
		}
		public override void OpenCallSite(sunContext context, int argumentCount)
		{
			var point = context.Text.CallFunction(argumentCount);
			CallSites.Add(point);
		}
		public override void CloseCallSites(sunContext context)
		{
			foreach (var callSite in CallSites)
			{
				context.Text.ClosePoint(callSite, Offset);
			}
		}
	}

	class sunParameterInfo : IEnumerable<string>
	{
		string[] Parameters { get; set; }
		public int Minimum { get { return Parameters.Length; } }
		public bool IsVariadic { get; private set; }

		public sunParameterInfo(IEnumerable<sunIdentifier> parameters, bool variadic)
		{
			// validate parameter names
			var duplicate = parameters.FirstOrDefault(a => parameters.Count(b => a.Value == b.Value) > 1);
			if (duplicate != null)
			{
				throw new sunRedeclaredParameterException(duplicate);
			}
			Parameters = parameters.Select(i => i.Value).ToArray();
			IsVariadic = variadic;
		}
		public sunParameterInfo(IEnumerable<string> parameters, bool variadic)
		{
			// validate parameter names
			Parameters = parameters.ToArray();
			IsVariadic = variadic;
		}

		public bool ValidateArgumentCount(int count)
		{
			return IsVariadic ? count >= Minimum : count == Minimum;
		}

		public IEnumerator<string> GetEnumerator() { return Parameters.GetArrayEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunStorableSymbol : sunSymbol
	{
		protected sunStorableSymbol(string name)
			: base(name)
		{

		}

		public override void Compile(sunContext context)
		{
			CompileGet(context); // compile get by default
		}
		public abstract void CompileGet(sunContext context);
		public abstract void CompileSet(sunContext context);
		public virtual void CompileInc(sunContext context)
		{
			CompileGet(context);
			context.Text.PushInt(1);
			context.Text.Add();
		}
		public virtual void CompileDec(sunContext context)
		{
			CompileGet(context);
			context.Text.PushInt(1);
			context.Text.Sub();
		}
	}

	class sunVariableSymbol : sunStorableSymbol
	{
		public int Display { get; private set; }
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Variable; } }
		public override uint Data { get { return (uint)Index; } }

		public sunVariableSymbol(string name, int display, int index)
			: base(name)
		{
			Display = display;
			Index = index;
		}

		public override void CompileGet(sunContext context)
		{
			context.Text.PushVariable(Display, Index);
		}
		public override void CompileSet(sunContext context)
		{
			context.Text.StoreVariable(Display, Index);
		}
		public override void CompileInc(sunContext context)
		{
			context.Text.IncVariable(Display, Index);
		}
		public override void CompileDec(sunContext context)
		{
			context.Text.DecVariable(Display, Index);
		}
	}

	class sunConstantSymbol : sunStorableSymbol
	{
		sunExpression Expression { get; set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Constant; } }
		public override uint Data { get { return 0; } }

		public sunConstantSymbol(string name, sunExpression expression)
			: base(name)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			Expression = expression;
		}

		public override void CompileGet(sunContext context)
		{
			Expression.Compile(context);
		}
		public override void CompileSet(sunContext context)
		{
			// checks against this have to be implemented at a higher level
			throw new InvalidOperationException();
		}
	}

	enum sunSymbolType
	{
		Builtin,
		Function,
		Variable,
		Constant,
	}
}
