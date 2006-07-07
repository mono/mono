using System;
using Cairo;

namespace Cairo.Snippets
{
	public class CairoSnippetsSVG
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
				string filename = "./" + snippet + ".svg";
				Surface surface = new SvgSurface(filename, IMAGE_WIDTH, IMAGE_WIDTH);
				Context cr = new Context(surface);
			
				cr.Save();
				Snippets.InvokeSnippet(snip, snippet, cr, IMAGE_WIDTH, IMAGE_HEIGHT);
				cr.ShowPage();
				cr.Restore();
				surface.Finish ();
			}
		}
	}
}

