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
	class sunLogOR : sunOperator {
		public override int Precedence { get { return 0; } }

		public sunLogOR(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteOR(); }
	}

	// precedence 1
	class sunLogAND : sunOperator {
		public override int Precedence { get { return 1; } }

		public sunLogAND(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteAND(); }
	}

	// precedence 2
	class sunBitOR : sunOperator {
		public override int Precedence { get { return 2; } }

		public sunBitOR(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteBOR(); }
	}

	// precedence 3
	class sunBitAND : sunOperator {
		public override int Precedence { get { return 3; } }

		public sunBitAND(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteBAND(); }
	}

	// precedence 4
	class sunEq : sunOperator {
		public override int Precedence { get { return 4; } }

		public sunEq(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteEQ(); }
	}

	class sunNtEq : sunOperator {
		public override int Precedence { get { return 4; } }

		public sunNtEq(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNE(); }
	}

	// precedence 5
	class sunLt : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunLt(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteLT(); }
	}

	class sunLtEq : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunLtEq(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteLE(); }
	}

	class sunGt : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunGt(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteGT(); }
	}

	class sunGtEq : sunOperator {
		public override int Precedence { get { return 5; } }

		public sunGtEq(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteGE(); }
	}

	// precedence 6
	class sunBitLsh : sunOperator {
		public override int Precedence { get { return 6; } }

		public sunBitLsh(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSHL(); }
	}

	class sunBitRsh : sunOperator {
		public override int Precedence { get { return 6; } }

		public sunBitRsh(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSHR(); }
	}

	// precedence 7
	class sunAdd : sunOperator {
		public override int Precedence { get { return 7; } }

		public sunAdd(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteADD(); }
	}

	class sunSub : sunOperator {
		public override int Precedence { get { return 7; } }

		public sunSub(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteSUB(); }
	}

	// precedence 8
	class sunMul : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunMul(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteMUL(); }
	}

	class sunDiv : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunDiv(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteDIV(); }
	}

	class sunMod : sunOperator {
		public override int Precedence { get { return 8; } }

		public sunMod(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteMOD(); }
	}

	// precedence 9
	class sunLogNOT : sunOperator {
		public override int Precedence { get { return 9; } }

		public sunLogNOT(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNOT(); }
	}
	class sunNeg : sunOperator {
		public override int Precedence { get { return 9; } }

		public sunNeg(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler) { compiler.Binary.WriteNEG(); }
	}

	// assignment operators
	class sunAssign : sunOperator {
		public override Associativity Associativity { get { return Associativity.Right; } }
		public override int Precedence { get { return -1; } }

		public sunAssign(sunSourceLocation location)
			: base(location) { }

		public virtual void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			expression.Compile(compiler);
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignAdd : sunAssign {
		public sunAssignAdd(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteADD();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignSub : sunAssign {
		public sunAssignSub(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSUB();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignMul : sunAssign {
		public sunAssignMul(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteMUL();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignDiv : sunAssign {
		public sunAssignDiv(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteDIV();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignMod : sunAssign {
		public sunAssignMod(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteMOD();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitAND : sunAssign {
		public sunAssignBitAND(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteBAND();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitOR : sunAssign {
		public sunAssignBitOR(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteBOR();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitLsh : sunAssign {
		public sunAssignBitLsh(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSHL();
			symbol.CompileSet(compiler);
		}
	}

	class sunAssignBitRsh : sunAssign {
		public sunAssignBitRsh(sunSourceLocation location)
			: base(location) { }

		public override void Compile(sunCompiler compiler, sunStorableSymbol symbol, sunExpression expression) {
			symbol.CompileGet(compiler);
			expression.Compile(compiler);
			compiler.Binary.WriteSHR();
			symbol.CompileSet(compiler);
		}
	}
}
