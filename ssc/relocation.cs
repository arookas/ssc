using System;

namespace arookas {
	abstract class sunRelocation {
		protected sunPoint mPoint;

		public abstract void Relocate(sunCompiler compiler);
	}

	abstract class sunRelocation<TSymbol> : sunRelocation where TSymbol : sunSymbol {
		protected TSymbol mSymbol;

		protected sunRelocation(TSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			mSymbol = symbol;
		}
	}

	class sunBuiltinCallSite : sunRelocation<sunBuiltinSymbol> {
		int mArgCount;

		public sunBuiltinCallSite(sunBuiltinSymbol symbol, sunCompiler compiler, int argCount)
			: base (symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteFUNC(0, 0);
			mArgCount = argCount;
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteFUNC(mSymbol.Index, mArgCount);
		}
	}

	class sunFunctionCallSite : sunRelocation<sunFunctionSymbol> {
		int mArgCount;

			public sunFunctionCallSite(sunFunctionSymbol symbol, sunCompiler compiler, int argCount)
				: base(symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteCALL(0, 0);
			mArgCount = argCount;
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteCALL(mSymbol.Offset, mArgCount);
		}
	}

	class sunVariableGetSite : sunRelocation<sunVariableSymbol> {
		public sunVariableGetSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteVAR(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteVAR(mSymbol.Display, mSymbol.Index);
		}
	}

	class sunVariableSetSite : sunRelocation<sunVariableSymbol> {
		public sunVariableSetSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteASS(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteASS(mSymbol.Display, mSymbol.Index);
		}
	}

	class sunVariableIncSite : sunRelocation<sunVariableSymbol> {
		public sunVariableIncSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteINC(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteINC(mSymbol.Display, mSymbol.Index);
		}
	}

	class sunVariableDecSite : sunRelocation<sunVariableSymbol> {
		public sunVariableDecSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteDEC(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteDEC(mSymbol.Display, mSymbol.Index);
		}
	}
}
