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
public class TimeSpanTest : Assertion {

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

		AssertEquals ("A1", "00:02:03.4567890", t1.ToString ());
		t1 = new TimeSpan (1,2,3);
		AssertEquals ("A2", "01:02:03", t1.ToString ());
		t1 = new TimeSpan (1,2,3,4);
		AssertEquals ("A3", "1.02:03:04", t1.ToString ());
		t1 = new TimeSpan (1,2,3,4,5);
		AssertEquals ("A4", "1.02:03:04.0050000", t1.ToString ());
		t1 = new TimeSpan (-1,2,-3,4,-5);
		AssertEquals ("A5", "-22:02:56.0050000", t1.ToString ());
		t1 = new TimeSpan (0,25,0,0,0);
		AssertEquals ("A6", "1.01:00:00", t1.ToString ());
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
		AssertEquals ("Days", 10650320, ts.Days);
		AssertEquals ("Hours", 0, ts.Hours);
		AssertEquals ("Minutes", 14, ts.Minutes);
		AssertEquals ("Seconds", 28, ts.Seconds);
		AssertEquals ("Milliseconds", 352, ts.Milliseconds);
		AssertEquals ("Ticks", 9201876488683520000, ts.Ticks);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
	public void NoOverflowInHoursMinsSecondsMS () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
		AssertEquals ("Days", 24879, ts.Days);
		AssertEquals ("Hours", 22, ts.Hours);
		AssertEquals ("Minutes", 44, ts.Minutes);
		AssertEquals ("Seconds", 30, ts.Seconds);
		AssertEquals ("Milliseconds", 647, ts.Milliseconds);
		AssertEquals ("Ticks", 21496274706470000, ts.Ticks);
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
			AssertEquals (prefix + "Days", -(h / 24), ts.Days);
			AssertEquals (prefix + "Hours", -(h % 24), ts.Hours);
			AssertEquals (prefix + "Minutes", 0, ts.Minutes);
			AssertEquals (prefix + "Seconds", 0, ts.Seconds);
			AssertEquals (prefix + "Milliseconds", 0, ts.Milliseconds);
			AssertEquals (prefix + "Ticks", -36000000000 * h, ts.Ticks);
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
		AssertEquals ("Max-Days", 0, ts.Days);
		AssertEquals ("Max-Hours", -1, ts.Hours);
		AssertEquals ("Max-Minutes", 0, ts.Minutes);
		AssertEquals ("Max-Seconds", 0, ts.Seconds);
		AssertEquals ("Max-Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Max-Ticks", -36000000000, ts.Ticks);

