//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//   Martin Baulig (martin@gnome.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
//   (C) 2001 Marcel Narings

//
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
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
	/// <summary>
	/// The DateTime structure represents dates and time ranging from
	/// 1-1-0001 12:00:00 AM to 31-12-9999 23:59:00 Common Era.
	/// </summary>
	/// 
	[Serializable]
	[StructLayout (LayoutKind.Auto)]
	public struct DateTime : IFormattable, IConvertible,
#if NET_2_0
		IComparable, IComparable<DateTime>
#else
		IComparable
#endif
	{
		private TimeSpan ticks;

		private const int dp400 = 146097;
		private const int dp100 = 36524;
		private const int dp4 = 1461;

		// w32 file time starts counting from 1/1/1601 00:00 GMT
		// which is the constant ticks from the .NET epoch
		private const long w32file_epoch = 504911232000000000L;

		//private const long MAX_VALUE_TICKS = 3155378975400000000L;
		// -- Microsoft .NET has this value.
		private const long MAX_VALUE_TICKS = 3155378975999999999L;

		//
		// The UnixEpoch, it begins on Jan 1, 1970 at 0:0:0, expressed
		// in Ticks
		//
		internal const long UnixEpoch = 621355968000000000L;

		// for OLE Automation dates
		private const long ticks18991230 = 599264352000000000L;
		private const double OAMinValue = -657435.0d;
		private const double OAMaxValue = 2958466.0d;

		public static readonly DateTime MaxValue = new DateTime (false, MAX_VALUE_TICKS);
		public static readonly DateTime MinValue = new DateTime (false, 0);

		private static readonly string[] commonFormats = {
			// For compatibility with MS's CLR, this format (which
			// doesn't have a one-letter equivalent) is parsed
			// too. It's important because it's used in XML
			// serialization.

			// Note that those format should be tried only for
			// invalid patterns; 

			// FIXME: SOME OF those patterns looks tried against 
			// the current culture, since some patterns fail in 
			// some culture.

			"yyyy-MM-dd",
			"yyyy-MM-ddTHH:mm:sszzz",
			"yyyy-MM-ddTHH:mm:ss.fffffff",
			"yyyy-MM-ddTHH:mm:ss.fffffffzzz",
			// UTC / allow any separator
			"yyyy/MM/ddTHH:mm:ssZ",
			"yyyy/M/dZ",
			// bug #58938
			"yyyy/M/d HH:mm:ss",
			// bug #47720
			"yyyy/MM/dd HH:mm:ss 'GMT'",
			// Close to RFC1123, but without 'GMT'
			"ddd, d MMM yyyy HH:mm:ss",
			// use UTC ('Z'; not literal "'Z'")
			// FIXME: 1078(af-ZA) and 1079(ka-GE) reject it
			"yyyy/MM/dd HH':'mm':'ssZ", 

			// DayOfTheWeek, dd full_month_name yyyy
			// FIXME: 1054(th-TH) rejects it
			"dddd, dd MMMM yyyy",
			// DayOfTheWeek, dd yyyy. This works for every locales.
			"MMMM dd, yyyy",
#if NET_1_1
			// X509Certificate pattern is accepted by Parse() *in every culture*
			"yyyyMMddHHmmssZ",
#endif
			// In Parse() the 'r' equivalent pattern is first parsed as universal time
			"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
/*
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
*/
		};

		private enum Which 
		{
			Day,
			DayYear,
			Month,
			Year
		};
	
		private static readonly int[] daysmonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
		private static readonly int[] daysmonthleap = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	

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
				second < 0 || second > 59 ||
				millisecond < 0 || millisecond > 999)
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
			return AddTicks (ts.Ticks);
		}

		public DateTime AddDays (double days)
		{
			return AddMilliseconds (Math.Round (days * 86400000));
		}
		
		public DateTime AddTicks (long t)
		{
			if ((t + ticks.Ticks) > MAX_VALUE_TICKS || (t + ticks.Ticks) < 0) {
				throw new ArgumentOutOfRangeException();
			}
			return new DateTime (t + ticks.Ticks);
		}

		public DateTime AddHours (double hours)
		{
			return AddMilliseconds (hours * 3600000);
		}

		public DateTime AddMilliseconds (double ms)
		{
			if ((ms * TimeSpan.TicksPerMillisecond) > long.MaxValue ||
					(ms * TimeSpan.TicksPerMillisecond) < long.MinValue) {
				throw new ArgumentOutOfRangeException();
			}
			long msticks = (long) (ms * TimeSpan.TicksPerMillisecond);

			return AddTicks (msticks);
		}

		// required to match MS implementation for OADate (OLE Automation)
		private DateTime AddRoundedMilliseconds (double ms)
		{
			if ((ms * TimeSpan.TicksPerMillisecond) > long.MaxValue ||
				(ms * TimeSpan.TicksPerMillisecond) < long.MinValue) {
 				throw new ArgumentOutOfRangeException ();
 			}
			long msticks = (long) (ms += ms > 0 ? 0.5 : -0.5) * TimeSpan.TicksPerMillisecond;

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

#if NET_2_0
		public int CompareTo (DateTime value)
		{
			return Compare (this, value);
		}

		public bool Equals (DateTime value)
		{
			return value.ticks == ticks;
		}
#endif

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
			if (fileTime < 0)
				throw new ArgumentOutOfRangeException ("fileTime", "< 0");

			return new DateTime (true, w32file_epoch + fileTime);
		}

