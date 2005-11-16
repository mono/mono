using System;
using Cairo;

namespace Cairo.Snippets
{
	public class CairoSnippetsPNG
	{
		public static int IMAGE_WIDTH = 256;
		public static int IMAGE_HEIGHT = 256;

		public static double LINE_WIDTH = 0.04;

		public static void Main(string[] args)
		{
			// call the snippets
			Snippets snip = new Snippets();
			foreach (string snippet in Snippets.snippets)
			{
				string filename = "./" + snippet + ".png";
				Surface surface = new ImageSurface(Format.ARGB32, IMAGE_WIDTH, IMAGE_WIDTH);
				Graphics cr = new Graphics(surface);
			
				cr.Save();
				Snippets.InvokeSnippet(snip, snippet, cr, IMAGE_WIDTH, IMAGE_HEIGHT);
    	   	    surface.WriteToPng(filename);
				cr.Restore();
			}
		}
	}
}

