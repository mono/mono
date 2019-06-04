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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer ("System.Windows.Forms.Design.ButtonBaseDesigner, " + Consts.AssemblySystem_Design,
		   "System.ComponentModel.Design.IDesigner")]
	public abstract class ButtonBase : Control
	{
		#region Local Variables
		private FlatStyle		flat_style;
		private int			image_index;
		internal Image			image;
		internal ImageList		image_list;
		private ContentAlignment	image_alignment;
		internal ContentAlignment	text_alignment;
		private bool			is_default;
		internal bool			is_pressed;
//		private bool			enter_state;
		internal StringFormat		text_format;
		internal bool 			paint_as_acceptbutton;
		
		// Properties are 2.0, but variables used in 1.1 for common drawing code
		private bool			auto_ellipsis;
		private FlatButtonAppearance	flat_button_appearance;
		private string			image_key;
		private TextImageRelation	text_image_relation;
		private TextFormatFlags		text_format_flags;
		private bool			use_mnemonic;
		private bool			use_visual_style_back_color;
		#endregion	// Local Variables

		#region Public Constructors
		protected ButtonBase() : base()
		{
			flat_style	= FlatStyle.Standard;
			flat_button_appearance = new FlatButtonAppearance (this);
			this.image_key = string.Empty;
			this.text_image_relation = TextImageRelation.Overlay;
			this.use_mnemonic = true;
			use_visual_style_back_color = true;
			image_index	= -1;
			image		= null;
			image_list	= null;
			image_alignment	= ContentAlignment.MiddleCenter;
			ImeMode         = ImeMode.Disable;
			text_alignment	= ContentAlignment.MiddleCenter;
			is_default	= false;
			is_pressed	= false;
			text_format	= new StringFormat();
			text_format.Alignment = StringAlignment.Center;
			text_format.LineAlignment = StringAlignment.Center;
			text_format.HotkeyPrefix = HotkeyPrefix.Show;
			text_format.FormatFlags |= StringFormatFlags.LineLimit;

			text_format_flags = TextFormatFlags.HorizontalCenter;
			text_format_flags |= TextFormatFlags.VerticalCenter;
			text_format_flags |= TextFormatFlags.TextBoxControl;

			SetStyle (ControlStyles.ResizeRedraw | 
				ControlStyles.Opaque | 
				ControlStyles.UserMouse | 
				ControlStyles.SupportsTransparentBackColor | 
				ControlStyles.CacheText |
				ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.StandardClick, false);

			can_cache_preferred_size = true;
		}
		#endregion	// Public Constructors

		#region Public Properties
		[Browsable (true)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[MWFCategory("Behavior")]
		public bool AutoEllipsis {
			get { return this.auto_ellipsis; }
			set
			{
				if (this.auto_ellipsis != value) {
					this.auto_ellipsis = value;

					if (this.auto_ellipsis) {
						text_format_flags |= TextFormatFlags.EndEllipsis;
						text_format_flags &= ~TextFormatFlags.WordBreak;
					} else {
						text_format_flags &= ~TextFormatFlags.EndEllipsis;
						text_format_flags |= TextFormatFlags.WordBreak;
					}

					if (Parent != null)
						Parent.PerformLayout (this, "AutoEllipsis");
					this.Invalidate ();
				}
			}
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[MWFCategory("Layout")]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (true)]
		[MWFCategory("Appearance")]
		public FlatButtonAppearance FlatAppearance {
			get { return flat_button_appearance; }
		}

		[Localizable(true)]
		[DefaultValue(FlatStyle.Standard)]
		[MWFDescription("Determines look of button"), MWFCategory("Appearance")]
		public FlatStyle FlatStyle {
			get { return flat_style; }
			set { 
				if (flat_style != value) {
					flat_style = value;

					if (Parent != null)
						Parent.PerformLayout (this, "FlatStyle");
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[MWFDescription("Sets image to be displayed on button face"), MWFCategory("Appearance")]
		public Image Image {
			get {
				if (this.image != null)
					return this.image;

				if (this.image_index >= 0)
					if (this.image_list != null)
						return this.image_list.Images[this.image_index];

				if (!string.IsNullOrEmpty (this.image_key))
					if (this.image_list != null)
						return this.image_list.Images[this.image_key];
				return null;
			}
			set {
				if (this.image != value) {
					this.image = value;
					this.image_index = -1;
					this.image_key = string.Empty;
					this.image_list = null;

					if (this.AutoSize && this.Parent != null)
						this.Parent.PerformLayout (this, "Image");

					Invalidate ();
				}
			}
		}

		internal bool ShouldSerializeImage ()
		{
			return this.Image != null;
		}

		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleCenter)]
		[MWFDescription("Sets the alignment of the image to be displayed on button face"), MWFCategory("Appearance")]
		public ContentAlignment ImageAlign {
			get { return image_alignment; }
			set {
				if (image_alignment != value) {
					image_alignment = value;
					Invalidate ();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(-1)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter(typeof(ImageIndexConverter))]
		[MWFDescription("Index of image to display, if ImageList is used for button face images"), MWFCategory("Appearance")]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int ImageIndex {
			get {
				if (image_list == null)
					return -1;

				return image_index;
			}
			set {
				if (this.image_index != value) {
					this.image_index = value;
					this.image = null;
					this.image_key = string.Empty;
					Invalidate ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (ImageKeyConverter))]
		[MWFCategory("Appearance")]
		public string ImageKey {
			get { return this.image_key; }
			set {
				if (this.image_key != value) {
					this.image = null;
					this.image_index = -1;
					this.image_key = value;
					this.Invalidate ();
				}
			}
		}

		[DefaultValue(null)]
		[MWFDescription("ImageList used for ImageIndex"), MWFCategory("Appearance")]
		[RefreshProperties (RefreshProperties.Repaint)]
		public ImageList ImageList {
			get { return image_list; }
			set {
				if (image_list != value) {
					image_list = value;
				
					if (value != null && image != null)
						image = null;
				
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[SettingsBindable (true)]
		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleCenter)]
		[MWFDescription("Alignment for button text"), MWFCategory("Appearance")]
		public virtual ContentAlignment TextAlign {
			get { return text_alignment; }
			set {
				if (text_alignment != value) {
					text_alignment = value;

					text_format_flags &= ~TextFormatFlags.Bottom;
					text_format_flags &= ~TextFormatFlags.Top;
					text_format_flags &= ~TextFormatFlags.Left;
					text_format_flags &= ~TextFormatFlags.Right;
					text_format_flags &= ~TextFormatFlags.HorizontalCenter;
					text_format_flags &= ~TextFormatFlags.VerticalCenter;
					
					switch (text_alignment) {
						case ContentAlignment.TopLeft:
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Near;
							break;

						case ContentAlignment.TopCenter:
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Near;
							text_format_flags |= TextFormatFlags.HorizontalCenter;
							break;

						case ContentAlignment.TopRight:
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Near;
							text_format_flags |= TextFormatFlags.Right;
							break;

						case ContentAlignment.MiddleLeft:
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Center;
							text_format_flags |= TextFormatFlags.VerticalCenter;
							break;

						case ContentAlignment.MiddleCenter:
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Center;
							text_format_flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
							break;

						case ContentAlignment.MiddleRight:
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Center;
							text_format_flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
							break;

						case ContentAlignment.BottomLeft:
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Far;
							text_format_flags |= TextFormatFlags.Bottom;
							break;

						case ContentAlignment.BottomCenter:
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Far;
							text_format_flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
							break;

						case ContentAlignment.BottomRight:
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Far;
							text_format_flags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
							break;
					}
					
					Invalidate();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (TextImageRelation.Overlay)]
		[MWFCategory("Appearance")]
		public TextImageRelation TextImageRelation {
			get { return this.text_image_relation; }
			set {
				if (!Enum.IsDefined (typeof (TextImageRelation), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextImageRelation", value));

				if (this.text_image_relation != value) {
					this.text_image_relation = value;
					
					if (this.AutoSize && this.Parent != null)
						this.Parent.PerformLayout (this, "TextImageRelation");
					
					this.Invalidate ();
				}
			}
		}

		[DefaultValue (false)]
		[MWFCategory("Behavior")]
		public bool UseCompatibleTextRendering {
			get { return use_compatible_text_rendering; }
			set {
				if (use_compatible_text_rendering != value) {
					use_compatible_text_rendering = value;
					if (Parent != null)
						Parent.PerformLayout (this, "UseCompatibleTextRendering");
					Invalidate ();
				}
			}
		}
		
		[DefaultValue (true)]
		[MWFCategory("Appearance")]
		public bool UseMnemonic {
			get { return this.use_mnemonic; }
			set {
				if (this.use_mnemonic != value) {
					this.use_mnemonic = value;

					if (this.use_mnemonic)
						text_format_flags &= ~TextFormatFlags.NoPrefix;
					else
						text_format_flags |= TextFormatFlags.NoPrefix;

					this.Invalidate ();
				}
			}
		}

		[MWFCategory("Appearance")]
		public bool UseVisualStyleBackColor {
			get { return use_visual_style_back_color; }
			set {
				if (use_visual_style_back_color != value) {
					use_visual_style_back_color = value;
					Invalidate ();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ButtonBaseDefaultSize; }
		}

		protected internal bool IsDefault {
			get { return is_default; }
			set {
				if (is_default != value) {
					is_default = value;
					Invalidate ();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Methods
		// The base calls into GetPreferredSizeCore, which we will override in our subclasses
		public override Size GetPreferredSize (Size proposedSize)
		{
			return base.GetPreferredSize (proposedSize);
		}
		#endregion
		
		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ButtonBaseAccessibleObject (this);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnGotFocus (EventArgs e)
		{
			Invalidate ();
			base.OnGotFocus (e);
		}

		protected override void OnKeyDown (KeyEventArgs kevent)
		{
			if (kevent.KeyData == Keys.Space) {
				is_pressed = true;
				Invalidate ();
				kevent.Handled = true;
			}
			
			base.OnKeyDown (kevent);
		}

		protected override void OnKeyUp (KeyEventArgs kevent)
		{
			if (kevent.KeyData == Keys.Space) {
				is_pressed = false;
				Invalidate ();
				OnClick (EventArgs.Empty);
				kevent.Handled = true;
			}
			
			base.OnKeyUp (kevent);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			Invalidate ();
			base.OnLostFocus (e);
		}

		protected override void OnMouseDown (MouseEventArgs mevent)
		{
			if ((mevent.Button & MouseButtons.Left) != 0) {
				is_pressed = true;
				Invalidate ();
			}

			base.OnMouseDown (mevent);
		}

		protected override void OnMouseEnter (EventArgs eventargs)
		{
			is_entered = true;
			Invalidate ();
			base.OnMouseEnter (eventargs);
		}

		protected override void OnMouseLeave (EventArgs eventargs)
		{
			is_entered = false;
			Invalidate ();
			base.OnMouseLeave (eventargs);
		}

		protected override void OnMouseMove (MouseEventArgs mevent) {
			bool inside = false;
			bool redraw = false;

			if (ClientRectangle.Contains (mevent.Location))
				inside = true;

			// If the button was pressed and we leave, release the button press and vice versa
			if ((mevent.Button & MouseButtons.Left) != 0) {
				if (this.Capture && (inside != is_pressed)) {
					is_pressed = inside;
					redraw = true;
				}
			}

			if (is_entered != inside) {
				is_entered = inside;
				redraw = true;
			}

			if (redraw)
				Invalidate ();

			base.OnMouseMove (mevent);
		}

		protected override void OnMouseUp (MouseEventArgs mevent)
		{
			if (this.Capture && ((mevent.Button & MouseButtons.Left) != 0)) {
				this.Capture = false;

				if (is_pressed) {
					is_pressed = false;
					Invalidate ();
				} else if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
					Invalidate ();
				}

				if (ClientRectangle.Contains (mevent.Location))
					if (!ValidationFailed) {
						OnClick (EventArgs.Empty);
						OnMouseClick (mevent);
					}
			}
			
			base.OnMouseUp (mevent);
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			Draw (pevent);
			base.OnPaint (pevent);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			Invalidate ();
			base.OnTextChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			if (!Visible) {
				is_pressed = false;
				is_entered = false;
			}
			
			base.OnVisibleChanged (e);
		}

		protected void ResetFlagsandPaint ()
		{
			// Nothing to do; MS internal
			// Should we do Invalidate (); ?
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
				case Msg.WM_LBUTTONDBLCLK: {
					HaveDoubleClick ();
					break;
				}

				case Msg.WM_MBUTTONDBLCLK: {
					HaveDoubleClick ();
					break;
				}

				case Msg.WM_RBUTTONDBLCLK: {
					HaveDoubleClick ();
					break;
				}
			}
			
			base.WndProc (ref m);
		}
		#endregion	// Public Instance Properties

		#region	Public Events
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}
		#endregion	// Events

		#region Internal Properties
		internal ButtonState ButtonState {
			get {
				ButtonState ret = ButtonState.Normal;

				if (Enabled) {
					// Popup style is only followed as long as the mouse isn't "in" the control
					if (is_entered) {
						if (flat_style == FlatStyle.Flat) {
							ret |= ButtonState.Flat;
						}
					} else {
						if (flat_style == FlatStyle.Flat || flat_style == FlatStyle.Popup) {
							ret |= ButtonState.Flat;
						}
					}

					if (is_entered && is_pressed) {
						ret |= ButtonState.Pushed;
					}
				} else {
					ret |= ButtonState.Inactive;
					if ((flat_style == FlatStyle.Flat) || (flat_style == FlatStyle.Popup)) {
						ret |= ButtonState.Flat;
					}
				}
				return ret;
			}
		}

		internal bool Pressed {
			get { return this.is_pressed; }
		}
		
		// The flags to be used for MeasureText and DrawText
		internal TextFormatFlags TextFormatFlags {
			get { return this.text_format_flags; }
		}
		#endregion
		
		#region Internal Methods
		// Derived classes should override Draw method and we dont want
		// to break the control signature, hence this approach.
		internal virtual void Draw (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawButtonBase (pevent.Graphics, pevent.ClipRectangle, this);
		}
		
		internal virtual void HaveDoubleClick ()
		{
			// override me
		}

		internal override void OnPaintBackgroundInternal (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}
		#endregion	// Internal Methods

		#region ButtonBaseAccessibleObject sub-class
		[ComVisible (true)]
		public class ButtonBaseAccessibleObject : ControlAccessibleObject
		{
			#region ButtonBaseAccessibleObject Local Variables
			private new Control owner;
			#endregion	// ButtonBaseAccessibleObject Local Variables

			#region ButtonBaseAccessibleObject Constructors
			public ButtonBaseAccessibleObject (Control owner) : base (owner)
			{
				if (owner == null)
					throw new ArgumentNullException ("owner");
					
				this.owner = owner;
				default_action = "Press";
				role = AccessibleRole.PushButton;
			}
			#endregion	// ButtonBaseAccessibleObject Constructors

			#region Public Properties
			public override AccessibleStates State {
				get { return base.State; }
			}
			#endregion
			
			#region ButtonBaseAccessibleObject Methods
			public override void DoDefaultAction ()
			{
				((ButtonBase)owner).OnClick (EventArgs.Empty);
			}
			#endregion	// ButtonBaseAccessibleObject Methods
		}
		#endregion	// ButtonBaseAccessibleObject sub-class
	}
}
