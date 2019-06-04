//
// PNG Codec class testing unit
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
	public class PngCodecTest {

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

		private bool IsArm64Process ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix || !Environment.Is64BitProcess)
				return false;

			try {
				var process = new global::System.Diagnostics.Process ();
				process.StartInfo.FileName = "uname";
				process.StartInfo.Arguments = "-m";
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.UseShellExecute = false;
				process.Start ();
				process.WaitForExit ();
				var output = process.StandardOutput.ReadToEnd ();

				return output.Trim () == "aarch64";
			} catch {
				return false;
			}
		}

		/* Checks bitmap features on a known 1bbp bitmap */
		[Test]
		public void Bitmap1bitFeatures ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/1bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.AreEqual (PixelFormat.Format1bppIndexed, bmp.PixelFormat);

				Assert.AreEqual (0, bmp.Palette.Flags, "Palette.Flags");
				Assert.AreEqual (2, bmp.Palette.Entries.Length, "Palette.Entries");
				Assert.AreEqual (-16777216, bmp.Palette.Entries[0].ToArgb (), "Palette.0");
				Assert.AreEqual (-1, bmp.Palette.Entries[1].ToArgb (), "Palette.1");

				Assert.AreEqual (288, bmp.Width, "bmp.Width");
				Assert.AreEqual (384, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (288, rect.Width, "rect.Width");
				Assert.AreEqual (384, rect.Height, "rect.Height");

				Assert.AreEqual (288, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (384, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap1bitPixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/1bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 32) {
					for (int y = 0; y < bmp.Height; y += 32)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-1, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-1, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-1, bmp.GetPixel (0, 64).ToArgb (), "0,64");
				Assert.AreEqual (-1, bmp.GetPixel (0, 96).ToArgb (), "0,96");
				Assert.AreEqual (-1, bmp.GetPixel (0, 128).ToArgb (), "0,128");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 160).ToArgb (), "0,160");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 192).ToArgb (), "0,192");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 224).ToArgb (), "0,224");
				Assert.AreEqual (-1, bmp.GetPixel (0, 256).ToArgb (), "0,256");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 288).ToArgb (), "0,288");
				Assert.AreEqual (-1, bmp.GetPixel (0, 320).ToArgb (), "0,320");
				Assert.AreEqual (-16777216, bmp.GetPixel (0, 352).ToArgb (), "0,352");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 0).ToArgb (), "32,0");
				Assert.AreEqual (-1, bmp.GetPixel (32, 32).ToArgb (), "32,32");
				Assert.AreEqual (-1, bmp.GetPixel (32, 64).ToArgb (), "32,64");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 96).ToArgb (), "32,96");
				Assert.AreEqual (-1, bmp.GetPixel (32, 128).ToArgb (), "32,128");
				Assert.AreEqual (-1, bmp.GetPixel (32, 160).ToArgb (), "32,160");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 192).ToArgb (), "32,192");
				Assert.AreEqual (-1, bmp.GetPixel (32, 224).ToArgb (), "32,224");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 256).ToArgb (), "32,256");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 288).ToArgb (), "32,288");
				Assert.AreEqual (-1, bmp.GetPixel (32, 320).ToArgb (), "32,320");
				Assert.AreEqual (-1, bmp.GetPixel (32, 352).ToArgb (), "32,352");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 0).ToArgb (), "64,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 32).ToArgb (), "64,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 64).ToArgb (), "64,64");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 96).ToArgb (), "64,96");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 128).ToArgb (), "64,128");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 160).ToArgb (), "64,160");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 192).ToArgb (), "64,192");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 224).ToArgb (), "64,224");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 256).ToArgb (), "64,256");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 288).ToArgb (), "64,288");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 320).ToArgb (), "64,320");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 352).ToArgb (), "64,352");
				Assert.AreEqual (-1, bmp.GetPixel (96, 0).ToArgb (), "96,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 32).ToArgb (), "96,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 64).ToArgb (), "96,64");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 96).ToArgb (), "96,96");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 128).ToArgb (), "96,128");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 160).ToArgb (), "96,160");
				Assert.AreEqual (-1, bmp.GetPixel (96, 192).ToArgb (), "96,192");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 224).ToArgb (), "96,224");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 256).ToArgb (), "96,256");
				Assert.AreEqual (-16777216, bmp.GetPixel (96, 288).ToArgb (), "96,288");
				Assert.AreEqual (-1, bmp.GetPixel (96, 320).ToArgb (), "96,320");
				Assert.AreEqual (-1, bmp.GetPixel (96, 352).ToArgb (), "96,352");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 0).ToArgb (), "128,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 32).ToArgb (), "128,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 64).ToArgb (), "128,64");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 96).ToArgb (), "128,96");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 128).ToArgb (), "128,128");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 160).ToArgb (), "128,160");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 192).ToArgb (), "128,192");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 224).ToArgb (), "128,224");
				Assert.AreEqual (-1, bmp.GetPixel (128, 256).ToArgb (), "128,256");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 288).ToArgb (), "128,288");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 320).ToArgb (), "128,320");
				Assert.AreEqual (-16777216, bmp.GetPixel (128, 352).ToArgb (), "128,352");
				Assert.AreEqual (-1, bmp.GetPixel (160, 0).ToArgb (), "160,0");
				Assert.AreEqual (-1, bmp.GetPixel (160, 32).ToArgb (), "160,32");
				Assert.AreEqual (-16777216, bmp.GetPixel (160, 64).ToArgb (), "160,64");
				Assert.AreEqual (-1, bmp.GetPixel (160, 96).ToArgb (), "160,96");
				Assert.AreEqual (-16777216, bmp.GetPixel (160, 128).ToArgb (), "160,128");
				Assert.AreEqual (-1, bmp.GetPixel (160, 160).ToArgb (), "160,160");
