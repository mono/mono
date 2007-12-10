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

		public DateTimeOffset AddSeconds (double seconds)
		{
			return new DateTimeOffset (this.dt.AddSeconds (seconds));
		}
		
		public int CompareTo (DateTimeOffset other)
		{
			return UtcDateTime.CompareTo (other);
		}

		public int CompareTo (object other)
		{
			return UtcDateTime.CompareTo (other);
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

		public override int GetHashCode ()
		{
			return dt.GetHashCode () ^ utc_offset.GetHashCode ();
		}

		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public void OnDeserialization (object sender)
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public string ToString (string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException ();	
		}

		public DateTime DateTime {
			get {
				if (dt.Kind == DateTimeKind.Unspecified)
					return dt;
				else
					return DateTime.SpecifyKind (dt, DateTimeKind.Unspecified);
			}
		}

		public TimeSpan Offset {
			get {
				return utc_offset;	
			}	
		}

		public DateTime UtcDateTime {
			get {
				return DateTime.SpecifyKind (dt - utc_offset, DateTimeKind.Utc);
			}	
		}
		
		public static DateTimeOffset Now {
			get {
				return new DateTimeOffset (DateTime.Now);
			}
		}
		
		public static bool operator < (DateTimeOffset dto1, DateTimeOffset dto2)
		{
			return dto1.UtcDateTime < dto2.UtcDateTime;
		}
		
		public static bool operator > (DateTimeOffset dto1, DateTimeOffset dto2)
		{
			return dto1.UtcDateTime > dto2.UtcDateTime;
		}			
	}
}

#endif
