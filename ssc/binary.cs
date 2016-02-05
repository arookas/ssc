using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using arookas.IO.Binary;

namespace arookas {
	class sunBinary : IDisposable {
		aBinaryWriter mWriter;
		sunBinarySection mText, mData, mDataString, mSymbol, mSymbolString;
		int mDataCount, mSymbolCount, mVarCount;

		public uint Offset {
			get { return mText.Offset; }
		}

		public sunBinary(Stream output) {
			mWriter = new aBinaryWriter(output, Endianness.Big, Encoding.GetEncoding(932));
			mText = new sunBinarySection();
			mData = new sunBinarySection();
			mDataString = new sunBinarySection();
			mSymbol = new sunBinarySection();
			mSymbolString = new sunBinarySection();
			mWriter.PushAnchor();
		}

		// close
		public void Dispose() {
			Close();
		}
		public void Close() {
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
		public void Keep() {
			mText.Writer.Keep();
		}
		public void Back() {
			mText.Writer.Back();
		}
		public void Goto(uint offset) {
			mText.Writer.Goto(offset);
		}

		public sunPoint OpenPoint() {
			return new sunPoint(Offset);
		}
		public void ClosePoint(sunPoint point) {
			ClosePoint(point, Offset);
		}
		public void ClosePoint(sunPoint point, uint offset) {
			Keep();
			Goto(point.Offset);
			mText.Writer.Write32(offset);
			Back();
		}

		public void WriteINT(int value) {
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
		public void WriteFLT(float value) {
#if DEBUG
			TraceInstruction("flt {0}", value);
#endif
			mText.Writer.Write8(0x01);
			mText.Writer.WriteF32(value);
		}
		public void WriteSTR(int index) {
#if DEBUG
			TraceInstruction("str {0}", index);
#endif
			mText.Writer.Write8(0x02);
			mText.Writer.WriteS32(index);
		}
		public void WriteADR(uint value) {
#if DEBUG
			TraceInstruction("adr ${0:X8}", value);
#endif
			mText.Writer.Write8(0x03);
			mText.Writer.Write32(value);
		}
		public void WriteVAR(int display, int index) {
#if DEBUG
			TraceInstruction("var {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x04);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteNOP() {
#if DEBUG
			TraceInstruction("nop");
#endif
			mText.Writer.Write8(0x05);
		}
		public void WriteINC(int display, int index) {
#if DEBUG
			TraceInstruction("inc {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x06);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteDEC(int display, int index) {
#if DEBUG
			TraceInstruction("dec {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x07);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteADD() {
#if DEBUG
			TraceInstruction("add");
#endif
			mText.Writer.Write8(0x08);
		}
		public void WriteSUB() {
#if DEBUG
			TraceInstruction("sub");
#endif
			mText.Writer.Write8(0x09);
		}
		public void WriteMUL() {
#if DEBUG
			TraceInstruction("mul");
#endif
			mText.Writer.Write8(0x0A);
		}
		public void WriteDIV() {
#if DEBUG
			TraceInstruction("div");
#endif
			mText.Writer.Write8(0x0B);
		}
		public void WriteMOD() {
#if DEBUG
			TraceInstruction("mod");
#endif
			mText.Writer.Write8(0x0C);
		}
		public void WriteASS(int display, int index) {
#if DEBUG
			TraceInstruction("ass {0} {1}", display, index);
#endif
			mText.Writer.Write8(0x0D);
			mText.Writer.Write8(0x04); // unused (skipped over by TSpcInterp)
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteEQ() {
#if DEBUG
			TraceInstruction("eq");
#endif
			mText.Writer.Write8(0x0E);
		}
		public void WriteNE() {
#if DEBUG
			TraceInstruction("ne");
#endif
			mText.Writer.Write8(0x0F);
		}
		public void WriteGT() {
#if DEBUG
			TraceInstruction("gt");
#endif
			mText.Writer.Write8(0x10);
		}
		public void WriteLT() {
#if DEBUG
			TraceInstruction("lt");
#endif
			mText.Writer.Write8(0x11);
		}
		public void WriteGE() {
#if DEBUG
			TraceInstruction("ge");
#endif
			mText.Writer.Write8(0x12);
		}
		public void WriteLE() {
#if DEBUG
			TraceInstruction("le");
#endif
			mText.Writer.Write8(0x13);
		}
		public void WriteNEG() {
#if DEBUG
			TraceInstruction("neg");
#endif
			mText.Writer.Write8(0x14);
		}
		public void WriteNOT() {
#if DEBUG
			TraceInstruction("not");
#endif
			mText.Writer.Write8(0x15);
		}
		public void WriteAND() {
#if DEBUG
			TraceInstruction("and");
#endif
			mText.Writer.Write8(0x16);
		}
		public void WriteOR() {
#if DEBUG
			TraceInstruction("or");
#endif
			mText.Writer.Write8(0x17);
		}
		public void WriteBAND() {
#if DEBUG
			TraceInstruction("band");
#endif
			mText.Writer.Write8(0x18);
		}
		public void WriteBOR() {
#if DEBUG
			TraceInstruction("bor");
#endif
			mText.Writer.Write8(0x19);
		}
		public void WriteSHL() {
#if DEBUG
			TraceInstruction("shl");
#endif
			mText.Writer.Write8(0x1A);
		}
		public void WriteSHR() {
#if DEBUG
			TraceInstruction("shr");
#endif
			mText.Writer.Write8(0x1B);
		}
		public void WriteCALL(uint offset, int count) {
#if DEBUG
			TraceInstruction("call ${0:X8} {1}", offset, count);
#endif
			mText.Writer.Write8(0x1C);
			mText.Writer.Write32(offset);
			mText.Writer.WriteS32(count);
		}
		public void WriteFUNC(int index, int count) {
#if DEBUG
			TraceInstruction("func {0} {1}", index, count);
#endif
			mText.Writer.Write8(0x1D);
			mText.Writer.WriteS32(index);
			mText.Writer.WriteS32(count);
		}
		public void WriteMKFR(int count) {
#if DEBUG
			TraceInstruction("mkfr {0}", count);
#endif
			mText.Writer.Write8(0x1E);
			mText.Writer.WriteS32(count);
		}
		public void WriteMKDS(int display) {
#if DEBUG
			TraceInstruction("mkds {0}", display);
#endif
			mText.Writer.Write8(0x1F);
			mText.Writer.WriteS32(display);
		}
		public void WriteRET() {
#if DEBUG
			TraceInstruction("ret");
#endif
			mText.Writer.Write8(0x20);
		}
		public void WriteRET0() {
#if DEBUG
			TraceInstruction("ret0");
#endif
			mText.Writer.Write8(0x21);
		}
		public sunPoint WriteJNE() {
#if DEBUG
			TraceInstruction("jne # UNFINISHED");
#endif
			mText.Writer.Write8(0x22);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public void WriteJNE(sunPoint point) {
#if DEBUG
			TraceInstruction("jne {0:X8}", point.Offset);
#endif
			mText.Writer.Write8(0x22);
			mText.Writer.Write32(point.Offset);
		}
		public sunPoint WriteJMP() {
#if DEBUG
			TraceInstruction("jmp # UNFINISHED");
#endif
			mText.Writer.Write8(0x23);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public void WriteJMP(sunPoint point) {
#if DEBUG
			TraceInstruction("jmp {0:X8}", point.Offset);
#endif
			mText.Writer.Write8(0x23);
			mText.Writer.Write32(point.Offset);
		}
		public void WritePOP() {
#if DEBUG
			TraceInstruction("pop");
#endif
			mText.Writer.Write8(0x24);
		}
		public void WriteINT0() {
#if DEBUG
			TraceInstruction("int0");
#endif
			mText.Writer.Write8(0x25);
		}
		public void WriteINT1() {
#if DEBUG
			TraceInstruction("int1");
#endif
			mText.Writer.Write8(0x26);
		}
		public void WriteEND() {
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
		public void WriteData(string data) {
			if (data == null) {
				throw new ArgumentNullException("data");
			}
			mData.Writer.Write32(mDataString.Size);
			mDataString.Writer.WriteString(data, aBinaryStringFormat.NullTerminated);
			++mDataCount;
		}

		// symbol
		public void WriteSymbol(sunSymbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			WriteSymbol(symbol.Type, symbol.Name, symbol.Data);
		}
		public void WriteSymbol(sunSymbolType type, string name, uint data) {
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

	struct sunPoint {
		readonly uint mOffset;

		public uint Offset {
			get { return mOffset; }
		}

		public sunPoint(uint offset) {
			mOffset = offset;
		}

		public static implicit operator uint(sunPoint point) {
			return point.mOffset;
		}
	}
}
