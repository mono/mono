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

namespace MonoTests.System
{

public class DateTimeTest : TestCase
{
	static long[] myTicks = {
		631501920000000000L,	// 25 Feb 2002 - 00:00:00
		631502475130080000L,	// 25 Feb 2002 - 15:25:13,8
		631502115130080000L	// 25 Feb 2002 - 05:25:13,8
	};

        public DateTimeTest (string name): base(name) {}

	public static ITest Suite
	{
		get {
			TestSuite suite = new TestSuite ();
			return suite;
		}
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
		AssertEquals("B14", "2002-02-25 01:25:13Z", t1.ToString ("u"));
		AssertEquals("B15", "Monday, 25 February 2002 01:25:13", t1.ToString ("U"));
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
		AssertEquals("C26", "+4", t1.ToString ("%z"));
		AssertEquals("C27", "+04", t1.ToString ("zz"));
		AssertEquals("C28", "+04:00", t1.ToString ("zzz"));
		AssertEquals("C29", " : ", t1.ToString (" : "));
		AssertEquals("C30", " / ", t1.ToString (" / "));
		AssertEquals("C31", " yyy ", t1.ToString (" 'yyy' "));
		AssertEquals("C32", " d", t1.ToString (" \\d"));
	}

	protected override void RunTest ()
	{
		TestCtors ();
		TestToString ();
	}

}

}
