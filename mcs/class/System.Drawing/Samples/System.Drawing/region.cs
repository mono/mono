using System;
using System.Drawing;
using System.Drawing.Drawing2D;


//
// Dumps an empty region
//

namespace MyFormProject
{

	class MainForm 
	{
		public MainForm()
		{

		}

		public static void Main(string[] args)
		{
			Region region = new Region ();
			
			RectangleF[] rects = region.GetRegionScans (new Matrix ());
			
			for (int i = 0; i < rects.Length; i++)
				Console.WriteLine ("{0}", rects [i]);
		}
	}

}
