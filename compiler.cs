using System;
using System.Diagnostics;
using System.IO;

namespace arookas
{
	public class sunCompiler
	{
		public sunCompilerResults Compile(string name, Stream output)
		{
			return Compile(name, output, sunImportResolver.Default);
		}
		public sunCompilerResults Compile(string name, Stream output, sunImportResolver resolver)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			if (resolver == null)
			{
				throw new ArgumentNullException("resolver");
			}
			var results = new sunCompilerResults();
			var timer = Stopwatch.StartNew();
			try
			{
				sunContext context = new sunContext(output, resolver);
				var result = context.Import(name);
				if (result != sunImportResult.Loaded)
				{
					throw new sunImportException(name, result);
				}
				context.Text.Terminate(); // NOTETOSELF: don't do this via sunNode.Compile because imported files will add this as well
				foreach (var function in context.SymbolTable.Functions)
				{
					function.Compile(context);
				}
				foreach (var function in context.SymbolTable.Functions)
				{
					function.CloseCallSites(context);
				}
				results.DataCount = context.DataTable.Count;
				results.SymbolCount = context.SymbolTable.Count;
				results.BuiltinCount = context.SymbolTable.BuiltinCount;
				results.FunctionCount = context.SymbolTable.FunctionCount;
				results.VariableCount = context.SymbolTable.VariableCount;
				context.Dispose();
			}
			catch (sunCompilerException ex)
			{
				results.Error = ex;
			}
			timer.Stop();
			results.CompileTime = timer.Elapsed;
			return results;
		}
	}

	public class sunCompilerResults
	{
		// success
		public bool Success { get { return Error == null; } }
		public sunCompilerException Error { get; internal set; }

		// statistics
		public int DataCount { get; internal set; }
		public int SymbolCount { get; internal set; }
		public int BuiltinCount { get; internal set; }
		public int FunctionCount { get; internal set; }
		public int VariableCount { get; internal set; }

		public TimeSpan CompileTime { get; internal set; }
	}
}
