//
// Direct GDI+ API unit tests
//
// Authors:
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class GDIPlusTest {

		static readonly HandleRef HandleRefZero = new HandleRef (null, IntPtr.Zero);

		// for the moment this LOGFONT is different (and ok) from the one defined internally in SD
		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class LOGFONT {
			public int lfHeight = 0;
			public int lfWidth = 0;
			public int lfEscapement = 0;
			public int lfOrientation = 0;
			public int lfWeight = 0;
			public byte lfItalic = 0;
			public byte lfUnderline = 0;
			public byte lfStrikeOut = 0;
			public byte lfCharSet = 0;
			public byte lfOutPrecision = 0;
			public byte lfClipPrecision = 0;
			public byte lfQuality = 0;
			public byte lfPitchAndFamily = 0;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceName = null;
		}

		// CustomLineCap

		[Test]
		public void CreateCustomLineCap ()
		{
			IntPtr cap;

			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			// test invalid conditions for #81829
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreateCustomLineCap (HandleRefZero, new HandleRef (this, path), LineCap.Flat, 1.0f, out cap), "GdipCreateCustomLineCap-FillPath-Null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteCustomLineCap (new HandleRef (this, cap)), "GdipDeleteCustomLineCap-1");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreateCustomLineCap (new HandleRef (this, path), HandleRefZero, LineCap.Flat, 1.0f, out cap), "GdipCreateCustomLineCap-StrokePath-Null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteCustomLineCap (new HandleRef (this, cap)), "GdipDeleteCustomLineCap-2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		// FontFamily
		[Test]
		public void DeleteFontFamily ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteFontFamily (IntPtr.Zero), "null");

			IntPtr font_family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out font_family);
			if (font_family != IntPtr.Zero)
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "first");
			else
				Assert.Ignore ("Arial isn't available on this platform");
		}

		[Test]
		[Category ("NotWorking")]
		public void DeleteFontFamily_DoubleDispose ()
		{
			IntPtr font_family;
			GDIPlus.GdipGetGenericFontFamilySerif (out font_family);
			// first dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "first");
			// second dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "second");
		}

		[Test]
		public void CloneFontFamily ()
		{
			IntPtr font_family = IntPtr.Zero;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipCloneFontFamily (HandleRefZero, out font_family), "GdipCloneFontFamily(null)");

			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out font_family);
			if (font_family != IntPtr.Zero) {
				var font_family_handle = new HandleRef (this, font_family);

				IntPtr clone;
				Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCloneFontFamily (font_family_handle, out clone), "GdipCloneFontFamily(arial)");
				Assert.IsTrue (clone != IntPtr.Zero, "clone");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "GdipDeleteFontFamily(arial)");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (clone), "GdipDeleteFontFamily(clone)");
			} else
				Assert.Ignore ("Arial isn't available on this platform");
		}

		// Font
		[Test]
		public void CreateFont ()
		{
			IntPtr family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out family);
			if (family == IntPtr.Zero)
				Assert.Ignore ("Arial isn't available on this platform");

			IntPtr font;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateFont (family, 10f, FontStyle.Regular, GraphicsUnit.Point, out font), "GdipCreateFont");
			Assert.IsTrue (font != IntPtr.Zero, "font");

			LOGFONT lf = new LOGFONT ();
			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetLogFont (font, IntPtr.Zero, (object) lf), "GdipGetLogFont-null-graphics");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null-graphics");
			// other lf members looks like garbage

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			GDIPlus.GdipGetImageGraphicsContext (image, out graphics);
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetLogFont (IntPtr.Zero, graphics, (object) lf), "GdipGetLogFont-null");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetLogFont (font, graphics, (object) lf), "GdipGetLogFont");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet");
			// strangely this is 1 in the managed side

			lf.lfCharSet = 2;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetLogFont (font, graphics, (object) lf), "GdipGetLogFont-2");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet");
			// strangely this is 2 in the managed side

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFont (font), "GdipDeleteFont");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteFont (IntPtr.Zero), "GdipDeleteFont-null");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (family), "GdipDeleteFontFamily");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
		}

		// Bitmap
		[Test]
		public void CreateBitmapFromScan0 ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateBitmapFromScan0 (-1, 10, 10, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp), "negative width");
		}

		[Test]
		public void Format1bppIndexed_GetSetPixel ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format1bppIndexed, IntPtr.Zero, out bmp), "GdipCreateBitmapFromScan0");
			Assert.IsTrue (bmp != IntPtr.Zero, "bmp");
			try {
				int argb;
				Assert.AreEqual (Status.Ok, GDIPlus.GdipBitmapGetPixel (bmp, 0, 0, out argb), "GdipBitmapGetPixel");
				Assert.AreEqual (-16777216, argb, "argb");
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapSetPixel (bmp, 0, 0, argb), "GdipBitmapSetPixel");
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (bmp), "GdipDisposeImage");
			}
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus doesn't support this format
		public void Format16bppGrayScale_GetSetPixel ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format16bppGrayScale, IntPtr.Zero, out bmp), "GdipCreateBitmapFromScan0");
			Assert.IsTrue (bmp != IntPtr.Zero, "bmp");
			try {
				int argb = 0;
				// and MS GDI+ can get or set pixels on it
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapGetPixel (bmp, 0, 0, out argb), "GdipBitmapGetPixel");
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapSetPixel (bmp, 0, 0, argb), "GdipBitmapSetPixel");
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (bmp), "GdipDisposeImage");
			}
		}

		[Test]
		public void Unlock ()
		{
			IntPtr bmp;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp);
			Assert.IsTrue (bmp != IntPtr.Zero, "bmp");

			BitmapData bd = null;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "BitmapData");

			bd = new BitmapData ();
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapUnlockBits (IntPtr.Zero, bd), "handle");

			Assert.AreEqual (Status.Win32Error, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "not locked");

			Rectangle rect = new Rectangle (2, 2, 5, 5);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipBitmapLockBits (bmp, ref rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb, bd), "locked");

			Assert.AreEqual (rect.Width, bd.Width, "Width");
			Assert.AreEqual (rect.Height, bd.Height, "Height");
			Assert.AreEqual (PixelFormat.Format24bppRgb, bd.PixelFormat, "PixelFormat");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "unlocked");

			Assert.AreEqual (Status.Win32Error, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "unlocked-twice");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (bmp), "GdipDisposeImage");
		}

		// Brush
		[Test]
		public void DeleteBrush ()
		{
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipDeleteBrush (HandleRefZero), "GdipDeleteBrush");
		}

		// Graphics
		[Test]
		public void GdipGetImageGraphicsContext_Null ()
		{
			IntPtr graphics;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImageGraphicsContext (IntPtr.Zero, out graphics), "GdipGetImageGraphicsContext");
		}

		private void Graphics_DrawImage (IntPtr image, bool metafile)
		{
			IntPtr graphics;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			if (metafile) {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImage (graphics, image, Single.MinValue, Single.MaxValue), "FloatMinMax");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageI (graphics, image, Int32.MinValue, Int32.MaxValue), "IntMinMax");
			} else {
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImage (graphics, image, Single.MinValue, Single.MaxValue), "FloatOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImage (graphics, image, 1073741888, 0), "FloatXMax");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImage (graphics, image, 1073741889, 0), "FloatXMaxOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImage (graphics, image, 0, 1073741888), "FloatYMax");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImage (graphics, image, 0, 1073741889), "FloatYMaxOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImage (graphics, image, -1073741888, 0), "FloatXMin");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImage (graphics, image, -1073741889, 0), "FloatXMinOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImage (graphics, image, 0, -1073741888), "FloatYMin");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImage (graphics, image, 0, -1073741889), "FloatYMinOverflow");

				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImageI (graphics, image, Int32.MinValue, Int32.MaxValue), "IntOverflow");
				// the real limit of MS GDI+ is 1073741951 but differs (by a very few) from the float limit 
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageI (graphics, image, 1073741824, 0), "IntXMax");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImageI (graphics, image, 1073741952, 0), "IntXMaxOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageI (graphics, image, 0, 1073741824), "IntYMax");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImageI (graphics, image, 0, 1073741952), "IntYMaxOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageI (graphics, image, -1073741824, 0), "IntXMin");
				// the real limit of MS GDI+ is -1073741825 but int-to-float convertion in libgdiplus turns this into a -1073741824
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImageI (graphics, image, -1073741899, 0), "IntXMinOverflow");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageI (graphics, image, 0, -1073741824), "IntYMin");
				Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipDrawImageI (graphics, image, 0, -1073741899), "IntYMinOverflow");
			}

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Display, IntPtr.Zero, null, IntPtr.Zero), "Display");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Document, IntPtr.Zero, null, IntPtr.Zero), "Document");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Inch, IntPtr.Zero, null, IntPtr.Zero), "Inch");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Millimeter, IntPtr.Zero, null, IntPtr.Zero), "Millimeter");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Pixel, IntPtr.Zero, null, IntPtr.Zero), "Pixel");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.Point, IntPtr.Zero, null, IntPtr.Zero), "Point");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, GraphicsUnit.World, IntPtr.Zero, null, IntPtr.Zero), "World");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawImageRectRectI (graphics, image, 0, 0, 10, 10, 0, 0, 10, 10, (GraphicsUnit) Int32.MinValue, IntPtr.Zero, null, IntPtr.Zero), "invalid");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteGraphics (IntPtr.Zero), "GdipDeleteGraphics-null");
		}

		[Test]
		public void Graphics_FromImage_Bitmap ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			Graphics_DrawImage (image, false);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		[Test]
		[Category ("NotWorking")] // incomplete GdipDrawImageRectRect[I] support
		public void Graphics_FromImage_Metafile ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr metafile = IntPtr.Zero;
					IntPtr hdc = g.GetHdc ();
					try {
						RectangleF rect = new RectangleF (10, 20, 100, 200);
						Assert.AreEqual (Status.Ok, GDIPlus.GdipRecordMetafileFileName ("test-drawimage.emf", hdc, EmfType.EmfPlusOnly, ref rect, MetafileFrameUnit.GdiCompatible, null, out metafile), "GdipRecordMetafileFileName");
						Assert.IsTrue (metafile != IntPtr.Zero, "image");

						Graphics_DrawImage (metafile, true);
					}
					finally {
						Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (metafile), "GdipDisposeImage");
					}
				}
			}
		}

		[Test]
		public void GdipCreateFromHDC_Null ()
		{
			IntPtr graphics;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateFromHDC (IntPtr.Zero, out graphics), "GdipCreateFromHDC(null)");
		}

		[Test]
		public void DrawRectangles ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			Rectangle[] r = new Rectangle[1] { new Rectangle (1, 2, -2, -1) };
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectanglesI (graphics, IntPtr.Zero, r, 1), "GdipDrawRectanglesI-PenNull");

			RectangleF[] rf = new RectangleF[2] { new RectangleF (1, 2, -2, -1), new RectangleF (0, 0, 10, 10) };
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectangles (graphics, IntPtr.Zero, rf, 2), "GdipDrawRectanglesI-PenNull");

			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0, 0f, GraphicsUnit.World, out pen), "GdipCreatePen1");
			Assert.IsTrue (pen != IntPtr.Zero, "pen");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectanglesI (IntPtr.Zero, pen, r, 1), "GdipDrawRectanglesI-GraphicsNull");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectangles (IntPtr.Zero, pen, rf, 2), "GdipDrawRectangles-GraphicsNull");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectanglesI (graphics, pen, null, 1), "GdipDrawRectanglesI-RectanglesNull");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectangles (graphics, pen, null, 1), "GdipDrawRectangles-RectanglesNull");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectanglesI (graphics, pen, new Rectangle[0], 0), "GdipDrawRectanglesI-RectanglesEmpty");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectangles (graphics, pen, new RectangleF[0], 0), "GdipDrawRectangles-RectanglesEmpty");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectanglesI (graphics, pen, r, -1), "GdipDrawRectanglesI-RectanglesNegative");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDrawRectangles (graphics, pen, rf, -1), "GdipDrawRectangles-RectanglesNegative");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawRectanglesI (graphics, pen, r, 1), "GdipDrawRectanglesI-Rectangles1");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDrawRectangles (graphics, pen, rf, 2), "GdipDrawRectangles-Rectangles2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		[Test]
		public void MeasureCharacterRanges ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			IntPtr family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out family);
			if (family == IntPtr.Zero)
				Assert.Ignore ("Arial isn't available on this platform");

			IntPtr font;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateFont (family, 10f, FontStyle.Regular, GraphicsUnit.Point, out font), "GdipCreateFont");
			Assert.IsTrue (font != IntPtr.Zero, "font");

			RectangleF layout = new RectangleF ();
			IntPtr[] regions = new IntPtr[1];
			IntPtr format;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericDefault (out format), "GdipStringFormatGetGenericDefault");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMeasureCharacterRanges (IntPtr.Zero, "a", 1, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMeasureCharacterRanges (graphics, null, 0, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges-null string");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (format, out count), "GdipGetStringFormatMeasurableCharacterRangeCount");
			Assert.AreEqual (0, count, "count");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipMeasureCharacterRanges (graphics, "a", 1, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (format), "GdipDeleteStringFormat");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFont (font), "GdipDeleteFont");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (family), "GdipDeleteFontFamily");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		// GraphicsPath
		[Test]
		public void GetPointCount_Zero ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			Assert.IsTrue (path != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPointCount (IntPtr.Zero, out count), "GdipGetPointCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (0, count, "Count");

			PointF[] points = new PointF[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (IntPtr.Zero, points, count), "GdipGetPathPoints-null-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (path, null, count), "GdipGetPathPoints-null-2");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (path, points, count), "GdipGetPathPoints");
			// can't get the points if the count is zero!

			byte[] types = new byte[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (IntPtr.Zero, types, count), "GdipGetPathTypes-null-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (path, null, count), "GdipGetPathTypes-null-2");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (path, types, count), "GdipGetPathTypes");
			// can't get the types if the count is zero!

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (IntPtr.Zero, pts_2f, pts_2f.Length), "GdipAddPathLine2-null-path");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (path, null, pts_2f.Length), "GdipAddPathLine2-null-points");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (path, pts_2f, -1), "GdipAddPathLine2-negative-count");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			points = new PointF[count];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathPoints (path, points, count), "GdipGetPathPoints-ok");

			types = new byte[count];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathTypes (path, types, count), "GdipGetPathTypes-ok");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipResetPath (path), "GdipResetPath");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipResetPath (IntPtr.Zero), "GdipResetPath-null");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeletePath (IntPtr.Zero), "GdipDeletePath-null");
		}

		[Test]
		public void Widen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0, 0f, GraphicsUnit.World, out pen), "GdipCreatePen1");

			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipWidenPath (IntPtr.Zero, pen, matrix, 1.0f), "GdipWidenPath-null-path");
			// empty path
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath");

			// add something to the path
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine (IntPtr.Zero, 1, 1, 10, 10), "GdipAddPathLine");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath-2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");
		}

		// GraphicsPathIterator
		[Test]
		public void GraphicsPathIterator ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			var pathHandle = new HandleRef (this, path);

			IntPtr iter;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreatePathIter (out iter, pathHandle), "GdipCreatePathIter");
			var iterHandle = new HandleRef (this, iter);

			int count = -1;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterGetCount (HandleRefZero, out count), "GdipPathIterGetCount-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterGetCount (iterHandle, out count), "GdipPathIterGetCount");
			Assert.AreEqual (0, count, "count-1");

			count = -1;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterGetSubpathCount (HandleRefZero, out count), "GdipPathIterGetSubpathCount-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterGetSubpathCount (iterHandle, out count), "GdipPathIterGetSubpathCount");
			Assert.AreEqual (0, count, "count-2");

			bool curve;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterHasCurve (HandleRefZero, out curve), "GdipPathIterHasCurve-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterHasCurve (iterHandle, out curve), "GdipPathIterHasCurve");
			Assert.IsFalse (curve, "curve");

			int result;
			PointF[] points = new PointF[count];
			byte[] types = new byte[count];
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (HandleRefZero, out result, points, types, count), "GdipPathIterEnumerate-iter");
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (iterHandle, out result, null, types, count), "GdipPathIterEnumerate-points");
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (iterHandle, out result, points, null, count), "GdipPathIterEnumerate-types");
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iterHandle, out result, points, types, -1), "GdipPathIterEnumerate-count");
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iterHandle, out result, points, types, count), "GdipPathIterEnumerate");

