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
			return UtcDateTime.CompareTo (other);
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

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string ToString (IFormatProvider formatProvider)
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public string ToString (string format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string ToString (string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException ();	
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
