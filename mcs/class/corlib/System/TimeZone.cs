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
using System.Collections;
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
		static long timezone_check;

		// Constructor
		protected TimeZone ()
		{
		}

		// Properties
		public static TimeZone CurrentTimeZone {
			get {
				long now = DateTime.GetNow ();
				
				if (currentTimeZone == null || (now - timezone_check) > TimeSpan.TicksPerMinute) {
					currentTimeZone = new CurrentSystemTimeZone (now);
					timezone_check = now;
				}
				
				return currentTimeZone;
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

			TimeSpan utcOffset = GetUtcOffset (time);
			if (utcOffset.Ticks > 0) {
				if (DateTime.MaxValue - utcOffset < time)
					return DateTime.SpecifyKind (DateTime.MaxValue, DateTimeKind.Local);
			} else if (utcOffset.Ticks < 0) {
				if (time.Ticks + utcOffset.Ticks < DateTime.MinValue.Ticks)
					return DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Local);
			}

			DateTime local = time.Add (utcOffset);
			DaylightTime dlt = GetDaylightChanges (time.Year);
			if (dlt.Delta.Ticks == 0)
				return DateTime.SpecifyKind (local, DateTimeKind.Local);

			// FIXME: check all of the combination of
			//	- basis: local-based or UTC-based
			//	- hemisphere: Northern or Southern
			//	- offset: positive or negative

			// PST should work fine here.
			if (local < dlt.End && dlt.End.Subtract (dlt.Delta) <= local)
				return DateTime.SpecifyKind (local, DateTimeKind.Local);

			TimeSpan localOffset = GetUtcOffset (local);
			return DateTime.SpecifyKind (time.Add (localOffset), DateTimeKind.Local);
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

		//
		// This routine returns the TimeDiff that would have to be
		// added to "time" to turn it into a local time.   This would
		// be equivalent to call ToLocalTime.
		//
		// There is one important consideration:
		//
		//    This information is only valid during the minute it
		//    was called.
		//
		//    This only works with a real time, not one of the boundary
		//    cases like DateTime.MaxValue, so validation must be done
		//    before.
		// 
		//    This is intended to be used by DateTime.Now
		//
		// We use a minute, just to be conservative and cope with
		// any potential time zones that might be defined in the future
		// that might not nicely fit in hour or half-hour steps. 
		//    
		internal TimeSpan GetLocalTimeDiff (DateTime time)
		{
			return GetLocalTimeDiff (time, GetUtcOffset (time));
		}

		//
		// This routine is intended to be called by GetLocalTimeDiff(DatetTime)
		// or by ToLocalTime after validation has been performed
		//
		// time is the time to map, utc_offset is the utc_offset that
		// has been computed for calling GetUtcOffset on time.
		//
		// When called by GetLocalTime, utc_offset is assumed to come
		// from a time constructed by new DateTime (DateTime.GetNow ()), that
		// is a valid time.
		//
		// When called by ToLocalTime ranges are checked before this is
		// called.
		//
		internal TimeSpan GetLocalTimeDiff (DateTime time, TimeSpan utc_offset)
		{
			DaylightTime dlt = GetDaylightChanges (time.Year);

			if (dlt.Delta.Ticks == 0)
				return utc_offset;

			DateTime local = time.Add (utc_offset);
			if (local < dlt.End && dlt.End.Subtract (dlt.Delta) <= local)
				return utc_offset;

			if (local >= dlt.Start && dlt.Start.Add (dlt.Delta) > local)
				return utc_offset - dlt.Delta;

			return GetUtcOffset (local);
		}
	}

	[Serializable]
	internal class CurrentSystemTimeZone : TimeZone, IDeserializationCallback {

		// Fields
		private string m_standardName;
		private string m_daylightName;

		// A yearwise cache of DaylightTime.
		private Hashtable m_CachedDaylightChanges = new Hashtable (1);

		// the offset when daylightsaving is not on (in ticks)
		private long m_ticksOffset;

		// the offset when daylightsaving is not on.
		[NonSerialized]
		private TimeSpan utcOffsetWithOutDLS;
  
		// the offset when daylightsaving is on.
		[NonSerialized]
		private TimeSpan utcOffsetWithDLS;

		internal enum TimeZoneData
		{
			DaylightSavingStartIdx,
			DaylightSavingEndIdx,
			UtcOffsetIdx,
			AdditionalDaylightOffsetIdx
		};

		internal enum TimeZoneNames
		{
			StandardNameIdx,
			DaylightNameIdx
		};

		// Internal method to get timezone data.
		//    data[0]:  start of daylight saving time (in DateTime ticks).
		//    data[1]:  end of daylight saving time (in DateTime ticks).
		//    data[2]:  utcoffset (in TimeSpan ticks).
		//    data[3]:  additional offset when daylight saving (in TimeSpan ticks).
		//    name[0]:  name of this timezone when not daylight saving.
		//    name[1]:  name of this timezone when daylight saving.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool GetTimeZoneData (int year, out Int64[] data, out string[] names);

		// Constructor
		internal CurrentSystemTimeZone ()
		{
		}

		//
		// Initialized by the constructor
		//
		static int this_year;
		static DaylightTime this_year_dlt;
		
		//
		// The "lnow" parameter must be the current time, I could have moved
		// the code here, but I do not want to interfere with serialization
		// which is why I kept the other constructor around
		//
		internal CurrentSystemTimeZone (long lnow)
		{
			Int64[] data;
			string[] names;

			DateTime now = new DateTime (lnow);
			if (!GetTimeZoneData (now.Year, out data, out names))
				throw new NotSupportedException (Locale.GetText ("Can't get timezone name."));

			m_standardName = Locale.GetText (names[(int)TimeZoneNames.StandardNameIdx]);
			m_daylightName = Locale.GetText (names[(int)TimeZoneNames.DaylightNameIdx]);

			m_ticksOffset = data[(int)TimeZoneData.UtcOffsetIdx];

			DaylightTime dlt = GetDaylightTimeFromData (data);
			m_CachedDaylightChanges.Add (now.Year, dlt);
			OnDeserialization (dlt);
		}

		// Properties
		public override string DaylightName {
			get { return m_daylightName; }
		}

		public override string StandardName {
			get { return m_standardName; }
		}

		// Methods
		public override DaylightTime GetDaylightChanges (int year)
		{
			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException ("year", year +
					Locale.GetText (" is not in a range between 1 and 9999."));

			//
			// First we try the case for this year, very common, and is used
			// by DateTime.Now (a popular call) indirectly.
			//
			if (year == this_year)
				return this_year_dlt;
			
			lock (m_CachedDaylightChanges) {
				DaylightTime dlt = (DaylightTime) m_CachedDaylightChanges [year];
				if (dlt == null) {
					Int64[] data;
					string[] names;

					if (!GetTimeZoneData (year, out data, out names))
						throw new ArgumentException (Locale.GetText ("Can't get timezone data for " + year));

					dlt = GetDaylightTimeFromData (data);
					m_CachedDaylightChanges.Add (year, dlt);
				}
				return dlt;
			}
		}

		public override TimeSpan GetUtcOffset (DateTime time)
		{
			if (IsDaylightSavingTime (time))
				return utcOffsetWithDLS;

			return utcOffsetWithOutDLS;
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			OnDeserialization (null);
		}

		private void OnDeserialization (DaylightTime dlt)
		{
			if (dlt == null) {
				Int64[] data;
				string[] names;

				this_year = DateTime.Now.Year;
				if (!GetTimeZoneData (this_year, out data, out names))
					throw new ArgumentException (Locale.GetText ("Can't get timezone data for " + this_year));
				dlt = GetDaylightTimeFromData (data);
			} else
				this_year = dlt.Start.Year;
			
			utcOffsetWithOutDLS = new TimeSpan (m_ticksOffset);
			utcOffsetWithDLS = new TimeSpan (m_ticksOffset + dlt.Delta.Ticks);
			this_year_dlt = dlt;
		}

		private DaylightTime GetDaylightTimeFromData (long[] data)
		{
			return new DaylightTime (new DateTime (data[(int)TimeZoneData.DaylightSavingStartIdx]),
				new DateTime (data[(int)TimeZoneData.DaylightSavingEndIdx]),
				new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]));
		}

	}
}
