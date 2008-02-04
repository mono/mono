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

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
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
		public void ConvertToString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
			DateTime date = DateTime.Now;

			Assert.AreEqual (date.ToString (info.ShortDatePattern + " " + 
				info.ShortTimePattern, culture), converter.ConvertToString (
				null, culture, date));
		}

		[Test]
		public void ConvertFromString ()
		{
			CultureInfo culture = new MyCultureInfo ();
			DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
			DateTime date = DateTime.Now;

			DateTime newDate = (DateTime) converter.ConvertFrom (null, culture, date.ToString("G", info));

			Assert.AreEqual (date.Year, newDate.Year, "#1");
			Assert.AreEqual (date.Month, newDate.Month, "#2");
			Assert.AreEqual (date.Day, newDate.Day, "#3");
			Assert.AreEqual (date.Hour, newDate.Hour, "#4");
			Assert.AreEqual (date.Minute, newDate.Minute, "#5");
			Assert.AreEqual (date.Second, newDate.Second, "#6");
			Assert.AreEqual (0, newDate.Millisecond, "#7");
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
