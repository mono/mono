//
// ICO Codec class testing unit
//
// Authors:
// 	Jordi Mas i Hern�ndez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
	public class IconCodecTest {

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

		[Test]
		public void Image16 ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico");
			using (Image image = Image.FromFile (sInFile)) {
				Assert.IsTrue (image.RawFormat.Equals (ImageFormat.Icon), "Icon");
				// note that image is "promoted" to 32bits
				Assert.AreEqual (PixelFormat.Format32bppArgb, image.PixelFormat);
				Assert.AreEqual (73746, image.Flags, "bmp.Flags");
				Assert.AreEqual (16, image.Palette.Entries.Length, "Palette");

				using (Bitmap bmp = new Bitmap (image)) {
					Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.MemoryBmp), "Icon");
					Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
					Assert.AreEqual (2, bmp.Flags, "bmp.Flags");
					Assert.AreEqual (0, bmp.Palette.Entries.Length, "Palette");
				}
			}
		}

		// simley.ico has 48x48, 32x32 and 16x16 images (in that order)
		[Test]
		public void Bitmap16Features ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/smiley.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				// note that image is "promoted" to 32bits
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (16, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (-16777216, bmp.Palette.Entries[0].ToArgb (), "Palette#0");
				Assert.AreEqual (-16777216, bmp.Palette.Entries[1].ToArgb (), "Palette#1");
				Assert.AreEqual (-16744448, bmp.Palette.Entries[2].ToArgb (), "Palette#2");
				Assert.AreEqual (-8355840, bmp.Palette.Entries[3].ToArgb (), "Palette#3");
				Assert.AreEqual (-16777088, bmp.Palette.Entries[4].ToArgb (), "Palette#4");
				Assert.AreEqual (-8388480, bmp.Palette.Entries[5].ToArgb (), "Palette#5");
				Assert.AreEqual (-16744320, bmp.Palette.Entries[6].ToArgb (), "Palette#6");
				Assert.AreEqual (-4144960, bmp.Palette.Entries[7].ToArgb (), "Palette#7");
				Assert.AreEqual (-8355712, bmp.Palette.Entries[8].ToArgb (), "Palette#8");
				Assert.AreEqual (-65536, bmp.Palette.Entries[9].ToArgb (), "Palette#9");
				Assert.AreEqual (-16711936, bmp.Palette.Entries[10].ToArgb (), "Palette#10");
				Assert.AreEqual (-256, bmp.Palette.Entries[11].ToArgb (), "Palette#11");
				Assert.AreEqual (-16776961, bmp.Palette.Entries[12].ToArgb (), "Palette#12");
				Assert.AreEqual (-65281, bmp.Palette.Entries[13].ToArgb (), "Palette#13");
				Assert.AreEqual (-16711681, bmp.Palette.Entries[14].ToArgb (), "Palette#14");
				Assert.AreEqual (-1, bmp.Palette.Entries[15].ToArgb (), "Palette#15");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
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
		public void Bitmap16Pixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/smiley.ico");
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
		public void Bitmap16Data ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/smiley.ico");
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

		// VisualPng.ico only has a 32x32 size available
		[Test]
		public void Bitmap32Features ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/VisualPng.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (16, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (-16777216, bmp.Palette.Entries[0].ToArgb (), "Palette#0");
				Assert.AreEqual (-8388608, bmp.Palette.Entries[1].ToArgb (), "Palette#1");
				Assert.AreEqual (-16744448, bmp.Palette.Entries[2].ToArgb (), "Palette#2");
				Assert.AreEqual (-8355840, bmp.Palette.Entries[3].ToArgb (), "Palette#3");
				Assert.AreEqual (-16777088, bmp.Palette.Entries[4].ToArgb (), "Palette#4");
				Assert.AreEqual (-8388480, bmp.Palette.Entries[5].ToArgb (), "Palette#5");
				Assert.AreEqual (-16744320, bmp.Palette.Entries[6].ToArgb (), "Palette#6");
				Assert.AreEqual (-4144960, bmp.Palette.Entries[7].ToArgb (), "Palette#7");
				Assert.AreEqual (-8355712, bmp.Palette.Entries[8].ToArgb (), "Palette#8");
				Assert.AreEqual (-65536, bmp.Palette.Entries[9].ToArgb (), "Palette#9");
				Assert.AreEqual (-16711936, bmp.Palette.Entries[10].ToArgb (), "Palette#10");
				Assert.AreEqual (-256, bmp.Palette.Entries[11].ToArgb (), "Palette#11");
				Assert.AreEqual (-16776961, bmp.Palette.Entries[12].ToArgb (), "Palette#12");
				Assert.AreEqual (-65281, bmp.Palette.Entries[13].ToArgb (), "Palette#13");
				Assert.AreEqual (-16711681, bmp.Palette.Entries[14].ToArgb (), "Palette#14");
				Assert.AreEqual (-1, bmp.Palette.Entries[15].ToArgb (), "Palette#15");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
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
		public void Bitmap32Pixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/VisualPng.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 4) {
					for (int y = 0; y < bmp.Height; y += 4)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (0, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-8388608, bmp.GetPixel (0, 4).ToArgb (), "0,4");
				Assert.AreEqual (0, bmp.GetPixel (0, 8).ToArgb (), "0,8");
				Assert.AreEqual (0, bmp.GetPixel (0, 12).ToArgb (), "0,12");
				Assert.AreEqual (0, bmp.GetPixel (0, 16).ToArgb (), "0,16");
				Assert.AreEqual (0, bmp.GetPixel (0, 20).ToArgb (), "0,20");
				Assert.AreEqual (0, bmp.GetPixel (0, 24).ToArgb (), "0,24");
				Assert.AreEqual (0, bmp.GetPixel (0, 28).ToArgb (), "0,28");
				Assert.AreEqual (0, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (0, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (0, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (0, bmp.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0, bmp.GetPixel (4, 20).ToArgb (), "4,20");
				Assert.AreEqual (0, bmp.GetPixel (4, 24).ToArgb (), "4,24");
				Assert.AreEqual (0, bmp.GetPixel (4, 28).ToArgb (), "4,28");
				Assert.AreEqual (0, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (0, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (0, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (0, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (0, bmp.GetPixel (8, 16).ToArgb (), "8,16");
				Assert.AreEqual (-65536, bmp.GetPixel (8, 20).ToArgb (), "8,20");
				Assert.AreEqual (0, bmp.GetPixel (8, 24).ToArgb (), "8,24");
				Assert.AreEqual (0, bmp.GetPixel (8, 28).ToArgb (), "8,28");
				Assert.AreEqual (0, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (0, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-8388608, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (0, bmp.GetPixel (12, 12).ToArgb (), "12,12");
				Assert.AreEqual (0, bmp.GetPixel (12, 16).ToArgb (), "12,16");
				Assert.AreEqual (-65536, bmp.GetPixel (12, 20).ToArgb (), "12,20");
				Assert.AreEqual (0, bmp.GetPixel (12, 24).ToArgb (), "12,24");
				Assert.AreEqual (0, bmp.GetPixel (12, 28).ToArgb (), "12,28");
				Assert.AreEqual (0, bmp.GetPixel (16, 0).ToArgb (), "16,0");
				Assert.AreEqual (0, bmp.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0, bmp.GetPixel (16, 8).ToArgb (), "16,8");
				Assert.AreEqual (0, bmp.GetPixel (16, 12).ToArgb (), "16,12");
				Assert.AreEqual (0, bmp.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0, bmp.GetPixel (16, 20).ToArgb (), "16,20");
				Assert.AreEqual (-65536, bmp.GetPixel (16, 24).ToArgb (), "16,24");
				Assert.AreEqual (0, bmp.GetPixel (16, 28).ToArgb (), "16,28");
				Assert.AreEqual (0, bmp.GetPixel (20, 0).ToArgb (), "20,0");
				Assert.AreEqual (0, bmp.GetPixel (20, 4).ToArgb (), "20,4");
				Assert.AreEqual (-8388608, bmp.GetPixel (20, 8).ToArgb (), "20,8");
				Assert.AreEqual (0, bmp.GetPixel (20, 12).ToArgb (), "20,12");
				Assert.AreEqual (0, bmp.GetPixel (20, 16).ToArgb (), "20,16");
				Assert.AreEqual (0, bmp.GetPixel (20, 20).ToArgb (), "20,20");
				Assert.AreEqual (0, bmp.GetPixel (20, 24).ToArgb (), "20,24");
				Assert.AreEqual (0, bmp.GetPixel (20, 28).ToArgb (), "20,28");
				Assert.AreEqual (0, bmp.GetPixel (24, 0).ToArgb (), "24,0");
				Assert.AreEqual (0, bmp.GetPixel (24, 4).ToArgb (), "24,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (24, 8).ToArgb (), "24,8");
				Assert.AreEqual (0, bmp.GetPixel (24, 12).ToArgb (), "24,12");
				Assert.AreEqual (0, bmp.GetPixel (24, 16).ToArgb (), "24,16");
				Assert.AreEqual (0, bmp.GetPixel (24, 20).ToArgb (), "24,20");
				Assert.AreEqual (0, bmp.GetPixel (24, 24).ToArgb (), "24,24");
				Assert.AreEqual (0, bmp.GetPixel (24, 28).ToArgb (), "24,28");
				Assert.AreEqual (0, bmp.GetPixel (28, 0).ToArgb (), "28,0");
				Assert.AreEqual (0, bmp.GetPixel (28, 4).ToArgb (), "28,4");
				Assert.AreEqual (0, bmp.GetPixel (28, 8).ToArgb (), "28,8");
				Assert.AreEqual (0, bmp.GetPixel (28, 12).ToArgb (), "28,12");
				Assert.AreEqual (0, bmp.GetPixel (28, 16).ToArgb (), "28,16");
				Assert.AreEqual (0, bmp.GetPixel (28, 20).ToArgb (), "28,20");
				Assert.AreEqual (-8388608, bmp.GetPixel (28, 24).ToArgb (), "28,24");
				Assert.AreEqual (0, bmp.GetPixel (28, 28).ToArgb (), "28,28");
#endif
			}
		}

		[Test]
		public void Bitmap32Data ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/VisualPng.ico");
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
						Assert.AreEqual (0, *(scan + 169), "169");
						Assert.AreEqual (0, *(scan + 182), "182");
						Assert.AreEqual (0, *(scan + 195), "195");
						Assert.AreEqual (0, *(scan + 208), "208");
						Assert.AreEqual (0, *(scan + 221), "221");
						Assert.AreEqual (0, *(scan + 234), "234");
						Assert.AreEqual (0, *(scan + 247), "247");
						Assert.AreEqual (0, *(scan + 260), "260");
						Assert.AreEqual (0, *(scan + 273), "273");
						Assert.AreEqual (0, *(scan + 286), "286");
						Assert.AreEqual (0, *(scan + 299), "299");
						Assert.AreEqual (0, *(scan + 312), "312");
						Assert.AreEqual (0, *(scan + 325), "325");
						Assert.AreEqual (0, *(scan + 338), "338");
						Assert.AreEqual (0, *(scan + 351), "351");
						Assert.AreEqual (0, *(scan + 364), "364");
						Assert.AreEqual (0, *(scan + 377), "377");
						Assert.AreEqual (0, *(scan + 390), "390");
						Assert.AreEqual (0, *(scan + 403), "403");
						Assert.AreEqual (0, *(scan + 416), "416");
						Assert.AreEqual (0, *(scan + 429), "429");
						Assert.AreEqual (0, *(scan + 442), "442");
						Assert.AreEqual (0, *(scan + 455), "455");
						Assert.AreEqual (0, *(scan + 468), "468");
						Assert.AreEqual (0, *(scan + 481), "481");
						Assert.AreEqual (128, *(scan + 494), "494");
						Assert.AreEqual (0, *(scan + 507), "507");
						Assert.AreEqual (0, *(scan + 520), "520");
						Assert.AreEqual (0, *(scan + 533), "533");
						Assert.AreEqual (0, *(scan + 546), "546");
						Assert.AreEqual (0, *(scan + 559), "559");
						Assert.AreEqual (128, *(scan + 572), "572");
						Assert.AreEqual (0, *(scan + 585), "585");
						Assert.AreEqual (0, *(scan + 598), "598");
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
						Assert.AreEqual (0, *(scan + 780), "780");
						Assert.AreEqual (0, *(scan + 793), "793");
						Assert.AreEqual (128, *(scan + 806), "806");
						Assert.AreEqual (0, *(scan + 819), "819");
						Assert.AreEqual (0, *(scan + 832), "832");
						Assert.AreEqual (128, *(scan + 845), "845");
						Assert.AreEqual (0, *(scan + 858), "858");
						Assert.AreEqual (0, *(scan + 871), "871");
						Assert.AreEqual (0, *(scan + 884), "884");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		// 48x48x1.ico only has a 48x48 size available
		[Test]
		public void Bitmap48Features ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/48x48x1.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (2, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (-16777216, bmp.Palette.Entries[0].ToArgb (), "Palette#0");
				Assert.AreEqual (-1, bmp.Palette.Entries[1].ToArgb (), "Palette#1");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
				Assert.AreEqual (48, bmp.Width, "bmp.Width");
				Assert.AreEqual (48, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (48, rect.Width, "rect.Width");
				Assert.AreEqual (48, rect.Height, "rect.Height");

				Assert.AreEqual (48, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (48, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap48Pixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/48x48x1.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 4) {
					for (int y = 0; y < bmp.Height; y += 4)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 4).ToArgb (), "0,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 8).ToArgb (), "0,8");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 12).ToArgb (), "0,12");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 16).ToArgb (), "0,16");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 20).ToArgb (), "0,20");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 24).ToArgb (), "0,24");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 28).ToArgb (), "0,28");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 36).ToArgb (), "0,36");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 40).ToArgb (), "0,40");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 44).ToArgb (), "0,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (0, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (0, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (0, bmp.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0, bmp.GetPixel (4, 20).ToArgb (), "4,20");
				Assert.AreEqual (0, bmp.GetPixel (4, 24).ToArgb (), "4,24");
				Assert.AreEqual (0, bmp.GetPixel (4, 28).ToArgb (), "4,28");
				Assert.AreEqual (0, bmp.GetPixel (4, 32).ToArgb (), "4,32");
				Assert.AreEqual (0, bmp.GetPixel (4, 36).ToArgb (), "4,36");
				Assert.AreEqual (0, bmp.GetPixel (4, 40).ToArgb (), "4,40");
				Assert.AreEqual (-1, bmp.GetPixel (4, 44).ToArgb (), "4,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (0, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 16).ToArgb (), "8,16");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 20).ToArgb (), "8,20");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 24).ToArgb (), "8,24");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 28).ToArgb (), "8,28");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 32).ToArgb (), "8,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 36).ToArgb (), "8,36");
				Assert.AreEqual (-16777216, bmp.GetPixel (8, 40).ToArgb (), "8,40");
				Assert.AreEqual (-1, bmp.GetPixel (8, 44).ToArgb (), "8,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (0, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (-1, bmp.GetPixel (12, 12).ToArgb (), "12,12");
				Assert.AreEqual (-1, bmp.GetPixel (12, 16).ToArgb (), "12,16");
				Assert.AreEqual (-1, bmp.GetPixel (12, 20).ToArgb (), "12,20");
				Assert.AreEqual (-1, bmp.GetPixel (12, 24).ToArgb (), "12,24");
				Assert.AreEqual (-1, bmp.GetPixel (12, 28).ToArgb (), "12,28");
				Assert.AreEqual (-1, bmp.GetPixel (12, 32).ToArgb (), "12,32");
				Assert.AreEqual (0, bmp.GetPixel (12, 36).ToArgb (), "12,36");
				Assert.AreEqual (-16777216, bmp.GetPixel (12, 40).ToArgb (), "12,40");
				Assert.AreEqual (-1, bmp.GetPixel (12, 44).ToArgb (), "12,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (16, 0).ToArgb (), "16,0");
				Assert.AreEqual (0, bmp.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (16, 8).ToArgb (), "16,8");
				Assert.AreEqual (-1, bmp.GetPixel (16, 12).ToArgb (), "16,12");
				Assert.AreEqual (0, bmp.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0, bmp.GetPixel (16, 20).ToArgb (), "16,20");
				Assert.AreEqual (0, bmp.GetPixel (16, 24).ToArgb (), "16,24");
				Assert.AreEqual (0, bmp.GetPixel (16, 28).ToArgb (), "16,28");
				Assert.AreEqual (-1, bmp.GetPixel (16, 32).ToArgb (), "16,32");
				Assert.AreEqual (0, bmp.GetPixel (16, 36).ToArgb (), "16,36");
				Assert.AreEqual (-16777216, bmp.GetPixel (16, 40).ToArgb (), "16,40");
				Assert.AreEqual (-1, bmp.GetPixel (16, 44).ToArgb (), "16,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (20, 0).ToArgb (), "20,0");
				Assert.AreEqual (0, bmp.GetPixel (20, 4).ToArgb (), "20,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (20, 8).ToArgb (), "20,8");
				Assert.AreEqual (-1, bmp.GetPixel (20, 12).ToArgb (), "20,12");
				Assert.AreEqual (0, bmp.GetPixel (20, 16).ToArgb (), "20,16");
				Assert.AreEqual (-16777216, bmp.GetPixel (20, 20).ToArgb (), "20,20");
				Assert.AreEqual (-16777216, bmp.GetPixel (20, 24).ToArgb (), "20,24");
				Assert.AreEqual (0, bmp.GetPixel (20, 28).ToArgb (), "20,28");
				Assert.AreEqual (-1, bmp.GetPixel (20, 32).ToArgb (), "20,32");
				Assert.AreEqual (0, bmp.GetPixel (20, 36).ToArgb (), "20,36");
				Assert.AreEqual (-16777216, bmp.GetPixel (20, 40).ToArgb (), "20,40");
				Assert.AreEqual (-1, bmp.GetPixel (20, 44).ToArgb (), "20,44");
				Assert.AreEqual (-16777216, bmp.GetPixel (24, 0).ToArgb (), "24,0");
				Assert.AreEqual (0, bmp.GetPixel (24, 4).ToArgb (), "24,4");
				Assert.AreEqual (-16777216, bmp.GetPixel (24, 8).ToArgb (), "24,8");
				Assert.AreEqual (-1, bmp.GetPixel (24, 12).ToArgb (), "24,12");
				Assert.AreEqual (0, bmp.GetPixel (24, 16).ToArgb (), "24,16");
#endif
			}
		}

		[Test]
		public void Bitmap48Data ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/48x48x1.ico");
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
						Assert.AreEqual (0, *(scan + 169), "169");
						Assert.AreEqual (0, *(scan + 182), "182");
						Assert.AreEqual (0, *(scan + 195), "195");
						Assert.AreEqual (0, *(scan + 208), "208");
						Assert.AreEqual (0, *(scan + 221), "221");
						Assert.AreEqual (0, *(scan + 234), "234");
						Assert.AreEqual (0, *(scan + 247), "247");
						Assert.AreEqual (0, *(scan + 260), "260");
						Assert.AreEqual (0, *(scan + 273), "273");
						Assert.AreEqual (0, *(scan + 286), "286");
						Assert.AreEqual (255, *(scan + 299), "299");
						Assert.AreEqual (255, *(scan + 312), "312");
						Assert.AreEqual (255, *(scan + 325), "325");
						Assert.AreEqual (255, *(scan + 338), "338");
						Assert.AreEqual (255, *(scan + 351), "351");
						Assert.AreEqual (255, *(scan + 364), "364");
						Assert.AreEqual (255, *(scan + 377), "377");
						Assert.AreEqual (255, *(scan + 390), "390");
						Assert.AreEqual (255, *(scan + 403), "403");
						Assert.AreEqual (255, *(scan + 416), "416");
						Assert.AreEqual (0, *(scan + 429), "429");
						Assert.AreEqual (255, *(scan + 442), "442");
						Assert.AreEqual (255, *(scan + 455), "455");
						Assert.AreEqual (255, *(scan + 468), "468");
						Assert.AreEqual (255, *(scan + 481), "481");
						Assert.AreEqual (255, *(scan + 494), "494");
						Assert.AreEqual (255, *(scan + 507), "507");
						Assert.AreEqual (255, *(scan + 520), "520");
						Assert.AreEqual (255, *(scan + 533), "533");
						Assert.AreEqual (255, *(scan + 546), "546");
						Assert.AreEqual (255, *(scan + 559), "559");
						Assert.AreEqual (0, *(scan + 572), "572");
						Assert.AreEqual (255, *(scan + 585), "585");
						Assert.AreEqual (0, *(scan + 598), "598");
						Assert.AreEqual (0, *(scan + 611), "611");
						Assert.AreEqual (0, *(scan + 624), "624");
						Assert.AreEqual (0, *(scan + 637), "637");
						Assert.AreEqual (0, *(scan + 650), "650");
						Assert.AreEqual (0, *(scan + 663), "663");
						Assert.AreEqual (0, *(scan + 676), "676");
						Assert.AreEqual (0, *(scan + 689), "689");
						Assert.AreEqual (0, *(scan + 702), "702");
						Assert.AreEqual (0, *(scan + 715), "715");
						Assert.AreEqual (255, *(scan + 728), "728");
						Assert.AreEqual (0, *(scan + 741), "741");
						Assert.AreEqual (0, *(scan + 754), "754");
						Assert.AreEqual (0, *(scan + 767), "767");
						Assert.AreEqual (0, *(scan + 780), "780");
						Assert.AreEqual (0, *(scan + 793), "793");
						Assert.AreEqual (0, *(scan + 806), "806");
						Assert.AreEqual (0, *(scan + 819), "819");
						Assert.AreEqual (0, *(scan + 832), "832");
						Assert.AreEqual (0, *(scan + 845), "845");
						Assert.AreEqual (0, *(scan + 858), "858");
						Assert.AreEqual (255, *(scan + 871), "871");
						Assert.AreEqual (0, *(scan + 884), "884");
						Assert.AreEqual (0, *(scan + 897), "897");
						Assert.AreEqual (0, *(scan + 910), "910");
						Assert.AreEqual (0, *(scan + 923), "923");
						Assert.AreEqual (0, *(scan + 936), "936");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		// 64x64x256 only has a 64x64 size available
		[Test]
		public void Bitmap64Features ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/64x64x256.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (256, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
				Assert.AreEqual (64, bmp.Width, "bmp.Width");
				Assert.AreEqual (64, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (64, rect.Width, "rect.Width");
				Assert.AreEqual (64, rect.Height, "rect.Height");

				Assert.AreEqual (64, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (64, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap64Pixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/64x64x256.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 4) {
					for (int y = 0; y < bmp.Height; y += 4)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-65383, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 4).ToArgb (), "0,4");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 8).ToArgb (), "0,8");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 12).ToArgb (), "0,12");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 16).ToArgb (), "0,16");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 20).ToArgb (), "0,20");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 24).ToArgb (), "0,24");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 28).ToArgb (), "0,28");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 36).ToArgb (), "0,36");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 40).ToArgb (), "0,40");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 44).ToArgb (), "0,44");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 48).ToArgb (), "0,48");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 52).ToArgb (), "0,52");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 56).ToArgb (), "0,56");
				Assert.AreEqual (-65383, bmp.GetPixel (0, 60).ToArgb (), "0,60");
				Assert.AreEqual (-65383, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 20).ToArgb (), "4,20");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 24).ToArgb (), "4,24");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 28).ToArgb (), "4,28");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 32).ToArgb (), "4,32");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 36).ToArgb (), "4,36");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 40).ToArgb (), "4,40");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 44).ToArgb (), "4,44");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 48).ToArgb (), "4,48");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 52).ToArgb (), "4,52");
				Assert.AreEqual (-10079335, bmp.GetPixel (4, 56).ToArgb (), "4,56");
				Assert.AreEqual (0, bmp.GetPixel (4, 60).ToArgb (), "4,60");
				Assert.AreEqual (-65383, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (-10079335, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 16).ToArgb (), "8,16");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 20).ToArgb (), "8,20");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 24).ToArgb (), "8,24");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 28).ToArgb (), "8,28");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 32).ToArgb (), "8,32");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 36).ToArgb (), "8,36");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 40).ToArgb (), "8,40");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 44).ToArgb (), "8,44");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 48).ToArgb (), "8,48");
				Assert.AreEqual (-3342490, bmp.GetPixel (8, 52).ToArgb (), "8,52");
				Assert.AreEqual (0, bmp.GetPixel (8, 56).ToArgb (), "8,56");
				Assert.AreEqual (0, bmp.GetPixel (8, 60).ToArgb (), "8,60");
				Assert.AreEqual (-65383, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (-10079335, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-3342490, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 12).ToArgb (), "12,12");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 16).ToArgb (), "12,16");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 20).ToArgb (), "12,20");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 24).ToArgb (), "12,24");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 28).ToArgb (), "12,28");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 32).ToArgb (), "12,32");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 36).ToArgb (), "12,36");
				Assert.AreEqual (-33664, bmp.GetPixel (12, 40).ToArgb (), "12,40");
