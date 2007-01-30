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
using System.Security.Permissions;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
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
		[ExpectedException (typeof (FileNotFoundException))]
		public void FileDoesNotExists ()
		{
			Image.FromFile ("FileDoesNotExists.jpg");
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
		[ExpectedException (typeof (OutOfMemoryException))]
		public void GetThumbnailImage_Height_Zero ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Image tn = bmp.GetThumbnailImage (5, 0, new Image.GetThumbnailImageAbort (CallbackFalse), IntPtr.Zero);
			}
		}

		[Test]
		[ExpectedException (typeof (OutOfMemoryException))]
		public void GetThumbnailImage_Width_Negative ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				Image tn = bmp.GetThumbnailImage (-5, 5, new Image.GetThumbnailImageAbort (CallbackFalse), IntPtr.Zero);
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
#if !NET_2_0
		[Category ("NotDotNet")] // MS 1.x throws an ArgumentNullException in this case
#endif
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
				if ((p == 4) || (p == 128))
					throw;
			}
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS 1.x throws an ArgumentNullException in this case
#endif
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
#if NET_2_0
		[Category ("NotWorking")]	// http://bugzilla.ximian.com/show_bug.cgi?id=80558
#else
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void XmlSerialize ()
		{
			new XmlSerializer (typeof (Image));
		}
	}
}
