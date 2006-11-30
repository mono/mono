//
// TextRenderer.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Text;

namespace System.Windows.Forms
{
	public sealed class TextRenderer
	{
		private TextRenderer ()
		{
		}
		
		#region Public Methods
		[MonoTODO("This should be correct for Windows, other platforms need a more accurate fallback method than the one provided")]
		public static void DrawText (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
		{
			if (text == null || text.Length == 0)
				return;
				
			if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows) {
				Rectangle new_bounds = bounds;
				new_bounds.Offset ((int)(dc as Graphics).Transform.OffsetX + 0, (int)(dc as Graphics).Transform.OffsetY + 0);
				IntPtr hdc = dc.GetHdc ();

				SetTextColor (hdc, ColorTranslator.ToWin32 (foreColor));
				SetBkMode (hdc, 1);	//1-Transparent, 2-Opaque

				VisualStyles.UXTheme.RECT r = VisualStyles.UXTheme.RECT.FromRectangle (new_bounds);

				if (font != null)
					SelectObject (hdc, font.ToHfont ());

				DrawText (hdc, text, text.Length, ref r, (int)flags);
				dc.ReleaseHdc ();
			}
			else {
				Graphics g;

				if (dc is Graphics)
					g = (Graphics)dc;
				else
					g = Graphics.FromHdc (dc.GetHdc ());

				StringFormat sf = FlagsToStringFormat (flags);

				using (Brush b = new SolidBrush (foreColor))
					g.DrawString (text, font, b, bounds, sf);

				if (!(dc is Graphics)) {
					g.Dispose ();
					dc.ReleaseHdc ();
				}
			}
		}

