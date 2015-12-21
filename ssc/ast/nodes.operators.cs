using PerCederberg.Grammatica.Runtime;

namespace arookas
{
	enum Associativity
	{
		Left,
		Right,
	}

	abstract class sunOperator : sunNode
	{
		public virtual Associativity Associativity { get { return Associativity.Left; } }
		public abstract int Precedence { get; }

		public bool IsLeftAssociative { get { return Associativity == Associativity.Left; } }
		public bool IsRightAssociative { get { return Associativity == Associativity.Right; } }

		protected sunOperator(sunSourceLocation location)
			: base(location)
		{

		}
	}

	// precedence 0
	class sunLogOR : sunOperator
	{
		public override int Precedence { get { return 0; } }

		public sunLogOR(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.LogOR(); }
	}

	// precedence 1
	class sunLogAND : sunOperator
	{
		public override int Precedence { get { return 1; } }

		public sunLogAND(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.LogAND(); }
	}

	// precedence 2
	class sunBitOR : sunOperator
	{
		public override int Precedence { get { return 2; } }

		public sunBitOR(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.BitOR(); }
	}

	// precedence 3
	class sunBitAND : sunOperator
	{
		public override int Precedence { get { return 3; } }

		public sunBitAND(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.BitAND(); }
	}

	// precedence 4
	class sunEq : sunOperator
	{
		public override int Precedence { get { return 4; } }

		public sunEq(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Eq(); }
	}

	class sunNtEq : sunOperator
	{
		public override int Precedence { get { return 4; } }

		public sunNtEq(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.NtEq(); }
	}

	// precedence 5
	class sunLt : sunOperator
	{
		public override int Precedence { get { return 5; } }

		public sunLt(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Lt(); }
	}

	class sunLtEq : sunOperator
	{
		public override int Precedence { get { return 5; } }

		public sunLtEq(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.LtEq(); }
	}

	class sunGt : sunOperator
	{
		public override int Precedence { get { return 5; } }

		public sunGt(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Gt(); }
	}

	class sunGtEq : sunOperator
	{
		public override int Precedence { get { return 5; } }

		public sunGtEq(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.GtEq(); }
	}

	// precedence 6
	class sunBitLsh : sunOperator
	{
		public override int Precedence { get { return 6; } }

		public sunBitLsh(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.ShL(); }
	}

	class sunBitRsh : sunOperator
	{
		public override int Precedence { get { return 6; } }

		public sunBitRsh(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.ShR(); }
	}

	// precedence 7
	class sunAdd : sunOperator
	{
		public override int Precedence { get { return 7; } }

		public sunAdd(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Add(); }
	}

	class sunSub : sunOperator
	{
		public override int Precedence { get { return 7; } }

		public sunSub(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Sub(); }
	}

	// precedence 8
	class sunMul : sunOperator
	{
		public override int Precedence { get { return 8; } }

		public sunMul(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Mul(); }
	}

	class sunDiv : sunOperator
	{
		public override int Precedence { get { return 8; } }

		public sunDiv(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Div(); }
	}

	class sunMod : sunOperator
	{
		public override int Precedence { get { return 8; } }

		public sunMod(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Mod(); }
	}

	// precedence 9
	class sunLogNOT : sunOperator
	{
		public override int Precedence { get { return 9; } }

		public sunLogNOT(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.LogNOT(); }
	}
	class sunNeg : sunOperator
	{
		public override int Precedence { get { return 9; } }

		public sunNeg(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context) { context.Text.Neg(); }
	}

	// assignment operators
	class sunAssign : sunOperator
	{
		public override Associativity Associativity { get { return Associativity.Right; } }
		public override int Precedence { get { return -1; } }

		public sunAssign(sunSourceLocation location)
			: base(location)
		{

		}

		public virtual void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			expression.Compile(context);
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignAdd : sunAssign
	{
		public sunAssignAdd(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.Add();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignSub : sunAssign
	{
		public sunAssignSub(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.Sub();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignMul : sunAssign
	{
		public sunAssignMul(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.Mul();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignDiv : sunAssign
	{
		public sunAssignDiv(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.Div();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignMod : sunAssign
	{
		public sunAssignMod(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.Mod();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignBitAND : sunAssign
	{
		public sunAssignBitAND(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.BitAND();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignBitOR : sunAssign
	{
		public sunAssignBitOR(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.BitOR();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignBitLsh : sunAssign
	{
		public sunAssignBitLsh(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.ShL();
			context.Text.StoreVariable(variableInfo);
		}
	}

	class sunAssignBitRsh : sunAssign
	{
		public sunAssignBitRsh(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context, sunVariableSymbol variableInfo, sunExpression expression)
		{
			context.Text.PushVariable(variableInfo);
			expression.Compile(context);
			context.Text.ShR();
			context.Text.StoreVariable(variableInfo);
		}
	}
}
