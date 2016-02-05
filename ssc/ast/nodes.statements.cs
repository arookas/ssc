namespace arookas {
	class sunCompoundStatement : sunNode {
		public sunCompoundStatement(sunSourceLocation location)
			: base(location) { }
	}

	class sunStatementBlock : sunNode {
		public sunStatementBlock(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
#if SSC_SCOPES
			compiler.Context.Scopes.Push(compiler.Context.Scopes.Top.Type);
#endif
			base.Compile(compiler);
#if SSC_SCOPES
			compiler.Context.Scopes.Pop();
#endif
		}
	}

	class sunImport : sunNode {
		public sunStringLiteral ImportFile { get { return this[0] as sunStringLiteral; } }

		public sunImport(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			var result = compiler.Import(ImportFile.Value);
			switch (result) {
				case sunImportResult.Missing:
				case sunImportResult.FailedToLoad: {
					throw new sunMissingImportException(this);
				}
			}
		}
	}

	class sunNameLabel : sunNode {
		public sunIdentifier Label { get { return this[0] as sunIdentifier; } }

		public sunNameLabel(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) {
			compiler.Context.PushNameLabel(this);
		}
	}
}
