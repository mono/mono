#if WASM

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

namespace System {

	public partial class TimeZoneInfo {

		static TimeZoneInfo CreateLocal ()
		{
			return TimeZoneInfo.Utc;
		}

		static TimeZoneInfo FindSystemTimeZoneByIdCore (string id)
		{
			throw new NotImplementedException ();
		}

		static void GetSystemTimeZonesCore (List<TimeZoneInfo> systemTimeZones)
		{
		}
	}
}

#endif