		ts = new TimeSpan (0, Int32.MaxValue - 596522, 0, 0, 0);
		AssertEquals ("Days", -24855, ts.Days);
		AssertEquals ("Hours", -3, ts.Hours);
		AssertEquals ("Minutes", 0, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -21474828000000000, ts.Ticks);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	[Category ("NotWorking")]
#endif
	public void MaxHours_BreakPoint () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue - 596523, 0, 0, 0);
		AssertEquals ("Days", 24855, ts.Days);
		AssertEquals ("Hours", 2, ts.Hours);
		AssertEquals ("Minutes", 28, ts.Minutes);
		AssertEquals ("Seconds", 16, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", 21474808960000000, ts.Ticks);
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
			AssertEquals (prefix + "Days", (h / 24), ts.Days);
			AssertEquals (prefix + "Hours", (h % 24), ts.Hours);
			AssertEquals (prefix + "Minutes", 0, ts.Minutes);
			AssertEquals (prefix + "Seconds", 0, ts.Seconds);
			AssertEquals (prefix + "Milliseconds", 0, ts.Milliseconds);
			AssertEquals (prefix + "Ticks", 36000000000 * h, ts.Ticks);
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
		AssertEquals ("Days", -10675199, ts.Days);
		AssertEquals ("Hours", -2, ts.Hours);
		AssertEquals ("Minutes", 0, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -9223372008000000000, ts.Ticks);
#else
		// LAMESPEC: the lowest hours are "special"
		TimeSpan ts = new TimeSpan (0, Int32.MinValue, 0, 0, 0);
		AssertEquals ("Min-Days", 0, ts.Days);
		AssertEquals ("Min-Hours", 0, ts.Hours);
		AssertEquals ("Min-Minutes", 0, ts.Minutes);
		AssertEquals ("Min-Seconds", 0, ts.Seconds);
		AssertEquals ("Min-Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Min-Ticks", 0, ts.Ticks);

		ts = new TimeSpan (0, -2146887125, 0, 0, 0);
		AssertEquals ("Days", 24855, ts.Days);
		AssertEquals ("Hours", 3, ts.Hours);
		AssertEquals ("Minutes", 0, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", 21474828000000000, ts.Ticks);
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
		AssertEquals ("Days", -24855, ts.Days);
		AssertEquals ("Hours", -2, ts.Hours);
		AssertEquals ("Minutes", -28, ts.Minutes);
		AssertEquals ("Seconds", -16, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -21474808960000000, ts.Ticks);
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
			AssertEquals (prefix + "Days", (h / 1440), ts.Days);
			AssertEquals (prefix + "Hours", ((h / 60) % 24), ts.Hours);
			AssertEquals (prefix + "Minutes", (h % 60), ts.Minutes);
			AssertEquals (prefix + "Seconds", 0, ts.Seconds);
			AssertEquals (prefix + "Milliseconds", 0, ts.Milliseconds);
			AssertEquals (prefix + "Ticks", (600000000L * h), ts.Ticks);
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
		AssertEquals ("Max-Days", 177919, ts.Days);
		AssertEquals ("Max-Hours", 23, ts.Hours);
		AssertEquals ("Max-Minutes", 38, ts.Minutes);
		AssertEquals ("Max-Seconds", 0, ts.Seconds);
		AssertEquals ("Max-Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Max-Ticks", 153722866800000000, ts.Ticks);
#else
		// LAMESPEC: the highest minutes are "special"
		ts = new TimeSpan (0, 0, Int32.MaxValue, 0, 0);
		AssertEquals ("Max-Days", 0, ts.Days);
		AssertEquals ("Max-Hours", 0, ts.Hours);
		AssertEquals ("Max-Minutes", -1, ts.Minutes);
		AssertEquals ("Max-Seconds", 0, ts.Seconds);
		AssertEquals ("Max-Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Max-Ticks", -600000000, ts.Ticks);

		ts = new TimeSpan (0, 0, Int32.MaxValue - 35791393, 0, 0);
		AssertEquals ("Days", -24855, ts.Days);
		AssertEquals ("Hours", -3, ts.Hours);
		AssertEquals ("Minutes", -14, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -21474836400000000, ts.Ticks);
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
		AssertEquals ("Days", 0, ts.Days);
		AssertEquals ("Hours", 0, ts.Hours);
		AssertEquals ("Minutes", -52, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -31200000000, ts.Ticks);
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
			AssertEquals (prefix + "Days", (h / 1440), ts.Days);
			AssertEquals (prefix + "Hours", ((h / 60) % 24), ts.Hours);
			AssertEquals (prefix + "Minutes", (h % 60), ts.Minutes);
			AssertEquals (prefix + "Seconds", 0, ts.Seconds);
			AssertEquals (prefix + "Milliseconds", 0, ts.Milliseconds);
			AssertEquals (prefix + "Ticks", (600000000L * h), ts.Ticks);
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
		AssertEquals ("Days", -1491308, ts.Days);
		AssertEquals ("Hours", -2, ts.Hours);
		AssertEquals ("Minutes", -8, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -1288490188800000000, ts.Ticks);
#else
		// LAMESPEC: the highest minutes are "special"
		ts = new TimeSpan (0, 0, Int32.MinValue, 0, 0);
		AssertEquals ("Min-Days", 0, ts.Days);
		AssertEquals ("Min-Hours", 0, ts.Hours);
		AssertEquals ("Min-Minutes", 0, ts.Minutes);
		AssertEquals ("Min-Seconds", 0, ts.Seconds);
		AssertEquals ("Min-Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Min-Ticks", 0, ts.Ticks);

		ts = new TimeSpan (0, 0, -2111692254, 0, 0);
		AssertEquals ("Days", 24855, ts.Days);
		AssertEquals ("Hours", 3, ts.Hours);
		AssertEquals ("Minutes", 14, ts.Minutes);
		AssertEquals ("Seconds", 0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", 21474836400000000, ts.Ticks);
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
		AssertEquals ("Days", -1466452, ts.Days);
		AssertEquals ("Hours", -22, ts.Hours);
		AssertEquals ("Minutes", -53, ts.Minutes);
		AssertEquals ("Seconds", -0, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -1267015351800000000, ts.Ticks);
#else
		TimeSpan ts = new TimeSpan (0, 0, -2111692253, 0, 0);
		AssertEquals ("Days", -24855, ts.Days);
		AssertEquals ("Hours", -3, ts.Hours);
		AssertEquals ("Minutes", -13, ts.Minutes);
		AssertEquals ("Seconds", -16, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -21474835960000000, ts.Ticks);
#endif
	}

	[Test]
	public void MaxSeconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, Int32.MaxValue, 0);
		AssertEquals ("Days", 24855, ts.Days);
		AssertEquals ("Hours", 3, ts.Hours);
		AssertEquals ("Minutes", 14, ts.Minutes);
		AssertEquals ("Seconds", 7, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", 21474836470000000, ts.Ticks);
	}

	[Test]
	public void MinSeconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, Int32.MinValue, 0);
		AssertEquals ("Days", -24855, ts.Days);
		AssertEquals ("Hours", -3, ts.Hours);
		AssertEquals ("Minutes", -14, ts.Minutes);
		AssertEquals ("Seconds", -8, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -21474836480000000, ts.Ticks);
	}

	[Test]
	public void MaxMilliseconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, 0, Int32.MaxValue);
		AssertEquals ("Days", 24, ts.Days);
		AssertEquals ("Hours", 20, ts.Hours);
		AssertEquals ("Minutes", 31, ts.Minutes);
		AssertEquals ("Seconds", 23, ts.Seconds);
		AssertEquals ("Milliseconds", 647, ts.Milliseconds);
		AssertEquals ("Ticks", 21474836470000, ts.Ticks);
	}

	[Test]
	public void MinMilliseconds () 
	{
		TimeSpan ts = new TimeSpan (0, 0, 0, 0, Int32.MinValue);
		AssertEquals ("Days", -24, ts.Days);
		AssertEquals ("Hours", -20, ts.Hours);
		AssertEquals ("Minutes", -31, ts.Minutes);
		AssertEquals ("Seconds", -23, ts.Seconds);
		AssertEquals ("Milliseconds", -648, ts.Milliseconds);
		AssertEquals ("Ticks", -21474836480000, ts.Ticks);
	}

	[Test]
	public void NegativeTimeSpan () 
	{
		TimeSpan ts = new TimeSpan (-23, -59, -59);
		AssertEquals ("Days", 0, ts.Days);
		AssertEquals ("Hours", -23, ts.Hours);
		AssertEquals ("Minutes", -59, ts.Minutes);
		AssertEquals ("Seconds", -59, ts.Seconds);
		AssertEquals ("Milliseconds", 0, ts.Milliseconds);
		AssertEquals ("Ticks", -863990000000, ts.Ticks);
	}

	public void TestProperties ()
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;

		AssertEquals ("A1", 1, t1.Days);
		AssertEquals ("A2", 2, t1.Hours);
		AssertEquals ("A3", 3, t1.Minutes);
		AssertEquals ("A4", 4, t1.Seconds);
		AssertEquals ("A5", 5, t1.Milliseconds);
		AssertEquals ("A6", -1, t2.Days);
		AssertEquals ("A7", -2, t2.Hours);
		AssertEquals ("A8", -3, t2.Minutes);
		AssertEquals ("A9", -4, t2.Seconds);
		AssertEquals ("A10", -5, t2.Milliseconds);
	}

	public void TestAdd ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 + t2;
		TimeSpan t4 = t1.Add (t2);
		TimeSpan t5;
		bool exception;

		AssertEquals ("A1", 3, t3.Days);
		AssertEquals ("A2", 5, t3.Hours);
		AssertEquals ("A3", 7, t3.Minutes);
		AssertEquals ("A4", 9, t3.Seconds);
		AssertEquals ("A5", 11, t3.Milliseconds);
		AssertEquals ("A6", "3.05:07:09.0110000", t4.ToString ());
		try
		{
			t5 = TimeSpan.MaxValue + new TimeSpan (1);			
			exception = false;
		}
		catch (OverflowException)
		{
			exception = true;
		}
		Assert ("A7", exception);
	}

	public void TestCompare ()
	{
		TimeSpan t1 = new TimeSpan (-1);
		TimeSpan t2 = new TimeSpan (1);
		int res;
		bool exception;

		AssertEquals ("A1", -1, TimeSpan.Compare (t1, t2));
		AssertEquals ("A2", 1, TimeSpan.Compare (t2, t1));
		AssertEquals ("A3", 0, TimeSpan.Compare (t2, t2));
		AssertEquals ("A4", -1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue));
		AssertEquals ("A5", -1, t1.CompareTo (t2));
		AssertEquals ("A6", 1, t2.CompareTo (t1));
		AssertEquals ("A7", 0, t2.CompareTo (t2));
		AssertEquals ("A8", -1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue));

		AssertEquals ("A9", 1, TimeSpan.Zero.CompareTo (null));
		
		try
		{
			res = TimeSpan.Zero.CompareTo("");
			exception = false;	
		}
		catch (ArgumentException)
		{
			exception = true;
		}
		Assert ("A10", exception);

		AssertEquals ("A11", false, t1 == t2);
		AssertEquals ("A12", false, t1 > t2);
		AssertEquals ("A13", false, t1 >= t2);
		AssertEquals ("A14", true, t1 != t2);
		AssertEquals ("A15", true, t1 < t2);
		AssertEquals ("A16", true, t1 <= t2);
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

		AssertEquals ("A1", "-00:00:00.0012345", new TimeSpan (12345).Negate ().ToString ());
		AssertEquals ("A2", "00:00:00.0012345", new TimeSpan (-12345).Duration ().ToString ());
			
		try
		{
			t1 = TimeSpan.MinValue.Duration ();
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert ("A4", exception);

		AssertEquals ("A5", "-00:00:00.0000077", (-(new TimeSpan (77))).ToString ());
		AssertEquals("A6", "00:00:00.0000077", (+(new TimeSpan(77))).ToString());
	}

	public void TestEquals ()
	{
		TimeSpan t1 = new TimeSpan (1);
		TimeSpan t2 = new TimeSpan (2);
		string s = "justastring";

		AssertEquals ("A1", true, t1.Equals (t1));
		AssertEquals ("A2", false, t1.Equals (t2));
		AssertEquals ("A3", false, t1.Equals (s));
		AssertEquals ("A4", false, t1.Equals (null));
		AssertEquals ("A5", true, TimeSpan.Equals (t1, t1));
		AssertEquals ("A6", false, TimeSpan.Equals (t1, t2));
		AssertEquals ("A7", false, TimeSpan.Equals (t1, null));
		AssertEquals ("A8", false, TimeSpan.Equals (t1, s));
		AssertEquals ("A9", false, TimeSpan.Equals (s, t2));
		AssertEquals ("A10", true, TimeSpan.Equals (null,null));
	}

	public void TestFromXXXX ()
	{
		AssertEquals ("A1", "12.08:16:48", TimeSpan.FromDays (12.345).ToString ());
		AssertEquals ("A2", "12:20:42", TimeSpan.FromHours (12.345).ToString ());
		AssertEquals ("A3", "00:12:20.7000000", TimeSpan.FromMinutes (12.345).ToString ());
		AssertEquals ("A4", "00:00:12.3450000", TimeSpan.FromSeconds (12.345).ToString ());
		AssertEquals ("A5", "00:00:00.0120000", TimeSpan.FromMilliseconds (12.345).ToString ());
		AssertEquals ("A6", "00:00:00.0012345", TimeSpan.FromTicks (12345).ToString ());
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
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromDays (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromDays (Double.NegativeInfinity));
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
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromHours (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromHours (Double.NegativeInfinity));
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
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromMilliseconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromMilliseconds (Double.NegativeInfinity));
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
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromMinutes (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromMinutes (Double.NegativeInfinity));
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
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromSeconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromSeconds (Double.NegativeInfinity));
	}

	public void TestGetHashCode ()
	{
		AssertEquals ("A1", 77, new TimeSpan (77).GetHashCode ());
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
		AssertEquals ("A1", expectFormat, formatException);
		AssertEquals ("A2", expectOverflow, overflowException);

		if (!expectOverflow && !expectFormat) {
			AssertEquals ("A3", expect, result);
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
		AssertEquals ("Days", 1, ts.Days);
	}

	public void TestSubstract ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 - t2;
		TimeSpan t4 = t1.Subtract (t2);
		TimeSpan t5;
		bool exception;

		AssertEquals ("A1", "1.01:01:01.0010000", t3.ToString ());
		AssertEquals ("A2", "1.01:01:01.0010000", t4.ToString ());
		try {
			t5 = TimeSpan.MinValue - new TimeSpan (1);
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert ("A3", exception);
	}

	public void TestToString () 
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;
		
		AssertEquals ("A1", "1.02:03:04.0050000", t1.ToString ());
		AssertEquals ("A2", "-1.02:03:04.0050000", t2.ToString ());
		AssertEquals ("A3", "10675199.02:48:05.4775807", TimeSpan.MaxValue.ToString ());
		AssertEquals ("A4", "-10675199.02:48:05.4775808", TimeSpan.MinValue.ToString ());
	}

	[Test]
	public void ToString_Constants () 
	{
		AssertEquals ("Zero", "00:00:00", TimeSpan.Zero.ToString ());
		AssertEquals ("MaxValue", "10675199.02:48:05.4775807", TimeSpan.MaxValue.ToString ());
		AssertEquals ("MinValue", "-10675199.02:48:05.4775808", TimeSpan.MinValue.ToString ());
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
		AssertEquals ("MaxValue", TimeSpan.MaxValue, TimeSpan.Parse ("10675199.02:48:05.4775807"));
		AssertEquals ("MinValue", TimeSpan.MinValue, TimeSpan.Parse ("-10675199.02:48:05.4775808"));
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
}

}
