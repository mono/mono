//
// TimeZoneTest.cs - NUnit Test Cases for the System.TimeZone struct
//
// author:
//   Martin Baulig (martin@gnome.org)
//
//   (C) 2002 Martin Baulig
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{

public class TimeZoneTest : TestCase
{
	public TimeZoneTest() : base ("MonoTests.System.TimeZoneTest testcase") {}
        public TimeZoneTest (string name): base(name) {}

	public static ITest Suite
	{
		get {
			return new TestSuite (typeof(TimeZoneTest));
		}
	}

	public void TestCtors ()
	{
		TimeZone t1 = TimeZone.CurrentTimeZone;
		AssertEquals("A01", "CET", t1.StandardName);
		AssertEquals("A02", "CEST", t1.DaylightName);

		DaylightTime d1 = t1.GetDaylightChanges (2002);
		AssertEquals("A03", "03/31/2002 01:00:00", d1.Start.ToString ("G"));
		AssertEquals("A04", "10/27/2002 01:00:00", d1.End.ToString ("G"));
		AssertEquals("A05", 36000000000L, d1.Delta.Ticks);

		DaylightTime d2 = t1.GetDaylightChanges (1996);
		AssertEquals("A06", "03/31/1996 01:00:00", d2.Start.ToString ("G"));
		AssertEquals("A07", "10/27/1996 01:00:00", d2.End.ToString ("G"));
		AssertEquals("A08", 36000000000L, d2.Delta.Ticks);

		DateTime d3 = new DateTime (2002,2,25);
		AssertEquals("A09", false, t1.IsDaylightSavingTime (d3));
		DateTime d4 = new DateTime (2002,4,2);
		AssertEquals("A10", true, t1.IsDaylightSavingTime (d4));
		DateTime d5 = new DateTime (2002,11,4);
		AssertEquals("A11", false, t1.IsDaylightSavingTime (d5));

		AssertEquals("A12", 36000000000L, t1.GetUtcOffset (d3).Ticks);
		AssertEquals("A13", 72000000000L, t1.GetUtcOffset (d4).Ticks);
		AssertEquals("A14", 36000000000L, t1.GetUtcOffset (d5).Ticks);
        }

	protected override void RunTest ()
	{
		CultureInfo oldcult = Thread.CurrentThread.CurrentCulture;

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("");

		TestCtors ();

		Thread.CurrentThread.CurrentCulture = oldcult;
	}

}

}
