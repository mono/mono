//
// System.Drawing.ColorTranslator.cs
//
// Copyright (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com
//
// Authors:
//	Dennis Hayes (dennish@raytek.com)
//	Ravindra (rkumar@novell.com)
//
//
 
using System;
using System.ComponentModel;

namespace System.Drawing
{
	public sealed class ColorTranslator
	{
		private ColorTranslator () { }

		public static Color FromHtml (string HtmlFromColor)
		{
			TypeConverter converter = TypeDescriptor.GetConverter (typeof (Color));
			return (Color) converter.ConvertFromString (HtmlFromColor);
		}

		public static Color FromOle (int OleFromColor)
		{
			// OleColor format is BGR
			int R = OleFromColor & 0xFF;
			int G = (OleFromColor >> 8) & 0xFF;
			int B = (OleFromColor >> 16) & 0xFF;

			Color retcolor = Color.FromArgb (255, R, G, B);
			foreach (Color c in Color.NamedColors.Values) {
				if (c == retcolor)
					return c;
			}

			foreach (Color c in Color.SystemColors.Values) {
				if (c == retcolor)
					return c;
			}

			return retcolor;
		}

		public static Color FromWin32 (int Win32FromColor)
		{
			// Win32Color format is BGR
			int R = Win32FromColor & 0xFF;
			int G = (Win32FromColor >> 8) & 0xFF;
			int B = (Win32FromColor >> 16) & 0xFF;

			Color retcolor = Color.FromArgb (255, R, G, B);
			foreach (Color c in Color.NamedColors.Values) {
				if (c == retcolor)
					return c;
			}

			foreach (Color c in Color.SystemColors.Values) {
				if (c == retcolor)
					return c;
			}

			return retcolor;
		}

		public static string ToHtml (Color c)
		{
			if (c.IsEmpty)
				return "";

			string result;

			if (c.IsNamedColor)
				result = c.Name;
			else
				result = String.Format ("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

			return result;
		}

		public static int ToOle (Color color)
		{
			// OleColor format is BGR, same as Win32

			return  ((color.B << 16) | (color.G << 8) | color.R);
		}

		public static int ToWin32 (Color color)
		{
			// Win32Color format is BGR, Same as OleColor

			return  ((color.B << 16) | (color.G << 8) | color.R);
		}
	}
}
