using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunScopeStack : IEnumerable<sunScope> {
		List<sunScope> mStack;
#if SSC_SCOPES
		int mLocals;
#endif

		public int Count {
			get { return mStack.Count; }
		}
		
#if SSC_SCOPES
		public sunScope Root {
			get { return this.FirstOrDefault(i => i.Type == Top.Type); }
		}
		public sunScope Script {
			get { return this.FirstOrDefault(i => i.Type == sunScopeType.Script); }
		}
		public sunScope Function {
			get { return this.FirstOrDefault(i => i.Type == sunScopeType.Function); }
		}
#endif
		public sunScope Top {
			get { return this[Count - 1]; }
		}

		public sunScope this[int index] {
			get { return mStack[index]; }
		}

		public sunScopeStack() {
			mStack = new List<sunScope>(8);
#if SSC_SCOPES
			Push(sunScopeType.Script); // push global scope
#else
			Push();
#endif
		}

		public void Push() {
#if SSC_SCOPES
			Push(Top.Type);
#else
			mStack.Add(new sunScope());
#endif
		}
#if SSC_SCOPES
		public void Push(sunScopeType type) {
			mStack.Add(new sunScope(type));
		}
#endif
		public void Pop(sunCompiler compiler) {
			if (Count > 1) {
#if SSC_SCOPES
				if (Top.Type == sunScopeType.Script) {
					mLocals = 0; // left the function, reset locals
				}
#else
				// close relocations while we still have references to the symbols
				foreach (var variable in Top) {
					variable.CloseRelocations(compiler);
				}
#endif
				mStack.RemoveAt(Count - 1);
			}
		}
		public void Clear() {
			mStack.Clear();
#if SSC_SCOPES
			Push(sunScopeType.Script); // add global scope
			mLocals = 0;
#else
			Push();
#endif
		}

		public sunVariableSymbol DeclareVariable(string name) {
#if SSC_SCOPES
			switch (Top.Type) {
				case sunScopeType.Script: return DeclareGlobal(name);
				case sunScopeType.Function: return DeclareLocal(name);
			}
			return null;
#else
			return Top.DeclareVariable(name, Count - 1, Top.VariableCount);
#endif
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression) {
			return Top.DeclareConstant(name, expression);
		}
#if SSC_SCOPES
		sunVariableSymbol DeclareGlobal(string name) {
			// symbol's display/index will be
			// filled out by the relocation code
			return Top.DeclareVariable(name, 0, 0);
		}
		sunVariableSymbol DeclareLocal(string name) {
			var symbol = Top.DeclareVariable(name, 1, mLocals);
			if (symbol != null) {
				++mLocals;
			}
			return symbol;
		}
#endif

		public IEnumerator<sunScope> GetEnumerator() {
			return mStack.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	class sunScope : IEnumerable<sunStorableSymbol> {
		List<sunStorableSymbol> mStorables;
#if SSC_SCOPES
		sunScopeType mType;
#endif

#if SSC_SCOPES
		public sunScopeType Type {
			get { return mType; }
		}
#endif

#if SSC_SCOPES
		public sunScope(sunScopeType type) {
#else
		public sunScope() {
#endif
			mStorables = new List<sunStorableSymbol>(10);
#if SSC_SCOPES
			mType = type;
#endif
		}

		public int StorableCount {
			get { return mStorables.Count; }
		}
		public int VariableCount {
			get { return mStorables.Count(i => i is sunVariableSymbol); }
		}
		public int ConstantCount {
			get { return mStorables.Count(i => i is sunConstantSymbol); }
		}

		public bool GetIsDeclared(string name) {
			return mStorables.Any(v => v.Name == name);
		}

		public sunVariableSymbol DeclareVariable(string name, int display, int index) {
			if (GetIsDeclared(name)) {
				return null;
			}
			var symbol = new sunVariableSymbol(name, display, index);
			mStorables.Add(symbol);
			return symbol;
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression) {
			if (GetIsDeclared(name)) {
				return null;
			}
			var symbol = new sunConstantSymbol(name, expression);
			mStorables.Add(symbol);
			return symbol;
		}

		public sunStorableSymbol ResolveStorable(string name) {
			return mStorables.FirstOrDefault(i => i.Name == name);
		}
		public sunVariableSymbol ResolveVariable(string name) {
			return ResolveStorable(name) as sunVariableSymbol;
		}
		public sunConstantSymbol ResolveConstant(string name) {
			return ResolveStorable(name) as sunConstantSymbol;
		}

		public IEnumerator<sunStorableSymbol> GetEnumerator() {
			return mStorables.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

#if SSC_SCOPES
	enum sunScopeType {
		Script,
		Function,
	}
#endif
}
