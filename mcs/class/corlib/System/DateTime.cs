//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//
//   (C) 2001 Marcel Narings

using System;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace System
{
	/// <summary>
	/// The DateTime structure represents dates and time ranging from
	/// 1-1-0001 12:00:00 AM to 31-12-9999 23:59:00 Common Era.
	/// </summary>
	/// 
	public struct DateTime : IComparable , IFormattable  , IConvertible
	{
		private TimeSpan ticks;
		private TimeSpan utcoffset;

		private const int dp400 = 146097;
		private const int dp100 = 36524;
		private const int dp4 = 1461;

		// w32 file time starts counting from 1/1/1601 00:00 GMT
		// which is the constant ticks from the .NET epoch
		private const long w32file_epoch = 504911232000000000L;

		//
		// The UnixEpoch, it begins on Jan 1, 1970 at 0:0:0, expressed
		// in Ticks
		//
		internal const long UnixEpoch = 621355968000000000L;
		
		public static readonly DateTime MaxValue = new DateTime (false,TimeSpan.MaxValue);
		public static readonly DateTime MinValue = new DateTime (false,TimeSpan.MinValue);
		
		private enum Which 
		{
			Day,
			DayYear,
			Month,
			Year
		};
	
		private static int[] daysmonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
		private static int[] daysmonthleap = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	

		private static int AbsoluteDays (int year, int month, int day)
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
			int totaldays = this.ticks.Days;

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
			// `local' must default to false here to avoid
			// a recursion loop.
			: this (false, newticks) {}

		internal DateTime (bool local, long newticks)
			: this (true, new TimeSpan (newticks))
		{
			if (local) {
				TimeZone tz = TimeZone.CurrentTimeZone;

				utcoffset = tz.GetUtcOffset (this);

				ticks = ticks + utcoffset;
			}
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

			ticks = new TimeSpan (AbsoluteDays(year,month,day), hour, minute, second, millisecond);
			utcoffset = new TimeSpan (0);
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

		internal DateTime (bool check, TimeSpan value)
		{
			if (check && (value.Ticks < MinValue.Ticks || value.Ticks > MaxValue.Ticks))
				throw new ArgumentOutOfRangeException ();

			ticks = value;

			utcoffset = new TimeSpan (0);
		}

		/* Properties  */
		 
		public DateTime Date 
		{
			get	
			{ 
				return new DateTime (Year, Month, Day);
			}
		}
        
		public int Month 
		{
			get	
			{ 
				return FromTicks(Which.Month); 
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
				return ( (DayOfWeek) ((ticks.Days+1) % 7) ); 
			}
		}

		public int DayOfYear 
		{
			get 
			{ 
				return FromTicks(Which.DayYear); 
			}
		}

		public TimeSpan TimeOfDay 
		{
			get	
			{ 
				return new TimeSpan(ticks.Ticks % TimeSpan.TicksPerDay );
			}
			
		}

		public int Hour 
		{
			get 
			{ 
				return ticks.Hours;
			}
		}

		public int Minute 
		{
			get 
			{ 
				return ticks.Minutes;
			}
		}

		public int Second 
		{
			get	
			{ 
				return ticks.Seconds;
			}
		}

		public int Millisecond 
		{
			get 
			{ 
				return ticks.Milliseconds;
			}
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern long GetNow ();

		public static DateTime Now 
		{
			get	
			{
				return new DateTime (true, GetNow ());
			}
		}

		public long Ticks
		{ 
			get	
			{ 
				return ticks.Ticks;
			}
		}
	
		public static DateTime Today 
		{
			get	
			{
				return new DateTime (true, (GetNow () / TimeSpan.TicksPerDay) * TimeSpan.TicksPerDay);
			}
		}

		public static DateTime UtcNow 
		{
			get {
				return new DateTime (GetNow ());
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
			return new DateTime (true, ticks) + ts;
		}

		public DateTime AddDays (double days)
		{
			return AddMilliseconds (days * 86400000);
		}
		
		public DateTime AddTicks (long t)
		{
			return Add (new TimeSpan (t));
		}

		public DateTime AddHours (double hours)
		{
			return AddMilliseconds (hours * 3600000);
		}

		public DateTime AddMilliseconds (double ms)
		{
			long msticks;
			
			msticks = (long) (ms += ms > 0 ? 0.5 : -0.5) * TimeSpan.TicksPerMillisecond ; 

			return AddTicks (msticks);
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
				return 1;

			if (!(v is System.DateTime))
				throw new ArgumentException (Locale.GetText (
					"Value is not a System.DateTime"));

			return Compare (this, (DateTime) v);
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

		public static DateTime FromFileTime (long fileTime) 
		{
			return new DateTime (w32file_epoch + fileTime);
		}

		// TODO: Implement me.
		[MonoTODO]
		public static DateTime FromOADate (double d)
		{
				return new DateTime(0);
		}
		
		// TODO: Implement me.
		[MonoTODO]
		public string[] GetDateTimeFormats() 
		{
			return null;
		}

		//TODO: implement me
		[MonoTODO]
		public string[] GetDateTimeFormats(char format)
		{
			return null;
		}
		
		// TODO: implement me
		[MonoTODO]
		public string[] GetDateTimeFormats(IFormatProvider provider)
		{
			return null;
		}

		//TODO: implement me 
		[MonoTODO]
		public string[] GetDateTimeFormats(char format,IFormatProvider provider	)
		{
			return null;
		}

		public override int GetHashCode ()
		{
			return (int) ticks.Ticks;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DateTime;
		}

		public static bool IsLeapYear (int year)
		{
			return  ( (year % 4 == 0 && year % 100 != 0) || year % 400 == 0) ;
		}

		[MonoTODO]
		public static DateTime Parse (string s)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime ParseExact(string s,	string format, IFormatProvider provider	)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style )
		{
			// TODO: Implement me
			return new DateTime (0);
		
		}

		[MonoTODO]
		public static DateTime ParseExact( string s, string[] formats, IFormatProvider provider,
						   DateTimeStyles style )
		{
			// TODO: Implement me
			return new DateTime (0);
		
		}
		
		public TimeSpan Subtract(DateTime dt)
		{   
			return new TimeSpan(ticks.Ticks) - dt.ticks;
		}

		public DateTime Subtract(TimeSpan ts)
		{
			TimeSpan newticks;

			newticks = (new TimeSpan (ticks.Ticks)) - ts;
			return new DateTime(true,newticks);
		}

		public long ToFileTime()
		{
			if(ticks.Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return(ticks.Ticks - w32file_epoch);
		}

		public string ToLongDateString()
		{
			return ToString ("D");
		}

		public string ToLongTimeString()
		{
			return ToString ("T");
		}

		[MonoTODO]
		public double ToOADate()
		{
			// TODO implement me 
			return 0;
		}

		public string ToShortDateString()
		{
			return ToString ("d");
		}

		public string ToShortTimeString()
		{
			return ToString ("t");
		}
		
		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		internal string _GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc)
		{
			String pattern, f1, f2;

			useutc = false;

			switch (format)
			{
			case 'd':
				pattern = dfi.ShortDatePattern;
				break;
			case 'D':
				pattern = dfi.LongDatePattern;
				break;
			case 'f':
				f1 = dfi.LongDatePattern;
				f2 = dfi.ShortTimePattern;
				pattern = String.Concat (f1, " ");
				pattern = String.Concat (pattern, f2);
				break;
			case 'F':
				pattern = dfi.FullDateTimePattern;
				break;
			case 'g':
				f1 = dfi.ShortDatePattern;
				f2 = dfi.ShortTimePattern;
				pattern = String.Concat (f1, " ");
				pattern = String.Concat (pattern, f2);
				break;
			case 'G':
				f1 = dfi.ShortDatePattern;
				f2 = dfi.LongTimePattern;
				pattern = String.Concat (f1, " ");
				pattern = String.Concat (pattern, f2);
				break;
			case 'm':
			case 'M':
				pattern = dfi.MonthDayPattern;
				break;
			case 'r':
			case 'R':
				pattern = dfi.RFC1123Pattern;
				break;
			case 's':
				pattern = dfi.SortableDateTimePattern;
				break;
			case 't':
				pattern = dfi.ShortTimePattern;
				break;
			case 'T':
				pattern = dfi.LongTimePattern;
				break;
			case 'u':
				pattern = dfi.UniversalSortableDateTimePattern;
				useutc = true;
				break;
			case 'U':
				f1 = dfi.LongDatePattern;
				f2 = dfi.LongTimePattern;
				pattern = String.Concat (f1, " ");
				pattern = String.Concat (pattern, f2);
				useutc = true;
				break;
			case 'y':
			case 'Y':
				pattern = dfi.YearMonthPattern;
				break;
			default:
				pattern = null;
				break;
			}

			Console.Write ("Pattern: ");
			Console.WriteLine (pattern);

			return pattern;
		}

		internal string _ToString (string format, DateTimeFormatInfo dfi)
		{
			String str = null, result = null;
			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			while (pos+num < len)
			{
				if (chars[pos] == '\'')
				{
					num = 1;
					while (pos+num < len)
					{
						if (chars[pos+num] == '\'')
							break;

						result = String.Concat (result, chars[pos+num]);

						num = num + 1;
					}
					if (pos+num > len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					pos = pos + num + 1;
					num = 0;
					continue;
				}
				else if (chars[pos] == '\\')
				{
					if (pos+1 >= len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					result = String.Concat (result, chars[pos]);
					pos = pos + 1;
					continue;
				}
				else if (chars[pos] == '%')
				{
					pos = pos + 1;
					continue;
				}


				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num]))
				{
					num = num + 1;
					continue;
				}

				switch (chars[pos])
				{
				case 'd':
					if (num == 0)
						str = Day.ToString ("d");
					else if (num == 1)
						str = Day.ToString ("d02");
					else if (num == 2)
						str = dfi.GetAbbreviatedDayName (DayOfWeek);
					else
					{
						str = dfi.GetDayName (DayOfWeek);
						num = 3;
					}
					break;
				case 'M':
					if (num == 0)
						str = Month.ToString ("d");
					else if (num == 1)
						str = Month.ToString ("d02");
					else if (num == 2)
						str = dfi.GetAbbreviatedMonthName (Month);
					else
					{
						str = dfi.GetMonthName (Month);
						num = 3;
					}
					break;
				case 'y':
					if (num == 0)
					{
						int shortyear = Year % 100;
						str = shortyear.ToString ("d");
					}
					else if (num < 3)
					{
						int shortyear = Year % 100;
						str = shortyear.ToString ("d02");
						num = 1;
					}
					else
					{
						str = Year.ToString ("d");
						num = 3;
					}
					break;
				case 'g':
					// FIXME
					break;
				case 'f':
					num = Math.Min (num, 6);

					long ms = (long) Millisecond;
					long exp = 10;
					for (int i = 0; i < num; i++)
						exp = exp * 10;
					long maxexp = TimeSpan.TicksPerMillisecond;

					exp = Math.Min (exp, maxexp);
					ms = ms * exp / maxexp;

					String prec = (num+1).ToString ("d02");
					str = ms.ToString (String.Concat ("d", prec));

					break;
				case 'h':
					if (num == 0)
					{
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d");
					}
					else
					{
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d02");
						num = 1;
					}
					break;
				case 'H':
					if (num == 0)
						str = Hour.ToString ("d");
					else
					{
						str = Hour.ToString ("d02");
						num = 1;
					}
					break;
				case 'm':
					if (num == 0)
						str = Minute.ToString ("d");
					else
					{
						str = Minute.ToString ("d02");
						num = 1;
					}
					break;
				case 's':
					if (num == 0)
						str = Second.ToString ("d");
					else
					{
						str = Second.ToString ("d02");
						num = 1;
					}
					break;
				case 't':
					if (Hour < 12)
						str = dfi.AMDesignator;
					else
						str = dfi.PMDesignator;

					if (num == 0)
						str = str.Substring (0,1);
					else
						num = 1;
					break;
				case 'z':
					if (num == 0)
					{
						int offset = utcoffset.Hours;
						str = offset.ToString ("d");
						if (offset > 0)
							str = String.Concat ("+", str);
					}
					else if (num == 1)
					{
						int offset = utcoffset.Hours;
						str = offset.ToString ("d02");
						if (offset > 0)
							str = String.Concat ("+", str);
					}
					else if (num == 2)
					{
						int offhour = utcoffset.Hours;
						int offminute = utcoffset.Minutes;
						str = offhour.ToString ("d02");
						str = String.Concat (str, dfi.TimeSeparator);
						str = String.Concat (str, offminute.ToString ("d02"));
						if (offhour > 0)
							str = String.Concat ("+", str);
						num = 2;
					}
					break;
				case ':':
					str = dfi.TimeSeparator;
					num = 1;
					break;
				case '/':
					str = dfi.DateSeparator;
					num = 1;
					break;
				default:
					str = String.Concat (chars [pos]);
					num = 0;
					break;
				}

				result = String.Concat (result, str);
						
				pos = pos + num + 1;
				num = 0;
			}

			return result;
		}

		[MonoTODO]
		public string ToString (string format, IFormatProvider fp)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance(fp);

			if (format == null)
				format = dfi.FullDateTimePattern;

			bool useutc = false;

			if (format.Length == 1) {
				char fchar = (format.ToCharArray ())[0];
				format = this._GetStandardPattern (fchar, dfi, out useutc);
			}

			if (useutc)
				return this.ToUniversalTime ()._ToString (format, dfi);
			else
				return this._ToString (format, dfi);
		}

		public DateTime ToLocalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			return new DateTime (true, ticks + offset);
		}

		public DateTime ToUniversalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			return new DateTime (true, ticks - offset);
		}

		/*  OPERATORS */

		public static DateTime operator +(DateTime d, TimeSpan t)
		{
			return new DateTime (true, d.ticks + t);
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
			return new TimeSpan((d1.ticks - d2.ticks).Ticks);
		}

		public static DateTime operator -(DateTime d,TimeSpan t)
		{
			return new DateTime (true, d.ticks - t);
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
		[MonoTODO]
		public System.DateTime ToDateTime(IFormatProvider provider)
		{
			return new System.DateTime(true,this.ticks);
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

		[CLSCompliant(false)]
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
		
		[CLSCompliant(false)]
		public UInt32 ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		[CLSCompliant(false)]
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
