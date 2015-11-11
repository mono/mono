//
// TimeSpanTest.cs - NUnit Test Cases for the System.TimeSpan struct
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Duco Fijma
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

namespace MonoTests.System
{

[TestFixture]
public class TimeSpanTest {

	private void Debug (TimeSpan ts) 
	{
		Console.Out.WriteLine ("Days {0}", ts.Days);
		Console.Out.WriteLine ("Hours {0}", ts.Hours);
		Console.Out.WriteLine ("Minutes {0}", ts.Minutes);
		Console.Out.WriteLine ("Seconds {0}", ts.Seconds);
		Console.Out.WriteLine ("Milliseconds {0}", ts.Milliseconds);
		Console.Out.WriteLine ("Ticks {0}", ts.Ticks);
	}

	[Test]
	public void TestCtors ()
	{
		TimeSpan t1 = new TimeSpan (1234567890);

		Assert.AreEqual ("00:02:03.4567890", t1.ToString (), "A1");
		t1 = new TimeSpan (1,2,3);
		Assert.AreEqual ("01:02:03", t1.ToString (), "A2");
		t1 = new TimeSpan (1,2,3,4);
		Assert.AreEqual ("1.02:03:04", t1.ToString (), "A3");
		t1 = new TimeSpan (1,2,3,4,5);
		Assert.AreEqual ("1.02:03:04.0050000", t1.ToString (), "A4");
		t1 = new TimeSpan (-1,2,-3,4,-5);
		Assert.AreEqual ("-22:02:56.0050000", t1.ToString (), "A5");
		t1 = new TimeSpan (0,25,0,0,0);
		Assert.AreEqual ("1.01:00:00", t1.ToString (), "A6");
    }

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void DaysOverflow () 
	{
		int days = (int) (Int64.MaxValue / TimeSpan.TicksPerDay) + 1;
		TimeSpan ts = new TimeSpan (days, 0, 0, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TemporaryOverflow () 
	{
		// calculating part of this results in overflow (days)
		// but the negative hours, minutes, seconds & ms correct this
		int days = (int) (Int64.MaxValue / TimeSpan.TicksPerDay) + 1;
		TimeSpan ts = new TimeSpan (days, Int32.MinValue, Int32.MinValue, Int32.MinValue, Int32.MinValue);
		Assert.AreEqual (10650320, ts.Days, "Days");
		Assert.AreEqual (0, ts.Hours, "Hours");
		Assert.AreEqual (14, ts.Minutes, "Minutes");
		Assert.AreEqual (28, ts.Seconds, "Seconds");
		Assert.AreEqual (352, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (9201876488683520000, ts.Ticks, "Ticks");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void NoOverflowInHoursMinsSecondsMS () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
		Assert.AreEqual (24879, ts.Days, "Days");
		Assert.AreEqual (22, ts.Hours, "Hours");
		Assert.AreEqual (44, ts.Minutes, "Minutes");
		Assert.AreEqual (30, ts.Seconds, "Seconds");
		Assert.AreEqual (647, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21496274706470000, ts.Ticks, "Ticks");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MaxDays () 
	{
		new TimeSpan (Int32.MaxValue, 0, 0, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MinDays () 
	{
		new TimeSpan (Int32.MinValue, 0, 0, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MaxHours () 
	{
		// LAMESPEC: the highest hours are "special"
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue, 0, 0, 0);
		Assert.AreEqual (0, ts.Days, "Max-Days");
		Assert.AreEqual (-1, ts.Hours, "Max-Hours");
		Assert.AreEqual (0, ts.Minutes, "Max-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Max-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Max-Milliseconds");
		Assert.AreEqual (-36000000000, ts.Ticks, "Max-Ticks");

		ts = new TimeSpan (0, Int32.MaxValue - 596522, 0, 0, 0);
		Assert.AreEqual (-24855, ts.Days, "Days");
		Assert.AreEqual (-3, ts.Hours, "Hours");
		Assert.AreEqual (0, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474828000000000, ts.Ticks, "Ticks");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MaxHours_BreakPoint () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue - 596523, 0, 0, 0);
		Assert.AreEqual (24855, ts.Days, "Days");
		Assert.AreEqual (2, ts.Hours, "Hours");
		Assert.AreEqual (28, ts.Minutes, "Minutes");
		Assert.AreEqual (16, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21474808960000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MinHours () 
	{
		TimeSpan ts = new TimeSpan (0, -256204778, 0, 0, 0);
		Assert.AreEqual (-10675199, ts.Days, "Days");
		Assert.AreEqual (-2, ts.Hours, "Hours");
		Assert.AreEqual (0, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-9223372008000000000, ts.Ticks, "Ticks");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MinHours_BreakPoint () 
	{
		TimeSpan ts = new TimeSpan (0, -2146887124, 0, 0, 0);
		Assert.AreEqual (-24855, ts.Days, "Days");
		Assert.AreEqual (-2, ts.Hours, "Hours");
		Assert.AreEqual (-28, ts.Minutes, "Minutes");
		Assert.AreEqual (-16, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474808960000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MaxMinutes () 
	{
		TimeSpan ts;
		ts = new TimeSpan (0, 0, 256204778, 0, 0);
		Assert.AreEqual (177919, ts.Days, "Max-Days");
		Assert.AreEqual (23, ts.Hours, "Max-Hours");
		Assert.AreEqual (38, ts.Minutes, "Max-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Max-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Max-Milliseconds");
		Assert.AreEqual (153722866800000000, ts.Ticks, "Max-Ticks");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MaxMinutes_BreakPoint () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue - 35791394, 0, 0, 0);
		Assert.AreEqual (0, ts.Days, "Days");
		Assert.AreEqual (0, ts.Hours, "Hours");
		Assert.AreEqual (-52, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-31200000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MinMinutes () 
	{
		TimeSpan ts;
		ts = new TimeSpan (0, 0, Int32.MinValue, 0, 0);
		Assert.AreEqual (-1491308, ts.Days, "Days");
		Assert.AreEqual (-2, ts.Hours, "Hours");
		Assert.AreEqual (-8, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-1288490188800000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MinMinutes_BreakPoint () 
	{
		TimeSpan ts = new TimeSpan (0, 0, -2111692253, 0, 0);
		Assert.AreEqual (-1466452, ts.Days, "Days");
		Assert.AreEqual (-22, ts.Hours, "Hours");
		Assert.AreEqual (-53, ts.Minutes, "Minutes");
		Assert.AreEqual (-0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-1267015351800000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MaxSeconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, Int32.MaxValue, 0);
		Assert.AreEqual (24855, ts.Days, "Days");
		Assert.AreEqual (3, ts.Hours, "Hours");
		Assert.AreEqual (14, ts.Minutes, "Minutes");
		Assert.AreEqual (7, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21474836470000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MinSeconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, Int32.MinValue, 0);
		Assert.AreEqual (-24855, ts.Days, "Days");
		Assert.AreEqual (-3, ts.Hours, "Hours");
		Assert.AreEqual (-14, ts.Minutes, "Minutes");
		Assert.AreEqual (-8, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474836480000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MaxMilliseconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, 0, Int32.MaxValue);
		Assert.AreEqual (24, ts.Days, "Days");
		Assert.AreEqual (20, ts.Hours, "Hours");
		Assert.AreEqual (31, ts.Minutes, "Minutes");
		Assert.AreEqual (23, ts.Seconds, "Seconds");
		Assert.AreEqual (647, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21474836470000, ts.Ticks, "Ticks");
	}

	[Test]
	public void MinMilliseconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, 0, Int32.MinValue);
		Assert.AreEqual (-24, ts.Days, "Days");
		Assert.AreEqual (-20, ts.Hours, "Hours");
		Assert.AreEqual (-31, ts.Minutes, "Minutes");
		Assert.AreEqual (-23, ts.Seconds, "Seconds");
		Assert.AreEqual (-648, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474836480000, ts.Ticks, "Ticks");
	}

	[Test]
	public void NegativeTimeSpan () 
	{
		TimeSpan ts = new TimeSpan (-23, -59, -59);
		Assert.AreEqual (0, ts.Days, "Days");
		Assert.AreEqual (-23, ts.Hours, "Hours");
		Assert.AreEqual (-59, ts.Minutes, "Minutes");
		Assert.AreEqual (-59, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-863990000000, ts.Ticks, "Ticks");
	}

	[Test]
	public void TestProperties ()
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;

		Assert.AreEqual (1, t1.Days, "A1");
		Assert.AreEqual (2, t1.Hours, "A2");
		Assert.AreEqual (3, t1.Minutes, "A3");
		Assert.AreEqual (4, t1.Seconds, "A4");
		Assert.AreEqual (5, t1.Milliseconds, "A5");
		Assert.AreEqual (-1, t2.Days, "A6");
		Assert.AreEqual (-2, t2.Hours, "A7");
		Assert.AreEqual (-3, t2.Minutes, "A8");
		Assert.AreEqual (-4, t2.Seconds, "A9");
		Assert.AreEqual (-5, t2.Milliseconds, "A10");
	}

	[Test]
	public void TestAdd ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 + t2;
		TimeSpan t4 = t1.Add (t2);
		TimeSpan t5;
		bool exception;

		Assert.AreEqual (3, t3.Days, "A1");
		Assert.AreEqual (5, t3.Hours, "A2");
		Assert.AreEqual (7, t3.Minutes, "A3");
		Assert.AreEqual (9, t3.Seconds, "A4");
		Assert.AreEqual (11, t3.Milliseconds, "A5");
		Assert.AreEqual ("3.05:07:09.0110000", t4.ToString (), "A6");
		try
		{
			t5 = TimeSpan.MaxValue + new TimeSpan (1);			
			exception = false;
		}
		catch (OverflowException)
		{
			exception = true;
		}
		Assert.IsTrue (exception, "A7");
	}

	[Test]
	public void TestCompare ()
	{
		TimeSpan t1 = new TimeSpan (-1);
		TimeSpan t2 = new TimeSpan (1);
		int res;
		bool exception;

		Assert.AreEqual (-1, TimeSpan.Compare (t1, t2), "A1");
		Assert.AreEqual (1, TimeSpan.Compare (t2, t1), "A2");
		Assert.AreEqual (0, TimeSpan.Compare (t2, t2), "A3");
		Assert.AreEqual (-1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue), "A4");
		Assert.AreEqual (-1, t1.CompareTo (t2), "A5");
		Assert.AreEqual (1, t2.CompareTo (t1), "A6");
		Assert.AreEqual (0, t2.CompareTo (t2), "A7");
		Assert.AreEqual (-1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue), "A8");

		Assert.AreEqual (1, TimeSpan.Zero.CompareTo (null), "A9");
		
		try
		{
			res = TimeSpan.Zero.CompareTo("");
			exception = false;	
		}
		catch (ArgumentException)
		{
			exception = true;
		}
		Assert.IsTrue (exception, "A10");

		Assert.AreEqual (false, t1 == t2, "A11");
		Assert.AreEqual (false, t1 > t2, "A12");
		Assert.AreEqual (false, t1 >= t2, "A13");
		Assert.AreEqual (true, t1 != t2, "A14");
		Assert.AreEqual (true, t1 < t2, "A15");
		Assert.AreEqual (true, t1 <= t2, "A16");
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void NoNegateMinValue() {
		TimeSpan t1 = TimeSpan.MinValue.Negate ();
	}

	[Test]
	public void TestNegateAndDuration ()
	{
		TimeSpan t1;
		bool exception;

		Assert.AreEqual ("-00:00:00.0012345", new TimeSpan (12345).Negate ().ToString (), "A1");
		Assert.AreEqual ("00:00:00.0012345", new TimeSpan (-12345).Duration ().ToString (), "A2");
			
		try
		{
			t1 = TimeSpan.MinValue.Duration ();
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert.IsTrue (exception, "A4");

		Assert.AreEqual ("-00:00:00.0000077", (-(new TimeSpan (77))).ToString (), "A5");
		Assert.AreEqual ("00:00:00.0000077", (+(new TimeSpan(77))).ToString(), "A6");
	}

	[Test]
	public void TestEquals ()
	{
		TimeSpan t1 = new TimeSpan (1);
		TimeSpan t2 = new TimeSpan (2);
		string s = "justastring";

		Assert.AreEqual (true, t1.Equals (t1), "A1");
		Assert.AreEqual (false, t1.Equals (t2), "A2");
		Assert.AreEqual (false, t1.Equals (s), "A3");
		Assert.AreEqual (false, t1.Equals (null), "A4");
		Assert.AreEqual (true, TimeSpan.Equals (t1, t1), "A5");
		Assert.AreEqual (false, TimeSpan.Equals (t1, t2), "A6");
		Assert.AreEqual (false, TimeSpan.Equals (t1, null), "A7");
		Assert.AreEqual (false, TimeSpan.Equals (t1, s), "A8");
		Assert.AreEqual (false, TimeSpan.Equals (s, t2), "A9");
		Assert.AreEqual (true, TimeSpan.Equals (null, null), "A10");
	}

	[Test]
	public void TestFromXXXX ()
	{
		Assert.AreEqual ("12.08:16:48", TimeSpan.FromDays (12.345).ToString (), "A1");
		Assert.AreEqual ("12:20:42", TimeSpan.FromHours (12.345).ToString (), "A2");
		Assert.AreEqual ("00:12:20.7000000", TimeSpan.FromMinutes (12.345).ToString (), "A3");
		Assert.AreEqual ("00:00:12.3450000", TimeSpan.FromSeconds (12.345).ToString (), "A4");
		Assert.AreEqual ("00:00:00.0120000", TimeSpan.FromMilliseconds (12.345).ToString (), "A5");
		Assert.AreEqual ("00:00:00.0012345", TimeSpan.FromTicks (12345).ToString (), "A6");
		Assert.AreEqual ("-00:00:00.0010000", TimeSpan.FromMilliseconds (-0.5).ToString (), "A7");
		Assert.AreEqual ("00:00:00.0010000", TimeSpan.FromMilliseconds (0.5).ToString (), "A8");
		Assert.AreEqual ("-00:00:00.0030000", TimeSpan.FromMilliseconds (-2.5).ToString (), "A9");
		Assert.AreEqual ("00:00:00.0030000", TimeSpan.FromMilliseconds (2.5).ToString (), "A10");
		Assert.AreEqual ("00:00:00.0010000", TimeSpan.FromSeconds (0.0005).ToString (), "A11");
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_MinValue ()
	{
		TimeSpan.FromDays (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_MaxValue ()
	{
		TimeSpan.FromDays (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromDays_NaN ()
	{
		TimeSpan.FromDays (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.FromDays (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.FromDays (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_MinValue ()
	{
		TimeSpan.FromHours (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_MaxValue ()
	{
		TimeSpan.FromHours (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromHours_NaN ()
	{
		TimeSpan.FromHours (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.FromHours (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.FromHours (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_MinValue ()
	{
		TimeSpan.FromMilliseconds (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_MaxValue ()
	{
		TimeSpan.FromMilliseconds (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromMilliseconds_NaN ()
	{
		TimeSpan.FromMilliseconds (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.FromMilliseconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.FromMilliseconds (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_MinValue ()
	{
		TimeSpan.FromMinutes (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_MaxValue ()
	{
		TimeSpan.FromMinutes (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromMinutes_NaN ()
	{
		TimeSpan.FromMinutes (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.FromMinutes (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.FromMinutes (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_MinValue ()
	{
		TimeSpan.FromSeconds (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_MaxValue ()
	{
		TimeSpan.FromSeconds (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromSeconds_NaN ()
	{
		TimeSpan.FromSeconds (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.FromSeconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.FromSeconds (Double.NegativeInfinity));
	}

	[Test]
	public void TestGetHashCode ()
	{
		Assert.AreEqual (77, new TimeSpan (77).GetHashCode (), "A1");
	}

	private void ParseHelper (string s, bool expectFormat, bool expectOverflow, string expect)
	{
		bool formatException = false;
		bool overflowException = false;
		string result = "junk ";

		try {
			result =  TimeSpan.Parse (s).ToString ();
		}
		catch (OverflowException) {
			overflowException = true;
		}
		catch (FormatException) {
			formatException = true;
		}
		Assert.AreEqual (expectFormat, formatException, "A1 [" + s + "]");
		Assert.AreEqual (expectOverflow, overflowException, "A2 " + s + "]");

		if (!expectOverflow && !expectFormat) {
			Assert.AreEqual (expect, result, "A3 [" + s + "]");
		}
	}

	[Test]
	public void TestParse ()
	{
		ParseHelper (" 13:45:15 ",false, false, "13:45:15");
		ParseHelper (" -1:2:3 ", false, false, "-01:02:03");

		// In 4.0 when the first part is out of range, it parses it as day.
		ParseHelper (" 25:11:12 ", false, false, "25.11:12:00");
		ParseHelper (" 24:11:12 ", false, false, "24.11:12:00");
		ParseHelper (" 23:11:12 ", false, false, "23:11:12");

		ParseHelper ("-21.23:59:59.9999999", false, false, "-21.23:59:59.9999999");
		ParseHelper ("10:12  ", false, false, "10:12:00");
		ParseHelper ("aaa", true, false, "dontcare");

		ParseHelper ("100000000000000.1:1:1", false, true, "dontcare");
		ParseHelper ("24:60:60", false, true, "dontcare");
		ParseHelper ("0001:0002:0003.12     ", false, false, "01:02:03.1200000");

		// In 4.0 when a section has more than 7 digits an OverflowException is thrown.
		ParseHelper (" 1:2:3:12345678 ", false, true, "dontcare");

		ParseHelper ("10:11:12:13", false, false, "10.11:12:13"); // Days using : instead of . as separator
		ParseHelper ("10.11", true, false, "dontcare"); // days+hours is invalid

		// Force the use of french culture -which is using a non common NumberDecimalSeparator-
		// as current culture, to show that the Parse method is *actually* being culture sensitive
		// *and* also keeping the compatibility with '.'
		CultureInfo french_culture = CultureInfo.GetCultureInfo ("fr-FR");
		CultureInfo prev_culture = CultureInfo.CurrentCulture;
		try {
			Thread.CurrentThread.CurrentCulture = french_culture;
			ParseHelper ("10:10:10,006", false, false, "10:10:10.0060000");
			ParseHelper ("10:10:10.006", false, false, "10:10:10.0060000");
		} finally {
			// restore culture
			Thread.CurrentThread.CurrentCulture = prev_culture;
		}

		ParseHelper ("00:00:00", false, false, "00:00:00");
		ParseHelper ("00:10:00", false, false, "00:10:00");
	}

	// LAMESPEC: timespan in documentation is wrong - hh:mm:ss isn't mandatory
	[Test]
	public void Parse_Days_WithoutColon () 
	{
		TimeSpan ts = TimeSpan.Parse ("1");
		Assert.AreEqual (1, ts.Days, "Days");
	}

	[Test]
	public void TestSubstract ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 - t2;
		TimeSpan t4 = t1.Subtract (t2);
		TimeSpan t5;
		bool exception;

		Assert.AreEqual ("1.01:01:01.0010000", t3.ToString (), "A1");
		Assert.AreEqual ("1.01:01:01.0010000", t4.ToString (), "A2");
		try {
			t5 = TimeSpan.MinValue - new TimeSpan (1);
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert.IsTrue (exception, "A3");
	}

	[Test]
	public void TestToString () 
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;
		
		Assert.AreEqual ("1.02:03:04.0050000", t1.ToString (), "A1");
		Assert.AreEqual ("-1.02:03:04.0050000", t2.ToString (), "A2");
		Assert.AreEqual ("10675199.02:48:05.4775807", TimeSpan.MaxValue.ToString (), "A3");
		Assert.AreEqual ("-10675199.02:48:05.4775808", TimeSpan.MinValue.ToString (), "A4");
	}

	[Test]
	public void ToString_Constants () 
	{
		Assert.AreEqual ("00:00:00", TimeSpan.Zero.ToString (), "Zero");
		Assert.AreEqual ("10675199.02:48:05.4775807", TimeSpan.MaxValue.ToString (), "MaxValue");
		Assert.AreEqual ("-10675199.02:48:05.4775808", TimeSpan.MinValue.ToString (), "MinValue");
	}

	[Test]
	public void Parse_InvalidValuesAndFormat_ExceptionOrder () 
	{
		// hours should be between 0 and 23 but format is also invalid (too many dots)
		// In 2.0 overflow as precedence over format, but not in 4.0
		try {
			TimeSpan.Parse ("0.99.99.0");
			Assert.Fail ("#A1");
		} catch (FormatException) {
		}
		try {
			TimeSpan.Parse ("0.999999999999.99.0");
			Assert.Fail ("#A2");
		} catch (OverflowException) {
		}
	}

	[Test]
	public void Parse_MinMaxValues () 
	{
		Assert.AreEqual (TimeSpan.MaxValue, TimeSpan.Parse ("10675199.02:48:05.4775807"), "MaxValue");
		Assert.AreEqual (TimeSpan.MinValue, TimeSpan.Parse ("-10675199.02:48:05.4775808"), "MinValue");
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void Parse_OverMaxValue() 
	{
		TimeSpan.Parse ("10675199.02:48:05.4775808");
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void Parse_UnderMinValue() 
	{
		TimeSpan.Parse ("-10675199.02:48:05.4775809");
	}

	[Test]
	public void ParseMissingSeconds ()
	{
		// as seen in ML for http://resources.esri.com/arcgisserver/apis/silverlight/
		TimeSpan ts = TimeSpan.Parse ("0:0:.75");

		Assert.AreEqual (0, ts.Days, "Days");
		Assert.AreEqual (0, ts.Hours, "Hours");
		Assert.AreEqual (750, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (0, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (7500000, ts.Ticks, "Ticks");
		Assert.AreEqual (0.0000086805555555555555, ts.TotalDays, 0.00000000000000001, "TotalDays");
		Assert.AreEqual (0.00020833333333333332, ts.TotalHours, 0.00000000000000001, "TotalHours");
		Assert.AreEqual (750.0, ts.TotalMilliseconds, "TotalMilliseconds");
		Assert.AreEqual (0.0125, ts.TotalMinutes, "TotalMinutes");
		Assert.AreEqual (0.75, ts.TotalSeconds, "TotalSeconds");
	}

	// 'Ported' the Parse test to use TryParse
	[Test]
	public void TryParse ()
	{
		TimeSpan result;

		Assert.AreEqual (true, TimeSpan.TryParse (" 13:45:15 ", out result), "#A1");
		Assert.AreEqual ("13:45:15", result.ToString (), "#A2");

		Assert.AreEqual (true, TimeSpan.TryParse (" -1:2:3 ", out result), "#B1");
		Assert.AreEqual ("-01:02:03", result.ToString (), "#B2");

		Assert.AreEqual (false, TimeSpan.TryParse ("aaa", out result), "#C2");

		Assert.AreEqual (true, TimeSpan.TryParse ("-21.23:59:59.9999999", out result), "#D1");
		Assert.AreEqual ("-21.23:59:59.9999999", result.ToString (), "#D2");

		Assert.AreEqual (false, TimeSpan.TryParse ("100000000000000.1:1:1", out result), "#E1");
		Assert.AreEqual (false, TimeSpan.TryParse ("24:60:60", out result), "#E2");

		Assert.AreEqual (true, TimeSpan.TryParse ("0001:0002:0003.12     ", out result), "#F1");

		Assert.AreEqual (false, TimeSpan.TryParse (" 1:2:3:12345678 ", out result), "#G1");

		// Min and Max values
		Assert.AreEqual (true, TimeSpan.TryParse ("10675199.02:48:05.4775807", out result), "MaxValue#1");
		Assert.AreEqual (TimeSpan.MaxValue, result, "MaxValue#2");
		Assert.AreEqual (true, TimeSpan.TryParse ("-10675199.02:48:05.4775808", out result), "MinValue#1");
		Assert.AreEqual (TimeSpan.MinValue, result, "MinValue#2");

		// Force the use of french culture -which is using a non common NumberDecimalSeparator-
		// as current culture, to show that the Parse method is *actually* being culture sensitive
		CultureInfo french_culture = CultureInfo.GetCultureInfo ("fr-FR");
		CultureInfo prev_culture = CultureInfo.CurrentCulture;
		result = new TimeSpan (0, 10, 10, 10, 6);
		try {
			Thread.CurrentThread.CurrentCulture = french_culture;
			Assert.AreEqual (true, TimeSpan.TryParse ("10:10:10,006", out result), "#CultureSensitive1");
			Assert.AreEqual ("10:10:10.0060000", result.ToString (), "#CultureSensitive2");
		} finally {
			// restore culture
			Thread.CurrentThread.CurrentCulture = prev_culture;
		}
	}

	[Test]
	public void TryParseErrors ()
	{
		TimeSpan result;

		Assert.AreEqual (false, TimeSpan.TryParse ("0.99.99.0", out result), "Format#1");
		Assert.AreEqual (false, TimeSpan.TryParse ("10675199.02:48:05.4775808", out result), "OverMaxValue");
		Assert.AreEqual (false, TimeSpan.TryParse ("-10675199.02:48:05.4775809", out result), "UnderMinValue");
	}

	[Test]
	public void TryParseOverloads ()
	{ 
		TimeSpan result;

		// We use fr-FR culture since its NumericDecimalSeparator is not the same used by
		// most cultures - including the invariant one.
		CultureInfo french_culture = CultureInfo.GetCultureInfo ("fr-FR");
		Assert.AreEqual (true, TimeSpan.TryParse ("11:50:50,006", french_culture, out result), "#A1");

		// LAMESPEC - msdn states that an instance of DateTimeFormatInfo is retrieved to
		// obtain culture sensitive information, but at least in the betas that's false
		DateTimeFormatInfo format_info = new DateTimeFormatInfo ();
		format_info.TimeSeparator = ";";
		Assert.AreEqual (false, TimeSpan.TryParse ("11;50;50", format_info, out result), "#B1");
		Assert.AreEqual (true, TimeSpan.TryParse ("11:50:50", format_info, out result), "#B2");
	}

	[Test]
	public void ParseExact ()
	{
		CultureInfo french_culture = CultureInfo.GetCultureInfo ("fr-FR");
		CultureInfo us_culture = CultureInfo.GetCultureInfo ("en-US");

		// At this point we are only missing the style bites and then we are
		// pretty much done with the standard formats.

		//
		// 'g' format - this is the short and culture sensitive format
		//
		string [] g_format = new string [] { "g" };
		ParseExactHelper ("12", g_format, false, false, "12.00:00:00");
		ParseExactHelper ("11:12", g_format, false, false, "11:12:00");
		ParseExactHelper ("-11:12", g_format, false, false, "-11:12:00");
		ParseExactHelper ("25:13", g_format, true, false, "dontcare");
		ParseExactHelper ("11:66", g_format, true, false, "dontcare"); // I'd have expected OverflowExc here
		ParseExactHelper ("11:12:13", g_format, false, false, "11:12:13");
		ParseExactHelper ("-11:12:13", g_format, false, false, "-11:12:13");
		ParseExactHelper ("10.11:12:13", g_format, true, false, "dontcare"); // this should work as well
		ParseExactHelper ("10.11:12:13", g_format, true, false, "dontcare", us_culture);
		ParseExactHelper ("10.11:12:13", g_format, true, false, "dontcare", CultureInfo.InvariantCulture);
		ParseExactHelper ("10:11:12:66", g_format, true, false, "dontcare");
		ParseExactHelper ("10:11:12:13", g_format, false, false, "10.11:12:13");
		ParseExactHelper ("11:12:13.6", g_format, false, false, "11:12:13.6000000", CultureInfo.InvariantCulture);
		ParseExactHelper ("11:12:13,6", g_format, false, false, "11:12:13.6000000", french_culture);
		ParseExactHelper ("10:11:12:13.6", g_format, false, false, "10.11:12:13.6000000", us_culture);
		ParseExactHelper (" 10:11:12:13.6 ", g_format, false, false, "10.11:12:13.6000000", us_culture);
		ParseExactHelper ("10:11", g_format, false, false, "10:11:00", null, TimeSpanStyles.None);
		ParseExactHelper ("10:11", g_format, false, false, "10:11:00", null, TimeSpanStyles.AssumeNegative); // no effect

		// 
		// G format
		//
		string [] G_format = new string [] { "G" };
		ParseExactHelper ("9:10:12", G_format, true, false, "dontcare");
		ParseExactHelper ("9:10:12.6", G_format, true, false, "dontcare");
		ParseExactHelper ("3.9:10:12", G_format, true, false, "dontcare");
		ParseExactHelper ("3.9:10:12.153", G_format, true, false, "dontcare"); // this should be valid...
		ParseExactHelper ("3:9:10:12.153", G_format, false, false, "3.09:10:12.1530000", us_culture);
		ParseExactHelper ("0:9:10:12.153", G_format, false, false, "09:10:12.1530000", us_culture);
		ParseExactHelper ("03:09:10:12.153", G_format, false, false, "3.09:10:12.1530000", us_culture);
		ParseExactHelper ("003:009:0010:0012.00153", G_format, false, false, "3.09:10:12.0015300", us_culture);
		ParseExactHelper ("3:9:10:66.153", G_format, true, false, "dontcare"); // seconds out of range
		ParseExactHelper ("3:9:10:12.153", G_format, true, false, "dontcare", french_culture); // fr-FR uses ',' as decimal separator
		ParseExactHelper ("3:9:10:12,153", G_format, false, false, "3.09:10:12.1530000", french_culture);
		ParseExactHelper ("  3:9:10:12.153  ", G_format, false, false, "3.09:10:12.1530000", us_culture);
		ParseExactHelper ("3:9:10:13.153", G_format, false, false, "3.09:10:13.1530000", us_culture, TimeSpanStyles.AssumeNegative);

		// c format
		string [] c_format = new string [] { "c" };
		ParseExactHelper ("12", c_format, false, false, "12.00:00:00");
		ParseExactHelper ("12:11", c_format, false, false, "12:11:00");
		ParseExactHelper ("12:66", c_format, true, false, "dontcare");
		ParseExactHelper ("10.11:12", c_format, false, false, "10.11:12:00");
		ParseExactHelper ("10.11:12:13", c_format, false, false, "10.11:12:13");
		ParseExactHelper ("10:11:12:13", c_format, true, false, "dontcare"); // this is normally accepted in the Parse method
		ParseExactHelper ("10.11:12:13.6", c_format, false, false, "10.11:12:13.6000000");
		ParseExactHelper ("10:11:12,6", c_format, true, false, "dontcare");
		ParseExactHelper ("10:11:12,6", c_format, true, false, "dontcare", french_culture);
		ParseExactHelper ("  10:11:12.6  ", c_format, false, false, "10:11:12.6000000");
		ParseExactHelper ("10:12", c_format, false, false, "10:12:00", null, TimeSpanStyles.AssumeNegative);
		ParseExactHelper ("10:123456789999", c_format, true, false, "dontcare");

		ParseExactHelper ("10:12", new string [0], true, false, "dontcare");
		ParseExactHelper ("10:12", new string [] { String.Empty }, true, false, "dontcare");
		ParseExactHelper ("10:12", new string [] { null }, true, false, "dontcare");
	}

	[Test]
	public void ParseExactMultipleFormats ()
	{
		ParseExactHelper ("10:12", new string [] { "G", "g" }, false, false, "10:12:00");
		ParseExactHelper ("10:12", new string [] { "g", "G" }, false, false, "10:12:00");
		ParseExactHelper ("7.8:9:10", new string [] { "G", "g" }, true, false, "dontcare");
		ParseExactHelper ("7.8:9:10", new string [] { "G", "g", "c" }, false, false, "7.08:09:10");
		ParseExactHelper ("7:8:9:10", new string [] { "c", "g" }, false, false, "7.08:09:10");
		ParseExactHelper ("7:8:9:10", new string [] { "c", "G" }, true, false, "dontcare");
		ParseExactHelper ("7.123456789:1", new string [] { "c", "G", "g" }, true, false, "dontcare");
		ParseExactHelper ("7.123456789:1", new string [] { "G", "g", "c" }, true, false, "dontcare");
		ParseExactHelper ("1234567890123456", new string [] { "c", "g" }, true, false, "dontcare"); // I'd expect an OverflowException
		ParseExactHelper ("10:12", new string [] { null, "c", "g" }, true, false, "10:12:00");
		ParseExactHelper ("10:12", new string [] { String.Empty, "c", "g" }, true, false, "10:12:00");
	}

	[Test]
	public void ParseExactCustomFormats ()
	{
		// Days
		ParseExactHelper ("33", new string [] { "%d" }, false, false, "33.00:00:00");
		ParseExactHelper ("00", new string [] { "%d" }, false, false, "00:00:00");
		ParseExactHelper ("33", new string [] { "%dd" }, false, false, "33.00:00:00");
		ParseExactHelper ("3333", new string [] { "%d" }, false, false, "3333.00:00:00");
		ParseExactHelper ("3333", new string [] { "%ddd" }, true, false, "3333.00:00:00"); // 'dd' mismatch the digit count
		ParseExactHelper ("3333", new string [] { "%dddd" }, false, false, "3333.00:00:00");
		ParseExactHelper ("00033", new string [] { "%ddddd" }, false, false, "33.00:00:00");
		ParseExactHelper ("00033", new string [] { "%d" }, false, false, "33.00:00:00");
		ParseExactHelper ("00000003", new string [] { "%dddddddd" }, false, false, "3.00:00:00"); // up to 8 'd'
		ParseExactHelper ("000000003", new string [] { "%ddddddddd" }, true, false, "dontcare");
		ParseExactHelper ("33", new string [] { "d" }, true, false, "33.00:00:00"); // This is sort of weird.
		ParseExactHelper ("33", new string [] { "dd" }, false, false, "33.00:00:00");
		ParseExactHelper ("-33", new string [] { "%d" }, true, false, "dontcare");
		ParseExactHelper ("33", new string [] { "%d" }, false, false, "-33.00:00:00", null, TimeSpanStyles.AssumeNegative);

		// Hours
		ParseExactHelper ("12", new string [] { "%h" }, false, false, "12:00:00");
		ParseExactHelper ("00", new string [] { "%h" }, false, false, "00:00:00");
		ParseExactHelper ("012", new string [] { "%h" }, true, false, "dontcare"); // more than 2 digits
		ParseExactHelper ("00012", new string [] { "%hhhhh" }, true, false, "dontcare");
		ParseExactHelper ("15", new string [] { "%h" }, false, false, "15:00:00");
		ParseExactHelper ("24", new string [] { "%h" }, true, false, "dontcare");
		ParseExactHelper ("15", new string [] { "%hh" }, false, false, "15:00:00");
		ParseExactHelper ("1", new string [] { "%hh" }, true, false, "dontcare"); // 'hh' but a single digit
		ParseExactHelper ("01", new string [] { "%hh" }, false, false, "01:00:00");
		ParseExactHelper ("015", new string [] { "%hhh" }, true, false, "dontcare"); // Too many 'h'
		ParseExactHelper ("12", new string [] { "h" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "hh" }, false, false, "12:00:00");
		ParseExactHelper ("-15", new string [] {"%h"}, true, false, "dontcare"); // Explicit - not accepted
		ParseExactHelper ("15", new string [] { "%h" }, false, false, "-15:00:00", null, TimeSpanStyles.AssumeNegative);
		ParseExactHelper ("15", new string [] { "%H" }, true, false, "dontcare"); // Uppercase is not accepted

		// Minutes
		ParseExactHelper ("12", new string [] { "%m" }, false, false, "00:12:00");
		ParseExactHelper ("00", new string [] { "%m" }, false, false, "00:00:00");
		ParseExactHelper ("60", new string [] { "%m" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%mm" }, false, false, "00:12:00");
		ParseExactHelper ("1", new string [] { "%mm" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%mmm" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "m" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "mm" }, false, false, "00:12:00");
		ParseExactHelper ("-12", new string [] { "%m" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%m" }, false, false, "-00:12:00", null, TimeSpanStyles.AssumeNegative);
		ParseExactHelper ("12", new string [] { "%M" }, true, false, "dontcare");

		// Seconds
		ParseExactHelper ("12", new string [] { "%s" }, false, false, "00:00:12");
		ParseExactHelper ("00", new string [] { "%s" }, false, false, "00:00:00");
		ParseExactHelper ("000", new string [] { "%s" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%ss" }, false, false, "00:00:12");
		ParseExactHelper ("12", new string [] { "%sss" }, true, false, "dontcare");
		ParseExactHelper ("60", new string [] { "%s" }, true, false, "dontcare");
		ParseExactHelper ("-12", new string [] { "%s" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%s" }, false, false, "-00:00:12", null, TimeSpanStyles.AssumeNegative);

		// Fractions of seconds - f
		ParseExactHelper ("3", new string [] { "%f" }, false, false, "00:00:00.3000000");
		ParseExactHelper ("0", new string [] { "%f" }, false, false, "00:00:00");
		ParseExactHelper ("03", new string [] { "%f" }, true, false, "dontcare"); // This would work for other elements
		ParseExactHelper ("10", new string [] { "%f" }, true, false, "dontcare"); // Only a digit is accepted with '%f'
		ParseExactHelper ("3", new string [] { "%ff" }, true, false, "dontcare");
		ParseExactHelper ("12", new string [] { "%ff" }, false, false, "00:00:00.1200000");
		ParseExactHelper ("123", new string [] { "%ff" }, true, false, "dontcare");
		ParseExactHelper ("123", new string [] { "%fff" }, false, false, "00:00:00.1230000");
		ParseExactHelper ("1234", new string [] { "%ffff" }, false, false, "00:00:00.1234000");
		ParseExactHelper ("1234567", new string [] { "%fffffff" }, false, false, "00:00:00.1234567");
		ParseExactHelper ("1234567", new string [] { "%FfFFFFF" }, true, false, "dontcare"); // Mixed f and M
		ParseExactHelper ("12345678", new string [] { "%ffffffff" }, true, false, "dontcare");
		ParseExactHelper ("0000000", new string [] { "%fffffff" }, false, false, "00:00:00");

		// Fractions of second - F
		ParseExactHelper ("3", new string [] { "%F" }, false, false, "00:00:00.3000000");
		ParseExactHelper ("333", new string [] { "%FFFFF" }, false, false, "00:00:00.3330000");
		ParseExactHelper ("1234567", new string [] { "%FFFFFFF" }, false, false, "00:00:00.1234567");

		// Multiple symbols
		ParseExactHelper ("9:10", new string [] { @"h\:m" }, false, false, "09:10:00");
		ParseExactHelper ("9;10", new string [] { @"h\;m" }, false, false, "09:10:00");
		ParseExactHelper ("10:9", new string [] { @"m\:h" }, false, false, "09:10:00");
		ParseExactHelper ("10:9", new string [] { @"%m\:%h" }, false, false, "09:10:00");
		ParseExactHelper ("9 10", new string [] { @"h\ m" }, false, false, "09:10:00");
		ParseExactHelper ("9   10", new string [] { @"h\ \ \ m" }, false, false, "09:10:00");
		ParseExactHelper (" 9:10 ", new string [] { @"h\:m" }, true, false, "dontcare");
		ParseExactHelper ("9:10:11", new string [] { @"h\:m\:s" }, false, false, "09:10:11");
		ParseExactHelper ("9:10:11:6", new string [] { @"h\:m\:s\:f" }, false, false, "09:10:11.6000000");
		ParseExactHelper ("9:10:11:666", new string [] { @"h\:m\:s\:f" }, true, false, "dontcare"); // fff with 1 digit
		ParseExactHelper ("9:10:11:", new string [] { @"h\:m\:s\:F" }, false, false, "09:10:11"); // optional frac of seconds
		ParseExactHelper ("9:10:11:", new string [] { @"h\:m\:s\:FF" }, false, false, "09:10:11");
		ParseExactHelper ("9:10:11::", new string [] { @"h\:m\:s\:F\:" }, false, false, "09:10:11");
		ParseExactHelper ("8:9:10:11:6666666", new string [] { @"d\:h\:m\:s\:fffffff" }, false, false, "8.09:10:11.6666666");
		ParseExactHelper ("8:9:10:11:6666666", new string [] { @"d\:h\:m\:s\:fffffff" }, false, false, "-8.09:10:11.6666666", 
				null, TimeSpanStyles.AssumeNegative);
		ParseExactHelper ("9:10", new string [] { @"h\:h" }, true, false, "dontcare"); // Repeated element

		// Misc
		ParseExactHelper (" 0 ", new string [] { "%d" }, true, false, "dontcare");
		ParseExactHelper (" 0 ", new string [] { " %d " }, true, false, "dontcare");
		ParseExactHelper ("0", new string [] { " %d " }, true, false, "dontcare");
		ParseExactHelper ("::", new string [] { @"\:\:" }, false, false, "00:00:00"); // funny
		ParseExactHelper ("::", new string [] { @"\:\:" }, false, false, "00:00:00", null, TimeSpanStyles.AssumeNegative);
		ParseExactHelper (" 0", new string [] { @"\ d" }, false, false, "00:00:00");
		ParseExactHelper ("Interval = 12:13:14", new string [] { @"'Interval = 'h\:m\:s" }, false, false, "12:13:14");
	}

	void ParseExactHelper (string input, string [] formats, bool format_error, bool overflow_error, string expected, 
        IFormatProvider formatProvider = null, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None)
	{
		bool overflow_exc = false;
		bool format_exc = false;
		TimeSpan result = TimeSpan.Zero;

		try {
			result = TimeSpan.ParseExact (input, formats, formatProvider, timeSpanStyles);
		} catch (OverflowException) {
			overflow_exc = true;
		} catch (FormatException) {
			format_exc = true;
		}

		Assert.AreEqual (format_error, format_exc, "A1");
		Assert.AreEqual (overflow_error, overflow_exc, "A2");
		if (!overflow_exc && !format_exc)
			Assert.AreEqual (expected, result.ToString ());
	}

	// 'Ported' the ParseExact test to use TryParseExact instead.
	[Test]
	public void TryParseExact ()
	{
		CultureInfo french_culture = CultureInfo.GetCultureInfo ("fr-FR");
		CultureInfo us_culture = CultureInfo.GetCultureInfo ("en-US");

		//
		// 'g' format - this is the short and culture sensitive format
		//
		string [] g_format = new string [] { "g" };
		TryParseExactHelper ("12", g_format, false, "12.00:00:00");
		TryParseExactHelper ("11:12", g_format, false, "11:12:00");
		TryParseExactHelper ("-11:12", g_format, false, "-11:12:00");
		TryParseExactHelper ("25:13", g_format, true, "dontcare");
		TryParseExactHelper ("11:66", g_format, true, "dontcare"); // I'd have expected OverflowExc here
		TryParseExactHelper ("11:12:13", g_format, false, "11:12:13");
		TryParseExactHelper ("-11:12:13", g_format, false, "-11:12:13");
		TryParseExactHelper ("10.11:12:13", g_format, true, "dontcare"); // this should work as well
		TryParseExactHelper ("10.11:12:13", g_format, true, "dontcare", us_culture);
		TryParseExactHelper ("10.11:12:13", g_format, true, "dontcare", CultureInfo.InvariantCulture);
		TryParseExactHelper ("10:11:12:66", g_format, true, "dontcare");
		TryParseExactHelper ("10:11:12:13", g_format, false, "10.11:12:13");
		TryParseExactHelper ("11:12:13.6", g_format, false, "11:12:13.6000000", CultureInfo.InvariantCulture);
		TryParseExactHelper ("11:12:13,6", g_format, false, "11:12:13.6000000", french_culture);
		TryParseExactHelper ("10:11:12:13.6", g_format, false, "10.11:12:13.6000000", us_culture);
		TryParseExactHelper (" 10:11:12:13.6 ", g_format, false, "10.11:12:13.6000000", us_culture);
		TryParseExactHelper ("10:11", g_format, false, "10:11:00", null, TimeSpanStyles.None);
		TryParseExactHelper ("10:11", g_format, false, "10:11:00", null, TimeSpanStyles.AssumeNegative); // no effect

		// 
		// G format
		//
		string [] G_format = new string [] { "G" };
		TryParseExactHelper ("9:10:12", G_format, true, "dontcare");
		TryParseExactHelper ("9:10:12.6", G_format, true, "dontcare");
		TryParseExactHelper ("3.9:10:12", G_format, true, "dontcare");
		TryParseExactHelper ("3.9:10:12.153", G_format, true, "dontcare"); // this should be valid...
		TryParseExactHelper ("3:9:10:12.153", G_format, false, "3.09:10:12.1530000", us_culture);
		TryParseExactHelper ("0:9:10:12.153", G_format, false, "09:10:12.1530000", us_culture);
		TryParseExactHelper ("03:09:10:12.153", G_format, false, "3.09:10:12.1530000", us_culture);
		TryParseExactHelper ("003:009:0010:0012.00153", G_format, false, "3.09:10:12.0015300", us_culture);
		TryParseExactHelper ("3:9:10:66.153", G_format, true, "dontcare"); // seconds out of range
		TryParseExactHelper ("3:9:10:12.153", G_format, true, "dontcare", french_culture); // fr-FR uses ',' as decimal separator
		TryParseExactHelper ("3:9:10:12,153", G_format, false, "3.09:10:12.1530000", french_culture);
		TryParseExactHelper ("  3:9:10:12.153  ", G_format, false, "3.09:10:12.1530000", us_culture);
		TryParseExactHelper ("3:9:10:13.153", G_format, false, "3.09:10:13.1530000", us_culture, TimeSpanStyles.AssumeNegative);

		// c format
		string [] c_format = new string [] { "c" };
		TryParseExactHelper ("12", c_format, false, "12.00:00:00");
		TryParseExactHelper ("12:11", c_format, false, "12:11:00");
		TryParseExactHelper ("12:66", c_format, true, "dontcare");
		TryParseExactHelper ("10.11:12", c_format, false, "10.11:12:00");
		TryParseExactHelper ("10.11:12:13", c_format, false, "10.11:12:13");
		TryParseExactHelper ("10:11:12:13", c_format, true, "dontcare"); // this is normally accepted in the Parse method
		TryParseExactHelper ("10.11:12:13.6", c_format, false, "10.11:12:13.6000000");
		TryParseExactHelper ("10:11:12,6", c_format, true, "dontcare");
		TryParseExactHelper ("10:11:12,6", c_format, true, "dontcare", french_culture);
		TryParseExactHelper ("  10:11:12.6  ", c_format, false, "10:11:12.6000000");
		TryParseExactHelper ("10:12", c_format, false, "10:12:00", null, TimeSpanStyles.AssumeNegative);
		TryParseExactHelper ("10:123456789999", c_format, true, "dontcare");

		TryParseExactHelper ("10:12", new string [0], true, "dontcare");
		TryParseExactHelper ("10:12", new string [] { String.Empty }, true, "dontcare");
		TryParseExactHelper ("10:12", new string [] { null }, true, "dontcare");

		TryParseExactHelper (null, new string [] { null }, true, "dontcare");
	}

	void TryParseExactHelper (string input, string [] formats, bool error, string expected, IFormatProvider formatProvider = null,
			TimeSpanStyles styles = TimeSpanStyles.None)
	{
		TimeSpan result;
		bool success;

		success = TimeSpan.TryParseExact (input, formats, formatProvider, styles, out result);
		Assert.AreEqual (!error, success);
		if (!error)
			Assert.AreEqual (expected, result.ToString ());
	}

	[Test]
	public void ParseExactExceptions ()
	{
		try {
			TimeSpan.ParseExact (null, "g", null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException) {
		}

		try {
			TimeSpan.ParseExact ("10:12", (string)null, null);
			Assert.Fail ("#A2");
		} catch (ArgumentNullException) {
		}

		try {
			TimeSpan.ParseExact ("10:12", (string [])null, null);
			Assert.Fail ("#A3");
		} catch (ArgumentNullException) {
		}
	}

	[Test]
	public void ToStringOverloads ()
	{
		TimeSpan ts = new TimeSpan (1, 2, 3, 4, 6);

		// Simple version - culture invariant
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString (), "#A1");
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString ("c"), "#A2");
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString (null), "#A3");
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString (String.Empty), "#A4");

		//
		// IFormatProvider ones - use a culture changing numeric format.
		// Also, we use fr-FR as culture, since it uses some elements different to invariant culture
		//
		CultureInfo culture = CultureInfo.GetCultureInfo ("fr-FR");

		Assert.AreEqual ("1:2:03:04,006", ts.ToString ("g", culture), "#B1");
		Assert.AreEqual ("1:02:03:04,0060000", ts.ToString ("G", culture), "#B2");
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString ("c", culture), "#B3"); // 'c' format ignores CultureInfo
		Assert.AreEqual ("1.02:03:04.0060000", ts.ToString ("t", culture), "#B4"); // 't' and 'T' are the same as 'c'
		Assert.AreEqual("1.02:03:04.0060000", ts.ToString("T", culture), "#B5");

		ts = new TimeSpan (4, 5, 6);
		Assert.AreEqual ("4:05:06", ts.ToString ("g", culture), "#C1");
		Assert.AreEqual ("0:04:05:06,0000000", ts.ToString ("G", culture), "#C2");
	}

	[Test]
	public void ToStringCustomFormats ()
	{
		TimeSpan ts = new TimeSpan (1, 3, 5, 7);

		Assert.AreEqual ("1", ts.ToString ("%d"), "#A0");
		Assert.AreEqual ("3", ts.ToString ("%h"), "#A1");
		Assert.AreEqual ("5", ts.ToString ("%m"), "#A2");
		Assert.AreEqual ("7", ts.ToString ("%s"), "#A3");
		Assert.AreEqual ("0", ts.ToString ("%f"), "#A4");
		Assert.AreEqual (String.Empty, ts.ToString ("%F"), "#A5"); // Nothing to display

		Assert.AreEqual ("01", ts.ToString ("dd"), "#B0");
		Assert.AreEqual ("00000001", ts.ToString ("dddddddd"), "#B1");
		Assert.AreEqual ("03", ts.ToString ("hh"), "#B2");
		Assert.AreEqual ("05", ts.ToString ("mm"), "#B3");
		Assert.AreEqual ("07", ts.ToString ("ss"), "#B4");
		Assert.AreEqual ("00", ts.ToString ("ff"), "#B5");
		Assert.AreEqual ("0000000", ts.ToString ("fffffff"), "#B6");
		Assert.AreEqual (String.Empty, ts.ToString ("FF"), "#B7");

		Assert.AreEqual ("01;03;05", ts.ToString (@"dd\;hh\;mm"), "#C0");
		Assert.AreEqual ("05 07", ts.ToString (@"mm\ ss"), "#C1");
		Assert.AreEqual ("05 07 ", ts.ToString (@"mm\ ss\ FF"), "#C2");
		Assert.AreEqual ("Result = 3 hours with 5 minutes and 7 seconds",
				ts.ToString (@"'Result = 'h' hours with 'm' minutes and 's' seconds'"), "#C3");
		Assert.AreEqual ("  ", ts.ToString (@"\ \ "), "#C4");

		ts = new TimeSpan (1, 3, 5, 7, 153);
		Assert.AreEqual ("1", ts.ToString ("%F"), "#D0");
		Assert.AreEqual ("15", ts.ToString ("FF"), "#D1"); // Don't use %, as the parser gets confused here
		Assert.AreEqual ("153", ts.ToString ("FFFFFFF"), "#D2");

		// Negative values are shown without sign
		ts = new TimeSpan (-1, -3, -5);
		Assert.AreEqual ("1", ts.ToString ("%h"), "#E0");
		Assert.AreEqual ("3", ts.ToString ("%m"), "#E1");
		Assert.AreEqual ("5", ts.ToString ("%s"), "#E2");

		ts = new TimeSpan (123456789);
		Assert.AreEqual ("12.3", ts.ToString ("s\\.f"), "#F0");
		Assert.AreEqual ("12.3", ts.ToString ("s\\.F"), "#F1");
		Assert.AreEqual ("12.3456789", ts.ToString ("s\\.fffffff"), "#F2");
		Assert.AreEqual ("12.345678", ts.ToString ("s\\.ffffff"), "#F3");

		ts = new TimeSpan (1234);
		Assert.AreEqual ("0.000123", ts.ToString ("s\\.ffffff"), "#G0");
		Assert.AreEqual ("0.0001", ts.ToString ("s\\.ffff"), "#G1");
		Assert.AreEqual ("0.", ts.ToString ("s\\.F"), "#G2");
		Assert.AreEqual ("0.", ts.ToString ("s\\.FFF"), "#G3");

		ts = TimeSpan.FromSeconds (0.05);
		Assert.AreEqual (".0", ts.ToString ("\\.f"), "#H0");
		Assert.AreEqual (".", ts.ToString ("\\.F"), "#H1");
	}

	[Test]
	public void ToStringOverloadsErrors ()
	{
		TimeSpan ts = new TimeSpan (10, 10, 10);
		string result;

		try {
			result = ts.ToString ("non-valid");
			Assert.Fail ("#1");
		} catch (FormatException) {
		}

		try {
			result = ts.ToString ("C");
			Assert.Fail ("#2");
		} catch (FormatException) {
		}

		try
		{
			ts.ToString ("m");
			Assert.Fail ("#3");
		} catch (FormatException) {
		}

		try
		{
			ts.ToString ("d"); // Missing % for single char
			Assert.Fail ("#4");
		} catch (FormatException)
		{
		}

		try
		{
			ts.ToString ("ddddddddd");
			Assert.Fail ("#5");
		} catch (FormatException)
		{
		}

		try
		{
			ts.ToString ("hhh");
			Assert.Fail ("#5");
		} catch (FormatException)
		{
		}

		try
		{
			ts.ToString ("ffffffff");
			Assert.Fail ("6");
		} catch (FormatException)
		{
		}
	}
}

}
