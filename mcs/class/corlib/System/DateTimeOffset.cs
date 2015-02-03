/*
 * System.DateTimeOffset.cs
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *	Marek Safar (marek.safar@gmail.com)
 *
 *  Copyright (C) 2007 Novell, Inc (http://www.novell.com) 
 *  Copyright 2012 Xamarin, Inc (http://www.xamarin.com) 
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

        // Constants
        internal const Int64 MaxOffset = TimeSpan.TicksPerHour * 14;
        internal const Int64 MinOffset = -MaxOffset;
	
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
			try {
				if (!DateTime.CoreParse (input, formatProvider, styles, out d, out dto, true, ref exception))
					throw exception;
			} catch (ArgumentOutOfRangeException ex) {
				throw new FormatException ("The UTC representation falls outside the 1-9999 year range", ex);
			}

			if (d.Ticks != 0 && dto.Ticks == 0)
				throw new FormatException ("The UTC representation falls outside the 1-9999 year range");

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

			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (formatProvider);
			DateTime d;
			DateTimeOffset result;
			Exception exception = null;
			bool longYear = false;
			try {
				if (!DateTime.CoreParseExact (input, formats, dfi, styles, out d, out result, true, ref longYear, true, ref exception, true))
					throw exception;
			} catch (ArgumentOutOfRangeException ex) {
				throw new FormatException ("The UTC representation falls outside the 1-9999 year range", ex);
			}

			return result;
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
