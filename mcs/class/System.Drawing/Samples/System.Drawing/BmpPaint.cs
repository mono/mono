//
// BmpPaint.cs sample application
//
// Author:
//   Alexandre Pigolkine(pigolkine@gmx.de)
// 
//
// (C) Ximian, Inc.  http://www.ximian.com
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

namespace MonoSamples.System.Drawing
{
	public class BmpPaint
	{
		static public void CreateBitmap (string filename, ImageFormat imgFmt) {
			Bitmap	bmp = new Bitmap (100, 100, PixelFormat.Format24bppRgb);
			Console.WriteLine ("Bitmap created OK {0}", bmp != null);
			if ( bmp != null) {
				Console.WriteLine ("Bitmap Pixelformat {0}", bmp.PixelFormat);
			}

			Graphics gr = Graphics.FromImage (bmp);
			Console.WriteLine ("Graphics created OK {0}", gr != null);
			if (gr != null) {
				Pen p = new Pen (Color.Red, 2);
				gr.DrawLine (p, 10.0F, 10.0F, 90.0F, 90.0F);
				gr.DrawRectangle (p, 10.0F, 10.0F, 80.0F, 80.0F);
				p.Dispose ();
			}
			bmp.Save (filename, imgFmt);
			gr.Dispose ();
			bmp.Dispose ();
			Console.WriteLine ("Bitmap stored to " + filename);
		}

		static public void PaintOnBitmap (string filename, string newname, ImageFormat imgFmt) {
			Bitmap	bmp = new Bitmap (filename);
			Console.WriteLine ("Bitmap read OK {0}", bmp != null);
			if (bmp != null) {
				Console.WriteLine ("Bitmap Pixelformat {0}", bmp.PixelFormat);
			}
			Graphics gr = Graphics.FromImage (bmp);
			Console.WriteLine ("Graphics created OK {0}", gr != null);
			if (gr != null) {
				Pen p = new Pen (Color.Blue, 2);
				gr.DrawLine (p, 20.0F, 20.0F, 80.0F, 80.0F);
				gr.DrawRectangle (p, 20.0F, 20.0F, 60.0F, 60.0F);
				p.Dispose ();
			}
			bmp.Save (newname, imgFmt);
			gr.Dispose ();
			bmp.Dispose ();
			Console.WriteLine ("Modified Bitmap stored to " + newname);
		}
	
		static public void Main (string[] args) {
			CreateBitmap ("BmpPaint.bmp", ImageFormat.Bmp);
			PaintOnBitmap ("BmpPaint.bmp", "Bmp-Painted.bmp", ImageFormat.Bmp);
			CreateBitmap ("BmpPaint.jpg", ImageFormat.Jpeg);
			PaintOnBitmap ("BmpPaint.jpg", "Bmp-Painted.jpg", ImageFormat.Jpeg);
			CreateBitmap ("BmpPaint.png", ImageFormat.Png);
			PaintOnBitmap ("BmpPaint.png", "Bmp-Painted.png", ImageFormat.Png);
		}
	}
}
