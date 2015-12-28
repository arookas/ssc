namespace arookas
{
	class sunYield : sunNode
	{
		public sunYield(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("yield");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunExit : sunNode
	{
		public sunExit(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("exit");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunDump : sunNode
	{
		public sunDump(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("dump");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunLock : sunNode
	{
		public sunLock(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("lock");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunUnlock : sunNode
	{
		public sunUnlock(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("unlock");
			context.Text.CallBuiltin(builtinInfo.Index, 0);
			context.Text.Pop();
		}
	}

	class sunIntCast : sunNode
	{
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		public sunIntCast(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("int");
			Argument.Compile(context);
			context.Text.CallBuiltin(builtinInfo.Index, 1);
		}
	}

	class sunFloatCast : sunNode
	{
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		public sunFloatCast(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("float");
			Argument.Compile(context);
			context.Text.CallBuiltin(builtinInfo.Index, 1);
		}
	}

	class sunTypeofCast : sunNode
	{
		public sunExpression Argument { get { return this[0] as sunExpression; } }

		public sunTypeofCast(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("typeof");
			Argument.Compile(context);
			context.Text.CallBuiltin(builtinInfo.Index, 1);
		}
	}

	class sunPrint : sunNode
	{
		public sunNode ArgumentList { get { return this[0]; } }

		public sunPrint(sunSourceLocation location)
			: base(location)
		{

		}

		public override void Compile(sunContext context)
		{
			var builtinInfo = context.ResolveSystemBuiltin("print");
			ArgumentList.Compile(context);
			context.Text.CallBuiltin(builtinInfo.Index, ArgumentList.Count);
			context.Text.Pop();
		}
	}
}
