using PerCederberg.Grammatica.Runtime;
using System;

namespace arookas
{
	// base exception type
	public class sunCompilerException : Exception
	{
		public sunCompilerException()
		{

		}
		public sunCompilerException(string format, params object[] args)
			: base(String.Format(format, args))
		{

		}
	}

	// exceptions that have a location in the source
	public abstract class sunSourceException : sunCompilerException
	{
		public abstract sunSourceLocation Location { get; }

		public sunSourceException()
		{

		}
		public sunSourceException(string format, params object[] args)
			: base(format, args)
		{

		}
	}

	public class sunImportException : sunCompilerException
	{
		public string Name { get; private set; }
		public sunImportResult Result { get; private set; }
		public override string Message
		{
			get
			{
				string format;
				switch (Result)
				{
					case sunImportResult.Loaded: format = "Script '{0}' loaded successfully."; break; // Error: Success!
					case sunImportResult.Skipped: format = "Script '{0}' was skipped."; break;
					case sunImportResult.Missing: format = "Script '{0}' could not be found."; break;
					case sunImportResult.FailedToLoad: format = "Script '{0}' failed to load."; break;
					default: format = "Name: {0}, Result: {1}"; break;
				}
				return String.Format(format, Name, Result);
			}
		}

		public sunImportException(string name, sunImportResult result)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (!result.IsDefined())
			{
				throw new ArgumentOutOfRangeException("name");
			}
			Name = name;
			Result = result;
		}
	}

	// wrapper around Grammatica exceptions
	class sunParserException : sunSourceException
	{
		string file;

		public ParseException Info { get; private set; }
		public override string Message { get { return Info.ErrorMessage; } }
		public override sunSourceLocation Location { get { return new sunSourceLocation(file, Info.Line, Info.Column); } }

		public sunParserException(string file, ParseException info)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.file = file;
			Info = info;
		}
	}

	// node exceptions
	abstract class sunNodeException<TNode> : sunSourceException where TNode : sunNode
	{
		public TNode Node { get; private set; }
		public override sunSourceLocation Location { get { return Node.Location; } }

		protected sunNodeException(TNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			Node = node;
		}
	}

	class sunRedeclaredBuiltinException : sunNodeException<sunBuiltinDeclaration>
	{
		public override string Message { get { return String.Format("Redeclared builtin '{0}'.", Node.Builtin.Value); } }

		public sunRedeclaredBuiltinException(sunBuiltinDeclaration node)
			: base(node)
		{

		}
	}
	class sunUndefinedFunctionException : sunNodeException<sunFunctionCall>
	{
		public override string Message { get { return String.Format("Undefined function or builtin '{0}'.", Node.Function.Value); } }

		public sunUndefinedFunctionException(sunFunctionCall node)
			: base(node)
		{

		}
	}
	class sunRedefinedFunctionException : sunNodeException<sunFunctionDefinition>
	{
		public override string Message { get { return String.Format("Redefined function '{0}'.", Node.Function.Value); } }

		public sunRedefinedFunctionException(sunFunctionDefinition node)
			: base(node)
		{

		}
	}
	class sunUndeclaredVariableException : sunNodeException<sunIdentifier>
	{
		public override string Message { get { return String.Format("Undeclared variable '{0}'.", Node.Value); } }

		public sunUndeclaredVariableException(sunIdentifier node)
			: base(node)
		{

		}
	}
	class sunRedeclaredVariableException : sunNodeException<sunIdentifier>
	{
		public override string Message { get { return String.Format("Redeclared variable '{0}'.", Node.Value); } }

		public sunRedeclaredVariableException(sunIdentifier node)
			: base(node)
		{

		}
	}
	class sunAssignConstantException : sunNodeException<sunIdentifier>
	{
		public override string Message { get { return String.Format("Constant '{0}' is read-only.", Node.Value); } }

		public sunAssignConstantException(sunIdentifier node)
			: base(node)
		{

		}
	}
	class sunRedeclaredParameterException : sunNodeException<sunIdentifier>
	{
		public override string Message { get { return String.Format("Redeclared parameter '{0}'.", Node.Value); } }

		public sunRedeclaredParameterException(sunIdentifier node)
			: base(node)
		{

		}
	}
	class sunVariadicFunctionException : sunNodeException<sunFunctionDefinition>
	{
		public override string Message { get { return String.Format("Function '{0}' is defined as a variadic function (only builtins may be variadic).", Node.Function.Value); } }

		public sunVariadicFunctionException(sunFunctionDefinition node)
			: base(node)
		{

		}
	}
	class sunEscapeSequenceException : sunNodeException<sunStringLiteral>
	{
		public override string Message { get { return String.Format("Bad escape sequence in string."); } }

		public sunEscapeSequenceException(sunStringLiteral node)
			: base(node)
		{

		}
	}
	class sunVariadicParameterListException : sunNodeException<sunParameterList>
	{
		public override string Message { get { return String.Format("Bad variadic parameter list."); } }

		public sunVariadicParameterListException(sunParameterList node)
			: base(node)
		{

		}
	}
	class sunArgumentCountException : sunNodeException<sunFunctionCall>
	{
		public sunCallableSymbol CalledSymbol { get; private set; }
		public int ArgumentMinimum { get { return CalledSymbol.Parameters.Minimum; } }
		public int ArgumentCount { get { return Node.Arguments.Count; } }

		public override string Message
		{
			get
			{
				string format;
				if (CalledSymbol.Parameters.IsVariadic)
				{
					// assuming to be missing because there's only a minimum
					format = "Missing {0} argument(s) (expected at least {1}; got {2}).";
				}
				else if (Node.Arguments.Count < CalledSymbol.Parameters.Minimum)
				{
					format = "Missing {0} argument(s) (expected {1}; got {2})."; // missing arguments
				}
				else
				{
					format = "Too many arguments (expected {1}; got {2})."; // extra arguments
				}
				return String.Format(format, ArgumentMinimum - ArgumentCount, ArgumentMinimum, ArgumentCount);
			}
		}

		public sunArgumentCountException(sunFunctionCall node, sunCallableSymbol calledSymbol)
			: base(node)
		{
			if (calledSymbol == null)
			{
				throw new ArgumentNullException("calledSymbol");
			}
			CalledSymbol = calledSymbol;
		}
	}
	class sunIdentifierException : sunNodeException<sunIdentifier>
	{
		public override string Message { get { return String.Format("Invalid identifier '{0}'.", Node.Value); } }

		public sunIdentifierException(sunIdentifier node)
			: base(node)
		{

		}
	}
	class sunMissingImportException : sunNodeException<sunImport>
	{
		public override string Message { get { return String.Format("Could not find import file '{0}'.", Node.ImportFile.Value); } }

		public sunMissingImportException(sunImport node)
			: base(node)
		{

		}
	}
	class sunBreakException : sunNodeException<sunBreak>
	{
		public override string Message { get { return "Break statements must be placed within a loop statement."; } }

		public sunBreakException(sunBreak node)
			: base(node)
		{

		}
	}
	class sunContinueException : sunNodeException<sunContinue>
	{
		public override string Message { get { return "Continue statements must be placed within a loop statement."; } }

		public sunContinueException(sunContinue node)
			: base(node)
		{

		}
	}
}
