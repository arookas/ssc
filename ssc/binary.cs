using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using arookas.IO.Binary;

namespace arookas {
	public abstract class sunBinary {
		public abstract uint Offset { get; }

		public virtual void Open() {
			// stub
		}
		public virtual void Close() {
			// stub
		}

		// text
		public abstract void Keep();
		public abstract void Back();
		public abstract void Goto(uint offset);

		public virtual void BeginText() {
			// stub
		}
		public abstract void WriteINT(int value);
		public abstract void WriteFLT(float value);
		public abstract void WriteSTR(int index);
		public abstract void WriteADR(uint value);
		public abstract void WriteVAR(int display, int index);
		public abstract void WriteNOP();
		public abstract void WriteINC(int display, int index);
		public abstract void WriteDEC(int display, int index);
		public abstract void WriteADD();
		public abstract void WriteSUB();
		public abstract void WriteMUL();
		public abstract void WriteDIV();
		public abstract void WriteMOD();
		public abstract void WriteASS(int display, int index);
		public abstract void WriteEQ();
		public abstract void WriteNE();
		public abstract void WriteGT();
		public abstract void WriteLT();
		public abstract void WriteGE();
		public abstract void WriteLE();
		public abstract void WriteNEG();
		public abstract void WriteNOT();
		public abstract void WriteAND();
		public abstract void WriteOR();
		public abstract void WriteBAND();
		public abstract void WriteBOR();
		public abstract void WriteSHL();
		public abstract void WriteSHR();
		public abstract void WriteCALL(uint offset, int count);
		public abstract void WriteFUNC(int index, int count);
		public abstract void WriteMKFR(int count);
		public abstract void WriteMKDS(int display);
		public abstract void WriteRET();
		public abstract void WriteRET0();
		public abstract void WriteJNE(uint offset);
		public abstract void WriteJMP(uint offset);
		public abstract void WritePOP();
		public abstract void WriteINT0();
		public abstract void WriteINT1();
		public abstract void WriteEND();
		public virtual void EndText() {
			// stub
		}

		public virtual void BeginData() {
			// stub
		}
		public abstract void WriteData(string data);
		public virtual void EndData() {
			// stub
		}

		public virtual void BeginSymbol() {
			// stub
		}
		public abstract void WriteSymbol(sunSymbolType type, string name, uint data);
		public virtual void EndSymbol() {
			// stub
		}
	}

	sealed class sunSpcBinary : sunBinary {
		aBinaryWriter mWriter;
		sunSpcStringTable mStringTable;
		uint mTextOffset, mDataOffset, mSymbolOffset;
		int mDataCount, mSymbolCount, mVarCount;

		public override uint Offset {
			get { return (uint)mWriter.Position; }
		}

		public sunSpcBinary(Stream output) {
			mWriter = new aBinaryWriter(output, Endianness.Big, Encoding.GetEncoding(932));
			mStringTable = new sunSpcStringTable(Encoding.GetEncoding(932));
		}

		public override void Open() {
			mWriter.PushAnchor();
			WriteHeader();
		}
		public override void Close() {
			WriteHeader();
			mWriter.PopAnchor();
		}

		void WriteHeader() {
			mWriter.Goto(0);
			mWriter.Write8(0x53); // 'S'
			mWriter.Write8(0x50); // 'P'
			mWriter.Write8(0x43); // 'C'
			mWriter.Write8(0x42); // 'B'
			mWriter.Write32(mTextOffset);
			mWriter.Write32(mDataOffset);
			mWriter.WriteS32(mDataCount);
			mWriter.Write32(mSymbolOffset);
			mWriter.WriteS32(mSymbolCount);
			mWriter.WriteS32(mVarCount);
		}

		// text
		public override void Keep() {
			mWriter.Keep();
		}
		public override void Back() {
			mWriter.Back();
		}
		public override void Goto(uint offset) {
			mWriter.Goto(offset);
		}

