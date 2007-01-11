//
// Icon class testing unit
//
// Authors:
// 	Sanjay Gupta <gsanjay@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]	
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class IconTest {
		
		Icon icon;
		Icon newIcon;
		Icon icon16, icon32, icon48, icon64, icon96;
		FileStream fs;
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
			String path = TestBitmap.getInFile ("bitmaps/smiley.ico");
			icon = new Icon (path);			
			fs1 = new FileStream (path, FileMode.Open);

			icon16 = new Icon (TestBitmap.getInFile ("bitmaps/16x16x16.ico"));
			icon32 = new Icon (TestBitmap.getInFile ("bitmaps/32x32x16.ico"));
			icon48 = new Icon (TestBitmap.getInFile ("bitmaps/48x48x1.ico"));
			icon64 = new Icon (TestBitmap.getInFile ("bitmaps/64x64x256.ico"));
			icon96 = new Icon (TestBitmap.getInFile ("bitmaps/96x96x256.ico"));
		}

		[TearDown]
		public void TearDown ()
		{
			if (fs != null)
				fs.Close ();
			if (fs1 != null)
				fs1.Close ();
			if (File.Exists ("newIcon.ico"))
				File.Delete ("newIcon.ico");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TestConstructors ()
		{
			Assert.AreEqual (32, icon.Height, "C#0a");
			Assert.AreEqual (32, icon.Width, "C#0b");

			newIcon = new Icon (fs1, 48, 48);
			Assert.AreEqual (48, newIcon.Height, "C#1a"); 			
			Assert.AreEqual (48, newIcon.Width, "C#1b");

			newIcon = new Icon (icon, 16, 16);
			Assert.AreEqual (16, newIcon.Height, "C#2a"); 			
			Assert.AreEqual (16, newIcon.Width, "C#2b");
		}				

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TestProperties ()
		{
			Assert.AreEqual (32, icon.Height, "P#1");
			Assert.AreEqual (32, icon.Width, "P#2");
			Assert.AreEqual (32, icon.Size.Width, "P#3");
			Assert.AreEqual (32, icon.Size.Height, "P#4");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TestMethods ()
		{
			/*
			
			TODO: This does not work on Win32
			
			newIcon = (Icon) icon.Clone ();
			Assert.AreEqual (32, newIcon.Height, "M#1a");
			Assert.AreEqual (32, newIcon.Width, "M#1b");
			
			Bitmap bmp = icon.ToBitmap();
			Assert.AreEqual (32, bmp.Height, "M#2a");
			Assert.AreEqual (32, bmp.Width, "M#2b");
			*/
			
			fs = new FileStream ("newIcon.ico", FileMode.Create);
			icon.Save (fs);
			
			Assert.AreEqual (fs1.Length, fs.Length, "M#3");			
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

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExtractAssociatedIcon_Null ()
		{
			Icon.ExtractAssociatedIcon (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExtractAssociatedIcon_Empty ()
		{
			Icon.ExtractAssociatedIcon (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ExtractAssociatedIcon_DoesNotExists ()
		{
			Icon.ExtractAssociatedIcon ("does-not-exists.png");
		}
#endif
	}

	[TestFixture]	
	public class IconFullTrustTest {
#if NET_2_0
		[Test]
		public void ExtractAssociatedIcon ()
		{
			string filename_dll = Assembly.GetExecutingAssembly ().Location;
			Assert.IsNotNull (Icon.ExtractAssociatedIcon (filename_dll), "dll");
		}
#endif
	}
}
