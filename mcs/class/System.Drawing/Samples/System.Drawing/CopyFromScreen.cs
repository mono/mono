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
		Bitmap	bmp = new Bitmap (800, 800);
		Graphics gr = Graphics.FromImage (bmp);

		gr.CopyFromScreen (0, 0/*src*/, 0,0 /*dst*/, new Size (800, 800));		
		bmp.Save ("CopyFromScreen.bmp");
	}

}


