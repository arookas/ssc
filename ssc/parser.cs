using PerCederberg.Grammatica.Runtime;
using System.Linq;

namespace arookas {
	class sunParser {
		static string[] sKeywords = {
			"import",
			"builtin", "function", "var", "const", "local",
			"if", "while", "do", "for",
			"return", "break", "continue",
			"yield", "exit", "lock", "unlock",
			"true", "false",
		};

		__sunParser mParser;
		sunScriptFile mFile;

		public sunNode Parse(sunScriptFile file) {
			mFile = file;
			using (var input = mFile.CreateReader()) {
				try {
					mParser = new __sunParser(input);
					var node = mParser.Parse();
					return CreateAst(node);
				}
				catch (ParserLogException ex) {
					throw new sunParserException(file.Name, mFile.Id, ex[0]);
				}
			}
		}

		sunNode CreateAst(Node node) {
			var ast = ConvertNode(node);
			if (ast == null) {
				return null;
			}
			if (node is Production) {
				var production = node as Production;
				for (int i = 0; i < production.Count; ++i) {
					var child = CreateAst(production[i]);
					if (child != null) {
						ast.Add(child);
					}
				}
			}
			if (ast.Count == 1) {
				switch (GetId(node)) {
					case __sunConstants.ROOT_STATEMENT:
					case __sunConstants.STATEMENT:
					case __sunConstants.COMPOUND_STATEMENT:
					case __sunConstants.COMPOUND_STATEMENT_ITEM:
					case __sunConstants.VARIABLE_AUGMENT:
					case __sunConstants.ASSIGNMENT_OPERATOR:
					case __sunConstants.BINARY_OPERATOR:
					case __sunConstants.UNARY_OPERATOR:
					case __sunConstants.AUGMENT_OPERATOR:
					case __sunConstants.TERM: {
							return ast[0];
						}
				}
			}
			return ast;
		}
		sunNode ConvertNode(Node node) {
			var id = GetId(node);
			var parent = GetId(node.Parent);
			var location = new sunSourceLocation(mFile.Name, mFile.Id, node.StartLine, node.StartColumn);
			var token = "";
			if (node is Token) {
				token = (node as Token).Image;
			}

			// statements
			switch (id) {
				case __sunConstants.SCRIPT: return new sunNode(location);
				case __sunConstants.ROOT_STATEMENT: return new sunNode(location);
				case __sunConstants.STATEMENT: return new sunNode(location);
				case __sunConstants.STATEMENT_BLOCK: return new sunStatementBlock(location);
				case __sunConstants.COMPOUND_STATEMENT: return new sunCompoundStatement(location);
				case __sunConstants.COMPOUND_STATEMENT_ITEM: return new sunNode(location);

				case __sunConstants.IMPORT_STATEMENT: return new sunImport(location);
				case __sunConstants.NAME_LABEL: return new sunNameLabel(location);

				case __sunConstants.YIELD_STATEMENT: return new sunYield(location);
				case __sunConstants.EXIT_STATEMENT: return new sunExit(location);
				case __sunConstants.LOCK_STATEMENT: return new sunLock(location);
				case __sunConstants.UNLOCK_STATEMENT: return new sunUnlock(location);
			}

			// literals
			switch (id) {
				case __sunConstants.INTEGER_LITERAL: return new sunIntLiteral(location, token);
				case __sunConstants.HEX_LITERAL: return new sunHexLiteral(location, token);
				case __sunConstants.FLOAT_LITERAL: return new sunFloatLiteral(location, token);
				case __sunConstants.ADDRESS_LITERAL: return new sunAddressLiteral(location, token);
				case __sunConstants.STRING_LITERAL: return new sunStringLiteral(location, token);
				case __sunConstants.IDENTIFIER: return new sunIdentifier(location, token);
				case __sunConstants.ELLIPSIS: return new sunEllipsis(location);
				case __sunConstants.TRUE: return new sunTrue(location);
				case __sunConstants.FALSE: return new sunFalse(location);
			}

			// operators
			switch (id) {
				case __sunConstants.ADD: return new sunAddOperator(location);
				case __sunConstants.SUB: {
						if (parent == __sunConstants.UNARY_OPERATOR) {
							return new sunNegateOperator(location);
						}
						return new sunSubtractOperator(location);
					}
				case __sunConstants.MUL: return new sunMultiplyOperator(location);
				case __sunConstants.DIV: return new sunDivideOperator(location);
				case __sunConstants.MOD: return new sunModuloOperator(location);

				case __sunConstants.BAND: return new sunBitwiseAndOperator(location);
				case __sunConstants.BOR: return new sunBitwiseOrOperator(location);
				case __sunConstants.LSH: return new sunShiftLeftOperator(location);
				case __sunConstants.RSH: return new sunShiftRightOperator(location);

				case __sunConstants.AND: return new sunLogicalAndOperator(location);
				case __sunConstants.OR: return new sunLogicalOrOperator(location);
				case __sunConstants.NOT: return new sunLogicalNotOperator(location);

				case __sunConstants.EQ: return new sunEqualOperator(location);
				case __sunConstants.NE: return new sunNotEqualOperator(location);
				case __sunConstants.LT: return new sunLessThanOperator(location);
				case __sunConstants.GT: return new sunGreaterThanOperator(location);
				case __sunConstants.LE: return new sunLessEqualOperator(location);
				case __sunConstants.GE: return new sunGreaterEqualOperator(location);

				case __sunConstants.ASSIGN: return new sunAssignOperator(location);
				case __sunConstants.ASSIGN_ADD: return new sunAssignAddOperator(location);
				case __sunConstants.ASSIGN_SUB: return new sunAssignSubtractOperator(location);
				case __sunConstants.ASSIGN_MUL: return new sunAssignMultiplyOperator(location);
				case __sunConstants.ASSIGN_DIV: return new sunAssignDivideOperator(location);
				case __sunConstants.ASSIGN_MOD: return new sunAssignModuloOperator(location);

				case __sunConstants.ASSIGN_BAND: return new sunAssignBitwiseAndOperator(location);
				case __sunConstants.ASSIGN_BOR: return new sunAssignBitwiseOrOperator(location);
				case __sunConstants.ASSIGN_LSH: return new sunAssignShiftLeftOperator(location);
				case __sunConstants.ASSIGN_RSH: return new sunAssignShiftRightOperator(location);

				case __sunConstants.INCREMENT: return new sunIncrement(location);
				case __sunConstants.DECREMENT: return new sunDecrement(location);

				case __sunConstants.ASSIGNMENT_OPERATOR: return new sunNode(location);
				case __sunConstants.TERNARY_OPERATOR: return new sunTernaryOperator(location);
				case __sunConstants.BINARY_OPERATOR: return new sunNode(location);
				case __sunConstants.UNARY_OPERATOR: return new sunNode(location);
				case __sunConstants.AUGMENT_OPERATOR: return new sunNode(location);
			}

			// expressions
			switch (id) {
				case __sunConstants.EXPRESSION: return new sunExpression(location);
				case __sunConstants.OPERAND: return new sunOperand(location);
				case __sunConstants.TERM: return new sunNode(location);

				case __sunConstants.UNARY_OPERATOR_LIST: return new sunUnaryOperatorList(location);

				case __sunConstants.PREFIX_AUGMENT: return new sunPrefixAugment(location);
				case __sunConstants.POSTFIX_AUGMENT: return new sunPostfixAugment(location);
			}

			// builtins
			switch (id) {
				case __sunConstants.BUILTIN_DECLARATION: return new sunBuiltinDeclaration(location);
				case __sunConstants.BUILTIN_MODIFIERS: return new sunNode(location);
			}

			// functions
			switch (id) {
				case __sunConstants.FUNCTION_DEFINITION: return new sunFunctionDefinition(location);
				case __sunConstants.FUNCTION_MODIFIERS: return new sunNode(location);
				case __sunConstants.FUNCTION_CALL: return new sunFunctionCall(location);

				case __sunConstants.PARAMETER_LIST: return new sunParameterList(location);
				case __sunConstants.ARGUMENT_LIST: return new sunNode(location);
			}

			// variables
			switch (id) {
				case __sunConstants.VARIABLE_REFERENCE: return new sunStorableReference(location);
				case __sunConstants.VARIABLE_DECLARATION: return new sunVariableDeclaration(location);
				case __sunConstants.VARIABLE_DEFINITION: return new sunVariableDefinition(location);
				case __sunConstants.VARIABLE_MODIFIERS: return new sunNode(location);
				case __sunConstants.VARIABLE_ASSIGNMENT: return new sunStorableAssignment(location);
				case __sunConstants.VARIABLE_AUGMENT: return new sunNode(location);
			}

			// constants
			switch (id) {
				case __sunConstants.CONST_DEFINITION: return new sunConstantDefinition(location);
				case __sunConstants.CONST_MODIFIERS: return new sunNode(location);
			}

			// flow control
			switch (id) {
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
			
			// keywords
			if (id == __sunConstants.CONST) {
				switch (parent) {
					case __sunConstants.FUNCTION_MODIFIERS:
					case __sunConstants.BUILTIN_MODIFIERS: {
							return new sunConstKeyword(location);
						}
				}
			}
			if (id == __sunConstants.LOCAL) {
				return new sunLocalKeyword(location);
			}

			// emergency fallback
			return null;
		}
		static __sunConstants GetId(Node node) {
			if (node == null) {
				return (__sunConstants)(-1);
			}
			return (__sunConstants)node.Id;
		}

		public static bool IsKeyword(string name) {
			return sKeywords.Contains(name);
		}
	}
}
