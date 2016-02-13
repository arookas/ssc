using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace arookas
{
	static class SSC {
		static void Main(string[] args) {
			Message("ssc v0.1 arookas\n");
			var cmd = new aCommandLine(args);
			if (cmd.Count == 0) {
				Message("Usage: ssc -input <input.sun> [-output <output.sb>]\n");
				Pause();
				Exit(1);
			}
			var compiler = new sunCompiler();
			int exitCode = 0;
			string inputFile, outputFile;
			ReadCmdLine(cmd, out inputFile, out outputFile);
			using (var output = OpenOutput(outputFile)) {
				var results = compiler.Compile(inputFile, output);
				if (results.Success) {
					Message("Finished compiling in {0:F2}ms.\n", results.CompileTime.TotalMilliseconds);
					Message("  Data count: {0}\n", results.DataCount);
					Message("Symbol count: {0}\n", results.SymbolCount);
					Message(" -  builtins: {0}\n", results.BuiltinCount);
					Message(" - functions: {0}\n", results.FunctionCount);
					Message(" - variables: {0}\n", results.VariableCount);
				}
				else {
					if (results.Error is sunSourceException) {
						var error = results.Error as sunSourceException;
						Error("  in file \"{0}\"\n  at line {1}, col {2}\n\n{3}{4}", error.Location.ScriptName, error.Location.Line, error.Location.Column, GetErrorPreview(error.Location), error.Message);
						exitCode = 1;
					}
					else {
						var error = results.Error;
						Error("{0}", error.Message);
						exitCode = 1;
					}
				}
			}
			Pause();
			Exit(exitCode);
		}

		static Stream OpenOutput(string path) {
			try {
				return File.Create(path);
			}
			catch {
				Error("Failed to create output file '{0}'.", path);
				Pause();
				Exit(1);
			}
			return null;
		}

		// command-line
		static void ReadCmdLine(aCommandLine cmd, out string inputFile, out string outputFile) {
			inputFile = null;
			outputFile = null;
			foreach (var prm in cmd) {
				switch (prm.Name) {
					case "-input": GetInput(prm, ref inputFile); break;
					case "-output": GetOutput(prm, ref outputFile); break;
				}
			}
			if (inputFile == null) {
				Error("Missing -input option.\n");
				Pause();
				Exit(1);
			}
			if (outputFile == null) {
				outputFile = Path.ChangeExtension(inputFile, ".sb");
			}
		}
		static void GetInput(aCommandLineParameter prm, ref string inputFile) {
			if (inputFile != null) {
				Error("Only one -input option is allowed.\n");
				Pause();
				Exit(1);
			}
			if (prm.Count != 1) {
				Error("Incorrect number of arguments in -input option.\n");
				Pause();
				Exit(1);
			}
			inputFile = prm[0];
		}
		static void GetOutput(aCommandLineParameter prm, ref string outputFile) {
			if (outputFile != null) {
				Error("Only one -output option is allowed.\n");
				Pause();
				Exit(1);
			}
			if (prm.Count != 1) {
				Error("Incorrect number of arguments in -output option.\n");
				Pause();
				Exit(1);
			}
			outputFile = prm[0];
		}

		// error preview
		static string GetErrorPreview(sunSourceLocation location) {
			Stream file;
			try {
				file = File.OpenRead(location.ScriptName);
			}
			catch {
				// simply don't do a preview if opening a file fails
				return "";
			}
			using (var reader = new StreamReader(file)) {
				// skip to line
				for (var line = 1; line < location.Line; ++line) {
					reader.ReadLine();
				}
				// generate column string
				var sb = new StringBuilder();
				var preview = reader.ReadLine();
				sb.AppendLine(preview);
				for (var column = 1; column < location.Column; ++column) {
					var c = preview[column - 1];
					if (IsFullWidth(c)) {
						sb.Append("  "); // full-width hack
					}
					else if (c == '\t') {
						sb.Append('\t');
					}
					else {
						sb.Append(" ");
					}
				}
				sb.Append("^");
				sb.Append("\n");
				return sb.ToString();
			}
		}
		static bool IsFullWidth(char c) {
			return (c >= 0x2E80 && c <= 0x9FFF) || (c >= 0xFF00 && c <= 0xFFEF);
		}

		// output
		static void Message(string format, params object[] args) {
			Console.Write(format, args);
		}
		static void Warning(string format, params object[] args) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING:\n");
			Message(format, args);
			Console.ResetColor();
		}
		static void Error(string format, params object[] args) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR:\n");
			Message(format, args);
			Console.ResetColor();
		}

		[Conditional("DEBUG")] static void Pause() {
			Console.ReadKey();
		}
		static void Exit(int code) {
			Environment.Exit(code);
		}
	}
}
