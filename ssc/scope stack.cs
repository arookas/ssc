using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunScopeStack : IEnumerable<sunScope>
	{
		List<sunScope> stack = new List<sunScope>(8);
		int GlobalCount { get { return stack.Where(i => i.Type == sunScopeType.Script).Sum(i => i.StorableCount); } }
		int LocalCount { get { return stack.Where(i => i.Type == sunScopeType.Function).Sum(i => i.StorableCount); } }

		public int Count { get { return stack.Count; } }

		public sunScope Root { get { return this[0]; } }
		public sunScope Top { get { return this[Count - 1]; } }

		public sunScope this[int index] { get { return stack[index]; } }

		public sunScopeStack()
		{
			Push(sunScopeType.Script); // push global scope
		}

		public void Push(sunScopeType type)
		{
			stack.Add(new sunScope(type));
		}
		public void Pop()
		{
			if (Count > 1)
			{
				stack.RemoveAt(Count - 1);
			}
		}
		public void Clear()
		{
			stack.Clear();
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

		public IEnumerator<sunScope> GetEnumerator() { return stack.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	class sunScope
	{
		List<sunStorableSymbol> storables = new List<sunStorableSymbol>(10);
		public sunScopeType Type { get; private set; }

		public sunScope(sunScopeType type)
		{
			Type = type;
		}

		public int StorableCount { get { return storables.Count; } }
		public int VariableCount { get { return storables.OfType<sunVariableSymbol>().Count(); } }
		public int ConstantCount { get { return storables.OfType<sunConstantSymbol>().Count(); } }

		public bool GetIsDeclared(string name) { return storables.Any(v => v.Name == name); }

		public sunVariableSymbol DeclareVariable(string name, int display, int index)
		{
			if (GetIsDeclared(name))
			{
				return null;
			}
			var variableInfo = new sunVariableSymbol(name, display, index);
			storables.Add(variableInfo);
			return variableInfo;
		}
		public sunConstantSymbol DeclareConstant(string name, sunExpression expression)
		{
			if (GetIsDeclared(name))
			{
				return null;
			}
			var constantSymbol = new sunConstantSymbol(name, expression);
			storables.Add(constantSymbol);
			return constantSymbol;
		}
		
		public sunStorableSymbol ResolveStorable(string name) { return storables.FirstOrDefault(i => i.Name == name); }
		public sunVariableSymbol ResolveVariable(string name) { return storables.OfType<sunVariableSymbol>().FirstOrDefault(i => i.Name == name); }
		public sunConstantSymbol ResolveConstant(string name) { return storables.OfType<sunConstantSymbol>().FirstOrDefault(i => i.Name == name); }
	}

	enum sunScopeType
	{
		Script, // outside of a function
		Function, // inside of a function
	}

	class sunConstInfo
	{
		public string Name { get; private set; }
		public sunExpression Expression { get; private set; }

		public sunConstInfo(string name, sunExpression expression)
		{
			Name = name;
			Expression = expression;
		}
	}
}
