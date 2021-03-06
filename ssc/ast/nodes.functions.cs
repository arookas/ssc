﻿using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunBuiltinDeclaration : sunNode {
		public sunIdentifier Name { get { return this[1] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[2] as sunParameterList; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunBuiltinDeclaration(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.DeclareBuiltin(this);
			symbol.Modifiers = Modifiers;
			if ((symbol.Modifiers & sunSymbolModifiers.Local) != 0) {
				throw new sunInvalidModifierException(this[0]); // local builtins are not supported
			}
		}
	}

	class sunFunctionDefinition : sunNode {
		public sunIdentifier Name { get { return this[1] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[2] as sunParameterList; } }
		public sunNode Body { get { return this[3]; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunFunctionDefinition(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			// this defines the function in the context
			// it doesn't compile the definition body
			var symbol = compiler.Context.DefineFunction(this);
			symbol.Modifiers = Modifiers;
		}
	}

	class sunFunctionCall : sunNode, sunTerm {
		public sunIdentifier Name { get { return this[0] as sunIdentifier; } }
		public sunNode Arguments { get { return this[1] as sunNode; } }

		bool IsStatement { get { return !(Parent is sunOperand); } }

		public sunFunctionCall(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.MustResolveCallable(this);
			if (!symbol.Parameters.ValidateArgumentCount(Arguments.Count)) {
				throw new sunArgumentCountException(this, symbol);
			}
			Arguments.Compile(compiler);
			symbol.OpenRelocation(symbol.CreateCallSite(compiler, Arguments.Count));
			if (IsStatement) {
				compiler.Binary.WritePOP();
			}
		}
		
		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			var symbol = context.MustResolveCallable(this);
			var flags = sunExpressionFlags.Calls;
			if ((symbol.Modifiers & sunSymbolModifiers.Constant) == 0) {
				flags |= sunExpressionFlags.Dynamic;
			}
			return flags;
		}
	}

	class sunParameterList : sunNode {
		public IEnumerable<sunIdentifier> Parameters { get { return this.OfType<sunIdentifier>(); } }
		public bool IsVariadic { get { return Count > 0 && this[Count - 1] is sunEllipsis; } }
		public sunParameterInfo ParameterInfo { get { return new sunParameterInfo(Parameters, IsVariadic); } }

		public sunParameterList(sunSourceLocation location)
			: base(location) { }
	}

	class sunEllipsis : sunNode {
		public sunEllipsis(sunSourceLocation location)
			: base(location) { }
	}
}
