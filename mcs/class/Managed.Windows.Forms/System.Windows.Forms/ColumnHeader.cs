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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Ravindra (rkumar@novell.com)
//


// COMPLETE


using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
#if NET_2_0
	[TypeConverter (typeof (ColumnHeaderConverter))]
#endif
	public class ColumnHeader : Component, ICloneable
	{
		#region Instance Variables
		private StringFormat format = new StringFormat ();
		private string text = "ColumnHeader";
		private HorizontalAlignment text_alignment = HorizontalAlignment.Left;
		private int width = ThemeEngine.Current.ListViewDefaultColumnWidth;
#if NET_2_0
		private int image_index = -1;
		private string image_key = String.Empty;
		private string name = String.Empty;
		private object tag;
#endif
		private int display_index = -1;

		// internal variables
		Rectangle column_rect = Rectangle.Empty;
		bool pressed = false;
		ListView owner;
		#endregion	// Instance Variables

		#region Internal Constructor
		internal ColumnHeader (ListView owner, string text,
				       HorizontalAlignment alignment, int width)
		{
			this.owner = owner;
			this.text = text;
			this.width = width;
			this.text_alignment = alignment;
			CalcColumnHeader ();
		}

#if NET_2_0
		internal ColumnHeader (string key, string text, int width, HorizontalAlignment textAlign)
		{
			Name = key;
			Text = text;
			this.width = width;
			this.text_alignment = textAlign;
			CalcColumnHeader ();
		}
#endif
		#endregion	// Internal Constructor

		#region Public Constructors
		public ColumnHeader () { }

#if NET_2_0
		public ColumnHeader (int imageIndex)
		{
			ImageIndex = imageIndex;
		}

		public ColumnHeader (string imageKey)
		{
			ImageKey = imageKey;
		}
#endif
		#endregion	// Public Constructors

		#region Private Internal Methods Properties
		internal bool Pressed {
			get { return pressed; }
			set { pressed = value; }
		}

		internal int X {
			get { return column_rect.X; }
			set { column_rect.X = value; }
		}

		internal int Y {
			get { return column_rect.Y; }
			set { column_rect.Y = value; }
		}

		internal int Wd {
			get { return column_rect.Width; }
			set { column_rect.Width = value; }
		}

		internal int Ht {
			get { return column_rect.Height; }
			set { column_rect.Height = value; }
		}

		internal Rectangle Rect {
			get { return column_rect; }
			set { column_rect = value; }
		}

		internal StringFormat Format {
			get { return format; }
		}

		internal int InternalDisplayIndex {
			get { return display_index; }
			set { display_index = value; }
		}

		internal void CalcColumnHeader ()
		{
			if (text_alignment == HorizontalAlignment.Center)
				format.Alignment = StringAlignment.Center;
			else if (text_alignment == HorizontalAlignment.Right)
				format.Alignment = StringAlignment.Far;
			else
				format.Alignment = StringAlignment.Near;
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisCharacter;
			// text is wrappable only in LargeIcon and SmallIcon views
			format.FormatFlags = StringFormatFlags.NoWrap;

			if (owner != null)
				column_rect.Height = ThemeEngine.Current.ListViewGetHeaderHeight (owner, owner.Font);
			else
				column_rect.Height = ThemeEngine.Current.ListViewGetHeaderHeight (null, ThemeEngine.Current.DefaultFont);

			if (width >= 0)
				column_rect.Width = width;
			else if (Index != -1) {
				column_rect.Width = owner.GetChildColumnSize (Index).Width;
				width = column_rect.Width;
			} else
				column_rect.Width = 0;
		}

		internal void SetListView (ListView list_view)
		{
			owner = list_view;
		}

		#endregion	// Private Internal Methods Properties

		#region Public Instance Properties

#if NET_2_0
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int DisplayIndex {
			get {
				if (owner == null)
					return display_index;
					
				return owner.GetReorderedColumnIndex (this);
			}
			set {
				if (owner == null) {
					display_index = value;
					return;
				}
				if (value < 0 || value >= owner.Columns.Count)
					throw new ArgumentOutOfRangeException ("DisplayIndex");

				owner.ReorderColumn (this, value, false);
			}
		}

		[DefaultValue (-1)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (ImageIndexConverter))]
		public int ImageIndex {
			get {
				return image_index;
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("ImageIndex");

				image_index = value;
				image_key = String.Empty;

				if (owner != null)
					owner.header_control.Invalidate ();
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (ImageKeyConverter))]
		public string ImageKey {
			get {
				return image_key;
			}
			set {
				image_key = value == null ? String.Empty : value;
				image_index = -1;

				if (owner != null)
					owner.header_control.Invalidate ();
			}
		}

		[Browsable (false)]
		public ImageList ImageList {
			get {
				if (owner == null)
					return null;

				return owner.SmallImageList;
			}
		}
