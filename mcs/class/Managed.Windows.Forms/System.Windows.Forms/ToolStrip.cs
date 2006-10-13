//
// ToolStrip.cs
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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent ("ItemClicked")]
	[DefaultProperty ("Items")]
	public class ToolStrip : ScrollableControl, IComponent, IDisposable
	{
		#region Private Variables
		private Color back_color;
		private Color fore_color;
		private ToolStripGripDisplayStyle grip_display_style;
		private Padding grip_margin;
		private ToolStripGripStyle grip_style;
		private Size image_scaling_size;
		private ToolStripItemCollection items;
		private LayoutEngine layout_engine;
		private ToolStripLayoutStyle layout_style;
		private Orientation orientation;
		private ToolStripRenderer renderer;
		private ToolStripRenderMode render_mode;
		private bool show_item_tool_tips;

		private ToolStripItem mouse_currently_over;
		#endregion

		#region Public Constructors
		public ToolStrip () : this (null)
		{
		}

		public ToolStrip (params ToolStripItem[] items) : base ()
		{
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);

			this.AutoSize = false;
			this.back_color = Control.DefaultBackColor;
			this.dock_style = this.DefaultDock;
			this.fore_color = Control.DefaultForeColor;
			this.grip_display_style = ToolStripGripDisplayStyle.Vertical;
			this.grip_margin = this.DefaultGripMargin;
			this.grip_style = ToolStripGripStyle.Visible;
			this.image_scaling_size = new Size (16, 16);
			this.items = new ToolStripItemCollection (this, items);
			this.layout_engine = new ToolStripSplitStackLayout ();
			this.layout_style = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.orientation = Orientation.Horizontal;
			this.Padding = this.DefaultPadding;
			this.renderer = new ToolStripProfessionalRenderer ();
			this.render_mode = ToolStripRenderMode.ManagerRenderMode;
			this.show_item_tool_tips = this.DefaultShowItemToolTips;

			DoAutoSize ();
		}
		#endregion

		#region Public Properties
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set {
				base.Anchor = value;
				base.dock_style = DockStyle.None;
			}
		}
		
		new public Color BackColor {
			get { return this.back_color; }
			set { this.back_color = value; }
		}

		public override Rectangle DisplayRectangle {
			get {
				if (this.orientation == Orientation.Horizontal)
					if (this.grip_style == ToolStripGripStyle.Hidden)
						return new Rectangle (this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
					else
						return new Rectangle (this.GripRectangle.Right + this.GripMargin.Right + this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal - this.GripRectangle.Right - this.GripMargin.Right, this.Height - this.Padding.Vertical);
				else
					if (this.grip_style == ToolStripGripStyle.Hidden)
						return new Rectangle (this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
					else
						return new Rectangle (this.Padding.Left, this.GripRectangle.Bottom + this.GripMargin.Bottom + this.Padding.Top, this.Width - this.Padding.Horizontal - this.GripRectangle.Right - this.GripMargin.Right, this.Height - this.Padding.Vertical);
			}
		}

		[DefaultValue (DockStyle.Top)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if (base.Dock != value) {			
					base.Dock = value;
					base.anchor_style = AnchorStyles.Left | AnchorStyles.Top;
					
					switch (value) {
						case DockStyle.Top:
						case DockStyle.Bottom:
						case DockStyle.None:
							this.orientation = Orientation.Horizontal;
							break;
						case DockStyle.Left:
						case DockStyle.Right:
							this.orientation = Orientation.Vertical;
							break;
					}

					DoAutoSize ();
				}
			}
		}

		[Browsable (false)]
		public Color ForeColor {
			get { return this.fore_color; }
			set { this.fore_color = value; }
		}

		[Browsable (false)]
		public ToolStripGripDisplayStyle GripDisplayStyle {
			get { return this.orientation == Orientation.Vertical ? ToolStripGripDisplayStyle.Horizontal : ToolStripGripDisplayStyle.Vertical; }
		}

		public Padding GripMargin {
			get { return this.grip_margin; }
			set { this.grip_margin = value; }
		}

		[Browsable (false)]
		public Rectangle GripRectangle {
			get {
				if (this.grip_style == ToolStripGripStyle.Hidden)
					return Rectangle.Empty;

				if (this.orientation == Orientation.Horizontal)
					return new Rectangle (this.grip_margin.Left + this.Padding.Left, this.Padding.Top, 3, this.Height);
				else
					return new Rectangle (this.Padding.Left, this.grip_margin.Top + this.Padding.Top, this.Width, 3);
			}
		}

		[DefaultValue (ToolStripGripStyle.Visible)]
		public ToolStripGripStyle GripStyle {
			get { return grip_style; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripGripStyle), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripGripStyle", value));
				grip_style = value;
			}
		}

		public Size ImageScalingSize {
			get { return this.image_scaling_size; }
			set { this.image_scaling_size = value; }
		}

		[Browsable (false)]
		public bool IsDropDown {
			get {
				//if (this is ToolStripDropDown)
				//        return true;

				return false;
			}
		}

		public virtual ToolStripItemCollection Items {
			get { return this.items; }
		}

		public override LayoutEngine LayoutEngine {
			get { return this.layout_engine; }
		}

		public ToolStripLayoutStyle LayoutStyle {
			get { return layout_style; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripLayoutStyle), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripLayoutStyle", value));

				this.layout_style = value;
				DoAutoSize ();
			}
		}

		[Browsable (false)]
		public Orientation Orientation {
			get { return this.orientation; }
		}

		[Browsable (false)]
		public ToolStripRenderer Renderer {
			get { return this.renderer; }
			set { this.renderer = value; }
		}

		public ToolStripRenderMode RenderMode {
			get { return this.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripRenderMode", value));

				if (value == ToolStripRenderMode.Custom && this.renderer == null)
					throw new NotSupportedException ("Must set Renderer property before setting RenderMode to Custom");
				if (value == ToolStripRenderMode.Professional || value == ToolStripRenderMode.System)
					this.renderer = new ToolStripProfessionalRenderer ();

				this.render_mode = value;
			}
		}

		[MonoTODO ("Need 2.0 ToolTip to implement tool tips.")]
		[DefaultValue (true)]
		public bool ShowItemToolTips {
			get { return this.show_item_tool_tips; }
			set { this.show_item_tool_tips = value; }
		}
		#endregion

		#region Protected Properties
		protected virtual DockStyle DefaultDock { get { return DockStyle.Top; } }
		protected virtual Padding DefaultGripMargin { get { return new Padding (2); } }
		protected override Padding DefaultMargin { get { return Padding.Empty; } }
		[MonoTODO ("This should override Control.DefaultPadding once it exists.")]
		protected virtual Padding DefaultPadding { get { return new Padding (0, 0, 1, 0); } }
		protected virtual bool DefaultShowItemToolTips { get { return true; } }
		protected override Size DefaultSize { get { return new Size (100, 25); } }
		#endregion

		#region Public Methods
		public ToolStripItem GetItemAt (Point point)
		{
			foreach (ToolStripItem tsi in this.items)
				if (tsi.Bounds.Contains (point))
					return tsi;

			return null;
		}

		public ToolStripItem GetItemAt (int x, int y)
		{
			return GetItemAt (new Point (x, y));
		}

		public virtual ToolStripItem GetNextItem (ToolStripItem start, ArrowDirection direction)
		{
			if (!Enum.IsDefined (typeof (ArrowDirection), direction))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ArrowDirection", direction));

			int index = this.items.IndexOf (start);

			switch (direction) {
				case ArrowDirection.Left:
				case ArrowDirection.Up:
					if (index > 0)
						return this.items[index - 1];
	
					return this.items[this.items.Count - 1];
				case ArrowDirection.Right:
				case ArrowDirection.Down:
					if (index + 1 >= this.items.Count)
						return this.items[0];

					return this.items[index + 1];
			}

			return null;
		}

		public override string ToString ()
		{
			return String.Format ("System.Windows.Forms.ToolStrip, Name: {0}, Items: {1}", this.name, this.items.Count.ToString ());
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			AccessibleObject ao = new AccessibleObject (this);
			
			ao.role = AccessibleRole.ToolBar;
			
			return ao;
		}
		
		protected override ControlCollection CreateControlsInstance ()
		{
			return base.CreateControlsInstance ();
		}

		protected internal virtual ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			if (text == "-")
				return new ToolStripSeparator ();

			return new ToolStripButton (text, image, onClick);
		}

		protected override void OnDockChanged (EventArgs e)
		{
			base.OnDockChanged (e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnInvalidated (InvalidateEventArgs e)
		{
			base.OnInvalidated (e);
		}

		protected internal virtual void OnItemAdded (ToolStripItemEventArgs e)
		{
			if (ItemAdded != null) ItemAdded (this, e);
		}

		protected virtual void OnItemClicked (ToolStripItemClickedEventArgs e)
		{
			if (ItemClicked != null) ItemClicked (this, e);
		}

		protected internal virtual void OnItemRemoved (ToolStripItemEventArgs e)
		{
			if (ItemRemoved != null) ItemRemoved (this, e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout (e);

			DoAutoSize ();

			this.layout_engine.Layout (this, e);
			this.OnLayoutCompleted (EventArgs.Empty);
			this.Invalidate ();
		}

		protected virtual void OnLayoutCompleted (EventArgs e)
		{
			if (LayoutCompleted != null) LayoutCompleted (this, e);
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
		}

		protected override void OnMouseDown (MouseEventArgs mea)
		{
			base.OnMouseDown (mea);

			if (mouse_currently_over != null)
				mouse_currently_over.DoMouseDown (mea);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);

			if (mouse_currently_over != null) {
				mouse_currently_over.DoMouseLeave (e);
				mouse_currently_over = null;
			}
		}

		protected override void OnMouseMove (MouseEventArgs mea)
		{
			base.OnMouseMove (mea);

			ToolStripItem tsi = this.GetItemAt (mea.X, mea.Y);

			if (tsi != null) {
				if (tsi == mouse_currently_over) 
					tsi.DoMouseMove (mea);
				else {
					if (mouse_currently_over != null)
						mouse_currently_over.DoMouseLeave (mea);

					mouse_currently_over = tsi;
					tsi.DoMouseEnter (mea);
					tsi.DoMouseMove (mea);
				}
			}
			else {
				if (mouse_currently_over != null) {
					mouse_currently_over.DoMouseLeave (mea);
					mouse_currently_over = null;
				}
			}
		}

		protected override void OnMouseUp (MouseEventArgs mea)
		{
			base.OnMouseUp (mea);

			if (mouse_currently_over != null)
				mouse_currently_over.DoMouseUp (mea);
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			// Draw the grip
			this.OnPaintGrip (e);

			// Make each item draw itself (if within the ClipRectangle)
			foreach (ToolStripItem tsi in this.items) {
				if (tsi.Available == false || !e.ClipRectangle.IntersectsWith (tsi.Bounds))
					continue;

				e.Graphics.TranslateTransform (tsi.Bounds.Left, tsi.Bounds.Top);
				tsi.DoPaint (e);
				e.Graphics.ResetTransform ();
			}
		}

		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);

			Rectangle affected_bounds = new Rectangle (new Point (0, 0), this.Size);

			this.renderer.DrawToolStripBackground (new ToolStripRenderEventArgs (pevent.Graphics, this, affected_bounds, Color.Empty));
			this.renderer.DrawToolStripBorder (new ToolStripRenderEventArgs (pevent.Graphics, this, affected_bounds, Color.Empty));
		}

		protected internal virtual void OnPaintGrip (PaintEventArgs e)
		{
			if (PaintGrip != null) PaintGrip (this, e);

			if (this.orientation == Orientation.Horizontal)
				e.Graphics.TranslateTransform (2, 0);
			else
				e.Graphics.TranslateTransform (0, 2);
				
			this.renderer.DrawGrip (new ToolStripGripRenderEventArgs (e.Graphics, this, this.GripRectangle, this.grip_display_style, this.grip_style));
			e.Graphics.ResetTransform ();
		}

		protected virtual void OnRendererChange (EventArgs e)
		{
			if (RendererChanged != null) RendererChanged (this, e);
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}

		protected override void OnTabStopChanged (EventArgs e)
		{
			base.OnTabStopChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		[MonoTODO("Implement for overflow")]
		protected virtual void SetDisplayedItems ()
		{

		}

		protected static void SetItemParent (ToolStripItem item, ToolStrip parent)
		{
			item.Parent = parent;
		}

		protected override void SetVisibleCore (bool value)
		{
			base.SetVisibleCore (value);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion

		#region To Be Removed
		// These are things I overrode to get the behavior I needed, but MS's API
		// says they aren't overridden.  So I need to hide them (?) or implement
		// them some other way...
		protected override void OnMouseHover (EventArgs e)
		{
			base.OnMouseHover (e);

			if (mouse_currently_over != null)
				mouse_currently_over.DoMouseHover (e);
		}

		protected override void OnClick (EventArgs e)
		{
			base.OnClick (e);

			if (mouse_currently_over != null) {
				mouse_currently_over.PerformClick ();
				OnItemClicked (new ToolStripItemClickedEventArgs (mouse_currently_over));
			}
		}

		protected override void OnDoubleClick (EventArgs e)
		{
			base.OnDoubleClick (e);

			if (mouse_currently_over != null)
				mouse_currently_over.DoDoubleClick (e);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AutoSizeChanged;
		[Browsable (false)]
		public event EventHandler ForeColorChanged;
		public event ToolStripItemEventHandler ItemAdded;
		public event ToolStripItemClickedEventHandler ItemClicked;
		public event ToolStripItemEventHandler ItemRemoved;
		public event EventHandler LayoutCompleted;
		public event EventHandler LayoutStyleChanged;
		public event PaintEventHandler PaintGrip;
		public event EventHandler RendererChanged;
		#endregion

		#region Private Methods
		private void DoAutoSize ()
		{
			if (this.AutoSize == true)
				this.Size = GetPreferredSize ();
		}

		private Size GetPreferredSize ()
		{
			Size new_size = Size.Empty;

			if (this.orientation == Orientation.Horizontal) {
				foreach (ToolStripItem tsi in this.items)
					if (tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom > new_size.Height)
						new_size.Height = tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom;

				new_size.Height += this.Padding.Top + this.Padding.Bottom;
				new_size.Width = this.Width;
			} else {
				foreach (ToolStripItem tsi in this.items) 
					if (tsi.GetPreferredSize (Size.Empty).Width + tsi.Margin.Left + tsi.Margin.Right > new_size.Width)
						new_size.Width = tsi.GetPreferredSize (Size.Empty).Width + tsi.Margin.Left + tsi.Margin.Right;

				new_size.Width += this.Padding.Left + this.Padding.Right;
				new_size.Height = this.Height;
			}

			return new_size;
		}
		#endregion
	}
}
#endif