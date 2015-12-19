using arookas.Collections;
using arookas.IO.Binary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunSymbolTable : IEnumerable<sunSymbolInfo>
	{
		List<sunSymbolInfo> symbols = new List<sunSymbolInfo>(10);

		public int Count { get { return symbols.Count; } }
		public int BuiltinCount { get { return symbols.Count(sym => sym.Type == sunSymbolType.Builtin); } }
		public int FunctionCount { get { return symbols.Count(sym => sym.Type == sunSymbolType.Function); } }
		public int VariableCount { get { return symbols.Count(sym => sym.Type == sunSymbolType.Variable); } }

		public IEnumerable<sunCallableSymbolInfo> Callables { get { return symbols.OfType<sunCallableSymbolInfo>(); } }
		public IEnumerable<sunBuiltinInfo> Builtins { get { return symbols.Where(sym => sym.Type == sunSymbolType.Builtin).Cast<sunBuiltinInfo>(); } }
		public IEnumerable<sunFunctionInfo> Functions { get { return symbols.Where(sym => sym.Type == sunSymbolType.Function).Cast<sunFunctionInfo>(); } }
		public IEnumerable<sunVariableInfo> Variables { get { return symbols.Where(sym => sym.Type == sunSymbolType.Variable).Cast<sunVariableInfo>(); } }

		public void Add(sunSymbolInfo symbol) { symbols.Add(symbol); }
		public void Clear() { symbols.Clear(); }

		public void Write(aBinaryWriter writer)
		{
			int ofs = 0;
			foreach (var sym in this)
			{
				writer.WriteS32((int)sym.Type);
				writer.WriteS32(ofs);
				writer.Write32(sym.Data);

				// runtime fields
				writer.WriteS32(0);
				writer.WriteS32(0);

				ofs += sym.Name.Length + 1; // include null terminator
			}
			foreach (var sym in this)
			{
				writer.WriteString(sym.Name, aBinaryStringFormat.NullTerminated);
			}
		}

		public IEnumerator<sunSymbolInfo> GetEnumerator() { return symbols.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunSymbolInfo
	{
		public string Name { get; private set; }

		// symbol table
		public abstract sunSymbolType Type { get; }
		public abstract uint Data { get; }

		protected sunSymbolInfo(string name)
		{
			Name = name;
		}
	}

	abstract class sunCallableSymbolInfo : sunSymbolInfo
	{
		public sunParameterInfo Parameters { get; private set; }
		protected List<sunPoint> CallSites { get; private set; }

		public bool HasCallSites { get { return CallSites.Count > 0; } }

		protected sunCallableSymbolInfo(string name, sunParameterInfo parameterInfo)
			: base(name)
		{
			Parameters = parameterInfo;
			CallSites = new List<sunPoint>(10);
		}

		public abstract void OpenCallSite(sunContext context, int argumentCount);
		public abstract void CloseCallSites(sunContext context);

		public abstract void Compile(sunContext context);
	}

	class sunBuiltinInfo : sunCallableSymbolInfo
	{
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Builtin; } }
		public override uint Data { get { return (uint)Index; } }

		public sunBuiltinInfo(string name, sunParameterInfo parameters, int index)
			: base(name, parameters)
		{
			Index = index;
		}

		public override void Compile(sunContext context)
		{
			throw new InvalidOperationException("Cannot compile builtins.");
		}
		public override void OpenCallSite(sunContext context, int argumentCount)
		{
			context.Text.CallBuiltin(Index, argumentCount);
		}
		public override void CloseCallSites(sunContext context)
		{
			// do nothing
		}
	}

	class sunFunctionInfo : sunCallableSymbolInfo
	{
		sunNode Body { get; set; }
		public uint Offset { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Function; } }
		public override uint Data { get { return (uint)Offset; } }

		public sunFunctionInfo(string name, sunParameterInfo parameters, sunNode body)
			: base(name, parameters)
		{
			Body = body;
		}

		public override void Compile(sunContext context)
		{
			Offset = context.Text.Offset;
			context.Scopes.Push(sunScopeType.Function);
			foreach (var parameter in Parameters)
			{
				context.DeclareParameter(parameter);
			}
			context.Text.StoreDisplay(1);
			Body.Compile(context);
			context.Text.ReturnVoid();
			context.Scopes.Pop();
		}
		public override void OpenCallSite(sunContext context, int argumentCount)
		{
			var point = context.Text.CallFunction(argumentCount);
			CallSites.Add(point);
		}
		public override void CloseCallSites(sunContext context)
		{
			foreach (var callSite in CallSites)
			{
				context.Text.ClosePoint(callSite, Offset);
			}
		}

		static int CalculateMaxLocalCount(sunNode node)
		{
			int locals = 0;
			int maxChildLocals = 0;
			foreach (var child in node)
			{
				if (child is sunVariableDeclaration || child is sunVariableDefinition)
				{
					++locals;
				}
				else if (child is sunCompoundStatement)
				{
					locals += CalculateMaxLocalCount(child); // HACK: compound statements aren't their own scope
				}
				else
				{
					int childLocals = CalculateMaxLocalCount(child);
					if (childLocals > maxChildLocals)
					{
						maxChildLocals = childLocals;
					}
				}
			}
			return locals + maxChildLocals;
		}
	}

	class sunParameterInfo : IEnumerable<string>
	{
		string[] Parameters { get; set; }
		public int Minimum { get { return Parameters.Length; } }
		public bool IsVariadic { get; private set; }

		public sunParameterInfo(IEnumerable<sunIdentifier> parameters, bool variadic)
		{
			// validate parameter names
			var duplicate = parameters.FirstOrDefault(a => parameters.Count(b => a.Value == b.Value) > 1);
			if (duplicate != null)
			{
				throw new sunRedeclaredParameterException(duplicate);
			}
			Parameters = parameters.Select(i => i.Value).ToArray();
			IsVariadic = variadic;
		}
		public sunParameterInfo(IEnumerable<string> parameters, bool variadic)
		{
			// validate parameter names
			Parameters = parameters.ToArray();
			IsVariadic = variadic;
		}

		public bool ValidateArgumentCount(int count)
		{
			return IsVariadic ? count >= Minimum : count == Minimum;
		}

		public IEnumerator<string> GetEnumerator() { return Parameters.GetArrayEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	class sunVariableInfo : sunSymbolInfo
	{
		public int Display { get; private set; }
		public int Index { get; private set; }

		// symbol table
		public override sunSymbolType Type { get { return sunSymbolType.Variable; } }
		public override uint Data { get { return (uint)Index; } }

		public sunVariableInfo(string name, int display, int index)
			: base(name)
		{
			Display = display;
			Index = index;
		}
	}

	enum sunSymbolType
	{
		Builtin,
		Function,
		Variable,
	}
}
