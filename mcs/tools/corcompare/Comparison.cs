//
// Comparison.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
//
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

using System;
using System.Collections.Generic;

namespace GuiCompare {

	public enum ComparisonStatus {
		None,
		Missing,
		Extra,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (CompType type,
		                       string name)
		{
			this.type = type;
			this.name = name;
			this.children = new List<ComparisonNode>();
			this.messages = new List<string>();
			this.todos = new List<string>();
		}

		public void AddChild (ComparisonNode node)
		{
			children.Add (node);
			node.parent = this;
		}

		public void PropagateCounts ()
		{
			Todo = todos.Count;
			Niex = throws_niex ? 1 : 0;
			foreach (ComparisonNode n in children) {
				n.PropagateCounts ();
				Extra += n.Extra + (n.status == ComparisonStatus.Extra ? 1 : 0);
				Missing += n.Missing + (n.status == ComparisonStatus.Missing ? 1 : 0);
				Present += n.Present; // XXX
				Todo += n.Todo;
				Niex += n.Niex;
				Warning += n.Warning + (n.status == ComparisonStatus.Error ? 1 : 0);
			}
		}

		public void AddError (string msg)
		{
			status = ComparisonStatus.Error;
			messages.Add (msg);
		}
		
		public ComparisonStatus status;
		public CompType type;

		public ComparisonNode parent;

		public string name;
		public List<string> messages;
		public List<string> todos;
		public bool throws_niex;
		
		public int Extra;
		public int Missing;
		public int Present;
		public int Warning;
		public int Todo;
		public int Niex;

		public List<ComparisonNode> children;
	}
}
