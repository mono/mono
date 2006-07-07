using System;
using Cairo;

namespace Cairo.Snippets
{
	public class CairoSnippetsPS
	{
		public static int IMAGE_WIDTH = 256;
		public static int IMAGE_HEIGHT = 256;

		public static void Main(string[] args)
		{
			// call the snippets
			Snippets snip = new Snippets();
			Surface surface = new PSSurface("snippets.ps", IMAGE_WIDTH, IMAGE_WIDTH);
			Context cr = new Context(surface);
			foreach (string snippet in Snippets.snippets)
			{
				cr.Save();
				Snippets.InvokeSnippet(snip, snippet, cr, IMAGE_WIDTH, IMAGE_HEIGHT);
				cr.ShowPage();
				cr.Restore();
			}
			surface.Finish ();
		}
	}
}

