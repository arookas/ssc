using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunScopeStack : IEnumerable<sunScope> {
		List<sunScope> mStack;
		int mGlobals, mLocals;

		public int Count {
			get { return mStack.Count; }
		}

		public sunScope Root {
			get { return this.FirstOrDefault(i => i.Type == Top.Type); }
		}
		public sunScope Script {
			get { return this.FirstOrDefault(i => i.Type == sunScopeType.Script); }
		}
		public sunScope Function {
			get { return this.FirstOrDefault(i => i.Type == sunScopeType.Function); }
		}
		public sunScope Top {
			get { return this[Count - 1]; }
		}

		public sunScope this[int index] {
			get { return mStack[index]; }
		}

		public sunScopeStack() {
			mStack = new List<sunScope>(8);
			Push(sunScopeType.Script); // push global scope
		}

		public void Push() { Push(Top.Type); }
		public void Push(sunScopeType type) {
			mStack.Add(new sunScope(type));
		}
		public void Pop() {
			if (Count > 1) {
				mStack.RemoveAt(Count - 1);
			}
		}
		public void Clear() {
			mStack.Clear();
			Push(sunScopeType.Script); // add global scope
			mGlobals = 0;
			mLocals = 0;
		}

		public void ResetGlobalCount() {
			mGlobals = 0;
		}
		public void ResetLocalCount() {
			mLocals = 0;
		}

		public sunVariableSymbol DeclareVariable(string name) {
			switch (Top.Type) {
				case sunScopeType.Script: return DeclareGlobal(name);
				case sunScopeType.Function: return DeclareLocal(name);
			}
			return null;
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression) {
			return Top.DeclareConstant(name, expression);
		}
		sunVariableSymbol DeclareGlobal(string name) {
			var symbol = Top.DeclareVariable(name, 0, mGlobals);
			if (symbol != null) {
				++mGlobals;
			}
			return symbol;
		}
		sunVariableSymbol DeclareLocal(string name) {
			var symbol = Top.DeclareVariable(name, 1, mLocals);
			if (symbol != null) {
				++mLocals;
			}
			return symbol;
		}

		public IEnumerator<sunScope> GetEnumerator() {
			return mStack.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	class sunScope : IEnumerable<sunStorableSymbol> {
		List<sunStorableSymbol> mStorables;
		sunScopeType mType;

		public sunScopeType Type {
			get { return mType; }
		}
		
		public sunScope(sunScopeType type) {
			mStorables = new List<sunStorableSymbol>(10);
			mType = type;
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

	enum sunScopeType {
		Script, // outside of a function
		Function, // inside of a function
	}
}
