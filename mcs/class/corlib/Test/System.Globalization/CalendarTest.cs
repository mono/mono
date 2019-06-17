// CalendarTest.cs
//
// (C) 2002 Ulrich Kunitz
//

using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace MonoTests.System.Globalization
{

sealed class Date {
	private int _day, _month, _year, _era;

	public int Day {
		get {
			return _day;
		}
		set {
			if (value < 1 || value > 31)
				throw new ArgumentOutOfRangeException(
					"Day",
					"Day must be between 1 and 31.");
			_day = value;
		}
	}

	public int Month {
		get {
			return _month;
		}
		set {
			if (value < 1 || value > 13)
				throw new ArgumentOutOfRangeException(
					"Month",
					"Month must be between 1 and 13.");
			_month = value;
		}
	}

	public int Year {
		get {
			return _year;
		}
		set {
			_year = value;
		}
	}

	public int Era {
		get {
			return _era;
		}
		set {
			if (value < 1 || value > 10)
				throw new ArgumentOutOfRangeException(
					"Era",
					"Era must be between 1 and 10.");
			_era = value;
		}
	}
					
	public Date(int day, int month, int year, int era) {
		Day = day;
		Month = month;
		Year = year;
		Era = era;
	}
	public Date(int day, int month, int year) : this(day,month,year,1) {}
	public Date() : this(1,1,1,1) {}

	public DateTime ToDateTime(Calendar cal) {
		return cal.ToDateTime(Year,Month,Day,0,0,0,0,Era);
	}

	public void FromDateTime(Calendar cal, DateTime time) {
		Date dmy = new Date();
		dmy.Day = cal.GetDayOfMonth(time);
		dmy.Month = cal.GetMonth(time);
		dmy.Year = cal.GetYear(time);
		dmy.Era = cal.GetEra(time);
		Day = dmy.Day;
		Month = dmy.Month;
		Year = dmy.Year;
		Era = dmy.Era;
	}

	public override string ToString() {
		StringWriter sw = new StringWriter();
		sw.Write("{0}.{1}.{2}", Day, Month, Year);
		if (Era != 1) {
			sw.Write(" era {0}", Era);
		}
		return sw.ToString();
	}

	public override bool Equals(Object b) {
		if (b == null || GetType() != b.GetType())
			return false;
		return Equals(this, (Date)b);
	}

	public static bool Equals(Date a, Date b) {
		if (a == b)
			return true;
		if (a.Year != b.Year)
			return false;
		if (a.Month != b.Month)
			return false;
		if (a.Day != b.Day)
			return false;
		if (a.Era != b.Era)
			return false;
		return true;
	}

	public override int GetHashCode() {
		return ToString().GetHashCode();
	}
} // class Date

[TestFixture]
[Category ("Calendars")]
public class CalendarTest {
	private Calendar[] acal;
	private GregorianCalendar gcal;
	private JulianCalendar jucal;
	private HijriCalendar hical;
	private HebrewCalendar hecal;
	private JapaneseCalendar jacal;
	private TaiwanCalendar tacal;
	private KoreanCalendar kcal;
	private ThaiBuddhistCalendar tbcal;
	private ChineseLunisolarCalendar clcal;
	private TaiwanLunisolarCalendar tlcal;
	private JapaneseLunisolarCalendar jlcal;
	private KoreanLunisolarCalendar klcal;

	[SetUp]
	protected void SetUp() {
		gcal = new GregorianCalendar();
		jucal = new JulianCalendar();
		hical = new HijriCalendar();
		hecal = new HebrewCalendar();
		jacal = new JapaneseCalendar();
		tacal = new TaiwanCalendar();
		kcal = new KoreanCalendar();
		tbcal = new ThaiBuddhistCalendar();
		acal = new Calendar[] {
			gcal, jucal, hical, hecal, jacal,
			tacal, kcal, tbcal};
		clcal = new ChineseLunisolarCalendar ();
		tlcal = new TaiwanLunisolarCalendar ();
		jlcal = new JapaneseLunisolarCalendar ();
		klcal = new KoreanLunisolarCalendar ();
	}

	private void RowCheck(params Date[] adate) {
		if (adate.Length != acal.Length)
			throw new ArgumentException(
				"Number of Date arguments doesn't match " +
				"length of calendar array.");

		DateTime timeG = adate[0].ToDateTime(gcal);
		for (int i = 0; i < acal.Length; i++) {
			Date date = adate[i];
			if (date == null)
				continue;
			Calendar cal = acal[i];

			DateTime time = date.ToDateTime(cal);
			StringWriter sw = new StringWriter();
			sw.Write("Calendar {0} computes wrong DateTime.",
				cal);
			Assert.AreEqual(timeG, time, sw.ToString());

			sw = new StringWriter();
			Date ndate = new Date();
			ndate.FromDateTime(cal, time);
			sw.Write("Calendar {0} computes wrong date", cal);
			Assert.AreEqual(date, ndate, sw.ToString());
		}
	}

	// We are testing the implementation against the reference dates in
	// Calendrical Calcualation. Please note that the CLR uses another
	// epoch for the HijriCalendar, which might be the perfect thing
	// to do.
	[Test]
	public void TestCCTable() {
		// Gr Ju Hi He Ja Ta Ko TB
		RowCheck(new Date(24,9,70,1),
			new Date(26,9,70,1),
			null,
			null,
			null,
			null,
			new Date(24,9,2403,1),
			new Date(24,9,613,1));
		RowCheck(new Date(2,10,135,1),
			new Date(3,10,135,1),
			null,
			null,
			null,
			null,
			new Date(2,10,2468,1),
			new Date(2,10,678,1));
		RowCheck(new Date(8,1,470,1),
			new Date(7,1,470,1),
			null,
			null,
			null,
			null,
			new Date(8,1,2803,1),
			new Date(8,1,1013,1));
		RowCheck(new Date(20,5,576,1),
			new Date(18,5,576,1),
			null,
			null,
			null,
			null,
			new Date(20,5,2909,1),
			new Date(20,5,1119,1));
		RowCheck(new Date(10,11,694,1),
			new Date(7,11,694,1),
			new Date(14,7,75,1),
			null,
			null,
			null,
			new Date(10,11,3027,1),
			new Date(10,11,1237,1));
		RowCheck(new Date(25,4,1013,1),
			new Date(19,4,1013,1),
			new Date(6,10,403,1),
			null,
			null,
			null,
			new Date(25,4,3346,1),
			new Date(25,4,1556,1));
		RowCheck(new Date(24,5,1096,1),
			new Date(18,5,1096,1),
			new Date(23,5,489,1),
			null,
			null,
			null,
			new Date(24,5,3429,1),
			new Date(24,5,1639,1));
		RowCheck(new Date(23,3,1190,1),
			new Date(16,3,1190,1),
			new Date(8,2,586,1),
			null,
			null,
			null,
			new Date(23,3,3523,1),
			new Date(23,3,1733,1));
		RowCheck(new Date(10,3,1240,1),
			new Date(3,3,1240,1),
			new Date(8,8,637,1),
			null,
			null,
			null,
			new Date(10,3,3573,1),
			new Date(10,3,1783,1));
		RowCheck(new Date(2,4,1288,1),
			new Date(26,3,1288,1),
			new Date(21,2,687,1),
			null,
			null,
			null,
			new Date(2,4,3621,1),
			new Date(2,4,1831,1));
		RowCheck(new Date(27,4,1298,1),
			new Date(20,4,1298,1),
			new Date(8,7,697,1),
			null,
			null,
			null,
			new Date(27,4,3631,1),
			new Date(27,4,1841,1));
		RowCheck(new Date(12,6,1391,1),
			new Date(4,6,1391,1),
			new Date(2,7,793,1),
			null,
			null,
			null,
			new Date(12,6,3724,1),
			new Date(12,6,1934,1));
		RowCheck(new Date(3,2,1436,1),
			new Date(25,1,1436,1),
			new Date(7,7,839,1),
			null,
			null,
			null,
			new Date(3,2,3769,1),
			new Date(3,2,1979,1));
		RowCheck(new Date(9,4,1492,1),
			new Date(31,3,1492,1),
			new Date(2,6,897,1),
			null,
			null,
			null,
			new Date(9,4,3825,1),
			new Date(9,4,2035,1));
		RowCheck(new Date(19,9,1553,1),
			new Date(9,9,1553,1),
			new Date(1,10,960,1),
			null,
			null,
			null,
			new Date(19,9,3886,1),
			new Date(19,9,2096,1));
		RowCheck(new Date(5,3,1560,1),
			new Date(24,2,1560,1),
			new Date(28,5,967,1),
			null,
			null,
			null,
			new Date(5,3,3893,1),
			new Date(5,3,2103,1));
		RowCheck(new Date(10,6,1648,1),
			new Date(31,5,1648,1),
			new Date(19,5,1058,1),
			new Date(20,9,5408,1),
			null,
			null,
			new Date(10,6,3981,1),
			new Date(10,6,2191,1));
		RowCheck(new Date(30,6,1680,1),
			new Date(20,6,1680,1),
			new Date(3,6,1091,1),
			new Date(3,11,5440,1),
			null,
			null,
			new Date(30,6,4013,1),
			new Date(30,6,2223,1));
		RowCheck(new Date(24,7,1716,1),
			new Date(13,7,1716,1),
			new Date(5,8,1128,1),
			new Date(5,11,5476,1),
			null,
			null,
			new Date(24,7,4049,1),
			new Date(24,7,2259,1));
		RowCheck(new Date(19,6,1768,1),
			new Date(8,6,1768,1),
			new Date(4,2,1182,1),
			new Date(4,10,5528,1),
			null,
			null,
			new Date(19,6,4101,1),
			new Date(19,6,2311,1));
		RowCheck(new Date(2,8,1819,1),
			new Date(21,7,1819,1),
			new Date(11,10,1234,1),
			new Date(11,11,5579,1),
			null,
			null,
			new Date(2,8,4152,1),
			new Date(2,8,2362,1));
		RowCheck(new Date(27,3,1839,1),
			new Date(15,3,1839,1),
			new Date(12,1,1255,1),
			new Date(12,7,5599,1),
			null,
			null,
			new Date(27,3,4172,1),
			new Date(27,3,2382,1));
		RowCheck(new Date(19,4,1903,1),
			new Date(6,4,1903,1),
			new Date(22,1,1321,1),
			new Date(22,7,5663,1),
			new Date(19,4,36,1),
			null,
			new Date(19,4,4236,1),
			new Date(19,4,2446,1));
		RowCheck(new Date(25,8,1929,1),
			new Date(12,8,1929,1),
			new Date(20,3,1348,1),
			new Date(19,12,5689,1),
			new Date(25,8,4,3),
			new Date(25,8,18,1),
			new Date(25,8,4262,1),
			new Date(25,8,2472,1));
		RowCheck(new Date(29,9,1941,1),
			new Date(16,9,1941,1),
			new Date(9,9,1360,1),
			new Date(8,1,5702,1),
			new Date(29,9,16,3),
			new Date(29,9,30,1),
			new Date(29,9,4274,1),
			new Date(29,9,2484,1));
		RowCheck(new Date(19,4,1943,1),
			new Date(6,4,1943,1),
			new Date(14,4,1362,1),
			new Date(14,8,5703,1),
			new Date(19,4,18,3),
			new Date(19,4,32,1),
			new Date(19,4,4276,1),
			new Date(19,4,2486,1));
		RowCheck(new Date(7,10,1943,1),
			new Date(24,9,1943,1),
			new Date(8,10,1362,1),
			new Date(8,1,5704,1),
			new Date(7,10,18,3),
			new Date(7,10,32,1),
			new Date(7,10,4276,1),
			new Date(7,10,2486,1));
		RowCheck(new Date(17,3,1992,1),
			new Date(4,3,1992,1),
			new Date(14,9,1412,1),
			new Date(12,7,5752,1),
			new Date(17,3,4,4),
			new Date(17,3,81,1),
			new Date(17,3,4325,1),
			new Date(17,3,2535,1));
		RowCheck(new Date(25,5,1996,1),
			new Date(12,5,1996,1),
			new Date(8,1,1417,1),
			new Date(7,9,5756,1),
			new Date(25,5,8,4),
			new Date(25,5,85,1),
			new Date(25,5,4329,1),
			new Date(25,5,2539,1));
		RowCheck(new Date(10,11,2038,1),
			new Date(28,10,2038,1),
			new Date(13,10,1460,1),
			new Date(12,2,5799,1),
			new Date(10,11,20,5),
			new Date(10,11,127,1),
			new Date(10,11,4371,1),
			new Date(10,11,2581,1));
		RowCheck(new Date(18,7,2094,1),
			new Date(5,7,2094,1),
			new Date(6,3,1518,1),
			new Date(5,11,5854,1),
			new Date(18,7,76,5),
			new Date(18,7,183,1),
			new Date(18,7,4427,1),
			new Date(18,7,2637,1));
	}

	[Test]
	public void TestCalendarType() {
		GregorianCalendar gc = new GregorianCalendar(
			GregorianCalendarTypes.Arabic);
		Assert.AreEqual(GregorianCalendarTypes.Arabic,
						gc.CalendarType,
						"A01 Gregorian ctor with GregorianCalendarTypes parameter");
		gc.CalendarType = GregorianCalendarTypes.MiddleEastFrench;
		Assert.AreEqual(GregorianCalendarTypes.MiddleEastFrench, 
					 gc.CalendarType, 
					 "A02 GregorianCalendar.CalendarType");
			
	}

	[Test]
	public void TestStandardEras() {
		Assert.AreEqual(1, GregorianCalendar.ADEra, "B01 ADEra");
		Assert.AreEqual(1, HebrewCalendar.HebrewEra, "B02 HebrewEra");
		Assert.AreEqual(1, HijriCalendar.HijriEra, "B03 HjriEra");
		Assert.AreEqual(1, JulianCalendar.JulianEra, "B04 JulianEra");
		Assert.AreEqual(1, KoreanCalendar.KoreanEra, "B05 KoreanEra");
		Assert.AreEqual(1, ThaiBuddhistCalendar.ThaiBuddhistEra, "B06 ThaiBuddhistEra");
			
		Assert.AreEqual(1, ChineseLunisolarCalendar.ChineseEra, "CNLunisor");
		Assert.AreEqual(1, JapaneseLunisolarCalendar.JapaneseEra, "JPLunisor");
		Assert.AreEqual(1, KoreanLunisolarCalendar.GregorianEra, "KRLunisor");
	}

	[Test]
	public void TestCurrentEra() {
		Assert.AreEqual(0, GregorianCalendar.CurrentEra, 
					 "C01 GregorianCalendar.CurrentEra");
		Assert.AreEqual(0, HebrewCalendar.CurrentEra,
					 "C02 HebrewCalendar.CurrentEra");
		Assert.AreEqual(0, HijriCalendar.CurrentEra, 
					 "C03 HijriCalendar.CurrentEra");
		Assert.AreEqual(0, JapaneseCalendar.CurrentEra, 
					 "C04 JapaneseCalendar.CurrentEra");
		Assert.AreEqual(0, JulianCalendar.CurrentEra, 
					 "C05 JulianCalendar.CurrentEra");
		Assert.AreEqual(0, KoreanCalendar.CurrentEra,
					 "C06 KoreanCalendar.CurrentEra");
		Assert.AreEqual(0, TaiwanCalendar.CurrentEra,
					 "C07 TaiwanCalendar.CurrentEra");
		Assert.AreEqual(0, ThaiBuddhistCalendar.CurrentEra,
					 "C08 ThaiBuddhistCalendar.CurrentEra");
	}

	[Test]
	public void TestErasProperty() {
		foreach (Calendar cal in acal) {
			int check = 1;
			if (cal is JapaneseCalendar)
				check = 5;
			Assert.AreEqual(check, cal.Eras.Length,
						 String.Format("D01 {0}.Eras.Length", cal));
			cal.Eras[0] = 29;
			Assert.IsTrue(cal.Eras[0] != 29, String.Format("D02 {0}.Eras readonly", cal));
		}
	}

	[Test]
	public void TestErasProperty2() {
		Assert.AreEqual(1, clcal.Eras.Length, "cn");
		Assert.AreEqual(1, tlcal.Eras.Length, "tw");
		Assert.AreEqual(3, jlcal.Eras.Length, "jp");
		Assert.AreEqual(1, klcal.Eras.Length, "kr");

		Assert.AreEqual(5, jlcal.Eras [0], "jp.1");
		Assert.AreEqual(4, jlcal.Eras [1], "jp.2");
		Assert.AreEqual(3, jlcal.Eras [2], "jp.3");
	}

	[Test]
	public void TestTwoDigitYearMax() {
		Assert.AreEqual(2029, gcal.TwoDigitYearMax,
					 "E01 TwoDigitYearMax GregorianCalendar");
		Assert.AreEqual(5790, hecal.TwoDigitYearMax,
					 "E02 TwoDigitYearMax HebrewCalendar");
		Assert.AreEqual(1451, hical.TwoDigitYearMax,
					 "E03 TwoDigitYearMax HijriCalendar");
		Assert.AreEqual(99, jacal.TwoDigitYearMax,
					 "E04 TwoDigitYearMax JapaneseCalendar");
		Assert.AreEqual(2029, jucal.TwoDigitYearMax,
					 "E05 TwoDigitYearMax JulianCalendar");
		Assert.AreEqual(4362, kcal.TwoDigitYearMax,
					 "E06 TwoDigitYearMax KoreanCalendar");
		Assert.AreEqual(99, tacal.TwoDigitYearMax,
					 "E07 TwoDigitYearMax TaiwanCalendar");
		Assert.AreEqual(2572, tbcal.TwoDigitYearMax,
					 "E08 TwoDigitYearMax ThaiBuddhistCalendar");
		foreach (Calendar cal in acal) {
			bool exception = false;
			try { 
				cal.TwoDigitYearMax = 99;
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}

			Assert.IsFalse(exception,
				   String.Format("E09 {0}.TwoDigitYearMax 99 " +
								 " out of range exception", cal));

			exception = false;
			int m = 10000;
			try {
				m = cal.GetYear(DateTime.MaxValue)+1;
				cal.TwoDigitYearMax = m;
			}
			catch (ArgumentException) {
				exception = true;
			}
			Assert.IsTrue(exception,
				   String.Format("E10 {0}.TwoDigitYearMax out " +
								 " of range exception value {1}",
								 cal, m));
		}
	}

	[Test] // wrt bug #76252.
	public void HebrewCalendarGetDaysInMonth ()
	{
		HebrewCalendar c = new HebrewCalendar ();
		int year = c.GetYear (new DateTime (2005, 9, 1));
		Assert.AreEqual (5765, year);
		int days = c.GetDaysInMonth (year, 13, 1);
		Assert.AreEqual (29, days);
	}

	[Test] // bug #81783
	public void GregorianAddMonth ()
	{
		GregorianCalendar c = new GregorianCalendar ();
		DateTime d = new DateTime (2007, 5, 31);
		DateTime prev = c.AddMonths (d, -1);
		Assert.AreEqual (4, prev.Month, "prev");
		DateTime next = c.AddMonths (d, 1);
		Assert.AreEqual (6, next.Month, "next");

		d = new DateTime (2003, 12, 5);
		prev = c.AddMonths (d, -13);
		Assert.AreEqual (new DateTime (2002, 11, 5), prev, "prev2");
		next = c.AddMonths (d, 6);
		Assert.AreEqual (new DateTime (2004, 6, 5), next, "next2");
	}

	[Test]
	public void AddYearOnLeapYear ()
	{
		GregorianCalendar c = new GregorianCalendar ();
		DateTime d = new DateTime (2004, 2, 29);
		DateTime prev = c.AddYears (d, -1);
		Assert.AreEqual (2, prev.Month, "prev");
		DateTime next = c.AddYears (d, 1);
		Assert.AreEqual (2, next.Month, "next");
	}

	[Test]
	public void GetLeapMonth ()
	{
		GregorianCalendar gc = new GregorianCalendar ();
		Assert.AreEqual (0, gc.GetLeapMonth (2007), "#1-1");
		Assert.AreEqual (0, gc.GetLeapMonth (2008), "#1-2");
		Assert.AreEqual (0, gc.GetLeapMonth (2100), "#1-3");
		Assert.AreEqual (0, gc.GetLeapMonth (2000), "#1-4");

		JulianCalendar jc = new JulianCalendar ();
		Assert.AreEqual (0, jc.GetLeapMonth (2007), "#2-1");
		Assert.AreEqual (0, jc.GetLeapMonth (2008), "#2-2");
		Assert.AreEqual (0, jc.GetLeapMonth (2100), "#2-3");
		Assert.AreEqual (0, jc.GetLeapMonth (2000), "#2-4");
		Assert.AreEqual (0, jc.GetLeapMonth (2009), "#2-5");
		Assert.AreEqual (0, jc.GetLeapMonth (2010), "#2-6");

		HebrewCalendar hc = new HebrewCalendar ();
		// 3rd, 6th, 8th, 11th 14th and 17th year in every 19 are leap.
		// 5339 % 19 = 0.
		Assert.AreEqual (0, hc.GetLeapMonth (5343), "#3-1");
		Assert.AreEqual (0, hc.GetLeapMonth (5344), "#3-2");
		Assert.AreEqual (7, hc.GetLeapMonth (5345), "#3-3");
		Assert.AreEqual (0, hc.GetLeapMonth (5346), "#3-4");
		Assert.AreEqual (7, hc.GetLeapMonth (5347), "#3-5");
		Assert.AreEqual (0, hc.GetLeapMonth (5348), "#3-6");
		Assert.AreEqual (0, hc.GetLeapMonth (5349), "#3-7");

		ThaiBuddhistCalendar tc = new ThaiBuddhistCalendar ();
		Assert.AreEqual (0, tc.GetLeapMonth (2520), "#4-1");
		Assert.AreEqual (0, tc.GetLeapMonth (2521), "#4-2");
		Assert.AreEqual (0, tc.GetLeapMonth (2522), "#4-3");
		Assert.AreEqual (0, tc.GetLeapMonth (2523), "#4-4");

		ChineseLunisolarCalendar cc = new ChineseLunisolarCalendar ();
		Assert.AreEqual (0, cc.GetLeapMonth (2000), "#5-1");
		Assert.AreEqual (5, cc.GetLeapMonth (2001), "#5-2");
		Assert.AreEqual (0, cc.GetLeapMonth (2002), "#5-3");
		Assert.AreEqual (0, cc.GetLeapMonth (2003), "#5-4");
		Assert.AreEqual (3, cc.GetLeapMonth (2004), "#5-5");
		Assert.AreEqual (0, cc.GetLeapMonth (2005), "#5-6");
		Assert.AreEqual (8, cc.GetLeapMonth (2006), "#5-7");
		Assert.AreEqual (0, cc.GetLeapMonth (2007), "#5-8");
		Assert.AreEqual (0, cc.GetLeapMonth (2008), "#5-9");
		Assert.AreEqual (6, cc.GetLeapMonth (2009), "#5-10");
		Assert.AreEqual (0, cc.GetLeapMonth (2010), "#5-11");
		Assert.AreEqual (0, cc.GetLeapMonth (2011), "#5-12");
		Assert.AreEqual (5, cc.GetLeapMonth (2012), "#5-13");
		Assert.AreEqual (0, cc.GetLeapMonth (2013), "#5-14");
		Assert.AreEqual (10, cc.GetLeapMonth (2014), "#5-15");
		Assert.AreEqual (0, cc.GetLeapMonth (2015), "#5-16");
		Assert.AreEqual (0, cc.GetLeapMonth (2016), "#5-17");
		Assert.AreEqual (7, cc.GetLeapMonth (2017), "#5-18");
		Assert.AreEqual (0, cc.GetLeapMonth (2018), "#5-19");
		Assert.AreEqual (0, cc.GetLeapMonth (2019), "#5-20");

		KoreanLunisolarCalendar kc = new KoreanLunisolarCalendar ();
		Assert.AreEqual (0, kc.GetLeapMonth (2000), "#6-1");
		Assert.AreEqual (5, kc.GetLeapMonth (2001), "#6-2");
		Assert.AreEqual (0, kc.GetLeapMonth (2002), "#6-3");
		Assert.AreEqual (0, kc.GetLeapMonth (2003), "#6-4");
		Assert.AreEqual (3, kc.GetLeapMonth (2004), "#6-5");
		Assert.AreEqual (0, kc.GetLeapMonth (2005), "#6-6");
		Assert.AreEqual (8, kc.GetLeapMonth (2006), "#6-7");
		Assert.AreEqual (0, kc.GetLeapMonth (2007), "#6-8");
		Assert.AreEqual (0, kc.GetLeapMonth (2008), "#6-9");
		Assert.AreEqual (6, kc.GetLeapMonth (2009), "#6-10");
		Assert.AreEqual (0, kc.GetLeapMonth (2010), "#6-11");
		Assert.AreEqual (0, kc.GetLeapMonth (2011), "#6-12");
		Assert.AreEqual (4, kc.GetLeapMonth (2012)); // off from cn by 1, "#6-13");
		Assert.AreEqual (0, kc.GetLeapMonth (2013), "#6-14");
		Assert.AreEqual (10, kc.GetLeapMonth (2014), "#6-15");
		Assert.AreEqual (0, kc.GetLeapMonth (2015), "#6-16");
		Assert.AreEqual (0, kc.GetLeapMonth (2016), "#6-17");
		Assert.AreEqual (6, kc.GetLeapMonth (2017)); // off from cn by 1, "#6-18");
		Assert.AreEqual (0, kc.GetLeapMonth (2018), "#6-19");
		Assert.AreEqual (0, kc.GetLeapMonth (2019), "#6-20");
	}

	[Test]
	public void GetWeekOfYear ()
	{
		GregorianCalendar gc = new GregorianCalendar ();
		Assert.AreEqual (1, gc.GetWeekOfYear (new DateTime (2007, 1, 1), CalendarWeekRule.FirstDay, DayOfWeek.Sunday), "#1");
		//Assert.AreEqual (1, gc.GetWeekOfYear (new DateTime (2000, 1, 1), CalendarWeekRule.FirstDay, DayOfWeek.Sunday), "#2");
		Assert.AreEqual (3, gc.GetWeekOfYear (new DateTime (2000, 1, 10), CalendarWeekRule.FirstDay, DayOfWeek.Sunday), "#2");
		Assert.AreEqual (2, gc.GetWeekOfYear (new DateTime (2000, 1, 10), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday), "#3");
		Assert.AreEqual (2, gc.GetWeekOfYear (new DateTime (2000, 1, 10), CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday), "#4");
		Assert.AreEqual (52, gc.GetWeekOfYear (new DateTime (2000, 1, 1), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday), "#5");
		Assert.AreEqual (52, gc.GetWeekOfYear (new DateTime (2000, 1, 1), CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday), "#6");
	}

	[Test]
	public void TestToFourDigitYear() {
		foreach (Calendar cal in acal) {
			bool working = true;
			int mod = 2000;
			if (cal is HebrewCalendar)
				mod = 5500; 
			if (cal is KoreanCalendar)
				mod = 3000;
			if (cal is JapaneseCalendar)
				working = false;
			if (cal is TaiwanCalendar)
				working = false;
			cal.TwoDigitYearMax = mod + 229;
			Assert.AreEqual(mod+229 , cal.TwoDigitYearMax,
						 String.Format("F01 {0}.TwoDigitYearMax", cal));
			Assert.AreEqual(working ? mod+229 : 29,
						 cal.ToFourDigitYear(29),
						 String.Format("F02 {0}.ToFourDigitYear(29)",cal));
			Assert.AreEqual(
				working ? mod+130 : 30,
				cal.ToFourDigitYear(30),
				String.Format("F03 {0}.ToFourDigitYear(30)",
							  cal));
			Assert.AreEqual(mod, cal.ToFourDigitYear(mod),
				String.Format("F04 {0}.ToFourDigitYear({1})",
							  cal, mod));

			bool exception = false;
			try {
				cal.ToFourDigitYear(-1);
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}
			Assert.IsTrue(exception, String.Format(
				"F05 {0}.ToFourDigitYear(-1) exception",
				cal));
			exception = false;
			try {
				cal.ToFourDigitYear(15000);
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}
			Assert.IsTrue(exception, String.Format(
				"F05 {0}.ToFourDigitYear(15000) exception",
				cal));
		}
	}

	[Test]
	public void TestToFourDigitYear2 ()
	{
		GregorianCalendar gc = new GregorianCalendar ();
		Assert.AreEqual (2029, gc.ToFourDigitYear (29), "#1-1");
		Assert.AreEqual (1930, gc.ToFourDigitYear (30), "#1-2");
		Assert.AreEqual (2029, gc.ToFourDigitYear (2029), "#1-3");
		Assert.AreEqual (2030, gc.ToFourDigitYear (2030), "#1-4");

		HebrewCalendar hbc = new HebrewCalendar ();
		Assert.AreEqual (5790, hbc.ToFourDigitYear (90), "#2-1");
		Assert.AreEqual (5691, hbc.ToFourDigitYear (91), "#2-2");
		Assert.AreEqual (5790, hbc.ToFourDigitYear (5790), "#2-3");
		Assert.AreEqual (5691, hbc.ToFourDigitYear (5691), "#2-4");
		Assert.AreEqual (5999, hbc.ToFourDigitYear (5999), "#2-5");
		// LAMESPEC: .NET fails to throw an exception unlike documented
		/*
		try {
			hbc.ToFourDigitYear (6000);
			Assert.Fail ("#2-6");
		} catch (ArgumentOutOfRangeException) {
		}
		*/

		ThaiBuddhistCalendar tc = new ThaiBuddhistCalendar ();
		Assert.AreEqual (2572, tc.ToFourDigitYear (72), "#3-1");
		Assert.AreEqual (2473, tc.ToFourDigitYear (73), "#3-2");
		Assert.AreEqual (2572, tc.ToFourDigitYear (2572), "#3-3");
		Assert.AreEqual (2573, tc.ToFourDigitYear (2573), "#3-4");
		Assert.AreEqual (9999, tc.ToFourDigitYear (9999), "#3-5");
		// LAMESPEC: .NET fails to throw an exception unlike documented
		/*
		try {
			tc.ToFourDigitYear (10000);
			Assert.Fail ("#3-6");
		} catch (ArgumentOutOfRangeException) {
		}
		*/

		KoreanCalendar kc = new KoreanCalendar ();
		Assert.AreEqual (4362, kc.ToFourDigitYear (62), "#4-1");
		Assert.AreEqual (4263, kc.ToFourDigitYear (63), "#4-2");
		Assert.AreEqual (4362, kc.ToFourDigitYear (4362), "#4-3");
		Assert.AreEqual (4363, kc.ToFourDigitYear (4363), "#4-4");
	}

	public void TestDaysInYear (Calendar calendar, int year)
	{
		var daysInYear = calendar.GetDaysInYear (year);
		var daysInMonths = 0;
		var monthInYear = calendar.GetMonthsInYear (year);
		for (var m = 1; m <= monthInYear; m++)
			daysInMonths += calendar.GetDaysInMonth (year, m);

		Assert.AreEqual (daysInYear, daysInMonths, string.Format("Calendar:{0} Year:{1}",calendar.GetType(), year));
	}

	[Test]
	public void DaysInYear ()
	{
		var calendars = new List<Calendar> (acal) {
			new UmAlQuraCalendar ()
		};

		foreach (var calendar in calendars) {
			var minYear = calendar.GetYear (calendar.MinSupportedDateTime);
			var maxYear = calendar.GetYear (calendar.MaxSupportedDateTime) - 1 ;
			var midYear = calendar.GetYear (DateTime.Now);
			var yearsTested = Math.Min (1000, (maxYear - minYear) / 2);

			midYear -= yearsTested / 2;

			int y1 = minYear, y2 = maxYear, y3 = midYear;
			for (var i = 0; i < yearsTested; i++) {
				TestDaysInYear (calendar, y1);
				TestDaysInYear (calendar, y2);
				if (y3 > minYear && y3 < maxYear)
					TestDaysInYear (calendar, y3);

				y1++; y2--; y3++;
			}
		}
	}


	[Test]
	public void TestJapaneseCalendarDateParsing ()
	{
		CultureInfo ciJapanese = new CultureInfo ("ja-JP") { DateTimeFormat = { Calendar = new JapaneseCalendar () } };

		DateTime dt = new DateTime (1970, 1, 1);
		string eraName = dt.ToString ("gg", ciJapanese);
		Assert.AreEqual (new DateTime (1995, 1, 1), DateTime.Parse (eraName + " 70/1/1 0:00:00", ciJapanese));
	}

	// TODO: more tests :-)
} // class CalendarTest
	
} // namespace MonoTests.System.Globalization
