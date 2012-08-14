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


using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultEvent ("Click")]
	[DefaultProperty ("Text")]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	[Designer ("System.Windows.Forms.Design.ToolStripItemDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class ToolStripItem : Component, IDropTarget, IComponent, IDisposable
	{
		#region Private Variables
		private AccessibleObject accessibility_object;
		private string accessible_default_action_description;
		private bool allow_drop;
		private ToolStripItemAlignment alignment;
		private AnchorStyles anchor;
		private bool available;
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
		private string image_key;
		private ToolStripItemImageScaling image_scaling;
		private Color image_transparent_color;
		private bool is_disposed;
		internal bool is_pressed;
		private bool is_selected;
		private Padding margin;
		private MergeAction merge_action;
		private int merge_index;
		private string name;
		private ToolStripItemOverflow overflow;
		private ToolStrip owner;
		internal ToolStripItem owner_item;
		private Padding padding;
		private ToolStripItemPlacement placement;
		private RightToLeft right_to_left;
		private bool right_to_left_auto_mirror_image;
		private Object tag;
		private string text;
		private ContentAlignment text_align;
		private ToolStripTextDirection text_direction;
		private TextImageRelation text_image_relation;
		private string tool_tip_text;
		private bool visible;

		private EventHandler frame_handler;	// For animating images
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
			this.anchor = AnchorStyles.Left | AnchorStyles.Top;
			this.auto_size = true;
			this.auto_tool_tip = this.DefaultAutoToolTip;
			this.available = true;
			this.back_color = Color.Empty;
			this.background_image_layout = ImageLayout.Tile;
			this.can_select = true;
			this.display_style = this.DefaultDisplayStyle;
			this.dock = DockStyle.None;
			this.enabled = true;
			this.fore_color = Color.Empty;
			this.image = image;
			this.image_align = ContentAlignment.MiddleCenter;
			this.image_index = -1;
			this.image_key = string.Empty;
			this.image_scaling = ToolStripItemImageScaling.SizeToFit;
			this.image_transparent_color = Color.Empty;
			this.margin = this.DefaultMargin;
			this.merge_action = MergeAction.Append;
			this.merge_index = -1;
			this.name = name;
			this.overflow = ToolStripItemOverflow.AsNeeded;
			this.padding = this.DefaultPadding;
			this.placement = ToolStripItemPlacement.None;
			this.right_to_left = RightToLeft.Inherit;
			this.bounds.Size = this.DefaultSize;
			this.text = text;
			this.text_align = ContentAlignment.MiddleCenter;
			this.text_direction = DefaultTextDirection;
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
		
		[DefaultValue (ToolStripItemAlignment.Left)]
		public ToolStripItemAlignment Alignment {
			get { return this.alignment; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripItemAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripItemAlignment", value));

				if (this.alignment != value) {
					this.alignment = value;
					this.CalculateAutoSize (); 
				}
			}
		}

		[MonoTODO ("Stub, does nothing")]
		[Browsable (false)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual bool AllowDrop {
			get { return this.allow_drop; }
			set { this.allow_drop = value; }
		}
		
		[Browsable (false)]
		[DefaultValue (AnchorStyles.Top | AnchorStyles.Left)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AnchorStyles Anchor {
			get { return this.anchor; }
			set { this.anchor = value; }
		}
			
		[Localizable (true)]
		[DefaultValue (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[RefreshProperties (RefreshProperties.All)]
		public bool AutoSize {
			get { return this.auto_size; }
			set { 
				this.auto_size = value; 
				this.CalculateAutoSize (); 
			}
		}

		[DefaultValue (false)]
		public bool AutoToolTip {
			get { return this.auto_tool_tip; }
			set { this.auto_tool_tip = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Available {
			get { return this.available; }
			set {
				if (this.available != value) {
					available = value;
					visible = value;

					if (this.parent != null)
						parent.PerformLayout (); 
						
					OnAvailableChanged (EventArgs.Empty); 
					OnVisibleChanged (EventArgs.Empty);
				}
			}
		}

		public virtual Color BackColor {
			get {
				if (back_color != Color.Empty)
					return back_color;

				if (Parent != null)
					return parent.BackColor;

				return Control.DefaultBackColor;
			}
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
				if (this is ToolStripLabel || this is ToolStripStatusLabel)
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
				}
			}
		}

		[Browsable (false)]
		public bool IsDisposed {
			get { return this.is_disposed; }
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
			get { 
				if (Parent != null)
					if (!Parent.Enabled)
						return false;

				if (Owner != null)
					if (!Owner.Enabled)
						return false;
						
				return enabled;
			}
			set { 
				if (this.enabled != value) {
					this.enabled = value; 
					OnEnabledChanged (EventArgs.Empty); 
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		public virtual Font Font {
			get { 
				if (font != null)
					return font;
					
				if (Parent != null)
					return Parent.Font;
					
				return DefaultFont;
			}
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
			get { 
				if (fore_color != Color.Empty)
					return fore_color;
					
				if (Parent != null)
					return parent.ForeColor;
					
				return Control.DefaultForeColor;
			}
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
				this.Size = new Size (this.Size.Width, value); 
				this.explicit_size.Height = value;
				
				if (this.Visible) {
					this.CalculateAutoSize ();
					this.OnBoundsChanged ();
					this.Invalidate (); 
				} 
			}
		}

		[Localizable (true)]
		public virtual Image Image {
			get { 
				if (this.image != null)
					return this.image;
					
				if (this.image_index >= 0)
					if (this.owner != null && this.owner.ImageList != null && this.owner.ImageList.Images.Count > this.image_index)
						return this.owner.ImageList.Images[this.image_index];


				if (!string.IsNullOrEmpty (this.image_key))
					if (this.owner != null && this.owner.ImageList != null && this.owner.ImageList.Images.Count > this.image_index)
						return this.owner.ImageList.Images[this.image_key];
						
				return null;
			}
			set {
				if (this.image != value) {
					StopAnimation ();
					
					this.image = value; 
					this.image_index = -1;
					this.image_key = string.Empty;
					this.CalculateAutoSize (); 
					this.Invalidate ();
					
					BeginAnimation ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (ContentAlignment.MiddleCenter)]
		public ContentAlignment ImageAlign {
			get { return this.image_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));

				if (image_align != value) {
					this.image_align = value;
					this.CalculateAutoSize (); 
				}
			}
		}

		[Localizable (true)]
		[Browsable (false)]
		[RelatedImageList ("Owner.ImageList")]
		[TypeConverter (typeof (NoneExcludedImageIndexConverter))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[Editor ("System.Windows.Forms.Design.ToolStripImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public int ImageIndex {
			get { return this.image_index; }
			set {
				if (this.image_index != value) {
					// Lamespec: MSDN says ArgumentException, tests say otherwise
					if (value < -1)
						throw new ArgumentOutOfRangeException ("ImageIndex cannot be less than -1");

					this.image_index = value;
					this.image = null;
					this.image_key = string.Empty;
					this.CalculateAutoSize ();
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		[Browsable (false)]
		[RelatedImageList ("Owner.ImageList")]
		[TypeConverter (typeof (ImageKeyConverter))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[Editor ("System.Windows.Forms.Design.ToolStripImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageKey {
			get { return this.image_key; }
			set { 
				if (this.image_key != value) {
					this.image = null;
					this.image_index = -1;
					this.image_key = value;
					this.CalculateAutoSize ();
					this.Invalidate ();
				}
			}
		}
		
		[Localizable (true)]
		[DefaultValue (ToolStripItemImageScaling.SizeToFit)]
		public ToolStripItemImageScaling ImageScaling {
			get { return this.image_scaling; }
			set { 
				if (image_scaling != value) {
					this.image_scaling = value; 
					this.CalculateAutoSize (); 
				}
			}
		}

		[Localizable (true)]
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

		[Browsable (false)]
		public bool IsOnOverflow {
			get { return this.placement == ToolStripItemPlacement.Overflow; }
		}
		
		public Padding Margin {
			get { return this.margin; }
			set {
				this.margin = value; 
				this.CalculateAutoSize ();
			}
		}

		[DefaultValue (MergeAction.Append)]
		public MergeAction MergeAction {
			get { return this.merge_action; }
			set {
				if (!Enum.IsDefined (typeof (MergeAction), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for MergeAction", value));
					
				this.merge_action = value;
			}
		}

		[DefaultValue (-1)]
		public int MergeIndex {
			get { return this.merge_index; }
			set { this.merge_index = value; }
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
					if (this.owner != null)
						this.owner.Items.Remove (this);
					
					if (value != null)	
						value.Items.Add (this);
					else
						this.owner = null;
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

		[Browsable (false)]
		public ToolStripItemPlacement Placement {
			get { return this.placement; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Pressed { get { return this.is_pressed; } }

		[MonoTODO ("RTL not implemented")]
		[Localizable (true)]
		public virtual RightToLeft RightToLeft {
			get { return this.right_to_left; }
			set { 
				if (this.right_to_left != value) {
					this.right_to_left = value;
					this.OnRightToLeftChanged (EventArgs.Empty);
				}
			}
		}
		
		[Localizable (true)]
		[DefaultValue (false)]
		public bool RightToLeftAutoMirrorImage {
			get { return this.right_to_left_auto_mirror_image; }
			set { 
				if (this.right_to_left_auto_mirror_image != value) {
					this.right_to_left_auto_mirror_image = value;
					this.Invalidate ();
				}
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Selected { get { return this.is_selected; } }

		[Localizable (true)]
		public virtual Size Size {
			get { 
				if (!this.AutoSize && this.explicit_size != Size.Empty) 
					return this.explicit_size; 
					
				return this.bounds.Size; 
			}
			set { 
				this.bounds.Size = value; 
				this.explicit_size = value;
				
				if (this.Visible) {
					this.CalculateAutoSize ();
					this.OnBoundsChanged (); 
				}
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
					this.Invalidate (); 
					this.CalculateAutoSize (); 
					this.Invalidate ();
					this.OnTextChanged (EventArgs.Empty); 
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

				if (this.text_align != value) {
					this.text_align = value;
					this.CalculateAutoSize (); 
				}
			}
		}

		public virtual ToolStripTextDirection TextDirection {
			get {
				if (this.text_direction == ToolStripTextDirection.Inherit) {
					if (this.Parent != null)
						return this.Parent.TextDirection;
					else
						return ToolStripTextDirection.Horizontal;
				}

				return this.text_direction;
			}
			set {
				if (!Enum.IsDefined (typeof (ToolStripTextDirection), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripTextDirection", value));
				
				if (this.text_direction != value) {
					this.text_direction = value;
					this.CalculateAutoSize ();
					this.Invalidate ();
				}
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
		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
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
					this.available = value;
					this.SetVisibleCore (value);
					if (this.Owner != null)
						this.Owner.PerformLayout ();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Width {
			get { return this.Size.Width; }
			set { 
				this.Size = new Size (value, this.Size.Height); 
				this.explicit_size.Width = value;
				
				if (this.Visible) {
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
				if (this.parent != value) {
					ToolStrip old_parent = this.parent;
					this.parent = value; 
					OnParentChanged(old_parent, this.parent);
				}
			}
		}
		protected internal virtual bool ShowKeyboardCues { get { return false; } }
		#endregion

		#region Public Methods
		[MonoTODO ("Stub, does nothing")]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DragDropEffects DoDragDrop (Object data, DragDropEffects allowedEffects)
		{
			return allowedEffects;
		}
		
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
		public virtual void ResetBackColor () { this.BackColor = Color.Empty; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetDisplayStyle () { this.display_style = this.DefaultDisplayStyle; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetFont () { this.font = null; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetForeColor () { this.ForeColor = Color.Empty; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetImage () { this.image = null; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ResetMargin () { this.margin = this.DefaultMargin; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ResetPadding () { this.padding = this.DefaultPadding; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetRightToLeft () { this.right_to_left = RightToLeft.Inherit; }
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetTextDirection () { this.TextDirection = this.DefaultTextDirection; }

		public void Select ()
		{
			if (!this.is_selected && this.CanSelect) {
				this.is_selected = true;
				
				if (this.Parent != null) {
					if (this.Visible && this.Parent.Focused && this is ToolStripControlHost)
						(this as ToolStripControlHost).Focus ();
						
					this.Invalidate ();
					this.Parent.NotifySelectedChanged (this);
				}
				OnUIASelectionChanged ();
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
			if (!is_disposed && disposing)
				is_disposed = true;

			if (image != null) {
				StopAnimation ();
				image = null;
			}

			if (owner != null)
				owner.Items.Remove (this);
			
			base.Dispose (disposing);
		}
		
		protected internal virtual bool IsInputChar (char charCode)
		{
			return false;
		}
		
		protected internal virtual bool IsInputKey (Keys keyData)
		{
			return false;
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
			OnLayout (new LayoutEventArgs(null, string.Empty));
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
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnDragDrop (DragEventArgs dragEvent)
		{
			DragEventHandler eh = (DragEventHandler)(Events[DragDropEvent]);
			if (eh != null)
				eh (this, dragEvent);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnDragEnter (DragEventArgs dragEvent)
		{
			DragEventHandler eh = (DragEventHandler)(Events[DragEnterEvent]);
			if (eh != null)
				eh (this, dragEvent);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnDragLeave (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[DragLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnDragOver (DragEventArgs dragEvent)
		{
			DragEventHandler eh = (DragEventHandler)(Events[DragOverEvent]);
			if (eh != null)
				eh (this, dragEvent);
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

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs giveFeedbackEvent)
		{
			GiveFeedbackEventHandler eh = (GiveFeedbackEventHandler)(Events[GiveFeedbackEvent]);
			if (eh != null)
				eh (this, giveFeedbackEvent);
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
				OnUIASelectionChanged ();
			}

			EventHandler eh = (EventHandler)(Events [MouseLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMouseMove (MouseEventArgs mea)
		{
			if (this.Enabled) {
				MouseEventHandler eh = (MouseEventHandler)(Events [MouseMoveEvent]);
				if (eh != null)
					eh (this, mea);
			}
		}

		protected virtual void OnMouseUp (MouseEventArgs e)
		{
			if (this.Enabled) {
				this.is_pressed = false;
				this.Invalidate ();

				if (this.IsOnDropDown)
					if (!(this is ToolStripDropDownItem) || !(this as ToolStripDropDownItem).HasDropDownItems || (this as ToolStripDropDownItem).DropDown.Visible == false) {
						if ((this.Parent as ToolStripDropDown).OwnerItem != null)
							((this.Parent as ToolStripDropDown).OwnerItem as ToolStripDropDownItem).HideDropDown ();
						else
							(this.Parent as ToolStripDropDown).Hide ();
					}
						
				
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

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void OnOwnerFontChanged (EventArgs e)
		{
			this.CalculateAutoSize ();
			OnFontChanged (EventArgs.Empty);
		}

		void OnPaintInternal (PaintEventArgs e)
		{
			// Have the background rendered independently from OnPaint
			if (this.parent != null)
				this.parent.Renderer.DrawItemBackground (new ToolStripItemRenderEventArgs (e.Graphics, this));

			OnPaint (e);
		}

		protected virtual void OnPaint (PaintEventArgs e)
		{
			PaintEventHandler eh = (PaintEventHandler)(Events [PaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		// This is never called.
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnParentBackColorChanged (EventArgs e)
		{
		}
		
		protected virtual void OnParentChanged (ToolStrip oldParent, ToolStrip newParent)
		{
			this.text_size = TextRenderer.MeasureText (this.Text == null ? string.Empty : this.text, this.Font, Size.Empty, TextFormatFlags.HidePrefix);
			
			if (oldParent != null)
				oldParent.PerformLayout ();
				
			if (newParent != null)
				newParent.PerformLayout ();
		}

		protected internal virtual void OnParentEnabledChanged (EventArgs e)
		{
			this.OnEnabledChanged (e);
		}

		// This is never called.
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnParentForeColorChanged (EventArgs e)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void OnParentRightToLeftChanged (EventArgs e)
		{
			this.OnRightToLeftChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnQueryContinueDrag (QueryContinueDragEventArgs queryContinueDragEvent)
		{
			QueryContinueDragEventHandler eh = (QueryContinueDragEventHandler)(Events[QueryContinueDragEvent]);
			if (eh != null)
				eh (this, queryContinueDragEvent);
		}
		
		protected virtual void OnRightToLeftChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[RightToLeftChangedEvent]);
			if (eh != null)
				eh (this, e);
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

		protected internal virtual bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			return false;
		}
		
		protected internal virtual bool ProcessDialogKey (Keys keyData)
		{
			if (this.Selected && keyData == Keys.Enter) {
				this.FireEvent (EventArgs.Empty, ToolStripItemEventType.Click);
				return true;
			}
				
			return false;
		}
		
		// ProcessMnemonic will only be called if we are supposed to handle
		// it.  None of that fancy "thinking" needed!
		protected internal virtual bool ProcessMnemonic (char charCode)
		{
			ToolStripManager.SetActiveToolStrip (this.Parent, true);
			this.PerformClick ();
			return true;
		}
		
		protected internal virtual void SetBounds (Rectangle bounds)
		{
			if (this.bounds != bounds) {
				this.bounds = bounds;
				OnBoundsChanged ();
			}
		}
		
		protected virtual void SetVisibleCore (bool visible)
		{
			this.visible = visible;
			this.OnVisibleChanged (EventArgs.Empty);
			
			if (this.visible)
				BeginAnimation ();
			else
				StopAnimation ();
			this.Invalidate ();
		}
		#endregion

		#region Public Events
		static object AvailableChangedEvent = new object ();
		static object BackColorChangedEvent = new object ();
		static object ClickEvent = new object ();
		static object DisplayStyleChangedEvent = new object ();
		static object DoubleClickEvent = new object ();
		static object DragDropEvent = new object ();
		static object DragEnterEvent = new object ();
		static object DragLeaveEvent = new object ();
		static object DragOverEvent = new object ();
		static object EnabledChangedEvent = new object ();
		static object ForeColorChangedEvent = new object ();
		static object GiveFeedbackEvent = new object ();
		static object LocationChangedEvent = new object ();
		static object MouseDownEvent = new object ();
		static object MouseEnterEvent = new object ();
		static object MouseHoverEvent = new object ();
		static object MouseLeaveEvent = new object ();
		static object MouseMoveEvent = new object ();
		static object MouseUpEvent = new object ();
		static object OwnerChangedEvent = new object ();
		static object PaintEvent = new object ();
		static object QueryAccessibilityHelpEvent = new object ();
		static object QueryContinueDragEvent = new object ();
		static object RightToLeftChangedEvent = new object ();
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

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DragEventHandler DragDrop {
			add { Events.AddHandler (DragDropEvent, value); }
			remove { Events.RemoveHandler (DragDropEvent, value); }
		}

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DragEventHandler DragEnter {
			add { Events.AddHandler (DragEnterEvent, value); }
			remove { Events.RemoveHandler (DragEnterEvent, value); }
		}

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler DragLeave {
			add { Events.AddHandler (DragLeaveEvent, value); }
			remove { Events.RemoveHandler (DragLeaveEvent, value); }
		}

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DragEventHandler DragOver {
			add { Events.AddHandler (DragOverEvent, value); }
			remove { Events.RemoveHandler (DragOverEvent, value); }
		}

		public event EventHandler EnabledChanged {
			add { Events.AddHandler (EnabledChangedEvent, value); }
			remove {Events.RemoveHandler (EnabledChangedEvent, value); }
		}

		public event EventHandler ForeColorChanged {
			add { Events.AddHandler (ForeColorChangedEvent, value); }
			remove {Events.RemoveHandler (ForeColorChangedEvent, value); }
		}

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event GiveFeedbackEventHandler GiveFeedback {
			add { Events.AddHandler (GiveFeedbackEvent, value); }
			remove { Events.RemoveHandler (GiveFeedbackEvent, value); }
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

		[MonoTODO ("Event never raised")]
		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { Events.AddHandler (QueryAccessibilityHelpEvent, value); }
			remove { Events.RemoveHandler (QueryAccessibilityHelpEvent, value); }
		}

		[MonoTODO ("Event never raised")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event QueryContinueDragEventHandler QueryContinueDrag {
			add { Events.AddHandler (QueryContinueDragEvent, value); }
			remove { Events.RemoveHandler (QueryContinueDragEvent, value); }
		}

		public event EventHandler RightToLeftChanged {
			add { Events.AddHandler (RightToLeftChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftChangedEvent, value); }
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
			this.text_size = TextRenderer.MeasureText (this.Text == null ? string.Empty: this.text, this.Font, Size.Empty, TextFormatFlags.HidePrefix);

			// If our text is rotated, flip the width and height
			ToolStripTextDirection direction = this.TextDirection;
			
			if (direction == ToolStripTextDirection.Vertical270 || direction == ToolStripTextDirection.Vertical90)
				this.text_size = new Size (this.text_size.Height, this.text_size.Width);
			
			if (!this.auto_size || this is ToolStripControlHost)
				return;
			//this.text_size.Width += 6;

			Size final_size = this.CalculatePreferredSize (Size.Empty);

			if (final_size != this.Size) {
				this.bounds.Width = final_size.Width;
				if (this.parent != null)
					this.parent.PerformLayout ();
			}
		}

		internal virtual Size CalculatePreferredSize (Size constrainingSize)
		{
			if (!this.auto_size)
				return this.explicit_size;
				
			Size preferred_size = this.DefaultSize;

			switch (this.display_style) {
				case ToolStripItemDisplayStyle.Text:
					int width = text_size.Width + this.padding.Horizontal;
					int height = text_size.Height + this.padding.Vertical;
					preferred_size = new Size (width, height);
					break;
				case ToolStripItemDisplayStyle.Image:
					if (this.GetImageSize () == Size.Empty)
						preferred_size = this.DefaultSize;
					else {
						switch (this.image_scaling) {
							case ToolStripItemImageScaling.None:
								preferred_size = this.GetImageSize ();
								break;
							case ToolStripItemImageScaling.SizeToFit:
								if (this.parent == null)
									preferred_size = this.GetImageSize ();
								else
									preferred_size = this.parent.ImageScalingSize;
								break;
						}
					}
					break;
				case ToolStripItemDisplayStyle.ImageAndText:
					int width2 = text_size.Width + this.padding.Horizontal;
					int height2 = text_size.Height + this.padding.Vertical;

					if (this.GetImageSize () != Size.Empty) {
						Size image_size = this.GetImageSize ();
						
						if (this.image_scaling == ToolStripItemImageScaling.SizeToFit && this.parent != null)
							image_size = this.parent.ImageScalingSize;
						
						switch (this.text_image_relation) {
							case TextImageRelation.Overlay:
								width2 = Math.Max (width2, image_size.Width);
								height2 = Math.Max (height2, image_size.Height);
								break;
							case TextImageRelation.ImageAboveText:
							case TextImageRelation.TextAboveImage:
								width2 = Math.Max (width2, image_size.Width);
								height2 += image_size.Height;
								break;
							case TextImageRelation.ImageBeforeText:
							case TextImageRelation.TextBeforeImage:
								height2 = Math.Max (height2, image_size.Height);
								width2 += image_size.Width;
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
					if (this.Image != null && this.UseImageMargin)
						image_rect = AlignInRectangle (contentRectangle, GetImageSize (), this.image_align);
					break;
				case ToolStripItemDisplayStyle.ImageAndText:
					if (this.text != string.Empty && (this.Image == null || !this.UseImageMargin))
						text_rect = AlignInRectangle (contentRectangle, this.text_size, this.text_align);
					else if (this.text == string.Empty && (this.Image == null || !this.UseImageMargin))
						break;
					else if (this.text == string.Empty && this.Image != null)
						image_rect = AlignInRectangle (contentRectangle, GetImageSize (), this.image_align);
					else {
						Rectangle text_area;
						Rectangle image_area;

						switch (this.text_image_relation) {
							case TextImageRelation.Overlay:
								text_rect = AlignInRectangle (contentRectangle, this.text_size, this.text_align);
								image_rect = AlignInRectangle (contentRectangle, GetImageSize (), this.image_align);
								break;
							case TextImageRelation.ImageAboveText:
								text_area = new Rectangle (contentRectangle.Left, contentRectangle.Bottom - (text_size.Height - 4), contentRectangle.Width, text_size.Height - 4);
								image_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, contentRectangle.Width, contentRectangle.Height - text_area.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, GetImageSize (), this.image_align);
								break;
							case TextImageRelation.TextAboveImage:
								text_area = new Rectangle (contentRectangle.Left, contentRectangle.Top, contentRectangle.Width, text_size.Height - 4);
								image_area = new Rectangle (contentRectangle.Left, text_area.Bottom, contentRectangle.Width, contentRectangle.Height - text_area.Height);

								text_rect = AlignInRectangle (text_area, this.text_size, this.text_align);
								image_rect = AlignInRectangle (image_area, GetImageSize (), this.image_align);
								break;
							case TextImageRelation.ImageBeforeText:
								LayoutTextBeforeOrAfterImage (contentRectangle, false, text_size, GetImageSize (), text_align, image_align, out text_rect, out image_rect);
								break;
							case TextImageRelation.TextBeforeImage:
								LayoutTextBeforeOrAfterImage (contentRectangle, true, text_size, GetImageSize (), text_align, image_align, out text_rect, out image_rect);
								break;
						}
					}
					break;
			}
		}

		private static Font DefaultFont { get { return new Font ("Tahoma", 8.25f); } }
		
		internal virtual ToolStripTextDirection DefaultTextDirection { get { return ToolStripTextDirection.Inherit; } }

		internal virtual void Dismiss (ToolStripDropDownCloseReason reason)
		{
			if (is_selected) {
				this.is_selected = false;
				this.Invalidate ();
				OnUIASelectionChanged ();
			}
		}
		
		internal virtual ToolStrip GetTopLevelToolStrip ()
		{
			if (this.Parent != null)
				return this.Parent.GetTopLevelToolStrip ();
				
			return null;
		}

		private void LayoutTextBeforeOrAfterImage (Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, ContentAlignment textAlign, ContentAlignment imageAlign, out Rectangle textRect, out Rectangle imageRect)
		{
			int element_spacing = 0;	// Spacing between the Text and the Image
			int total_width = textSize.Width + element_spacing + imageSize.Width;
			int excess_width = totalArea.Width - total_width;
			int offset = 0;
			
			Rectangle final_text_rect;
			Rectangle final_image_rect;

			HorizontalAlignment h_text = GetHorizontalAlignment (textAlign);
			HorizontalAlignment h_image = GetHorizontalAlignment (imageAlign);
			
			if (h_image == HorizontalAlignment.Left)
				offset = 0;
			else if (h_image == HorizontalAlignment.Right && h_text == HorizontalAlignment.Right)
				offset = excess_width;
			else if (h_image == HorizontalAlignment.Center && (h_text == HorizontalAlignment.Left || h_text == HorizontalAlignment.Center))
				offset += (int)(excess_width / 3);
			else
				offset += (int)(2 * (excess_width / 3));
				
			if (textFirst) {
				final_text_rect = new Rectangle (totalArea.Left + offset, AlignInRectangle (totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
				final_image_rect = new Rectangle (final_text_rect.Right + element_spacing, AlignInRectangle (totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
			} else {
				final_image_rect = new Rectangle (totalArea.Left + offset, AlignInRectangle (totalArea, imageSize, imageAlign).Top, imageSize.Width, imageSize.Height);
				final_text_rect = new Rectangle (final_image_rect.Right + element_spacing, AlignInRectangle (totalArea, textSize, textAlign).Top, textSize.Width, textSize.Height);
			}
			
			textRect = final_text_rect;
			imageRect = final_image_rect;
		}
		
		private HorizontalAlignment GetHorizontalAlignment (ContentAlignment align)
		{
			switch (align) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					return HorizontalAlignment.Left;
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					return HorizontalAlignment.Center;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					return HorizontalAlignment.Right;
			}
			
			return HorizontalAlignment.Left;
		}
		
		internal Size GetImageSize ()
		{
			// Get the actual size of our internal image -or-
			// Get the ImageList.ImageSize if we are using ImageLists
			if (this.image_scaling == ToolStripItemImageScaling.None) {
				if (this.image != null)
					return image.Size;
					
				if (this.image_index >= 0 || !string.IsNullOrEmpty (this.image_key))
					if (this.owner != null && this.owner.ImageList != null)
						return this.owner.ImageList.ImageSize;
			} else {
				// If we have an image and a parent, return ImageScalingSize
				if (this.Parent == null)
					return Size.Empty;
					
				if (this.image != null)
					return this.Parent.ImageScalingSize;

				if (this.image_index >= 0 || !string.IsNullOrEmpty (this.image_key))
					if (this.owner != null && this.owner.ImageList != null)
						return this.Parent.ImageScalingSize;
			}
			
			return Size.Empty;
		}
		
		internal string GetToolTip ()
		{
			if (this.auto_tool_tip && string.IsNullOrEmpty (this.tool_tip_text))
				return this.Text;
				
			return this.tool_tip_text;
		}
		
		internal void FireEvent (EventArgs e, ToolStripItemEventType met)
		{
			// If we're disabled, don't fire any of these events, except Paint
			if (!this.Enabled && met != ToolStripItemEventType.Paint)
				return;
				
			switch (met) {
				case ToolStripItemEventType.MouseUp:
					this.HandleClick (((MouseEventArgs)e).Clicks, e);
					this.OnMouseUp ((MouseEventArgs)e);
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
					this.OnPaintInternal ((PaintEventArgs)e);
					break;
				case ToolStripItemEventType.Click:
					this.HandleClick (1, e);
					break;
			}
		}
		
		internal virtual void HandleClick (int mouse_clicks, EventArgs e)
		{
			if (Parent == null)
				return;
			this.Parent.HandleItemClick (this);
			if (mouse_clicks == 2 && double_click_enabled)
				this.OnDoubleClick (e);
			else
				this.OnClick (e);
		}
		
		internal virtual void SetPlacement (ToolStripItemPlacement placement)
		{
			this.placement = placement;
		}

		private void BeginAnimation ()
		{
			if (image != null && ImageAnimator.CanAnimate (image)) {
				frame_handler = new EventHandler (OnAnimateImage);
				ImageAnimator.Animate (image, frame_handler);
			}
		}

		private void OnAnimateImage (object sender, EventArgs e)
		{
			// This is called from a worker thread,BeginInvoke is used
			// so the control is updated from the correct thread

			// Check if we have a handle again, since it may have gotten
			// destroyed since the last time we checked.
			if (Parent == null || !Parent.IsHandleCreated)
				return;

			Parent.BeginInvoke (new EventHandler (UpdateAnimatedImage), new object[] { this, e });
		}

		private void StopAnimation ()
		{
			if (frame_handler == null)
				return;
				
			ImageAnimator.StopAnimate (image, frame_handler);
			frame_handler = null;
		}

		private void UpdateAnimatedImage (object sender, EventArgs e)
		{
			// Check if we have a handle again, since it may have gotten
			// destroyed since the last time we checked.
			if (Parent == null || !Parent.IsHandleCreated)
				return;

			ImageAnimator.UpdateFrames (image);
			Invalidate ();
		}

		internal bool ShowMargin {
			get {
				if (!this.IsOnDropDown)
					return true;

				if (!(this.Owner is ToolStripDropDownMenu))
					return false;

				ToolStripDropDownMenu tsddm = (ToolStripDropDownMenu)this.Owner;

				return tsddm.ShowCheckMargin || tsddm.ShowImageMargin;
			}
		}

		internal bool UseImageMargin {
			get {
				if (!this.IsOnDropDown)
					return true;

				if (!(this.Owner is ToolStripDropDownMenu))
					return false;

				ToolStripDropDownMenu tsddm = (ToolStripDropDownMenu)this.Owner;

				return tsddm.ShowImageMargin || tsddm.ShowCheckMargin;
			}
		}

		internal virtual bool InternalVisible {
			get { return this.visible; }
			set { this.visible = value; Invalidate (); }
		}

		internal ToolStrip InternalOwner {
			set {
				if (this.owner != value) {
					this.owner = value;
					if (this.owner != null)
						this.CalculateAutoSize ();
					OnOwnerChanged (EventArgs.Empty);
				}
			}
		}

		internal Point Location {
			get { return this.bounds.Location; }
			set {
				if (this.bounds.Location != value) {
					this.bounds.Location = value;
					this.OnLocationChanged (EventArgs.Empty);
				}
			}
		}

		internal int Top {
			get { return this.bounds.Y; }
			set {
				if (this.bounds.Y != value) {
					this.bounds.Y = value;
					this.OnLocationChanged (EventArgs.Empty);
				}
			}
		}

		internal int Left {
			get { return this.bounds.X; }
			set {
				if (this.bounds.X != value) {
					this.bounds.X = value;
					this.OnLocationChanged (EventArgs.Empty);
				}
			}
		}
		
		internal int Right { get { return this.bounds.Right; } }
		internal int Bottom { get { return this.bounds.Bottom; } }
		#endregion

		#region IDropTarget Members
		void IDropTarget.OnDragDrop (DragEventArgs dragEvent)
		{
			OnDragDrop (dragEvent);
		}

		void IDropTarget.OnDragEnter (DragEventArgs dragEvent)
		{
			OnDragEnter (dragEvent);
		}

		void IDropTarget.OnDragLeave (EventArgs e)
		{
			OnDragLeave (e);
		}

		void IDropTarget.OnDragOver (DragEventArgs dragEvent)
		{
			OnDragOver (dragEvent);
		}
		#endregion

		#region UIA Framework: Methods, Properties and Events

		static object UIASelectionChangedEvent = new object ();

		internal event EventHandler UIASelectionChanged {
			add { Events.AddHandler (UIASelectionChangedEvent, value); }
			remove { Events.RemoveHandler (UIASelectionChangedEvent, value); }
		}

		internal void OnUIASelectionChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIASelectionChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		#endregion

		[ComVisible (true)]
		public class ToolStripItemAccessibleObject : AccessibleObject
		{
			internal ToolStripItem owner_item;
			
			public ToolStripItemAccessibleObject (ToolStripItem ownerItem)
			{
				if (ownerItem == null)
					throw new ArgumentNullException ("ownerItem");
					
				this.owner_item = ownerItem;
				base.default_action = string.Empty;
				base.keyboard_shortcut = string.Empty;
				base.name = string.Empty;
				base.value = string.Empty;
			}

			#region Public Properties
			public override Rectangle Bounds {
				get {
					return owner_item.Visible ? owner_item.Bounds : Rectangle.Empty;
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
						return owner_item.Text;
						
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

			public override int GetHelpTopic (out string fileName)
			{
				return base.GetHelpTopic (out fileName);
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection)
			{
				return base.Navigate (navigationDirection);
			}

			public override string ToString ()
			{
				return string.Format ("ToolStripItemAccessibleObject: Owner = {0}", owner_item.ToString());
			}
			#endregion
		}
	}

	internal class NoneExcludedImageIndexConverter : ImageIndexConverter
	{
	}
}
