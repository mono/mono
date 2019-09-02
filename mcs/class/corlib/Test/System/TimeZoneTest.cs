//
// TimeZoneTest.cs - NUnit Test Cases for the System.TimeZone struct
//
// Authors:
//	Martin Baulig (martin@gnome.org)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//   (C) 2002 Martin Baulig
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System {

[TestFixture]
public class TimeZoneTest {

	private void CET (TimeZone t1) 
	{
		Assert.IsTrue("CET" == t1.StandardName || "W. Europe Standard Time" == t1.StandardName, "A01");
		Assert.IsTrue("CEST" == t1.DaylightName || "W. Europe Daylight Time" == t1.DaylightName, "A02");
	
		DaylightTime d1 = t1.GetDaylightChanges (2002);
		Assert.AreEqual("03/31/2002 02:00:00", d1.Start.ToString ("G", CultureInfo.InvariantCulture), "A03");
		Assert.AreEqual("10/27/2002 03:00:00", d1.End.ToString ("G", CultureInfo.InvariantCulture), "A04");
		Assert.AreEqual(36000000000L, d1.Delta.Ticks, "A05");
	
		DaylightTime d2 = t1.GetDaylightChanges (1996);
		Assert.AreEqual("03/31/1996 02:00:00", d2.Start.ToString ("G", CultureInfo.InvariantCulture), "A06");
		Assert.AreEqual("10/27/1996 03:00:00", d2.End.ToString ("G", CultureInfo.InvariantCulture), "A07");
		Assert.AreEqual(36000000000L, d2.Delta.Ticks, "A08");
	
		DateTime d3 = new DateTime (2002,2,25);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d3), "A09");
		DateTime d4 = new DateTime (2002,4,2);
		Assert.AreEqual(true, t1.IsDaylightSavingTime (d4), "A10");
		DateTime d5 = new DateTime (2002,11,4);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d5), "A11");
	
		Assert.AreEqual(36000000000L, t1.GetUtcOffset (d3).Ticks, "A12");
		Assert.AreEqual(72000000000L, t1.GetUtcOffset (d4).Ticks, "A13");
		Assert.AreEqual(36000000000L, t1.GetUtcOffset (d5).Ticks, "A14");

		// Test TimeZone methods with UTC DateTime in DST.
		DateTime d6 = d4.ToUniversalTime ();
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d6), "A15");
		Assert.AreEqual(0, t1.GetUtcOffset (d6).Ticks, "A16");
	}

	private void EST (TimeZone t1) 
	{
		Assert.IsTrue("EST" == t1.StandardName || "Eastern Standard Time" == t1.StandardName, "B01");
		Assert.IsTrue("EDT" == t1.DaylightName || "Eastern Daylight Time" == t1.DaylightName, "B02");

		DaylightTime d1 = t1.GetDaylightChanges (2002);
		Assert.AreEqual(36000000000L, d1.Delta.Ticks, "B05");

		DaylightTime d2 = t1.GetDaylightChanges (1996);
		Assert.AreEqual(36000000000L, d2.Delta.Ticks, "B08");

		DateTime d3 = new DateTime (2002,2,25);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d3), "B09");
		DateTime d4 = new DateTime (2002,4,8);
		Assert.AreEqual(true, t1.IsDaylightSavingTime (d4), "B10");
		
		DateTime d5 = new DateTime (2002,11,4);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d5), "B11");

		Assert.AreEqual(-180000000000L, t1.GetUtcOffset (d3).Ticks, "B12");
		Assert.AreEqual(-144000000000L, t1.GetUtcOffset (d4).Ticks, "B13");
		Assert.AreEqual(-180000000000L, t1.GetUtcOffset (d5).Ticks, "B14");

		// Test TimeZone methods with UTC DateTime in DST.
		DateTime d6 = d4.ToUniversalTime ();
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d6), "B15");
		Assert.AreEqual(0, t1.GetUtcOffset (d6).Ticks, "B16");
	}

	private void TST (TimeZone t1) 
	{
		Assert.AreEqual("Tokyo Standard Time", t1.StandardName, "C01");
		Assert.AreEqual("Tokyo Standard Time", t1.DaylightName, "C02");

		DateTime d3 = new DateTime (2002,2,25);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d3), "C09");
		DateTime d4 = new DateTime (2002,4,8);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d4), "C10");
		DateTime d5 = new DateTime (2002,11,4);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d5), "C11");

		Assert.AreEqual(324000000000L, t1.GetUtcOffset (d3).Ticks, "C12");
		Assert.AreEqual(324000000000L, t1.GetUtcOffset (d4).Ticks, "C13");
		Assert.AreEqual(324000000000L, t1.GetUtcOffset (d5).Ticks, "C14");
	}

	private void GMT (TimeZone t1) {
		// Probably wont work on MS.NET, but is better than nothing. Where do
		// we change our implementation to match theirs?
		
		Assert.AreEqual("GMT", t1.StandardName, "D01");
		Assert.IsTrue("BST" == t1.DaylightName || "IST" == t1.DaylightName, "D02");
	
		DaylightTime d1 = t1.GetDaylightChanges (2002);
		Assert.AreEqual("03/31/2002 01:00:00", d1.Start.ToString ("G", CultureInfo.InvariantCulture), "D03");
		Assert.AreEqual("10/27/2002 02:00:00", d1.End.ToString ("G", CultureInfo.InvariantCulture), "D04");
		Assert.AreEqual(36000000000L, d1.Delta.Ticks, "D05");
	
		DaylightTime d2 = t1.GetDaylightChanges (1996);
		Assert.AreEqual("03/31/1996 01:00:00", d2.Start.ToString ("G", CultureInfo.InvariantCulture), "D06");
		Assert.AreEqual("10/27/1996 02:00:00", d2.End.ToString ("G", CultureInfo.InvariantCulture), "D07");
		Assert.AreEqual(36000000000L, d2.Delta.Ticks, "D08");
	
		DateTime d3 = new DateTime (2002,2,25);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d3), "D09");
		DateTime d4 = new DateTime (2002,4,2);
		Assert.AreEqual(true, t1.IsDaylightSavingTime (d4), "D10");
		DateTime d5 = new DateTime (2002,11,4);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d5), "D11");
	
		Assert.AreEqual(0L, t1.GetUtcOffset (d3).Ticks, "D12");
		Assert.AreEqual(36000000000L, t1.GetUtcOffset (d4).Ticks, "D13");
		Assert.AreEqual(0L, t1.GetUtcOffset (d5).Ticks, "D14");

		// Test TimeZone methods with UTC DateTime in DST.
		DateTime d6 = d4.ToUniversalTime ();
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d6), "D15");
		Assert.AreEqual(0, t1.GetUtcOffset (d6).Ticks, "D16");
	}

	private void NZST(TimeZone t1) {
		Assert.AreEqual("NZST", t1.StandardName, "E01");
		Assert.AreEqual("NZDT", t1.DaylightName, "E02");

		DaylightTime d1 = t1.GetDaylightChanges (2013);
		Assert.AreEqual("09/29/2013 02:00:00", d1.Start.ToString ("G", CultureInfo.InvariantCulture), "E03");
		Assert.AreEqual("04/07/2013 03:00:00", d1.End.ToString ("G", CultureInfo.InvariantCulture), "E04");
		Assert.AreEqual(36000000000L, d1.Delta.Ticks, "E05");

		DaylightTime d2 = t1.GetDaylightChanges (2001);
		Assert.AreEqual("10/07/2001 02:00:00", d2.Start.ToString ("G", CultureInfo.InvariantCulture), "E06");
		Assert.AreEqual("03/18/2001 03:00:00", d2.End.ToString ("G", CultureInfo.InvariantCulture), "E07");
		Assert.AreEqual(36000000000L, d2.Delta.Ticks, "E08");

		DateTime d3 = new DateTime(2013,02,15);
		Assert.AreEqual(true, t1.IsDaylightSavingTime (d3), "E09");
		DateTime d4 = new DateTime(2013,04,30);
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d4), "E10");
		DateTime d5 = new DateTime(2013,11,03);
		Assert.AreEqual(true, t1.IsDaylightSavingTime (d5), "E11");

		Assert.AreEqual(36000000000L /*hour*/ * 13L, t1.GetUtcOffset (d3).Ticks, "E12");
		Assert.AreEqual(36000000000L /*hour*/ * 12L, t1.GetUtcOffset (d4).Ticks, "E13");
		Assert.AreEqual(36000000000L /*hour*/ * 13L, t1.GetUtcOffset (d5).Ticks, "E14");

		// Test TimeZone methods with UTC DateTime in DST.
		DateTime d6 = d5.ToUniversalTime ();
		Assert.AreEqual(false, t1.IsDaylightSavingTime (d6), "E15");
		Assert.AreEqual(0, t1.GetUtcOffset (d6).Ticks, "E16");
	}

	[Test]
	public void TestCtors ()
	{
		TimeZone t1 = TimeZone.CurrentTimeZone;
		switch (t1.StandardName) {
			case "W. Europe Standard Time":
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
#if MOBILE
				if (string.IsNullOrEmpty (t1.DaylightName))
					Assert.Ignore ("This test may fail due to: http://www.openradar.me/38174449. This value is also empty on recent Android versions.");
#endif
				GMT (t1);
				break;
			case "NZST":
				NZST (t1);
				break;
			default:
				NUnit.Framework.Assert.Ignore ("Your time zone (" + t1.StandardName + ") isn't defined in the test case");
				break;
		}
        }

	[Test]
	public void CurrentTimeZone_SerializationRoundtrip ()
	{
		TimeZone tz = TimeZone.CurrentTimeZone;
		BinaryFormatter bf = new BinaryFormatter ();
		MemoryStream ms = new MemoryStream ();
		bf.Serialize (ms, tz);

		ms.Position = 0;
		TimeZone clone = (TimeZone) bf.Deserialize (ms);

		Assert.AreEqual (tz.DaylightName, clone.DaylightName, "DaylightName");
		Assert.AreEqual (tz.StandardName, clone.StandardName, "StandardName");
	}

	static private byte[] serialized_timezone = {
		0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00, 
		0x1C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x43, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x54, 
		0x69, 0x6D, 0x65, 0x5A, 0x6F, 0x6E, 0x65, 0x04, 0x00, 0x00, 0x00, 0x17, 0x6D, 0x5F, 0x43, 0x61, 0x63, 0x68, 0x65, 0x64, 0x44, 0x61, 
		0x79, 0x6C, 0x69, 0x67, 0x68, 0x74, 0x43, 0x68, 0x61, 0x6E, 0x67, 0x65, 0x73, 0x0D, 0x6D, 0x5F, 0x74, 0x69, 0x63, 0x6B, 0x73, 0x4F, 
		0x66, 0x66, 0x73, 0x65, 0x74, 0x0E, 0x6D, 0x5F, 0x73, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x4E, 0x61, 0x6D, 0x65, 0x0E, 0x6D, 
		0x5F, 0x64, 0x61, 0x79, 0x6C, 0x69, 0x67, 0x68, 0x74, 0x4E, 0x61, 0x6D, 0x65, 0x03, 0x00, 0x01, 0x01, 0x1C, 0x53, 0x79, 0x73, 0x74, 
		0x65, 0x6D, 0x2E, 0x43, 0x6F, 0x6C, 0x6C, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x73, 0x2E, 0x48, 0x61, 0x73, 0x68, 0x74, 0x61, 0x62, 
		0x6C, 0x65, 0x09, 0x09, 0x02, 0x00, 0x00, 0x00, 0x00, 0xF8, 0x29, 0x17, 0xD6, 0xFF, 0xFF, 0xFF, 0x06, 0x03, 0x00, 0x00, 0x00, 0x15, 
		0x45, 0x61, 0x73, 0x74, 0x65, 0x72, 0x6E, 0x20, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x20, 0x54, 0x69, 0x6D, 0x65, 0x0A, 
		0x04, 0x02, 0x00, 0x00, 0x00, 0x1C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x43, 0x6F, 0x6C, 0x6C, 0x65, 0x63, 0x74, 0x69, 0x6F, 
		0x6E, 0x73, 0x2E, 0x48, 0x61, 0x73, 0x68, 0x74, 0x61, 0x62, 0x6C, 0x65, 0x07, 0x00, 0x00, 0x00, 0x0A, 0x4C, 0x6F, 0x61, 0x64, 0x46, 
		0x61, 0x63, 0x74, 0x6F, 0x72, 0x07, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x08, 0x43, 0x6F, 0x6D, 0x70, 0x61, 0x72, 0x65, 0x72, 
		0x10, 0x48, 0x61, 0x73, 0x68, 0x43, 0x6F, 0x64, 0x65, 0x50, 0x72, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x72, 0x08, 0x48, 0x61, 0x73, 0x68, 
		0x53, 0x69, 0x7A, 0x65, 0x04, 0x4B, 0x65, 0x79, 0x73, 0x06, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x73, 0x00, 0x00, 0x03, 0x03, 0x00, 0x05, 
		0x05, 0x0B, 0x08, 0x1C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x43, 0x6F, 0x6C, 0x6C, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x73, 
		0x2E, 0x49, 0x43, 0x6F, 0x6D, 0x70, 0x61, 0x72, 0x65, 0x72, 0x24, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x43, 0x6F, 0x6C, 0x6C, 
		0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x73, 0x2E, 0x49, 0x48, 0x61, 0x73, 0x68, 0x43, 0x6F, 0x64, 0x65, 0x50, 0x72, 0x6F, 0x76, 0x69, 
		0x64, 0x65, 0x72, 0x08, 0xEC, 0x51, 0x38, 0x3F, 0x03, 0x00, 0x00, 0x00, 0x0A, 0x0A, 0x0B, 0x00, 0x00, 0x00, 0x09, 0x04, 0x00, 0x00, 
		0x00, 0x09, 0x05, 0x00, 0x00, 0x00, 0x10, 0x04, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x08, 0x08, 0xCC, 0x07, 0x00, 0x00, 0x08, 
		0x08, 0xD5, 0x07, 0x00, 0x00, 0x08, 0x08, 0xD2, 0x07, 0x00, 0x00, 0x10, 0x05, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x09, 0x06, 
		0x00, 0x00, 0x00, 0x09, 0x07, 0x00, 0x00, 0x00, 0x09, 0x08, 0x00, 0x00, 0x00, 0x04, 0x06, 0x00, 0x00, 0x00, 0x21, 0x53, 0x79, 0x73, 
		0x74, 0x65, 0x6D, 0x2E, 0x47, 0x6C, 0x6F, 0x62, 0x61, 0x6C, 0x69, 0x7A, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x2E, 0x44, 0x61, 0x79, 0x6C, 
		0x69, 0x67, 0x68, 0x74, 0x54, 0x69, 0x6D, 0x65, 0x03, 0x00, 0x00, 0x00, 0x07, 0x6D, 0x5F, 0x73, 0x74, 0x61, 0x72, 0x74, 0x05, 0x6D, 
		0x5F, 0x65, 0x6E, 0x64, 0x07, 0x6D, 0x5F, 0x64, 0x65, 0x6C, 0x74, 0x61, 0x00, 0x00, 0x00, 0x0D, 0x0D, 0x0C, 0x00, 0x10, 0xFA, 0x0F, 
		0x3D, 0xF2, 0xBC, 0x88, 0x00, 0x50, 0xD5, 0xB1, 0xC1, 0x91, 0xBD, 0x88, 0x00, 0x68, 0xC4, 0x61, 0x08, 0x00, 0x00, 0x00, 0x01, 0x07, 
		0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x50, 0x23, 0xFA, 0x07, 0x06, 0xC7, 0x88, 0x00, 0xD0, 0xE2, 0xC4, 0x0C, 0xAB, 0xC7, 
		0x88, 0x00, 0x68, 0xC4, 0x61, 0x08, 0x00, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x50, 0x0C, 0x0F, 
		0xEF, 0xAB, 0xC3, 0x88, 0x00, 0x90, 0xE7, 0xB0, 0x73, 0x4B, 0xC4, 0x88, 0x00, 0x68, 0xC4, 0x61, 0x08, 0x00, 0x00, 0x00, 0x0B
	};

	[Test]
	[Category ("NotWorking")] 
	// 1.x - deserialize but strings are null
	// 2.x - eexception when creating a datetime with a negative value
	public void DeserializeKnownValue ()
	{
		MemoryStream ms = new MemoryStream (serialized_timezone);
		BinaryFormatter bf = new BinaryFormatter ();
		TimeZone tz = (TimeZone) bf.Deserialize (ms);
		Assert.AreEqual ("Eastern Daylight Time", tz.DaylightName, "DaylightName");
		Assert.AreEqual ("Eastern Standard Time", tz.StandardName, "StandardName");
	}

	[Test]
	public void ToLocalTimeAtDSTBoundaries ()
	{
		TimeZone tz = TimeZone.CurrentTimeZone;
		DateTime dst_start_utc = tz.GetDaylightChanges(2007).Start.ToUniversalTime ();

		if (dst_start_utc == DateTime.MinValue)
			Assert.Ignore ("Couldn't get beginning of daylight saving time in 2007.");
		Assert.IsTrue (tz.ToLocalTime (dst_start_utc.Subtract (new TimeSpan (0, 1, 0))) < tz.ToLocalTime (dst_start_utc), "0:1:59 < 0:3:00");
		Assert.IsTrue (tz.ToLocalTime (dst_start_utc) < tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (0, 1, 0))), "0:3:00 < 0:3:01");
		Assert.IsTrue (tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (0, 1, 0))) < tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (0, 59, 0))), "0:3:01 < 0:3:59");
		Assert.IsTrue (tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (0, 59, 0))) < tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (1, 0, 0))), "0:3:59 < 0:4:00");
		Assert.IsTrue (tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (1, 0, 0))) < tz.ToLocalTime (dst_start_utc.Add (new TimeSpan (1, 1, 0))), "0:4:00 < 0:4:01");
	}

		[Test]
		[Category ("MobileNotWorking")]
		public void GetUTCNowAtDSTBoundaries ()
		{
			TimeZoneInfo.TransitionTime startTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday);

			TimeZoneInfo.TransitionTime endTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday);

			TimeSpan delta = TimeSpan.FromMinutes(60.0);
			TimeZoneInfo.AdjustmentRule adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1970, 1, 1), DateTime.MaxValue.Date, delta, startTransition, endTransition);
			TimeZoneInfo.TransitionTime startTrans = adjustment.DaylightTransitionStart;
			TimeZoneInfo.TransitionTime endTrans = adjustment.DaylightTransitionEnd;
			TimeZoneInfo.AdjustmentRule[] adjustments = { adjustment };

			TimeZoneInfo tzInfo = TimeZoneInfo.CreateCustomTimeZone("MY Standard Time", TimeSpan.Zero, "MST", "MST", "MDT", adjustments);

			TimeZoneInfoTest.SetLocal(tzInfo);

			DateTime st = new DateTime(2016, 3, 27, 1, 0, 0, DateTimeKind.Local);
			Assert.IsTrue (!tzInfo.IsDaylightSavingTime(st));	
			Assert.IsTrue (!tzInfo.IsAmbiguousTime(st));
			Assert.IsTrue ((TimeZoneInfo.ConvertTimeToUtc(st).Hour == 1));
			st = new DateTime(2016, 3, 27, 3, 0, 0, DateTimeKind.Local);
			Assert.IsTrue (tzInfo.IsDaylightSavingTime(st));	
			Assert.IsTrue (!tzInfo.IsAmbiguousTime(st));
			Assert.IsTrue ((TimeZoneInfo.ConvertTimeToUtc(st).Hour == 2));
			st = new DateTime(2016, 10, 30, 2, 0, 0, DateTimeKind.Local);
