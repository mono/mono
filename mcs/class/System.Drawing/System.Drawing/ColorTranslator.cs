//
// System.Drawing.ColorTranslator.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
// Dennis Hayes (dennish@raytek.com)
// Inital Implimentation 3/25/2002
// All conversions based on best guess, will improve over time
// 
using System;
namespace System.Drawing {
	public class ColorTranslator{
		// From converisons
		/// <summary>
		/// 
		/// </summary>
		/// <param name="HtmlFromColor"></param>
		/// <returns></returns>
		public static Color FromHtml(string HtmlFromColor){
			// TODO: 
			// If first char is "#"
				//convert "#RRGGBB" to int and use Color.FromARGB(int) to create color
			// else //it is a color name
			//If there is a single digit at the end of the name, remove it.
			// Call Color.FromKnownColor(HtmlFromColor)  

			//At least some Html strings match .NET Colors,
			// so this should work for those colors.
			// .NET colors, XWindows colors, and WWWC web colors 
			// are (according to Charles Pretziod) base the same
			//colors, so many shouold work if any do.
			//return Color.FromKnownColor(HtmlFromColor);
			return Color.Empty;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="OLEFromColor"></param>
		/// <returns></returns>
		public static Color FromOle(int OLEFromColor){
			//int newcolor;
			//TODO: swap RB bytes i.e. AARRGGBB to AABBGGRR
			//return Color.FromArgb(newcolor);
			return Color.Empty;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Win32FromColor"></param>
		/// <returns></returns>
		public static Color FromWin32(int Win32FromColor){
			//int newcolor;
			//TODO: swap RB bytes i.e. AARRGGBB to AABBGGRR
			//return Color.FromArgb(newcolor);
			return Color.Empty;
		}

		// To conversions
		public static string ToHtml (Color c)
		{
			if (c.IsEmpty)
				return "";

			string result;

			if (c.IsNamedColor)
				result = c.ToString ();
			else
				result = String.Format ("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

			return result;
		}
		/// <summary>
		/// converts from BGR to RGB
		/// </summary>
		/// <param name="OleToColor"></param>
		/// <returns></returns>
		public static int ToOle(Color FromColor){
			// TODO: Swap red and blue(from argb), convert to int(toargb)
			// Same as ToWin32
			return (Color.FromArgb(FromColor.B,FromColor.G,FromColor.R)).ToArgb();
		}

		/// <summary>
		/// converts from RGB to BGR
		/// </summary>
		/// <param name="Win32ToColor"></param>
		/// <returns></returns>
		public static int ToWin32(Color FromColor){
			// TODO: Swap red and blue(from argb), convert to int(toargb)
			// Same as ToOle
			return (Color.FromArgb(FromColor.B,FromColor.G,FromColor.R)).ToArgb();
		}
	}
}




