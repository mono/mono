// 
// System.Globalization.Calendar.cs
//
// Nick made it.  (nick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com/
//

namespace System.Globalization
{
	public abstract class Calendar
	{
		protected Calendar ();

		public const int CurrentEra;
		
		public abstract int[] Eras {get;}
		public virtual int TwoDigitYearMax {get; set;}
		
		public virtual DateTime AddDays ( DateTime time, int days );
		public virtual DateTime AddHours ( DateTime time, int hours );
		public virtual DateTime AddMilliseconds ( DateTime time, double milliseconds );
		public virtual DateTime AddMinutes ( DateTime time, int minutes );
		public virtual DateTime AddMonths ( DateTime time, int months );
		public virtual DateTime AddSeconds ( DateTime time, int seconds );
		public virtual DateTime AddWeeks ( DateTime time, int weeks );
		public virtual DateTime AddYears ( DateTime time, int years );
		

		public abstract int GetDayOfMonth ( DateTime time );
		public abstract DayOfWeek GetDayOfWeek ( DateTime time );
		public abstract GetDayOfYear ( DateTime time );
		public virtual int GetDaysInMonth ( int year, int month );
		public abstract int GetDaysInMonth ( int year, int month, int era );
		public virtual int GetDaysInYear ( int year );
		public abstract int GetDaysInYear ( int year, int era );
		public abstract int GetEra ( DateTime time );
		public virtual int GetHour ( DateTime time );
		public virtual double GetMilliseconds ( DateTime time );
		public virtual int GetMinute ( DateTime time );
		public abstract int GetMonth ( DateTime time );
		public virtual int GetMonthsInYear ( int year );
		public abstract int GetMonthsInYear ( int year, int era );
		public virtual int GetSecond ( DateTime time );
		public virtual int GetWeekOfYear ( DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek );
		public abstract int GetYear ( DateTime time );
		public virtual bool IsLeapDay ( int year, int month, int day );
		public abstract bool IsLeapDay ( int year, int month, int day, int era );
		public virtual bool IsLeapMonth ( int year, int month );
		public abstract bool IsLeapMonth ( int year, int month, int era );
		public virtual bool IsLeapYear ( int year );
		public abstract bool IsLeapYear ( int year, int era );
		public virtual DateTime ToDateTime ( int year, int month, int day, int hour, int minute, int second, int millisecond );
		public abstract DateTime ToDateTime ( int year, int month, int date, int hour, int minute, int second, int millisecond, int era );
		public virtual int ToFourDigitYear ( int year );
	}
}

