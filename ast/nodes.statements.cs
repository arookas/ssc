namespace arookas
{
	class sunStatementBlock : sunNode
	{
		public sunStatementBlock(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.Scopes.Push();
			base.Compile(context);
			context.Scopes.Pop();
		}
	}

	class sunImport : sunNode
	{
		public sunStringLiteral ImportFile { get { return this[0] as sunStringLiteral; } }

		public sunImport(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var file = context.Imports.ResolveImport(this);
			if (file == null)
			{
				return; // the file has already been imported
			}
			context.Compile(file);
		}
	}

	class sunNameLabel : sunNode
	{
		public sunIdentifier Label { get { return this[0] as sunIdentifier; } }

		public sunNameLabel(sunSourceLocation location)
			: base(location)
		{

		}
	}
}
