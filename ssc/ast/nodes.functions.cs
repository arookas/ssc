using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunBuiltinDeclaration : sunNode {
		public sunIdentifier Builtin { get { return this[1] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[2] as sunParameterList; } }

		public sunBuiltinDeclaration(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.DeclareBuiltin(this);
		}
	}

	class sunFunctionDefinition : sunNode {
		public sunIdentifier Function { get { return this[1] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[2] as sunParameterList; } }
		public sunNode Body { get { return this[3]; } }

		public sunFunctionDefinition(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			// this defines the function in the context
			// it doesn't compile the definition body
			compiler.Context.DefineFunction(this);
		}
	}

	class sunFunctionCall : sunNode, sunTerm {
		public sunIdentifier Function { get { return this[0] as sunIdentifier; } }
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
			symbol.OpenCallSite(compiler, Arguments.Count);
			if (IsStatement) {
				compiler.Binary.WritePOP();
			}
		}
		
		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			return sunExpressionFlags.Calls | sunExpressionFlags.Dynamic;
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
