using System;
using System.IO;

namespace arookas
{
	static class SSC
	{
		public static void Main(string[] args)
		{
			string inFile = args[0];
			var compiler = new sunCompiler();
			using (var output = OpenWrite(Path.ChangeExtension(inFile, ".sb")))
			{
				var results = compiler.Compile(inFile, output);
				if (!results.Success)
				{
					if (results.Error is sunScriptException)
					{
						var error = results.Error as sunScriptException;
						Console.WriteLine("ERROR:\n  \"{0}\"\n  pos ({1}, {2})\n{3}", error.Location.File, error.Location.Line, error.Location.Column, error.Message);
						Console.ReadKey();
						return;
					}
					else
					{
						var error = results.Error;
						Console.WriteLine("ERROR:\n", error.Message);
						Console.ReadKey();
						return;
					}
				}
				Console.WriteLine("Finished compiling in {0:F2}ms.", results.CompileTime.TotalMilliseconds);
				Console.WriteLine("Symbol count: {0}", results.SymbolCount);
				Console.WriteLine(" -  builtins: {0}", results.BuiltinCount);
				Console.WriteLine(" - functions: {0}", results.FunctionCount);
				Console.WriteLine(" - variables: {0}", results.VariableCount);
				Console.ReadKey();
			}
		}

		static FileStream OpenRead(string path)
		{
			try
			{
				return File.OpenRead(path);
			}
			catch
			{
				Console.WriteLine("Failed to open the file '{0}'.\nPlease make sure the file exists and is not currently in use.", Path.GetFileName(path));
				Console.ReadKey();
				Environment.Exit(1);
				return null;
			}
		}
		static FileStream OpenWrite(string path)
		{
			try
			{
				return File.Create(path);
			}
			catch
			{
				Console.WriteLine("Failed to create the file '{0}'.", Path.GetFileName(path));
				Console.ReadKey();
				Environment.Exit(1);
				return null;
			}
		}
	}
}
