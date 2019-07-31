// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// Inspired by various parts of CoreRT, most notably TimeZoneInfo.WinRT.cs.

#if WIN_PLATFORM

using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System
{
	partial class TimeZoneInfo
	{

		internal struct SYSTEMTIME
		{
			internal ushort wYear;
			internal ushort wMonth;
			internal ushort wDayOfWeek;
			internal ushort wDay;
			internal ushort wHour;
			internal ushort wMinute;
			internal ushort wSecond;
			internal ushort wMilliseconds;
		}

		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TIME_ZONE_INFORMATION
		{
			internal int Bias;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst=32)]
			internal string StandardName;
			internal SYSTEMTIME StandardDate;
			internal int StandardBias;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst=32)]
			internal string DaylightName;
			internal SYSTEMTIME DaylightDate;
			internal int DaylightBias;
		}

		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct DYNAMIC_TIME_ZONE_INFORMATION
		{
			internal TIME_ZONE_INFORMATION TZI;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			internal string TimeZoneKeyName;
			internal byte DynamicDaylightTimeDisabled;
		}

		internal const uint TIME_ZONE_ID_INVALID = 0xffffffff;
		internal const uint ERROR_NO_MORE_ITEMS = 259;

		[DllImport ("api-ms-win-core-timezone-l1-1-0.dll")]
		internal extern static uint EnumDynamicTimeZoneInformation (uint dwIndex, out DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation);
		[DllImport ("api-ms-win-core-timezone-l1-1-0.dll")]
		internal extern static uint GetDynamicTimeZoneInformation (out DYNAMIC_TIME_ZONE_INFORMATION pTimeZoneInformation);
		[DllImport ("api-ms-win-core-timezone-l1-1-0.dll")]
		internal extern static uint GetDynamicTimeZoneInformationEffectiveYears(ref DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation, out uint FirstYear, out uint LastYear);
		[DllImport ("api-ms-win-core-timezone-l1-1-0.dll")]
		internal extern static bool GetTimeZoneInformationForYear(ushort wYear, ref DYNAMIC_TIME_ZONE_INFORMATION pdtzi, out TIME_ZONE_INFORMATION ptzi);

		internal static AdjustmentRule CreateAdjustmentRuleFromTimeZoneInformation (ref DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation, DateTime startDate, DateTime endDate, int defaultBaseUtcOffset)
		{
			bool supportsDst = (timeZoneInformation.TZI.StandardDate.wMonth != 0);

			if (!supportsDst) {
				if (timeZoneInformation.TZI.Bias == defaultBaseUtcOffset) {
					// this rule will not contain any information to be used to adjust dates. just ignore it
					return null;
				}

				return AdjustmentRule.CreateAdjustmentRule (
					startDate,
					endDate,
					TimeSpan.Zero, // no daylight saving transition
					TransitionTime.CreateFixedDateRule (DateTime.MinValue, 1, 1),
					TransitionTime.CreateFixedDateRule (DateTime.MinValue.AddMilliseconds(1), 1, 1),
					new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.TZI.Bias, 0));  // Bias delta is all what we need from this rule
			}

			//
			// Create an AdjustmentRule with TransitionTime objects
			//
			TransitionTime daylightTransitionStart;
			if (!TransitionTimeFromTimeZoneInformation (timeZoneInformation, out daylightTransitionStart, true /* start date */)) {
				return null;
			}

			TransitionTime daylightTransitionEnd;
			if (!TransitionTimeFromTimeZoneInformation (timeZoneInformation, out daylightTransitionEnd, false /* end date */)) {
				return null;
			}

			if (daylightTransitionStart.Equals(daylightTransitionEnd)) {
				// this happens when the time zone does support DST but the OS has DST disabled
				return null;
			}

			return AdjustmentRule.CreateAdjustmentRule (
				startDate,
				endDate,
				new TimeSpan (0, -timeZoneInformation.TZI.DaylightBias, 0),
				(TransitionTime) daylightTransitionStart,
				(TransitionTime) daylightTransitionEnd,
				new TimeSpan (0, defaultBaseUtcOffset - timeZoneInformation.TZI.Bias, 0));
		}

		//
		// TransitionTimeFromTimeZoneInformation -
		//
		// Converts a TimeZoneInformation (REG_TZI_FORMAT struct) to a TransitionTime
		//
		// * when the argument 'readStart' is true the corresponding daylightTransitionTimeStart field is read
		// * when the argument 'readStart' is false the corresponding dayightTransitionTimeEnd field is read
		//
		private static bool TransitionTimeFromTimeZoneInformation (DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation, out TransitionTime transitionTime, bool readStartDate)
		{
			//
			// SYSTEMTIME - 
			//
			// If the time zone does not support daylight saving time or if the caller needs
			// to disable daylight saving time, the wMonth member in the SYSTEMTIME structure
			// must be zero. If this date is specified, the DaylightDate value in the 
			// TIME_ZONE_INFORMATION structure must also be specified. Otherwise, the system 
			// assumes the time zone data is invalid and no changes will be applied.
			//
			bool supportsDst = (timeZoneInformation.TZI.StandardDate.wMonth != 0);

			if (!supportsDst) {
				transitionTime = default (TransitionTime);
				return false;
			}

			//
			// SYSTEMTIME -
			//
			// * FixedDateRule -
			//   If the Year member is not zero, the transition date is absolute; it will only occur one time
			//
			// * FloatingDateRule -
			//   To select the correct day in the month, set the Year member to zero, the Hour and Minute 
			//   members to the transition time, the DayOfWeek member to the appropriate weekday, and the
			//   Day member to indicate the occurence of the day of the week within the month (first through fifth).
			//
			//   Using this notation, specify the 2:00a.m. on the first Sunday in April as follows: 
			//   Hour	  = 2, 
			//   Month	 = 4,
			//   DayOfWeek = 0,
			//   Day	   = 1.
			//
			//   Specify 2:00a.m. on the last Thursday in October as follows:
			//   Hour	  = 2,
			//   Month	 = 10,
			//   DayOfWeek = 4,
			//   Day	   = 5.
			//
			if (readStartDate) {
				//
				// read the "daylightTransitionStart"
				//
				if (timeZoneInformation.TZI.DaylightDate.wYear == 0) {
					transitionTime = TransitionTime.CreateFloatingDateRule (
									 new DateTime (1,	/* year  */
												   1,	/* month */
												   1,	/* day   */
												   timeZoneInformation.TZI.DaylightDate.wHour,
												   timeZoneInformation.TZI.DaylightDate.wMinute,
												   timeZoneInformation.TZI.DaylightDate.wSecond,
												   timeZoneInformation.TZI.DaylightDate.wMilliseconds),
									 timeZoneInformation.TZI.DaylightDate.wMonth,
									 timeZoneInformation.TZI.DaylightDate.wDay,   /* Week 1-5 */
									 (DayOfWeek)timeZoneInformation.TZI.DaylightDate.wDayOfWeek);
				} else {
					transitionTime = TransitionTime.CreateFixedDateRule (
									 new DateTime (1,	/* year  */
												   1,	/* month */
												   1,	/* day   */
												   timeZoneInformation.TZI.DaylightDate.wHour,
												   timeZoneInformation.TZI.DaylightDate.wMinute,
												   timeZoneInformation.TZI.DaylightDate.wSecond,
												   timeZoneInformation.TZI.DaylightDate.wMilliseconds),
									 timeZoneInformation.TZI.DaylightDate.wMonth,
									 timeZoneInformation.TZI.DaylightDate.wDay);
				}
			} else {
				//
				// read the "daylightTransitionEnd"
				//
				if (timeZoneInformation.TZI.StandardDate.wYear == 0) {
					transitionTime = TransitionTime.CreateFloatingDateRule (
									 new DateTime (1,	/* year  */
												   1,	/* month */
												   1,	/* day   */
												   timeZoneInformation.TZI.StandardDate.wHour,
												   timeZoneInformation.TZI.StandardDate.wMinute,
												   timeZoneInformation.TZI.StandardDate.wSecond,
												   timeZoneInformation.TZI.StandardDate.wMilliseconds),
									 timeZoneInformation.TZI.StandardDate.wMonth,
									 timeZoneInformation.TZI.StandardDate.wDay,   /* Week 1-5 */
									 (DayOfWeek)timeZoneInformation.TZI.StandardDate.wDayOfWeek);
				} else {
					transitionTime = TransitionTime.CreateFixedDateRule (
									 new DateTime (1,	/* year  */
												   1,	/* month */
												   1,	/* day   */
												   timeZoneInformation.TZI.StandardDate.wHour,
												   timeZoneInformation.TZI.StandardDate.wMinute,
												   timeZoneInformation.TZI.StandardDate.wSecond,
												   timeZoneInformation.TZI.StandardDate.wMilliseconds),
									 timeZoneInformation.TZI.StandardDate.wMonth,
									 timeZoneInformation.TZI.StandardDate.wDay);
				}
			}

			return true;
		}

		internal static TimeZoneInfo TryCreateTimeZone (DYNAMIC_TIME_ZONE_INFORMATION timeZoneInformation)
		{
			uint firstYear = 0, lastYear = 0;
			AdjustmentRule rule;
			AdjustmentRule[] zoneRules = null;
			int defaultBaseUtcOffset = timeZoneInformation.TZI.Bias;

			if (String.IsNullOrEmpty (timeZoneInformation.TimeZoneKeyName))
				return null;

			//
			// First get the adjustment rules
			//

			try {
				if (GetDynamicTimeZoneInformationEffectiveYears (ref timeZoneInformation, out firstYear, out lastYear) != 0) {
					firstYear = lastYear = 0;
				}
			} catch {
				// If we don't have GetDynamicTimeZoneInformationEffectiveYears()
				firstYear = lastYear = 0;
			}

			if (firstYear == lastYear) {
				rule = CreateAdjustmentRuleFromTimeZoneInformation (ref timeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date, defaultBaseUtcOffset);
				if (rule != null)
					zoneRules = new AdjustmentRule [1] { rule };
			} else {
				DYNAMIC_TIME_ZONE_INFORMATION dtzi = default (DYNAMIC_TIME_ZONE_INFORMATION);
				List<AdjustmentRule> rules = new List<AdjustmentRule> ();
				//
				// First rule
				//

				if (!GetTimeZoneInformationForYear ((ushort) firstYear, ref timeZoneInformation, out dtzi.TZI))
					return null;
				rule = CreateAdjustmentRuleFromTimeZoneInformation (ref dtzi, DateTime.MinValue.Date, new DateTime ((int) firstYear, 12, 31), defaultBaseUtcOffset);
				if (rule != null)
					rules.Add (rule);

				for (uint i = firstYear + 1; i < lastYear; i++) {
					if (!GetTimeZoneInformationForYear ((ushort) i, ref timeZoneInformation, out dtzi.TZI))
						return null;
					rule = CreateAdjustmentRuleFromTimeZoneInformation (ref dtzi, new DateTime ((int) i, 1, 1), new DateTime ((int) i, 12, 31), defaultBaseUtcOffset);
					if (rule != null)
						rules.Add (rule);
				}

				//
				// Last rule
				//

				if (!GetTimeZoneInformationForYear ((ushort) lastYear, ref timeZoneInformation, out dtzi.TZI))
					return null;
				rule = CreateAdjustmentRuleFromTimeZoneInformation (ref dtzi, new DateTime ((int) lastYear, 1, 1), DateTime.MaxValue.Date, defaultBaseUtcOffset);
				if (rule != null)
					rules.Add (rule);

				if (rules.Count > 0)
					zoneRules = rules.ToArray ();
			}

			return new TimeZoneInfo (
				timeZoneInformation.TimeZoneKeyName,
				new TimeSpan (0, -(timeZoneInformation.TZI.Bias), 0),
				timeZoneInformation.TZI.StandardName,   // we use the display name as the standared names
				timeZoneInformation.TZI.StandardName,
				timeZoneInformation.TZI.DaylightName,
				zoneRules,
				false);
		}

		internal static TimeZoneInfo GetLocalTimeZoneInfoWinRTFallback ()
		{
			try {
				DYNAMIC_TIME_ZONE_INFORMATION dtzi;
				var result = GetDynamicTimeZoneInformation (out dtzi);
				if (result == TIME_ZONE_ID_INVALID)
					return Utc;
				TimeZoneInfo timeZoneInfo = TryCreateTimeZone (dtzi);
				return timeZoneInfo != null ? timeZoneInfo : Utc;
			} catch {
				return Utc;
			}
		}

		internal static TimeZoneInfo FindSystemTimeZoneByIdWinRTFallback (string id)
		{
			foreach (var tzi in GetSystemTimeZones ()) {
				if (String.Compare (id, tzi.Id, StringComparison.Ordinal) == 0)
					return tzi;
			}

			throw new TimeZoneNotFoundException ();
		}

		internal static List<TimeZoneInfo> GetSystemTimeZonesWinRTFallback ()
		{
			var result = new List<TimeZoneInfo> ();
			try {
				uint index = 0;
				DYNAMIC_TIME_ZONE_INFORMATION dtzi;
				while (EnumDynamicTimeZoneInformation (index++, out dtzi) != ERROR_NO_MORE_ITEMS) {
					var timeZoneInfo = TryCreateTimeZone (dtzi);
					if (timeZoneInfo != null)
						result.Add (timeZoneInfo);
				}
			} catch {
				// EnumDynamicTimeZoneInformation() might not be available.
			}

			if (result.Count == 0) {
				result.Add (Local);
				result.Add (Utc);
			}

			result.Sort ((x, y) =>
			{
				int comparison = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
				return comparison == 0 ? string.CompareOrdinal(x.DisplayName, y.DisplayName) : comparison;
			});

			return result;
		}
	}
}

#endif // !FULL_AOT_DESKTOP || WIN_PLATFORM
