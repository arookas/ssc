using arookas.IO.Binary;

namespace arookas {
	class Symbol {
		SymbolType mType;
		uint mStringOffset;
		uint mData;
		// NOTE: the other two fields are runtime fields (hash and linker storage)

		public SymbolType Type { get { return mType; } }
		public uint StringOffset { get { return mStringOffset; } }
		public uint Data { get { return mData; } }

		public Symbol(aBinaryReader reader) {
			mType = (SymbolType)reader.Read32();
			mStringOffset = reader.Read32();
			mData = reader.Read32();
			// skip the last two fields
			reader.Read32();
			reader.Read32();
		}
	}

	enum SymbolType {
		Builtin,
		Function,
		Variable,
	}
}
