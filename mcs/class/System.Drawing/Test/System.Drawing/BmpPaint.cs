//
// BmpPaint.cs test application
//
// Author:
//   Alexandre Pigolkine(pigolkine@gmx.de)
// 
//
// (C) Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;

public class BmpPaint {
	static public void CreateBitmap (string filename, ImageFormat imgFmt) 
	{
		Bitmap	bmp = new Bitmap(100,100, PixelFormat.Format24bppRgb);
		Console.WriteLine("Bitmap created OK {0}", bmp != null);
		if( bmp != null) {
			Console.WriteLine("Bitmap Pixelformat {0}", bmp.PixelFormat);
		}
		Graphics gr = Graphics.FromImage(bmp);
		Console.WriteLine("Graphics created OK {0}", gr != null);
		if( gr != null) {
			Pen p = new Pen(Color.Red, 2);
			gr.DrawLine(p, 10.0F, 10.0F, 90.0F, 90.0F);
			gr.DrawRectangle(p, 10.0F, 10.0F, 80.0F, 80.0F);
			p.Dispose();
		}
		bmp.Save(filename, imgFmt);
		gr.Dispose();
		bmp.Dispose();
		Console.WriteLine("Bitmap stored to " + filename);
	}
	
	static public void PaintOnBitmap (string filename) 
	{
		Bitmap	bmp = new Bitmap(filename);
		Console.WriteLine("Bitmap readed OK {0}", bmp != null);
		if( bmp != null) {
			Console.WriteLine("Bitmap Pixelformat {0}", bmp.PixelFormat);
		}
		Graphics gr = Graphics.FromImage(bmp);
		Console.WriteLine("Graphics created OK {0}", gr != null);
		if( gr != null) {
			Pen p = new Pen(Color.Blue, 2);
			gr.DrawLine(p, 20.0F, 20.0F, 80.0F, 80.0F);
			gr.DrawRectangle(p, 20.0F, 20.0F, 60.0F, 60.0F);
			p.Dispose();
		}
		bmp.Save(filename);
		gr.Dispose();
		bmp.Dispose();
		Console.WriteLine("Modified Bitmap stored to " + filename);
	}
	
	static public void Main( string[] args) 
	{
		CreateBitmap ("file.bmp", ImageFormat.Bmp);
		PaintOnBitmap ("file.bmp");
		CreateBitmap ("file.jpg", ImageFormat.Jpeg);
		PaintOnBitmap ("file.jpg");
		CreateBitmap ("file.png", ImageFormat.Png);
		PaintOnBitmap ("file.png");
	}
};