#endif
			}
		}

		[Test]
		public void Bitmap64Data ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/64x64x256.ico");
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
						// 97 is prime (so we're not affected by a recurring pattern)
						for (int p = 0; p < size; p += 97) {
							Console.WriteLine ("\t\t\t\t\t\tAssert.AreEqual ({0}, *(scan + {1}), \"{1}\");", *(scan + p), p);
						}
#else
						// sampling values from a well known bitmap
						Assert.AreEqual (153, *(scan + 0), "0");
						Assert.AreEqual (0, *(scan + 97), "97");
						Assert.AreEqual (255, *(scan + 194), "194");
						Assert.AreEqual (0, *(scan + 291), "291");
						Assert.AreEqual (0, *(scan + 388), "388");
						Assert.AreEqual (204, *(scan + 485), "485");
						Assert.AreEqual (204, *(scan + 582), "582");
						Assert.AreEqual (0, *(scan + 679), "679");
						Assert.AreEqual (204, *(scan + 776), "776");
						Assert.AreEqual (153, *(scan + 873), "873");
						Assert.AreEqual (0, *(scan + 970), "970");
						Assert.AreEqual (0, *(scan + 1067), "1067");
						Assert.AreEqual (153, *(scan + 1164), "1164");
						Assert.AreEqual (153, *(scan + 1261), "1261");
						Assert.AreEqual (102, *(scan + 1358), "1358");
						Assert.AreEqual (0, *(scan + 1455), "1455");
						Assert.AreEqual (0, *(scan + 1552), "1552");
						Assert.AreEqual (204, *(scan + 1649), "1649");
						Assert.AreEqual (153, *(scan + 1746), "1746");
						Assert.AreEqual (0, *(scan + 1843), "1843");
						Assert.AreEqual (0, *(scan + 1940), "1940");
						Assert.AreEqual (51, *(scan + 2037), "2037");
						Assert.AreEqual (0, *(scan + 2134), "2134");
						Assert.AreEqual (0, *(scan + 2231), "2231");
						Assert.AreEqual (102, *(scan + 2328), "2328");
						Assert.AreEqual (124, *(scan + 2425), "2425");
						Assert.AreEqual (204, *(scan + 2522), "2522");
						Assert.AreEqual (0, *(scan + 2619), "2619");
						Assert.AreEqual (0, *(scan + 2716), "2716");
						Assert.AreEqual (204, *(scan + 2813), "2813");
						Assert.AreEqual (51, *(scan + 2910), "2910");
						Assert.AreEqual (0, *(scan + 3007), "3007");
						Assert.AreEqual (255, *(scan + 3104), "3104");
						Assert.AreEqual (0, *(scan + 3201), "3201");
						Assert.AreEqual (0, *(scan + 3298), "3298");
						Assert.AreEqual (0, *(scan + 3395), "3395");
						Assert.AreEqual (128, *(scan + 3492), "3492");
						Assert.AreEqual (0, *(scan + 3589), "3589");
						Assert.AreEqual (255, *(scan + 3686), "3686");
						Assert.AreEqual (128, *(scan + 3783), "3783");
						Assert.AreEqual (0, *(scan + 3880), "3880");
						Assert.AreEqual (128, *(scan + 3977), "3977");
						Assert.AreEqual (0, *(scan + 4074), "4074");
						Assert.AreEqual (0, *(scan + 4171), "4171");
						Assert.AreEqual (204, *(scan + 4268), "4268");
						Assert.AreEqual (0, *(scan + 4365), "4365");
						Assert.AreEqual (0, *(scan + 4462), "4462");
						Assert.AreEqual (102, *(scan + 4559), "4559");
						Assert.AreEqual (0, *(scan + 4656), "4656");
						Assert.AreEqual (0, *(scan + 4753), "4753");
						Assert.AreEqual (102, *(scan + 4850), "4850");
						Assert.AreEqual (0, *(scan + 4947), "4947");
						Assert.AreEqual (0, *(scan + 5044), "5044");
						Assert.AreEqual (204, *(scan + 5141), "5141");
						Assert.AreEqual (128, *(scan + 5238), "5238");
						Assert.AreEqual (0, *(scan + 5335), "5335");
						Assert.AreEqual (128, *(scan + 5432), "5432");
						Assert.AreEqual (128, *(scan + 5529), "5529");
						Assert.AreEqual (0, *(scan + 5626), "5626");
						Assert.AreEqual (255, *(scan + 5723), "5723");
						Assert.AreEqual (153, *(scan + 5820), "5820");
						Assert.AreEqual (0, *(scan + 5917), "5917");
						Assert.AreEqual (0, *(scan + 6014), "6014");
						Assert.AreEqual (51, *(scan + 6111), "6111");
						Assert.AreEqual (0, *(scan + 6208), "6208");
						Assert.AreEqual (255, *(scan + 6305), "6305");
						Assert.AreEqual (153, *(scan + 6402), "6402");
						Assert.AreEqual (0, *(scan + 6499), "6499");
						Assert.AreEqual (153, *(scan + 6596), "6596");
						Assert.AreEqual (102, *(scan + 6693), "6693");
						Assert.AreEqual (0, *(scan + 6790), "6790");
						Assert.AreEqual (204, *(scan + 6887), "6887");
						Assert.AreEqual (153, *(scan + 6984), "6984");
						Assert.AreEqual (0, *(scan + 7081), "7081");
						Assert.AreEqual (204, *(scan + 7178), "7178");
						Assert.AreEqual (153, *(scan + 7275), "7275");
						Assert.AreEqual (0, *(scan + 7372), "7372");
						Assert.AreEqual (0, *(scan + 7469), "7469");
						Assert.AreEqual (153, *(scan + 7566), "7566");
						Assert.AreEqual (0, *(scan + 7663), "7663");
						Assert.AreEqual (0, *(scan + 7760), "7760");
						Assert.AreEqual (153, *(scan + 7857), "7857");
						Assert.AreEqual (102, *(scan + 7954), "7954");
						Assert.AreEqual (102, *(scan + 8051), "8051");
						Assert.AreEqual (0, *(scan + 8148), "8148");
						Assert.AreEqual (0, *(scan + 8245), "8245");
						Assert.AreEqual (0, *(scan + 8342), "8342");
						Assert.AreEqual (204, *(scan + 8439), "8439");
						Assert.AreEqual (0, *(scan + 8536), "8536");
						Assert.AreEqual (204, *(scan + 8633), "8633");
						Assert.AreEqual (128, *(scan + 8730), "8730");
						Assert.AreEqual (0, *(scan + 8827), "8827");
						Assert.AreEqual (0, *(scan + 8924), "8924");
						Assert.AreEqual (153, *(scan + 9021), "9021");
						Assert.AreEqual (153, *(scan + 9118), "9118");
						Assert.AreEqual (255, *(scan + 9215), "9215");
						Assert.AreEqual (0, *(scan + 9312), "9312");
						Assert.AreEqual (0, *(scan + 9409), "9409");
						Assert.AreEqual (204, *(scan + 9506), "9506");
						Assert.AreEqual (0, *(scan + 9603), "9603");
						Assert.AreEqual (0, *(scan + 9700), "9700");
						Assert.AreEqual (0, *(scan + 9797), "9797");
						Assert.AreEqual (128, *(scan + 9894), "9894");
						Assert.AreEqual (0, *(scan + 9991), "9991");
						Assert.AreEqual (0, *(scan + 10088), "10088");
						Assert.AreEqual (0, *(scan + 10185), "10185");
						Assert.AreEqual (102, *(scan + 10282), "10282");
						Assert.AreEqual (0, *(scan + 10379), "10379");
						Assert.AreEqual (0, *(scan + 10476), "10476");
						Assert.AreEqual (51, *(scan + 10573), "10573");
						Assert.AreEqual (204, *(scan + 10670), "10670");
						Assert.AreEqual (0, *(scan + 10767), "10767");
						Assert.AreEqual (0, *(scan + 10864), "10864");
						Assert.AreEqual (0, *(scan + 10961), "10961");
						Assert.AreEqual (153, *(scan + 11058), "11058");
						Assert.AreEqual (0, *(scan + 11155), "11155");
						Assert.AreEqual (0, *(scan + 11252), "11252");
						Assert.AreEqual (153, *(scan + 11349), "11349");
						Assert.AreEqual (51, *(scan + 11446), "11446");
						Assert.AreEqual (0, *(scan + 11543), "11543");
						Assert.AreEqual (0, *(scan + 11640), "11640");
						Assert.AreEqual (0, *(scan + 11737), "11737");
						Assert.AreEqual (204, *(scan + 11834), "11834");
						Assert.AreEqual (0, *(scan + 11931), "11931");
						Assert.AreEqual (0, *(scan + 12028), "12028");
						Assert.AreEqual (255, *(scan + 12125), "12125");
						Assert.AreEqual (153, *(scan + 12222), "12222");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		// 96x96x256.ico only has a 96x96 size available
		[Test]
		public void Bitmap96Features ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/96x96x256.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (256, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
				Assert.AreEqual (96, bmp.Width, "bmp.Width");
				Assert.AreEqual (96, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (96, rect.Width, "rect.Width");
				Assert.AreEqual (96, rect.Height, "rect.Height");

				Assert.AreEqual (96, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (96, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap96Pixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/96x96x256.ico");
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
				Assert.AreEqual (0, bmp.GetPixel (0, 16).ToArgb (), "0,16");
				Assert.AreEqual (0, bmp.GetPixel (0, 20).ToArgb (), "0,20");
				Assert.AreEqual (0, bmp.GetPixel (0, 24).ToArgb (), "0,24");
				Assert.AreEqual (0, bmp.GetPixel (0, 28).ToArgb (), "0,28");
				Assert.AreEqual (0, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (0, bmp.GetPixel (0, 36).ToArgb (), "0,36");
				Assert.AreEqual (0, bmp.GetPixel (0, 40).ToArgb (), "0,40");
				Assert.AreEqual (0, bmp.GetPixel (0, 44).ToArgb (), "0,44");
				Assert.AreEqual (0, bmp.GetPixel (0, 48).ToArgb (), "0,48");
				Assert.AreEqual (0, bmp.GetPixel (0, 52).ToArgb (), "0,52");
				Assert.AreEqual (0, bmp.GetPixel (0, 56).ToArgb (), "0,56");
				Assert.AreEqual (0, bmp.GetPixel (0, 60).ToArgb (), "0,60");
				Assert.AreEqual (0, bmp.GetPixel (0, 64).ToArgb (), "0,64");
				Assert.AreEqual (-14935012, bmp.GetPixel (0, 68).ToArgb (), "0,68");
				Assert.AreEqual (0, bmp.GetPixel (0, 72).ToArgb (), "0,72");
				Assert.AreEqual (0, bmp.GetPixel (0, 76).ToArgb (), "0,76");
				Assert.AreEqual (0, bmp.GetPixel (0, 80).ToArgb (), "0,80");
				Assert.AreEqual (0, bmp.GetPixel (0, 84).ToArgb (), "0,84");
				Assert.AreEqual (0, bmp.GetPixel (0, 88).ToArgb (), "0,88");
				Assert.AreEqual (0, bmp.GetPixel (0, 92).ToArgb (), "0,92");
				Assert.AreEqual (0, bmp.GetPixel (4, 0).ToArgb (), "4,0");
				Assert.AreEqual (0, bmp.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (4, 8).ToArgb (), "4,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (4, 12).ToArgb (), "4,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (-14935012, bmp.GetPixel (4, 20).ToArgb (), "4,20");
				Assert.AreEqual (0, bmp.GetPixel (4, 24).ToArgb (), "4,24");
				Assert.AreEqual (0, bmp.GetPixel (4, 28).ToArgb (), "4,28");
				Assert.AreEqual (0, bmp.GetPixel (4, 32).ToArgb (), "4,32");
				Assert.AreEqual (0, bmp.GetPixel (4, 36).ToArgb (), "4,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (4, 40).ToArgb (), "4,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (4, 44).ToArgb (), "4,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (4, 48).ToArgb (), "4,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (4, 52).ToArgb (), "4,52");
				Assert.AreEqual (0, bmp.GetPixel (4, 56).ToArgb (), "4,56");
				Assert.AreEqual (0, bmp.GetPixel (4, 60).ToArgb (), "4,60");
				Assert.AreEqual (-3342541, bmp.GetPixel (4, 64).ToArgb (), "4,64");
				Assert.AreEqual (-3342541, bmp.GetPixel (4, 68).ToArgb (), "4,68");
				Assert.AreEqual (-3342541, bmp.GetPixel (4, 72).ToArgb (), "4,72");
				Assert.AreEqual (0, bmp.GetPixel (4, 76).ToArgb (), "4,76");
				Assert.AreEqual (0, bmp.GetPixel (4, 80).ToArgb (), "4,80");
				Assert.AreEqual (-26317, bmp.GetPixel (4, 84).ToArgb (), "4,84");
				Assert.AreEqual (-26317, bmp.GetPixel (4, 88).ToArgb (), "4,88");
				Assert.AreEqual (-26317, bmp.GetPixel (4, 92).ToArgb (), "4,92");
				Assert.AreEqual (0, bmp.GetPixel (8, 0).ToArgb (), "8,0");
				Assert.AreEqual (-14935012, bmp.GetPixel (8, 4).ToArgb (), "8,4");
				Assert.AreEqual (-3407872, bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (8, 12).ToArgb (), "8,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (8, 16).ToArgb (), "8,16");
				Assert.AreEqual (-3407872, bmp.GetPixel (8, 20).ToArgb (), "8,20");
				Assert.AreEqual (-14935012, bmp.GetPixel (8, 24).ToArgb (), "8,24");
				Assert.AreEqual (0, bmp.GetPixel (8, 28).ToArgb (), "8,28");
				Assert.AreEqual (0, bmp.GetPixel (8, 32).ToArgb (), "8,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 36).ToArgb (), "8,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 40).ToArgb (), "8,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 44).ToArgb (), "8,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 48).ToArgb (), "8,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 52).ToArgb (), "8,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (8, 56).ToArgb (), "8,56");
				Assert.AreEqual (0, bmp.GetPixel (8, 60).ToArgb (), "8,60");
				Assert.AreEqual (-3342541, bmp.GetPixel (8, 64).ToArgb (), "8,64");
				Assert.AreEqual (-3342541, bmp.GetPixel (8, 68).ToArgb (), "8,68");
				Assert.AreEqual (-3342541, bmp.GetPixel (8, 72).ToArgb (), "8,72");
				Assert.AreEqual (0, bmp.GetPixel (8, 76).ToArgb (), "8,76");
				Assert.AreEqual (0, bmp.GetPixel (8, 80).ToArgb (), "8,80");
				Assert.AreEqual (-26317, bmp.GetPixel (8, 84).ToArgb (), "8,84");
				Assert.AreEqual (-26317, bmp.GetPixel (8, 88).ToArgb (), "8,88");
				Assert.AreEqual (-26317, bmp.GetPixel (8, 92).ToArgb (), "8,92");
				Assert.AreEqual (0, bmp.GetPixel (12, 0).ToArgb (), "12,0");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 4).ToArgb (), "12,4");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 8).ToArgb (), "12,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 12).ToArgb (), "12,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 16).ToArgb (), "12,16");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 20).ToArgb (), "12,20");
				Assert.AreEqual (-3407872, bmp.GetPixel (12, 24).ToArgb (), "12,24");
				Assert.AreEqual (0, bmp.GetPixel (12, 28).ToArgb (), "12,28");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 32).ToArgb (), "12,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 36).ToArgb (), "12,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 40).ToArgb (), "12,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 44).ToArgb (), "12,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 48).ToArgb (), "12,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 52).ToArgb (), "12,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 56).ToArgb (), "12,56");
				Assert.AreEqual (-3368602, bmp.GetPixel (12, 60).ToArgb (), "12,60");
				Assert.AreEqual (-3342541, bmp.GetPixel (12, 64).ToArgb (), "12,64");
				Assert.AreEqual (-3342541, bmp.GetPixel (12, 68).ToArgb (), "12,68");
				Assert.AreEqual (-14935012, bmp.GetPixel (12, 72).ToArgb (), "12,72");
				Assert.AreEqual (0, bmp.GetPixel (12, 76).ToArgb (), "12,76");
				Assert.AreEqual (0, bmp.GetPixel (12, 80).ToArgb (), "12,80");
				Assert.AreEqual (-26317, bmp.GetPixel (12, 84).ToArgb (), "12,84");
				Assert.AreEqual (-26317, bmp.GetPixel (12, 88).ToArgb (), "12,88");
				Assert.AreEqual (-26317, bmp.GetPixel (12, 92).ToArgb (), "12,92");
				Assert.AreEqual (0, bmp.GetPixel (16, 0).ToArgb (), "16,0");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 8).ToArgb (), "16,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 12).ToArgb (), "16,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 20).ToArgb (), "16,20");
				Assert.AreEqual (-3407872, bmp.GetPixel (16, 24).ToArgb (), "16,24");
				Assert.AreEqual (0, bmp.GetPixel (16, 28).ToArgb (), "16,28");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 32).ToArgb (), "16,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 36).ToArgb (), "16,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 40).ToArgb (), "16,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 44).ToArgb (), "16,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 48).ToArgb (), "16,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 52).ToArgb (), "16,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 56).ToArgb (), "16,56");
				Assert.AreEqual (-3368602, bmp.GetPixel (16, 60).ToArgb (), "16,60");
				Assert.AreEqual (0, bmp.GetPixel (16, 64).ToArgb (), "16,64");
				Assert.AreEqual (0, bmp.GetPixel (16, 68).ToArgb (), "16,68");
				Assert.AreEqual (0, bmp.GetPixel (16, 72).ToArgb (), "16,72");
				Assert.AreEqual (0, bmp.GetPixel (16, 76).ToArgb (), "16,76");
				Assert.AreEqual (-14935012, bmp.GetPixel (16, 80).ToArgb (), "16,80");
				Assert.AreEqual (0, bmp.GetPixel (16, 84).ToArgb (), "16,84");
				Assert.AreEqual (-14935012, bmp.GetPixel (16, 88).ToArgb (), "16,88");
				Assert.AreEqual (0, bmp.GetPixel (16, 92).ToArgb (), "16,92");
				Assert.AreEqual (0, bmp.GetPixel (20, 0).ToArgb (), "20,0");
				Assert.AreEqual (-14935012, bmp.GetPixel (20, 4).ToArgb (), "20,4");
				Assert.AreEqual (-3407872, bmp.GetPixel (20, 8).ToArgb (), "20,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (20, 12).ToArgb (), "20,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (20, 16).ToArgb (), "20,16");
				Assert.AreEqual (-3407872, bmp.GetPixel (20, 20).ToArgb (), "20,20");
				Assert.AreEqual (-14935012, bmp.GetPixel (20, 24).ToArgb (), "20,24");
				Assert.AreEqual (0, bmp.GetPixel (20, 28).ToArgb (), "20,28");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 32).ToArgb (), "20,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 36).ToArgb (), "20,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 40).ToArgb (), "20,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 44).ToArgb (), "20,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 48).ToArgb (), "20,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 52).ToArgb (), "20,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 56).ToArgb (), "20,56");
				Assert.AreEqual (-3368602, bmp.GetPixel (20, 60).ToArgb (), "20,60");
				Assert.AreEqual (0, bmp.GetPixel (20, 64).ToArgb (), "20,64");
				Assert.AreEqual (0, bmp.GetPixel (20, 68).ToArgb (), "20,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (20, 72).ToArgb (), "20,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (20, 76).ToArgb (), "20,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (20, 80).ToArgb (), "20,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (20, 84).ToArgb (), "20,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (20, 88).ToArgb (), "20,88");
				Assert.AreEqual (0, bmp.GetPixel (20, 92).ToArgb (), "20,92");
				Assert.AreEqual (0, bmp.GetPixel (24, 0).ToArgb (), "24,0");
				Assert.AreEqual (0, bmp.GetPixel (24, 4).ToArgb (), "24,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (24, 8).ToArgb (), "24,8");
				Assert.AreEqual (-3407872, bmp.GetPixel (24, 12).ToArgb (), "24,12");
				Assert.AreEqual (-3407872, bmp.GetPixel (24, 16).ToArgb (), "24,16");
				Assert.AreEqual (-14935012, bmp.GetPixel (24, 20).ToArgb (), "24,20");
				Assert.AreEqual (0, bmp.GetPixel (24, 24).ToArgb (), "24,24");
				Assert.AreEqual (0, bmp.GetPixel (24, 28).ToArgb (), "24,28");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 32).ToArgb (), "24,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 36).ToArgb (), "24,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 40).ToArgb (), "24,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 44).ToArgb (), "24,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 48).ToArgb (), "24,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 52).ToArgb (), "24,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 56).ToArgb (), "24,56");
				Assert.AreEqual (-3368602, bmp.GetPixel (24, 60).ToArgb (), "24,60");
				Assert.AreEqual (0, bmp.GetPixel (24, 64).ToArgb (), "24,64");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 68).ToArgb (), "24,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 72).ToArgb (), "24,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 76).ToArgb (), "24,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 80).ToArgb (), "24,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 84).ToArgb (), "24,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (24, 88).ToArgb (), "24,88");
				Assert.AreEqual (-14935012, bmp.GetPixel (24, 92).ToArgb (), "24,92");
				Assert.AreEqual (0, bmp.GetPixel (28, 0).ToArgb (), "28,0");
				Assert.AreEqual (0, bmp.GetPixel (28, 4).ToArgb (), "28,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (28, 8).ToArgb (), "28,8");
				Assert.AreEqual (0, bmp.GetPixel (28, 12).ToArgb (), "28,12");
				Assert.AreEqual (0, bmp.GetPixel (28, 16).ToArgb (), "28,16");
				Assert.AreEqual (-14935012, bmp.GetPixel (28, 20).ToArgb (), "28,20");
				Assert.AreEqual (-16777012, bmp.GetPixel (28, 24).ToArgb (), "28,24");
				Assert.AreEqual (0, bmp.GetPixel (28, 28).ToArgb (), "28,28");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 32).ToArgb (), "28,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 36).ToArgb (), "28,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 40).ToArgb (), "28,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 44).ToArgb (), "28,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 48).ToArgb (), "28,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 52).ToArgb (), "28,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 56).ToArgb (), "28,56");
				Assert.AreEqual (-3368602, bmp.GetPixel (28, 60).ToArgb (), "28,60");
				Assert.AreEqual (0, bmp.GetPixel (28, 64).ToArgb (), "28,64");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 68).ToArgb (), "28,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 72).ToArgb (), "28,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 76).ToArgb (), "28,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 80).ToArgb (), "28,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 84).ToArgb (), "28,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 88).ToArgb (), "28,88");
				Assert.AreEqual (-13434829, bmp.GetPixel (28, 92).ToArgb (), "28,92");
				Assert.AreEqual (0, bmp.GetPixel (32, 0).ToArgb (), "32,0");
				Assert.AreEqual (-10027264, bmp.GetPixel (32, 4).ToArgb (), "32,4");
				Assert.AreEqual (-10027264, bmp.GetPixel (32, 8).ToArgb (), "32,8");
				Assert.AreEqual (-10027264, bmp.GetPixel (32, 12).ToArgb (), "32,12");
				Assert.AreEqual (-14935012, bmp.GetPixel (32, 16).ToArgb (), "32,16");
				Assert.AreEqual (-16777012, bmp.GetPixel (32, 20).ToArgb (), "32,20");
				Assert.AreEqual (-16777012, bmp.GetPixel (32, 24).ToArgb (), "32,24");
				Assert.AreEqual (-16777012, bmp.GetPixel (32, 28).ToArgb (), "32,28");
				Assert.AreEqual (0, bmp.GetPixel (32, 32).ToArgb (), "32,32");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 36).ToArgb (), "32,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 40).ToArgb (), "32,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 44).ToArgb (), "32,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 48).ToArgb (), "32,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 52).ToArgb (), "32,52");
				Assert.AreEqual (-3368602, bmp.GetPixel (32, 56).ToArgb (), "32,56");
				Assert.AreEqual (0, bmp.GetPixel (32, 60).ToArgb (), "32,60");
				Assert.AreEqual (0, bmp.GetPixel (32, 64).ToArgb (), "32,64");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 68).ToArgb (), "32,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 72).ToArgb (), "32,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 76).ToArgb (), "32,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 80).ToArgb (), "32,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 84).ToArgb (), "32,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 88).ToArgb (), "32,88");
				Assert.AreEqual (-13434829, bmp.GetPixel (32, 92).ToArgb (), "32,92");
				Assert.AreEqual (0, bmp.GetPixel (36, 0).ToArgb (), "36,0");
				Assert.AreEqual (-10027264, bmp.GetPixel (36, 4).ToArgb (), "36,4");
				Assert.AreEqual (-10027264, bmp.GetPixel (36, 8).ToArgb (), "36,8");
				Assert.AreEqual (-10027264, bmp.GetPixel (36, 12).ToArgb (), "36,12");
				Assert.AreEqual (-10027264, bmp.GetPixel (36, 16).ToArgb (), "36,16");
				Assert.AreEqual (-14935012, bmp.GetPixel (36, 20).ToArgb (), "36,20");
				Assert.AreEqual (-16777012, bmp.GetPixel (36, 24).ToArgb (), "36,24");
				Assert.AreEqual (0, bmp.GetPixel (36, 28).ToArgb (), "36,28");
				Assert.AreEqual (0, bmp.GetPixel (36, 32).ToArgb (), "36,32");
				Assert.AreEqual (0, bmp.GetPixel (36, 36).ToArgb (), "36,36");
				Assert.AreEqual (-3368602, bmp.GetPixel (36, 40).ToArgb (), "36,40");
				Assert.AreEqual (-3368602, bmp.GetPixel (36, 44).ToArgb (), "36,44");
				Assert.AreEqual (-3368602, bmp.GetPixel (36, 48).ToArgb (), "36,48");
				Assert.AreEqual (-3368602, bmp.GetPixel (36, 52).ToArgb (), "36,52");
				Assert.AreEqual (0, bmp.GetPixel (36, 56).ToArgb (), "36,56");
				Assert.AreEqual (0, bmp.GetPixel (36, 60).ToArgb (), "36,60");
				Assert.AreEqual (0, bmp.GetPixel (36, 64).ToArgb (), "36,64");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 68).ToArgb (), "36,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 72).ToArgb (), "36,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 76).ToArgb (), "36,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 80).ToArgb (), "36,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 84).ToArgb (), "36,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 88).ToArgb (), "36,88");
				Assert.AreEqual (-13434829, bmp.GetPixel (36, 92).ToArgb (), "36,92");
				Assert.AreEqual (0, bmp.GetPixel (40, 0).ToArgb (), "40,0");
				Assert.AreEqual (-10027264, bmp.GetPixel (40, 4).ToArgb (), "40,4");
				Assert.AreEqual (-10027264, bmp.GetPixel (40, 8).ToArgb (), "40,8");
				Assert.AreEqual (-10027264, bmp.GetPixel (40, 12).ToArgb (), "40,12");
				Assert.AreEqual (-14935012, bmp.GetPixel (40, 16).ToArgb (), "40,16");
				Assert.AreEqual (0, bmp.GetPixel (40, 20).ToArgb (), "40,20");
				Assert.AreEqual (0, bmp.GetPixel (40, 24).ToArgb (), "40,24");
				Assert.AreEqual (0, bmp.GetPixel (40, 28).ToArgb (), "40,28");
				Assert.AreEqual (-13408717, bmp.GetPixel (40, 32).ToArgb (), "40,32");
				Assert.AreEqual (-13408717, bmp.GetPixel (40, 36).ToArgb (), "40,36");
				Assert.AreEqual (0, bmp.GetPixel (40, 40).ToArgb (), "40,40");
				Assert.AreEqual (0, bmp.GetPixel (40, 44).ToArgb (), "40,44");
				Assert.AreEqual (-14935012, bmp.GetPixel (40, 48).ToArgb (), "40,48");
				Assert.AreEqual (0, bmp.GetPixel (40, 52).ToArgb (), "40,52");
				Assert.AreEqual (0, bmp.GetPixel (40, 56).ToArgb (), "40,56");
				Assert.AreEqual (-26317, bmp.GetPixel (40, 60).ToArgb (), "40,60");
				Assert.AreEqual (-26317, bmp.GetPixel (40, 64).ToArgb (), "40,64");
				Assert.AreEqual (-14935012, bmp.GetPixel (40, 68).ToArgb (), "40,68");
				Assert.AreEqual (-13434829, bmp.GetPixel (40, 72).ToArgb (), "40,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (40, 76).ToArgb (), "40,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (40, 80).ToArgb (), "40,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (40, 84).ToArgb (), "40,84");
				Assert.AreEqual (-13434829, bmp.GetPixel (40, 88).ToArgb (), "40,88");
				Assert.AreEqual (0, bmp.GetPixel (40, 92).ToArgb (), "40,92");
				Assert.AreEqual (0, bmp.GetPixel (44, 0).ToArgb (), "44,0");
				Assert.AreEqual (0, bmp.GetPixel (44, 4).ToArgb (), "44,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (44, 8).ToArgb (), "44,8");
				Assert.AreEqual (0, bmp.GetPixel (44, 12).ToArgb (), "44,12");
				Assert.AreEqual (0, bmp.GetPixel (44, 16).ToArgb (), "44,16");
				Assert.AreEqual (0, bmp.GetPixel (44, 20).ToArgb (), "44,20");
				Assert.AreEqual (0, bmp.GetPixel (44, 24).ToArgb (), "44,24");
				Assert.AreEqual (0, bmp.GetPixel (44, 28).ToArgb (), "44,28");
				Assert.AreEqual (-13408717, bmp.GetPixel (44, 32).ToArgb (), "44,32");
				Assert.AreEqual (-13408717, bmp.GetPixel (44, 36).ToArgb (), "44,36");
				Assert.AreEqual (0, bmp.GetPixel (44, 40).ToArgb (), "44,40");
				Assert.AreEqual (-13312, bmp.GetPixel (44, 44).ToArgb (), "44,44");
				Assert.AreEqual (-13312, bmp.GetPixel (44, 48).ToArgb (), "44,48");
				Assert.AreEqual (-13312, bmp.GetPixel (44, 52).ToArgb (), "44,52");
				Assert.AreEqual (-13312, bmp.GetPixel (44, 56).ToArgb (), "44,56");
				Assert.AreEqual (-14935012, bmp.GetPixel (44, 60).ToArgb (), "44,60");
				Assert.AreEqual (-14935012, bmp.GetPixel (44, 64).ToArgb (), "44,64");
				Assert.AreEqual (0, bmp.GetPixel (44, 68).ToArgb (), "44,68");
				Assert.AreEqual (-14935012, bmp.GetPixel (44, 72).ToArgb (), "44,72");
				Assert.AreEqual (-13434829, bmp.GetPixel (44, 76).ToArgb (), "44,76");
				Assert.AreEqual (-13434829, bmp.GetPixel (44, 80).ToArgb (), "44,80");
				Assert.AreEqual (-13434829, bmp.GetPixel (44, 84).ToArgb (), "44,84");
				Assert.AreEqual (-14935012, bmp.GetPixel (44, 88).ToArgb (), "44,88");
				Assert.AreEqual (0, bmp.GetPixel (44, 92).ToArgb (), "44,92");
				Assert.AreEqual (0, bmp.GetPixel (48, 0).ToArgb (), "48,0");
				Assert.AreEqual (0, bmp.GetPixel (48, 4).ToArgb (), "48,4");
				Assert.AreEqual (0, bmp.GetPixel (48, 8).ToArgb (), "48,8");
				Assert.AreEqual (0, bmp.GetPixel (48, 12).ToArgb (), "48,12");
				Assert.AreEqual (-52429, bmp.GetPixel (48, 16).ToArgb (), "48,16");
				Assert.AreEqual (-52429, bmp.GetPixel (48, 20).ToArgb (), "48,20");
				Assert.AreEqual (-52429, bmp.GetPixel (48, 24).ToArgb (), "48,24");
				Assert.AreEqual (-52429, bmp.GetPixel (48, 28).ToArgb (), "48,28");
				Assert.AreEqual (-14935012, bmp.GetPixel (48, 32).ToArgb (), "48,32");
				Assert.AreEqual (0, bmp.GetPixel (48, 36).ToArgb (), "48,36");
				Assert.AreEqual (-14935012, bmp.GetPixel (48, 40).ToArgb (), "48,40");
				Assert.AreEqual (-13312, bmp.GetPixel (48, 44).ToArgb (), "48,44");
				Assert.AreEqual (-13312, bmp.GetPixel (48, 48).ToArgb (), "48,48");
				Assert.AreEqual (-13312, bmp.GetPixel (48, 52).ToArgb (), "48,52");
				Assert.AreEqual (-13312, bmp.GetPixel (48, 56).ToArgb (), "48,56");
				Assert.AreEqual (0, bmp.GetPixel (48, 60).ToArgb (), "48,60");
				Assert.AreEqual (1842204, bmp.GetPixel (48, 64).ToArgb (), "48,64");
				Assert.AreEqual (-3355546, bmp.GetPixel (48, 68).ToArgb (), "48,68");
				Assert.AreEqual (-3355546, bmp.GetPixel (48, 72).ToArgb (), "48,72");
				Assert.AreEqual (0, bmp.GetPixel (48, 76).ToArgb (), "48,76");
				Assert.AreEqual (0, bmp.GetPixel (48, 80).ToArgb (), "48,80");
				Assert.AreEqual (0, bmp.GetPixel (48, 84).ToArgb (), "48,84");
				Assert.AreEqual (0, bmp.GetPixel (48, 88).ToArgb (), "48,88");
				Assert.AreEqual (0, bmp.GetPixel (48, 92).ToArgb (), "48,92");
				Assert.AreEqual (0, bmp.GetPixel (52, 0).ToArgb (), "52,0");
				Assert.AreEqual (0, bmp.GetPixel (52, 4).ToArgb (), "52,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (52, 8).ToArgb (), "52,8");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 12).ToArgb (), "52,12");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 16).ToArgb (), "52,16");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 20).ToArgb (), "52,20");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 24).ToArgb (), "52,24");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 28).ToArgb (), "52,28");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 32).ToArgb (), "52,32");
				Assert.AreEqual (-52429, bmp.GetPixel (52, 36).ToArgb (), "52,36");
				Assert.AreEqual (-14935012, bmp.GetPixel (52, 40).ToArgb (), "52,40");
				Assert.AreEqual (-13312, bmp.GetPixel (52, 44).ToArgb (), "52,44");
				Assert.AreEqual (-13312, bmp.GetPixel (52, 48).ToArgb (), "52,48");
				Assert.AreEqual (-13312, bmp.GetPixel (52, 52).ToArgb (), "52,52");
				Assert.AreEqual (-13312, bmp.GetPixel (52, 56).ToArgb (), "52,56");
				Assert.AreEqual (0, bmp.GetPixel (52, 60).ToArgb (), "52,60");
				Assert.AreEqual (-3355546, bmp.GetPixel (52, 64).ToArgb (), "52,64");
				Assert.AreEqual (-3355546, bmp.GetPixel (52, 68).ToArgb (), "52,68");
				Assert.AreEqual (-3355546, bmp.GetPixel (52, 72).ToArgb (), "52,72");
				Assert.AreEqual (-3355546, bmp.GetPixel (52, 76).ToArgb (), "52,76");
				Assert.AreEqual (0, bmp.GetPixel (52, 80).ToArgb (), "52,80");
				Assert.AreEqual (-6737101, bmp.GetPixel (52, 84).ToArgb (), "52,84");
				Assert.AreEqual (-6737101, bmp.GetPixel (52, 88).ToArgb (), "52,88");
				Assert.AreEqual (-6737101, bmp.GetPixel (52, 92).ToArgb (), "52,92");
				Assert.AreEqual (0, bmp.GetPixel (56, 0).ToArgb (), "56,0");
				Assert.AreEqual (0, bmp.GetPixel (56, 4).ToArgb (), "56,4");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 8).ToArgb (), "56,8");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 12).ToArgb (), "56,12");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 16).ToArgb (), "56,16");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 20).ToArgb (), "56,20");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 24).ToArgb (), "56,24");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 28).ToArgb (), "56,28");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 32).ToArgb (), "56,32");
				Assert.AreEqual (-52429, bmp.GetPixel (56, 36).ToArgb (), "56,36");
				Assert.AreEqual (-14935012, bmp.GetPixel (56, 40).ToArgb (), "56,40");
				Assert.AreEqual (-13312, bmp.GetPixel (56, 44).ToArgb (), "56,44");
				Assert.AreEqual (-13312, bmp.GetPixel (56, 48).ToArgb (), "56,48");
				Assert.AreEqual (-13312, bmp.GetPixel (56, 52).ToArgb (), "56,52");
				Assert.AreEqual (-13312, bmp.GetPixel (56, 56).ToArgb (), "56,56");
				Assert.AreEqual (0, bmp.GetPixel (56, 60).ToArgb (), "56,60");
				Assert.AreEqual (-3355546, bmp.GetPixel (56, 64).ToArgb (), "56,64");
				Assert.AreEqual (-3355546, bmp.GetPixel (56, 68).ToArgb (), "56,68");
				Assert.AreEqual (-3355546, bmp.GetPixel (56, 72).ToArgb (), "56,72");
				Assert.AreEqual (-3355546, bmp.GetPixel (56, 76).ToArgb (), "56,76");
				Assert.AreEqual (-6737101, bmp.GetPixel (56, 80).ToArgb (), "56,80");
				Assert.AreEqual (-6737101, bmp.GetPixel (56, 84).ToArgb (), "56,84");
				Assert.AreEqual (-6737101, bmp.GetPixel (56, 88).ToArgb (), "56,88");
				Assert.AreEqual (-6737101, bmp.GetPixel (56, 92).ToArgb (), "56,92");
				Assert.AreEqual (0, bmp.GetPixel (60, 0).ToArgb (), "60,0");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 4).ToArgb (), "60,4");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 8).ToArgb (), "60,8");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 12).ToArgb (), "60,12");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 16).ToArgb (), "60,16");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 20).ToArgb (), "60,20");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 24).ToArgb (), "60,24");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 28).ToArgb (), "60,28");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 32).ToArgb (), "60,32");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 36).ToArgb (), "60,36");
				Assert.AreEqual (-52429, bmp.GetPixel (60, 40).ToArgb (), "60,40");
				Assert.AreEqual (0, bmp.GetPixel (60, 44).ToArgb (), "60,44");
				Assert.AreEqual (-14935012, bmp.GetPixel (60, 48).ToArgb (), "60,48");
				Assert.AreEqual (0, bmp.GetPixel (60, 52).ToArgb (), "60,52");
				Assert.AreEqual (0, bmp.GetPixel (60, 56).ToArgb (), "60,56");
				Assert.AreEqual (0, bmp.GetPixel (60, 60).ToArgb (), "60,60");
				Assert.AreEqual (0, bmp.GetPixel (60, 64).ToArgb (), "60,64");
				Assert.AreEqual (-3355546, bmp.GetPixel (60, 68).ToArgb (), "60,68");
				Assert.AreEqual (-3355546, bmp.GetPixel (60, 72).ToArgb (), "60,72");
				Assert.AreEqual (0, bmp.GetPixel (60, 76).ToArgb (), "60,76");
				Assert.AreEqual (-6737101, bmp.GetPixel (60, 80).ToArgb (), "60,80");
				Assert.AreEqual (-6737101, bmp.GetPixel (60, 84).ToArgb (), "60,84");
				Assert.AreEqual (-6737101, bmp.GetPixel (60, 88).ToArgb (), "60,88");
				Assert.AreEqual (-6737101, bmp.GetPixel (60, 92).ToArgb (), "60,92");
				Assert.AreEqual (0, bmp.GetPixel (64, 0).ToArgb (), "64,0");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 4).ToArgb (), "64,4");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 8).ToArgb (), "64,8");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 12).ToArgb (), "64,12");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 16).ToArgb (), "64,16");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 20).ToArgb (), "64,20");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 24).ToArgb (), "64,24");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 28).ToArgb (), "64,28");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 32).ToArgb (), "64,32");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 36).ToArgb (), "64,36");
				Assert.AreEqual (-52429, bmp.GetPixel (64, 40).ToArgb (), "64,40");
				Assert.AreEqual (-14935012, bmp.GetPixel (64, 44).ToArgb (), "64,44");
				Assert.AreEqual (0, bmp.GetPixel (64, 48).ToArgb (), "64,48");
				Assert.AreEqual (0, bmp.GetPixel (64, 52).ToArgb (), "64,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (64, 56).ToArgb (), "64,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (64, 60).ToArgb (), "64,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (64, 64).ToArgb (), "64,64");
				Assert.AreEqual (-14935012, bmp.GetPixel (64, 68).ToArgb (), "64,68");
				Assert.AreEqual (0, bmp.GetPixel (64, 72).ToArgb (), "64,72");
				Assert.AreEqual (0, bmp.GetPixel (64, 76).ToArgb (), "64,76");
				Assert.AreEqual (0, bmp.GetPixel (64, 80).ToArgb (), "64,80");
				Assert.AreEqual (-6737101, bmp.GetPixel (64, 84).ToArgb (), "64,84");
				Assert.AreEqual (-6737101, bmp.GetPixel (64, 88).ToArgb (), "64,88");
				Assert.AreEqual (-14935012, bmp.GetPixel (64, 92).ToArgb (), "64,92");
				Assert.AreEqual (-14935012, bmp.GetPixel (68, 0).ToArgb (), "68,0");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 4).ToArgb (), "68,4");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 8).ToArgb (), "68,8");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 12).ToArgb (), "68,12");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 16).ToArgb (), "68,16");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 20).ToArgb (), "68,20");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 24).ToArgb (), "68,24");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 28).ToArgb (), "68,28");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 32).ToArgb (), "68,32");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 36).ToArgb (), "68,36");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 40).ToArgb (), "68,40");
				Assert.AreEqual (-52429, bmp.GetPixel (68, 44).ToArgb (), "68,44");
				Assert.AreEqual (0, bmp.GetPixel (68, 48).ToArgb (), "68,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (68, 52).ToArgb (), "68,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (68, 56).ToArgb (), "68,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (68, 60).ToArgb (), "68,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (68, 64).ToArgb (), "68,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (68, 68).ToArgb (), "68,68");
				Assert.AreEqual (-14935012, bmp.GetPixel (68, 72).ToArgb (), "68,72");
				Assert.AreEqual (-16751002, bmp.GetPixel (68, 76).ToArgb (), "68,76");
				Assert.AreEqual (-16751002, bmp.GetPixel (68, 80).ToArgb (), "68,80");
				Assert.AreEqual (0, bmp.GetPixel (68, 84).ToArgb (), "68,84");
				Assert.AreEqual (0, bmp.GetPixel (68, 88).ToArgb (), "68,88");
				Assert.AreEqual (-39373, bmp.GetPixel (68, 92).ToArgb (), "68,92");
				Assert.AreEqual (-14935012, bmp.GetPixel (72, 0).ToArgb (), "72,0");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 4).ToArgb (), "72,4");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 8).ToArgb (), "72,8");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 12).ToArgb (), "72,12");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 16).ToArgb (), "72,16");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 20).ToArgb (), "72,20");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 24).ToArgb (), "72,24");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 28).ToArgb (), "72,28");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 32).ToArgb (), "72,32");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 36).ToArgb (), "72,36");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 40).ToArgb (), "72,40");
				Assert.AreEqual (-52429, bmp.GetPixel (72, 44).ToArgb (), "72,44");
				Assert.AreEqual (-14935012, bmp.GetPixel (72, 48).ToArgb (), "72,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 52).ToArgb (), "72,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 56).ToArgb (), "72,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 60).ToArgb (), "72,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 64).ToArgb (), "72,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 68).ToArgb (), "72,68");
				Assert.AreEqual (-6750157, bmp.GetPixel (72, 72).ToArgb (), "72,72");
				Assert.AreEqual (0, bmp.GetPixel (72, 76).ToArgb (), "72,76");
				Assert.AreEqual (0, bmp.GetPixel (72, 80).ToArgb (), "72,80");
				Assert.AreEqual (0, bmp.GetPixel (72, 84).ToArgb (), "72,84");
				Assert.AreEqual (0, bmp.GetPixel (72, 88).ToArgb (), "72,88");
				Assert.AreEqual (-39373, bmp.GetPixel (72, 92).ToArgb (), "72,92");
				Assert.AreEqual (0, bmp.GetPixel (76, 0).ToArgb (), "76,0");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 4).ToArgb (), "76,4");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 8).ToArgb (), "76,8");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 12).ToArgb (), "76,12");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 16).ToArgb (), "76,16");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 20).ToArgb (), "76,20");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 24).ToArgb (), "76,24");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 28).ToArgb (), "76,28");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 32).ToArgb (), "76,32");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 36).ToArgb (), "76,36");
				Assert.AreEqual (-52429, bmp.GetPixel (76, 40).ToArgb (), "76,40");
				Assert.AreEqual (-14935012, bmp.GetPixel (76, 44).ToArgb (), "76,44");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 48).ToArgb (), "76,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 52).ToArgb (), "76,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 56).ToArgb (), "76,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 60).ToArgb (), "76,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 64).ToArgb (), "76,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 68).ToArgb (), "76,68");
				Assert.AreEqual (-6750157, bmp.GetPixel (76, 72).ToArgb (), "76,72");
				Assert.AreEqual (-14935012, bmp.GetPixel (76, 76).ToArgb (), "76,76");
				Assert.AreEqual (0, bmp.GetPixel (76, 80).ToArgb (), "76,80");
				Assert.AreEqual (-65383, bmp.GetPixel (76, 84).ToArgb (), "76,84");
				Assert.AreEqual (-65383, bmp.GetPixel (76, 88).ToArgb (), "76,88");
				Assert.AreEqual (-14935012, bmp.GetPixel (76, 92).ToArgb (), "76,92");
				Assert.AreEqual (0, bmp.GetPixel (80, 0).ToArgb (), "80,0");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 4).ToArgb (), "80,4");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 8).ToArgb (), "80,8");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 12).ToArgb (), "80,12");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 16).ToArgb (), "80,16");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 20).ToArgb (), "80,20");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 24).ToArgb (), "80,24");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 28).ToArgb (), "80,28");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 32).ToArgb (), "80,32");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 36).ToArgb (), "80,36");
				Assert.AreEqual (-52429, bmp.GetPixel (80, 40).ToArgb (), "80,40");
				Assert.AreEqual (0, bmp.GetPixel (80, 44).ToArgb (), "80,44");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 48).ToArgb (), "80,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 52).ToArgb (), "80,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 56).ToArgb (), "80,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 60).ToArgb (), "80,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 64).ToArgb (), "80,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 68).ToArgb (), "80,68");
				Assert.AreEqual (-6750157, bmp.GetPixel (80, 72).ToArgb (), "80,72");
				Assert.AreEqual (-14935012, bmp.GetPixel (80, 76).ToArgb (), "80,76");
				Assert.AreEqual (-65383, bmp.GetPixel (80, 80).ToArgb (), "80,80");
				Assert.AreEqual (-65383, bmp.GetPixel (80, 84).ToArgb (), "80,84");
				Assert.AreEqual (-65383, bmp.GetPixel (80, 88).ToArgb (), "80,88");
				Assert.AreEqual (-65383, bmp.GetPixel (80, 92).ToArgb (), "80,92");
				Assert.AreEqual (0, bmp.GetPixel (84, 0).ToArgb (), "84,0");
				Assert.AreEqual (0, bmp.GetPixel (84, 4).ToArgb (), "84,4");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 8).ToArgb (), "84,8");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 12).ToArgb (), "84,12");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 16).ToArgb (), "84,16");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 20).ToArgb (), "84,20");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 24).ToArgb (), "84,24");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 28).ToArgb (), "84,28");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 32).ToArgb (), "84,32");
				Assert.AreEqual (-52429, bmp.GetPixel (84, 36).ToArgb (), "84,36");
				Assert.AreEqual (-14935012, bmp.GetPixel (84, 40).ToArgb (), "84,40");
				Assert.AreEqual (0, bmp.GetPixel (84, 44).ToArgb (), "84,44");
				Assert.AreEqual (-14935012, bmp.GetPixel (84, 48).ToArgb (), "84,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 52).ToArgb (), "84,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 56).ToArgb (), "84,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 60).ToArgb (), "84,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 64).ToArgb (), "84,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 68).ToArgb (), "84,68");
				Assert.AreEqual (-6750157, bmp.GetPixel (84, 72).ToArgb (), "84,72");
				Assert.AreEqual (0, bmp.GetPixel (84, 76).ToArgb (), "84,76");
				Assert.AreEqual (-65383, bmp.GetPixel (84, 80).ToArgb (), "84,80");
				Assert.AreEqual (-65383, bmp.GetPixel (84, 84).ToArgb (), "84,84");
				Assert.AreEqual (-65383, bmp.GetPixel (84, 88).ToArgb (), "84,88");
				Assert.AreEqual (-65383, bmp.GetPixel (84, 92).ToArgb (), "84,92");
				Assert.AreEqual (0, bmp.GetPixel (88, 0).ToArgb (), "88,0");
				Assert.AreEqual (-3342490, bmp.GetPixel (88, 4).ToArgb (), "88,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (88, 8).ToArgb (), "88,8");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 12).ToArgb (), "88,12");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 16).ToArgb (), "88,16");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 20).ToArgb (), "88,20");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 24).ToArgb (), "88,24");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 28).ToArgb (), "88,28");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 32).ToArgb (), "88,32");
				Assert.AreEqual (-52429, bmp.GetPixel (88, 36).ToArgb (), "88,36");
				Assert.AreEqual (0, bmp.GetPixel (88, 40).ToArgb (), "88,40");
				Assert.AreEqual (-16777063, bmp.GetPixel (88, 44).ToArgb (), "88,44");
				Assert.AreEqual (0, bmp.GetPixel (88, 48).ToArgb (), "88,48");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 52).ToArgb (), "88,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 56).ToArgb (), "88,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 60).ToArgb (), "88,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 64).ToArgb (), "88,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 68).ToArgb (), "88,68");
				Assert.AreEqual (-6750157, bmp.GetPixel (88, 72).ToArgb (), "88,72");
				Assert.AreEqual (0, bmp.GetPixel (88, 76).ToArgb (), "88,76");
				Assert.AreEqual (-65383, bmp.GetPixel (88, 80).ToArgb (), "88,80");
				Assert.AreEqual (-65383, bmp.GetPixel (88, 84).ToArgb (), "88,84");
				Assert.AreEqual (-65383, bmp.GetPixel (88, 88).ToArgb (), "88,88");
				Assert.AreEqual (-65383, bmp.GetPixel (88, 92).ToArgb (), "88,92");
				Assert.AreEqual (-14935012, bmp.GetPixel (92, 0).ToArgb (), "92,0");
				Assert.AreEqual (-3342490, bmp.GetPixel (92, 4).ToArgb (), "92,4");
				Assert.AreEqual (-14935012, bmp.GetPixel (92, 8).ToArgb (), "92,8");
				Assert.AreEqual (0, bmp.GetPixel (92, 12).ToArgb (), "92,12");
				Assert.AreEqual (-52429, bmp.GetPixel (92, 16).ToArgb (), "92,16");
				Assert.AreEqual (-52429, bmp.GetPixel (92, 20).ToArgb (), "92,20");
				Assert.AreEqual (-52429, bmp.GetPixel (92, 24).ToArgb (), "92,24");
				Assert.AreEqual (-52429, bmp.GetPixel (92, 28).ToArgb (), "92,28");
				Assert.AreEqual (-14935012, bmp.GetPixel (92, 32).ToArgb (), "92,32");
				Assert.AreEqual (0, bmp.GetPixel (92, 36).ToArgb (), "92,36");
				Assert.AreEqual (0, bmp.GetPixel (92, 40).ToArgb (), "92,40");
				Assert.AreEqual (0, bmp.GetPixel (92, 44).ToArgb (), "92,44");
				Assert.AreEqual (0, bmp.GetPixel (92, 48).ToArgb (), "92,48");
				Assert.AreEqual (0, bmp.GetPixel (92, 52).ToArgb (), "92,52");
				Assert.AreEqual (-6750157, bmp.GetPixel (92, 56).ToArgb (), "92,56");
				Assert.AreEqual (-6750157, bmp.GetPixel (92, 60).ToArgb (), "92,60");
				Assert.AreEqual (-6750157, bmp.GetPixel (92, 64).ToArgb (), "92,64");
				Assert.AreEqual (-6750157, bmp.GetPixel (92, 68).ToArgb (), "92,68");
				Assert.AreEqual (0, bmp.GetPixel (92, 72).ToArgb (), "92,72");
				Assert.AreEqual (0, bmp.GetPixel (92, 76).ToArgb (), "92,76");
				Assert.AreEqual (-65383, bmp.GetPixel (92, 80).ToArgb (), "92,80");
				Assert.AreEqual (-65383, bmp.GetPixel (92, 84).ToArgb (), "92,84");
				Assert.AreEqual (-65383, bmp.GetPixel (92, 88).ToArgb (), "92,88");
				Assert.AreEqual (-65383, bmp.GetPixel (92, 92).ToArgb (), "92,92");
