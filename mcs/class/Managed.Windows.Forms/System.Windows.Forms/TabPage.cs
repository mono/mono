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
using System.Drawing;

namespace System.Windows.Forms {

	public class TabPage : Panel {

		private int image_index = -1;
		private string tooltip_text = String.Empty;
		private Rectangle tab_bounds;
		private int row;
		
		public TabPage ()
		{
			Visible = true;
		}

		public TabPage (string text) : base ()
		{
			Text = text;
		}

		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		public new bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		public int ImageIndex {
			get { return image_index; }
			set {
				if (image_index == value)
					return;
				image_index = value;
				UpdateOwner ();
			}
		}

		public new int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		public override string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				UpdateOwner ();
			}
		}

		public string ToolTipText {
			get { return tooltip_text; }
			set {
				if (value == null)
					value = String.Empty;
				tooltip_text = value;
			}
		}

		public new bool Visible {
			get { return base.Visible; }
			set { base.Visible = value; }
		}

		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		public new event EventHandler EnabledChanged {
			add { base.EnabledChanged += value; }
			remove { base.EnabledChanged -= value; }
		}

		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		public new event EventHandler VisibleChanged {
			add { base.VisibleChanged += value; }
			remove { base.VisibleChanged -= value; }
		}

		public static TabPage GetTabPageOfComponent (object comp)
		{
			Control control = comp as Control;
			if (control == null)
				return null;
			control = control.Parent;
			while (control != null) {
				if (control is TabPage)
					break;
				control = control.Parent;
			}
			return control as TabPage;
		}

		public override string ToString ()
		{
			return "TabPage: {" + Text + "}";
		}

		internal Rectangle TabBounds {
			get { return tab_bounds; }
			set { tab_bounds = value; }
		}

		internal int Row {
			get { return row; }
			set { row = value; }
		}

		private void UpdateOwner ()
		{
			if (Owner != null) {
				// Will do some loving to the owner here
			}
		}

		private TabControl Owner {
			get { return base.Parent as TabControl; }
		}

		protected override ControlCollection CreateControlsInstance ()
		{
			return new TabPageControlCollection (this);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) 
		{
			if (Owner != null && Owner.IsHandleCreated) {
				Rectangle display = Owner.DisplayRectangle;
				base.SetBoundsCore (Owner.DisplayRectangle.X, Owner.DisplayRectangle.Y,
							Owner.DisplayRectangle.Width, Owner.DisplayRectangle.Height,
							BoundsSpecified.All);
			} else {
				base.SetBoundsCore (x, y, width, height, specified);
			}
		}

		public class TabPageControlCollection : ControlCollection {

			private TabPage owner;

			public TabPageControlCollection (TabPage owner) : base (owner)
			{
				this.owner = owner;
			}

			public void Add (Control value)
			{
				base.Add (value);
			}
		}
	}

	
}
