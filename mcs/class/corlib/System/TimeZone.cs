//
// System.TimeZone.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// 	   Ajay Kumar Dwivedi (adwiv@yahoo.com)
//         Martin Baulig (martin@gnome.org)
//
// (C) Ximian, Inc.
//

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	
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
		public static TimeZone CurrentTimeZone
		{
			get {
				if (currentTimeZone == null)
					currentTimeZone = new CurrentTimeZone ();

				return currentTimeZone;
			}
		}
		
		public abstract string DaylightName
		{
			get;
		}

		public abstract string StandardName
		{
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
                                if (daylightTimes.Start.Ticks < time.Ticks
				    && daylightTimes.End.Ticks > time.Ticks)
                                        return true; // time lies between Start and End

			} else {  // We are in the southern hemisphere.
				if (time.Year == daylightTimes.Start.Year && time.Year == daylightTimes.End.Year)
                                        if (time.Ticks < daylightTimes.End.Ticks
					    || time.Ticks > daylightTimes.Start.Ticks)
                                                return true; // time is less than End OR more than Start 
                        }

			return false;
                }
		
		public virtual DateTime ToLocalTime (DateTime time)
		{
			return time + GetUtcOffset (time);
		}

		public virtual DateTime ToUniversalTime (DateTime time)
		{
			return time - GetUtcOffset (time);
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

		internal enum TimeZoneData {
			DaylightSavingStartIdx,
			DaylightSavingEndIdx,
			UtcOffsetIdx,
			AdditionalDaylightOffsetIdx
		};

		internal enum TimeZoneNames {
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
		private static extern bool GetTimeZoneData (int year,
							    out Int64[] data,
							    out string[] names);

		// Constructor
		internal CurrentTimeZone ()
			: base ()
		{
			Int64[] data;
			string[] names;

			DateTime now = new DateTime(DateTime.GetNow ());			
			if (!GetTimeZoneData (now.Year, out data, out names))
				throw new NotSupportedException (Locale.GetText ("Can't get timezone name"));

			standardName = Locale.GetText (names[(int)TimeZoneNames.StandardNameIdx]);
			daylightName = Locale.GetText (names[(int)TimeZoneNames.DaylightNameIdx]);

			utcOffsetWithOutDLS = new TimeSpan (data[(int)TimeZoneData.UtcOffsetIdx]);
			utcOffsetWithDLS = new TimeSpan (data[(int)TimeZoneData.UtcOffsetIdx] + data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]);
		}

		// Properties
		public override string DaylightName
                {
                        get { return daylightName; }
                }
                
                public override string StandardName 
                {
                        get { return standardName; }
                }

		// Methods
		[MonoTODO]
		public override DaylightTime GetDaylightChanges (int year)
                {
                        if (year < 1 || year > 9999)
                                throw new ArgumentOutOfRangeException ("year", year + " is not in a range between 1 and 9999.");

                        if (daylightCache [year] == null) {
				lock (this) {
		                        if (daylightCache [year] == null) {
						Int64[] data;
						string[] names;

						if (!GetTimeZoneData (year, out data, out names))
							throw new ArgumentException (Locale.GetText ("Can't get timezone data for " + year));

						DaylightTime dlt = new DaylightTime (new DateTime (data[(int)TimeZoneData.DaylightSavingStartIdx]),
										     new DateTime (data[(int)TimeZoneData.DaylightSavingEndIdx]),
										     new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]));
						daylightCache.Add (year, dlt);
					};
				};
                        }

			return (DaylightTime) daylightCache [year];
                }

		public override TimeSpan GetUtcOffset (DateTime time)
		{
                        if (IsDaylightSavingTime (time))
                                return utcOffsetWithDLS;
                        
                        return utcOffsetWithOutDLS;
                }
	}
}
