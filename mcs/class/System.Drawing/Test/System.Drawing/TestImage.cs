//
// Image class testing unit
//
// Authors:
// 	Jordi Mas i Hernàndez (jmas@softcatala.org>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2005 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
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
using System.Xml.Serialization;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Drawing{

	[TestFixture]
	public class ImageTest {

		private string fname;
		private bool callback;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			fname = Path.GetTempFileName ();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			try {
				File.Delete (fname);
			}
			catch {
			}
		}

		[SetUp]
		public void SetUp ()
		{
			callback = false;
		}

		[Test]
		public void FileDoesNotExists ()
		{
			Assert.Throws<FileNotFoundException> (() => Image.FromFile ("FileDoesNotExists.jpg"));
		}

		private bool CallbackTrue ()
		{
			callback = true;
			return true;
		}

		private bool CallbackFalse ()
		{
			callback = true;
			return false;
		}

		[Test]
		public void GetThumbnailImage_NullCallback_Tiff ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				// according to documentation a callback is mandatory
				Image tn = bmp.GetThumbnailImage (10, 5, null, IntPtr.Zero);
				Assert.AreEqual (5, tn.Height, "Height");
				Assert.AreEqual (10, tn.Width, "Width");
				Assert.IsFalse (callback, "Callback called");
				tn.Save (fname, ImageFormat.Tiff);
			}
		}

		[Test]
		public void GetThumbnailImage_Height_Zero ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Assert.Throws<OutOfMemoryException> (() => bmp.GetThumbnailImage (5, 0, new Image.GetThumbnailImageAbort (CallbackFalse), IntPtr.Zero));
			}
		}

		[Test]
		public void GetThumbnailImage_Width_Negative ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Assert.Throws<OutOfMemoryException> (() => bmp.GetThumbnailImage (-5, 5, new Image.GetThumbnailImageAbort (CallbackFalse), IntPtr.Zero));
			}
		}

		[Test]
		public void GetThumbnailImage_CallbackData_Invalid ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				// according to documentation IntPtr.Zero must be supplied as data
				Image tn = bmp.GetThumbnailImage (5, 5, new Image.GetThumbnailImageAbort (CallbackFalse), (IntPtr)Int32.MaxValue);
				Assert.AreEqual (5, tn.Height, "Height");
				Assert.AreEqual (5, tn.Width, "Width");
				Assert.IsFalse (callback, "Callback called");
				tn.Save (fname, ImageFormat.Tiff);
			}
		}

		[Test]
		public void GetThumbnailImage_SameSize_Bmp ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Image tn = bmp.GetThumbnailImage (10, 10, new Image.GetThumbnailImageAbort (CallbackFalse), IntPtr.Zero);
				Assert.AreEqual (10, tn.Height, "Height");
				Assert.AreEqual (10, tn.Width, "Width");
				Assert.IsFalse (callback, "Callback called");
				tn.Save (fname, ImageFormat.Bmp);
			}
		}

		[Test]
		public void GetThumbnailImage_Smaller_Gif ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Image tn = bmp.GetThumbnailImage (4, 4, new Image.GetThumbnailImageAbort (CallbackTrue), IntPtr.Zero);
				Assert.AreEqual (4, tn.Height, "Height");
				Assert.AreEqual (4, tn.Width, "Width");
				Assert.IsFalse (callback, "Callback called");
				tn.Save (fname, ImageFormat.Gif);
			}
		}

		[Test]
		public void GetThumbnailImage_Bigger_Png ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Image tn = bmp.GetThumbnailImage (40, 40, new Image.GetThumbnailImageAbort (CallbackTrue), IntPtr.Zero);
				Assert.AreEqual (40, tn.Height, "Height");
				Assert.AreEqual (40, tn.Width, "Width");
				Assert.IsFalse (callback, "Callback called");
				tn.Save (fname, ImageFormat.Png);
			}
		}

		[Test]
		public void Stream_Unlocked ()
		{
			try {
				Image img = null;
				using (MemoryStream ms = new MemoryStream ()) {
					using (Bitmap bmp = new Bitmap (10, 10)) {
						bmp.Save (ms, ImageFormat.Png);
					}
					ms.Position = 0;
					img = Image.FromStream (ms);
				}
				// stream isn't available anymore
				((Bitmap) img).MakeTransparent (Color.Transparent);
			}
			catch (OutOfMemoryException) {
				int p = (int) Environment.OSVersion.Platform;
				// libgdiplus (UNIX) doesn't lazy load the image so the
				// stream may be freed (and this exception will never occur)
				if ((p == 4) || (p == 128) || (p == 6))
					throw;
			}
		}

		[Test]
		public void Stream_Locked ()
		{
			Image img = null;
			using (MemoryStream ms = new MemoryStream ()) {
				using (Bitmap bmp = new Bitmap (10, 10)) {
					bmp.Save (ms, ImageFormat.Png);
				}
				ms.Position = 0;
				img = Image.FromStream (ms);
				// stream is available
				((Bitmap) img).MakeTransparent (Color.Transparent);
			}
		}

		[Test]
		[Category ("NotWorking")]	// http://bugzilla.ximian.com/show_bug.cgi?id=80558
		public void XmlSerialize ()
		{
			new XmlSerializer (typeof (Image));
		}

		private void Wmf (Image img)
		{
			Assert.IsFalse (img is Bitmap, "Bitmap");
			Assert.IsTrue (img is Metafile, "Metafile");
			// as Image
			Assert.AreEqual (327683, img.Flags, "Flags");
			Assert.IsTrue (img.RawFormat.Equals (ImageFormat.Wmf), "Wmf");
			Assert.IsNull (img.Tag, "Tag");
		}

		[Test]
		public void FromFile_Metafile_Wmf ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			using (Image img = Image.FromFile (filename)) {
				Wmf (img);
			}
		}

		[Test]
		public void FromStream_Metafile_Wmf ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			using (FileStream fs = File.OpenRead (filename)) {
				using (Image img = Image.FromStream (fs)) {
					Wmf (img);
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // https://bugzilla.novell.com/show_bug.cgi?id=338779
		public void FromStream_Metafile_Wmf_NotOrigin ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			using (FileStream fs = File.OpenRead (filename)) {
				fs.Position = fs.Length / 2;
				Assert.Throws<ArgumentException> (() => Image.FromStream (fs));
			}
		}

		private void Emf (Image img)
		{
			Assert.IsFalse (img is Bitmap, "Bitmap");
			Assert.IsTrue (img is Metafile, "Metafile");
			// as Image
			Assert.AreEqual (327683, img.Flags, "Flags");
			Assert.IsTrue (img.RawFormat.Equals (ImageFormat.Emf), "Emf");
			Assert.IsNull (img.Tag, "Tag");
		}

		[Test]
		public void FromFile_Metafile_Emf ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/milkmateya01.emf");
			using (Image img = Image.FromFile (filename)) {
				Emf (img);
			}
		}

		[Test]
		public void FromStream_Metafile_Emf ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/milkmateya01.emf");
			using (FileStream fs = File.OpenRead (filename)) {
				using (Image img = Image.FromStream (fs)) {
					Emf (img);
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // https://bugzilla.novell.com/show_bug.cgi?id=338779
		public void FromStream_Metafile_Emf_NotOrigin ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/milkmateya01.emf");
			using (FileStream fs = File.OpenRead (filename)) {
				fs.Position = fs.Length / 2;
				Assert.Throws<ArgumentException> (() => Image.FromStream (fs));
			}
		}

		[Test]
		public void FromFile_Invalid ()
		{
			string filename = Assembly.GetExecutingAssembly ().Location;
			Assert.Throws<OutOfMemoryException> (() => Image.FromFile (filename));
		}

		[Test]
		public void FromStream_Invalid ()
		{
			string filename = Assembly.GetExecutingAssembly ().Location;
			using (FileStream fs = File.OpenRead (filename)) {
				Assert.Throws<ArgumentException> (() => Image.FromStream (fs));
			}
		}

		private Bitmap GetBitmap ()
		{
			Bitmap bmp = new Bitmap (20, 10, PixelFormat.Format24bppRgb);
			using (Graphics g = Graphics.FromImage (bmp))
			{
				Pen pen = new Pen (Color.Black, 3);
				g.DrawRectangle (pen, 0, 0, 5, 10);
			}
			return bmp;
		}

		[Test]
		public void StreamSaveLoad ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				using (Bitmap bmp = GetBitmap ()) {
					Assert.AreEqual (0, ms.Position, "Position-1");
					bmp.Save (ms, ImageFormat.Bmp);
					Assert.IsTrue (ms.Position > 0, "Position-2");

					ms.Position = ms.Length;
					Assert.AreEqual (ms.Length, ms.Position, "Position-3");

					Bitmap bmp2 = (Bitmap)Image.FromStream (ms);
					Assert.IsTrue (ms.Position > 20, "Position-4");

					Assert.IsTrue (bmp2.RawFormat.Equals (ImageFormat.Bmp), "Bmp");

					Assert.AreEqual (bmp.GetPixel (0, 0), bmp2.GetPixel (0, 0), "0,0");
					Assert.AreEqual (bmp.GetPixel (10, 0), bmp2.GetPixel (10, 0), "10,0");
				}
			}
		}

		[Test]
		public void StreamJunkSaveLoad ()
		{
			using (MemoryStream ms = new MemoryStream ()) {
				// junk
				ms.WriteByte (0xff);
				ms.WriteByte (0xef);
				Assert.AreEqual (2, ms.Position, "Position-1");

				using (Bitmap bmp = GetBitmap ()) {
					bmp.Save (ms, ImageFormat.Bmp);
					Assert.IsTrue (ms.Position > 2, "Position-2");
					// exception here
					Assert.Throws<ArgumentException> (() => Image.FromStream (ms));
				}
			}
		}

		[Test]
		public void XmlSerialization ()
		{
			new XmlSerializer (typeof (Image));
		}
	}
}
