//
// Tests for System.Drawing.SizeConverter.cs 
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
	public class SizeConverterTest : Assertion
	{
		Size sz;
		Size szneg;
		SizeConverter szconv;
		String szStr;
		String sznegStr;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			sz = new Size (10, 20);
			szStr = sz.Width + ", " + sz.Height;

			szneg = new Size (-20, -30);
			sznegStr = szneg.Width + ", " + szneg.Height;

			szconv = (SizeConverter) TypeDescriptor.GetConverter (sz);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert ("CCF#1", szconv.CanConvertFrom (typeof (String)));
			Assert ("CCF#1a", szconv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#2", ! szconv.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#3", ! szconv.CanConvertFrom (null, typeof (RectangleF)));
			Assert ("CCF#4", ! szconv.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5", ! szconv.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6", ! szconv.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7", ! szconv.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8", ! szconv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9", ! szconv.CanConvertFrom (null, typeof (int)));
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", szconv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", szconv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#2", ! szconv.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#3", ! szconv.CanConvertTo (null, typeof (RectangleF)));
			Assert ("CCT#4", ! szconv.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5", ! szconv.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6", ! szconv.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7", ! szconv.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8", ! szconv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9", ! szconv.CanConvertTo (null, typeof (int)));
		}

		[Test]
		public void TestConvertFrom ()
		{
			AssertEquals ("CF#1", sz, (Size) szconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"10, 20"));
			AssertEquals ("CF#2", szneg, (Size) szconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"-20, -30"));

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture, "10");
				Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3", e is ArgumentException);
			}

			try {
				szconv.ConvertFrom ("10");
				Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3a", e is ArgumentException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    "1, 1, 1");
				Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#4", e is ArgumentException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    "*1, 1");
				Fail ("CF#5: must throw Exception");
			} catch {
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new Point (10, 10));
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new PointF (10, 10));
				Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#7", e is NotSupportedException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new Size (10, 10));
				Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#8", e is NotSupportedException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new SizeF (10, 10));
				Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#9", e is NotSupportedException);
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture, 0x10);
				Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#10", e is NotSupportedException);
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", szStr, (String) szconv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								sz, typeof (String)));
			AssertEquals ("CT#2", sznegStr, (String) szconv.ConvertTo (null,
							CultureInfo.InvariantCulture, szneg, 
							typeof (String)));

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
						  typeof (Size));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
						  typeof (SizeF));
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
						  typeof (Point));
				Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5", e is NotSupportedException);
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
						  typeof (PointF));
				Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6", e is NotSupportedException);
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
						  typeof (int));
				Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7", e is NotSupportedException);
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert ("GCIS#1", szconv.GetCreateInstanceSupported ());
			Assert ("GCIS#2", szconv.GetCreateInstanceSupported (null));
		}

		[Test]
		public void TestCreateInstance ()
		{
			Size szInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("Width", 10); ht.Add ("Height", 20);

			szInstance = (Size) szconv.CreateInstance (ht);
			AssertEquals ("CI#1", sz, szInstance);

			ht.Clear ();
			ht.Add ("Width", -20); ht.Add ("Height", -30);

			szInstance = (Size) szconv.CreateInstance (null, ht);
			AssertEquals ("CI#2", szneg, szInstance);

			// Property names are case-sensitive. It should throw 
			// NullRefExc if any of the property names does not match
			ht.Clear ();
			ht.Add ("width", 20); ht.Add ("Height", 30);
			try {
				szInstance = (Size) szconv.CreateInstance (null, ht);
				Fail ("CI#3: must throw NullReferenceException");
			} catch (Exception e) {
				Assert ("CI#3", e is NullReferenceException);
			}
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert ("GPS#1", szconv.GetPropertiesSupported ());
			Assert ("GPS#2", szconv.GetPropertiesSupported (null));
		}

		[Test]
		[Ignore ("This test fails because of bug #58435")]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = szconv.GetProperties (sz);
			AssertEquals ("GP1#1", 2, propsColl.Count);
			AssertEquals ("GP1#2", sz.Width, propsColl ["Width"].GetValue (sz));
			AssertEquals ("GP1#3", sz.Height, propsColl ["Height"].GetValue (sz));

			propsColl = szconv.GetProperties (null, szneg);
			AssertEquals ("GP2#1", 2, propsColl.Count);
			AssertEquals ("GP2#2", szneg.Width, propsColl ["Width"].GetValue (szneg));
			AssertEquals ("GP2#3", szneg.Height, propsColl ["Height"].GetValue (szneg));

			propsColl = szconv.GetProperties (null, sz, null);
			AssertEquals ("GP3#1", 3, propsColl.Count);
			AssertEquals ("GP3#2", sz.Width, propsColl ["Width"].GetValue (sz));
			AssertEquals ("GP3#3", sz.Height, propsColl ["Height"].GetValue (sz));
			AssertEquals ("GP3#4", sz.IsEmpty, propsColl ["IsEmpty"].GetValue (sz));

			Type type = typeof (Size);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = szconv.GetProperties (null, sz, attrs);
			AssertEquals ("GP3#5", 0, propsColl.Count);
		}
	}
}
