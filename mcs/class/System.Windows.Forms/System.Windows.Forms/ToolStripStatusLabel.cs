//
// StatusStrip.cs
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
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.StatusStrip)]
	public class ToolStripStatusLabel : ToolStripLabel
	{
		private ToolStripStatusLabelBorderSides border_sides;
		private Border3DStyle border_style;
		private bool spring;
		
		#region Public Constructors
		public ToolStripStatusLabel ()
			: this (String.Empty, null, null, String.Empty)
		{
		}

		public ToolStripStatusLabel (Image image)
			: this (String.Empty, image, null, String.Empty)
		{
		}

		public ToolStripStatusLabel (string text)
			: this (text, null, null, String.Empty)
		{
		}

		public ToolStripStatusLabel (string text, Image image)
			: this (text, image, null, String.Empty)
		{
		}

		public ToolStripStatusLabel (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, String.Empty)
		{
		}

		public ToolStripStatusLabel (string text, Image image, EventHandler onClick, string name)
			: base (text, image, false, onClick, name)
		{
			this.border_style = Border3DStyle.Flat;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new ToolStripItemAlignment Alignment {
			get { return base.Alignment; }
			set { base.Alignment = value; }
		}
		
		[DefaultValue (ToolStripStatusLabelBorderSides.None)]
		public ToolStripStatusLabelBorderSides BorderSides {
			get { return this.border_sides; }
			set { this.border_sides = value; }
		}
		
		[DefaultValue (Border3DStyle.Flat)]
		public Border3DStyle BorderStyle {
			get { return this.border_style; }
			set { this.border_style = value; }
		}
		
		[DefaultValue (false)]
		public bool Spring {
			get { return this.spring; }
			set {
				if (this.spring != value) {
					this.spring = value;
					CalculateAutoSize ();
				}
			}
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin {
			get { return new Padding (0, 3, 0, 2); }
		}
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			return base.GetPreferredSize (constrainingSize);
		}
		#endregion

		#region Protected Methods
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}
		#endregion
	}
}
