//
// DateTimeTest.cs - NUnit Test Cases for the System.DateTime struct
//
// author:
//   Martin Baulig (martin@gnome.org)
//
//   (C) 2002 Free Software Foundation
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{

public class DateTimeTest : TestCase
{
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

	protected override void SetUp() 
	{
		// the current culture determines the result of formatting
		oldcult = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("");
	}
	
	protected override void TearDown ()
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
		AssertEquals("B14", "2002-02-25 05:25:13Z", t1.ToString ("u"));
// FIXME: bugzilla #30030 
//		AssertEquals("B15", "Sunday, 24 February 2002 11:25:13", t1.ToUniversalTime().ToString ("U"));
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
		AssertEquals ("D07d", 04, t1.Hour);
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
		t2 = new DateTime (2003, 1, 25);
		t1 = DateTime.ParseExact ("25", "%d", null);
		AssertEquals ("E01", t2.Ticks, t1.Ticks);
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
	}

	public void TestParse ()
	{
		// Standard patterns
		DateTime t1 = DateTime.Parse ("02/25/2002");
		AssertEquals ("H00", myTicks[0], t1.Ticks);
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

		t1 = DateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
		t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1);
		AssertEquals ("H10a", 2002, t1.Year);
		AssertEquals ("H10b", 02, t1.Month);
		AssertEquals ("H10c", 25, t1.Day);
		AssertEquals ("H10d", 04, t1.Hour);
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
}

}