#if NET_1_1
		public static DateTime FromFileTimeUtc (long fileTime) 
		{
			if (fileTime < 0)
				throw new ArgumentOutOfRangeException ("fileTime", "< 0");

			return new DateTime (false, w32file_epoch + fileTime);
		}
#endif

		public static DateTime FromOADate (double d)
		{
			// An OLE Automation date is implemented as a floating-point number
			// whose value is the number of days from midnight, 30 December 1899.

			// d must be negative 657435.0 through positive 2958466.0.
			if ((d <= OAMinValue) || (d >= OAMaxValue))
				throw new ArgumentException ("d", "[-657435,2958466]");

			DateTime dt = new DateTime (ticks18991230);
			if (d < 0.0d) {
				Double days = Math.Ceiling (d);
				// integer part is the number of days (negative)
				dt = dt.AddRoundedMilliseconds (days * 86400000);
				// but decimals are the number of hours (in days fractions) and positive
				Double hours = (days - d);
				dt = dt.AddRoundedMilliseconds (hours * 86400000);
			}
			else {
				dt = dt.AddRoundedMilliseconds (d * 86400000);
			}

			return dt;
		}

		public string[] GetDateTimeFormats() 
		{
			return GetDateTimeFormats (CultureInfo.CurrentCulture);
		}

		public string[] GetDateTimeFormats(char format)
		{
			if ("dDgGfFmMrRstTuUyY".IndexOf (format) < 0)
				throw new FormatException ("Invalid format character.");
			string[] result = new string[1];
			result[0] = this.ToString(format.ToString());
			return result;
		}
		
		public string[] GetDateTimeFormats(IFormatProvider provider)
		{
			DateTimeFormatInfo info = (DateTimeFormatInfo) provider.GetFormat (typeof(DateTimeFormatInfo));
//			return GetDateTimeFormats (info.GetAllDateTimePatterns ());
			ArrayList al = new ArrayList ();
			foreach (char c in "dDgGfFmMrRstTuUyY")
				al.AddRange (GetDateTimeFormats (c, info));
			return al.ToArray (typeof (string)) as string [];
		}

		public string[] GetDateTimeFormats(char format,IFormatProvider provider	)
		{
			if ("dDgGfFmMrRstTuUyY".IndexOf (format) < 0)
				throw new FormatException ("Invalid format character.");

			// LAMESPEC: There is NO assurance that 'U' ALWAYS
			// euqals to 'F', but since we have to iterate all
			// the pattern strings, we cannot just use 
			// ToString("U", provider) here. I believe that the 
			// method's behavior cannot be formalized.
			bool adjustutc = false;
			switch (format) {
			case 'U':
//			case 'r':
//			case 'R':
//			case 'u':
				adjustutc = true;
				break;
			}
			DateTimeFormatInfo info = (DateTimeFormatInfo) provider.GetFormat (typeof(DateTimeFormatInfo));
			return GetDateTimeFormats (adjustutc, info.GetAllDateTimePatterns (format), info);
		}

		private string [] GetDateTimeFormats (bool adjustutc, string [] patterns, DateTimeFormatInfo dfi)
		{
			string [] results = new string [patterns.Length];
			DateTime val = adjustutc ? ToUniversalTime () : this;
			for (int i = 0; i < results.Length; i++)
				results [i] = val._ToString (patterns [i], dfi);
			return results;
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
			return Parse (s, fp, DateTimeStyles.AllowWhiteSpaces);
		}

		[MonoTODO ("see the comments inline")]
		public static DateTime Parse (string s, IFormatProvider fp, DateTimeStyles styles)
		{
			// This method should try only expected patterns. 
			// Should not try extra patterns.
			// Right now we also try InvariantCulture, but I
			// confirmed in some cases this method rejects what
			// InvariantCulture supports (can be checked against
			// "th-TH" with Gregorian Calendar). So basically it
			// should not be done.
			// I think it should be CurrentCulture to be tested,
			// but right now we don't support all the supported
			// patterns for each culture, so try InvariantCulture
			// as a quick remedy.
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));
			DateTime result;

			if (fp == null)
				fp = CultureInfo.CurrentCulture;
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (fp);

			bool longYear = false;
			// Try common formats.
			if (ParseExact (s, commonFormats, dfi, styles, out result, false, ref longYear))
				return result;

			// Try common formats, also with invariant culture
			if (ParseExact (s, commonFormats, DateTimeFormatInfo.InvariantInfo, styles, out result, false, ref longYear))
				return result;

			// Next, try all the patterns
			string [] patterns = new string [] {"d", "D", "g", "G", "f", "F", "m", "M", "r", "R", "s", "t", "T", "u", "U", "y", "Y"};

			if (ParseExact (s, patterns, dfi, styles, out result, false, ref longYear))
				return result;

			if (longYear) {
				throw new ArgumentOutOfRangeException ("year",
					"Valid values are between 1 and 9999 inclusive");
			}

			throw new FormatException ("String was not recognized as a valid DateTime.");
		}

		public static DateTime ParseExact (string s, string format, IFormatProvider fp)
		{
			return ParseExact (s, format, fp, DateTimeStyles.None);
		}

		internal static int _ParseNumber (string s, int digits,
						  bool leadingzero,
						  bool sloppy_parsing,
						  bool next_not_digit,
						  out int num_parsed)
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

			if (s.Length > digits &&
			    next_not_digit &&
			    Char.IsDigit (s[digits])) {
				/* More digits left over */
				num_parsed = -1;
				return(0);
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
				else if (values [i].Length == 0)
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
			if (maxlength <= 0)
				maxlength = value.Length;

			if (String.Compare (s, 0, value, 0, maxlength, true, CultureInfo.InvariantCulture) == 0) {
				num_parsed = maxlength;
				return true;
			}

			num_parsed = -1;
			return false;
		}

		internal static bool _DoParse (string s, string format, bool exact,
					       out DateTime result,
					       DateTimeFormatInfo dfi,
					       DateTimeStyles style,
					       ref bool longYear)
		{
			bool useutc = false, use_localtime = true;
			bool use_invariant = false;
			bool sloppy_parsing = false;
			if (format.Length == 1)
				format = _GetStandardPattern (format [0], dfi, out useutc, out use_invariant);
			else if (!exact && format.IndexOf ("GMT") >= 0)
				useutc = true;

			if ((style & DateTimeStyles.AllowLeadingWhite) != 0) {
				format = format.TrimStart (null);

				s = s.TrimStart (null);
			}

			if ((style & DateTimeStyles.AllowTrailingWhite) != 0) {
				format = format.TrimEnd (null);
				s = s.TrimEnd (null);
			}

			if (use_invariant)
				dfi = DateTimeFormatInfo.InvariantInfo;

			if ((style & DateTimeStyles.AllowInnerWhite) != 0)
				sloppy_parsing = true;

			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			int day = -1, dayofweek = -1, month = -1, year = -1;
			int hour = -1, minute = -1, second = -1;
			double fractionalSeconds = -1;
			int ampm = -1;
			int tzsign = -1, tzoffset = -1, tzoffmin = -1;
			bool next_not_digit;

			result = new DateTime (0);
			while (pos+num < len)
			{
				if (s.Length == 0)
					break;

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
				} else if (chars[pos] == '"') {
					num = 1;
					while (pos+num < len) {
						if (chars[pos+num] == '"')
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
				} else if (Char.IsWhiteSpace (s[0])) {
					s = s.Substring (1);

					if (Char.IsWhiteSpace (chars[pos])) {
						pos++;
						continue;
					}

					if (exact && (style & DateTimeStyles.AllowInnerWhite) == 0)
						return false;
					int ws = 0;
					while (ws < s.Length) {
						if (Char.IsWhiteSpace (s [ws]))
							ws++;
						else
							break;
					}
					s = s.Substring (ws);
				}


				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num])) {
					num++;
					continue;
				}

				int num_parsed = 0;

				if (pos+num+1 < len) {
					char next_char = chars[pos+num+1];
					
					next_not_digit = !(next_char == 'd' ||
							   next_char == 'M' ||
							   next_char == 'y' ||
							   next_char == 'h' ||
							   next_char == 'H' ||
							   next_char == 'm' ||
							   next_char == 's' ||
							   next_char == 'f' ||
							   next_char == 'z' ||
							   next_char == '"' ||
							   next_char == '\'' ||
							   Char.IsDigit (next_char));
				} else {
					next_not_digit = true;
				}
				
				switch (chars[pos])
				{
				case 'd':
					if (day != -1)
						return false;
					if (num == 0)
						day = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else if (num == 1)
						day = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
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
						month = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else if (num == 1)
						month = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
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
						year = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					} else if (num < 3) {
						year = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
					} else {
						year = _ParseNumber (s, 4, false, sloppy_parsing, next_not_digit, out num_parsed);
						if ((year >= 1000) && (num_parsed == 4) && (!longYear) && (s.Length > 4)) {
							int np = 0;
							int ly = _ParseNumber (s, 5, false, sloppy_parsing, next_not_digit, out np);
							longYear = (ly > 9999);
						}
						num = 3;
					}

					//FIXME: We should do use dfi.Calendat.TwoDigitYearMax
					if (num_parsed <= 2)
						year += (year < 30) ? 2000 : 1900;
					break;
				case 'h':
					if (hour != -1)
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
						num = 1;
					}

					if (hour > 12)
						return false;
					if (hour == 12)
						hour = 0;

					break;
				case 'H':
					if ((hour != -1) || (ampm >= 0))
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
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
						minute = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else
					{
						minute = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
						num = 1;
					}
					if (minute >= 60)
						return false;

					break;
				case 's':
					if (second != -1)
						return false;
					if (num == 0)
						second = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else
					{
						second = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
						num = 1;
					}
					if (second >= 60)
						return false;

					break;
				case 'f':
					if (fractionalSeconds != -1)
						return false;
					num = Math.Min (num, 6);
					double decimalNumber = (double) _ParseNumber (s, num+1, true, sloppy_parsing, next_not_digit, out num_parsed);
					if (num_parsed == -1)
						return false;

					else
						fractionalSeconds = decimalNumber / Math.Pow(10.0, num_parsed);
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
						tzoffset = _ParseNumber (s, 2, false, sloppy_parsing, next_not_digit, out num_parsed);
					else if (num == 1)
						tzoffset = _ParseNumber (s, 2, true, sloppy_parsing, false, out num_parsed);
					else
					{
						tzoffset = _ParseNumber (s, 2, true, sloppy_parsing, next_not_digit, out num_parsed);
						if (num_parsed < 0)
							return false;
						s = s.Substring (num_parsed);
						if (Char.IsDigit (s [0]))
							num_parsed = 0;
						else if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
							return false;
						s = s.Substring (num_parsed);
						tzoffmin = _ParseNumber (s, 2, true, sloppy_parsing, false, out num_parsed);
						if (num_parsed < 0)
							return false;
						num = 2;
					}
					break;

				// LAMESPEC: This should be part of UTCpattern
				// string and thus should not be considered here.
				//
				// Note that 'Z' is not defined as a pattern
				// character. Keep it for X509 certificate
				// verification. Also, "Z" != "'Z'" under MS.NET
				// ("'Z'" is just literal; handled above)
				case 'Z':
					if (s [0] != 'Z')
						return false;
					num = 0;
					num_parsed = 1;
					useutc = true;
					break;

				case ':':
					if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
						return false;
					break;
				case '/':
					/* Accept any character for
					 * DateSeparator, except
					 * TimeSeparator, a digit or a
					 * letter.  Not documented,
					 * but seems to be MS
					 * behaviour here.  See bug
					 * 54047.
					 */
					if (exact && s [0] != '/')
						return false;

					if (_ParseString (s, 0,
							  dfi.TimeSeparator,
							  out num_parsed) ||
					    Char.IsDigit (s[0]) ||
					    Char.IsLetter (s[0])) {
						return(false);
					}

					num = 0;
					if (num_parsed <= 0) {
						num_parsed = 1;
					}
					
					break;
				default:
					if (s[0] != chars[pos]) {
						// FIXME: It is not sure, but
						// IsLetter() is introduced 
						// because we have to reject 
						// "2002a02b25" but have to
						// allow "2002$02$25". The same
						// thing applies to '/' case.
						if (exact ||
							Char.IsDigit (s [0]) ||
							Char.IsLetter (s [0]))
							return false;
					}
					num = 0;
					num_parsed = 1;
					break;
				}

				if (num_parsed < 0)
					return false;

				s = s.Substring (num_parsed);

				if (!exact) {
					switch (chars [pos]) {
					case 'm':
					case 's':
					case 'f':
					case 'z':
						if (s.Length > 0 && s [0] == 'Z'
						    && (pos + 1 == chars.Length
						    || chars [pos + 1] != 'Z')) {
							useutc = true;
							s = s.Substring (1);
						}
						break;
					}
				}

				pos = pos + num + 1;
				num = 0;
			}

			if (exact && pos < len)
				return false;

			if (s.Length != 0) // extraneous tail.
				return false;

			if (hour == -1)
				hour = 0;
			if (minute == -1)
				minute = 0;

			if (second == -1)
				second = 0;
			if (fractionalSeconds == -1)
				fractionalSeconds = 0;

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
			
			// For anything out of range 
			// return false
			if ( year < 1 || year > 9999 || 
			month < 1 || month >12  ||
			day < 1 || day > DaysInMonth(year, month) ||
			hour < 0 || hour > 23 ||
			minute < 0 || minute > 59 ||
			second < 0 || second > 59 )
				return false;

			result = new DateTime (year, month, day, hour, minute, second, 0);
			result = result.AddSeconds(fractionalSeconds);