#endif
			}
		}

		[Test]
		public void Bitmap1bitData ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/1bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					Assert.AreEqual (864, data.Stride, "Stride");
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
						Assert.AreEqual (255, *(scan + 0), "0");
						Assert.AreEqual (255, *(scan + 1009), "1009");
						Assert.AreEqual (255, *(scan + 2018), "2018");
						Assert.AreEqual (255, *(scan + 3027), "3027");
						Assert.AreEqual (255, *(scan + 4036), "4036");
						Assert.AreEqual (255, *(scan + 5045), "5045");
						Assert.AreEqual (255, *(scan + 6054), "6054");
						Assert.AreEqual (255, *(scan + 7063), "7063");
						Assert.AreEqual (255, *(scan + 8072), "8072");
						Assert.AreEqual (255, *(scan + 9081), "9081");
						Assert.AreEqual (255, *(scan + 10090), "10090");
						Assert.AreEqual (0, *(scan + 11099), "11099");
						Assert.AreEqual (255, *(scan + 12108), "12108");
						Assert.AreEqual (255, *(scan + 13117), "13117");
						Assert.AreEqual (0, *(scan + 14126), "14126");
						Assert.AreEqual (255, *(scan + 15135), "15135");
						Assert.AreEqual (255, *(scan + 16144), "16144");
						Assert.AreEqual (0, *(scan + 17153), "17153");
						Assert.AreEqual (0, *(scan + 18162), "18162");
						Assert.AreEqual (255, *(scan + 19171), "19171");
						Assert.AreEqual (0, *(scan + 20180), "20180");
						Assert.AreEqual (255, *(scan + 21189), "21189");
						Assert.AreEqual (255, *(scan + 22198), "22198");
						Assert.AreEqual (0, *(scan + 23207), "23207");
						Assert.AreEqual (0, *(scan + 24216), "24216");
						Assert.AreEqual (0, *(scan + 25225), "25225");
						Assert.AreEqual (0, *(scan + 26234), "26234");
						Assert.AreEqual (255, *(scan + 27243), "27243");
						Assert.AreEqual (255, *(scan + 28252), "28252");
						Assert.AreEqual (0, *(scan + 29261), "29261");
						Assert.AreEqual (255, *(scan + 30270), "30270");
						Assert.AreEqual (0, *(scan + 31279), "31279");
						Assert.AreEqual (0, *(scan + 32288), "32288");
						Assert.AreEqual (255, *(scan + 33297), "33297");
						Assert.AreEqual (255, *(scan + 34306), "34306");
						Assert.AreEqual (255, *(scan + 35315), "35315");
						Assert.AreEqual (255, *(scan + 36324), "36324");
						Assert.AreEqual (0, *(scan + 37333), "37333");
						Assert.AreEqual (255, *(scan + 38342), "38342");
						Assert.AreEqual (255, *(scan + 39351), "39351");
						Assert.AreEqual (255, *(scan + 40360), "40360");
						Assert.AreEqual (255, *(scan + 41369), "41369");
						Assert.AreEqual (255, *(scan + 42378), "42378");
						Assert.AreEqual (0, *(scan + 43387), "43387");
						Assert.AreEqual (0, *(scan + 44396), "44396");
						Assert.AreEqual (255, *(scan + 45405), "45405");
						Assert.AreEqual (255, *(scan + 46414), "46414");
						Assert.AreEqual (255, *(scan + 47423), "47423");
						Assert.AreEqual (255, *(scan + 48432), "48432");
						Assert.AreEqual (255, *(scan + 49441), "49441");
						Assert.AreEqual (0, *(scan + 50450), "50450");
						Assert.AreEqual (0, *(scan + 51459), "51459");
						Assert.AreEqual (255, *(scan + 52468), "52468");
						Assert.AreEqual (255, *(scan + 53477), "53477");
						Assert.AreEqual (255, *(scan + 54486), "54486");
						Assert.AreEqual (0, *(scan + 55495), "55495");
						Assert.AreEqual (0, *(scan + 56504), "56504");
						Assert.AreEqual (0, *(scan + 57513), "57513");
						Assert.AreEqual (255, *(scan + 58522), "58522");
						Assert.AreEqual (255, *(scan + 59531), "59531");
						Assert.AreEqual (0, *(scan + 60540), "60540");
						Assert.AreEqual (0, *(scan + 61549), "61549");
						Assert.AreEqual (0, *(scan + 62558), "62558");
						Assert.AreEqual (0, *(scan + 63567), "63567");
						Assert.AreEqual (255, *(scan + 64576), "64576");
						Assert.AreEqual (0, *(scan + 65585), "65585");
						Assert.AreEqual (255, *(scan + 66594), "66594");
						Assert.AreEqual (255, *(scan + 67603), "67603");
						Assert.AreEqual (0, *(scan + 68612), "68612");
						Assert.AreEqual (0, *(scan + 69621), "69621");
						Assert.AreEqual (0, *(scan + 70630), "70630");
						Assert.AreEqual (0, *(scan + 71639), "71639");
						Assert.AreEqual (0, *(scan + 72648), "72648");
						Assert.AreEqual (255, *(scan + 73657), "73657");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		/* Checks bitmap features on a known 2bbp bitmap */
		[Test]
		[Category("NotWorking")]
		public void Bitmap2bitFeatures ()
		{
			if (IsArm64Process ())
				Assert.Ignore ("https://bugzilla.xamarin.com/show_bug.cgi?id=41171");

			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/81674-2bpp.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				// quite a promotion! (2 -> 32)
				Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);

				// MS returns a random Flags value (not a good sign)
				//Assert.AreEqual (0, bmp.Palette.Flags, "Palette.Flags");
				Assert.AreEqual (0, bmp.Palette.Entries.Length, "Palette.Entries");

				Assert.AreEqual (100, bmp.Width, "bmp.Width");
				Assert.AreEqual (100, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (100, rect.Width, "rect.Width");
				Assert.AreEqual (100, rect.Height, "rect.Height");

				Assert.AreEqual (100, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (100, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Bitmap2bitPixels ()
		{
			if (IsArm64Process ())
				Assert.Ignore ("https://bugzilla.xamarin.com/show_bug.cgi?id=41171");

			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/81674-2bpp.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 32) {
					for (int y = 0; y < bmp.Height; y += 32)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-11249559, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-11249559, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-11249559, bmp.GetPixel (0, 64).ToArgb (), "0,64");
				Assert.AreEqual (-11249559, bmp.GetPixel (0, 96).ToArgb (), "0,96");
				Assert.AreEqual (-11249559, bmp.GetPixel (32, 0).ToArgb (), "32,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (32, 32).ToArgb (), "32,32");
				Assert.AreEqual (-11249559, bmp.GetPixel (32, 64).ToArgb (), "32,64");
				Assert.AreEqual (-11249559, bmp.GetPixel (32, 96).ToArgb (), "32,96");
				Assert.AreEqual (-11249559, bmp.GetPixel (64, 0).ToArgb (), "64,0");
				Assert.AreEqual (-16777216, bmp.GetPixel (64, 32).ToArgb (), "64,32");
				Assert.AreEqual (-11249559, bmp.GetPixel (64, 64).ToArgb (), "64,64");
				Assert.AreEqual (-11249559, bmp.GetPixel (64, 96).ToArgb (), "64,96");
				Assert.AreEqual (-11249559, bmp.GetPixel (96, 0).ToArgb (), "96,0");
				Assert.AreEqual (-11249559, bmp.GetPixel (96, 32).ToArgb (), "96,32");
				Assert.AreEqual (-11249559, bmp.GetPixel (96, 64).ToArgb (), "96,64");
				Assert.AreEqual (-11249559, bmp.GetPixel (96, 96).ToArgb (), "96,96");
#endif
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Bitmap2bitData ()
		{
			if (IsArm64Process ())
				Assert.Ignore ("https://bugzilla.xamarin.com/show_bug.cgi?id=41171");

			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/81674-2bpp.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					Assert.AreEqual (300, data.Stride, "Stride");
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
						Assert.AreEqual (105, *(scan + 0), "0");
						Assert.AreEqual (88, *(scan + 1009), "1009");
						Assert.AreEqual (255, *(scan + 2018), "2018");
						Assert.AreEqual (105, *(scan + 3027), "3027");
						Assert.AreEqual (88, *(scan + 4036), "4036");
						Assert.AreEqual (84, *(scan + 5045), "5045");
						Assert.AreEqual (255, *(scan + 6054), "6054");
						Assert.AreEqual (88, *(scan + 7063), "7063");
						Assert.AreEqual (84, *(scan + 8072), "8072");
						Assert.AreEqual (0, *(scan + 9081), "9081");
						Assert.AreEqual (0, *(scan + 10090), "10090");
						Assert.AreEqual (84, *(scan + 11099), "11099");
						Assert.AreEqual (0, *(scan + 12108), "12108");
						Assert.AreEqual (88, *(scan + 13117), "13117");
						Assert.AreEqual (84, *(scan + 14126), "14126");
						Assert.AreEqual (105, *(scan + 15135), "15135");
						Assert.AreEqual (88, *(scan + 16144), "16144");
						Assert.AreEqual (84, *(scan + 17153), "17153");
						Assert.AreEqual (0, *(scan + 18162), "18162");
						Assert.AreEqual (88, *(scan + 19171), "19171");
						Assert.AreEqual (84, *(scan + 20180), "20180");
						Assert.AreEqual (0, *(scan + 21189), "21189");
						Assert.AreEqual (88, *(scan + 22198), "22198");
						Assert.AreEqual (84, *(scan + 23207), "23207");
						Assert.AreEqual (105, *(scan + 24216), "24216");
						Assert.AreEqual (88, *(scan + 25225), "25225");
						Assert.AreEqual (0, *(scan + 26234), "26234");
						Assert.AreEqual (105, *(scan + 27243), "27243");
						Assert.AreEqual (88, *(scan + 28252), "28252");
						Assert.AreEqual (84, *(scan + 29261), "29261");
#endif
					}
				}
				finally {
					bmp.UnlockBits (data);
				}
			}
		}

		/* Checks bitmap features on a known 4bbp bitmap */
		[Test]
		public void Bitmap4bitFeatures ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/4bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				GraphicsUnit unit = GraphicsUnit.World;
				RectangleF rect = bmp.GetBounds (ref unit);

				Assert.AreEqual (PixelFormat.Format4bppIndexed, bmp.PixelFormat);

				Assert.AreEqual (0, bmp.Palette.Flags, "Palette.Flags");
				Assert.AreEqual (16, bmp.Palette.Entries.Length, "Palette.Entries");
				Assert.AreEqual (-12106173, bmp.Palette.Entries[0].ToArgb (), "Palette.0");
				Assert.AreEqual (-10979957, bmp.Palette.Entries[1].ToArgb (), "Palette.1");
				Assert.AreEqual (-8879241, bmp.Palette.Entries[2].ToArgb (), "Palette.2");
				Assert.AreEqual (-10381134, bmp.Palette.Entries[3].ToArgb (), "Palette.3");
				Assert.AreEqual (-7441574, bmp.Palette.Entries[4].ToArgb (), "Palette.4");
				Assert.AreEqual (-6391673, bmp.Palette.Entries[5].ToArgb (), "Palette.5");
				Assert.AreEqual (-5861009, bmp.Palette.Entries[6].ToArgb (), "Palette.6");
				Assert.AreEqual (-3824008, bmp.Palette.Entries[7].ToArgb (), "Palette.7");
				Assert.AreEqual (-5790569, bmp.Palette.Entries[8].ToArgb (), "Palette.8");
				Assert.AreEqual (-6178617, bmp.Palette.Entries[9].ToArgb (), "Palette.9");
				Assert.AreEqual (-4668490, bmp.Palette.Entries[10].ToArgb (), "Palette.10");
				Assert.AreEqual (-5060143, bmp.Palette.Entries[11].ToArgb (), "Palette.11");
				Assert.AreEqual (-3492461, bmp.Palette.Entries[12].ToArgb (), "Palette.12");
				Assert.AreEqual (-2967099, bmp.Palette.Entries[13].ToArgb (), "Palette.13");
				Assert.AreEqual (-2175574, bmp.Palette.Entries[14].ToArgb (), "Palette.14");
				Assert.AreEqual (-1314578, bmp.Palette.Entries[15].ToArgb (), "Palette.15");

				Assert.AreEqual (288, bmp.Width, "bmp.Width");
				Assert.AreEqual (384, bmp.Height, "bmp.Height");

				Assert.AreEqual (0, rect.X, "rect.X");
				Assert.AreEqual (0, rect.Y, "rect.Y");
				Assert.AreEqual (288, rect.Width, "rect.Width");
				Assert.AreEqual (384, rect.Height, "rect.Height");

				Assert.AreEqual (288, bmp.Size.Width, "bmp.Size.Width");
				Assert.AreEqual (384, bmp.Size.Height, "bmp.Size.Height");
			}
		}

		[Test]
		public void Bitmap4bitPixels ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/4bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
