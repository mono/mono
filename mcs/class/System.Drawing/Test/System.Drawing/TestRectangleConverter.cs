//
// Tests for System.Drawing.RectangleConverter.cs 
//
// Author:
//	Ravindra (rkumar@novell.com)
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
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class RectangleConverterTest : Assertion
	{
		Rectangle rect;
		Rectangle rectneg;
		RectangleConverter rconv;
		String rectStr;
		String rectnegStr;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			rect = new Rectangle (10, 10, 20, 30);
			rectStr = rect.X + ", " + rect.Y + ", " + rect.Width + ", " + rect.Height;

			rectneg = new Rectangle (-10, -10, 20, 30);
			rectnegStr = rectneg.X + ", " + rectneg.Y + ", " + rectneg.Width + ", " + rectneg.Height;

			rconv = (RectangleConverter) TypeDescriptor.GetConverter (rect);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert ("CCF#1", rconv.CanConvertFrom (typeof (String)));
			Assert ("CCF#1a", rconv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#2", ! rconv.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#3", ! rconv.CanConvertFrom (null, typeof (RectangleF)));
			Assert ("CCF#4", ! rconv.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5", ! rconv.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6", ! rconv.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7", ! rconv.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8", ! rconv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9", ! rconv.CanConvertFrom (null, typeof (int)));
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", rconv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", rconv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#2", ! rconv.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#3", ! rconv.CanConvertTo (null, typeof (RectangleF)));
			Assert ("CCT#4", ! rconv.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5", ! rconv.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6", ! rconv.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7", ! rconv.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8", ! rconv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9", ! rconv.CanConvertTo (null, typeof (int)));
		}

		[Test]
		public void TestConvertFrom ()
		{
			AssertEquals ("CF#1", rect, (Rectangle) rconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"10, 10, 20, 30"));
			AssertEquals ("CF#2", rectneg, (Rectangle) rconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"-10, -10, 20, 30"));

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture, 
						   "10, 10");
				Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3", e is ArgumentException);
			}

			try {
				rconv.ConvertFrom ("10");
				Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3a", e is ArgumentException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "1, 1, 1, 1, 1");
				Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#4", e is ArgumentException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "*1, 1, 1, 1");
				Fail ("CF#5: must throw Exception");
			} catch {
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Rectangle (10, 10, 100, 100));
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new RectangleF (10, 10, 100, 100));
				Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#7", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#8", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new PointF (10, 10));
				Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#9", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Size (10, 10));
				Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#10", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Fail ("CF#11: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#11", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Fail ("CF#12: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#12", e is NotSupportedException);
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture, 1001);
				Fail ("CF#13: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#13", e is NotSupportedException);
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", rectStr, (String) rconv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								rect, typeof (String)));
			AssertEquals ("CT#2", rectnegStr, (String) rconv.ConvertTo (null, 
								CultureInfo.InvariantCulture,
								rectneg, typeof (String)));

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 rect, typeof (Rectangle));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (RectangleF));
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Size));
				Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (SizeF));
				Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Point));
				Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (PointF));
				Fail ("CT#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#8", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Object));
				Fail ("CT#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#9", e is NotSupportedException);
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (int));
				Fail ("CT#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#10", e is NotSupportedException);
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert ("GCIS#1", rconv.GetCreateInstanceSupported ());
			Assert ("GCIS#2", rconv.GetCreateInstanceSupported (null));
		}

		[Test]
		public void TestCreateInstance ()
		{
			Rectangle rectInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("X", 10); ht.Add ("Y", 10);
			ht.Add ("Width", 20); ht.Add ("Height", 30);

			rectInstance = (Rectangle) rconv.CreateInstance (ht);
			AssertEquals ("CI#1", rect, rectInstance);

			ht.Clear ();
			ht.Add ("X", -10); ht.Add ("Y", -10);
			ht.Add ("Width", 20); ht.Add ("Height", 30);

			rectInstance = (Rectangle) rconv.CreateInstance (null, ht);
			AssertEquals ("CI#2", rectneg, rectInstance);

			// Property names are case-sensitive. It should throw 
			// NullRefExc if any of the property names does not match
			ht.Clear ();
			ht.Add ("x", -10); ht.Add ("Y", -10);
			ht.Add ("Width", 20); ht.Add ("Height", 30);
			try {
				rectInstance = (Rectangle) rconv.CreateInstance (null, ht);
				Fail ("CI#3: must throw NullReferenceException");
			} catch (Exception e) {
				Assert ("CI#3", e is NullReferenceException);
			}
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert ("GPS#1", rconv.GetPropertiesSupported ());
			Assert ("GPS#2", rconv.GetPropertiesSupported (null));
		}

		[Test]
		[Ignore ("This test fails because of bug #58435")]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = rconv.GetProperties (rect);
			AssertEquals ("GP1#1", 4, propsColl.Count);
			AssertEquals ("GP1#2", rect.X, propsColl ["X"].GetValue (rect));
			AssertEquals ("GP1#3", rect.Y, propsColl ["Y"].GetValue (rect));
			AssertEquals ("GP1#4", rect.Width, propsColl ["Width"].GetValue (rect));
			AssertEquals ("GP1#5", rect.Height, propsColl ["Height"].GetValue (rect));

			propsColl = rconv.GetProperties (null, rectneg);
			AssertEquals ("GP2#1", 4, propsColl.Count);
			AssertEquals ("GP2#2", rectneg.X, propsColl ["X"].GetValue (rectneg));
			AssertEquals ("GP2#3", rectneg.Y, propsColl ["Y"].GetValue (rectneg));
			AssertEquals ("GP2#4", rectneg.Width, propsColl ["Width"].GetValue (rectneg));
			AssertEquals ("GP2#5", rectneg.Height, propsColl ["Height"].GetValue (rectneg));

			propsColl = rconv.GetProperties (null, rect, null);
			AssertEquals ("GP3#1", 11, propsColl.Count);
			AssertEquals ("GP3#2", rect.X, propsColl ["X"].GetValue (rect));
			AssertEquals ("GP3#3", rect.Y, propsColl ["Y"].GetValue (rect));
			AssertEquals ("GP3#4", rect.Width, propsColl ["Width"].GetValue (rect));
			AssertEquals ("GP3#5", rect.Height, propsColl ["Height"].GetValue (rect));

			AssertEquals ("GP3#6", rect.Top, propsColl ["Top"].GetValue (rect));
			AssertEquals ("GP3#7", rect.Bottom, propsColl ["Bottom"].GetValue (rect));
			AssertEquals ("GP3#8", rect.Left, propsColl ["Left"].GetValue (rect));
			AssertEquals ("GP3#9", rect.Right, propsColl ["Right"].GetValue (rect));
			AssertEquals ("GP3#10", rect.Location, propsColl ["Location"].GetValue (rect));
			AssertEquals ("GP3#11", rect.Size, propsColl ["Size"].GetValue (rect));
			AssertEquals ("GP3#12", rect.IsEmpty, propsColl ["IsEmpty"].GetValue (rect));

			Type type = typeof (Rectangle);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = rconv.GetProperties (null, rect, attrs);
			AssertEquals ("GP3#13", 0, propsColl.Count);
		}
	}
}
