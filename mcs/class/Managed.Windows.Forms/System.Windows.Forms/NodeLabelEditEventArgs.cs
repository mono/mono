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

namespace System.Windows.Forms {

	public class NodeLabelEditEventArgs : EventArgs {

		private TreeNode node;
		private string label;
		private bool cancel;

		public NodeLabelEditEventArgs (TreeNode node)
		{
			this.node = node;
		}

		public NodeLabelEditEventArgs (TreeNode node, string label) : this (node)
		{
			this.label = label;
		}

		public bool CancelEdit {
			get { return cancel; }
			set {
				cancel = value;
				
				if (cancel)
					node.EndEdit (true);
			}
		}

		public TreeNode Node {
			get { return node; }
		}

		public string Label {
			get { return label; }
		}

		internal void SetLabel (string label)
		{
			this.label = label;
		}
	}

}

