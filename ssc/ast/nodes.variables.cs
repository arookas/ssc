namespace arookas {
	class sunStorableReference : sunNode, sunTerm {
		public sunIdentifier Name { get { return this[0] as sunIdentifier; } }

		public sunStorableReference(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.MustResolveStorable(Name).Compile(compiler);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			var symbol = context.MustResolveStorable(Name);
			if (symbol is sunVariableSymbol) {
				return sunExpressionFlags.Variables | sunExpressionFlags.Dynamic;
			}
			else if (symbol is sunConstantSymbol) {
				return sunExpressionFlags.Constants;
			}
			return sunExpressionFlags.None;
		}
	}

	class sunVariableDeclaration : sunNode {
		public sunIdentifier Name { get { return this[1] as sunIdentifier; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunVariableDeclaration(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.DeclareVariable(this);
			symbol.Modifiers = Modifiers;
			if ((Modifiers & sunSymbolModifiers.Constant) != 0) {
				throw new sunInvalidModifierException(this[0]);
			}
		}
	}

	class sunVariableDefinition : sunNode {
		public sunIdentifier Name { get { return this[1] as sunIdentifier; } }
		public sunAssignOperator Operator { get { return this[2] as sunAssignOperator; } }
		public sunExpression Expression { get { return this[3] as sunExpression; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunVariableDefinition(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			// create the right type of symbol based on the const modifier
			var isConst = (Modifiers & sunSymbolModifiers.Constant) != 0;
			if (isConst) {
				// analyze the expression. this does two things:
				//   1) prevents recursion (i.e. the const referencing itself)
				//   2) asserts actual constness
				var flags = Expression.Analyze(compiler.Context);
				if (flags.HasFlag(sunExpressionFlags.Dynamic)) {
					throw new sunConstantExpressionException(Expression);
				}
				var symbol = compiler.Context.DeclareConstant(this);
				symbol.Modifiers = Modifiers;
			}
			else {
				var symbol = compiler.Context.DeclareVariable(this);
				symbol.Modifiers = Modifiers;
				Operator.Compile(compiler, symbol, Expression);
			}
		}
	}

	class sunStorableAssignment : sunNode {
		public sunIdentifier Name { get { return this[0] as sunIdentifier; } }
		public sunAssignOperator Operator { get { return this[1] as sunAssignOperator; } }
		public sunExpression Expression { get { return this[2] as sunExpression; } }

		public sunStorableAssignment(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.MustResolveStorable(Name);
			if (symbol is sunConstantSymbol) {
				throw new sunAssignConstantException(Name);
			}
			Operator.Compile(compiler, symbol, Expression);
		}
	}
}
