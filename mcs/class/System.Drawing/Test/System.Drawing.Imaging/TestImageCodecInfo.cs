//
// ImageCodecInfo class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Drawing
{

	[TestFixture]	
	public class TestImageCodecInfo 
	{
		Hashtable decoders;
		Hashtable encoders;

		ImageCodecInfo GetDecoder (string clsid)
		{
			return GetDecoder (new Guid (clsid));
		}

		ImageCodecInfo GetDecoder (Guid clsid)
		{
			return (ImageCodecInfo) decoders [clsid];
		}

		ImageCodecInfo GetEncoder (string clsid)
		{
			return GetEncoder (new Guid (clsid));
		}

		ImageCodecInfo GetEncoder (Guid clsid)
		{
			return (ImageCodecInfo) encoders [clsid];
		}

		[TestFixtureSetUp]
		public void FixtureGetReady()		
		{
			decoders = new Hashtable ();
			encoders = new Hashtable ();

			foreach (ImageCodecInfo decoder in ImageCodecInfo.GetImageDecoders())
				decoders[decoder.Clsid] = decoder;
		
			foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
				encoders[encoder.Clsid] = encoder;
		}
		
		[Test]
		public void BMPDecoder()
		{
			Assert.AreEqual (ImageFormat.Bmp.Guid,
				GetDecoder ("557cf400-1a04-11d3-9a73-0000f81ef32e").FormatID, "BMP");
		}

		[Test]
		public void GifDecoder()
		{
			Assert.AreEqual (ImageFormat.Gif.Guid,
				GetDecoder ("557cf402-1a04-11d3-9a73-0000f81ef32e").FormatID, "GIF");
		}
		
		[Test]
		public void JpegDecoder()
		{
			Assert.AreEqual (ImageFormat.Jpeg.Guid,
				GetDecoder ("557cf401-1a04-11d3-9a73-0000f81ef32e").FormatID, "JPEG");
		}

		[Test]
		public void PngDecoder()
		{
			Assert.AreEqual (ImageFormat.Png.Guid,
				GetDecoder ("557cf406-1a04-11d3-9a73-0000f81ef32e").FormatID, "PNG");
		}
		[Test]
		public void BMPEncoder() {
			Assert.AreEqual (ImageFormat.Bmp.Guid,
				GetEncoder ("557cf400-1a04-11d3-9a73-0000f81ef32e").FormatID, "BMP");
		}

		[Test]
		public void GifEncoder() {
			Assert.AreEqual (ImageFormat.Gif.Guid,
				GetEncoder ("557cf402-1a04-11d3-9a73-0000f81ef32e").FormatID, "GIF");
		}
		
		[Test]
		public void JpegEncoder() {
			Assert.AreEqual (ImageFormat.Jpeg.Guid,
				GetEncoder ("557cf401-1a04-11d3-9a73-0000f81ef32e").FormatID, "JPEG");
		}

		[Test]
		public void PngEncoder() {
			Assert.AreEqual (ImageFormat.Png.Guid,
				GetEncoder ("557cf406-1a04-11d3-9a73-0000f81ef32e").FormatID, "PNG");
		}
	}
}
