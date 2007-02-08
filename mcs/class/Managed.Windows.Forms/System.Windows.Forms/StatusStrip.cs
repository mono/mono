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
			base.CanOverflow = false;
			this.GripStyle = ToolStripGripStyle.Hidden;
			this.layout_style = ToolStripLayoutStyle.Table;
			this.sizing_grip = true;
			base.Stretch = true;
		}

		#region Public Properties
		[DefaultValue (DockStyle.Bottom)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[DefaultValue (false)]
		public new bool CanOverflow {
			get { return base.CanOverflow; }
			set { base.CanOverflow = value; }
		}
		
		[DefaultValue (ToolStripGripStyle.Hidden)]
		public new ToolStripGripStyle GripStyle {
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}
		
		[DefaultValue (ToolStripLayoutStyle.Table)]
		public new ToolStripLayoutStyle LayoutStyle {	
			get { return this.layout_style; }
			set { this.layout_style = value; }
		}
		
		[Browsable (false)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		
		[DefaultValue (false)]
		public new bool ShowItemToolTips {
			get { return base.ShowItemToolTips; }
			set { base.ShowItemToolTips = value; }
		}
		
		[Browsable (false)]
		public Rectangle SizeGripBounds {
			get { return new Rectangle (this.Width - 12, this.Top, 12, this.Height); }
		}
		
		[DefaultValue (true)]
		public bool SizingGrip {
			get { return this.sizing_grip; }
			set { this.sizing_grip = value; }
		}
		
		[DefaultValue (true)]
		public new bool Stretch {
			get { return base.Stretch; }
			set { base.Stretch = value; }
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
			
			if (this.sizing_grip)
				this.Renderer.DrawStatusStripSizingGrip (new ToolStripRenderEventArgs (pevent.Graphics, this));
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
				// If the mouse is over the size grip, change the cursor
				case Msg.WM_MOUSEMOVE: {
					if (FromParamToMouseButtons ((int) m.WParam.ToInt32()) == MouseButtons.None) {	
						Point p = new Point (LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));
						
						if (this.SizingGrip && this.SizeGripBounds.Contains (p)) {
							this.Cursor = Cursors.SizeNESW;
							return;
						} else
							this.Cursor = Cursors.Default;
					}

					break;
				}
				// If the left mouse button is pushed over the size grip,
				// send the WM a message to begin a window resize operation
				case Msg.WM_LBUTTONDOWN: {
					Point p = new Point (LowOrder ((int)m.LParam.ToInt32 ()), HighOrder ((int)m.LParam.ToInt32 ()));

					if (this.SizingGrip && this.SizeGripBounds.Contains (p)) {
						XplatUI.SendMessage (this.FindForm().Handle, Msg.WM_NCLBUTTONDOWN, (IntPtr) HitTest.HTBOTTOMRIGHT, IntPtr.Zero);
						return;
					}
					
					break;
				}
			}

			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
		#endregion
	}
}
#endif
