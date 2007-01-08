//
// ICO Codec class testing unit
//
// Authors:
// 	Jordi Mas i Hernàndez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class IconCodecTest {

		/* Get suffix to add to the filename */
		internal string getOutSufix ()
		{
			string s;

			int p = (int) Environment.OSVersion.Platform;
			if ((p == 4) || (p == 128))
				s = "-unix";
			else
				s = "-windows";

			if (Type.GetType ("Mono.Runtime", false) == null)
				s += "-msnet";
			else
				s += "-mono";

			return s;
		}

		/* Get the input directory depending on the runtime*/
		internal string getInFile (string file)
		{
			string sRslt = Path.GetFullPath ("../System.Drawing/" + file);

			if (!File.Exists (sRslt))
				sRslt = "Test/System.Drawing/" + file;

			return sRslt;
		}

		/* Checks bitmap features on a know 1bbp bitmap */
		[Test]
		[Category ("NotWorking")]
		public void Bitmap16Features ()
		{
			string sInFile = getInFile ("bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				// ??? why is it a 4bbp ?
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (16, bmp.Width, "bmp.Width");
				Assert.AreEqual (16, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (16, rect.Width, "rect.Width");
				Assert.AreEqual (16, rect.Height, "rect.Height");

				Assert.AreEqual (16, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (16, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Bitmap16Pixels ()
		{
			string sInFile = getInFile ("bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 4) {
					for (int y = 0; y < bmp.Height; y += 4)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (0, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (0, bmp.GetPixel (0, 4).ToArgb (), "0,4");
				Assert.AreEqual (0, bmp.GetPixel (0, 8).ToArgb (), "0,8");
				Assert.AreEqual (0, bmp.GetPixel (0, 12).ToArgb (), "0,12");
				Assert.AreEqual (0, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (-256, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (-256, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (-8355840, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (0, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (-256, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (-256, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (-256, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (0, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (0, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-8355840, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (0, bmp.GetPixel (12, 12).ToArgb (), "12,12");
#endif
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Bitmat16Data ()
		{
			string sInFile = getInFile ("bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					int size = data.Height * data.Stride;
					unsafe {
						byte* scan = (byte*) data.Scan0;
#if false
						// 13 is prime (so we're not affected by a recurring pattern)
						for (int p = 0; p < size; p += 13) {
							Console.WriteLine ("\t\t\t\t\t\tAssert.AreEqual ({0}, *(scan + {1}), \"{1}\");", *(scan + p), p);
						}
#else
						// sampling values from a well known bitmap
						Assert.AreEqual (0, *(scan + 0), "0");
						Assert.AreEqual (0, *(scan + 13), "13");
						Assert.AreEqual (0, *(scan + 26), "26");
						Assert.AreEqual (0, *(scan + 39), "39");
						Assert.AreEqual (0, *(scan + 52), "52");
						Assert.AreEqual (0, *(scan + 65), "65");
						Assert.AreEqual (0, *(scan + 78), "78");
						Assert.AreEqual (0, *(scan + 91), "91");
						Assert.AreEqual (0, *(scan + 104), "104");
						Assert.AreEqual (0, *(scan + 117), "117");
						Assert.AreEqual (0, *(scan + 130), "130");
						Assert.AreEqual (0, *(scan + 143), "143");
						Assert.AreEqual (0, *(scan + 156), "156");
						Assert.AreEqual (255, *(scan + 169), "169");
						Assert.AreEqual (0, *(scan + 182), "182");
						Assert.AreEqual (0, *(scan + 195), "195");
						Assert.AreEqual (255, *(scan + 208), "208");
						Assert.AreEqual (255, *(scan + 221), "221");
						Assert.AreEqual (0, *(scan + 234), "234");
						Assert.AreEqual (128, *(scan + 247), "247");
						Assert.AreEqual (0, *(scan + 260), "260");
						Assert.AreEqual (0, *(scan + 273), "273");
						Assert.AreEqual (0, *(scan + 286), "286");
						Assert.AreEqual (255, *(scan + 299), "299");
						Assert.AreEqual (0, *(scan + 312), "312");
						Assert.AreEqual (128, *(scan + 325), "325");
						Assert.AreEqual (0, *(scan + 338), "338");
						Assert.AreEqual (0, *(scan + 351), "351");
						Assert.AreEqual (255, *(scan + 364), "364");
						Assert.AreEqual (0, *(scan + 377), "377");
						Assert.AreEqual (0, *(scan + 390), "390");
						Assert.AreEqual (255, *(scan + 403), "403");
						Assert.AreEqual (255, *(scan + 416), "416");
						Assert.AreEqual (0, *(scan + 429), "429");
						Assert.AreEqual (255, *(scan + 442), "442");
						Assert.AreEqual (0, *(scan + 455), "455");
						Assert.AreEqual (0, *(scan + 468), "468");
						Assert.AreEqual (0, *(scan + 481), "481");
						Assert.AreEqual (255, *(scan + 494), "494");
						Assert.AreEqual (0, *(scan + 507), "507");
						Assert.AreEqual (0, *(scan + 520), "520");
						Assert.AreEqual (0, *(scan + 533), "533");
						Assert.AreEqual (0, *(scan + 546), "546");
						Assert.AreEqual (255, *(scan + 559), "559");
						Assert.AreEqual (0, *(scan + 572), "572");
						Assert.AreEqual (0, *(scan + 585), "585");
						Assert.AreEqual (255, *(scan + 598), "598");
						Assert.AreEqual (0, *(scan + 611), "611");
						Assert.AreEqual (0, *(scan + 624), "624");
						Assert.AreEqual (0, *(scan + 637), "637");
						Assert.AreEqual (128, *(scan + 650), "650");
						Assert.AreEqual (0, *(scan + 663), "663");
						Assert.AreEqual (0, *(scan + 676), "676");
						Assert.AreEqual (0, *(scan + 689), "689");
						Assert.AreEqual (0, *(scan + 702), "702");
						Assert.AreEqual (0, *(scan + 715), "715");
						Assert.AreEqual (0, *(scan + 728), "728");
						Assert.AreEqual (0, *(scan + 741), "741");
						Assert.AreEqual (0, *(scan + 754), "754");
						Assert.AreEqual (0, *(scan + 767), "767");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		/* Checks bitmap features on a know 1bbp bitmap */
		[Test]
		[Category ("NotWorking")]
		public void Bitmap32Features ()
		{
			string sInFile = getInFile ("bitmaps/VisualPng.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				// ??? why is it a 4bbp ?
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (32, bmp.Width, "bmp.Width");
				Assert.AreEqual (32, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (32, rect.Width, "rect.Width");
				Assert.AreEqual (32, rect.Height, "rect.Height");

				Assert.AreEqual (32, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (32, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Bitmap32Pixels ()
		{
			string sInFile = getInFile ("bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 4) {
					for (int y = 0; y < bmp.Height; y += 4)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (0, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (0, bmp.GetPixel (0, 4).ToArgb (), "0,4");
				Assert.AreEqual (0, bmp.GetPixel (0, 8).ToArgb (), "0,8");
				Assert.AreEqual (0, bmp.GetPixel (0, 12).ToArgb (), "0,12");
				Assert.AreEqual (0, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (-256, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (-256, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (-8355840, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (0, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (-256, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (-256, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (-256, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (0, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (0, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-8355840, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (0, bmp.GetPixel (12, 12).ToArgb (), "12,12");
#endif
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Bitmat32Data ()
		{
			string sInFile = getInFile ("bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					int size = data.Height * data.Stride;
					unsafe {
						byte* scan = (byte*) data.Scan0;
#if false
						// 13 is prime (so we're not affected by a recurring pattern)
						for (int p = 0; p < size; p += 13) {
							Console.WriteLine ("\t\t\t\t\t\tAssert.AreEqual ({0}, *(scan + {1}), \"{1}\");", *(scan + p), p);
						}
#else
						// sampling values from a well known bitmap
						Assert.AreEqual (0, *(scan + 0), "0");
						Assert.AreEqual (0, *(scan + 13), "13");
						Assert.AreEqual (0, *(scan + 26), "26");
						Assert.AreEqual (0, *(scan + 39), "39");
						Assert.AreEqual (0, *(scan + 52), "52");
						Assert.AreEqual (0, *(scan + 65), "65");
						Assert.AreEqual (0, *(scan + 78), "78");
						Assert.AreEqual (0, *(scan + 91), "91");
						Assert.AreEqual (0, *(scan + 104), "104");
						Assert.AreEqual (0, *(scan + 117), "117");
						Assert.AreEqual (0, *(scan + 130), "130");
						Assert.AreEqual (0, *(scan + 143), "143");
						Assert.AreEqual (0, *(scan + 156), "156");
						Assert.AreEqual (255, *(scan + 169), "169");
						Assert.AreEqual (0, *(scan + 182), "182");
						Assert.AreEqual (0, *(scan + 195), "195");
						Assert.AreEqual (255, *(scan + 208), "208");
						Assert.AreEqual (255, *(scan + 221), "221");
						Assert.AreEqual (0, *(scan + 234), "234");
						Assert.AreEqual (128, *(scan + 247), "247");
						Assert.AreEqual (0, *(scan + 260), "260");
						Assert.AreEqual (0, *(scan + 273), "273");
						Assert.AreEqual (0, *(scan + 286), "286");
						Assert.AreEqual (255, *(scan + 299), "299");
						Assert.AreEqual (0, *(scan + 312), "312");
						Assert.AreEqual (128, *(scan + 325), "325");
						Assert.AreEqual (0, *(scan + 338), "338");
						Assert.AreEqual (0, *(scan + 351), "351");
						Assert.AreEqual (255, *(scan + 364), "364");
						Assert.AreEqual (0, *(scan + 377), "377");
						Assert.AreEqual (0, *(scan + 390), "390");
						Assert.AreEqual (255, *(scan + 403), "403");
						Assert.AreEqual (255, *(scan + 416), "416");
						Assert.AreEqual (0, *(scan + 429), "429");
						Assert.AreEqual (255, *(scan + 442), "442");
						Assert.AreEqual (0, *(scan + 455), "455");
						Assert.AreEqual (0, *(scan + 468), "468");
						Assert.AreEqual (0, *(scan + 481), "481");
						Assert.AreEqual (255, *(scan + 494), "494");
						Assert.AreEqual (0, *(scan + 507), "507");
						Assert.AreEqual (0, *(scan + 520), "520");
						Assert.AreEqual (0, *(scan + 533), "533");
						Assert.AreEqual (0, *(scan + 546), "546");
						Assert.AreEqual (255, *(scan + 559), "559");
						Assert.AreEqual (0, *(scan + 572), "572");
						Assert.AreEqual (0, *(scan + 585), "585");
						Assert.AreEqual (255, *(scan + 598), "598");
						Assert.AreEqual (0, *(scan + 611), "611");
						Assert.AreEqual (0, *(scan + 624), "624");
						Assert.AreEqual (0, *(scan + 637), "637");
						Assert.AreEqual (128, *(scan + 650), "650");
						Assert.AreEqual (0, *(scan + 663), "663");
						Assert.AreEqual (0, *(scan + 676), "676");
						Assert.AreEqual (0, *(scan + 689), "689");
						Assert.AreEqual (0, *(scan + 702), "702");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		private void Save (PixelFormat original, PixelFormat expected, bool colorCheck)
		{
			string sOutFile = "linerect" + getOutSufix () + ".ico";

			// Save		
			Bitmap bmp = new Bitmap (100, 100, original);
			Graphics gr = Graphics.FromImage (bmp);

			using (Pen p = new Pen (Color.Red, 2)) {
				gr.DrawLine (p, 10.0F, 10.0F, 90.0F, 90.0F);
				gr.DrawRectangle (p, 10.0F, 10.0F, 80.0F, 80.0F);
			}

			try {
				// there's no encoder, so we're not saving a ICO but the alpha 
				// bit get sets so it's not like saving a bitmap either
				bmp.Save (sOutFile, ImageFormat.Icon);

				// Load
				using (Bitmap bmpLoad = new Bitmap (sOutFile)) {
					Assert.AreEqual (ImageFormat.Png, bmpLoad.RawFormat, "Png");
					Assert.AreEqual (expected, bmpLoad.PixelFormat, "PixelFormat");
					if (colorCheck) {
						Color color = bmpLoad.GetPixel (10, 10);
						Assert.AreEqual (Color.FromArgb (255, 255, 0, 0), color, "Red");
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
			Save (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, true);
		}

		[Test]
		public void Save_32bppRgb ()
		{
			Save (PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb, true);
		}

		[Test]
		public void Save_32bppArgb ()
		{
			Save (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, true);
		}

		[Test]
		public void Save_32bppPArgb ()
		{
			Save (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb, true);
		}

		[Test]
		[Category ("NotWorking")]
		public void Save_48bppRgb ()
		{
			Save (PixelFormat.Format48bppRgb, PixelFormat.Format48bppRgb, false);
		}

		[Test]
		[Category ("NotWorking")]
		public void Save_64bppArgb ()
		{
			Save (PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb, false);
		}

		[Test]
		[Category ("NotWorking")]
		public void Save_64bppPArgb ()
		{
			Save (PixelFormat.Format64bppPArgb, PixelFormat.Format64bppArgb, false);
		}
	}
}
