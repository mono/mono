//
// DateAndTime.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class DateAndTime {
		public static string DateString {
			get { 
				return DateTime.Today.ToString("MM-dd-yyyy");
			}
			
			set { 
				string [] formats = { "M-d-yyyy", "M-d-y", "M/d/yyyy", "M/d/y" };

				try {
					DateTime dtToday = DateTime.ParseExact(value, formats,
						DateTimeFormatInfo.CurrentInfo,
						DateTimeStyles.None);
				
					Today = dtToday;
				}
				catch {
					throw new InvalidCastException();
				}
			} 
		}

		public static System.DateTime Today {
			get { return DateTime.Today; }
			set { 
				// FIXME: This needs to use some OS specific code
				//	I've already written it for Windows
				//	and Unix won't be hard, but need an
				//	OS object from the compiler
				//	OS specific code needs to check permissions
				//	too, and throw an ArgumentOutOfRangeException
				//	if no permissions
//				DateTime dtNow = DateTime.Now;
//
//				SysTime.LocalTime = new DateTime(value.Year,
//					value.Month, value.Day, dtNow.Hour,
//					dtNow.Minute, dtNow.Second, dtNow.Millisecond);
				throw new NotImplementedException();
			} 
		}

		public static double Timer {  
			get { 
				DateTime DTNow = DateTime.Now;

				return DTNow.Hour * 3600 + DTNow.Minute * 60 +
					DTNow.Second + DTNow.Millisecond / 
					1000D;
			} 
		}

		public static System.DateTime Now {
			get { return DateTime.Now; }
		}

		public static System.DateTime TimeOfDay {  
			get { 
				TimeSpan TSpan = DateTime.Now.TimeOfDay;

				return new DateTime(1, 1, 1, TSpan.Hours, 
					TSpan.Minutes, TSpan.Seconds, 
					TSpan.Milliseconds); 
			}
			set { 
				// FIXME: This needs to use some OS specific code
				//	I've already written it for Windows
				//	and Unix won't be hard, but need an
				//	OS object from the compiler
				//	OS specific code needs to check permissions
				//	too, and throw an ArgumentOutOfRangeException
				//	if no permissions
//				DateTime dtToday = DateTime.Today;
//
//				SysTime.LocalTime = new DateTime(dtToday.Year,
//					dtToday.Month, dtToday.Day, value.Hour, 
//					value.Minute, value.Second, value.Millisecond);
				throw new NotImplementedException();
			} 
		}

		public static string TimeString {  
			get { return DateTime.Now.ToString("HH:mm:ss"); } 
			set { 
				string format = "HH:mm:ss";

				try {
					DateTime dtToday = DateTime.ParseExact(value, format,
						DateTimeFormatInfo.CurrentInfo,
						DateTimeStyles.None);
				
					TimeOfDay = dtToday;
				}
				catch {
					throw new InvalidCastException();
				} 
			} 
		}

		// Methods
		public static System.DateTime DateAdd (DateInterval Interval, 
			double Number, System.DateTime DateValue) {

			switch (Interval) {
				case DateInterval.Year:
					return DateValue.AddYears((int)Number);
				case DateInterval.Quarter:
					return DateValue.AddMonths((int)Number * 3);
				case DateInterval.Month:
					return DateValue.AddMonths((int)Number);
				case DateInterval.WeekOfYear:
					return DateValue.AddDays(Number * 7);
				case DateInterval.Day:
				case DateInterval.DayOfYear:
				case DateInterval.Weekday:
					return DateValue.AddDays(Number);
				case DateInterval.Hour:
					return DateValue.AddHours(Number);
				case DateInterval.Minute:
					return DateValue.AddMinutes(Number);
				case DateInterval.Second:
					return DateValue.AddSeconds(Number);
				default:
					throw new ArgumentException();
			}
		}

		private static DayOfWeek GetDayRule(FirstDayOfWeek StartOfWeek, DayOfWeek DayRule) 
		{
			switch (StartOfWeek) {
				case FirstDayOfWeek.System:
					return DayRule;
				case FirstDayOfWeek.Sunday:
					return DayOfWeek.Sunday;
				case FirstDayOfWeek.Monday:
					return DayOfWeek.Monday;
				case FirstDayOfWeek.Tuesday:
					return DayOfWeek.Tuesday;
				case FirstDayOfWeek.Wednesday:
					return DayOfWeek.Wednesday;
				case FirstDayOfWeek.Thursday:
					return DayOfWeek.Thursday;
				case FirstDayOfWeek.Friday:
					return DayOfWeek.Friday;
				case FirstDayOfWeek.Saturday:
					return DayOfWeek.Saturday;
				default:
					throw new ArgumentException();
			}
		}

		private static CalendarWeekRule GetWeekRule(FirstWeekOfYear StartOfYear, CalendarWeekRule WeekRule) 
		{
			switch (StartOfYear) {
				case FirstWeekOfYear.System:
					return WeekRule;
				case FirstWeekOfYear.FirstFourDays:
					return CalendarWeekRule.FirstFourDayWeek;
				case FirstWeekOfYear.FirstFullWeek:
					return CalendarWeekRule.FirstFullWeek;
				case FirstWeekOfYear.Jan1:
					return CalendarWeekRule.FirstDay;
				default:
					throw new ArgumentException();
			}
		}
		
		public static long DateDiff (DateInterval Interval, 
			System.DateTime Date1, System.DateTime Date2, 
			[Optional] [DefaultValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional] [DefaultValue(FirstWeekOfYear.Jan1)] 
			FirstWeekOfYear StartOfYear) 
		{
			
			int YearMonths;
			int YearQuarters;
			int YearWeeks;
			CalendarWeekRule WeekRule = CalendarWeekRule.FirstDay;
			DayOfWeek DayRule = DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			Calendar CurCalendar = CultureInfo.CurrentCulture.Calendar;

			switch (Interval) {
				case DateInterval.Year:
					return Date2.Year - Date1.Year;
				case DateInterval.Quarter:
					YearQuarters = (Date2.Year - Date1.Year) * 4;
					return Date2.Month / 4 - Date1.Month / 4 + YearQuarters;
				case DateInterval.Month:
					YearMonths = (Date2.Year - Date1.Year) * 12;
					return Date2.Month - Date1.Month + YearMonths;
				case DateInterval.WeekOfYear:
					YearWeeks = (Date2.Year - Date1.Year) * 53;
					DayRule = GetDayRule(StartOfWeek, DayRule);
					WeekRule = GetWeekRule(StartOfYear, WeekRule);
					return CurCalendar.GetWeekOfYear(Date2,	WeekRule, DayRule) -
						CurCalendar.GetWeekOfYear(Date1,WeekRule, DayRule) + 
						YearWeeks;
				case DateInterval.Weekday:
					return ((TimeSpan)(Date2.Subtract(Date1))).Days / 7;
				case DateInterval.DayOfYear:
				case DateInterval.Day:
					return ((TimeSpan)(Date2.Subtract(Date1))).Days;
				case DateInterval.Hour:
					return ((TimeSpan)(Date2.Subtract(Date1))).Hours;
				case DateInterval.Minute:
					return ((TimeSpan)(Date2.Subtract(Date1))).Minutes;
				case DateInterval.Second:
					return ((TimeSpan)(Date2.Subtract(Date1))).Seconds;
				default:
					throw new ArgumentException();
			}
		}

		private static int ConvertWeekDay(DayOfWeek Day, int Offset) 
		{

			int Weekday = (int)Day + Offset;

			if (Weekday > 7) {
				Weekday -= 7;
			}

			switch((DayOfWeek)Weekday) {
				case DayOfWeek.Sunday:
					return (int)FirstDayOfWeek.Sunday;
				case DayOfWeek.Monday:
					return (int)FirstDayOfWeek.Monday;
				case DayOfWeek.Tuesday:
					return (int)FirstDayOfWeek.Tuesday;
				case DayOfWeek.Wednesday:
					return (int)FirstDayOfWeek.Wednesday;
				case DayOfWeek.Thursday:
					return (int)FirstDayOfWeek.Thursday;
				case DayOfWeek.Friday:
					return (int)FirstDayOfWeek.Friday;
				case DayOfWeek.Saturday:
					return (int)FirstDayOfWeek.Saturday;
				default:
					throw new ArgumentException();
			}

		}

		public static int DatePart 
		(
			Microsoft.VisualBasic.DateInterval Interval, 
			System.DateTime DateValue, 
			[Optional] [DefaultValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional] [DefaultValue(FirstWeekOfYear.Jan1)] 
			FirstWeekOfYear StartOfYear) {
			
			CalendarWeekRule WeekRule = CalendarWeekRule.FirstDay;
			DayOfWeek DayRule = DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			Calendar CurCalendar = CultureInfo.CurrentCulture.Calendar;

			switch (Interval) {
				case DateInterval.Year:
					return DateValue.Year;
				case DateInterval.Quarter:
					return DateValue.Month / 4 + 1;
				case DateInterval.Month:
					return DateValue.Month;
				case DateInterval.WeekOfYear:
					DayRule = GetDayRule(StartOfWeek, DayRule);
					WeekRule = GetWeekRule(StartOfYear, WeekRule);
					return CurCalendar.GetWeekOfYear(DateValue, WeekRule, DayRule);
				case DateInterval.Weekday:
					return ConvertWeekDay(DateValue.DayOfWeek, (int)DayRule);
				case DateInterval.DayOfYear:
					return DateValue.DayOfYear;
				case DateInterval.Day:
					return DateValue.Day;
				case DateInterval.Hour:
					return DateValue.Hour;
				case DateInterval.Minute:
					return DateValue.Minute;
				case DateInterval.Second:
					return DateValue.Second;
				default:
					throw new ArgumentException();
			}
		}

		private static DateInterval DateIntervalFromString(string Interval) 
		{
			switch (Interval) {
				case "yyyy":
					return DateInterval.Year;
				case "q":
					return DateInterval.Quarter;
				case "m":
					return DateInterval.Month;
				case "ww":
					return DateInterval.WeekOfYear;
				case "w":
					return DateInterval.Weekday;
				case "d":
					return DateInterval.Day;
				case "y":
					return DateInterval.DayOfYear;
				case "h":
					return DateInterval.Hour;
				case "n":
					return DateInterval.Minute;
				case "s":
					return DateInterval.Second;
			default:
					throw new ArgumentException();
			}
		}

		public static System.DateTime DateAdd (string Interval, 
			double Number, System.Object DateValue) 
		{
			if (DateValue == null) {
				throw new ArgumentNullException("DateValue", "Value can not be null.");
			}
			if (!(DateValue is DateTime)) {
				throw new InvalidCastException();
			}
			
			return DateAdd(DateIntervalFromString(Interval), Number, (DateTime)DateValue);
		}

		public static System.Int64 DateDiff (string Interval, 
			System.Object Date1, System.Object Date2, 
			[Optional] [DefaultValue(FirstDayOfWeek.Sunday)]
			FirstDayOfWeek StartOfWeek, 
			[Optional] [DefaultValue(FirstWeekOfYear.Jan1)] 
			FirstWeekOfYear StartOfYear) 
		{
			if (Date1 == null) {
				throw new ArgumentNullException("Date1", "Value can not be null.");
			}
			if (Date2 == null) {
				throw new ArgumentNullException("Date2", "Value can not be null.");
			}
			if (!(Date1 is DateTime)) {
				throw new InvalidCastException();
			}
			if (!(Date2 is DateTime)) {
				throw new InvalidCastException();
			}
			
			return DateDiff(DateIntervalFromString(Interval), (DateTime)Date1, 
				(DateTime)Date2, StartOfWeek, StartOfYear);
			
		}

		public static System.Int32 DatePart (string Interval, 
			System.Object DateValue, 
			[Optional] [DefaultValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional] [DefaultValue(FirstWeekOfYear.Jan1)] 
			FirstWeekOfYear StartOfYear) 
		{
			if (DateValue == null) {
				throw new ArgumentNullException("DateValue", "Value can not be null.");
			}
			if (!(DateValue is DateTime)) {
				throw new InvalidCastException();
			}
			
			
			return DatePart(DateIntervalFromString(Interval), 
				(DateTime)DateValue, StartOfWeek, StartOfYear);
		}

		public static System.DateTime DateSerial (int Year, int Month, int Day) 
		{
			return new DateTime(Year, Month, Day); 
		}

		public static System.DateTime TimeSerial (int Hour, int Minute, int Second) 
		{
			return new DateTime(1, 1, 1, Hour, Minute, Second);
		}

		public static System.DateTime DateValue (string StringDate) 
		{ 
			return DateTime.Parse(StringDate);
		}

		public static System.DateTime TimeValue (string StringTime) 
		{ 
			return DateTime.Parse(StringTime);
		}

		public static int Year (System.DateTime DateValue) 
		{ 
			return DateValue.Year;
		}

		public static int Month (System.DateTime DateValue) 
		{ 
			return DateValue.Month;
		}

		public static int Day (System.DateTime DateValue) 
		{ 
			return DateValue.Day;
		}

		public static int Hour (System.DateTime TimeValue) 
		{ 
			return TimeValue.Hour;
		}

		public static int Minute (System.DateTime TimeValue) 
		{ 
			return TimeValue.Minute;
		}

		public static int Second (System.DateTime TimeValue) 
		{ 
			return TimeValue.Second;
		}

		public static int Weekday (System.DateTime DateValue, 
			[Optional] [DefaultValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek) 
		{ 
			return DatePart(DateInterval.Weekday, DateValue, StartOfWeek, FirstWeekOfYear.System);
		}

		public static System.String MonthName (int Month, 
			[Optional] [DefaultValue(false)] bool Abbreviate) 
		{ 
			if (Month < 1 || Month > 13) {
				throw new ArgumentException();
			}
			if (Abbreviate) {
				return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Month);
			}
			else {
				return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
			}
		}
		
		public static System.String WeekdayName (int Weekday, 
			[Optional] [DefaultValue(false)] System.Boolean Abbreviate, 
			[Optional] [DefaultValue(FirstDayOfWeek.System)] 
			FirstDayOfWeek FirstDayOfWeekValue) 
		{ 
			if (Weekday < 1 || Weekday > 7) {
				throw new ArgumentException();
			}
			Weekday += (int)FirstDayOfWeekValue;
			if (Weekday > 7) {
				Weekday -= 7;
			}
			if (Abbreviate) {
				return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)Weekday);
			}
			else {
				return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)Weekday);
			}
		}
	}
}
