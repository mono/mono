//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//   Martin Baulig (martin@gnome.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
//   (C) 2001 Marcel Narings
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Serialization;

namespace System
{
	/// <summary>
	/// The DateTime structure represents dates and time ranging from
	/// 1-1-0001 12:00:00 AM to 31-12-9999 23:59:00 Common Era.
	/// </summary>
	/// 
	[Serializable]
	[StructLayout (LayoutKind.Auto)]
	public struct DateTime : IFormattable, IConvertible, IComparable, ISerializable, IComparable<DateTime>, IEquatable <DateTime>
	{
#if MONOTOUCH
		static DateTime () {
			if (MonoTouchAOTHelper.FalseFlag) {
				var comparer = new System.Collections.Generic.GenericComparer <DateTime> ();
				var eqcomparer = new System.Collections.Generic.GenericEqualityComparer <DateTime> ();
			}
		}
#endif
		private TimeSpan ticks;

		DateTimeKind kind;

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

		public static readonly DateTime MaxValue = new DateTime (false, new TimeSpan (MAX_VALUE_TICKS));
		public static readonly DateTime MinValue = new DateTime (false, new TimeSpan (0));

		// DateTime.Parse patterns
		// Patterns are divided to date and time patterns. The algorithm will
		// try combinations of these patterns. The algorithm also looks for
		// day of the week, AM/PM GMT and Z independently of the patterns.
		private static readonly string[] ParseTimeFormats = new string [] {
			"H:m:s.fffffffzzz",
			"H:m:s.fffffff",
			"H:m:s.ffffff",
			"H:m:s.fffff",
			"H:m:s.ffff",
			"H:m:s.fff",
			"H:m:s.ff",
			"H:m:s.f",
			"H:m:s tt zzz",
			"H:m:szzz",
			"H:m:s",
			"H:mzzz",
			"H:m",
			"H tt", // Specifies AM to disallow '8'.
			"H'\u6642'm'\u5206's'\u79D2'",
		};

		// DateTime.Parse date patterns extend ParseExact patterns as follows:
		//   MMM - month short name or month full name
		//   MMMM - month number or short name or month full name

		// Parse behaves differently according to the ShorDatePattern of the
		// DateTimeFormatInfo. The following define the date patterns for
		// different orders of day, month and year in ShorDatePattern.
		// Note that the year cannot go between the day and the month.
		private static readonly string[] ParseYearDayMonthFormats = new string [] {
			"yyyy/M/dT",
			"M/yyyy/dT",
			"yyyy'\u5E74'M'\u6708'd'\u65E5",


			"yyyy/d/MMMM",
			"yyyy/MMM/d",
			"d/MMMM/yyyy",
			"MMM/d/yyyy",
			"d/yyyy/MMMM",
			"MMM/yyyy/d",

			"yy/d/M",
		};

		private static readonly string[] ParseYearMonthDayFormats = new string [] {
			"yyyy/M/dT",
			"M/yyyy/dT",
			"yyyy'\u5E74'M'\u6708'd'\u65E5",

			"yyyy/MMMM/d",
			"yyyy/d/MMM",
			"MMMM/d/yyyy",
			"d/MMM/yyyy",
			"MMMM/yyyy/d",
			"d/yyyy/MMM",

			"yy/MMMM/d",
			"yy/d/MMM",
			"MMM/yy/d",
		};

		private static readonly string[] ParseDayMonthYearFormats = new string [] {
			"yyyy/M/dT",
			"M/yyyy/dT",
			"yyyy'\u5E74'M'\u6708'd'\u65E5",

			"yyyy/MMMM/d",
			"yyyy/d/MMM",
			"d/MMMM/yyyy",
			"MMM/d/yyyy",
			"MMMM/yyyy/d",
			"d/yyyy/MMM",

			"d/MMMM/yy",
			"yy/MMM/d",
			"d/yy/MMM",
			"yy/d/MMM",
			"MMM/d/yy",
			"MMM/yy/d",
		};

		private static readonly string[] ParseMonthDayYearFormats = new string [] {
			"yyyy/M/dT",
			"M/yyyy/dT",
			"yyyy'\u5E74'M'\u6708'd'\u65E5",

			"yyyy/MMMM/d",
			"yyyy/d/MMM",
			"MMMM/d/yyyy",
			"d/MMM/yyyy",
			"MMMM/yyyy/d",
			"d/yyyy/MMM",

			"MMMM/d/yy",
			"MMM/yy/d",
			"d/MMM/yy",
			"yy/MMM/d",
			"d/yy/MMM",
			"yy/d/MMM",
		};

