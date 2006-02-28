//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class TestImageFormat
	{
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
			Assert.AreEqual (BmpImageFormat.ToString (), "Bmp", "ToStringTest#1");
			Assert.AreEqual (EmfImageFormat.ToString (), "Emf", "ToStringTest#2");
			Assert.AreEqual (ExifImageFormat.ToString (), "Exif", "ToStringTest#3");
			Assert.AreEqual (GifImageFormat.ToString (), "Gif", "ToStringTest#4");
			Assert.AreEqual (TiffImageFormat.ToString (), "Tiff", "ToStringTest#5");
			Assert.AreEqual (PngImageFormat.ToString (), "Png", "ToStringTest#6");
			Assert.AreEqual (MemoryBmpImageFormat.ToString (), "MemoryBmp", "ToStringTest#7");
			Assert.AreEqual (IconImageFormat.ToString (), "Icon", "ToStringTest#8");
			Assert.AreEqual (JpegImageFormat.ToString (), "Jpeg", "ToStringTest#9");
			Assert.AreEqual (WmfImageFormat.ToString (), "Wmf", "ToStringTest#10");
			Assert.AreEqual (CustomImageFormat.ToString (), "[ImageFormat: 48749428-316f-496a-ab30-c819a92b3137]", "ToStringTest#11");
		}
	}
}
