namespace arookas {
	class sunYield : sunNode {
		public sunYield(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.Yield;
			var site = symbol.CreateCallSite(compiler, 0);
			symbol.OpenRelocation(site);
			compiler.Binary.WritePOP();
		}
	}

	class sunExit : sunNode {
		public sunExit(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.Exit;
			var site = symbol.CreateCallSite(compiler, 0);
			symbol.OpenRelocation(site);
			compiler.Binary.WritePOP();
		}
	}

	class sunLock : sunNode {
		public sunLock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.Lock;
			var site = symbol.CreateCallSite(compiler, 0);
			symbol.OpenRelocation(site);
			compiler.Binary.WritePOP();
		}
	}

	class sunUnlock : sunNode {
		public sunUnlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.Unlock;
			var site = symbol.CreateCallSite(compiler, 0);
			symbol.OpenRelocation(site);
			compiler.Binary.WritePOP();
		}
	}
}
