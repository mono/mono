//
// System.Drawing.Design.FontNameEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

namespace System.Drawing.Design
{
	public class FontNameEditor : UITypeEditor
	{
		private const String PreviewString = "Ab";

		public  FontNameEditor()
		{
		}

		public override bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue (PaintValueEventArgs e)
		{
			Graphics G = e.Graphics;

			// Draw the background 
			G.FillRectangle (SystemBrushes.ActiveCaption, e.Bounds);

			// Draw the sample string
			string fontName = e.Value as string;
			if (fontName != null && fontName.Length > 0) {
				using (Font font = new Font (fontName, e.Bounds.Height, FontStyle.Regular, GraphicsUnit.Pixel)) {
					G.DrawString (PreviewString, font, SystemBrushes.ActiveCaptionText, e.Bounds);
				}
			}

			// Draw the border again to ensure it is not overlapped by the text
			G.DrawRectangle (Pens.Black, e.Bounds);
		}
	}
}
