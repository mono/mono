// CalendarTest.cs
//
// (C) 2002 Ulrich Kunitz
//

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

public class CalendarTest : TestCase {
	private Calendar[] acal;
	private GregorianCalendar gcal;
	private JulianCalendar jucal;
	private HijriCalendar hical;
	private HebrewCalendar hecal;
	private JapaneseCalendar jacal;
	private TaiwanCalendar tacal;
	private KoreanCalendar kcal;
	private ThaiBuddhistCalendar tbcal;

	protected override void SetUp() {
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
			AssertEquals(sw.ToString(), timeG, time);

			sw = new StringWriter();
			Date ndate = new Date();
			ndate.FromDateTime(cal, time);
			sw.Write("Calendar {0} computes wrong date", cal);
			AssertEquals(sw.ToString(), date, ndate);
		}
	}

	// We are testing the implementation against the reference dates in
	// Calendrical Calcualation. Please note that the CLR uses another
	// epoch for the HijriCalendar, which might be the perfect thing
	// to do.
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
			new Date(10,11,50,4),
			new Date(10,11,127,1),
			new Date(10,11,4371,1),
			new Date(10,11,2581,1));
		RowCheck(new Date(18,7,2094,1),
			new Date(5,7,2094,1),
			new Date(6,3,1518,1),
			new Date(5,11,5854,1),
			new Date(18,7,106,4),
			new Date(18,7,183,1),
			new Date(18,7,4427,1),
			new Date(18,7,2637,1));
	}

	public void TestCalendarType() {
		GregorianCalendar gc = new GregorianCalendar(
			GregorianCalendarTypes.Arabic);
		AssertEquals("A01 Gregorian ctor " +
			"with GregorianCalendarTypes parameter",
			GregorianCalendarTypes.Arabic,
			gc.CalendarType);
		gc.CalendarType = GregorianCalendarTypes.MiddleEastFrench;
		AssertEquals("A02 GregorianCalendar.CalendarType",
			GregorianCalendarTypes.MiddleEastFrench,
			gc.CalendarType);
	}

	public void TestStandardEras() {
		AssertEquals("B01 ADEra", 1, GregorianCalendar.ADEra);
		AssertEquals("B02 HebrewEra", 1, HebrewCalendar.HebrewEra);
		AssertEquals("B03 HjriEra", 1, HijriCalendar.HijriEra);
		AssertEquals("B04 JulianEra", 1, JulianCalendar.JulianEra);
		AssertEquals("B05 KoreanEra", 1, KoreanCalendar.KoreanEra);
		AssertEquals("B06 ThaiBuddhistEra", 1,
			ThaiBuddhistCalendar.ThaiBuddhistEra);
	}

	public void TestCurrentEra() {
		AssertEquals("C01 GregorianCalendar.CurrentEra",
			0, GregorianCalendar.CurrentEra);
		AssertEquals("C02 HebrewCalendar.CurrentEra",
			0, HebrewCalendar.CurrentEra);
		AssertEquals("C03 HijriCalendar.CurrentEra",
			0, HijriCalendar.CurrentEra);
		AssertEquals("C04 JapaneseCalendar.CurrentEra",
			0, JapaneseCalendar.CurrentEra);
		AssertEquals("C05 JulianCalendar.CurrentEra",
			0, JulianCalendar.CurrentEra);
		AssertEquals("C06 KoreanCalendar.CurrentEra",
			0, KoreanCalendar.CurrentEra);
		AssertEquals("C07 TaiwanCalendar.CurrentEra",
			0, TaiwanCalendar.CurrentEra);
		AssertEquals("C08 ThaiBuddhistCalendar.CurrentEra",
			0,
			ThaiBuddhistCalendar.CurrentEra);
	}

	public void TestErasProperty() {
		foreach (Calendar cal in acal) {
			int check = 1;
			if (cal is JapaneseCalendar)
				check = 4;
			AssertEquals(String.Format("D01 {0}.Eras.Length", cal),
				check, cal.Eras.Length);
			cal.Eras[0] = 29;
			Assert(String.Format("D02 {0}.Eras readonly", cal),
				cal.Eras[0] != 29);
		}
	}

	public void TestTwoDigitYearMax() {
		AssertEquals("E01 TwoDigitYearMax GregorianCalendar",
			2029, gcal.TwoDigitYearMax);
		AssertEquals("E02 TwoDigitYearMax HebrewCalendar",
			5790, hecal.TwoDigitYearMax);
		AssertEquals("E03 TwoDigitYearMax HijriCalendar",
			1451, hical.TwoDigitYearMax);
		AssertEquals("E04 TwoDigitYearMax JapaneseCalendar",
			99, jacal.TwoDigitYearMax);
		AssertEquals("E05 TwoDigitYearMax JulianCalendar",
			2029, jucal.TwoDigitYearMax);
		AssertEquals("E06 TwoDigitYearMax KoreanCalendar",
			4362, kcal.TwoDigitYearMax);
		AssertEquals("E07 TwoDigitYearMax TaiwanCalendar",
			99, tacal.TwoDigitYearMax);
		AssertEquals("E08 TwoDigitYearMax ThaiBuddhistCalendar",
			2572, tbcal.TwoDigitYearMax);
		foreach (Calendar cal in acal) {
			bool exception = false;
			try { 
				cal.TwoDigitYearMax = 99;
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}
			Assert(String.Format("E09 {0}.TwoDigitYearMax 99 " +
					" out of range exception", cal),
				exception);

			exception = false;
			int m = 10000;
			try {
				m = cal.GetYear(DateTime.MaxValue)+1;
				cal.TwoDigitYearMax = m;
			}
			catch (ArgumentException) {
				exception = true;
			}
			Assert(String.Format("E10 {0}.TwoDigitYearMax out " +
					" of range exception value {1}",
					cal, m),
				exception);
		}
	}

	/* UK TODO: breaks with current DateTime implementation.
	 * I've a newer one that works, but that requires to much changes.
	 * for now.
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
			AssertEquals(
				String.Format("F01 {0}.TwoDigitYearMax", cal),
					mod+229 , cal.TwoDigitYearMax);
			AssertEquals(
				String.Format("F02 {0}.ToFourDigitYear(29)",
					cal),
				working ? mod+229 : 29,
				cal.ToFourDigitYear(29));
			AssertEquals(
				String.Format("F03 {0}.ToFourDigitYear(30)",
					cal),
				working ? mod+130 : 30,
				cal.ToFourDigitYear(30));
			AssertEquals(
				String.Format("F04 {0}.ToFourDigitYear({1})",
					cal, mod),
				mod, cal.ToFourDigitYear(mod));
			bool exception = false;
			try {
				cal.ToFourDigitYear(-1);
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}
			Assert(String.Format(
				"F05 {0}.ToFourDigitYear(-1) exception",
				cal), exception);
			exception = false;
			try {
				cal.ToFourDigitYear(15000);
			}
			catch (ArgumentOutOfRangeException) {
				exception = true;
			}
			Assert(String.Format(
				"F05 {0}.ToFourDigitYear(15000) exception",
				cal), exception);
		}
	}
	*/

	// TODO: more tests :-)
} // class CalendarTest
	
} // namespace MonoTests.System.Globalization
