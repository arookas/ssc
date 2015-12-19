namespace arookas
{
	class sunVariableReference : sunNode
	{
		public sunIdentifier Variable { get { return this[0] as sunIdentifier; } }

		public sunVariableReference(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			sunVariableInfo variableInfo;
			sunConstInfo constInfo;
			context.ResolveVariableOrConstant(Variable, out variableInfo, out constInfo);
			if (variableInfo != null)
			{
				context.Text.PushVariable(variableInfo);
			}
			if (constInfo != null)
			{
				constInfo.Expression.Compile(context);
			}
		}
	}

	class sunVariableDeclaration : sunNode
	{
		public sunIdentifier Variable { get { return this[0] as sunIdentifier; } }

		public sunVariableDeclaration(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var variableInfo = context.DeclareVariable(Variable);
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
			var variableInfo = context.DeclareVariable(Variable);
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
			var variableInfo = context.ResolveVariable(Variable);
			Operator.Compile(context, variableInfo, Expression);
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
			var constInfo = context.DeclareConstant(Constant, Expression);
		}
	}
}
