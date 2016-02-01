using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	interface sunTerm {
		sunExpressionFlags GetExpressionFlags(sunContext context);
	}

	class sunExpression : sunNode, sunTerm {
		public sunExpression(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var operatorStack = new Stack<sunOperator>(32);
			CompileExpression(compiler, this, operatorStack);
		}
		public sunExpressionFlags Analyze(sunContext context) {
			return AnalyzeExpression(context, this);
		}

		static void CompileExpression(sunCompiler compiler, sunExpression expression, Stack<sunOperator> operatorStack) {
			// this implementation assumes that the expression production child list alternates between operand and operator
			// we can safely assume this as the grammar "operand {binary_operator operand}" enforces it
			int stackCount = operatorStack.Count;
			foreach (var node in expression) {
				if (node is sunOperand) {
					var operand = node as sunOperand;

					// term
					var term = operand.Term;
					if (term is sunExpression) {
						CompileExpression(compiler, term as sunExpression, operatorStack);
					}
					else {
						term.Compile(compiler);
					}
					var unaryOperators = operand.UnaryOperators;
					if (unaryOperators != null) {
						unaryOperators.Compile(compiler);
					}
				}
				else if (node is sunOperator) {
					var operatorNode = node as sunOperator;
					while (operatorStack.Count > stackCount &&
						(operatorNode.IsLeftAssociative && operatorNode.Precedence <= operatorStack.Peek().Precedence) ||
						(operatorNode.IsRightAssociative && operatorNode.Precedence < operatorStack.Peek().Precedence)) {
						operatorStack.Pop().Compile(compiler);
					}
					operatorStack.Push(operatorNode);
				}
			}
			while (operatorStack.Count > stackCount) {
				operatorStack.Pop().Compile(compiler);
			}
		}
		static sunExpressionFlags AnalyzeExpression(sunContext context, sunExpression expression) {
			var flags = sunExpressionFlags.None;
			foreach (var operand in expression.OfType<sunOperand>()) {
				var term = operand.Term as sunTerm;
				if (term != null) {
					flags |= term.GetExpressionFlags(context);
				}
			}
			return flags;
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return AnalyzeExpression(context, this);
		}
	}

	[Flags]
	enum sunExpressionFlags {
		None = 0,

		// contents
		Literals = 1 << 0,
		Variables = 1 << 1,
		Augments = 1 << 2,
		Calls = 1 << 3,
		Constants = 1 << 4,

		// description
		Dynamic = 1 << 5,
	}

	class sunOperand : sunNode {
		public sunNode UnaryOperators { get { return Count > 1 ? this[0] : null; } }
		public sunNode Term { get { return this[Count - 1]; } }

		public sunOperand(sunSourceLocation location)
			: base(location) { }

		// operands are compiled in sunExpression.Compile
	}

	class sunUnaryOperatorList : sunNode {
		public sunUnaryOperatorList(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			foreach (var child in this.Reverse()) {
				// compile unary operators in reverse order
				child.Compile(compiler);
			}
		}
	}

	class sunTernaryOperator : sunNode, sunTerm {
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunExpression TrueBody { get { return this[1] as sunExpression; } }
		public sunExpression FalseBody { get { return this[2] as sunExpression; } }

		public sunTernaryOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			Condition.Compile(compiler);
			var falsePrologue = compiler.Binary.WriteJNE();
			TrueBody.Compile(compiler);
			var trueEpilogue = compiler.Binary.WriteJMP();
			compiler.Binary.ClosePoint(falsePrologue);
			FalseBody.Compile(compiler);
			compiler.Binary.ClosePoint(trueEpilogue);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return Condition.Analyze(context) | TrueBody.Analyze(context) | FalseBody.Analyze(context);
		}
	}

	// increment/decrement
	class sunPostfixAugment : sunOperand, sunTerm {
		public sunIdentifier Variable { get { return this[0] as sunIdentifier; } }
		public sunAugment Augment { get { return this[1] as sunAugment; } }

		public sunPostfixAugment(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.MustResolveStorable(Variable);
			if (symbol is sunConstantSymbol) {
				throw new sunAssignConstantException(Variable);
			}
			if (Parent is sunOperand) {
				symbol.CompileGet(compiler);
			}
			Augment.Compile(compiler, symbol);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Augments;
		}
	}

	class sunPrefixAugment : sunOperand, sunTerm {
		public sunAugment Augment { get { return this[0] as sunAugment; } }
		public sunIdentifier Variable { get { return this[1] as sunIdentifier; } }

		public sunPrefixAugment(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.MustResolveStorable(Variable);
			if (symbol is sunConstantSymbol) {
				throw new sunAssignConstantException(Variable);
			}
			Augment.Compile(compiler, symbol);
			if (Parent is sunOperand) {
				symbol.CompileGet(compiler);
			}
		}
		
		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Augments;
		}
	}

	abstract class sunAugment : sunNode {
		protected sunAugment(sunSourceLocation location)
			: base(location) { }

		public abstract void Compile(sunCompiler compiler, sunStorableSymbol symbol);
	}

	class sunIncrement : sunAugment {
		public sunIncrement(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol) {
			symbol.CompileInc(compiler);
			symbol.CompileSet(compiler);
		}
	}

	class sunDecrement : sunAugment {
		public sunDecrement(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol) {
			symbol.CompileDec(compiler);
			symbol.CompileSet(compiler);
		}
	}
}
