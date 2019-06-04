//
// System.ComponentModel.DateTimeConverter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace MonoTests.System.ComponentModel
{
	using NUnit.Framework;

	[TestFixture]
	public class DateTimeConverterTests
	{
		private DateTimeConverter converter;
		private string pattern;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new DateTimeConverter ();

			DateTimeFormatInfo info = CultureInfo.CurrentCulture.DateTimeFormat;
			pattern = info.ShortDatePattern + " " + info.ShortTimePattern;
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (DateTime)), "#2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#3");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#4");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#2");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#3");
		}

		[Test]
		public void ConvertFrom_String ()
		{
			DateTime date = DateTime.Now;
			DateTime newDate = (DateTime) converter.ConvertFrom (null, CultureInfo.InvariantCulture, 
				date.ToString(CultureInfo.InvariantCulture));

			Assert.AreEqual (date.Year, newDate.Year, "#1");
			Assert.AreEqual (date.Month, newDate.Month, "#2");
			Assert.AreEqual (date.Day, newDate.Day, "#3");
			Assert.AreEqual (date.Hour, newDate.Hour, "#4");
			Assert.AreEqual (date.Minute, newDate.Minute, "#5");
			Assert.AreEqual (date.Second, newDate.Second, "#6");
			Assert.AreEqual (0, newDate.Millisecond, "#7");

			newDate = (DateTime) converter.ConvertFrom (null, CultureInfo.InvariantCulture, 
								    String.Empty);
			Assert.AreEqual (DateTime.MinValue, newDate, "#8");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Int32 ()
		{
			converter.ConvertFrom (10);
		}

		[Test]
		public void ConvertTo_MinValue ()
		{
			Assert.AreEqual (string.Empty, converter.ConvertTo (null, 
				CultureInfo.InvariantCulture, DateTime.MinValue, typeof (string)), "#1");
			Assert.AreEqual (string.Empty, converter.ConvertTo (null, 
				CultureInfo.CurrentCulture, DateTime.MinValue, typeof (string)), "#2");
			Assert.AreEqual (string.Empty, converter.ConvertTo (DateTime.MinValue, 
				typeof (string)), "#3");
		}

		[Test]
		public void ConvertTo_MaxValue ()
		{
			Assert.AreEqual (DateTime.MaxValue.ToString (CultureInfo.InvariantCulture), 
				converter.ConvertTo (null, CultureInfo.InvariantCulture, DateTime.MaxValue, 
				typeof (string)), "#1");

			// FIXME: We probably shouldn't be using CurrentCulture in these tests.
			if (CultureInfo.CurrentCulture == CultureInfo.InvariantCulture)
				return;
			Assert.AreEqual (DateTime.MaxValue.ToString (pattern, 
				CultureInfo.CurrentCulture), converter.ConvertTo (null, 
				CultureInfo.CurrentCulture, DateTime.MaxValue, typeof (string)),
				"#2");
			Assert.AreEqual (DateTime.MaxValue.ToString (pattern, 
				CultureInfo.CurrentCulture), converter.ConvertTo (DateTime.MaxValue, 
				typeof (string)), "#3");
		}

		[Test]
		public void ConvertToString_MinValue ()
		{
			Assert.AreEqual (string.Empty, converter.ConvertToString (null, 
				CultureInfo.InvariantCulture, DateTime.MinValue), "#1");

			Assert.AreEqual (string.Empty, converter.ConvertToString (null, 
				DateTime.MinValue), "#2");
			Assert.AreEqual (string.Empty, converter.ConvertToString (null, 
				CultureInfo.CurrentCulture, DateTime.MinValue), "#3");
			Assert.AreEqual (string.Empty, converter.ConvertToString (DateTime.MinValue),
				"#4");
		}

		[Test]
		public void ConvertToString_MaxValue ()
		{
			Assert.AreEqual (DateTime.MaxValue.ToString (CultureInfo.InvariantCulture), 
				converter.ConvertToString (null, CultureInfo.InvariantCulture, 
				DateTime.MaxValue), "#1");

			// FIXME: We probably shouldn't be using CurrentCulture in these tests.
			if (CultureInfo.CurrentCulture == CultureInfo.InvariantCulture)
				return;
			Assert.AreEqual (DateTime.MaxValue.ToString (pattern, CultureInfo.CurrentCulture),
				converter.ConvertToString (null, DateTime.MaxValue), "#2");
			Assert.AreEqual (DateTime.MaxValue.ToString (pattern, CultureInfo.CurrentCulture),
				converter.ConvertToString (null, CultureInfo.CurrentCulture,
				DateTime.MaxValue), "#3");
			Assert.AreEqual (DateTime.MaxValue.ToString (pattern, CultureInfo.CurrentCulture),
				converter.ConvertToString (DateTime.MaxValue), "#4");
		}

		[Test]
		[SetCulture ("en-GB")]
		[Category ("Calendars")]
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
			DateTime date = DateTime.Now;

			Assert.AreEqual (date.ToString (info.ShortDatePattern + " " + 
				info.ShortTimePattern, culture), converter.ConvertToString (
				null, culture, date));

			CultureInfo ciUS = new CultureInfo("en-US");
			CultureInfo ciGB = new CultureInfo("en-GB");
			CultureInfo ciDE = new CultureInfo("de-DE");
			//
			date = new DateTime(2008, 12, 31, 23, 59, 58, 5);
			DoTestToString("12/31/2008 11:59 pm", date, ciUS);
			DoTestToString("31/12/2008 23:59", date, ciGB);
			DoTestToString("31.12.2008 23:59", date, ciDE);
			DoTestToString("12/31/2008 23:59:58", date, CultureInfo.InvariantCulture);
			Assert.AreEqual("12/31/2008 23:59:58", converter.ConvertToInvariantString(date), "Invariant");
			//
			date = new DateTime(2008, 12, 31);
			DoTestToString("12/31/2008", date, ciUS);
			DoTestToString("31/12/2008", date, ciGB);
			DoTestToString("31.12.2008", date, ciDE);
			DoTestToString("2008-12-31", date, CultureInfo.InvariantCulture);
			Assert.AreEqual("2008-12-31", converter.ConvertToInvariantString(date), "Invariant");
		}
		private void DoTestToString(String expected, DateTime value, CultureInfo ci)
		{
			String message = ci.Name;
			if (message == null || message.Length == 0)
				message = "?Invariant";
			Assert.AreEqual(expected, converter.ConvertTo(null, ci, value, typeof(String)), message);
		}

		[Test]
		public void ConvertFromString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
			DateTime date = DateTime.Now;

			try {
				converter.ConvertFrom (null, culture, date.ToString("G", info));
			} catch (FormatException) {
			}
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_InvalidValue ()
		{
			converter.ConvertFrom ("*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_InvalidValue_Invariant ()
		{
			converter.ConvertFrom (null, CultureInfo.InvariantCulture, "*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromString_InvalidValue ()
		{
			converter.ConvertFromString ("*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromString_InvalidValue_Invariant ()
		{
			converter.ConvertFromString (null, CultureInfo.InvariantCulture, "*1");
		}

		[Serializable]
		private sealed class MyCultureInfo : CultureInfo
		{
			internal MyCultureInfo () : base ("en-US")
			{
			}

			public override object GetFormat (Type formatType)
			{
				if (formatType == typeof (DateTimeFormatInfo)) {
					DateTimeFormatInfo info = (DateTimeFormatInfo) ((DateTimeFormatInfo) base.GetFormat (formatType)).Clone ();
					info.ShortDatePattern = "MM?dd?yyyy";
					info.ShortTimePattern = "hh!mm";
					return DateTimeFormatInfo.ReadOnly (info);
				} else {
					return base.GetFormat (formatType);
				}
			}
		}
	}
}