		[MonoTODO ("This should be correct for Windows, other platforms need a more accurate fallback method than the one provided")]
		public static void DrawText (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, TextFormatFlags flags)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows) {
				IntPtr hdc = dc.GetHdc ();

				SetTextColor (hdc, ColorTranslator.ToWin32 (foreColor));
				SetBkMode (hdc, 1);	//1-Transparent, 2-Opaque

				Size sz = MeasureText(text, font);
				
				VisualStyles.UXTheme.RECT r = new System.Windows.Forms.VisualStyles.UXTheme.RECT(pt.X, pt.Y, pt.X + sz.Width, pt.Y + sz.Height);

				if (font != null)
					SelectObject (hdc, font.ToHfont ());

				DrawText (hdc, text, text.Length, ref r, (int)flags);
				dc.ReleaseHdc ();
			}
			else {
				Graphics g;

				if (dc is Graphics)
					g = (Graphics)dc;
				else
					g = Graphics.FromHdc (dc.GetHdc ());

				StringFormat sf = FlagsToStringFormat (flags);

				using (Brush b = new SolidBrush (foreColor))
					g.DrawString (text, font, b, pt, sf);

				if (!(dc is Graphics)) {
					g.Dispose ();
					dc.ReleaseHdc ();
				}
			}
		}

		[MonoTODO ("This should be correct for Windows, other platforms need a more accurate fallback method than the one provided")]
		public static Size MeasureText (string text, Font font)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows) {
				Bitmap b = new Bitmap (5, 5);
				Graphics g = Graphics.FromImage (b);
				
				IntPtr hdc = g.GetHdc ();

				if (font != null)
					SelectObject (hdc, font.ToHfont ());
					
				VisualStyles.UXTheme.SIZE text_size = new System.Windows.Forms.VisualStyles.UXTheme.SIZE();

				GetTextExtentPoint32 (hdc, text, text.Length, out text_size);
				
				g.ReleaseHdc();
				
				Size retval = text_size.ToSize();
				//retval.Height += 4;
				if (retval.Width > 0) retval.Width += 6;
				return retval;
			}
			else {
				Bitmap b = new Bitmap (5, 5);
				Graphics g = Graphics.FromImage (b);

				Size retval = g.MeasureString(text,font).ToSize();
				if (retval.Width > 0) retval.Width += 6;
				return retval;	
			}
		}

		[MonoTODO ("This should be correct for Windows, other platforms need a more accurate fallback method than the one provided")]
		public static Size MeasureText (string text, Font font, Size proposedSize, TextFormatFlags flags)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows) {
				if ((flags & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
				{
					text = text.Replace ("&", "");
				}
				
				Bitmap b = new Bitmap (5, 5);
				Graphics g = Graphics.FromImage (b);

				IntPtr hdc = g.GetHdc ();

				if (font != null)
					SelectObject (hdc, font.ToHfont ());

				VisualStyles.UXTheme.SIZE text_size = new System.Windows.Forms.VisualStyles.UXTheme.SIZE ();

				GetTextExtentPoint32 (hdc, text, text.Length, out text_size);

				g.ReleaseHdc ();

				Size retval = text_size.ToSize ();
				//retval.Height += 4;
				if (retval.Width > 0) retval.Width += 6;
				return retval;
			}
			else {
				Bitmap b = new Bitmap (5, 5);
				Graphics g = Graphics.FromImage (b);

				Size retval = g.MeasureString (text, font).ToSize ();
				if (retval.Width > 0) retval.Width += 6;
				return retval;
			}
		}
		#endregion

		#region Private Methods
		private static StringFormat FlagsToStringFormat (TextFormatFlags flags)
		{
			StringFormat sf = new StringFormat ();

			// Translation table: http://msdn.microsoft.com/msdnmag/issues/06/03/TextRendering/default.aspx?fig=true#fig4

			// Horizontal Alignment
			if ((flags & TextFormatFlags.HorizontalCenter) == TextFormatFlags.HorizontalCenter)
				sf.Alignment = StringAlignment.Center;
			else if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
				sf.Alignment = StringAlignment.Far;
			else
				sf.Alignment = StringAlignment.Near;

			// Vertical Alignment
			if ((flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
				sf.LineAlignment = StringAlignment.Far;
			else if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter)
				sf.LineAlignment = StringAlignment.Center;
			else
				sf.LineAlignment = StringAlignment.Near;

			// Ellipsis
			if ((flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis)
				sf.Trimming = StringTrimming.EllipsisCharacter;
			else if ((flags & TextFormatFlags.PathEllipsis) == TextFormatFlags.PathEllipsis)
				sf.Trimming = StringTrimming.EllipsisPath;
			else if ((flags & TextFormatFlags.WordEllipsis) == TextFormatFlags.WordEllipsis)
				sf.Trimming = StringTrimming.EllipsisWord;
			else
				sf.Trimming = StringTrimming.Character;

			// Hotkey Prefix
			if ((flags & TextFormatFlags.NoPrefix) == TextFormatFlags.NoPrefix)
				sf.HotkeyPrefix = HotkeyPrefix.None;
			else if ((flags & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
				sf.HotkeyPrefix = HotkeyPrefix.Hide;
			else
				sf.HotkeyPrefix = HotkeyPrefix.Show;

			// Text Padding
			if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding)
				sf.FormatFlags |= StringFormatFlags.FitBlackBox;

			// Text Wrapping
			if ((flags & TextFormatFlags.SingleLine) == TextFormatFlags.SingleLine)
				sf.FormatFlags |= StringFormatFlags.NoWrap;
			else if ((flags & TextFormatFlags.TextBoxControl) == TextFormatFlags.TextBoxControl)
				sf.FormatFlags |= StringFormatFlags.LineLimit;

			// Other Flags
			if ((flags & TextFormatFlags.RightToLeft) == TextFormatFlags.RightToLeft)
				sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			if ((flags & TextFormatFlags.NoClipping) == TextFormatFlags.NoClipping)
				sf.FormatFlags |= StringFormatFlags.NoClip;
			sf.FormatFlags |= StringFormatFlags.NoClip;

			return sf;
		}
		#endregion

		#region DllImports (Windows)
		[DllImport ("user32", CharSet = CharSet.Unicode)]
		static extern int DrawText (IntPtr hdc, string lpStr, int nCount, ref VisualStyles.UXTheme.RECT lpRect, int wFormat);

		[DllImport ("gdi32")]
		static extern int SetTextColor (IntPtr hdc, int crColor);

		[DllImport ("gdi32")]
		private extern static IntPtr SelectObject (IntPtr hDC, IntPtr hObject);

		[DllImport ("gdi32")]
		static extern int SetBkColor (IntPtr hdc, int crColor);

		[DllImport ("gdi32")]
		static extern int SetBkMode (IntPtr hdc, int iBkMode);

		[DllImport ("gdi32")]
		static extern bool GetTextExtentExPoint (IntPtr hdc, string lpszStr, int cchString, int nMaxExtent, IntPtr lpnFit, IntPtr alpDx, out VisualStyles.UXTheme.SIZE lpSize);

		[DllImport ("gdi32")]
		static extern bool GetTextExtentPoint32 (IntPtr hdc, string lpString, int cbString, out VisualStyles.UXTheme.SIZE lpSize);
		#endregion
	}

}
#endif
