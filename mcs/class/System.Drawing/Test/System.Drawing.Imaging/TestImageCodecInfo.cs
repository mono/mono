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
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
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
			ImageCodecInfo [] arrEnc  = ImageCodecInfo.GetImageDecoders ();
			ImageCodecInfo [] arrDec = ImageCodecInfo.GetImageEncoders ();
			decoders = new Hashtable ();
			encoders = new Hashtable ();

			foreach (ImageCodecInfo decoder in arrDec)
				decoders[decoder.Clsid] = decoder;
		
			foreach (ImageCodecInfo encoder in arrEnc)
				encoders[encoder.Clsid] = encoder;
		}

		static void Check (ImageCodecInfo e, ImageCodecInfo d, Guid FormatID, string CodecName, string DllName,
			string FilenameExtension, ImageCodecFlags Flags, string FormatDescription,
			string MimeType, int Version)
		{
			Regex extRegex = new Regex (@"^(\*\.\w+(;(\*\.\w+))*;)?"+
				Regex.Escape (FilenameExtension)+@"(;\*\.\w+(;(\*\.\w+))*)?$",
				RegexOptions.IgnoreCase | RegexOptions.Singleline);

			if (e != null) {
				Assert.AreEqual (FormatID, e.FormatID, "Encoder.FormatID");
				Assert.IsTrue (e.CodecName.IndexOf (CodecName)>=0,
					"Encoder.CodecName contains "+CodecName);
				Assert.AreEqual (DllName, e.DllName, "Encoder.DllName");
				Assert.IsTrue (extRegex.IsMatch (e.FilenameExtension),
					"Encoder.FilenameExtension is a right list with "+FilenameExtension);
				Assert.AreEqual (Flags, e.Flags, "Encoder.Flags");
				Assert.IsTrue (e.FormatDescription.IndexOf (FormatDescription)>=0,
					"Encoder.FormatDescription contains "+FormatDescription);
				Assert.IsTrue (e.MimeType.IndexOf (MimeType)>=0,
					"Encoder.MimeType contains "+MimeType);
			}
			if (d != null) {
				Assert.AreEqual (FormatID, d.FormatID, "Decoder.FormatID");
				Assert.IsTrue (d.CodecName.IndexOf (CodecName)>=0,
					"Decoder.CodecName contains "+CodecName);
				Assert.AreEqual (DllName, d.DllName, "Decoder.DllName");
				Assert.IsTrue (extRegex.IsMatch (d.FilenameExtension),
					"Decoder.FilenameExtension is a right list with "+FilenameExtension);
				Assert.AreEqual (Flags, d.Flags, "Decoder.Flags");
				Assert.IsTrue (d.FormatDescription.IndexOf (FormatDescription)>=0,
					"Decoder.FormatDescription contains "+FormatDescription);
				Assert.IsTrue (d.MimeType.IndexOf (MimeType)>=0,
					"Decoder.MimeType contains "+MimeType);
			}
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
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void BMPCodec()
		{
			Guid g = new Guid ("557cf400-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Bmp.Guid,
				"BMP", null, "*.BMP",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"BMP", "image/bmp", 1);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void GifCodec()
		{
			Guid g = new Guid ("557cf402-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Gif.Guid,
				"GIF", null, "*.GIF",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"GIF", "image/gif", 1);
		}
		
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void JpegCodec()
		{
			Guid g = new Guid ("557cf401-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Jpeg.Guid,
				"JPEG", null, "*.JPG",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"JPEG", "image/jpeg", 1);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void PngCodec()
		{
			Guid g = new Guid ("557cf406-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Png.Guid,
				"PNG", null, "*.PNG",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.Decoder|ImageCodecFlags.SupportBitmap,
				"PNG", "image/png", 1);
		}
		[Test]
		public void IconCodec() {
			Guid g = new Guid ("557cf407-1a04-11d3-9a73-0000f81ef32e");
			Check (null, GetDecoder (g), ImageFormat.Bmp.Guid,
				"ICO", null, "*.ICO",
				ImageCodecFlags.Builtin|ImageCodecFlags.Encoder|ImageCodecFlags.SupportBitmap,
				"ICO", "image/x-icon", 1);
		}

	}
}
