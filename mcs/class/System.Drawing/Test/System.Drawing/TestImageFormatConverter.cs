//
// Tests for System.Drawing.ImageFormatConverter.cs 
//
// Author:
//	Sanjay Gupta (gsanjay@novell.com)
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
	public class ImageFormatConverterTest
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
			Assert.IsTrue (imgFmtConv.CanConvertFrom (typeof (String)), "CCF#1");
			Assert.IsTrue (imgFmtConv.CanConvertFrom (null, typeof (String)), "CCF#1a");
			Assert.IsTrue (! imgFmtConv.CanConvertFrom (null, typeof (ImageFormat)), "CCF#2");
			Assert.IsTrue (! imgFmtConv.CanConvertFrom (null, typeof (Guid)), "CCF#3");
			Assert.IsTrue (! imgFmtConv.CanConvertFrom (null, typeof (Object)), "CCF#4");
			Assert.IsTrue (! imgFmtConv.CanConvertFrom (null, typeof (int)), "CCF#5");

			Assert.IsTrue (imgFmtConvFrmTD.CanConvertFrom (typeof (String)), "CCF#1A");
			Assert.IsTrue (imgFmtConvFrmTD.CanConvertFrom (null, typeof (String)), "CCF#1aA");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertFrom (null, typeof (ImageFormat)), "CCF#2A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertFrom (null, typeof (Guid)), "CCF#3A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertFrom (null, typeof (Object)), "CCF#4A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertFrom (null, typeof (int)), "CCF#5A");

		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (imgFmtConv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (imgFmtConv.CanConvertTo (null, typeof (String)), "CCT#1a");
			Assert.IsTrue (! imgFmtConv.CanConvertTo (null, typeof (ImageFormat)), "CCT#2");
			Assert.IsTrue (! imgFmtConv.CanConvertTo (null, typeof (Guid)), "CCT#3");
			Assert.IsTrue (! imgFmtConv.CanConvertTo (null, typeof (Object)), "CCT#4");
			Assert.IsTrue (! imgFmtConv.CanConvertTo (null, typeof (int)), "CCT#5");

			Assert.IsTrue (imgFmtConvFrmTD.CanConvertTo (typeof (String)), "CCT#1A");
			Assert.IsTrue (imgFmtConvFrmTD.CanConvertTo (null, typeof (String)), "CCT#1aA");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertTo (null, typeof (ImageFormat)), "CCT#2A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertTo (null, typeof (Guid)), "CCT#3A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertTo (null, typeof (Object)), "CCT#4A");
			Assert.IsTrue (! imgFmtConvFrmTD.CanConvertTo (null, typeof (int)), "CCT#5A");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (imageFmt, (ImageFormat) imgFmtConv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								ImageFormat.Bmp.ToString ()), "CF#1");
			
			try {
				imgFmtConv.ConvertFrom ("System.Drawing.String");
				Assert.Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2");
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Assert.Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2a");
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp);
				Assert.Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#3");
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp.Guid);
				Assert.Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#4");
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Assert.Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#5");
			}

			try {
				imgFmtConv.ConvertFrom (null, CultureInfo.InvariantCulture, 10);
				Assert.Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6");
			}

			
			Assert.AreEqual (imageFmt, (ImageFormat) imgFmtConvFrmTD.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								ImageFormat.Bmp.ToString ()), "CF#1A");
			
			try {
				imgFmtConvFrmTD.ConvertFrom ("System.Drawing.String");
				Assert.Fail ("CF#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2A");
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Assert.Fail ("CF#2aA: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2aA");
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp);
				Assert.Fail ("CF#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#3A");
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   ImageFormat.Bmp.Guid);
				Assert.Fail ("CF#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#4A");
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Assert.Fail ("CF#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#5A");
			}

			try {
				imgFmtConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, 10);
				Assert.Fail ("CF#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6A");
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (imageFmtStr, (String) imgFmtConv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								imageFmt, typeof (String)), "CT#1");

			Assert.AreEqual (imageFmtStr, (String) imgFmtConv.ConvertTo (imageFmt, 
									typeof (String)), "CT#1a");
							
			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 imageFmt, typeof (ImageFormat));
				Assert.Fail ("CT#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#2");
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Guid));
				Assert.Fail ("CT#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#2a");
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Object));
				Assert.Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3");
			}

			try {
				imgFmtConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (int));
				Assert.Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4");
			}


			Assert.AreEqual (imageFmtStr, (String) imgFmtConvFrmTD.ConvertTo (null,
								CultureInfo.InvariantCulture,
								imageFmt, typeof (String)), "CT#1A");

			Assert.AreEqual (imageFmtStr, (String) imgFmtConvFrmTD.ConvertTo (imageFmt, 
									typeof (String)), "CT#1aA");
							
			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, 
						 imageFmt, typeof (ImageFormat));
				Assert.Fail ("CT#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#2A");
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Guid));
				Assert.Fail ("CT#2aA: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#2aA");
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (Object));
				Assert.Fail ("CT#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3A");
			}

			try {
				imgFmtConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 imageFmt, typeof (int));
				Assert.Fail ("CT#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4A");
			}
		}

		
		/*[Test]
		public void TestGetStandardValuesSupported ()
		{
			Assert.IsTrue (imgFmtConv.GetPropertiesSupported (), "GSVS#1");
			Assert.IsTrue (imgFmtConv.GetPropertiesSupported (null), "GSVS#2");
		}

		[Test]
		public void TestGetStandardValues ()
		{			
			//MONO TODO			
		}*/
	}
}
