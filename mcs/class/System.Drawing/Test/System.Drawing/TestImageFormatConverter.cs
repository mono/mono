//
// Tests for System.Drawing.ImageFormatConverter.cs 
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
	public class ImageFormatConverterTest : Assertion
	{
		ImageFormat imageFmt;		
		ImageFormatConverter imgFmtConv;
		ImageFormatConverter imgFmtConvFrmTD;
		String imageFmtStr;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			imageFmt = ImageFormat.Bmp; 
			imageFmtStr = imageFmt.ToString ();
		
			imgFmtConv = new ImageFormatConverter();
			imgFmtConvFrmTD = (ImageFormatConverter) TypeDescriptor.GetConverter (imageFmt);			
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert ("CCF#1", imgFmtConv.CanConvertFrom (typeof (String)));
			Assert ("CCF#1a", imgFmtConv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#2", ! imgFmtConv.CanConvertFrom (null, typeof (ImageFormat)));
			Assert ("CCF#3", ! imgFmtConv.CanConvertFrom (null, typeof (Guid)));
			Assert ("CCF#4", ! imgFmtConv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#5", ! imgFmtConv.CanConvertFrom (null, typeof (int)));

			Assert ("CCF#1A", imgFmtConvFrmTD.CanConvertFrom (typeof (String)));
			Assert ("CCF#1aA", imgFmtConvFrmTD.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#2A", ! imgFmtConvFrmTD.CanConvertFrom (null, typeof (ImageFormat)));
			Assert ("CCF#3A", ! imgFmtConvFrmTD.CanConvertFrom (null, typeof (Guid)));
			Assert ("CCF#4A", ! imgFmtConvFrmTD.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#5A", ! imgFmtConvFrmTD.CanConvertFrom (null, typeof (int)));

		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", imgFmtConv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", imgFmtConv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#2", ! imgFmtConv.CanConvertTo (null, typeof (ImageFormat)));
			Assert ("CCT#3", ! imgFmtConv.CanConvertTo (null, typeof (Guid)));
			Assert ("CCT#4", ! imgFmtConv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#5", ! imgFmtConv.CanConvertTo (null, typeof (int)));

			Assert ("CCT#1A", imgFmtConvFrmTD.CanConvertTo (typeof (String)));
			Assert ("CCT#1aA", imgFmtConvFrmTD.CanConvertTo (null, typeof (String)));
			Assert ("CCT#2A", ! imgFmtConvFrmTD.CanConvertTo (null, typeof (ImageFormat)));
			Assert ("CCT#3A", ! imgFmtConvFrmTD.CanConvertTo (null, typeof (Guid)));
			Assert ("CCT#4A", ! imgFmtConvFrmTD.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#5A", ! imgFmtConvFrmTD.CanConvertTo (null, typeof (int)));
		}

		[Test]
		public void TestConvertFrom ()
		{
			AssertEquals ("CF#1", imageFmtStr, (String) imgFmtConv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								ImageFormat.Bmp.ToString ()));
			
			try {
				imgFmtConv.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2a", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp);
				Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp.Guid);
				Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture, 10);
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}

			
			AssertEquals ("CF#1", imageFmtStr, (String) imgFmtConvFrmTD.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								ImageFormat.Bmp.ToString ()));
			
			try {
				imgFmtConvFrmTD.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2a", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp);
				Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp.Guid);
				Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, 10);
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", imageFmtStr, (String) imgFmtConv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								imageFmt, typeof (String)));

			AssertEquals ("CT#1a", imageFmtStr, (String) imgFmtConv.ConvertTo (imageFmt, 
									typeof (String)));
							
			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 imageFmt, typeof (ImageFormat));
				Fail ("CT#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#2", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Guid));
				Fail ("CT#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#2", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Object));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (int));
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}


			AssertEquals ("CT#1A", imageFmtStr, (String) imgFmtConvFrmTD.ConvertTo (null,
								CultureInfo.InvariantCulture,
								imageFmt, typeof (String)));

			AssertEquals ("CT#1aA", imageFmtStr, (String) imgFmtConvFrmTD.ConvertTo (imageFmt, 
									typeof (String)));
							
			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, 
						 imageFmt, typeof (ImageFormat));
				Fail ("CT#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#2A", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Guid));
				Fail ("CT#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#2A", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Object));
				Fail ("CT#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3A", e is NotSupportedException);
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (int));
				Fail ("CT#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4A", e is NotSupportedException);
			}
		}

		
		/*[Test]
		public void TestGetStandardValuesSupported ()
		{
			Assert ("GSVS#1", imgFmtConv.GetPropertiesSupported ());
			Assert ("GSVS#2", imgFmtConv.GetPropertiesSupported (null));
		}

		[Test]
		public void TestGetStandardValues ()
		{			
			//MONO TODO			
		}*/
	}
}
