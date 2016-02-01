using System;
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
			mText.Writer.Write8(0x00);
			mText.Writer.WriteS32(value);
		}
		public void WriteFLT(float value) {
			mText.Writer.Write8(0x01);
			mText.Writer.WriteF32(value);
		}
		public void WriteSTR(int index) {
			mText.Writer.Write8(0x02);
			mText.Writer.WriteS32(index);
		}
		public void WriteADR(int value) {
			mText.Writer.Write8(0x03);
			mText.Writer.WriteS32(value);
		}
		public void WriteVAR(int display, int index) {
			mText.Writer.Write8(0x04);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteNOP() {
			mText.Writer.Write8(0x05);
		}
		public void WriteINC(int display, int index) {
			mText.Writer.Write8(0x06);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteDEC(int display, int index) {
			mText.Writer.Write8(0x07);
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteADD() {
			mText.Writer.Write8(0x08);
		}
		public void WriteSUB() {
			mText.Writer.Write8(0x09);
		}
		public void WriteMUL() {
			mText.Writer.Write8(0x0A);
		}
		public void WriteDIV() {
			mText.Writer.Write8(0x0B);
		}
		public void WriteMOD() {
			mText.Writer.Write8(0x0C);
		}
		public void WriteASS(int display, int index) {
			mText.Writer.Write8(0x0D);
			mText.Writer.Write8(0x04); // unused (skipped over by TSpcInterp)
			mText.Writer.WriteS32(display);
			mText.Writer.WriteS32(index);
		}
		public void WriteEQ() {
			mText.Writer.Write8(0x0E);
		}
		public void WriteNE() {
			mText.Writer.Write8(0x0F);
		}
		public void WriteGT() {
			mText.Writer.Write8(0x10);
		}
		public void WriteLT() {
			mText.Writer.Write8(0x11);
		}
		public void WriteGE() {
			mText.Writer.Write8(0x12);
		}
		public void WriteLE() {
			mText.Writer.Write8(0x13);
		}
		public void WriteNEG() {
			mText.Writer.Write8(0x14);
		}
		public void WriteNOT() {
			mText.Writer.Write8(0x15);
		}
		public void WriteAND() {
			mText.Writer.Write8(0x16);
		}
		public void WriteOR() {
			mText.Writer.Write8(0x17);
		}
		public void WriteBAND() {
			mText.Writer.Write8(0x18);
		}
		public void WriteBOR() {
			mText.Writer.Write8(0x19);
		}
		public void WriteSHL() {
			mText.Writer.Write8(0x1A);
		}
		public void WriteSHR() {
			mText.Writer.Write8(0x1B);
		}
		public sunPoint WriteCALL(int count) {
			mText.Writer.Write8(0x1C);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			mText.Writer.WriteS32(count);
			return point;
		}
		public void WriteCALL(sunPoint point, int count) {
			mText.Writer.Write8(0x1C);
			mText.Writer.Write32(point.Offset);
			mText.Writer.WriteS32(count);
		}
		public void WriteFUNC(int index, int count) {
			mText.Writer.Write8(0x1D);
			mText.Writer.WriteS32(index);
			mText.Writer.WriteS32(count);
		}
		public void WriteMKFR(int count) {
			mText.Writer.Write8(0x1E);
			mText.Writer.WriteS32(count);
		}
		public void WriteMKDS(int display) {
			mText.Writer.Write8(0x1F);
			mText.Writer.WriteS32(display);
		}
		public void WriteRET() {
			mText.Writer.Write8(0x20);
		}
		public void WriteRET0() {
			mText.Writer.Write8(0x21);
		}
		public sunPoint WriteJNE() {
			mText.Writer.Write8(0x22);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public void WriteJNE(sunPoint point) {
			mText.Writer.Write8(0x22);
			mText.Writer.Write32(point.Offset);
		}
		public sunPoint WriteJMP() {
			mText.Writer.Write8(0x23);
			sunPoint point = OpenPoint();
			mText.Writer.Write32(0); // dummy
			return point;
		}
		public void WriteJMP(sunPoint point) {
			mText.Writer.Write8(0x23);
			mText.Writer.Write32(point.Offset);
		}
		public void WritePOP() {
			mText.Writer.Write8(0x24);
		}
		public void WriteINT0() {
			mText.Writer.Write8(0x25);
		}
		public void WriteINT1() {
			mText.Writer.Write8(0x26);
		}
		public void WriteEND() {
			mText.Writer.Write8(0x27);
		}

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
