// ::MONO
//
// System.Globalization.GregorianCalendar.cs
//
// Copyright (C) Wictor Wilén 2001 (wictor@iBizkit.se)
//
// Contributors: Wictor Wilén
//
// Revisions
// 2001-09-15:	First draft
//
//
// TODO: testing
//
//

using System;

namespace System.Globalization
{

	
	/// <summary>
	/// Represents the Gregorian calendar.
	/// </summary>
	/// <remarks>The Gregorian calendar recognizes two eras: B.C. (before Christ) or B.C.E. (before common era), and A.D. (Latin "Anno Domini", which means "in the year of the Lord") or C.E. (common era). This implementation of the GregorianCalendar class recognizes only the current era (A.D. or C.E.).</remarks>
	// TODO: implement the BC era
	public class GregorianCalendar : Calendar
	{
		// private members
		private GregorianCalendarTypes _CalendarType;


		// Public Constants
		public const int ADEra = 1;
		
		// Public Instance Constructors
		// DONE!
		public GregorianCalendar()
		{
			_CalendarType = GregorianCalendarTypes.Localized;
		}

		// DONE!
		public GregorianCalendar(GregorianCalendarTypes type)
		{
			_CalendarType = type;
		}
		
		// DONE!
		public GregorianCalendarTypes CalendarType 
		{
			get
			{
				return _CalendarType;
			} 
			set
			{
				_CalendarType = value;
			}
		}

		// Public Instance Properties
		// DONE!
		public override int[] Eras 
		{
			get
			{
				return new int[] {1};
			}
		}
		// DONE!
		public override int TwoDigitYearMax 
		{
			get
			{
				return _TwoDigitYearMax;
			} 
			set
			{
				_TwoDigitYearMax = value;
			}
		}

		// Public Instance Methods
		// DONE!
		public override DateTime AddMonths ( DateTime time, int months )
		{
			if(months < -120000 || months > 120000)
				throw new System.ArgumentOutOfRangeException();
			DateTime dt = new DateTime(time.Ticks);			
			dt =  dt.AddMonths(months);
			return dt;
			
		}
		// DONE!
		public override DateTime AddYears ( DateTime time, int years )
		{
			DateTime dt = new DateTime(time.Ticks);
			return dt.AddYears(years);
		}

		// DONE!
		public override int GetDayOfMonth ( DateTime time )
		{
			return time.Day;
		}

		// DONE!
		public override DayOfWeek GetDayOfWeek ( DateTime time )
		{
			return time.DayOfWeek;
		}

		// DONE!
		public override int GetDayOfYear ( DateTime time )
		{
			return time.DayOfYear;
		}
		
		// DONE!
		public override int GetDaysInMonth ( int year, int month, int era )
		{
			if(	year < _MinYear || year > _MaxYear || 
				month < _MinMonth || month > _MaxMonth )				
				throw new System.ArgumentOutOfRangeException();

			if( era != ADEra)
				throw new System.ArgumentException();

			if(this.IsLeapYear(year))
				return _DaysInMonthLeap[month];
			else
				return _DaysInMonth[month];
		}
		

		// DONE!		
		public override int GetDaysInYear ( int year, int era )
		{
			if(year < _MinYear || year > _MaxYear)
				throw new System.ArgumentOutOfRangeException();

			if( era != ADEra)
				throw new System.ArgumentException();

			return this.GetDaysInYear(year);
		}

		// DONE!
		public override int GetEra ( DateTime time )
		{
			return ADEra;
		}
		// DONE!
		public override int GetMonth ( DateTime time )
		{
			return time.Month;
		}

		// DONE!		
		public override int GetMonthsInYear ( int year, int era )
		{
			if(year < _MinYear || year > _MaxYear || era != ADEra )
				throw new System.ArgumentOutOfRangeException();

			return _MaxMonth;
		}
		
		// DONE!
		public override int GetYear ( DateTime time )
		{			
			return time.Year;
		}

		// DONE!
		public override bool IsLeapDay ( int year, int month, int day, int era )
		{			
			int dim;

			if(day < _MinDay || month < _MinMonth || month > _MaxMonth)
				throw new System.ArgumentException();

			if(this.IsLeapYear(year,era))
				dim = _DaysInMonthLeap[month-1];
			else
				dim = _DaysInMonth[month-1];

			if( day > dim)
				throw new System.ArgumentException();

			if( month == 2 && day == 29)
				return true;
			
			return false;
		}
		// DONE!
		public override bool IsLeapMonth ( int year, int month )
		{
			if( year < _MinYear || year > _MaxYear || month < _MinMonth || month > _MaxMonth)
				throw new System.ArgumentException();
			return false;	
		}
		// DONE!
		public override bool IsLeapMonth ( int year, int month, int era )
		{
			if( year < _MinYear || year > _MaxYear || month < _MinMonth || month > _MaxMonth || era != ADEra)
				throw new System.ArgumentException();
			return false;
		}
		
		// DONE!
		public override bool IsLeapYear ( int year, int era )
		{
			if(year < _MinYear || year > _MaxYear || era != ADEra)
				throw new System.ArgumentOutOfRangeException();
			if( ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0) )
				return true;
			return false;
		}
		
		// DONE!
		public override DateTime ToDateTime ( int year, int month, int day, int hour, int minute, int second, int millisecond, int era )
		{
			// INFO: year, era and month is checked by GetDaysInMonth()
			int dim;
			dim = GetDaysInMonth(year,month);
			if( day < _MinDay || day > dim || 
				hour < _MinHour || hour > _MaxHour ||
				minute < _MinMinute || minute > _MaxMinute ||
				second < _MinSecond || second > _MaxSecond ||
				millisecond < _MinMillisecond || millisecond > _MaxMillisecond)
				throw new System.ArgumentException();

			return new DateTime(year,month,day,hour,minute,second,millisecond,this);
		}
		
		// DONE!
		public override int ToFourDigitYear ( int year )
		{
			int y = _TwoDigitYearMax % 100;
			if( year > y )
				y = _TwoDigitYearMax - y - 100 + year;
			else
				y = _TwoDigitYearMax - y + year;

			if( y < _MinYear || y > _MaxYear)
				throw new System.ArgumentException();
			return y;
		}

	}
}