		public override void BeginText() {
			mTextOffset = Offset;
			mWriter.PushAnchor();
		}
		public override void WriteINT(int value) {
			switch (value) { // shortcut commands
				case 0: WriteINT0(); return;
				case 1: WriteINT1(); return;
			}
			TraceInstruction("int {0} # ${0:X}", value);
			mWriter.Write8(0x00);
			mWriter.WriteS32(value);
		}
		public override void WriteFLT(float value) {
			TraceInstruction("flt {0}", value);
			mWriter.Write8(0x01);
			mWriter.WriteF32(value);
		}
		public override void WriteSTR(int index) {
			TraceInstruction("str {0}", index);
			mWriter.Write8(0x02);
			mWriter.WriteS32(index);
		}
		public override void WriteADR(uint value) {
			TraceInstruction("adr ${0:X8}", value);
			mWriter.Write8(0x03);
			mWriter.Write32(value);
		}
		public override void WriteVAR(int display, int index) {
			TraceInstruction("var {0} {1}", display, index);
			mWriter.Write8(0x04);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public override void WriteNOP() {
			TraceInstruction("nop");
			mWriter.Write8(0x05);
		}
		public override void WriteINC(int display, int index) {
			TraceInstruction("inc {0} {1}", display, index);
			mWriter.Write8(0x06);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public override void WriteDEC(int display, int index) {
			TraceInstruction("dec {0} {1}", display, index);
			mWriter.Write8(0x07);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public override void WriteADD() {
			TraceInstruction("add");
			mWriter.Write8(0x08);
		}
		public override void WriteSUB() {
			TraceInstruction("sub");
			mWriter.Write8(0x09);
		}
		public override void WriteMUL() {
			TraceInstruction("mul");
			mWriter.Write8(0x0A);
		}
		public override void WriteDIV() {
			TraceInstruction("div");
			mWriter.Write8(0x0B);
		}
		public override void WriteMOD() {
			TraceInstruction("mod");
			mWriter.Write8(0x0C);
		}
		public override void WriteASS(int display, int index) {
			TraceInstruction("ass {0} {1}", display, index);
			mWriter.Write8(0x0D);
			mWriter.Write8(0x04); // unused (skipped over by TSpcInterp)
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public override void WriteEQ() {
			TraceInstruction("eq");
			mWriter.Write8(0x0E);
		}
		public override void WriteNE() {
			TraceInstruction("ne");
			mWriter.Write8(0x0F);
		}
		public override void WriteGT() {
			TraceInstruction("gt");
			mWriter.Write8(0x10);
		}
		public override void WriteLT() {
			TraceInstruction("lt");
			mWriter.Write8(0x11);
		}
		public override void WriteGE() {
			TraceInstruction("ge");
			mWriter.Write8(0x12);
		}
		public override void WriteLE() {
			TraceInstruction("le");
			mWriter.Write8(0x13);
		}
		public override void WriteNEG() {
			TraceInstruction("neg");
			mWriter.Write8(0x14);
		}
		public override void WriteNOT() {
			TraceInstruction("not");
			mWriter.Write8(0x15);
		}
		public override void WriteAND() {
			TraceInstruction("and");
			mWriter.Write8(0x16);
		}
		public override void WriteOR() {
			TraceInstruction("or");
			mWriter.Write8(0x17);
		}
		public override void WriteBAND() {
			TraceInstruction("band");
			mWriter.Write8(0x18);
		}
		public override void WriteBOR() {
			TraceInstruction("bor");
			mWriter.Write8(0x19);
		}
		public override void WriteSHL() {
			TraceInstruction("shl");
			mWriter.Write8(0x1A);
		}
		public override void WriteSHR() {
			TraceInstruction("shr");
			mWriter.Write8(0x1B);
		}
		public override void WriteCALL(uint offset, int count) {
			TraceInstruction("call ${0:X8} {1}", offset, count);
			mWriter.Write8(0x1C);
			mWriter.Write32(offset);
			mWriter.WriteS32(count);
		}
		public override void WriteFUNC(int index, int count) {
			TraceInstruction("func {0} {1}", index, count);
			mWriter.Write8(0x1D);
			mWriter.WriteS32(index);
			mWriter.WriteS32(count);
		}
		public override void WriteMKFR(int count) {
			TraceInstruction("mkfr {0}", count);
			mWriter.Write8(0x1E);
			mWriter.WriteS32(count);
		}
		public override void WriteMKDS(int display) {
			TraceInstruction("mkds {0}", display);
			mWriter.Write8(0x1F);
			mWriter.WriteS32(display);
		}
		public override void WriteRET() {
			TraceInstruction("ret");
			mWriter.Write8(0x20);
		}
		public override void WriteRET0() {
			TraceInstruction("ret0");
			mWriter.Write8(0x21);
		}
		public override void WriteJNE(uint offset) {
			TraceInstruction("jne ${0:X8}", offset);
			mWriter.Write8(0x22);
			mWriter.Write32(offset);
		}
		public override void WriteJMP(uint offset) {
			TraceInstruction("jmp ${0:X8}", offset);
			mWriter.Write8(0x23);
			mWriter.Write32(offset);
		}
		public override void WritePOP() {
			TraceInstruction("pop");
			mWriter.Write8(0x24);
		}
		public override void WriteINT0() {
			TraceInstruction("int0");
			mWriter.Write8(0x25);
		}
		public override void WriteINT1() {
			TraceInstruction("int1");
			mWriter.Write8(0x26);
		}
		public override void WriteEND() {
			TraceInstruction("end");
			mWriter.Write8(0x27);
		}
		public override void EndText() {
			mWriter.PopAnchor();
		}

		[Conditional("DEBUG")]
		void TraceInstruction(string format, params object[] args) {
			var instruction = String.Format(format, args);
			Debug.WriteLine("{0:X8} {1}", mWriter.Position, instruction);
		}

		// data
		public override void BeginData() {
			mDataCount = 0;
			mDataOffset = Offset;
			mWriter.PushAnchor();
			mStringTable.Clear();
		}
		public override void WriteData(string data) {
			if (data == null) {
				throw new ArgumentNullException("data");
			}
			mWriter.Write32(mStringTable.Add(data));
			++mDataCount;
		}
		public override void EndData() {
			mWriter.WriteString(mStringTable.ToString());
			mWriter.PopAnchor();
			mStringTable.Clear();
		}

		// symbol
		public override void BeginSymbol() {
			mSymbolCount = 0;
			mSymbolOffset = Offset;
			mWriter.PushAnchor();
			mStringTable.Clear();
		}
		public override void WriteSymbol(sunSymbolType type, string name, uint data) {
			mWriter.WriteS32((int)type);
			mWriter.Write32(mStringTable.Add(name));
			mWriter.Write32(data);
			mWriter.Write32(0u); // runtime field (hash)
			mWriter.Write32(0u); // runtime field (funcptr)
			++mSymbolCount;
			if (type == sunSymbolType.Variable) {
				++mVarCount;
			}
		}
		public override void EndSymbol() {
			mWriter.WriteString(mStringTable.ToString());
			mWriter.PopAnchor();
			mStringTable.Clear();
		}

		class sunSpcStringTable {
			StringBuilder mBuilder;
			Encoding mEncoding;
			uint mSize;

			public sunSpcStringTable(Encoding encoding) {
				mBuilder = new StringBuilder(1024);
				mEncoding = encoding;
			}

			public uint Add(string value) {
				var size = mSize;
				mBuilder.Append(value);
				mBuilder.Append('\0');
				mSize += (uint)mEncoding.GetByteCount(value) + 1u; // + null terminator
				return size;
			}
			public override string ToString() {
				return mBuilder.ToString();
			}
			public void Clear() {
				mBuilder.Clear();
				mSize = 0;
			}
		}
	}
}
