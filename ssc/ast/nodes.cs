using System;
using System.Collections;
using System.Collections.Generic;

namespace arookas {
	public class sunSourceLocation {
		string mScriptName;
		ulong mScriptId;
		int mLine, mColumn;

		public string ScriptName {
			get { return mScriptName; }
		}
		public ulong ScriptId {
			get { return mScriptId; }
		}
		public int Line {
			get { return mLine; }
		}
		public int Column {
			get { return mColumn; }
		}

		public sunSourceLocation(string file, ulong id, int line, int column) {
			if (file == null) {
				throw new ArgumentNullException("file");
			}
			mScriptName = file;
			mScriptId = id;
			mLine = line;
			mColumn = column;
		}

		public override string ToString() {
			return String.Format("\"{0}\", ({1},{2})", mScriptName, mLine, mColumn);
		}
	}

	class sunNode : IEnumerable<sunNode> {
		List<sunNode> mChildren;

		public sunNode Parent { get; private set; }
		public sunSourceLocation Location { get; private set; }

		public int Count { get { return mChildren.Count; } }
		public sunNode this[int index] { get { return index >= 0 && index < Count ? mChildren[index] : null; } }

		public bool IsRoot { get { return Parent == null; } }
		public bool IsBranch { get { return Count > 0; } }
		public bool IsLeaf { get { return Count < 1; } }

		public int LocalCount {
			get {
#if SSC_SCOPES
				var locals = 0;
				var maxChildLocals = 0;
				foreach (var child in this) {
					if (child is sunVariableDeclaration || child is sunVariableDefinition) {
						++locals;
					}
					else if (child is sunCompoundStatement) {
						locals += child.LocalCount; // HACK: compound statements aren't their own scope
					}
					else if (!(child is sunFunctionDefinition)) { // don't recurse into function bodies
						var childLocals = child.LocalCount;
						if (childLocals > maxChildLocals) {
							maxChildLocals = childLocals;
						}
					}
				}
				return locals + maxChildLocals;
#else
				var locals = 0;
				foreach (var child in this) {
					if (child is sunVariableDeclaration || child is sunVariableDefinition) {
						++locals;
					}
					else if (!(child is sunFunctionDefinition)) { // don't recurse into function bodies
						locals += child.LocalCount;
					}
				}
				return locals;
#endif
			}
		}

		public sunNode(sunSourceLocation location) {
			mChildren = new List<sunNode>(5);
			Location = location;
		}

		public void Add(sunNode node) {
			if (node == null) {
				throw new ArgumentNullException("node");
			}
			if (node.Parent != null) {
				node.Parent.Remove(node);
			}
			node.Parent = this;
			mChildren.Add(node);
		}
		public void Remove(sunNode node) {
			if (node == null) {
				throw new ArgumentNullException("node");
			}
			if (node.Parent == this) {
				mChildren.Remove(node);
				node.Parent = null;
			}
		}
		public void Clear() {
			foreach (var child in this) {
				child.Parent = null;
			}
			mChildren.Clear();
		}

		public virtual void Compile(sunCompiler compiler) {
			foreach (var child in this) {
				child.Compile(compiler);
			}
		}
		protected bool TryCompile(sunNode node, sunCompiler compiler) {
			if (node == null) {
				return false;
			}
			node.Compile(compiler);
			return true;
		}

		public IEnumerator<sunNode> GetEnumerator() { return mChildren.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	abstract class sunToken<TValue> : sunNode {
		public TValue Value { get; protected set; }

		protected sunToken(sunSourceLocation location)
			: base(location) { }
	}
}
