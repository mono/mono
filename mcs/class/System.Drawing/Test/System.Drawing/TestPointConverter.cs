//
// Tests for System.Drawing.PointConverter.cs 
//
// Author:
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
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
	public class PointConverterTest : Assertion
	{
		Point pt;
		Point ptneg;
		PointConverter ptconv;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			pt = new Point (1, 2);
			ptneg = new Point (-2, -3);
			ptconv = (PointConverter) TypeDescriptor.GetConverter (pt);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert ("CCF#1", ptconv.CanConvertFrom (typeof (String)));
			Assert ("CCF#1a", ptconv.CanConvertFrom (null, typeof (String)));
			Assert ("CCF#2", ! ptconv.CanConvertFrom (null, typeof (Rectangle)));
			Assert ("CCF#3", ! ptconv.CanConvertFrom (null, typeof (RectangleF)));
			Assert ("CCF#4", ! ptconv.CanConvertFrom (null, typeof (Point)));
			Assert ("CCF#5", ! ptconv.CanConvertFrom (null, typeof (PointF)));
			Assert ("CCF#6", ! ptconv.CanConvertFrom (null, typeof (Size)));
			Assert ("CCF#7", ! ptconv.CanConvertFrom (null, typeof (SizeF)));
			Assert ("CCF#8", ! ptconv.CanConvertFrom (null, typeof (Object)));
			Assert ("CCF#9", ! ptconv.CanConvertFrom (null, typeof (int)));
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert ("CCT#1", ptconv.CanConvertTo (typeof (String)));
			Assert ("CCT#1a", ptconv.CanConvertTo (null, typeof (String)));
			Assert ("CCT#2", ! ptconv.CanConvertTo (null, typeof (Rectangle)));
			Assert ("CCT#3", ! ptconv.CanConvertTo (null, typeof (RectangleF)));
			Assert ("CCT#4", ! ptconv.CanConvertTo (null, typeof (Point)));
			Assert ("CCT#5", ! ptconv.CanConvertTo (null, typeof (PointF)));
			Assert ("CCT#6", ! ptconv.CanConvertTo (null, typeof (Size)));
			Assert ("CCT#7", ! ptconv.CanConvertTo (null, typeof (SizeF)));
			Assert ("CCT#8", ! ptconv.CanConvertTo (null, typeof (Object)));
			Assert ("CCT#9", ! ptconv.CanConvertTo (null, typeof (int)));
		}

		[Test]
		public void TestConvertFrom ()
		{
			AssertEquals ("CF#1", pt, (Point) ptconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"1, 2"));
			AssertEquals ("CF#1a", pt, (Point) ptconv.ConvertFrom ("1, 2"));
			AssertEquals ("CF#2", ptneg, (Point) ptconv.ConvertFrom (null,
								CultureInfo.InvariantCulture,
								"-2, -3"));
			AssertEquals ("CF#2a", ptneg, (Point) ptconv.ConvertFrom ("-2, -3"));

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, "1");
				Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3", e is ArgumentException);
			}

			try {
				ptconv.ConvertFrom ("1");
				Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#3a", e is ArgumentException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    "1, 1, 1");
				Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#4", e is ArgumentException);
			}

			try {
				ptconv.ConvertFrom ("1, 1, 1");
				Fail ("CF#4a: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("CF#4a", e is ArgumentException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    "*1, 1");
				Fail ("CF#5: must throw Exception");
			} catch {
			}

			try {
				ptconv.ConvertFrom ("*1, 1");
				Fail ("CF#5a: must throw Exception");
			} catch {
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new Point (1, 1));
				Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#6", e is NotSupportedException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new PointF (1, 1));
				Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#7", e is NotSupportedException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new Size (1, 1));
				Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#8", e is NotSupportedException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						    new SizeF (1, 1));
				Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#9", e is NotSupportedException);
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, 0x10);
				Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CF#10", e is NotSupportedException);
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			AssertEquals ("CT#1", pt.ToString (), (String) ptconv.ConvertTo (null,
								CultureInfo.InvariantCulture,
								pt, typeof (String)));
			AssertEquals ("CT#1a", pt.ToString (), (String) ptconv.ConvertTo (pt,
								typeof (String)));
			AssertEquals ("CT#2", ptneg.ToString (), (String) ptconv.ConvertTo (
							null, CultureInfo.InvariantCulture,
							ptneg, typeof (String)));
			AssertEquals ("CT#2a", ptneg.ToString (), (String) ptconv.ConvertTo (
								ptneg, typeof (String)));

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (Size));
				Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#3", e is NotSupportedException);
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (SizeF));
				Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#4", e is NotSupportedException);
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (Point));
				Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#5", e is NotSupportedException);
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (PointF));
				Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#6", e is NotSupportedException);
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (int));
				Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert ("CT#7", e is NotSupportedException);
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert ("GCIS#1", ptconv.GetCreateInstanceSupported ());
			Assert ("GCIS#2", ptconv.GetCreateInstanceSupported (null));
		}

		[Test]
		public void TestCreateInstance ()
		{
			Point ptInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("X", 1); ht.Add ("Y", 2);

			ptInstance = (Point) ptconv.CreateInstance (ht);
			AssertEquals ("CI#1", pt, ptInstance);

			ht.Clear ();
			ht.Add ("X", -2); ht.Add ("Y", -3);

			ptInstance = (Point) ptconv.CreateInstance (null, ht);
			AssertEquals ("CI#2", ptneg, ptInstance);

			// Property names are case-sensitive. It should throw 
			// NullRefExc if any of the property names does not match
			ht.Clear ();
			ht.Add ("x", 2); ht.Add ("Y", 3);
			try {
				ptInstance = (Point) ptconv.CreateInstance (null, ht);
				Fail ("CI#3: must throw NullReferenceException");
			} catch (Exception e) {
				Assert ("CI#3", e is NullReferenceException);
			}
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert ("GPS#1", ptconv.GetPropertiesSupported ());
			Assert ("GPS#2", ptconv.GetPropertiesSupported (null));
		}

		[Test]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = ptconv.GetProperties (pt);
			AssertEquals ("GP1#1", 2, propsColl.Count);
			AssertEquals ("GP1#2", pt.X, propsColl ["X"].GetValue (pt));
			AssertEquals ("GP1#3", pt.Y, propsColl ["Y"].GetValue (pt));

			propsColl = ptconv.GetProperties (null, ptneg);
			AssertEquals ("GP2#1", 2, propsColl.Count);
			AssertEquals ("GP2#2", ptneg.X, propsColl ["X"].GetValue (ptneg));
			AssertEquals ("GP2#3", ptneg.Y, propsColl ["Y"].GetValue (ptneg));

			propsColl = ptconv.GetProperties (null, pt, null);
			AssertEquals ("GP3#1", 3, propsColl.Count);
			AssertEquals ("GP3#2", pt.X, propsColl ["X"].GetValue (pt));
			AssertEquals ("GP3#3", pt.Y, propsColl ["Y"].GetValue (pt));
			AssertEquals ("GP3#4", pt.IsEmpty, propsColl ["IsEmpty"].GetValue (pt));

			Type type = typeof (Point);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = ptconv.GetProperties (null, pt, attrs);
			AssertEquals ("GP3#5", 0, propsColl.Count);
		}
	}
}
