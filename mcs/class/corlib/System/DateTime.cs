//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//   Martin Baulig (martin@gnome.org)
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
	[Serializable]
	public struct DateTime : IComparable, IFormattable, IConvertible
	{
		private TimeSpan ticks;

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
		public static readonly DateTime MinValue = new DateTime (false,0);

		private static string[] formats = {
			// For compatibility with MS's CLR, this format (which
			// doesn't have a one-letter equivalent) is parsed
			// too. It's important because it's used in XML
			// serialization.
			"yyyy-MM-ddTHH:mm:sszzz",
			// Full date and time
			"F", "G", "r", "s", "u", "U",
			// Full date and time, but no seconds
			"f", "g",
			// Only date
			"d", "D",
			// Only time
			"T", "t",
			// Only date, but no year
			"m",
			// Only date, but no day
			"y" 
		};

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
		{
			ticks = new TimeSpan (newticks);
			if (local) {
				TimeZone tz = TimeZone.CurrentTimeZone;

				TimeSpan utcoffset = tz.GetUtcOffset (this);

				ticks = ticks + utcoffset;
			}
			if (ticks.Ticks < MinValue.Ticks || ticks.Ticks > MaxValue.Ticks)
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
				throw new ArgumentOutOfRangeException ("Parameters describe an " +
									"unrepresentable DateTime.");

			ticks = new TimeSpan (AbsoluteDays(year,month,day), hour, minute, second, millisecond);
		}

		public DateTime (int year, int month, int day, Calendar calendar)
			: this (year, month, day, 0, 0, 0, 0, calendar)	{}

		
		public DateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (year, month, day, hour, minute, second, 0, calendar)	{}


		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
			: this (year, month, day, hour, minute, second, millisecond) 
		{
			if (calendar == null)
				throw new ArgumentNullException();
		}

		internal DateTime (bool check, TimeSpan value)
		{
			if (check && (value.Ticks < MinValue.Ticks || value.Ticks > MaxValue.Ticks))
			    throw new ArgumentOutOfRangeException ();

			ticks = value;
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
		internal static extern long GetNow ();

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
			get {
				DateTime now = Now;
				return new DateTime (now.Year, now.Month, now.Day);
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
			return new DateTime (true, w32file_epoch + fileTime);
		}

		public static DateTime FromOADate (double d)
		{
			// An OLE Automation date is implemented as a floating-point number
			// whose value is the number of days from midnight, 30 December 1899.

			// d must be negative 657435.0 through positive 2958466.0.

			if ((d < -657435.0) || (d > 2958466.0))
				throw new OverflowException();

			return (new DateTime(1899, 12, 30, 0, 0, 0)).AddDays(d);
		}

		public string[] GetDateTimeFormats() 
		{
			string[] result = new string[formats.Length];
			int index=0;
			foreach (string format in formats) {
				result [index] = this.ToString(format);
				index++;
			}
			return result;
		}

		public string[] GetDateTimeFormats(char format)
		{
			string[] result = new string[1];
			result[0] = this.ToString(format.ToString());
			return result;
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

		public static DateTime Parse (string s)
		{
			return Parse (s, null);
		}

		public static DateTime Parse (string s, IFormatProvider fp)
		{
			return Parse (s, null, DateTimeStyles.AllowWhiteSpaces);
		}

		public static DateTime Parse (string s, IFormatProvider fp, DateTimeStyles styles)
		{
			return ParseExact (s, formats, fp, styles);
		}

		public static DateTime ParseExact (string s, string format, IFormatProvider fp)
		{
			return ParseExact (s, format, fp, DateTimeStyles.None);
		}

		internal static int _ParseNumber (string s, int digits, bool leadingzero,
						  bool sloppy_parsing, out int num_parsed)
		{
			int number = 0, i;

			if (sloppy_parsing)
				leadingzero = false;

			if (!leadingzero) {
				int real_digits = 0;
				for (i = 0; i < s.Length && i < digits; i++) {
					if (!Char.IsDigit (s[i]))
						break;

					real_digits++;
				}

				digits = real_digits;
			}

			if (s.Length < digits) {
				num_parsed = -1;
				return 0;
			}

			for (i = 0; i < digits; i++) {
				char c = s[i];
				if (!Char.IsDigit (c)) {
					num_parsed = -1;
					return 0;
				}

				number = number * 10 + (byte) (c - '0');
			}

			num_parsed = digits;
			return number;
		}

		internal static int _ParseEnum (string s, string[] values, out int num_parsed)
		{
			int i;

			for (i = 0; i < values.Length; i++) {
				if (s.Length < values[i].Length)
					continue;
				String tmp = s.Substring (0, values[i].Length);
				if (String.Compare (tmp, values[i], true) == 0) {
					num_parsed = values[i].Length;
					return i;
				}
			}

			num_parsed = -1;
			return -1;
		}

		internal static bool _ParseString (string s, int maxlength, string value, out int num_parsed)
		{
			if (maxlength > 0)
				value = value.Substring (0, maxlength);

			s = s.Substring (0, value.Length);

			if (String.Compare (s, value, true) == 0) {
				num_parsed = value.Length;
				return true;
			}

			num_parsed = -1;
			return false;
		}

		internal static bool _DoParse (string s, string format, bool exact,
					       out DateTime result,
					       DateTimeFormatInfo dfi,
					       DateTimeStyles style)
		{
			bool useutc = false, use_localtime = true;
			bool sloppy_parsing = false;

			if (format.Length == 1)
				format = _GetStandardPattern (format[0], dfi, out useutc);

			if ((style & DateTimeStyles.AllowLeadingWhite) != 0) {
				format = format.TrimStart (null);

				s = s.TrimStart (null);
			}

			if ((style & DateTimeStyles.AllowTrailingWhite) != 0) {
				format = format.TrimEnd (null);
				s = s.TrimEnd (null);
			}

			if ((style & DateTimeStyles.AllowInnerWhite) != 0)
				sloppy_parsing = true;

			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			int day = -1, dayofweek = -1, month = -1, year = -1;
			int hour = -1, minute = -1, second = -1, millisecond = -1;
			int ampm = -1;
			int tzsign = -1, tzoffset = -1, tzoffmin = -1;

			result = new DateTime (0);
			while (pos+num < len)
			{
				if (s.Length == 0)
					break;

				if (Char.IsWhiteSpace (s[0])) {
					s = s.Substring (1);

					if (Char.IsWhiteSpace (chars[pos])) {
						pos++;
						continue;
					}

					if ((style & DateTimeStyles.AllowInnerWhite) == 0)
						return false;
				}

				if (chars[pos] == '\'') {
					num = 1;
					while (pos+num < len) {
						if (chars[pos+num] == '\'')
							break;

						if (s.Length == 0)
							return false;
						if (s[0] != chars[pos+num])
							return false;
						s = s.Substring (1);

						num++;
					}
					if (pos+num > len)
						return false;

					pos += num + 1;
					num = 0;
					continue;
				} else if (chars[pos] == '\\') {
					if (pos+1 >= len)
						return false;

					if (s[0] != chars[pos+num])
						return false;
					s = s.Substring (1);
					if (s.Length == 0)
						return false;

					pos++;
					continue;
				} else if (chars[pos] == '%') {
					pos++;
					continue;
				}

				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num])) {
					num++;
					continue;
				}

				int num_parsed = 0;

				switch (chars[pos])
				{
				case 'd':
					if (day != -1)
						return false;
					if (num == 0)
						day = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						day = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
					else if (num == 2)
						dayofweek = _ParseEnum (s, dfi.AbbreviatedDayNames, out num_parsed);
					else
					{
						dayofweek = _ParseEnum (s, dfi.DayNames, out num_parsed);
						num = 3;
					}
					break;
				case 'M':
					if (month != -1)
						return false;
					if (num == 0)
						month = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						month = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
					else if (num == 2)
						month = _ParseEnum (s, dfi.AbbreviatedMonthNames , out num_parsed) + 1;
					else
					{
						month = _ParseEnum (s, dfi.MonthNames, out num_parsed) + 1;
						num = 3;
					}
					break;
				case 'y':
					if (year != -1)
						return false;

					if (num == 0) {
						year = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
						year += (year < 30) ? 2000 : 1900;
					} else if (num < 3) {
						year = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						year += (year < 30) ? 2000 : 1900;
					} else {
						year = _ParseNumber (s, 4, false, sloppy_parsing, out num_parsed);
						num = 3;
					}
					// if there is another digit next to the ones we just parsed, then the year value
					// is too big for sure.
					//if (num_parsed < s.Length && Char.IsDigit(s[num_parsed]) || (year != 0 && (year < 1 || year > 9999)))
					if (year != 0 && (year < 1 || year > 9999))
						throw new ArgumentOutOfRangeException ("year", "Valid " + 
								"values are between 1 and 9999 inclusive");
					break;
				case 'h':
					if (hour != -1)
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						num = 1;
					}

					if (hour >= 12)
						return false;

					break;
				case 'H':
					if ((hour != -1) || (ampm >= 0))
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						num = 1;
					}
					if (hour >= 24)
						return false;

					ampm = -2;
					break;
				case 'm':
					if (minute != -1)
						return false;
					if (num == 0)
						minute = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else
					{
						minute = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						num = 1;
					}
					if (minute >= 60)
						return false;

					break;
				case 's':
					if (second != -1)
						return false;
					if (num == 0)
						second = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else
					{
						second = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						num = 1;
					}
					if (second >= 60)
						return false;

					break;
				case 'f':
					if (millisecond != -1)
						return false;
					num = Math.Min (num, 6);
					millisecond = _ParseNumber (s, num+1, true, sloppy_parsing, out num_parsed);
					if (millisecond >= 1000)
						return false;
					break;
				case 't':
					if (ampm != -1)
						return false;
					if (num == 0)
					{
						if (_ParseString (s, 1, dfi.AMDesignator, out num_parsed))
							ampm = 0;
						else if (_ParseString (s, 1, dfi.PMDesignator, out num_parsed))
							ampm = 1;
						else
							return false;
					}
					else
					{
						if (_ParseString (s, 0, dfi.AMDesignator, out num_parsed))
							ampm = 0;
						else if (_ParseString (s, 0, dfi.PMDesignator, out num_parsed))
							ampm = 1;
						else
							return false;
						num = 1;
					}
					break;
				case 'z':
					if (tzsign != -1)
						return false;
					if (s[0] == '+')
						tzsign = 0;
					else if (s[0] == '-')
						tzsign = 1;
					else
						return false;
					s = s.Substring (1);
					if (num == 0)
						tzoffset = _ParseNumber (s, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						tzoffset = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
					else
					{
						tzoffset = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						if (num_parsed < 0)
							return false;
						s = s.Substring (num_parsed);
						if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
							return false;
						s = s.Substring (num_parsed);
						tzoffmin = _ParseNumber (s, 2, true, sloppy_parsing, out num_parsed);
						if (num_parsed < 0)
							return false;
						num = 2;
					}
					break;
				case ':':
					if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
						return false;
					break;
				case '/':
					if (!_ParseString (s, 0, dfi.DateSeparator, out num_parsed))
						return false;
					break;
				default:
					if (s[0] != chars[pos])
						return false;
					num = 0;
					num_parsed = 1;
					break;
				}

				if (num_parsed < 0)
					return false;

				s = s.Substring (num_parsed);

				pos = pos + num + 1;
				num = 0;
			}

			if (hour == -1)
				hour = 0;
			if (minute == -1)
				minute = 0;

			if (second == -1)
				second = 0;
			if (millisecond == -1)
				millisecond = 0;

			// If no date was given
			if ((day == -1) && (month == -1) && (year == -1)) {
				if ((style & DateTimeStyles.NoCurrentDateDefault) != 0) {
					day = 1;
					month = 1;
					year = 1;
				} else {
					day = Today.Day;
					month = Today.Month;
					year = Today.Year;
				}
			}


			if (day == -1)
				day = 1;
			if (month == -1)
				month = 1;
			if (year == -1) {
				if ((style & DateTimeStyles.NoCurrentDateDefault) != 0)
					year = 1;
				else
					year = Today.Year;
			}

			if (ampm == 1)
				hour = hour + 12;

			result = new DateTime (year, month, day, hour, minute, second, millisecond);

			if ((dayofweek != -1) && (dayofweek != (int) result.DayOfWeek))
				throw new FormatException (Locale.GetText ("String was not recognized as valid DateTime because the day of week was incorrect."));

			// If no timezone was specified, default to the local timezone.
			TimeSpan utcoffset;

			if (useutc)
				utcoffset = new TimeSpan (0, 0, 0);
			else if (tzsign == -1) {
				TimeZone tz = TimeZone.CurrentTimeZone;
				utcoffset = tz.GetUtcOffset (result);
			} else {
				if ((style & DateTimeStyles.AdjustToUniversal) != 0)
					use_localtime = false;

				if (tzoffmin == -1)
					tzoffmin = 0;
				if (tzoffset == -1)
					tzoffset = 0;
				if (tzsign == 1)
					tzoffset = -tzoffset;

				utcoffset = new TimeSpan (tzoffset, tzoffmin, 0);
			}

			long newticks = (result.ticks - utcoffset).Ticks;

			result = new DateTime (use_localtime, newticks);

			return true;
		}


		public static DateTime ParseExact (string s, string format,
						   IFormatProvider fp, DateTimeStyles style)
		{
			string[] formats;

			formats = new string [1];
			formats[0] = format;

			return ParseExact (s, formats, fp, style);
		}

		public static DateTime ParseExact (string s, string[] formats,
						   IFormatProvider fp,
						   DateTimeStyles style)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (fp);

			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));
			if (formats.Length == 0)
				throw new ArgumentNullException (Locale.GetText ("format is null"));

			int i;
			for (i = 0; i < formats.Length; i++)
			{
				DateTime result;

				if (_DoParse (s, formats[i], true, out result, dfi, style))
					return result;
			}

			throw new FormatException ();
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
			DateTime universalTime = ToUniversalTime();
			
			if (universalTime.Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return(universalTime.Ticks - w32file_epoch);
		}

		public string ToLongDateString()
		{
			return ToString ("D");
		}

		public string ToLongTimeString()
		{
			return ToString ("T");
		}

		public double ToOADate()
		{
			DateTime p = new DateTime(1899, 12, 30, 0, 0, 0);
			TimeSpan t = new TimeSpan (this.Ticks - p.Ticks);
			return t.TotalDays;
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
			return ToString ("G", null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		internal static string _GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc)
		{
			String pattern;

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
				pattern = dfi.LongDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'F':
				pattern = dfi.FullDateTimePattern;
				break;
			case 'g':
				pattern = dfi.ShortDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'G':
				pattern = dfi.ShortDatePattern + " " + dfi.LongTimePattern;
				break;
			case 'm':
			case 'M':
				pattern = dfi.MonthDayPattern;
				break;
			case 'r':
			case 'R':
				pattern = dfi.RFC1123Pattern;
				// commented by LP 09/jun/2002, rfc 1123 pattern is always in GMT
				// useutc= true;
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
				pattern = dfi.LongDatePattern + " " + dfi.LongTimePattern;
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

			return pattern;
		}

		internal string _ToString (string format, DateTimeFormatInfo dfi)
		{
			String str = null, result = null;
			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			TimeZone tz = TimeZone.CurrentTimeZone;
			TimeSpan utcoffset = tz.GetUtcOffset (this);

			while (pos < len)
			{
				if (chars[pos] == '\'') {
					num = 1;
					while (pos+num <= len) {
						if (chars[pos+num] == '\'')
							break;

						result += chars[pos+num];
						num++;
					}
					if (pos+num > len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					pos += num+1;
					num = 0;
					continue;
				} else if (chars[pos] == '\\') {
					if (pos+1 >= len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					result += chars[pos+1];
					pos += 2;
					continue;
				} else if (chars[pos] == '%') {
					pos++;
					continue;
				}

				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num])) {
					num++;
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
					else {
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
					else {
						str = dfi.GetMonthName (Month);
						num = 3;
					}
					break;
				case 'y':
					if (num == 0) {
						int shortyear = Year % 100;
						str = shortyear.ToString ("d");
					} else if (num == 1) {
						int shortyear = Year % 100;
						str = shortyear.ToString ("d02");
					} else {
						str = Year.ToString ("d04");
						num = 3;
					}
					break;
				case 'g':
					// FIXME
					break;
				case 'f':
					num = Math.Min (num, 6);

					long relativeTicks = ticks.Ticks % TimeSpan.TicksPerSecond;

					long exp = 10;
					for (int i = 0; i < num; i++)
						exp = exp * 10;

					long frac = relativeTicks * exp / TimeSpan.TicksPerSecond;

					String prec = (num+1).ToString ("d02");
					str = frac.ToString (String.Concat ("d", prec));

					break;
				case 'h':

					if (num == 0) {
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d");
					} else {
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d02");
						num = 1;
					}
					break;
				case 'H':
					if (num == 0)
						str = Hour.ToString ("d");
					else {
						str = Hour.ToString ("d02");
						num = 1;
					}
					break;
				case 'm':
					if (num == 0)
						str = Minute.ToString ("d");
					else {
						str = Minute.ToString ("d02");
						num = 1;
					}
					break;
				case 's':
					if (num == 0)
						str = Second.ToString ("d");
					else {
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
					if (num == 0) {
						int offset = utcoffset.Hours;
						str = offset.ToString ("d");
						str = String.Concat ((offset >= 0) ? "+" : "", str);
					} 
					else if (num == 1) 
					{
						int offset = utcoffset.Hours;
						str = offset.ToString ("d02");
						str = String.Concat ((offset >= 0) ? "+" : "", str);
					} 
					else if (num == 2) 
					{
						int offhour = utcoffset.Hours;
						int offminute = utcoffset.Minutes;
						str = offhour.ToString ("d02");
						str = String.Concat (str, dfi.TimeSeparator);
						str = String.Concat (str, offminute.ToString ("d02"));
						str = String.Concat ((offhour >= 0) ? "+" : "", str);
						num = 2;
					}
					break;
				case ':':
					str = dfi.TimeSeparator;
					num = 0;
					break;
				case '/':
					str = dfi.DateSeparator;
					num = 0;
					break;
				default:
					str = String.Concat (chars [pos]);
					num = 0;
					break;
				}

				result = String.Concat (result, str);
						
				pos += num + 1;
				num = 0;
			}

			return result;
		}

		public string ToString (string format, IFormatProvider fp)

		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance(fp);

			if (format == null)
				format = dfi.FullDateTimePattern;

			bool useutc = false;

			if (format.Length == 1) {
				char fchar = (format.ToCharArray ())[0];
				format = _GetStandardPattern (fchar, dfi, out useutc);
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

			return new DateTime (false, ticks - offset);
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

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		byte IConvertible.ToByte(IFormatProvider provider)
		{
			throw new InvalidCastException();

		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		// TODO Implement me
		[MonoTODO]
		public System.DateTime ToDateTime(IFormatProvider provider)
		{
			return new System.DateTime(true,this.ticks);
		} 
		
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			 throw new InvalidCastException();
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		Int16 IConvertible.ToInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		Int32 IConvertible.ToInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		Int64 IConvertible.ToInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		[CLSCompliant(false)]
		SByte IConvertible.ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		Single IConvertible.ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		object IConvertible.ToType(Type conversionType,IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		UInt16 IConvertible.ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		[CLSCompliant(false)]
		UInt32 IConvertible.ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		[CLSCompliant(false)]
		UInt64 IConvertible.ToUInt64(IFormatProvider provider)

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
