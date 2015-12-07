using PerCederberg.Grammatica.Runtime;
using System.IO;
using System.Linq;

namespace arookas
{
	class sunParser
	{
		static string[] keywords =
		{
			"import",
			"builtin", "function", "var", "const",
			"if", "while", "do", "for",
			"return", "break", "continue",
			"yield", "exit", "dump", "lock", "unlock", "int", "float", "typeof", "print",
			"true", "false",
		};

		public sunNode Parse(string file)
		{
			using (var input = new StreamReader(file))
			{
				var parser = new __sunParser(input);
				var node = parser.Parse();
				return CreateAst(file, node);
			}
		}

		static sunNode CreateAst(string file, Node node)
		{
			var ast = ConvertNode(file, node);
			if (ast == null)
			{
				return null;
			}
			// children
			if (node is Production)
			{
				var production = node as Production;
				for (int i = 0; i < production.Count; ++i)
				{
					var child = CreateAst(file, production[i]);
					if (child != null)
					{
						ast.Add(child);
					}
				}
			}
			// transcience
			if (ast.Count == 1)
			{
				switch (GetId(node))
				{
					case __sunConstants.ROOT_STATEMENT:
					case __sunConstants.STATEMENT:
					case __sunConstants.COMPOUND_STATEMENT:
					case __sunConstants.COMPOUND_STATEMENT_ITEM:
					case __sunConstants.ASSIGNMENT_OPERATOR:
					case __sunConstants.BINARY_OPERATOR:
					case __sunConstants.UNARY_OPERATOR:
					case __sunConstants.TERM:
					case __sunConstants.PARAMETER:
					case __sunConstants.ARGUMENT_LIST:
					case __sunConstants.ARGUMENT:
					{
						return ast[0];
					}
				}
			}
			return ast;
		}
		static sunNode ConvertNode(string file, Node node)
		{
			var location = new sunSourceLocation(file, node.StartLine, node.StartColumn);
			string token = null;
			if (node is Token)
			{
				token = (node as Token).Image;
			}

			// statements
			switch (GetId(node))
			{
				case __sunConstants.SCRIPT: return new sunNode(location);
				case __sunConstants.ROOT_STATEMENT: return new sunNode(location);
				case __sunConstants.STATEMENT: return new sunNode(location);
				case __sunConstants.STATEMENT_BLOCK: return new sunStatementBlock(location);
				case __sunConstants.COMPOUND_STATEMENT: return new sunNode(location);
				case __sunConstants.COMPOUND_STATEMENT_ITEM: return new sunNode(location);

				case __sunConstants.IMPORT_STATEMENT: return new sunImport(location);
				case __sunConstants.NAME_LABEL: return new sunNameLabel(location);

				case __sunConstants.YIELD_STATEMENT: return new sunYield(location);
				case __sunConstants.EXIT_STATEMENT: return new sunExit(location);
				case __sunConstants.DUMP_STATEMENT: return new sunDump(location);
				case __sunConstants.LOCK_STATEMENT: return new sunLock(location);
				case __sunConstants.UNLOCK_STATEMENT: return new sunUnlock(location);
				case __sunConstants.PRINT_STATEMENT: return new sunPrint(location);
			}

			// literals
			switch (GetId(node))
			{
				case __sunConstants.INT_NUMBER: return new sunIntLiteral(location, token);
				case __sunConstants.HEX_NUMBER: return new sunHexLiteral(location, token);
				case __sunConstants.DEC_NUMBER: return new sunFloatLiteral(location, token);
				case __sunConstants.STRING: return new sunStringLiteral(location, token);
				case __sunConstants.IDENTIFIER: return new sunIdentifier(location, token);
				case __sunConstants.ELLIPSIS: return new sunEllipsis(location);
				case __sunConstants.TRUE: return new sunTrue(location, token);
				case __sunConstants.FALSE: return new sunFalse(location, token);
			}

			// operators
			switch (GetId(node))
			{
				case __sunConstants.ADD: return new sunAdd(location);
				case __sunConstants.SUB:
				{
					if (GetId(node.Parent) == __sunConstants.UNARY_OPERATOR)
					{
						return new sunNeg(location);
					}
					return new sunSub(location);
				}
				case __sunConstants.MUL: return new sunMul(location);
				case __sunConstants.DIV: return new sunDiv(location);
				case __sunConstants.MOD: return new sunMod(location);

				case __sunConstants.BIT_AND: return new sunBitAND(location);
				case __sunConstants.BIT_OR: return new sunBitOR(location);
				case __sunConstants.BIT_LSH: return new sunBitLsh(location);
				case __sunConstants.BIT_RSH: return new sunBitRsh(location);

				case __sunConstants.LOG_AND: return new sunLogAND(location);
				case __sunConstants.LOG_OR: return new sunLogOR(location);
				case __sunConstants.LOG_NOT: return new sunLogNOT(location);

				case __sunConstants.EQ: return new sunEq(location);
				case __sunConstants.NEQ: return new sunNtEq(location);
				case __sunConstants.LT: return new sunLt(location);
				case __sunConstants.GT: return new sunGt(location);
				case __sunConstants.LTEQ: return new sunLtEq(location);
				case __sunConstants.GTEQ: return new sunGtEq(location);

				case __sunConstants.ASSIGN: return new sunAssign(location);
				case __sunConstants.ASSIGN_ADD: return new sunAssignAdd(location);
				case __sunConstants.ASSIGN_SUB: return new sunAssignSub(location);
				case __sunConstants.ASSIGN_MUL: return new sunAssignMul(location);
				case __sunConstants.ASSIGN_DIV: return new sunAssignDiv(location);
				case __sunConstants.ASSIGN_MOD: return new sunAssignMod(location);

				case __sunConstants.ASSIGN_BIT_AND: return new sunAssignBitAND(location);
				case __sunConstants.ASSIGN_BIT_OR: return new sunAssignBitOR(location);
				case __sunConstants.ASSIGN_BIT_LSH: return new sunAssignBitLsh(location);
				case __sunConstants.ASSIGN_BIT_RSH: return new sunAssignBitRsh(location);

				case __sunConstants.ASSIGNMENT_OPERATOR: return new sunNode(location);
				case __sunConstants.TERNARY_OPERATOR: return new sunTernaryOperator(location);
				case __sunConstants.BINARY_OPERATOR: return new sunNode(location);
				case __sunConstants.UNARY_OPERATOR: return new sunNode(location);
			}

			// expressions
			switch (GetId(node))
			{
				case __sunConstants.EXPRESSION: return new sunExpression(location);
				case __sunConstants.OPERAND: return new sunOperand(location);
				case __sunConstants.TERM: return new sunNode(location);
				
				case __sunConstants.UNARY_OPERATOR_LIST: return new sunUnaryOperatorList(location);

				case __sunConstants.INT_CAST: return new sunIntCast(location);
				case __sunConstants.FLOAT_CAST: return new sunFloatCast(location);
				case __sunConstants.TYPEOF_CAST: return new sunTypeofCast(location);
			}

			// builtins
			switch (GetId(node))
			{
				case __sunConstants.BUILTIN_DECLARATION: return new sunBuiltinDeclaration(location);
			}

			// functions
			switch (GetId(node))
			{
				case __sunConstants.FUNCTION_DEFINITION: return new sunFunctionDefinition(location);
				case __sunConstants.FUNCTION_CALL: return new sunFunctionCall(location);

				case __sunConstants.PARAMETER_LIST: return new sunParameterList(location);
				case __sunConstants.PARAMETER: return new sunNode(location);
				case __sunConstants.ARGUMENT_LIST: return new sunNode(location);
				case __sunConstants.ARGUMENT: return new sunNode(location);
			}

			// variables
			switch (GetId(node))
			{
				case __sunConstants.VARIABLE_REFERENCE: return new sunVariableReference(location);
				case __sunConstants.VARIABLE_DECLARATION: return new sunVariableDeclaration(location);
				case __sunConstants.VARIABLE_DEFINITION: return new sunVariableDefinition(location);
				case __sunConstants.VARIABLE_ASSIGNMENT: return new sunVariableAssignment(location);
			}

			// constants
			switch (GetId(node))
			{
				case __sunConstants.CONST_DEFINITION: return new sunConstDefinition(location);
			}

			// flow control
			switch (GetId(node))
			{
				case __sunConstants.IF_STATEMENT: return new sunIf(location);
				case __sunConstants.WHILE_STATEMENT: return new sunWhile(location);
				case __sunConstants.DO_STATEMENT: return new sunDo(location);
				case __sunConstants.FOR_STATEMENT: return new sunFor(location);
				case __sunConstants.FOR_DECLARATION: return new sunForDeclaration(location);
				case __sunConstants.FOR_CONDITION: return new sunForCondition(location);
				case __sunConstants.FOR_ITERATION: return new sunForIteration(location);

				case __sunConstants.RETURN_STATEMENT: return new sunReturn(location);
				case __sunConstants.BREAK_STATEMENT: return new sunBreak(location);
				case __sunConstants.CONTINUE_STATEMENT: return new sunContinue(location);
			}

			// cleanup keywords punctuation
			switch (GetId(node))
			{
				// keywords
				case __sunConstants.IMPORT:
				case __sunConstants.BUILTIN:
				case __sunConstants.FUNCTION:
				case __sunConstants.VAR:
				case __sunConstants.CONST:

				case __sunConstants.IF:
				case __sunConstants.ELSE:
				case __sunConstants.DO:
				case __sunConstants.WHILE:
				case __sunConstants.FOR:

				case __sunConstants.RETURN:
				case __sunConstants.BREAK:
				case __sunConstants.CONTINUE:

				case __sunConstants.YIELD:
				case __sunConstants.EXIT:
				case __sunConstants.DUMP:
				case __sunConstants.LOCK:
				case __sunConstants.UNLOCK:
				case __sunConstants.INT:
				case __sunConstants.FLOAT:
				case __sunConstants.TYPEOF:
				case __sunConstants.PRINT:

				case __sunConstants.TRUE:
				case __sunConstants.FALSE:

				// punctuation
				case __sunConstants.L_BRACE:
				case __sunConstants.R_BRACE:
				case __sunConstants.L_PAREN:
				case __sunConstants.R_PAREN:
				case __sunConstants.L_BRACKET:
				case __sunConstants.R_BRACKET:
				case __sunConstants.COLON:
				case __sunConstants.SEMICOLON:
				case __sunConstants.COMMA:
				case __sunConstants.DOT:
				// case __sunConstants.ELLIPSIS: // do not exclude ellipsis for variadic parameters
				case __sunConstants.QMARK:
				{
					return null;
				}
			}

			// emergency fallback
			return null;
		}
		static __sunConstants GetId(Node node)
		{
			return (__sunConstants)node.Id;
		}

		public static bool IsKeyword(string name)
		{
			return keywords.Contains(name);
		}
	}
}
