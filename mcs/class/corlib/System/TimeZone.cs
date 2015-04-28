//
// System.TimeZone.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Ajay Kumar Dwivedi (adwiv@yahoo.com)
//   Martin Baulig (martin@gnome.org)
//
// (C) Ximian, Inc.
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
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
//
// TODO:
//
//    Rewrite ToLocalTime to use GetLocalTimeDiff(DateTime,TimeSpan),
//    this should only leave the validation at the beginning (for MaxValue)
//    and then call the helper function.  This would remove all the
//    ifdefs in that code, and replace it with only one, for the construction
//    of the object.
//
//    Rewrite ToUniversalTime to use a similar setup to that
//
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	public abstract class TimeZone
	{
		// Fields
		static TimeZone currentTimeZone;

		[NonSerialized]
		static object tz_lock = new object ();
		[NonSerialized]
		static long timezone_check;

		// Constructor
		protected TimeZone ()
		{
		}

		// Properties
		public static TimeZone CurrentTimeZone {
			get {
				long now = DateTime.UtcNow.Ticks;
				TimeZone tz = currentTimeZone;
				
				lock (tz_lock) {
					if (tz == null || Math.Abs (now - timezone_check) > TimeSpan.TicksPerMinute) {
						tz = new CurrentSystemTimeZone ();
						timezone_check = now;

						currentTimeZone = tz;
					}
				}
				
				return tz;
			}
		}

		public abstract string DaylightName {
			get;
		}

		public abstract string StandardName {
			get;
		}

		// Methods
		public abstract DaylightTime GetDaylightChanges (int year);

		public abstract TimeSpan GetUtcOffset (DateTime time);

		public virtual bool IsDaylightSavingTime (DateTime time)
		{
			return IsDaylightSavingTime (time, GetDaylightChanges (time.Year));
		}

		public static bool IsDaylightSavingTime (DateTime time, DaylightTime daylightTimes)
		{
			if (daylightTimes == null)
				throw new ArgumentNullException ("daylightTimes");

			// If Start == End, then DST is off
			if (daylightTimes.Start.Ticks == daylightTimes.End.Ticks)
				return false;

			//We are in the northern hemisphere.
			if (daylightTimes.Start.Ticks < daylightTimes.End.Ticks) {
				if (daylightTimes.Start.Ticks < time.Ticks && daylightTimes.End.Ticks > time.Ticks)
					return true; // time lies between Start and End

			}
			else {  // We are in the southern hemisphere.
				if (time.Year == daylightTimes.Start.Year && time.Year == daylightTimes.End.Year)
					if (time.Ticks < daylightTimes.End.Ticks || time.Ticks > daylightTimes.Start.Ticks)
						return true; // time is less than End OR more than Start 
			}

			return false;
		}

		public virtual DateTime ToLocalTime (DateTime time)
		{
			if (time.Kind == DateTimeKind.Local)
				return time;

			TimeSpan utcOffset = GetUtcOffset (new DateTime (time.Ticks));
			if (utcOffset.Ticks > 0) {
				if (DateTime.MaxValue - utcOffset < time)
					return DateTime.SpecifyKind (DateTime.MaxValue, DateTimeKind.Local);
			} else if (utcOffset.Ticks < 0) {
				if (time.Ticks + utcOffset.Ticks < DateTime.MinValue.Ticks)
					return DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Local);
			}

			return DateTime.SpecifyKind (time.Add (utcOffset), DateTimeKind.Local);
		}

		public virtual DateTime ToUniversalTime (DateTime time)
		{
			if (time.Kind == DateTimeKind.Utc)
				return time;

			TimeSpan offset = GetUtcOffset (time);

			if (offset.Ticks < 0) {
				if (DateTime.MaxValue + offset < time)
					return DateTime.SpecifyKind (DateTime.MaxValue, DateTimeKind.Utc);
			} else if (offset.Ticks > 0) {
				if (DateTime.MinValue + offset > time)
					return DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Utc);
			}

			return DateTime.SpecifyKind (new DateTime (time.Ticks - offset.Ticks), DateTimeKind.Utc);
		}

		internal static void ClearCachedData ()
		{
			currentTimeZone = null;
		}
	}

	[Serializable]
	internal class CurrentSystemTimeZone : TimeZone {

		readonly  TimeZoneInfo  LocalTimeZone;

		// Constructor
		internal CurrentSystemTimeZone ()
		{
			LocalTimeZone = TimeZoneInfo.Local;
		}

		public override string DaylightName {
			get {
				return LocalTimeZone.DaylightName;
			}
		}

		public override string StandardName {
			get {
				return LocalTimeZone.StandardName;
			}
		}

		public override System.Globalization.DaylightTime GetDaylightChanges (int year)
		{
			return LocalTimeZone.GetDaylightChanges (year);
		}

		public override TimeSpan GetUtcOffset (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
				return TimeSpan.Zero;

			return LocalTimeZone.GetUtcOffset (dateTime);
		}

		public override bool IsDaylightSavingTime (DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc)
				return false;

			return LocalTimeZone.IsDaylightSavingTime (dateTime);
		}
	}
}