#if !MOBILE
			Assert.IsFalse (tzInfo.IsDaylightSavingTime(st));	
			Assert.IsFalse (!tzInfo.IsAmbiguousTime(st));
			Assert.IsFalse ((TimeZoneInfo.ConvertTimeToUtc(st).Hour == 1));
			st = new DateTime(2016, 10, 30, 3, 0, 0, DateTimeKind.Local);
			Assert.IsTrue (!tzInfo.IsDaylightSavingTime(st));	
			Assert.IsFalse (tzInfo.IsAmbiguousTime(st));
			Assert.IsTrue ((TimeZoneInfo.ConvertTimeToUtc(st).Hour == 3));
			st = new DateTime(2016, 10, 30, 4, 0, 0, DateTimeKind.Local);
			Assert.IsTrue (!tzInfo.IsDaylightSavingTime(st));	
			Assert.IsTrue (!tzInfo.IsAmbiguousTime(st));
			Assert.IsTrue ((TimeZoneInfo.ConvertTimeToUtc(st).Hour == 4));
#endif
		}

		[Test]
		public void GetUtcOffsetAtDSTBoundary ()
		{
			/*
			 * Getting a definitive list of timezones which do or don't observe Daylight
			 * Savings is difficult (can't say America's or USA definitively) and lengthy see 
			 *
			 * http://en.wikipedia.org/wiki/Daylight_saving_time_by_country
			 *
			 * as a good starting point for a list.
			 *
			 * The following are SOME of the timezones/regions which do support daylight savings.
			 *
			 * Pacific/Auckland
			 * Pacific/Sydney
			 * USA (EST, CST, MST, PST, AKST) note this does not cover all states or regions
			 * Europe/London (GMT)
			 * CET (member states of the European Union)
			 *
			 * This test should work in all the above timezones
			 */


			TimeZone tz = TimeZone.CurrentTimeZone;
			int year = DateTime.Now.Year;
			DaylightTime daylightChanges = tz.GetDaylightChanges(year);
			DateTime dst_end = daylightChanges.End;

			if (dst_end == DateTime.MinValue)
				Assert.Ignore (tz.StandardName + " did not observe daylight saving time during " + year + ".");

			var standardOffset = tz.GetUtcOffset(daylightChanges.Start.AddMinutes(-1));
			var dstOffset = tz.GetUtcOffset(daylightChanges.Start.AddMinutes(1));

//			Assert.AreEqual(standardOffset, tz.GetUtcOffset (dst_end));
			Assert.AreEqual(dstOffset, tz.GetUtcOffset (dst_end.Add (daylightChanges.Delta.Negate ().Add (TimeSpan.FromSeconds(1)))));
			Assert.AreEqual(dstOffset, tz.GetUtcOffset (dst_end.Add(daylightChanges.Delta.Negate ())));
		}


		[Test]
		public void StaticProperties ()
		{
			Assert.IsNotNull (TimeZoneInfo.Local, "Local");
			Assert.IsNotNull (TimeZoneInfo.Utc, "Utc");
		}
		
		[Test]
		[Category ("NotWasm")]
		public void FindSystemTimeZoneById ()
		{
			TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById (TimeZoneInfoTest.MapTimeZoneId ("America/Toronto"));
			Assert.IsTrue ("EDT" == tzi.DaylightName || "Eastern Daylight Time" == tzi.DaylightName, "DaylightName");
			Assert.IsTrue ("EST" == tzi.StandardName || "Eastern Standard Time" == tzi.StandardName, "StandardName");
			Assert.IsTrue (tzi.SupportsDaylightSavingTime, "SupportsDaylightSavingTime");
		}

		[Test]
		public void OldEraToLocalTime ()
		{
			TimeSpan offset = TimeSpan.Zero;
			var dto = new DateTimeOffset (new DateTime (1900, 1, 1).Ticks, offset);

			// Should never throw
			dto.ToLocalTime ();
		}

#if !WIN_PLATFORM
		// On device we cannot read the OS file system to look for /etc/localtime
		// and /usr/share/zoneinfo - so we must initialize the BCL TimeZoneInfo
		// from NSTimeZoneInfo. The tests here check the code paths between the
		// two types - if they break then TimeZoneInfo work work at all
		// ref: http://bugzilla.xamarin.com/show_bug.cgi?id=1790
		
		[Test]
		[Category ("NotWasm")]
		public void GetSystemTimeZones ()
		{
			Assert.That (TimeZoneInfo.GetSystemTimeZones ().Count, Is.GreaterThan (400), "GetSystemTimeZones");
		}
#endif
	}
}
