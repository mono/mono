/*
 * System.DateTimeOffset
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *	Marek Safar (marek.safar@gmail.com)
 *
 *  Copyright (C) 2007 Novell, Inc (http://www.novell.com) 
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


#if NET_2_0 // Introduced by .NET 3.5 for 2.0 mscorlib

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System
{
	[Serializable]
	[StructLayout (LayoutKind.Auto)]
	public struct DateTimeOffset : IComparable, IFormattable, ISerializable, IDeserializationCallback, IComparable<DateTimeOffset>, IEquatable<DateTimeOffset>
	{
		public static readonly DateTimeOffset MaxValue = new DateTimeOffset (DateTime.MaxValue);
		public static readonly DateTimeOffset MinValue = new DateTimeOffset (DateTime.MaxValue);
		
		DateTime dt;
		TimeSpan utc_offset;
	
		public DateTimeOffset (DateTime dateTime)
		{
			dt = dateTime;

			if (dateTime.Kind == DateTimeKind.Utc)
				utc_offset = TimeSpan.Zero;
			else 
				utc_offset = TimeZone.CurrentTimeZone.GetUtcOffset (dateTime);
				
			if (UtcDateTime < DateTime.MinValue || UtcDateTime > DateTime.MaxValue)
				throw new ArgumentOutOfRangeException ("The UTC date and time that results from applying the offset is earlier than MinValue or later than MaxValue.");

		}
		
		public DateTimeOffset (DateTime dateTime, TimeSpan offset)
		{
			if (dateTime.Kind == DateTimeKind.Utc && offset != TimeSpan.Zero)
				throw new ArgumentException ("dateTime.Kind equals Utc and offset does not equal zero.");

			if (dateTime.Kind == DateTimeKind.Local && offset != TimeZone.CurrentTimeZone.GetUtcOffset (dateTime))
				throw new ArgumentException ("dateTime.Kind equals Local and offset does not equal the offset of the system's local time zone.");

			if (offset.Ticks % TimeSpan.TicksPerMinute != 0)
				throw new ArgumentException ("offset is not specified in whole minutes.");

			if (offset < new TimeSpan (-14, 0 ,0) || offset > new TimeSpan (14, 0, 0))
				throw new ArgumentOutOfRangeException ("offset is less than -14 hours or greater than 14 hours.");

			dt = dateTime;
			utc_offset = offset;

			if (UtcDateTime < DateTime.MinValue || UtcDateTime > DateTime.MaxValue)
				throw new ArgumentOutOfRangeException ("The UtcDateTime property is earlier than MinValue or later than MaxValue.");
		}

		public DateTimeOffset (long ticks, TimeSpan offset) : this (new DateTime (ticks), offset)
		{
		}

		public DateTimeOffset (int year, int month, int day, int hour, int minute, int second, TimeSpan offset) : 
			this (new DateTime (year, month, day, hour, minute, second), offset)
		{
		}

		public DateTimeOffset (int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset) :
			this (new DateTime (year, month, day, hour, minute, second, millisecond), offset)
		{
		}

		public DateTimeOffset (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, TimeSpan offset) :
			this (new DateTime (year, month, day, hour, minute, second, millisecond, calendar), offset)
		{
		}

		public DateTimeOffset Add (TimeSpan timeSpan)
		{
			return new DateTimeOffset (dt.Add (timeSpan), utc_offset);
		}
	
		public DateTimeOffset AddDays (double days)
		{
			return new DateTimeOffset (dt.AddDays (days), utc_offset);	
		}
		
		public DateTimeOffset AddHours (double hours)
		{
			return new DateTimeOffset (dt.AddHours (hours), utc_offset);
		}

		public static DateTimeOffset operator + (DateTimeOffset dateTimeTz, TimeSpan timeSpan)
		{
			return dateTimeTz.Add (timeSpan);
		}

		public DateTimeOffset AddMilliseconds (double milliseconds)
		{
			return new DateTimeOffset (dt.AddMilliseconds (milliseconds), utc_offset);
		}

		public DateTimeOffset AddMinutes (double minutes)
		{
			return new DateTimeOffset (dt.AddMinutes (minutes), utc_offset);	
		}

		public DateTimeOffset AddMonths (int months)
		{
			return new DateTimeOffset (dt.AddMonths (months), utc_offset);
		}

		public DateTimeOffset AddSeconds (double seconds)
		{
			return new DateTimeOffset (dt.AddSeconds (seconds), utc_offset);
		}

		public DateTimeOffset AddTicks (long ticks)
		{
			return new DateTimeOffset (dt.AddTicks (ticks), utc_offset);	
		}

		public DateTimeOffset AddYears (int years)
		{
			return new DateTimeOffset (dt.AddYears (years), utc_offset);
		}

		public static int Compare (DateTimeOffset first, DateTimeOffset second)
		{
			return first.CompareTo (second);	
		}

		public int CompareTo (DateTimeOffset other)
		{
			return UtcDateTime.CompareTo (other.UtcDateTime);
		}

		public int CompareTo (object other)
		{
			return CompareTo ((DateTimeOffset) other);
		}

		public static bool operator == (DateTimeOffset left, DateTimeOffset right)
		{
			return left.Equals (right);	
		}

		public bool Equals (DateTimeOffset other)
		{
			return UtcDateTime == other.UtcDateTime;
		}

		public override bool Equals (object other)
		{
			return UtcDateTime == ((DateTimeOffset) other).UtcDateTime;
		}

		public static bool Equals (DateTimeOffset first, DateTimeOffset second)
		{
			return first.Equals (second);	
		}

		public bool EqualsExact (DateTimeOffset other)
		{
			return dt == other.dt && utc_offset == other.utc_offset;	
		}

		public static DateTimeOffset FromFileTime (long fileTime)
		{
			if (fileTime < 0 || fileTime > MaxValue.Ticks)
				throw new ArgumentOutOfRangeException ("fileTime is less than zero or greater than DateTimeOffset.MaxValue.Ticks.");
			
			return new DateTimeOffset (DateTime.FromFileTime (fileTime), TimeZone.CurrentTimeZone.GetUtcOffset (DateTime.FromFileTime (fileTime)));

		}

		public override int GetHashCode ()
		{
			return dt.GetHashCode () ^ utc_offset.GetHashCode ();
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("datetime", dt);
			info.AddValue ("offset", utc_offset);
		}

		public static bool operator > (DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime > right.UtcDateTime;
		}			

		public static bool operator >= (DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime >= right.UtcDateTime;
		}			

		public static implicit operator DateTimeOffset (DateTime dateTime)
		{
			return new DateTimeOffset (dateTime);
		}

		public static bool operator != (DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime != right.UtcDateTime;
		}

		public static bool operator < (DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime < right.UtcDateTime;
		}
		
		public static bool operator <= (DateTimeOffset left, DateTimeOffset right)
		{
			return left.UtcDateTime <= right.UtcDateTime;
		}
	
		[MonoTODO]
		void IDeserializationCallback.OnDeserialization (object sender)
		{
		}

		[MonoTODO]
		public static DateTimeOffset Parse (string input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DateTimeOffset ParseExact (string input, string format, IFormatProvider formatProvider)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
				
			if (format == null)
				throw new ArgumentNullException ("format");

			if (input == String.Empty)
				throw new FormatException ("input is an empty string");

			if (format == String.Empty)
				throw new FormatException ("format is an empty string");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DateTimeOffset ParseExact (string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
				
			if (format == null)
				throw new ArgumentNullException ("format");

			if (input == String.Empty)
				throw new FormatException ("input is an empty string");

			if (format == String.Empty)
				throw new FormatException ("format is an empty string");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DateTimeOffset ParseExact (string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
				

			if (input == String.Empty)
				throw new FormatException ("input is an empty string");

			throw new NotImplementedException ();
		}

		public TimeSpan Subtract (DateTimeOffset other)
		{
			return UtcDateTime - other.UtcDateTime;
		}

		public DateTimeOffset Subtract (TimeSpan timeSpan)
		{
			return Add (-timeSpan);
		}

		public static TimeSpan operator - (DateTimeOffset left, DateTimeOffset right)
		{
			return left.Subtract (right);
		}

		public static DateTimeOffset operator - (DateTimeOffset dateTimeTz, TimeSpan timeSpan)
		{
			return dateTimeTz.Subtract (timeSpan);	
		}

		public long ToFileTime ()
		{
			return UtcDateTime.ToFileTime ();
		}

		public DateTimeOffset ToLocalTime ()
		{
			return new DateTimeOffset (UtcDateTime.ToLocalTime (), TimeZone.CurrentTimeZone.GetUtcOffset (UtcDateTime.ToLocalTime ()));
		}

		public DateTimeOffset ToOffset (TimeSpan offset)
		{
			return new DateTimeOffset (dt - utc_offset + offset, offset);
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider formatProvider)
		{
			return ToString (null, formatProvider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider formatProvider)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance(formatProvider);

			if (format == null || format == String.Empty)
				format = dfi.ShortDatePattern + " " + dfi.LongTimePattern + " zzz";

			bool to_utc = false;
			if (format.Length == 1) {
				char fchar = format [0];
				try {
					format = _GetStandardPattern (fchar, dfi, out to_utc);
				} catch {
					format = null;
				}
		
				if (format == null)
					throw new FormatException ("format is not one of the format specifier characters defined for DateTimeFormatInfo");
			}
			return to_utc ? ToUniversalTime ()._ToString (format, dfi) : _ToString (format, dfi);
		}

		static int CountRepeat (string fmt, int p, char c)
		{
			int l = fmt.Length;
			int i = p + 1;
			while ((i < l) && (fmt [i] == c)) 
				i++;
			
			return i - p;
		}

		static unsafe void ZeroPad (StringBuilder output, int digits, int len)
		{
			// more than enough for an int
			char* buffer = stackalloc char [16];
			int pos = 16;
			
			do {
				buffer [-- pos] = (char) ('0' + digits % 10);
				digits /= 10;
				len --;
			} while (digits > 0);
			
			while (len -- > 0)
				buffer [-- pos] = '0';
			
			output.Append (new string (buffer, pos, 16 - pos));
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
	
		string _ToString (string format, DateTimeFormatInfo dfi)
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
				bool omitZeros = false;
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
				case 'F':
					omitZeros = true;
					goto case 'f';
				case 'f':
					// fraction of second, to same number of
					// digits as there are f's

					tokLen = CountRepeat (format, i, ch);
					if (tokLen > 7)
						throw new FormatException ("Invalid Format String");

					int dec = (int)((long)(this.Ticks % TimeSpan.TicksPerSecond) / (long) Math.Pow (10, 7 - tokLen));
					int startLen = result.Length;
					ZeroPad (result, dec, tokLen);

					if (omitZeros) {
						while (result.Length > startLen && result [result.Length - 1] == '0')
							result.Length--;
						// when the value was 0, then trim even preceding '.' (!) It is fixed character.
						if (dec == 0 && startLen > 0 && result [startLen - 1] == '.')
							result.Length--;
					}

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

					if (utc_offset.Ticks >= 0)
						result.Append ('+');
					else
						result.Append ('-');

					switch (tokLen) {
					case 1:
						result.Append (Math.Abs (utc_offset.Hours));
						break;
					case 2:
						result.Append (Math.Abs (utc_offset.Hours).ToString ("00"));
						break;
					default:
						result.Append (Math.Abs (utc_offset.Hours).ToString ("00"));
						result.Append (':');
						result.Append (Math.Abs (utc_offset.Minutes).ToString ("00"));
						break;
					}
					break;
				case 'K': // maps to 'zzz'
					tokLen = 1;
					if (utc_offset.Ticks >= 0)
						result.Append ('+');
					else
						result.Append ('-');
					result.Append (Math.Abs (utc_offset.Hours).ToString ("00"));
					result.Append (':');
					result.Append (Math.Abs (utc_offset.Minutes).ToString ("00"));
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
						ZeroPad (result, dfi.Calendar.GetDayOfMonth (this.DateTime), tokLen == 1 ? 1 : 2);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedDayName (dfi.Calendar.GetDayOfWeek (this.DateTime)));
					else
						result.Append (dfi.GetDayName (dfi.Calendar.GetDayOfWeek (this.DateTime)));

					break;
				case 'M':
					// Month.m(m?) = month # (with leading 0 if two mm)
					// mmm = 3 letter name
					// mmmm+ = full name
					tokLen = CountRepeat (format, i, ch);
					int month = dfi.Calendar.GetMonth(this.DateTime);
					if (tokLen <= 2)
						ZeroPad (result, month, tokLen);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedMonthName (month));
					else
						result.Append (dfi.GetMonthName (month));

					break;
				case 'y':
					// Year. y(y?) = two digit year, with leading 0 if yy
					// yyy+ full year with leading zeros if needed.
					tokLen = CountRepeat (format, i, ch);

					if (tokLen <= 2)
						ZeroPad (result, dfi.Calendar.GetYear (this.DateTime) % 100, tokLen);
					else
						ZeroPad (result, dfi.Calendar.GetYear (this.DateTime), tokLen);
					break;

				case 'g':
					// Era name
					tokLen = CountRepeat (format, i, ch);
					result.Append (dfi.GetEraName (dfi.Calendar.GetEra (this.DateTime)));
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

		static string _GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool to_utc)
		{
			string pattern;

			to_utc = false;

			switch (format) {
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
			case 'o':
				pattern = dfi.RoundtripPattern;
				break;
			case 'r':
			case 'R':
				pattern = dfi.RFC1123Pattern;
				to_utc = true;
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
				to_utc = true;
				break;
			case 'U':
				throw new FormatException (String.Format ("Invalid format pattern: '{0}'", format));
			case 'y':
			case 'Y':
				pattern = dfi.YearMonthPattern;
				break;
			default:
				throw new FormatException (String.Format ("Invalid format pattern: '{0}'", format));
			}

			return pattern;
		}

		public DateTimeOffset ToUniversalTime ()
		{
			return new DateTimeOffset (UtcDateTime, TimeSpan.Zero);	
		}

		public static bool TryParse (string input, out DateTimeOffset result)
		{
			try {
				result = Parse (input);
				return true;
			} catch {
				result = MinValue;
				return false;
			}
		}

		public static bool TryParse (string input, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			try {
				result = Parse (input, formatProvider, styles);
				return true;
			} catch {
				result = MinValue;
				return false;
			}	
		}

		public static bool TryParseExact (string input, string format, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			try {
				result = ParseExact (input, format, formatProvider, styles);
				return true;
			} catch {
				result = MinValue;
				return false;
			}
		}

		public static bool TryParseExact (string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
		{
			try {
				result = ParseExact (input, formats, formatProvider, styles);
				return true;
			} catch {
				result = MinValue;
				return false;
			}
		}

		public DateTime Date {
			get { return DateTime.SpecifyKind (dt.Date, DateTimeKind.Unspecified); }
		}

		public DateTime DateTime {
			get { return DateTime.SpecifyKind (dt, DateTimeKind.Unspecified); }
		}

		public int Day {
			get { return dt.Day; }
		}

		public DayOfWeek DayOfWeek {
			get { return dt.DayOfWeek; }
		}

		public int DayOfYear {
			get { return dt.DayOfYear; }
		}

		public int Hour {
			get { return dt.Hour; }
		}

		public DateTime LocalDateTime {
			get { return UtcDateTime.ToLocalTime (); }
		}

		public int Millisecond {
			get { return dt.Millisecond; }
		}

		public int Minute {
			get { return dt.Minute; }
		}

		public int Month {
			get { return dt.Month; }
		}

		public static DateTimeOffset Now {
			get { return new DateTimeOffset (DateTime.Now);}
		}

		public TimeSpan Offset {
			get { return utc_offset; }	
		}

		public int Second {
			get { return dt.Second; }
		}

		public long Ticks {
			get { return dt.Ticks; }	
		}

		public TimeSpan TimeOfDay {
			get { return dt.TimeOfDay; }
		}

		public DateTime UtcDateTime {
			get { return DateTime.SpecifyKind (dt - utc_offset, DateTimeKind.Utc); }	
		}
		
		public static DateTimeOffset UtcNow {
			get { return new DateTimeOffset (DateTime.UtcNow); }
		}

		public long UtcTicks {
			get { return UtcDateTime.Ticks; }
		}

		public int Year {
			get { return dt.Year; }
		}
	}
}
#endif
