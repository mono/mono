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
	public class SizeConverterTest
	{
		Size sz;
		Size szneg;
		SizeConverter szconv;
		String szStrInvariant;
		String sznegStrInvariant;

		[SetUp]
		public void SetUp ()
		{
			sz = new Size (10, 20);
			szStrInvariant = sz.Width + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + sz.Height;

			szneg = new Size (-20, -30);
			sznegStrInvariant = szneg.Width + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + szneg.Height;

			szconv = (SizeConverter) TypeDescriptor.GetConverter (sz);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (szconv.CanConvertFrom (typeof (String)), "CCF#1");
			Assert.IsTrue (szconv.CanConvertFrom (null, typeof (String)), "CCF#2");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (Rectangle)), "CCF#3");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (RectangleF)), "CCF#4");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (Point)), "CCF#5");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (PointF)), "CCF#6");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (Size)), "CCF#7");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (SizeF)), "CCF#8");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (Object)), "CCF#9");
			Assert.IsFalse (szconv.CanConvertFrom (null, typeof (int)), "CCF#10");
			Assert.IsTrue (szconv.CanConvertFrom (null, typeof (InstanceDescriptor)), "CCF#11");
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (szconv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (szconv.CanConvertTo (null, typeof (String)), "CCT#2");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (Rectangle)), "CCT#3");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (RectangleF)), "CCT#4");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (Point)), "CCT#5");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (PointF)), "CCT#6");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (Size)), "CCT#7");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (SizeF)), "CCT#8");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (Object)), "CCT#9");
			Assert.IsFalse (szconv.CanConvertTo (null, typeof (int)), "CCT#10");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (sz, (Size) szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
				"10, 20"), "CF#1");
			Assert.AreEqual (szneg, (Size) szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
				"-20, -30"), "CF#2");

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture, "10");
				Assert.Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3");
			}

			try {
				szconv.ConvertFrom ("10");
				Assert.Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3a");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					"1, 1, 1");
				Assert.Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#4");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					"*1, 1");
				Assert.Fail ("CF#5-1: must throw Exception");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "CF#5-2");
				Assert.IsNotNull (ex.InnerException, "CF#5-3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "CF#5-4");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new Point (10, 10));
				Assert.Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new PointF (10, 10));
				Assert.Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#7");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new Size (10, 10));
				Assert.Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#8");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new SizeF (10, 10));
				Assert.Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#9");
			}

			try {
				szconv.ConvertFrom (null, CultureInfo.InvariantCulture, 0x10);
				Assert.Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#10");
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (szStrInvariant, (String) szconv.ConvertTo (null,
				CultureInfo.InvariantCulture, sz, typeof (String)), "CT#1");
			Assert.AreEqual (sznegStrInvariant, (String) szconv.ConvertTo (null,
				CultureInfo.InvariantCulture, szneg, typeof (String)), "CT#2");

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
					typeof (Size));
				Assert.Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3");
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
					typeof (SizeF));
				Assert.Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4");
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
					typeof (Point));
				Assert.Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#5");
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
					typeof (PointF));
				Assert.Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#6");
			}

			try {
				szconv.ConvertTo (null, CultureInfo.InvariantCulture, sz,
					typeof (int));
				Assert.Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#7");
			}

			try {
				// culture == null
				szconv.ConvertTo (null, null, sz, typeof (string));
			} catch (NullReferenceException) {
				Assert.Fail ("CT#8: must not throw NullReferenceException");
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert.IsTrue (szconv.GetCreateInstanceSupported (), "GCIS#1");
			Assert.IsTrue (szconv.GetCreateInstanceSupported (null), "GCIS#2");
		}

		[Test]
		public void TestCreateInstance ()
		{
			Size szInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("Width", 10); ht.Add ("Height", 20);

			szInstance = (Size) szconv.CreateInstance (ht);
			Assert.AreEqual (sz, szInstance, "CI#1");

			ht.Clear ();
			ht.Add ("Width", -20); ht.Add ("Height", -30);

			szInstance = (Size) szconv.CreateInstance (null, ht);
			Assert.AreEqual (szneg, szInstance, "CI#2");
		}

		[Test]
		public void TestCreateInstance_CaseSensitive ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add ("width", 20);
			ht.Add ("Height", 30);
			Assert.Throws<ArgumentException> (() => szconv.CreateInstance (null, ht));
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert.IsTrue (szconv.GetPropertiesSupported (), "GPS#1");
			Assert.IsTrue (szconv.GetPropertiesSupported (null), "GPS#2");
		}

		[Test]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = szconv.GetProperties (sz);
			Assert.AreEqual (2, propsColl.Count, "GP1#1");
			Assert.AreEqual (sz.Width, propsColl["Width"].GetValue (sz), "GP1#2");
			Assert.AreEqual (sz.Height, propsColl["Height"].GetValue (sz), "GP1#3");

			propsColl = szconv.GetProperties (null, szneg);
			Assert.AreEqual (2, propsColl.Count, "GP2#1");
			Assert.AreEqual (szneg.Width, propsColl["Width"].GetValue (szneg), "GP2#2");
			Assert.AreEqual (szneg.Height, propsColl["Height"].GetValue (szneg), "GP2#3");

			propsColl = szconv.GetProperties (null, sz, null);
			Assert.AreEqual (3, propsColl.Count, "GP3#1");
			Assert.AreEqual (sz.Width, propsColl["Width"].GetValue (sz), "GP3#2");
			Assert.AreEqual (sz.Height, propsColl["Height"].GetValue (sz), "GP3#3");
			Assert.AreEqual (sz.IsEmpty, propsColl["IsEmpty"].GetValue (sz), "GP3#4");

			Type type = typeof (Size);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = szconv.GetProperties (null, sz, attrs);
			Assert.AreEqual (0, propsColl.Count, "GP3#5");
		}

		[Test]
		public void ConvertFromInvariantString_string ()
		{
			Assert.AreEqual (sz, szconv.ConvertFromInvariantString (szStrInvariant),
				"CFISS#1");
			Assert.AreEqual (szneg, szconv.ConvertFromInvariantString (sznegStrInvariant),
				"CFISS#2");
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_1 ()
		{
			Assert.Throws<ArgumentException> (() => szconv.ConvertFromInvariantString ("1, 2, 3"));
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_2 ()
		{
			try {
				szconv.ConvertFromInvariantString ("hello");
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
			CultureInfo culture = CultureInfo.CurrentCulture;
			Assert.Throws<ArgumentException> (() => szconv.ConvertFromString (string.Format(culture,
				"1{0} 2{0} 3{0} 4{0} 5", culture.TextInfo.ListSeparator)));
		}

		[Test]
		public void ConvertFromString_string_exc_2 ()
		{
			try {
				szconv.ConvertFromString ("hello");
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
			Assert.AreEqual (szStrInvariant, szconv.ConvertToInvariantString (sz),
				"CFISS#1");
			Assert.AreEqual (sznegStrInvariant, szconv.ConvertToInvariantString (szneg),
				"CFISS#2");
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
			Assert.IsFalse (szconv.GetStandardValuesSupported ());
		}

		[Test]
		public void GetStandardValues ()
		{
			Assert.IsNull (szconv.GetStandardValues ());
		}

		[Test]
		public void GetStandardValuesExclusive ()
		{
			Assert.IsFalse (szconv.GetStandardValuesExclusive ());
		}

		private void PerformConvertFromStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (sz, szconv.ConvertFromString (CreateSizeString (culture, sz)),
				"CFSS#1-" + culture.Name);
			Assert.AreEqual (szneg, szconv.ConvertFromString (CreateSizeString (culture, szneg)),
				"CFSS#2-" + culture.Name);
		}

		private void PerformConvertToStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (CreateSizeString (culture, sz), szconv.ConvertToString (sz),
				"CFISS#1-" + culture.Name);
			Assert.AreEqual (CreateSizeString (culture, szneg), szconv.ConvertToString (szneg),
				"CFISS#2-" + culture.Name);
		}

		private static string CreateSizeString (Size size)
		{
			return CreateSizeString (CultureInfo.CurrentCulture, size);
		}

		private static string CreateSizeString (CultureInfo culture, Size size)
		{
			return string.Format ("{0}{1} {2}", size.Width.ToString (culture),
				culture.TextInfo.ListSeparator, size.Height.ToString (culture));
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
