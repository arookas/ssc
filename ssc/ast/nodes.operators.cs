using PerCederberg.Grammatica.Runtime;

namespace arookas {
	enum Associativity {
		Left,
		Right,
	}

	abstract class sunOperator : sunNode {
		public virtual Associativity Associativity { get { return Associativity.Left; } }
		public abstract int Precedence { get; }

		public bool IsLeftAssociative { get { return Associativity == Associativity.Left; } }
		public bool IsRightAssociative { get { return Associativity == Associativity.Right; } }

		protected sunOperator(sunSourceLocation location)
			: base(location) { }
	}

	// precedence 0
	class sunLogicalOrOperator : sunOperator {
		public override int Precedence { get { return 0; } }

		public sunLogicalOrOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteOR(); }
	}

	// precedence 1
	class sunLogicalAndOperator : sunOperator {
		public override int Precedence { get { return 1; } }

		public sunLogicalAndOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteAND(); }
	}

	// precedence 2
	class sunBitwiseOrOperator : sunOperator {
		public override int Precedence { get { return 2; } }

		public sunBitwiseOrOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteBOR(); }
	}

	// precedence 3
	class sunBitwiseAndOperator : sunOperator {
		public override int Precedence { get { return 3; } }

		public sunBitwiseAndOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteBAND(); }
	}

	// precedence 4
	class sunEqualOperator : sunOperator {
		public override int Precedence { get { return 4; } }

		public sunEqualOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteEQ(); }
	}

	class sunNotEqualOperator : sunOperator {
		public override int Precedence { get { return 4; } }

		public sunNotEqualOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNE(); }
	}

	// precedence 5
	class sunLessThanOperator : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunLessThanOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteLT(); }
	}

	class sunLessEqualOperator : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunLessEqualOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteLE(); }
	}

	class sunGreaterThanOperator : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunGreaterThanOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteGT(); }
	}

	class sunGreaterEqualOperator : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunGreaterEqualOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteGE(); }
	}

	// precedence 6
	class sunShiftLeftOperator : sunOperator {
		public override int Precedence { get { return 6; } }

		public sunShiftLeftOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSHL(); }
	}

	class sunShiftRightOperator : sunOperator {
		public override int Precedence { get { return 6; } }

		public sunShiftRightOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSHR(); }
	}

	// precedence 7
	class sunAddOperator : sunOperator {
		public override int Precedence { get { return 7; } }

		public sunAddOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteADD(); }
	}

	class sunSubtractOperator : sunOperator {
		public override int Precedence { get { return 7; } }

		public sunSubtractOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSUB(); }
	}

	// precedence 8
	class sunMultiplyOperator : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunMultiplyOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteMUL(); }
	}

	class sunDivideOperator : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunDivideOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteDIV(); }
	}

	class sunModuloOperator : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunModuloOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteMOD(); }
	}

	// precedence 9
	class sunLogicalNotOperator : sunOperator {
		public override int Precedence { get { return 9; } }

		public sunLogicalNotOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNOT(); }
	}
	class sunNegateOperator : sunOperator {
		public override int Precedence { get { return 9; } }

		public sunNegateOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNEG(); }
	}

	// assignment operators
	class sunAssignOperator : sunOperator {
		public override Associativity Associativity { get { return Associativity.Right; } }
		public override int Precedence { get { return -1; } }

		public sunAssignOperator(sunSourceLocation location)
			: base(location) { }

		public virtual void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			expression.Compile(compiler);
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignAddOperator : sunAssignOperator {
		public sunAssignAddOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteADD();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignSubtractOperator : sunAssignOperator {
		public sunAssignSubtractOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSUB();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignMultiplyOperator : sunAssignOperator {
		public sunAssignMultiplyOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteMUL();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignDivideOperator : sunAssignOperator {
		public sunAssignDivideOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteDIV();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignModuloOperator : sunAssignOperator {
		public sunAssignModuloOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteMOD();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitwiseAndOperator : sunAssignOperator {
		public sunAssignBitwiseAndOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteBAND();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitwiseOrOperator : sunAssignOperator {
		public sunAssignBitwiseOrOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteBOR();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignShiftLeftOperator : sunAssignOperator {
		public sunAssignShiftLeftOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSHL();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignShiftRightOperator : sunAssignOperator {
		public sunAssignShiftRightOperator(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSHR();
			symbol.CompileSet(compiler);
		}
	}
}
