// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Collections;

namespace System.Windows.Forms {

	internal class OpenTreeNodeEnumerator : IEnumerator {

		private TreeNode start;
		private TreeNode current;
		private bool started;

		public OpenTreeNodeEnumerator (TreeNode start)
		{
			this.start = start;
		}

		public object Current {
			get { return current; }
		}

		public TreeNode CurrentNode {
			get { return current; }
		}

		public bool MoveNext ()
		{
			if (!started) {
				started = true;
				current = start;
				return (current != null);
			}

			if (current.is_expanded && current.Nodes.Count > 0) {
				current = current.Nodes [0];
				return true;
			}

			TreeNode prev = current;
			TreeNode next = current.NextNode;
			while (next == null) {
				// The next node is null so we need to move back up the tree until we hit the top
				if (prev.parent == null)
					return false;
				prev = prev.parent;
				if (prev.parent != null)
					next = prev.NextNode;
			}
			current = next;
			return true;
		}
		
		public bool MovePrevious ()
		{
			if (!started) {
				started = true;
				current = start;
				return (current != null);
			}

			if (current.PrevNode != null) {
				// Drill down as far as possible
				TreeNode prev = current.PrevNode;
				TreeNode walk = prev;
				while (walk != null) {
					prev = walk;
					if (!walk.is_expanded)
						break;
					walk = walk.LastNode;
				}
				current = prev;
				return true;
			}

			if (current.Parent == null)
				return false;

			current = current.Parent;
			return true;
		}

		public void Reset ()
		{
			started = false;
		}
	}
}

