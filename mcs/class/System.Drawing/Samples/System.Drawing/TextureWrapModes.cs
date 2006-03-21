//
// Sample application for drawing figures using TextureBrush
// with different WrapModes
//
// Author:
//   Ravindra (rkumar@novell.com)
//

// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
			bmp.Save ("TextureWrapModes.png", ImageFormat.Png);
		}
		
		// Main to draw the things
		public static void Main () 
 		{
 			// Make sure that the image dimensions are 
			// sufficient to hold all the test results.
			// TextureWrapModes (imgName, width, height, top, spacing)

			TextureWrapModes twm = new TextureWrapModes ("horse.bmp", 650,
									1850, 0, 50);
 			
 			// Draw different wrapmodes
			twm.DrawWrapModes ();

 			// Save the drawing when done
 			twm.SaveDrawing ();
		}
 	}
}
