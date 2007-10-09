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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jonathan Pobst	monkey@jpobst.com
//
//

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace System.Windows.Forms
{
	internal class TextBoxTextRenderer
	{
		private static Size max_size;
		private static bool use_textrenderer;
		private static StringFormat sf_nonprinting;
		
		static TextBoxTextRenderer ()
		{
			// On Windows, we want to use TextRenderer (GDI)
			// On Linux, we want to use DrawString (GDI+)
			// TextRenderer provides translation from TextRenderer to
			// DrawString, but I doubt it's exact enough.
			// Another option would be to put Pango here for Linux.
			int platform = (int)Environment.OSVersion.Platform;
			
			if (platform == 4 || platform == 128)
				use_textrenderer = false;
			else
				use_textrenderer = true;
				
			max_size = new Size (int.MaxValue, int.MaxValue);
			
			sf_nonprinting = new StringFormat (StringFormat.GenericTypographic);
			sf_nonprinting.Trimming = StringTrimming.None;
			sf_nonprinting.FormatFlags = StringFormatFlags.DisplayFormatControl;			
		}
		
		public static void DrawText (Graphics g, string text, Font font, Color color, float x, float y, bool showNonPrint)
		{
			if (!use_textrenderer) {
				if (showNonPrint)
					g.DrawString (text, font, ThemeEngine.Current.ResPool.GetSolidBrush (color), x, y, sf_nonprinting);
				else
					g.DrawString (text, font, ThemeEngine.Current.ResPool.GetSolidBrush (color), x, y, StringFormat.GenericTypographic);
			} else {
				if (showNonPrint)
					TextRenderer.DrawTextInternal (g, text, font, new Rectangle (new Point ((int)x, (int)y), max_size), color, TextFormatFlags.NoPadding, false);
				else
					TextRenderer.DrawTextInternal (g, text, font, new Rectangle (new Point ((int)x, (int)y), max_size), color, TextFormatFlags.NoPadding, false);
			}
		}
		
		public static SizeF MeasureText (Graphics g, string text, Font font)
		{
			if (!use_textrenderer)
				return g.MeasureString (text, font, 10000, sf_nonprinting);
			else
				return TextRenderer.MeasureTextInternal (g, text, font, Size.Empty, TextFormatFlags.NoPadding, false);
		}
	}
}