//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (HandleRefZero, out result, points, types, 0, 0), "GdipPathIterCopyData-iter");
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (iterHandle, out result, null, types, 0, 0), "GdipPathIterCopyData-points");
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (iterHandle, out result, points, null, 0, 0), "GdipPathIterCopyData-types");
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iterHandle, out result, points, types, -1, 0), "GdipPathIterCopyData-start");
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iterHandle, out result, points, types, 0, -1), "GdipPathIterCopyData-end");
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iterHandle, out result, points, types, 0, 0), "GdipPathIterCopyData");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterNextMarkerPath (HandleRefZero, out result, pathHandle), "GdipPathIterNextMarkerPath-iter");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iterHandle, out result, HandleRefZero), "GdipPathIterNextMarkerPath-path");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iterHandle, out result, pathHandle), "GdipPathIterNextMarkerPath");

			result = -1;
			int start = -1;
			int end = -1;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterNextMarker (HandleRefZero, out result, out start, out end), "GdipPathIterNextMarker-iter");
			start = -1;
			end = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextMarker (iterHandle, out result, out start, out end), "GdipPathIterNextMarker");
			Assert.AreEqual (0, result, "result-4");
			Assert.AreEqual (-1, start, "start-1");
			Assert.AreEqual (-1, end, "end-1");

			byte pathType = 255;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterNextPathType (HandleRefZero, out result, out pathType, out start, out end), "GdipPathIterNextPathType-iter");
			pathType = 255;
			start = -1;
			end = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextPathType (iterHandle, out result, out pathType, out start, out end), "GdipPathIterNextPathType");
			Assert.AreEqual (0, result, "result-5");
			Assert.AreEqual (255, pathType, "pathType");
			Assert.AreEqual (-1, start, "start-2");
			Assert.AreEqual (-1, end, "end-2");

			bool closed = false;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterNextSubpathPath (HandleRefZero, out result, HandleRefZero, out closed), "GdipPathIterNextSubpathPath-iter");
			closed = false;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iterHandle, out result, HandleRefZero, out closed), "GdipPathIterNextSubpathPath-path");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iterHandle, out result, pathHandle, out closed), "GdipPathIterNextSubpathPath");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterNextSubpath (HandleRefZero, out result, out start, out end, out closed), "GdipPathIterNextSubpath-iter");
			start = -1;
			end = -1;
			closed = false;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextSubpath (iterHandle, out result, out start, out end, out closed), "GdipPathIterNextSubpath");
			Assert.AreEqual (-1, start, "start-3");
			Assert.AreEqual (-1, end, "end-3");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipPathIterRewind (HandleRefZero), "GdipPathIterRewind-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterRewind (iterHandle), "GdipPathIterRewind");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipDeletePathIter (HandleRefZero), "GdipDeletePathIter-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeletePathIter (iterHandle), "GdipDeletePathIter");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		[Test]
		public void GraphicsPathIterator_WithoutPath ()
		{
			// a path isn't required to create an iterator - ensure we never crash for any api
			IntPtr iter;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreatePathIter (out iter, HandleRefZero), "GdipCreatePathIter-null");
			var iterHandle = new HandleRef (this, iter);

			int count = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterGetCount (iterHandle, out count), "GdipPathIterGetCount");
			Assert.AreEqual (0, count, "count-1");

			count = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterGetSubpathCount (iterHandle, out count), "GdipPathIterGetSubpathCount");
			Assert.AreEqual (0, count, "count-2");

			bool curve;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterHasCurve (iterHandle, out curve), "GdipPathIterHasCurve");

			int result = -1;
