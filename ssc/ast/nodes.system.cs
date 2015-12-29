namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Text.WriteFUNC((int)sunSystemBuiltins.Yield, 0);
			context.Text.WritePOP();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Text.WriteFUNC((int)sunSystemBuiltins.Exit, 0);
			context.Text.WritePOP();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Text.WriteFUNC((int)sunSystemBuiltins.Lock, 0);
			context.Text.WritePOP();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			context.Text.WriteFUNC((int)sunSystemBuiltins.Unlock, 0);
			context.Text.WritePOP();
		}
	}

	abstract class sunCast : sunNode, sunTerm {
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		protected sunCast(sunSourceLocation location)
			: base(location) { }

		protected void Compile(sunContext context, int index) {
			Argument.Compile(context);
			context.Text.WriteFUNC(index, 1);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Casts | Argument.Analyze(context);
		}
	}

	class sunIntCast : sunCast {
		public sunIntCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, (int)sunSystemBuiltins.Int);
		}
	}

	class sunFloatCast : sunCast {
		public sunFloatCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, (int)sunSystemBuiltins.Float);
		}
	}

	class sunTypeofCast : sunCast {
		public sunTypeofCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, (int)sunSystemBuiltins.Typeof);
		}
	}
}
