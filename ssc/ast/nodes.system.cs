namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("yield");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("exit");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunDump : sunNode {
		public sunDump(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("dump");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("lock");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("unlock");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	abstract class sunCast : sunNode, sunTerm {
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		protected sunCast(sunSourceLocation location)
			: base(location) { }

		protected void Compile(sunContext context, sunBuiltinSymbol symbol) {
			Argument.Compile(context);
			context.Text.CallBuiltin(symbol.Index, 1);
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

	class sunPrint : sunNode {
		public sunNode ArgumentList { get { return this[0]; } }

		public sunPrint(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunContext context) {
			var builtinInfo = context.ResolveSystemBuiltin("print");
			ArgumentList.Compile(context);
			context.Text.CallBuiltin(builtinInfo.Index, ArgumentList.Count);
			context.Text.Pop();
		}
	}
}
