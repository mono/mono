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
// $Revision: 1.1 $
// $Modtime: $
// $Log: ColumnHeader.cs,v $
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
		private string text = "ColumnHeader";
		private HorizontalAlignment textAlignment = HorizontalAlignment.Left;
		private int width = 60;
		#endregion	// Instance Variables

		#region Internal Constructor
		internal ColumnHeader (ListView owner, string text, HorizontalAlignment alignment, int width)
		{
			this.owner = owner;
			this.text = text;
			this.width = width;
			this.textAlignment = alignment;
		}
		#endregion	// Internal Constructor

		#region Public Constructors
		public ColumnHeader () { }
		#endregion	// Public Constructors

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
			set { text = value; }
		}

		[DefaultValue (HorizontalAlignment.Left)]
		[Localizable (true)]
		public HorizontalAlignment TextAlign {
			get { return textAlignment; }
			set { textAlignment = value; }
		}

		[DefaultValue (60)]
		[Localizable (true)]
		public int Width {
			get { return width; }
			set { width = value; }
		}
		#endregion // Public Instance Properties

		#region Public Methods
		public virtual object Clone ()
		{
			ColumnHeader columnHeader = new ColumnHeader ();
			columnHeader.Text = text;
			columnHeader.TextAlign = textAlignment;
			columnHeader.Width = width;
			columnHeader.owner = owner;
			return columnHeader;
		}

		public override string ToString ()
		{
			return string.Format ("ColumnHeader: Text: {0}", text);
		}
		#endregion //Public Methods

		#region Protected Methods
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
		#endregion //Protected Methods
	}
}
