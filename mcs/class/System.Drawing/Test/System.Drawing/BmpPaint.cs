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
	static public void Main( string[] args) {
		Bitmap	bmp = new Bitmap(100,100);
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
		bmp.Save("file.bmp", ImageFormat.Bmp);
		gr.Dispose();
		bmp.Dispose();
	}
};
