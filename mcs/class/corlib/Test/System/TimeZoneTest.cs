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

namespace MonoTests.System {

public class TimeZoneTest : TestCase {

	private CultureInfo oldcult;

	public TimeZoneTest() {}

	protected override void SetUp ()
	{
		oldcult = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("");
	}
	
	protected override void TearDown ()
	{
		Thread.CurrentThread.CurrentCulture = oldcult;
	}

	private void CET (TimeZone t1) 
	{
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

	private void EST (TimeZone t1) 
	{
		AssertEquals("B01", "Eastern Standard Time", t1.StandardName);
		AssertEquals("B02", "Eastern Daylight Time", t1.DaylightName);

		DaylightTime d1 = t1.GetDaylightChanges (2002);
		AssertEquals("B03", "04/07/2002 02:00:00", d1.Start.ToString ("G"));
		AssertEquals("B04", "10/27/2002 02:00:00", d1.End.ToString ("G"));
		AssertEquals("B05", 36000000000L, d1.Delta.Ticks);

		DaylightTime d2 = t1.GetDaylightChanges (1996);
		AssertEquals("B06", "04/07/1996 02:00:00", d2.Start.ToString ("G"));
		AssertEquals("B07", "10/27/1996 02:00:00", d2.End.ToString ("G"));
		AssertEquals("B08", 36000000000L, d2.Delta.Ticks);

		DateTime d3 = new DateTime (2002,2,25);
		AssertEquals("B09", false, t1.IsDaylightSavingTime (d3));
		DateTime d4 = new DateTime (2002,4,8);
		AssertEquals("B10", true, t1.IsDaylightSavingTime (d4));
		DateTime d5 = new DateTime (2002,11,4);
		AssertEquals("B11", false, t1.IsDaylightSavingTime (d5));

		AssertEquals("B12", -180000000000L, t1.GetUtcOffset (d3).Ticks);
		AssertEquals("B13", -144000000000L, t1.GetUtcOffset (d4).Ticks);
		AssertEquals("B14", -180000000000L, t1.GetUtcOffset (d5).Ticks);
	}

	private void TST (TimeZone t1) 
	{
		AssertEquals("C01", "Tokyo Standard Time", t1.StandardName);
		AssertEquals("C02", "Tokyo Standard Time", t1.DaylightName);

		DateTime d3 = new DateTime (2002,2,25);
		AssertEquals("C09", false, t1.IsDaylightSavingTime (d3));
		DateTime d4 = new DateTime (2002,4,8);
		AssertEquals("C10", false, t1.IsDaylightSavingTime (d4));
		DateTime d5 = new DateTime (2002,11,4);
		AssertEquals("C11", false, t1.IsDaylightSavingTime (d5));

		AssertEquals("C12", 324000000000L, t1.GetUtcOffset (d3).Ticks);
		AssertEquals("C13", 324000000000L, t1.GetUtcOffset (d4).Ticks);
		AssertEquals("C14", 324000000000L, t1.GetUtcOffset (d5).Ticks);
	}

	private void GMT (TimeZone t1) {
		// Probably wont work on MS.NET, but is better than nothing. Where do
		// we change our implementation to match theirs?
		
		AssertEquals("D01", "GMT", t1.StandardName);
		AssertEquals("D02", "BST", t1.DaylightName);
	
		DaylightTime d1 = t1.GetDaylightChanges (2002);
		AssertEquals("D03", "03/31/2002 01:00:00", d1.Start.ToString ("G"));
		AssertEquals("D04", "10/27/2002 01:00:00", d1.End.ToString ("G"));
		AssertEquals("D05", 36000000000L, d1.Delta.Ticks);
	
		DaylightTime d2 = t1.GetDaylightChanges (1996);
		AssertEquals("D06", "03/31/1996 01:00:00", d2.Start.ToString ("G"));
		AssertEquals("D07", "10/27/1996 01:00:00", d2.End.ToString ("G"));
		AssertEquals("D08", 36000000000L, d2.Delta.Ticks);
	
		DateTime d3 = new DateTime (2002,2,25);
		AssertEquals("D09", false, t1.IsDaylightSavingTime (d3));
		DateTime d4 = new DateTime (2002,4,2);
		AssertEquals("D10", true, t1.IsDaylightSavingTime (d4));
		DateTime d5 = new DateTime (2002,11,4);
		AssertEquals("D11", false, t1.IsDaylightSavingTime (d5));
	
		AssertEquals("D12", 0L, t1.GetUtcOffset (d3).Ticks);
		AssertEquals("D13", 36000000000L, t1.GetUtcOffset (d4).Ticks);
		AssertEquals("D14", 0L, t1.GetUtcOffset (d5).Ticks);
	}



	public void TestCtors ()
	{
		TimeZone t1 = TimeZone.CurrentTimeZone;
		switch (t1.StandardName) {
			case "CET":
				CET (t1);
				break;
			case "Eastern Standard Time":
			case "EST":
				EST (t1);
				break;
			case "Tokyo Standard Time":
				TST (t1);
				break;
			case "GMT":
				GMT (t1);
				break;
			default:
				NUnit.Framework.Assert.Ignore ("Your time zone (" + t1.StandardName + ") isn't defined in the test case");
				break;
		}
        }
}

}