		// Patterns influenced by the MonthDayPattern in DateTimeFormatInfo.
		// Note that these patterns cannot be followed by the time.
		private static readonly string[] MonthDayShortFormats = new string [] {
			"MMMM/d",
			"d/MMM",
			"yyyy/MMMM",
		};
		private static readonly string[] DayMonthShortFormats = new string [] {
			"d/MMMM",
			"MMM/yy",
			"yyyy/MMMM",
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
		public DateTime (long ticks)
		{
			this.ticks = new TimeSpan (ticks);
			if (ticks < MinValue.Ticks || ticks > MaxValue.Ticks) {
				string msg = Locale.GetText ("Value {0} is outside the valid range [{1},{2}].", 
					ticks, MinValue.Ticks, MaxValue.Ticks);
				throw new ArgumentOutOfRangeException ("ticks", msg);
			}
			kind = DateTimeKind.Unspecified;
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

			kind = DateTimeKind.Unspecified;
		}

		public DateTime (int year, int month, int day, Calendar calendar)
			: this (year, month, day, 0, 0, 0, 0, calendar)
		{
		}
		
		public DateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (year, month, day, hour, minute, second, 0, calendar)
		{
		}

		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
		{
			if (calendar == null)
				throw new ArgumentNullException ("calendar");
			ticks = calendar.ToDateTime (year, month, day, hour, minute, second, millisecond).ticks;
			kind = DateTimeKind.Unspecified;
		}

		internal DateTime (bool check, TimeSpan value)
		{
			if (check && (value.Ticks < MinValue.Ticks || value.Ticks > MaxValue.Ticks))
				throw new ArgumentOutOfRangeException ();

			ticks = value;

			kind = DateTimeKind.Unspecified;
		}

		public DateTime (long ticks, DateTimeKind kind) : this (ticks)
		{
			CheckDateTimeKind (kind);
			this.kind = kind;
		}

		public DateTime (int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
			: this (year, month, day, hour, minute, second)
		{
			CheckDateTimeKind (kind);
			this.kind = kind;
		}

		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
			: this (year, month, day, hour, minute, second, millisecond)
		{
			CheckDateTimeKind (kind);
			this.kind = kind;
		}

		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
			: this (year, month, day, hour, minute, second, millisecond, calendar)
		{
			CheckDateTimeKind (kind);
			this.kind = kind;
		}

		//
		// Not visible, but can be invoked during deserialization
		//
		DateTime (SerializationInfo info, StreamingContext context)
		{
			if (info.HasKey ("dateData")){
				long dateData = info.GetInt64 ("dateData");
				kind = (DateTimeKind) (dateData >> 62);
				ticks = new TimeSpan (dateData & 0x3fffffffffffffff);
			} else if (info.HasKey ("ticks")){
				ticks = new TimeSpan (info.GetInt64 ("ticks"));
				kind = DateTimeKind.Unspecified;
			} else {
				kind = DateTimeKind.Unspecified;
				ticks = new TimeSpan (0);
			}
		}
		
			      
		/* Properties  */

		public DateTime Date 
		{
			get	
			{ 
				DateTime ret = new DateTime (Year, Month, Day);
				ret.kind = kind;
				return ret;
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
		internal static extern long GetTimeMonotonic ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern long GetNow ();

		//
		// To reduce the time consumed by DateTime.Now, we keep
		// the difference to map the system time into a local
		// time into `to_local_time_span', we record the timestamp
		// for this in `last_now'
		//
		static object to_local_time_span_object;
		static long last_now;
		
		public static DateTime Now 
		{
			get	
			{
				long now = GetNow ();
				DateTime dt = new DateTime (now);

				if ((now - last_now) > TimeSpan.TicksPerMinute){
					to_local_time_span_object = TimeZone.CurrentTimeZone.GetLocalTimeDiff (dt);
					last_now = now;

				}

				// This is boxed, so we avoid locking.
				DateTime ret = dt + (TimeSpan) to_local_time_span_object;
				ret.kind = DateTimeKind.Local;
				return ret;
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
				DateTime today = new DateTime (now.Year, now.Month, now.Day);
				today.kind = now.kind;
				return today;
			}
		}

		public static DateTime UtcNow 
		{
			get {
				return new DateTime (GetNow (), DateTimeKind.Utc);
			}
		}

		public int Year 
		{
			get 
			{ 
				return FromTicks(Which.Year); 
			}
		}

		public DateTimeKind Kind {
			get {
				return kind;
			}
		}

		/* methods */

		public DateTime Add (TimeSpan value)
		{
			DateTime ret = AddTicks (value.Ticks);
			ret.kind = kind;
			return ret;
		}

		public DateTime AddDays (double value)
		{
			return AddMilliseconds (Math.Round (value * 86400000));
		}
		
		public DateTime AddTicks (long value)
		{
			if ((value + ticks.Ticks) > MAX_VALUE_TICKS || (value + ticks.Ticks) < 0) {
				throw new ArgumentOutOfRangeException();
			}
			DateTime ret = new DateTime (value + ticks.Ticks);
			ret.kind = kind;
			return ret;
		}

		public DateTime AddHours (double value)
		{
			return AddMilliseconds (value * 3600000);
		}

		public DateTime AddMilliseconds (double value)
		{
			if ((value * TimeSpan.TicksPerMillisecond) > long.MaxValue ||
					(value * TimeSpan.TicksPerMillisecond) < long.MinValue) {
				throw new ArgumentOutOfRangeException();
			}
			long msticks = (long) Math.Round (value * TimeSpan.TicksPerMillisecond);

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

		public DateTime AddMinutes (double value)
		{
			return AddMilliseconds (value * 60000);
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
			temp.kind = kind;
			return  temp.Add (this.TimeOfDay);
		}

		public DateTime AddSeconds (double value)
		{
			return AddMilliseconds (value * 1000);
		}

		public DateTime AddYears (int value)
		{
			return AddMonths (value * 12);
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

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.DateTime))
				throw new ArgumentException (Locale.GetText (
					"Value is not a System.DateTime"));

			return Compare (this, (DateTime) value);
		}

		public bool IsDaylightSavingTime ()
		{
			if (kind == DateTimeKind.Utc)
				return false;
			return TimeZone.CurrentTimeZone.IsDaylightSavingTime (this);
		}

		public int CompareTo (DateTime value)
		{
			return Compare (this, value);
		}

		public bool Equals (DateTime value)
		{
			return value.ticks == ticks;
		}

		public long ToBinary ()
		{
			switch (kind) {
			case DateTimeKind.Utc:
				return Ticks | 0x4000000000000000;
			case DateTimeKind.Local:
				return (long) ((ulong) ToUniversalTime ().Ticks | 0x8000000000000000);
			default:
				return Ticks;
			}
		}

		public static DateTime FromBinary (long dateData)
		{
			switch ((ulong)dateData >> 62) {
			case 1:
				return new DateTime (dateData ^ 0x4000000000000000, DateTimeKind.Utc);
			case 0:
				return new DateTime (dateData, DateTimeKind.Unspecified);
			default:
				return new DateTime (dateData & 0x3fffffffffffffff, DateTimeKind.Utc).ToLocalTime ();
			}
		}

		public static DateTime SpecifyKind (DateTime value, DateTimeKind kind)
		{
			return new DateTime (value.Ticks, kind);
		}

		public static int DaysInMonth (int year, int month)
		{
			int[] days ;

			if (month < 1 || month >12)
				throw new ArgumentOutOfRangeException ();

			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException ();

			days = (IsLeapYear(year) ? daysmonthleap  : daysmonth);
			return days[month];			
		}
		
		public override bool Equals (object value)
		{
			if (!(value is System.DateTime))
				return false;

			return ((DateTime) value).ticks == ticks;
		}

		public static bool Equals (DateTime t1, DateTime t2 )
		{
			return (t1.ticks == t2.ticks );
		}

		public static DateTime FromFileTime (long fileTime) 
		{
			if (fileTime < 0)
				throw new ArgumentOutOfRangeException ("fileTime", "< 0");

			return new DateTime (w32file_epoch + fileTime).ToLocalTime ();
		}

		public static DateTime FromFileTimeUtc (long fileTime) 
		{
			if (fileTime < 0)
				throw new ArgumentOutOfRangeException ("fileTime", "< 0");

			return new DateTime (w32file_epoch + fileTime);
		}

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
			return GetDateTimeFormats (adjustutc, info.GetAllRawDateTimePatterns (format), info);
		}

		private string [] GetDateTimeFormats (bool adjustutc, string [] patterns, DateTimeFormatInfo dfi)
		{
			string [] results = new string [patterns.Length];
			DateTime val = adjustutc ? ToUniversalTime () : this;
			for (int i = 0; i < results.Length; i++)
				results [i] = DateTimeUtils.ToString (val, patterns [i], dfi);
			return results;
		}

		private void CheckDateTimeKind (DateTimeKind kind) {
			if ((kind != DateTimeKind.Unspecified) && (kind != DateTimeKind.Utc) && (kind != DateTimeKind.Local))
				throw new ArgumentException ("Invalid DateTimeKind value.", "kind");
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
			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException ();
			return  ( (year % 4 == 0 && year % 100 != 0) || year % 400 == 0) ;
		}

		public static DateTime Parse (string s)
		{
			return Parse (s, null);
		}

		public static DateTime Parse (string s, IFormatProvider provider)
		{
			return Parse (s, provider, DateTimeStyles.AllowWhiteSpaces);
		}

		public static DateTime Parse (string s, IFormatProvider provider, DateTimeStyles styles)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			DateTime res;
			DateTimeOffset dto;
			Exception exception = null;
			if (!CoreParse (s, provider, styles, out res, out dto, true, ref exception))
				throw exception;
			
			return res;
		}

		const string formatExceptionMessage = "String was not recognized as a valid DateTime.";
		
		internal static bool CoreParse (string s, IFormatProvider provider, DateTimeStyles styles,
					      out DateTime result, out DateTimeOffset dto, bool setExceptionOnError, ref Exception exception)
		{
			dto = new DateTimeOffset (0, TimeSpan.Zero);
			if (s == null || s.Length == 0) {
				if (setExceptionOnError)
					exception = new FormatException (formatExceptionMessage);
				result = MinValue;
				return false;
			}

			if (provider == null)
				provider = CultureInfo.CurrentCulture;
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (provider);

			// Try first all the combinations of ParseAllDateFormats & ParseTimeFormats
			string[] allDateFormats = YearMonthDayFormats (dfi, setExceptionOnError, ref exception);
			if (allDateFormats == null){
				result = MinValue;
				return false;
			}

			bool longYear = false;
			for (int i = 0; i < allDateFormats.Length; i++) {
				string firstPart = allDateFormats [i];
				bool incompleteFormat = false;
				if (_DoParse (s, firstPart, "", false, out result, out dto, dfi, styles, true, ref incompleteFormat, ref longYear))
					return true;

				if (!incompleteFormat)
					continue;

				for (int j = 0; j < ParseTimeFormats.Length; j++) {
					if (_DoParse (s, firstPart, ParseTimeFormats [j], false, out result, out dto, dfi, styles, true, ref incompleteFormat, ref longYear))
						return true;
				}
			}

			//
			// Month day formats
			//
			int dayIndex = dfi.MonthDayPattern.IndexOf('d');
			int monthIndex = dfi.MonthDayPattern.IndexOf('M');
			if (dayIndex == -1 || monthIndex == -1){
				result = MinValue;
				if (setExceptionOnError)
					exception = new FormatException (Locale.GetText("Order of month and date is not defined by {0}", dfi.MonthDayPattern));
				return false;
			}
			bool is_day_before_month = dayIndex < monthIndex;
			string[] monthDayFormats = is_day_before_month ? DayMonthShortFormats : MonthDayShortFormats;
			for (int i = 0; i < monthDayFormats.Length; i++) {
				bool incompleteFormat = false;
				if (_DoParse (s, monthDayFormats[i], "", false, out result, out dto, dfi, styles, true, ref incompleteFormat, ref longYear))
					return true;
			}
			
			for (int j = 0; j < ParseTimeFormats.Length; j++) {
				string firstPart = ParseTimeFormats [j];
				bool incompleteFormat = false;
				if (_DoParse (s, firstPart, "", false, out result, out dto, dfi, styles, false, ref incompleteFormat, ref longYear))
					return true;
				if (!incompleteFormat)
					continue;

				for (int i = 0; i < monthDayFormats.Length; i++) {
					if (_DoParse (s, firstPart, monthDayFormats [i], false, out result, out dto, dfi, styles, false, ref incompleteFormat, ref longYear))
						return true;
				}
				for (int i = 0; i < allDateFormats.Length; i++) {
					string dateFormat = allDateFormats [i];
					if (dateFormat[dateFormat.Length - 1] == 'T')
						continue; // T formats must be before the time part
					if (_DoParse (s, firstPart, dateFormat, false, out result, out dto, dfi, styles, false, ref incompleteFormat, ref longYear))
						return true;
				}
			}

			// Try as a last resort all the patterns
			if (ParseExact (s, dfi.GetAllDateTimePatternsInternal (), dfi, styles, out result, false, ref longYear, setExceptionOnError, ref exception))
				return true;

			if (!setExceptionOnError)
				return false;
			
			// .NET 2.x does not throw an ArgumentOutOfRangeException, but .NET 1.1 does.
			exception = new FormatException (formatExceptionMessage);
			return false;
		}

		public static DateTime ParseExact (string s, string format, IFormatProvider provider)
		{
			return ParseExact (s, format, provider, DateTimeStyles.None);
		}

		private static string[] YearMonthDayFormats (DateTimeFormatInfo dfi, bool setExceptionOnError, ref Exception exc)
		{
			int dayIndex = dfi.ShortDatePattern.IndexOf('d');
			int monthIndex = dfi.ShortDatePattern.IndexOf('M');
			int yearIndex = dfi.ShortDatePattern.IndexOf('y');
			if (dayIndex == -1 || monthIndex == -1 || yearIndex == -1){
				if (setExceptionOnError)
					exc = new FormatException (Locale.GetText("Order of year, month and date is not defined by {0}", dfi.ShortDatePattern));
				return null;
			}

			if (yearIndex < monthIndex)
				if (monthIndex < dayIndex)
					return ParseYearMonthDayFormats;
				else if (yearIndex < dayIndex)
					return ParseYearDayMonthFormats;
				else {
					// The year cannot be between the date and the month
					if (setExceptionOnError)
						exc = new FormatException (Locale.GetText("Order of date, year and month defined by {0} is not supported", dfi.ShortDatePattern));
					return null;
				}
			else if (dayIndex < monthIndex)
				return ParseDayMonthYearFormats;
			else if (dayIndex < yearIndex)
				return ParseMonthDayYearFormats;
			else {
				// The year cannot be between the month and the date
				if (setExceptionOnError)
					exc = new FormatException (Locale.GetText("Order of month, year and date defined by {0} is not supported", dfi.ShortDatePattern));
				return null;
			}
		}

		private static int _ParseNumber (string s, int valuePos,
						 int min_digits,
						 int digits,
						 bool leadingzero,
						 bool sloppy_parsing,
						 out int num_parsed)
		{
			int number = 0, i;

			if (sloppy_parsing)
				leadingzero = false;

			if (!leadingzero) {
				int real_digits = 0;
				for (i = valuePos; i < s.Length && i < digits + valuePos; i++) {
					if (!Char.IsDigit (s[i]))
						break;

					real_digits++;
				}

				digits = real_digits;
			}
			if (digits < min_digits) {
				num_parsed = -1;
				return 0;
			}

			if (s.Length - valuePos < digits) {
				num_parsed = -1;
				return 0;
			}

			for (i = valuePos; i < digits + valuePos; i++) {
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

		private static int _ParseEnum (string s, int sPos, string[] values, string[] invValues, bool exact, out int num_parsed)
		{
			// FIXME: I know this is somehow lame code. Probably
			// it should iterate all the enum value and return
			// the longest match. However right now I don't see
			// anything but "1" and "10" - "12" that might match
			// two or more values. (They are only abbrev month
			// names, so do reverse order search). See bug #80094.
			for (int i = values.Length - 1; i >= 0; i--) {
				if (!exact && invValues [i].Length > values[i].Length) {
					if (invValues [i].Length > 0 && _ParseString (s, sPos, 0, invValues [i], out num_parsed))
						return i;
					if (values [i].Length > 0 && _ParseString (s, sPos, 0, values [i], out num_parsed))
						return i;
				}
				else {
					if (values [i].Length > 0 && _ParseString (s, sPos, 0, values [i], out num_parsed))
						return i;
					if (!exact && invValues [i].Length > 0 && _ParseString (s, sPos, 0, invValues [i], out num_parsed))
					return i;
				}
			}

			num_parsed = -1;
			return -1;
		}

		private static bool _ParseString (string s, int sPos, int maxlength, string value, out int num_parsed)
		{
			if (maxlength <= 0)
				maxlength = value.Length;

			if (sPos + maxlength <= s.Length && String.Compare (s, sPos, value, 0, maxlength, true, CultureInfo.InvariantCulture) == 0) {
				num_parsed = maxlength;
				return true;
			}

			num_parsed = -1;
			return false;
		}

		// Note that in case of Parse (exact == false) we check both for AM/PM
		// and the culture spcific AM/PM strings.
		private static bool _ParseAmPm(string s,
					       int valuePos,
					       int num,
					       DateTimeFormatInfo dfi,
					       bool exact,
					       out int num_parsed,
					       ref int ampm)
		{
			num_parsed = -1;
			if (ampm != -1)
				return false;

			if (!IsLetter (s, valuePos)) {
				if (dfi.AMDesignator != "")
					return false;
				if (exact)
					ampm = 0;
				num_parsed = 0;
				return true;
			}
			DateTimeFormatInfo invInfo = DateTimeFormatInfo.InvariantInfo;
			if (!exact && _ParseString (s, valuePos, num, invInfo.PMDesignator, out num_parsed) ||
			    dfi.PMDesignator != "" && _ParseString(s, valuePos, num, dfi.PMDesignator, out num_parsed))
				ampm = 1;
			else if (!exact && _ParseString (s, valuePos, num, invInfo.AMDesignator, out num_parsed) ||
			         _ParseString (s, valuePos, num, dfi.AMDesignator, out num_parsed)) {
				if (exact || num_parsed != 0)
					ampm = 0;
			}
			else
				return false;
			return true;
		}

		// Note that in case of Parse (exact == false) we check both for ':'
		// and the culture spcific TimeSperator
		private static bool _ParseTimeSeparator (string s, int sPos, DateTimeFormatInfo dfi, bool exact, out int num_parsed)
		{
			return _ParseString (s, sPos, 0, dfi.TimeSeparator, out num_parsed) ||
			       !exact && _ParseString (s, sPos, 0, ":", out num_parsed);
		}

		// Accept any character for DateSeparator, except TimeSeparator,
		// a digit or a letter.
		// Not documented, but seems to be MS behaviour here.  See bug 54047.
		private static bool _ParseDateSeparator (string s, int sPos, DateTimeFormatInfo dfi, bool exact, out int num_parsed)
		{
			num_parsed = -1;
			if (exact && s [sPos] != '/')
				return false;

			if (_ParseTimeSeparator (s, sPos, dfi, exact, out num_parsed) ||
				Char.IsDigit (s [sPos]) || Char.IsLetter (s [sPos]))
				return(false);

			num_parsed = 1;
			return true;
		}

		private static bool IsLetter (string s, int pos)
		{
			return pos < s.Length && Char.IsLetter (s [pos]);
		}

		// To implement better DateTime.Parse we use two format strings one
		// for Date and one for Time. This allows us to define two different
		// arrays of formats for Time and Dates and to combine them more or less
		// efficiently. When this mode is used flexibleTwoPartsParsing is true.
		private static bool _DoParse (string s,
					      string firstPart,
					      string secondPart,
					      bool exact,
					      out DateTime result,
					      out DateTimeOffset dto,
					      DateTimeFormatInfo dfi,
					      DateTimeStyles style,
					      bool firstPartIsDate,
					      ref bool incompleteFormat,
					      ref bool longYear)
		{
			bool useutc = false;
			bool use_invariant = false;
			bool sloppy_parsing = false;
			dto = new DateTimeOffset (0, TimeSpan.Zero);
			bool flexibleTwoPartsParsing = !exact && secondPart != null;
			incompleteFormat = false;
			int valuePos = 0;
			string format = firstPart;
			bool afterTFormat = false;
			DateTimeFormatInfo invInfo = DateTimeFormatInfo.InvariantInfo;
			if (format.Length == 1)
				format = DateTimeUtils.GetStandardPattern (format [0], dfi, out useutc, out use_invariant);

			result = new DateTime (0);
			if (format == null)
				return false;

			if (s == null)
				return false;
				
			if ((style & DateTimeStyles.AllowLeadingWhite) != 0) {
				format = format.TrimStart (null);

				s = s.TrimStart (null); // it could be optimized, but will make little good.
			}

			if ((style & DateTimeStyles.AllowTrailingWhite) != 0) {
				format = format.TrimEnd (null);
				s = s.TrimEnd (null); // it could be optimized, but will make little good.
			}

			if (use_invariant)
				dfi = invInfo;

			if ((style & DateTimeStyles.AllowInnerWhite) != 0)
				sloppy_parsing = true;

			string chars = format;
			int len = format.Length, pos = 0, num = 0;
			if (len == 0)
				return false;

			int day = -1, dayofweek = -1, month = -1, year = -1;
			int hour = -1, minute = -1, second = -1;
			double fractionalSeconds = -1;
			int ampm = -1;
			int tzsign = -1, tzoffset = -1, tzoffmin = -1;
			bool isFirstPart = true;

			for (; ; )
			{
				if (valuePos == s.Length)
					break;

				int num_parsed = 0;
				if (flexibleTwoPartsParsing && pos + num == 0)
				{
					bool isLetter = IsLetter(s, valuePos);
					if (isLetter) {
						if (s [valuePos] == 'Z')
							num_parsed = 1;
						else
							_ParseString (s, valuePos, 0, "GMT", out num_parsed);
						if (num_parsed > 0 && !IsLetter (s, valuePos + num_parsed)) {
							valuePos += num_parsed;
							useutc = true;
							continue;
						}
					}
					if (!afterTFormat && _ParseAmPm (s, valuePos, 0, dfi, exact, out num_parsed, ref ampm)) {
						if (IsLetter (s, valuePos + num_parsed))
							ampm = -1;
						else if (num_parsed > 0) {
							valuePos += num_parsed;
							continue;
						}
					}

					if (!afterTFormat && dayofweek == -1 && isLetter) {
						dayofweek = _ParseEnum (s, valuePos, dfi.RawDayNames, invInfo.RawDayNames, exact, out num_parsed);
						if (dayofweek == -1)
							dayofweek = _ParseEnum (s, valuePos, dfi.RawAbbreviatedDayNames, invInfo.RawAbbreviatedDayNames, exact, out num_parsed);
						if (dayofweek != -1 && !IsLetter (s, valuePos + num_parsed)) {
							valuePos += num_parsed;
							continue;
						}
						else
							dayofweek = -1;
					}

					if (char.IsWhiteSpace (s [valuePos]) || s [valuePos] == ',') {
						valuePos += 1;
						continue;
					}
					num_parsed = 0;
				}

				if (pos + num >= len)
				{
					if (flexibleTwoPartsParsing && num == 0) {
						afterTFormat = isFirstPart && firstPart [firstPart.Length - 1] == 'T';
						if (!isFirstPart && format == "")
							break;

						pos = 0;
						if (isFirstPart)
							format = secondPart;
						else
							format = "";
						chars = format;
						len = chars.Length;
						isFirstPart = false;
						continue;
					}
					break;
				}

				bool leading_zeros = true;

				if (chars[pos] == '\'') {
					num = 1;
					while (pos+num < len) {
						if (chars[pos+num] == '\'')
							break;

						if (valuePos == s.Length || s [valuePos] != chars [pos + num])
							return false;

						valuePos++;
						num++;
					}

					pos += num + 1;
					num = 0;
					continue;
				} else if (chars[pos] == '"') {
					num = 1;
					while (pos+num < len) {
						if (chars[pos+num] == '"')
							break;

						if (valuePos == s.Length || s [valuePos] != chars[pos+num])
							return false;

						valuePos++;
						num++;
					}

					pos += num + 1;
					num = 0;
					continue;
				} else if (chars[pos] == '\\') {
					pos += num + 1;
					num = 0;
					if (pos >= len)
						return false;
					if (s [valuePos] != chars [pos])
						return false;

					valuePos++;
					pos++;
					continue;
				} else if (chars[pos] == '%') {
					pos++;
					continue;
				} else if (char.IsWhiteSpace (s [valuePos]) ||
					s [valuePos] == ',' && (!exact && chars [pos] == '/' || Char.IsWhiteSpace (chars [pos]))) {
					valuePos++;
					num = 0;
					if (exact && (style & DateTimeStyles.AllowInnerWhite) == 0) {
						if (!Char.IsWhiteSpace (chars[pos]))
							return false;
						pos++;
						continue;
					}

					int ws = valuePos;
					while (ws < s.Length) {
						if (Char.IsWhiteSpace (s [ws]) || s [ws] == ',')
							ws++;
						else
							break;
					}
					valuePos = ws;
					ws = pos;
					while (ws < chars.Length) {
						if (Char.IsWhiteSpace (chars [ws]) || chars [ws] == ',')
							ws++;
						else
							break;
					}
					pos = ws;
					// A whitespace may match a '/' in the pattern.
					if (!exact && pos < chars.Length && chars[pos] == '/')
						if (!_ParseDateSeparator (s, valuePos, dfi, exact, out num_parsed))
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
					if (num < 2 && day != -1 || num >= 2 && dayofweek != -1)
						return false;
					if (num == 0)
						day = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						day = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);
					else if (num == 2)
						dayofweek = _ParseEnum (s, valuePos, dfi.RawAbbreviatedDayNames, invInfo.RawAbbreviatedDayNames, exact, out num_parsed);
					else
						dayofweek = _ParseEnum (s, valuePos, dfi.RawDayNames, invInfo.RawDayNames, exact, out num_parsed);
					break;
				case 'M':
					if (month != -1)
						return false;

					if (flexibleTwoPartsParsing) {
						num_parsed = -1;
						if (num == 0 || num == 3)
							month = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
						if (num > 1 && num_parsed == -1)
							month = _ParseEnum (s, valuePos, dfi.RawMonthNames, invInfo.RawMonthNames, exact, out num_parsed) + 1;
						if (num > 1 && num_parsed == -1)
							month = _ParseEnum (s, valuePos, dfi.RawAbbreviatedMonthNames, invInfo.RawAbbreviatedMonthNames, exact, out num_parsed) + 1;
						break;
					}

					if (num == 0)
						month = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						month = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);
					else if (num == 2)
						month = _ParseEnum (s, valuePos, dfi.RawAbbreviatedMonthNames, invInfo.RawAbbreviatedMonthNames, exact, out num_parsed) + 1;
					else
						month = _ParseEnum (s, valuePos, dfi.RawMonthNames, invInfo.RawMonthNames, exact, out num_parsed) + 1;
					break;
				case 'y':
					if (year != -1)
						return false;

					if (num == 0) {
						year = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					} else if (num < 3) {
						year = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);
					} else {
						year = _ParseNumber (s, valuePos, exact ? 4 : 3, 4, false, sloppy_parsing, out num_parsed);
						if ((year >= 1000) && (num_parsed == 4) && (!longYear) && (s.Length > 4 + valuePos)) {
							int np = 0;
							int ly = _ParseNumber (s, valuePos, 5, 5, false, sloppy_parsing, out np);
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
						hour = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else
						hour = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);

					if (hour > 12)
						return false;
					if (hour == 12)
						hour = 0;

					break;
				case 'H':
					if (hour != -1 || !flexibleTwoPartsParsing && ampm >= 0)
						return false;
					if (num == 0)
						hour = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else
						hour = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);

					if (hour >= 24)
						return false;

//					ampm = -2;
					break;
				case 'm':
					if (minute != -1)
						return false;
					if (num == 0)
						minute = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else
						minute = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);

					if (minute >= 60)
						return false;

					break;
				case 's':
					if (second != -1)
						return false;
					if (num == 0)
						second = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else
						second = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);

					if (second >= 60)
						return false;

					break;
				case 'F':
					leading_zeros = false;
					goto case 'f';
				case 'f':
					if (num > 6 || fractionalSeconds != -1)
						return false;
					double decimalNumber = (double) _ParseNumber (s, valuePos, 0, num+1, leading_zeros, sloppy_parsing, out num_parsed);
					if (num_parsed == -1)
						return false;
					fractionalSeconds = decimalNumber / Math.Pow(10.0, num_parsed);
					break;
				case 't':
					if (!_ParseAmPm (s, valuePos, num > 0 ? 0 : 1, dfi, exact, out num_parsed, ref ampm))
							return false;
					break;
				case 'z':
					if (tzsign != -1)
						return false;

					if (s [valuePos] == '+')
						tzsign = 0;
					else if (s [valuePos] == '-')
						tzsign = 1;
					else
						return false;
					valuePos++;

					if (num == 0)
						tzoffset = _ParseNumber (s, valuePos, 1, 2, false, sloppy_parsing, out num_parsed);
					else if (num == 1)
						tzoffset = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);
					else {
						tzoffset = _ParseNumber (s, valuePos, 1, 2, true, /*sloppy_parsing*/true, out num_parsed);
						valuePos += num_parsed;
						if (num_parsed < 0)
							return false;

						num_parsed = 0;
						if (valuePos < s.Length && Char.IsDigit (s [valuePos]) ||
							_ParseTimeSeparator (s, valuePos, dfi, exact, out num_parsed)) {
							valuePos += num_parsed;
							tzoffmin = _ParseNumber (s, valuePos, 1, 2, true, sloppy_parsing, out num_parsed);
							if (num_parsed < 0)
								return false;
						}
						else if (!flexibleTwoPartsParsing)
							return false;
						else
							num_parsed = 0;
					}
					break;
				case 'K':
					if (s [valuePos] == 'Z') {
						valuePos++;
						useutc = true;						
					}
					else if (s [valuePos] == '+' || s [valuePos] == '-') {
						if (tzsign != -1)
							return false;
						if (s [valuePos] == '+')
							tzsign = 0;
						else if (s [valuePos] == '-')
							tzsign = 1;
						valuePos++;

						// zzz
						tzoffset = _ParseNumber (s, valuePos, 0, 2, true, sloppy_parsing, out num_parsed);
						valuePos += num_parsed;
						if (num_parsed < 0)
							return false;

						if (Char.IsDigit (s [valuePos]))
							num_parsed = 0;
						else if (!_ParseString (s, valuePos, 0, dfi.TimeSeparator, out num_parsed))
							return false;
						valuePos += num_parsed;

						tzoffmin = _ParseNumber (s, valuePos, 0, 2, true, sloppy_parsing, out num_parsed);
						num = 2;
						if (num_parsed < 0)
							return false;
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
					if (s [valuePos] != 'Z')
						return false;
					num = 0;
					num_parsed = 1;
					useutc = true;
					break;
				case 'G':
					if (s [valuePos] != 'G')
						return false;

					if ((pos + 2 < len) && (valuePos + 2 < s.Length) &&
						(chars [pos + 1] == 'M') && (s[valuePos + 1] == 'M') &&
						(chars [pos + 2] == 'T') && (s[valuePos + 2] == 'T'))
					{
						useutc = true;
						num = 2;
						num_parsed = 3;
					}
					else {
						num = 0;
						num_parsed = 1;
					}
					break;
				case ':':
					if (!_ParseTimeSeparator (s, valuePos, dfi, exact, out num_parsed))
						return false;
					break;
				case '/':
					if (!_ParseDateSeparator (s, valuePos, dfi, exact, out num_parsed))
						return false;

					num = 0;
					break;
				default:
					if (s [valuePos] != chars [pos])
							return false;

					num = 0;
					num_parsed = 1;
					break;
				}

