using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunScopeStack : IEnumerable<sunScope>
	{
		List<sunScope> stack = new List<sunScope>(8);

		public int Count { get { return stack.Count; } }
		public bool IsRoot { get { return Count == 1; } }

		public sunScope Root { get { return this[0]; } }
		public sunScope Top { get { return this[Count - 1]; } }

		public sunScope this[int index] { get { return stack[index]; } }

		public sunScopeStack()
		{
			Push(); // push global scope
		}

		public void Push()
		{
			stack.Add(new sunScope());
		}
		public void Pop()
		{
			if (!IsRoot)
			{
				stack.RemoveAt(Count - 1);
			}
		}
		public void Clear()
		{
			stack.Clear();
			Push(); // add global scope
		}

		public bool GetIsVariableDeclared(string name) { return stack.Any(i => i.GetIsVariableDeclared(name)); }

		public IEnumerator<sunScope> GetEnumerator() { return stack.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	class sunScope
	{
		List<sunVariableInfo> variables = new List<sunVariableInfo>(10);
		List<sunConstInfo> constants = new List<sunConstInfo>(10);

		public int VariableCount { get { return variables.Count; } }
		public int ConstantCount { get { return constants.Count; } }

		public bool GetIsVariableDeclared(string name) { return variables.Any(v => v.Name == name); }
		public sunVariableInfo DeclareVariable(string name, int display)
		{
			if (GetIsVariableDeclared(name) || GetIsConstantDeclared(name))
			{
				return null;
			}
			var variableInfo = new sunVariableInfo(name, display, variables.Count);
			variables.Add(variableInfo);
			return variableInfo;
		}
		public sunVariableInfo ResolveVariable(string name) { return variables.FirstOrDefault(v => v.Name == name); }

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
