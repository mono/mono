//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//
//   (C) 2001 Marcel Narings

using System;
using System.Globalization ;


namespace System
{
	/// <summary>
	/// The DateTime structure represents dates and time ranging from 1-1-0001 12:00:00 AM to 31-12-9999 23:59:00 Common Era.
	/// </summary>
	/// 
	public struct DateTime : IComparable , IFormattable  , IConvertible
	{
		long ticks;

		private const long MaxTicks = 3155378975999999999L;
		private const long MinTicks = 0L;
		private const int dp400 = 146097;
		private const int dp100 = 36524;
		private const int dp4 = 1461;
		
		public static readonly DateTime MaxValue = new DateTime (MaxTicks);
		public static readonly DateTime MinValue = new DateTime (MinTicks);
		
		private enum Which 
		{
			Day,
			DayYear,
			Month,
			Year
		};
	
		private static int[] daysmonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
		private static int[] daysmonthleap = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	

		private static long AbsoluteDays (int year, int month, int day)
		{
			int[] days;
			int temp = 0, m=1 ;
		
			days = (IsLeapYear(year) ? daysmonthleap  : daysmonth);
			
			while (m < month)
				temp += days[m++];
			return ((day-1) + temp + (365* (year-1)) + ((year-1)/4) - ((year-1)/100) + ((year-1)/400));
		}

		private int FromTicks(Which what)
		{
			int num400, num100, num4, numyears; 
			int M =1;

			int[] days = daysmonth;
			int totaldays = (int) (ticks / TimeSpan.TicksPerDay);
			
			num400 = (totaldays / dp400);
			totaldays -=  num400 * dp400;
		
			num100 = (totaldays / dp100);
			if (num100 == 4)   // leap
				num100 = 3;
			totaldays -= (num100 * dp100);

			num4 = totaldays / dp4;
			totaldays -= (num4 * dp4);

			numyears = totaldays / 365 ;
			
			if (numyears == 4)  //leap
				numyears =3 ;
			if (what == Which.Year )
				return num400*400 + num100*100 + num4*4 + numyears + 1;

			totaldays -= (numyears * 365) ;
			if (what == Which.DayYear )
				return totaldays + 1;
			
			if  ((numyears==3) && ((num100 == 3) || !(num4 == 24)) ) //31 dec leapyear
				days = daysmonthleap;
			        
			while (totaldays >= days[M])
				totaldays -= days[M++];

			if (what == Which.Month )
				return M;

			return totaldays +1; 


		}


		// Constructors
		
		/// <summary>
		/// Constructs a DateTime for specified ticks
		/// </summary>
		/// 
		public DateTime (long newticks) 
		{
			ticks = newticks;
		
			if ( newticks < MinValue.ticks || newticks > MaxValue.ticks)
				throw new ArgumentOutOfRangeException ();
		}

		public DateTime (int year, int month, int day)
			: this (year, month, day,0,0,0,0) {}

		public DateTime (int year, int month, int day, int hour, int minute, int second)
			: this (year, month, day, hour, minute, second, 0)	{}

		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond)
		{
			if ( year < 1 || year > 9999 || 
				month < 1 || month >12  ||
				day < 1 || day > DaysInMonth(year, month) ||
				hour < 0 || hour > 23 ||
				minute < 0 || minute > 59 ||
				second < 0 || second > 59 )
				throw new ArgumentOutOfRangeException() ;
				
			ticks = AbsoluteDays(year,month,day) * TimeSpan.TicksPerDay + 
				hour * TimeSpan.TicksPerHour + 
				minute * TimeSpan.TicksPerMinute + 
				second * TimeSpan.TicksPerSecond + 
				millisecond * TimeSpan.TicksPerMillisecond ; 
			
			if (ticks < MinValue.ticks || ticks > MaxValue.ticks )
				throw new ArgumentException() ;
		}

		public DateTime (int year, int month, int day, Calendar calendar)
			: this (year, month, day, 0, 0, 0, 0, calendar)	{}

		
		public DateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (year, month, day, hour, minute, second, 0, calendar)	{}


		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
			: this(year, month, day, hour, minute, second, millisecond) 
		{
			if ( calendar == null)
				throw new ArgumentNullException();
		}


		/* Properties  */
		 
		public DateTime Date 
		{
			get	
			{ 
				return new DateTime(ticks - (ticks % TimeSpan.TicksPerDay )) ; 
			}
		}
        
		public int Day
		{
			get 
			{ 
				return FromTicks(Which.Day); 
			}
		}

		public DayOfWeek DayOfWeek 
		{
			get 
			{ 
				return ( (DayOfWeek) (((ticks / TimeSpan.TicksPerDay)+1) % 7) ); 
			}
		}

