//
// DateTimeTest.cs - NUnit Test Cases for the System.DateTime struct
//
// author:
//   Martin Baulig (martin@gnome.org)
//
//   (C) 2002 Free Software Foundation
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Threading;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class DateTimeTest
	{
		[Flags]
		internal enum Resolution : ushort
		{
			Year = 64,
			Month = 96,
			Day = 112,
			Hour = 120,
			Minute = 124,
			Second = 126,
			Millisecond = 127,
			_Month = 32,
			_Day = 16,
			_Hour = 8,
			_Minute = 4,
			_Second = 2,
			_Millisecond = 1
		}

		internal void DTAssertEquals (DateTime actual, DateTime expected, Resolution resolution)
		{
			DTAssertEquals (actual, expected, resolution, "");
		}

		internal void DTAssertEquals (DateTime expected, DateTime actual, Resolution resolution, string message)
		{
			if ((resolution & Resolution.Year) != 0)
				Assert.AreEqual (expected.Year, actual.Year, message);
			if ((resolution & Resolution._Month) != 0)
				Assert.AreEqual (expected.Month, actual.Month, message);
			if ((resolution & Resolution._Day) != 0)
				Assert.AreEqual (expected.Day, actual.Day, message);
			if ((resolution & Resolution._Hour) != 0)
				Assert.AreEqual (expected.Hour, actual.Hour, message);
			if ((resolution & Resolution._Minute) != 0)
				Assert.AreEqual (expected.Minute, actual.Minute, message);
			if ((resolution & Resolution._Second) != 0)
				Assert.AreEqual (expected.Second, actual.Second, message);
			if ((resolution & Resolution._Millisecond) != 0)
				Assert.AreEqual (expected.Millisecond, actual.Millisecond, message);
		}

		private CultureInfo oldcult;
	
		long[] myTicks = {
			631501920000000000L,	// 25 Feb 2002 - 00:00:00
			631502475130080000L,	// 25 Feb 2002 - 15:25:13,8
			631502115130080000L,	// 25 Feb 2002 - 05:25:13,8
			631502115000000000L,	// 25 Feb 2002 - 05:25:00
			631502115130000000L,	// 25 Feb 2002 - 05:25:13
			631502079130000000L,	// 25 Feb 2002 - 04:25:13
			629197085770000000L,    // 06 Nov 1994 - 08:49:37 
			631796544000000000L,    // 01 Feb 2003 - 00:00:00
		};

		[SetUp]
		public void SetUp() 
		{
			// the current culture determines the result of formatting
			oldcult = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("");
		}
	
		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = oldcult;
		}

		[Test]
		public void TestCtors ()
		{
			DateTime t1 = new DateTime (2002,2,25);
			Assert.AreEqual (myTicks [0], t1.Ticks, "A01");
			DateTime t2 = new DateTime (2002,2,25,15,25,13,8);
			Assert.AreEqual (myTicks [1], t2.Ticks, "A02");
			Assert.AreEqual (myTicks [0], t2.Date.Ticks, "A03");
			Assert.AreEqual (2002, t2.Year, "A04");
			Assert.AreEqual (2, t2.Month, "A05");
			Assert.AreEqual (25, t2.Day, "A06");
			Assert.AreEqual (15, t2.Hour, "A07");
			Assert.AreEqual (25, t2.Minute, "A08");
			Assert.AreEqual (13, t2.Second, "A09");
			Assert.AreEqual (8, t2.Millisecond, "A10");
			DateTime t3 = new DateTime (2002,2,25,5,25,13,8);
			Assert.AreEqual (myTicks [2], t3.Ticks, "A11");
		}

		[Test]
		public void Constructor_Max ()
		{
			Assert.AreEqual (3155378975999990000, new DateTime (9999, 12, 31, 23, 59, 59, 999).Ticks, "Max");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_Milliseconds_Negative () 
		{
			new DateTime (9999, 12, 31, 23, 59, 59, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_Milliseconds_1000 () 
		{
			new DateTime (9999, 12, 31, 23, 59, 59, 1000);
		}
		
		[Test]
		public void Fields ()
		{
			Assert.AreEqual (3155378975999999999L, DateTime.MaxValue.Ticks, "#1");
			Assert.AreEqual (0, DateTime.MinValue.Ticks, "#2");
		}
		
		[Test]
		public void Add ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			TimeSpan span = new TimeSpan (3, 54, 1);
			DateTime t2 = t1.Add (span);

			Assert.AreEqual (25, t2.Day, "#1");
			Assert.AreEqual (19, t2.Hour, "#2");
			Assert.AreEqual (19, t2.Minute, "#3");
			Assert.AreEqual (14, t2.Second, "#4");

			Assert.AreEqual (25, t1.Day, "#5");
			Assert.AreEqual (15, t1.Hour, "#6");
			Assert.AreEqual (25, t1.Minute, "#7");
			Assert.AreEqual (13, t1.Second, "#8");
		}
		
		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void AddOutOfRangeException1 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.Add (TimeSpan.MaxValue);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void AddOutOfRangeException2 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.Add (TimeSpan.MinValue);
		}
	
		[Test]
		public void AddDays ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1 = t1.AddDays (3);
			Assert.AreEqual (28, t1.Day, "#A1");
			Assert.AreEqual (15, t1.Hour, "#A2");
			Assert.AreEqual (25, t1.Minute, "#A3");
			Assert.AreEqual (13, t1.Second, "#A4");
			
			t1 = t1.AddDays (1.9);
			Assert.AreEqual (2, t1.Day, "#B1");
			Assert.AreEqual (13, t1.Hour, "#B2");
			Assert.AreEqual (1, t1.Minute, "#B3");
			Assert.AreEqual (13, t1.Second, "#B4");

			t1 = t1.AddDays (0.2);
			Assert.AreEqual (2, t1.Day, "#C1");
			Assert.AreEqual (17, t1.Hour, "#C2");
			Assert.AreEqual (49, t1.Minute, "#C3");
			Assert.AreEqual (13, t1.Second, "#C4");
		}
		
		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void AddDaysOutOfRangeException1 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.AddDays (10000000);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void AddDaysOutOfRangeException2 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.AddDays (-10000000);
		}

		[Test]
		public void AddHours ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1 = t1.AddHours (10);
			Assert.AreEqual (26, t1.Day, "#A1");
			Assert.AreEqual (1, t1.Hour, "#A2");
			Assert.AreEqual (25, t1.Minute, "#A3");
			Assert.AreEqual (13, t1.Second, "#A4");
			
			t1 = t1.AddHours (-3.7);
			Assert.AreEqual (25, t1.Day, "#B1");
			Assert.AreEqual (21, t1.Hour, "#B2");
			Assert.AreEqual (43, t1.Minute, "#B3");
			Assert.AreEqual (13, t1.Second, "#B4");

			t1 = t1.AddHours (3.732);
			Assert.AreEqual (26, t1.Day, "#C1");
			Assert.AreEqual (1, t1.Hour, "#C2");
			Assert.AreEqual (27, t1.Minute, "#C3");
			Assert.AreEqual (8, t1.Second, "#C4");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		[Category("MobileNotWorking")]
		public void AddHoursOutOfRangeException1 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.AddHours (9E100);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		[Category("MobileNotWorking")]
		public void AddHoursOutOfRangeException2 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.AddHours (-9E100);
		}

		[Test]
		public void AddMilliseconds ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1 = t1.AddMilliseconds (1E10);
			Assert.AreEqual (21, t1.Day, "#A1");
			Assert.AreEqual (9, t1.Hour, "#A2");
			Assert.AreEqual (11, t1.Minute, "#A3");
			Assert.AreEqual (53, t1.Second, "#A4");
			
			t1 = t1.AddMilliseconds (-19E10);
			Assert.AreEqual (13, t1.Day, "#B1");
			Assert.AreEqual (7, t1.Hour, "#B2");
			Assert.AreEqual (25, t1.Minute, "#B3");
			Assert.AreEqual (13, t1.Second, "#B4");

			t1 = t1.AddMilliseconds (15.623);
			Assert.AreEqual (13, t1.Day, "#C1");
			Assert.AreEqual (7, t1.Hour, "#C2");
			Assert.AreEqual (25, t1.Minute, "#C3");
			Assert.AreEqual (13, t1.Second, "#C4");
		}

		const long MaxMillis = 315537897600000;

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void AddMillisecondsOutOfRangeException1 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			// double to long conversion with overflow lead to "unspecified value", 
			// ref: https://msdn.microsoft.com/en-us/library/yht2cx7b.aspx
			// so we adapt the test to avoid this condition based on the real limit
			// see https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/datetime.cs#L90
			t1.AddMilliseconds (MaxMillis + 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void AddMillisecondsOutOfRangeException2 ()
		{
			DateTime t1 = new DateTime (myTicks [1]);
			t1.AddMilliseconds (-MaxMillis-1);
		}

		[Test]
		public void TestToString ()
		{
			DateTime t1 = new DateTime (myTicks[2]);
			DateTime t2 = new DateTime (myTicks[1]);
			DateTime t3 = new DateTime (999, 1, 2, 3, 4, 5);
			// Standard patterns
			Assert.AreEqual ("02/25/2002", t1.ToString ("d"), "#A1");
			Assert.AreEqual ("Monday, 25 February 2002", t1.ToString ("D"), "#A2");
			Assert.AreEqual ("Monday, 25 February 2002 05:25", t1.ToString ("f"), "#A3");
			Assert.AreEqual ("Monday, 25 February 2002 05:25:13", t1.ToString ("F"), "#A4");
			Assert.AreEqual ("02/25/2002 05:25", t1.ToString ("g"), "#A5");
			Assert.AreEqual ("02/25/2002 05:25:13", t1.ToString ("G"), "#A6");
			Assert.AreEqual ("February 25", t1.ToString ("m"), "#A7");
			Assert.AreEqual ("February 25", t1.ToString ("M"), "#A8");
			Assert.AreEqual ("Mon, 25 Feb 2002 05:25:13 GMT", t1.ToString ("r"), "#A9");
			Assert.AreEqual ("Mon, 25 Feb 2002 05:25:13 GMT", t1.ToString ("R"), "#A10");
			Assert.AreEqual ("2002-02-25T05:25:13", t1.ToString ("s"), "#A11");
			Assert.AreEqual ("05:25", t1.ToString ("t"), "#A12");
			Assert.AreEqual ("05:25:13", t1.ToString ("T"), "#A13");
			Assert.AreEqual ("2002-02-25 05:25:13Z", t1.ToString ("u"), "#A14");
			// FIXME: this test is timezone dependent
			// Assert.AreEqual ("Sunday, 24 February 2002 11:25:13", t1.ToString ("U"), "#A15");
			Assert.AreEqual ("2002 February", t1.ToString ("y"), "#A16");
			Assert.AreEqual ("2002 February", t1.ToString ("Y"), "#A17");
			Assert.AreEqual ("02/25/2002 05:25:13", t1.ToString (""), "#A18");

			// Custom patterns
			Assert.AreEqual ("25", t1.ToString ("%d"), "#B1");
			Assert.AreEqual ("25", t1.ToString ("dd"), "#B2");
			Assert.AreEqual ("Mon", t1.ToString ("ddd"), "#B3");
			Assert.AreEqual ("Monday", t1.ToString ("dddd"), "#B4");
			Assert.AreEqual ("2", t1.ToString ("%M"), "#B5");
			Assert.AreEqual ("02", t1.ToString ("MM"), "#B6");
			Assert.AreEqual ("Feb", t1.ToString ("MMM"), "#B7");
			Assert.AreEqual ("February", t1.ToString ("MMMM"), "#B8");
			Assert.AreEqual ("2", t1.ToString ("%y"), "#B9");
			Assert.AreEqual ("02", t1.ToString ("yy"), "#B10");
			Assert.AreEqual ("2002", t1.ToString ("yyyy"), "#B11");
			Assert.AreEqual ("5", t1.ToString ("%h"), "#B12");
			Assert.AreEqual ("05", t1.ToString ("hh"), "#B13");
			Assert.AreEqual ("3", t2.ToString ("%h"), "#B14");
			Assert.AreEqual ("03", t2.ToString ("hh"), "#B15");
			Assert.AreEqual ("15", t2.ToString ("%H"), "#B16");
			Assert.AreEqual ("15", t2.ToString ("HH"), "#B17");
			Assert.AreEqual ("25", t2.ToString ("%m"), "#B18");
			Assert.AreEqual ("25", t2.ToString ("mm"), "#B19");
			Assert.AreEqual ("13", t2.ToString ("%s"), "#B20");
			Assert.AreEqual ("13", t2.ToString ("ss"), "#B21");
			Assert.AreEqual ("A", t1.ToString ("%t"), "#B22");
			Assert.AreEqual ("P", t2.ToString ("%t"), "#B23");
			Assert.AreEqual ("AM", t1.ToString ("tt"), "#B24");
			Assert.AreEqual ("PM", t2.ToString ("tt"), "#B25");
			long offset = TimeZone.CurrentTimeZone.GetUtcOffset(t1).Ticks / 36000000000;
			// Must specify '+0' for GMT
			Assert.AreEqual (offset.ToString ("+#;-#;+0"), t1.ToString ("%z"), "#B26");
			Assert.AreEqual (offset.ToString ("+00;-00;+00"), t1.ToString ("zz"), "#B28");
			// This does not work in, eg banglore, because their timezone has an offset of
			// +05:30
			//Assert.AreEqual (offset.ToString("+00;-00;00") + ":00", t1.ToString ("zzz"), "#B28");
			Assert.AreEqual (" : ", t1.ToString (" : "), "#B29");
			Assert.AreEqual (" / ", t1.ToString (" / "), "#B30");
			Assert.AreEqual (" yyy ", t1.ToString (" 'yyy' "), "#B31");
			Assert.AreEqual (" d", t1.ToString (" \\d"), "#B32");
			Assert.AreEqual ("2002", t1.ToString ("yyy"), "#B33");
			Assert.AreEqual ("0002002", t1.ToString ("yyyyyyy"), "#B34");
			Assert.AreEqual ("999", t3.ToString ("yyy"), "#B33");
			Assert.AreEqual ("0999", t3.ToString ("yyyy"), "#B33");
		}

		[Test]
		public void TestToStringGenitive ()
		{
			DateTime dt = new DateTime (2010, 1, 2, 3, 4, 5);
			var dtf = new CultureInfo ("cs-cz").DateTimeFormat;

			Assert.AreEqual ("2 ledna", dt.ToString ("d MMMM", dtf), "#A1");
			Assert.AreEqual ("2. ledna", dt.ToString ("d. MMMM", dtf), "#A2");
			Assert.AreEqual ("leden", dt.ToString ("MMMM", dtf), "#A4");
			Assert.AreEqual ("leden", dt.ToString ("MMMMMMM", dtf), "#A5");
		}

		[Test]
		public void ParseExact_Format_Empty ()
		{
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", string.Empty, null);
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", string.Empty, null,
					DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void ParseExact_Format_Null ()
		{
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", (string) null, null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("format", ex.ParamName, "#A6");
			}

			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", (string) null, null,
					DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("format", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void ParseExact_Formats_Empty ()
		{
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", new string [0],
					null, DateTimeStyles.None);
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			string [] formats = new string [] { "G", string.Empty, "d" };
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", formats, null,
					DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void ParseExact_Formats_Null ()
		{
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", (string []) null,
					null, DateTimeStyles.None);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("formats", ex.ParamName, "#A6");
			}

			string [] formats = new string [] { "G", null, "d" };
			try {
				DateTime.ParseExact ("2002-02-25 04:25:13Z", formats, null,
					DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void ParseExact_String_Empty ()
		{
			try {
				DateTime.ParseExact (string.Empty, "G", null);
				Assert.Fail ("#A1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				DateTime.ParseExact (string.Empty, "G", null, DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				DateTime.ParseExact (string.Empty, new string [] { "G" }, null,
					DateTimeStyles.None);
				Assert.Fail ("#C1");
			} catch (FormatException ex) {
				// Format specifier was invalid
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}
		}

		[Test]
		public void ParseExact_String_Null ()
		{
			try {
				DateTime.ParseExact ((string) null, "G", null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("s", ex.ParamName, "#A6");
			}

			try {
				DateTime.ParseExact ((string) null, "G", null, DateTimeStyles.None);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("s", ex.ParamName, "#B6");
			}

			try {
				DateTime.ParseExact ((string) null, new string [] { "G" }, null,
					DateTimeStyles.None);
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("s", ex.ParamName, "#C6");
			}
		}

		[Test]
		public void TestParseExact3 ()
		{
			DateTime t1 = DateTime.ParseExact ("2002-02-25 04:25:13Z", "u", null);
			Assert.AreEqual (2002, t1.Year, "#1");
			Assert.AreEqual (02, t1.Month, "#2");
			Assert.AreEqual (25, t1.Day, "#3");
			Assert.AreEqual (04, t1.Hour, "#4");
			Assert.AreEqual (25, t1.Minute, "#5");
			Assert.AreEqual (13, t1.Second, "#6");
		}

		[Test]
		public void TestParseExact4 ()
		{
			// Checks 24hours format used together with the AM/PM designator
			string s = "6/28/2004 13:00:00 AM";
			string f = "M/d/yyyy HH':'mm':'ss tt";
			try {
				DateTime.ParseExact (s, f, CultureInfo.InvariantCulture);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			s = "6/28/2004 04:00:00 AM";
			DateTime.ParseExact (s, f, CultureInfo.InvariantCulture);

			s = "6/28/2004 06:00:00 PM";
			try {
				DateTime.ParseExact (s, f, CultureInfo.InvariantCulture);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			s = "6/28/2004 17:00:00 PM";
			DateTime.ParseExact (s, f, CultureInfo.InvariantCulture);
		}
		
		[Test]
		public void TestParseExact4_2 ()
		{
			// bug #63137
			DateTime.ParseExact ("Wed, 12 May 2004 20:51:09 +0200",
				@"ddd, d MMM yyyy H:m:s zzz",
				CultureInfo.CreateSpecificCulture("en-us"),
				DateTimeStyles.AllowInnerWhite);
		}

		[Test]
		public void TestParseExact5 ()
		{
			DateTime dt = DateTime.ParseExact ("Wed, 12 May 2004 20:51:09 -02:30",
							@"ddd, d MMM yyyy H:m:s zzz",
							CultureInfo.CreateSpecificCulture("en-us"),
							DateTimeStyles.AllowInnerWhite);
			dt = dt.ToUniversalTime ();
			Assert.AreEqual (23, dt.Hour, "Hour");
			Assert.AreEqual (21, dt.Minute, "Minute");
		}

		[Test]
		public void TestParseExactMiliseconds ()
		{
			string s = "1984-09-17T09:00:00.6Z";
			DateTime dt = DateTime.ParseExact (s, "yyyy-MM-dd'T'HH:mm:ss.FFF'Z'", CultureInfo.InvariantCulture);
			Assert.AreEqual (new DateTime (1984, 9, 17, 9, 0, 0, 600), dt, "#1");

			s = "1984-09-17T09:00:00Z";
			dt = DateTime.ParseExact (s, "yyyy-MM-dd'T'HH:mm:ss.FFF'Z'", CultureInfo.InvariantCulture);
			Assert.AreEqual (new DateTime (1984, 9, 17, 9, 0, 0, 0), dt, "#2");

			s = "1984-09-17T09:00:00Z";
			dt = DateTime.ParseExact (s, "yyyy-MM-dd'T'HH:mm:ssFFF'Z'", CultureInfo.InvariantCulture);
			Assert.AreEqual (new DateTime (1984, 9, 17, 9, 0, 0, 0), dt, "#3");
		}

		[Test]
		public void TestParseExact ()
		{
			// Standard patterns
			DateTime t1 = DateTime.ParseExact ("02/25/2002", "d", null);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A1");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002", "D", null);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A2");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25", "f", null);
			Assert.AreEqual (myTicks [3], t1.Ticks, "#A3");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13", "F", null);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#A4");
			t1 = DateTime.ParseExact ("02/25/2002 05:25", "g", null);
			Assert.AreEqual (myTicks [3], t1.Ticks, "#A5");
			t1 = DateTime.ParseExact ("02/25/2002 05:25:13", "G", null);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#A6");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 04:25:13", "U", null);
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			Assert.AreEqual (2002, t1.Year, "#A7");
			Assert.AreEqual (02, t1.Month, "#A8");
			Assert.AreEqual (25, t1.Day, "#A9");
			Assert.AreEqual (04, t1.Hour, "#A10");
			Assert.AreEqual (25, t1.Minute, "#A11");
			Assert.AreEqual (13, t1.Second, "#A12");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 04:25:13", "U", null);
			Assert.AreEqual ("Monday, 25 February 2002 04:25:13", t1.ToString ("U"), "#A13");

			DateTime t2 = new DateTime (DateTime.Today.Year, 2, 25);
			t1 = DateTime.ParseExact ("February 25", "m", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#B1");

			t2 = new DateTime (DateTime.Today.Year, 2, 25);
			t1 = DateTime.ParseExact ("February 25", "M", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#B2");

			t1 = DateTime.ParseExact ("Mon, 25 Feb 2002 04:25:13 GMT", "r", null);
			Assert.AreEqual (2002, t1.Year, "#C1");
			Assert.AreEqual (02, t1.Month, "#C2");
			Assert.AreEqual (25, t1.Day, "#C3");
			Assert.AreEqual (04, t1.Hour, "#C4");
			Assert.AreEqual (25, t1.Minute, "#C5");
			Assert.AreEqual (13, t1.Second, "#C6");

			t1 = DateTime.ParseExact ("Mon, 25 Feb 2002 04:25:13 GMT", "R", null);
			Assert.AreEqual (2002, t1.Year, "#D1");
			Assert.AreEqual (02, t1.Month, "#D2");
			Assert.AreEqual (25, t1.Day, "#D3");
			Assert.AreEqual (04, t1.Hour, "#D4");
			Assert.AreEqual (25, t1.Minute, "#D5");
			Assert.AreEqual (13, t1.Second, "#D6");

			t1 = DateTime.ParseExact ("2002-02-25T05:25:13", "s", null);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#E1");

			t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = DateTime.ParseExact ("05:25", "t", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#E2");

			t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = DateTime.ParseExact ("05:25:13", "T", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#E3");

			t2 = new DateTime (2002, 2, 1);
			t1 = DateTime.ParseExact ("2002 February", "y", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#E4");

			t2 = new DateTime (2002, 2, 1);
			t1 = DateTime.ParseExact ("2002 February", "Y", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#E5");

			// Custom patterns
			t2 = new DateTime (DateTime.Now.Year, 1, 25);
			t1 = DateTime.ParseExact ("25", "%d", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#F1");
			t1 = DateTime.ParseExact ("25", "dd", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#F2");

			t2 = new DateTime (DateTime.Today.Year, 2, 1);
			t1 = DateTime.ParseExact ("2", "%M", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G1");
			t1 = DateTime.ParseExact ("02", "MM", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G2");
			t1 = DateTime.ParseExact ("Feb", "MMM", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G3");
			t1 = DateTime.ParseExact ("February", "MMMM", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G4");

			t2 = new DateTime (2005, 1, 1);
			t1 = DateTime.ParseExact ("5", "%y", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G5");
			t1 = DateTime.ParseExact ("05", "yy", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G6");
			t1 = DateTime.ParseExact ("2005", "yyyy", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G7");

			t2 = DateTime.Today + new TimeSpan (5, 0, 0);
			t1 = DateTime.ParseExact ("5A", "ht", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G8");
			t1 = DateTime.ParseExact ("05A", "hht", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G9");

			t2 = DateTime.Today + new TimeSpan (15, 0, 0);
			t1 = DateTime.ParseExact ("3P", "ht", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G10");
			t1 = DateTime.ParseExact ("03P", "hht", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G11");

			t2 = DateTime.Today + new TimeSpan (5, 0, 0);
			t1 = DateTime.ParseExact ("5", "%H", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G12");

			t2 = DateTime.Today + new TimeSpan (15, 0, 0);
			t1 = DateTime.ParseExact ("15", "%H", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G13");
			t1 = DateTime.ParseExact ("15", "HH", null);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#G14");

			// Time zones
#if false
			// Fails durring DST for msft and mono
			t2 = DateTime.Today + new TimeSpan (17, 18, 0);
			t1 = DateTime.ParseExact ("11:18AM -5", "h:mmtt z", null);
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
				t1 += new TimeSpan(1, 0, 0);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#H1");
			
			t1 = DateTime.ParseExact ("11:18AM -05:00", "h:mmtt zzz", null);
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
				t1 += new TimeSpan(1, 0, 0);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#H2");

			t1 = DateTime.ParseExact ("7:18PM +03", "h:mmtt zz", null);
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
				t1 += new TimeSpan(1, 0, 0);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#H3");

			t1 = DateTime.ParseExact ("7:48PM +03:30", "h:mmtt zzz", null);
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
				t1 += new TimeSpan(1, 0, 0);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#H4");
#endif

			// Options
			t2 = DateTime.Today + new TimeSpan (16, 18, 0);
			t1 = DateTime.ParseExact ("11:18AM -5", "h:mmtt z",
						  null, DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#I1");

			t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13", "F",
						  null, DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#I2");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13",
						  "dddd, dd MMMM yyyy HH:mm:ss",
						  null, DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#I3");

			t1 = DateTime.ParseExact ("02/25/2002", "d", null,
						  DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#I4");

			t1 = DateTime.ParseExact ("    02/25/2002", "d", null,
						  DateTimeStyles.AllowLeadingWhite);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#I5");

			t1 = DateTime.ParseExact ("02/25/2002    ", "d", null,
						  DateTimeStyles.AllowTrailingWhite);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#I6");

			t1 = DateTime.ParseExact ("  02 / 25 / 2002    ", "d", null,
						  DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#I7");

			// Multi Custom Patterns
			string rfc1123_date = "r";
			string rfc850_date = "dddd, dd'-'MMM'-'yy HH':'mm':'ss 'GMT'";
			string asctime_date = "ddd MMM d HH':'mm':'ss yyyy";
			string [] formats = new string [] {rfc1123_date, rfc850_date, asctime_date};
			CultureInfo enUS = new CultureInfo("en-US", false);
			t1 = DateTime.ParseExact ("Sun, 06 Nov 1994 08:49:37 GMT", formats[0], enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J1");
			t1 = DateTime.ParseExact ("Sunday, 06-Nov-94 08:49:37 GMT", formats[1], enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J2");
			t1 = DateTime.ParseExact ("Sun Nov  6 08:49:37 1994", formats[2], enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J3");
			t1 = DateTime.ParseExact ("Sun, 06 Nov 1994 08:49:37 GMT", formats, enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J4");
			t1 = DateTime.ParseExact ("Sunday, 06-Nov-94 08:49:37 GMT", formats, enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J5");
			t1 = DateTime.ParseExact ("Sun Nov  6 08:49:37 1994", formats, enUS, 
						DateTimeStyles.AllowWhiteSpaces);
			Assert.AreEqual (myTicks [6], t1.Ticks, "#J6");
			t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13",
						"ddddddd, dd MMMMMMM yyyy HHHHH:mmmmm:sssss",
						null, DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual (myTicks[4], t1.Ticks, "#J7");
		
			// Bug 52274
			t1 = DateTime.ParseExact ("--12--", "--MM--" , null);
			Assert.AreEqual (12, t1.Month, "#K1");
			t1=DateTime.ParseExact ("--12-24", "--MM-dd" , null);
			Assert.AreEqual (24, t1.Day, "#K2");
			Assert.AreEqual (12, t1.Month, "#K3");
			t1=DateTime.ParseExact ("---24", "---dd" , null);
			Assert.AreEqual (24, t1.Day, "#K4");

			// Bug 63376
			t1 = DateTime.ParseExact ("18Aug2004 12:33:00", "ddMMMyyyy hh:mm:ss", new CultureInfo ("en-US"));
			Assert.AreEqual (0, t1.Hour, "hh allows 12, though it's useless");

			// Bug 74775
			DateTime.ParseExact ("Tue, 12 Apr 2005 10:10:04 +0100",
				"Tue, 12 Apr 2005 10:10:04 +0100", enUS);
			try {
				DateTime.ParseExact ("Tue, 12 Apr 2005 10:10:04 +00000",
					"ddd, dd MMM yyyy HH':'mm':'ss zzz", enUS);
				Assert.Fail ("#L1");
			} catch (FormatException) {
			}

			// Bug #75213 : literal escaping.
			t1 = DateTime.ParseExact ("20050707132527Z",
				"yyyyMMddHHmmss\\Z", CultureInfo.InvariantCulture);
			Assert.AreEqual (632563395270000000, t1.Ticks, "#L2");

			// XAttributeTest.CastDateTimeOffsets():#6b
			// It is said broken, probably due to timezone difference.
			//t1 = DateTime.ParseExact ("2039-10-31T12:34:56.7552+00:00", "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
			//			  CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			//Assert.AreEqual (643393064967552000, t1.Ticks, "#M");
		}

		[Test]
		public void TestParse2 ()
		{
			DateTime t1 = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			Assert.AreEqual (4, t1.Hour);
		}

		[Test]
		public void TestUtcOffset_Novell710512 ()
		{
			// test for bug Novell #710512
			if (TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero)
				Assert.Ignore("Test doesn't apply with current time zone");

			var localTime = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			var utcTime = TimeZone.CurrentTimeZone.ToUniversalTime(localTime);
			Assert.AreEqual (TimeSpan.Zero, TimeZone.CurrentTimeZone.GetUtcOffset (utcTime));
		}

		[Test]
		public void TestParseDateFirst ()
		{
			// Standard patterns
			CultureInfo USCultureInfo = new CultureInfo("en-US");
			DateTime t1 = DateTime.Parse ("02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A1");
			t1 = DateTime.Parse ("2002-02-25", USCultureInfo);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A2");
			t1 = DateTime.Parse ("Monday, 25 February 2002");
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A3");
			t1 = DateTime.Parse ("Monday, 25 February 2002 05:25");
			Assert.AreEqual (myTicks [3], t1.Ticks, "#A4");
			t1 = DateTime.Parse ("Monday, 25 February 2002 05:25:13");
			Assert.AreEqual (myTicks [4], t1.Ticks, "#A5");
			t1 = DateTime.Parse ("02/25/2002 05:25", USCultureInfo);
			Assert.AreEqual (myTicks [3], t1.Ticks, "#A6");
			t1 = DateTime.Parse ("02/25/2002 05:25:13", USCultureInfo);
			Assert.AreEqual (myTicks [4], t1.Ticks, "#A7");
			t1 = DateTime.Parse ("2002-02-25 04:25:13Z");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
			Assert.AreEqual (2002, t1.Year, "#A8");
			Assert.AreEqual (02, t1.Month, "#A9");
			Assert.AreEqual (25, t1.Day, "#A10");
			Assert.AreEqual (04, t1.Hour, "#A11");
			Assert.AreEqual (25, t1.Minute, "#A12");
			Assert.AreEqual (13, t1.Second, "#A13");
			t1 = DateTime.Parse ("Mon,02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks [0], t1.Ticks, "#A14");
			DateTime t2 = new DateTime (1999, 1, 2, 0, 3, 4);
			t1 = DateTime.Parse (t2.ToLongTimeString ());
			Assert.AreEqual (0, t1.Hour, "#A14");

			// parsed as UTC string
			t1 = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime (t1);
			Assert.AreEqual (2002, t1.Year, "#C1");
			Assert.AreEqual (02, t1.Month, "#C2");
			Assert.AreEqual (25, t1.Day, "#C3");
			Assert.AreEqual (4, t1.Hour, "#C4");
			Assert.AreEqual (25, t1.Minute, "#C5");
			Assert.AreEqual (13, t1.Second, "#C6");

			// Some date 'T' time formats
			t1 = DateTime.Parse ("2002-02-25T05:25");
			Assert.AreEqual (myTicks [3], t1.Ticks, "#D1");

			t1 = DateTime.Parse ("2002-02-25T05:25:13");
			Assert.AreEqual (myTicks [4], t1.Ticks, "#D1");
			t1 = DateTime.Parse ("2002-02-25T05:25:13.008");
			Assert.AreEqual (myTicks [2], t1.Ticks, "#D1");
			t1 = DateTime.Parse ("02-2002-25T05:25:13");
			Assert.AreEqual (myTicks [4], t1.Ticks, "#D1");

			// Day month
			t2 = new DateTime (DateTime.Today.Year, 2, 25);
			t1 = DateTime.Parse ("February 25", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#B1");

			t2 = new DateTime (DateTime.Today.Year, 2, 8);
			t1 = DateTime.Parse ("February 08", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#B2");

			t2 = new DateTime (DateTime.Today.Year, 2, 8);
			t1 = DateTime.Parse ("February 8", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D6");

			// Month year
			t2 = new DateTime (2002, 2, 1);
			t1 = DateTime.Parse ("2002 February");
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D4");

			t2 = new DateTime (2002, 2, 1);
			t1 = DateTime.Parse ("2002 February", new CultureInfo ("ja-JP"));
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D5");

			// bug #72132
			t2 = new DateTime (2002, 2, 25, 5, 25, 22);
			t1 = DateTime.Parse ("Monday, 25 February 2002 05:25:22",
				new CultureInfo ("hi-IN"));
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D7");
			t2 = new DateTime (2002, 2, 25, 5, 25, 0);
			t1 = DateTime.Parse ("Monday, 25 February 2002 05:25",
				new CultureInfo ("hi-IN"));
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D8");

			// MM-yyyy-dd + different time formats
			t1 = DateTime.Parse ("02-2002-25 05:25", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#E1");
			t1 = DateTime.Parse ("02-2002-25 05:25:13", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks, "#E1");
			t1 = DateTime.Parse ("02-2002-25 05:25:13 Mon", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks, "#E2");
			t1 = DateTime.Parse ("02-2002-25 05:25:13 Monday", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks, "#E3");
			t1 = DateTime.Parse ("02-2002-25 05:25:13.008", USCultureInfo);
			Assert.AreEqual (myTicks[2], t1.Ticks, "#E4");

			// Formats with timezone
			long offset = TimeZone.CurrentTimeZone.GetUtcOffset(t1).Ticks;
			long hourTicks = 36000000000L;
			long halfHourTicks = hourTicks / 2;
			t1 = DateTime.Parse ("02-2002-25 05:25+01", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks + hourTicks - offset, "#F1");
			t1 = DateTime.Parse ("02-2002-25 05:25-01", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks - hourTicks - offset, "#F2");
			t1 = DateTime.Parse ("02-2002-25 05:25+00:30", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks + hourTicks/2 - offset, "#F3");
			t1 = DateTime.Parse ("02-2002-25 05:25:13+02", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks + 2*hourTicks - offset, "#F4");

			// NET 1.0 doesn't accept second fractions and time zone.
			t1 = DateTime.Parse ("2002-02-25 05:25:13.008-02");
			Assert.AreEqual (myTicks[2], t1.Ticks - 2*hourTicks - offset, "#F5");
			// NET 1.0 doesn't parse well time zone with AM afterwards.
			t1 = DateTime.Parse ("02-25-2002 05:25:13-02 AM", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks - 2*hourTicks - offset, "#F6");
			t1 = DateTime.Parse ("25 Feb 2002 05:25:13-02 AM", USCultureInfo);
			Assert.AreEqual (myTicks[4], t1.Ticks - 2*hourTicks - offset, "#F6");
		}

		[Test]
		public void TestParseTimeFirst ()
		{
			CultureInfo USCultureInfo = new CultureInfo("en-US");

			// Hour only patterns
			DateTime t2 = DateTime.Today + new TimeSpan (5,25,0);
			DateTime t1 = DateTime.Parse ("05:25");
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#C1");
			t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = DateTime.Parse ("05:25:13");
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#B2");

			// Test with different date formats
			t1 = DateTime.Parse ("05:25 02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#B1");
			t1 = DateTime.Parse ("05:25:13 2002-02-25");
			Assert.AreEqual (myTicks[4], t1.Ticks, "#B2");
			t1 = DateTime.Parse ("05:25:13.008 02-2002-25");
			Assert.AreEqual (myTicks[2], t1.Ticks, "#B3");
			t1 = DateTime.Parse ("05:25:13.008 Feb 25 2002");
			Assert.AreEqual (myTicks[2], t1.Ticks, "#B4");
			t1 = DateTime.Parse ("05:25:13.008 25 Feb 2002");
			Assert.AreEqual (myTicks[2], t1.Ticks, "#B5");

			// Add AM and day of the week
			t1 = DateTime.Parse ("AM 05:25:13 2002-02-25");
			Assert.AreEqual (myTicks[4], t1.Ticks, "#C1");
			t1 = DateTime.Parse ("Monday05:25 02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#C2");
			t1 = DateTime.Parse ("Mon 05:25 AM 02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#C3");
			t1 = DateTime.Parse ("AM 05:25 Monday, 02/25/2002", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#C4");
			t1 = DateTime.Parse ("05:25 02/25/2002 Monday", USCultureInfo);
			Assert.AreEqual (myTicks[3], t1.Ticks, "#C5");
			t1 = DateTime.Parse ("PM 03:25:13.008 02-2002-25");
			Assert.AreEqual (myTicks[1], t1.Ticks, "#C6");

			// ASP.NET QuickStarts
			t2 = new DateTime (2002, 10, 7, 15, 6, 0);
			t1 = DateTime.Parse ("3:06 PM 10/7/2002", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D1");
			t2 = new DateTime (2002, 10, 7, 15, 6, 0);
			t1 = DateTime.Parse ("3:06 pm 10/7/2002", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D2");
			t2 = new DateTime (2002, 10, 7, 3, 6, 0);
			t1 = DateTime.Parse ("3:06 AM 10/7/2002", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D3");
			t2 = new DateTime (2002, 10, 7, 3, 6, 0);
			t1 = DateTime.Parse ("3:06 am 10/7/2002", USCultureInfo);
			Assert.AreEqual (t2.Ticks, t1.Ticks, "#D4");
		}

		[Test]
		public void TestParseWithDifferentShortDatePatterns ()
		{
			CultureInfo cultureInfo = new CultureInfo("en-US");
			DateTimeFormatInfo dateFormatInfo = cultureInfo.DateTimeFormat;
			DateTime t1 = DateTime.Parse ("02/01/2003", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#A1");

			// Day, month year behaviour
			dateFormatInfo.ShortDatePattern = "dd/MM/yyyy";
			t1 = DateTime.Parse ("01/02/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#B1");
			t1 = DateTime.Parse ("01/02/2003", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#B2");
			t1 = DateTime.Parse ("2003/02/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#B3");
			t1 = DateTime.Parse ("01/Feb/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#B4");
			t1 = DateTime.Parse ("Feb/01/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#B5");

			// Month, day year behaviour
			dateFormatInfo.ShortDatePattern = "MM/dd/yyyy";
			t1 = DateTime.Parse ("02/01/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#C1");
			t1 = DateTime.Parse ("02/01/2003", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#C2");
			t1 = DateTime.Parse ("2003/02/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#C3");
			t1 = DateTime.Parse ("01/Feb/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#C4");
			t1 = DateTime.Parse ("Feb/01/03", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#C5");

			// Year, month day behaviour
			dateFormatInfo.ShortDatePattern = "yyyy/MM/dd";
			t1 = DateTime.Parse ("03/02/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#D1");
			t1 = DateTime.Parse ("02/01/2003", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#D2");
			t1 = DateTime.Parse ("2003/02/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#D3");
			t1 = DateTime.Parse ("03/Feb/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#D4");
			t1 = DateTime.Parse ("Feb/03/01", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#D5");

			// Year, day month behaviour
			// Note that no culture I am aware of has this pattern, and indeed
			dateFormatInfo.ShortDatePattern = "yyyy/dd/MM";
			t1 = DateTime.Parse ("03/01/02", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#E1");
			t1 = DateTime.Parse ("01/02/2003", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#E2");

			t1 = DateTime.Parse ("2003/01/02", cultureInfo);
			Assert.AreEqual (myTicks[7], t1.Ticks, "#E3");

			// For some reason the following throws an exception on .Net
			// t1 = DateTime.Parse ("03/Feb/01", cultureInfo);
			// Assert.AreEqual (myTicks[7], t1.Ticks, "#E4");
			// t1 = DateTime.Parse ("03/01/Feb", cultureInfo);
			// Assert.AreEqual (myTicks[7], t1.Ticks, "#E5");
			// t1 = DateTime.Parse ("Feb/01/03", cultureInfo);
			// Assert.AreEqual (myTicks[7], t1.Ticks, "#E6");
		}

		[Test]
		public void TestParseWithDifferentMonthDayPatterns ()
		{
			CultureInfo cultureInfo = new CultureInfo("en-US");
			DateTimeFormatInfo dateFormatInfo = cultureInfo.DateTimeFormat;
			DateTime t1 = DateTime.Parse ("Feb 03", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#A1");
			Assert.AreEqual (3, t1.Day, "#A2");

			// Day month behaviour
			dateFormatInfo.MonthDayPattern = "dd/MM";

			t1 = DateTime.Parse ("Feb 03", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#B1");
			Assert.AreEqual (1, t1.Day, "#B2");
			Assert.AreEqual (2003, t1.Year, "#B3");

			t1 = DateTime.Parse ("03/02", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#B6");
			Assert.AreEqual (3, t1.Day, "#B7");
			t1 = DateTime.Parse ("03 Feb", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#B8");
			Assert.AreEqual (3, t1.Day, "#B9");

			// Month day behaviour
			dateFormatInfo.MonthDayPattern = "MM/dd";
			t1 = DateTime.Parse ("Feb 03", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#C1");
			Assert.AreEqual (3, t1.Day, "#C2");
			t1 = DateTime.Parse ("02/03", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#C3");
			Assert.AreEqual (3, t1.Day, "#C4");
			t1 = DateTime.Parse ("03 Feb", cultureInfo);
			Assert.AreEqual (2, t1.Month, "#C5");
			Assert.AreEqual (3, t1.Day, "#C6");
		}

		[Test]
		public void TestParse3 ()
		{
			string s = "Wednesday, 09 June 2004";
			DateTime.ParseExact (s, "dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
			try {
				DateTime.ParseExact (s, "dddd, dd MMMM yyyy", new CultureInfo ("ja-JP"));
				Assert.Fail ("ja-JP culture does not support format \"dddd, dd MMMM yyyy\"");
			} catch (FormatException) {
			}

			// Ok, now we can assume ParseExact() works expectedly.

			DateTime.Parse (s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
			DateTime.Parse (s, new CultureInfo ("ja-JP"), DateTimeStyles.AllowWhiteSpaces);
			//DateTime.Parse (s, null); currently am not sure if it works for _every_ culture.
		}


		[Test] // bug #74936
		public void TestParse4 ()
		{
			try {
				DateTime.Parse("1");
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				DateTime.Parse("1000");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				DateTime.Parse("8:");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test] // bug #71289
		public void Parse_Bug71289a ()
		{
			DateTime.Parse ("Sat,,,,,, 01 Oct 1994 03:00:00", CultureInfo.InvariantCulture);
		}

		[Test]
		public void Parse_Bug71289b ()
		{
			// more example...
			DateTime.Parse ("Sat,,, 01,,, Oct,,, ,,,1994 03:00:00", CultureInfo.InvariantCulture);
		}

		[Test]
		public void TryParse_Bug11630 ()
		{
			DateTime parsed;

			Assert.IsTrue (DateTime.TryParse ("10Feb2013", out parsed));
			Assert.AreEqual (new DateTime (2013, 2, 10), parsed);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Parse_CommaAfterHours ()
		{
			// ',' after 03 is not allowed.
			DateTime.Parse ("Sat,,, 01,,, Oct,,, ,,,1994 03,:00:00", CultureInfo.InvariantCulture);
		}

		[Test] // bug #72788
		public void Parse_Bug72788 ()
		{
			DateTime dt = DateTime.Parse ("21/02/05", new CultureInfo ("fr-FR"));
			Assert.AreEqual (2005, dt.Year, "#1");
			Assert.AreEqual (02, dt.Month, "#2");
			Assert.AreEqual (21, dt.Day, "#3");
		}

		[Test] // bug #322510
		public void Parse_HourDesignator ()
		{
			DateTime dt;
			DateTime now = DateTime.Now;

			dt = DateTime.Parse ("12:00:00 AM", new CultureInfo ("en-US"));
			Assert.AreEqual (now.Year, dt.Year, "#A1");
			Assert.AreEqual (now.Month, dt.Month, "#A2");
			Assert.AreEqual (now.Day, dt.Day, "#A3");
			Assert.AreEqual (0, dt.Hour, "#A4");
			Assert.AreEqual (0, dt.Minute, "#A5");
			Assert.AreEqual (0, dt.Second, "#A6");
			Assert.AreEqual (0, dt.Millisecond, "#A7");

			dt = DateTime.Parse ("12:00:00 PM", new CultureInfo ("en-US"));
			Assert.AreEqual (now.Year, dt.Year, "#B1");
			Assert.AreEqual (now.Month, dt.Month, "#B2");
			Assert.AreEqual (now.Day, dt.Day, "#B3");
			Assert.AreEqual (12, dt.Hour, "#B4");
			Assert.AreEqual (0, dt.Minute, "#B5");
			Assert.AreEqual (0, dt.Second, "#B6");
			Assert.AreEqual (0, dt.Millisecond, "#B7");
		}

		[Test]
		public void Parse_Bug53023b ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.SpecificCultures)) {
				if (ci.Name == "ar-SA")
					continue;

				try {
					DateTime.Parse ("01-Sep-05", ci);

					// FIXME: Our UmAlQuraCalendar/HijriCalendar calendars support month days - 1 only (fail on last day in month)
					if (ci.Calendar is UmAlQuraCalendar || ci.Calendar is HijriCalendar)
						continue;

					DateTime.Parse ("4:35:35 AM", ci);
				} catch {
					Assert.Fail (ci.Name);
				}
			}
		}

		[Test]
		public void Parse_SameTimeAndDateSeparator ()
		{
			var fiFI = (CultureInfo) CultureInfo.GetCultureInfo("fi-FI").Clone();

			fiFI.DateTimeFormat.TimeSeparator = fiFI.DateTimeFormat.DateSeparator;

			var dt = DateTime.Parse("4.3.2010", fiFI);

			Assert.AreEqual (2010, dt.Year, "#1");
			Assert.AreEqual (3, dt.Month, "#2");
			Assert.AreEqual (4, dt.Day, "#3");
		}


		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Parse_RequireSpaceSeparator ()
		{
			DateTime.Parse ("05:25:132002-02-25", CultureInfo.InvariantCulture);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Parse_DontAccept2DigitsYears ()
		{
			// don't allow 2 digit years where we require 4.
			DateTime.ParseExact ("05", "yyyy", CultureInfo.InvariantCulture);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Parse_DontAcceptEmptyHours ()
		{
			DateTime.ParseExact (":05", "H:m", CultureInfo.InvariantCulture);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Parse_DontAcceptEmptyMinutes ()
		{
			DateTime.ParseExact ("0::0", "H:m:s", CultureInfo.InvariantCulture);
		}

		[Test]
		public void ParseCOMDependentFormat ()
		{
			// Japanese format.
			DateTime.Parse (String.Format (
				"{0}\u5E74{1}\u6708{2}\u65E5 {3}\u6642{4}\u5206{5}\u79D2",
				2006, 3, 1, 15, 32, 42), new CultureInfo (""));

			try {
				// incorrect year mark.
				DateTime.Parse (String.Format (
					"{0}\u4E00{1}\u6708{2}\u65E5 {3}\u6642{4}\u5206{5}\u79D2",
					2006, 3, 1, 15, 32, 42), new CultureInfo (""));
				Assert.Fail ();
			} catch (FormatException) {
			}
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void ParseFormatException1 ()
		{
			// Following string is not a correct French date i.e.
			// MM/dd/yyyy HH:mm:ss since it expects d/M/yyyy HH:mm:ss
			// instead (however fr-FR accepts both MM/dd/yyyy and
			// dd/MM/yyyy, which means that we can't just throw exceptions 
			// on overflow).
			String frDateTime = "11/13/2003 11:28:15";
			IFormatProvider format = new CultureInfo("fr-FR", true);
			DateTime t1 = DateTime.Parse(frDateTime, format);
		}
	
		[Test]
		[ExpectedException(typeof (FormatException))]
		public void ParseFormatExceptionForInvalidYear ()
		{
			// Bug #77633.  In .NET 1..1, the expected exception is ArgumentOutOfRangeException
			// In .NET 2.0, the expected exception is FormatException
			// build a string with the year of 5 digits
			string s = "1/1/10000";
			DateTime dt = DateTime.Parse (s);
		}
	
		[Test]
		public void TestOA ()
		{
			double number=5000.41443;
			DateTime d = DateTime.FromOADate(number);
			DTAssertEquals (d, new DateTime(1913, 9, 8, 9, 56, 46, 0), Resolution.Second, "#1");
			Assert.AreEqual (d.ToOADate(), number, "#2");
		}

		[Test]
		public void ParseAllowsQueerString ()
		{
			DateTime.Parse ("Sat,,,,,, 01 Oct 1994 03:00:00", CultureInfo.InvariantCulture);
		}

		[Test]
		public void ParseUtcNonUtc ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("es-ES");

			CultureInfo ci;
			string s, s2, s3, d;
			DateTime dt;
			DateTimeFormatInfo dfi = DateTimeFormatInfo.InvariantInfo;
			s = dfi.UniversalSortableDateTimePattern;
			s2 = "r";

			s3 = "s";

			long tick1 = 631789220960000000; // 2003-01-23 12:34:56 as is
			long tick2 = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (tick1)).Ticks; // adjusted to local time

			// invariant
			ci = CultureInfo.InvariantCulture;

			d = "2003/01/23 12:34:56";
			dt = DateTime.Parse (d, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#1:" + d);

			d = "2003/01/23 12:34:56 GMT";
			dt = DateTime.Parse (d, ci);
			Assert.AreEqual (tick2, dt.Ticks, "#2:" + d);

			d = "Thu, 23 Jan 2003 12:34:56 GMT";
			dt = DateTime.ParseExact (d, s2, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#3:" + d);

			d = "2003-01-23 12:34:56Z";
			dt = DateTime.ParseExact (d, s, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#4:" + d);

			d = "2003-01-23T12:34:56";
			dt = DateTime.ParseExact (d, s3, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#5:" + d);

			// ja-JP ... it should be culture independent
			ci = new CultureInfo ("ja-JP");

			d = "2003/01/23 12:34:56";
			dt = DateTime.Parse (d, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#6:" + d);

			d = "2003/01/23 12:34:56 GMT";
			dt = DateTime.Parse (d, ci);
			Assert.AreEqual (tick2, dt.Ticks, "#7:" + d);

			d = "Thu, 23 Jan 2003 12:34:56 GMT";
			dt = DateTime.ParseExact (d, s2, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#8:" + d);

			d = "2003-01-23 12:34:56Z";
			dt = DateTime.ParseExact (d, s, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#9:" + d);

			d = "2003-01-23T12:34:56";
			dt = DateTime.ParseExact (d, s3, ci);
			Assert.AreEqual (tick1, dt.Ticks, "#10:" + d);
		}

		[Test]
		public void TimeZoneAdjustment ()
		{
			CultureInfo ci = Thread.CurrentThread.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
				DateTime d1 = DateTime.ParseExact ("2004/06/30", "yyyy/MM/dd", null);
				DateTime d2 = DateTime.ParseExact ("2004/06/30Z", "yyyy/MM/dd'Z'", null);
				DateTime d3 = DateTime.ParseExact ("Wed, 30 Jun 2004 00:00:00 GMT", "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", null);
				DateTime d4 = DateTime.ParseExact ("2004-06-30 00:00:00Z", "yyyy'-'MM'-'dd HH':'mm':'ss'Z'", null);
				StringWriter sw = new StringWriter ();
				sw.Write ("{0} {1}", d1.Ticks, d1);
				Assert.AreEqual ("632241504000000000 6/30/2004 12:00:00 AM", sw.ToString (), "#1");
				sw.GetStringBuilder ().Length = 0;
				sw.Write ("{0} {1}", d2.Ticks, d2);
				Assert.AreEqual ("632241504000000000 6/30/2004 12:00:00 AM", sw.ToString (), "#2");
				sw.GetStringBuilder ().Length = 0;
				sw.Write ("{0} {1}", d3.Ticks, d3);
				Assert.AreEqual ("632241504000000000 6/30/2004 12:00:00 AM", sw.ToString (), "#3");
				sw.GetStringBuilder ().Length = 0;
				sw.Write ("{0} {1}", d4.Ticks, d4);
				Assert.AreEqual ("632241504000000000 6/30/2004 12:00:00 AM", sw.ToString (), "#4");
			} finally {
				Thread.CurrentThread.CurrentCulture = ci;
			}

			// bug #76082
			Assert.AreEqual (DateTime.MinValue, DateTime.ParseExact ("00010101T00:00:00",
					"yyyyMMdd'T'HH':'mm':'ss", DateTimeFormatInfo.InvariantInfo), "#5");
		}

		[Test]
		public void DateTimeStylesAdjustToUniversal ()
		{
			// bug #75995 : AdjustToUniversal
			DateTime t1 = DateTime.Parse ("2005-09-05T22:29:00Z",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual ("2005-09-05 22:29:00Z", t1.ToString ("u"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromOADate_Min () 
		{
			// minimum documented value isn't inclusive
			DateTime.FromOADate (-657435.0d);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromOADate_Max () 
		{
			// maximum documented value isn't inclusive
			DateTime.FromOADate (2958466.0d);
		}

		[Test]
		public void FromOADate ()
		{
			// Note: OA (OLE Automation) dates aren't timezone sensitive
			Assert.AreEqual (599264352000000000, DateTime.FromOADate (0.0d).Ticks, "#1");
			Assert.AreEqual (31242239136000000, DateTime.FromOADate (-657434.999d).Ticks, "#2");
			Assert.AreEqual (3155378975136000000, DateTime.FromOADate (2958465.999d).Ticks, "#3");
		}

		[Test]
		public void ToOADate ()
		{
			// Note: OA (OLE Automation) dates aren't timezone sensitive
			DateTime d = new DateTime (0);
			Assert.AreEqual (0.0d, d.ToOADate (), "#1");
			d = new DateTime (599264352000000000);
			Assert.AreEqual (0.0d, d.ToOADate (), "#2");
			d = new DateTime (31242239136000000);
			Assert.AreEqual ("-657434.999", d.ToOADate ().ToString (), "#3");
			d = new DateTime (3155378975136000000);
			Assert.AreEqual (2958465.999d, d.ToOADate (), "#4");
		}

		[Test]
		public void ToOADate_OverMax ()
		{
			DateTime d = new DateTime (3155378975136000001);
			Assert.AreEqual (2958465.999d, d.ToOADate ());
		}

		[Test]
		public void ToOADate_UnderMin ()
		{
			DateTime d = new DateTime (31242239135999999);
			Assert.AreEqual (-657434.999d, d.ToOADate ());
		}

		[Test]
		public void ToOADate_MinValue ()
		{
			Assert.AreEqual (0, DateTime.MinValue.ToOADate ());
		}

		[Test] // bug52075
		public void MaxValueYear ()
		{
			Assert.AreEqual ("9999", DateTime.MaxValue.Year.ToString ());
		}

		[Test]
		public void X509Certificate () 
		{
			// if this test fails then *ALL* or *MOST* X509Certificate tests will also fails
			DateTime dt = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null);
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "#1");
			dt = dt.ToUniversalTime ();
			Assert.AreEqual (DateTimeKind.Utc, dt.Kind, "#2");
			Assert.AreEqual ("03/12/1996 18:38:47", dt.ToString (), "#3");

			// technically this is invalid (PKIX) because of the missing seconds but it exists so...
			dt = DateTime.ParseExact ("9602231915Z", "yyMMddHHmmZ", null);
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "#4");
			dt = dt.ToUniversalTime ();
			Assert.AreEqual (DateTimeKind.Utc, dt.Kind, "#5");
			Assert.AreEqual ("02/23/1996 19:15:00", dt.ToString (), "#6");

			dt = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null, DateTimeStyles.AdjustToUniversal);
			Assert.AreEqual (DateTimeKind.Utc, dt.Kind, "#7");
		}

		[Test]
		public void ZLiteral ()
		{
			// However, "Z" and "'Z'" are different.
			DateTime dt = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmss'Z'", null);
			DateTime dtz = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null);
			Assert.AreEqual (DateTimeKind.Unspecified, dt.Kind, "#1");
			dt = dt.ToLocalTime ();
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "#2");
			Assert.AreEqual (DateTimeKind.Local, dtz.Kind, "#3");
			Assert.AreEqual (dt, dtz, "#4");
		}

		[Test] // bug 56436
		public void QuotedFormat ()
		{
			string date = "28/Mar/2004:19:12:37 +0200";
			string [] expectedFormats = {"dd\"/\"MMM\"/\"yyyy:HH:mm:ss zz\"00\""};
			DateTime.ParseExact (date, expectedFormats, null, DateTimeStyles.AllowWhiteSpaces);
		}

		[Test]
		public void CultureIndependentTests ()
		{
			// Here I aggregated some tests mainly because of test 
			// performance (iterating all the culture is heavy process).
		
			for (int i = 0; i < 32768; i++) {
				CultureInfo ci = null;
				string stage = "init";
				try {
					try {
						ci = new CultureInfo (i);
						// In fact InvatiantCulture is not neutral.
						// See bug #59716.
						if (ci.IsNeutralCulture && ci != CultureInfo.InvariantCulture)
							continue;
					} catch (Exception) {
						continue;
					}
					Thread.CurrentThread.CurrentCulture = ci;
					DateTime dt;

					switch (ci.LCID) {
					case 1025: // ar-SA
					case 2559: // qps-plocm
						continue; // fails too many tests below on .NET.
					}

					// Common patterns
					// X509Certificate pattern is _always_ accepted.
					stage = "1";
					dt = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null);

					stage = "2";
					// fails with many cultures on .NET.
	//				if (i != 127)
	//					dt = DateTime.Parse ("19960312183847Z");

					stage = "3";
					dt = DateTime.Parse ("2004-05-26T03:29:01.1234567");
					stage = "4";
					dt = DateTime.Parse ("2004-05-26T03:29:01.1234567-07:00");

					// memo: the least LCID is 127, and then 1025(ar-SA)

					// "th-TH" locale rejects them since in
					// ThaiBuddhistCalendar the week of a day is different.
					// (and also for years).
					switch (ci.LCID) {
					case 1054:
					case 1128: // ha-Latn-NG
						break;
					default:
						try {
							stage = "5";
							dt = DateTime.Parse ("Sat, 29 Oct 1994 12:00:00 GMT", ci);
						} catch (FormatException ex) {
							Assert.Fail (String.Format ("stage 5.1 RFC1123: culture {0} {1} failed: {2}", i, ci, ex.Message));
						}

						// bug #47720
						if (dt != TimeZone.CurrentTimeZone.ToUniversalTime (dt))
							Assert.IsTrue (12 != dt.Hour, String.Format ("bug #47720 on culture {0} {1}", ci.LCID, ci));

						// variant of RFC1123
						try {
							stage = "6";
							dt = DateTime.Parse ("Sat, 1 Oct 1994 03:00:00", ci);
						} catch (FormatException ex) {
							Assert.Fail (String.Format ("stage 6.1 RFC1123 variant: culture {0} {1} failed: {2}", i, ci, ex.Message));
						}
						stage = "7";
						Assert.AreEqual (3, dt.Hour, String.Format ("stage 7.1 RFC1123 variant on culture {0} {1}", ci.LCID, ci));
						break;
					}

					switch (ci.LCID) {
					case 1054: // th-TH
					case 1123: // ps-AF
					case 1125: // div-MV
					case 1164: // prs-AF
						break;
					default:
						stage = "8";
						// 02/25/2002 04:25:13 as is
						long tick1 = 631502079130000000;
						long tick2 = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (tick1)).Ticks; // adjusted to local time
						dt = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT", ci);
						Assert.AreEqual (tick2, dt.Ticks, String.Format ("GMT variant. culture={0} {1}", i, ci));
						break;
					}


					// ka-GE rejects these formats under MS.NET. 
					// I wonder why. Also, those tests fail under .NET 1.0.
					if (ci.LCID != 1079) {
						stage = "9";
						dt = DateTime.Parse ("2002-02-25");
						stage = "10";
						dt = DateTime.Parse ("2002-02-25Z");
						stage = "11";
						dt = DateTime.Parse ("2002-02-25T19:20:00+09:00");
						switch (ci.LCID) {
						case 1038: // FIXME: MS passes this culture.
						case 1062: // FIXME: MS passes this culture.
						case 1078: // MS does not pass this culture. Dunno why.
							break;
						default:
							break;
						}
						stage = "14";
						dt = DateTime.Parse ("2002-02-25 12:01:03");
						stage = "17";
						if (ci.DateTimeFormat.TimeSeparator != ".")
							dt = DateTime.Parse ("2002.02.25 12:01:03");
						stage = "18";
						dt = DateTime.Parse ("2003/01/23 01:34:56 GMT");
						dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
						Assert.AreEqual (1, dt.Hour, String.Format ("stage 18.1 RFC1123 UTC {0} {1}", i, ci));
						stage = "19";
						// This test was fixed from 12:34:56 to
						// 01:34:56 since 1078 af-ZA failed
						// because of hour interpretation
						// difference (af-ZA expects 0).
						// (IMHO it is MS BUG though.)
						dt = DateTime.Parse ("2003/01/23 12:34:56 GMT");
						dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
						if (i != 1078)
							Assert.AreEqual (12, dt.Hour, String.Format ("stage 18.1 RFC1123 UTC {0} {1}", i, ci));
					}
				} catch (FormatException ex) {
					Assert.Fail (String.Format ("stage {3}: Culture {0} {1} failed: {2}", i, ci, ex.Message, stage));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToFileTime_MinValue () 
		{
			DateTime.FromFileTime (Int64.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToFileTime_Negative () 
		{
			DateTime.FromFileTime (-1);
		}

		[Test]
		public void ToFileTime () 
		{
			long u = DateTime.FromFileTimeUtc (0).Ticks;
			Assert.AreEqual (504911232000000000, u, "#A1");
			long max = DateTime.MaxValue.Ticks - 504911232000000000; // w32file_epoch
			Assert.AreEqual (3155378975999999999, DateTime.FromFileTimeUtc (max).Ticks, "#A2");

			long t = DateTime.FromFileTime (0).Ticks;
			Assert.IsTrue (t > (u - TimeSpan.TicksPerDay), "#B1");
			Assert.IsTrue (t < (u + TimeSpan.TicksPerDay), "#B2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToFileTimeUtc_MinValue () 
		{
			DateTime.FromFileTimeUtc (Int64.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToFileTimeUtc_Negative () 
		{
			DateTime.FromFileTimeUtc (-1);
		}

		[Test]
		public void ToFileTimeUtc ()
		{
			// Randomly generated time outside DST.
			var utc = new DateTime (1993, 01, 28, 08, 49, 48, DateTimeKind.Utc);
			var local = utc.ToLocalTime ();
			var unspecified = new DateTime (1993, 01, 28, 08, 49, 48);

			Assert.AreEqual (DateTimeKind.Utc, utc.Kind);
			Assert.AreEqual (DateTimeKind.Local, local.Kind);
			Assert.AreEqual (DateTimeKind.Unspecified, unspecified.Kind);

			Assert.AreEqual (628638077880000000, utc.Ticks);
			Console.WriteLine (local.Ticks - utc.Ticks);

			var offset = TimeZone.CurrentTimeZone.GetUtcOffset (local);

			var utcFt = utc.ToFileTime ();
			var localFt = local.ToFileTime ();
			var unspecifiedFt = unspecified.ToFileTime ();

			var utcUft = utc.ToFileTimeUtc ();
			var localUft = local.ToFileTimeUtc ();
			var unspecifiedUft = unspecified.ToFileTimeUtc ();

			Assert.AreEqual (123726845880000000, utcFt);
			Assert.AreEqual (utcFt, localFt);

			Assert.AreEqual (offset.Ticks, utcFt - unspecifiedFt);

			Assert.AreEqual (utcFt, utcUft);
			Assert.AreEqual (utcFt, localUft);
			Assert.AreEqual (utcFt, unspecifiedUft);
		}
		
		[Test]
		public void FromFileTimeUtcTest ()
		{
			DateTime dt = DateTime.FromFileTimeUtc (123456);
			Assert.AreEqual (dt.Kind, DateTimeKind.Utc, "#Kind");
			Assert.AreEqual (dt.Ticks, 504911232000123456, "#Ticks");
		}

 		[Test]
		public void Milliseconds ()
		{
			DateTime dt = DateTime.Parse ("2004-05-26T03:29:01.1234567-07:00");
			dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
			Assert.AreEqual (632211641411234567, dt.Ticks);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseNotExact ()
		{
			// The error reported is:
			// String was not recognized as valid DateTime
			DateTime dt = DateTime.Parse ("2004-05-26T03:29:01-07:00 foo");
			dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
			Assert.AreEqual (632211641410000000, dt.Ticks);
		}

		[Test]
		public void ParseExact_Bug80094 ()
		{
			// we can safely change the curernt culture, as the original value will
			// be restored on TearDown
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("ja-JP");
			string y = string.Format ("{0}-{1}-{2} {3}", DateTime.Now.Year.ToString (),
				"11", "29", "06:34");
			DateTime date = DateTime.ParseExact (y, "yyyy-MMM-dd hh:mm", null);
			Assert.AreEqual (DateTime.Now.Year, date.Year, "#1");
			Assert.AreEqual (11, date.Month, "#2");
			Assert.AreEqual (29, date.Day, "#3");
			Assert.AreEqual (6, date.Hour, "#4");
			Assert.AreEqual (34, date.Minute, "#5");
			Assert.AreEqual (0, date.Second, "#6");
			Assert.AreEqual (0, date.Millisecond, "#7");
		}

		[Test]
		public void ParseExact_Bug324845 ()
		{
			DateTime ctime = new DateTime (2007, 7, 23, 19, 19, 45);
			ctime = ctime.ToUniversalTime ();
			string instr = ctime.ToString ("yyyyMMddHHmmss");

			DateTime t = DateTime.ParseExact (instr, "yyyyMMddHHmmss", null, DateTimeStyles.AssumeUniversal);
			Assert.AreEqual (2007, t.Year);
			Assert.AreEqual (7, t.Month);
			Assert.AreEqual (23, t.Day);
			Assert.AreEqual (19, t.Hour);
			Assert.AreEqual (19, t.Minute);
			Assert.AreEqual (45, t.Second);

		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExactIsExact()
		{
			DateTime.ParseExact ("2004-05-26T03:29:01-07:00 foo", "yyyy-MM-ddTHH:mm:sszzz", null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExactDoesNotEatZ ()
		{
			DateTime.ParseExact ("2004-05-26T03:29:01", "yyyy-MM-ddTHH:mm:ssZ", null);
		}

		[Test]
		public void ParseExactMilliseconds ()
		{
			DateTime dt = DateTime.ParseExact ("2004-05-26T03:29:01.1234567-07:00", "yyyy-MM-ddTHH:mm:ss.fffffffzzz", null);
			dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
			Assert.AreEqual (632211641411234567, dt.Ticks);
		}

		[Test]
		public void NoColonTimeZone ()
		{
			Assert.IsTrue (DateTime.Parse ("2004-05-26T03:29:01-0700").Ticks
				!= DateTime.Parse ("2004-05-26T03:29:01-0800").Ticks);
		}

		[Test]
		public void WithColonTimeZone ()
		{
			Assert.IsTrue (DateTime.Parse ("2004-05-26T03:29:01-07:00").Ticks
				!= DateTime.Parse ("2004-05-26T03:29:01-08:00").Ticks);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void EmptyFormatPattern ()
		{
			DateTime.ParseExact (String.Empty, String.Empty, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Boolean () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (bool), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Byte () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (byte), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Char () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (char), null);
		}

		[Test]
		public void IConvertible_ToType_DateTime () 
		{
			DateTime dt = DateTime.Now;
			DateTime dt2 = (DateTime) ((IConvertible)dt).ToType (typeof (DateTime), null);
			Assert.IsTrue (dt.Equals (dt2));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_DBNull () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (DBNull), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Decimal () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (decimal), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Double () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (double), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IConvertible_ToType_Empty () 
		{
			((IConvertible)DateTime.Now).ToType (null, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Int16 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (short), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Int32 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (int), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Int64 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (long), null);
		}

		[Test]
		public void IConvertible_ToType_Object () 
		{
			DateTime dt = DateTime.Now;
			object o = ((IConvertible)dt).ToType (typeof (object), null);
			Assert.IsTrue (dt.Equals (o));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_SByte () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (sbyte), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_Single () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (float), null);
		}

		[Test]
		public void IConvertible_ToType_String () 
		{
			DateTime dt = DateTime.Now;
			string s = (string) ((IConvertible)dt).ToType (typeof (string), null);
			Assert.AreEqual (s, dt.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_UInt16 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (ushort), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_UInt32 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (uint), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IConvertible_ToType_UInt64 () 
		{
			((IConvertible)DateTime.Now).ToType (typeof (ulong), null);
		}

		[Test]
		public void Bug352210 ()
		{
			DateTime dt = DateTime.ParseExact ("2007-06-15T10:30:10.5", "yyyy-MM-ddTHH:mm:ss.f", null);
			Assert.AreEqual (633175002105000000, dt.Ticks, "#1");
			Assert.AreEqual (DateTimeKind.Unspecified, dt.Kind, "#2");
		}

		[Test]
		public void Bug352210_New ()
		{
			long ticksUTC = 633377759060000000;
			long ticksLocal = ticksUTC + TimeZone.CurrentTimeZone.GetUtcOffset (new DateTime (ticksUTC)).Ticks;
			CultureInfo ci = CultureInfo.InvariantCulture;
			DateTime dt;

			// Should return same time with Unspecified kind
			dt = DateTime.ParseExact ("2008-02-05 02:38:26", "yyyy-MM-dd HH:mm:ss", ci);
			Assert.AreEqual (DateTimeKind.Unspecified, dt.Kind, "A1");
			Assert.AreEqual (ticksUTC, dt.Ticks, "A2");

			// Should return same time with Unspecified kind
			dt = DateTime.ParseExact ("2008-02-05 02:38:26Z", "u", ci);
			Assert.AreEqual (DateTimeKind.Unspecified, dt.Kind, "B1");
			Assert.AreEqual (ticksUTC, dt.Ticks, "B2");

			// Should adjust to local time with Local kind
			dt = DateTime.ParseExact ("2008-02-05 00:38:26-02:00", "yyyy-MM-dd HH:mm:ssK", ci);
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "C1");
			Assert.AreEqual (ticksLocal, dt.Ticks, "C2");

			// Should ignore AssumeUniversal since a timezone specifier is in the format string
			// and return time adjusted to local time with Local kind
			dt = DateTime.ParseExact ("2008-02-05 00:38:26 -2", "yyyy-MM-dd HH:mm:ss z", ci, DateTimeStyles.AssumeUniversal);
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "D1");
			Assert.AreEqual (ticksLocal, dt.Ticks, "D2");

			try {
				// GMT in format string can be used to specify time zone
				dt = DateTime.ParseExact ("2008-02-05 02:38:26 GMT", "yyyy-MM-dd HH:mm:ss GMT", ci);
				Assert.AreEqual (DateTimeKind.Local, dt.Kind, "E1");
				Assert.AreEqual (ticksLocal, dt.Ticks, "E2");
			}
			catch {
				Assert.Fail ("E3");
			}

			try {
				// Same as above even when surrounded with other characters
				dt = DateTime.ParseExact ("2008-02-05 02:38:26 qqGMTqq", "yyyy-MM-dd HH:mm:ss qqGMTqq", ci);
				Assert.AreEqual (DateTimeKind.Local, dt.Kind, "F1");
				Assert.AreEqual (ticksLocal, dt.Ticks, "F2");
			}
			catch {
				Assert.Fail ("F3");
			}

			try {
				// But single quoted GMT in format string should not specify time zone
				dt = DateTime.ParseExact ("2008-02-05 02:38:26 GMT", "yyyy-MM-dd HH:mm:ss 'GMT'", ci);
				Assert.AreEqual (DateTimeKind.Unspecified, dt.Kind, "G1");
				Assert.AreEqual (ticksUTC, dt.Ticks, "G2");
			}
			catch {
				Assert.Fail ("G3");
			}

			dt = DateTime.Parse ("GMT 2008-02-05 02:38:26", ci);
			Assert.AreEqual (DateTimeKind.Local, dt.Kind, "H1");
			Assert.AreEqual (ticksLocal, dt.Ticks, "H2");
		}

		[Test]
		public void Bug377042 ()
		{
			string [] f = new string [] {
				"yyyy-MM-ddTHH:mm:ssZ",
				"yyyy-MM-ddTHH:mm:sszzzz",
				"yyyy-MM-dd"
				};
			DateTimeStyles dts = DateTimeStyles.AdjustToUniversal;
			dts |= DateTimeStyles.AssumeUniversal;
			DateTime result = DateTime.ParseExact ("2005-01-01T01:11:11+8:00", f, new DateTimeFormatInfo (), dts);
		}

        [Test]
        public void TestParseExactXmlTimeFormats()
        {
            //Xamarin Bug 16742
            string[] xmlTimeFormats = {
                "HH:mm:ss", "HH:mm:ss.FFFFFFF",
                "HH:mm:sszzz", "HH:mm:ss.FFFFFFFzzz",
                "HH:mm:ssZ", "HH:mm:ss.FFFFFFFZ"
            };
            DateTimeStyles style = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

            //time local
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.0", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.000", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.0000", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.00000", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.000000", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000).Ticks,
                DateTime.ParseExact("13:30:44.0000000", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 123).Ticks,
                DateTime.ParseExact("13:30:44.123", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);

            //time with zero timezone
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.00Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.000Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0000Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.00000Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.000000Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0000000Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 123, DateTimeKind.Utc).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.123Z", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);

            //time with timezone
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.00+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.000+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0000+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.00000+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.000000+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 000, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.0000000+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
            Assert.AreEqual(new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 44, 123, new TimeSpan(13, 0, 0)).ToLocalTime().Ticks,
                DateTime.ParseExact("13:30:44.123+13:00", xmlTimeFormats, DateTimeFormatInfo.InvariantInfo, style).Ticks);
        }

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void EmptyString ()
		{
			DateTime.Parse ("");
		}

		[Test]
		public void TryEmptyString ()
		{
			DateTime date;
			Assert.IsFalse (DateTime.TryParse ("", out date));
		}

		[Test]
		public void Kind ()
		{
			if (DateTime.Now == DateTime.UtcNow)
				Assert.Ignore (); // This test does not make sense.
			if (TimeZone.CurrentTimeZone.GetUtcOffset (DateTime.UtcNow)
				!= TimeZone.CurrentTimeZone.GetUtcOffset (DateTime.Now))
				Assert.Ignore (); // In this case it does not satisfy the test premises.

			Assert.AreEqual (DateTimeKind.Local, DateTime.Now.Kind, "#A1");
			Assert.AreEqual (DateTimeKind.Local, DateTime.Today.Kind, "#A2");

			DateTime utc = DateTime.UtcNow;
			DateTime now = new DateTime (utc.Ticks + TimeZone.
				CurrentTimeZone.GetUtcOffset (utc).Ticks, DateTimeKind.Local);
			DateTime utctouniv = utc.ToUniversalTime ();
			DateTime nowtouniv = now.ToUniversalTime ();
			DateTime utctoloc = utc.ToLocalTime ();
			DateTime nowtoloc = now.ToLocalTime ();

			Assert.AreEqual (DateTimeKind.Utc, utc.Kind, "#B1");
			Assert.AreEqual (DateTimeKind.Local, now.Kind, "#B2");
			Assert.AreEqual (DateTimeKind.Utc, utctouniv.Kind, "#B3");
			Assert.AreEqual (DateTimeKind.Utc, nowtouniv.Kind, "#B4");
			Assert.AreEqual (DateTimeKind.Local, utctoloc.Kind, "#B5");
			Assert.AreEqual (DateTimeKind.Local, nowtoloc.Kind, "#B6");
			Assert.AreEqual (utc, utctouniv, "#B7");
			Assert.AreEqual (utc, nowtouniv, "#B8");
			Assert.AreEqual (now, nowtoloc, "#B9");
			Assert.AreEqual (now, utctoloc, "#B10");
		}

		[Test]
		public void InstanceMembersAndKind ()
		{
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.Date.Kind, "#1");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.Add (TimeSpan.FromMinutes (1)).Kind, "#2");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.Subtract (TimeSpan.FromMinutes (1)).Kind, "#3");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddDays (1).Kind, "#4");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddTicks (1).Kind, "#5");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddHours (1).Kind, "#6");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddMinutes (1).Kind, "#7");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddSeconds (1).Kind, "#8");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddMilliseconds (1).Kind, "#9");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddMonths (1).Kind, "#10");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.UtcNow.AddYears (1).Kind, "#11");
			Assert.AreEqual (DateTimeKind.Utc, (DateTime.UtcNow + TimeSpan.FromMinutes (1)).Kind, "#12");
			Assert.AreEqual (DateTimeKind.Utc, (DateTime.UtcNow - TimeSpan.FromMinutes (1)).Kind, "#13");
		}

		[Test]
		public void CompareTicks ()
		{
			// Only ticks are compared, not kind.
			var d = new DateTime (0, DateTimeKind.Utc);
			var f = new DateTime (0);

			Assert.AreEqual (d == f, true, "#1");
		}
		
		[Test]
		public void FromBinary ()
		{
			DateTime dt_utc = DateTime.FromBinary (0x4000000000000001);
			Assert.AreEqual (DateTimeKind.Utc, dt_utc.Kind, "#1");
			Assert.AreEqual (1, dt_utc.Ticks, "#2");

			DateTime dt_local = DateTime.FromBinary (unchecked ((long) 0x8000000000000001));
			Assert.AreEqual (DateTimeKind.Local, dt_local.Kind, "#3");

			DateTime dt_unspecified = DateTime.FromBinary (0x0000000000000001);
			Assert.AreEqual (DateTimeKind.Unspecified, dt_unspecified.Kind, "#4");
			Assert.AreEqual (1, dt_unspecified.Ticks, "#5");

			DateTime dt_local2 = DateTime.FromBinary (unchecked ((long) 0xC000000000000001));
			Assert.AreEqual (DateTimeKind.Local, dt_local2.Kind, "#6");
			Assert.AreEqual (dt_local.Ticks, dt_local2.Ticks, "#7");
		}

		[Test]
		public void ToBinary ()
		{
			DateTime dt_local = new DateTime (1, DateTimeKind.Local);
			Assert.AreEqual (1, (ulong) dt_local.ToBinary () >> 63, "#1");
			Assert.AreEqual (1, dt_local.Ticks, "#2");

			DateTime dt_utc = new DateTime (1, DateTimeKind.Utc);
			Assert.AreEqual (0x4000000000000001, dt_utc.ToBinary (), "#3");
			Assert.AreEqual (1, dt_utc.Ticks, "#4");

			DateTime dt_unspecified = new DateTime (1, DateTimeKind.Unspecified);
			Assert.AreEqual (1, dt_unspecified.ToBinary (), "#5");
			Assert.AreEqual (1, dt_unspecified.Ticks, "#6");
		}

		[Test]
		public void RoundtripBinary ()
		{
			DateTime dt = DateTime.Now;
			DateTime dt2 = DateTime.SpecifyKind (dt, DateTimeKind.Utc);
			DateTime dt3 = DateTime.SpecifyKind (dt, DateTimeKind.Unspecified);
			Assert.AreEqual (dt, DateTime.FromBinary (dt.ToBinary ()), "#1");
			Assert.AreEqual (dt2, DateTime.FromBinary (dt2.ToBinary ()), "#2");
			Assert.AreEqual (dt3, DateTime.FromBinary (dt3.ToBinary ()), "#3");
			Assert.AreEqual (DateTimeKind.Local, DateTime.FromBinary (dt.ToBinary ()).Kind, "#4");
			Assert.AreEqual (DateTimeKind.Utc, DateTime.FromBinary (dt2.ToBinary ()).Kind, "#5");
			Assert.AreEqual (DateTimeKind.Unspecified, DateTime.FromBinary (dt3.ToBinary ()).Kind, "#6");

			Assert.AreEqual (TimeZone.CurrentTimeZone.GetUtcOffset (dt).Ticks, dt3.ToBinary () - (dt.ToBinary () & 0x7FFFFFFFFFFFFFFF), "#7");
		}

		[Test]
		public void TestMin ()
		{
			// This should never throw.
			DateTime.MinValue.ToLocalTime ();
		}

		[Test]
		public void OmittedSecondsFraction ()
		{
			DateTime today = DateTime.Today;
			Assert.AreEqual ("00:00:00.13579", today.AddTicks (1357900).ToString ("HH:mm:ss.FFFFFFF"), "#1");
			DateTime dt = DateTime.ParseExact ("00:00:00.13579", "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
			Assert.AreEqual (today, dt.AddTicks (-1357900), "#2");
			// it's more than strange ...
			Assert.AreEqual (String.Empty, today.ToString (".FFFFFFF"), "#3");
			Assert.AreEqual ("$", today.ToString ("$FFFFFFF"), "#4");
		}

		[Test]
		public void KindInPattern ()
		{
			// only 2.0 supports 'K'
			Assert.AreEqual ("00:00:00", new DateTime (2000, 1, 1).ToString ("HH:mm:ssK"), "#1");
			Assert.AreEqual ('Z', DateTime.Today.ToUniversalTime ().ToString ("HH:mm:ssK") [8], "#2");
			Assert.AreEqual ("00:00:00+09:00".Length, DateTime.Today.ToString ("HH:mm:ssK").Length, "#3");
		}

		[Test]
		public void RoundtripPattern ()
		{
			// only 2.0 supports 'o'
			Assert.AreEqual ("2000-01-01T00:00:00.0000000", new DateTime (2000, 1, 1).ToString ("o"), "#1");
			Assert.AreEqual ("2000-01-01T00:00:00.0000000Z", DateTime.SpecifyKind (new DateTime (2000, 1, 1), DateTimeKind.Utc).ToString ("o"), "#2");
			Assert.AreEqual ("2000-01-01T00:00:00.0000000+09:00".Length, DateTime.SpecifyKind (
				new DateTime (2000, 1, 1), DateTimeKind.Local).ToString ("o").Length, "#3");

			var culture = new CultureInfo ("ps-AF");
			Assert.AreEqual ("1976-06-19T00:00:00.0000000", new DateTime(1976, 6, 19).ToString ("O", culture), "#4");
			Assert.AreEqual ("1976-06-19T00:00:00.0000000", new DateTime(1976, 6, 19).ToString ("o", culture), "#5");
		}

		[Test]
		public void KindPattern ()
		{
			// no matter how the format string contains 'K' and the
			// output string contains kind information, it does not
			// assure that the string is parsed as roundtrip kind.

			// only 2.0 supports 'K'
			string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";
			CultureInfo ci = CultureInfo.CurrentCulture;
			DateTime dt = DateTime.SpecifyKind (new DateTime (2007, 11, 1, 2, 30, 45), DateTimeKind.Utc);
			string s = dt.ToString (format);
			DateTime d1 = DateTime.ParseExact (s, format, ci); // d1 is parsed as a local time.
			Assert.AreEqual (dt.Ticks, d1.ToUniversalTime ().Ticks, "#1");
			// .NET expects Local here, while s ends with 'Z' and should be parsed as UTC.
			Assert.AreEqual (DateTimeKind.Local, d1.Kind, "#2");

			format = "yyyy-MM-dd'T'HH:mm:ssK";
			ci = CultureInfo.CurrentCulture;
			dt = new DateTime (2007, 11, 1, 2, 30, 45);
			s = dt.ToString (format);
			d1 = DateTime.ParseExact (s, format, ci);
			Assert.AreEqual (dt.Ticks, d1.Ticks, "#3");
			Assert.AreEqual (DateTimeKind.Unspecified, d1.Kind, "#4");
		}

		[Test]
		public void TestRoundTrip () {
			DateTime result;
			DateTimeStyles roundTripStyle = DateTimeStyles.RoundtripKind;
			string utcDate = "2008-02-21T11:14:18.2721262Z";
			string localDate = "2008-02-21T11:14:18.2721262+02:00";
			string unspec = "2008-02-21T11:14:18.2721262";
			String [] formats = {"yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"};

			result = DateTime.ParseExact (localDate, formats, CultureInfo.InvariantCulture, roundTripStyle);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.ToUniversalTime ().Ticks, 633391820582721262);
			
			result = DateTime.ParseExact (unspec, formats, CultureInfo.InvariantCulture, roundTripStyle);
			Assert.AreEqual (result.Kind, DateTimeKind.Unspecified);
			Assert.AreEqual (result.Ticks, 633391892582721262);
			
			result = DateTime.ParseExact (utcDate, formats, CultureInfo.InvariantCulture, roundTripStyle);
			Assert.AreEqual (result.Kind, DateTimeKind.Utc);
			Assert.AreEqual (result.Ticks, 633391892582721262);

		}		

		[Test]
		public void TestRegularStyle () {
			DateTime result;
			DateTimeStyles style = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;
			string utcDate = "2008-02-21T11:14:18.2721262Z";
			string localDate = "2008-02-21T11:14:18.2721262+02:00";
			string unspec = "2008-02-21T11:14:18.2721262";
			String [] formats = {"yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"};

			result = DateTime.ParseExact (localDate, formats, CultureInfo.InvariantCulture, style);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.ToUniversalTime ().Ticks, 633391820582721262);

			result = DateTime.ParseExact (unspec, formats, CultureInfo.InvariantCulture, style);
			Assert.AreEqual (result.Kind, DateTimeKind.Unspecified);			
			Assert.AreEqual (result.Ticks, 633391892582721262);

			result = DateTime.ParseExact (utcDate, formats, CultureInfo.InvariantCulture, style);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.ToUniversalTime ().Ticks, 633391892582721262);
		}

		[Test]
		public void TestAssumeLocal () {
			DateTime result;
			DateTimeStyles assumeLocal =  DateTimeStyles.AssumeLocal;
			string utcDate = "2008-02-21T11:14:18.2721262Z";
			string localDate = "2008-02-21T11:14:18.2721262+02:00";
			string unspec = "2008-02-21T11:14:18.2721262";
			String [] formats = {"yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"};

			result = DateTime.ParseExact (localDate, formats, CultureInfo.InvariantCulture, assumeLocal);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.ToUniversalTime ().Ticks, 633391820582721262);

			result = DateTime.ParseExact (unspec, formats, CultureInfo.InvariantCulture, assumeLocal);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.Ticks, 633391892582721262);

			result = DateTime.ParseExact (utcDate, formats, CultureInfo.InvariantCulture, assumeLocal);
			Assert.AreEqual (result.Kind, DateTimeKind.Local);
			Assert.AreEqual (result.ToUniversalTime ().Ticks, 633391892582721262);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IllegalStyleCombination1()
		{
			DateTimeStyles illegal = DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeLocal;
			DateTime.ParseExact ("", "", null, illegal);			
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IllegalStyleCombination2()
		{
			DateTimeStyles illegal = DateTimeStyles.RoundtripKind | DateTimeStyles.AdjustToUniversal;
			DateTime.ParseExact ("", "", null, illegal);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IllegalStyleCombination3()
		{
			DateTimeStyles illegal = DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal;
			DateTime.ParseExact ("", "", null, illegal);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IllegalStyleCombination4()
		{
			DateTimeStyles illegal = DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal;
			DateTime.ParseExact ("", "", null, illegal);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TrailingDot ()
		{
			DateTime.ParseExact ("12:00:00", "HH:mm:ss.", null);
		}

		[Test]
		public void TrailingFlexibleMilliseconds ()
		{
			// bug #444103.
			DateTime.ParseExact ("12:00:00", "HH:mm:ss.FFFFFFF", null);
		}
               
		[Test]
		public void TryParseExact_NullString ()
		{
			DateTime dt;
			DateTime.TryParseExact(null, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture,
					       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt);
			Assert.AreEqual(default(DateTime), dt);
		}

		[Test]
		public void MSAndZ ()
		{
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo ("en-US");
			DateTime dt;
			if (!DateTime.TryParse ("2009.02.24T13:57:07.000 -0800", cultureInfo.DateTimeFormat,
						DateTimeStyles.None, out dt))
				Assert.Fail ("Failed");
		}

		[Test]
		public void Parse_InvalidShortDate ()
		{
			DateTime expected = new DateTime (2011, 03, 22, 07, 32, 00, DateTimeKind.Utc).ToLocalTime();
			DateTime expected2 = new DateTime (2011, 03, 22, 08, 32, 00, DateTimeKind.Utc);

			string [] cultures = new string [] {"es-ES", "en-US", "en-GB", "de-DE", "fr-FR"
				,"es", "en", "de", "fr"
				};
			
			foreach (string culture in cultures) {
				CultureInfo ci = new CultureInfo (culture);
				ci.DateTimeFormat.ShortDatePattern = "d";

				Assert.AreEqual (expected,  DateTime.Parse ("2011-03-22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#a01 - " + culture);
				Assert.AreEqual (expected,  DateTime.Parse ("2011/03/22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#a02 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("2011-03-22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#a03 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("2011/03/22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#a04 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("03/2011/22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#a05 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("03-2011-22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#a06 - " + culture);
				Assert.AreEqual (expected,  DateTime.Parse ("03/2011/22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#a07 - " + culture);
				ci.DateTimeFormat.DateSeparator = "%";
				Assert.AreEqual (expected,  DateTime.Parse ("2011-03-22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#b01 - " + culture);
				Assert.AreEqual (expected,  DateTime.Parse ("2011/03/22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#b02 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("2011-03-22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#b03 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("2011/03/22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#b04 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("03/2011/22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#b05 - " + culture);
				Assert.AreEqual (expected2, DateTime.Parse ("03-2011-22T08:32:00", ci, DateTimeStyles.RoundtripKind), "#b06 - " + culture);
				Assert.AreEqual (expected,  DateTime.Parse ("03/2011/22 08:32:00+01:00", ci, DateTimeStyles.RoundtripKind), "#b07 - " + culture);
			}
		}

		// Bug 3392
		[Test]
		public void Parse_DateWithTimeZone_TimeZoneShouldBeCorrectlyRead ()
		{
			string testDateWithTimeZoneInfo  = "2012-01-14T15:09:42.692875+03:00";
			long expectedUtcTics = 634621397826928750;

			DateTimeOffset result = DateTimeOffset.Parse (testDateWithTimeZoneInfo, null, DateTimeStyles.RoundtripKind);

			Assert.AreEqual (expectedUtcTics, result.UtcTicks);
		}
		
		[Test]
		public void GenitiveMonth ()
		{
			var ci = new CultureInfo ("ru-RU");
			var dt = new DateTime (2012, 9, 15);
			Assert.AreEqual ("15 сентября", dt.ToString ("m", ci));
		}

		[Test]
		public void Parse_ThaiCalendar ()
		{
			var culture = CultureInfo.GetCultureInfo ("th-TH");
			Assert.IsTrue (culture.Calendar is ThaiBuddhistCalendar);
			var dt = DateTime.Now.Date;
			var s = dt.ToString (culture);
			var parsed = DateTime.Parse (s, culture);

			Assert.AreEqual (dt, parsed, "#1");
		}

		[Test]
		public void ISO8601FractionalDigits ()
		{
			string date = "2014-08-25T01:20:23.601911612343423423465789789365674575676746756747467Z";
			long expectedTicks = 635445264236019116;

			var dt = DateTime.Parse (date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

			Assert.AreEqual (expectedTicks, dt.Ticks);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ISO8601FractionalDigitsException1 ()
		{
			string date = "2014-08-25T01:20:23.60191161234342342346578978936567457567:6746756747467Z";
			DateTime.Parse (date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ISO8601FractionalDigitsException2 ()
		{
			string date = "2014-08-25T01:20:23.6019116-12343423423465789789365674575676746756747467Z";
			DateTime.Parse (date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ISO8601FractionalDigitsException3 ()
		{
			string date = "2014-08-25T01:20:23.601911612343423423465789789365674575676746756747467%Z";
			DateTime.Parse (date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}

		[Test]
		public void Year_2 ()
		{
			var res = DateTime.Parse ("12-002", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind);			
			Assert.AreEqual (2, res.Year, "#1");
			Assert.AreEqual (12, res.Month, "#2");			
		}

		[Test]
		[SetCulture ("en-us")]
		public void ToUniversalTime_TimeZoneOffsetShouldNotOverflow ()
		{
			var m = DateTime.MaxValue;
			var res = m.ToUniversalTime ();

			// It does not matter which time zone but we should never overflow or have DateTime.MinValue
			Assert.AreEqual (9999, res.Year, "#1");
			Assert.AreEqual (12, res.Month, "#2");
			Assert.AreEqual (31, res.Day, "#3");
			Assert.AreEqual (DateTimeKind.Utc, res.Kind, "#4");

			m = DateTime.MinValue;
			res = m.ToUniversalTime ();

			// It does not matter which time zone but we should never overflow or have DateTime.MinValue
			Assert.AreEqual (1, res.Year, "#10");
			Assert.AreEqual (1, res.Month, "#11");
			Assert.AreEqual (1, res.Day, "#12");
			Assert.AreEqual (DateTimeKind.Utc, res.Kind, "#13");
		}

		[Test] // https://github.com/mono/mono/issues/11317
		public void DateTimeKoCulture ()
		{
			foreach (var culture in new [] { new CultureInfo ("ko"), new CultureInfo ("ko-KR") })
			{
				var dateTimeAm = new DateTime (2018, 1, 1, 11, 0, 0);
				var dateTimePm = new DateTime (2018, 1, 1, 13, 0, 0);

				Assert.AreEqual ("오전 11:00:00", dateTimeAm.ToString ("T", culture.DateTimeFormat));
				Assert.AreEqual ("오후 1:00:00", dateTimePm.ToString ("T", culture.DateTimeFormat));
			}
		}
	}
}