#if false
				for (int x = 0; x < bmp.Width; x += 32) {
					for (int y = 0; y < bmp.Height; y += 32)
						Console.WriteLine ("\t\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
				}
#else
				// sampling values from a well known bitmap
				Assert.AreEqual (-10381134, bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (-1314578, bmp.GetPixel (0, 32).ToArgb (), "0,32");
				Assert.AreEqual (-1314578, bmp.GetPixel (0, 64).ToArgb (), "0,64");
				Assert.AreEqual (-1314578, bmp.GetPixel (0, 96).ToArgb (), "0,96");
				Assert.AreEqual (-3824008, bmp.GetPixel (0, 128).ToArgb (), "0,128");
				Assert.AreEqual (-12106173, bmp.GetPixel (0, 160).ToArgb (), "0,160");
				Assert.AreEqual (-12106173, bmp.GetPixel (0, 192).ToArgb (), "0,192");
				Assert.AreEqual (-12106173, bmp.GetPixel (0, 224).ToArgb (), "0,224");
				Assert.AreEqual (-12106173, bmp.GetPixel (0, 256).ToArgb (), "0,256");
				Assert.AreEqual (-7441574, bmp.GetPixel (0, 288).ToArgb (), "0,288");
				Assert.AreEqual (-3492461, bmp.GetPixel (0, 320).ToArgb (), "0,320");
				Assert.AreEqual (-5861009, bmp.GetPixel (0, 352).ToArgb (), "0,352");
				Assert.AreEqual (-10381134, bmp.GetPixel (32, 0).ToArgb (), "32,0");
				Assert.AreEqual (-1314578, bmp.GetPixel (32, 32).ToArgb (), "32,32");
				Assert.AreEqual (-7441574, bmp.GetPixel (32, 64).ToArgb (), "32,64");
				Assert.AreEqual (-12106173, bmp.GetPixel (32, 96).ToArgb (), "32,96");
				Assert.AreEqual (-1314578, bmp.GetPixel (32, 128).ToArgb (), "32,128");
				Assert.AreEqual (-1314578, bmp.GetPixel (32, 160).ToArgb (), "32,160");
				Assert.AreEqual (-12106173, bmp.GetPixel (32, 192).ToArgb (), "32,192");
				Assert.AreEqual (-12106173, bmp.GetPixel (32, 224).ToArgb (), "32,224");
				Assert.AreEqual (-12106173, bmp.GetPixel (32, 256).ToArgb (), "32,256");
				Assert.AreEqual (-12106173, bmp.GetPixel (32, 288).ToArgb (), "32,288");
				Assert.AreEqual (-3492461, bmp.GetPixel (32, 320).ToArgb (), "32,320");
				Assert.AreEqual (-2175574, bmp.GetPixel (32, 352).ToArgb (), "32,352");
				Assert.AreEqual (-6178617, bmp.GetPixel (64, 0).ToArgb (), "64,0");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 32).ToArgb (), "64,32");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 64).ToArgb (), "64,64");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 96).ToArgb (), "64,96");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 128).ToArgb (), "64,128");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 160).ToArgb (), "64,160");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 192).ToArgb (), "64,192");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 224).ToArgb (), "64,224");
				Assert.AreEqual (-5790569, bmp.GetPixel (64, 256).ToArgb (), "64,256");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 288).ToArgb (), "64,288");
				Assert.AreEqual (-12106173, bmp.GetPixel (64, 320).ToArgb (), "64,320");
				Assert.AreEqual (-5790569, bmp.GetPixel (64, 352).ToArgb (), "64,352");
				Assert.AreEqual (-1314578, bmp.GetPixel (96, 0).ToArgb (), "96,0");
				Assert.AreEqual (-10381134, bmp.GetPixel (96, 32).ToArgb (), "96,32");
				Assert.AreEqual (-12106173, bmp.GetPixel (96, 64).ToArgb (), "96,64");
				Assert.AreEqual (-12106173, bmp.GetPixel (96, 96).ToArgb (), "96,96");
				Assert.AreEqual (-7441574, bmp.GetPixel (96, 128).ToArgb (), "96,128");
				Assert.AreEqual (-12106173, bmp.GetPixel (96, 160).ToArgb (), "96,160");
				Assert.AreEqual (-5790569, bmp.GetPixel (96, 192).ToArgb (), "96,192");
				Assert.AreEqual (-12106173, bmp.GetPixel (96, 224).ToArgb (), "96,224");
				Assert.AreEqual (-4668490, bmp.GetPixel (96, 256).ToArgb (), "96,256");
				Assert.AreEqual (-12106173, bmp.GetPixel (96, 288).ToArgb (), "96,288");
				Assert.AreEqual (-1314578, bmp.GetPixel (96, 320).ToArgb (), "96,320");
				Assert.AreEqual (-3492461, bmp.GetPixel (96, 352).ToArgb (), "96,352");
				Assert.AreEqual (-5861009, bmp.GetPixel (128, 0).ToArgb (), "128,0");
				Assert.AreEqual (-7441574, bmp.GetPixel (128, 32).ToArgb (), "128,32");
				Assert.AreEqual (-7441574, bmp.GetPixel (128, 64).ToArgb (), "128,64");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 96).ToArgb (), "128,96");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 128).ToArgb (), "128,128");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 160).ToArgb (), "128,160");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 192).ToArgb (), "128,192");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 224).ToArgb (), "128,224");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 256).ToArgb (), "128,256");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 288).ToArgb (), "128,288");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 320).ToArgb (), "128,320");
				Assert.AreEqual (-12106173, bmp.GetPixel (128, 352).ToArgb (), "128,352");
				Assert.AreEqual (-1314578, bmp.GetPixel (160, 0).ToArgb (), "160,0");
				Assert.AreEqual (-1314578, bmp.GetPixel (160, 32).ToArgb (), "160,32");
				Assert.AreEqual (-12106173, bmp.GetPixel (160, 64).ToArgb (), "160,64");
				Assert.AreEqual (-1314578, bmp.GetPixel (160, 96).ToArgb (), "160,96");
				Assert.AreEqual (-12106173, bmp.GetPixel (160, 128).ToArgb (), "160,128");
				Assert.AreEqual (-5790569, bmp.GetPixel (160, 160).ToArgb (), "160,160");
				Assert.AreEqual (-12106173, bmp.GetPixel (160, 192).ToArgb (), "160,192");
