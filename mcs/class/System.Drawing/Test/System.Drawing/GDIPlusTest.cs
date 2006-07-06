//
// Direct GDI+ API unit tests
//
// Authors:
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
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	// copied from Mono's System.Drawing.dll gdiEnums.cs
	internal enum Status {
		Ok = 0,
		GenericError = 1,
		InvalidParameter = 2,
		OutOfMemory = 3,
		ObjectBusy = 4,
		InsufficientBuffer = 5,
		NotImplemented = 6,
		Win32Error = 7,
		WrongState = 8,
		Aborted = 9,
		FileNotFound = 10,
		ValueOverflow = 11,
		AccessDenied = 12,
		UnknownImageFormat = 13,
		FontFamilyNotFound = 14,
		FontStyleNotFound = 15,
		NotTrueTypeFont = 16,
		UnsupportedGdiplusVersion = 17,
		GdiplusNotInitialized = 18,
		PropertyNotFound = 19,
		PropertyNotSupported = 20,
		ProfileNotFound = 21
	}

	[TestFixture]
	public class GDIPlusTest {

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromScan0 (int width, int height, int stride, PixelFormat format, IntPtr scan0, out IntPtr bmp);

		[Test]
		public void CreateBitmapFromScan0 ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.InvalidParameter, GdipCreateBitmapFromScan0 (-1, 10, 10, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp), "negative width");
		}


		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDisposeImage (IntPtr image);

		[Test]
		public void DisposeImage ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipDisposeImage (IntPtr.Zero), "null");

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DisposeImage_Dual ()
		{
			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			// first dispose
			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "first");
			// second dispose
			Assert.AreEqual (Status.ObjectBusy, GdipDisposeImage (image), "second");
		}


		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetImageThumbnail (IntPtr image, uint width, uint height, out IntPtr thumbImage, IntPtr callback, IntPtr callBackData);

		[Test]
		[Category ("NotWorking")] // libgdiplus doesn't implement GdipGetImageThumbnail (it is done inside S.D)
		public void GetImageThumbnail ()
		{
			IntPtr ptr;

			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipGetImageThumbnail (IntPtr.Zero, 10, 10, out ptr, IntPtr.Zero, IntPtr.Zero));

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			try {
				// invalid width (0)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 0, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid width (negative)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 0x8000000, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (0)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 10, 0, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (negative)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 10, 0x8000000, out ptr, IntPtr.Zero, IntPtr.Zero));
			}
			finally {
				GdipDisposeImage (image);
			}
		}
	}
}
