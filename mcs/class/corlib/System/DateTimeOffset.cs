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

		public static DateTimeOffset Parse (string input)
		{
			return Parse (input, null);
		}

		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider)
		{
			return Parse (input, formatProvider, DateTimeStyles.AllowWhiteSpaces);
		}

		[MonoTODO]
		public static DateTimeOffset Parse (string input, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			throw new NotImplementedException ();
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

			DateTimeOffset result;
			if (!ParseExact (input, formats, DateTimeFormatInfo.GetInstance (formatProvider), styles, out result))
				throw new FormatException ();

			return result;
		}

		private static bool ParseExact (string input, string [] formats,
				DateTimeFormatInfo dfi, DateTimeStyles styles, out DateTimeOffset ret)
		{
			foreach (string format in formats)
			{
				if (format == null || format == String.Empty)
					throw new FormatException ("Invlid Format Sting");

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
			bool useutc=false, use_invariants = false;
			if (format.Length == 1)
				format = DateTimeUtils.GetStandardPattern (format[0], dfi, out useutc, out use_invariants, true);

			int year = -1;
			int month = -1;
			int day = -1;
			int hour = -1;
			int minute = -1;
			TimeSpan offset = TimeSpan.Zero;

			int fi = 0; //format iterator
			int ii = 0; //input iterator
			while (fi < format.Length) {
				int tokLen;
				char ch = format [fi];

				switch (ch) {
				case 'd':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							day = Int32.Parse (input.Substring (ii, l));
							ii += l;
							break;
						case 2:
							day = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						case 3:
						case 4:
							ii = input.IndexOf (format [fi + tokLen]);
							break;
						default:
							throw new FormatException ();
							break;
					}
					
					break;
				case 'h':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch(tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							if (hour == -1)
								hour = 0;
							hour += Int32.Parse (input.Substring (ii, l));
							ii += l;
							break;
						case 2:
							if (hour == -1)
								hour = 0;
							hour += Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						default:
							throw new FormatException ();
							break;
					}
					break;
				case 'H':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							hour = Int32.Parse (input.Substring (ii, l));
							ii += l;
							break;
						case 2:
							hour = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						default:
							throw new FormatException ();
							break;
					}
					break;
				case 'm':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							minute = Int32.Parse (input.Substring (ii, l));
							ii += l;
							break;
						case 2:
							minute = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						default:
							throw new FormatException ();
							break;
					}
					break;
				case 'M':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							month = Int32.Parse (input.Substring (ii, l));
							ii += l;
							break;
						case 2:
							month = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						case 3:
							for (int i = 0; i < 13; i++)
								if (input.Substring (ii).StartsWith (dfi.AbbreviatedMonthNames [i])) {
									month = i + 1;
									ii += dfi.AbbreviatedMonthNames [i].Length;
									break;
								}
							break;
						case 4:
							for (int i = 0; i < 13; i++)
								if (input.Substring (ii).StartsWith (dfi.MonthNames [i])) {
									month = i + 1;
									ii += dfi.MonthNames [i].Length;
									break;
								}
							break;
						default:
							throw new FormatException ();
					}
					break;
				case 't':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							if (input [ii] == dfi.PMDesignator [0]) {
								if (hour == -1)
									hour = 0;
								hour += 12;
							} else if (input [ii] != dfi.AMDesignator [0])
								throw new FormatException ();
							ii ++;
							break;
						case 2:
							if (input.Substring (ii).StartsWith (dfi.PMDesignator)) {
								if (hour == -1)
									hour = 0;
								hour += 12;
								ii += dfi.PMDesignator.Length;
							} else if (input.Substring (ii).StartsWith (dfi.AMDesignator))
								ii += dfi.AMDesignator.Length;
							else
								throw new FormatException ();
							break;

						default:
							throw new FormatException ();
							break;
					}
					break;
				case 'y':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							year = Int32.Parse (input.Substring (ii, l));
							year += DateTime.Now.Year - DateTime.Now.Year % 100;
							ii += l;
							break;
						case 2:
							year = Int32.Parse (input.Substring (ii, 2));
							year += DateTime.Now.Year - DateTime.Now.Year % 100;
							ii += 2;
							break;
						case 3:
							throw new NotImplementedException ();
							//ToString ("yyy") seems to print 4-digits year...
							year = Int32.Parse (input.Substring (ii, 3));
							year += DateTime.Now.Year - DateTime.Now.Year % 1000;
							ii += 3;
							break;
						default:
							year = Int32.Parse (input.Substring (ii, tokLen));
							ii += tokLen;
							break;
					}
					break;
				case 'z':
					tokLen = DateTimeUtils.CountRepeat (format, fi, ch);
					int off_h, off_m = 0, sign;
					switch (input[ii]) {
						case '+': sign = 1; break;
						case '-': sign = -1; break;
						default:
							throw new FormatException ();
							break;
					}
					ii ++;
					switch (tokLen) {
						case 1:
							int l = 1;
							if (input.Length != ii + 1 && System.Char.IsDigit (input[ii + 1]))
								l = 2;
							off_h = Int32.Parse (input.Substring (ii, l));
							break;
						case 2:
							off_h = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						case 3:
							off_h = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							if (input [ii++] != ':')
								throw new FormatException ();
							off_m = Int32.Parse (input.Substring (ii, 2));
							ii += 2;
							break;
						default:
							throw new FormatException ();
							break;
					}
					offset = new TimeSpan (sign * off_h, sign * off_m, 0);
					break;
				case ':':
					tokLen = 1;
					if (!input.Substring (ii).StartsWith (dfi.TimeSeparator))
						throw new FormatException ();
					ii += dfi.TimeSeparator.Length;
					break;
				case '/':
					tokLen = 1;
					if (!input.Substring (ii).StartsWith (dfi.DateSeparator))
						throw new FormatException ();
					ii += dfi.DateSeparator.Length;
					break;
				case ' ':
					tokLen = 1;
					ii++;
					break;
				default:
					tokLen = 1;
					ii++;
					Console.WriteLine ("un-parsed character: {0}", ch);
					break;
				}
				fi += tokLen;
			}

			if (day > 0 && month > 0 && year > 0 && hour > 0 && minute > 0) {
				result = new DateTimeOffset (year, month, day, hour, minute, 0, offset);
				return true;
			}
			if (day > 0 && month > 0 && year > 0) {
				result = new DateTimeOffset (year, month, day, 0, 0, 0, offset);
				return true;
			}

			result = DateTimeOffset.MinValue;
			return false;
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
#endif
