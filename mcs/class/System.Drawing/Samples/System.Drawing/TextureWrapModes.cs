//
// Sample application for drawing figures using TextureBrush
// with different WrapModes
//
// Author:
//   Ravindra (rkumar@novell.com)
//
// (C) 2004 Novell, Inc. http://www.novell.com
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoSamples.System.Drawing
{
	public class TextureWrapModes
	{
		Image img; // To be used by brush
		Graphics gr; // To be used for creating a new bitmap
		Bitmap bmp;
		int currentTop;
		int spacing;
		int left = 100;
		int width = 450;
		int height = 250;
		
		public TextureWrapModes (string imgName, int wd, int ht, int top, int sp)
		{
			currentTop = top;
			spacing = sp;
			bmp = new Bitmap (wd,ht);
			gr = Graphics.FromImage (bmp);
			img = Image.FromFile ("./bitmaps/" + imgName);
		}
		
		public void DrawWrapModes ()
		{
			int top = currentTop;
			top += spacing;
			TextureBrush tbr = new TextureBrush (img);

			// #1: Clamp
			tbr.WrapMode = WrapMode.Clamp;
	 		gr.FillRectangle (tbr, 0, 0, width, height);
 			top = top + height + spacing;

			tbr = new TextureBrush (img);

	 		// #2: Default
	 		gr.FillRectangle (tbr, left, top, width, height);
 			top = top + height + spacing;

			// #3: Tile
			tbr.WrapMode = WrapMode.Tile;
	 		gr.FillRectangle (tbr, left, top, width, height);
 			top = top + height + spacing;

			// #4: TileFlipX
			tbr.WrapMode = WrapMode.TileFlipX;
	 		gr.FillRectangle (tbr, left, top, width, height);
 			top = top + height + spacing;

			// #5: TileFlipY
			tbr.WrapMode = WrapMode.TileFlipY;
	 		gr.FillRectangle (tbr, left, top, width, height);
 			top = top + height + spacing;

			// #6: TileFlipXY
			tbr.WrapMode = WrapMode.TileFlipXY;
	 		gr.FillRectangle (tbr, left, top, width, height);
 			top = top + height + spacing;

			currentTop = top;
		}

		public void SaveDrawing ()
		{
			// save the bmp
			bmp.Save ("TextureWrapModes.bmp");
		}
		
		// Main to test the things
		public static void Main () 
 		{
 			// Make sure that the image dimensions are 
			// sufficient to hold all the test results.
			// TextureWrapModes (imgName, width, height, top, spacing)

			TextureWrapModes twm = new TextureWrapModes ("horse.bmp", 650,
									1800, 0, 50);
 			
 			// Test the constructors
			twm.DrawWrapModes ();

 			// Save the drawing when done
 			twm.SaveDrawing ();
		}
 	}
}
