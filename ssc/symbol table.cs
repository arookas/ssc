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
		string mName;

		public string Name {
			get { return mName; }
		}

		// symbol table
		public abstract sunSymbolType Type { get; }
		public abstract uint Data { get; }

		protected sunSymbol(string name) {
			mName = name;
		}

		public abstract void Compile(sunCompiler compiler);
	}

	abstract class sunCallableSymbol : sunSymbol {
		public sunParameterInfo Parameters { get; private set; }

		public abstract bool HasCallSites { get; }

		protected sunCallableSymbol(string name, sunParameterInfo parameterInfo)
			: base(name) {
			Parameters = parameterInfo;
		}

		public abstract void OpenCallSite(sunCompiler compiler, int argumentCount);
		public abstract void CloseCallSites(sunCompiler compiler);
	}

	class sunBuiltinSymbol : sunCallableSymbol {
		int mIndex;
		List<sunBuiltinCallSite> mCallSites;

		public int Index {
			get { return mIndex; }
			set { mIndex = value; }
		}

		public override bool HasCallSites {
			get { return mCallSites.Count > 0; }
		}

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Builtin; } }
		public override uint Data { get { return (uint)Index; } }

		public sunBuiltinSymbol(string name, int index)
			: this(name, null, index) { }
		public sunBuiltinSymbol(string name, sunParameterInfo parameters, int index)
			: base(name, parameters) {
			mIndex = index;
			mCallSites = new List<sunBuiltinCallSite>(10);
		}

		public override void Compile(sunCompiler compiler) {
			// don't compile builtins
		}
		public override void OpenCallSite(sunCompiler compiler, int argumentCount) {
			var callSite = new sunBuiltinCallSite(compiler.Binary.OpenPoint(), argumentCount);
			mCallSites.Add(callSite);
			compiler.Binary.WriteFUNC(0, 0); // dummy
		}
		public override void CloseCallSites(sunCompiler compiler) {
			compiler.Binary.Keep();
			foreach (var callSite in mCallSites) {
				compiler.Binary.Goto(callSite.Point);
				compiler.Binary.WriteFUNC(mIndex, callSite.ArgCount);
			}
			compiler.Binary.Back();
		}

		struct sunBuiltinCallSite {
			sunPoint mPoint;
			int mArgCount;

			public sunPoint Point {
				get { return mPoint; }
			}
			public int ArgCount {
				get { return mArgCount; }
			}

			public sunBuiltinCallSite(sunPoint point, int argCount) {
				mPoint = point;
				mArgCount = argCount;
			}
		}
	}

	class sunFunctionSymbol : sunCallableSymbol {
		uint mOffset;
		sunNode mBody;
		List<sunPoint> mCallSites;

		public uint Offset {
			get { return mOffset; }
		}

		public override bool HasCallSites {
			get { return mCallSites.Count > 0; }
		}

		// symbol table
		public override sunSymbolType Type {
			get { return sunSymbolType.Function; }
		}
		public override uint Data {
			get { return (uint)Offset; }
		}

		public sunFunctionSymbol(string name, sunParameterInfo parameters, sunNode body)
			: base(name, parameters) {
			if (body == null) {
				throw new ArgumentNullException("body");
			}
			mCallSites = new List<sunPoint>(5);
			mBody = body;
		}

		public override void Compile(sunCompiler compiler) {
			mOffset = compiler.Binary.Offset;
			compiler.Context.Scopes.Push(sunScopeType.Function);
			compiler.Context.Scopes.ResetLocalCount();
			foreach (var parameter in Parameters) {
				compiler.Context.Scopes.DeclareVariable(parameter); // since there is no AST node for these, they won't affect MaxLocalCount
			}
			compiler.Binary.WriteMKDS(1);
			compiler.Binary.WriteMKFR(mBody.MaxLocalCount);
			mBody.Compile(compiler);
			compiler.Binary.WriteRET0();
			compiler.Context.Scopes.Pop();
		}
		public override void OpenCallSite(sunCompiler compiler, int argumentCount) {
			var point = compiler.Binary.WriteCALL(argumentCount);
			mCallSites.Add(point);
		}
		public override void CloseCallSites(sunCompiler compiler) {
			foreach (var callSite in mCallSites) {
				compiler.Binary.ClosePoint(callSite, Offset);
			}
		}
	}

	class sunParameterInfo : IEnumerable<string> {
		string[] mParameters;
		bool mVariadic;

		public int Minimum {
			get { return mParameters.Length; }
		}
		public bool IsVariadic {
			get { return mVariadic; }
		}

		public sunParameterInfo(IEnumerable<sunIdentifier> parameters, bool variadic) {
			// validate parameter names
			var duplicate = parameters.FirstOrDefault(a => parameters.Count(b => a.Value == b.Value) > 1);
			if (duplicate != null) {
				throw new sunRedeclaredParameterException(duplicate);
			}
			mParameters = parameters.Select(i => i.Value).ToArray();
			mVariadic = variadic;
		}
		public sunParameterInfo(IEnumerable<string> parameters, bool variadic) {
			// validate parameter names
			mParameters = parameters.ToArray();
			mVariadic = variadic;
		}

		public bool ValidateArgumentCount(int count) {
			return mVariadic ? count >= Minimum : count == Minimum;
		}

		public IEnumerator<string> GetEnumerator() {
			return mParameters.GetArrayEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
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
		int mDisplay, mIndex;

		public int Display {
			get { return mDisplay; }
		}
		public int Index {
			get { return mIndex; }
		}

		// symbol table
		public override sunSymbolType Type {
			get { return sunSymbolType.Variable; }
		}
		public override uint Data {
			get { return (uint)Index; }
		}

		public sunVariableSymbol(string name, int display, int index)
			: base(name) {
			mDisplay = display;
			mIndex = index;
		}

		public override void CompileGet(sunCompiler compiler) {
			compiler.Binary.WriteVAR(mDisplay, mIndex);
		}
		public override void CompileSet(sunCompiler compiler) {
			compiler.Binary.WriteASS(mDisplay, mIndex);
		}
		public override void CompileInc(sunCompiler compiler) {
			compiler.Binary.WriteINC(mDisplay, mIndex);
		}
		public override void CompileDec(sunCompiler compiler) {
			compiler.Binary.WriteDEC(mDisplay, mIndex);
		}
	}

	class sunConstantSymbol : sunStorableSymbol {
		sunExpression mExpression;

		// symbol table
		public override sunSymbolType Type {
			get { return sunSymbolType.Constant; }
		}
		public override uint Data {
			get { return 0; }
		}

		public sunConstantSymbol(string name, sunExpression expression)
			: base(name) {
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			mExpression = expression;
		}

		public override void CompileGet(sunCompiler compiler) {
			mExpression.Compile(compiler);
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
