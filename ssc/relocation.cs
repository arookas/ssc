using System;

namespace arookas {
	abstract class sunRelocation {
		protected sunBinary mBinary;
		protected uint mPoint;

		protected sunRelocation(sunBinary binary) {
			if (binary == null) {
				throw new ArgumentNullException("binary");
			}
			mBinary = binary;
			mPoint = mBinary.Offset;
		}

		public abstract void Relocate();
	}

	abstract class sunSymbolRelocation<TSymbol> : sunRelocation where TSymbol : sunSymbol {
		protected TSymbol mSymbol;

		protected sunSymbolRelocation(sunBinary binary, TSymbol symbol)
			: base(binary) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			mSymbol = symbol;
		}
	}

	class sunBuiltinCallSite : sunSymbolRelocation<sunBuiltinSymbol> {
		int mArgCount;

		public sunBuiltinCallSite(sunBinary binary, sunBuiltinSymbol symbol, int argCount)
			: base (binary, symbol) {
			mArgCount = argCount;
			mBinary.WriteFUNC(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteFUNC(mSymbol.Index, mArgCount);
			mBinary.Back();
		}
	}

	class sunFunctionCallSite : sunSymbolRelocation<sunFunctionSymbol> {
		int mArgCount;

		public sunFunctionCallSite(sunBinary binary, sunFunctionSymbol symbol, int argCount)
			: base(binary, symbol) {
			mArgCount = argCount;
			mBinary.WriteCALL(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteCALL(mSymbol.Offset, mArgCount);
			mBinary.Back();
		}
	}

	class sunVariableGetSite : sunSymbolRelocation<sunVariableSymbol> {
		public sunVariableGetSite(sunBinary binary, sunVariableSymbol symbol)
			: base(binary, symbol) {
			mBinary.WriteVAR(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteVAR(mSymbol.Display, mSymbol.Index);
			mBinary.Back();
		}
	}

	class sunVariableSetSite : sunSymbolRelocation<sunVariableSymbol> {
		public sunVariableSetSite(sunBinary binary, sunVariableSymbol symbol)
			: base(binary, symbol) {
			mBinary.WriteASS(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteASS(mSymbol.Display, mSymbol.Index);
			mBinary.Back();
		}
	}

	class sunVariableIncSite : sunSymbolRelocation<sunVariableSymbol> {
		public sunVariableIncSite(sunBinary binary, sunVariableSymbol symbol)
			: base(binary, symbol) {
			mBinary.WriteINC(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteINC(mSymbol.Display, mSymbol.Index);
			mBinary.Back();
		}
	}

	class sunVariableDecSite : sunSymbolRelocation<sunVariableSymbol> {
		public sunVariableDecSite(sunBinary binary, sunVariableSymbol symbol)
			: base(binary, symbol) {
			mBinary.WriteDEC(0, 0);
		}

		public override void Relocate() {
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteDEC(mSymbol.Display, mSymbol.Index);
			mBinary.Back();
		}
	}

	class sunJumpNotEqualSite : sunRelocation {
		public sunJumpNotEqualSite(sunBinary binary)
			: base(binary) {
			mBinary.WriteJNE(0);
		}

		public override void Relocate() {
			var offset = mBinary.Offset;
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteJNE(offset);
			mBinary.Back();
		}
	}

	class sunJumpSite : sunRelocation {
		public sunJumpSite(sunBinary binary)
			: base(binary) {
			mBinary.WriteJMP(0);
		}

		public override void Relocate() {
			var offset = mBinary.Offset;
			mBinary.Keep();
			mBinary.Goto(mPoint);
			mBinary.WriteJMP(offset);
			mBinary.Back();
		}
	}
}
