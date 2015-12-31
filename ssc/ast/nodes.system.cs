namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Yield.OpenCallSite(context, 0);
			context.Text.WritePOP();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Exit.OpenCallSite(context, 0);
			context.Text.WritePOP();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Lock.OpenCallSite(context, 0);
			context.Text.WritePOP();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Unlock.OpenCallSite(context, 0);
			context.Text.WritePOP();
		}
	}

	abstract class sunCast : sunNode, sunTerm {
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		protected sunCast(sunSourceLocation location)
			: base(location) { }

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Casts | Argument.Analyze(context);
		}
	}

	class sunIntCast : sunCast {
		public sunIntCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Argument.Compile(context);
			context.Int.OpenCallSite(context, 1);
		}
	}

	class sunFloatCast : sunCast {
		public sunFloatCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Argument.Compile(context);
			context.Float.OpenCallSite(context, 1);
		}
	}

	class sunTypeofCast : sunCast {
		public sunTypeofCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Argument.Compile(context);
			context.Typeof.OpenCallSite(context, 1);
		}
	}
}
