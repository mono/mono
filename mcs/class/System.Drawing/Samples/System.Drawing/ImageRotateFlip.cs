//
// Image rotation / flip
//
// Author:
//      Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc
//

using System;
using System.Drawing;
using System.Drawing.Imaging;

public class ImageRotateSample
{
	static Pen p = new Pen(Color.Red, 2);	
	static SolidBrush br = new SolidBrush(Color.White);
	
	public static void CreateImage (RotateFlipType rotate, int movex, int movey, string text, Bitmap dest, Graphics grdest)
	{
		Bitmap	bmp = new Bitmap (80, 80, PixelFormat.Format32bppArgb);			
		Graphics gr = Graphics.FromImage (bmp);
		Color clr;
		
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



