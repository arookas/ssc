using arookas.IO.Binary;
using System.Collections;
using System.Collections.Generic;

namespace arookas {
	class sunDataTable : IEnumerable<string> {
		List<string> data = new List<string>(10);

		public int Count { get { return data.Count; } }

		public int Add(string value) {
			int index = data.IndexOf(value);
			if (index < 0) {
				index = data.Count;
				data.Add(value);
			}
			return index;
		}
		public void Clear() { data.Clear(); }

		public void Write(aBinaryWriter writer) {
			int ofs = 0;
			foreach (var value in this) {
				writer.WriteS32(ofs);
				var length = writer.Encoding.GetByteCount(value);
				ofs += length + 1; // include terminator
			}
			foreach (var value in this) {
				writer.WriteString(value, aBinaryStringFormat.NullTerminated);
			}
		}

		public IEnumerator<string> GetEnumerator() { return data.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
