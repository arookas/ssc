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

		public IEnumerator<string> GetEnumerator() { return data.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
