using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunBuiltinDeclaration : sunNode
	{
		public sunIdentifier Builtin { get { return this[0] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[1] as sunParameterList; } }

		public sunBuiltinDeclaration(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.DeclareBuiltin(this);
		}
	}

	class sunFunctionDefinition : sunNode
	{
		public sunIdentifier Function { get { return this[0] as sunIdentifier; } }
		public sunParameterList Parameters { get { return this[1] as sunParameterList; } }
		public sunNode Body { get { return this[2]; } }

		public sunFunctionDefinition(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			context.DefineFunction(this); // possibly counter intuitively, this defines the function in the context; it doesn't compile the definition body
		}
	}

	class sunFunctionCall : sunNode
	{
		public sunIdentifier Function { get { return this[0] as sunIdentifier; } }
		public sunNode Arguments { get { return this[1] as sunNode; } }

		bool IsStatement { get { return !(Parent is sunOperand); } }

		public sunFunctionCall(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var callableInfo = context.MustResolveCallable(this);
			if (!callableInfo.Parameters.ValidateArgumentCount(Arguments.Count))
			{
				throw new sunArgumentCountException(this, callableInfo);
			}
			Arguments.Compile(context);
			callableInfo.OpenCallSite(context, Arguments.Count);
			if (IsStatement)
			{
				context.Text.Pop();
			}
		}
	}

	class sunParameterList : sunNode
	{
		public IEnumerable<sunIdentifier> Parameters { get { return this.OfType<sunIdentifier>(); } }
		public bool IsVariadic { get { return Count > 0 && this[Count - 1] is sunEllipsis; } }
		public sunParameterInfo ParameterInfo { get { return new sunParameterInfo(Parameters, IsVariadic); } }

		public sunParameterList(sunSourceLocation location)
			: base(location)
		{
			int count = this.Count(i => i is sunEllipsis);
			if (count > 1 || (count > 0 && !(this[Count - 1] is sunEllipsis)))
			{
				throw new sunVariadicParameterListException(this);
			}
		}
	}

	class sunEllipsis : sunNode
	{
		public sunEllipsis(sunSourceLocation location)
			: base(location)
		{

		}
	}
}
