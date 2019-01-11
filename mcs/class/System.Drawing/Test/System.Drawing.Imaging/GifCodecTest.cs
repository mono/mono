//
// GIF Codec class testing unit
//
// Authors:
// 	Jordi Mas i Hernàndez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006, 2007 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Permissions;
using System.Text;
using NUnit.Framework;

using MonoTests.Helpers;
namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	public class GifCodecTest {

		/* Get suffix to add to the filename */
		internal string getOutSufix ()
		{
			string s;

			int p = (int) Environment.OSVersion.Platform;
			if ((p == 4) || (p == 128) || (p == 6))
				s = "-unix";
			else
				s = "-windows";

			if (Type.GetType ("Mono.Runtime", false) == null)
				s += "-msnet";
			else
				s += "-mono";

			return s;
		}

		/* Checks bitmap features on a know 1bbp bitmap */
		/* Checks bitmap features on a know 1bbp bitmap */
		private void Bitmap8bitsFeatures (string filename)
		{
			using (Bitmap bmp = new Bitmap (filename)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.AreEqual (PixelFormat.Format8bppIndexed, bmp.PixelFormat);
				Assert.AreEqual (110, bmp.Width, "bmp.Width");
				Assert.AreEqual (100, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (110, rect.Width, "rect.Width");
				Assert.AreEqual (100, rect.Height, "rect.Height");

				Assert.AreEqual (110, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (100, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap8bitsFeatures_Gif89 ()
		{
			Bitmap8bitsFeatures (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/nature24bits.gif"));
		}

		[Test]
		public void Bitmap8bitsFeatures_Gif87 ()
		{
			Bitmap8bitsFeatures (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/nature24bits87.gif"));
		}

		private void Bitmap8bitsPixels (string filename)
		{
			using (Bitmap bmp = new Bitmap (filename)) {
#if false
				for (int x = 0; x < bmp.Width; x += 32) {
					for (int y = 0; y < bmp.Height; y += 32)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-10644802, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-12630705, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-14537409, bmp.GetPixel (0, 64).ToArgb (), "0,64");
				Assert.AreEqual (-14672099, bmp.GetPixel (0, 96).ToArgb (), "0,96");
				Assert.AreEqual (-526863, bmp.GetPixel (32, 0).ToArgb (), "32,0");
				Assert.AreEqual (-10263970, bmp.GetPixel (32, 32).ToArgb (), "32,32");
				Assert.AreEqual (-10461317, bmp.GetPixel (32, 64).ToArgb (), "32,64");
				Assert.AreEqual (-9722415, bmp.GetPixel (32, 96).ToArgb (), "32,96");
				Assert.AreEqual (-131076, bmp.GetPixel (64, 0).ToArgb (), "64,0");
				Assert.AreEqual (-2702435, bmp.GetPixel (64, 32).ToArgb (), "64,32");
				Assert.AreEqual (-6325922, bmp.GetPixel (64, 64).ToArgb (), "64,64");
				Assert.AreEqual (-12411924, bmp.GetPixel (64, 96).ToArgb (), "64,96");
				Assert.AreEqual (-131076, bmp.GetPixel (96, 0).ToArgb (), "96,0");
				Assert.AreEqual (-7766649, bmp.GetPixel (96, 32).ToArgb (), "96,32");
				Assert.AreEqual (-11512986, bmp.GetPixel (96, 64).ToArgb (), "96,64");
				Assert.AreEqual (-12616230, bmp.GetPixel (96, 96).ToArgb (), "96,96");
#endif
			}
		}

		[Test]
		public void Bitmap8bitsPixels_Gif89 ()
		{
			Bitmap8bitsPixels (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/nature24bits.gif"));
		}

		[Test]
		public void Bitmap8bitsPixels_Gif87 ()
		{
			Bitmap8bitsPixels (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/nature24bits87.gif"));
		}

		[Test]
		public void Bitmap8bitsData ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/nature24bits.gif");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					Assert.AreEqual (332, data.Stride, "Stride");
					int size = data.Height * data.Stride;
					unsafe {
						byte* scan = (byte*) data.Scan0;
#if false
						// 1009 is the first prime after 1000 (so we're not affected by a recurring pattern)
						for (int p = 0; p < size; p += 1009) {
							Console.WriteLine ("\t\t\t\t\t\tAssert.AreEqual ({0}, *(scan + {1}), \"{1}\");", *(scan + p), p);
						}
#else
						// sampling values from a well known bitmap
						Assert.AreEqual (190, *(scan + 0), "0");
						Assert.AreEqual (217, *(scan + 1009), "1009");
						Assert.AreEqual (120, *(scan + 2018), "2018");
						Assert.AreEqual (253, *(scan + 3027), "3027");
						Assert.AreEqual (233, *(scan + 4036), "4036");
						Assert.AreEqual (176, *(scan + 5045), "5045");
						Assert.AreEqual (151, *(scan + 6054), "6054");
						Assert.AreEqual (220, *(scan + 7063), "7063");
						Assert.AreEqual (139, *(scan + 8072), "8072");
						Assert.AreEqual (121, *(scan + 9081), "9081");
						Assert.AreEqual (160, *(scan + 10090), "10090");
						Assert.AreEqual (92, *(scan + 11099), "11099");
						Assert.AreEqual (96, *(scan + 12108), "12108");
						Assert.AreEqual (64, *(scan + 13117), "13117");
						Assert.AreEqual (156, *(scan + 14126), "14126");
						Assert.AreEqual (68, *(scan + 15135), "15135");
						Assert.AreEqual (156, *(scan + 16144), "16144");
						Assert.AreEqual (84, *(scan + 17153), "17153");
						Assert.AreEqual (55, *(scan + 18162), "18162");
						Assert.AreEqual (68, *(scan + 19171), "19171");
						Assert.AreEqual (116, *(scan + 20180), "20180");
						Assert.AreEqual (61, *(scan + 21189), "21189");
						Assert.AreEqual (69, *(scan + 22198), "22198");
						Assert.AreEqual (75, *(scan + 23207), "23207");
						Assert.AreEqual (61, *(scan + 24216), "24216");
						Assert.AreEqual (66, *(scan + 25225), "25225");
						Assert.AreEqual (40, *(scan + 26234), "26234");
						Assert.AreEqual (55, *(scan + 27243), "27243");
						Assert.AreEqual (53, *(scan + 28252), "28252");
						Assert.AreEqual (215, *(scan + 29261), "29261");
						Assert.AreEqual (99, *(scan + 30270), "30270");
						Assert.AreEqual (67, *(scan + 31279), "31279");
						Assert.AreEqual (142, *(scan + 32288), "32288");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		[Test]
		public void Interlaced ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/81773-interlaced.gif");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				for (int i = 0; i < 255; i++) {
					Color c = bmp.GetPixel (0, i);
					Assert.AreEqual (255, c.A, "A" + i.ToString ());
					Assert.AreEqual (i, c.R, "R" + i.ToString ());
					Assert.AreEqual (i, c.G, "G" + i.ToString ());
					Assert.AreEqual (i, c.B, "B" + i.ToString ());
				}
			}
		}

		private void Save (PixelFormat original, PixelFormat expected, bool exactColorCheck)
		{
			string sOutFile = String.Format ("linerect{0}-{1}.gif", getOutSufix (), expected.ToString ());

			// Save		
			Bitmap bmp = new Bitmap (100, 100, original);
			Graphics gr = Graphics.FromImage (bmp);

			using (Pen p = new Pen (Color.Red, 2)) {
				gr.DrawLine (p, 10.0F, 10.0F, 90.0F, 90.0F);
				gr.DrawRectangle (p, 10.0F, 10.0F, 80.0F, 80.0F);
			}

			try {
				bmp.Save (sOutFile, ImageFormat.Gif);

				// Load
				using (Bitmap bmpLoad = new Bitmap (sOutFile)) {
					Assert.AreEqual (expected, bmpLoad.PixelFormat, "PixelFormat");
					Color color = bmpLoad.GetPixel (10, 10);
					if (exactColorCheck) {
						Assert.AreEqual (Color.FromArgb (255, 255, 0, 0), color, "Red");
					} else {
// FIXME: we don't save a pure red (F8 instead of FF) into the file so the color-check assert will fail
// this is due to libgif's QuantizeBuffer. An alternative would be to make our own that checks if less than 256 colors
// are used in the bitmap (or else use QuantizeBuffer).
						Assert.AreEqual (255, color.A, "A");
						Assert.IsTrue (color.R >= 248, "R");
						Assert.AreEqual (0, color.G, "G");
						Assert.AreEqual (0, color.B, "B");
					}
				}
			}
			finally {
				gr.Dispose ();
				bmp.Dispose ();
				try {
					File.Delete (sOutFile);
				}
				catch {
				}
			}
		}

		[Test]
		public void Save_24bppRgb ()
		{
			Save (PixelFormat.Format24bppRgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		public void Save_32bppRgb ()
		{
			Save (PixelFormat.Format32bppRgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		public void Save_32bppArgb ()
		{
			Save (PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		public void Save_32bppPArgb ()
		{
			Save (PixelFormat.Format32bppPArgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus/cairo can't create a bitmap with this format
		public void Save_48bppRgb ()
		{
			Save (PixelFormat.Format48bppRgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus/cairo can't create a bitmap with this format
		public void Save_64bppArgb ()
		{
			Save (PixelFormat.Format64bppArgb, PixelFormat.Format8bppIndexed, false);
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus/cairo can't create a bitmap with this format
		public void Save_64bppPArgb ()
		{
			Save (PixelFormat.Format64bppPArgb, PixelFormat.Format8bppIndexed, false);
		}
	}
}