#endif

		[Browsable (false)]
		public int Index {
			get {
				if (owner != null && owner.Columns != null
				    && owner.Columns.Contains (this)) {
					return owner.Columns.IndexOf (this);
				}
				return -1;
			}
		}

		[Browsable (false)]
		public ListView ListView {
			get { return owner; }
		}

#if NET_2_0
		[Browsable (false)]
		public string Name {
			get {
				return name;
			}
			set {
				name = value == null ? String.Empty : value;
			}
		}

		[DefaultValue (null)]
		[BindableAttribute (true)]
		[LocalizableAttribute (false)]
		[TypeConverter (typeof (StringConverter))]
		public object Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}
#endif

		[Localizable (true)]
		public string Text {
			get { return text; }
			set {
				if (text != value) {
					text = value;
					if (owner != null)
						owner.Redraw (true);

#if NET_2_0
					// UIA Framework: Raising Value changed event
					OnUIATextChanged ();
#endif
				}
			}
		}

		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign {
			get { return text_alignment; }
			set {
				text_alignment = value;
				if (owner != null)
					owner.Redraw (true);
			}
		}

		[DefaultValue (60)]
		[Localizable (true)]
		public int Width {
			get { return width; }
			set {
				if (width != value) {
					width = value;
					if (owner != null) {
						owner.Redraw (true);
						owner.RaiseColumnWidthChanged (this);
					}
				}
			}
		}
		#endregion // Public Instance Properties

		#region Public Methods
#if NET_2_0
		public void AutoResize (ColumnHeaderAutoResizeStyle headerAutoResize)
		{
			switch (headerAutoResize) {
				case ColumnHeaderAutoResizeStyle.None:
					break;
				case ColumnHeaderAutoResizeStyle.ColumnContent:
					Width = -1;
					break;
				case ColumnHeaderAutoResizeStyle.HeaderSize:
					Width = -2;
					break;
				default:
					throw new InvalidEnumArgumentException ("headerAutoResize", (int) headerAutoResize,
							typeof (ColumnHeaderAutoResizeStyle));
			}
		}
#endif

		public object Clone ()
		{
			ColumnHeader columnHeader = new ColumnHeader ();
			columnHeader.text = text;
			columnHeader.text_alignment = text_alignment;
			columnHeader.width = width;
			columnHeader.owner = owner;
			columnHeader.format = (StringFormat) Format.Clone ();
			columnHeader.column_rect = Rectangle.Empty;
			return columnHeader;
		}

		public override string ToString ()
		{
			return string.Format ("ColumnHeader: Text: {0}", text);
		}
		#endregion // Public Methods

		#region Protected Methods
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
		#endregion // Protected Methods

#if NET_2_0
		
		#region UIA Framework: Methods, Properties and Events

		static object UIATextChangedEvent = new object ();

		internal event EventHandler UIATextChanged {
			add { Events.AddHandler (UIATextChangedEvent, value); }
			remove { Events.RemoveHandler (UIATextChangedEvent, value); }
		}

		private void OnUIATextChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIATextChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		#endregion

#endif
	}
}
