using PerCederberg.Grammatica.Runtime;
using System.Linq;

namespace arookas {
	class sunIf : sunNode {
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunNode TrueBody { get { return this[1]; } }
		public sunNode FalseBody { get { return this[2]; } }

		public sunIf(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Condition.Compile(context);
			var trueBodyEpilogue = context.Text.WriteJNE();
			TrueBody.Compile(context);
			var falseBody = FalseBody;
			if (falseBody != null) {
				var falseBodyEpilogue = context.Text.WriteJMP();
				context.Text.ClosePoint(trueBodyEpilogue);
				falseBody.Compile(context);
				context.Text.ClosePoint(falseBodyEpilogue);
			}
			else {
				context.Text.ClosePoint(trueBodyEpilogue);
			}
		}
	}

	abstract class sunLoop : sunNode {
		protected sunLoop(sunSourceLocation location)
			: base(location) { }

		public void PushLoop(sunContext context) {
			var name = context.PopNameLabel();
			if (name == null) {
				context.Loops.Push();
			}
			else {
				context.Loops.Push(name.Label.Value);
			}
		}
	}

	class sunWhile : sunLoop {
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunNode Body { get { return this[1]; } }

		public sunWhile(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			PushLoop(context);
			var bodyPrologue = context.Text.OpenPoint();
			var continuePoint = context.Text.OpenPoint();
			Condition.Compile(context);
			var bodyEpilogue = context.Text.WriteJNE();
			Body.Compile(context);
			context.Text.WriteJMP(bodyPrologue);
			context.Text.ClosePoint(bodyEpilogue);
			var breakPoint = context.Text.OpenPoint();
			context.Loops.Pop(context, breakPoint, continuePoint);
		}
	}

	class sunDo : sunLoop {
		public sunNode Body { get { return this[0]; } }
		public sunExpression Condition { get { return this[1] as sunExpression; } }

		public sunDo(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			PushLoop(context);
			var bodyPrologue = context.Text.OpenPoint();
			Body.Compile(context);
			var continuePoint = context.Text.OpenPoint();
			Condition.Compile(context);
			var bodyEpilogue = context.Text.WriteJNE();
			context.Text.WriteJMP(bodyPrologue);
			context.Text.ClosePoint(bodyEpilogue);
			var breakPoint = context.Text.OpenPoint();
			context.Loops.Pop(context, breakPoint, continuePoint);
		}
	}

	class sunFor : sunLoop {
		public sunForDeclaration Declaration { get { return this.FirstOrDefault(i => i is sunForDeclaration) as sunForDeclaration; } }
		public sunForCondition Condition { get { return this.FirstOrDefault(i => i is sunForCondition) as sunForCondition; } }
		public sunForIteration Iteration { get { return this.FirstOrDefault(i => i is sunForIteration) as sunForIteration; } }
		public sunNode Body { get { return this[Count - 1]; } }

		public sunFor(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Scopes.Push();
			PushLoop(context);
			TryCompile(Declaration, context);
			var bodyPrologue = context.Text.OpenPoint();
			TryCompile(Condition, context);
			var bodyEpilogue = context.Text.WriteJNE();
			Body.Compile(context);
			var continuePoint = context.Text.OpenPoint();
			TryCompile(Iteration, context);
			context.Text.WriteJMP(bodyPrologue);
			context.Text.ClosePoint(bodyEpilogue);
			var breakPoint = context.Text.OpenPoint();
			context.Loops.Pop(context, breakPoint, continuePoint);
			context.Scopes.Pop();
		}
	}
	class sunForDeclaration : sunNode {
		public sunForDeclaration(sunSourceLocation location)
			: base(location) { }
	}
	class sunForCondition : sunNode {
		public sunForCondition(sunSourceLocation location)
			: base(location) { }
	}
	class sunForIteration : sunNode {
		public sunForIteration(sunSourceLocation location)
			: base(location) { }
	}

	class sunReturn : sunNode {
		public sunExpression Expression { get { return this[0] as sunExpression; } }

		public sunReturn(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var expression = Expression;
			if (expression != null) {
				expression.Compile(context);
				context.Text.WriteRET();
			}
			else {
				context.Text.WriteRET0();
			}
		}
	}

	class sunBreak : sunNode {
		public bool IsNamed { get { return Count > 0; } }
		public sunIdentifier NameLabel { get { return this[0] as sunIdentifier; } }

		public sunBreak(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var point = context.Text.WriteJMP();
			if (!context.Loops.AddBreak(point, IsNamed ? NameLabel.Value : null)) {
				throw new sunBreakException(this);
			}
		}
	}

	class sunContinue : sunNode {
		public bool IsNamed { get { return Count > 0; } }
		public sunIdentifier NameLabel { get { return this[0] as sunIdentifier; } }

		public sunContinue(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var point = context.Text.WriteJMP();
			if (!context.Loops.AddContinue(point, IsNamed ? NameLabel.Value : null)) {
				throw new sunContinueException(this);
			}
		}
	}
}
