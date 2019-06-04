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
		private bool sizing_grip;
		
		public StatusStrip ()
		{
			SetStyle (ControlStyles.ResizeRedraw, true);
			
			base.CanOverflow = false;
			this.GripStyle = ToolStripGripStyle.Hidden;
			base.LayoutStyle = ToolStripLayoutStyle.Table;
			base.RenderMode = ToolStripRenderMode.System;
			this.sizing_grip = true;
			base.Stretch = true;
		}

		#region Public Properties
		[DefaultValue (DockStyle.Bottom)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[Browsable (false)]
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
			get { return base.LayoutStyle; }
			set { base.LayoutStyle = value; }
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
			get { return new Rectangle (this.Width - 12, 0, 12, this.Height); }
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
			get {
				if (Orientation == Orientation.Horizontal) {
					return new Padding (1, 0, 14, 0);
				} else {
					return new Padding (1, 3, 1, 22);					
				}
			}
		}

		protected override bool DefaultShowItemToolTips {
			get { return false; }
		}

		protected override Size DefaultSize {
			get { return new Size (200, 22); }
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new StatusStripAccessibleObject ();
		}
		
		protected internal override ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			if (text == "-")
				return new ToolStripSeparator ();
				
			return new ToolStripStatusLabel (text, image, onClick);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			if (this.LayoutStyle == ToolStripLayoutStyle.Table)
				this.OnSpringTableLayoutCore ();
			base.OnLayout (levent);
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
			
			if (this.sizing_grip)
				this.Renderer.DrawStatusStripSizingGrip (new ToolStripRenderEventArgs (e.Graphics, this, Bounds, SystemColors.Control));
		}

		protected virtual void OnSpringTableLayoutCore ()
		{
			this.SuspendLayout ();

			TableLayoutSettings layoutSettings = (TableLayoutSettings) LayoutSettings;
			layoutSettings.RowCount = 0;
			layoutSettings.ColumnCount = 0;
			layoutSettings.ColumnStyles.Clear ();
			layoutSettings.RowStyles.Clear ();

			if (Orientation == Orientation.Horizontal) {
				foreach (ToolStripItem tsi in this.Items) {
					ColumnStyle style = new ColumnStyle ();
					if (tsi is ToolStripStatusLabel status_label && status_label.Spring) {
						style.SizeType = SizeType.Percent;
						style.Width = 100;
						tsi.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
					} else {
						tsi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
					}
					layoutSettings.ColumnStyles.Add (style);
				}
				layoutSettings.ColumnStyles.Add (new ColumnStyle ());
				layoutSettings.ColumnCount = layoutSettings.ColumnStyles.Count;
			} else {
				foreach (ToolStripItem tsi in this.Items) {
					RowStyle style = new RowStyle ();
					if (tsi is ToolStripStatusLabel status_label && status_label.Spring) {
						style.SizeType = SizeType.Percent;
						style.Height = 100;
						tsi.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
					} else {
						tsi.Anchor = AnchorStyles.Left | AnchorStyles.Right;
					}
					layoutSettings.RowStyles.Add (style);
				}
				layoutSettings.RowStyles.Add (new RowStyle ());
				layoutSettings.RowCount = layoutSettings.RowStyles.Count;
			}

			this.ResumeLayout (false);
		}

		protected override void SetDisplayedItems ()
		{
			// Only clean the internal collection, without modifying Owner/Parent on items.
			this.displayed_items.ClearInternal ();

			foreach (ToolStripItem tsi in this.Items)
				if (tsi.Placement == ToolStripItemPlacement.Main && tsi.Available) {
					this.displayed_items.AddNoOwnerOrLayout (tsi);
					tsi.Parent = this;
				}
		}
		
		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
				// If the mouse is over the size grip, change the cursor
				case Msg.WM_MOUSEMOVE: {
					if (FromParamToMouseButtons ((int) m.WParam.ToInt32()) == MouseButtons.None) {	
						Point p = new Point (LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()));
						
						if (this.SizingGrip && this.SizeGripBounds.Contains (p)) {
							this.Cursor = Cursors.SizeNWSE;
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
					Form form = FindForm ();

					if (this.SizingGrip && this.SizeGripBounds.Contains (p)) {
						// For top level forms it's not enoug to send a NCLBUTTONDOWN message, so
						// we make a direct call to our XplatUI engine.
						if (!form.IsMdiChild)
							XplatUI.BeginMoveResize (form.Handle);

						XplatUI.SendMessage (form.Handle, Msg.WM_NCLBUTTONDOWN, (IntPtr) HitTest.HTBOTTOMRIGHT, IntPtr.Zero);
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

		#region StatusStripAccessibleObject
		private class StatusStripAccessibleObject : AccessibleObject
		{
		}
		#endregion
	}
}
