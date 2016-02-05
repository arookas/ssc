using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunIf : sunNode {
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunNode TrueBody { get { return this[1]; } }
		public sunNode FalseBody { get { return this[2]; } }

		public sunIf(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			Condition.Compile(compiler);
			var trueBodyEpilogue = compiler.Binary.WriteJNE();
			TrueBody.Compile(compiler);
			var falseBody = FalseBody;
			if (falseBody != null) {
				var falseBodyEpilogue = compiler.Binary.WriteJMP();
				compiler.Binary.ClosePoint(trueBodyEpilogue);
				falseBody.Compile(compiler);
				compiler.Binary.ClosePoint(falseBodyEpilogue);
			}
			else {
				compiler.Binary.ClosePoint(trueBodyEpilogue);
			}
		}
	}

	abstract class sunLoopNode : sunNode {
		protected sunLoopNode(sunSourceLocation location)
			: base(location) { }

		protected sunLoop PushLoop(sunContext context) {
			var name = context.PopNameLabel();
			if (name == null) {
				return context.Loops.Push();
			}
			return context.Loops.Push(name.Label.Value);
		}
		protected sunLoop PushLoop(sunContext context, sunLoopFlags flags) {
			var name = context.PopNameLabel();
			if (name == null) {
				return context.Loops.Push(flags);
			}
			return context.Loops.Push(name.Label.Value, flags);
		}
	}

	class sunWhile : sunLoopNode {
		public sunExpression Condition { get { return this[0] as sunExpression; } }
		public sunNode Body { get { return this[1]; } }

		public sunWhile(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var loop = PushLoop(compiler.Context);
			var bodyPrologue = compiler.Binary.OpenPoint();
			loop.ContinuePoint = compiler.Binary.OpenPoint();
			Condition.Compile(compiler);
			var bodyEpilogue = compiler.Binary.WriteJNE();
			Body.Compile(compiler);
			compiler.Binary.WriteJMP(bodyPrologue);
			compiler.Binary.ClosePoint(bodyEpilogue);
			loop.BreakPoint = compiler.Binary.OpenPoint();
			compiler.Context.Loops.Pop(compiler);
		}
	}

	class sunDo : sunLoopNode {
		public sunNode Body { get { return this[0]; } }
		public sunExpression Condition { get { return this[1] as sunExpression; } }

		public sunDo(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var loop = PushLoop(compiler.Context);
			var bodyPrologue = compiler.Binary.OpenPoint();
			Body.Compile(compiler);
			loop.ContinuePoint = compiler.Binary.OpenPoint();
			Condition.Compile(compiler);
			var bodyEpilogue = compiler.Binary.WriteJNE();
			compiler.Binary.WriteJMP(bodyPrologue);
			compiler.Binary.ClosePoint(bodyEpilogue);
			loop.BreakPoint = compiler.Binary.OpenPoint();
			compiler.Context.Loops.Pop(compiler);
		}
	}

	class sunFor : sunLoopNode {
		public sunForDeclaration Declaration { get { return this.FirstOrDefault(i => i is sunForDeclaration) as sunForDeclaration; } }
		public sunForCondition Condition { get { return this.FirstOrDefault(i => i is sunForCondition) as sunForCondition; } }
		public sunForIteration Iteration { get { return this.FirstOrDefault(i => i is sunForIteration) as sunForIteration; } }
		public sunNode Body { get { return this[Count - 1]; } }

		public sunFor(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
#if SSC_SCOPES
			compiler.Context.Scopes.Push();
#endif
			var loop = PushLoop(compiler.Context);
			TryCompile(Declaration, compiler);
			var bodyPrologue = compiler.Binary.OpenPoint();
			TryCompile(Condition, compiler);
			var bodyEpilogue = compiler.Binary.WriteJNE();
			Body.Compile(compiler);
			loop.ContinuePoint = compiler.Binary.OpenPoint();
			TryCompile(Iteration, compiler);
			compiler.Binary.WriteJMP(bodyPrologue);
			compiler.Binary.ClosePoint(bodyEpilogue);
			loop.BreakPoint = compiler.Binary.OpenPoint();
			compiler.Context.Loops.Pop(compiler);
#if SSC_SCOPES
			compiler.Context.Scopes.Pop();
#endif
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

		public override void Compile(sunCompiler compiler) {
			var expression = Expression;
			if (expression != null) {
				expression.Compile(compiler);
				compiler.Binary.WriteRET();
			}
			else {
				compiler.Binary.WriteRET0();
			}
		}
	}

	class sunBreak : sunNode {
		public bool IsNamed { get { return Count > 0; } }
		public sunIdentifier NameLabel { get { return this[0] as sunIdentifier; } }

		public sunBreak(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var point = compiler.Binary.WriteJMP();
			var success = true;
			if (IsNamed) {
				success = compiler.Context.Loops.AddBreak(point, NameLabel.Value);
			}
			else {
				success = compiler.Context.Loops.AddBreak(point);
			}
			if (!success) {
				throw new sunBreakException(this);
			}
		}
	}

	class sunContinue : sunNode {
		public bool IsNamed { get { return Count > 0; } }
		public sunIdentifier NameLabel { get { return this[0] as sunIdentifier; } }

		public sunContinue(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var point = compiler.Binary.WriteJMP();
			var success = true;
			if (IsNamed) {
				success = compiler.Context.Loops.AddContinue(point, NameLabel.Value);
			}
			else {
				success = compiler.Context.Loops.AddContinue(point);
			}
			if (!success) {
				throw new sunContinueException(this);
			}
		}
	}
}
