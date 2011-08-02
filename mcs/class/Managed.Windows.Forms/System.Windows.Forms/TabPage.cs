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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent("Click")]
	[DesignTimeVisible(false)]
	[DefaultProperty("Text")]
	[Designer("System.Windows.Forms.Design.TabPageDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItem(false)]
	public class TabPage : Panel {
		#region Fields
		private int imageIndex = -1;
		private string imageKey;
		private string tooltip_text = String.Empty;
		private Rectangle tab_bounds;
		private int row;
		private bool use_visual_style_back_color;
		#endregion	// Fields
		
		#region Public Constructors
		public TabPage ()
		{
			Visible = true;

			SetStyle (ControlStyles.CacheText, true);
		}

		public TabPage (string text) : base ()
		{
			base.Text = text;
		}

		#endregion	// Public Constructors

		#region .NET 2.0 Public Instance Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[Localizable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override AutoSizeMode AutoSizeMode {
			get { return base.AutoSizeMode; }
			set { base.AutoSizeMode = value; }
		}

		[Browsable (false)]
		[DefaultValue ("{Width=0, Height=0}")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Size PreferredSize {
			get { return base.PreferredSize; }
		}
		
		[DefaultValue (false)]
		public bool UseVisualStyleBackColor {
			get { return use_visual_style_back_color; }
			set { use_visual_style_back_color = value; }
		}

		public override Color BackColor {
			get { return base.BackColor; }
			set { use_visual_style_back_color = false; base.BackColor = value; }
		}
		#endregion

		#region Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue(-1)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(ImageIndexConverter))]
		public int ImageIndex {
			get { return imageIndex; }
			set {
				if (imageIndex == value)
					return;
				imageIndex = value;
				UpdateOwner ();
			}
		}

		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter (typeof (ImageKeyConverter))]
		public string ImageKey
		{
			get { return imageKey; }
			set {
				imageKey = value;
				TabControl control = this.Parent as TabControl;
				if (control != null) {
					ImageIndex = control.ImageList.Images.IndexOfKey (imageKey);
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Always)]
		[Browsable(true)]
		[Localizable(true)]
		public override string Text {
			get { return base.Text; }
			set {
				if (value == base.Text)
					return;
				base.Text = value;
				UpdateOwner ();
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		public string ToolTipText {
			get { return tooltip_text; }
			set {
				if (value == null)
					value = String.Empty;
				tooltip_text = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool Visible {
			get { return base.Visible; }
			set { /* according to MS docs we can ignore this */ }
		}

		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static TabPage GetTabPageOfComponent (object comp)
		{
			Control control = comp as Control;
			if (control == null)
				return null;
			control = control.Parent;
			while (control != null) {
				if (control is TabPage)
					break;
				control = control.Parent;
			}
			return control as TabPage;
		}

		#endregion	// Public Static Methods

		#region Public Instance Methods
		public override string ToString ()
		{
			return "TabPage: {" + Text + "}";
		}

		#endregion	// Public Instance Methods

		#region	Internal & Private Methods and Properties
		internal Rectangle TabBounds {
			get { return tab_bounds; }
			set { tab_bounds = value; }
		}

		internal int Row {
			get { return row; }
			set { row = value; }
		}

		private void UpdateOwner ()
		{
			if (Owner != null) {
				Owner.Redraw ();
			}
		}

		private TabControl Owner {
			get { return base.Parent as TabControl; }
		}

		internal void SetVisible (bool value)
		{
			base.Visible = value;
		}

		#endregion	// Internal & Private Methods and Properties

		#region Protected Instance Methods
		protected override ControlCollection CreateControlsInstance ()
		{
			return new TabPageControlCollection (this);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) 
		{
			if (Owner != null && Owner.IsHandleCreated) {
				Rectangle display = Owner.DisplayRectangle;

				base.SetBoundsCore (display.X, display.Y,
							display.Width, display.Height,
							BoundsSpecified.All);
			} else {
				base.SetBoundsCore (x, y, width, height, specified);
			}
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { base.EnabledChanged += value; }
			remove { base.EnabledChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler LocationChanged {
			add { base.LocationChanged += value; }
			remove { base.LocationChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler VisibleChanged {
			add { base.VisibleChanged += value; }
			remove { base.VisibleChanged -= value; }
		}

		#endregion	// Events

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Point Location {
			get {
				return base.Location;
			}

			set {
				base.Location = value;
			}
		}

		#region Class TabPageControlCollection
		[ComVisible (false)]
		public class TabPageControlCollection : ControlCollection {

			//private TabPage owner;

			public TabPageControlCollection (TabPage owner) : base (owner)
			{
				//this.owner = owner;
			}

			public override void Add (Control value)
			{
				base.Add (value);
			}
		}
		#endregion	// Class TabPageControlCollection

	}

	
}
