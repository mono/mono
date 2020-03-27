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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Everaldo Canuto (ecanuto@novell.com)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Theming.Default
{
	internal class LabelPainter
	{
		public LabelPainter ()
		{
		}

		public virtual void Draw (Graphics dc, Rectangle client_rectangle, Label label) 
		{
			Rectangle rect = label.PaddingClientRectangle;

			label.DrawImage (dc, label.Image, rect, label.ImageAlign);

			rect.Height = Math.Max(rect.Height, label.Font.Height);

			if (label.Enabled) {
				dc.DrawString (label.Text, label.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (label.ForeColor),
					rect, label.string_format);
			} else {
				ControlPaint.DrawStringDisabled (
					dc, label.Text, label.Font, label.BackColor, rect, label.string_format);
			}
		}

		public virtual Size DefaultSize {
			get { return new Size (100, 23); }
		}
	}
}