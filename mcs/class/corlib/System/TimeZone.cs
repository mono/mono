//
// System.TimeZone.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// 	   Ajay Kumar Dwivedi (adwiv@yahoo.com)
//
// (C) Ximian, Inc.
//

using System.Collections;
using System.Globalization;

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
			int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear (time);
			DaylightTime daylight = GetDaylightChanges (currentYear);

			return IsDaylightSavingTime (time, daylight);
		}

		public virtual bool IsDaylightSavingTime (DateTime time, DaylightTime daylightTimes)
		{
			if (daylightTimes == null)
				throw new ArgumentNullException ("daylightTimes is null.");

			if (daylightTimes.Delta == TimeSpan.Zero)
				return false; 

			if ((daylightTimes.Start <= time) && (time < daylightTimes.End))
				return true;
			else // time doesn't not fall in the span of daylightTimes
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

		// Constructor
		[MonoTODO ("Add internal calls to initialize the fields")]
		internal CurrentTimeZone ()
			: base ()
		{
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
                                throw new ArgumentOutOfRangeException (year + " is not in a range between 1 and 9999.");

                        if (daylightCache [year] == null) {
                                //TODO: do something and put the DaylightTime corresponding to the time in cache.
                                DaylightTime dlt = new DaylightTime (new DateTime (0),
						     new DateTime (0), new TimeSpan (0));
				daylightCache.Add (year, dlt);
                        }

			return (DaylightTime) daylightCache [year];
                }

		public override TimeSpan GetUtcOffset(DateTime time)
		{
                        if (IsDaylightSavingTime (time))
                                return utcOffsetWithDLS;
                        
                        return utcOffsetWithOutDLS;
                }
	}
}
