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
		private ImageList image_list;
		private Size image_scaling_size;
		private ToolStripItemCollection items;
		private LayoutEngine layout_engine;
		private LayoutSettings layout_settings;
		private ToolStripLayoutStyle layout_style;
		private Orientation orientation;
		private ToolStripRenderer renderer;
		private ToolStripRenderMode render_mode;
		private bool show_item_tool_tips;
		private bool stretch;

		private ToolStripItem mouse_currently_over;
		private bool menu_selected;
		private bool need_to_release_menu;
		#endregion

		#region Public Constructors
		public ToolStrip () : this (null)
		{
		}

		public ToolStrip (params ToolStripItem[] items) : base ()
		{
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);

			this.SuspendLayout ();
			base.AutoSize = true;
			this.back_color = Control.DefaultBackColor;
			base.CausesValidation = false;
			this.dock_style = this.DefaultDock;
			base.Font = new Font ("Tahoma", 8.25f);
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
			this.renderer = null;
			this.render_mode = ToolStripRenderMode.ManagerRenderMode;
			this.show_item_tool_tips = this.DefaultShowItemToolTips;
			base.TabStop = false;
			this.ResumeLayout ();
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

		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}
		
		new public Color BackColor {
			get { return this.back_color; }
			set { this.back_color = value; }
		}

		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}
		
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}
		
		public override Rectangle DisplayRectangle {
			get {
				if (this.orientation == Orientation.Horizontal)
					if (this.grip_style == ToolStripGripStyle.Hidden)
						return new Rectangle (this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
					else
						return new Rectangle (this.GripRectangle.Right + this.GripMargin.Right, this.Padding.Top, this.Width - this.Padding.Horizontal - this.GripRectangle.Right - this.GripMargin.Right, this.Height - this.Padding.Vertical);
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
					if (!Enum.IsDefined (typeof (DockStyle), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for DockStyle", value));
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

		public override Font Font {
			get { return base.Font; }
			set { 
				if (base.Font != value) {
					base.Font = value; 
					this.PerformLayout (); 
				}
			}
		}
		
		[Browsable (false)]
		public new Color ForeColor {
			get { return this.fore_color; }
			set { 
				if (this.fore_color != value) {
					this.fore_color = value; 
					this.OnForeColorChanged (EventArgs.Empty); 
				}
			}
		}

		[Browsable (false)]
		public ToolStripGripDisplayStyle GripDisplayStyle {
			get { return this.orientation == Orientation.Vertical ? ToolStripGripDisplayStyle.Horizontal : ToolStripGripDisplayStyle.Vertical; }
		}

		public Padding GripMargin {
			get { return this.grip_margin; }
			set { 
				if (this.grip_margin != value) {
					this.grip_margin = value; 
					this.PerformLayout (); 
				}
			}
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
			get { return this.grip_style; }
			set {
				if (this.grip_style != value) {
					if (!Enum.IsDefined (typeof (ToolStripGripStyle), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripGripStyle", value));
					this.grip_style = value;
					this.PerformLayout ();
				}
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		public ImageList ImageList {
			get { return this.image_list; }
			set { this.image_list = value; }
		}
		
		public Size ImageScalingSize {
			get { return this.image_scaling_size; }
			set { this.image_scaling_size = value; }
		}

		[Browsable (false)]
		public bool IsDropDown {
			get {
				if (this is ToolStripDropDown)
					return true;

				return false;
			}
		}

		public virtual ToolStripItemCollection Items {
			get { return this.items; }
		}

		public override LayoutEngine LayoutEngine {
			get { return new ToolStripSplitStackLayout(); }
		}

		[Browsable (false)]
		[DefaultValue (null)]
		public LayoutSettings LayoutSettings {
			get { return this.layout_settings; }
			set { this.layout_settings = value; }
		}
		
		public ToolStripLayoutStyle LayoutStyle {
			get { return layout_style; }
			set {
				if (this.layout_style != value) {
					if (!Enum.IsDefined (typeof (ToolStripLayoutStyle), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripLayoutStyle", value));

					this.layout_style = value;
					this.PerformLayout ();
					this.OnLayoutStyleChanged (EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		public Orientation Orientation {
			get { return this.orientation; }
		}

		[Browsable (false)]
		public ToolStripRenderer Renderer {
			get { 
				if (this.render_mode == ToolStripRenderMode.ManagerRenderMode)
					return ToolStripManager.Renderer;
					
				return this.renderer; 
			}
			set { 
				if (this.renderer != value) {
					this.renderer = value; 
					this.render_mode = ToolStripRenderMode.Custom;
					this.PerformLayout ();
					this.OnRendererChanged (EventArgs.Empty);
				}
			}
		}

		public ToolStripRenderMode RenderMode {
			get { return this.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripRenderMode", value));

				if (value == ToolStripRenderMode.Custom && this.renderer == null)
					throw new NotSupportedException ("Must set Renderer property before setting RenderMode to Custom");
				if (value == ToolStripRenderMode.Professional || value == ToolStripRenderMode.System)
					this.Renderer = new ToolStripProfessionalRenderer ();

				this.render_mode = value;
			}
		}

		[MonoTODO ("Need 2.0 ToolTip to implement tool tips.")]
		[DefaultValue (true)]
		public bool ShowItemToolTips {
			get { return this.show_item_tool_tips; }
			set { this.show_item_tool_tips = value; }
		}
		
		[DefaultValue (false)]
		public virtual bool Stretch {
			get { return this.stretch; }
			set { this.stretch = value; }
		}
		
		[DefaultValue (false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
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
				if (tsi.Visible && tsi.Bounds.Contains (point))
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
			return String.Format ("{0}, Name: {1}, Items: {2}", base.ToString(), this.Name, this.items.Count.ToString ());
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

			if (this is ToolStripDropDown)
				return new ToolStripMenuItem (text, image, onClick);
				
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
			ToolStripItemEventHandler eh = (ToolStripItemEventHandler)(Events [ItemAddedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnItemClicked (ToolStripItemClickedEventArgs e)
		{
			ToolStripItemClickedEventHandler eh = (ToolStripItemClickedEventHandler)(Events [ItemClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnItemRemoved (ToolStripItemEventArgs e)
		{
			ToolStripItemEventHandler eh = (ToolStripItemEventHandler)(Events [ItemRemovedEvent]);
			if (eh != null)
				eh (this, e);
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
			EventHandler eh = (EventHandler)(Events [LayoutCompletedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnLayoutStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[LayoutStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
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
			if (mouse_currently_over != null)
			{
				if (this is MenuStrip && !(mouse_currently_over as ToolStripMenuItem).HasDropDownItems) {
					if (!menu_selected)
						(this as MenuStrip).FireMenuActivate ();
					
					need_to_release_menu = true; 
					return;
				}
					
				mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseDown);
				
				need_to_release_menu = false;

				if (this is MenuStrip && !menu_selected) {
					(this as MenuStrip).FireMenuActivate ();
					menu_selected = true;				
				} else if (this is MenuStrip && menu_selected)
					need_to_release_menu = true;
			} else {
				if (this is MenuStrip)
					this.HideMenus (true, ToolStripDropDownCloseReason.AppClicked);
			}
			
			if (this is MenuStrip)
				this.Capture = false;

			base.OnMouseDown (mea);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			if (mouse_currently_over != null) {
				mouse_currently_over.FireEvent (e, ToolStripItemEventType.MouseLeave);
				mouse_currently_over = null;
			}

			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs mea)
		{
			ToolStripItem tsi = this.GetItemAt (mea.X, mea.Y);

			if (tsi != null) {
				if (tsi == mouse_currently_over) 
					tsi.FireEvent (mea, ToolStripItemEventType.MouseMove);
				else {
					if (mouse_currently_over != null) {
						mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseLeave);
						
						if (mouse_currently_over is ToolStripMenuItem)
							(mouse_currently_over as ToolStripMenuItem).HideDropDown(ToolStripDropDownCloseReason.Keyboard);
					} else {
						foreach (ToolStripItem tsi2 in this.Items)
							if (tsi2 is ToolStripMenuItem)
								(tsi2 as ToolStripMenuItem).HideDropDown (ToolStripDropDownCloseReason.Keyboard);
					}
						
					mouse_currently_over = tsi;
					
					tsi.FireEvent (mea, ToolStripItemEventType.MouseEnter);
					tsi.FireEvent (mea, ToolStripItemEventType.MouseMove);

					if (menu_selected && mouse_currently_over is ToolStripDropDownItem && (mouse_currently_over as ToolStripDropDownItem).HasDropDownItems) {
						(mouse_currently_over as ToolStripDropDownItem).DropDown.OwnerItem = (ToolStripMenuItem)mouse_currently_over;
						(mouse_currently_over as ToolStripDropDownItem).DropDown.Show ((mouse_currently_over as ToolStripDropDownItem).DropDownLocation);
					}
				}
			} else {
				if (mouse_currently_over != null) {
					mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseLeave);
					mouse_currently_over = null;
				}
			}

			base.OnMouseMove (mea);
		}

		protected override void OnMouseUp (MouseEventArgs mea)
		{
			if (mouse_currently_over != null) {
				mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseUp);

				// The event handler may have blocked until the mouse moved off of the ToolStripItem
				if (mouse_currently_over == null)
					return;
					
				OnItemClicked (new ToolStripItemClickedEventArgs (mouse_currently_over));
				
				if (mouse_currently_over.IsOnDropDown)
					need_to_release_menu = true;
					
				if (this is MenuStrip)
					if (!(mouse_currently_over as ToolStripMenuItem).HasDropDownItems && !(need_to_release_menu && menu_selected))
						(this as MenuStrip).FireMenuDeactivate ();
			}

			if (this is MenuStrip && need_to_release_menu)
				this.HideMenus (true, ToolStripDropDownCloseReason.ItemClicked);

			base.OnMouseUp (mea);
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
				tsi.FireEvent (e, ToolStripItemEventType.Paint);
				e.Graphics.ResetTransform ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);

			Rectangle affected_bounds = new Rectangle (new Point (0, 0), this.Size);
			Rectangle connected_area = Rectangle.Empty;

			if (this is ToolStripDropDown && (this as ToolStripDropDown).OwnerItem != null && !(this as ToolStripDropDown).OwnerItem.IsOnDropDown)
				connected_area = new Rectangle (1, 0, (this as ToolStripDropDown).OwnerItem.Width - 2, 2);
			
			ToolStripRenderEventArgs e = new ToolStripRenderEventArgs (pevent.Graphics, this, affected_bounds, Color.Empty);
			e.InternalConnectedArea = connected_area;
			
			this.Renderer.DrawToolStripBackground (e);
			this.Renderer.DrawToolStripBorder (e);
		}

		protected internal virtual void OnPaintGrip (PaintEventArgs e)
		{
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintGripEvent]);
			if (eh != null)
				eh (this, e);

			if (!(this is MenuStrip)) {
				if (this.orientation == Orientation.Horizontal)
					e.Graphics.TranslateTransform (2, 0);
				else
					e.Graphics.TranslateTransform (0, 2);
			}
			
			this.Renderer.DrawGrip (new ToolStripGripRenderEventArgs (e.Graphics, this, this.GripRectangle, this.grip_display_style, this.grip_style));
			e.Graphics.ResetTransform ();
		}

		protected virtual void OnRendererChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RendererChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
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
				mouse_currently_over.FireEvent (e, ToolStripItemEventType.MouseHover);
		}
		#endregion

		#region Public Events
		static object AutoSizeChangedEvent = new object ();
		static object ItemAddedEvent = new object ();
		static object ItemClickedEvent = new object ();
		static object ItemRemovedEvent = new object ();
		static object LayoutCompletedEvent = new object ();
		static object LayoutStyleChangedEvent = new object ();
		static object PaintGripEvent = new object ();
		static object RendererChangedEvent = new object ();

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeChangedEvent, value); }
		}

		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		public new event EventHandler CursorChanged {
			add { base.CursorChanged += value; }
			remove { base.CursorChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		public event ToolStripItemEventHandler ItemAdded {
			add { Events.AddHandler (ItemAddedEvent, value); }
			remove { Events.RemoveHandler (ItemAddedEvent, value); }
		}

		public event ToolStripItemClickedEventHandler ItemClicked {
			add { Events.AddHandler (ItemClickedEvent, value); }
			remove { Events.RemoveHandler (ItemClickedEvent, value); }
		}

		public event ToolStripItemEventHandler ItemRemoved {
			add { Events.AddHandler (ItemRemovedEvent, value); }
			remove { Events.RemoveHandler (ItemRemovedEvent, value); }
		}

		public event EventHandler LayoutCompleted {
			add { Events.AddHandler (LayoutCompletedEvent, value); }
			remove { Events.RemoveHandler (LayoutCompletedEvent, value); }
		}

		public event EventHandler LayoutStyleChanged {
			add { Events.AddHandler (LayoutStyleChangedEvent, value); }
			remove { Events.RemoveHandler (LayoutStyleChangedEvent, value); }
		}

		public event PaintEventHandler PaintGrip {
			add { Events.AddHandler (PaintGripEvent, value); }
			remove { Events.RemoveHandler (PaintGripEvent, value); }
		}

		public event EventHandler RendererChanged {
			add { Events.AddHandler (RendererChangedEvent, value); }
			remove { Events.RemoveHandler (RendererChangedEvent, value); }
		}
		#endregion

		#region Private Methods
		private void DoAutoSize ()
		{
			if (this.AutoSize == true && this.Dock == DockStyle.None)
				this.Size = GetPreferredSize (Size.Empty);
		}

		public override Size GetPreferredSize (Size proposedSize)
		{
			Size new_size = Size.Empty;

			if (this.orientation == Orientation.Vertical) {
				foreach (ToolStripItem tsi in this.items)
					if (tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom > new_size.Height)
						new_size.Height = tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom;

				new_size.Height += this.Padding.Top + this.Padding.Bottom;
				new_size.Width = this.Width;
			} else {
				foreach (ToolStripItem tsi in this.items) 
					if (tsi.Visible)
						new_size.Width += tsi.GetPreferredSize (Size.Empty).Width + tsi.Margin.Left + tsi.Margin.Right;

				new_size.Height = this.Height;
			}

			new_size.Width += (this.GripRectangle.Width + this.GripMargin.Horizontal + this.Padding.Horizontal + 4);
			return new_size;
		}
		
		internal void HideMenus (bool release, ToolStripDropDownCloseReason reason)
		{
			if (this is MenuStrip && release && menu_selected)
				(this as MenuStrip).FireMenuDeactivate ();
				
			if (release)
				menu_selected = false;
				
			foreach (ToolStripDropDownItem tsi in this.Items)
				if (tsi.Visible)
					tsi.DropDown.Close ();
		}

		//private void OnCursorChanged (EventArgs e)
		//{
		//        EventHandler eh = (EventHandler)(Events[CursorChangedEvent]);
		//        if (eh != null)
		//                eh (this, e);
		//}
		#endregion
	}
}
#endif
