using arookas.IO.Binary;

namespace arookas {
	class sunWriter {
		aBinaryWriter mWriter;

		public uint Offset { get { return (uint)mWriter.Position; } }

		public sunWriter(aBinaryWriter writer) {
			this.mWriter = writer;
		}

		public sunPoint OpenPoint() { return new sunPoint(Offset); }
		public void ClosePoint(sunPoint point) { ClosePoint(point, (uint)mWriter.Position); }
		public void ClosePoint(sunPoint point, uint offset) {
			mWriter.Keep();
			mWriter.Goto(point.Offset);
			mWriter.Write32(offset);
			mWriter.Back();
		}

		public void WriteINT(int value) {
			switch (value) { // shortcut commands
				case 0: WriteINT0(); return;
				case 1: WriteINT1(); return;
			}
			mWriter.Write8(0x00);
			mWriter.WriteS32(value);
		}
		public void WriteFLT(float value) {
			mWriter.Write8(0x01);
			mWriter.WriteF32(value);
		}
		public void WriteSTR(int index) {
			mWriter.Write8(0x02);
			mWriter.WriteS32(index);
		}
		public void WriteADR(int value) {
			mWriter.Write8(0x03);
			mWriter.WriteS32(value);
		}
		public void WriteVAR(int display, int index) {
			mWriter.Write8(0x04);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public void WriteNOP() { mWriter.Write8(0x05); }
		public void WriteINC(int display, int index) {
			mWriter.Write8(0x06);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public void WriteDEC(int display, int index) {
			mWriter.Write8(0x07);
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public void WriteADD() { mWriter.Write8(0x08); }
		public void WriteSUB() { mWriter.Write8(0x09); }
		public void WriteMUL() { mWriter.Write8(0x0A); }
		public void WriteDIV() { mWriter.Write8(0x0B); }
		public void WriteMOD() { mWriter.Write8(0x0C); }
		public void WriteASS(int display, int index) {
			mWriter.Write8(0x0D);
			mWriter.Write8(0x04); // unused (skipped over by TSpcInterp)
			mWriter.WriteS32(display);
			mWriter.WriteS32(index);
		}
		public void WriteEQ() { mWriter.Write8(0x0E); }
		public void WriteNE() { mWriter.Write8(0x0F); }
		public void WriteGT() { mWriter.Write8(0x10); }
		public void WriteLT() { mWriter.Write8(0x11); }
		public void WriteGE() { mWriter.Write8(0x12); }
		public void WriteLE() { mWriter.Write8(0x13); }
		public void WriteNEG() { mWriter.Write8(0x14); }
		public void WriteNOT() { mWriter.Write8(0x15); }
		public void WriteAND() { mWriter.Write8(0x16); }
		public void WriteOR() { mWriter.Write8(0x17); }
		public void WriteBAND() { mWriter.Write8(0x18); }
		public void WriteBOR() { mWriter.Write8(0x19); }
		public void WriteSHL() { mWriter.Write8(0x1A); }
		public void WriteSHR() { mWriter.Write8(0x1B); }
		public sunPoint WriteCALL(int count) {
			mWriter.Write8(0x1C);
			sunPoint point = OpenPoint();
			mWriter.Write32(0); // dummy
			mWriter.WriteS32(count);
			return point;
		}
		public void WriteCALL(sunPoint point, int count) {
			mWriter.Write8(0x1C);
			mWriter.Write32(point.Offset);
			mWriter.WriteS32(count);
		}
		public void WriteFUNC(int index, int count) {
			mWriter.Write8(0x1D);
			mWriter.WriteS32(index);
			mWriter.WriteS32(count);
		}
		public void WriteMKFR(int count) {
			mWriter.Write8(0x1E);
			mWriter.WriteS32(count);
		}
		public void WriteMKDS(int display) {
			mWriter.Write8(0x1F);
			mWriter.WriteS32(display);
		}
		public void WriteRET() { mWriter.Write8(0x20); }
		public void WriteRET0() { mWriter.Write8(0x21); }
		public sunPoint WriteJNE() {
			mWriter.Write8(0x22);
			sunPoint point = OpenPoint();
			mWriter.Write32(0); // dummy
			return point;
		}
		public sunPoint WriteJMP() {
			mWriter.Write8(0x23);
			sunPoint point = OpenPoint();
			mWriter.Write32(0); // dummy
			return point;
		}
		public void WriteJNE(sunPoint point) {
			mWriter.Write8(0x22);
			mWriter.Write32(point.Offset);
		}
		public void WriteJMP(sunPoint point) {
			mWriter.Write8(0x23);
			mWriter.Write32(point.Offset);
		}
		public void WritePOP() { mWriter.Write8(0x24); }
		public void WriteINT0() { mWriter.Write8(0x25); }
		public void WriteINT1() { mWriter.Write8(0x26); }
		public void WriteEND() { mWriter.Write8(0x27); }
	}

	struct sunPoint {
		readonly uint mOffset;
		public uint Offset { get { return mOffset; } }

		public sunPoint(uint offset) {
			mOffset = offset;
		}
	}
}
