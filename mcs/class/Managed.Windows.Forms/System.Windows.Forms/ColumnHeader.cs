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
// $Revision: 1.3 $
// $Modtime: $
// $Log: ColumnHeader.cs,v $
// Revision 1.3  2004/10/26 09:33:00  ravindra
// Added some internal members and calculations for ColumnHeader.
//
// Revision 1.2  2004/10/15 15:06:44  ravindra
// Flushing some formatting changes.
//
// Revision 1.1  2004/09/30 13:25:33  ravindra
// Supporting class for ListView control.
//
//
// COMPLETE
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	public class ColumnHeader : Component, ICloneable
	{
		#region Instance Variables
		internal ListView owner;
		private StringFormat format;
		private string text = "ColumnHeader";
		private HorizontalAlignment text_alignment = HorizontalAlignment.Left;
		private int width = ThemeEngine.Current.DefaultColumnWidth;
		// column area
		internal Rectangle column_rect = Rectangle.Empty;
		#endregion	// Instance Variables

		#region Internal Constructor
		internal ColumnHeader (ListView owner, string text, HorizontalAlignment alignment, int width)
		{
			this.owner = owner;
			this.text = text;
			this.width = width;
			this.text_alignment = alignment;
			CalcColumnHeader ();
		}
		#endregion	// Internal Constructor

		#region Public Constructors
		public ColumnHeader () { }
		#endregion	// Public Constructors

		#region Private Internal Methods Properties
		// Since this class inherits from MarshalByRef,
		// we can't do ColumnHeader.column_rect.XXX. Hence,
		// we have following properties to work around.
		internal int X {
			get { return this.column_rect.X; }
			set { this.column_rect.X = value; }
		}

		internal int Y {
			get { return this.column_rect.Y; }
			set { this.column_rect.Y = value; }
		}

		internal int Wd {
			get { return this.column_rect.Width; }
			set { this.column_rect.Width = value; }
		}

		internal int Ht {
			get { return this.column_rect.Height; }
			set { this.column_rect.Height = value; }
		}

		internal Rectangle Rect {
			get { return this.column_rect; }
		}

		internal StringFormat Format {
			get { return this.format; }
		}

		internal void CalcColumnHeader ()
		{
			format = new StringFormat ();
			if (text_alignment == HorizontalAlignment.Center)
				format.Alignment = StringAlignment.Center;
			else if (text_alignment == HorizontalAlignment.Right)
				format.Alignment = StringAlignment.Far;
			else
				format.Alignment = StringAlignment.Near;
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisWord;
			// text is wrappable only in LargeIcon and SmallIcon views
			format.FormatFlags = StringFormatFlags.NoWrap;

			if (width >= 0) {
				this.column_rect.Width = width;
				if (owner != null)
					this.column_rect.Height = owner.Font.Height;
				else
					this.column_rect.Height = ThemeEngine.Current.DefaultFont.Height;
			}
			else if (this.Index != -1)
				this.column_rect.Size = owner.GetChildColumnSize (this.Index);
			else
				this.column_rect.Size = Size.Empty;
		}
		#endregion	// Private Internal Methods Properties

		#region Public Instance Properties
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

		[Localizable (true)]
		public string Text {
			get { return text; }
			set {
				text = value;
				if (owner != null)
					owner.Redraw (true);
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
				width = value;
				if (owner != null)
					owner.Redraw (true);
			}
		}
		#endregion // Public Instance Properties

		#region Public Methods
		public virtual object Clone ()
		{
			ColumnHeader columnHeader = new ColumnHeader ();
			columnHeader.text = text;
			columnHeader.text_alignment = text_alignment;
			columnHeader.width = width;
			columnHeader.owner = owner;
			columnHeader.column_rect = column_rect = new Rectangle (Point.Empty, Size.Empty);
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
	}
}
