//
// StatusStrip.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class StatusStrip : ToolStrip
	{
		private ToolStripLayoutStyle layout_style;
		private bool sizing_grip;
		
		public StatusStrip ()
		{
			this.GripStyle = ToolStripGripStyle.Hidden;
			this.layout_style = ToolStripLayoutStyle.Table;
			this.sizing_grip = false;
			base.stretch = true;
		}

		#region Public Properties
		public override DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}
		
		public ToolStripGripStyle GripStyle {
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}
		
		public ToolStripLayoutStyle LayoutStyle {	
			get { return this.layout_style; }
			set { this.layout_style = value; }
		}
		
		public Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		
		public bool ShowItemToolTips {
			get { return base.ShowItemToolTips; }
			set { base.ShowItemToolTips = value; }
		}
		
		public Rectangle SizeGripBounds {
			get { return Rectangle.Empty; }
		}
		
		public bool SizingGrip {
			get { return this.sizing_grip; }
			set { this.sizing_grip = value; }
		}
		
		public bool Stretch {
			get { return this.stretch; }
			set { this.stretch = value; }
		}
		#endregion

		#region Protected Properties
		protected override DockStyle DefaultDock {
			get { return DockStyle.Bottom; }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (1, 0, 14, 0); }
		}

		protected override bool DefaultShowItemToolTips {
			get { return false; }
		}

		protected override Size DefaultSize {
			get { return new Size (200, 22); }
		}
		#endregion

		#region Protected Methods
		protected internal override ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			if (text == "-")
				return new ToolStripSeparator ();
				
			return new ToolStripLabel (text, image, false, onClick);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout (e);
		}

		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		public event EventHandler PaddingChanged;
		#endregion
	}
}
#endif