//			PointF[] points = new PointF[count];
//			byte[] types = new byte[count];
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iterHandle, out result, points, types, count), "GdipPathIterEnumerate");
//			Assert.AreEqual (0, result, "result-1");

//			result = -1;
//			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iterHandle, out result, points, types, 0, 0), "GdipPathIterCopyData");
//			Assert.AreEqual (0, result, "result-2");

			result = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iterHandle, out result, HandleRefZero), "GdipPathIterNextMarkerPath");
			Assert.AreEqual (0, result, "result-3");

			result = -1;
			int start = -1;
			int end = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextMarker (iterHandle, out result, out start, out end), "GdipPathIterNextMarker");
			Assert.AreEqual (0, result, "result-4");
			Assert.AreEqual (-1, start, "start-1");
			Assert.AreEqual (-1, end, "end-1");

			result = -1;
			byte pathType = 255;
			start = -1;
			end = -1;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextPathType (iterHandle, out result, out pathType, out start, out end), "GdipPathIterNextPathType");
			Assert.AreEqual (0, result, "result-5");
			Assert.AreEqual (255, pathType, "pathType");
			Assert.AreEqual (-1, start, "start-2");
			Assert.AreEqual (-1, end, "end-2");

			bool closed = false;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iterHandle, out result, HandleRefZero, out closed), "GdipPathIterNextSubpathPath");

			start = -1;
			end = -1;
			closed = false;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterNextSubpath (iterHandle, out result, out start, out end, out closed), "GdipPathIterNextSubpath");
			Assert.AreEqual (-1, start, "start-3");
			Assert.AreEqual (-1, end, "end-3");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipPathIterRewind (iterHandle), "GdipPathIterRewind");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeletePathIter (iterHandle), "GdipDeletePathIter");
		}

		// Matrix
		[Test]
		public void Matrix ()
		{
			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			bool result;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixIdentity (IntPtr.Zero, out result), "GdipIsMatrixIdentity-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixIdentity (matrix, out result), "GdipIsMatrixIdentity");
			Assert.IsTrue (result, "identity");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixInvertible (IntPtr.Zero, out result), "GdipIsMatrixInvertible-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixInvertible (matrix, out result), "GdipIsMatrixInvertible");
			Assert.IsTrue (result, "invertible");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipInvertMatrix (IntPtr.Zero), "GdipInvertMatrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipInvertMatrix (matrix), "GdipInvertMatrix");

			PointF[] points = new PointF[1];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (IntPtr.Zero, points, 1), "GdipTransformMatrixPoints-null-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (matrix, null, 1), "GdipTransformMatrixPoints-matrix-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (matrix, points, 0), "GdipTransformMatrixPoints-matrix-points-0");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTransformMatrixPoints (matrix, points, 1), "GdipTransformMatrixPoints");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (IntPtr.Zero, points, 1), "GdipVectorTransformMatrixPoints-null-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (matrix, null, 1), "GdipVectorTransformMatrixPoints-matrix-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (matrix, points, 0), "GdipVectorTransformMatrixPoints-matrix-points-0");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipVectorTransformMatrixPoints (matrix, points, 1), "GdipVectorTransformMatrixPoints");

			IntPtr clone;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCloneMatrix (IntPtr.Zero, out clone), "GdipCloneMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneMatrix (matrix, out clone), "GdipCloneMatrix");
			Assert.IsTrue (clone != IntPtr.Zero, "Handle-clone");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixEqual (IntPtr.Zero, clone, out result), "GdipIsMatrixEqual-null-clone");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixEqual (matrix, IntPtr.Zero, out result), "GdipIsMatrixEqual-matrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixEqual (matrix, clone, out result), "GdipIsMatrixEqual-matrix-clone");
			Assert.IsTrue (result, "equal");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (IntPtr.Zero, clone, MatrixOrder.Append), "GdipMultiplyMatrix-null-clone-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (matrix, IntPtr.Zero, MatrixOrder.Prepend), "GdipMultiplyMatrix-matrix-null-prepend");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (matrix, clone, (MatrixOrder)Int32.MinValue), "GdipMultiplyMatrix-matrix-clone-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipMultiplyMatrix (matrix, clone, MatrixOrder.Append), "GdipMultiplyMatrix-matrix-clone-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTranslateMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipTranslateMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTranslateMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipTranslateMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTranslateMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipTranslateMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTranslateMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipTranslateMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipScaleMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipScaleMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipScaleMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipScaleMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipScaleMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipScaleMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipScaleMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipScaleMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipShearMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipShearMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipShearMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipShearMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipShearMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipShearMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipShearMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipShearMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteMatrix (IntPtr.Zero), "GdipDeleteMatrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (clone), "GdipDeleteMatrix-clone");
		}

		[Test]
		public void Matrix2 ()
		{
			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix2 (Single.MinValue, Single.MaxValue, Single.NegativeInfinity, 
				Single.PositiveInfinity, Single.NaN, Single.Epsilon, out matrix), "GdipCreateMatrix2");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			// check data
			float[] elements = new float[6];
			IntPtr data = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (float)) * 6);
			try {
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMatrixElements (IntPtr.Zero, data), "GdipSetMatrixElements-null-matrix");
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMatrixElements (matrix, IntPtr.Zero), "GdipSetMatrixElements-null-data");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetMatrixElements (matrix, data), "GdipSetMatrixElements-null-matrix");
				Marshal.Copy (data, elements, 0, 6);
				Assert.AreEqual (Single.MinValue, elements [0], "0");
				Assert.AreEqual (Single.MaxValue, elements [1], "1");
				Assert.AreEqual (Single.NegativeInfinity, elements [2], "2");
				Assert.AreEqual (Single.PositiveInfinity, elements [3], "3");
				Assert.AreEqual (Single.NaN, elements [4], "4");
				Assert.AreEqual (Single.Epsilon, elements [5], "5");
			}
			finally {
				Marshal.FreeHGlobal (data);
			}

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetMatrixElements (IntPtr.Zero, 0f, 0f, 0f, 0f, 0f, 0f), "GdipSetMatrixElements-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetMatrixElements (matrix, 0f, 0f, 0f, 0f, 0f, 0f), "GdipSetMatrixElements");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
		}

		[Test]
		public void Matrix3 ()
		{
			RectangleF rect = new RectangleF ();
			IntPtr matrix;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateMatrix3 (ref rect, null, out matrix), "GdipCreateMatrix3-null");

			// provding less than 3 points would results in AccessViolationException under MS 2.0 but can't happen using the managed SD code
			PointF[] points = new PointF[3];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-rect");
			rect = new RectangleF (10f, 20f, 0f, 40f);
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-width");
			rect = new RectangleF (10f, 20f, 30f, 0f);
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-height");

			rect = new RectangleF (0f, 0f, 30f, 40f);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-3");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
		}

		// Image
		[Test]
		public void DisposeImage ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDisposeImage (IntPtr.Zero), "null");

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DisposeImage_Dual ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			// first dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "first");
			// second dispose
			Assert.AreEqual (Status.ObjectBusy, GDIPlus.GdipDisposeImage (image), "second");
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus doesn't implement GdipGetImageThumbnail (it is done inside S.D)
		public void GetImageThumbnail ()
		{
			IntPtr ptr;

			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImageThumbnail (IntPtr.Zero, 10, 10, out ptr, IntPtr.Zero, IntPtr.Zero));

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			try {
				// invalid width (0)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 0, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid width (negative)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 0x8000000, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (0)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 10, 0, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (negative)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 10, 0x8000000, out ptr, IntPtr.Zero, IntPtr.Zero));
			}
			finally {
				GDIPlus.GdipDisposeImage (image);
			}
		}

		[Test]
		public void Icon ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/64x64x256.ico");
			IntPtr bitmap;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			try {
				int size;
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePaletteSize (bitmap, out size), "GdipGetImagePaletteSize");
				Assert.AreEqual (1032, size, "size");

				IntPtr clone;
				Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneImage (bitmap, out clone), "GdipCloneImage");
				try {
					Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePaletteSize (clone, out size), "GdipGetImagePaletteSize/Clone");
					Assert.AreEqual (1032, size, "size/clone");
				}
				finally {
					GDIPlus.GdipDisposeImage (clone);
				}

				IntPtr palette = Marshal.AllocHGlobal (size);
				try {

					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (IntPtr.Zero, palette, size), "GdipGetImagePalette(null,palette,size)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (bitmap, IntPtr.Zero, size), "GdipGetImagePalette(bitmap,null,size)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (bitmap, palette, 0), "GdipGetImagePalette(bitmap,palette,0)");
					Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePalette (bitmap, palette, size), "GdipGetImagePalette");

					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImagePalette (IntPtr.Zero, palette), "GdipSetImagePalette(null,palette)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImagePalette (bitmap, IntPtr.Zero), "GdipSetImagePalette(bitmap,null)");
					Assert.AreEqual (Status.Ok, GDIPlus.GdipSetImagePalette (bitmap, palette), "GdipSetImagePalette");
				}
				finally {
					Marshal.FreeHGlobal (palette);
				}
			}
			finally {
				GDIPlus.GdipDisposeImage (bitmap);
			}
		}

		[Test]
		public void FromFile_IndexedBitmap ()
		{
			// despite it's name it's a 4bpp indexed bitmap
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/almogaver1bit.bmp");
			IntPtr graphics;

			IntPtr image;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipLoadImageFromFile (filename, out image), "GdipLoadImageFromFile");
			try {
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext/image");
				Assert.AreEqual (IntPtr.Zero, graphics, "image/graphics");
			}
			finally {
				GDIPlus.GdipDisposeImage (image);
			}

			IntPtr bitmap;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			try {
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageGraphicsContext (bitmap, out graphics), "GdipGetImageGraphicsContext/bitmap");
				Assert.AreEqual (IntPtr.Zero, graphics, "bitmap/graphics");
			}
			finally {
				GDIPlus.GdipDisposeImage (bitmap);
			}
		}

		[Test]
		public void GdipLoadImageFromFile_FileNotFound ()
		{
			string filename = "filenotfound";

			IntPtr image;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipLoadImageFromFile (filename, out image), "GdipLoadImageFromFile");
			Assert.AreEqual (IntPtr.Zero, image, "image handle");

			// this doesn't throw a OutOfMemoryException
			Assert.Throws<FileNotFoundException> (() => Image.FromFile (filename));
		}

		[Test]
		public void GdipCreateBitmapFromFile_FileNotFound ()
		{
			string filename = "filenotfound";

			IntPtr bitmap;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			Assert.AreEqual (IntPtr.Zero, bitmap, "bitmap handle");

			Assert.Throws<ArgumentException> (() => new Bitmap (filename));
		}

		[Test]
		public void Encoder ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);

			Guid g = new Guid ();
			uint size = UInt32.MaxValue;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetEncoderParameterListSize (IntPtr.Zero, ref g, out size), "GdipGetEncoderParameterListSize-null-guid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-1");
			// note: can't test a null Guid (it's a struct)
