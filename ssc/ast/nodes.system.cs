namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("yield");
			context.Text.WriteFUNC(builtinInfo.Index, 0);
			context.Text.WritePOP();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("exit");
			context.Text.WriteFUNC(builtinInfo.Index, 0);
			context.Text.WritePOP();
			context.Text.WritePOP();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("lock");
			context.Text.WriteFUNC(builtinInfo.Index, 0);
			context.Text.WritePOP();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("unlock");
			context.Text.WriteFUNC(builtinInfo.Index, 0);
			context.Text.WritePOP();
		}
	}

	abstract class sunCast : sunNode, sunTerm {
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		protected sunCast(sunSourceLocation location)
			: base(location) { }

		protected void Compile(sunContext context, sunBuiltinSymbol symbol) {
			Argument.Compile(context);
			context.Text.WriteFUNC(symbol.Index, 1);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Casts | Argument.Analyze(context);
		}
	}

	class sunIntCast : sunCast {
		public sunIntCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, context.ResolveSystemBuiltin("int"));
		}
	}

	class sunFloatCast : sunCast {
		public sunFloatCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, context.ResolveSystemBuiltin("float"));
		}
	}

	class sunTypeofCast : sunCast {
		public sunTypeofCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			Compile(context, context.ResolveSystemBuiltin("typeof"));
		}
	}
}
