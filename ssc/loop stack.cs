using System;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunLoopStack {
		Stack<sunLoop> mLoops;

		sunLoop Top { get { return mLoops.Peek(); } }
		sunLoop this[string name] { get { return mLoops.FirstOrDefault(i => i.Name == name); } }
		public int Count { get { return mLoops.Count; } }

		public sunLoopStack() {
			mLoops = new Stack<sunLoop>(5);
		}

		public sunLoop Push() {
			return Push(new sunLoop());
		}
		public sunLoop Push(sunLoopFlags flags) {
			return Push(new sunLoop(flags));
		}
		public sunLoop Push(string name) {
			return Push(new sunLoop(name));
		}
		public sunLoop Push(string name, sunLoopFlags flags) {
			return Push(new sunLoop(name, flags));
		}
		sunLoop Push(sunLoop loop) {
			if (loop == null) {
				throw new ArgumentNullException("loop");
			}
			mLoops.Push(loop);
			return loop;
		}
		public void Pop(sunCompiler compiler) {
			if (Count < 1) {
				return;
			}
			mLoops.Pop().Close(compiler);
		}

		public void Clear() {
			mLoops.Clear();
		}

		public bool AddBreak(sunPoint point) { return AddBreak(point, null); }
		public bool AddContinue(sunPoint point) { return AddContinue(point, null); }
		public bool AddBreak(sunPoint point, string name) {
			if (Count < 1) {
				return false;
			}
			var loop = name == null ? Top : this[name];
			if (loop == null) {
				return false;
			}
			loop.AddBreak(point);
			return true;
		}
		public bool AddContinue(sunPoint point, string name) {
			if (Count < 1) {
				return false;
			}
			var loop = name == null ? Top : this[name];
			if (loop == null) {
				return false;
			}
			loop.AddContinue(point);
			return true;
		}

	}

	class sunLoop {
		string mName;
		List<sunPoint> mBreaks, mContinues;
		sunLoopFlags mFlags;
		sunPoint mBreakPoint, mContinuePoint;

		public string Name { get { return mName; } }
		public bool HasName { get { return Name != null; } }
		public sunPoint BreakPoint { get { return mBreakPoint; } set { mBreakPoint = value; } }
		public sunPoint ContinuePoint { get { return mContinuePoint; } set { mContinuePoint = value; } }

		public sunLoop() {
			mName = null;
			mBreaks = new List<sunPoint>(5);
			mContinues = new List<sunPoint>(5);
			mFlags = sunLoopFlags.ConsumeBreak | sunLoopFlags.ConsumeContinue;
		}
		public sunLoop(sunLoopFlags flags) {
			mFlags = flags;
		}
		public sunLoop(string name) {
			mName = name;
		}
		public sunLoop(string name, sunLoopFlags flags) {
			mName = name;
			mFlags = flags;
		}

		bool HasFlag(sunLoopFlags flags) {
			return (mFlags & flags) != 0;
		}

		public bool AddBreak(sunPoint point) {
			if (!HasFlag(sunLoopFlags.ConsumeBreak)) {
				return false;
			}
			mBreaks.Add(point);
			return true;
		}
		public bool AddContinue(sunPoint point) {
			if (!HasFlag(sunLoopFlags.ConsumeContinue)) {
				return false;
			}
			mContinues.Add(point);
			return true;
		}
		public void Close(sunCompiler compiler) {
			if (HasFlag(sunLoopFlags.ConsumeBreak)) {
				foreach (var b in mBreaks) {
					compiler.Binary.ClosePoint(b, mBreakPoint);
				}
			}
			if (HasFlag(sunLoopFlags.ConsumeContinue)) {
				foreach (var c in mContinues) {
					compiler.Binary.ClosePoint(c, mContinuePoint);
				}
			}
		}
	}

	[Flags]
	enum sunLoopFlags {
		None = 0,
		ConsumeBreak = 1,
		ConsumeContinue = 2,
	}
}
