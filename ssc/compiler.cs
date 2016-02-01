using System;
using System.Diagnostics;
using System.IO;

namespace arookas {
	public class sunCompiler {
		sunContext mContext;
		sunBinary mBinary;
		sunImportResolver mResolver;
		sunParser mParser;

		internal sunContext Context {
			get { return mContext; }
		}
		internal sunBinary Binary {
			get { return mBinary; }
		}
		internal sunImportResolver ImportResolver {
			get { return mResolver; }
		}

		public sunCompiler() {
			mContext = new sunContext();
			mParser = new sunParser();
		}

		public sunCompilerResults Compile(string name, Stream output) {
			return Compile(name, output, sunImportResolver.Default);
		}
		public sunCompilerResults Compile(string name, Stream output, sunImportResolver resolver) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (output == null) {
				throw new ArgumentNullException("output");
			}
			if (resolver == null) {
				throw new ArgumentNullException("resolver");
			}
			var results = new sunCompilerResults();
			var timer = Stopwatch.StartNew();
			try {
				mResolver = resolver;
				mContext.Clear();
				using (mBinary = new sunBinary(output)) {
					var result = Import(name);
					if (result != sunImportResult.Loaded) {
						throw new sunImportException(name, result);
					}
					mBinary.WriteEND(); // NOTETOSELF: don't do this via sunNode.Compile because imported files will add this as well
					foreach (var callable in mContext.SymbolTable.Callables) {
						callable.Compile(this);
					}
					foreach (var callable in mContext.SymbolTable.Callables) {
						callable.CloseCallSites(this);
					}
					foreach (var data in mContext.DataTable) {
						mBinary.WriteData(data);
					}
					foreach (var symbol in mContext.SymbolTable) {
						mBinary.WriteSymbol(symbol.Type, symbol.Name, symbol.Data);
					}
				}
				results.DataCount = mContext.DataTable.Count;
				results.SymbolCount = mContext.SymbolTable.Count;
				results.BuiltinCount = mContext.SymbolTable.BuiltinCount;
				results.FunctionCount = mContext.SymbolTable.FunctionCount;
				results.VariableCount = mContext.SymbolTable.VariableCount;
			}
			catch (sunCompilerException ex) {
				results.Error = ex;
			}
			timer.Stop();
			results.CompileTime = timer.Elapsed;
			return results;
		}

		internal sunImportResult Import(string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			sunScriptFile file;
			var result = mResolver.ResolveImport(name, out file);
			if (result == sunImportResult.Loaded) {
				try {
					mResolver.EnterFile(file);
					mContext.PushLocal();
					mParser.Parse(file).Compile(this);
					mContext.PopLocal();
					mResolver.ExitFile(file);
				}
				finally {
					file.Dispose();
				}
			}
			return result;
		}
	}

	public class sunCompilerResults {
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
