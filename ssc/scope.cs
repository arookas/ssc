using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunScopeStack : IEnumerable<sunScope> {
		List<sunScope> Stack { get; set; }

		public int Count { get { return Stack.Count; } }
#if SSC_PACK_VARS
		int GlobalCount { get { return Stack.Where(i => i.Type == sunScopeType.Script).Sum(i => i.VariableCount); } }
		int LocalCount { get { return Stack.Where(i => i.Type == sunScopeType.Function).Sum(i => i.VariableCount); } }
#else
		int GlobalCount { get; set; }
		int LocalCount { get; set; }
#endif

		public sunScope Root { get { return this.FirstOrDefault(i => i.Type == Top.Type); } }
		public sunScope Script { get { return this.FirstOrDefault(i => i.Type == sunScopeType.Script); } }
		public sunScope Function { get { return this.FirstOrDefault(i => i.Type == sunScopeType.Function); } }
		public sunScope Top { get { return this[Count - 1]; } }

		public sunScope this[int index] { get { return Stack[index]; } }

		public sunScopeStack() {
			Stack = new List<sunScope>(8);
			Push(sunScopeType.Script); // push global scope
		}

		public void Push() { Push(Top.Type); }
		public void Push(sunScopeType type) {
			Stack.Add(new sunScope(type));
		}
		public void Pop() {
			if (Count > 1) {
				Stack.RemoveAt(Count - 1);
			}
		}
		public void Clear() {
			Stack.Clear();
			Push(sunScopeType.Script); // add global scope
		}

#if !SSC_PACK_VARS
		public void ResetGlobalCount() { GlobalCount = 0; }
		public void ResetLocalCount() { LocalCount = 0; }
#endif

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
			var symbol = Top.DeclareVariable(name, 0, GlobalCount);
#if !SSC_PACK_VARS
			if (symbol != null) {
				++GlobalCount;
			}
#endif
			return symbol;
		}
		sunVariableSymbol DeclareLocal(string name) {
			var symbol = Top.DeclareVariable(name, 1, LocalCount);
#if !SSC_PACK_VARS
			if (symbol != null) {
				++LocalCount;
			}
#endif
			return symbol;
		}

		public IEnumerator<sunScope> GetEnumerator() { return Stack.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	class sunScope {
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
	}

	enum sunScopeType {
		Script, // outside of a function
		Function, // inside of a function
	}
}
