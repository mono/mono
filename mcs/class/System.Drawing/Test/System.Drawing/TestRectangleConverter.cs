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
	public class RectangleConverterTest
	{
		Rectangle rect;
		Rectangle rectneg;
		RectangleConverter rconv;
		String rectStrInvariant;
		String rectnegStrInvariant;

		[SetUp]
		public void SetUp ()
		{
			rect = new Rectangle (10, 10, 20, 30);
			rectStrInvariant = rect.X + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " +
			rect.Y + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " +
			rect.Width + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + 
			rect.Height;

			rectneg = new Rectangle (-10, -10, 20, 30);
			rectnegStrInvariant = rectneg.X + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " 
			+ rectneg.Y + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + 
			rectneg.Width + CultureInfo.InvariantCulture.TextInfo.ListSeparator + " " + rectneg.Height;

			rconv = (RectangleConverter) TypeDescriptor.GetConverter (rect);
		}

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (rconv.CanConvertFrom (typeof (String)), "CCF#1");
			Assert.IsTrue (rconv.CanConvertFrom (null, typeof (String)), "CCF#2");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (Rectangle)), "CCF#3");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (RectangleF)), "CCF#4");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (Point)), "CCF#5");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (PointF)), "CCF#6");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (Size)), "CCF#7");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (SizeF)), "CCF#8");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (Object)), "CCF#9");
			Assert.IsFalse (rconv.CanConvertFrom (null, typeof (int)), "CCF#10");
			Assert.IsTrue (rconv.CanConvertFrom (null, typeof (InstanceDescriptor)), "CCF#11");
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (rconv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (rconv.CanConvertTo (null, typeof (String)), "CCT#2");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (Rectangle)), "CCT#3");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (RectangleF)), "CCT#4");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (Point)), "CCT#5");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (PointF)), "CCT#6");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (Size)), "CCT#7");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (SizeF)), "CCT#8");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (Object)), "CCT#9");
			Assert.IsFalse (rconv.CanConvertTo (null, typeof (int)), "CCT#10");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (rect, (Rectangle) rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
								"10, 10, 20, 30"), "CF#1");
			Assert.AreEqual (rectneg, (Rectangle) rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
								"-10, -10, 20, 30"), "CF#2");

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture, 
						   "10, 10");
				Assert.Fail ("CF#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3");
			}

			try {
				rconv.ConvertFrom ("10");
				Assert.Fail ("CF#3a: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#3a");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "1, 1, 1, 1, 1");
				Assert.Fail ("CF#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "CF#4");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   "*1, 1, 1, 1");
				Assert.Fail ("CF#5: must throw Exception");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "CF#5-2");
				Assert.IsNotNull (ex.InnerException, "CF#5-3");
				Assert.AreEqual (typeof (FormatException), ex.InnerException.GetType (), "CF#5-4");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Rectangle (10, 10, 100, 100));
				Assert.Fail ("CF#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#6");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new RectangleF (10, 10, 100, 100));
				Assert.Fail ("CF#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#7");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Point (10, 10));
				Assert.Fail ("CF#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#8");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new PointF (10, 10));
				Assert.Fail ("CF#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#9");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Size (10, 10));
				Assert.Fail ("CF#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#10");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new SizeF (10, 10));
				Assert.Fail ("CF#11: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#11");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture,
						   new Object ());
				Assert.Fail ("CF#12: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#12");
			}

			try {
				rconv.ConvertFrom (null, CultureInfo.InvariantCulture, 1001);
				Assert.Fail ("CF#13: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CF#13");
			}
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (rectStrInvariant, (String) rconv.ConvertTo (null,
				CultureInfo.InvariantCulture, rect, typeof (String)), "CT#1");
			Assert.AreEqual (rectnegStrInvariant, (String) rconv.ConvertTo (null,
				CultureInfo.InvariantCulture, rectneg, typeof (String)), "CT#2");

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture, 
						 rect, typeof (Rectangle));
				Assert.Fail ("CT#3: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#3");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (RectangleF));
				Assert.Fail ("CT#4: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#4");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Size));
				Assert.Fail ("CT#5: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#5");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (SizeF));
				Assert.Fail ("CT#6: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#6");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Point));
				Assert.Fail ("CT#7: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#7");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (PointF));
				Assert.Fail ("CT#8: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#8");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (Object));
				Assert.Fail ("CT#9: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#9");
			}

			try {
				rconv.ConvertTo (null, CultureInfo.InvariantCulture,
						 rect, typeof (int));
				Assert.Fail ("CT#10: must throw NotSupportedException");
			} catch (Exception e) {
				Assert.IsTrue (e is NotSupportedException, "CT#10");
			}
		}

		[Test]
		public void TestGetCreateInstanceSupported ()
		{
			Assert.IsTrue (rconv.GetCreateInstanceSupported (), "GCIS#1");
			Assert.IsTrue (rconv.GetCreateInstanceSupported (null), "GCIS#2");
		}

		[Test]
		public void TestCreateInstance ()
		{
			Rectangle rectInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("X", 10); ht.Add ("Y", 10);
			ht.Add ("Width", 20); ht.Add ("Height", 30);

			rectInstance = (Rectangle) rconv.CreateInstance (ht);
			Assert.AreEqual (rect, rectInstance, "CI#1");

			ht.Clear ();
			ht.Add ("X", -10); ht.Add ("Y", -10);
			ht.Add ("Width", 20); ht.Add ("Height", 30);

			rectInstance = (Rectangle) rconv.CreateInstance (null, ht);
			Assert.AreEqual (rectneg, rectInstance, "CI#2");
		}

		[Test]
		public void TestCreateInstance_CaseSensitive ()
		{
			Hashtable ht = new Hashtable ();
			ht.Add ("x", -10);
			ht.Add ("Y", -10);
			ht.Add ("Width", 20);
			ht.Add ("Height", 30);
			Assert.Throws<ArgumentException> (() => rconv.CreateInstance (null, ht));
		}

		[Test]
		public void TestGetPropertiesSupported ()
		{
			Assert.IsTrue (rconv.GetPropertiesSupported (), "GPS#1");
			Assert.IsTrue (rconv.GetPropertiesSupported (null), "GPS#2");
		}

		[Test]
		public void TestGetProperties ()
		{
			Attribute [] attrs;
			PropertyDescriptorCollection propsColl;

			propsColl = rconv.GetProperties (rect);
			Assert.AreEqual (4, propsColl.Count, "GP1#1");
			Assert.AreEqual (rect.X, propsColl["X"].GetValue (rect), "GP1#2");
			Assert.AreEqual (rect.Y, propsColl["Y"].GetValue (rect), "GP1#3");
			Assert.AreEqual (rect.Width, propsColl["Width"].GetValue (rect), "GP1#4");
			Assert.AreEqual (rect.Height, propsColl["Height"].GetValue (rect), "GP1#5");

			propsColl = rconv.GetProperties (null, rectneg);
			Assert.AreEqual (4, propsColl.Count, "GP2#1");
			Assert.AreEqual (rectneg.X, propsColl["X"].GetValue (rectneg), "GP2#2");
			Assert.AreEqual (rectneg.Y, propsColl["Y"].GetValue (rectneg), "GP2#3");
			Assert.AreEqual (rectneg.Width, propsColl["Width"].GetValue (rectneg), "GP2#4");
			Assert.AreEqual (rectneg.Height, propsColl["Height"].GetValue (rectneg), "GP2#5");

			propsColl = rconv.GetProperties (null, rect, null);
			Assert.AreEqual (11, propsColl.Count, "GP3#1");
			Assert.AreEqual (rect.X, propsColl["X"].GetValue (rect), "GP3#2");
			Assert.AreEqual (rect.Y, propsColl["Y"].GetValue (rect), "GP3#3");
			Assert.AreEqual (rect.Width, propsColl["Width"].GetValue (rect), "GP3#4");
			Assert.AreEqual (rect.Height, propsColl["Height"].GetValue (rect), "GP3#5");

			Assert.AreEqual (rect.Top, propsColl["Top"].GetValue (rect), "GP3#6");
			Assert.AreEqual (rect.Bottom, propsColl["Bottom"].GetValue (rect), "GP3#7");
			Assert.AreEqual (rect.Left, propsColl["Left"].GetValue (rect), "GP3#8");
			Assert.AreEqual (rect.Right, propsColl["Right"].GetValue (rect), "GP3#9");
			Assert.AreEqual (rect.Location, propsColl["Location"].GetValue (rect), "GP3#10");
			Assert.AreEqual (rect.Size, propsColl["Size"].GetValue (rect), "GP3#11");
			Assert.AreEqual (rect.IsEmpty, propsColl["IsEmpty"].GetValue (rect), "GP3#12");

			Type type = typeof (Rectangle);
			attrs = Attribute.GetCustomAttributes (type, true);
			propsColl = rconv.GetProperties (null, rect, attrs);
			Assert.AreEqual (0, propsColl.Count, "GP3#13");
		}

		[Test]
		public void ConvertFromInvariantString_string ()
		{
			Assert.AreEqual (rect, rconv.ConvertFromInvariantString (rectStrInvariant),
				"CFISS#1");
			Assert.AreEqual (rectneg, rconv.ConvertFromInvariantString (rectnegStrInvariant),
				"CFISS#2");
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_1 ()
		{
			Assert.Throws<ArgumentException> (() => rconv.ConvertFromInvariantString ("1, 2, 3"));
		}

		[Test]
		public void ConvertFromInvariantString_string_exc_2 ()
		{
			try {
				rconv.ConvertFromInvariantString ("hello");
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
			Assert.Throws<ArgumentException> (() => rconv.ConvertFromString (string.Format(culture,
				"1{0} 2{0} 3{0} 4{0} 5", culture.TextInfo.ListSeparator)));
		}

		[Test]
		public void ConvertFromString_string_exc_2 ()
		{
			try {
				rconv.ConvertFromString ("hello");
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
			Assert.AreEqual (rectStrInvariant, rconv.ConvertToInvariantString (rect),
				"CFISS#1");
			Assert.AreEqual (rectnegStrInvariant, rconv.ConvertToInvariantString (rectneg),
				"CFISS#2");
		}

		[Test]
		public void ConvertToString_string () {
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
			Assert.IsFalse (rconv.GetStandardValuesSupported ());
		}

		[Test]
		public void GetStandardValues ()
		{
			Assert.IsNull (rconv.GetStandardValues ());
		}

		[Test]
		public void GetStandardValuesExclusive ()
		{
			Assert.IsFalse (rconv.GetStandardValuesExclusive ());
		}

		private void PerformConvertFromStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (rect, rconv.ConvertFromString (CreateRectangleString (rect)),
				"CFSS#1-" + culture.Name);
			Assert.AreEqual (rectneg, rconv.ConvertFromString (CreateRectangleString (rectneg)),
				"CFSS#2-" + culture.Name);
		}

		private void PerformConvertToStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (CreateRectangleString (rect), rconv.ConvertToString (rect),
				"CFISS#1-" + culture.Name);
			Assert.AreEqual (CreateRectangleString (rectneg), rconv.ConvertToString (rectneg),
				"CFISS#2-" + culture.Name);
		}

		private static string CreateRectangleString (Rectangle rectangle)
		{
			return CreateRectangleString (CultureInfo.CurrentCulture, rectangle);
		}

		private static string CreateRectangleString (CultureInfo culture, Rectangle rectangle)
		{
			return string.Format ("{0}{1} {2}{1} {3}{1} {4}", rectangle.X.ToString (culture),
				culture.TextInfo.ListSeparator, rectangle.Y.ToString (culture),
				rectangle.Width.ToString (culture), rectangle.Height.ToString (culture));
		}

		[Serializable]
		private sealed class MyCultureInfo : CultureInfo
		{
			internal MyCultureInfo ()
				: base ("en-US")
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
