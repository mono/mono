// ::MONO
//
// System.Globalization.Calendar.cs
//
// Copyright (C) Wictor Wilén 2001 (wictor@iBizkit.se)
//
// Contributors: Marcel Narings, Wictor Wilén
//
// Revisions
// 2001-09-14:	First draft
// 2001-09-15:	First release
//
//
// TODO: testing
//
//

using System;

namespace System.Globalization
{
	/// <summary>
	/// Implmentation of the System.Globalization.Calendar class
	/// </summary>
	public abstract class Calendar
	{
		/// <summary>
		/// The Calendar Constructor
		/// </summary>
		protected Calendar () 
		{
			_MaxDateTime = DateTime.MaxValue;
			_MinDateTime = DateTime.MinValue;
		}
		[CLSCompliant(false)]
		protected int _TwoDigitYearMax;

		[CLSCompliant(false)]
		protected static int[] _DaysInMonth = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
		[CLSCompliant(false)]
		protected static int[] _DaysInMonthLeap = {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
		

		// these can be overridden, for example using "new protected const int _MinYear = 1;"
		[CLSCompliant(false)]
		protected const int _MinYear = 1;
		[CLSCompliant(false)]
		protected const int _MaxYear = 9999;
		[CLSCompliant(false)]
		protected const int _MinDay = 0;
		[CLSCompliant(false)]
		protected const int _MinMonth = 1;
		[CLSCompliant(false)]
		protected const int _MaxMonth = 12;
		[CLSCompliant(false)]
		protected const int _MinHour = 0;
		[CLSCompliant(false)]
		protected const int _MaxHour = 23;
		[CLSCompliant(false)]
		protected const int _MinMinute = 0;
		[CLSCompliant(false)]
		protected const int _MaxMinute = 59;
		[CLSCompliant(false)]
		protected const int _MinSecond = 0;
		[CLSCompliant(false)]
		protected const int _MaxSecond = 59;
		[CLSCompliant(false)]
		protected const int _MinMillisecond = 0;
		[CLSCompliant(false)]
		protected const int _MaxMillisecond = 999;

		[CLSCompliant(false)]
		private const long _TicksPerMillisecond = 10000;
		[CLSCompliant(false)]
		private const long _TicksPerSecond = 10000000;
		[CLSCompliant(false)]
		private const long _TicksPerMinute = 600000000;
		[CLSCompliant(false)]
		private const long _TicksPerHour = 36000000000;
		[CLSCompliant(false)]
		private const long _TicksPerDay = 864000000000;
		[CLSCompliant(false)]
		private const long _TicksPerWeek = 6048000000000;

		[CLSCompliant(false)]
		protected DateTime _MaxDateTime;
		[CLSCompliant(false)]
		protected DateTime _MinDateTime;

		
		/// <summary>
		/// The Currentera constant
		/// </summary>
		public const int CurrentEra = 0;
		
		/// <summary>
		/// Returns an array of the available eras
		/// </summary>
		public abstract int[] Eras {get;}

		// DONE!
		/// <summary>
		/// The Two digit max
		/// </summary>
		public virtual int TwoDigitYearMax 
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

		// DONE!
		public virtual DateTime AddDays ( DateTime time, int days )
		{
			return new DateTime(time.Ticks).AddTicks(_TicksPerDay*days);
		}

		// DONE!
		public virtual DateTime AddHours ( DateTime time, int hours )
		{
			return new DateTime(time.Ticks).AddTicks(_TicksPerHour*hours);
		}

		// DONE!
		public virtual DateTime AddMilliseconds ( DateTime time, double milliseconds )
		{
			DateTime t = new DateTime(time.Ticks);
			return t.AddMilliseconds(milliseconds);
		}

		// DONE!
		public virtual DateTime AddMinutes ( DateTime time, int minutes )
		{
			return new DateTime(time.Ticks).AddTicks(_TicksPerMinute * minutes);
		}

		// DONE!
		/// <summary>
		/// Returns a DateTime that is the specified number of months away from the specified DateTime
		/// </summary>
		/// <param name="time"></param>
		/// <param name="months"></param>
		/// <returns></returns>
		/// <remarks>Calculates correct comapared to .NET Beta 2</remarks>
		public virtual DateTime AddMonths ( DateTime time, int months )		
		{
			DateTime t = new DateTime(time.Ticks);
			return t.AddMonths(months);
		}

		// DONE!
		public virtual DateTime AddSeconds ( DateTime time, int seconds )		
		{
			return new DateTime(time.Ticks).AddTicks(_TicksPerSecond * seconds);
		}

		// DONE!
		public virtual DateTime AddWeeks ( DateTime time, int weeks )		
		{
			return new DateTime(time.Ticks).AddTicks(_TicksPerWeek * weeks);
		}

		// DONE!
		public virtual DateTime AddYears ( DateTime time, int years )		
		{
			DateTime t = new DateTime(time.Ticks);
			return t.AddYears(years);
		}

		

		// DONE!
		public abstract int GetDayOfMonth ( DateTime time );

		// DONE!
		public abstract DayOfWeek GetDayOfWeek ( DateTime time );

		// DONE!
		public abstract int GetDayOfYear ( DateTime time );

		// DONE!
		public virtual int GetDaysInMonth ( int year, int month )
		{
			if(year < _MinYear || year > _MaxYear || month < _MinMonth || month > _MaxMonth)
				throw new System.ArgumentOutOfRangeException();

			if(this.IsLeapYear(year))
				return _DaysInMonthLeap[month];
			else
				return _DaysInMonth[month];
		}

		// DONE!
		public abstract int GetDaysInMonth ( int year, int month, int era );

		// DONE!
		public virtual int GetDaysInYear ( int year)
		{
			if( year < _MinYear || year > _MaxYear)
				throw new System.ArgumentOutOfRangeException();

			if(this.IsLeapYear(year))
				return 366;
			else
				return 365;
		}

		// DONE!
		public abstract int GetDaysInYear ( int year, int era );
		
		// DONE!
		public abstract int GetEra ( DateTime time );
		
		// DONE!
		public virtual int GetHour ( DateTime time )
		{
			return time.Hour;
		}
		// DONE!
		public virtual double GetMilliseconds ( DateTime time )
		{
			return time.Millisecond;
		}
		// DONE!
		public virtual int GetMinute ( DateTime time )
		{
			return time.Minute;
		}

		// DONE!
		public abstract int GetMonth ( DateTime time );
		
		// DONE!
		public virtual int GetMonthsInYear ( int year )
		{
			if( year < _MinYear || year > _MaxYear)
				throw new System.ArgumentException();

			return _MaxMonth;
		}

		// DONE!
		public abstract int GetMonthsInYear ( int year, int era );

		// DONE!
		public virtual int GetSecond ( DateTime time )
		{
			return time.Second;
		}

		// DONE!
		/// <summary>
		/// Gets the week of the year that includes the date in the specified DateTime
		/// </summary>
		/// <param name="time"></param>
		/// <param name="rule"></param>
		/// <param name="firstDayOfWeek"></param>
		/// <returns></returns>
		/// <remarks>.NET beta 2 calculates this erroneous, but this one is ok(? I think...)</remarks>
		public virtual int GetWeekOfYear ( DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek )
		{
			if( firstDayOfWeek < DayOfWeek.Sunday || firstDayOfWeek > DayOfWeek.Saturday)
				throw new System.ArgumentOutOfRangeException();
			
			int week;
			int days = 0;
			
			int[] dim;
			if(this.IsLeapYear(time.Year))
				dim = _DaysInMonthLeap;
			else
				dim = _DaysInMonth;

			DateTime jan1 = new DateTime(time.Year, 1, 1);

			for( int i = 0; i < time.Month-1; i++)
				days += dim[i];
			days += time.Day;

			switch(rule)
			{
				case CalendarWeekRule.FirstDay:
					while(jan1.DayOfWeek != firstDayOfWeek)
					{
						days--;
						jan1 = jan1.AddTicks(_TicksPerDay);
					}
					break;
				case CalendarWeekRule.FirstFourDayWeek:
					while(jan1.DayOfWeek < firstDayOfWeek)
					{
						days--;
						jan1 = jan1.AddTicks(_TicksPerDay);
					}
					break;
				case CalendarWeekRule.FirstFullWeek:
					if(jan1.DayOfWeek != firstDayOfWeek)
					{
						do
						{
							days--;
							jan1 = jan1.AddTicks(_TicksPerDay);
						}
						while(jan1.DayOfWeek != firstDayOfWeek);
					}
					break;
				default:
					throw new System.ArgumentOutOfRangeException();
			}

			if(days <= 0)
				week = GetWeekOfYear(new DateTime(time.Year-1,12,31), rule, firstDayOfWeek);
			else
				week = (--days / 7) + 1;
			
			return week;
		}

		// DONE!
		public abstract int GetYear ( DateTime time );
		
		// DONE!
		// TODO: verify this for the Calendar Class
		public virtual bool IsLeapDay ( int year, int month, int day )
		{
			int dim;

			if(day < _MinDay || month < _MinMonth || month > _MaxMonth)
				throw new System.ArgumentOutOfRangeException();

			if(this.IsLeapYear(year))
				dim = _DaysInMonthLeap[month-1];
			else
				dim = _DaysInMonth[month-1];

			if( day > dim)
				throw new System.ArgumentOutOfRangeException();

			if( month == 2 && day == 29)
				return true;
			
			return false;
		}

		// DONE!
		public abstract bool IsLeapDay ( int year, int month, int day, int era );

		// DONE!
		public virtual bool IsLeapMonth ( int year, int month )
		{
			if( year < _MinYear || year > _MaxYear || month < _MinMonth || month > _MaxMonth)
				throw new System.ArgumentOutOfRangeException();

			if(this.IsLeapYear(year))
			{
				return true;
			}
			else
				return false;
		}
		
		// DONE!
		public abstract bool IsLeapMonth ( int year, int month, int era );

		public virtual bool IsLeapYear ( int year )
		{
			if(year < _MinYear || year > _MaxYear )
				throw new System.ArgumentOutOfRangeException();
			if(year % 4 == 0) // TODO: verify this for the Calendar class!
				return true;
			return false;
		}

		// DONE!
		public abstract bool IsLeapYear ( int year, int era );

		// DONE!
		public virtual DateTime ToDateTime ( int year, int month, int day, int hour, int minute, int second, int millisecond )
		{
			int dim;
			dim = GetDaysInMonth(year,month);
			if( day < _MinDay || day > dim || 
				hour < _MinHour || hour > _MaxHour ||
				minute < _MinMinute || minute > _MaxMinute ||
				second < _MinSecond || second > _MaxSecond ||
				millisecond < _MinMillisecond || millisecond > _MaxMillisecond)
				throw new System.ArgumentOutOfRangeException();

			return new DateTime(year,month,day,hour,minute,second,millisecond,this);
		}

		// DONE!
		public abstract DateTime ToDateTime ( int year, int month, int date, int hour, int minute, int second, int millisecond, int era );

		// DONE!
		public virtual int ToFourDigitYear ( int year )
		{
			int i = year - ( _TwoDigitYearMax % 100 );
			if( year > 0 )
				return _TwoDigitYearMax - 100 + year;
			else
				return _TwoDigitYearMax + year;
		}
	}
}
