using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas {
	class sunContext {
		Stack<sunNameLabel> mNameStack;

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
			AddSystemSymbols();
		}

		public void Clear() {
			DataTable.Clear();
			SymbolTable.Clear();
			Scopes.Clear();
			Loops.Clear();
			mNameStack.Clear();

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
			var local = (node.Modifiers & sunSymbolModifiers.Local) != 0;
			var name = MangleSymbolName(node.Name.Value, node.Location.ScriptId, false, local);
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
			var local = MangleSymbolName(global, node.Location.ScriptId, false, true);
			var symbol = SymbolTable.Callables.FirstOrDefault(i => i.Name == local);
			if (symbol != null) {
				return symbol;
			}
			symbol = SymbolTable.Callables.FirstOrDefault(i => i.Name == global);
			if (symbol != null) {
				return symbol;
			}
			return null;
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
			var local = (modifiers & sunSymbolModifiers.Local) != 0;
			var name = MangleSymbolName(node.Value, node.Location.ScriptId, false, local);
			if (Scopes.Any(i => i.GetIsDeclared(name))) {
				throw new sunRedeclaredVariableException(node);
			}
			var symbol = Scopes.DeclareVariable(name);
			if (Scopes.Top.Type == sunScopeType.Script) { // global-scope variables are added to the symbol table
				SymbolTable.Add(symbol);
			}
			return symbol;
		}
		public sunConstantSymbol DeclareConstant(sunConstantDefinition node) {
			return DeclareConstant(node.Name, node.Expression, node.Modifiers);
		}
		sunConstantSymbol DeclareConstant(sunIdentifier node, sunExpression expression, sunSymbolModifiers modifiers) {
			var local = (modifiers & sunSymbolModifiers.Local) != 0;
			var name = MangleSymbolName(node.Value, node.Location.ScriptId, false, local);
			if (Scopes.Any(i => i.GetIsDeclared(name))) {
				throw new sunRedeclaredVariableException(node);
			}
			return Scopes.DeclareConstant(name, expression);
		}

		public sunStorableSymbol ResolveStorable(sunIdentifier node) {
			var global = node.Value;
			var local = MangleSymbolName(global, node.Location.ScriptId, false, true);
			var symbol = ResolveStorable(local);
			if (symbol != null) {
				return symbol;
			}
			symbol = ResolveStorable(global);
			if (symbol != null) {
				return symbol;
			}
			return null;
		}
		sunStorableSymbol ResolveStorable(string name) {
			for (int i = Scopes.Count - 1; i >= 0; --i) {
				var symbol = Scopes[i].ResolveStorable(name);
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
			var symbol = Scopes.DeclareVariable(MangleSystemSymbol(name));
			SymbolTable.Add(symbol);
			return symbol;
		}

		// static util
		static string MangleSystemSymbol(string basename) {
			return MangleSymbolName(basename, 0, true, false);
		}
		static string MangleLocalSymbol(string basename, ulong id) {
			return MangleSymbolName(basename, id, false, true);
		}
		static string MangleSymbolName(string basename, ulong id, bool system, bool local) {
			if (!system && !local) {
				return basename;
			}
			var sb = new StringBuilder(basename.Length + 16);
			if (system) {
				sb.Append('$');
			}
			sb.Append(basename);
			if (local) {
				sb.AppendFormat("@{0}", id);
			}
			return sb.ToString();
		}
	}
}
