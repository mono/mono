//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//
//   (C) 2001 Marcel Narings

using System.Globalization ;
namespace System
{

	public struct DateTime : IComparable //, IFormattable, IConvertible
	{
		long ticks;

		public static readonly DateTime MaxValue = new DateTime(3155378975999999999L);
		public static readonly DateTime MinValue = new DateTime(0L);
		
		private enum Which 
		{
			Day,
			DayYear,
			Month,
			Year
		};

		// Constructors
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
			// TODO implement calendar
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
				return (DayOfWeek) (((ticks / TimeSpan.TicksPerDay)+1) % 7); 
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
				return (int) ((ticks % TimeSpan.TicksPerDay) / TimeSpan.TicksPerHour);  
			}
		}

		public int Millisecond 
		{
			get 
			{ 
				return (int) (ticks % TimeSpan.TicksPerSecond / TimeSpan.TicksPerMillisecond); 
			}
		}
		
		public int Minute 
		{
			get 
			{ 
				return (int) (ticks % TimeSpan.TicksPerHour / TimeSpan.TicksPerMinute); 
			}
		}

		public int Month 
		{
			get 
			{ 
				return FromTicks(Which.Month); 
			}
		}

		// TODO IMPLEMENT ME 
		//public static DateTime Now {get;}

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
		//public static DateTime Today {get;}

		//TODO implement
		//public static DateTime UtcNow {get;}

		public int Year 
		{
			get 
			{ 
				return FromTicks(Which.Year); 
			}
		}


		/* methods */

		public DateTime AddTicks( long t )
		{
			return new DateTime(ticks + t);
		}

		// FIXME: Implement me.
		public DateTime AddDays( double days )
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public DateTime AddHours( double hours )
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public DateTime AddMilliseconds( double ms	)
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public DateTime AddMinutes(	double minutes )
		{
			return new DateTime (0);
		}
		
		// TODO: Implement me.
		public DateTime AddMonths( int months )
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public DateTime AddSeconds(double seconds )
		{
			return new DateTime (0);
		}

		// TODO: Implement me.
		public DateTime AddYears(int years )
		{
			return new DateTime (0);
		}

		public static int Compare( DateTime t1,	DateTime t2	)
		{
			if (t1.ticks < t2.ticks) 
				return -1;
			else if (t1.ticks > t2.ticks) 
				return 1;
			else
				return 0;
		}

		// FIXME check this
		public int CompareTo (object v)
		{
			if ( v == null)
				return 1 ;

			if (!(v is System.DateTime))
				throw new ArgumentException ("Value is not a System.DateTime");

			return Compare (this , (DateTime) v);
		}

		public static int DaysInMonth(int year, int month)
		{
			int[] dayspermonth = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	

			if (month < 1 || month >12)
				throw new ArgumentOutOfRangeException ();
		 
			if (month == 2 && IsLeapYear(year))
				return 29;
			else
				return dayspermonth[month];			
		}

		
		public override bool Equals (object o)
		{
			if (!(o is System.DateTime))
				return false;

			return ((DateTime) o).ticks == ticks;
		}

		public static new bool Equals(DateTime t1, DateTime t2 )
		{
			return (t1.ticks == t2.ticks );
		}

		// TODO: Implement me.
		public static DateTime FromFileTime(long fileTime) 
		{
			return new DateTime(0);
		}

		// TODO: Implement me.
		public static DateTime FromOADate(double d)
		{
			return new DateTime(0);
		}
		
		// TODO: Implement me.
		//public string[] GetDateTimeFormats();

		//TODO: implement me 
		//public string[] GetDateTimeFormats(	char format	)
		
		// TODO: implement me 
		//public string[] GetDateTimeFormats(	IFormatProvider provider)

		//TODO: implement me 
		//public string[] GetDateTimeFormats(char format,IFormatProvider provider	)


		public override int GetHashCode ()
		{
			return (int) ticks;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DateTime;
		}

		public static bool IsLeapYear(int year)
		{
			return ( !(year %4 > 0 ) && (year %100 > 0) || !(year %400 > 0) ) ;
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

		public static DateTime ParseExact(	string s, string format, IFormatProvider provider,	DateTimeStyles style )
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
		{   //TODO : implement me
			return new TimeSpan(ticks - dt.ticks );
		}

		public DateTime Subtract(TimeSpan ts)
		{	// TODO : implement me 
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
			return "";
		}

		public string ToLongTimeString()
		{
			// TODO implement me 
			return "";
		}

		public double ToOADate()
		{
			// TODO implement me 
			return 0;
		}

		public string ToShortDateString()
		{
			// TODO implement me 
			return "";
		}

		public string ToShortTimeString()
		{
			// TODO implement me
			return "";
		}
        

		public override string ToString ()
		{
			// TODO: Implement me

			return "";
		}

		public string ToString (IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		public DateTime ToUniversalTime()
		{
			// TODO: implement me 
			return new DateTime(0);
		}

		/*  OPERATORS */

		public static DateTime operator +( DateTime d,	TimeSpan t )
		{
			return new DateTime (d.ticks + t.Ticks );
		}

		public static bool operator ==(	DateTime d1, DateTime d2 )
		{
			return (d1.ticks == d2.ticks );
		}

		public static bool operator >(DateTime t1,DateTime t2)
		{
			return (t1.ticks > t2.ticks );
		}

		public static bool operator >=(	DateTime t1,DateTime t2	)
		{
			return (t1.ticks >= t2.ticks);
		}

		public static bool operator !=( DateTime d1, DateTime d2)
		{
			return (d1.ticks != d2.ticks );
		}

		public static bool operator <( DateTime t1,	DateTime t2	)
		{
			return (t1.ticks < t2.ticks );
		}

		public static bool operator <=(	DateTime t1,DateTime t2	)
		{
			return (t1.ticks <= t2.ticks );
		}

		public static TimeSpan operator -(DateTime d1,DateTime d2)
		{
			return new TimeSpan(d1.ticks - d2.ticks );
		}

		public static DateTime operator -(DateTime d,TimeSpan t	)
		{
			return new DateTime (d.ticks - t.Ticks );
		}



		private static long AbsoluteDays (int year, int month, int day)
		{
			int[] days = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
			int temp = 0, m=1 ;
		
			if (IsLeapYear(year))
				days[2] = 29;
			while (m < month)
				temp += days[m++];
			return ((day-1) + temp + (365* (year-1)) + ((year-1)/4) - ((year-1)/100) + ((year-1)/400));
			
		}

		private  int FromTicks(Which what)
		{
			const int dp400 = 146097;
			const int dp100 = 36524;
			const int dp4 = 1461;
		
			int[] days = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
			int totaldays = (int) (ticks / TimeSpan.TicksPerDay );
		
			int num400 = (totaldays / dp400);
			totaldays -=  num400 * dp400;
		
			int num100 = (totaldays / dp100);
			if (num100 == 4)   // leap
				num100 = 3;
			totaldays -= (num100 * dp100);

			int num4 = totaldays / dp4;
			totaldays -= (num4 * dp4);

			int numyears = totaldays / 365 ;
			
			if (numyears == 4)  //leap
				numyears =3 ;
			if (what == Which.Year )
				return num400*400 + num100*100 + num4*4 + numyears + 1;

			totaldays -= (numyears * 365) ;
			if (what ==Which.DayYear )
				return totaldays + 1;
			
			if  ((numyears==3) && ((num100 == 3) || !(num4 == 24)) ) //31 dec leapyear
				days[2] = 29;
		
	        
			int M =1;
			while (totaldays >= days[M])
				totaldays -= days[M++];

			if (what == Which.Month )
				return M;

			return totaldays +1;
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

		