#if false
			Assert.AreEqual (Status. FileNotFound, GDIPlus.GdipGetEncoderParameterListSize (image, ref g, out size), "GdipGetEncoderParameterListSize-image-badguid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-2");

			g = new Guid ("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetEncoderParameterListSize (image, ref g, out size), "GdipGetEncoderParameterListSize-image-guid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-3");
#endif
			GDIPlus.GdipDisposeImage (image);
		}

		// ImageAttribute
		[Test]
		public void ImageAttribute ()
		{
			IntPtr attr;
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreateImageAttributes (out attr), "GdipCreateImageAttributes");
			var attrHandle = new HandleRef (this, attr);

			IntPtr clone;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipCloneImageAttributes (HandleRefZero, out clone), "GdipCloneImageAttributes");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCloneImageAttributes (attrHandle, out clone), "GdipCloneImageAttributes");
			var cloneHandle = new HandleRef (this, clone);

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipSetImageAttributesColorMatrix (attrHandle, ColorAdjustType.Default, true, null, null, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-true-matrix1");
			// the first color matrix can be null if enableFlag is false
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipSetImageAttributesColorMatrix (attrHandle, ColorAdjustType.Default, false, null, null, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-false-matrix1");
			ColorMatrix cm = new ColorMatrix ();
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipSetImageAttributesColorMatrix (HandleRefZero, ColorAdjustType.Default, true, cm, null, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipSetImageAttributesColorMatrix (attrHandle, ColorAdjustType.Default, true, cm, null, ColorMatrixFlag.Default), "GdipCloneImageAttributes");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipDisposeImageAttributes (HandleRefZero), "GdipDisposeImageAttributes-null");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDisposeImageAttributes (attrHandle), "GdipDisposeImageAttributes");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDisposeImageAttributes (cloneHandle), "GdipDisposeImageAttributes-clone");
		}

		// PathGradientBrush
		[Test]
		public void CreatePathGradient ()
		{
			PointF[] points = null;
			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "null");

			points = new PointF [0];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "empty");

			points = new PointF[1];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 1, WrapMode.Clamp, out brush), "one");

			points = null;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "null/two");

			points = new PointF[2] { new PointF (1, 2), new PointF (20, 30) };
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "two");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");
			var brushHandle = new HandleRef (this, brush);

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");
			// can't call that for 1 count!

			Assert.AreEqual (0, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
		}

		[Test]
		public void CreatePathGradient_FromPath_Line ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradientFromPath (IntPtr.Zero, out brush), "null");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "empty path");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");
			var brushHandle = new HandleRef (this, brush);			

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (0, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		[Test]
		public void CreatePathGradient_FromPath_Lines ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			IntPtr brush;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");
			var brushHandle = new HandleRef (this, brush);

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (0, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		// Pen
		[Test]
		public void CreatePen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0, 0f, GraphicsUnit.World, out pen), "GdipCreatePen1");
			Assert.IsTrue (pen != IntPtr.Zero, "pen");

			DashStyle ds;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenDashStyle (pen, out ds), "GdipGetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashStyle (IntPtr.Zero, out ds), "GdipGetPenDashStyle-null");

			ds = DashStyle.Custom;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetPenDashStyle (pen, ds), "GdipSetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetPenDashStyle (IntPtr.Zero, ds), "GdipSetPenDashStyle-null");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenDashCount (pen, out count), "GdipGetPenDashCount");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashCount (IntPtr.Zero, out count), "GdipGetPenDashCount-null");
			Assert.AreEqual (0, count, "count");

			float[] dash = new float[count];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetPenDashArray (pen, dash, count), "GdipGetPenDashArray");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashArray (IntPtr.Zero, dash, count), "GdipGetPenDashArray-null-pen");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashArray (pen, null, count), "GdipGetPenDashArray-null-dash");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeletePen (IntPtr.Zero), "GdipDeletePen-null");
		}

		[Test]
		public void PenColor_81266 ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0x7f0000ff, 1f, GraphicsUnit.Pixel, out pen), "GdipCreatePen1");
			try {
				int color = 0;
				IntPtr brush;
				HandleRef brushHandle;
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenBrushFill (IntPtr.Zero, out brush), "GdipGetPenBrushFill-null");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenBrushFill (pen, out brush), "GdipGetPenBrushFill");
				brushHandle = new HandleRef (this, brush);

				try {
					Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipGetSolidFillColor (HandleRefZero, out color), "GdipGetSolidFillColor-null");
					Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetSolidFillColor (brushHandle, out color), "GdipGetSolidFillColor-0");
					Assert.AreEqual (0x7f0000ff, color, "color-0");

					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetPenColor (IntPtr.Zero, 0x7fff0000), "GdipSetPenColor-null");
					Assert.AreEqual (Status.Ok, GDIPlus.GdipSetPenColor (pen, 0x7fff0000), "GdipSetPenColor");

					Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetSolidFillColor (brushHandle, out color), "GdipGetSolidFillColor-1");
					// previous brush color didn't change
					Assert.AreEqual (0x7f0000ff, color, "color-1");
				}
				finally {
					Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
				}

				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenBrushFill (pen, out brush), "GdipGetPenBrushFill-2");
				brushHandle = new HandleRef (this, brush);
				try {
					Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetSolidFillColor (brushHandle, out color), "GdipGetSolidFillColor-2");
					// new brush color is updated
					Assert.AreEqual (0x7fff0000, color, "color-2");
				}
				finally {
					Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush-2");
				}
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");
			}
		}

		// Region
		[Test]
		public void CreateRegionRgnData ()
		{
			IntPtr region;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateRegionRgnData (null, 0, out region));

			byte[] data = new byte[0];
			Assert.AreEqual (Status.GenericError, GDIPlus.GdipCreateRegionRgnData (data, 0, out region));
		}

		[Test]
		public void DrawingOperations ()
		{
			IntPtr graphics, image;

			IntPtr pen;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero,
							   out image);

			GDIPlus.GdipGetImageGraphicsContext (image, out graphics);
			GDIPlus.GdipCreatePen1 (0, 0f, GraphicsUnit.World, out pen);

			// DrawCurve

			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawCurveI (IntPtr.Zero, IntPtr.Zero, null, 0));

			Assert.AreEqual (Status.InvalidParameter, 
					 GDIPlus.GdipDrawCurveI (graphics, pen, new Point [] {}, 0),
					 "DrawCurve with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawCurveI (graphics, pen,
								 new Point [] { new Point (1, 1) }, 1),
					 "DrawCurve with 1 pt");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipDrawCurveI (graphics, pen,
					                         new Point [] { new Point (1, 1),
					                                        new Point (2, 2) }, 2),
					 "DrawCurve with 2 pts");

			// DrawClosedCurve

			Assert.AreEqual (Status.InvalidParameter, 
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen, new Point [] {}, 0),
					 "DrawClosedCurve with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen,
					                               new Point [] { new Point (1, 1) }, 1),
					 "DrawClosedCurve with 1 pt");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen,
					                               new Point [] { new Point (1, 1),
					                                              new Point (2, 2) }, 2),
					 "DrawClosedCurve with 2 pt2");

			// DrawPolygon

			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawPolygonI (graphics, pen, new Point [] {}, 0),
					 "DrawPolygon with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawPolygonI (graphics, pen,
					                           new Point [] { new Point (1, 1) }, 1),
					 "DrawPolygon with only one pt");

			GDIPlus.GdipDeletePen (pen);			

			// FillClosedCurve

			IntPtr brush;
			GDIPlus.GdipCreateSolidFill (0, out brush);
			var brushHandle = new HandleRef (this, brush);

			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush, new Point [] {}, 0),
					 "FillClosedCurve with no pts");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush, 
												new Point [] { new Point (1, 1) }, 1),
					 "FillClosedCurve with 1 pt");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush,
					                               new Point [] { new Point (1, 1),
					                                              new Point (2, 2) }, 2),
					 "FillClosedCurve with 2 pts");
			
			GDIPlus.GdipDeleteBrush (brushHandle);
			
			GDIPlus.GdipDeleteGraphics (graphics);
			GDIPlus.GdipDisposeImage (image);
		}

		// StringFormat
		private void CheckStringFormat (IntPtr sf, StringFormatFlags exepcted_flags, StringTrimming expected_trimmings)
		{
			StringAlignment sa = StringAlignment.Center;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatAlign (IntPtr.Zero, out sa), "GdipGetStringFormatAlign-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatAlign (sf, out sa), "GdipGetStringFormatAlign");
			Assert.AreEqual (StringAlignment.Near, sa, "StringAlignment-1");

			StringAlignment la = StringAlignment.Center;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatLineAlign (IntPtr.Zero, out la), "GdipGetStringFormatLineAlign-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatLineAlign (sf, out la), "GdipGetStringFormatLineAlign");
			Assert.AreEqual (StringAlignment.Near, la, "StringAlignment-2");

			StringFormatFlags flags = StringFormatFlags.DirectionRightToLeft;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatFlags (IntPtr.Zero, out flags), "GdipGetStringFormatFlags-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatFlags (sf, out flags), "GdipGetStringFormatFlags");
			Assert.AreEqual (exepcted_flags, flags, "StringFormatFlags");

			HotkeyPrefix hotkey = HotkeyPrefix.Show;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatHotkeyPrefix (IntPtr.Zero, out hotkey), "GdipGetStringFormatHotkeyPrefix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatHotkeyPrefix (sf, out hotkey), "GdipGetStringFormatHotkeyPrefix");
			Assert.AreEqual (HotkeyPrefix.None, hotkey, "HotkeyPrefix");

			StringTrimming trimming = StringTrimming.None;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatTrimming (IntPtr.Zero, out trimming), "GdipGetStringFormatTrimming-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTrimming (sf, out trimming), "GdipGetStringFormatTrimming");
			Assert.AreEqual (expected_trimmings, trimming, "StringTrimming");

			StringDigitSubstitute sub = StringDigitSubstitute.Traditional;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatDigitSubstitution (IntPtr.Zero, 0, out sub), "GdipGetStringFormatDigitSubstitution-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatDigitSubstitution (sf, 0, out sub), "GdipGetStringFormatDigitSubstitution");
			Assert.AreEqual (StringDigitSubstitute.User, sub, "StringDigitSubstitute");

			int count;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (IntPtr.Zero, out count), "GdipGetStringFormatMeasurableCharacterRangeCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (sf, out count), "GdipGetStringFormatMeasurableCharacterRangeCount");
			Assert.AreEqual (0, count, "count");
		}

		[Test]
		public void StringFormat ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateStringFormat (Int32.MinValue, Int32.MinValue, out sf), "GdipCreateStringFormat");

			CheckStringFormat (sf, (StringFormatFlags) Int32.MinValue, StringTrimming.Character);

			CharacterRange[] ranges = new CharacterRange[32];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (IntPtr.Zero, 1, ranges), "GdipSetStringFormatMeasurableCharacterRanges-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (IntPtr.Zero, -1, ranges), "GdipSetStringFormatMeasurableCharacterRanges-negative");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 1, ranges), "GdipSetStringFormatMeasurableCharacterRanges");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 32, ranges), "GdipSetStringFormatMeasurableCharacterRanges-32");
			Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 33, ranges), "GdipSetStringFormatMeasurableCharacterRanges-33");

			float first = Single.MinValue;
			float[] tabs = new float[1];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatTabStops (IntPtr.Zero, 1.0f, 1, tabs), "GdipSetStringFormatTabStops-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, 1, null), "GdipSetStringFormatTabStops-null/tabs");
			
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, -1, tabs), "GdipSetStringFormatTabStops-negative");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTabStops (sf, 1, out first, tabs), "GdipGetStringFormatTabStops-negative");
			Assert.AreEqual (0.0f, first, "first-negative");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, 1, tabs), "GdipSetStringFormatTabStops");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTabStops (sf, 1, out first, tabs), "GdipGetStringFormatTabStops");
			Assert.AreEqual (1.0f, first, "first");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteStringFormat (IntPtr.Zero), "GdipDeleteStringFormat-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_Clone ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateStringFormat (Int32.MinValue, Int32.MinValue, out sf), "GdipCreateStringFormat");

			IntPtr clone;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCloneStringFormat (IntPtr.Zero, out clone), "GdipCloneStringFormat");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneStringFormat (sf, out clone), "GdipCloneStringFormat");

			CheckStringFormat (clone, (StringFormatFlags) Int32.MinValue, StringTrimming.Character);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (clone), "GdipDeleteStringFormat-clone");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_GenericDefault ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericDefault (out sf), "GdipStringFormatGetGenericDefault");

			CheckStringFormat (sf, (StringFormatFlags) 0, StringTrimming.Character);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_GenericTypographic ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericTypographic (out sf), "GdipStringFormatGetGenericTypographic");

			StringFormatFlags flags = StringFormatFlags.NoClip | StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox;
			CheckStringFormat (sf, flags , StringTrimming.None);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		// TextureBrush
		[Test]
		public void Texture ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			var imageHandle = new HandleRef (this, image);

			IntPtr brush;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipCreateTexture (HandleRefZero, (int) WrapMode.Tile, out brush), "GdipCreateTexture-image");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture (imageHandle, Int32.MinValue, out brush), "GdipCreateTexture-wrapmode");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreateTexture (imageHandle, (int) WrapMode.Tile, out brush), "GdipCreateTexture");
			var brushHandle = new HandleRef (this, brush);			

			IntPtr image2;
