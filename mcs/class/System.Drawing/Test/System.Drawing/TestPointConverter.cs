//
// Tests for System.Drawing.PointConverter.cs 
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
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class PointConverterTest
	{
		Point pt;
		Point ptneg;
		PointConverter ptconv;
		String ptStr;
		String ptnegStr;

		[SetUp]
		public void SetUp ()
		{
			pt = new Point (1, 2);
			ptStr = pt.X + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + pt.Y;

			ptneg = new Point (-2, -3);
			ptnegStr = ptneg.X + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + ptneg.Y;

			ptconv = (PointConverter) TypeDescriptor.GetConverter (pt);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (ptconv.CanConvertFrom (typeof (String)), "CCF#1");
			Assert.IsTrue (ptconv.CanConvertFrom (null, typeof (String)), "CCF#2");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (Rectangle)), "CCF#3");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (RectangleF)), "CCF#4");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (Point)), "CCF#5");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (PointF)), "CCF#6");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (Size)), "CCF#7");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (SizeF)), "CCF#8");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (Object)), "CCF#9");
			Assert.IsFalse (ptconv.CanConvertFrom (null, typeof (int)), "CCF#10");
			Assert.IsTrue (ptconv.CanConvertFrom (null, typeof (InstanceDescriptor)), "CCF#11");
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (ptconv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (ptconv.CanConvertTo (null, typeof (String)), "CCT#2");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (Rectangle)), "CCT#3");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (RectangleF)), "CCT#4");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (Point)), "CCT#5");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (PointF)), "CCT#6");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (Size)), "CCT#7");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (SizeF)), "CCT#8");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (Object)), "CCT#9");
			Assert.IsFalse (ptconv.CanConvertTo (null, typeof (int)), "CCT#10");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (pt, (Point) ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
								"1, 2"), "CF#1");
			Assert.AreEqual (ptneg, (Point) ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
								"-2, -3"), "CF#2");

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, "1");
				Assert.Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3");
			}

			try {
				ptconv.ConvertFrom ("1");
				Assert.Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3a");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, "1, 1, 1");
				Assert.Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#4");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, "*1, 1");
				Assert.Fail ("CF#5-1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "CF#5-2");
				Assert.IsNotNull (ex.InnerException, "CF#5-3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "CF#5-4");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, 
					new Point (1, 1));
				Assert.Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new PointF (1, 1));
				Assert.Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#7");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, 
					new Size (1, 1));
				Assert.Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#8");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new SizeF (1, 1));
				Assert.Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#9");
			}

			try {
				ptconv.ConvertFrom (null, CultureInfo.InvariantCulture, 0x10);
				Assert.Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#10");
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (ptStr, (String) ptconv.ConvertTo (null, CultureInfo.InvariantCulture,
								pt, typeof (String)), "CT#1");
			Assert.AreEqual (ptnegStr, (String) ptconv.ConvertTo (null, CultureInfo.InvariantCulture,
								ptneg, typeof (String)), "CT#2");

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (Size));
				Assert.Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3");
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (SizeF));
				Assert.Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4");
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (Point));
				Assert.Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#5");
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (PointF));
				Assert.Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#6");
			}

			try {
				ptconv.ConvertTo (null, CultureInfo.InvariantCulture, pt,
						  typeof (int));
				Assert.Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#7");
			}

			try {
				// culture == null
				ptconv.ConvertTo (null, null, pt, typeof (string));
			} catch (NullReferenceException) {
				Assert.Fail ("CT#8: must not throw NullReferenceException");
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert.IsTrue (ptconv.GetCreateInstanceSupported (), "GCIS#1");
			Assert.IsTrue (ptconv.GetCreateInstanceSupported (null), "GCIS#2");
		}

		[Test]
		public void TestCreateInstance ()
		{
			Point ptInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("X", 1); ht.Add ("Y", 2);

			ptInstance = (Point) ptconv.CreateInstance (ht);
			Assert.AreEqual (pt, ptInstance, "CI#1");

			ht.Clear ();
			ht.Add ("X", -2); ht.Add ("Y", -3);

			ptInstance = (Point) ptconv.CreateInstance (null, ht);
			Assert.AreEqual (ptneg, ptInstance, "CI#2");
		}

		[Test]
		public void TestCreateInstance_CaseSensitive ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add ("x", 2);
			ht.Add ("Y", 3);
			Assert.Throws<ArgumentException> (() => ptconv.CreateInstance (null, ht));
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert.IsTrue (ptconv.GetPropertiesSupported (), "GPS#1");
			Assert.IsTrue (ptconv.GetPropertiesSupported (null), "GPS#2");
		}

		[Test]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = ptconv.GetProperties (pt);
			Assert.AreEqual (2, propsColl.Count, "GP1#1");
			Assert.AreEqual (pt.X, propsColl["X"].GetValue (pt), "GP1#2");
			Assert.AreEqual (pt.Y, propsColl["Y"].GetValue (pt), "GP1#3");

			propsColl = ptconv.GetProperties (null, ptneg);
			Assert.AreEqual (2, propsColl.Count, "GP2#1");
			Assert.AreEqual (ptneg.X, propsColl["X"].GetValue (ptneg), "GP2#2");
			Assert.AreEqual (ptneg.Y, propsColl["Y"].GetValue (ptneg), "GP2#3");

			propsColl = ptconv.GetProperties (null, pt, null);
			Assert.AreEqual (3, propsColl.Count, "GP3#1");
			Assert.AreEqual (pt.X, propsColl["X"].GetValue (pt), "GP3#2");
			Assert.AreEqual (pt.Y, propsColl["Y"].GetValue (pt), "GP3#3");
			Assert.AreEqual (pt.IsEmpty, propsColl["IsEmpty"].GetValue (pt), "GP3#4");

			Type type = typeof (Point);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = ptconv.GetProperties (null, pt, attrs);
			Assert.AreEqual (0, propsColl.Count, "GP3#5");
		}

		[Test]
		public void ConvertFromInvariantString_string ()
		{
			Assert.AreEqual (pt, ptconv.ConvertFromInvariantString ("1, 2"), "CFISS#1");
			Assert.AreEqual (ptneg, ptconv.ConvertFromInvariantString ("-2, -3"), "CFISS#2");
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_1 ()
		{
			Assert.Throws<ArgumentException> (() => ptconv.ConvertFromInvariantString ("1"));
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_2 ()
		{
			try {
				ptconv.ConvertFromInvariantString ("hello");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertFromString_string ()
		{
			// save current culture
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

			try {
				PerformConvertFromStringTest (new CultureInfo ("en-US"));
				PerformConvertFromStringTest (new CultureInfo ("nl-BE"));
				PerformConvertFromStringTest (new MyCultureInfo ());
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		[Test]
		public void ConvertFromString_string_exc_1 ()
		{
			Assert.Throws<ArgumentException> (() => ptconv.ConvertFromString ("1"));
		}

		[Test]
		public void ConvertFromString_string_exc_2 ()
		{
			try {
				ptconv.ConvertFromString ("hello");
				Assert.Fail ("#1");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "#3");
			}
		}

		[Test]
		public void ConvertToInvariantString_string ()
		{
			Assert.AreEqual ("1" + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " 2",
				ptconv.ConvertToInvariantString (pt), "CFISS#1");
			Assert.AreEqual ("-2" + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " -3",
				ptconv.ConvertToInvariantString (ptneg), "CFISS#2");
		}

		[Test]
		public void ConvertToString_string ()
		{
			// save current culture
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

			try {
				PerformConvertToStringTest (new CultureInfo ("en-US"));
				PerformConvertToStringTest (new CultureInfo ("nl-BE"));
				PerformConvertToStringTest (new MyCultureInfo ());
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		[Test]
		public void GetStandardValuesSupported ()
		{
			Assert.IsFalse (ptconv.GetStandardValuesSupported ());
		}

		[Test]
		public void GetStandardValues ()
		{
			Assert.IsNull (ptconv.GetStandardValues ());
		}

		[Test]
		public void GetStandardValuesExclusive ()
		{
			Assert.IsFalse (ptconv.GetStandardValuesExclusive ());
		}

		private void PerformConvertFromStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (pt, ptconv.ConvertFromString (CreatePointString (culture, pt)),
				"CFSS#1-" + culture.Name);
			Assert.AreEqual (ptneg, ptconv.ConvertFromString (CreatePointString (culture, ptneg)),
				"CFSS#2-" + culture.Name);
		}

		private void PerformConvertToStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (CreatePointString (culture, pt), ptconv.ConvertToString (pt),
				"CFISS#1-" + culture.Name);
			Assert.AreEqual (CreatePointString (culture, ptneg), ptconv.ConvertToString (ptneg),
				"CFISS#2-" + culture.Name);
		}

		private static string CreatePointString (Point point)
		{
			return CreatePointString (CultureInfo.CurrentCulture, point);
		}

		private static string CreatePointString (CultureInfo culture, Point point)
		{
			return string.Format ("{0}{1} {2}", point.X.ToString (culture),
				culture.TextInfo.ListSeparator, point.Y.ToString (culture));
		}

		[Serializable]
		private sealed class MyCultureInfo : CultureInfo
		{
			internal MyCultureInfo () : base ("en-US")
			{
			}

			public override object GetFormat (Type formatType)
			{
				if (formatType == typeof (NumberFormatInfo)) {
					NumberFormatInfo nfi = (NumberFormatInfo) ((NumberFormatInfo) base.GetFormat (formatType)).Clone ();

					nfi.NegativeSign = "myNegativeSign";
					return NumberFormatInfo.ReadOnly (nfi);
				} else {
					return base.GetFormat (formatType);
				}
			}
		}
	}
}
