//
// Tests for System.Drawing.ImageConverter.cs 
//
// Author:
//	Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class ImageConverterTest : Assertion
	{
		Image image;		
		ImageConverter imgConv;
		ImageConverter imgConvFrmTD;
		String imageStr;
		byte [] imageBytes;

		[TearDown]
		public void TearDown () {}

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
					Fail ("SU#1: Read Failure"); 
			} catch (Exception e) {
				Fail ("SU#2 Exception thrown while reading. Exception is: "+e.Message);
			} finally {
				stream.Close ();
			}
		
			stream.Close ();

		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert ("CCF#1", imgConv.CanConvertFrom (typeof (byte [])));
			Assert ("CCF#1a", imgConv.CanConvertFrom (null, typeof (byte [])));
			Assert ("CCF#1b", imgConv.CanConvertFrom (null, imageBytes.GetType ()));
			Assert ("CCF#2", ! imgConv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#3", ! imgConv.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#4", ! imgConv.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5", ! imgConv.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6", ! imgConv.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7", ! imgConv.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8", ! imgConv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9", ! imgConv.CanConvertFrom (null, typeof (int)));
			Assert ("CCF#9", ! imgConv.CanConvertFrom (null, typeof (Metafile)));

			Assert ("CCF#1A", imgConvFrmTD.CanConvertFrom (typeof (byte [])));
			Assert ("CCF#1aA", imgConvFrmTD.CanConvertFrom (null, typeof (byte [])));
			Assert ("CCF#1bA", imgConvFrmTD.CanConvertFrom (null, imageBytes.GetType ()));
			Assert ("CCF#2A", ! imgConvFrmTD.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#3A", ! imgConvFrmTD.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#4A", ! imgConvFrmTD.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5A", ! imgConvFrmTD.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6A", ! imgConvFrmTD.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7A", ! imgConvFrmTD.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8A", ! imgConvFrmTD.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9A", ! imgConvFrmTD.CanConvertFrom (null, typeof (int)));
			Assert ("CCF#9A", ! imgConvFrmTD.CanConvertFrom (null, typeof (Metafile)));

		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", imgConv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", imgConv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#1b", imgConv.CanConvertTo (null, imageStr.GetType ()));
			Assert ("CCT#2", imgConv.CanConvertTo (typeof (byte [])));
			Assert ("CCT#2a", imgConv.CanConvertTo (null, typeof (byte [])));
			Assert ("CCT#2b", imgConv.CanConvertTo (null, imageBytes.GetType ()));
			Assert ("CCT#3", ! imgConv.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#4", ! imgConv.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5", ! imgConv.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6", ! imgConv.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7", ! imgConv.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8", ! imgConv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9", ! imgConv.CanConvertTo (null, typeof (int)));

			Assert ("CCT#1A", imgConvFrmTD.CanConvertTo (typeof (String)));
			Assert ("CCT#1aA", imgConvFrmTD.CanConvertTo (null, typeof (String)));
			Assert ("CCT#1bA", imgConvFrmTD.CanConvertTo (null, imageStr.GetType ()));
			Assert ("CCT#2A", imgConvFrmTD.CanConvertTo (typeof (byte [])));
			Assert ("CCT#2aA", imgConvFrmTD.CanConvertTo (null, typeof (byte [])));
			Assert ("CCT#2bA", imgConvFrmTD.CanConvertTo (null, imageBytes.GetType ()));
			Assert ("CCT#3A", ! imgConvFrmTD.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#4A", ! imgConvFrmTD.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5A", ! imgConvFrmTD.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6A", ! imgConvFrmTD.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7A", ! imgConvFrmTD.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8A", ! imgConvFrmTD.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9A", ! imgConvFrmTD.CanConvertTo (null, typeof (int)));

		}

		[Test]
		public void TestConvertFrom ()
		{
			Image newImage = (Image) imgConv.ConvertFrom (null, CultureInfo.InvariantCulture, imageBytes);
			
			AssertEquals ("CF#1", image.Height, newImage.Height);
			AssertEquals ("CF#1a", image.Width, newImage.Width);
			
			try {
				imgConv.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2", e is NotSupportedException);
			}

			try {
				imgConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2a", e is NotSupportedException);
			}

			try {
				imgConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3", e is NotSupportedException);
			}

			try {
				imgConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4", e is NotSupportedException);
			}

			try {
				imgConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5", e is NotSupportedException);
			}

			try {
				imgConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}


			newImage = (Image) imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, imageBytes);

			AssertEquals ("CF#1A", image.Height, newImage.Height);
			AssertEquals ("CF#1aA", image.Width, newImage.Width);
			
			
			try {
				imgConvFrmTD.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2aA: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2aA", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Fail ("CF#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Fail ("CF#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Fail ("CF#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6A", e is NotSupportedException);
			}

		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", imageStr, (String) imgConv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								image, typeof (String)));

			AssertEquals ("CT#1a", imageStr, (String) imgConv.ConvertTo (image, 
									typeof (String)));
				
			byte [] newImageBytes = (byte []) imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
											image, imageBytes.GetType ());

			AssertEquals ("CT#2", imageBytes.Length, newImageBytes.Length);

			newImageBytes = (byte []) imgConv.ConvertTo (image, imageBytes.GetType ());
			
			AssertEquals ("CT#2a", imageBytes.Length, newImageBytes.Length);

			
			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 image, typeof (Rectangle));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, image.GetType ());
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Size));
				Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Bitmap));
				Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Point));
				Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Metafile));
				Fail ("CT#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#8", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Object));
				Fail ("CT#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#9", e is NotSupportedException);
			}

			try {
				imgConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (int));
				Fail ("CT#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#10", e is NotSupportedException);
			}


			AssertEquals ("CT#1A", imageStr, (String) imgConvFrmTD.ConvertTo (null,
								CultureInfo.InvariantCulture,
								image, typeof (String)));

			AssertEquals ("CT#1aA", imageStr, (String) imgConvFrmTD.ConvertTo (image, 
									typeof (String)));
				
			newImageBytes = (byte []) imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
											image, imageBytes.GetType ());

			AssertEquals ("CT#2A", imageBytes.Length, newImageBytes.Length);

			newImageBytes = (byte []) imgConvFrmTD.ConvertTo (image, imageBytes.GetType ());
			
			AssertEquals ("CT#2aA", imageBytes.Length, newImageBytes.Length);
			
			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, 
						 image, typeof (Rectangle));
				Fail ("CT#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, image.GetType ());
				Fail ("CT#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Size));
				Fail ("CT#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Bitmap));
				Fail ("CT#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Point));
				Fail ("CT#7A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Metafile));
				Fail ("CT#8A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#8A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (Object));
				Fail ("CT#9A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#9A", e is NotSupportedException);
			}

			try {
				imgConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 image, typeof (int));
				Fail ("CT#10A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#10A", e is NotSupportedException);
			}
		}

		
		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert ("GPS#1", imgConv.GetPropertiesSupported ());
			Assert ("GPS#2", imgConv.GetPropertiesSupported (null));
		}

		[Test]
		[Ignore ("This test fails because of bug #58435")]
		public void TestGetProperties ()
		{
			PropertyDescriptorCollection propsColl;

			propsColl = imgConv.GetProperties (null, image, null);
			AssertEquals ("GP1#1", 13, propsColl.Count);
			
			propsColl = imgConv.GetProperties (null, image);
			AssertEquals ("GP1#2", 6, propsColl.Count);

			propsColl = imgConv.GetProperties (image);
			AssertEquals ("GP1#3", 6, propsColl.Count);

			propsColl = TypeDescriptor.GetProperties (typeof (Image));
			AssertEquals ("GP1#4", 13, propsColl.Count);
			
			propsColl = imgConvFrmTD.GetProperties (null, image, null);
			AssertEquals ("GP1#1A", 13, propsColl.Count);
			
			propsColl = imgConvFrmTD.GetProperties (null, image);
			AssertEquals ("GP1#2A", 6, propsColl.Count);

			propsColl = imgConvFrmTD.GetProperties (image);
			AssertEquals ("GP1#3A", 6, propsColl.Count);
			
		}
	}
}
