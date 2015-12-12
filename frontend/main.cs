using System;
using System.Diagnostics;
using System.IO;

namespace arookas
{
	static class SSC
	{
		static void Main(string[] args)
		{
			Message("ssc v0.1 arookas\n");
			var cmd = new CommandLine(args);
			if (cmd.Count == 0)
			{
				Message("Usage: ssc -input <input.sun> [-output <output.sb>]\n");
				Pause();
				Exit(1);
			}
			var compiler = new sunCompiler();
			int exitCode = 0;
			string inputFile, outputFile;
			ReadCmdLine(cmd, out inputFile, out outputFile);
			using (var output = OpenOutput(outputFile))
			{
				var results = compiler.Compile(inputFile, output);
				if (results.Success)
				{
					Message("Finished compiling in {0:F2}ms.\n", results.CompileTime.TotalMilliseconds);
					Message("  Data count: {0}\n", results.DataCount);
					Message("Symbol count: {0}\n", results.SymbolCount);
					Message(" -  builtins: {0}\n", results.BuiltinCount);
					Message(" - functions: {0}\n", results.FunctionCount);
					Message(" - variables: {0}\n", results.VariableCount);
				}
				else
				{
					if (results.Error is sunScriptException)
					{
						var error = results.Error as sunScriptException;
						Error("  \"{0}\"\n  pos ({1}, {2})\n{3}", error.Location.File, error.Location.Line, error.Location.Column, error.Message);
						exitCode = 1;
					}
					else
					{
						var error = results.Error;
						Error("{0}", error.Message);
						exitCode = 1;
					}
				}
			}
			Pause();
			Exit(exitCode);
		}

		static Stream OpenOutput(string path)
		{
			try
			{
				return File.Create(path);
			}
			catch
			{
				Error("Failed to create output file '{0}'.", path);
				Pause();
				Exit(1);
			}
			return null;
		}

		static void ReadCmdLine(CommandLine cmd, out string inputFile, out string outputFile)
		{
			inputFile = null;
			outputFile = null;
			foreach (var prm in cmd)
			{
				switch (prm.Name)
				{
					case "-input": GetInput(prm, ref inputFile); break;
					case "-output": GetOutput(prm, ref outputFile); break;
				}
			}
			if (inputFile == null)
			{
				Error("Missing -input option.\n");
				Pause();
				Exit(1);
			}
			if (outputFile == null)
			{
				outputFile = Path.ChangeExtension(inputFile, ".sb");
			}
		}
		static void GetInput(CommandLineParameter prm, ref string inputFile)
		{
			if (inputFile != null)
			{
				Error("Only one -input option is allowed.\n");
				Pause();
				Exit(1);
			}
			if (prm.Count != 1)
			{
				Error("Incorrect number of arguments in -input option.\n");
				Pause();
				Exit(1);
			}
			inputFile = prm[0];
		}
		static void GetOutput(CommandLineParameter prm, ref string outputFile)
		{
			if (outputFile != null)
			{
				Error("Only one -output option is allowed.\n");
				Pause();
				Exit(1);
			}
			if (prm.Count != 1)
			{
				Error("Incorrect number of arguments in -output option.\n");
				Pause();
				Exit(1);
			}
			outputFile = prm[0];
		}

		static void Message(string format, params object[] args) { Console.Write(format, args); }
		static void Warning(string format, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING:\n");
			Message(format, args);
			Console.ResetColor();
		}
		static void Error(string format, params object[] args)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR:\n");
			Message(format, args);
			Console.ResetColor();
		}
		[Conditional("DEBUG")] static void Pause()
		{
			Console.ReadKey();
		}
		static void Exit(int code) { Environment.Exit(code); }
	}
}
