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

		ImageCodecInfo GetEncoder (Guid clsid)
		{
			return (ImageCodecInfo) encoders [clsid];
		}

		ImageCodecInfo GetDecoder (Guid clsid) {
			return (ImageCodecInfo) decoders [clsid];
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

		static void Check (ImageCodecInfo e, ImageCodecInfo d, Guid FormatID, string CodecName, string DllName,
			string FilenameExtension, ImageCodecFlags Flags, string FormatDescription,
			string MimeType/*, byte [][] SignatureMasks*/)
		{
			Assert.AreEqual (FormatID, e.FormatID, "Encoder.FormatID");
			Assert.AreEqual (CodecName, e.CodecName, "Encoder.CodecName");
			Assert.AreEqual (DllName, e.DllName, "Encoder.DllName");
			Assert.AreEqual (FilenameExtension, e.FilenameExtension, "Encoder.FilenameExtension");
			Assert.AreEqual (Flags, e.Flags, "Encoder.Flags");
			Assert.AreEqual (FormatDescription, e.FormatDescription, "Encoder.FormatDescription");
			Assert.AreEqual (MimeType, e.MimeType, "Encoder.MimeType");

			Assert.AreEqual (FormatID, d.FormatID, "Decoder.FormatID");
			Assert.AreEqual (CodecName, d.CodecName, "Decoder.CodecName");
			Assert.AreEqual (DllName, d.DllName, "Decoder.DllName");
			Assert.AreEqual (FilenameExtension, d.FilenameExtension, "Decoder.FilenameExtension");
			Assert.AreEqual (Flags, d.Flags, "Decoder.Flags");
			Assert.AreEqual (FormatDescription, d.FormatDescription, "Decoder.FormatDescription");
			Assert.AreEqual (MimeType, d.MimeType, "Decoder.MimeType");
			/*
			if (SignatureMasks == null) {
				Assert.AreEqual (null, e.SignatureMasks, "Encoder.SignatureMasks");
				Assert.AreEqual (null, d.SignatureMasks, "Decoder.SignatureMasks");
			}
			else {
				Assert.AreEqual (SignatureMasks.Length, e.SignatureMasks.Length, "Encoder.SignatureMasks.Length");
				Assert.AreEqual (SignatureMasks.Length, d.SignatureMasks.Length, "Decoder.SignatureMasks.Length");
				for (int i = 0; i < SignatureMasks.Length; i++) {
					Assert.AreEqual (SignatureMasks[i].Length, e.SignatureMasks[i].Length,
						"Encoder.SignatureMasks["+i.ToString ()+"].Length");
					Assert.AreEqual (SignatureMasks[i].Length, d.SignatureMasks[i].Length,
						"Decoder.SignatureMasks["+i.ToString ()+"].Length");
					for (int j = 0; j < SignatureMasks[i].Length; j++) {
						Assert.AreEqual (SignatureMasks[i][j], e.SignatureMasks[i][j],
							"Encoder.SignatureMasks["+i.ToString ()+"]["+j.ToString ()+"]");
						Assert.AreEqual (SignatureMasks[i][j], d.SignatureMasks[i][j],
							"Decoder.SignatureMasks["+i.ToString ()+"]["+j.ToString ()+"]");
					}
				}
			}
			*/
		}

		[Test]
		public void BMPCodec()
		{
			Guid g = new Guid ("557cf400-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Bmp.Guid,
				"Built-in BMP Codec", null, "*.BMP;*.DIB;*.RLE",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"BMP", "image/bmp");
		}

		[Test]
		public void GifCodec()
		{
			Guid g = new Guid ("557cf402-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Gif.Guid,
				"Built-in GIF Codec", null, "*.GIF",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"GIF", "image/gif");
		}
		
		[Test]
		public void JpegCodec()
		{
			Guid g = new Guid ("557cf401-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Jpeg.Guid,
				"Built-in JPEG Codec", null, "*.JPG;*.JPEG;*.JPE;*.JFIF",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"JPEG", "image/jpeg");
		}

		[Test]
		public void PngCodec()
		{
			Guid g = new Guid ("557cf406-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Png.Guid,
				"Built-in PNG Codec", null, "*.PNG",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"PNG", "image/png");
		}
	}
}
