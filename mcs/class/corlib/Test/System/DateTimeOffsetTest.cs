//
// DateTimeOffsetTest.cs - NUnit Test Cases for the System.DateTimeOffset struct
//
// Authors:
//	Stephane Delcroix  (sdelcroix@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System.Globalization;
using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class DateTimeOffsetTest
	{
		//ctor exception checking...
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UtcWithWrongOffset ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Utc);
			TimeSpan offset = new TimeSpan (2, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LocalWithWrongOffset ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Local);
			TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset (dt) + new TimeSpan (1,0,0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OffsetNotInWholeminutes ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (1,0,59);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void OffsetOutOfRange1 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (14, 1, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void OffsetOutOfRange2 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (-14, -1, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void UtcDateTimeOutOfRange1 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (1, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void UtcDateTimeOutOfRange2 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.MaxValue, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (-1, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		public void CompareTwoDateInDiffTZ ()
		{
			DateTimeOffset dt1 = new DateTimeOffset (2007, 12, 16, 15, 06, 00, new TimeSpan (1, 0, 0));
			DateTimeOffset dt2 = new DateTimeOffset (2007, 12, 16, 9, 06, 00, new TimeSpan (-5, 0, 0));
			DateTimeOffset dt3 = new DateTimeOffset (2007, 12, 16, 14, 06, 00, new TimeSpan (1, 0, 0));
			object o = dt1;
			Assert.IsTrue (dt1.CompareTo (dt2) == 0);
			Assert.IsTrue (DateTimeOffset.Compare (dt1, dt2) == 0);
			Assert.IsTrue (dt1 == dt2);
			Assert.IsTrue (dt1.Equals (dt2));
			Assert.IsFalse (dt1 == dt3);
			Assert.IsTrue (dt1 != dt3);
			Assert.IsFalse (dt1.EqualsExact (dt2));
			Assert.IsTrue (dt1.CompareTo (dt3) > 0);
			Assert.IsTrue (dt1.CompareTo (o) == 0);
		}

		[Test]
		public void UtcDateTime ()
		{
			DateTimeOffset dt = new DateTimeOffset (2007, 12, 16, 15, 49, 00, new TimeSpan (1, 0, 0));
			object o = dt.UtcDateTime;
			Assert.IsTrue (o is DateTime);
			Assert.IsTrue (dt.UtcDateTime == new DateTime (2007,12,16,14,49,00,DateTimeKind.Utc));
		}

		[Test]
		public void ParameterlessToString ()
		{
			DateTimeOffset dt = new DateTimeOffset (2007, 12, 18, 12, 16, 30, new TimeSpan (1, 0, 0));
			Assert.AreEqual ("12/18/2007 12:16:30 PM +01:00", dt.ToString (new CultureInfo ("en-us")));
			dt = new DateTimeOffset (2007, 12, 18, 12, 16, 30, new TimeSpan (-5, 0, 0));
			Assert.AreEqual ("12/18/2007 12:16:30 PM -05:00", dt.ToString (new CultureInfo ("en-us")));
			dt = new DateTimeOffset (2007, 12, 18, 12, 16, 30, TimeSpan.Zero);
			Assert.AreEqual ("12/18/2007 12:16:30 PM +00:00", dt.ToString (new CultureInfo ("en-us")));
		}

		[Test]
		public void ToStringWithCultureInfo ()
		{
			DateTimeOffset dto = new DateTimeOffset(2007, 5, 1, 9, 0, 0, TimeSpan.Zero);
			Assert.AreEqual ("05/01/2007 09:00:00 +00:00", dto.ToString (CultureInfo.InvariantCulture));
			Assert.AreEqual ("5/1/2007 9:00:00 AM +00:00", dto.ToString (new CultureInfo ("en-us")));
			Assert.AreEqual ("01/05/2007 09:00:00 +00:00", dto.ToString (new CultureInfo ("fr-fr")));
			Assert.AreEqual ("01.05.2007 09:00:00 +00:00", dto.ToString (new CultureInfo ("de-DE")));
			Assert.AreEqual ("01/05/2007 9:00:00 +00:00", dto.ToString (new CultureInfo ("es-ES")));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToStringExceptionalCase1 ()
		{
			DateTimeOffset.Now.ToString (";");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToStringExceptionalCase2 ()
		{
			DateTimeOffset.Now.ToString ("U");
		}

		[Test]
		public void ToStringWithFormat ()
		{
			DateTimeOffset dto = new DateTimeOffset (2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0));
			Assert.AreEqual ("10/31/2007", dto.ToString ("d", new CultureInfo ("en-us")));
			Assert.AreEqual ("Wednesday, October 31, 2007", dto.ToString ("D", new CultureInfo ("en-us")));
			Assert.AreEqual ("9:00 PM", dto.ToString ("t", new CultureInfo ("en-us")));
			Assert.AreEqual ("9:00:00 PM", dto.ToString ("T", new CultureInfo ("en-us")));
			Assert.AreEqual ("Wednesday, October 31, 2007 9:00 PM", dto.ToString ("f", new CultureInfo ("en-us")));
			Assert.AreEqual ("Wednesday, October 31, 2007 9:00:00 PM", dto.ToString ("F", new CultureInfo ("en-us")));
			Assert.AreEqual ("10/31/2007 9:00 PM", dto.ToString ("g", new CultureInfo ("en-us")));
			Assert.AreEqual ("10/31/2007 9:00:00 PM", dto.ToString ("G", new CultureInfo ("en-us")));
			Assert.AreEqual ("October 31", dto.ToString ("M", new CultureInfo ("en-us")));
			Assert.AreEqual (dto.ToString ("m", new CultureInfo ("en-us")), dto.ToString ("M", new CultureInfo ("en-us")));
			Assert.AreEqual ("Thu, 01 Nov 2007 05:00:00 GMT", dto.ToString ("R", new CultureInfo ("en-us")));
			Assert.AreEqual (dto.ToString ("r", new CultureInfo ("en-us")), dto.ToString ("R", new CultureInfo ("en-us")));
			Assert.AreEqual ("2007-10-31T21:00:00", dto.ToString ("s", new CultureInfo ("en-us")));
			Assert.AreEqual ("2007-11-01 05:00:00Z", dto.ToString ("u", new CultureInfo ("en-us")));
			Assert.AreEqual ("October, 2007", dto.ToString ("Y", new CultureInfo ("en-us")));
			Assert.AreEqual (dto.ToString ("y", new CultureInfo ("en-us")), dto.ToString ("Y", new CultureInfo ("en-us")));
		}

		[Test]
		public void ToStringWithFormatAndCulture ()
		{
			DateTimeOffset dto = new DateTimeOffset (2007, 11, 1, 9, 0, 0, new TimeSpan(-7, 0, 0));
			string format = "dddd, MMM dd yyyy HH:mm:ss zzz";
			Assert.AreEqual ("Thursday, Nov 01 2007 09:00:00 -07:00", dto.ToString (format, null as DateTimeFormatInfo));
			Assert.AreEqual ("Thursday, Nov 01 2007 09:00:00 -07:00", dto.ToString (format, CultureInfo.InvariantCulture));
			Assert.AreEqual ("jeudi, nov. 01 2007 09:00:00 -07:00", dto.ToString (format, new CultureInfo ("fr-FR")));
			Assert.AreEqual ("jueves, nov 01 2007 09:00:00 -07:00", dto.ToString (format, new CultureInfo ("es-ES")));
		}
	}
}
#endif

