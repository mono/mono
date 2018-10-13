//
// Tests for System.Drawing.IconConverter.cs 
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

using MonoTests.Helpers;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class IconConverterTest
	{
		Icon icon;		
		IconConverter icoConv;
		IconConverter icoConvFrmTD;
		String iconStr;
		byte [] iconBytes;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			icon = new Icon (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/VisualPng.ico"));
			iconStr = icon.ToString ();
		
			icoConv = new IconConverter();
			icoConvFrmTD = (IconConverter) TypeDescriptor.GetConverter (icon);
			
			Stream stream = new FileStream (TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/VisualPng1.ico"), FileMode.Open);
			int length = (int) stream.Length;
			iconBytes = new byte [length];
 			
			try {
				if (stream.Read (iconBytes, 0, length) != length)
					Assert.Fail ("SU#1: Read Failure"); 
			} catch (Exception e) {
				Assert.Fail ("SU#2 Exception thrown while reading. Exception is: "+e.Message);
			} finally {
				stream.Close ();
			}
		
			stream.Close ();

		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (icoConv.CanConvertFrom (typeof (byte [])), "CCF#1");
			Assert.IsTrue (icoConv.CanConvertFrom (null, typeof (byte [])), "CCF#1a");
			Assert.IsTrue (icoConv.CanConvertFrom (null, iconBytes.GetType ()), "CCF#1b");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (String)), "CCF#2");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (Rectangle)), "CCF#3");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (Point)), "CCF#4");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (PointF)), "CCF#5");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (Size)), "CCF#6");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (SizeF)), "CCF#7");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (Object)), "CCF#8");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (int)), "CCF#9");
			Assert.IsTrue (! icoConv.CanConvertFrom (null, typeof (Metafile)), "CCF#10");

			Assert.IsTrue (icoConvFrmTD.CanConvertFrom (typeof (byte [])), "CCF#1A");
			Assert.IsTrue (icoConvFrmTD.CanConvertFrom (null, typeof (byte [])), "CCF#1aA");
			Assert.IsTrue (icoConvFrmTD.CanConvertFrom (null, iconBytes.GetType ()), "CCF#1bA");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (String)), "CCF#2A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (Rectangle)), "CCF#3A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (Point)), "CCF#4A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (PointF)), "CCF#5A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (Size)), "CCF#6A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (SizeF)), "CCF#7A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (Object)), "CCF#8A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (int)), "CCF#9A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertFrom (null, typeof (Metafile)), "CCF#10A");

		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (icoConv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (icoConv.CanConvertTo (null, typeof (String)), "CCT#1a");
			Assert.IsTrue (icoConv.CanConvertTo (null, iconStr.GetType ()), "CCT#1b");
			Assert.IsTrue (icoConv.CanConvertTo (typeof (byte [])), "CCT#2");
			Assert.IsTrue (icoConv.CanConvertTo (null, typeof (byte [])), "CCT#2a");
			Assert.IsTrue (icoConv.CanConvertTo (null, iconBytes.GetType ()), "CCT#2b");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (Rectangle)), "CCT#3");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (Point)), "CCT#4");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (PointF)), "CCT#5");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (Size)), "CCT#6");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (SizeF)), "CCT#7");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (Object)), "CCT#8");
			Assert.IsTrue (! icoConv.CanConvertTo (null, typeof (int)), "CCT#9");

			Assert.IsTrue (icoConvFrmTD.CanConvertTo (typeof (String)), "CCT#1A");
			Assert.IsTrue (icoConvFrmTD.CanConvertTo (null, typeof (String)), "CCT#1aA");
			Assert.IsTrue (icoConvFrmTD.CanConvertTo (null, iconStr.GetType ()), "CCT#1bA");
			Assert.IsTrue (icoConvFrmTD.CanConvertTo (typeof (byte [])), "CCT#2A");
			Assert.IsTrue (icoConvFrmTD.CanConvertTo (null, typeof (byte [])), "CCT#2aA");
			Assert.IsTrue (icoConvFrmTD.CanConvertTo (null, iconBytes.GetType ()), "CCT#2bA");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (Rectangle)), "CCT#3A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (Point)), "CCT#4A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (PointF)), "CCT#5A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (Size)), "CCT#6A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (SizeF)), "CCT#7A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (Object)), "CCT#8A");
			Assert.IsTrue (! icoConvFrmTD.CanConvertTo (null, typeof (int)), "CCT#9A");

		}

		[Test]
		public void TestConvertFrom ()
		{
			Icon newIcon = (Icon) icoConv.ConvertFrom (null, CultureInfo.InvariantCulture, iconBytes);

			Assert.AreEqual (icon.Height, newIcon.Height, "CF#1");
			Assert.AreEqual (icon.Width, newIcon.Width, "CF#1a" );
			
			try {
				icoConv.ConvertFrom ("System.Drawing.String");
				Assert.Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2");
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Assert.Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2a");
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Assert.Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#3");
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Assert.Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#4");
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Assert.Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#5");
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Assert.Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6");
			}


			newIcon = (Icon) icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, iconBytes);

			Assert.AreEqual (icon.Height, newIcon.Height, "CF#1A");
			Assert.AreEqual (icon.Width, newIcon.Width, "CF#1Aa");
			
			try {
				icoConvFrmTD.ConvertFrom ("System.Drawing.String");
				Assert.Fail ("CF#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2A");
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Assert.Fail ("CF#2aA: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#2aA");
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Assert.Fail ("CF#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#3A");
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Assert.Fail ("CF#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#4A");
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Assert.Fail ("CF#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#5A");
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Assert.Fail ("CF#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6A");
			}

		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (iconStr, (String) icoConv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								icon, typeof (String)), "CT#1");

			Assert.AreEqual (iconStr, (String) icoConv.ConvertTo (icon, 
									typeof (String)), "CT#1a");
				
			/*byte [] newIconBytes = (byte []) icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
											icon, iconBytes.GetType ());
		
			Assert.AreEqual (iconBytes.Length, newIconBytes.Length, "CT#2");

			newIconBytes = (byte []) icoConv.ConvertTo (icon, iconBytes.GetType ());
			
			Assert.AreEqual (iconBytes.Length, newIconBytes.Length, "CT#2a");

						
			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 icon, typeof (Rectangle));
				Assert.Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue ( e is NotSupportedException, "CT#3");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, icon.GetType ());
				Assert.Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Size));
				Assert.Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#5");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Bitmap));
				Assert.Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue ( e is NotSupportedException, "CT#6");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Point));
				Assert.Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#7");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Metafile));
				Assert.Fail ("CT#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#8");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Object));
				Assert.Fail ("CT#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#9");
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (int));
				Assert.Fail ("CT#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#10");
			}*/


			Assert.AreEqual (iconStr, (String) icoConvFrmTD.ConvertTo (null,
								CultureInfo.InvariantCulture,
								icon, typeof (String)), "CT#1A");

			Assert.AreEqual (iconStr, (String) icoConvFrmTD.ConvertTo (icon, 
									typeof (String)), "CT#1aA");
				
			/*newIconBytes = (byte []) icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
											icon, iconBytes.GetType ());
		
			Assert.AreEqual (iconBytes.Length, newIconBytes.Length, "CT#2A");

			newIconBytes = (byte []) icoConvFrmTD.ConvertTo (icon, iconBytes.GetType ());
			
			Assert.AreEqual (iconBytes.Length, newIconBytes.Length, "CT#2aA");
			
			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, 
						 icon, typeof (Rectangle));
				Assert.Fail ("CT#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, icon.GetType ());
				Assert.Fail ("CT#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Size));
				Assert.Fail ("CT#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#5A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Bitmap));
				Assert.Fail ("CT#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#6A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Point));
				Assert.Fail ("CT#7A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#7A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Metafile));
				Assert.Fail ("CT#8A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#8A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Object));
				Assert.Fail ("CT#9A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#9A");
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (int));
				Assert.Fail ("CT#10A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#10A");
			}*/

			Assert.AreEqual ("(none)", (string) icoConv.ConvertTo (null, typeof (string)), "CT#2");
		}
				
	}
}
