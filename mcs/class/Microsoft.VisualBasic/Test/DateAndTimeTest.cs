// Collection.cs - NUnit Test Cases for Microsoft.VisualBasic.Collection
//
// Author:
//   Chris J. Breisch (cjbreisch@altavista.net)
//
// (C) Chris J. Breisch
// 

using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;
using Microsoft.VisualBasic;

namespace MonoTests.Microsoft.VisualBasic
{

	public class DateAndTimeTest : TestCase {

		private CultureInfo oldcult;
	
		public DateAndTimeTest() : base ("Microsoft.VisualBasic.DateAndTime") {}
		public DateAndTimeTest(string name) : base(name) {}

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

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(DateAndTimeTest)); 
			}
		}

		public void TestDateString() {
			string s = DateAndTime.DateString;
			DateTime dtNow = DateTime.Today;
			AssertEquals("#DS01", dtNow.ToShortDateString(), DateTime.Parse(s).ToShortDateString());

			// TODO: Add a test for setting the date string too
		}

		public void TestToday() {
			AssertEquals("#TO01", DateTime.Today, DateAndTime.Today);

			// TODO: Add a test for setting Today
		}

		public void TestTimer() {
			double secTimer = DateAndTime.Timer;
			DateTime dtNow = DateTime.Now;
			double secNow = dtNow.Hour * 3600 + dtNow.Minute * 60 + dtNow.Second + (dtNow.Millisecond + 1) / 1000D;
			double secTimer2 = DateAndTime.Timer + .001;
			
			// waste a little time
			for (int i = 0; i < int.MaxValue; i++);

			// get another timer
			double secTimer3 = DateAndTime.Timer;
			
			// should be same time within a reasonable tolerance
			Assert("#TI01", secNow >= secTimer);
			Assert("#TI02: SecTimer2=" + secTimer2 + " secNow=" + secNow, secTimer2 >= secNow);

			// third timer should be greater than the first
			Assert("#TI03", secTimer3 > secTimer);
		}

		public void TestNow() {
			DateTime dtNow = DateTime.Now;
			DateTime dtTest = DateAndTime.Now;
			DateTime dtNow2 = DateTime.Now;

			Assert("#N01", dtTest >= dtNow);
			Assert("#N02", dtNow2 >= dtTest);
		}

		public void TestTimeOfDay() {
			DateTime dtNow = DateTime.Now;
			TimeSpan tsNow = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
			DateTime dtTest = DateAndTime.TimeOfDay;
			TimeSpan tsTest = new TimeSpan(dtTest.Hour, dtTest.Minute, dtTest.Second);
			DateTime dtNow2 = DateTime.Now;
			TimeSpan tsNow2 = new TimeSpan(dtNow2.Hour, dtNow2.Minute, dtNow2.Second);
			
			Assert("#TOD01", tsTest.Ticks >= tsNow.Ticks);
			Assert("#TOD02", tsNow2.Ticks >= tsTest.Ticks);

			// TODO: add a test case for setting time of day
		}

		public void TestTimeString() {
			DateTime dtNow = DateTime.Now;
			TimeSpan tsNow = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
			string s = DateAndTime.TimeString;
			DateTime dtTest = DateTime.Parse(s);
			TimeSpan tsTest = new TimeSpan(dtTest.Hour, dtTest.Minute, dtTest.Second);
			DateTime dtNow2 = DateTime.Now;
			TimeSpan tsNow2 = new TimeSpan(dtNow2.Hour, dtNow2.Minute, dtNow2.Second);
			
			Assert("#TS01", tsTest.Ticks >= tsNow.Ticks);
			Assert("#TS02", tsNow2.Ticks >= tsTest.Ticks);

			// TODO: add a test case for setting TimeString
		}

		public void TestDateAdd() {
			DateTime dtNow = DateTime.Now;

			AssertEquals("#DA01", dtNow.AddYears(1), DateAndTime.DateAdd(DateInterval.Year, 1, dtNow));
			AssertEquals("#DA02", dtNow.AddYears(-1), DateAndTime.DateAdd("yyyy", -1, dtNow));


			bool caughtException = false;
			
			try {
				DateAndTime.DateAdd("foo", 1, dtNow);
			} 
			catch (Exception e) {
				AssertEquals("#DA03", e.GetType(), typeof(ArgumentException));
				caughtException = true;
			}

			AssertEquals("#DA04", caughtException, true);

			AssertEquals("#DA05", dtNow.AddMonths(6), DateAndTime.DateAdd(DateInterval.Quarter, 2, dtNow));
			AssertEquals("#DA06", dtNow.AddMonths(-6), DateAndTime.DateAdd("q", -2, dtNow));

			AssertEquals("#DA07", dtNow.AddMonths(3), DateAndTime.DateAdd(DateInterval.Month, 3, dtNow));
			AssertEquals("#DA08", dtNow.AddMonths(-3), DateAndTime.DateAdd("m", -3, dtNow));

			AssertEquals("#DA09", dtNow.AddDays(28), DateAndTime.DateAdd(DateInterval.WeekOfYear, 4, dtNow));
			AssertEquals("#DA10", dtNow.AddDays(-28), DateAndTime.DateAdd("ww", -4, dtNow));

			AssertEquals("#DA11", dtNow.AddDays(5), DateAndTime.DateAdd(DateInterval.Weekday, 5, dtNow));
			AssertEquals("#DA12", dtNow.AddDays(-5), DateAndTime.DateAdd("w", -5, dtNow));

			AssertEquals("#DA13", dtNow.AddDays(6), DateAndTime.DateAdd(DateInterval.DayOfYear, 6, dtNow));
			AssertEquals("#DA14", dtNow.AddDays(-6), DateAndTime.DateAdd("y", -6, dtNow));

			AssertEquals("#DA15", dtNow.AddDays(7), DateAndTime.DateAdd(DateInterval.Day, 7, dtNow));
			AssertEquals("#DA16", dtNow.AddDays(-7), DateAndTime.DateAdd("d", -7, dtNow));

			AssertEquals("#DA17", dtNow.AddHours(8), DateAndTime.DateAdd(DateInterval.Hour, 8, dtNow));
			AssertEquals("#DA18", dtNow.AddHours(-8), DateAndTime.DateAdd(DateInterval.Hour, -8, dtNow));

			AssertEquals("#DA19", dtNow.AddMinutes(9), DateAndTime.DateAdd(DateInterval.Minute, 9, dtNow));
			AssertEquals("#DA20", dtNow.AddMinutes(-9), DateAndTime.DateAdd("n", -9, dtNow));

			AssertEquals("#DA21", dtNow.AddSeconds(10), DateAndTime.DateAdd(DateInterval.Second, 10, dtNow));
			AssertEquals("#DA22", dtNow.AddSeconds(-10), DateAndTime.DateAdd("s", -10, dtNow));

			caughtException = false;

			try {
				DateAndTime.DateAdd(DateInterval.Year, int.MinValue, dtNow);
			}
			catch (Exception e) {
				caughtException = true;
				AssertEquals("#DA23", e.GetType(), typeof(Exception));
			}

			// AssertEquals("#DA24", caughtException, true);
		}

		public void TestDateDiff () {
			DateTime dtNow = DateTime.Now;
			DateTime dtOld = dtNow.AddYears(-1);

			// TODO: Test this better
			long diff = DateAndTime.DateDiff(DateInterval.Year, dtOld, dtNow, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals("#DD01", dtNow, dtOld.AddYears((int)diff));

			DateTime dtJan1 = new DateTime(2002, 1, 1);
			DateTime dtDec31 = new DateTime(2001, 12, 31);

			diff = DateAndTime.DateDiff(DateInterval.Year, dtDec31, dtJan1, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals("#DD02", 1L, diff);

			diff = DateAndTime.DateDiff(DateInterval.Quarter, dtDec31, dtJan1, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals("#DD03", 1L, diff);

			diff = DateAndTime.DateDiff(DateInterval.Month, dtDec31, dtJan1, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals("#DD04", 1L, diff);

			DateTime dtJan4 = new DateTime(2001, 1, 4);	// This is a Thursday
			DateTime dtJan9 = new DateTime(2001, 1, 9);	// This is the next Tuesday
			
			
			long WD = DateAndTime.DateDiff(DateInterval.Weekday, dtJan4, dtJan9, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals ("#DD05", 0L, WD);

			long WY = DateAndTime.DateDiff(DateInterval.WeekOfYear, dtJan4, dtJan9, FirstDayOfWeek.System, FirstWeekOfYear.System);

			AssertEquals ("#DD06", 1L, WY);
		}

		public void TestDatePart () {
			DateTime dtJan4 = new DateTime(2001, 1, 4);

			// TODO: Test this better

			AssertEquals("#DP01", 2001, DateAndTime.DatePart(DateInterval.Year, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.System));
			AssertEquals("#DP02", 1, DateAndTime.DatePart(DateInterval.Quarter, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.System));
			AssertEquals("#DP03", 1, DateAndTime.DatePart(DateInterval.Month, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.System));
			AssertEquals("#DP04", 1, DateAndTime.DatePart(DateInterval.WeekOfYear, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP05", 53, DateAndTime.DatePart(DateInterval.WeekOfYear, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.FirstFullWeek));
			AssertEquals("#DP06", 1, DateAndTime.DatePart(DateInterval.WeekOfYear, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.Jan1));
			AssertEquals("#DP07", 1, DateAndTime.DatePart(DateInterval.WeekOfYear, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.System));
			AssertEquals("#DP08", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Friday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP09", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Saturday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP10", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Sunday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP11", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Monday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP12", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Tuesday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP13", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Wednesday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP14", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.Thursday, FirstWeekOfYear.FirstFourDays));
			AssertEquals("#DP15", 5, DateAndTime.DatePart(DateInterval.Weekday, dtJan4, FirstDayOfWeek.System, FirstWeekOfYear.FirstFourDays));


		}

		public void TestDateSerial () {
			DateTime dtJan4 = new DateTime(2001, 1, 4);
			DateTime dtSerial = DateAndTime.DateSerial(2001, 1, 4);

			AssertEquals("#DS01", dtJan4, dtSerial);
		}

		public void TestTimeSerial () {
			bool caughtException = false;

			try {
				DateAndTime.TimeSerial(0, -1440, -1);
			}
			catch (Exception e) {
				AssertEquals("#TS01", e.GetType(), typeof(ArgumentOutOfRangeException));
				caughtException = true;
			}
			AssertEquals("#TS02", true, caughtException);

			AssertEquals("#TS03", new DateTime(1, 1, 1, 1, 1, 1), DateAndTime.TimeSerial(1, 1, 1));
				
		}

		public void TestDateValue () {
			try {
				DateAndTime.DateValue("This is not a date.");
			}
			catch (FormatException) {
				/* do nothing.  this is what we expect */
			}
			catch (Exception e) {
				Fail ("Unexpected exception:" + e);
			}
			AssertEquals("#DV03", new DateTime(1969, 2, 12), DateAndTime.DateValue("February 12, 1969"));
		}

		public void TestTimeValue () {
			try {
				DateAndTime.TimeValue("This is not a time.");
			}
			catch (FormatException) {
				/* do nothing.  this is what we expect */
			}
			catch (Exception e) {
				Fail ("Unexpected exception:" + e);
			}
			AssertEquals("#TV03", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 35, 17), DateAndTime.TimeValue("4:35:17 PM"));
		}

		public void TestYear () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#Y01", jan1.Year, DateAndTime.Year(jan1));
		}

		public void TestMonth () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#MO01", jan1.Month, DateAndTime.Month(jan1));
		}
		
		public void TestDay () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#D01", jan1.Day, DateAndTime.Day(jan1));
		}

		public void TestHour () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#H01", jan1.Hour, DateAndTime.Hour(jan1));
		}

		public void TestMinute () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#MI01", jan1.Minute, DateAndTime.Minute(jan1));
		}

		public void TestSecond () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#S01", jan1.Second, DateAndTime.Second(jan1));
		}

		public void TestWeekday () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#W01", (int)jan1.DayOfWeek + 1, DateAndTime.Weekday(jan1, FirstDayOfWeek.System));
		}

		public void TestMonthName () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#MN01", CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(jan1.Month),
				DateAndTime.MonthName(jan1.Month, true));
			AssertEquals("#MN02", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(jan1.Month),
				DateAndTime.MonthName(jan1.Month, false));

			bool caughtException = false;

			try {
				DateAndTime.MonthName(0, false);
			}
			catch (Exception e) {
				AssertEquals("#MN03", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}
			AssertEquals("#MN04", true, caughtException);

			caughtException = false;
			
			try {
				DateAndTime.MonthName(14, false);
			}
			catch (Exception e) {
				AssertEquals("#MN05", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}
			AssertEquals("#MN06", true, caughtException);

			//AssertEquals("#MN07", "", DateAndTime.MonthName(13, false));
		}

		public void TestWeekdayName () {
			DateTime jan1 = new DateTime(2001, 1, 1, 1, 1, 1);
			AssertEquals("#WN01", "Tue",
				DateAndTime.WeekdayName((int)jan1.DayOfWeek + 1, true, FirstDayOfWeek.System));
			AssertEquals("#WN02", "Tuesday",
				DateAndTime.WeekdayName((int)jan1.DayOfWeek + 1, false, FirstDayOfWeek.System));

			bool caughtException = false;

			try {
				DateAndTime.WeekdayName(0, false, FirstDayOfWeek.System);
			}
			catch (Exception e) {
				AssertEquals("#WN03", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}
			AssertEquals("#WN04", true, caughtException);

			caughtException = false;
			
			try {
				DateAndTime.WeekdayName(8, false, FirstDayOfWeek.System);
			}
			catch (Exception e) {
				AssertEquals("#WN05", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}
			AssertEquals("#WN06", true, caughtException);

			AssertEquals("#WN07", "Tuesday", DateAndTime.WeekdayName((int)jan1.DayOfWeek + 1, false, FirstDayOfWeek.System));
		}


		


	}
}
