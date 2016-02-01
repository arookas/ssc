using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas {
	class sunContext {
		Stack<sunNameLabel> mNameStack;
		Stack<long> mLocalStack;
		long mLocal;

		public sunDataTable DataTable { get; private set; }
		public sunSymbolTable SymbolTable { get; private set; }
		public sunScopeStack Scopes { get; private set; }
		public sunLoopStack Loops { get; private set; }

		// system builtins
		public sunCallableSymbol Yield { get; private set; }
		public sunCallableSymbol Exit { get; private set; }
		public sunCallableSymbol Lock { get; private set; }
		public sunCallableSymbol Unlock { get; private set; }

		// system variables
		public sunStorableSymbol Switch { get; private set; }

		public sunContext() {
			DataTable = new sunDataTable();
			SymbolTable = new sunSymbolTable();
			Scopes = new sunScopeStack();
			Loops = new sunLoopStack();
			mNameStack = new Stack<sunNameLabel>(10);
			mLocalStack = new Stack<long>(10);
			AddSystemSymbols();
		}

		public void Clear() {
			DataTable.Clear();
			SymbolTable.Clear();
			Scopes.Clear();
			Loops.Clear();
			mNameStack.Clear();
			mLocalStack.Clear();
			mLocal = 0;

			// reinstall system symbols
			AddSystemSymbols();
		}

		// callables
		public sunBuiltinSymbol DeclareBuiltin(sunBuiltinDeclaration node) {
			if (SymbolTable.Callables.Any(i => i.Name == node.Name.Value)) {
				throw new sunRedeclaredBuiltinException(node);
			}
			var symbol = new sunBuiltinSymbol(node.Name.Value, node.Parameters.ParameterInfo, SymbolTable.Count);
			SymbolTable.Add(symbol);
			return symbol;
		}
		public sunFunctionSymbol DefineFunction(sunFunctionDefinition node) {
			var name = MangleSymbolName(node.Name.Value, false, (node.Modifiers & sunSymbolModifiers.Local) != 0);
			if (node.Parameters.IsVariadic) {
				throw new sunVariadicFunctionException(node);
			}
			if (SymbolTable.Callables.Any(i => i.Name == name)) {
				throw new sunRedefinedFunctionException(node);
			}
			var symbol = new sunFunctionSymbol(name, node.Parameters.ParameterInfo, node.Body);
			SymbolTable.Add(symbol);
			return symbol;
		}
		public sunCallableSymbol ResolveCallable(sunFunctionCall node) {
			var global = node.Name.Value;
			var local = MangleSymbolName(global, false, true);
			var symbol = SymbolTable.Callables.FirstOrDefault(i => i.Name == local);
			if (symbol != null) {
				return symbol;
			}
			symbol = SymbolTable.Callables.FirstOrDefault(i => i.Name == global);
			if (symbol != null) {
				return symbol;
			}
			throw new sunUndefinedFunctionException(node);
		}
		public sunCallableSymbol MustResolveCallable(sunFunctionCall node) {
			var symbol = ResolveCallable(node);
			if (symbol == null) {
				throw new sunUndefinedFunctionException(node);
			}
			return symbol;
		}

		// storables
		public sunVariableSymbol DeclareVariable(sunVariableDeclaration node) {
			return DeclareVariable(node.Name, node.Modifiers);
		}
		public sunVariableSymbol DeclareVariable(sunVariableDefinition node) {
			return DeclareVariable(node.Name, node.Modifiers);
		}
		sunVariableSymbol DeclareVariable(sunIdentifier node, sunSymbolModifiers modifiers) {
			var name = MangleSymbolName(node.Value, false, false);
#if SSC_PACK_VARS
			if (Scopes.Top.GetIsDeclared(name)) {
				throw new sunRedeclaredVariableException(node);
			}
#else
			if (Scopes.Any(i => i.GetIsDeclared(name))) {
				throw new sunRedeclaredVariableException(node);
			}
#endif
			var symbol = Scopes.DeclareVariable(name);
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
		public sunConstantSymbol DeclareConstant(sunConstantDefinition node) {
			return DeclareConstant(node.Name, node.Expression, node.Modifiers);
		}
		sunConstantSymbol DeclareConstant(sunIdentifier node, sunExpression expression, sunSymbolModifiers modifiers) {
			var name = MangleSymbolName(node.Value, false, (modifiers & sunSymbolModifiers.Local) != 0);
#if SSC_PACK_VARS
			if (Scopes.Top.GetIsDeclared(name)) {
				throw new sunRedeclaredVariableException(node);
			}
#else
			if (Scopes.Any(i => i.GetIsDeclared(name))) {
				throw new sunRedeclaredVariableException(node);
			}
#endif
			return Scopes.DeclareConstant(name, expression);
		}
		public sunStorableSymbol ResolveStorable(sunIdentifier node) {
			var global = node.Value;
			var local = MangleSymbolName(global, false, true);
			for (int i = Scopes.Count - 1; i >= 0; --i) {
				var symbol = Scopes[i].ResolveStorable(local);
				if (symbol != null) {
					return symbol;
				}
				symbol = Scopes[i].ResolveStorable(global);
				if (symbol != null) {
					return symbol;
				}
			}
			return null;
		}
		public sunVariableSymbol ResolveVariable(sunIdentifier node) {
			return ResolveStorable(node) as sunVariableSymbol;
		}
		public sunConstantSymbol ResolveConstant(sunIdentifier node) {
			return ResolveStorable(node) as sunConstantSymbol;
		}
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

		// name labels
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

		// locals
		public void PushLocal() {
			mLocalStack.Push(mLocal++);
		}
		public void PopLocal() {
			if (mLocalStack.Count > 0) {
				mLocalStack.Pop();
			}
		}

		// system symbols
		void AddSystemSymbols() {
			// add system builtins
			Yield = AddSystemBuiltin("yield");
			Exit = AddSystemBuiltin("exit");
			Lock = AddSystemBuiltin("lock");
			Unlock = AddSystemBuiltin("unlock");

			// add system variables
			Switch = AddSystemVariable("switch"); // storage for switch statements
		}
		sunCallableSymbol AddSystemBuiltin(string name) {
			var symbol = new sunBuiltinSymbol(name, SymbolTable.Count);
			SymbolTable.Add(symbol);
			return symbol;
		}
		sunStorableSymbol AddSystemVariable(string name) {
			var symbol = Scopes.DeclareVariable(MangleSymbolName(name, true, false));
			SymbolTable.Add(symbol);
			return symbol;
		}

		// static util
		string MangleSymbolName(string basename, bool system, bool local) {
			var prefix = "";
			var suffix = "";
			if (system) {
				prefix = "$";
			}
			if (local) {
				suffix = String.Format("@{0}", mLocal);
			}
			if (prefix == "" && suffix == "") {
				return basename;
			}
			return String.Concat(prefix, basename, suffix);
		}
	}
}