				if (num_parsed < 0)
					return false;

				valuePos += num_parsed;

				if (!exact && !flexibleTwoPartsParsing) {
					switch (chars [pos]) {
					case 'm':
					case 's':
					case 'F':
					case 'f':
					case 'z':
						if (s.Length > valuePos && s [valuePos] == 'Z' &&
							(pos + 1 == chars.Length || chars [pos + 1] != 'Z')) {
							useutc = true;
							valuePos++;
						}
						break;
					}
				}

				pos = pos + num + 1;
				num = 0;
			}

			if (pos + 1 < len && chars [pos] == '.' && chars [pos + 1] == 'F') {
				pos++;
				while (pos < len && chars [pos] == 'F') // '.FFF....' can be mapped to nothing. See bug #444103
					pos++;
			}
			while (pos < len && chars [pos] == 'K') // 'K' can be mapped to nothing
				pos++;

			if (pos < len)
				return false;

			if (s.Length > valuePos) // extraneous tail.
			{
				if (valuePos == 0)
					return false;

				if (Char.IsDigit (s [valuePos]) && Char.IsDigit (s [valuePos - 1]))
					return false;
				if (Char.IsLetter (s [valuePos]) && Char.IsLetter (s [valuePos - 1]))
					return false;
				incompleteFormat = true;
				return false;
			}

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
					day = DateTime.Today.Day;
					month = DateTime.Today.Month;
					year = DateTime.Today.Year;
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
					year = DateTime.Today.Year;
			}

			if (ampm == 0 && hour == 12)
				hour = 0;

			if (ampm == 1 && (!flexibleTwoPartsParsing || hour < 12))
				hour = hour + 12;
			
			// For anything out of range 
			// return false
			if (year < 1 || year > 9999 || 
				month < 1 || month >12  ||
				day < 1 || day > DateTime.DaysInMonth(year, month) ||
				hour < 0 || hour > 23 ||
				minute < 0 || minute > 59 ||
				second < 0 || second > 59)
				return false;

			result = new DateTime (year, month, day, hour, minute, second, 0);
			result = result.AddSeconds(fractionalSeconds);

			if (dayofweek != -1 && dayofweek != (int) result.DayOfWeek)
				return false;

			if (tzsign == -1) {
				if (result != DateTime.MinValue) {
					try {
						dto = new DateTimeOffset (result);
					} catch { } // We handle this error in DateTimeOffset.Parse
				}
			} else {
				if (tzoffmin == -1)
					tzoffmin = 0;
				if (tzoffset == -1)
					tzoffset = 0;
				if (tzsign == 1) {
					tzoffset = -tzoffset;
					tzoffmin = -tzoffmin;
				}
				try {
					dto = new DateTimeOffset (result, new TimeSpan (tzoffset, tzoffmin, 0));
				} catch {} // We handle this error in DateTimeOffset.Parse
			}
			bool adjustToUniversal = (style & DateTimeStyles.AdjustToUniversal) != 0;
			
			if (tzsign != -1) {
				long newticks = (result.ticks - dto.Offset).Ticks;
				if (newticks < 0)
					newticks += TimeSpan.TicksPerDay;
				result = new DateTime (false, new TimeSpan (newticks));
				result.kind = DateTimeKind.Utc;
				if ((style & DateTimeStyles.RoundtripKind) != 0)
					result = result.ToLocalTime ();
			}
			else if (useutc || ((style & DateTimeStyles.AssumeUniversal) != 0))
				result.kind = DateTimeKind.Utc;
			else if ((style & DateTimeStyles.AssumeLocal) != 0)
				result.kind = DateTimeKind.Local;						

			bool adjustToLocal = !adjustToUniversal && (style & DateTimeStyles.RoundtripKind) == 0;
			if (result.kind != DateTimeKind.Unspecified)
			{				
				if (adjustToUniversal)
					result = result.ToUniversalTime ();
				else if (adjustToLocal)
					result = result.ToLocalTime ();
			}
			return true;
		}

		public static DateTime ParseExact (string s, string format,
						   IFormatProvider provider, DateTimeStyles style)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			string [] formats = new string [1];
			formats[0] = format;

			return ParseExact (s, formats, provider, style);
		}

		public static DateTime ParseExact (string s, string[] formats,
						   IFormatProvider provider,
						   DateTimeStyles style)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (provider);
			CheckStyle (style);
			if (s == null)
				throw new ArgumentNullException ("s");
			if (formats == null)
				throw new ArgumentNullException ("formats");
			if (formats.Length == 0)
				throw new FormatException ("Format specifier was invalid.");

			DateTime result;
			bool longYear = false;
			Exception e = null;
			if (!ParseExact (s, formats, dfi, style, out result, true, ref longYear, true, ref e))
				throw e;
			return result;
		}		

		private static void CheckStyle (DateTimeStyles style)
		{
			if ( (style & DateTimeStyles.RoundtripKind) != 0)
			{
				if ((style & DateTimeStyles.AdjustToUniversal) != 0 || (style & DateTimeStyles.AssumeLocal) != 0 ||
					 (style & DateTimeStyles.AssumeUniversal) != 0)
					throw new ArgumentException ("The DateTimeStyles value RoundtripKind cannot be used with the values AssumeLocal, Asersal or AdjustToUniversal.", "style");
			}
			if ((style & DateTimeStyles.AssumeUniversal) != 0 && (style & DateTimeStyles.AssumeLocal) != 0)			
				throw new ArgumentException ("The DateTimeStyles values AssumeLocal and AssumeUniversal cannot be used together.", "style");
		}

		public static bool TryParse (string s, out DateTime result)
		{
			if (s != null){
				try {
					Exception exception = null;
					DateTimeOffset dto;

					return CoreParse (s, null, DateTimeStyles.AllowWhiteSpaces, out result, out dto, false, ref exception);
				} catch { }
			}
			result = MinValue;
			return false;
		}
		
		public static bool TryParse (string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
		{
			if (s != null){
				try {
					Exception exception = null;
					DateTimeOffset dto;
					
					return CoreParse (s, provider, styles, out result, out dto, false, ref exception);
				} catch {}
			} 
			result = MinValue;
			return false;
		}
		
		public static bool TryParseExact (string s, string format,
						  IFormatProvider provider,
						  DateTimeStyles style,
						  out DateTime result)
		{
			string[] formats;
			formats = new string [1];
			formats[0] = format;

			return TryParseExact (s, formats, provider, style, out result);
		}

		public static bool TryParseExact (string s, string[] formats,
						  IFormatProvider provider,
						  DateTimeStyles style,
						  out DateTime result)
		{
			try {
				DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (provider);

				bool longYear = false;
				Exception e = null;
				return ParseExact (s, formats, dfi, style, out result, true, ref longYear, false, ref e);
			} catch {
				result = MinValue;
				return false;
			}
		}

		private static bool ParseExact (string s, string [] formats,
						DateTimeFormatInfo dfi, DateTimeStyles style, out DateTime ret,
						bool exact, ref bool longYear,
						bool setExceptionOnError, ref Exception exception)
		{
			int i;
			bool incompleteFormat = false;
			for (i = 0; i < formats.Length; i++)
			{
				DateTime result;
				string format = formats[i];
				if (format == null || format == String.Empty)
					break;

				DateTimeOffset dto;
				if (_DoParse (s, formats[i], null, exact, out result, out dto, dfi, style, false, ref incompleteFormat, ref longYear)) {
					ret = result;
					return true;
				}
			}

			if (setExceptionOnError)
				exception = new FormatException ("Invalid format string");
			ret = DateTime.MinValue;
			return false;
		}
		
		public TimeSpan Subtract (DateTime value)
		{
			return new TimeSpan (ticks.Ticks) - value.ticks;
		}

		public DateTime Subtract(TimeSpan value)
		{
			TimeSpan newticks;

			newticks = (new TimeSpan (ticks.Ticks)) - value;
			DateTime ret = new DateTime (true,newticks);
			ret.kind = kind;
			return ret;
		}

		public long ToFileTime()
		{
			DateTime universalTime = ToUniversalTime();
			
			if (universalTime.Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return(universalTime.Ticks - w32file_epoch);
		}

		public long ToFileTimeUtc()
		{
			if (Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return (Ticks - w32file_epoch);
		}

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

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}
	
		public string ToString (string format, IFormatProvider provider)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (provider);

			if (format == null || format == String.Empty)
				format = "G";

			bool useutc = false, use_invariant = false;

			if (format.Length == 1) {
				char fchar = format [0];
				format = DateTimeUtils.GetStandardPattern (fchar, dfi, out useutc, out use_invariant);
				if (fchar == 'U')
					return DateTimeUtils.ToString (ToUniversalTime (), format, dfi);
//					return ToUniversalTime()._ToString (format, dfi);

				if (format == null)
					throw new FormatException ("format is not one of the format specifier characters defined for DateTimeFormatInfo");
			}

			// Don't convert UTC value. It just adds 'Z' for 
			// 'u' format, for the same ticks.
			return DateTimeUtils.ToString (this, format, dfi);
		}

		public DateTime ToLocalTime ()
		{
			return TimeZone.CurrentTimeZone.ToLocalTime (this);
		}

		public DateTime ToUniversalTime()
		{
			return TimeZone.CurrentTimeZone.ToUniversalTime (this);
		}

		/*  OPERATORS */

		public static DateTime operator +(DateTime d, TimeSpan t)
		{
			DateTime ret = new DateTime (true, d.ticks + t);
			ret.kind = d.kind;
			return ret;
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
			DateTime ret = new DateTime (true, d.ticks - t);
			ret.kind = d.kind;
			return ret;
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

		object IConvertible.ToType (Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
				throw new ArgumentNullException ("targetType");

			if (targetType == typeof (DateTime))
				return this;
			else if (targetType == typeof (String))
				return this.ToString (provider);
			else if (targetType == typeof (Object))
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

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			long t = ticks.Ticks;
			info.AddValue ("ticks", t);

			// This is the new .NET format, encodes the kind on the top bits
			info.AddValue ("dateData", t | (((uint)kind) << 62));
		}
		
	}
}