#endif
			}
		}

		[Test]
		public void Bitmap96Data ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/96x96x256.ico");
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
						// 97 is prime (so we're not affected by a recurring pattern)
						for (int p = 0; p < size; p += 97) {
							Console.WriteLine ("\t\t\t\t\t\tAssert.AreEqual ({0}, *(scan + {1}), \"{1}\");", *(scan + p), p);
						}
#else
						// sampling values from a well known bitmap
						Assert.AreEqual (0, *(scan + 0), "0");
						Assert.AreEqual (0, *(scan + 97), "97");
						Assert.AreEqual (0, *(scan + 194), "194");
						Assert.AreEqual (0, *(scan + 291), "291");
						Assert.AreEqual (0, *(scan + 388), "388");
						Assert.AreEqual (28, *(scan + 485), "485");
						Assert.AreEqual (0, *(scan + 582), "582");
						Assert.AreEqual (28, *(scan + 679), "679");
						Assert.AreEqual (255, *(scan + 776), "776");
						Assert.AreEqual (0, *(scan + 873), "873");
						Assert.AreEqual (255, *(scan + 970), "970");
						Assert.AreEqual (255, *(scan + 1067), "1067");
						Assert.AreEqual (0, *(scan + 1164), "1164");
						Assert.AreEqual (255, *(scan + 1261), "1261");
						Assert.AreEqual (255, *(scan + 1358), "1358");
						Assert.AreEqual (0, *(scan + 1455), "1455");
						Assert.AreEqual (255, *(scan + 1552), "1552");
						Assert.AreEqual (255, *(scan + 1649), "1649");
						Assert.AreEqual (0, *(scan + 1746), "1746");
						Assert.AreEqual (255, *(scan + 1843), "1843");
						Assert.AreEqual (255, *(scan + 1940), "1940");
						Assert.AreEqual (0, *(scan + 2037), "2037");
						Assert.AreEqual (255, *(scan + 2134), "2134");
						Assert.AreEqual (255, *(scan + 2231), "2231");
						Assert.AreEqual (0, *(scan + 2328), "2328");
						Assert.AreEqual (255, *(scan + 2425), "2425");
						Assert.AreEqual (255, *(scan + 2522), "2522");
						Assert.AreEqual (0, *(scan + 2619), "2619");
						Assert.AreEqual (255, *(scan + 2716), "2716");
						Assert.AreEqual (255, *(scan + 2813), "2813");
						Assert.AreEqual (0, *(scan + 2910), "2910");
						Assert.AreEqual (255, *(scan + 3007), "3007");
						Assert.AreEqual (255, *(scan + 3104), "3104");
						Assert.AreEqual (0, *(scan + 3201), "3201");
						Assert.AreEqual (255, *(scan + 3298), "3298");
						Assert.AreEqual (255, *(scan + 3395), "3395");
						Assert.AreEqual (0, *(scan + 3492), "3492");
						Assert.AreEqual (0, *(scan + 3589), "3589");
						Assert.AreEqual (255, *(scan + 3686), "3686");
						Assert.AreEqual (0, *(scan + 3783), "3783");
						Assert.AreEqual (0, *(scan + 3880), "3880");
						Assert.AreEqual (255, *(scan + 3977), "3977");
						Assert.AreEqual (0, *(scan + 4074), "4074");
						Assert.AreEqual (0, *(scan + 4171), "4171");
						Assert.AreEqual (255, *(scan + 4268), "4268");
						Assert.AreEqual (0, *(scan + 4365), "4365");
						Assert.AreEqual (28, *(scan + 4462), "4462");
						Assert.AreEqual (255, *(scan + 4559), "4559");
						Assert.AreEqual (0, *(scan + 4656), "4656");
						Assert.AreEqual (51, *(scan + 4753), "4753");
						Assert.AreEqual (255, *(scan + 4850), "4850");
						Assert.AreEqual (0, *(scan + 4947), "4947");
						Assert.AreEqual (51, *(scan + 5044), "5044");
						Assert.AreEqual (255, *(scan + 5141), "5141");
						Assert.AreEqual (0, *(scan + 5238), "5238");
						Assert.AreEqual (51, *(scan + 5335), "5335");
						Assert.AreEqual (255, *(scan + 5432), "5432");
						Assert.AreEqual (0, *(scan + 5529), "5529");
						Assert.AreEqual (51, *(scan + 5626), "5626");
						Assert.AreEqual (255, *(scan + 5723), "5723");
						Assert.AreEqual (0, *(scan + 5820), "5820");
						Assert.AreEqual (51, *(scan + 5917), "5917");
						Assert.AreEqual (255, *(scan + 6014), "6014");
						Assert.AreEqual (0, *(scan + 6111), "6111");
						Assert.AreEqual (51, *(scan + 6208), "6208");
						Assert.AreEqual (255, *(scan + 6305), "6305");
						Assert.AreEqual (0, *(scan + 6402), "6402");
						Assert.AreEqual (51, *(scan + 6499), "6499");
						Assert.AreEqual (255, *(scan + 6596), "6596");
						Assert.AreEqual (0, *(scan + 6693), "6693");
						Assert.AreEqual (51, *(scan + 6790), "6790");
						Assert.AreEqual (255, *(scan + 6887), "6887");
						Assert.AreEqual (0, *(scan + 6984), "6984");
						Assert.AreEqual (51, *(scan + 7081), "7081");
						Assert.AreEqual (255, *(scan + 7178), "7178");
						Assert.AreEqual (0, *(scan + 7275), "7275");
						Assert.AreEqual (51, *(scan + 7372), "7372");
						Assert.AreEqual (255, *(scan + 7469), "7469");
						Assert.AreEqual (0, *(scan + 7566), "7566");
						Assert.AreEqual (51, *(scan + 7663), "7663");
						Assert.AreEqual (255, *(scan + 7760), "7760");
						Assert.AreEqual (0, *(scan + 7857), "7857");
						Assert.AreEqual (51, *(scan + 7954), "7954");
						Assert.AreEqual (255, *(scan + 8051), "8051");
						Assert.AreEqual (0, *(scan + 8148), "8148");
						Assert.AreEqual (51, *(scan + 8245), "8245");
						Assert.AreEqual (255, *(scan + 8342), "8342");
						Assert.AreEqual (0, *(scan + 8439), "8439");
						Assert.AreEqual (51, *(scan + 8536), "8536");
						Assert.AreEqual (28, *(scan + 8633), "8633");
						Assert.AreEqual (0, *(scan + 8730), "8730");
						Assert.AreEqual (51, *(scan + 8827), "8827");
						Assert.AreEqual (0, *(scan + 8924), "8924");
						Assert.AreEqual (0, *(scan + 9021), "9021");
						Assert.AreEqual (51, *(scan + 9118), "9118");
						Assert.AreEqual (0, *(scan + 9215), "9215");
						Assert.AreEqual (0, *(scan + 9312), "9312");
						Assert.AreEqual (51, *(scan + 9409), "9409");
						Assert.AreEqual (0, *(scan + 9506), "9506");
						Assert.AreEqual (0, *(scan + 9603), "9603");
						Assert.AreEqual (51, *(scan + 9700), "9700");
						Assert.AreEqual (0, *(scan + 9797), "9797");
						Assert.AreEqual (28, *(scan + 9894), "9894");
						Assert.AreEqual (51, *(scan + 9991), "9991");
						Assert.AreEqual (0, *(scan + 10088), "10088");
						Assert.AreEqual (0, *(scan + 10185), "10185");
						Assert.AreEqual (51, *(scan + 10282), "10282");
						Assert.AreEqual (0, *(scan + 10379), "10379");
						Assert.AreEqual (0, *(scan + 10476), "10476");
						Assert.AreEqual (51, *(scan + 10573), "10573");
						Assert.AreEqual (0, *(scan + 10670), "10670");
						Assert.AreEqual (0, *(scan + 10767), "10767");
						Assert.AreEqual (51, *(scan + 10864), "10864");
						Assert.AreEqual (204, *(scan + 10961), "10961");
						Assert.AreEqual (0, *(scan + 11058), "11058");
						Assert.AreEqual (51, *(scan + 11155), "11155");
						Assert.AreEqual (204, *(scan + 11252), "11252");
						Assert.AreEqual (0, *(scan + 11349), "11349");
						Assert.AreEqual (51, *(scan + 11446), "11446");
						Assert.AreEqual (204, *(scan + 11543), "11543");
						Assert.AreEqual (0, *(scan + 11640), "11640");
						Assert.AreEqual (51, *(scan + 11737), "11737");
						Assert.AreEqual (204, *(scan + 11834), "11834");
						Assert.AreEqual (0, *(scan + 11931), "11931");
						Assert.AreEqual (51, *(scan + 12028), "12028");
						Assert.AreEqual (204, *(scan + 12125), "12125");
						Assert.AreEqual (0, *(scan + 12222), "12222");
						Assert.AreEqual (51, *(scan + 12319), "12319");
						Assert.AreEqual (204, *(scan + 12416), "12416");
						Assert.AreEqual (28, *(scan + 12513), "12513");
						Assert.AreEqual (51, *(scan + 12610), "12610");
						Assert.AreEqual (204, *(scan + 12707), "12707");
						Assert.AreEqual (0, *(scan + 12804), "12804");
						Assert.AreEqual (28, *(scan + 12901), "12901");
						Assert.AreEqual (204, *(scan + 12998), "12998");
						Assert.AreEqual (0, *(scan + 13095), "13095");
						Assert.AreEqual (0, *(scan + 13192), "13192");
						Assert.AreEqual (204, *(scan + 13289), "13289");
						Assert.AreEqual (0, *(scan + 13386), "13386");
						Assert.AreEqual (0, *(scan + 13483), "13483");
						Assert.AreEqual (204, *(scan + 13580), "13580");
						Assert.AreEqual (0, *(scan + 13677), "13677");
						Assert.AreEqual (28, *(scan + 13774), "13774");
						Assert.AreEqual (204, *(scan + 13871), "13871");
						Assert.AreEqual (0, *(scan + 13968), "13968");
						Assert.AreEqual (0, *(scan + 14065), "14065");
						Assert.AreEqual (204, *(scan + 14162), "14162");
						Assert.AreEqual (0, *(scan + 14259), "14259");
						Assert.AreEqual (0, *(scan + 14356), "14356");
						Assert.AreEqual (204, *(scan + 14453), "14453");
						Assert.AreEqual (0, *(scan + 14550), "14550");
						Assert.AreEqual (0, *(scan + 14647), "14647");
						Assert.AreEqual (204, *(scan + 14744), "14744");
						Assert.AreEqual (0, *(scan + 14841), "14841");
						Assert.AreEqual (0, *(scan + 14938), "14938");
						Assert.AreEqual (204, *(scan + 15035), "15035");
						Assert.AreEqual (0, *(scan + 15132), "15132");
						Assert.AreEqual (0, *(scan + 15229), "15229");
						Assert.AreEqual (204, *(scan + 15326), "15326");
						Assert.AreEqual (0, *(scan + 15423), "15423");
						Assert.AreEqual (0, *(scan + 15520), "15520");
						Assert.AreEqual (204, *(scan + 15617), "15617");
						Assert.AreEqual (0, *(scan + 15714), "15714");
						Assert.AreEqual (0, *(scan + 15811), "15811");
						Assert.AreEqual (204, *(scan + 15908), "15908");
						Assert.AreEqual (0, *(scan + 16005), "16005");
						Assert.AreEqual (0, *(scan + 16102), "16102");
						Assert.AreEqual (204, *(scan + 16199), "16199");
						Assert.AreEqual (0, *(scan + 16296), "16296");
						Assert.AreEqual (0, *(scan + 16393), "16393");
						Assert.AreEqual (204, *(scan + 16490), "16490");
						Assert.AreEqual (0, *(scan + 16587), "16587");
						Assert.AreEqual (0, *(scan + 16684), "16684");
						Assert.AreEqual (204, *(scan + 16781), "16781");
						Assert.AreEqual (0, *(scan + 16878), "16878");
						Assert.AreEqual (0, *(scan + 16975), "16975");
						Assert.AreEqual (204, *(scan + 17072), "17072");
						Assert.AreEqual (0, *(scan + 17169), "17169");
						Assert.AreEqual (0, *(scan + 17266), "17266");
						Assert.AreEqual (204, *(scan + 17363), "17363");
						Assert.AreEqual (0, *(scan + 17460), "17460");
						Assert.AreEqual (0, *(scan + 17557), "17557");
						Assert.AreEqual (28, *(scan + 17654), "17654");
						Assert.AreEqual (0, *(scan + 17751), "17751");
						Assert.AreEqual (0, *(scan + 17848), "17848");
						Assert.AreEqual (0, *(scan + 17945), "17945");
						Assert.AreEqual (28, *(scan + 18042), "18042");
						Assert.AreEqual (0, *(scan + 18139), "18139");
						Assert.AreEqual (0, *(scan + 18236), "18236");
						Assert.AreEqual (51, *(scan + 18333), "18333");
						Assert.AreEqual (28, *(scan + 18430), "18430");
						Assert.AreEqual (0, *(scan + 18527), "18527");
						Assert.AreEqual (51, *(scan + 18624), "18624");
						Assert.AreEqual (0, *(scan + 18721), "18721");
						Assert.AreEqual (28, *(scan + 18818), "18818");
						Assert.AreEqual (51, *(scan + 18915), "18915");
						Assert.AreEqual (255, *(scan + 19012), "19012");
						Assert.AreEqual (51, *(scan + 19109), "19109");
						Assert.AreEqual (51, *(scan + 19206), "19206");
						Assert.AreEqual (255, *(scan + 19303), "19303");
						Assert.AreEqual (51, *(scan + 19400), "19400");
						Assert.AreEqual (51, *(scan + 19497), "19497");
						Assert.AreEqual (255, *(scan + 19594), "19594");
						Assert.AreEqual (51, *(scan + 19691), "19691");
						Assert.AreEqual (51, *(scan + 19788), "19788");
						Assert.AreEqual (255, *(scan + 19885), "19885");
						Assert.AreEqual (51, *(scan + 19982), "19982");
						Assert.AreEqual (51, *(scan + 20079), "20079");
						Assert.AreEqual (255, *(scan + 20176), "20176");
						Assert.AreEqual (51, *(scan + 20273), "20273");
						Assert.AreEqual (51, *(scan + 20370), "20370");
						Assert.AreEqual (255, *(scan + 20467), "20467");
						Assert.AreEqual (51, *(scan + 20564), "20564");
						Assert.AreEqual (51, *(scan + 20661), "20661");
						Assert.AreEqual (255, *(scan + 20758), "20758");
						Assert.AreEqual (51, *(scan + 20855), "20855");
						Assert.AreEqual (51, *(scan + 20952), "20952");
						Assert.AreEqual (255, *(scan + 21049), "21049");
						Assert.AreEqual (51, *(scan + 21146), "21146");
						Assert.AreEqual (51, *(scan + 21243), "21243");
						Assert.AreEqual (28, *(scan + 21340), "21340");
						Assert.AreEqual (51, *(scan + 21437), "21437");
						Assert.AreEqual (51, *(scan + 21534), "21534");
						Assert.AreEqual (0, *(scan + 21631), "21631");
						Assert.AreEqual (51, *(scan + 21728), "21728");
						Assert.AreEqual (28, *(scan + 21825), "21825");
						Assert.AreEqual (0, *(scan + 21922), "21922");
						Assert.AreEqual (51, *(scan + 22019), "22019");
						Assert.AreEqual (28, *(scan + 22116), "22116");
						Assert.AreEqual (0, *(scan + 22213), "22213");
						Assert.AreEqual (51, *(scan + 22310), "22310");
						Assert.AreEqual (0, *(scan + 22407), "22407");
						Assert.AreEqual (0, *(scan + 22504), "22504");
						Assert.AreEqual (51, *(scan + 22601), "22601");
						Assert.AreEqual (0, *(scan + 22698), "22698");
						Assert.AreEqual (0, *(scan + 22795), "22795");
						Assert.AreEqual (51, *(scan + 22892), "22892");
						Assert.AreEqual (28, *(scan + 22989), "22989");
						Assert.AreEqual (0, *(scan + 23086), "23086");
						Assert.AreEqual (28, *(scan + 23183), "23183");
						Assert.AreEqual (153, *(scan + 23280), "23280");
						Assert.AreEqual (28, *(scan + 23377), "23377");
						Assert.AreEqual (0, *(scan + 23474), "23474");
						Assert.AreEqual (153, *(scan + 23571), "23571");
						Assert.AreEqual (28, *(scan + 23668), "23668");
						Assert.AreEqual (0, *(scan + 23765), "23765");
						Assert.AreEqual (153, *(scan + 23862), "23862");
						Assert.AreEqual (0, *(scan + 23959), "23959");
						Assert.AreEqual (28, *(scan + 24056), "24056");
						Assert.AreEqual (153, *(scan + 24153), "24153");
						Assert.AreEqual (0, *(scan + 24250), "24250");
						Assert.AreEqual (153, *(scan + 24347), "24347");
						Assert.AreEqual (153, *(scan + 24444), "24444");
						Assert.AreEqual (0, *(scan + 24541), "24541");
						Assert.AreEqual (153, *(scan + 24638), "24638");
						Assert.AreEqual (153, *(scan + 24735), "24735");
						Assert.AreEqual (0, *(scan + 24832), "24832");
						Assert.AreEqual (153, *(scan + 24929), "24929");
						Assert.AreEqual (153, *(scan + 25026), "25026");
						Assert.AreEqual (0, *(scan + 25123), "25123");
						Assert.AreEqual (153, *(scan + 25220), "25220");
						Assert.AreEqual (153, *(scan + 25317), "25317");
						Assert.AreEqual (0, *(scan + 25414), "25414");
						Assert.AreEqual (153, *(scan + 25511), "25511");
						Assert.AreEqual (153, *(scan + 25608), "25608");
						Assert.AreEqual (0, *(scan + 25705), "25705");
						Assert.AreEqual (153, *(scan + 25802), "25802");
						Assert.AreEqual (153, *(scan + 25899), "25899");
						Assert.AreEqual (0, *(scan + 25996), "25996");
						Assert.AreEqual (153, *(scan + 26093), "26093");
						Assert.AreEqual (153, *(scan + 26190), "26190");
						Assert.AreEqual (0, *(scan + 26287), "26287");
						Assert.AreEqual (153, *(scan + 26384), "26384");
						Assert.AreEqual (153, *(scan + 26481), "26481");
						Assert.AreEqual (0, *(scan + 26578), "26578");
						Assert.AreEqual (153, *(scan + 26675), "26675");
						Assert.AreEqual (153, *(scan + 26772), "26772");
						Assert.AreEqual (28, *(scan + 26869), "26869");
						Assert.AreEqual (153, *(scan + 26966), "26966");
						Assert.AreEqual (28, *(scan + 27063), "27063");
						Assert.AreEqual (28, *(scan + 27160), "27160");
						Assert.AreEqual (28, *(scan + 27257), "27257");
						Assert.AreEqual (0, *(scan + 27354), "27354");
						Assert.AreEqual (0, *(scan + 27451), "27451");
						Assert.AreEqual (0, *(scan + 27548), "27548");
						Assert.AreEqual (0, *(scan + 27645), "27645");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		[Test]
		public void Xp32bppIconFeatures ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/32bpp.ico");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.IsTrue (bmp.RawFormat.Equals (ImageFormat.Icon), "Icon");
				// note that image is "promoted" to 32bits
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
				Assert.AreEqual (73746, bmp.Flags, "bmp.Flags");
				Assert.AreEqual (0, bmp.Palette.Entries.Length, "Palette");
				Assert.AreEqual (1, bmp.FrameDimensionsList.Length, "FrameDimensionsList");
				Assert.AreEqual (0, bmp.PropertyIdList.Length, "PropertyIdList");
				Assert.AreEqual (0, bmp.PropertyItems.Length, "PropertyItems");
				Assert.IsNull (bmp.Tag, "Tag");
				Assert.AreEqual (96.0f, bmp.HorizontalResolution, "HorizontalResolution");
				Assert.AreEqual (96.0f, bmp.VerticalResolution, "VerticalResolution");
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
