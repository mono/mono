//
// Copyright (C) 2005,2006 Novell, Inc (http://www.novell.com)
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
// Authors:
//	Jordi Mas i Hernàndez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ImageFormatTest {

		private static ImageFormat BmpImageFormat = new ImageFormat (new Guid ("b96b3cab-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat EmfImageFormat = new ImageFormat (new Guid ("b96b3cac-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat ExifImageFormat = new ImageFormat (new Guid ("b96b3cb2-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat GifImageFormat = new ImageFormat (new Guid ("b96b3cb0-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat TiffImageFormat = new ImageFormat (new Guid ("b96b3cb1-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat PngImageFormat = new ImageFormat(new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat MemoryBmpImageFormat = new ImageFormat (new Guid ("b96b3caa-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat IconImageFormat = new ImageFormat (new Guid ("b96b3cb5-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat JpegImageFormat = new ImageFormat(new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat WmfImageFormat = new ImageFormat (new Guid ("b96b3cad-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat CustomImageFormat = new ImageFormat (new Guid ("48749428-316f-496a-ab30-c819a92b3137"));

		[Test]
		public void DefaultImageFormats ()
		{
			Assert.AreEqual (BmpImageFormat.Guid, ImageFormat.Bmp.Guid, "DefaultImageFormats#1");
			Assert.AreEqual (EmfImageFormat.Guid, ImageFormat.Emf.Guid, "DefaultImageFormats#2");
			Assert.AreEqual (ExifImageFormat.Guid, ImageFormat.Exif.Guid, "DefaultImageFormats#3");
			Assert.AreEqual (GifImageFormat.Guid, ImageFormat.Gif.Guid, "DefaultImageFormats#4");
			Assert.AreEqual (TiffImageFormat.Guid, ImageFormat.Tiff.Guid, "DefaultImageFormats#5");
			Assert.AreEqual (PngImageFormat.Guid, ImageFormat.Png.Guid, "DefaultImageFormats#6");
			Assert.AreEqual (MemoryBmpImageFormat.Guid, ImageFormat.MemoryBmp.Guid, "DefaultImageFormats#7");
			Assert.AreEqual (IconImageFormat.Guid, ImageFormat.Icon.Guid, "DefaultImageFormats#8");
			Assert.AreEqual (JpegImageFormat.Guid, ImageFormat.Jpeg.Guid, "DefaultImageFormats#9");
			Assert.AreEqual (WmfImageFormat.Guid, ImageFormat.Wmf.Guid, "DefaultImageFormats#10");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("[ImageFormat: b96b3cab-0728-11d3-9d7b-0000f81ef32e]", BmpImageFormat.ToString (),"ToStringTest#1");
			Assert.AreEqual ("[ImageFormat: b96b3cac-0728-11d3-9d7b-0000f81ef32e]", EmfImageFormat.ToString (), "ToStringTest#2");
			Assert.AreEqual ("[ImageFormat: b96b3cb2-0728-11d3-9d7b-0000f81ef32e]", ExifImageFormat.ToString (), "ToStringTest#3");
			Assert.AreEqual ("[ImageFormat: b96b3cb0-0728-11d3-9d7b-0000f81ef32e]", GifImageFormat.ToString (), "ToStringTest#4");
			Assert.AreEqual ("[ImageFormat: b96b3cb1-0728-11d3-9d7b-0000f81ef32e]", TiffImageFormat.ToString (), "ToStringTest#5");
			Assert.AreEqual ("[ImageFormat: b96b3caf-0728-11d3-9d7b-0000f81ef32e]", PngImageFormat.ToString (), "ToStringTest#6");
			Assert.AreEqual ("[ImageFormat: b96b3caa-0728-11d3-9d7b-0000f81ef32e]", MemoryBmpImageFormat.ToString (), "ToStringTest#7");
			Assert.AreEqual ("[ImageFormat: b96b3cb5-0728-11d3-9d7b-0000f81ef32e]", IconImageFormat.ToString (), "ToStringTest#8");
			Assert.AreEqual ("[ImageFormat: b96b3cae-0728-11d3-9d7b-0000f81ef32e]", JpegImageFormat.ToString (), "ToStringTest#9");
			Assert.AreEqual ("[ImageFormat: b96b3cad-0728-11d3-9d7b-0000f81ef32e]", WmfImageFormat.ToString (), "ToStringTest#10");
			Assert.AreEqual ("[ImageFormat: 48749428-316f-496a-ab30-c819a92b3137]", CustomImageFormat.ToString (), "ToStringTest#11");
		}

		[Test]
		public void WellKnown_ToString ()
		{
			Assert.AreEqual ("Bmp", ImageFormat.Bmp.ToString (),"ToStringTest#1");
			Assert.AreEqual ("Emf", ImageFormat.Emf.ToString (), "ToStringTest#2");
			Assert.AreEqual ("Exif", ImageFormat.Exif.ToString (), "ToStringTest#3");
			Assert.AreEqual ("Gif", ImageFormat.Gif.ToString (), "ToStringTest#4");
			Assert.AreEqual ("Tiff", ImageFormat.Tiff.ToString (), "ToStringTest#5");
			Assert.AreEqual ("Png", ImageFormat.Png.ToString (), "ToStringTest#6");
			Assert.AreEqual ("MemoryBMP", ImageFormat.MemoryBmp.ToString (), "ToStringTest#7");
			Assert.AreEqual ("Icon", ImageFormat.Icon.ToString (), "ToStringTest#8");
			Assert.AreEqual ("Jpeg", ImageFormat.Jpeg.ToString (), "ToStringTest#9");
			Assert.AreEqual ("Wmf", ImageFormat.Wmf.ToString (), "ToStringTest#10");
		}

		[Test]
		public void TestEquals ()
		{
			Assert.IsTrue (BmpImageFormat.Equals (BmpImageFormat), "Bmp-Bmp");
			Assert.IsTrue (EmfImageFormat.Equals (EmfImageFormat), "Emf-Emf");
			Assert.IsTrue (ExifImageFormat.Equals (ExifImageFormat), "Exif-Exif");
			Assert.IsTrue (GifImageFormat.Equals (GifImageFormat), "Gif-Gif");
			Assert.IsTrue (TiffImageFormat.Equals (TiffImageFormat), "Tiff-Tiff");
			Assert.IsTrue (PngImageFormat.Equals (PngImageFormat), "Png-Png");
			Assert.IsTrue (MemoryBmpImageFormat.Equals (MemoryBmpImageFormat), "MemoryBmp-MemoryBmp");
			Assert.IsTrue (IconImageFormat.Equals (IconImageFormat), "Icon-Icon");
			Assert.IsTrue (JpegImageFormat.Equals (JpegImageFormat), "Jpeg-Jpeg");
			Assert.IsTrue (WmfImageFormat.Equals (WmfImageFormat), "Wmf-Wmf");
			Assert.IsTrue (CustomImageFormat.Equals (CustomImageFormat), "Custom-Custom");

			Assert.IsFalse (BmpImageFormat.Equals (EmfImageFormat), "Bmp-Emf");
			Assert.IsFalse (BmpImageFormat.Equals ("Bmp"), "Bmp-String-1");
			Assert.IsFalse (BmpImageFormat.Equals (BmpImageFormat.ToString ()), "Bmp-String-2");
		}

		[Test]
		public void TestGetHashCode ()
		{
			Assert.AreEqual (BmpImageFormat.GetHashCode (), BmpImageFormat.Guid.GetHashCode (), "Bmp");
			Assert.AreEqual (EmfImageFormat.GetHashCode (), EmfImageFormat.Guid.GetHashCode (), "Emf");
			Assert.AreEqual (ExifImageFormat.GetHashCode (), ExifImageFormat.Guid.GetHashCode (), "Exif");
			Assert.AreEqual (GifImageFormat.GetHashCode (), GifImageFormat.Guid.GetHashCode (), "Gif");
			Assert.AreEqual (TiffImageFormat.GetHashCode (), TiffImageFormat.Guid.GetHashCode (), "Tiff");
			Assert.AreEqual (PngImageFormat.GetHashCode (), PngImageFormat.Guid.GetHashCode (), "Png");
			Assert.AreEqual (MemoryBmpImageFormat.GetHashCode (), MemoryBmpImageFormat.Guid.GetHashCode (), "MemoryBmp");
			Assert.AreEqual (IconImageFormat.GetHashCode (), IconImageFormat.Guid.GetHashCode (), "Icon");
			Assert.AreEqual (JpegImageFormat.GetHashCode (), JpegImageFormat.Guid.GetHashCode (), "Jpeg");
			Assert.AreEqual (WmfImageFormat.GetHashCode (), WmfImageFormat.Guid.GetHashCode (), "Wmf");
			Assert.AreEqual (CustomImageFormat.GetHashCode (), CustomImageFormat.Guid.GetHashCode (), "Custom");
		}
	}
}