		public int DayOfYear 
		{
			get 
			{ 
				return FromTicks(Which.DayYear); 
			}
		}

		public int Hour 
		{
			get 
			{ 
				return ( (int) ((ticks % TimeSpan.TicksPerDay) / TimeSpan.TicksPerHour) );  
			}
		}

		public int Millisecond 
		{
			get 
			{ 
				return ( (int) (ticks % TimeSpan.TicksPerSecond / TimeSpan.TicksPerMillisecond) ); 
			}
		}
		
		public int Minute 
		{
			get 
			{ 
				return ( (int) (ticks % TimeSpan.TicksPerHour / TimeSpan.TicksPerMinute) ); 
			}
		}

		public int Month 
		{
			get	
			{ 
				return FromTicks(Which.Month); 
			}
		}

		// TODO implement me  		 
		public static DateTime Now 
		{
			get	
			{ 
				return new DateTime (0); 
			}
		}

		public int Second 
		{
			get	
			{ 
				return (int) (ticks % TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond); 
			}
		}

		public long Ticks
		{ 
			get	
			{ 
				return ticks ; 
			}
		}
	
		public TimeSpan TimeOfDay 
		{
			get	
			{ 
				return new TimeSpan(ticks % TimeSpan.TicksPerDay );
			}
			
		}

		//TODO implement
		public static DateTime Today 
		{
			get	
			{
				return new DateTime (0);
			}
		}

		//TODO implement
		public static DateTime UtcNow 
		{
			get {
				return new DateTime (0);
			}
		}

		public int Year 
		{
			get 
			{ 
				return FromTicks(Which.Year); 
			}
		}

		/* methods */

		public DateTime Add (TimeSpan ts)
		{
			long newticks ;

			newticks = ticks + ts.Ticks ;

			if (ts.Ticks < MinTicks || ts.Ticks > MaxTicks || 
				newticks < MinTicks || newticks > MaxTicks)
				throw new ArgumentException ();
			
			return new DateTime (newticks);
		}

		public DateTime AddDays (double days)
		{
			return AddMilliseconds (days * 86400000);
		}
		
		public DateTime AddTicks (long t)
		{
			long newticks = ticks + t; 

			if (t<MinTicks || t>MaxTicks || newticks<MinTicks || newticks>MaxTicks)
				throw new ArgumentException ();

			return new DateTime(newticks);
		}

		public DateTime AddHours (double hours)
		{
			return AddMilliseconds (hours * 3600000);
		}

		public DateTime AddMilliseconds (double ms)
		{
			long msticks, newticks;
			
			msticks = (long) (ms += ms > 0 ? 0.5 : -0.5) * TimeSpan.TicksPerMillisecond ; 
			newticks = ticks + msticks ;

			if (msticks < MinTicks || msticks > MaxTicks ||
				newticks < MinTicks || newticks > MaxTicks)
				throw new ArgumentException ();

			return new DateTime (newticks);
		}

		public DateTime AddMinutes (double minutes)
		{
			return AddMilliseconds (minutes * 60000);
		}
		
		public DateTime AddMonths (int months)
		{
			int day, month, year,  maxday ;
			DateTime temp ;

			day = this.Day;
			month = this.Month + (months % 12);
			year = this.Year + months/12 ;
			
			if (month < 1)
			{
				month = 12 + month ;
				year -- ;
			}
			else if (month>12) 
			{
				month = month -12;
				year ++;
			}
			maxday = DaysInMonth(year, month);
			if (day > maxday)
				day = maxday;

			temp = new DateTime (year, month, day);
            return  temp.Add (this.TimeOfDay);
		}

		public DateTime AddSeconds (double seconds)
		{
			return AddMilliseconds (seconds*1000);
		}

		public DateTime AddYears (int years )
		{
			return AddMonths(years * 12);
		}

		public static int Compare (DateTime t1,	DateTime t2)
		{
			if (t1.ticks < t2.ticks) 
				return -1;
			else if (t1.ticks > t2.ticks) 
				return 1;
			else
				return 0;
		}

		public int CompareTo (object v)
		{
			if ( v == null)
				return 1 ;

			if (!(v is System.DateTime))
				throw new ArgumentException ("Value is not a System.DateTime");

			return Compare (this , (DateTime) v);
		}

		public static int DaysInMonth (int year, int month)
		{
			int[] days ;

			if (month < 1 || month >12)
				throw new ArgumentOutOfRangeException ();

			days = (IsLeapYear(year) ? daysmonthleap  : daysmonth);
			return days[month];			
		}
		
		public override bool Equals (object o)
		{
			if (!(o is System.DateTime))
				return false;

			return ((DateTime) o).ticks == ticks;
		}

