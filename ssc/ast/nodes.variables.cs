namespace arookas
{
	class sunStorableReference : sunNode
	{
		public sunIdentifier Storable { get { return this[0] as sunIdentifier; } }

		public sunStorableReference(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.MustResolveStorable(Storable).Compile(context);
		}
	}

	class sunVariableDeclaration : sunNode
	{
		public sunIdentifier Storable { get { return this[0] as sunIdentifier; } }

		public sunVariableDeclaration(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.DeclareVariable(Storable);
		}
	}

	class sunVariableDefinition : sunVariableAssignment
	{
		public sunVariableDefinition(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.DeclareVariable(Storable);
			base.Compile(context);
		}
	}

	class sunVariableAssignment : sunVariableDeclaration
	{
		public sunAssign Operator { get { return this[1] as sunAssign; } }
		public sunExpression Expression { get { return this[2] as sunExpression; } }

		public sunVariableAssignment(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var symbol = context.MustResolveStorable(Storable);
			if (symbol is sunConstantSymbol)
			{
				throw new sunAssignConstantException(Storable);
			}
			Operator.Compile(context, symbol as sunVariableSymbol, Expression);
		}
	}

	class sunConstDefinition : sunNode
	{
		public sunIdentifier Constant { get { return this[0] as sunIdentifier; } }
		public sunExpression Expression { get { return this[2] as sunExpression; } }

		public sunConstDefinition(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.DeclareConstant(Constant, Expression);
		}
	}
}