#endif
			}
		}

		[Test]
		public void Bitmap4bitData ()
		{
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/4bit.png");
			using (Bitmap bmp = new Bitmap (sInFile)) {
				BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try {
					Assert.AreEqual (bmp.Height, data.Height, "Height");
					Assert.AreEqual (bmp.Width, data.Width, "Width");
					Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
					Assert.AreEqual (864, data.Stride, "Stride");
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
						Assert.AreEqual (178, *(scan + 0), "0");
						Assert.AreEqual (184, *(scan + 1009), "1009");
						Assert.AreEqual (235, *(scan + 2018), "2018");
						Assert.AreEqual (209, *(scan + 3027), "3027");
						Assert.AreEqual (240, *(scan + 4036), "4036");
						Assert.AreEqual (142, *(scan + 5045), "5045");
						Assert.AreEqual (139, *(scan + 6054), "6054");
						Assert.AreEqual (152, *(scan + 7063), "7063");
						Assert.AreEqual (235, *(scan + 8072), "8072");
						Assert.AreEqual (209, *(scan + 9081), "9081");
						Assert.AreEqual (240, *(scan + 10090), "10090");
						Assert.AreEqual (142, *(scan + 11099), "11099");
						Assert.AreEqual (199, *(scan + 12108), "12108");
						Assert.AreEqual (201, *(scan + 13117), "13117");
						Assert.AreEqual (97, *(scan + 14126), "14126");
						Assert.AreEqual (238, *(scan + 15135), "15135");
						Assert.AreEqual (240, *(scan + 16144), "16144");
						Assert.AreEqual (158, *(scan + 17153), "17153");
						Assert.AreEqual (119, *(scan + 18162), "18162");
						Assert.AreEqual (201, *(scan + 19171), "19171");
						Assert.AreEqual (88, *(scan + 20180), "20180");
						Assert.AreEqual (238, *(scan + 21189), "21189");
						Assert.AreEqual (240, *(scan + 22198), "22198");
						Assert.AreEqual (120, *(scan + 23207), "23207");
						Assert.AreEqual (182, *(scan + 24216), "24216");
						Assert.AreEqual (70, *(scan + 25225), "25225");
						Assert.AreEqual (71, *(scan + 26234), "26234");
						Assert.AreEqual (238, *(scan + 27243), "27243");
						Assert.AreEqual (240, *(scan + 28252), "28252");
						Assert.AreEqual (120, *(scan + 29261), "29261");
						Assert.AreEqual (238, *(scan + 30270), "30270");
						Assert.AreEqual (70, *(scan + 31279), "31279");
						Assert.AreEqual (71, *(scan + 32288), "32288");
						Assert.AreEqual (238, *(scan + 33297), "33297");
						Assert.AreEqual (240, *(scan + 34306), "34306");
						Assert.AreEqual (210, *(scan + 35315), "35315");
						Assert.AreEqual (238, *(scan + 36324), "36324");
						Assert.AreEqual (70, *(scan + 37333), "37333");
						Assert.AreEqual (97, *(scan + 38342), "38342");
						Assert.AreEqual (238, *(scan + 39351), "39351");
						Assert.AreEqual (240, *(scan + 40360), "40360");
						Assert.AreEqual (235, *(scan + 41369), "41369");
						Assert.AreEqual (238, *(scan + 42378), "42378");
						Assert.AreEqual (117, *(scan + 43387), "43387");
						Assert.AreEqual (158, *(scan + 44396), "44396");
						Assert.AreEqual (170, *(scan + 45405), "45405");
						Assert.AreEqual (240, *(scan + 46414), "46414");
						Assert.AreEqual (235, *(scan + 47423), "47423");
						Assert.AreEqual (209, *(scan + 48432), "48432");
						Assert.AreEqual (120, *(scan + 49441), "49441");
						Assert.AreEqual (71, *(scan + 50450), "50450");
						Assert.AreEqual (119, *(scan + 51459), "51459");
						Assert.AreEqual (240, *(scan + 52468), "52468");
						Assert.AreEqual (235, *(scan + 53477), "53477");
						Assert.AreEqual (209, *(scan + 54486), "54486");
						Assert.AreEqual (70, *(scan + 55495), "55495");
						Assert.AreEqual (71, *(scan + 56504), "56504");
						Assert.AreEqual (67, *(scan + 57513), "57513");
						Assert.AreEqual (240, *(scan + 58522), "58522");
						Assert.AreEqual (167, *(scan + 59531), "59531");
						Assert.AreEqual (67, *(scan + 60540), "60540");
						Assert.AreEqual (70, *(scan + 61549), "61549");
						Assert.AreEqual (71, *(scan + 62558), "62558");
						Assert.AreEqual (67, *(scan + 63567), "63567");
						Assert.AreEqual (240, *(scan + 64576), "64576");
						Assert.AreEqual (120, *(scan + 65585), "65585");
						Assert.AreEqual (182, *(scan + 66594), "66594");
						Assert.AreEqual (70, *(scan + 67603), "67603");
						Assert.AreEqual (120, *(scan + 68612), "68612");
						Assert.AreEqual (67, *(scan + 69621), "69621");
						Assert.AreEqual (70, *(scan + 70630), "70630");
						Assert.AreEqual (71, *(scan + 71639), "71639");
						Assert.AreEqual (90, *(scan + 72648), "72648");
						Assert.AreEqual (240, *(scan + 73657), "73657");
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
			string sOutFile = String.Format ("linerect{0}-{1}.png", getOutSufix (), expected.ToString ());

			// Save		
			Bitmap bmp = new Bitmap (100, 100, original);
			Graphics gr = Graphics.FromImage (bmp);

			using (Pen p = new Pen (Color.BlueViolet, 2)) {
				gr.DrawLine (p, 10.0F, 10.0F, 90.0F, 90.0F);
				gr.DrawRectangle (p, 10.0F, 10.0F, 80.0F, 80.0F);
			}

			try {
				bmp.Save (sOutFile, ImageFormat.Png);

				// Load
				using (Bitmap bmpLoad = new Bitmap (sOutFile)) {
					Assert.AreEqual (expected, bmpLoad.PixelFormat, "PixelFormat");
					if (colorCheck) {
						Color color = bmpLoad.GetPixel (10, 10);
						Assert.AreEqual (Color.FromArgb (255, 138, 43, 226), color, "BlueViolet");
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