// this would throw an AccessViolationException under MS 2.0 (missing null check?)
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetTextureImage (IntPtr.Zero, out image2), "GdipGetTextureImage-brush");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetTextureImage (brushHandle, out image2), "GdipGetTextureImage");
			Assert.IsFalse (image == image2, "image");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image2), "GdipDisposeImage-image2");
		}

		[Test]
		public void Texture2 ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			var imageHandle = new HandleRef (this, image);

			IntPtr brush;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipCreateTexture2 (HandleRefZero, (int) WrapMode.Tile, 0, 0, 10, 10, out brush), "GdipCreateTexture2-image");
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipCreateTexture2 (HandleRefZero, Int32.MinValue, 0, 0, 10, 10, out brush), "GdipCreateTexture2-wrapmode");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 0, 0, 0, 10, out brush), "GdipCreateTexture2-width");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 0, 0, 10, 0, out brush), "GdipCreateTexture2-height");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, -1, 0, 0, 10, out brush), "GdipCreateTexture2-x");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 0, -1, 10, 0, out brush), "GdipCreateTexture2-y");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 1, 0, 10, 10, out brush), "GdipCreateTexture2-too-wide");
			Assert.AreEqual ((int) Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 0, 1, 10, 10, out brush), "GdipCreateTexture2-too-tall");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipCreateTexture2 (imageHandle, (int) WrapMode.Tile, 0, 0, 10, 10, out brush), "GdipCreateTexture2");
			var brushHandle = new HandleRef (this, brush);			

			int wm;
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipGetTextureWrapMode (HandleRefZero, out wm), "GdipGetTextureWrapMode-brush");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetTextureWrapMode (brushHandle, out wm), "GdipGetTextureWrapMode");
			Assert.AreEqual ((int) WrapMode.Tile, wm, "WrapMode");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipSetTextureWrapMode (HandleRefZero, (int) WrapMode.Clamp), "GdipSetTextureWrapMode-brush");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipSetTextureWrapMode (brushHandle, (int) WrapMode.Clamp), "GdipSetTextureWrapMode");
			GDIPlus.GdipGetTextureWrapMode (brushHandle, out wm);
			Assert.AreEqual ((int) WrapMode.Clamp, wm, "WrapMode.Clamp");

			// an invalid WrapMode is ignored
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipSetTextureWrapMode (brushHandle, Int32.MinValue), "GdipSetTextureWrapMode-wrapmode");
			GDIPlus.GdipGetTextureWrapMode (brushHandle, out wm);
			Assert.AreEqual ((int) WrapMode.Clamp, wm, "WrapMode/Invalid");

			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		[Test]
		public void TextureIA ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			var imageHandle = new HandleRef (this, image);

			IntPtr brush;
			Assert.AreEqual ((int)Status.InvalidParameter, GDIPlus.GdipCreateTextureIA (HandleRefZero, HandleRefZero, 0, 0, 10, 10, out brush), "GdipCreateTexture2-image");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 0, 0, 0, 10, out brush), "GdipCreateTexture2-width");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 0, 0, 10, 0, out brush), "GdipCreateTexture2-height");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, -1, 0, 10, 10, out brush), "GdipCreateTexture2-x");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 0, -1, 10, 10, out brush), "GdipCreateTexture2-y");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 1, 0, 10, 10, out brush), "GdipCreateTexture2-too-wide");
			Assert.AreEqual ((int)Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 0, 1, 10, 10, out brush), "GdipCreateTexture2-too-tall");

			Assert.AreEqual ((int)Status.Ok, GDIPlus.GdipCreateTextureIA (imageHandle, HandleRefZero, 0, 0, 10, 10, out brush), "GdipCreateTexture2");
			var brushHandle = new HandleRef (this, brush);

			// TODO - handle ImageAttribute in the tests

			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");
			var matrixHandle = new HandleRef (this, matrix);

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipGetTextureTransform (HandleRefZero, matrixHandle), "GdipGetTextureTransform-brush");
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipGetTextureTransform (brushHandle, HandleRefZero), "GdipGetTextureTransform-matrix");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipGetTextureTransform (brushHandle, matrixHandle), "GdipGetTextureTransform");

			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipSetTextureTransform (HandleRefZero, matrixHandle), "GdipSetTextureTransform-brush");
			Assert.AreEqual ((int) Status.InvalidParameter, GDIPlus.GdipSetTextureTransform (brushHandle, HandleRefZero), "GdipSetTextureTransform-matrix");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipSetTextureTransform (brushHandle, matrixHandle), "GdipSetTextureTransform");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual ((int) Status.Ok, GDIPlus.GdipDeleteBrush (brushHandle), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		private void CheckMetafileHeader (MetafileHeader header)
		{
			MetafileHeader mh1 = new Metafile (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf")).GetMetafileHeader ();
			// compare MetafileHeader
			Assert.AreEqual (mh1.Bounds.X, header.Bounds.X, "Bounds.X");
			Assert.AreEqual (mh1.Bounds.Y, header.Bounds.Y, "Bounds.Y");
			Assert.AreEqual (mh1.Bounds.Width, header.Bounds.Width, "Bounds.Width");
			Assert.AreEqual (mh1.Bounds.Height, header.Bounds.Height, "Bounds.Height");
			Assert.AreEqual (mh1.DpiX, header.DpiX, "DpiX");
			Assert.AreEqual (mh1.DpiY, header.DpiY, "DpiY");
			Assert.AreEqual (mh1.EmfPlusHeaderSize, header.EmfPlusHeaderSize, "EmfPlusHeaderSize");
			Assert.AreEqual (mh1.LogicalDpiX, header.LogicalDpiX, "LogicalDpiX");
			Assert.AreEqual (mh1.LogicalDpiY, header.LogicalDpiY, "LogicalDpiY");
			Assert.AreEqual (mh1.MetafileSize, header.MetafileSize, "MetafileSize");
			Assert.AreEqual (mh1.Type, header.Type, "Type");
			Assert.AreEqual (mh1.Version, header.Version, "Version");
			// compare MetaHeader
			MetaHeader mh1h = mh1.WmfHeader;
			MetaHeader mh2h = header.WmfHeader;
			Assert.AreEqual (mh1h.HeaderSize, mh2h.HeaderSize, "HeaderSize");
			Assert.AreEqual (mh1h.MaxRecord, mh2h.MaxRecord, "MaxRecord");
			Assert.AreEqual (mh1h.NoObjects, mh2h.NoObjects, "NoObjects");
			Assert.AreEqual (mh1h.NoParameters, mh2h.NoParameters, "NoParameters");
			Assert.AreEqual (mh1h.Size, mh2h.Size, "Size");
			Assert.AreEqual (mh1h.Type, mh2h.Type, "Type");
			Assert.AreEqual (mh1h.Version, mh2h.Version, "Version");
		}

		[Test]
		public void Metafile ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			IntPtr metafile = IntPtr.Zero;

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateMetafileFromFile (null, out metafile), "GdipCreateMetafileFromFile(null)");
			Assert.AreEqual (Status.GenericError, GDIPlus.GdipCreateMetafileFromFile ("doesnotexists", out metafile), "GdipCreateMetafileFromFile(doesnotexists)");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMetafileFromFile (filename, out metafile), "GdipCreateMetafileFromFile");

			// looks like it applies to EmfOnly and EmfDual (not EmfPlus or Wmf*)
			uint limit = UInt32.MaxValue;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileDownLevelRasterizationLimit (IntPtr.Zero, ref limit), "GdipGetMetafileDownLevelRasterizationLimit/null");
			Assert.AreEqual (Status.WrongState, GDIPlus.GdipGetMetafileDownLevelRasterizationLimit (metafile, ref limit), "GdipGetMetafileDownLevelRasterizationLimit");
			Assert.AreEqual (UInt32.MaxValue, limit, "DownLevelRasterizationLimit");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetMetafileDownLevelRasterizationLimit (IntPtr.Zero, limit), "GdipSetMetafileDownLevelRasterizationLimit/null");
			Assert.AreEqual (Status.WrongState, GDIPlus.GdipSetMetafileDownLevelRasterizationLimit (metafile, limit), "GdipSetMetafileDownLevelRasterizationLimit");

			int size = Marshal.SizeOf (typeof (MetafileHeader));
			IntPtr header = Marshal.AllocHGlobal (size);
			try {
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromMetafile (IntPtr.Zero, header), "GdipGetMetafileHeaderFromMetafile(null,header)");
// We get access violation here!
//				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromMetafile (metafile, IntPtr.Zero), "GdipGetMetafileHeaderFromMetafile(metafile,null)");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetMetafileHeaderFromMetafile (metafile, header), "GdipGetMetafileHeaderFromMetafile(metafile,header)");

				MetafileHeader mh2 = new Metafile (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf")).GetMetafileHeader ();
				Marshal.PtrToStructure (header, mh2);
				CheckMetafileHeader (mh2);
			}
			finally {
				Marshal.FreeHGlobal (header);
			}

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (metafile), "GdipDisposeImage");
		}

		[Test]
		public void Metafile_GetMetafileHeaderFromFile ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");

			int size = Marshal.SizeOf (typeof (MetafileHeader));
			IntPtr ptr = Marshal.AllocHGlobal (size);
			try {
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromFile ("does-not-exists", ptr), "GdipGetMetafileHeaderFromFile(doesnotexists,ptr)");
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromFile (null, ptr), "GdipGetMetafileHeaderFromFile(null,ptr)");
// We get access violation here!
//				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromFile (filename, IntPtr.Zero), "GdipGetMetafileHeaderFromFile(file,null)");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetMetafileHeaderFromFile (filename, ptr), "GdipGetMetafileHeaderFromFile(file,ptr)");

				MetafileHeader header = new Metafile (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf")).GetMetafileHeader ();
				Marshal.PtrToStructure (ptr, header);
				CheckMetafileHeader (header);
			}
			finally {
				Marshal.FreeHGlobal (ptr);
			}
		}

		[Test]
		public void Metafile_Hemf ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			IntPtr metafile = IntPtr.Zero;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMetafileFromFile (filename, out metafile), "GdipCreateMetafileFromFile");

			IntPtr emf = IntPtr.Zero;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetHemfFromMetafile (IntPtr.Zero, out emf), "GdipGetHemfFromMetafile(null,emf)");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetHemfFromMetafile (metafile, out emf), "GdipGetHemfFromMetafile(metafile,emf)");

			int size = Marshal.SizeOf (typeof (MetafileHeader));
			IntPtr header = Marshal.AllocHGlobal (size);
			try {
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromEmf (IntPtr.Zero, header), "GdipGetMetafileHeaderFromEmf(null,header)");
// We get access violation here!
//				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromEmf (emf, IntPtr.Zero), "GdipGetMetafileHeaderFromEmf(emf,null)");
				// the HEMF handle cannot be used here
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMetafileHeaderFromEmf (emf, header), "GdipGetMetafileHeaderFromEmf(emf,header)");
			}
			finally {
				Marshal.FreeHGlobal (header);
			}

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (metafile), "GdipDisposeImage");
		}

		private void InImageAPI (IntPtr image)
		{
			IntPtr ptr = IntPtr.Zero;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageGraphicsContext (image, out ptr), "GdipGetImageGraphicsContext");

			RectangleF bounds;
			GraphicsUnit unit = GraphicsUnit.Display;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageBounds (image, out bounds, ref unit), "GdipGetImageBounds");
			Assert.AreEqual (-30, bounds.X, "bounds.X");
			Assert.AreEqual (-40, bounds.Y, "bounds.Y");
			Assert.AreEqual (3096, bounds.Width, "bounds.Width");
			Assert.AreEqual (4127, bounds.Height, "bounds.Height");
			Assert.AreEqual (GraphicsUnit.Pixel, unit, "uint");

			float width, height;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageDimension (image, out width, out height), "GdipGetImageDimension");
			Assert.AreEqual (12976.6328f, width, 0.001f, "GdipGetImageDimension/Width");
			Assert.AreEqual (17297.9863f, height, 0.02f, "GdipGetImageDimension/Height");

			ImageType type;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageType (image, out type), "GdipGetImageType");
			Assert.AreEqual (ImageType.Metafile, type, "Metafile");

			uint w;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageWidth (image, out w), "GdipGetImageWidth");
			Assert.AreEqual (3096, w, "GdipGetImageWidth/Width");

			uint h;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageHeight (image, out h), "GdipGetImageHeight");
			Assert.AreEqual (4127, h, "GdipGetImageHeight/Height");

			float horz;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageHorizontalResolution (image, out horz), "GdipGetImageHorizontalResolution");
			Assert.AreEqual (606, horz, "HorizontalResolution");

			float vert;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageVerticalResolution (image, out vert), "GdipGetImageVerticalResolution");
			Assert.AreEqual (606, vert, "VerticalResolution");

			int flags;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageFlags (image, out flags), "GdipGetImageFlags");
			Assert.AreEqual (327683, flags, "Flags");

			Guid format;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageRawFormat (image, out format), "GdipGetImageRawFormat");
			Assert.AreEqual ("b96b3cad-0728-11d3-9d7b-0000f81ef32e", format.ToString (), "Format");

			PixelFormat pformat;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePixelFormat (image, out pformat), "GdipGetImagePixelFormat");
			Assert.AreEqual (PixelFormat.Format32bppRgb, pformat, "PixelFormat");

			uint count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageGetFrameDimensionsCount (image, out count), "GdipImageGetFrameDimensionsCount");
			Assert.AreEqual (1, count, "FrameDimensionsCount");

			Guid[] dimid = new Guid[1];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageGetFrameDimensionsList (image, dimid, count), "GdipImageGetFrameDimensionsList");
			Assert.AreEqual ("7462dc86-6180-4c7e-8e3f-ee7333a7a483", dimid[0].ToString (), "Id[0]");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipImageGetFrameDimensionsList (image, dimid, 0), "GdipImageGetFrameDimensionsList/0");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipImageGetFrameDimensionsList (image, dimid, 2), "GdipImageGetFrameDimensionsList/2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageGetFrameCount (image, ref dimid[0], out count), "GdipImageGetFrameCount");
			Assert.AreEqual (1, count, "FrameCount");
			Guid g = Guid.Empty;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageGetFrameCount (image, ref g, out count), "GdipImageGetFrameCount/Empty");
			Assert.AreEqual (1, count, "FrameCount/Empty");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageSelectActiveFrame (image, ref dimid[0], 0), "GdipImageSelectActiveFrame");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageSelectActiveFrame (image, ref g, 0), "GdipImageSelectActiveFrame/Empty");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageSelectActiveFrame (image, ref dimid[0], Int32.MinValue), "GdipImageSelectActiveFrame/MinValue");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipImageSelectActiveFrame (image, ref dimid[0], Int32.MaxValue), "GdipImageSelectActiveFrame/MaxValue");

			// woohoo :)
			foreach (RotateFlipType rft in Enum.GetValues (typeof (RotateFlipType))) {
				Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipImageRotateFlip (image, rft), rft.ToString ());
			}

			int size;
			Assert.AreEqual (Status.GenericError, GDIPlus.GdipGetImagePaletteSize (image, out size), "GdipGetImagePaletteSize");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetImagePalette (image, image, 1024), "GdipGetImagePalette");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipSetImagePalette (image, image), "GdipSetImagePalette");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPropertyCount (image, out count), "GdipGetPropertyCount");
			Assert.AreEqual (0, count, "PropertyCount");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetPropertyIdList (image, 0, new int[1]), "GdipGetPropertyIdList");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetPropertyItemSize (image, 0, out size), "GdipGetPropertyItemSize");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetPropertyItem (image, 0, size, image), "GdipGetPropertyItem");
			int numbers;
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetPropertySize (image, out size, out numbers), "GdipGetPropertySize");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipRemovePropertyItem (image, 0), "GdipRemovePropertyItem");
			//Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipSetPropertyItem (image, image), "GdipSetPropertyItem");
			Assert.AreEqual (Status.NotImplemented, GDIPlus.GdipGetAllPropertyItems (image, 1000, 1, image), "GdipGetAllPropertyItems");

			Guid wmf = ImageFormat.Wmf.Guid;
			Assert.AreEqual (Status.FileNotFound, GDIPlus.GdipGetEncoderParameterListSize (image, ref wmf, out count), "GdipGetEncoderParameterListSize/wmf");
			Assert.AreEqual (Status.FileNotFound, GDIPlus.GdipGetEncoderParameterListSize (image, ref g, out count), "GdipGetEncoderParameterListSize/unknown");

			Assert.AreEqual (Status.FileNotFound, GDIPlus.GdipGetEncoderParameterList (image, ref wmf, count, image), "GdipGetEncoderParameterList/wmf");
			Assert.AreEqual (Status.FileNotFound, GDIPlus.GdipGetEncoderParameterList (image, ref g, count, image), "GdipGetEncoderParameterList/unknown");

			IntPtr clone;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneImage (image, out clone), "GdipCloneImage");
			try {
				Assert.IsFalse (image == clone, "Handle");
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (clone), "GdipDisposeImage");
			}
		}

		[Test]
		public void MetafileAsImage_InImageAPI ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			IntPtr image = IntPtr.Zero;

			Assert.AreEqual (Status.Ok, GDIPlus.GdipLoadImageFromFile (filename, out image), "GdipLoadImageFromFile");
			try {
				InImageAPI (image);
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
			}
		}

		[Test]
		public void Metafile_InImageAPI ()
		{
			string filename = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/telescope_01.wmf");
			IntPtr metafile = IntPtr.Zero;

			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMetafileFromFile (filename, out metafile), "GdipCreateMetafileFromFile");
			try {
				InImageAPI (metafile);
			}
			finally {
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (metafile), "GdipDisposeImage");
			}
		}

		private void RecordFileName (IntPtr hdc, EmfType type, MetafileFrameUnit unit)
		{
			string filename = String.Format ("test-{0}-{1}.emf", type, unit);
			IntPtr metafile;
			RectangleF rect = new RectangleF (10, 20, 100, 200);
			Status status = GDIPlus.GdipRecordMetafileFileName (filename, hdc, type, ref rect, unit, filename, out metafile);
			if (metafile != IntPtr.Zero)
				GDIPlus.GdipDisposeImage (metafile);
			if (status == Status.Ok)
				File.Delete (filename);
			Assert.AreEqual (Status.Ok, status, filename);
		}

		private Status RecordFileName_EmptyRectangle (IntPtr hdc, MetafileFrameUnit unit)
		{
			string filename = String.Format ("emptyrectangle-{0}.emf", unit);
			IntPtr metafile = IntPtr.Zero;
			RectangleF empty = new RectangleF ();
			Status status = GDIPlus.GdipRecordMetafileFileName (filename, hdc, EmfType.EmfPlusDual, ref empty, unit, filename, out metafile);
			if (metafile != IntPtr.Zero)
				GDIPlus.GdipDisposeImage (metafile);
			if (status == Status.Ok)
				File.Delete (filename);
			return status;
		}

		[Test]
		public void RecordMetafileFileName ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppArgb)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					IntPtr hdc = g.GetHdc ();
					try {
						IntPtr metafile;
						RectangleF rect = new RectangleF ();
						Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipRecordMetafileFileName (null, hdc, EmfType.EmfPlusOnly, ref rect, MetafileFrameUnit.GdiCompatible, "unit test", out metafile), "filename-null");
						Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipRecordMetafileFileName ("a.emf", IntPtr.Zero, EmfType.EmfPlusOnly, ref rect, MetafileFrameUnit.GdiCompatible, "unit test", out metafile), "hdc-null");
						Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipRecordMetafileFileName ("b.emf", hdc, (EmfType)Int32.MaxValue, ref rect, MetafileFrameUnit.GdiCompatible, "unit test", out metafile), "type-invalid");
						Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipRecordMetafileFileName ("c.emf", hdc, EmfType.EmfPlusOnly, ref rect, (MetafileFrameUnit)Int32.MaxValue, "unit test", out metafile), "unit-invalid");
						Assert.AreEqual (Status.Ok, GDIPlus.GdipRecordMetafileFileName ("d.emf", hdc, EmfType.EmfPlusOnly, ref rect, MetafileFrameUnit.GdiCompatible, null, out metafile), "description-null");
						GDIPlus.GdipDisposeImage (metafile);
						File.Delete ("d.emf");
						// test some variations
						Assert.AreEqual (Status.GenericError, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.Document), "EmptyRectangle-Document");
						Assert.AreEqual (Status.GenericError, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.Inch), "EmptyRectangle-Inch");
						Assert.AreEqual (Status.GenericError, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.Millimeter), "EmptyRectangle-Millimeter");
						Assert.AreEqual (Status.GenericError, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.Pixel), "EmptyRectangle-Pixel");
						Assert.AreEqual (Status.GenericError, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.Point), "EmptyRectangle-Point");
						Assert.AreEqual (Status.Ok, RecordFileName_EmptyRectangle (hdc, MetafileFrameUnit.GdiCompatible), "EmptyRectangle-GdiCompatible");
						RecordFileName (hdc, EmfType.EmfOnly, MetafileFrameUnit.Document);
						RecordFileName (hdc, EmfType.EmfPlusDual, MetafileFrameUnit.GdiCompatible);
						RecordFileName (hdc, EmfType.EmfPlusOnly, MetafileFrameUnit.Inch);
						RecordFileName (hdc, EmfType.EmfOnly, MetafileFrameUnit.Millimeter);
						RecordFileName (hdc, EmfType.EmfPlusDual, MetafileFrameUnit.Pixel);
						RecordFileName (hdc, EmfType.EmfPlusOnly, MetafileFrameUnit.Point);
					}
					finally {
						g.ReleaseHdc (hdc);
					}
				}
			}
		}
	}
}
