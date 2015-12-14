using arookas.IO.Binary;

namespace arookas
{
	class sunWriter
	{
		aBinaryWriter writer;

		public uint Offset { get { return (uint)writer.Position; } }

		public sunWriter(aBinaryWriter writer)
		{
			this.writer = writer;
		}

		public sunPoint OpenPoint() { return new sunPoint(Offset); }
		public void ClosePoint(sunPoint point)
		{
			ClosePoint(point, (uint)writer.Position);
		}
		public void ClosePoint(sunPoint point, uint offset)
		{
			writer.Keep();
			writer.Goto(point.Offset);
			writer.Write32(offset);
			writer.Back();
		}

		public void PushInt(int value)
		{
			switch (value) // shortcut commands
			{
				case 0: writer.Write8(0x25); return;
				case 1: writer.Write8(0x26); return;
			}
			writer.Write8(0x00);
			writer.WriteS32(value);
		}
		public void PushFloat(float value)
		{
			writer.Write8(0x01);
			writer.WriteF32(value);
		}
		public void PushData(int dataIndex)
		{
			writer.Write8(0x02);
			writer.WriteS32(dataIndex);
		}
		public void PushAddress(int value)
		{
			writer.Write8(0x03);
			writer.WriteS32(value);
		}
		public void PushVariable(sunVariableInfo variableInfo)
		{
			PushVariable(variableInfo.Display, variableInfo.Index);
		}
		public void PushVariable(int display, int variableIndex)
		{
			writer.Write8(0x04);
			writer.WriteS32(display);
			writer.WriteS32(variableIndex);
		}

		public void Nop()
		{
			writer.Write8(0x05);
		}

		public void IncVariable(sunVariableInfo variableInfo)
		{
			IncVariable(variableInfo.Display, variableInfo.Index);
		}
		public void DecVariable(sunVariableInfo variableInfo)
		{
			DecVariable(variableInfo.Display, variableInfo.Index);
		}
		public void IncVariable(int display, int variableIndex)
		{
			writer.Write8(0x06);
			writer.WriteS32(display);
			writer.WriteS32(variableIndex);
		}
		public void DecVariable(int display, int variableIndex)
		{
			writer.Write8(0x07);
			writer.WriteS32(display);
			writer.WriteS32(variableIndex);
		}

		public void Add() { writer.Write8(0x08); }
		public void Sub() { writer.Write8(0x09); }
		public void Mul() { writer.Write8(0x0A); }
		public void Div() { writer.Write8(0x0B); }
		public void Mod() { writer.Write8(0x0C); }

		public void StoreVariable(sunVariableInfo variableInfo)
		{
			StoreVariable(variableInfo.Display, variableInfo.Index);
		}
		public void StoreVariable(int display, int variableIndex)
		{
			writer.Write8(0x0D);
			writer.Write8(0x04); // unused (skipped over by TSpcInterp)
			writer.WriteS32(display);
			writer.WriteS32(variableIndex);
		}

		public void Eq() { writer.Write8(0x0E); }
		public void NtEq() { writer.Write8(0x0F); }
		public void Gt() { writer.Write8(0x10); }
		public void Lt() { writer.Write8(0x11); }
		public void GtEq() { writer.Write8(0x12); }
		public void LtEq() { writer.Write8(0x13); }
		public void Neg() { writer.Write8(0x14); }
		public void LogNOT() { writer.Write8(0x15); }
		public void LogAND() { writer.Write8(0x16); }
		public void LogOR() { writer.Write8(0x17); }
		public void BitAND() { writer.Write8(0x18); }
		public void BitOR() { writer.Write8(0x19); }
		public void ShL() { writer.Write8(0x1A); }
		public void ShR() { writer.Write8(0x1B); }

		public sunPoint CallFunction(int argumentCount)
		{
			writer.Write8(0x1C);
			sunPoint point = OpenPoint();
			writer.Write32(0); // dummy
			writer.WriteS32(argumentCount);
			return point;
		}
		public void CallFunction(sunPoint point, int argumentCount)
		{
			writer.Write8(0x1C);
			writer.Write32(point.Offset);
			writer.WriteS32(argumentCount);
		}
		public void CallBuiltin(int symbolIndex, int argumentCount)
		{
			writer.Write8(0x1D);
			writer.WriteS32(symbolIndex);
			writer.WriteS32(argumentCount);
		}

		public void DeclareLocal(int count)
		{
			writer.Write8(0x1E);
			writer.WriteS32(count);
		}
		public void StoreDisplay(int display)
		{
			writer.Write8(0x1F);
			writer.WriteS32(display);
		}

		public void ReturnValue() { writer.Write8(0x20); }
		public void ReturnVoid() { writer.Write8(0x21); }

		public sunPoint GotoIfZero()
		{
			writer.Write8(0x22);
			sunPoint point = OpenPoint();
			writer.Write32(0); // dummy
			return point;
		}
		public sunPoint Goto()
		{
			writer.Write8(0x23);
			sunPoint point = OpenPoint();
			writer.Write32(0); // dummy
			return point;
		}
		public void GotoIfZero(sunPoint point)
		{
			writer.Write8(0x22);
			writer.Write32(point.Offset);
		}
		public void Goto(sunPoint point)
		{
			writer.Write8(0x23);
			writer.Write32(point.Offset);
		}
		public void Pop() { writer.Write8(0x24); }

		public void Terminate() { writer.Write8(0x27); }
	}

	struct sunPoint
	{
		readonly uint offset;
		public uint Offset { get { return offset; } }

		public sunPoint(uint offset)
		{
			this.offset = offset;
		}
	}
}
