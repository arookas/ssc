using System;
using System.Collections;
using System.Collections.Generic;

namespace arookas
{
	public class sunSourceLocation
	{
		public string File { get; private set; }
		public int Line { get; private set; }
		public int Column { get; private set; }

		public sunSourceLocation(string file, int line, int column)
		{
			File = file;
			Line = line;
			Column = column;
		}

		public override string ToString()
		{
			return String.Format("\"{0}\", ({1},{2})", File, Line, Column);
		}
	}

	class sunNode : IEnumerable<sunNode>
	{
		List<sunNode> mChildren;

		public sunNode Parent { get; private set; }
		public sunSourceLocation Location { get; private set; }

		public int Count { get { return mChildren.Count; } }
		public sunNode this[int index] { get { return index >= 0 && index < Count ? mChildren[index] : null; } }

		public bool IsRoot { get { return Parent == null; } }
		public bool IsBranch { get { return Count > 0; } }
		public bool IsLeaf { get { return Count < 1; } }

		public int MaxLocalCount
		{
			get
			{
				int locals = 0;
				int maxChildLocals = 0;
				foreach (var child in this)
				{
					if (child is sunVariableDeclaration || child is sunVariableDefinition)
					{
						++locals;
					}
					else if (child is sunCompoundStatement)
					{
						locals += child.MaxLocalCount; // HACK: compound statements aren't their own scope
					}
					else if (!(child is sunFunctionDefinition)) // don't recurse into function bodies
					{
						int childLocals = child.MaxLocalCount;
						if (childLocals > maxChildLocals)
						{
							maxChildLocals = childLocals;
						}
					}
				}
				return locals + maxChildLocals;
			}
		}

		public sunNode(sunSourceLocation location)
		{
			mChildren = new List<sunNode>(5);
			Location = location;
		}

		public void Add(sunNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (node.Parent != null)
			{
				node.Parent.Remove(node);
			}
			node.Parent = this;
			mChildren.Add(node);
		}
		public void Remove(sunNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (node.Parent == this)
			{
				mChildren.Remove(node);
				node.Parent = null;
			}
		}
		public void Clear()
		{
			foreach (var child in this)
			{
				child.Parent = null;
			}
			mChildren.Clear();
		}

		public virtual void Compile(sunContext context)
		{
			// Simply compile all children nodes by default. This is here for the transcient nodes' implementations
			// (sunStatement, sunCompoundStatement, etc.) so I only have to type this once. sunExpression is careful
			// to override this with the custom shunting-yard algorithm implementation.
			foreach (var child in this)
			{
				child.Compile(context);
			}
		}
		protected bool TryCompile(sunNode node, sunContext context)
		{
			if (node != null)
			{
				node.Compile(context);
				return true;
			}
			return false;
		}

		public IEnumerator<sunNode> GetEnumerator() { return mChildren.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunToken<TValue> : sunNode
	{
		public TValue Value { get; protected set; }

		protected sunToken(sunSourceLocation location)
			: base(location)
		{

		}
	}
}
