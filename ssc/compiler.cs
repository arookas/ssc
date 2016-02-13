using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
			return Compile(name, new sunSpcBinary(output), resolver);
		}
		public sunCompilerResults Compile(string name, sunBinary binary, sunImportResolver resolver) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (binary == null) {
				throw new ArgumentNullException("binary");
			}
			if (resolver == null) {
				throw new ArgumentNullException("resolver");
			}
			var results = new sunCompilerResults();
			var timer = Stopwatch.StartNew();
			try {
				mBinary = binary;
				mResolver = resolver;
				mContext.Clear();
				mBinary.Open();
				mBinary.BeginText();
				CompileBody(name);
				CompileFunctions();
#if SSC_CLEAN_SYMBOLS
				CleanSymbols();
#endif
				CompileRelocations();
				mBinary.EndText();
				mBinary.BeginData();
				CompileData();
				mBinary.EndData();
				mBinary.BeginSymbol();
				CompileSymbols();
				mBinary.EndSymbol();
				mBinary.Close();

				results.Data = mContext.DataTable.ToArray();
				results.Symbols = mContext.SymbolTable.Select(i => new sunSymbolInfo(i.Type, i.Name)).ToArray();
			}
			catch (sunCompilerException ex) {
				results.Error = ex;
			}
			timer.Stop();
			results.CompileTime = timer.Elapsed;
			return results;
		}

		void CompileBody(string name) {
			var result = Import(name);
			if (result != sunImportResult.Loaded) {
				throw new sunImportException(name, result);
			}
			mBinary.WriteEND();
		}
#if SSC_CLEAN_FUNCTIONS
		void CompileFunctions() {
			while (DoCompileFunctions() > 0) ;
		}
		int DoCompileFunctions() {
			var count = 0;
			foreach (var callable in mContext.SymbolTable.Get<sunCallableSymbol>().Where(i => i.HasRelocations && i.CompileCount == 0)) {
				callable.Compile(this);
				++count;
			}
			return count;
		}
#else
		void CompileFunctions() {
			foreach (var callable in mContext.SymbolTable.Get<sunCallableSymbol>()) {
				callable.Compile(this);
			}
		}
#endif
#if SSC_CLEAN_SYMBOLS
		void CleanSymbols() {
			var i = 0;
			while (i < mContext.SymbolTable.Count) {
				if (!mContext.SymbolTable[i].HasRelocations) {
					mContext.SymbolTable.RemoveAt(i);
					continue;
				}
				++i;
			}
			for (i = 0; i < mContext.SymbolTable.Count; ++i) {
				var builtin = mContext.SymbolTable[i] as sunBuiltinSymbol;
				if (builtin == null) {
					continue;
				}
				builtin.Index = i;
			}
			var vars = 0;
			for (i = 0; i < mContext.SymbolTable.Count; ++i) {
				var variable = mContext.SymbolTable[i] as sunVariableSymbol;
				if (variable == null) {
					continue;
				}
				variable.Display = 0;
				variable.Index = vars++;
			}
		}
#endif
		void CompileRelocations() {
			foreach (var symbol in mContext.SymbolTable) {
				symbol.CloseRelocations(this);
			}
		}
		void CompileData() {
			foreach (var data in mContext.DataTable) {
				mBinary.WriteData(data);
			}
		}
		void CompileSymbols() {
			foreach (var symbol in mContext.SymbolTable) {
				mBinary.WriteSymbol(symbol.Type, symbol.Name, symbol.Data);
			}
		}

		internal sunImportResult Import(string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			sunScriptFile file;
			var result = mResolver.Resolve(name, out file);
			if (result == sunImportResult.Loaded) {
				try {
					mResolver.Enter(file);
					mParser.Parse(file).Compile(this);
					mResolver.Exit(file);
				}
				finally {
					file.Dispose();
				}
			}
			return result;
		}
	}

	public class sunCompilerResults {
		sunCompilerException mError;
		TimeSpan mTime;
		string[] mData;
		sunSymbolInfo[] mSymbols;

		public bool Success {
			get { return mError == null; }
		}

		public sunCompilerException Error {
			get { return mError; }
			internal set { mError = value; }
		}
		public TimeSpan CompileTime {
			get { return mTime; }
			internal set { mTime = value; }
		}
		public string[] Data {
			get { return mData; }
			internal set { mData = value; }
		}
		public sunSymbolInfo[] Symbols {
			get { return mSymbols; }
			internal set { mSymbols = value; }
		}
	}

	public class sunSymbolInfo {
		sunSymbolType mType;
		string mName;

		public sunSymbolType Type {
			get { return mType; }
		}
		public string Name {
			get { return mName; }
		}

		internal sunSymbolInfo(sunSymbolType type, string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			mType = type;
			mName = name;
		}
	}
}
