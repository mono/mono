using System;
using Cairo;

namespace Cairo.Snippets
{
	public class CairoSnippetsPDF
	{
		public static int IMAGE_WIDTH = 256;
		public static int IMAGE_HEIGHT = 256;

		public static double LINE_WIDTH = 0.04;

		public static void Main(string[] args)
		{
			// call the snippets
			Snippets snip = new Snippets();
			Surface surface = new PdfSurface("snippets.pdf", IMAGE_WIDTH, IMAGE_WIDTH);
			Context cr = new Context(surface);

			foreach (string snippet in Snippets.snippets)
			{
				cr.Save();
				Snippets.InvokeSnippet(snip, snippet, cr, IMAGE_WIDTH, IMAGE_HEIGHT);
				cr.ShowPage ();
				cr.Restore();
			}
			surface.Finish ();
		}
	}
}

