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
}
