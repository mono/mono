namespace System.Globalization
{
	static class GlobalizationGate
	{
		internal static string[] GetJapaneseEraNames ()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			return JapaneseCalendar.EraNames ();
		}

		internal static string[] GetJapaneseEnglishEraNames ()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			return JapaneseCalendar.EnglishEraNames ();
		}

		internal static Calendar GetJapaneseCalendarDefaultInstance ()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			return JapaneseCalendar.GetDefaultInstance();
		}

		static volatile DateTimeFormatInfo s_jajpDTFI;
		static volatile DateTimeFormatInfo s_zhtwDTFI;

		//
		// Create a Japanese DTFI which uses JapaneseCalendar.  This is used to parse
		// date string with Japanese era name correctly even when the supplied DTFI
		// does not use Japanese calendar.
		// The created instance is stored in global s_jajpDTFI.
		//
		// Moved here from CoreFX.
		//
		internal static DateTimeFormatInfo GetJapaneseCalendarDTFI()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			DateTimeFormatInfo temp = s_jajpDTFI;
			if (temp == null) {
				temp = new CultureInfo("ja-JP", false).DateTimeFormat;
				temp.Calendar = GlobalizationGate.GetJapaneseCalendarDefaultInstance();
				s_jajpDTFI = temp;
			}
			return (temp);
		}

		//
		// Create a Taiwan DTFI which uses TaiwanCalendar.  This is used to parse
		// date string with era name correctly even when the supplied DTFI
		// does not use Taiwan calendar.
		// The created instance is stored in global s_zhtwDTFI.
		//
		// Moved here from CoreFX.
		//
		internal static DateTimeFormatInfo GetTaiwanCalendarDTFI()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			DateTimeFormatInfo temp = s_zhtwDTFI;
			if (temp == null) {
				temp = new CultureInfo("zh-TW", false).DateTimeFormat;
				temp.Calendar = TaiwanCalendar.GetDefaultInstance();
				s_zhtwDTFI = temp;
			}
			return (temp);
		}

		internal static Calendar GetTaiwanCalendarDefaultInstance ()
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			return TaiwanCalendar.GetDefaultInstance();
		}

		internal static bool IsJapaneseCalendar (Calendar calendar)
		{
			if (GlobalizationMode.Invariant)
				throw new PlatformNotSupportedException ();
			return calendar.GetType () == typeof (JapaneseCalendar);
		}
	}
}
