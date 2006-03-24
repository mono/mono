//
// Sample that show the default SystemFonts and its names
// Requieres .NET 2.0 class library
//

using System;
using System.Drawing;


public class SystemFontsSample
{
	public static void Main ()
	{
		Console.WriteLine ("--> CaptionFont [{0}] {1}", SystemFonts.CaptionFont.SystemFontName, SystemFonts.CaptionFont);
		Console.WriteLine ("--> DefaultFont [{0}] {1}", SystemFonts.DefaultFont.SystemFontName, SystemFonts.DefaultFont);
		Console.WriteLine ("--> DialogFont [{0}] {1}", SystemFonts.DialogFont.SystemFontName, SystemFonts.DialogFont);
		Console.WriteLine ("--> IconTitleFont [{0}] {1}", SystemFonts.IconTitleFont.SystemFontName, SystemFonts.IconTitleFont);
		Console.WriteLine ("--> MenuFont [{0}] {1}", SystemFonts.MenuFont.SystemFontName, SystemFonts.MenuFont);
		Console.WriteLine ("--> MessageBoxFont [{0}] {1}", SystemFonts.MessageBoxFont.SystemFontName, SystemFonts.MessageBoxFont);
		Console.WriteLine ("--> SmallCaptionFont [{0}] {1}", SystemFonts.SmallCaptionFont.SystemFontName, SystemFonts.SmallCaptionFont);
		Console.WriteLine ("--> StatusFont [{0}] {1}", SystemFonts.StatusFont.SystemFontName, SystemFonts.StatusFont);

		Font fnt = new Font ("Arial", 12);
		Console.WriteLine ("--> IsSystemFontName {0} {1}", SystemFonts.StatusFont.IsSystemFont, fnt.IsSystemFont);
	}

}


