using arookas.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunSymbolTable : IEnumerable<sunSymbol> {
		List<sunSymbol> mSymbols;

		public int Count { get { return mSymbols.Count; } }

		public sunSymbol this[int index] {
			get { return mSymbols[index]; }
		}

		public sunSymbolTable() {
			mSymbols = new List<sunSymbol>(10);
		}

		public void Add(sunSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			mSymbols.Add(symbol);
		}
		public void Remove(sunSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			mSymbols.Remove(symbol);
		}
		public void RemoveAt(int index) {
			mSymbols.RemoveAt(index);
		}
		public void Clear() {
			mSymbols.Clear();
		}

		public IEnumerable<sunSymbol> Get() {
			return mSymbols;
		}
		public IEnumerable<TSymbol> Get<TSymbol>() where TSymbol : sunSymbol {
			return mSymbols.OfType<TSymbol>();
		}

		public int GetCount<TSymbol>() where TSymbol : sunSymbol {
			return mSymbols.Count(i => i is TSymbol);
		}

		public IEnumerator<sunSymbol> GetEnumerator() {
			return mSymbols.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	abstract class sunSymbol {
		string mName;
		sunSymbolModifiers mModifiers;
		List<sunRelocation> mRelocations;

		public string Name {
			get { return mName; }
		}
		public sunSymbolModifiers Modifiers {
			get { return mModifiers; }
			set { mModifiers = value; }
		}

		public bool HasRelocations {
			get { return mRelocations.Count > 0; }
		}

		// symbol table
		public abstract sunSymbolType Type { get; }
		public abstract uint Data { get; }

		protected sunSymbol(string name) {
			mName = name;
			mRelocations = new List<sunRelocation>(10);
		}

		public abstract void Compile(sunCompiler compiler);

		public void OpenRelocation(sunRelocation relocation) {
			if (relocation == null) {
				throw new ArgumentNullException("relocation");
			}
			mRelocations.Add(relocation);
		}
		public void CloseRelocations(sunCompiler compiler) {
			compiler.Binary.Keep();
			foreach (var relocation in mRelocations) {
				relocation.Relocate();
			}
			compiler.Binary.Back();
		}

		public static sunSymbolModifiers GetModifiers(sunNode modifierlist) {
			if (modifierlist == null) {
				return sunSymbolModifiers.None;
			}
			var modifiers = sunSymbolModifiers.None;
			if (modifierlist.Any(i => i is sunConstModifier)) {
				modifiers |= sunSymbolModifiers.Constant;
			}
			if (modifierlist.Any(i => i is sunLocalModifier)) {
				modifiers |= sunSymbolModifiers.Local;
			}
			return modifiers;
		}
	}

	abstract class sunCallableSymbol : sunSymbol {
		sunParameterInfo mParameters;
		protected int mCompiles;

		public sunParameterInfo Parameters {
			get { return mParameters; }
		}

		public int CompileCount {
			get { return mCompiles; }
		}

		protected sunCallableSymbol(string name, sunParameterInfo parameterInfo)
			: base(name) {
			mParameters = parameterInfo;
		}

		public abstract sunRelocation CreateCallSite(sunCompiler compiler, int argCount);
	}

	class sunBuiltinSymbol : sunCallableSymbol {
		int mIndex;

		public int Index {
			get { return mIndex; }
			set { mIndex = value; }
		}

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Builtin; } }
		public override uint Data { get { return (uint)mIndex; } }

		public sunBuiltinSymbol(string name, int index)
			: this(name, null, index) { }
		public sunBuiltinSymbol(string name, sunParameterInfo parameters, int index)
			: base(name, parameters) {
			mIndex = index;
		}

		public override void Compile(sunCompiler compiler) {
			// don't compile builtins
			++mCompiles;
		}
		public override sunRelocation CreateCallSite(sunCompiler compiler, int argCount) {
			return new sunBuiltinCallSite(compiler.Binary, this, argCount);
		}
	}

	class sunFunctionSymbol : sunCallableSymbol {
		uint mOffset;
		sunNode mBody;

		public uint Offset {
			get { return mOffset; }
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
			mBody = body;
		}

		public override void Compile(sunCompiler compiler) {
			mOffset = compiler.Binary.Offset;
#if SSC_SCOPES
			compiler.Context.Scopes.Push(sunScopeType.Function);
#else
			compiler.Context.Scopes.Push();
#endif
			foreach (var parameter in Parameters) {
				compiler.Context.Scopes.DeclareVariable(parameter); // since there is no AST node for these, they won't affect MaxLocalCount
			}
			compiler.Binary.WriteMKDS(1);
			var locals = mBody.LocalCount;
			if (locals > 0) {
				compiler.Binary.WriteMKFR(locals);
			}
			mBody.Compile(compiler);
			compiler.Binary.WriteRET0();
			compiler.Context.Scopes.Pop(compiler);
			++mCompiles;
		}
		public override sunRelocation CreateCallSite(sunCompiler compiler, int argCount) {
			return new sunFunctionCallSite(compiler.Binary, this, argCount);
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
			set { mDisplay = value; }
		}
		public int Index {
			get { return mIndex; }
			set { mIndex = value; }
		}

		// symbol table
		public override sunSymbolType Type {
			get { return sunSymbolType.Variable; }
		}
		public override uint Data {
			get { return (uint)Index; }
		}

		public sunVariableSymbol(string name)
			: this(name, 0, 0) { }
		public sunVariableSymbol(string name, int display, int index)
			: base(name) {
			mDisplay = display;
			mIndex = index;
		}

		public override void CompileGet(sunCompiler compiler) {
			OpenRelocation(new sunVariableGetSite(compiler.Binary, this));
		}
		public override void CompileSet(sunCompiler compiler) {
			OpenRelocation(new sunVariableSetSite(compiler.Binary, this));
		}
		public override void CompileInc(sunCompiler compiler) {
			OpenRelocation(new sunVariableIncSite(compiler.Binary, this));
		}
		public override void CompileDec(sunCompiler compiler) {
			OpenRelocation(new sunVariableDecSite(compiler.Binary, this));
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

	public enum sunSymbolType {
		Builtin,
		Function,
		Variable,
		Constant,
	}

	[Flags]
	enum sunSymbolModifiers {
		None = 0,
		Constant = 1 << 0,
		Local = 1 << 1,
	}
}
