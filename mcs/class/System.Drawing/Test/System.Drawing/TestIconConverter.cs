//
// Tests for System.Drawing.IconConverter.cs 
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
	public class IconConverterTest : Assertion
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
			icon = new Icon (TestBitmap.getInFile ("bitmaps/VisualPng.ico"));
			iconStr = icon.ToString ();
		
			icoConv = new IconConverter();
			icoConvFrmTD = (IconConverter) TypeDescriptor.GetConverter (icon);
			
			Stream stream = new FileStream (TestBitmap.getInFile ("bitmaps/VisualPng1.ico"), FileMode.Open);
			int length = (int) stream.Length;
			iconBytes = new byte [length];
 			
			try {
				if (stream.Read (iconBytes, 0, length) != length)
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
			Assert ("CCF#1", icoConv.CanConvertFrom (typeof (byte [])));
			Assert ("CCF#1a", icoConv.CanConvertFrom (null, typeof (byte [])));
			Assert ("CCF#1b", icoConv.CanConvertFrom (null, iconBytes.GetType ()));
			Assert ("CCF#2", ! icoConv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#3", ! icoConv.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#4", ! icoConv.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5", ! icoConv.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6", ! icoConv.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7", ! icoConv.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8", ! icoConv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9", ! icoConv.CanConvertFrom (null, typeof (int)));
			Assert ("CCF#9", ! icoConv.CanConvertFrom (null, typeof (Metafile)));

			Assert ("CCF#1A", icoConvFrmTD.CanConvertFrom (typeof (byte [])));
			Assert ("CCF#1aA", icoConvFrmTD.CanConvertFrom (null, typeof (byte [])));
			Assert ("CCF#1bA", icoConvFrmTD.CanConvertFrom (null, iconBytes.GetType ()));
			Assert ("CCF#2A", ! icoConvFrmTD.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#3A", ! icoConvFrmTD.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#4A", ! icoConvFrmTD.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5A", ! icoConvFrmTD.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6A", ! icoConvFrmTD.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7A", ! icoConvFrmTD.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8A", ! icoConvFrmTD.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9A", ! icoConvFrmTD.CanConvertFrom (null, typeof (int)));
			Assert ("CCF#9A", ! icoConvFrmTD.CanConvertFrom (null, typeof (Metafile)));

		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", icoConv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", icoConv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#1b", icoConv.CanConvertTo (null, iconStr.GetType ()));
			Assert ("CCT#2", icoConv.CanConvertTo (typeof (byte [])));
			Assert ("CCT#2a", icoConv.CanConvertTo (null, typeof (byte [])));
			Assert ("CCT#2b", icoConv.CanConvertTo (null, iconBytes.GetType ()));
			Assert ("CCT#3", ! icoConv.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#4", ! icoConv.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5", ! icoConv.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6", ! icoConv.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7", ! icoConv.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8", ! icoConv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9", ! icoConv.CanConvertTo (null, typeof (int)));

			Assert ("CCT#1A", icoConvFrmTD.CanConvertTo (typeof (String)));
			Assert ("CCT#1aA", icoConvFrmTD.CanConvertTo (null, typeof (String)));
			Assert ("CCT#1bA", icoConvFrmTD.CanConvertTo (null, iconStr.GetType ()));
			Assert ("CCT#2A", icoConvFrmTD.CanConvertTo (typeof (byte [])));
			Assert ("CCT#2aA", icoConvFrmTD.CanConvertTo (null, typeof (byte [])));
			Assert ("CCT#2bA", icoConvFrmTD.CanConvertTo (null, iconBytes.GetType ()));
			Assert ("CCT#3A", ! icoConvFrmTD.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#4A", ! icoConvFrmTD.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5A", ! icoConvFrmTD.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6A", ! icoConvFrmTD.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7A", ! icoConvFrmTD.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8A", ! icoConvFrmTD.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9A", ! icoConvFrmTD.CanConvertTo (null, typeof (int)));

		}

		[Test]
		public void TestConvertFrom ()
		{
			Icon newIcon = (Icon) icoConv.ConvertFrom (null, CultureInfo.InvariantCulture, iconBytes);

			AssertEquals ("CF#1", icon.Height, newIcon.Height );
			AssertEquals ("CF#1a", icon.Width, newIcon.Width );
			
			try {
				icoConv.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2", e is NotSupportedException);
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2a: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2a", e is NotSupportedException);
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Fail ("CF#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3", e is NotSupportedException);
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Fail ("CF#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4", e is NotSupportedException);
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Fail ("CF#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5", e is NotSupportedException);
			}

			try {
				icoConv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}


			newIcon = (Icon) icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture, iconBytes);

			AssertEquals ("CF#1A", icon.Height, newIcon.Height);
			AssertEquals ("CF#1Aa", icon.Width, newIcon.Width);
			
			try {
				icoConvFrmTD.ConvertFrom ("System.Drawing.String");
				Fail ("CF#2A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "System.Drawing.String");
				Fail ("CF#2aA: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#2aA", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Bitmap (20, 20));
				Fail ("CF#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#3A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Fail ("CF#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#4A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Fail ("CF#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#5A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6A", e is NotSupportedException);
			}

		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", iconStr, (String) icoConv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								icon, typeof (String)));

			AssertEquals ("CT#1a", iconStr, (String) icoConv.ConvertTo (icon, 
									typeof (String)));
				
			byte [] newIconBytes = (byte []) icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
											icon, iconBytes.GetType ());
		
			AssertEquals ("CT#2", iconBytes.Length, newIconBytes.Length);

			newIconBytes = (byte []) icoConv.ConvertTo (icon, iconBytes.GetType ());
			
			AssertEquals ("CT#2a", iconBytes.Length, newIconBytes.Length);

			
			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 icon, typeof (Rectangle));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, icon.GetType ());
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Size));
				Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Bitmap));
				Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Point));
				Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Metafile));
				Fail ("CT#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#8", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Object));
				Fail ("CT#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#9", e is NotSupportedException);
			}

			try {
				icoConv.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (int));
				Fail ("CT#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#10", e is NotSupportedException);
			}


			AssertEquals ("CT#1A", iconStr, (String) icoConvFrmTD.ConvertTo (null,
								CultureInfo.InvariantCulture,
								icon, typeof (String)));

			AssertEquals ("CT#1aA", iconStr, (String) icoConvFrmTD.ConvertTo (icon, 
									typeof (String)));
				
			newIconBytes = (byte []) icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
											icon, iconBytes.GetType ());
		
			AssertEquals ("CT#2A", iconBytes.Length, newIconBytes.Length);

			newIconBytes = (byte []) icoConvFrmTD.ConvertTo (icon, iconBytes.GetType ());
			
			AssertEquals ("CT#2aA", iconBytes.Length, newIconBytes.Length);

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture, 
						 icon, typeof (Rectangle));
				Fail ("CT#3A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, icon.GetType ());
				Fail ("CT#4A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Size));
				Fail ("CT#5A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Bitmap));
				Fail ("CT#6A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Point));
				Fail ("CT#7A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Metafile));
				Fail ("CT#8A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#8A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (Object));
				Fail ("CT#9A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#9A", e is NotSupportedException);
			}

			try {
				icoConvFrmTD.ConvertTo (null, CultureInfo.InvariantCulture,
						 icon, typeof (int));
				Fail ("CT#10A: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#10A", e is NotSupportedException);
			}
		}
				
	}
}
