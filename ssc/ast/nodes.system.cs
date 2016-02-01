namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.Yield.OpenCallSite(compiler, 0);
			compiler.Binary.WritePOP();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.Exit.OpenCallSite(compiler, 0);
			compiler.Binary.WritePOP();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.Lock.OpenCallSite(compiler, 0);
			compiler.Binary.WritePOP();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.Unlock.OpenCallSite(compiler, 0);
			compiler.Binary.WritePOP();
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

		public override void Compile(sunCompiler compiler) {
			Argument.Compile(compiler);
			compiler.Context.Int.OpenCallSite(compiler, 1);
		}
	}

	class sunFloatCast : sunCast {
		public sunFloatCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			Argument.Compile(compiler);
			compiler.Context.Float.OpenCallSite(compiler, 1);
		}
	}

	class sunTypeofCast : sunCast {
		public sunTypeofCast(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			Argument.Compile(compiler);
			compiler.Context.Typeof.OpenCallSite(compiler, 1);
		}
	}
}
