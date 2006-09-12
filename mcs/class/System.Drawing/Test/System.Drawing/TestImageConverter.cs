//
// Tests for System.Drawing.ImageConverter.cs 
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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


using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Permissions;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ImageConverterTest 
	{
		Image image;		
		ImageConverter imgConv;
		ImageConverter imgConvFrmTD;
		String imageStr;
		byte [] imageBytes;

		[SetUp]
		public void SetUp ()		
		{
			image = Image.FromFile (TestBitmap.getInFile ("bitmaps/almogaver24bits.bmp"));
			imageStr = image.ToString ();
		
			imgConv = new ImageConverter();
			imgConvFrmTD = (ImageConverter) TypeDescriptor.GetConverter (image);
			
			Stream stream = new FileStream (TestBitmap.getInFile ("bitmaps/almogaver24bits1.bmp"), FileMode.Open);
			int length = (int) stream.Length;
			imageBytes = new byte [length];
 			
			try {
				if (stream.Read (imageBytes, 0, length) != length)
					Assert.Fail ("SU#1: Read Failure"); 
			} catch (Exception e) {
				Assert.Fail ("SU#2 Exception thrown while reading. Exception is: "+e.Message);
			} finally {
				stream.Close ();
			}
		
			stream.Close ();

		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (imgConv.CanConvertFrom (typeof (byte [])), "CCF#1");
			Assert.IsTrue (imgConv.CanConvertFrom (null, typeof (byte [])), "CCF#1a");
			Assert.IsTrue (imgConv.CanConvertFrom (null, imageBytes.GetType ()), "CCF#1b");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (String)), "CCF#2");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (Rectangle)), "CCF#3");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (Point)), "CCF#4");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (PointF)), "CCF#5");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (Size)), "CCF#6");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (SizeF)), "CCF#7");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (Object)), "CCF#8");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (int)), "CCF#9");
			Assert.IsTrue (! imgConv.CanConvertFrom (null, typeof (Metafile)), "CCF#10");

			Assert.IsTrue (imgConvFrmTD.CanConvertFrom (typeof (byte [])), "CCF#1A");
			Assert.IsTrue (imgConvFrmTD.CanConvertFrom (null, typeof (byte [])), "CCF#1aA");
			Assert.IsTrue (imgConvFrmTD.CanConvertFrom (null, imageBytes.GetType ()), "CCF#1bA");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (String)), "CCF#2A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (Rectangle)), "CCF#3A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (Point)), "CCF#4A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (PointF)), "CCF#5A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (Size)), "CCF#6A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (SizeF)), "CCF#7A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (Object)), "CCF#8A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (int)), "CCF#9A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertFrom (null, typeof (Metafile)), "CCF#10A");

		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (imgConv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (imgConv.CanConvertTo (null, typeof (String)), "CCT#1a");
			Assert.IsTrue (imgConv.CanConvertTo (null, imageStr.GetType ()), "CCT#1b");
			Assert.IsTrue (imgConv.CanConvertTo (typeof (byte [])), "CCT#2");
			Assert.IsTrue (imgConv.CanConvertTo (null, typeof (byte [])), "CCT#2a");
			Assert.IsTrue (imgConv.CanConvertTo (null, imageBytes.GetType ()), "CCT#2b");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (Rectangle)), "CCT#3");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (Point)), "CCT#4");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (PointF)), "CCT#5");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (Size)), "CCT#6");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (SizeF)), "CCT#7");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (Object)), "CCT#8");
			Assert.IsTrue (! imgConv.CanConvertTo (null, typeof (int)), "CCT#9");

			Assert.IsTrue (imgConvFrmTD.CanConvertTo (typeof (String)), "CCT#1A");
			Assert.IsTrue (imgConvFrmTD.CanConvertTo (null, typeof (String)), "CCT#1aA");
			Assert.IsTrue (imgConvFrmTD.CanConvertTo (null, imageStr.GetType ()), "CCT#1bA");
			Assert.IsTrue (imgConvFrmTD.CanConvertTo (typeof (byte [])), "CCT#2A");
			Assert.IsTrue (imgConvFrmTD.CanConvertTo (null, typeof (byte [])), "CCT#2aA");
			Assert.IsTrue (imgConvFrmTD.CanConvertTo (null, imageBytes.GetType ()), "CCT#2bA");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (Rectangle)), "CCT#3A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (Point)), "CCT#4A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (PointF)), "CCT#5A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (Size)), "CCT#6A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (SizeF)), "CCT#7A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (Object)), "CCT#8A");
			Assert.IsTrue (! imgConvFrmTD.CanConvertTo (null, typeof (int)), "CCT#9A");

		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void ConvertFrom ()
		{
			Image newImage = (Image) imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, imageBytes);
			
			Assert.AreEqual (image.Height, newImage.Height, "CF#1");
			Assert.AreEqual (image.Width, newImage.Width, "CF#1a");

			Assert.AreEqual ("(none)", imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, null, typeof (string)), "Null/Empty");

			newImage = (Image) imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, imageBytes);

			Assert.AreEqual (image.Height, newImage.Height, "CF#1A");
			Assert.AreEqual (image.Width, newImage.Width, "CF#1aA");
		}
		
		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_BadString ()
		{
			imgConv.ConvertFrom ("System.Drawing.String");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_BadString_WithCulture ()
		{
			imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, "System.Drawing.String");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Bitmap ()
		{
			imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, new Bitmap (20, 20));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Point ()
		{
			imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, new Point (10, 10));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_SizeF ()
		{
			imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, new SizeF (10, 10));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, new Object ());
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_BadString ()
		{
			imgConvFrmTD.ConvertFrom ("System.Drawing.String");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_BadString_Culture ()
		{
			imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, "System.Drawing.String");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_Bitmap ()
		{
			imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, new Bitmap (20, 20));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_Point ()
		{
			imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, new Point (10, 10));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_SizeF ()
		{
			imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, new SizeF (10, 10));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertFrom_Object ()
		{
			imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, new Object ());
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void ConvertTo ()
		{
			Assert.AreEqual (imageStr, (String) imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
				image, typeof (string)), "CT#1");

			Assert.AreEqual (imageStr, (String) imgConv.ConvertTo (image, typeof (string)), "CT#1a");

			Assert.AreEqual (imageStr, (String) imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
				image, typeof (string)), "CT#1A");

			Assert.AreEqual (imageStr, (String) imgConvFrmTD.ConvertTo (image, typeof (string)), "CT#1aA");
		}
				
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ConvertTo_ByteArray ()
		{
			byte[] newImageBytes = (byte[]) imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
				image, imageBytes.GetType ());

			Assert.AreEqual (imageBytes.Length, newImageBytes.Length, "CT#2");

			newImageBytes = (byte[]) imgConv.ConvertTo (image, imageBytes.GetType ());
			
			Assert.AreEqual (imageBytes.Length, newImageBytes.Length, "CT#2a");

			newImageBytes = (byte[]) imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
				image, imageBytes.GetType ());

			Assert.AreEqual (imageBytes.Length, newImageBytes.Length, "CT#2A");

			newImageBytes = (byte[]) imgConvFrmTD.ConvertTo (image, imageBytes.GetType ());
			
			Assert.AreEqual (imageBytes.Length, newImageBytes.Length, "CT#2aA");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Rectangle ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Rectangle));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Image ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, image.GetType ());
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Size ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Size));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Bitmap ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Bitmap));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Point ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Point));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Metafile ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Metafile));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Object ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Object));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_Int ()
		{
			imgConv.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (int));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Rectangle ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Rectangle));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Image ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, image.GetType ());
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Size ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Size));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Bitmap ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Bitmap));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Point ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Point));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Metafile ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Metafile));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Object ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (Object));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeDescriptor_ConvertTo_Int ()
		{
			imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, image, typeof (int));
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void TestGetPropertiesSupported ()
		{
			Assert.IsTrue (imgConv.GetPropertiesSupported (), "GPS#1");
			Assert.IsTrue (imgConv.GetPropertiesSupported (null), "GPS#2");
		}

		[Test]
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		public void TestGetProperties ()
		{
#if NET_2_0
			int basecount = 1;
#else
			int basecount = 0;
#endif
			PropertyDescriptorCollection propsColl;

			propsColl = imgConv.GetProperties (null, image, null);
			Assert.AreEqual (13 + basecount, propsColl.Count, "GP1#1");
			
			propsColl = imgConv.GetProperties (null, image);
			Assert.AreEqual (6 + basecount, propsColl.Count, "GP1#2");

			propsColl = imgConv.GetProperties (image);
			Assert.AreEqual (6 + basecount, propsColl.Count, "GP1#3");

			propsColl = TypeDescriptor.GetProperties (typeof (Image));
			Assert.AreEqual (13 + basecount, propsColl.Count, "GP1#4");
			
			propsColl = imgConvFrmTD.GetProperties (null, image, null);
			Assert.AreEqual (13 + basecount, propsColl.Count, "GP1#1A");
			
			propsColl = imgConvFrmTD.GetProperties (null, image);
			Assert.AreEqual (6 + basecount, propsColl.Count, "GP1#2A");

			propsColl = imgConvFrmTD.GetProperties (image);
			Assert.AreEqual (6 + basecount, propsColl.Count, "GP1#3A");
		}
	}
}
