//
// System.TimeZone.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Ajay Kumar Dwivedi (adwiv@yahoo.com)
//   Martin Baulig (martin@gnome.org)
//
// (C) Ximian, Inc.
//

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

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
	public abstract class TimeZone
	{
		// Fields
		private static TimeZone currentTimeZone;

		// Constructor
		protected TimeZone ()
		{
		}

		// Properties
		public static TimeZone CurrentTimeZone {
			get {
				if (currentTimeZone == null)
					currentTimeZone = new CurrentTimeZone ();

				return currentTimeZone;
			}
		}

		internal static void ClearCurrentTimeZone ()
		{
			currentTimeZone = null;
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
			DaylightTime dlt = GetDaylightChanges (time.Year);
			TimeSpan utcOffset = GetUtcOffset (time);
			if (utcOffset.Ticks > 0) {
				if (DateTime.MaxValue - utcOffset < time)
					return DateTime.MaxValue;
			//} else if (utcOffset.Ticks < 0) {
			//	LAMESPEC: MS.NET fails to check validity here
			//	it may throw ArgumentOutOfRangeException.
			}

			DateTime local = time.Add (utcOffset);
			if (dlt.Delta.Ticks == 0)
				return local;

			// FIXME: check all of the combination of
			//	- basis: local-based or UTC-based
			//	- hemisphere: Northern or Southern
			//	- offset: positive or negative

			// PST should work fine here.
			if (local < dlt.End && dlt.End.Subtract (dlt.Delta) <= local)
				return local;
			if (local >= dlt.Start && dlt.Start.Add (dlt.Delta) > local)
				return local.Subtract (dlt.Delta);

			TimeSpan localOffset = GetUtcOffset (local);
			return time.Add (localOffset);
		}

		public virtual DateTime ToUniversalTime (DateTime time)
		{
			TimeSpan offset = GetUtcOffset (time);

			if (offset.Ticks < 0) {
				if (DateTime.MaxValue + offset < time)
					return DateTime.MaxValue;
			} else if (offset.Ticks > 0) {
				if (DateTime.MinValue + offset > time)
					return DateTime.MinValue;
			}

			return new DateTime (time.Ticks - offset.Ticks);
		}
	}

	internal class CurrentTimeZone : TimeZone
	{
		// Fields
		private static string daylightName;
		private static string standardName;

		// A yearwise cache of DaylightTime.
		private static Hashtable daylightCache = new Hashtable (1);

		// the offset when daylightsaving is not on.
		private static TimeSpan utcOffsetWithOutDLS;

		// the offset when daylightsaving is on.
		private static TimeSpan utcOffsetWithDLS;

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
		internal CurrentTimeZone ()
			: base ()
		{
			Int64[] data;
			string[] names;

			DateTime now = new DateTime(DateTime.GetNow ());			
			if (!GetTimeZoneData (now.Year, out data, out names))
				throw new NotSupportedException (Locale.GetText ("Can't get timezone name."));

			standardName = Locale.GetText (names[(int)TimeZoneNames.StandardNameIdx]);
			daylightName = Locale.GetText (names[(int)TimeZoneNames.DaylightNameIdx]);

			utcOffsetWithOutDLS = new TimeSpan (data[(int)TimeZoneData.UtcOffsetIdx]);
			utcOffsetWithDLS = new TimeSpan (data[(int)TimeZoneData.UtcOffsetIdx]
				+ data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]);
		}

		// Properties
		public override string DaylightName {
			get { return daylightName; }
		}

		public override string StandardName {
			get { return standardName; }
		}

		// Methods
		[MonoTODO]
		public override DaylightTime GetDaylightChanges (int year)
		{
			if (year < 1 || year > 9999)
				throw new ArgumentOutOfRangeException ("year", year +
					Locale.GetText (" is not in a range between 1 and 9999."));

			lock (daylightCache) {
				DaylightTime dlt = (DaylightTime) daylightCache [year];
				if (dlt == null) {
					Int64[] data;
					string[] names;

					if (!GetTimeZoneData (year, out data, out names))
						throw new ArgumentException (Locale.GetText ("Can't get timezone data for " + year));

					dlt = new DaylightTime (new DateTime (data[(int)TimeZoneData.DaylightSavingStartIdx]),
									     new DateTime (data[(int)TimeZoneData.DaylightSavingEndIdx]),
									     new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]));
					daylightCache.Add (year, dlt);
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
	}
}
