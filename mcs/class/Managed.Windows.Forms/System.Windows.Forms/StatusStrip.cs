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
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new StatusStripAccessibleObject ();
		}
		
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

		protected override void OnLayout (LayoutEventArgs levent)
		{
			this.OnSpringTableLayoutCore ();
			this.Invalidate ();
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
			
			if (this.sizing_grip)
				this.Renderer.DrawStatusStripSizingGrip (new ToolStripRenderEventArgs (e.Graphics, this, Bounds, SystemColors.Control));
		}

		protected virtual void OnSpringTableLayoutCore ()
		{
			if (!this.Created)
				return;

			ToolStripItemOverflow[] overflow = new ToolStripItemOverflow[this.Items.Count];
			ToolStripItemPlacement[] placement = new ToolStripItemPlacement[this.Items.Count];
			Size proposedSize = new Size (0, Bounds.Height);
			int[] widths = new int[this.Items.Count];
			int total_width = 0;
			int toolstrip_width = DisplayRectangle.Width;
			int i = 0;
			int spring_count = 0;

			foreach (ToolStripItem tsi in this.Items) {
				overflow[i] = tsi.Overflow;
				widths[i] = tsi.GetPreferredSize (proposedSize).Width + tsi.Margin.Horizontal;
				placement[i] = tsi.Overflow == ToolStripItemOverflow.Always ? ToolStripItemPlacement.None : ToolStripItemPlacement.Main;
				placement[i] = tsi.Available && tsi.InternalVisible ? placement[i] : ToolStripItemPlacement.None;
				total_width += placement[i] == ToolStripItemPlacement.Main ? widths[i] : 0;
				if (tsi is ToolStripStatusLabel && (tsi as ToolStripStatusLabel).Spring)
					spring_count++;
					
				i++;
			}

			while (total_width > toolstrip_width) {
				bool removed_one = false;

				// Start at the right, removing Overflow.AsNeeded first
				for (int j = widths.Length - 1; j >= 0; j--)
					if (overflow[j] == ToolStripItemOverflow.AsNeeded && placement[j] == ToolStripItemPlacement.Main) {
						placement[j] = ToolStripItemPlacement.None;
						total_width -= widths[j];
						removed_one = true;
						break;
					}

				// If we didn't remove any AsNeeded ones, we have to start removing Never ones
				// These are not put on the Overflow, they are simply not shown
				if (!removed_one)
					for (int j = widths.Length - 1; j >= 0; j--)
						if (overflow[j] == ToolStripItemOverflow.Never && placement[j] == ToolStripItemPlacement.Main) {
							placement[j] = ToolStripItemPlacement.None;
							total_width -= widths[j];
							removed_one = true;
							break;
						}

				// There's nothing left to remove, break or we will loop forever	
				if (!removed_one)
					break;
			}

			if (spring_count > 0) {
				int per_item = (toolstrip_width - total_width) / spring_count;
				i = 0;
				
				foreach (ToolStripItem tsi in this.Items) {
					if (tsi is ToolStripStatusLabel && (tsi as ToolStripStatusLabel).Spring)
						widths[i] += per_item;
						
					i++;
				}
			}

			i = 0;
			Point layout_pointer = new Point (this.DisplayRectangle.Left, this.DisplayRectangle.Top);
			int button_height = this.DisplayRectangle.Height;

			// Now we should know where everything goes, so lay everything out
			foreach (ToolStripItem tsi in this.Items) {
				tsi.SetPlacement (placement[i]);

				if (placement[i] == ToolStripItemPlacement.Main) {
					tsi.SetBounds (new Rectangle (layout_pointer.X + tsi.Margin.Left, layout_pointer.Y + tsi.Margin.Top, widths[i] - tsi.Margin.Horizontal, button_height - tsi.Margin.Vertical));
					layout_pointer.X += widths[i];
				}

				i++;
			}

			this.SetDisplayedItems ();
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
