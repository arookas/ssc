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

		public virtual sunPoint OpenPoint() {
			return new sunPoint(Offset);
		}
		public virtual void ClosePoint(sunPoint point) {
			ClosePoint(point, Offset);
		}
		public abstract void ClosePoint(sunPoint point, uint offset);

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
		public abstract sunPoint WriteJNE();
		public abstract void WriteJNE(sunPoint point);
		public abstract sunPoint WriteJMP();
		public abstract void WriteJMP(sunPoint point);
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
		internal void WriteSymbol(sunSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			WriteSymbol(symbol.Type, symbol.Name, symbol.Data);
		}
		public abstract void WriteSymbol(sunSymbolType type, string name, uint data);
		public virtual void EndSymbol() {
			// stub
		}
	}

	public struct sunPoint {
		readonly uint mOffset;

		public uint Offset {
			get { return mOffset; }
		}

		public sunPoint(uint offset) {
			mOffset = offset;
		}

		public static implicit operator sunPoint(uint offset) {
			return new sunPoint(offset);
		}
		public static implicit operator uint(sunPoint point) {
			return point.mOffset;
		}
	}

	sealed class sunSpcBinary : sunBinary {
		aBinaryWriter mWriter;
		sunBinarySection mText, mData, mDataString, mSymbol, mSymbolString;
		int mDataCount, mSymbolCount, mVarCount;

		public override uint Offset {
			get { return mText.Offset; }
		}

		public sunSpcBinary(Stream output) {
			mWriter = new aBinaryWriter(output, Endianness.Big, Encoding.GetEncoding(932));
			mText = new sunBinarySection();
			mData = new sunBinarySection();
			mDataString = new sunBinarySection();
			mSymbol = new sunBinarySection();
			mSymbolString = new sunBinarySection();
			mWriter.PushAnchor();
		}

		public override void ClosePoint(sunPoint point, uint offset) {
			Keep();
			Goto(point.Offset);
			mText.Writer.Write32(offset);
			Back();
		}

		public override void Close() {
			// header
			mWriter.WriteString("SPCB");
			mWriter.Write32(0x1C);
			mWriter.Write32(0x1C + mText.Size);
			mWriter.WriteS32(mDataCount);
			mWriter.Write32(0x1C + mText.Size + mData.Size + mDataString.Size);
			mWriter.WriteS32(mSymbolCount);
			mWriter.WriteS32(mVarCount);

			// sections
			mText.Copy(mWriter);
			mData.Copy(mWriter);
			mDataString.Copy(mWriter);
			mSymbol.Copy(mWriter);
			mSymbolString.Copy(mWriter);
		}

		// text
		public override void Keep() {
			mText.Writer.Keep();
		}
		public override void Back() {
			mText.Writer.Back();
		}
		public override void Goto(uint offset) {
			mText.Writer.Goto(offset);
		}

		public override void WriteINT(int value) {
			switch (value) { // shortcut commands
				case 0: WriteINT0(); return;
				case 1: WriteINT1(); return;
			}
#if DEBUG
			TraceInstruction("int {0} # ${0:X}", value);
#endif
			mText.Writer.Write8(0x00);
			mText.Writer.WriteS32(value);
		}
		public override void WriteFLT(float value) {
#if DEBUG
			TraceInstruction("flt {0}", value);
#endif
			mText.Writer.Write8(0x01);
			mText.Writer.WriteF32(value);
		}
		public override void WriteSTR(int index) {
#if DEBUG
			TraceInstruction("str {0}", index);
#endif
			mText.Writer.Write8(0x02);
			mText.Writer.WriteS32(index);
		}
		public override void WriteADR(uint value) {
#if DEBUG
			TraceInstruction("adr ${0:X8}", value);
#endif
			mText.Writer.Write8(0x03);
			mText.Writer.Write32(value);
		}
		public override void WriteVAR(int display, int index) {
#if DEBUG
			TraceInstruction("var {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x04);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public override void WriteNOP() {
#if DEBUG
			TraceInstruction("nop");
#endif
			mText.Writer.Write8(0x05);
		}
		public override void WriteINC(int display, int index) {
#if DEBUG
			TraceInstruction("inc {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x06);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public override void WriteDEC(int display, int index) {
#if DEBUG
			TraceInstruction("dec {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x07);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public override void WriteADD() {
#if DEBUG
			TraceInstruction("add");
#endif
			mText.Writer.Write8(0x08);
		}
		public override void WriteSUB() {
#if DEBUG
			TraceInstruction("sub");
#endif
			mText.Writer.Write8(0x09);
		}
		public override void WriteMUL() {
#if DEBUG
			TraceInstruction("mul");
#endif
			mText.Writer.Write8(0x0A);
		}
		public override void WriteDIV() {
#if DEBUG
			TraceInstruction("div");
#endif
			mText.Writer.Write8(0x0B);
		}
		public override void WriteMOD() {
#if DEBUG
			TraceInstruction("mod");
#endif
			mText.Writer.Write8(0x0C);
		}
		public override void WriteASS(int display, int index) {
#if DEBUG
			TraceInstruction("ass {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x0D);
			mText.Writer.Write8(0x04); // unused (skipped over by TSpcInterp)
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public override void WriteEQ() {
#if DEBUG
			TraceInstruction("eq");
#endif
			mText.Writer.Write8(0x0E);
		}
		public override void WriteNE() {
#if DEBUG
			TraceInstruction("ne");
#endif
			mText.Writer.Write8(0x0F);
		}
		public override void WriteGT() {
#if DEBUG
			TraceInstruction("gt");
#endif
			mText.Writer.Write8(0x10);
		}
		public override void WriteLT() {
#if DEBUG
			TraceInstruction("lt");
#endif
			mText.Writer.Write8(0x11);
		}
		public override void WriteGE() {
#if DEBUG
			TraceInstruction("ge");
#endif
			mText.Writer.Write8(0x12);
		}
		public override void WriteLE() {
#if DEBUG
			TraceInstruction("le");
#endif
			mText.Writer.Write8(0x13);
		}
		public override void WriteNEG() {
#if DEBUG
			TraceInstruction("neg");
#endif
			mText.Writer.Write8(0x14);
		}
		public override void WriteNOT() {
#if DEBUG
			TraceInstruction("not");
#endif
			mText.Writer.Write8(0x15);
		}
		public override void WriteAND() {
#if DEBUG
			TraceInstruction("and");
#endif
			mText.Writer.Write8(0x16);
		}
		public override void WriteOR() {
#if DEBUG
			TraceInstruction("or");
#endif
			mText.Writer.Write8(0x17);
		}
		public override void WriteBAND() {
#if DEBUG
			TraceInstruction("band");
#endif
			mText.Writer.Write8(0x18);
		}
		public override void WriteBOR() {
#if DEBUG
			TraceInstruction("bor");
#endif
			mText.Writer.Write8(0x19);
		}
		public override void WriteSHL() {
#if DEBUG
			TraceInstruction("shl");
#endif
			mText.Writer.Write8(0x1A);
		}
		public override void WriteSHR() {
#if DEBUG
			TraceInstruction("shr");
#endif
			mText.Writer.Write8(0x1B);
		}
		public override void WriteCALL(uint offset, int count) {
#if DEBUG
			TraceInstruction("call ${0:X8} {1}", offset, count);
#endif
			mText.Writer.Write8(0x1C);
			mText.Writer.Write32(offset);
			mText.Writer.WriteS32(count);
		}
		public override void WriteFUNC(int index, int count) {
#if DEBUG
			TraceInstruction("func {0} {1}", index, count);
#endif
			mText.Writer.Write8(0x1D);
			mText.Writer.WriteS32(index);
			mText.Writer.WriteS32(count);
		}
		public override void WriteMKFR(int count) {
#if DEBUG
			TraceInstruction("mkfr {0}", count);
#endif
			mText.Writer.Write8(0x1E);
			mText.Writer.WriteS32(count);
		}
		public override void WriteMKDS(int display) {
#if DEBUG
			TraceInstruction("mkds {0}", display);
#endif
			mText.Writer.Write8(0x1F);
			mText.Writer.WriteS32(display);
		}
		public override void WriteRET() {
#if DEBUG
			TraceInstruction("ret");
#endif
			mText.Writer.Write8(0x20);
		}
		public override void WriteRET0() {
#if DEBUG
			TraceInstruction("ret0");
#endif
			mText.Writer.Write8(0x21);
		}
		public override sunPoint WriteJNE() {
#if DEBUG
			TraceInstruction("jne # UNFINISHED");
#endif
			mText.Writer.Write8(0x22);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public override void WriteJNE(sunPoint point) {
#if DEBUG
			TraceInstruction("jne {0:X8}", point.Offset);
#endif
			mText.Writer.Write8(0x22);
			mText.Writer.Write32(point.Offset);
		}
		public override sunPoint WriteJMP() {
#if DEBUG
			TraceInstruction("jmp # UNFINISHED");
#endif
			mText.Writer.Write8(0x23);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public override void WriteJMP(sunPoint point) {
#if DEBUG
			TraceInstruction("jmp {0:X8}", point.Offset);
#endif
			mText.Writer.Write8(0x23);
			mText.Writer.Write32(point.Offset);
		}
		public override void WritePOP() {
#if DEBUG
			TraceInstruction("pop");
#endif
			mText.Writer.Write8(0x24);
		}
		public override void WriteINT0() {
#if DEBUG
			TraceInstruction("int0");
#endif
			mText.Writer.Write8(0x25);
		}
		public override void WriteINT1() {
#if DEBUG
			TraceInstruction("int1");
#endif
			mText.Writer.Write8(0x26);
		}
		public override void WriteEND() {
#if DEBUG
			TraceInstruction("end");
#endif
			mText.Writer.Write8(0x27);
		}

#if DEBUG
		void TraceInstruction(string format, params object[] args) {
			var instruction = String.Format(format, args);
			Debug.WriteLine("{0:X8} {1}", mText.Size, instruction);
		}
#endif

		// data
		public override void WriteData(string data) {
			if (data == null) {
				throw new ArgumentNullException("data");
			}
			mData.Writer.Write32(mDataString.Size);
			mDataString.Writer.WriteString(data, aBinaryStringFormat.NullTerminated);
			++mDataCount;
		}

		// symbol
		public override void WriteSymbol(sunSymbolType type, string name, uint data) {
			mSymbol.Writer.WriteS32((int)type);
			mSymbol.Writer.Write32(mSymbolString.Size);
			mSymbol.Writer.Write32(data);
			mSymbol.Writer.Write32(0u); // runtime field (hash)
			mSymbol.Writer.Write32(0u); // runtime field (funcptr)
			mSymbolString.Writer.WriteString(name, aBinaryStringFormat.NullTerminated);
			++mSymbolCount;
			if (type == sunSymbolType.Variable) {
				++mVarCount;
			}
		}

		class sunBinarySection : IDisposable {
			readonly aBinaryWriter mWriter;
			readonly MemoryStream mStream;

			public aBinaryWriter Writer {
				get { return mWriter; }
			}
			public MemoryStream Stream {
				get { return mStream; }
			}

			public uint Offset {
				get { return (uint)mWriter.Position; }
			}
			public uint Size {
				get { return (uint)mWriter.Length; }
			}

			public sunBinarySection() {
				mStream = new MemoryStream(1024);
				mWriter = new aBinaryWriter(mStream, Endianness.Big, Encoding.GetEncoding(932));
			}

			public void Dispose() {
				mWriter.Dispose();
				mStream.Dispose();
			}
			public void Copy(aBinaryWriter writer) {
				if (writer == null) {
					throw new ArgumentNullException("writer");
				}
				writer.Write8s(mStream.GetBuffer(), (int)Size);
			}
		}
	}
}
