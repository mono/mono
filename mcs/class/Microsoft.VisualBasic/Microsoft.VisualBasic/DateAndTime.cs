//
// DateAndTime.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Pablo Cardona (pcardona37@hotmail.com) CRL Team

// (C) 2002 Chris J Breisch
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic 
{
	[StandardModule] 
	sealed public class DateAndTime {

		private DateAndTime ()
		{
			//Nobody should see constructor
		}

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
		
                [DllImport("libc")]
                static extern int stime (ref int t);

		public static System.DateTime Today {
			get { 
				return DateTime.Today; 
			}
			set { 
				System.DateTime Now = DateTime.Now;
                                System.DateTime NewDate = new DateTime(value.Year, value.Month, value.Day,
								       Now.Hour, Now.Minute, Now.Second, Now.Millisecond);
                                System.TimeSpan secondsTimeSpan = NewDate.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
                                int seconds = (int) secondsTimeSpan.TotalSeconds;

                                if(stime(ref seconds) == -1)
                                        throw new UnauthorizedAccessException("The caller is not the super-user.");
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
        		         Today = DateTime.Now;
		                 System.DateTime NewTime = new DateTime(Today.Year, Today.Month, Today.Day,
                		                                        value.Hour,value.Minute,value.Second);

		                 TimeSpan secondsTimeSpan = NewTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                		 int seconds = (int) secondsTimeSpan.TotalSeconds;

                		 if(stime(ref seconds) == -1)
					 throw new UnauthorizedAccessException("The caller is not the super-user.");
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
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional, __DefaultArgumentValue(FirstWeekOfYear.Jan1)] 
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
					if (CurCalendar == null)
						throw new NotImplementedException("Looks like CultureInfo.CurrentCulture.Calendar is still returning null");
					return CurCalendar.GetWeekOfYear(Date2,	WeekRule, DayRule) -
						CurCalendar.GetWeekOfYear(Date1,WeekRule, DayRule) + 
						YearWeeks;
				case DateInterval.Weekday:
					return ((TimeSpan)(Date2.Subtract(Date1))).Days / 7;
				case DateInterval.DayOfYear:
				case DateInterval.Day:
					return ((TimeSpan)(Date2.Subtract(Date1))).Days;
				case DateInterval.Hour:
					return (int)((TimeSpan)(Date2.Subtract(Date1))).TotalHours;
				case DateInterval.Minute:
					return (int)((TimeSpan)(Date2.Subtract(Date1))).TotalMinutes;
				case DateInterval.Second:
					return (int)((TimeSpan)(Date2.Subtract(Date1))).TotalSeconds;
				default:
					throw new ArgumentException();
			}
		}

		private static int ConvertWeekDay(DayOfWeek Day, int Offset) 
		{
			if (Offset == 0)
				return (int)Day+1;

			int Weekday = (int)Day + 1 - Offset;
			if (Weekday < 0)
				Weekday += 7;

			return Weekday + 1;

			/*if(Offset >= 7)
				Offset  -= 7;

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
			}*/

		}

		public static int DatePart 
		(
			Microsoft.VisualBasic.DateInterval Interval, 
			System.DateTime DateValue, 
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional, __DefaultArgumentValue(FirstWeekOfYear.Jan1)] 
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
					return ConvertWeekDay(DateValue.DayOfWeek, (int)StartOfWeek);
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
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.Sunday)]
			FirstDayOfWeek StartOfWeek, 
			[Optional, __DefaultArgumentValue(FirstWeekOfYear.Jan1)] 
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
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek, 
			[Optional, __DefaultArgumentValue(FirstWeekOfYear.Jan1)] 
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
			DateTime date;

			if (Year < 0)
				Year = Year + DateTime.Now.Year;
			else if (Year >= 0 && Year <= 29)
				Year += 2000;
			else if(Year >= 30 && Year <= 99)
				Year += 1900;

			date = new DateTime(Year, 1, 1); 

			date = date.AddMonths(Month - 1);

			date = date.AddDays(Day - 1);

			return date;
		}

		public static System.DateTime TimeSerial (int Hour, int Minute, int Second) 
		{
			int day = 1;

			if (Second < 0)	{
				if (Minute == 0 && Hour == 0)
					Second += 60;
				else if (Minute == 0){
					Second += 60;
					Minute = 59;
					Hour--;     
				}
				else {
					Second += 60;
					Minute--;                    
				}                
			}
			else if(Second > 59){
				Minute += Second/60;
				Second = Second%60;
			}

			if (Minute < 0)	{
				if (Hour == 0)
					Minute += 60;
				else {    
					Minute += 60;
					Hour--;
				}        
			}
			else if (Minute > 59){
				Hour += Minute/60;
				Minute = Minute%60;
			}

			if (Hour < 0)
				Hour += 24;
			else if (Hour > 23)	{
				day += Hour/24;
				Hour = Hour%24;
			}

			return new DateTime(1, 1, day, Hour, Minute, Second);
		}

		public static System.DateTime DateValue (string StringDate) 
		{ 
			string[] expectedFormats = {"D", "d", "G", "g", "f" ,"F", "m", "M", "r", "R",
							"s", "T", "t", "U", "u", "Y", "y",
							"MMM dd, yy", "MMMM dd, yy", "MMM dd, yyyy", "MMMM dd, yyyy"};
			
			try {
				return DateTime.ParseExact(StringDate, expectedFormats,
							System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat,
							System.Globalization.DateTimeStyles.NoCurrentDateDefault);
			} 
			catch (FormatException exception) {
				throw new InvalidCastException(null, exception);
			}
		}

		public static System.DateTime TimeValue (string StringTime) 
		{ 
			try {
				return DateTime.MinValue + DateTime.Parse(StringTime).TimeOfDay;
			} catch (FormatException exception) {
				throw new InvalidCastException(null, exception);
			}
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
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.Sunday)] 
			FirstDayOfWeek StartOfWeek) 
		{ 
			return DatePart(DateInterval.Weekday, DateValue, StartOfWeek, FirstWeekOfYear.System);
		}

		public static System.String MonthName (int Month, 
			[Optional, __DefaultArgumentValue(false)] bool Abbreviate) 
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
			[Optional, __DefaultArgumentValue(false)] System.Boolean Abbreviate, 
			[Optional, __DefaultArgumentValue(FirstDayOfWeek.System)] 
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
