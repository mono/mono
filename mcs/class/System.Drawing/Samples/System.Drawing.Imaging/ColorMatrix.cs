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
// Copyright (c) 2005 Novell, Inc.
//
// Author:
//
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// Tests color matrix processing
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

//
public class SampleColorMatrix{

	public static Color ProcessColorMatrix (Color color, ColorMatrix colorMatrix)
	{
		Bitmap bmp = new Bitmap (64, 64);
		Graphics gr = Graphics.FromImage (bmp);
		ImageAttributes imageAttr = new ImageAttributes ();

		bmp.SetPixel (0,0, color);

		imageAttr.SetColorMatrix (colorMatrix);
		gr.DrawImage (bmp, new Rectangle (0, 0, 64,64), 0,0, 64,64, GraphicsUnit.Pixel, imageAttr);

		Console.WriteLine ("{0} - > {1}", color,  bmp.GetPixel (0,0));
		return bmp.GetPixel (0,0);

	}
	
	public static void ProcessImageColorMatrix (string sin, string sout, ColorMatrix colorMatrix)
	{
		Bitmap bmp_in = new Bitmap (sin);		
		Bitmap bmp_out = new Bitmap (bmp_in.Width, bmp_in.Height, bmp_in.PixelFormat);		
		
		Graphics gr = Graphics.FromImage (bmp_out);
		ImageAttributes imageAttr = new ImageAttributes ();
		
		imageAttr.SetColorMatrix (colorMatrix);

		gr.DrawImage (bmp_in, new Rectangle (0, 0, bmp_out.Width, bmp_out.Height), 
			0,0, bmp_out.Width, bmp_out.Height, GraphicsUnit.Pixel, imageAttr);		
		
		imageAttr.Dispose ();			
		bmp_out.Save (sout);
		bmp_in.Dispose ();
		bmp_out.Dispose ();
		Console.WriteLine ("Saving image file {0}", sout);
	}

	public static void Main(string[] args)
	{
		Color clr_src, clr_rslt;

		Console.WriteLine ("Red");
		/* Red */
		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0},
				new float[] 	{0,	1,	0, 	0, 	0},
				new float[] 	{0,	0,	1, 	0, 	0},
				new float[] 	{0,	0,	0, 	1, 	0},
				new float[] 	{0,	0,	0, 	0, 	0},
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{2,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	1, 	0}, //A
				new float[] 	{0.2f,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{0.5f,	0,	0, 	0, 	0}, //R
				new float[] 	{0.5f,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	1, 	0}, //A
				new float[] 	{0.2f,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		/* Green */
		Console.WriteLine ("Green");
		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	2,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0.5f,	0, 	1, 	0}, //A
				new float[] 	{0,	0.1f,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	2,	0, 	1, 	0}, //A
				new float[] 	{0,	0.5f,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	0,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0.5f,	0, 	1, 	0}, //A
				new float[] 	{0,	0f,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		/* Blue */
		Console.WriteLine ("Blue");
		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1.5f, 	0, 	0}, //B
				new float[] 	{0,	0,	0.5f, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 40, 25);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	1, 	0, 	0}, //G
				new float[] 	{0,	0,	0, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 100, 25);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	0,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0.5f,	0, 	1, 	0}, //A
				new float[] 	{0,	0f,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		/* Blue */
		Console.WriteLine ("All");
		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0.5f,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0.1f,	1.5f, 	0, 	0}, //B
				new float[] 	{0.5f,	3,	0.5f, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 10, 20, 25);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}

		{
			ColorMatrix colorMatrix = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0.5f, 	0}, //R
				new float[] 	{0,	1,	1, 	0, 	0}, //G
				new float[] 	{0,	0,	0.8f, 	0, 	0}, //B
				new float[] 	{0,	2,	0, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 100, 25);
			clr_rslt = ProcessColorMatrix (clr_src, colorMatrix);
		}		
		
		Console.WriteLine ("Images ---");
		
		// Load an image and convert it to gray scale
		ColorMatrix grayscale = new ColorMatrix();
		grayscale.Matrix00 = 1/3f;
		grayscale.Matrix01 = 1/3f;
		grayscale.Matrix02 = 1/3f;
		grayscale.Matrix10 = 1/3f;
		grayscale.Matrix11 = 1/3f;
		grayscale.Matrix12 = 1/3f;
		grayscale.Matrix20 = 1/3f;
		grayscale.Matrix21 = 1/3f;
		grayscale.Matrix22 = 1/3f; 		
		ProcessImageColorMatrix ("../System.Drawing/bitmaps/horse.bmp", "greyscale.bmp", grayscale);
		
		// Load an image and convert it to sepia
		
		ColorMatrix sepia = new ColorMatrix (new float[][] {
			new float[] {0.393f, 0.349f, 0.272f, 0, 0},
		        new float[] {0.769f, 0.686f, 0.534f, 0, 0},
		        new float[] {0.189f, 0.168f, 0.131f, 0, 0},
		        new float[] {     0,      0,      0, 1, 0},
		        new float[] {     0,      0,      0, 0, 1}
		});
		
		ProcessImageColorMatrix ("../System.Drawing/bitmaps/horse.bmp", "sepia.bmp", sepia);

	}

}


