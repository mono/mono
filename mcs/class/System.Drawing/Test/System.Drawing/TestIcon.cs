//
// Icon class testing unit
//
// Authors:
//	Gary Barnett <gary.barnett.mono@gmail.com>
// 	Sanjay Gupta <gsanjay@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2006-2008 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Drawing {

	[TestFixture]	
	public class IconTest {
		
		Icon icon;
		Icon icon16, icon32, icon48, icon64, icon96;
		FileStream fs1;

		static string filename_dll;

		// static ctor are executed outside the Deny
		static IconTest ()
		{
			filename_dll = Assembly.GetExecutingAssembly ().Location;
		}
		
		[SetUp]
		public void SetUp ()		
		{
			String path = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/smiley.ico");
			icon = new Icon (path);			
			fs1 = new FileStream (path, FileMode.Open);

			icon16 = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico"));
			icon32 = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/32x32x16.ico"));
			icon48 = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/48x48x1.ico"));
			icon64 = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/64x64x256.ico"));
			icon96 = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/96x96x256.ico"));
		}

		[TearDown]
		public void TearDown ()
		{
			if (fs1 != null)
				fs1.Close ();
			if (File.Exists ("newIcon.ico"))
				File.Delete ("newIcon.ico");
		}

		[Test]
		public void TestConstructors ()
		{
			Assert.AreEqual (32, icon.Height, "C#0a");
			Assert.AreEqual (32, icon.Width, "C#0b");

			Icon newIcon = new Icon (fs1, 48, 48);
			Assert.AreEqual (48, newIcon.Height, "C#1a"); 			
			Assert.AreEqual (48, newIcon.Width, "C#1b");

			newIcon = new Icon (icon, 16, 16);
			Assert.AreEqual (16, newIcon.Height, "C#2a"); 			
			Assert.AreEqual (16, newIcon.Width, "C#2b");
		}

		[Test]
		public void Constructor_IconNull_Int_Int ()
		{
			Assert.Throws<ArgumentException> (() => new Icon ((Icon)null, 32, 32));
		}

		[Test]
		public void Constructor_Icon_IntNegative_Int ()
		{
			Icon neg = new Icon (icon, -32, 32);
			Assert.AreEqual (32, neg.Height, "Height");
			Assert.AreEqual (32, neg.Width, "Width");
		}

		[Test]
		public void Constructor_IconNull_Size ()
		{
			Assert.Throws<ArgumentException> (() => new Icon ((Icon) null, new Size (32, 32)));
		}

		[Test]
		public void Constructor_Icon_Size_Negative ()
		{
			Icon neg = new Icon (icon, new Size (-32, -32));
			Assert.AreEqual (16, neg.Height, "Height");
			Assert.AreEqual (16, neg.Width, "Width");
		}

		[Test]
		public void Constructor_Icon_Int_Int_NonSquare ()
		{
			Icon non_square = new Icon (icon, 32, 16);
			Assert.AreEqual (32, non_square.Height, "Height");
			Assert.AreEqual (32, non_square.Width, "Width");
		}

		[Test]
		public void Constructor_Icon_GetNormalSizeFromIconWith256 ()
		{
			string filepath = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/323511.ico");

			Icon orig = new Icon (filepath);
			Assert.AreEqual (32,orig.Height);
			Assert.AreEqual (32,orig.Width);

			Icon ret = new Icon (orig, 48, 48);
			Assert.AreEqual (48, ret.Height);
			Assert.AreEqual (48, ret.Width);
		}

		[Test]
		public void Constructor_Icon_DoesntReturn256Passing0 ()
		{
			string filepath = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/323511.ico");
			
			Icon orig = new Icon (filepath);
			Assert.AreEqual (32,orig.Height);
			Assert.AreEqual (32,orig.Width);
			
			Icon ret = new Icon (orig, 0, 0);
			Assert.AreNotEqual (0, ret.Height);
			Assert.AreNotEqual (0, ret.Width);
		}

		[Test]
		public void Constructor_Icon_DoesntReturn256Passing1 ()
		{
			string filepath = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/323511.ico");
			
			Icon orig = new Icon (filepath);
			Assert.AreEqual (32,orig.Height);
			Assert.AreEqual (32,orig.Width);
			
			Icon ret = new Icon (orig, 1, 1);
			Assert.AreNotEqual (0, ret.Height);
			Assert.AreNotEqual (0, ret.Width);
		}

		[Test]
		public void Constructor_StreamNull ()
		{
			Assert.Throws<ArgumentException> (() => new Icon ((Stream) null));
		}

		[Test]
		public void Constructor_StreamNull_Int_Int ()
		{
			Assert.Throws<ArgumentException> (() => new Icon ((Stream) null, 32, 32));
		}

		[Test]
		public void Constructor_StringNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new Icon ((string) null));
		}

		[Test]
		public void Constructor_TypeNull_String ()
		{
			Assert.Throws<NullReferenceException> (() => new Icon ((Type) null, "mono.ico"));
		}

		[Test]
		public void Constructor_Type_StringNull ()
		{
			Assert.Throws<ArgumentException> (() => new Icon (typeof (Icon), null));
		}
		[Test]
		public void Constructor_StreamNull_Size ()
		{
			Assert.Throws<ArgumentException> (() => new Icon ((Stream) null, new Size (32, 32)));
		}

		[Test]
		public void Constructor_StringNull_Size ()
		{
			Assert.Throws<ArgumentNullException> (() => new Icon ((string) null, new Size (32, 32)));
		}

		[Test]
		public void Constructor_StringNull_Int_Int ()
		{
			Assert.Throws<ArgumentNullException> (() => new Icon ((string) null, 32, 32));
		}

		[Test]
		public void TestProperties ()
		{
			Assert.AreEqual (32, icon.Height, "P#1");
			Assert.AreEqual (32, icon.Width, "P#2");
			Assert.AreEqual (32, icon.Size.Width, "P#3");
			Assert.AreEqual (32, icon.Size.Height, "P#4");
		}

		[Test]
		public void Clone ()
		{
			Icon clone = (Icon) icon.Clone ();
			Assert.AreEqual (32, clone.Height, "Height");
			Assert.AreEqual (32, clone.Width, "Width");
			Assert.AreEqual (32, clone.Size.Width, "Size.Width");
			Assert.AreEqual (32, clone.Size.Height, "Size.Height");
		}

		[Test]
		public void CloneHandleIcon ()
		{
			Icon clone = (Icon) Icon.FromHandle (SystemIcons.Hand.Handle).Clone ();
			Assert.AreEqual (SystemIcons.Hand.Height, clone.Height, "Height");
			Assert.AreEqual (SystemIcons.Hand.Width, clone.Width, "Width");
			Assert.AreEqual (SystemIcons.Hand.Size.Width, clone.Size.Width, "Size.Width");
			Assert.AreEqual (SystemIcons.Hand.Size.Height, clone.Size.Height, "Size.Height");
		}

		private void XPIcon (int size)
		{
			// note: the Icon(string,Size) or Icon(string,int,int) doesn't exists under 1.x
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/32bpp.ico"))) {
				using (Icon xp = new Icon (fs, size, size)) {
					Assert.AreEqual (size, xp.Height, "Height");
					Assert.AreEqual (size, xp.Width, "Width");
					Assert.AreEqual (size, xp.Size.Width, "Size.Width");
					Assert.AreEqual (size, xp.Size.Height, "Size.Height");

					Bitmap bmp = xp.ToBitmap ();
					Assert.AreEqual (size, bmp.Height, "Bitmap.Height");
					Assert.AreEqual (size, bmp.Width, "Bitmap.Width");
					Assert.AreEqual (size, bmp.Size.Width, "Bitmap.Size.Width");
					Assert.AreEqual (size, bmp.Size.Height, "Bitmap.Size.Height");
				}
			}
		}

		[Test]
		public void Icon32bits_XP16 ()
		{
			XPIcon (16);
		}

		[Test]
		public void Icon32bits_XP32 ()
		{
			XPIcon (32);
		}

		[Test]
		public void Icon32bits_XP48 ()
		{
			XPIcon (48);
		}

		[Test]
		public void SelectFromUnusualSize_Small16 ()
		{
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/80509.ico"))) {
				using (Icon xp = new Icon (fs, 16, 16)) {
					Assert.AreEqual (16, xp.Height, "Height");
					Assert.AreEqual (10, xp.Width, "Width");
					Assert.AreEqual (10, xp.Size.Width, "Size.Width");
					Assert.AreEqual (16, xp.Size.Height, "Size.Height");
				}
			}
		}

		[Test]
		public void SelectFromUnusualSize_Normal32 ()
		{
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/80509.ico"))) {
				using (Icon xp = new Icon (fs, 32, 32)) {
					Assert.AreEqual (22, xp.Height, "Height");
					Assert.AreEqual (11, xp.Width, "Width");
					Assert.AreEqual (11, xp.Size.Width, "Size.Width");
					Assert.AreEqual (22, xp.Size.Height, "Size.Height");
				}
			}
		}

		internal static void SaveAndCompare (string msg, Icon icon, bool alpha)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				icon.Save (ms);
				ms.Position = 0;

				using (Icon loaded = new Icon (ms)) {
					Assert.AreEqual (icon.Height, loaded.Height, msg + ".Loaded.Height");
					Assert.AreEqual (icon.Width, loaded.Width, msg + ".Loaded.Width");

					using (Bitmap expected = icon.ToBitmap ()) {
						using (Bitmap actual = loaded.ToBitmap ()) {
							Assert.AreEqual (expected.Height, actual.Height, msg + ".Bitmap.Height");
							Assert.AreEqual (expected.Width, actual.Width, msg + ".Bitmap.Width");

							for (int y = 0; y < expected.Height; y++) {
								for (int x = 0; x < expected.Width; x++) {
									Color e = expected.GetPixel (x, y);
									Color a = actual.GetPixel (x, y);
									if (alpha)
										Assert.AreEqual (e.A, a.A, String.Format ("{0}:{1}x{2}:A", msg, x, y));
									Assert.AreEqual (e.R, a.R, String.Format ("{0}:{1}x{2}:R", msg, x, y));
									Assert.AreEqual (e.G, a.G, String.Format ("{0}:{1}x{2}:G", msg, x, y));
									Assert.AreEqual (e.B, a.B, String.Format ("{0}:{1}x{2}:B", msg, x, y));
								}
							}
						}
					}
				}
			}
		}

		[Test]
		public void Save ()
		{
			SaveAndCompare ("16", icon16, true);
			SaveAndCompare ("32", icon32, true);
			SaveAndCompare ("48", icon48, true);
			SaveAndCompare ("64", icon64, true);
			SaveAndCompare ("96", icon96, true);
		}

		[Test] // bug #410608
		public void Save_256 ()
		{
			string filepath = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/323511.ico");

			using (Icon icon = new Icon (filepath)) {
				// bug #415809 fixed
				SaveAndCompare ("256", icon, true);
			}

			// binary comparison
			var orig = new MemoryStream (File.ReadAllBytes (filepath));
			var saved = new MemoryStream ();
			using (Icon icon = new Icon (filepath))
				icon.Save (saved);
			FileAssert.AreEqual (orig, saved, "binary comparison");
		}

		[Test]
		public void Save_Null ()
		{
			Assert.Throws<NullReferenceException> (() => icon.Save (null));
		}

		[Test]
		public void Icon16ToBitmap ()
		{
			using (Bitmap b = icon16.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon16.Height, b.Height, "Height");
				Assert.AreEqual (icon16.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon32ToBitmap ()
		{
			using (Bitmap b = icon32.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon32.Height, b.Height, "Height");
				Assert.AreEqual (icon32.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon48ToBitmap ()
		{
			using (Bitmap b = icon48.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon48.Height, b.Height, "Height");
				Assert.AreEqual (icon48.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon64ToBitmap ()
		{
			using (Bitmap b = icon64.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon64.Height, b.Height, "Height");
				Assert.AreEqual (icon64.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon96ToBitmap ()
		{
			using (Bitmap b = icon96.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon96.Height, b.Height, "Height");
				Assert.AreEqual (icon96.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test] // bug #415581
		public void Icon256ToBitmap ()
		{
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/415581.ico"))) {
				Icon icon = new Icon (fs, 48, 48);
				using (Bitmap b = icon.ToBitmap ()) {
					Assert.AreEqual (0, b.Palette.Entries.Length, "#A1");
					Assert.AreEqual (48, b.Height, "#A2");
					Assert.AreEqual (48, b.Width, "#A3");
					Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "#A4");
					Assert.AreEqual (2, b.Flags, "#A5");
				}
				icon.Dispose ();
			}

			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/415581.ico"))) {
				Icon icon = new Icon (fs, 256, 256);
				using (Bitmap b = icon.ToBitmap ()) {
					Assert.AreEqual (0, b.Palette.Entries.Length, "#B1");
					Assert.AreEqual (48, b.Height, "#B2");
					Assert.AreEqual (48, b.Width, "#B3");
					Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "#B4");
					Assert.AreEqual (2, b.Flags, "#B5");
				}
			}
		}

		[Test]
		public void Icon256ToBitmap_Request0 ()
		{
			// 415581.ico has 2 images, the 256 and 48
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/415581.ico"))) {
				Icon icon = new Icon (fs, 0, 0);
				using (Bitmap b = icon.ToBitmap ()) {
					Assert.AreEqual (0, b.Palette.Entries.Length, "#B1");
					Assert.AreEqual (48, b.Height, "#B2");
					Assert.AreEqual (48, b.Width, "#B3");
					Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "#B4");
					Assert.AreEqual (2, b.Flags, "#B5");
				}
			}
		}

		[Test]
		public void Only256InFile ()
		{
			using (FileStream fs = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/only256.ico"))) {
				Assert.Throws<Win32Exception> (() => new Icon (fs, 0, 0));
			}
		}


		[Test]
		public void ExtractAssociatedIcon_Null ()
		{
			Assert.Throws<ArgumentException> (() => Icon.ExtractAssociatedIcon (null));
		}

		[Test]
		public void ExtractAssociatedIcon_Empty ()
		{
			Assert.Throws<ArgumentException> (() => Icon.ExtractAssociatedIcon (String.Empty));
		}

		[Test]
		public void ExtractAssociatedIcon_DoesNotExists ()
		{
			Assert.Throws<FileNotFoundException> (() => Icon.ExtractAssociatedIcon ("does-not-exists.png"));
		}

		private static bool RunningOnUnix {
			get {
				int p = (int) Environment.OSVersion.Platform;

				return (p == 4) || (p == 6) || (p == 128);
			}
		}
	}

	[TestFixture]
	public class IconFullTrustTest {
		[Test]
		public void ExtractAssociatedIcon ()
		{
			string filename_dll = Assembly.GetExecutingAssembly ().Location;
			Assert.IsNotNull (Icon.ExtractAssociatedIcon (filename_dll), "dll");
		}

		[Test]
		public void HandleRoundtrip ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.Handle;
				using (Icon icon2 = Icon.FromHandle (handle)) {
					Assert.AreEqual (16, icon2.Height, "FromHandle.Height");
					Assert.AreEqual (16, icon2.Width, "FromHandle.Width");
					Assert.AreEqual (handle, icon2.Handle, "FromHandle.Handle");
					IconTest.SaveAndCompare ("Handle", icon2, false);
				}
			}
			// unlike other cases (HICON, HBITMAP) handle DOESN'T survives original icon disposal
			// commented / using freed memory is risky ;-)
			/*using (Icon icon3 = Icon.FromHandle (handle)) {
				Assert.AreEqual (0, icon3.Height, "Survivor.Height");
				Assert.AreEqual (0, icon3.Width, "Survivor.Width");
				Assert.AreEqual (handle, icon3.Handle, "Survivor.Handle");
			}*/
		}

		[Test]
		public void CreateMultipleIconFromSameHandle ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.Handle;
				using (Icon icon2 = Icon.FromHandle (handle)) {
					Assert.AreEqual (16, icon2.Height, "2.Height");
					Assert.AreEqual (16, icon2.Width, "2.Width");
					Assert.AreEqual (handle, icon2.Handle, "2.Handle");
					IconTest.SaveAndCompare ("Handle2", icon2, false);
				}
				using (Icon icon3 = Icon.FromHandle (handle)) {
					Assert.AreEqual (16, icon3.Height, "3.Height");
					Assert.AreEqual (16, icon3.Width, "3.Width");
					Assert.AreEqual (handle, icon3.Handle, "3.Handle");
					IconTest.SaveAndCompare ("Handle3", icon3, false);
				}
			}
			// unlike other cases (HICON, HBITMAP) handle DOESN'T survives original icon disposal
			// commented / using freed memory is risky ;-)
		}

		[Test]
		public void HiconRoundtrip ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.ToBitmap ().GetHicon ();
			}
			// HICON survives
			using (Icon icon2 = Icon.FromHandle (handle)) {
				Assert.AreEqual (16, icon2.Height, "Survivor.Height");
				Assert.AreEqual (16, icon2.Width, "Survivor.Width");
				Assert.AreEqual (handle, icon2.Handle, "Survivor.Handle");
				IconTest.SaveAndCompare ("HICON", icon2, false);
			}
		}

		[Test]
		public void CreateMultipleIconFromSameHICON ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.ToBitmap ().GetHicon ();
			}
			// HICON survives
			using (Icon icon2 = Icon.FromHandle (handle)) {
				Assert.AreEqual (16, icon2.Height, "2.Height");
				Assert.AreEqual (16, icon2.Width, "2.Width");
				Assert.AreEqual (handle, icon2.Handle, "2.Handle");
				IconTest.SaveAndCompare ("HICON2", icon2, false);
			}
			using (Icon icon3 = Icon.FromHandle (handle)) {
				Assert.AreEqual (16, icon3.Height, "3.Height");
				Assert.AreEqual (16, icon3.Width, "3.Width");
				Assert.AreEqual (handle, icon3.Handle, "3.Handle");
				IconTest.SaveAndCompare ("HICON", icon3, false);
			}
		}
	}
}