//Console.WriteLine ("**** Parsed as {1} {0} {2}", new object [] {useutc ? "[u]" : "", format, use_localtime ? "[lt]" : ""});
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
			if (formats == null || formats.Length == 0)
				throw new ArgumentNullException (Locale.GetText ("format is null"));

			DateTime result;
			bool longYear = false;
			if (!ParseExact (s, formats, dfi, style, out result, true, ref longYear))
				throw new FormatException ();
			return result;
		}
		
		private static bool ParseExact (string s, string [] formats,
			DateTimeFormatInfo dfi, DateTimeStyles style, out DateTime ret,
			bool exact, ref bool longYear)
		{
			int i;
			for (i = 0; i < formats.Length; i++)
			{
				DateTime result;

				if (_DoParse (s, formats[i], exact, out result, dfi, style, ref longYear)) {
					ret = result;
					return true;
				}
			}
			ret = DateTime.MinValue;
			return false;
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

#if NET_1_1
		public long ToFileTimeUtc()
		{
			if (Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return (Ticks - w32file_epoch);
		}
#endif

		public string ToLongDateString()
		{
			return ToString ("D");
		}

		public string ToLongTimeString()
		{
			return ToString ("T");
		}

		public double ToOADate ()
		{
			long t = this.Ticks;
			// uninitialized DateTime case
			if (t == 0)
				return 0;
			// we can't reach minimum value
			if (t < 31242239136000000)
				return OAMinValue + 0.001;

			TimeSpan ts = new TimeSpan (this.Ticks - ticks18991230);
			double result = ts.TotalDays;
			// t < 0 (where 599264352000000000 == 0.0d for OA)
			if (t < 599264352000000000) {
				// negative days (int) but decimals are positive
				double d = Math.Ceiling (result);
				result = d - 2 - (result - d);
			}
			else {
				// we can't reach maximum value
				if (result >= OAMaxValue)
					result = OAMaxValue - 0.00000001d;
			}
			return result;
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

		internal static string _GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc, out bool use_invariant)
		{
			String pattern;

			useutc = false;
			use_invariant = false;

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
				// uncommented by AE 27/may/2004
//				useutc = true;
				use_invariant = true;
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
//				pattern = dfi.LongDatePattern + " " + dfi.LongTimePattern;
				pattern = dfi.FullDateTimePattern;
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
			// the length of the format is usually a good guess of the number
			// of chars in the result. Might save us a few bytes sometimes
			// Add + 10 for cases like mmmm dddd
			StringBuilder result = new StringBuilder (format.Length + 10);

			// For some cases, the output should not use culture dependent calendar
			DateTimeFormatInfo inv = DateTimeFormatInfo.InvariantInfo;
			if (format == inv.RFC1123Pattern)
				dfi = inv;
			else if (format == inv.UniversalSortableDateTimePattern)
				dfi = inv;

			int i = 0;

			while (i < format.Length) {
				int tokLen;
				char ch = format [i];

				switch (ch) {

				//
				// Time Formats
				//
				case 'h':
					// hour, [1, 12]
					tokLen = CountRepeat (format, i, ch);

					int hr = this.Hour % 12;
					if (hr == 0)
						hr = 12;

					ZeroPad (result, hr, tokLen == 1 ? 1 : 2);
					break;
				case 'H':
					// hour, [0, 23]
					tokLen = CountRepeat (format, i, ch);
					ZeroPad (result, this.Hour, tokLen == 1 ? 1 : 2);
					break;
				case 'm':
					// minute, [0, 59]
					tokLen = CountRepeat (format, i, ch);
					ZeroPad (result, this.Minute, tokLen == 1 ? 1 : 2);
					break;
				case 's':
					// second [0, 29]
					tokLen = CountRepeat (format, i, ch);
					ZeroPad (result, this.Second, tokLen == 1 ? 1 : 2);
					break;
				case 'f':
					// fraction of second, to same number of
					// digits as there are f's

					tokLen = CountRepeat (format, i, ch);
					if (tokLen > 7)
						throw new FormatException ("Invalid Format String");

					int dec = (int)((long)(this.Ticks % TimeSpan.TicksPerSecond) / (long) Math.Pow (10, 7 - tokLen));
					ZeroPad (result, dec, tokLen);

					break;
				case 't':
					// AM/PM. t == first char, tt+ == full
					tokLen = CountRepeat (format, i, ch);
					string desig = this.Hour < 12 ? dfi.AMDesignator : dfi.PMDesignator;

					if (tokLen == 1) {
						if (desig.Length >= 1)
							result.Append (desig [0]);
					}
					else
						result.Append (desig);

					break;
				case 'z':
					// timezone. t = +/-h; tt = +/-hh; ttt+=+/-hh:mm
					tokLen = CountRepeat (format, i, ch);
					TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset (this);

					if (offset.Ticks >= 0)
						result.Append ('+');
					else
						result.Append ('-');

					switch (tokLen) {
					case 1:
						result.Append (Math.Abs (offset.Hours));
						break;
					case 2:
						result.Append (Math.Abs (offset.Hours).ToString ("00"));
						break;
					default:
						result.Append (Math.Abs (offset.Hours).ToString ("00"));
						result.Append (':');
						result.Append (Math.Abs (offset.Minutes).ToString ("00"));
						break;
					}
					break;
				//
				// Date tokens
				//
				case 'd':
					// day. d(d?) = day of month (leading 0 if two d's)
					// ddd = three leter day of week
					// dddd+ full day-of-week
					tokLen = CountRepeat (format, i, ch);

					if (tokLen <= 2)
						ZeroPad (result, dfi.Calendar.GetDayOfMonth (this), tokLen == 1 ? 1 : 2);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedDayName (dfi.Calendar.GetDayOfWeek (this)));
					else
						result.Append (dfi.GetDayName (dfi.Calendar.GetDayOfWeek (this)));

					break;
				case 'M':
					// Month.m(m?) = month # (with leading 0 if two mm)
					// mmm = 3 letter name
					// mmmm+ = full name
					tokLen = CountRepeat (format, i, ch);
					int month = dfi.Calendar.GetMonth(this);
					if (tokLen <= 2)
						ZeroPad (result, month, tokLen);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedMonthName (month));
					else
						result.Append (dfi.GetMonthName (month));

					break;
				case 'y':
					// Year. y(y?) = two digit year, with leading 0 if yy
					// yyy+ full year, if yyy and yr < 1000, displayed as three digits
					tokLen = CountRepeat (format, i, ch);

					if (tokLen <= 2)
						ZeroPad (result, dfi.Calendar.GetYear (this) % 100, tokLen);
					else
						result.Append (dfi.Calendar.GetYear (this).ToString ("D" + (tokLen == 3 ? 3 : 4)));

					break;
				case 'g':
					// Era name
					tokLen = CountRepeat (format, i, ch);
					result.Append (dfi.GetEraName (dfi.Calendar.GetEra (this)));
					break;

				//
				// Other
				//
				case ':':
					result.Append (dfi.TimeSeparator);
					tokLen = 1;
					break;
				case '/':
					result.Append (dfi.DateSeparator);
					tokLen = 1;
					break;
				case '\'': case '"':
					tokLen = ParseQuotedString (format, i, result);
					break;
				case '%':
					if (i >= format.Length - 1)
						throw new FormatException ("% at end of date time string");
					if (format [i + 1] == '%')
						throw new FormatException ("%% in date string");

					// Look for the next char
					tokLen = 1;
					break;
				case '\\':
					// C-Style escape
					if (i >= format.Length - 1)
						throw new FormatException ("\\ at end of date time string");

					result.Append (format [i + 1]);
					tokLen = 2;

					break;
				default:
					// catch all
					result.Append (ch);
					tokLen = 1;
					break;
				}
				i += tokLen;
			}
			return result.ToString ();
		}
		
		static int CountRepeat (string fmt, int p, char c)
		{
			int l = fmt.Length;
			int i = p + 1;
			while ((i < l) && (fmt [i] == c)) 
				i++;
			
			return i - p;
		}
		
		static int ParseQuotedString (string fmt, int pos, StringBuilder output)
		{
			// pos == position of " or '
			
			int len = fmt.Length;
			int start = pos;
			char quoteChar = fmt [pos++];
			
			while (pos < len) {
				char ch = fmt [pos++];
				
				if (ch == quoteChar)
					return pos - start;
				
				if (ch == '\\') {
					// C-Style escape
					if (pos >= len)
						throw new FormatException("Un-ended quote");
	
					output.Append (fmt [pos++]);
				} else {
					output.Append (ch);
				}
			}

			throw new FormatException("Un-ended quote");
		}
		
		static void ZeroPad (StringBuilder output, int digits, int padding)
		{
			output.Append (digits.ToString (new string ('0', padding)));
		}

		public string ToString (string format, IFormatProvider fp)

		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance(fp);

			if (format == null)
				format = "G";

			bool useutc = false, use_invariant = false;

			if (format.Length == 1) {
				char fchar = format [0];
				format = _GetStandardPattern (fchar, dfi, out useutc, out use_invariant);
			}

			// Don't convert UTC value. It just adds 'Z' for 
			// 'u' format, for the same ticks.
			return this._ToString (format, dfi);
		}

		public DateTime ToLocalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			if (offset.Ticks > 0) {
				if (DateTime.MaxValue - offset < this)
					return DateTime.MaxValue;
			} else if (offset.Ticks < 0) {
				// MS.NET fails to check validity here 
				// - it may throw ArgumentOutOfRangeException
				/*
				if (DateTime.MinValue - offset > this)
					return DateTime.MinValue;
				*/
			}
			
			DateTime lt = new DateTime(true, ticks+offset);
			TimeSpan ltoffset = tz.GetUtcOffset(lt);
			if(ltoffset != offset)
				lt = lt.Add(ltoffset.Subtract(offset));

			return lt;
		}

		public DateTime ToUniversalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			if (offset.Ticks < 0) {
				if (DateTime.MaxValue + offset < this)
					return DateTime.MaxValue;
			} else if (offset.Ticks > 0) {
				if (DateTime.MinValue + offset > this)
					return DateTime.MinValue;
			}

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

		System.DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return this;
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

		SByte IConvertible.ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		Single IConvertible.ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			if (conversionType == null)
				throw new ArgumentNullException ("conversionType");

			if (conversionType == typeof (DateTime))
				return this;
			else if (conversionType == typeof (String))
				return this.ToString (provider);
			else if (conversionType == typeof (Object))
				return this;
			else
				throw new InvalidCastException();
		}

		UInt16 IConvertible.ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		UInt32 IConvertible.ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

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
