//
// CultureData.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

using System;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[StructLayout (LayoutKind.Sequential)]
	class CultureData
	{
#region Sync with object-internals.h
		// Time
		private String sAM1159; // (user can override) AM designator
		private String sPM2359; // (user can override) PM designator
		private String sTimeSeparator;
		private volatile String[] saLongTimes; // (user can override) time format
		private volatile String[] saShortTimes; // short time format

		// Calendar specific data
		private int iFirstDayOfWeek; // (user can override) first day of week (gregorian really)
		private int iFirstWeekOfYear; // (user can override) first week of year (gregorian really)
#endregion		
        private volatile int[] waCalendars; // all available calendar type(s).  The first one is the default calendar

        // Store for specific data about each calendar
        private CalendarData[] calendars; // Store for specific calendar data

		// Language
		private String sISO639Language; // ISO 639 Language Name

		readonly string sRealName;

		bool bUseOverrides;

		// TODO: should query runtime with culture name for a list of culture's calendars
		int calendarId;

		int numberIndex;

		int iDefaultAnsiCodePage;
		int iDefaultOemCodePage;
		int iDefaultMacCodePage;
		int iDefaultEbcdicCodePage;
		bool isRightToLeft;
		string sListSeparator;

		private CultureData (string name)
		{
			this.sRealName = name;
		}

		static CultureData s_Invariant;

		public static CultureData Invariant {
			get {
				if (s_Invariant == null) {
					var invariant = new CultureData ("");

					// Language
					invariant.sISO639Language = "iv";                   // ISO 639 Language Name

					// Time
					invariant.sAM1159 = "AM";                   // AM designator
					invariant.sPM2359 = "PM";                   // PM designator
					invariant.sTimeSeparator = ":";
					invariant.saLongTimes = new String[] { "HH:mm:ss" };                             // time format
					invariant.saShortTimes = new String[] { "HH:mm", "hh:mm tt", "H:mm", "h:mm tt" }; // short time format

					// Calendar specific data
					invariant.iFirstDayOfWeek = 0;                      // first day of week
					invariant.iFirstWeekOfYear = 0;                      // first week of year
					invariant.waCalendars = new int[] { (int)CalendarId.GREGORIAN };       // all available calendar type(s).  The first one is the default calendar

					// Store for specific data about each calendar
		    		invariant.calendars = new CalendarData[CalendarData.MAX_CALENDARS];
		    		invariant.calendars[0] = CalendarData.Invariant;

					invariant.iDefaultAnsiCodePage = 1252;                   // default ansi code page ID (ACP)
					invariant.iDefaultOemCodePage = 437;                    // default oem code page ID (OCP or OEM)
					invariant.iDefaultMacCodePage = 10000;                  // default macintosh code page
					invariant.iDefaultEbcdicCodePage = 037;                    // default EBCDIC code page

					invariant.sListSeparator = ",";
					
					Interlocked.CompareExchange (ref s_Invariant, invariant, null);
				}

				return s_Invariant;
			}
		}

		public static CultureData GetCultureData (string cultureName, bool useUserOverride)
		{
			try {
				var ci = new CultureInfo (cultureName, useUserOverride);
				return ci.m_cultureData;
			} catch {
				return null;
			}
		}

		public static CultureData GetCultureData (string cultureName, bool useUserOverride, int datetimeIndex, int calendarId, int numberIndex, string iso2lang,
			int ansiCodePage, int oemCodePage, int macCodePage, int ebcdicCodePage, bool rightToLeft, string listSeparator)
		{
			if (string.IsNullOrEmpty (cultureName))
				return Invariant;

			var cd = new CultureData (cultureName);
			cd.fill_culture_data (datetimeIndex);
			cd.bUseOverrides = useUserOverride;
			cd.calendarId = calendarId;
			cd.numberIndex = numberIndex;
			cd.sISO639Language = iso2lang;
			cd.iDefaultAnsiCodePage = ansiCodePage;
			cd.iDefaultOemCodePage = oemCodePage;
			cd.iDefaultMacCodePage = macCodePage;
			cd.iDefaultEbcdicCodePage = ebcdicCodePage;
			cd.isRightToLeft = rightToLeft;
			cd.sListSeparator = listSeparator;
			return cd;
		}

		internal static CultureData GetCultureData (int culture, bool bUseUserOverride)
		{
			// Legacy path which we should never hit
			return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void fill_culture_data (int datetimeIndex);

		public CalendarData GetCalendar (int calendarId)
		{
			// arrays are 0 based, calendarIds are 1 based
			int calendarIndex = calendarId - 1;

			// Have to have calendars
			if (calendars == null)
			{
				calendars = new CalendarData[CalendarData.MAX_CALENDARS];
			}

			var calendarData = calendars[calendarIndex];
			if (calendarData == null) {
				calendarData = new CalendarData (sRealName, calendarId, bUseOverrides);
				calendars [calendarIndex] = calendarData;
			}

			return calendarData;
		}

		internal String[] LongTimes {
			get {
				return saLongTimes;
			}
		}

		internal String[] ShortTimes {
			get {
				return saShortTimes;
			}
		}

		internal String SISO639LANGNAME {
			get {
				return sISO639Language;
			}
		}

		internal int IFIRSTDAYOFWEEK {
			get {
				return iFirstDayOfWeek;
			}
		}

		internal int IFIRSTWEEKOFYEAR {
			get {
				return iFirstWeekOfYear;
			}
		}

		internal String SAM1159 {
			get {
				return sAM1159;
			}
		}

		internal String SPM2359 {
			get {
				return sPM2359;
			}
		}

		internal String TimeSeparator {
			get {
				return sTimeSeparator;
			}
		}

		internal int[] CalendarIds {
			get {
				if (this.waCalendars == null) {
					// Need this specialization because GetJapaneseCalendarDTFI/GetTaiwanCalendarDTFI depend on
					// optional calendars
					switch (sISO639Language) {
					case "ja":
						waCalendars = new int[] { calendarId, Calendar.CAL_JAPAN };
						break;
					case "zh":
						waCalendars = new int[] { calendarId, Calendar.CAL_TAIWAN };
						break;
					case "he":
						waCalendars = new int[] { calendarId, Calendar.CAL_HEBREW };
						break;
					default:
						waCalendars = new int [] { calendarId };
						break;
					}
				}

				return waCalendars;
			}
		}

		internal CalendarId[] GetCalendarIds() 
		{
			var items = new CalendarId[CalendarIds.Length];
			for (int i = 0; i < CalendarIds.Length; i++)
				items[i] = (CalendarId)CalendarIds[i];
			return items;
		}

		internal bool IsInvariantCulture {
			get {
				return string.IsNullOrEmpty (sRealName);
			}
		}

		internal String CultureName {
			get {
				return sRealName;
			}
		}

		internal String SCOMPAREINFO {
			get {
				return "";
			}
		}

		internal String STEXTINFO {
			get {
				return sRealName;
			}
		}

		internal int ILANGUAGE {
			get {
				return 0;
			}
		}

		internal int IDEFAULTANSICODEPAGE {
			get {
				return iDefaultAnsiCodePage;
			}
		}

		internal int IDEFAULTOEMCODEPAGE {
			get {
				return iDefaultOemCodePage;
			}
		}

		internal int IDEFAULTMACCODEPAGE {
			get {
				return iDefaultMacCodePage;
			}
		}

		internal int IDEFAULTEBCDICCODEPAGE {
			get {
				return iDefaultEbcdicCodePage;
			}
		}

		internal bool IsRightToLeft {
			get {
				return isRightToLeft;
			}
		}

		internal String SLIST {
			get {
				return sListSeparator;
			}
		}

#region from reference sources

        // Are overrides enabled?
        internal bool UseUserOverride
        {
            get
            {
                return this.bUseOverrides;
            }
        }

		// Native calendar names.  index of optional calendar - 1, empty if no optional calendar at that number
		internal String CalendarName(int calendarId)
		{
			// Get the calendar
			return GetCalendar(calendarId).sNativeName;
		}

		// All of our era names
		internal String[] EraNames(int calendarId)
		{
			Contract.Assert(calendarId > 0, "[CultureData.saEraNames] Expected Calendar.ID > 0");

			return this.GetCalendar(calendarId).saEraNames;
		}

		internal String[] AbbrevEraNames(int calendarId)
		{
			Contract.Assert(calendarId > 0, "[CultureData.saAbbrevEraNames] Expected Calendar.ID > 0");

			return this.GetCalendar(calendarId).saAbbrevEraNames;
		}

		internal String[] AbbreviatedEnglishEraNames(int calendarId)
		{
			Contract.Assert(calendarId > 0, "[CultureData.saAbbrevEraNames] Expected Calendar.ID > 0");

			return this.GetCalendar(calendarId).saAbbrevEnglishEraNames;
		}

		// (user can override default only) short date format
		internal String[] ShortDates(int calendarId)
		{
			return GetCalendar(calendarId).saShortDates;
		}

		// (user can override default only) long date format
		internal String[] LongDates(int calendarId)
		{
			return GetCalendar(calendarId).saLongDates;
		}

		// (user can override) date year/month format.
		internal String[] YearMonths(int calendarId)
		{
			return GetCalendar(calendarId).saYearMonths;
		}

		// day names
		internal string[] DayNames(int calendarId)
		{
			return GetCalendar(calendarId).saDayNames;
		}

		// abbreviated day names
		internal string[] AbbreviatedDayNames(int calendarId)
		{
			// Get abbreviated day names for this calendar from the OS if necessary
			return GetCalendar(calendarId).saAbbrevDayNames;
		}

		// The super short day names
		internal string[] SuperShortDayNames(int calendarId)
		{
			return GetCalendar(calendarId).saSuperShortDayNames;
		}

		// month names
		internal string[] MonthNames(int calendarId)
		{
			return GetCalendar(calendarId).saMonthNames;
		}

		// Genitive month names
		internal string[] GenitiveMonthNames(int calendarId)
		{
			return GetCalendar(calendarId).saMonthGenitiveNames;
		}

		// month names
		internal string[] AbbreviatedMonthNames(int calendarId)
		{
			return GetCalendar(calendarId).saAbbrevMonthNames;
		}

		// Genitive month names
		internal string[] AbbreviatedGenitiveMonthNames(int calendarId)
		{
			return GetCalendar(calendarId).saAbbrevMonthGenitiveNames;
		}

		// Leap year month names
		// Note: This only applies to Hebrew, and it basically adds a "1" to the 6th month name
		// the non-leap names skip the 7th name in the normal month name array
		internal string[] LeapYearMonthNames(int calendarId)
		{
			return GetCalendar(calendarId).saLeapYearMonthNames;
		}

		// month/day format (single string, no override)
		internal String MonthDay(int calendarId)
		{
			return GetCalendar(calendarId).sMonthDay;
		}

		// Date separator (derived from short date format)
		internal String DateSeparator(int calendarId)
		{
			return GetDateSeparator(ShortDates(calendarId)[0]);
		}

		// NOTE: this method is used through reflection by System.Globalization.CultureXmlParser.ReadDateElement()
		// and breaking changes here will not show up at build time, only at run time.
		static private String GetDateSeparator(String format)
		{
			// Date format separator (ie: / in 9/1/03)
			//
			// We calculate this from the provided short date
			//

			//
			//  Find the date separator so that we can pretend we know SDATE.
			//
			return GetSeparator(format, "dyM");
		}

		private static string GetSeparator(string format, string timeParts)
		{
			int index = IndexOfTimePart(format, 0, timeParts);

			if (index != -1)
			{
				// Found a time part, find out when it changes
				char cTimePart = format[index];

				do
				{
					index++;
				} while (index < format.Length && format[index] == cTimePart);

				int separatorStart = index;

				// Now we need to find the end of the separator
				if (separatorStart < format.Length)
				{
					int separatorEnd = IndexOfTimePart(format, separatorStart, timeParts);
					if (separatorEnd != -1)
					{
						// From [separatorStart, count) is our string, except we need to unescape
						return UnescapeNlsString(format, separatorStart, separatorEnd - 1);
					}
				}
			}

			return String.Empty;
		}

		private static int IndexOfTimePart(string format, int startIndex, string timeParts)
		{
			Contract.Assert(startIndex >= 0, "startIndex cannot be negative");
			Contract.Assert(timeParts.IndexOfAny(new char[] { '\'', '\\' }) == -1, "timeParts cannot include quote characters");
			bool inQuote = false;
			for (int i = startIndex; i < format.Length; ++i)
			{
				// See if we have a time Part
				if (!inQuote && timeParts.IndexOf(format[i]) != -1)
				{
					return i;
				}
				switch (format[i])
				{
					case '\\':
						if (i + 1 < format.Length)
						{
							++i;
							switch (format[i])
							{
								case '\'':
								case '\\':
									break;
								default:
									--i; //backup since we will move over this next
									break;
							}
						}
						break;
					case '\'':
						inQuote = !inQuote;
						break;
				}
			}

			return -1;
		}

		////////////////////////////////////////////////////////////////////////////
		//
		// Unescape a NLS style quote string
		//
		// This removes single quotes:
		//      'fred' -> fred
		//      'fred -> fred
		//      fred' -> fred
		//      fred's -> freds
		//
		// This removes the first \ of escaped characters:
		//      fred\'s -> fred's
		//      a\\b -> a\b
		//      a\b -> ab
		//
		// We don't build the stringbuilder unless we find a ' or a \.  If we find a ' or a \, we
		// always build a stringbuilder because we need to remove the ' or \.
		//
		////////////////////////////////////////////////////////////////////////////
		static private String UnescapeNlsString(String str, int start, int end)
		{
			Contract.Requires(str != null);
			Contract.Requires(start >= 0);
			Contract.Requires(end >= 0);
			StringBuilder result = null;

			for (int i = start; i < str.Length && i <= end; i++)
			{
				switch (str[i])
				{
					case '\'':
						if (result == null)
						{
							result = new StringBuilder(str, start, i - start, str.Length);
						}
						break;
					case '\\':
						if (result == null)
						{
							result = new StringBuilder(str, start, i - start, str.Length);
						}
						++i;
						if (i < str.Length)
						{
							result.Append(str[i]);
						}
						break;
					default:
						if (result != null)
						{
							result.Append(str[i]);
						}
						break;
				}
			}

			if (result == null)
				return (str.Substring(start, end - start + 1));

			return (result.ToString());
		}

#endregion


		static internal String[] ReescapeWin32Strings(String[] array)
		{
			return array;
		}

		static internal String ReescapeWin32String(String str)
		{
			return str;
		}

		internal static bool IsCustomCultureId(int cultureId)
		{
			return false;
		}

		internal void GetNFIValues (NumberFormatInfo nfi)
		{
			if (this.IsInvariantCulture)
			{
				// Same as default values
			}
			else
			{
				//
				// We don't have information for the following four.  All cultures use
				// the same value of the number formatting values.
				//
				// PercentDecimalDigits
				// PercentDecimalSeparator
				// PercentGroupSize
				// PercentGroupSeparator
				//
				fill_number_data (nfi, numberIndex);
			}

			//
			// We don't have percent values, so use the number values
			//
			nfi.percentDecimalDigits = nfi.numberDecimalDigits;
			nfi.percentDecimalSeparator = nfi.numberDecimalSeparator;
			nfi.percentGroupSizes = nfi.numberGroupSizes;
			nfi.percentGroupSeparator = nfi.numberGroupSeparator;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void fill_number_data (NumberFormatInfo nfi, int numberIndex);
	}
}
