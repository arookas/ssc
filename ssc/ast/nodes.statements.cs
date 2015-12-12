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
			context.Scopes.Push(context.Scopes.Top.Type);
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
			var result = context.Import(ImportFile.Value);
			switch (result)
			{
				case sunImportResult.Missing:
				case sunImportResult.FailedToLoad: throw new sunMissingImportException(this);
			}
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
