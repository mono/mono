//
// ToolStripItem.cs
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

namespace System.Windows.Forms
{
	[DefaultEvent ("Click")]
	[DefaultProperty ("Text")]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	public abstract class ToolStripItem : Component, IDropTarget, IComponent, IDisposable
	{
		#region Private Variables
		private AccessibleObject accessibility_object;
		private string accessible_default_action_description;
		private ToolStripItemAlignment alignment;
		private bool auto_size;
		private bool auto_tool_tip;
		private Color back_color;
		private Image background_image;
		private ImageLayout background_image_layout;
		private Rectangle bounds;
		private bool can_select;
		private ToolStripItemDisplayStyle display_style;
		private DockStyle dock;
		private bool double_click_enabled;
		private bool enabled;
		private Size explicit_size;
		private Font font;
		private Color fore_color;
		private Image image;
		private ContentAlignment image_align;
		private int image_index;
		private ToolStripItemImageScaling image_scaling;
		private Color image_transparent_color;
		internal bool is_pressed;
		private bool is_selected;
		private Padding margin;
		private string name;
		private ToolStripItemOverflow overflow;
		private ToolStrip owner;
		internal ToolStripItem owner_item;
		private Padding padding;
		private ToolStripItemPlacement placement;
		private RightToLeft right_to_left;
		private Object tag;
		private string text;
		private ContentAlignment text_align;
		private TextImageRelation text_image_relation;
		private string tool_tip_text;
		private bool visible;

		private ToolStrip parent;
		private Size text_size;
		#endregion

		#region Public Constructors
		protected ToolStripItem ()
			: this (String.Empty, null, null, String.Empty)
		{
		}

		protected ToolStripItem (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, String.Empty)
		{
		}

		protected ToolStripItem (string text, Image image, EventHandler onClick, string name)
		{
			this.alignment = ToolStripItemAlignment.Left;
			this.auto_size = true;
			this.auto_tool_tip = this.DefaultAutoToolTip;
			this.back_color = Control.DefaultBackColor;
			this.background_image_layout = ImageLayout.Tile;
			this.can_select = true;
			this.display_style = this.DefaultDisplayStyle;
			this.dock = DockStyle.None;
			this.enabled = true;
			this.font = new Font ("Tahoma", 8.25f);
			this.fore_color = Control.DefaultForeColor;
			this.image = image;
			this.image_align = ContentAlignment.MiddleCenter;
			this.image_index = -1;
			this.image_scaling = ToolStripItemImageScaling.SizeToFit;
			this.image_transparent_color = Color.Empty;
			this.margin = this.DefaultMargin;
			this.name = name;
			this.overflow = ToolStripItemOverflow.AsNeeded;
			this.padding = this.DefaultPadding;
			this.placement = ToolStripItemPlacement.None;
			this.right_to_left = RightToLeft.Inherit;
			this.bounds.Size = this.DefaultSize;
			this.text = text;
			this.text_align = ContentAlignment.MiddleCenter;
			this.text_image_relation = TextImageRelation.ImageBeforeText;
			this.visible = true;

			this.Click += onClick;
			OnLayout (new LayoutEventArgs (null, string.Empty));
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AccessibleObject AccessibilityObject {
			get { 
				if (this.accessibility_object == null)
					this.accessibility_object = CreateAccessibilityInstance ();
					
				return this.accessibility_object;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string AccessibleDefaultActionDescription {
			get {
				if (this.accessibility_object == null)
					return null;
				
				return this.accessible_default_action_description;
			}
			set { this.accessible_default_action_description = value; }
		}

		[Localizable (true)]
		[DefaultValue (null)]
		public string AccessibleDescription {
			get {
				if (this.accessibility_object == null)
					return null;
				
				return this.AccessibilityObject.Description;
			}
			set { this.AccessibilityObject.description = value; }
		}

		[Localizable (true)]
		[DefaultValue (null)]
		public string AccessibleName {
			get { 
				if (this.accessibility_object == null)
					return null;
					
				return this.AccessibilityObject.Name; 
			}
			set { this.AccessibilityObject.Name = value; }
		}
		
		[DefaultValue (AccessibleRole.Default)]
		public AccessibleRole AccessibleRole {
			get
			{
				if (this.accessibility_object == null)
					return AccessibleRole.Default;
				
				return this.AccessibilityObject.Role;
			}
			set { this.AccessibilityObject.role = value; }
		}
		
		[MonoTODO]
		[DefaultValue (ToolStripItemAlignment.Left)]
		public ToolStripItemAlignment Alignment {
			get { return this.alignment; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripItemAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripItemAlignment", value));

				this.alignment = value;
			}
		}

		[Localizable (true)]
		[DefaultValue (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.All)]
		public bool AutoSize {
			get { return this.auto_size; }
			set { 
				this.auto_size = value; 
				this.CalculateAutoSize (); 
			}
		}

		[MonoTODO ("Need 2.0 ToolTip to implement tool tips.")]
		[DefaultValue (false)]
		public bool AutoToolTip {
			get { return this.auto_tool_tip; }
			set { this.auto_tool_tip = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Available {
			get { return this.visible; }
			set {
				if (this.visible != value) {
					visible = value;

					if (this.parent != null)
						parent.PerformLayout (); 
						
					OnAvailableChanged (EventArgs.Empty); 
					OnVisibleChanged (EventArgs.Empty);
				}
			}
		}

		public virtual Color BackColor {
			get { return this.back_color; }
			set {
				if (this.back_color != value) {
					back_color = value;
					OnBackColorChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (null)]
		public virtual Image BackgroundImage {
			get { return this.background_image; }
			set { 
				if (this.background_image != value) {
					this.background_image = value;
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (ImageLayout.Tile)]
		public virtual ImageLayout BackgroundImageLayout {
			get { return this.background_image_layout; }
			set { 
				if (this.background_image_layout != value) {
					this.background_image_layout = value;
					this.Invalidate (); 
				}
			}
		}

		[Browsable (false)]
		public virtual Rectangle Bounds {
			get { return this.bounds; }
		}

		[Browsable (false)]
		public virtual bool CanSelect {
			get { return this.can_select; }
		}

		[Browsable (false)]
		public Rectangle ContentRectangle {
			get {
				// ToolStripLabels don't have a border
				if (this is ToolStripLabel)
					return new Rectangle (0, 0, this.bounds.Width, this.bounds.Height);

				if (this is ToolStripDropDownButton && (this as ToolStripDropDownButton).ShowDropDownArrow)
					return new Rectangle (2, 2, this.bounds.Width - 13, this.bounds.Height - 4);

				return new Rectangle (2, 2, this.bounds.Width - 4, this.bounds.Height - 4);
			}
		}

		public virtual ToolStripItemDisplayStyle DisplayStyle {
			get { return this.display_style; }
			set {
				if (this.display_style != value) {
					this.display_style = value; 
					this.CalculateAutoSize (); 
					OnDisplayStyleChanged (EventArgs.Empty);
					if (this.Parent != null)
						this.Parent.PerformLayout ();
				}
			}
		}

		[Browsable (false)]
		[DefaultValue (DockStyle.None)]
		public DockStyle Dock {
			get { return this.dock; }
			set {
				if (this.dock != value) {
					if (!Enum.IsDefined (typeof (DockStyle), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for DockStyle", value));

					this.dock = value;
					this.CalculateAutoSize ();
				}
			}
		}

		[DefaultValue (false)]
		public bool DoubleClickEnabled {
			get { return this.double_click_enabled; }
			set { this.double_click_enabled = value; }
		}

		[Localizable (true)]
		[DefaultValue (true)]
		public virtual bool Enabled {
			get { return enabled; }
			set { 
				if (this.enabled != value) {
					this.enabled = value; 
					OnEnabledChanged (EventArgs.Empty); 
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		public virtual Font Font
		{
			get { return this.font; }
			set { 
				if (this.font != value) {
					this.font = value; 
					this.CalculateAutoSize (); 
					this.OnFontChanged (EventArgs.Empty); 
					this.Invalidate ();
				}
			}
		}

		public virtual Color ForeColor {
			get { return this.fore_color; }
			set { 
				if (this.fore_color != value) {
					this.fore_color = value; 
					this.OnForeColorChanged (EventArgs.Empty); 
					this.Invalidate ();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Height {
			get { return this.Size.Height; }
			set { 
				this.bounds.Height = value; 
				this.explicit_size.Height = value;
				
				if (this.visible) {
					this.CalculateAutoSize ();
					this.OnBoundsChanged ();
					this.Invalidate (); 
				} 
			}
		}

		[Localizable (true)]
		public virtual Image Image {
			get { return this.image; }
			set {
				this.image = value; 
				this.CalculateAutoSize (); 
				this.Invalidate ();
			}
		}

		[Localizable (true)]
		[DefaultValue (ContentAlignment.MiddleCenter)]
		public ContentAlignment ImageAlign {
			get { return this.image_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));

				this.image_align = value;
				this.Invalidate ();
			}
		}

		[Localizable (true)]
		[Browsable (false)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[TypeConverter (typeof (ImageIndexConverter))]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int ImageIndex {
			get { return this.image_index; }
			set {
				// Lamespec: MSDN says ArgumentException, tests say otherwise
				if (value < -1)
					throw new ArgumentOutOfRangeException ("ImageIndex cannot be less than -1");

				this.image_index = value;
				this.CalculateAutoSize ();
				this.Invalidate ();
			}
		}

		[Localizable (true)]
		[DefaultValue (ToolStripItemImageScaling.SizeToFit)]
		public ToolStripItemImageScaling ImageScaling {
			get { return this.image_scaling; }
			set { 
				this.image_scaling = value; 
				this.CalculateAutoSize (); 
				this.Invalidate (); 
			}
		}

		[Localizable (true)]
		[MonoTODO ("Stub, doesn't actually do anything yet.")]
		public Color ImageTransparentColor {
			get { return this.image_transparent_color; }
			set { this.image_transparent_color = value; }
		}
		
		[Browsable (false)]
		public bool IsOnDropDown {
			get {
				if (this.parent != null && this.parent is ToolStripDropDown)
					return true;

				return false;
			}
		}

		public Padding Margin {
			get { return this.margin; }
			set {
				this.margin = value; 
				this.CalculateAutoSize ();
			}
		}

		[DefaultValue (null)]
		[Browsable (false)]
		public string Name {
			get { return this.name; }
			set { this.name = value; }
		}

		[DefaultValue (ToolStripItemOverflow.AsNeeded)]
		public ToolStripItemOverflow Overflow {
			get { return this.overflow; }
			set { 
				if (this.overflow != value) {
					if (!Enum.IsDefined (typeof (ToolStripItemOverflow), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripItemOverflow", value));
				
					this.overflow = value;
					
					if (owner != null)
						owner.PerformLayout ();
				}
			}
		}
			
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ToolStrip Owner {
			get { return this.owner; }
			set { 
				if (this.owner != value) {
					this.owner = value; 
					this.CalculateAutoSize (); 
					OnOwnerChanged (EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ToolStripItem OwnerItem {
			get { return this.owner_item; }
		}

		public virtual Padding Padding {
			get { return this.padding; }
			set { 
				this.padding = value; 
				this.CalculateAutoSize (); 
				this.Invalidate (); 
			}
		}

		public ToolStripItemPlacement Placement {
			get { return this.placement; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Pressed { get { return this.is_pressed; } }

		[MonoTODO ("Stub, not implemented")]
		[Localizable (true)]
		[DefaultValue (RightToLeft.Inherit)]
		public virtual RightToLeft RightToLeft {
			get { return this.right_to_left; }
			set { this.right_to_left = value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Selected { get { return this.is_selected; } }

		[Localizable (true)]
		public virtual Size Size {
			get { 
				if (this.explicit_size != Size.Empty) 
					return this.explicit_size; 
					
				return this.bounds.Size; 
			}
			set { 
				this.bounds.Size = value; 
				this.explicit_size = value;
				
				if (this.visible) {
				this.CalculateAutoSize ();
				OnBoundsChanged (); }
			}
		}

		[Localizable (false)]
		[Bindable (true)]
		[DefaultValue (null)]
		[TypeConverter (typeof (StringConverter))]
		public Object Tag {
			get { return this.tag; }
			set { this.tag = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public virtual string Text
		{
			get { return this.text; }
			set { 
				if (this.text != value) { 
					this.text = value; 
					this.CalculateAutoSize (); 
					this.OnTextChanged (EventArgs.Empty); 
					this.Invalidate (); 
				} 
			}
		}

		[Localizable (true)]
		[DefaultValue (ContentAlignment.MiddleCenter)]
		public virtual ContentAlignment TextAlign {
			get { return this.text_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));
				this.text_align = value;
				this.Invalidate ();
			}
		}

		[Localizable (true)]
		[DefaultValue (TextImageRelation.ImageBeforeText)]
		public TextImageRelation TextImageRelation {
			get { return this.text_image_relation; }
			set { 
				this.text_image_relation = value; 
				this.CalculateAutoSize (); 
				this.Invalidate (); 
			}
		}

		[Localizable (true)]
		public string ToolTipText {
			get { return this.tool_tip_text; }
			set { this.tool_tip_text = value; }
		}

		[Localizable (true)]
		public bool Visible {
			get { 
				if (this.parent == null)
					return false;
			
				return this.visible && this.parent.Visible; 
			}
			set { 
				if (this.visible != value) {
					this.visible = value; 
					this.OnVisibleChanged (EventArgs.Empty); 
					this.Invalidate ();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Width {
			get { return this.Size.Width; }
			set { 
				this.bounds.Width = value; 
				this.explicit_size.Width = value;
				
				if (this.visible) {
					this.CalculateAutoSize ();
					this.OnBoundsChanged ();
					this.Invalidate ();
				}
			}
		}
		#endregion

		#region Protected Properties
		protected virtual bool DefaultAutoToolTip { get { return false; } }
		protected virtual ToolStripItemDisplayStyle DefaultDisplayStyle { get { return ToolStripItemDisplayStyle.ImageAndText; } }
		protected internal virtual Padding DefaultMargin { get { return new Padding (0, 1, 0, 2); } }
		protected virtual Padding DefaultPadding { get { return new Padding (); } }
		protected virtual Size DefaultSize { get { return new Size (23, 23); } }
		protected internal virtual bool DismissWhenClicked { get { return true; } }
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected internal ToolStrip Parent {
			get { return this.parent; }
			set { 
				ToolStrip old_parent = this.parent;
				this.parent = value; 
				OnParentChanged(old_parent, this.parent);
			}
		}
		protected internal virtual bool ShowKeyboardCues { get { return false; } }
		#endregion

		#region Public Methods
		public ToolStrip GetCurrentParent ()
		{ 
			return this.parent; 
		}

		public virtual Size GetPreferredSize (Size constrainingSize)
		{
			return this.CalculatePreferredSize (constrainingSize);
		}

		public void Invalidate ()
		{
			if (parent != null)
				parent.Invalidate (this.bounds);
		}

		public void Invalidate (Rectangle r)
		{
			if (parent != null)
				parent.Invalidate (r);
		}

		public void PerformClick ()
		{ 
			this.OnClick (EventArgs.Empty); 
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetBackColor () { this.BackColor = Control.DefaultBackColor; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetDisplayStyle () { this.display_style = this.DefaultDisplayStyle; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetFont () { this.font = new Font ("Tahoma", 8.25f); }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetForeColor () { this.ForeColor = Control.DefaultForeColor; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetImage () { this.image = null; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ResetMargin () { this.margin = this.DefaultMargin; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ResetPadding () { this.padding = this.DefaultPadding; }

		public void Select ()
		{
			if (!this.is_selected && this.CanSelect) {
				this.is_selected = true;
				this.Invalidate ();
				this.Parent.NotifySelectedChanged (this);
			}
		}

		public override string ToString ()
		{
			return this.text;
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripItemAccessibleObject (this);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
		
		protected virtual void OnAvailableChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AvailableChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnBackColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBoundsChanged ()
		{
			OnLayout (new LayoutEventArgs(null, ""));
		}

		protected virtual void OnClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnDisplayStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DisplayStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDoubleClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
			if (eh != null)
				eh (this, e);

			if (!double_click_enabled)
				OnClick (e);
		}

		void IDropTarget.OnDragDrop (DragEventArgs e)
		{
			// XXX
		}

		void IDropTarget.OnDragEnter (DragEventArgs e)
		{
			// XXX
		}

		void IDropTarget.OnDragLeave (EventArgs e)
		{
			// XXX
		}

		void IDropTarget.OnDragOver (DragEventArgs e)
		{
			// XXX
		}

		protected virtual void OnEnabledChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [EnabledChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnFontChanged (EventArgs e)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnForeColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnLayout (LayoutEventArgs e)
		{
		}

		protected virtual void OnLocationChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LocationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMouseDown (MouseEventArgs e)
		{
			if (this.Enabled) {
				this.is_pressed = true;
				this.Invalidate ();

				MouseEventHandler eh = (MouseEventHandler)(Events [MouseDownEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnMouseEnter (EventArgs e)
		{
			this.Select ();

			EventHandler eh = (EventHandler)(Events [MouseEnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMouseHover (EventArgs e)
		{
			if (this.Enabled) {
				EventHandler eh = (EventHandler)(Events [MouseHoverEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnMouseLeave (EventArgs e)
		{
			if (this.CanSelect) {
				this.is_selected = false;
				this.is_pressed = false;
				this.Invalidate ();
			}

			EventHandler eh = (EventHandler)(Events [MouseLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMouseMove (MouseEventArgs e)
		{
			if (this.Enabled) {
				MouseEventHandler eh = (MouseEventHandler)(Events [MouseMoveEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnMouseUp (MouseEventArgs e)
		{
			if (this.Enabled) {
				this.is_pressed = false;
				this.Invalidate ();

				if (this.IsOnDropDown)
					if (!(this is ToolStripDropDownItem)|| (this as ToolStripDropDownItem).DropDown.Visible == false)
						((this.Parent as ToolStripDropDown).OwnerItem as ToolStripDropDownItem).HideDropDown ();;
				
				MouseEventHandler eh = (MouseEventHandler)(Events [MouseUpEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnOwnerChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [OwnerChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPaint (PaintEventArgs e)
		{
			if (this.parent != null)
				this.parent.Renderer.DrawItemBackground (new ToolStripItemRenderEventArgs (e.Graphics, this));
				
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnParentChanged (ToolStrip oldParent, ToolStrip newParent)
		{
			if (oldParent != null)
				oldParent.PerformLayout ();
				
			if (newParent != null)
				newParent.PerformLayout ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnTextChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnVisibleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [VisibleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void SetBounds (Rectangle bounds)
		{
			if (this.bounds != bounds) {
				this.bounds = bounds;
				OnBoundsChanged ();
			}
		}
		#endregion

		#region Public Events
		static object AvailableChangedEvent = new object ();
		static object BackColorChangedEvent = new object ();
		static object ClickEvent = new object ();
		static object DisplayStyleChangedEvent = new object ();
		static object DoubleClickEvent = new object ();
		static object EnabledChangedEvent = new object ();
		static object ForeColorChangedEvent = new object ();
		static object LocationChangedEvent = new object ();
		static object MouseDownEvent = new object ();
		static object MouseEnterEvent = new object ();
		static object MouseHoverEvent = new object ();
		static object MouseLeaveEvent = new object ();
		static object MouseMoveEvent = new object ();
		static object MouseUpEvent = new object ();
		static object OwnerChangedEvent = new object ();
		static object PaintEvent = new object ();
		static object TextChangedEvent = new object ();
		static object VisibleChangedEvent = new object ();

		[Browsable (false)]
		public event EventHandler AvailableChanged {
			add { Events.AddHandler (AvailableChangedEvent, value); }
			remove {Events.RemoveHandler (AvailableChangedEvent, value); }
		}

		public event EventHandler BackColorChanged {
			add { Events.AddHandler (BackColorChangedEvent, value); }
			remove {Events.RemoveHandler (BackColorChangedEvent, value); }
		}

		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove {Events.RemoveHandler (ClickEvent, value); }
		}

		public event EventHandler DisplayStyleChanged {
			add { Events.AddHandler (DisplayStyleChangedEvent, value); }
			remove {Events.RemoveHandler (DisplayStyleChangedEvent, value); }
		}

		public event EventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove {Events.RemoveHandler (DoubleClickEvent, value); }
		}

		public event EventHandler EnabledChanged {
			add { Events.AddHandler (EnabledChangedEvent, value); }
			remove {Events.RemoveHandler (EnabledChangedEvent, value); }
		}

		public event EventHandler ForeColorChanged {
			add { Events.AddHandler (ForeColorChangedEvent, value); }
			remove {Events.RemoveHandler (ForeColorChangedEvent, value); }
		}

		public event EventHandler LocationChanged {
			add { Events.AddHandler (LocationChangedEvent, value); }
			remove {Events.RemoveHandler (LocationChangedEvent, value); }
		}

		public event MouseEventHandler MouseDown {
			add { Events.AddHandler (MouseDownEvent, value); }
			remove {Events.RemoveHandler (MouseDownEvent, value); }
		}

		public event EventHandler MouseEnter {
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove {Events.RemoveHandler (MouseEnterEvent, value); }
		}

		public event EventHandler MouseHover {
			add { Events.AddHandler (MouseHoverEvent, value); }
			remove {Events.RemoveHandler (MouseHoverEvent, value); }
		}

		public event EventHandler MouseLeave {
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove {Events.RemoveHandler (MouseLeaveEvent, value); }
		}

		public event MouseEventHandler MouseMove {
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove {Events.RemoveHandler (MouseMoveEvent, value); }
		}

		public event MouseEventHandler MouseUp {
			add { Events.AddHandler (MouseUpEvent, value); }
			remove {Events.RemoveHandler (MouseUpEvent, value); }
		}

		public event EventHandler OwnerChanged {
			add { Events.AddHandler (OwnerChangedEvent, value); }
			remove {Events.RemoveHandler (OwnerChangedEvent, value); }
		}

		public event PaintEventHandler Paint {
			add { Events.AddHandler (PaintEvent, value); }
			remove {Events.RemoveHandler (PaintEvent, value); }
		}

		public event EventHandler TextChanged {
			add { Events.AddHandler (TextChangedEvent, value); }
			remove {Events.RemoveHandler (TextChangedEvent, value); }
		}

		public event EventHandler VisibleChanged {
			add { Events.AddHandler (VisibleChangedEvent, value); }
			remove {Events.RemoveHandler (VisibleChangedEvent, value); }
		}
		#endregion

		#region Internal Methods
		internal Rectangle AlignInRectangle (Rectangle outer, Size inner, ContentAlignment align)
		{
			int x = 0;
			int y = 0;

			if (align == ContentAlignment.BottomLeft || align == ContentAlignment.MiddleLeft || align == ContentAlignment.TopLeft)
				x = outer.X;
			else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.TopCenter)
				x = Math.Max (outer.X + ((outer.Width - inner.Width) / 2), outer.Left);
			else if (align == ContentAlignment.BottomRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.TopRight)
				x = outer.Right - inner.Width;
			if (align == ContentAlignment.TopCenter || align == ContentAlignment.TopLeft || align == ContentAlignment.TopRight)
				y = outer.Y;
			else if (align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleRight)
				y = outer.Y + (outer.Height - inner.Height) / 2;
			else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomRight || align == ContentAlignment.BottomLeft)
				y = outer.Bottom - inner.Height;

			return new Rectangle (x, y, Math.Min (inner.Width, outer.Width), Math.Min (inner.Height, outer.Height));
		}

		internal void CalculateAutoSize ()
		{
			if (!this.auto_size || this is ToolStripControlHost)
				return;

			this.text_size = TextRenderer.MeasureText (this.Text == null ? string.Empty: this.text, this.Font, Size.Empty, TextFormatFlags.HidePrefix);
			//this.text_size.Width += 6;

			Size final_size = this.CalculatePreferredSize (Size.Empty);

			if (final_size != this.Size) {
				this.bounds.Size = final_size;
				if (this.parent != null)
					this.parent.PerformLayout ();
			}
		}

		internal Size CalculatePreferredSize (Size constrainingSize)
		{
			Size preferred_size = this.DefaultSize;

			switch (this.display_style) {
				case ToolStripItemDisplayStyle.Text:
					int width = text_size.Width + this.padding.Horizontal;
					int height = text_size.Height + this.padding.Vertical;
					preferred_size = new Size (width, height);
					break;
				case ToolStripItemDisplayStyle.Image:
					if (this.image == null)
						preferred_size = this.DefaultSize;
					else {
						switch (this.image_scaling) {
							case ToolStripItemImageScaling.None:
								preferred_size = this.image.Size;
								break;
							case ToolStripItemImageScaling.SizeToFit:
								if (this.parent == null)
									preferred_size = this.image.Size;
								else
									preferred_size = this.parent.ImageScalingSize;
								break;
						}
					}
					break;
				case ToolStripItemDisplayStyle.ImageAndText:
					int width2 = text_size.Width + this.padding.Horizontal;
					int height2 = text_size.Height + this.padding.Vertical;

					if (this.image != null) {
						switch (this.text_image_relation) {
							case TextImageRelation.Overlay:
								width2 = Math.Max (width2, this.image.Width);
								height2 = Math.Max (height2, this.image.Height);
								break;
							case TextImageRelation.ImageAboveText:
							case TextImageRelation.TextAboveImage:
								width2 = Math.Max (width2, this.image.Width);
								height2 += this.image.Height;
								break;
							case TextImageRelation.ImageBeforeText:
							case TextImageRelation.TextBeforeImage:
								height2 = Math.Max (height2, this.image.Height);
								width2 += this.image.Width;
								break;
						}
					}

					preferred_size = new Size (width2, height2);
					break;
			}

			if (!(this is ToolStripLabel)) {		// Everything but labels have a border
				preferred_size.Height += 4;
				preferred_size.Width += 4;
			}
			
			// Account for ToolStripDropDownButton's drop down arrow
			if (this is ToolStripDropDownButton && (this as ToolStripDropDownButton).ShowDropDownArrow)
				preferred_size.Width += 9;

			return preferred_size;
		}

		internal void CalculateTextAndImageRectangles (out Rectangle text_rect, out Rectangle image_rect)
		{
			this.CalculateTextAndImageRectangles (this.ContentRectangle, out text_rect, out image_rect);
		}
		
		internal void CalculateTextAndImageRectangles (Rectangle contentRectangle, out Rectangle text_rect, out Rectangle image_rect)
		{
			text_rect = Rectangle.Empty;
			image_rect = Rectangle.Empty;

			switch (this.display_style) {
				case ToolStripItemDisplayStyle.None:
					break;
				case ToolStripItemDisplayStyle.Text:
					if (this.text != string.Empty)
						text_rect = AlignInRectangle (contentRectangle, this.text_size, this.text_align);
					break;
				case ToolStripItemDisplayStyle.Image:
					if (this.image != null)
						image_rect = AlignInRectangle (contentRectangle, this.image.Size, this.image_align);
					break;
				case ToolStripItemDisplayStyle.ImageAndText:
					if (this.text != string.Empty && this.image == null)
						text_rect = AlignInRectangle (contentRectangle, this.text_size, this.text_align);
					else if (this.text == string.Empty && this.image != null)
						image_rect = AlignInRectangle (contentRectangle, this.image.Size, this.image_align);
					else if (this.text == string.Empty && this.image == null)
						break;
					else {
						Rectangle text_area;
						Rectangle image_area;

						switch (this.text_image_relation) {
							case TextImageRelation.Overlay:
								text_rect = AlignInRectangle (contentRectangle, this.text_size, this.text_align);
								image_rect = AlignInRectangle (contentRectangle, this.image.Size, this.image_align);
								break;
							case TextImageRelation.ImageAboveText:
								text_area = new Rectangle (contentRectangle.Left, contentRectangle.Bottom - (text_size.Height - 4), contentRectangle.Width, text_size.Height - 4);
								image_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, contentRectangle.Width, contentRectangle.Height - text_area.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, this.image.Size, this.image_align);
								break;
							case TextImageRelation.TextAboveImage:
								text_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, contentRectangle.Width, text_size.Height - 4);
								image_area = new Rectangle (contentRectangle.Left, text_area.Bottom, contentRectangle.Width, contentRectangle.Height - text_area.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, this.image.Size, this.image_align);
								break;
							case TextImageRelation.ImageBeforeText:
								text_area = new Rectangle (contentRectangle.Right - this.text_size.Width, contentRectangle.Top, this.text_size.Width, contentRectangle.Height);
								image_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, text_area.Left - contentRectangle.Left, contentRectangle.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, this.image.Size, this.image_align);
								break;
							case TextImageRelation.TextBeforeImage:
								text_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, this.text_size.Width, contentRectangle.Height);
								image_area = new Rectangle (text_area.Right, contentRectangle.Top, contentRectangle.Width - text_area.Width, contentRectangle.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, this.image.Size, this.image_align);
								break;
						}
					}
					break;
			}
		}

		internal void FireEvent (EventArgs e, ToolStripItemEventType met)
		{
			switch (met) {
				case ToolStripItemEventType.MouseUp:
					this.OnMouseUp ((MouseEventArgs)e);
					this.OnClick ((MouseEventArgs)e);
					break;
				case ToolStripItemEventType.MouseDown:
					this.OnMouseDown ((MouseEventArgs)e);
					break;
				case ToolStripItemEventType.MouseEnter:
					this.OnMouseEnter (e);
					break;
				case ToolStripItemEventType.MouseHover:
					this.OnMouseHover (e);
					break;
				case ToolStripItemEventType.MouseLeave:
					this.OnMouseLeave (e);
					break;
				case ToolStripItemEventType.MouseMove:
					this.OnMouseMove ((MouseEventArgs)e);
					break;
				case ToolStripItemEventType.Paint:
					this.OnPaint ((PaintEventArgs)e);
					break;
			}
		}
		
		internal void SetPlacement (ToolStripItemPlacement placement)
		{
			this.placement = placement;
		}
		#endregion
		
		public class ToolStripItemAccessibleObject : AccessibleObject
		{
			private ToolStripItem owner;
			
			public ToolStripItemAccessibleObject (ToolStripItem ownerItem)
			{
				if (ownerItem == null)
					throw new ArgumentNullException ("ownerItem");
					
				this.owner = ownerItem;
				base.default_action = string.Empty;
				base.keyboard_shortcut = string.Empty;
				base.name = string.Empty;
				base.value = string.Empty;
			}

			#region Public Properties
			public override Rectangle Bounds {
				get {
					return owner.Visible ? owner.Bounds : Rectangle.Empty;
				}
			}

			public override string DefaultAction {
				get { return base.DefaultAction; }
			}

			public override string Description {
				get { return base.Description; }
			}

			public override string Help {
				get { return base.Help; }
			}

			public override string KeyboardShortcut {
				get { return base.KeyboardShortcut; }
			}

			public override string Name {
				get {
					if (base.name == string.Empty)
						return owner.Text;
						
					return base.Name;
				}
				set { base.Name = value; }
			}

			public override AccessibleObject Parent {
				get { return base.Parent; }
			}

			public override AccessibleRole Role {
				get { return base.Role; }
			}

			public override AccessibleStates State {
				get { return base.State; }
			}
			#endregion

			#region Public Methods
			public void AddState (AccessibleStates state)
			{
				base.state = state;
			}

			public override void DoDefaultAction ()
			{
				base.DoDefaultAction ();
			}

			public override int GetHelpTopic (out string FileName)
			{
				return base.GetHelpTopic (out FileName);
			}

			public override AccessibleObject Navigate (AccessibleNavigation navdir)
			{
				return base.Navigate (navdir);
			}

			public override string ToString ()
			{
				return string.Format ("ToolStripItemAccessibleObject: Owner = {0}", owner.ToString());
			}
			#endregion
		}
	}
}
#endif
