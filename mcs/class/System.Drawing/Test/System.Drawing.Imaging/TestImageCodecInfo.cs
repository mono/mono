//
// ImageCodecInfo class testing unit
//
// Authors:
// 	Jordi Mas i Hernàndez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ImageCodecInfoTest {

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

			foreach (ImageCodecInfo decoder in ImageCodecInfo.GetImageDecoders ())
				decoders[decoder.Clsid] = decoder;
		
			foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders ())
				encoders[encoder.Clsid] = encoder;
		}

		static void Check (ImageCodecInfo e, ImageCodecInfo d, Guid FormatID, string CodecName, string DllName,
			string FilenameExtension, ImageCodecFlags Flags, string FormatDescription,
			string MimeType, int Version, int signatureLength, string mask, string pattern, string pattern2)
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

				Assert.AreEqual (signatureLength, e.SignatureMasks.Length, "Encoder.SignatureMasks.Length");
				for (int i = 0; i < signatureLength; i++) {
					Assert.AreEqual (mask, BitConverter.ToString (e.SignatureMasks[i]), String.Format ("Encoder.SignatureMasks[{0}]", i));
				}
				Assert.AreEqual (signatureLength, e.SignaturePatterns.Length, "Encoder.SignaturePatterns.Length");
				Assert.AreEqual (pattern, BitConverter.ToString (e.SignaturePatterns[0]), "Encoder.SignaturePatterns[0]");
				if (pattern2 != null)
					Assert.AreEqual (pattern2, BitConverter.ToString (e.SignaturePatterns[1]), "Encoder.SignaturePatterns[1]");
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

				Assert.AreEqual (signatureLength, d.SignatureMasks.Length, "Decoder.SignatureMasks.Length");
				for (int i = 0; i < signatureLength; i++) {
					Assert.AreEqual (mask, BitConverter.ToString (d.SignatureMasks[i]), String.Format ("Decoder.SignatureMasks[{0}]", i));
				}
				Assert.AreEqual (signatureLength, d.SignaturePatterns.Length, "Decoder.SignaturePatterns.Length");
				Assert.AreEqual (pattern, BitConverter.ToString (d.SignaturePatterns[0]), "Decoder.SignaturePatterns[0]");
				if (pattern2 != null)
					Assert.AreEqual (pattern2, BitConverter.ToString (d.SignaturePatterns[1]), "Decoder.SignaturePatterns[1]");
			}
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Decoders ()
		{
			Assert.AreEqual (8, decoders.Count, "Count");
			foreach (DictionaryEntry de in decoders) {
				string guid = de.Key.ToString ();
				switch (guid) {
				case "557cf402-1a04-11d3-9a73-0000f81ef32e": // GIF
				case "557cf403-1a04-11d3-9a73-0000f81ef32e": // EMF
				case "557cf400-1a04-11d3-9a73-0000f81ef32e": // BMP/DIB/RLE
				case "557cf401-1a04-11d3-9a73-0000f81ef32e": // JPG,JPEG,JPE,JFIF
				case "557cf406-1a04-11d3-9a73-0000f81ef32e": // PNG
				case "557cf407-1a04-11d3-9a73-0000f81ef32e": // ICO
				case "557cf404-1a04-11d3-9a73-0000f81ef32e": // WMF
				case "557cf405-1a04-11d3-9a73-0000f81ef32e": // TIF,TIFF
					break;
				default:
					Assert.Ignore ("Unknown decoder " + guid);
					break;
				}
			}
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Encoders ()
		{
			Assert.AreEqual (5, encoders.Count, "Count");
			foreach (DictionaryEntry de in encoders) {
				string guid = de.Key.ToString ();
				switch (guid) {
				case "557cf402-1a04-11d3-9a73-0000f81ef32e": // GIF
				case "557cf400-1a04-11d3-9a73-0000f81ef32e": // BMP/DIB/RLE
				case "557cf401-1a04-11d3-9a73-0000f81ef32e": // JPG,JPEG,JPE,JFIF
				case "557cf406-1a04-11d3-9a73-0000f81ef32e": // PNG
				case "557cf405-1a04-11d3-9a73-0000f81ef32e": // TIF,TIFF
					break;
				default:
					Assert.Ignore ("Unknown encoder " + guid);
					break;
				}
			}
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
				ImageCodecFlags.Builtin | ImageCodecFlags.Encoder | ImageCodecFlags.Decoder | ImageCodecFlags.SupportBitmap,
				"BMP", "image/bmp", 1, 1, "FF-FF", "42-4D", null);
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
				"GIF", "image/gif", 1, 2, "FF-FF-FF-FF-FF-FF", "47-49-46-38-39-61", "47-49-46-38-37-61");
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
				"JPEG", "image/jpeg", 1, 1, "FF-FF", "FF-D8", null);
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
				"PNG", "image/png", 1, 1, "FF-FF-FF-FF-FF-FF-FF-FF", "89-50-4E-47-0D-0A-1A-0A", null);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TiffCodec ()
		{
			Guid g = new Guid ("557cf405-1a04-11d3-9a73-0000f81ef32e");
			Check (GetEncoder (g), GetDecoder (g), ImageFormat.Tiff.Guid,
				"TIFF", null, "*.TIF;*.TIFF",
				ImageCodecFlags.Builtin | ImageCodecFlags.Encoder | ImageCodecFlags.Decoder | ImageCodecFlags.SupportBitmap,
				"TIFF", "image/tiff", 1, 2, "FF-FF", "49-49", "4D-4D");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void IconCodec_Encoder ()
		{
			Guid g = new Guid ("557cf407-1a04-11d3-9a73-0000f81ef32e");
			Assert.IsNull (GetEncoder (g), "Encoder");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void IconCodec_Decoder ()
		{
			Guid g = new Guid ("557cf407-1a04-11d3-9a73-0000f81ef32e");
			Check (null, GetDecoder (g), ImageFormat.Icon.Guid,
				"ICO", null, "*.ICO",
				ImageCodecFlags.Builtin | ImageCodecFlags.Decoder | ImageCodecFlags.SupportBitmap,
				"ICO", "image/x-icon", 1, 1, "FF-FF-FF-FF", "00-00-01-00", null);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void EmfCodec_Encoder ()
		{
			Guid g = new Guid ("557cf403-1a04-11d3-9a73-0000f81ef32e");
			Assert.IsNull (GetEncoder (g), "Encoder");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void EmfCodec_Decoder ()
		{
			Guid g = new Guid ("557cf403-1a04-11d3-9a73-0000f81ef32e");
			Check (null, GetDecoder (g), ImageFormat.Emf.Guid,
				"EMF", null, "*.EMF",
				ImageCodecFlags.Builtin | ImageCodecFlags.Decoder | ImageCodecFlags.SupportBitmap,
				"EMF", "image/x-emf", 1, 1, "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-FF-FF",
				"00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-20-45-4D-46", null);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void WmfCodec_Encoder ()
		{
			Guid g = new Guid ("557cf404-1a04-11d3-9a73-0000f81ef32e");
			Assert.IsNull (GetEncoder (g), "Encoder");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void WmfCodec_Decoder ()
		{
			Guid g = new Guid ("557cf404-1a04-11d3-9a73-0000f81ef32e");
			Check (null, GetDecoder (g), ImageFormat.Wmf.Guid,
				"WMF", null, "*.WMF",
				ImageCodecFlags.Builtin | ImageCodecFlags.Decoder | ImageCodecFlags.SupportBitmap,
				"WMF", "image/x-wmf", 1, 1, "FF-FF-FF-FF", "D7-CD-C6-9A", null);
		}
	}
}
