using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas {
	class sunContext {
		bool mOpen;
		aBinaryWriter mWriter;
		uint mTextOffset, mDataOffset, mSymbolOffset;
		Stack<sunNameLabel> mNameStack;

		public sunWriter Text { get; private set; }
		public sunDataTable DataTable { get; private set; }
		public sunSymbolTable SymbolTable { get; private set; }
		public sunScopeStack Scopes { get; private set; }
		public sunLoopStack Loops { get; private set; }
		public sunImportResolver ImportResolver { get; private set; }

		// system builtins
		public sunCallableSymbol Yield { get; private set; }
		public sunCallableSymbol Exit { get; private set; }
		public sunCallableSymbol Lock { get; private set; }
		public sunCallableSymbol Unlock { get; private set; }
		public sunCallableSymbol Int { get; private set; }
		public sunCallableSymbol Float { get; private set; }
		public sunCallableSymbol Typeof { get; private set; }

		// system variables
		public sunStorableSymbol Switch { get; private set; }

		public sunContext() {
			DataTable = new sunDataTable();
			SymbolTable = new sunSymbolTable();
			Scopes = new sunScopeStack();
			Loops = new sunLoopStack();
			mNameStack = new Stack<sunNameLabel>(5);
		}

		// open/close
		public void Open(Stream output) { Open(output, sunImportResolver.Default); }
		public void Open(Stream output, sunImportResolver importResolver) {
			if (mOpen) {
				throw new InvalidOperationException();
			}
			if (output == null) {
				throw new ArgumentNullException("output");
			}
			if (importResolver == null) {
				throw new ArgumentNullException("importResolver");
			}
			mOpen = true;
			DataTable.Clear();
			SymbolTable.Clear();
			Scopes.Clear();
			Loops.Clear();
			mNameStack.Clear();
			ImportResolver = importResolver;
			mWriter = new aBinaryWriter(output, Endianness.Big, Encoding.GetEncoding(932));
			Text = new sunWriter(mWriter);
			mWriter.PushAnchor();
			AddSystemSymbols();

			WriteHeader(); // dummy header

			// begin text block
			mTextOffset = (uint)mWriter.Position;
			mWriter.PushAnchor(); // match code offsets and writer offsets
			
		}
		public void Close() {
			if (!mOpen) {
				throw new InvalidOperationException();
			}
			mWriter.PopAnchor();
			mDataOffset = (uint)mWriter.Position;
			DataTable.Write(mWriter);
			mSymbolOffset = (uint)mWriter.Position;
			SymbolTable.Write(mWriter);
			mWriter.Goto(0);
			WriteHeader();
			mOpen = false;
		}

		// imports/compilation
		public sunImportResult Import(string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			sunScriptFile file;
			var result = ImportResolver.ResolveImport(name, out file);
			if (result == sunImportResult.Loaded) {
				try {
					ImportResolver.EnterFile(file);
					var parser = new sunParser();
					var tree = parser.Parse(file);
					tree.Compile(this);
					ImportResolver.ExitFile(file);
				}
				finally {
					file.Dispose();
				}
			}
			return result;
		}

		// callables
		public sunBuiltinSymbol DeclareBuiltin(sunBuiltinDeclaration node) {
			if (SymbolTable.Callables.Any(i => i.Name == node.Builtin.Value)) {
				throw new sunRedeclaredBuiltinException(node);
			}
			var symbol = new sunBuiltinSymbol(node.Builtin.Value, node.Parameters.ParameterInfo, SymbolTable.Count);
			SymbolTable.Add(symbol);
			return symbol;
		}
		public sunFunctionSymbol DefineFunction(sunFunctionDefinition node) {
			if (node.Parameters.IsVariadic) {
				throw new sunVariadicFunctionException(node);
			}
			if (SymbolTable.Callables.Any(i => i.Name == node.Function.Value)) {
				throw new sunRedefinedFunctionException(node);
			}
			var symbol = new sunFunctionSymbol(node.Function.Value, node.Parameters.ParameterInfo, node.Body);
			SymbolTable.Add(symbol);
			return symbol;
		}
		public sunCallableSymbol ResolveCallable(sunFunctionCall node) {
			var symbol = SymbolTable.Callables.FirstOrDefault(i => i.Name == node.Function.Value);
			if (symbol == null) {
				throw new sunUndefinedFunctionException(node);
			}
			return symbol;
		}
		public sunCallableSymbol MustResolveCallable(sunFunctionCall node) {
			var symbol = ResolveCallable(node);
			if (symbol == null) {
				throw new sunUndefinedFunctionException(node);
			}
			return symbol;
		}

		// storables
		public sunVariableSymbol DeclareVariable(sunIdentifier node) {
#if SSC_PACK_VARS
			if (Scopes.Top.GetIsDeclared(node.Value)) {
				throw new sunRedeclaredVariableException(node);
			}
#else
			if (Scopes.Any(i => i.GetIsDeclared(node.Value))) {
				throw new sunRedeclaredVariableException(node);
			}
#endif
			var symbol = Scopes.DeclareVariable(node.Value);
			if (Scopes.Top.Type == sunScopeType.Script) { // global-scope variables are added to the symbol table
#if SSC_PACK_VARS
				// only add the variable symbol if there isn't one with this index already
				if (!SymbolTable.Variables.Any(i => i.Index == symbol.Index)) {
					SymbolTable.Add(new sunVariableSymbol(String.Format("global{0}", symbol.Index), symbol.Display, symbol.Index));
				}
#else
				SymbolTable.Add(symbol);
#endif
			}
			return symbol;
		}
		public sunConstantSymbol DeclareConstant(sunIdentifier node, sunExpression expression) {
#if SSC_PACK_VARS
			if (Scopes.Top.GetIsDeclared(node.Value)) {
				throw new sunRedeclaredVariableException(node);
			}
#else
			if (Scopes.Any(i => i.GetIsDeclared(node.Value))) {
				throw new sunRedeclaredVariableException(node);
			}
#endif
			return Scopes.DeclareConstant(node.Value, expression);
		}
		public sunStorableSymbol ResolveStorable(sunIdentifier node) {
			for (int i = Scopes.Count - 1; i >= 0; --i) {
				var symbol = Scopes[i].ResolveStorable(node.Value);
				if (symbol != null) {
					return symbol;
				}
			}
			return null;
		}
		public sunVariableSymbol ResolveVariable(sunIdentifier node) { return ResolveStorable(node) as sunVariableSymbol; }
		public sunConstantSymbol ResolveConstant(sunIdentifier node) { return ResolveStorable(node) as sunConstantSymbol; }
		public sunStorableSymbol MustResolveStorable(sunIdentifier node) {
			var symbol = ResolveStorable(node);
			if (symbol == null) {
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}
		public sunVariableSymbol MustResolveVariable(sunIdentifier node) {
			var symbol = ResolveVariable(node);
			if (symbol == null) {
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}
		public sunConstantSymbol MustResolveConstant(sunIdentifier node) {
			var symbol = ResolveConstant(node);
			if (symbol == null) {
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}

		public void PushNameLabel(sunNameLabel label) {
			if (label == null) {
				throw new ArgumentNullException("label");
			}
			mNameStack.Push(label);
		}
		public sunNameLabel PopNameLabel() {
			if (mNameStack.Count > 0) {
				return mNameStack.Pop();
			}
			return null;
		}

		// system symbols
		void AddSystemSymbols() {
			// add system builtins
			Yield = AddSystemBuiltin("yield");
			Exit = AddSystemBuiltin("exit");
			Lock = AddSystemBuiltin("lock");
			Unlock = AddSystemBuiltin("unlock");
			Int = AddSystemBuiltin("int");
			Float = AddSystemBuiltin("float");
			Typeof = AddSystemBuiltin("typeof");

			// add system variables
			Switch = AddSystemVariable("$switch"); // storage for switch statements
		}
		sunCallableSymbol AddSystemBuiltin(string name) {
			var symbol = new sunBuiltinSymbol(name, SymbolTable.Count);
			SymbolTable.Add(symbol);
			return symbol;
		}
		sunStorableSymbol AddSystemVariable(string name) {
			var symbol = Scopes.DeclareVariable(name);
			SymbolTable.Add(symbol);
			return symbol;
		}
		void WriteHeader() {
			mWriter.WriteString("SPCB");
			mWriter.Write32(mTextOffset);
			mWriter.Write32(mDataOffset);
			mWriter.WriteS32(DataTable.Count);
			mWriter.Write32(mSymbolOffset);
			mWriter.WriteS32(SymbolTable.Count);
			mWriter.WriteS32(SymbolTable.VariableCount);
		}
	}
}
