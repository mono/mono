//
// System.TimeZone.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System.Globalization;

namespace System {

	[Serializable]
	public abstract class TimeZone
	{
		// Fields
		static TimeZone local;
		
		// Constructor
		protected TimeZone ()
		{
		}

		// Properties
		public static TimeZone CurrentTimeZone
		{
			get { return local; }
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
			int currentYear = Calendar.GetYear (time);
			DaylightTime daylight = GetDaylightChanges (currentYear);
			if ((daylight.Start <= time) && (time < daylight.End))
				return true;
			else
				return false;
		}

		public virtual bool IsDaylightSavingTime (DateTime time, DaylightTime daylightTimes)
		{
			if (daylightTimes == null)
				throw new ArgumentNullException ("daylightTimes is null.");
			if ((daylightTimes.Start <= time) && (time < daylightTimes.End))
				return true;
			else
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
}
