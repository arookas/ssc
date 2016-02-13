using System;
using System.Collections.Generic;
using System.Linq;

namespace arookas {
	class sunLoopStack {
		Stack<sunLoop> mLoops;

		sunLoop Top {
			get { return mLoops.Peek(); }
		}
		public int Count {
			get { return mLoops.Count; }
		}
		
		sunLoop this[string name] {
			get { return mLoops.FirstOrDefault(i => i.Name == name); }
		}

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

		public bool AddBreak(sunRelocation point) {
			if (Count > 0) {
				Top.AddBreak(point);
				return true;
			}
			return false;
		}
		public bool AddContinue(sunRelocation point) {
			if (Count > 0) {
				Top.AddContinue(point);
				return true;
			}
			return false;
		}
		public bool AddBreak(sunRelocation point, string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (Count > 0) {
				var loop = this[name];
				if (loop != null) {
					loop.AddBreak(point);
					return true;
				}
			}
			return false;
		}
		public bool AddContinue(sunRelocation point, string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (Count > 0) {
				var loop = this[name];
				if (loop != null) {
					loop.AddContinue(point);
					return true;
				}
			}
			return false;
		}

	}

	class sunLoop {
		string mName;
		List<sunRelocation> mBreaks, mContinues;
		sunLoopFlags mFlags;
		uint mBreakPoint, mContinuePoint;

		public string Name {
			get { return mName; }
		}
		public bool HasName {
			get { return Name != null; }
		}
		public uint BreakPoint {
			get { return mBreakPoint; }
			set { mBreakPoint = value; }
		}
		public uint ContinuePoint {
			get { return mContinuePoint; }
			set { mContinuePoint = value; }
		}

		public sunLoop() {
			mName = null;
			mBreaks = new List<sunRelocation>(5);
			mContinues = new List<sunRelocation>(5);
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

		public bool AddBreak(sunRelocation point) {
			if (!HasFlag(sunLoopFlags.ConsumeBreak)) {
				return false;
			}
			mBreaks.Add(point);
			return true;
		}
		public bool AddContinue(sunRelocation point) {
			if (!HasFlag(sunLoopFlags.ConsumeContinue)) {
				return false;
			}
			mContinues.Add(point);
			return true;
		}
		public void Close(sunCompiler compiler) {
			if (HasFlag(sunLoopFlags.ConsumeBreak)) {
				foreach (var b in mBreaks) {
					b.Relocate();
				}
			}
			if (HasFlag(sunLoopFlags.ConsumeContinue)) {
				foreach (var c in mContinues) {
					c.Relocate();
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