		public static bool Equals (DateTime t1, DateTime t2 )
		{
			return (t1.ticks == t2.ticks );
		}

		// TODO: Implement me.
		public static DateTime FromFileTime (long fileTime) 
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public static DateTime FromOADate (double d)
		{
				return new DateTime(0);
		}
		
		// TODO: Implement me.
		public string[] GetDateTimeFormats() 
		{
			return null;
		}

		//TODO: implement me 
		public string[] GetDateTimeFormats(	char format	)
		{
			return null;
		}
		
		// TODO: implement me 
		public string[] GetDateTimeFormats(	IFormatProvider provider)
		{
			return null;
		}

		//TODO: implement me 
		public string[] GetDateTimeFormats(char format,IFormatProvider provider	)
		{
			return null;
		}

		public override int GetHashCode ()
		{
			return (int) ticks;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DateTime;
		}

		public static bool IsLeapYear (int year)
		{
			return  ( (year % 4 == 0 && year % 100 != 0) || year % 400 == 0) ;
		}

		public static DateTime Parse (string s)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime ParseExact(string s,	string format, IFormatProvider provider	)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style )
		{
			// TODO: Implement me
			return new DateTime (0);
		
		}

		public static DateTime ParseExact( string s, string[] formats, IFormatProvider provider, DateTimeStyles style )
		{
			// TODO: Implement me
			return new DateTime (0);
		
		}
		
		public TimeSpan Subtract(DateTime dt )
		{   
			return new TimeSpan(ticks - dt.ticks );
		}

		public DateTime Subtract(TimeSpan ts)
		{	
			return new DateTime(ticks - ts.Ticks );
		}

		public long ToFileTime()
		{
				// TODO: Implement me
			return 0 ;
		}

		public DateTime ToLocalTime()
		{
			// TODO Implement me 
			return new DateTime (0);
		}

		public string ToLongDateString()
		{
			// TODO implement me 
			return "ToLongDateString";
		}

		public string ToLongTimeString()
		{
			// TODO implement me 
			return "ToLongTimeString";
		}

		public double ToOADate()
		{
			// TODO implement me 
			return 0;
		}

		public string ToShortDateString()
		{
			// TODO implement me 
			return "ToShortDateString";
		}

		public string ToShortTimeString()
		{
			// TODO implement me
			return "ToShortTimeString";
		}
        
		public override string ToString ()
		{
			// TODO: Implement me
			return "" ;
		}

		public string ToString (IFormatProvider fp)
		{
			// TODO: Implement me.
			return "ToString1";
		}

		public string ToString (string format)
		{
			// TODO: Implement me.
			return "ToString2";
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
				return "" ;
		}

		public DateTime ToUniversalTime()
		{
			// TODO: implement me 
			return new DateTime(0);
		}

		/*  OPERATORS */

		public static DateTime operator +(DateTime d, TimeSpan t)
		{
			return new DateTime (d.ticks + t.Ticks);
		}

		public static bool operator ==(DateTime d1, DateTime d2)
		{
			return (d1.ticks == d2.ticks);
		}

		public static bool operator >(DateTime t1,DateTime t2)
		{
			return (t1.ticks > t2.ticks);
		}

		public static bool operator >=(DateTime t1,DateTime t2)
		{
			return (t1.ticks >= t2.ticks);
		}

		public static bool operator !=(DateTime d1, DateTime d2)
		{
			return (d1.ticks != d2.ticks);
		}

		public static bool operator <(DateTime t1,	DateTime t2)
		{
			return (t1.ticks < t2.ticks );
		}

		public static bool operator <=(DateTime t1,DateTime t2)
		{
			return (t1.ticks <= t2.ticks);
		}

		public static TimeSpan operator -(DateTime d1,DateTime d2)
		{
			return new TimeSpan(d1.ticks - d2.ticks);
		}

		public static DateTime operator -(DateTime d,TimeSpan t	)
		{
			return new DateTime (d.ticks - t.Ticks);
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		public byte ToByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		// TODO Implement me  
		public System.DateTime ToDateTime(IFormatProvider provider)
		{
			return new System.DateTime(this.ticks);
		} 
		
		public decimal ToDecimal(IFormatProvider provider)
		{
			 throw new InvalidCastException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int16 ToInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int32 ToInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int64 ToInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public SByte ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Single ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public object ToType(Type conversionType,IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		UInt16 System.IConvertible.ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public UInt32 ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public UInt64 ToUInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
	}
}

namespace System
{
	public enum DayOfWeek
	{
		Sunday,
		Monday,
		Tuesday,
		Wednesday,
		Thursday,
		Friday,
		Saturday
	}
}

