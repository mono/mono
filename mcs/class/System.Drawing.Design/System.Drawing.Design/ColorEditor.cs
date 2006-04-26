//
// System.Drawing.Design.ColorEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 

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

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Drawing.Design
{
	public class ColorEditor : UITypeEditor
	{
		private ColorDialog colorEdit;

		public ColorEditor()
		{
		}

		public override object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			// TODO MS.Net is using a in place color editor. We just use the modal
			// Windows.Forms.ColorDialog to keep things simple for now
			// especially as Windows.Forms are not fully implemented right now

			colorEdit = new ColorDialog ();

			if (value is Color)
				colorEdit.Color = (Color) value;
			else
				// Set default
				colorEdit.Color = Color.White; // TODO set which color as default?

			colorEdit.FullOpen = true;
			DialogResult result = colorEdit.ShowDialog();

			if (result == DialogResult.OK)
				return colorEdit.Color;
			else
				return value;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;

			// TODO UITypeEditorEditStyle.DropDown is returned by the MS.Net library
			// see EditValue why we use a modal window for now
			// return UITypeEditorEditStyle.DropDown;
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			if (e.Value != null)
			{
				Color C = (Color) e.Value;
				using (SolidBrush sb = new SolidBrush (C))
					G.FillRectangle (sb, e.Bounds);
			}
		}
	}
}
