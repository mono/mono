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
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent ("ItemClicked")]
	[DefaultProperty ("Items")]
	public class ToolStrip : ScrollableControl, IComponent, IDisposable
	{
		#region Private Variables
		private bool allow_merge;
		private Color back_color;
		private bool can_overflow;
		private ToolStrip currently_merged_with;
		private ToolStripDropDownDirection default_drop_down_direction;
		internal ToolStripItemCollection displayed_items;
		private Color fore_color;
		private Padding grip_margin;
		private ToolStripGripStyle grip_style;
		private List<ToolStripItem> hidden_merged_items;
		private ImageList image_list;
		private Size image_scaling_size;
		private bool is_currently_merged;
		private ToolStripItemCollection items;
		private bool keyboard_active;
		private LayoutEngine layout_engine;
		private LayoutSettings layout_settings;
		private ToolStripLayoutStyle layout_style;
		private Orientation orientation;
		private ToolStripOverflowButton overflow_button;
		private List<ToolStripItem> pre_merge_items;
		private ToolStripRenderer renderer;
		private ToolStripRenderMode render_mode;
		private Timer tooltip_timer;
		private ToolTip.ToolTipWindow tooltip_window;
		private bool show_item_tool_tips;
		private bool stretch;

		private ToolStripItem mouse_currently_over;
		internal bool menu_selected;
		private ToolStripItem tooltip_currently_showing;
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
			this.items = new ToolStripItemCollection (this, items);
			this.allow_merge = true;
			base.AutoSize = true;
			this.back_color = Control.DefaultBackColor;
			this.can_overflow = true;
			base.CausesValidation = false;
			this.default_drop_down_direction = ToolStripDropDownDirection.BelowRight;
			this.displayed_items = new ToolStripItemCollection (this, null);
			this.Dock = this.DefaultDock;
			base.Font = new Font ("Tahoma", 8.25f);
			this.fore_color = Control.DefaultForeColor;
			this.grip_margin = this.DefaultGripMargin;
			this.grip_style = ToolStripGripStyle.Visible;
			this.image_scaling_size = new Size (16, 16);
			this.layout_style = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.orientation = Orientation.Horizontal;
			if (!(this is ToolStripDropDown))
				this.overflow_button = new ToolStripOverflowButton (this);
			this.renderer = null;
			this.render_mode = ToolStripRenderMode.ManagerRenderMode;
			this.show_item_tool_tips = this.DefaultShowItemToolTips;
			base.TabStop = false;
			this.ResumeLayout ();
			DoAutoSize ();
			
			// Register with the ToolStripManager
			ToolStripManager.AddToolStrip (this);
		}
		#endregion

		#region Public Properties
		public bool AllowMerge {
			get { return this.allow_merge; }
			set { this.allow_merge = false; }
		}
		
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Point AutoScrollPosition {
			get { return base.AutoScrollPosition; }
			set { base.AutoScrollPosition = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (true)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}
		
		new public Color BackColor {
			get { return this.back_color; }
			set { this.back_color = value; }
		}

		[DefaultValue (true)]
		public bool CanOverflow {
			get { return this.can_overflow; }
			set { this.can_overflow = value; }
		}
		
		[Browsable (false)]
		[DefaultValue (false)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ControlCollection Controls {
			get { return base.Controls; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}
		
		[Browsable (false)]
		public virtual ToolStripDropDownDirection DefaultDropDownDirection {
			get { return this.default_drop_down_direction; }
			set { 
				if (!Enum.IsDefined (typeof (ToolStripDropDownDirection), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripDropDownDirection", value));
					
				this.default_drop_down_direction = value;
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				if (this.orientation == Orientation.Horizontal)
					if (this.grip_style == ToolStripGripStyle.Hidden || this.layout_style == ToolStripLayoutStyle.Flow || this.layout_style == ToolStripLayoutStyle.Table)
						return new Rectangle (this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
					else
						return new Rectangle (this.GripRectangle.Right + this.GripMargin.Right, this.Padding.Top, this.Width - this.Padding.Horizontal - this.GripRectangle.Right - this.GripMargin.Right, this.Height - this.Padding.Vertical);
				else
					if (this.grip_style == ToolStripGripStyle.Hidden || this.layout_style == ToolStripLayoutStyle.Flow || this.layout_style == ToolStripLayoutStyle.Table)
						return new Rectangle (this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
					else
						return new Rectangle (this.Padding.Left, this.GripRectangle.Bottom + this.GripMargin.Bottom + this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical - this.GripRectangle.Bottom - this.GripMargin.Bottom);
			}
		}

		[DefaultValue (DockStyle.Top)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if (base.Dock != value) {
					base.Dock = value;
					
					switch (value) {
						case DockStyle.Top:
						case DockStyle.Bottom:
						case DockStyle.None:
							this.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
							break;
						case DockStyle.Left:
						case DockStyle.Right:
							this.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
							break;
					}
				}
			}
		}

		public override Font Font {
			get { return base.Font; }
			set { 
				if (base.Font != value) {
					base.Font = value;
					
					foreach (ToolStripItem tsi in this.Items)
						tsi.OnOwnerFontChanged (EventArgs.Empty);
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
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool HasChildren {
			get { return base.HasChildren; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new HScrollProperties HorizontalScroll {
			get { return base.HorizontalScroll; }
		}
		
		[Browsable (false)]
		[DefaultValue (null)]
		public ImageList ImageList {
			get { return this.image_list; }
			set { this.image_list = value; }
		}

		[DefaultValue ("{Width=16, Height=16}")]
		public Size ImageScalingSize {
			get { return this.image_scaling_size; }
			set { this.image_scaling_size = value; }
		}

		[MonoTODO ("Always returns false, dragging not implemented yet.")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool IsCurrentlyDragging {
			get { return false; }
		}
		
		[Browsable (false)]
		public bool IsDropDown {
			get {
				if (this is ToolStripDropDown)
					return true;

				return false;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual ToolStripItemCollection Items {
			get { return this.items; }
		}

		public override LayoutEngine LayoutEngine {
			get { 
				 if (layout_engine == null)
					this.layout_engine = new ToolStripSplitStackLayout ();
					
				 return this.layout_engine;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public LayoutSettings LayoutSettings {
			get { return this.layout_settings; }
			set { this.layout_settings = value; }
		}
		
		[AmbientValue (ToolStripLayoutStyle.StackWithOverflow)]
		public ToolStripLayoutStyle LayoutStyle {
			get { return layout_style; }
			set {
				if (this.layout_style != value) {
					if (!Enum.IsDefined (typeof (ToolStripLayoutStyle), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripLayoutStyle", value));

					this.layout_style = value;

					if (this.layout_style == ToolStripLayoutStyle.Flow)
						this.layout_engine = new FlowLayout ();
					else
						this.layout_engine = new ToolStripSplitStackLayout ();

					if (this.layout_style == ToolStripLayoutStyle.StackWithOverflow) {
						if (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right)
							this.layout_style = ToolStripLayoutStyle.VerticalStackWithOverflow;
						else
							this.layout_style = ToolStripLayoutStyle.HorizontalStackWithOverflow;
					}

					if (this.layout_style == ToolStripLayoutStyle.HorizontalStackWithOverflow)
						this.orientation = Orientation.Horizontal;
					else if (this.layout_style == ToolStripLayoutStyle.VerticalStackWithOverflow)
						this.orientation = Orientation.Vertical;
						
					this.layout_settings = this.CreateLayoutSettings (value);
					
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
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public ToolStripOverflowButton OverflowButton {
			get { return this.overflow_button; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		[DefaultValue (true)]
		public bool ShowItemToolTips {
			get { return this.show_item_tool_tips; }
			set { this.show_item_tool_tips = value; }
		}
		
		[DefaultValue (false)]
		public bool Stretch {
			get { return this.stretch; }
			set { this.stretch = value; }
		}
		
		[DefaultValue (false)]
		[DispId(-516)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new VScrollProperties VerticalScroll {
			get { return base.VerticalScroll; }
		}
		#endregion

		#region Protected Properties
		protected virtual DockStyle DefaultDock { get { return DockStyle.Top; } }
		protected virtual Padding DefaultGripMargin { get { return new Padding (2); } }
		protected override Padding DefaultMargin { get { return Padding.Empty; } }
		protected override Padding DefaultPadding { get { return new Padding (0, 0, 1, 0); } }
		protected virtual bool DefaultShowItemToolTips { get { return true; } }
		protected override Size DefaultSize { get { return new Size (100, 25); } }
		protected internal virtual ToolStripItemCollection DisplayedItems { get { return this.displayed_items; } }
		#endregion

		#region Public Methods
		public new Control GetChildAtPoint (Point point)
		{
			return base.GetChildAtPoint (point);
		}
		
		public ToolStripItem GetItemAt (Point point)
		{
			foreach (ToolStripItem tsi in this.displayed_items)
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

			ToolStripItem current_best = null;
			int current_best_point;
			
			switch (direction) {
				case ArrowDirection.Right:
					current_best_point = int.MaxValue;

					if (start != null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Left >= start.Right && loop_tsi.Left < current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Left;
							}
							
					if (current_best == null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Left < current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Left;
							}
							
					break;
				case ArrowDirection.Up:
					current_best_point = int.MinValue;

					if (start != null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Bottom <= start.Top && loop_tsi.Top > current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Top;
							}

					if (current_best == null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Top > current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Top;
							}

					break;
				case ArrowDirection.Left:
					current_best_point = int.MinValue;

					if (start != null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Right <= start.Left && loop_tsi.Left > current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Left;
							}

					if (current_best == null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Left > current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Left;
							}

					break;
				case ArrowDirection.Down:
					current_best_point = int.MaxValue;

					if (start != null) 
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Top >= start.Bottom && loop_tsi.Bottom < current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Top;
							}

					if (current_best == null)
						foreach (ToolStripItem loop_tsi in this.DisplayedItems)
							if (loop_tsi.Top < current_best_point && loop_tsi.Visible && loop_tsi.CanSelect) {
								current_best = loop_tsi;
								current_best_point = loop_tsi.Top;
							}

					break;
			}

			return current_best;
		}

		public void ResetMinimumSize ()
		{
			this.MinimumSize = new Size (-1, -1);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void SetAutoScrollMargin (int x, int y)
		{
			base.SetAutoScrollMargin (x, y);
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

		protected virtual LayoutSettings CreateLayoutSettings (ToolStripLayoutStyle layoutStyle)
		{
			switch (layoutStyle) {
				case ToolStripLayoutStyle.Flow:
					return new FlowLayoutSettings ();
				case ToolStripLayoutStyle.Table:
					//return new TableLayoutSettings ();
				case ToolStripLayoutStyle.StackWithOverflow:
				case ToolStripLayoutStyle.HorizontalStackWithOverflow:
				case ToolStripLayoutStyle.VerticalStackWithOverflow:
				default:
					return null;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (!IsDisposed) {
				ToolStripManager.RemoveToolStrip (this);
				base.Dispose (disposing);
			}
		}
		
		protected override void OnDockChanged (EventArgs e)
		{
			base.OnDockChanged (e);
		}

		protected override bool IsInputChar (char charCode)
		{
			return base.IsInputChar (charCode);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey (keyData);
		}
		
		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
			
			foreach (ToolStripItem tsi in this.Items)
				tsi.OnParentEnabledChanged (EventArgs.Empty);
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
			e.Item.Available = true;
			e.Item.SetPlacement (ToolStripItemPlacement.Main);
			this.DoAutoSize ();
			this.PerformLayout ();
			
			ToolStripItemEventHandler eh = (ToolStripItemEventHandler)(Events [ItemAddedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnItemClicked (ToolStripItemClickedEventArgs e)
		{
			ToolStripManager.SetActiveToolStrip (null);
			
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
			DoAutoSize ();
			base.OnLayout (e);

			this.SetDisplayedItems ();
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

		protected override void OnMouseCaptureChanged (EventArgs e)
		{
			base.OnMouseCaptureChanged (e);
		}
		
		protected override void OnMouseDown (MouseEventArgs mea)
		{
			if (mouse_currently_over != null)
			{
				if (this is MenuStrip && !(mouse_currently_over as ToolStripMenuItem).HasDropDownItems) {
					if (!menu_selected)
						(this as MenuStrip).FireMenuActivate ();
					
					return;
				}
					
				mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseDown);
				
				if (this is MenuStrip && !menu_selected) {
					(this as MenuStrip).FireMenuActivate ();
					menu_selected = true;				
				}
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
				MouseLeftItem (mouse_currently_over);
				mouse_currently_over.FireEvent (e, ToolStripItemEventType.MouseLeave);
				mouse_currently_over = null;
			}

			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs mea)
		{
			ToolStripItem tsi;
			// Find the item we are now 
			if (this.overflow_button != null && this.overflow_button.Visible && this.overflow_button.Bounds.Contains (mea.Location))
				tsi = this.overflow_button;
			else
				tsi = this.GetItemAt (mea.X, mea.Y);

			if (tsi != null) {
				// If we were already hovering on this item, just send a mouse move
				if (tsi == mouse_currently_over) 
					tsi.FireEvent (mea, ToolStripItemEventType.MouseMove);
				else {
					// If we were over a different item, fire a mouse leave on it
					if (mouse_currently_over != null) {
						MouseLeftItem (tsi);
						mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseLeave);
					}
					
					// Set the new item we are currently over
					mouse_currently_over = tsi;
					
					// Fire mouse enter and mouse move
					tsi.FireEvent (mea, ToolStripItemEventType.MouseEnter);
					MouseEnteredItem (tsi);
					tsi.FireEvent (mea, ToolStripItemEventType.MouseMove);

					// If we're over something with a drop down, show it
					if (menu_selected && mouse_currently_over.Enabled && mouse_currently_over is ToolStripDropDownItem && (mouse_currently_over as ToolStripDropDownItem).HasDropDownItems)
						(mouse_currently_over as ToolStripDropDownItem).ShowDropDown ();
				}
			} else {
				// We're not over anything now, just fire the mouse leave on what we used to be over
				if (mouse_currently_over != null) {
					MouseLeftItem (tsi);
					mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseLeave);
					mouse_currently_over = null;
				}
			}
			
			base.OnMouseMove (mea);
		}

		protected override void OnMouseUp (MouseEventArgs mea)
		{
			// If we're currently over an item (set in MouseMove)
			if (mouse_currently_over != null) {
				// Fire the item's MouseUp event
				mouse_currently_over.FireEvent (mea, ToolStripItemEventType.MouseUp);

				// The event handler may have blocked until the mouse moved off of the ToolStripItem
				if (mouse_currently_over == null)
					return;
					
				// Fire our ItemClicked event
				OnItemClicked (new ToolStripItemClickedEventArgs (mouse_currently_over));
			}

			base.OnMouseUp (mea);
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			// Draw the grip
			this.OnPaintGrip (e);

			// Make each item draw itself
			foreach (ToolStripItem tsi in this.displayed_items) {
				e.Graphics.TranslateTransform (tsi.Bounds.Left, tsi.Bounds.Top);
				tsi.FireEvent (e, ToolStripItemEventType.Paint);
				e.Graphics.ResetTransform ();
			}

			// Paint the Overflow button if it's visible
			if (this.overflow_button != null && this.overflow_button.Visible) {
				e.Graphics.TranslateTransform (this.overflow_button.Bounds.Left, this.overflow_button.Bounds.Top);
				this.overflow_button.FireEvent (e, ToolStripItemEventType.Paint);
				e.Graphics.ResetTransform ();
			}

			Rectangle affected_bounds = new Rectangle (new Point (0, 0), this.Size);
			Rectangle connected_area = Rectangle.Empty;

			if (this is ToolStripDropDown && (this as ToolStripDropDown).OwnerItem != null && !(this as ToolStripDropDown).OwnerItem.IsOnDropDown)
				connected_area = new Rectangle (1, 0, (this as ToolStripDropDown).OwnerItem.Width - 2, 2);

			ToolStripRenderEventArgs pevent = new ToolStripRenderEventArgs (e.Graphics, this, affected_bounds, Color.Empty);
			pevent.InternalConnectedArea = connected_area;

			this.Renderer.DrawToolStripBorder (pevent);
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
		}

		protected internal virtual void OnPaintGrip (PaintEventArgs e)
		{
			// Never draw a grip with these two layouts
			if (this.layout_style == ToolStripLayoutStyle.Flow || this.layout_style == ToolStripLayoutStyle.Table)
				return;
			
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintGripEvent]);
			if (eh != null)
				eh (this, e);

			if (!(this is MenuStrip)) {
				if (this.orientation == Orientation.Horizontal)
					e.Graphics.TranslateTransform (2, 0);
				else
					e.Graphics.TranslateTransform (0, 2);
			}

			this.Renderer.DrawGrip (new ToolStripGripRenderEventArgs (e.Graphics, this, this.GripRectangle, this.GripDisplayStyle, this.grip_style));
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

			foreach (ToolStripItem tsi in this.Items)
				tsi.OnParentRightToLeftChanged (e);
		}

		protected override void OnScroll (ScrollEventArgs se)
		{
			base.OnScroll (se);
		}
		
		protected override void OnTabStopChanged (EventArgs e)
		{
			base.OnTabStopChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			if ((keyData & Keys.Alt) == Keys.Alt && this.KeyboardActive) {
				this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.Keyboard);
				return true;
			}
			
			return base.ProcessCmdKey (ref msg, keyData);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			// Give each item a chance to handle the key
			foreach (ToolStripItem tsi in this.Items)
				if (tsi.ProcessDialogKey (keyData))
					return true;
			
			// See if I want to handle it
			if (this.ProcessArrowKey (keyData))
				return true;
			
			switch (keyData) {
				case Keys.Escape:
					this.Dismiss (ToolStripDropDownCloseReason.Keyboard);
					return true;
			}

			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}
		
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected virtual void SetDisplayedItems ()
		{
			this.displayed_items.Clear ();
			
			foreach (ToolStripItem tsi in this.items)
				if (tsi.Placement == ToolStripItemPlacement.Main && tsi.Available) {
					this.displayed_items.AddNoOwnerOrLayout (tsi);
					tsi.Parent = this; 
				}
				else if (tsi.Placement == ToolStripItemPlacement.Overflow)
					tsi.Parent = this.OverflowButton.DropDown; 
			
			if (this.OverflowButton != null)
				this.OverflowButton.DropDown.SetDisplayedItems ();
		}

		protected internal void SetItemLocation (ToolStripItem item, Point location)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
				
			if (item.Owner != this)
				throw new NotSupportedException ("The item is not owned by this ToolStrip");
				
			item.SetBounds (new Rectangle (location, item.Size));
		}
		
		protected internal static void SetItemParent (ToolStripItem item, ToolStrip parent)
		{
			if (item.Parent != null) {
				item.Parent.Items.RemoveNoOwnerOrLayout (item);
				
				if (item.Parent is ToolStripOverflow)
					(item.Parent as ToolStripOverflow).ParentToolStrip.Items.RemoveNoOwnerOrLayout (item);
			}
			
			parent.Items.AddNoOwnerOrLayout (item);
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

		#region Public Events
		static object ItemAddedEvent = new object ();
		static object ItemClickedEvent = new object ();
		static object ItemRemovedEvent = new object ();
		static object LayoutCompletedEvent = new object ();
		static object LayoutStyleChangedEvent = new object ();
		static object PaintGripEvent = new object ();
		static object RendererChangedEvent = new object ();

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		[Browsable (false)]
		public new event ControlEventHandler ControlAdded {
			add { base.ControlAdded += value; }
			remove { base.ControlAdded -= value; }
		}

		[Browsable (false)]
		public new event ControlEventHandler ControlRemoved {
			add { base.ControlRemoved += value; }
			remove { base.ControlRemoved -= value; }
		}
		
		[Browsable (false)]
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

		#region Internal Properties
		internal virtual bool KeyboardActive
		{
			get { return this.keyboard_active; }
			set {
				if (this.keyboard_active != value) {
					this.keyboard_active = value;
					
					if (value)
						Application.KeyboardCapture = this;
					else if (Application.KeyboardCapture == this)
						Application.KeyboardCapture = null;
				}
			}
		}
		#endregion
		
		#region Private Methods
		internal void ChangeSelection (ToolStripItem nextItem)
		{
			if (Application.KeyboardCapture != this)
				ToolStripManager.SetActiveToolStrip (this);
				
			foreach (ToolStripItem tsi in this.Items)
				if (tsi != nextItem)
					tsi.Dismiss (ToolStripDropDownCloseReason.Keyboard);
					
			nextItem.Select ();
			
			if (nextItem.Parent is MenuStrip && (nextItem.Parent as MenuStrip).MenuDroppedDown)
				(nextItem as ToolStripMenuItem).HandleAutoExpansion ();
		}
		
		internal virtual void Dismiss ()
		{
			this.Dismiss (ToolStripDropDownCloseReason.AppClicked);
		}
		
		internal virtual void Dismiss (ToolStripDropDownCloseReason reason)
		{
			// Release our stranglehold on the keyboard
			this.KeyboardActive = false;
			
			// Set our drop down flag to false;
			this.menu_selected = false;
			
			// Make sure all of our items are deselected and repainted
			foreach (ToolStripItem tsi in this.Items)
				tsi.Dismiss (reason);
		}
		
		private void DoAutoSize ()
		{
			if (this.AutoSize == true && this.Dock == DockStyle.None)
				this.Size = GetPreferredSize (Size.Empty);
				
			if (this.AutoSize == true && this.Orientation == Orientation.Horizontal && (this.Dock == DockStyle.Top || this.Dock == DockStyle.Bottom))
				this.Height = GetPreferredSize (Size.Empty).Height;
		}

		internal ToolStripItem GetCurrentlySelectedItem ()
		{
			foreach (ToolStripItem tsi in this.DisplayedItems)
				if (tsi.Selected)
					return tsi;
					
			return null;
		}
		
		public override Size GetPreferredSize (Size proposedSize)
		{
			Size new_size = new Size (0, this.Height);

			if (this.orientation == Orientation.Vertical) {
				foreach (ToolStripItem tsi in this.items)
					if (tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom > new_size.Height)
						new_size.Height = tsi.GetPreferredSize (Size.Empty).Height + tsi.Margin.Top + tsi.Margin.Bottom;

				new_size.Height += this.Padding.Top + this.Padding.Bottom;
				new_size.Width = this.Width;
			} else {
				foreach (ToolStripItem tsi in this.items) 
					if (tsi.Available) {
						Size tsi_preferred = tsi.GetPreferredSize (Size.Empty);
						new_size.Width += tsi_preferred.Width + tsi.Margin.Left + tsi.Margin.Right;
						
						if (new_size.Height < (this.Padding.Vertical + tsi_preferred.Height))
							new_size.Height = (this.Padding.Vertical + tsi_preferred.Height);
					}
			}

			new_size.Width += (this.GripRectangle.Width + this.GripMargin.Horizontal + this.Padding.Horizontal + 4);
			return new_size;
		}
		
		internal virtual ToolStrip GetTopLevelToolStrip ()
		{
			return this;
		}
		
		internal void HandleItemClick (ToolStripItem dismissingItem)
		{
			this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.ItemClicked);
			this.OnItemClicked (new ToolStripItemClickedEventArgs (dismissingItem));
		}
		
		internal void HideMenus (bool release, ToolStripDropDownCloseReason reason)
		{
			if (this is MenuStrip && release && menu_selected)
				(this as MenuStrip).FireMenuDeactivate ();
				
			if (release)
				menu_selected = false;
				
			NotifySelectedChanged (null);
		}

		internal void NotifySelectedChanged (ToolStripItem tsi)
		{
			foreach (ToolStripItem tsi2 in this.DisplayedItems)
				if (tsi != tsi2)
					if (tsi2 is ToolStripDropDownItem)
						(tsi2 as ToolStripDropDownItem).HideDropDown (ToolStripDropDownCloseReason.Keyboard);

			if (this.OverflowButton != null) {
				ToolStripItemCollection tsic = this.OverflowButton.DropDown.DisplayedItems;
				
				foreach (ToolStripItem tsi2 in tsic)
					if (tsi != tsi2)
						if (tsi2 is ToolStripDropDownItem)
							(tsi2 as ToolStripDropDownItem).HideDropDown (ToolStripDropDownCloseReason.Keyboard);
			
				this.OverflowButton.HideDropDown ();
			}
			
			foreach (ToolStripItem tsi2 in this.Items)
				if (tsi != tsi2)
					tsi2.Dismiss (ToolStripDropDownCloseReason.Keyboard);
		}
		
		internal virtual bool OnMenuKey ()
		{
			return false;
		}

		internal virtual bool ProcessArrowKey (Keys keyData)
		{
			switch (keyData) {
				case Keys.Right:
				case Keys.Tab:
					this.SelectNextToolStripItem (this.GetCurrentlySelectedItem (), true);
					return true;
				case Keys.Left:
				case Keys.Shift | Keys.Tab:
					this.SelectNextToolStripItem (this.GetCurrentlySelectedItem (), false);
					return true;
			}

			return false;
		}

		internal virtual void SelectNextToolStripItem (ToolStripItem start, bool forward)
		{
			ToolStripItem next_item = this.GetNextItem (start, forward ? ArrowDirection.Right : ArrowDirection.Left);
			
			this.ChangeSelection (next_item);
		}

		#region Stuff for ToolTips
		private void MouseEnteredItem (ToolStripItem item)
		{
			if (this.show_item_tool_tips) {
				tooltip_currently_showing = item;
				ToolTipTimer.Start ();
			}
		}
		
		private void MouseLeftItem (ToolStripItem item)
		{
			ToolTipTimer.Stop ();
			ToolTipWindow.Hide ();
			tooltip_currently_showing = null;
		}
		
		private Timer ToolTipTimer {
			get {
				if (tooltip_timer == null) {
					tooltip_timer = new Timer ();
					tooltip_timer.Enabled = false;
					tooltip_timer.Interval = 500;
					tooltip_timer.Tick += new EventHandler (ToolTipTimer_Tick);
				}
				
				return tooltip_timer;
			}
		}
		
		private ToolTip.ToolTipWindow ToolTipWindow {
			get {
				if (tooltip_window == null)
					tooltip_window = new ToolTip.ToolTipWindow ();
					
				return tooltip_window;
			}
		}
		
		private void ToolTipTimer_Tick (object o, EventArgs args)
		{
			string tooltip = tooltip_currently_showing.GetToolTip ();
			
			if (!string.IsNullOrEmpty (tooltip))
				ToolTipWindow.Present (this, tooltip);

			tooltip_currently_showing.FireEvent (EventArgs.Empty, ToolStripItemEventType.MouseHover);

			ToolTipTimer.Stop ();
		}
		#endregion

		#region Stuff for Merging
		internal ToolStrip CurrentlyMergedWith {
			get { return this.currently_merged_with; }
			set { this.currently_merged_with = value; }
		}
		
		internal List<ToolStripItem> HiddenMergedItems {
			get {
				if (this.hidden_merged_items == null)
					this.hidden_merged_items = new List<ToolStripItem> ();
					
				return this.hidden_merged_items;
			}
		}
		
		internal bool IsCurrentlyMerged {
			get { return this.is_currently_merged; }
			set { 
				this.is_currently_merged = value; 
				
				if (!value && this is MenuStrip) 
					foreach (ToolStripMenuItem tsmi in this.Items)
						tsmi.DropDown.IsCurrentlyMerged = value;
			 }
		}
		
		internal void BeginMerge ()
		{
			if (!IsCurrentlyMerged) {
				IsCurrentlyMerged = true;
				
				if (this.pre_merge_items == null) {
					this.pre_merge_items = new List<ToolStripItem> ();
			
				foreach (ToolStripItem tsi in this.Items)
					this.pre_merge_items.Add (tsi);
				}
				
				if (this is MenuStrip)
					foreach (ToolStripMenuItem tsmi in this.Items)
						tsmi.DropDown.BeginMerge ();
			}
		}
		
		internal void RevertMergeItem (ToolStripItem item)
		{
			int index = 0;

			// Remove it from it's current Parent
			if (item.Parent != null && item.Parent != this) {
				if (item.Parent is ToolStripOverflow)
					(item.Parent as ToolStripOverflow).ParentToolStrip.Items.RemoveNoOwnerOrLayout (item);
				else
					item.Parent.Items.RemoveNoOwnerOrLayout (item);

				item.Parent = item.Owner;	
			}
			
			// Find where the item was before the merge
			index = item.Owner.pre_merge_items.IndexOf (item);

			// Find the first pre-merge item that was after this item, that
			// is currently in the Items collection.  Insert our item before
			// that one.
			for (int i = index; i < this.pre_merge_items.Count; i++) {
				if (this.Items.Contains (this.pre_merge_items[i])) {
					item.Owner.Items.InsertNoOwnerOrLayout (this.Items.IndexOf (this.pre_merge_items[i]), item);
					return;
				}
			}
			
			// There aren't any items that are supposed to be after this item,
			// so just append it to the end.
			item.Owner.Items.AddNoOwnerOrLayout (item);
		}
		#endregion
		#endregion
	}
}
#endif
