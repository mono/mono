/*
 * System.DateTimeOffset.cs
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
#if MONOTOUCH
		static DateTimeOffset () {
			if (MonoTouchAOTHelper.FalseFlag) {
				var comparer = new System.Collections.Generic.GenericComparer <DateTimeOffset> ();
				var eqcomparer = new System.Collections.Generic.GenericEqualityComparer <DateTimeOffset> ();
			}
		}
#endif
		public static readonly DateTimeOffset MaxValue = new DateTimeOffset (DateTime.MaxValue, TimeSpan.Zero);
		public static readonly DateTimeOffset MinValue = new DateTimeOffset (DateTime.MinValue, TimeSpan.Zero);
		
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
			return new DateTimeOffset (dt.Add (timeSpan).Ticks, utc_offset);
		}
	
		public DateTimeOffset AddDays (double days)
		{
			return new DateTimeOffset (dt.AddDays (days).Ticks, utc_offset);	
		}
		
		public DateTimeOffset AddHours (double hours)
		{
			return new DateTimeOffset (dt.AddHours (hours).Ticks, utc_offset);
		}

		public static DateTimeOffset operator + (DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
		{
			return dateTimeOffset.Add (timeSpan);
		}

		public DateTimeOffset AddMilliseconds (double milliseconds)
		{
			return new DateTimeOffset (dt.AddMilliseconds (milliseconds).Ticks, utc_offset);
		}

		public DateTimeOffset AddMinutes (double minutes)
		{
			return new DateTimeOffset (dt.AddMinutes (minutes).Ticks, utc_offset);	
		}

		public DateTimeOffset AddMonths (int months)
		{
			return new DateTimeOffset (dt.AddMonths (months).Ticks, utc_offset);
		}

		public DateTimeOffset AddSeconds (double seconds)
		{
			return new DateTimeOffset (dt.AddSeconds (seconds).Ticks, utc_offset);
		}

		public DateTimeOffset AddTicks (long ticks)
		{
			return new DateTimeOffset (dt.AddTicks (ticks).Ticks, utc_offset);	
		}

		public DateTimeOffset AddYears (int years)
		{
			return new DateTimeOffset (dt.AddYears (years).Ticks, utc_offset);
		}

		public static int Compare (DateTimeOffset first, DateTimeOffset second)
		{
			return first.CompareTo (second);	
		}

		public int CompareTo (DateTimeOffset other)
		{
			return UtcDateTime.CompareTo (other.UtcDateTime);
		}

		int IComparable.CompareTo (object obj)
		{
			return CompareTo ((DateTimeOffset) obj);
		}

		public static bool operator == (DateTimeOffset left, DateTimeOffset right)
		{
			return left.Equals (right);	
		}

		public bool Equals (DateTimeOffset other)
		{
			return UtcDateTime == other.UtcDateTime;
		}

		public override bool Equals (object obj)
		{
			if (obj is DateTimeOffset)
				return UtcDateTime == ((DateTimeOffset) obj).UtcDateTime;
			return false;
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

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, SerializationFormatter = true)]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			// An example SOAP serialization on MSFT is the following, so field 
			// names "DateTime" and "OffsetMinutes":
			//    <SOAP-ENV:Envelope ...>
			//    <SOAP-ENV:Body>
			//    <a1:DateTimeOffset id="ref-1" xmlns:a1="http://schemas.microsoft.com/clr/ns/System">
			//    <DateTime xsi:type="xsd:dateTime">2007-01-02T12:30:50.0000000+00:00</DateTime>
			//    <OffsetMinutes>0</OffsetMinutes>
			//    </a1:DateTimeOffset>
			//    </SOAP-ENV:Body>
			//    </SOAP-ENV:Envelope>
			DateTime dt0 = new DateTime (dt.Ticks).Subtract (utc_offset);
			info.AddValue ("DateTime", dt0);
			// MSFT BinaryFormatter output contains primitive code 6, i.e. Int16.
			info.AddValue ("OffsetMinutes", (Int16)utc_offset.TotalMinutes);
		}

		[System.Security.Permissions.SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, SerializationFormatter = true)]
		private DateTimeOffset(SerializationInfo info, StreamingContext context)
		{
			DateTime dt0 = (DateTime)info.GetValue ("DateTime", typeof(DateTime));
			Int16 totalMinutes = info.GetInt16 ("OffsetMinutes");
			utc_offset = TimeSpan.FromMinutes(totalMinutes);
			dt = dt0.Add(utc_offset);
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

		public static DateTimeOffset Parse (string input)
		{
			return Parse (input, null);
		}

		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider)
		{
			return Parse (input, formatProvider, DateTimeStyles.AllowWhiteSpaces);
		}

		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			DateTime d;
			DateTimeOffset dto;
			Exception exception = null;
			if (!DateTime.CoreParse (input, formatProvider, styles, out d, out dto, true, ref exception))
				throw exception;

			if (d.Ticks != 0 && dto.Ticks == 0)
				throw new ArgumentOutOfRangeException ("The UTC representation falls outside the 1-9999 year range");

			return dto;
		}

		public static DateTimeOffset ParseExact (string input, string format, IFormatProvider formatProvider)
		{
			return ParseExact (input, format, formatProvider, DateTimeStyles.AssumeLocal);
		}

		public static DateTimeOffset ParseExact (string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			if (format == String.Empty)
				throw new FormatException ("format is an empty string");

			return ParseExact (input, new string [] {format}, formatProvider, styles);
		}

		public static DateTimeOffset ParseExact (string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (input == String.Empty)
				throw new FormatException ("input is an empty string");

			if (formats == null)
				throw new ArgumentNullException ("formats");

			if (formats.Length == 0)
				throw new FormatException ("Invalid format specifier");

			if ((styles & DateTimeStyles.AssumeLocal) != 0 && (styles & DateTimeStyles.AssumeUniversal) != 0)
				throw new ArgumentException ("styles parameter contains incompatible flags");

			DateTimeOffset result;
			if (!ParseExact (input, formats, DateTimeFormatInfo.GetInstance (formatProvider), styles, out result))
				throw new FormatException ("Invalid format string");

			return result;
		}

		private static bool ParseExact (string input, string [] formats,
				DateTimeFormatInfo dfi, DateTimeStyles styles, out DateTimeOffset ret)
		{
			foreach (string format in formats)
			{
				if (format == null || format == String.Empty)
					throw new FormatException ("Invalid format string");

				DateTimeOffset result;
				if (DoParse (input, format, false, out result, dfi, styles)) {
					ret = result;
					return true;
				}
			}
			ret = DateTimeOffset.MinValue;
			return false;
		}

		private static bool DoParse (string input, 
				string format,
				bool exact,
				out DateTimeOffset result,
				DateTimeFormatInfo dfi,
				DateTimeStyles styles)
		{
			if ((styles & DateTimeStyles.AllowLeadingWhite) != 0) {
				format = format.TrimStart (null);
				input = input.TrimStart (null);
			}

			if ((styles & DateTimeStyles.AllowTrailingWhite) != 0) {
				format = format.TrimEnd (null);
				input = input.TrimEnd (null);
			}

			bool allow_white_spaces = false;
			if ((styles & DateTimeStyles.AllowInnerWhite) != 0)
				allow_white_spaces = true;

			bool useutc = false, use_invariants = false;
			if (format.Length == 1)
				format = DateTimeUtils.GetStandardPattern (format[0], dfi, out useutc, out use_invariants, true);

			int year = -1;
			int month = -1;
			int day = -1;
			int partial_hour = -1; // for 'hh tt' formats
			int hour = -1;
			int minute = -1;
			int second = -1;
			double fraction = -1;
			int temp_int = -1;
			TimeSpan offset = TimeSpan.MinValue;

			result = DateTimeOffset.MinValue;

			int fi = 0; //format iterator
			int ii = 0; //input iterator
			while (fi < format.Length) {
				int tokLen;
				char ch = format [fi];

				switch (ch) {
				case 'd':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (day != -1 || tokLen > 4)
						return false;

					if (tokLen <= 2)
						ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out day);
					else
						ii += ParseEnum (input, ii, tokLen == 3 ? dfi.AbbreviatedDayNames : dfi.DayNames, allow_white_spaces, out temp_int); 
					break;
				case 'f':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					ii += ParseNumber (input, ii, tokLen, true, allow_white_spaces, out temp_int);
					if (fraction >= 0 || tokLen > 7 || temp_int == -1)
						return false;
					fraction = (double)temp_int / Math.Pow (10, tokLen);
					break;
				case 'F':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					int digits;
					int read = ParseNumber (input, ii, tokLen, true, allow_white_spaces, out temp_int, out digits);
					if (temp_int == -1)
						ii += ParseNumber (input, ii, digits, true, allow_white_spaces, out temp_int);
					else
						ii += read;
					if (fraction >= 0 || tokLen > 7 || temp_int == -1)
						return false;	
					fraction = (double)temp_int / Math.Pow (10, digits);	
					break;
				case 'h':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (hour != -1 || tokLen > 2)
						return false;

					ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out temp_int);
					if (temp_int == -1)
						return false;

					if (partial_hour == -1)
						partial_hour = temp_int;
					else 
						hour = partial_hour + temp_int;
					break;
				case 'H':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (hour != -1 || tokLen > 2)
						return false;

					ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out hour);
					break;
				case 'm':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (minute != -1 || tokLen > 2)
						return false;

					ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out minute);
					break;
				case 'M':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (month != -1 || tokLen > 4)
						return false;

					if (tokLen <= 2)
						ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out month);
					else {
						ii += ParseEnum (input, ii, tokLen == 3 ? dfi.AbbreviatedMonthNames : dfi.MonthNames, allow_white_spaces, out month);
						month += 1;
					}

					break;
				case 's':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (second != -1 || tokLen > 2)
						return false;
					ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out second);
					break;
				case 't':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (hour != -1 || tokLen > 2)
						return false;

					ii += ParseEnum (input, ii,
							 tokLen == 1 ? new string[] {new string (dfi.AMDesignator[0], 1), new string (dfi.PMDesignator[0], 0)} 
							 	     : new string[] {dfi.AMDesignator, dfi.PMDesignator},
							 allow_white_spaces, out temp_int);
					if (temp_int == -1)
						return false;

					if (partial_hour == -1)
						partial_hour = temp_int * 12;
					else
						hour = partial_hour + temp_int * 12;
					break;
				case 'y':
					if (year != -1)
						return false;

					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (tokLen <= 2) {
						ii += ParseNumber (input, ii, 2, tokLen == 2, allow_white_spaces, out year);
						if (year != -1)
							year += DateTime.Now.Year - DateTime.Now.Year % 100;
					} else if (tokLen <= 4) { // yyy and yyyy accept up to 5 digits with leading 0
						int digit_parsed;
						ii += ParseNumber (input, ii, 5, false, allow_white_spaces, out year, out digit_parsed);
						if (digit_parsed < tokLen || (digit_parsed > tokLen && (year / Math.Pow (10, digit_parsed - 1) < 1)))
							return false;
					} else
						ii += ParseNumber (input, ii, tokLen, true, allow_white_spaces, out year);
					break;

					// The documentation is incorrect, they claim that K is the same as 'zz', but
					// it actually allows the format to contain 4 digits for the offset
				case 'K':
					tokLen = 1;
					int off_h, off_m = 0, sign;
					temp_int = 0;
					ii += ParseEnum (input, ii, new string [] {"-", "+"}, allow_white_spaces, out sign);
					ii += ParseNumber (input, ii, 4, false, false, out off_h);
					if (off_h == -1 || off_m == -1 || sign == -1)
						return false;

					if (sign == 0)
						sign = -1;
					offset = new TimeSpan (sign * off_h, sign * off_m, 0);
					break;
					
				case 'z':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					if (offset != TimeSpan.MinValue || tokLen > 3)
						return false;

					off_m = 0;
					temp_int = 0;
					ii += ParseEnum (input, ii, new string [] {"-", "+"}, allow_white_spaces, out sign);
					ii += ParseNumber (input, ii, 2, tokLen != 1, false, out off_h);
					if (tokLen == 3) {
						ii += ParseEnum (input, ii, new string [] {dfi.TimeSeparator}, false, out temp_int);
						ii += ParseNumber (input, ii, 2, true, false, out off_m);
					}
					if (off_h == -1 || off_m == -1 || sign == -1)
						return false;

					if (sign == 0)
						sign = -1;
					offset = new TimeSpan (sign * off_h, sign * off_m, 0);
					break;
				case ':':
					tokLen = 1;
					ii += ParseEnum (input, ii, new string [] {dfi.TimeSeparator}, false, out temp_int);
					if (temp_int == -1)
						return false;
					break;
				case '/':
					tokLen = 1;
					ii += ParseEnum (input, ii, new string [] {dfi.DateSeparator}, false, out temp_int);
					if (temp_int == -1)
						return false;
					break;
				case '%':
					tokLen = 1;
					if (fi != 0) 
						return false;
					break;
				case ' ':
					tokLen = 1;
					ii += ParseChar (input, ii, ' ', false, out temp_int);
					if (temp_int == -1)
						return false;
					break;
				case '\\':
					tokLen = 2;
					ii += ParseChar (input, ii, format [fi + 1], allow_white_spaces, out temp_int);
					if (temp_int == -1)
						return false;
					break;
				default:
					//Console.WriteLine ("un-parsed character: {0}", ch);
					tokLen = 1;
					ii += ParseChar (input, ii, format [fi], allow_white_spaces, out temp_int);
					if (temp_int == -1)
						return false;
					break;
				}
				fi += tokLen;
			}

			//Console.WriteLine ("{0}-{1}-{2} {3}:{4} {5}", year, month, day, hour, minute, offset);
			if (offset == TimeSpan.MinValue && (styles & DateTimeStyles.AssumeLocal) != 0)
				offset = TimeZone.CurrentTimeZone.GetUtcOffset (DateTime.Now);

			if (offset == TimeSpan.MinValue && (styles & DateTimeStyles.AssumeUniversal) != 0)
				offset = TimeSpan.Zero;

			if (hour < 0)		hour = 0;
			if (minute < 0)		minute = 0;
			if (second < 0)		second = 0;
			if (fraction < 0)	fraction = 0;
			if (year > 0 && month > 0 && day > 0) {
				result = new DateTimeOffset (year, month, day, hour, minute, second, 0, offset);
				result = result.AddSeconds (fraction);
				if ((styles & DateTimeStyles.AdjustToUniversal) != 0)
					result = result.ToUniversalTime ();
				return true;
			}

			return false;
		}

		private static int ParseNumber (string input, int pos, int digits, bool leading_zero, bool allow_leading_white, out int result)
		{
			int digit_parsed;
			return ParseNumber (input, pos, digits, leading_zero, allow_leading_white, out result, out digit_parsed);
		}

		private static int ParseNumber (string input, int pos, int digits, bool leading_zero, bool allow_leading_white, out int result, out int digit_parsed)
		{
			int char_parsed = 0;
			digit_parsed = 0;
			result = 0;
			for (; allow_leading_white && pos < input.Length && input[pos] == ' '; pos++)
				char_parsed++;

			for (; pos < input.Length && Char.IsDigit (input[pos]) && digits > 0; pos ++, char_parsed++, digit_parsed++, digits --)
				result = 10 * result + (byte) (input[pos] - '0');

			if (leading_zero && digits > 0)
				result = -1;

			if (digit_parsed == 0)
				result = -1;

			return char_parsed;
		}

		private static int ParseEnum (string input, int pos, string [] enums, bool allow_leading_white, out int result)
		{
			int char_parsed = 0;
			result = -1;
			for (; allow_leading_white && pos < input.Length && input[pos] == ' '; pos++)
				char_parsed ++;
			
			for (int i = 0; i < enums.Length; i++)
				if (input.Substring(pos).StartsWith (enums [i])) {
					result = i;
					break;
				}

			if (result >= 0)
				char_parsed += enums[result].Length;

			return char_parsed;	
		}
	
		private static int ParseChar (string input, int pos, char c, bool allow_leading_white, out int result)
		{
			int char_parsed = 0;
			result = -1;
			for (; allow_leading_white && pos < input.Length && input[pos] == ' '; pos++, char_parsed++)
				;

			if (pos < input.Length && input[pos] == c){
				result = (int) c;
				char_parsed ++;
			}

			return char_parsed;
		}

		public TimeSpan Subtract (DateTimeOffset value)
		{
			return UtcDateTime - value.UtcDateTime;
		}

		public DateTimeOffset Subtract (TimeSpan value)
		{
			return Add (-value);
		}

		public static TimeSpan operator - (DateTimeOffset left, DateTimeOffset right)
		{
			return left.Subtract (right);
		}

		public static DateTimeOffset operator - (DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
		{
			return dateTimeOffset.Subtract (timeSpan);	
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

			bool to_utc = false, use_invariant = false;
			if (format.Length == 1) {
				char fchar = format [0];
				try {
					format = DateTimeUtils.GetStandardPattern (fchar, dfi, out to_utc, out use_invariant, true);
				} catch {
					format = null;
				}
		
				if (format == null)
					throw new FormatException ("format is not one of the format specifier characters defined for DateTimeFormatInfo");
			}
			return to_utc ? DateTimeUtils.ToString (UtcDateTime, TimeSpan.Zero, format, dfi) : DateTimeUtils.ToString (DateTime, Offset, format, dfi);
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
