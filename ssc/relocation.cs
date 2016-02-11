using System;

namespace arookas {
	abstract class sunRelocation {
		protected sunPoint mPoint;

		public abstract void Relocate(sunCompiler compiler);
	}

	abstract class sunSymbolRelocation<TSymbol> : sunRelocation where TSymbol : sunSymbol {
		protected TSymbol mSymbol;

		protected sunRelocation(TSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			mSymbol = symbol;
		}
	}

	class sunBuiltinCallSite : sunSymbolRelocation<sunBuiltinSymbol> {
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

		public sunVariableGetSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
	class sunVariableGetSite : sunSymbolRelocation<sunVariableSymbol> {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteVAR(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteVAR(mSymbol.Display, mSymbol.Index);
		}
	}

		public sunVariableSetSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
	class sunVariableSetSite : sunSymbolRelocation<sunVariableSymbol> {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteASS(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteASS(mSymbol.Display, mSymbol.Index);
		}
	}

		public sunVariableIncSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
	class sunVariableIncSite : sunSymbolRelocation<sunVariableSymbol> {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteINC(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteINC(mSymbol.Display, mSymbol.Index);
		}
	}

		public sunVariableDecSite(sunVariableSymbol symbol, sunCompiler compiler)
			: base(symbol) {
	class sunVariableDecSite : sunSymbolRelocation<sunVariableSymbol> {
			mPoint = compiler.Binary.OpenPoint();
			compiler.Binary.WriteDEC(0, 0);
		}

		public override void Relocate(sunCompiler compiler) {
			compiler.Binary.Goto(mPoint);
			compiler.Binary.WriteDEC(mSymbol.Display, mSymbol.Index);
		}
	}
}
