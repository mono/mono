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
using System.Drawing.Drawing2D;
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

		// FontFamily

		[DllImport ("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipCreateFontFamilyFromName (
			[MarshalAs (UnmanagedType.LPWStr)] string fName, IntPtr collection, out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetGenericFontFamilySerif (out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteFontFamily (IntPtr fontfamily);

		[Test]
		public void DeleteFontFamily ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteFontFamily (IntPtr.Zero), "null");

			IntPtr font_family;
			GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out font_family);
			Assert.IsTrue (font_family != IntPtr.Zero, "GdipCreateFontFamilyFromName");
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DeleteFontFamily_DoubleDispose ()
		{
			IntPtr font_family;
			GdipGetGenericFontFamilySerif (out font_family);
			// first dispose
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "first");
			// second dispose
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "second");
		}

		// Bitmap

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromScan0 (int width, int height, int stride, PixelFormat format, IntPtr scan0, out IntPtr bmp);

		[Test]
		public void CreateBitmapFromScan0 ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.InvalidParameter, GdipCreateBitmapFromScan0 (-1, 10, 10, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp), "negative width");
		}

		// Brush

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipDeleteBrush (IntPtr brush);

		[Test]
		public void DeleteBrush ()
		{
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteBrush (IntPtr.Zero), "GdipDeleteBrush");
		}

		// GraphicsPath

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreatePath (FillMode brushMode, out IntPtr path);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPointCount (IntPtr path, out int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPathPoints (IntPtr path, [Out] PointF[] points, int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPathTypes (IntPtr path, [Out] byte[] types, int count);

		[Test]
		public void GetPointCount_Zero ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			Assert.IsTrue (path != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (0, count, "Count");

			PointF[] points = new PointF[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathPoints (path, points, count), "GdipGetPathPoints");
			// can't get the points if the count is zero!

			byte[] types = new byte[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathTypes (path, types, count), "GdipGetPathTypes");
			// can't get the types if the count is zero!
		}

		// Image

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

		// PathGradientBrush

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipCreatePathGradient (PointF[] points, int count, WrapMode wrapMode, out IntPtr brush);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetPathGradientBlendCount (IntPtr brush, out int count);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetPathGradientPresetBlend (IntPtr brush, int[] blend, float[] positions, int count);


		[Test]
		public void CreatePathGradient ()
		{
			PointF[] points = null;
			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "null");

			points = new PointF [0];
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "empty");

			points = new PointF[1];
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 1, WrapMode.Clamp, out brush), "one");

			points = new PointF[2] { new PointF (1, 2), new PointF (20, 30) };
			Assert.AreEqual (Status.Ok, GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "two");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");
			// can't call that for 1 count!

			Assert.AreEqual (Status.Ok, GdipDeleteBrush (brush), "GdipDeleteBrush");
		}


		// Region

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipCreateRegionRgnData (byte[] data, int size, out IntPtr region);

		[Test]
		public void CreateRegionRgnData ()
		{
			IntPtr region;
			Assert.AreEqual (Status.InvalidParameter, GdipCreateRegionRgnData (null, 0, out region));

			byte[] data = new byte[0];
			Assert.AreEqual (Status.GenericError, GdipCreateRegionRgnData (data, 0, out region));
		}
	}
}
