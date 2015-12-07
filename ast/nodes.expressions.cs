using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunExpression : sunNode
	{
		public sunExpression(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			Stack<sunOperator> operatorStack = new Stack<sunOperator>(32);
			AnalyzeExpression(context, this, operatorStack);
		}

		void AnalyzeExpression(sunContext context, sunExpression expression, Stack<sunOperator> operatorStack)
		{
			// this implementation assumes that the expression production child list alternates between operand and operator
			// we can safely assume this as the grammar "operand {binary_operator operand}" enforces it
			int stackCount = operatorStack.Count;
			foreach (var node in expression)
			{
				if (node is sunOperand)
				{
					var operand = node as sunOperand;

					// term
					var term = operand.Term;
					if (term is sunExpression)
					{
						AnalyzeExpression(context, term as sunExpression, operatorStack);
					}
					else
					{
						term.Compile(context);
					}
					var unaryOperators = operand.UnaryOperators;
					if (unaryOperators != null)
					{
						unaryOperators.Compile(context);
					}
				}
				else if (node is sunOperator)
				{
					var operatorNode = node as sunOperator;
					while (operatorStack.Count > stackCount &&
						(operatorNode.IsLeftAssociative && operatorNode.Precedence <= operatorStack.Peek().Precedence) ||
						(operatorNode.IsRightAssociative && operatorNode.Precedence < operatorStack.Peek().Precedence))
					{
						operatorStack.Pop().Compile(context);
					}
					operatorStack.Push(operatorNode);
				}
			}
			while (operatorStack.Count > stackCount)
			{
				operatorStack.Pop().Compile(context);
			}
		}
	}

	class sunOperand : sunNode
	{
		public sunNode UnaryOperators { get { return Count > 1 ? this[0] : null; } }
		public sunNode Term { get { return this[Count - 1]; } }

		public sunOperand(sunSourceLocation location)
			: base(location)
		{

		}

		// operands are compiled in sunExpression.Compile
	}

	class sunUnaryOperatorList : sunNode
	{
		public sunUnaryOperatorList(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			foreach (var child in this.Reverse())
			{
				// compile unary operators in reverse order
				child.Compile(context);
			}
		}
	}

	class sunTernaryOperator : sunNode
	{
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunExpression TrueBody { get { return this[1] as sunExpression; } }
		public sunExpression FalseBody { get { return this[2] as sunExpression; } }

		public sunTernaryOperator(sunSourceLocation node)
			: base(node)
		{

		}

		public override void Compile(sunContext context)
		{
			Condition.Compile(context);
			var falsePrologue = context.Text.GotoIfZero();
			TrueBody.Compile(context);
			var trueEpilogue = context.Text.Goto();
			context.Text.ClosePoint(falsePrologue);
			FalseBody.Compile(context);
			context.Text.ClosePoint(trueEpilogue);
		}
	}
}
