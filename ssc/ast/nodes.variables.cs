namespace arookas {
	class sunStorableReference : sunNode, sunTerm {
		public sunIdentifier Storable { get { return this[0] as sunIdentifier; } }

		public sunStorableReference(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.MustResolveStorable(Storable).Compile(compiler);
		}

		sunExpressionFlags sunTerm.GetExpressionFlags(sunContext context) {
			var symbol = context.MustResolveStorable(Storable);
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
		public sunIdentifier Variable { get { return this[1] as sunIdentifier; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunVariableDeclaration(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.DeclareVariable(Variable);
		}
	}

	class sunVariableDefinition : sunNode {
		public sunIdentifier Variable { get { return this[1] as sunIdentifier; } }
		public sunAssign Operator { get { return this[2] as sunAssign; } }
		public sunExpression Expression { get { return this[3] as sunExpression; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]); }
		}

		public sunVariableDefinition(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.DeclareVariable(Variable);
			Operator.Compile(compiler, symbol, Expression);
		}
	}

	class sunStorableAssignment : sunNode {
		public sunIdentifier Storable { get { return this[1] as sunIdentifier; } }
		public sunAssign Operator { get { return this[2] as sunAssign; } }
		public sunExpression Expression { get { return this[3] as sunExpression; } }

		public sunStorableAssignment(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var symbol = compiler.Context.MustResolveStorable(Storable);
			if (symbol is sunConstantSymbol) {
				throw new sunAssignConstantException(Storable);
			}
			Operator.Compile(compiler, symbol, Expression);
		}
	}

	class sunConstantDefinition : sunNode {
		public sunIdentifier Constant { get { return this[1] as sunIdentifier; } }
		public sunExpression Expression { get { return this[3] as sunExpression; } }

		public sunSymbolModifiers Modifiers {
			get { return sunSymbol.GetModifiers(this[0]) | sunSymbolModifiers.Constant; }
		}

		public sunConstantDefinition(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			// analyze the expression. this does two things:
			//   1) prevents recursion (i.e. the const referencing itself)
			//   2) asserts actual constness
			var flags = Expression.Analyze(compiler.Context);
			if (flags.HasFlag(sunExpressionFlags.Dynamic)) {
				throw new sunConstantExpressionException(Expression);
			}
			compiler.Context.DeclareConstant(Constant, Expression);
		}
	}
}
