//
// ToolStripPanelRow.cs
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

using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms.Layout;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[ToolboxItem (false)]
	public class ToolStripPanelRow : Component, IComponent, IDisposable, IBounds
	{
		private Rectangle bounds;
		internal List<Control> controls;
		private LayoutEngine layout_engine;
		private Padding margin;
		private Padding padding;
		private ToolStripPanel parent;

		#region Public Constructors
		public ToolStripPanelRow (ToolStripPanel parent)
		{
			this.bounds = Rectangle.Empty;
			this.controls = new List<Control> ();
			this.layout_engine = new DefaultLayout ();
			this.parent = parent;
		}
		#endregion

		#region Public Properties
		public Rectangle Bounds {
			get { return this.bounds; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control[] Controls {
			get { return this.controls.ToArray (); }
		}

		public Rectangle DisplayRectangle {
			get { return this.Bounds; }
		}

		public LayoutEngine LayoutEngine {
			get { 
				if (this.layout_engine == null)
					this.layout_engine = new DefaultLayout ();
					
				return this.layout_engine;
			}
		}

		public Padding Margin {
			get { return this.margin; }
			set { this.margin = value; }
		}

		public Orientation Orientation {
			get { return this.parent.Orientation; }
		}

		public virtual Padding Padding {
			get { return this.padding; }
			set { this.padding = value; }
		}

		public ToolStripPanel ToolStripPanel {
			get { return this.parent; }
		} 
		#endregion

		#region Protected Properties
		protected virtual Padding DefaultMargin { get { return Padding.Empty; } }
		protected virtual Padding DefaultPadding { get { return Padding.Empty; } }
		#endregion

		#region Public Methods
		public bool CanMove (ToolStrip toolStripToDrag)
		{
			// If something uses Stretch, it gets a whole Row to itself
			if (this.controls.Count > 0)
				if (toolStripToDrag.Stretch || (this.controls[0] as ToolStrip).Stretch)
					return false;
			
			int width = 0;
			
			foreach (ToolStrip ts in this.controls)
				width += (ts.Width + ts.Margin.Horizontal);
				
			if (width + toolStripToDrag.Width + toolStripToDrag.Margin.Horizontal <= this.bounds.Width)
				return true;
				
			return false;
		}
		#endregion

		#region Protected Methods
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
		
		protected void OnBoundsChanged (Rectangle oldBounds, Rectangle newBounds)
		{
		}
		
		protected internal virtual void OnControlAdded (Control control, int index)
		{
			control.SizeChanged += new EventHandler (control_SizeChanged);
			controls.Add (control);
			this.OnLayout (new LayoutEventArgs (control, string.Empty));
		}
		
		protected internal virtual void OnControlRemoved (Control control, int index)
		{
			control.SizeChanged -= new EventHandler (control_SizeChanged);
			controls.Remove (control);
			this.OnLayout (new LayoutEventArgs (control, string.Empty));
		}
		
		protected virtual void OnLayout (LayoutEventArgs e)
		{
			int height = 0;
			
			if (this.Orientation == Orientation.Horizontal) {
				foreach (ToolStrip ts in this.controls)
					if (ts.Height > height)
						height = ts.Height;
						
				if (height != this.bounds.Height)
					this.bounds.Height = height;
			} else {
				foreach (ToolStrip ts in this.controls)
					if (ts.GetPreferredSize (Size.Empty).Width > height)
						height = ts.GetPreferredSize (Size.Empty).Width;

				if (height != this.bounds.Width)
					this.bounds.Width = height;
			}
				
			this.Layout (this, e);
		}
		
		protected internal virtual void OnOrientationChanged ()
		{
		}
		#endregion

		#region Private/Internal Methods
		internal void SetBounds (Rectangle bounds)
		{
			if (this.bounds != bounds) {
				Rectangle old_bounds = this.bounds;
				this.bounds = bounds;
				this.OnBoundsChanged (old_bounds, bounds);
				this.OnLayout (new LayoutEventArgs (null, "Bounds"));
			}
		}
		
		private bool Layout (object container, LayoutEventArgs args)
		{
			ToolStripPanelRow tspr = (ToolStripPanelRow)container;
			Point position = tspr.DisplayRectangle.Location;
			foreach (ToolStrip ts in tspr.Controls)
			{
				if (Orientation == Orientation.Horizontal) {
					if (ts.Stretch)
						ts.Width = this.bounds.Width - ts.Margin.Horizontal - this.Padding.Horizontal;
					else
						ts.Width = ts.GetToolStripPreferredSize (Size.Empty).Width;
						
					position.X += ts.Margin.Left;
					ts.Location = position;
					
					position.X += (ts.Width + ts.Margin.Left);
				} else {
					if (ts.Stretch)
						ts.Size = new Size (ts.GetToolStripPreferredSize (Size.Empty).Width, this.bounds.Height - ts.Margin.Vertical - this.Padding.Vertical);
					else
						ts.Size = ts.GetToolStripPreferredSize (Size.Empty);

					position.Y += ts.Margin.Top;
					ts.Location = position;

					position.Y += (ts.Height + ts.Margin.Top);
				}
			}
			
			return false;
		}

		void control_SizeChanged (object sender, EventArgs e)
		{
			this.OnLayout (new LayoutEventArgs ((Control)sender, string.Empty));
		}
		#endregion
	}
}
