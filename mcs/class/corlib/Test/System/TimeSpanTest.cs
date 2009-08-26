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
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
	[Ignore ("too long")]
	public void MaxHours_TooLong () 
	{
		// LAMESPEC: the highest hours are "special"
		for (int i=0; i < 596523; i++) {
			TimeSpan ts = new TimeSpan (0, Int32.MaxValue - i, 0, 0, 0);
			int h = i + 1;
			string prefix = i.ToString () + '-';
			Assert.AreEqual (-(h / 24), ts.Days, prefix + "Days");
			Assert.AreEqual (-(h % 24), ts.Hours, prefix + "Hours");
			Assert.AreEqual (0, ts.Minutes, prefix + "Minutes");
			Assert.AreEqual (0, ts.Seconds, prefix + "Seconds");
			Assert.AreEqual (0, ts.Milliseconds, prefix + "Milliseconds");
			Assert.AreEqual (-36000000000 * h, ts.Ticks, prefix + "Ticks");
		}
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
	[Ignore ("too long")]
	public void MinHours_TooLong () 
	{
		// LAMESPEC: the lowest hours are "special"
		for (int i=Int32.MinValue; i < -2146887124; i++) {
			TimeSpan ts = new TimeSpan (0, i, 0, 0, 0);
			int h = i + Int32.MaxValue + 1;
			string prefix = i.ToString () + '-';
			Assert.AreEqual ((h / 24), ts.Days, prefix + "Days");
			Assert.AreEqual ((h % 24), ts.Hours, prefix + "Hours");
			Assert.AreEqual (0, ts.Minutes, prefix + "Minutes");
			Assert.AreEqual (0, ts.Seconds, prefix + "Seconds");
			Assert.AreEqual (0, ts.Milliseconds, prefix + "Milliseconds");
			Assert.AreEqual (36000000000 * h, ts.Ticks, prefix + "Ticks");
		}
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void MinHours () 
	{
#if NET_2_0
		TimeSpan ts = new TimeSpan (0, -256204778, 0, 0, 0);
		Assert.AreEqual (-10675199, ts.Days, "Days");
		Assert.AreEqual (-2, ts.Hours, "Hours");
		Assert.AreEqual (0, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-9223372008000000000, ts.Ticks, "Ticks");
#else
		// LAMESPEC: the lowest hours are "special"
		TimeSpan ts = new TimeSpan (0, Int32.MinValue, 0, 0, 0);
		Assert.AreEqual (0, ts.Days, "Min-Days");
		Assert.AreEqual (0, ts.Hours, "Min-Hours");
		Assert.AreEqual (0, ts.Minutes, "Min-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Min-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Min-Milliseconds");
		Assert.AreEqual (0, ts.Ticks, "Min-Ticks");

		ts = new TimeSpan (0, -2146887125, 0, 0, 0);
		Assert.AreEqual (24855, ts.Days, "Days");
		Assert.AreEqual (3, ts.Hours, "Hours");
		Assert.AreEqual (0, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21474828000000000, ts.Ticks, "Ticks");
#endif
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
	[Ignore ("too long")]
	public void MaxMinutes_TooLong () 
	{
		// LAMESPEC: the highest minutes are "special"
		for (int i=0; i < 35791394; i++) {
			TimeSpan ts = new TimeSpan (0, 0, Int32.MaxValue - i, 0, 0);
			long h = -(i + 1);
			string prefix = i.ToString () + '-';
			Assert.AreEqual ((h / 1440), ts.Days, prefix + "Days");
			Assert.AreEqual (((h / 60) % 24), ts.Hours, prefix + "Hours");
			Assert.AreEqual ((h % 60), ts.Minutes, prefix + "Minutes");
			Assert.AreEqual (0, ts.Seconds, prefix + "Seconds");
			Assert.AreEqual (0, ts.Milliseconds, prefix + "Milliseconds");
			Assert.AreEqual ((600000000L * h), ts.Ticks, prefix + "Ticks");
		}
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void MaxMinutes () 
	{
		TimeSpan ts;
#if NET_2_0
		ts = new TimeSpan (0, 0, 256204778, 0, 0);
		Assert.AreEqual (177919, ts.Days, "Max-Days");
		Assert.AreEqual (23, ts.Hours, "Max-Hours");
		Assert.AreEqual (38, ts.Minutes, "Max-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Max-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Max-Milliseconds");
		Assert.AreEqual (153722866800000000, ts.Ticks, "Max-Ticks");
#else
		// LAMESPEC: the highest minutes are "special"
		ts = new TimeSpan (0, 0, Int32.MaxValue, 0, 0);
		Assert.AreEqual (0, ts.Days, "Max-Days");
		Assert.AreEqual (0, ts.Hours, "Max-Hours");
		Assert.AreEqual (-1, ts.Minutes, "Max-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Max-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Max-Milliseconds");
		Assert.AreEqual (-600000000, ts.Ticks, "Max-Ticks");

		ts = new TimeSpan (0, 0, Int32.MaxValue - 35791393, 0, 0);
		Assert.AreEqual (-24855, ts.Days, "Days");
		Assert.AreEqual (-3, ts.Hours, "Hours");
		Assert.AreEqual (-14, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474836400000000, ts.Ticks, "Ticks");
#endif
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
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
	[Ignore ("too long")]
	public void MinMinutes_TooLong () 
	{
		// LAMESPEC: the highest minutes are "special"
		for (int i=Int32.MinValue; i < -2111692253; i++) {
			TimeSpan ts = new TimeSpan (0, 0, i, 0, 0);
			long h = i + Int32.MaxValue + 1;
			string prefix = i.ToString () + '-';
			Assert.AreEqual ((h / 1440), ts.Days, prefix + "Days");
			Assert.AreEqual (((h / 60) % 24), ts.Hours, prefix + "Hours");
			Assert.AreEqual ((h % 60), ts.Minutes, prefix + "Minutes");
			Assert.AreEqual (0, ts.Seconds, prefix + "Seconds");
			Assert.AreEqual (0, ts.Milliseconds, prefix + "Milliseconds");
			Assert.AreEqual ((600000000L * h), ts.Ticks, prefix + "Ticks");
		}
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void MinMinutes () 
	{
		TimeSpan ts;
#if NET_2_0
		ts = new TimeSpan (0, 0, Int32.MinValue, 0, 0);
		Assert.AreEqual (-1491308, ts.Days, "Days");
		Assert.AreEqual (-2, ts.Hours, "Hours");
		Assert.AreEqual (-8, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-1288490188800000000, ts.Ticks, "Ticks");
#else
		// LAMESPEC: the highest minutes are "special"
		ts = new TimeSpan (0, 0, Int32.MinValue, 0, 0);
		Assert.AreEqual (0, ts.Days, "Min-Days");
		Assert.AreEqual (0, ts.Hours, "Min-Hours");
		Assert.AreEqual (0, ts.Minutes, "Min-Minutes");
		Assert.AreEqual (0, ts.Seconds, "Min-Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Min-Milliseconds");
		Assert.AreEqual (0, ts.Ticks, "Min-Ticks");

		ts = new TimeSpan (0, 0, -2111692254, 0, 0);
		Assert.AreEqual (24855, ts.Days, "Days");
		Assert.AreEqual (3, ts.Hours, "Hours");
		Assert.AreEqual (14, ts.Minutes, "Minutes");
		Assert.AreEqual (0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (21474836400000000, ts.Ticks, "Ticks");
#endif
	}

	[Test]
#if NET_2_0
	[Category ("NotWorking")]
#endif
	public void MinMinutes_BreakPoint () 
	{
#if NET_2_0
		TimeSpan ts = new TimeSpan (0, 0, -2111692253, 0, 0);
		Assert.AreEqual (-1466452, ts.Days, "Days");
		Assert.AreEqual (-22, ts.Hours, "Hours");
		Assert.AreEqual (-53, ts.Minutes, "Minutes");
		Assert.AreEqual (-0, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-1267015351800000000, ts.Ticks, "Ticks");
#else
		TimeSpan ts = new TimeSpan (0, 0, -2111692253, 0, 0);
		Assert.AreEqual (-24855, ts.Days, "Days");
		Assert.AreEqual (-3, ts.Hours, "Hours");
		Assert.AreEqual (-13, ts.Minutes, "Minutes");
		Assert.AreEqual (-16, ts.Seconds, "Seconds");
		Assert.AreEqual (0, ts.Milliseconds, "Milliseconds");
		Assert.AreEqual (-21474835960000000, ts.Ticks, "Ticks");
#endif
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

	public void TestFromXXXX ()
	{
		Assert.AreEqual ("12.08:16:48", TimeSpan.FromDays (12.345).ToString (), "A1");
		Assert.AreEqual ("12:20:42", TimeSpan.FromHours (12.345).ToString (), "A2");
		Assert.AreEqual ("00:12:20.7000000", TimeSpan.FromMinutes (12.345).ToString (), "A3");
		Assert.AreEqual ("00:00:12.3450000", TimeSpan.FromSeconds (12.345).ToString (), "A4");
		Assert.AreEqual ("00:00:00.0120000", TimeSpan.FromMilliseconds (12.345).ToString (), "A5");
		Assert.AreEqual ("00:00:00.0012345", TimeSpan.FromTicks (12345).ToString (), "A6");
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
		Assert.AreEqual (expectFormat, formatException, "A1");
		Assert.AreEqual (expectOverflow, overflowException, "A2");

		if (!expectOverflow && !expectFormat) {
			Assert.AreEqual (expect, result, "A3");
		}
	}

	public void TestParse ()
	{
		ParseHelper (" 13:45:15 ",false, false, "13:45:15");
		ParseHelper (" -1:2:3 ", false, false, "-01:02:03");

		ParseHelper (" 25:0:0 ",false, true, "dontcare");
		ParseHelper ("aaa", true, false, "dontcare");

		ParseHelper ("-21.23:59:59.9999999", false, false, "-21.23:59:59.9999999");

		ParseHelper ("100000000000000.1:1:1", false, true, "dontcare");
		ParseHelper ("24:60:60", false, true, "dontcare");
		ParseHelper ("0001:0002:0003.12     ", false, false, "01:02:03.1200000");

		ParseHelper (" 1:2:3:12345678 ", true, false, "dontcare"); 
	}

	// LAMESPEC: timespan in documentation is wrong - hh:mm:ss isn't mandatory
	[Test]
	public void Parse_Days_WithoutColon () 
	{
		TimeSpan ts = TimeSpan.Parse ("1");
		Assert.AreEqual (1, ts.Days, "Days");
	}

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
	[ExpectedException (typeof (OverflowException))]
	public void Parse_InvalidValuesAndFormat_ExceptionOrder () 
	{
		// hours should be between 0 and 23 but format is also invalid (too many dots)
		TimeSpan.Parse ("0.99.99.0");
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
}

}
