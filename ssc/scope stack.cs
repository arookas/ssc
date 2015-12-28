using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunScopeStack : IEnumerable<sunScope>
	{
		List<sunScope> Stack { get; set; }
		int GlobalCount { get { return Stack.Where(i => i.Type == sunScopeType.Script).Sum(i => i.StorableCount); } }
		int LocalCount { get { return Stack.Where(i => i.Type == sunScopeType.Function).Sum(i => i.StorableCount); } }

		public int Count { get { return Stack.Count; } }

		public sunScope Root { get { return this.FirstOrDefault(i => i.Type == Top.Type); } }
		public sunScope Script { get { return this.FirstOrDefault(i => i.Type == sunScopeType.Script); } }
		public sunScope Function { get { return this.FirstOrDefault(i => i.Type == sunScopeType.Function); } }
		public sunScope Top { get { return this[Count - 1]; } }

		public sunScope this[int index] { get { return Stack[index]; } }

		public sunScopeStack()
		{
			Stack = new List<sunScope>(8);
			Push(sunScopeType.Script); // push global scope
		}

		public void Push(sunScopeType type)
		{
			Stack.Add(new sunScope(type));
		}
		public void Pop()
		{
			if (Count > 1)
			{
				Stack.RemoveAt(Count - 1);
			}
		}
		public void Clear()
		{
			Stack.Clear();
			Push(sunScopeType.Script); // add global scope
		}

		public sunVariableSymbol DeclareVariable(string name)
		{
			switch (Top.Type)
			{
				case sunScopeType.Script: return DeclareGlobal(name);
				case sunScopeType.Function: return DeclareLocal(name);
			}
			return null;
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression)
		{
			return Top.DeclareConstant(name, expression);
		}
		sunVariableSymbol DeclareGlobal(string name)
		{
			var variableInfo = Top.DeclareVariable(name, 0, GlobalCount);
			if (variableInfo == null)
			{
				return null;
			}
			return variableInfo;
		}
		sunVariableSymbol DeclareLocal(string name)
		{
			var variableInfo = Top.DeclareVariable(name, 1, LocalCount);
			if (variableInfo == null)
			{
				return null;
			}
			return variableInfo;
		}

		public IEnumerator<sunScope> GetEnumerator() { return Stack.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	class sunScope
	{
		List<sunStorableSymbol> Storables { get; set; }
		IEnumerable<sunVariableSymbol> Variables { get { return Storables.OfType<sunVariableSymbol>(); } }
		IEnumerable<sunConstantSymbol> Constants { get { return Storables.OfType<sunConstantSymbol>(); } }
		public sunScopeType Type { get; private set; }

		public sunScope(sunScopeType type)
		{
			Storables = new List<sunStorableSymbol>(10);
			Type = type;
		}

		public int StorableCount { get { return Storables.Count; } }
		public int VariableCount { get { return Variables.Count(); } }
		public int ConstantCount { get { return Constants.Count(); } }

		public bool GetIsDeclared(string name) { return Storables.Any(v => v.Name == name); }

		public sunVariableSymbol DeclareVariable(string name, int display, int index)
		{
			if (GetIsDeclared(name))
			{
				return null;
			}
			var symbol = new sunVariableSymbol(name, display, index);
			Storables.Add(symbol);
			return symbol;
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression)
		{
			if (GetIsDeclared(name))
			{
				return null;
			}
			var symbol = new sunConstantSymbol(name, expression);
			Storables.Add(symbol);
			return symbol;
		}

		public sunStorableSymbol ResolveStorable(string name) { return Storables.FirstOrDefault(i => i.Name == name); }
		public sunVariableSymbol ResolveVariable(string name) { return Variables.FirstOrDefault(i => i.Name == name); }
		public sunConstantSymbol ResolveConstant(string name) { return Constants.FirstOrDefault(i => i.Name == name); }
	}

	enum sunScopeType
	{
		Script, // outside of a function
		Function, // inside of a function
	}
}
