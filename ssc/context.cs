using arookas.IO.Binary;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas
{
	class sunContext
	{
		bool mOpen;
		aBinaryWriter mWriter;
		uint mTextOffset, mDataOffset, mSymbolOffset;
		int mVarCount;

		public sunWriter Text { get; private set; }
		public sunDataTable DataTable { get; private set; }
		public sunSymbolTable SymbolTable { get; private set; }
		public sunScopeStack Scopes { get; private set; }
		public sunLoopStack Loops { get; private set; }
		public sunImportResolver ImportResolver { get; private set; }

		public sunContext()
		{
			DataTable = new sunDataTable();
			SymbolTable = new sunSymbolTable();
			Scopes = new sunScopeStack();
			Loops = new sunLoopStack();
		}

		// open/close
		public void Open(Stream output)
		{
			Open(output, sunImportResolver.Default);
		}
		public void Open(Stream output, sunImportResolver importResolver)
		{
			if (mOpen)
			{
				throw new InvalidOperationException();
			}
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			if (importResolver == null)
			{
				throw new ArgumentNullException("importResolver");
			}
			mOpen = true;
			DataTable.Clear();
			SymbolTable.Clear();
			Scopes.Clear();
			Loops.Clear();
			ImportResolver = importResolver;
			mWriter = new aBinaryWriter(output, Endianness.Big, Encoding.GetEncoding(932));
			Text = new sunWriter(mWriter);
			mWriter.PushAnchor();

			WriteHeader(); // dummy header

			// begin text block
			mTextOffset = (uint)mWriter.Position;
			mWriter.PushAnchor(); // match code offsets and writer offsets

			// add system builtins
			DeclareSystemBuiltin("yield", false);
			DeclareSystemBuiltin("exit", false);
			DeclareSystemBuiltin("dump", false);
			DeclareSystemBuiltin("lock", false);
			DeclareSystemBuiltin("unlock", false);
			DeclareSystemBuiltin("int", false, "x");
			DeclareSystemBuiltin("float", false, "x");
			DeclareSystemBuiltin("typeof", false, "x");
			DeclareSystemBuiltin("print", true);
		}
		public void Close()
		{
			if (!mOpen)
			{
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
		public sunImportResult Import(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			sunScriptFile file;
			var result = ImportResolver.ResolveImport(name, out file);
			if (result == sunImportResult.Loaded)
			{
				try
				{
					ImportResolver.EnterFile(file);
					var parser = new sunParser();
					var tree = parser.Parse(file);
					mVarCount += tree.MaxLocalCount;
					tree.Compile(this);
					ImportResolver.ExitFile(file);
				}
				finally
				{
					file.Dispose();
				}
			}
			return result;
		}

		// callables
		public sunBuiltinSymbol DeclareBuiltin(sunBuiltinDeclaration node)
		{
			var symbolInfo = SymbolTable.Callables.FirstOrDefault(f => f.Name == node.Builtin.Value);
			if (symbolInfo != null)
			{
				throw new sunRedeclaredBuiltinException(node);
			}
			var builtinInfo = new sunBuiltinSymbol(node.Builtin.Value, node.Parameters.ParameterInfo, SymbolTable.Count);
			SymbolTable.Add(builtinInfo);
			return builtinInfo;
		}
		public sunFunctionSymbol DefineFunction(sunFunctionDefinition node)
		{
			if (node.Parameters.IsVariadic)
			{
				throw new sunVariadicFunctionException(node);
			}
			var symbolInfo = SymbolTable.Callables.FirstOrDefault(f => f.Name == node.Function.Value);
			if (symbolInfo != null)
			{
				throw new sunRedefinedFunctionException(node);
			}
			var functionInfo = new sunFunctionSymbol(node.Function.Value, node.Parameters.ParameterInfo, node.Body);
			SymbolTable.Add(functionInfo);
			return functionInfo;
		}
		public sunCallableSymbol ResolveCallable(sunFunctionCall node)
		{
			var symbol = SymbolTable.Callables.FirstOrDefault(f => f.Name == node.Function.Value);
			if (symbol == null)
			{
				throw new sunUndefinedFunctionException(node);
			}
			return symbol;
		}
		public sunCallableSymbol MustResolveCallable(sunFunctionCall node)
		{
			var symbol = ResolveCallable(node);
			if (symbol == null)
			{
				throw new sunUndefinedFunctionException(node);
			}
			return symbol;
		}

		public sunBuiltinSymbol DeclareSystemBuiltin(string name, bool variadic, params string[] parameters)
		{
			var builtinInfo = SymbolTable.Builtins.FirstOrDefault(f => f.Name == name);
			if (builtinInfo == null)
			{
				builtinInfo = new sunBuiltinSymbol(name, new sunParameterInfo(parameters, variadic), SymbolTable.Count);
				SymbolTable.Add(builtinInfo);
			}
			return builtinInfo;
		}
		public sunBuiltinSymbol ResolveSystemBuiltin(string name)
		{
			return SymbolTable.Builtins.FirstOrDefault(f => f.Name == name);
		}

		// storables
		public sunVariableSymbol DeclareVariable(sunIdentifier node)
		{
			// assert variable is not already declared in current scope
			if (Scopes.Top.GetIsDeclared(node.Value))
			{
				throw new sunRedeclaredVariableException(node);
			}
			return Scopes.DeclareVariable(node.Value);
		}
		public sunConstantSymbol DeclareConstant(sunIdentifier node, sunExpression expression)
		{
			if (Scopes.Top.GetIsDeclared(node.Value))
			{
				throw new sunRedeclaredVariableException(node);
			}
			return Scopes.DeclareConstant(node.Value, expression);
		}
		public sunStorableSymbol ResolveStorable(sunIdentifier node)
		{
			for (int i = Scopes.Count - 1; i >= 0; --i)
			{
				var symbol = Scopes[i].ResolveStorable(node.Value);
				if (symbol != null)
				{
					return symbol;
				}
			}
			return null;
		}
		public sunVariableSymbol ResolveVariable(sunIdentifier node)
		{
			for (int i = Scopes.Count - 1; i >= 0; --i)
			{
				var symbol = Scopes[i].ResolveVariable(node.Value);
				if (symbol != null)
				{
					return symbol;
				}
			}
			return null;
		}
		public sunConstantSymbol ResolveConstant(sunIdentifier node)
		{
			for (int i = Scopes.Count - 1; i >= 0; --i)
			{
				var symbol = Scopes[i].ResolveConstant(node.Value);
				if (symbol != null)
				{
					return symbol;
				}
			}
			return null;
		}
		public sunStorableSymbol MustResolveStorable(sunIdentifier node)
		{
			var symbol = ResolveStorable(node);
			if (symbol == null)
			{
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}
		public sunVariableSymbol MustResolveVariable(sunIdentifier node)
		{
			var symbol = ResolveVariable(node);
			if (symbol == null)
			{
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}
		public sunConstantSymbol MustResolveConstant(sunIdentifier node)
		{
			var symbol = ResolveConstant(node);
			if (symbol == null)
			{
				throw new sunUndeclaredVariableException(node);
			}
			return symbol;
		}

		void WriteHeader()
		{
			mWriter.WriteString("SPCB");
			mWriter.Write32(mTextOffset);
			mWriter.Write32(mDataOffset);
			mWriter.WriteS32(DataTable.Count);
			mWriter.Write32(mSymbolOffset);
			mWriter.WriteS32(SymbolTable.Count);
			mWriter.WriteS32(mVarCount);
		}
	}
}
