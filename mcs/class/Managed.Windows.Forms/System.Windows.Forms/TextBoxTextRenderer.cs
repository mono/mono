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
using System.Collections;

namespace System.Windows.Forms
{
	internal class TextBoxTextRenderer
	{
		private static Size max_size;
		private static bool use_textrenderer;
		private static StringFormat sf_nonprinting;
		private static StringFormat sf_printing;
		private static Hashtable measure_cache;
				
		static TextBoxTextRenderer ()
		{
			// On Windows, we want to use TextRenderer (GDI)
			// On Linux, we want to use DrawString (GDI+)
			// TextRenderer provides translation from TextRenderer to
			// DrawString, but I doubt it's exact enough.
			// Another option would be to put Pango here for Linux.
			int platform = (int)Environment.OSVersion.Platform;
			
			if (platform == 4 || platform == 128 || platform == 6)
				use_textrenderer = false;
			else
				use_textrenderer = true;

			// windows 2000 doesn't draw with gdi if bounds are In32.MaxValue
			max_size = new Size (Int16.MaxValue, Int16.MaxValue);
			
			sf_nonprinting = new StringFormat (StringFormat.GenericTypographic);
			sf_nonprinting.Trimming = StringTrimming.None;
			sf_nonprinting.FormatFlags = StringFormatFlags.DisplayFormatControl;	
			sf_nonprinting.HotkeyPrefix = HotkeyPrefix.None;		

			sf_printing = StringFormat.GenericTypographic;
			sf_printing.HotkeyPrefix = HotkeyPrefix.None;
			
			measure_cache = new Hashtable ();
		}
		
		public static void DrawText (Graphics g, string text, Font font, Color color, float x, float y, bool showNonPrint)
		{
			if (!use_textrenderer) {
				if (showNonPrint)
					g.DrawString (text, font, ThemeEngine.Current.ResPool.GetSolidBrush (color), x, y, sf_nonprinting);
				else
					g.DrawString (text, font, ThemeEngine.Current.ResPool.GetSolidBrush (color), x, y, sf_printing);
			} else {
				if (showNonPrint)
					TextRenderer.DrawTextInternal (g, text, font, new Rectangle (new Point ((int)x, (int)y), max_size), color, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix, false);
				else
					TextRenderer.DrawTextInternal (g, text, font, new Rectangle (new Point ((int)x, (int)y), max_size), color, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix, false);
			}
		}
		
		public static SizeF MeasureText (Graphics g, string text, Font font)
		{
			// Due to the way the TextBox currently works, it measures each
			// character one at a time.  And it does this alot.  So here we
			// are implementing a cache for each font/character combination
			// measurement.  Since the number of fonts and number of characters
			// used tends to be small, this is a good performance gain for
			// not too much memory.
			if (text.Length == 1) {
				string key = font.GetHashCode ().ToString () + "|" + text;
				
				if (measure_cache.ContainsKey (key)) {
					return (SizeF)measure_cache[key];
				} else {
					SizeF size;
					
					if (!use_textrenderer)
						size = g.MeasureString (text, font, 10000, sf_nonprinting);
					else
						size = TextRenderer.MeasureTextInternal (g, text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix, false);
				
					measure_cache[key] = size;
				
					return size;
				}
			}
			
			if (!use_textrenderer)
				return g.MeasureString (text, font, 10000, sf_nonprinting);
			else
				return TextRenderer.MeasureTextInternal (g, text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix, false);
		}
	}
}
