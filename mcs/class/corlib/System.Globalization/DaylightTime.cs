//
// System.Globalization.DaylightTime
//
// Author:
//   Chris Hynes (chrish@assistedsolutions.com)
//
// (C) 2001 Chris Hynes
//

using System.Globalization;

namespace System.Globalization
{
	public class DaylightTime
	{
		DateTime start, end;
		TimeSpan delta;

		public DaylightTime(DateTime start, DateTime end, TimeSpan delta)
		{
			this.start = start;
			this.end = end;
			this.delta = delta;
		}

		public DateTime Start
		{
			get
			{
				return start;
			}
		}

		public DateTime End
		{
			get
			{
				return end;
			}
		}

		public TimeSpan Delta
		{
			get
			{
				return delta;
			}
		}
	}
}
