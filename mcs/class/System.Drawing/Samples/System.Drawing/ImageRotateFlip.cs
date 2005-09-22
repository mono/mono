//
// Image rotation / flip
//
// Author:
//      Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc
//

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
using System.Drawing.Imaging;

public class ImageRotateSample
{
	static Pen p = new Pen(Color.Red, 2);	
	static SolidBrush br = new SolidBrush(Color.Black);
	
	public static void CreateImage (RotateFlipType rotate, int movex, int movey, string text, Bitmap dest, Graphics grdest)
	{
		Color clr;
		Bitmap	bmp = new Bitmap (80, 80, PixelFormat.Format32bppArgb);	
		Graphics gr = Graphics.FromImage (bmp);
		gr.Clear (Color.White);
		
		gr.DrawLine (p, 10.0F, 10.0F, 70.0F, 70.0F);
		gr.DrawLine (p, 10.0F, 15.0F, 70.0F, 15.0F);
		gr.DrawRectangle (p, 10.0F, 10.0F, 60.0F, 60.0F);				
		bmp.RotateFlip (rotate);		
		
		for (int y = 0; y < 80; y++) {
			for (int x = 0; x < 80; x++) {				
				clr = bmp.GetPixel (x,y);
				dest.SetPixel (x+movex, y+movey, clr);
			}
		}							
		
		grdest.DrawString (text, new Font ("Arial", 8), br,  movex+5, movey+85);		
	}
		
	public static void Main(string[] argv) 
	{

		string filename = "output.bmp";
		Bitmap	bmp = new Bitmap(800,800, PixelFormat.Format32bppArgb);
		Console.WriteLine("Bitmap created OK {0}", bmp != null);
	
		Graphics gr = Graphics.FromImage(bmp);
		gr.Clear (Color.White);
		
		CreateImage (RotateFlipType.RotateNoneFlipNone, 0, 0, "RotateNoneFlipNone", bmp, gr);
		CreateImage (RotateFlipType.Rotate90FlipNone, 150, 0, "Rotate90FlipNone", bmp, gr);
		CreateImage (RotateFlipType.Rotate180FlipNone, 300, 0, "Rotate180FlipNone", bmp, gr);
		CreateImage (RotateFlipType.Rotate270FlipNone, 450, 0, "Rotate270FlipNone", bmp, gr);
		
		CreateImage (RotateFlipType.RotateNoneFlipX, 0, 120, "RotateNoneFlipX", bmp, gr);
		CreateImage (RotateFlipType.Rotate90FlipX, 150, 120, "Rotate90FlipX", bmp, gr);
		CreateImage (RotateFlipType.Rotate180FlipX, 300, 120, "Rotate180FlipX", bmp, gr);
		CreateImage (RotateFlipType.Rotate270FlipX, 450, 120, "Rotate270FlipX", bmp, gr);
		
		bmp.Save(filename, ImageFormat.Bmp);
		
		Console.WriteLine("Bitmap stored to " + filename);		
	}
}



