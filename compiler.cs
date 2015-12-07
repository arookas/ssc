using PerCederberg.Grammatica.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace arookas
{
	public class sunCompiler
	{
		Stack<string> files;
		string defaultRootDir, rootDir;
		public string RootDir
		{
			get { return rootDir ?? defaultRootDir; }
			set { rootDir = value; }
		}

		public sunCompiler()
		{
			defaultRootDir = AppDomain.CurrentDomain.BaseDirectory;
		}

		public sunCompilerResults Compile(string file, Stream output)
		{
			var results = new sunCompilerResults();
			var timer = Stopwatch.StartNew();
			try
			{
				files = new Stack<string>(5);
				sunContext context = new sunContext(output, RootDir);
				context.EnterFile += EnterFile;
				context.ExitFile += ExitFile;
				context.Compile(file);
				context.Text.Terminate(); // NOTETOSELF: don't do this in sunScript because imported files will add this as well
				foreach (var function in context.SymbolTable.Functions)
				{
					function.Compile(context);
				}
				foreach (var function in context.SymbolTable.Functions)
				{
					function.CloseCallSites(context);
				}
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
			catch (ParserLogException ex)
			{
				results.Error = new sunParserException(files.Peek(), ex[0]);
			}
			timer.Stop();
			results.CompileTime = timer.Elapsed;
			return results;
		}

		void EnterFile(object sender, sunFileArgs e) { files.Push(e.File); }
		void ExitFile(object sender, sunFileArgs e) { files.Pop(); }
	}

	public class sunCompilerResults
	{
		// success
		public bool Success { get { return Error == null; } }
		public sunCompilerException Error { get; internal set; }

		// statistics
		public int SymbolCount { get; internal set; }
		public int BuiltinCount { get; internal set; }
		public int FunctionCount { get; internal set; }
		public int VariableCount { get; internal set; }

		public TimeSpan CompileTime { get; internal set; }
	}
}
