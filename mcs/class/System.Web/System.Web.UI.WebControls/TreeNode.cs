//
// System.Web.UI.WebControls.TreeNode.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ParseChildrenAttribute (true, "ChildNodes")]
	public class TreeNode: IStateManager, ICloneable
	{
		StateBag ViewState = new StateBag ();
		TreeNodeCollection nodes;
		bool marked;
		TreeView tree;
		TreeNode parent;
		int index;
		
		public TreeNode ()
		{
		}
		
		public TreeNode (string text)
		{
			Text = text;
		}
		
		public TreeNode (string text, string value)
		{
			Text = text;
			Value = value;
		}
		
		public TreeNode (string text, string value, string imageUrl)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
		}
		
		public TreeNode (string text, string value, string imageUrl, string navigateUrl, string target)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
			NavigateUrl = navigateUrl;
			Target = target;
		}
		
		internal TreeView Tree {
			get { return tree; }
			set {
				if (SelectedFlag) {
					if (value != null)
						value.SetSelectedNode (this);
					else if (tree != null)
						tree.SetSelectedNode (null);
				}
				tree = value;
				if (nodes != null)
					nodes.SetTree (tree);
			}
		}
		
		public virtual bool Checked {
			get {
				object o = ViewState ["Checked"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Checked"] = value;
			}
		}

		[DefaultValue (null)]
		[MergableProperty (false)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		public virtual TreeNodeCollection ChildNodes {
			get {
				if (nodes == null) {
					nodes = new TreeNodeCollection (this);
					if (IsTrackingViewState)
						((IStateManager)nodes).TrackViewState();
				}
				return nodes;
			}
		}

		public virtual bool Expanded {
			get {
				object o = ViewState ["Expanded"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Expanded"] = value;
			}
		}

		public virtual string ImageToolTip {
			get {
				object o = ViewState ["ImageToolTip"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ImageToolTip"] = value;
			}
		}

		public virtual string ImageUrl {
			get {
				object o = ViewState ["ImageUrl"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ImageUrl"] = value;
			}
		}

		public virtual string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}
		}

		public bool PopulateOnDemand {
			get {
				object o = ViewState ["PopulateOnDemand"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["PopulateOnDemand"] = value;
			}
		}

		public TreeNodeSelectAction SelectAction {
			get {
				object o = ViewState ["SelectAction"];
				if (o != null) return (TreeNodeSelectAction)o;
				return TreeNodeSelectAction.Select;
			}
			set {
				ViewState ["SelectAction"] = value;
			}
		}

		public bool ShowCheckBox {
			get {
				object o = ViewState ["ShowCheckBox"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["ShowCheckBox"] = value;
			}
		}

		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Target"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("The display text of the tree node.")]
		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Text"] = value;
			}
		}

		public virtual string ToolTip {
			get {
				object o = ViewState ["ToolTip"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["ToolTip"] = value;
			}
		}

		public virtual string Value {
			get {
				object o = ViewState ["Value"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Value"] = value;
			}
		}
		
		public virtual bool Selected {
			get {
				return SelectedFlag;
			}
			set {
				if (tree != null) {
					if (!value && tree.SelectedNode == this)
						tree.SetSelectedNode (null);
					else if (value)
						tree.SetSelectedNode (this);
				}
				else
					SelectedFlag = value;
			}
		}
		
		internal virtual bool SelectedFlag {
			get {
				object o = ViewState ["Selected"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Selected"] = value;
			}
		}
		
		internal int Index {
			get { return index; }
			set { index = value; }
		}
		
		
		public TreeNode Parent {
			get { return parent; }
		}
		
		internal void SetParent (TreeNode node) {
			parent = node;
		}
		
		
		public void Collapse ()
		{
			Expanded = false;
		}

		public void CollapseAll ()
		{
			SetExpandedRec (false, -1);
		}

		public void Expand ()
		{
			Expanded = true;
		}

		public void Expand (int depth)
		{
			SetExpandedRec (true, depth);
		}

		public void ExpandAll ()
		{
			SetExpandedRec (true, -1);
		}
		
		void SetExpandedRec (bool expanded, int depth)
		{
			Expanded = expanded;
			if (depth == 0) return;
			
			if (nodes != null) {
				foreach (TreeNode nod in nodes)
					nod.SetExpandedRec (expanded, depth - 1);
			}
		}
		
		public void Select ()
		{
			Selected = true;
		}
		
		public void ToggleExpandState ()
		{
			Expanded = !Expanded;
		}

		public void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object[] states = (object[]) savedState;
			ViewState.LoadViewState (states [0]);
			
			if (tree != null && SelectedFlag)
				tree.SetSelectedNode (this);
				
			((IStateManager)ChildNodes).LoadViewState (states [1]);
		}
		
		public object SaveViewState ()
		{
			object[] states = new object[2];
			states[0] = ViewState.SaveViewState();
			states[1] = (nodes == null ? null : ((IStateManager)nodes).SaveViewState());
			
			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;
		}
		
		public void TrackViewState ()
		{
			marked = true;
			ViewState.TrackViewState();

			if (nodes != null)
				((IStateManager)nodes).TrackViewState ();
		}
		
		public bool IsTrackingViewState
		{
			get { return marked; }
		}
		
		public object Clone ()
		{
			object o = SaveViewState ();
			TreeNode nod = new TreeNode ();
			nod.LoadViewState (o);
			return nod;
		}
	}
}

#endif
