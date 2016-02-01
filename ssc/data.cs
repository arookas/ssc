using System.Collections;
using System.Collections.Generic;

namespace arookas {
	class sunDataTable : IEnumerable<string> {
		List<string> mData;

		public int Count {
			get { return mData.Count; }
		}

		public sunDataTable() {
			mData = new List<string>(10);
		}

		public int Add(string value) {
			var index = mData.IndexOf(value);
			if (index < 0) {
				index = mData.Count;
				mData.Add(value);
			}
			return index;
		}
		public void Clear() {
			mData.Clear();
		}

		public IEnumerator<string> GetEnumerator() {
			return mData.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
