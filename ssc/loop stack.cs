using System.Collections.Generic;
using System.Linq;

namespace arookas
{
	class sunLoopStack
	{
		Stack<sunLoop> loops = new Stack<sunLoop>(5);
		sunLoop Top { get { return loops.Peek(); } }

		sunLoop this[string name] { get { return loops.FirstOrDefault(i => i.Name == name); } }
		public int Count { get { return loops.Count; } }

		public void Push() { Push(null); }
		public void Push(string name) { loops.Push(new sunLoop(name)); }
		public void Pop(sunContext context, sunPoint breakPoint, sunPoint continuePoint)
		{
			foreach (var _break in Top.Breaks)
			{
				context.Text.ClosePoint(_break, breakPoint.Offset);
			}
			foreach (var _continue in Top.Continues)
			{
				context.Text.ClosePoint(_continue, continuePoint.Offset);
			}
			loops.Pop();
		}

		public bool AddBreak(sunPoint point) { return AddBreak(point, null); }
		public bool AddContinue(sunPoint point) { return AddContinue(point, null); }
		public bool AddBreak(sunPoint point, string name)
		{
			if (Count < 1)
			{
				return false;
			}
			var loop = name == null ? Top : this[name];
			if (loop == null)
			{
				return false;
			}
			loop.Breaks.Add(point);
			return true;
		}
		public bool AddContinue(sunPoint point, string name)
		{
			if (Count < 1)
			{
				return false;
			}
			var loop = name == null ? Top : this[name];
			if (loop == null)
			{
				return false;
			}
			loop.Continues.Add(point);
			return true;
		}

		class sunLoop
		{
			public string Name { get; private set; }
			public List<sunPoint> Breaks { get; private set; }
			public List<sunPoint> Continues { get; private set; }

			public sunLoop()
				: this(null)
			{

			}
			public sunLoop(string name)
			{
				Name = name;
				Breaks = new List<sunPoint>(5);
				Continues = new List<sunPoint>(5);
			}
		}
	}
}
