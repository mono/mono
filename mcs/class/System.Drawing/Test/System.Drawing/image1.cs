//
// image1.cs test application
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

namespace ImageTest1 {
	public class ImageTest {
		public static void Main(string[] argv) {
			if( argv.Length == 1) {
				Bitmap bmp = new Bitmap(argv[0]);
				
				Console.WriteLine("Output file " + argv[0] + ".bmp");
				bmp.Save("output.bmp", ImageFormat.Bmp);
				
			}
			else {
				Console.WriteLine("usage: image1.exe <filename>");
			}
		}
	}
}
