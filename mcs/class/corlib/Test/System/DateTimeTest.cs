//
// DateTimeTest.cs - NUnit Test Cases for the System.DateTime struct
//
// author:
//   Martin Baulig (martin@gnome.org)
//
//   (C) 2002 Free Software Foundation
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{

[TestFixture]
public class DateTimeTest : Assertion
{
	[Flags]
	internal enum Resolution : ushort {
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
		
	internal void DTAssertEquals (DateTime actual, DateTime expected, Resolution resolution) {
		DTAssertEquals ("", actual, expected, resolution);
	}

	internal void DTAssertEquals (string message, DateTime expected, DateTime actual, Resolution resolution) {
		if ((resolution & Resolution.Year) != 0)
			AssertEquals (message, expected.Year, actual.Year);
		if ((resolution & Resolution._Month) != 0)
			AssertEquals (message, expected.Month, actual.Month);
		if ((resolution & Resolution._Day) != 0)
			AssertEquals (message, expected.Day, actual.Day);
		if ((resolution & Resolution._Hour) != 0)
			AssertEquals (message, expected.Hour, actual.Hour);
		if ((resolution & Resolution._Minute) != 0)
			AssertEquals (message, expected.Minute, actual.Minute);
		if ((resolution & Resolution._Second) != 0)
			AssertEquals (message, expected.Second, actual.Second);
		if ((resolution & Resolution._Millisecond) != 0)
			AssertEquals (message, expected.Millisecond, actual.Millisecond);
	}

	private CultureInfo oldcult;
	
	long[] myTicks = {
		631501920000000000L,	// 25 Feb 2002 - 00:00:00
		631502475130080000L,	// 25 Feb 2002 - 15:25:13,8
		631502115130080000L,	// 25 Feb 2002 - 05:25:13,8
		631502115000000000L,	// 25 Feb 2002 - 05:25:00
		631502115130000000L,	// 25 Feb 2002 - 05:25:13
		631502079130000000L,	// 25 Feb 2002 - 04:25:13
		629197085770000000L	// 06 Nov 1994 - 08:49:37 
	};

	public DateTimeTest() {}

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
	
	public void TestCtors ()
	{
		DateTime t1 = new DateTime (2002,2,25);
		AssertEquals("A01", myTicks[0], t1.Ticks);
		DateTime t2 = new DateTime (2002,2,25,15,25,13,8);
		AssertEquals("A02", myTicks[1], t2.Ticks);
		AssertEquals("A03", myTicks[0], t2.Date.Ticks);
		AssertEquals("A04", 2002, t2.Year);
		AssertEquals("A05", 2, t2.Month);
		AssertEquals("A06", 25, t2.Day);
		AssertEquals("A07", 15, t2.Hour);
		AssertEquals("A08", 25, t2.Minute);
		AssertEquals("A09", 13, t2.Second);
		AssertEquals("A10", 8, t2.Millisecond);
		DateTime t3 = new DateTime (2002,2,25,5,25,13,8);
		AssertEquals("A11", myTicks[2], t3.Ticks);
	}

	[Test]
	public void Constructor_Max () 
	{
		AssertEquals ("Max", 3155378975999990000, new DateTime (9999, 12, 31, 23, 59, 59, 999).Ticks);
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
		AssertEquals ("J01", 3155378975999999999L, DateTime.MaxValue.Ticks);					
		AssertEquals ("J02", 0, DateTime.MinValue.Ticks);
	}
	
	[Test]
	public void Add ()
	{
		DateTime t1 = new DateTime (myTicks [1]);
		TimeSpan span = new TimeSpan (3, 54, 1);
		DateTime t2 = t1.Add (span);
		
		AssertEquals ("K01", 25, t2.Day);
		AssertEquals ("K02", 19, t2.Hour);
		AssertEquals ("K03", 19, t2.Minute);
		AssertEquals ("K04", 14, t2.Second);
		
		AssertEquals ("K05", 25, t1.Day);
		AssertEquals ("K06", 15, t1.Hour);
		AssertEquals ("K07", 25, t1.Minute);
		AssertEquals ("K08", 13, t1.Second);		
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
		AssertEquals ("L01", 28, t1.Day);
		AssertEquals ("L02", 15, t1.Hour);
		AssertEquals ("L03", 25, t1.Minute);
		AssertEquals ("L04", 13, t1.Second);		
		
		t1 = t1.AddDays (1.9);
		AssertEquals ("L05", 2, t1.Day);
		AssertEquals ("L06", 13, t1.Hour);
		AssertEquals ("L07", 1, t1.Minute);
		AssertEquals ("L08", 13, t1.Second);		

		t1 = t1.AddDays (0.2);
		AssertEquals ("L09", 2, t1.Day);
		AssertEquals ("L10", 17, t1.Hour);
		AssertEquals ("L11", 49, t1.Minute);
		AssertEquals ("L12", 13, t1.Second);				
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
		AssertEquals ("N01", 26, t1.Day);
		AssertEquals ("N02", 1, t1.Hour);
		AssertEquals ("N03", 25, t1.Minute);
		AssertEquals ("N04", 13, t1.Second);		
		
		t1 = t1.AddHours (-3.7);
		AssertEquals ("N05", 25, t1.Day);
		AssertEquals ("N06", 21, t1.Hour);
		AssertEquals ("N07", 43, t1.Minute);
		AssertEquals ("N08", 13, t1.Second);		

		t1 = t1.AddHours (3.732);
		AssertEquals ("N09", 26, t1.Day);
		AssertEquals ("N10", 1, t1.Hour);
		AssertEquals ("N11", 27, t1.Minute);
		AssertEquals ("N12", 8, t1.Second);				
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void AddHoursOutOfRangeException1 ()
	{
		DateTime t1 = new DateTime (myTicks [1]);
		t1.AddHours (9E100);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
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
		AssertEquals ("O01", 21, t1.Day);
		AssertEquals ("O02", 9, t1.Hour);
		AssertEquals ("O03", 11, t1.Minute);
		AssertEquals ("O04", 53, t1.Second);		
		
		t1 = t1.AddMilliseconds (-19E10);
		AssertEquals ("O05", 13, t1.Day);
		AssertEquals ("O06", 7, t1.Hour);
		AssertEquals ("O07", 25, t1.Minute);
		AssertEquals ("O08", 13, t1.Second);		

		t1 = t1.AddMilliseconds (15.623);
		AssertEquals ("O09", 13, t1.Day);
		AssertEquals ("O10", 7, t1.Hour);
		AssertEquals ("O11", 25, t1.Minute);
		AssertEquals ("O12", 13, t1.Second);				
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void AddMillisecondsOutOfRangeException1 ()
	{
		DateTime t1 = new DateTime (myTicks [1]);
		t1.AddMilliseconds (9E100);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void AddMillisecondsOutOfRangeException2 ()
	{
		DateTime t1 = new DateTime (myTicks [1]);
		t1.AddMilliseconds (-9E100);
	}


	public void TestToString ()
	{
		DateTime t1 = new DateTime (myTicks[2]);
		DateTime t2 = new DateTime (myTicks[1]);
		// Standard patterns
		AssertEquals("B01", "02/25/2002", t1.ToString ("d"));
		AssertEquals("B02", "Monday, 25 February 2002", t1.ToString ("D"));
		AssertEquals("B03", "Monday, 25 February 2002 05:25", t1.ToString ("f"));
		AssertEquals("B04", "Monday, 25 February 2002 05:25:13", t1.ToString ("F"));
		AssertEquals("B05", "02/25/2002 05:25", t1.ToString ("g"));
		AssertEquals("B06", "02/25/2002 05:25:13", t1.ToString ("G"));
		AssertEquals("B07", "February 25", t1.ToString ("m"));
		AssertEquals("B08", "February 25", t1.ToString ("M"));
		AssertEquals("B09", "Mon, 25 Feb 2002 05:25:13 GMT", t1.ToString ("r"));
		AssertEquals("B10", "Mon, 25 Feb 2002 05:25:13 GMT", t1.ToString ("R"));
		AssertEquals("B11", "2002-02-25T05:25:13", t1.ToString ("s"));
		AssertEquals("B12", "05:25", t1.ToString ("t"));
		AssertEquals("B13", "05:25:13", t1.ToString ("T"));
		//AssertEquals("B14", "2002-02-25 05:25:13Z", t1.ToString ("u"));
		//AssertEquals("B15", "Sunday, 24 February 2002 11:25:13", t1.ToString ("U"));
		AssertEquals("B16", "2002 February", t1.ToString ("y"));
		AssertEquals("B17", "2002 February", t1.ToString ("Y"));

		// Custom patterns
		AssertEquals("C01", "25", t1.ToString ("%d"));
		AssertEquals("C02", "25", t1.ToString ("dd"));
		AssertEquals("C03", "Mon", t1.ToString ("ddd"));
		AssertEquals("C04", "Monday", t1.ToString ("dddd"));
		AssertEquals("C05", "2", t1.ToString ("%M"));
		AssertEquals("C06", "02", t1.ToString ("MM"));
		AssertEquals("C07", "Feb", t1.ToString ("MMM"));
		AssertEquals("C08", "February", t1.ToString ("MMMM"));
		AssertEquals("C09", "2", t1.ToString ("%y"));
		AssertEquals("C10", "02", t1.ToString ("yy"));
		AssertEquals("C11", "2002", t1.ToString ("yyyy"));
		AssertEquals("C12", "5", t1.ToString ("%h"));
		AssertEquals("C13", "05", t1.ToString ("hh"));
		AssertEquals("C14", "3", t2.ToString ("%h"));
		AssertEquals("C15", "03", t2.ToString ("hh"));
		AssertEquals("C16", "15", t2.ToString ("%H"));
		AssertEquals("C17", "15", t2.ToString ("HH"));
		AssertEquals("C18", "25", t2.ToString ("%m"));
		AssertEquals("C19", "25", t2.ToString ("mm"));
		AssertEquals("C20", "13", t2.ToString ("%s"));
		AssertEquals("C21", "13", t2.ToString ("ss"));
		AssertEquals("C22", "A", t1.ToString ("%t"));
		AssertEquals("C23", "P", t2.ToString ("%t"));
		AssertEquals("C24", "AM", t1.ToString ("tt"));
		AssertEquals("C25", "PM", t2.ToString ("tt"));
		long offset = TimeZone.CurrentTimeZone.GetUtcOffset(t1).Ticks / 36000000000;
		AssertEquals("C26", offset.ToString("+#;-#;0"), t1.ToString ("%z"));
		AssertEquals("C27", offset.ToString("+00;-00;00"), t1.ToString ("zz"));
		AssertEquals("C28", offset.ToString("+00;-00;00") + ":00", t1.ToString ("zzz"));
		AssertEquals("C29", " : ", t1.ToString (" : "));
		AssertEquals("C30", " / ", t1.ToString (" / "));
		AssertEquals("C31", " yyy ", t1.ToString (" 'yyy' "));
		AssertEquals("C32", " d", t1.ToString (" \\d"));
	}

	[Ignore ("need a better way to handle these tests with them running on different timezones")]
	public void TestParseExact2 () 
	{
		DateTime t1 = DateTime.ParseExact ("2002-02-25 04:25:13Z", "u", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		AssertEquals ("D07d", 04 + TimeZone.CurrentTimeZone.GetUtcOffset(t1).Hours, t1.Hour);

	}

	public void TestParseExact ()
	{
		// Standard patterns
		DateTime t1 = DateTime.ParseExact ("02/25/2002", "d", null);
		AssertEquals ("D01", myTicks[0], t1.Ticks);
		t1 = DateTime.ParseExact ("Monday, 25 February 2002", "D", null);
		AssertEquals ("D02", myTicks[0], t1.Ticks);
		t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25", "f", null);
		AssertEquals ("D03", myTicks[3], t1.Ticks);
		t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13", "F", null);
		AssertEquals ("D04", myTicks[4], t1.Ticks);
		t1 = DateTime.ParseExact ("02/25/2002 05:25", "g", null);
		AssertEquals ("D05", myTicks[3], t1.Ticks);
		t1 = DateTime.ParseExact ("02/25/2002 05:25:13", "G", null);
		AssertEquals ("D06", myTicks[4], t1.Ticks);
		t1 = DateTime.ParseExact ("2002-02-25 04:25:13Z", "u", null);
		AssertEquals ("D07a", 2002, t1.Year);
		AssertEquals ("D07b", 02, t1.Month);
		AssertEquals ("D07c", 25, t1.Day);
		AssertEquals ("D07e", 25, t1.Minute);
		AssertEquals ("D07f", 13, t1.Second);
		t1 = DateTime.ParseExact ("Monday, 25 February 2002 04:25:13", "U", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		AssertEquals ("D08a", 2002, t1.Year);
		AssertEquals ("D08b", 02, t1.Month);
		AssertEquals ("D08c", 25, t1.Day);
		AssertEquals ("D08d", 04, t1.Hour);
		AssertEquals ("D08e", 25, t1.Minute);
		AssertEquals ("D08f", 13, t1.Second);

		DateTime t2 = new DateTime (DateTime.Today.Year, 2, 25);
		t1 = DateTime.ParseExact ("February 25", "m", null);
		AssertEquals ("D09", t2.Ticks, t1.Ticks);

		t2 = new DateTime (DateTime.Today.Year, 2, 25);
		t1 = DateTime.ParseExact ("February 25", "M", null);
		AssertEquals ("D10", t2.Ticks, t1.Ticks);

		t1 = DateTime.ParseExact ("Mon, 25 Feb 2002 04:25:13 GMT", "r", null);
		AssertEquals ("D11a", 2002, t1.Year);
		AssertEquals ("D11b", 02, t1.Month);
		AssertEquals ("D11c", 25, t1.Day);
		AssertEquals ("D11d", 04, t1.Hour);
		AssertEquals ("D11e", 25, t1.Minute);
		AssertEquals ("D11f", 13, t1.Second);

		t1 = DateTime.ParseExact ("Mon, 25 Feb 2002 04:25:13 GMT", "R", null);
		AssertEquals ("D12a", 2002, t1.Year);
		AssertEquals ("D12b", 02, t1.Month);
		AssertEquals ("D12c", 25, t1.Day);
		AssertEquals ("D12d", 04, t1.Hour);
		AssertEquals ("D12e", 25, t1.Minute);
		AssertEquals ("D12f", 13, t1.Second);

		t1 = DateTime.ParseExact ("2002-02-25T05:25:13", "s", null);
		AssertEquals ("D13", myTicks[4], t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5,25,0);
		t1 = DateTime.ParseExact ("05:25", "t", null);
		AssertEquals("D14", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5,25,13);
		t1 = DateTime.ParseExact ("05:25:13", "T", null);
		AssertEquals("D15", t2.Ticks, t1.Ticks);

		t2 = new DateTime (2002, 2, 1);
		t1 = DateTime.ParseExact ("2002 February", "y", null);
		AssertEquals ("D16", t2.Ticks, t1.Ticks);

		t2 = new DateTime (2002, 2, 1);
		t1 = DateTime.ParseExact ("2002 February", "Y", null);
		AssertEquals ("D16", t2.Ticks, t1.Ticks);

		// Custom patterns
		t2 = new DateTime (DateTime.Now.Year, 1, 25);
		t1 = DateTime.ParseExact ("25", "%d", null);
		AssertEquals ("E01: " + t2 + " -- " + t1, t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("25", "dd", null);
		AssertEquals ("E02", t2.Ticks, t1.Ticks);

		t2 = new DateTime (DateTime.Today.Year, 2, 1);
		t1 = DateTime.ParseExact ("2", "%M", null);
		AssertEquals ("E03", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("02", "MM", null);
		AssertEquals ("E04", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("Feb", "MMM", null);
		AssertEquals ("E05", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("February", "MMMM", null);
		AssertEquals ("E06", t2.Ticks, t1.Ticks);

		t2 = new DateTime (2005, 1, 1);
		t1 = DateTime.ParseExact ("5", "%y", null);
		AssertEquals ("E07", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("05", "yy", null);
		AssertEquals ("E08", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("2005", "yyyy", null);
		AssertEquals ("E09", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5, 0, 0);
		t1 = DateTime.ParseExact ("5A", "ht", null);
		AssertEquals ("E10", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("05A", "hht", null);
		AssertEquals ("E11", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (15, 0, 0);
		t1 = DateTime.ParseExact ("3P", "ht", null);
		AssertEquals ("E12", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("03P", "hht", null);
		AssertEquals ("E13", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5, 0, 0);
		t1 = DateTime.ParseExact ("5", "%H", null);
		AssertEquals ("E14", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (15, 0, 0);
		t1 = DateTime.ParseExact ("15", "%H", null);
		AssertEquals ("E15", t2.Ticks, t1.Ticks);
		t1 = DateTime.ParseExact ("15", "HH", null);
		AssertEquals ("E16", t2.Ticks, t1.Ticks);

		// Time zones
		t2 = DateTime.Today + new TimeSpan (17, 18, 0);
		t1 = DateTime.ParseExact ("11:18AM -5", "h:mmtt z", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
			t1 += new TimeSpan(1, 0, 0);
		AssertEquals ("F01", t2.Ticks, t1.Ticks);

		t1 = DateTime.ParseExact ("11:18AM -05:00", "h:mmtt zzz", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
			t1 += new TimeSpan(1, 0, 0);
		AssertEquals ("F02", t2.Ticks, t1.Ticks);

		t1 = DateTime.ParseExact ("7:18PM +03", "h:mmtt zz", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
			t1 += new TimeSpan(1, 0, 0);
		AssertEquals ("F03", t2.Ticks, t1.Ticks);

		t1 = DateTime.ParseExact ("7:48PM +03:30", "h:mmtt zzz", null);
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(t1))
			t1 += new TimeSpan(1, 0, 0);
		AssertEquals ("F04", t2.Ticks, t1.Ticks);

		// Options
		t2 = DateTime.Today + new TimeSpan (16, 18, 0);
		t1 = DateTime.ParseExact ("11:18AM -5", "h:mmtt z",
					  null, DateTimeStyles.AdjustToUniversal);
		AssertEquals ("G01", t2.Ticks, t1.Ticks);

		t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13", "F",
					  null, DateTimeStyles.AdjustToUniversal);
		AssertEquals ("G02", myTicks[4], t1.Ticks);
		t1 = DateTime.ParseExact ("Monday, 25 February 2002 05:25:13",
					  "dddd, dd MMMM yyyy HH:mm:ss",
					  null, DateTimeStyles.AdjustToUniversal);
		AssertEquals ("G03", myTicks[4], t1.Ticks);

		t1 = DateTime.ParseExact ("02/25/2002", "d", null,
					  DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("G04", myTicks[0], t1.Ticks);

		t1 = DateTime.ParseExact ("    02/25/2002", "d", null,
					  DateTimeStyles.AllowLeadingWhite);
		AssertEquals ("G05", myTicks[0], t1.Ticks);

		t1 = DateTime.ParseExact ("02/25/2002    ", "d", null,
					  DateTimeStyles.AllowTrailingWhite);
		AssertEquals ("G06", myTicks[0], t1.Ticks);

		t1 = DateTime.ParseExact ("  02 / 25 / 2002    ", "d", null,
					  DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("G07", myTicks[0], t1.Ticks);

		// Multi Custom Patterns
		string rfc1123_date = "r";
		string rfc850_date = "dddd, dd'-'MMM'-'yy HH':'mm':'ss 'GMT'";
		string asctime_date = "ddd MMM d HH':'mm':'ss yyyy";
		string [] formats = new string [] {rfc1123_date, rfc850_date, asctime_date};
		CultureInfo enUS = new CultureInfo("en-US", false);
		t1 = DateTime.ParseExact ("Sun, 06 Nov 1994 08:49:37 GMT", formats[0], enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M01", myTicks[6], t1.Ticks);
		t1 = DateTime.ParseExact ("Sunday, 06-Nov-94 08:49:37 GMT", formats[1], enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M02", myTicks[6], t1.Ticks);
		t1 = DateTime.ParseExact ("Sun Nov  6 08:49:37 1994", formats[2], enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M03", myTicks[6], t1.Ticks);
		t1 = DateTime.ParseExact ("Sun, 06 Nov 1994 08:49:37 GMT", formats, enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M04", myTicks[6], t1.Ticks);
		t1 = DateTime.ParseExact ("Sunday, 06-Nov-94 08:49:37 GMT", formats, enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M05", myTicks[6], t1.Ticks);
		t1 = DateTime.ParseExact ("Sun Nov  6 08:49:37 1994", formats, enUS, 
					DateTimeStyles.AllowWhiteSpaces);
		AssertEquals ("M06", myTicks[6], t1.Ticks);
	
	        // Bug 52274
		t1 = DateTime.ParseExact ("--12--", "--MM--" , null);
		AssertEquals("Dash01", 12, t1.Month);
		t1=DateTime.ParseExact ("--12-24", "--MM-dd" , null);
		AssertEquals("Dash02", 24, t1.Day);
		AssertEquals("Dash03", 12, t1.Month);
		t1=DateTime.ParseExact ("---24", "---dd" , null);
		AssertEquals("Dash04", 24, t1.Day);

		// Bug 63376
		t1 = DateTime.ParseExact ("18Aug2004 12:33:00", "ddMMMyyyy hh:mm:ss", new CultureInfo ("en-US"));
		AssertEquals ("hh allows 12, though it's useless", 0, t1.Hour);
	}

	[Ignore ("need to fix tests that run on different timezones")]
	public void TestParse2 ()
	{
		DateTime t1 = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		AssertEquals ("H10d", 04 - TimeZone.CurrentTimeZone.GetUtcOffset(t1).Hours, t1.Hour);

	}
	
	public void TestParse ()
	{
		// Standard patterns
		DateTime t1 = DateTime.Parse ("02/25/2002");
		AssertEquals ("H00", myTicks[0], t1.Ticks);
		try {
			t1 = DateTime.Parse ("02-25-2002");
		}
		catch (Exception e) {
			Fail ("Unexpected exception. e=" + e);
		}
		try {
			t1 = DateTime.Parse ("2002-02-25");
		}
		catch (Exception e) {
			Fail ("Unexpected exception. e=" + e);
		}
		AssertEquals ("H01", myTicks[0], t1.Ticks);
		t1 = DateTime.Parse ("Monday, 25 February 2002");
		AssertEquals ("H02", myTicks[0], t1.Ticks);
		t1 = DateTime.Parse ("Monday, 25 February 2002 05:25");
		AssertEquals ("H03", myTicks[3], t1.Ticks);
		t1 = DateTime.Parse ("Monday, 25 February 2002 05:25:13");
		AssertEquals ("H04", myTicks[4], t1.Ticks);
		t1 = DateTime.Parse ("02/25/2002 05:25");
		AssertEquals ("H05", myTicks[3], t1.Ticks);
		t1 = DateTime.Parse ("02/25/2002 05:25:13");
		AssertEquals ("H06", myTicks[4], t1.Ticks);
		t1 = DateTime.Parse ("2002-02-25 04:25:13Z");
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		AssertEquals ("H07a", 2002, t1.Year);
		AssertEquals ("H07b", 02, t1.Month);
		AssertEquals ("H07c", 25, t1.Day);
		AssertEquals ("H07d", 04, t1.Hour);
		AssertEquals ("H07e", 25, t1.Minute);
		AssertEquals ("H07f", 13, t1.Second);

		DateTime t2 = new DateTime (DateTime.Today.Year, 2, 25);
		t1 = DateTime.Parse ("February 25");
		AssertEquals ("H08", t2.Ticks, t1.Ticks);

		t2 = new DateTime (DateTime.Today.Year, 2, 8);
		t1 = DateTime.Parse ("February 08");
		AssertEquals ("H09", t2.Ticks, t1.Ticks);

		// parsed as UTC string
		t1 = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime (t1);
		AssertEquals ("H10a", 2002, t1.Year);
		AssertEquals ("H10b", 02, t1.Month);
		AssertEquals ("H10c", 25, t1.Day);
		AssertEquals ("H10d", 4, t1.Hour);
		AssertEquals ("H10e", 25, t1.Minute);
		AssertEquals ("H10f", 13, t1.Second);

		t1 = DateTime.Parse ("2002-02-25T05:25:13");
		AssertEquals ("H11", myTicks[4], t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5,25,0);
		t1 = DateTime.Parse ("05:25");
		AssertEquals("H12", t2.Ticks, t1.Ticks);

		t2 = DateTime.Today + new TimeSpan (5,25,13);
		t1 = DateTime.Parse ("05:25:13");
		AssertEquals("H13", t2.Ticks, t1.Ticks);

		t2 = new DateTime (2002, 2, 1);
		t1 = DateTime.Parse ("2002 February");
		AssertEquals ("H14", t2.Ticks, t1.Ticks);

		t2 = new DateTime (2002, 2, 1);
		t1 = DateTime.Parse ("2002 February");
		AssertEquals ("H15", t2.Ticks, t1.Ticks);

		t2 = new DateTime (DateTime.Today.Year, 2, 8);
		t1 = DateTime.Parse ("February 8");
		AssertEquals ("H16", t2.Ticks, t1.Ticks);
	}

	public void TestParse3 ()
	{
		string s = "Wednesday, 09 June 2004";
		DateTime.ParseExact (s, "dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
		try {
			DateTime.ParseExact (s, "dddd, dd MMMM yyyy", new CultureInfo ("ja-JP"));
			Fail ("ja-JP culture does not support format \"dddd, dd MMMM yyyy\"");
		} catch (FormatException) {
		}

		// Ok, now we can assume ParseExact() works expectedly.

		DateTime.Parse (s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
		DateTime.Parse (s, new CultureInfo ("ja-JP"), DateTimeStyles.AllowWhiteSpaces);
//		DateTime.Parse (s, null); currently am not sure if it works for _every_ culture.
	}

	[Test]
	[ExpectedException(typeof (FormatException))]
	public void ParseFormatException1 ()
	{
		// Following string is not a correct French date i.e. dd/mm/yyyy.
		String frDateTime = "11/13/2003 11:28:15";
	        IFormatProvider format = new CultureInfo("fr-FR", true);
		DateTime t1 = DateTime.Parse(frDateTime, format);
	}

	public void TestOA ()
	{
		double number=5000.41443;
		DateTime d = DateTime.FromOADate(number);
		DTAssertEquals ("I01", d, new DateTime(1913, 9, 8, 9, 56, 46, 0), Resolution.Second);
		AssertEquals ("I02", d.ToOADate(), number);
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
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003/01/23 12:34:56 GMT";
		dt = DateTime.Parse (d, ci);
		AssertEquals (d, tick2, dt.Ticks);

		d = "Thu, 23 Jan 2003 12:34:56 GMT";
		dt = DateTime.ParseExact (d, s2, ci);
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003-01-23 12:34:56Z";
		dt = DateTime.ParseExact (d, s, ci);
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003-01-23T12:34:56";
		dt = DateTime.ParseExact (d, s3, ci);
		AssertEquals (d, tick1, dt.Ticks);

		// ja-JP ... it should be culture independent
		ci = new CultureInfo ("ja-JP");

		d = "2003/01/23 12:34:56";
		dt = DateTime.Parse (d, ci);
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003/01/23 12:34:56 GMT";
		dt = DateTime.Parse (d, ci);
		AssertEquals (d, tick2, dt.Ticks);

		d = "Thu, 23 Jan 2003 12:34:56 GMT";
		dt = DateTime.ParseExact (d, s2, ci);
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003-01-23 12:34:56Z";
		dt = DateTime.ParseExact (d, s, ci);
		AssertEquals (d, tick1, dt.Ticks);

		d = "2003-01-23T12:34:56";
		dt = DateTime.ParseExact (d, s3, ci);
		AssertEquals (d, tick1, dt.Ticks);
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
			AssertEquals ("#1", "632241504000000000 6/30/2004 12:00:00 AM", sw.ToString ());
			sw.GetStringBuilder ().Length = 0;
			sw.Write ("{0} {1}", d2.Ticks, d2);
			AssertEquals ("#2", "632241504000000000 6/30/2004 12:00:00 AM", sw.ToString ());
			sw.GetStringBuilder ().Length = 0;
			sw.Write ("{0} {1}", d3.Ticks, d3);
			AssertEquals ("#3", "632241504000000000 6/30/2004 12:00:00 AM", sw.ToString ());
			sw.GetStringBuilder ().Length = 0;
			sw.Write ("{0} {1}", d4.Ticks, d4);
			AssertEquals ("#4", "632241504000000000 6/30/2004 12:00:00 AM", sw.ToString ());
		} finally {
			Thread.CurrentThread.CurrentCulture = ci;
		}
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
		AssertEquals ("Ticks-Zero", 599264352000000000, DateTime.FromOADate (0.0d).Ticks);
		AssertEquals ("Ticks-Min", 31242239136000000, DateTime.FromOADate (-657434.999d).Ticks);
		AssertEquals ("Ticks-Max", 3155378975136000000, DateTime.FromOADate (2958465.999d).Ticks);
	}

	[Test]
	public void ToOADate ()
	{
		// Note: OA (OLE Automation) dates aren't timezone sensitive
		DateTime d = new DateTime (0);
		AssertEquals ("Unititialized", 0.0d, d.ToOADate ());
		d = new DateTime (599264352000000000);
		AssertEquals ("Ticks-Zero", 0.0d, d.ToOADate ());
		d = new DateTime (31242239136000000);
		AssertEquals ("Ticks-Min", -657434.999d, d.ToOADate ());
		d = new DateTime (3155378975136000000);
		AssertEquals ("Ticks-Max", 2958465.999d, d.ToOADate ());
	}

	[Test]
	public void ToOADate_OverMax ()
	{
		DateTime d = new DateTime (3155378975136000001);
		AssertEquals ("Over-Max", 2958465.999d, d.ToOADate ());
	}

	[Test]
	public void ToOADate_MaxValue ()
	{
		AssertEquals ("MaxValue", 2958465.99999999d, DateTime.MaxValue.ToOADate ());
	}

	[Test]
	public void ToOADate_UnderMin ()
	{
		DateTime d = new DateTime (31242239135999999);
		AssertEquals ("Under-Min", -657434.999d, d.ToOADate ());
	}

	[Test]
	public void ToOADate_MinValue ()
	{
		AssertEquals ("MinValue", 0, DateTime.MinValue.ToOADate ());
	}

	[Test]
	public void MaxValueYear () // bug52075
	{
		AssertEquals ("#01", "9999", DateTime.MaxValue.Year.ToString ());
	}

	[Test]
	public void X509Certificate () 
	{
		// if this test fails then *ALL* or *MOST* X509Certificate tests will also fails
		AssertEquals ("yyyyMMddHHmmssZ", "03/12/1996 18:38:47", DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null).ToUniversalTime ().ToString ());
		// technically this is invalid (PKIX) because of the missing seconds but it exists so...
		AssertEquals ("yyMMddHHmmZ", "02/23/1996 19:15:00", DateTime.ParseExact ("9602231915Z", "yyMMddHHmmZ", null).ToUniversalTime ().ToString ());

		// However, "Z" and "'Z'" are different.
		AssertEquals ("Z timezone handling",
			DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmss'Z'", null).ToLocalTime (),
			DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null));
	}

	[Test]
	public void QuotedFormat () // bug 56436
	{
		string date = "28/Mar/2004:19:12:37 +0200";
		string [] expectedFormats = {"dd\"/\"MMM\"/\"yyyy:HH:mm:ss zz\"00\""};
		DateTime mydate = DateTime.ParseExact (date, expectedFormats, null, DateTimeStyles.AllowWhiteSpaces);
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

				// Common patterns
				// X509Certificate pattern is _always_ accepted.
				stage = "1";
				dt = DateTime.ParseExact ("19960312183847Z", "yyyyMMddHHmmssZ", null);
#if NET_1_1
				stage = "2";
				// culture 1025 ar-SA fails
//				if (i != 127)
//					dt = DateTime.Parse ("19960312183847Z");
#endif
				stage = "3";
				dt = DateTime.Parse ("2004-05-26T03:29:01.1234567");
				stage = "4";
				dt = DateTime.Parse ("2004-05-26T03:29:01.1234567-07:00");

				// memo: the least LCID is 127, and then 1025(ar-SA)

				// "th-TH" locale rejects them since in
				// ThaiBuddhistCalendar the week of a day is different.
				// (and also for years).
				if (ci.LCID != 1054) {
					try {
						stage = "5";
						dt = DateTime.Parse ("Sat, 29 Oct 1994 12:00:00 GMT", ci);
					} catch (FormatException ex) {
						Fail (String.Format ("stage 5.1 RFC1123: culture {0} {1} failed: {2}", i, ci, ex.Message));
					}
/* comment out until we fix RFC1123
					// bug #47720
					if (dt != TimeZone.CurrentTimeZone.ToUniversalTime (dt))
						Assert (String.Format ("bug #47720 on culture {0} {1}", ci.LCID, ci), 12 != dt.Hour);
*/
					// variant of RFC1123
					try {
						stage = "6";
						dt = DateTime.Parse ("Sat, 1 Oct 1994 03:00:00", ci);
					} catch (FormatException ex) {
						Fail (String.Format ("stage 6.1 RFC1123 variant: culture {0} {1} failed: {2}", i, ci, ex.Message));
					}
					stage = "7";
					AssertEquals (String.Format ("stage 7.1 RFC1123 variant on culture {0} {1}", ci.LCID, ci), 3, dt.Hour);
				}

				switch (ci.LCID) {
				case 1025: // ar-SA
				case 1054: // th-TH
				case 1125: // div-MV
					break;
				default:
					stage = "8";
					// 02/25/2002 04:25:13 as is
					long tick1 = 631502079130000000;
					long tick2 = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (tick1)).Ticks; // adjusted to local time
					dt = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT", ci);
					AssertEquals (String.Format ("GMT variant. culture={0} {1}", i, ci), tick2, dt.Ticks);
					break;
				}

#if NET_1_1
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
						// bug #58938
						stage = "12";
						dt = DateTime.Parse ("2002#02#25 19:20:00");
						stage = "13";
						AssertEquals (String.Format ("bug #58938 on culture {0} {1}", ci.LCID, ci), 19, dt.Hour);
						break;
					}
					stage = "14";
					dt = DateTime.Parse ("2002-02-25 12:01:03");
					stage = "15";
					dt = DateTime.Parse ("2002#02#25 12:01:03");
					stage = "16";
					dt = DateTime.Parse ("2002%02%25 12:01:03");
					stage = "17";
					if (ci.DateTimeFormat.TimeSeparator != ".")
						dt = DateTime.Parse ("2002.02.25 12:01:03");
					stage = "18";
					dt = DateTime.Parse ("2003/01/23 01:34:56 GMT");
					dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
					AssertEquals (String.Format ("stage 18.1 RFC1123 UTC {0} {1}", i, ci), 1, dt.Hour);
					stage = "19";
					// This test was fixed from 12:34:56 to
					// 01:34:56 since 1078 af-ZA failed
					// because of hour interpretation
					// difference (af-ZA expects 0).
					// (IMHO it is MS BUG though.)
					dt = DateTime.Parse ("2003/01/23 12:34:56 GMT");
					dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
					if (i != 1078)
						AssertEquals (String.Format ("stage 18.1 RFC1123 UTC {0} {1}", i, ci), 12, dt.Hour);
				}
#endif
			} catch (FormatException ex) {
				Fail (String.Format ("stage {3}: Culture {0} {1} failed: {2}", i, ci, ex.Message, stage));
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
		AssertEquals ("Utc-0", 504911232000000000, u);
		long max = DateTime.MaxValue.Ticks - 504911232000000000; // w32file_epoch
		AssertEquals ("Utc-MaxValue", 3155378975999999999, DateTime.FromFileTimeUtc (max).Ticks);

		long t = DateTime.FromFileTime (0).Ticks;
		Assert ("ToFileTime>", t > (u - TimeSpan.TicksPerDay));
		Assert ("ToFileTime<", t < (u + TimeSpan.TicksPerDay));
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
	public void Milliseconds ()
	{
		DateTime dt = DateTime.Parse ("2004-05-26T03:29:01.1234567-07:00");
		dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
		AssertEquals ("DateTime with milliseconds", 632211641411234567, dt.Ticks);
	}

	[Test]
	public void ParseNotExact ()
	{
		DateTime dt = DateTime.Parse ("2004-05-26T03:29:01-07:00 foo");
		dt = TimeZone.CurrentTimeZone.ToUniversalTime (dt);
		AssertEquals ("DateTime.Parse not exact", 632211641410000000, dt.Ticks);
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
		AssertEquals ("DateTime.ParseExact with milliseconds", 632211641411234567, dt.Ticks);
	}

	[Test]
	public void NoColonTimeZone ()
	{
		AssertEquals ("DateTime with colon-less timezone", true, DateTime.Parse ("2004-05-26T03:29:01-0700").Ticks != DateTime.Parse ("2004-05-26T03:29:01-0800").Ticks);
	}

	[Test]
	public void WithColonTimeZone ()
	{
		AssertEquals ("DateTime with colon tiemzone", true, DateTime.Parse ("2004-05-26T03:29:01-07:00").Ticks != DateTime.Parse ("2004-05-26T03:29:01-08:00").Ticks);
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
		Assert ("Object", dt.Equals (dt2));
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
		Assert ("Object", dt.Equals (o));
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
		AssertEquals ("String", dt.ToString (), s);
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
}

}
