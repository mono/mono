//
// System.Windows.Forms.TreeNode.cs
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
// Autors:
//		Marek Safar		marek.safar@seznam.cz
//
//
//
//

// NOT COMPLETE

using System;
using System.Runtime.Serialization;

namespace System.Windows.Forms
{
	[Serializable]
	public class TreeNode: MarshalByRefObject, ICloneable, ISerializable
	{
		[Flags]
		enum Flags {
			HasCheck	= 0x01,
			IsSelected	= 0x02,
			IsExpanded	= 0x04
		}

		Flags flags;
		int index;
		int imageIndex = -1;
		TreeNodeCollection nodes;
		TreeNode parent;
		int selectedImageIndex = -1;
		object tag;
		string text = "";
		TreeView treeView;

		public TreeNode() {
		}

		public TreeNode (string text)
		{
			if (text != null)
				this.text = text;
		}

		public TreeNode (string text, TreeNode[] children):
			this (text)
		{
			Nodes.AddRange (children);
		}


		public TreeNode (string text, int imageIndex, int selectedImageIndex):
			this (text)
		{
			this.imageIndex = imageIndex;
			this.selectedImageIndex = selectedImageIndex;
		}

		public TreeNode (string text, int imageIndex, int selectedImageIndex, TreeNode[] children):
			this (text, imageIndex, selectedImageIndex)
		{
			Nodes.AddRange (children);
		}


		#region Properties

		public bool Checked { 
			get {
				return (flags & Flags.HasCheck) != 0;
			}
			set {
				flags |= Flags.HasCheck;
			}
		}	

		public TreeNode FirstNode { 
			get {
				return (nodes == null || nodes.Count == 0) ? null : nodes [0];
			}
		}

		public string FullPath { 
			get {
				if (treeView == null)
					throw new Exception ("No TreeView associated");

				return "";
			}
		}

		public int Index { 
			get {
				return index;
			}
		}

		public int ImageIndex { 
			get {
				return imageIndex;
			}
			set {
				imageIndex = value;
			}
		}

		public bool IsExpanded { 
			get {
				return (flags & Flags.IsExpanded) != 0;
			}
		}

		public bool IsSelected { 
			get {
				return (flags & Flags.IsSelected) != 0;
			}
		}

		[MonoTODO]
		public bool IsVisible {
			get {
				throw new NotImplementedException ();
			}
		}

		public TreeNode LastNode { 
			get {
				return (nodes == null || nodes.Count == 0) ? null : nodes [nodes.Count - 1];
			}
		}

		public TreeNode NextNode { 
			get {
				if (parent.nodes == null || index == parent.nodes.Count - 1)
					return null;

				return parent.nodes [index + 1];
			}
		}

		public TreeNodeCollection Nodes {
			get {
				if (nodes == null)
					nodes = new TreeNodeCollection (this);
				return nodes;
			}
		}

		public TreeNode Parent { 
			get {
				return parent;
			}
		}

		public TreeNode PrevNode { 
			get {
				if (parent.nodes == null || index == 0)
					return null;

				return parent.nodes [index - 1];
			}
		}

		public int SelectedImageIndex { 
			get {
				return selectedImageIndex;
			}
			set {
				selectedImageIndex = value;
			}
		}

		public object Tag { 
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

		public string Text {
			get {
				return text;
			}
			set {
				this.text = (value == null) ? "" : value;
			}
		}

		#endregion

		#region ICloneable Members

		[MonoTODO ("Copy drawing fields")]
		public object Clone()
		{
			TreeNode tn = new TreeNode (text, imageIndex, selectedImageIndex);
			if (nodes != null) {
				foreach (TreeNode child in nodes)
					tn.Nodes.Add ((TreeNode)child.Clone ());
			}
			tn.Tag = tag;
			tn.Checked = Checked;
			return tn;
		}

		#endregion

		#region ISerializable Members

		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion

		// Internal

		internal void SetParent (TreeNode parent)
		{
			this.parent = parent;
		}

		internal void SetIndex (int index)
		{
			this.index = index;
		}

	}
}
