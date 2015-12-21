using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunScopeStack : IEnumerable<sunScope>
	{
		List<sunScope> stack = new List<sunScope>(8);
		int GlobalCount { get { return stack.Where(i => i.Type == sunScopeType.Script).Sum(i => i.VariableCount); } }
		int LocalCount { get { return stack.Where(i => i.Type == sunScopeType.Function).Sum(i => i.VariableCount); } }

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
		List<sunVariableSymbol> variables = new List<sunVariableSymbol>(10);
		List<sunConstInfo> constants = new List<sunConstInfo>(10);
		public sunScopeType Type { get; private set; }

		public sunScope(sunScopeType type)
		{
			Type = type;
		}

		public int VariableCount { get { return variables.Count; } }
		public int ConstantCount { get { return constants.Count; } }

		public bool GetIsVariableDeclared(string name) { return variables.Any(v => v.Name == name); }
		public sunVariableSymbol DeclareVariable(string name, int display, int index)
		{
			if (GetIsVariableDeclared(name) || GetIsConstantDeclared(name))
			{
				return null;
			}
			var variableInfo = new sunVariableSymbol(name, display, index);
			variables.Add(variableInfo);
			return variableInfo;
		}
		public sunVariableSymbol ResolveVariable(string name) { return variables.FirstOrDefault(v => v.Name == name); }

		public bool GetIsConstantDeclared(string name) { return constants.Any(c => c.Name == name); }
		public sunConstInfo DeclareConstant(string name, sunExpression expression)
		{
			if (GetIsVariableDeclared(name) || GetIsConstantDeclared(name))
			{
				return null;
			}
			var constInfo = new sunConstInfo(name, expression);
			constants.Add(constInfo);
			return constInfo;
		}
		public sunConstInfo ResolveConstant(string name) { return constants.FirstOrDefault(c => c.Name == name); }
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
