using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunScopeStack : IEnumerable<sunScope> {
		List<sunScope> mStack;

		public int Count {
			get { return mStack.Count; }
		}
		
		public sunScope Top {
			get { return this[Count - 1]; }
		}

		public sunScope this[int index] {
			get { return mStack[index]; }
		}

		public sunScopeStack() {
			mStack = new List<sunScope>(8);
			Push();
		}

		public void Push() {
			mStack.Add(new sunScope());
		}
		public void Pop() {
			if (Count > 1) {
				// close relocations while we still have references to the symbols
				foreach (var variable in Top) {
					variable.CloseRelocations();
				}
				mStack.RemoveAt(Count - 1);
			}
		}
		public void Clear() {
			mStack.Clear();
			Push();
		}

		public sunVariableSymbol DeclareVariable(string name) {
			return Top.DeclareVariable(name, Count - 1, Top.VariableCount);
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression) {
			return Top.DeclareConstant(name, expression);
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

		public sunScope() {
			mStorables = new List<sunStorableSymbol>(10);
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
}
