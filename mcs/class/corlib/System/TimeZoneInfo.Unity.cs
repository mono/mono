//
// System.TimeZoneInfo helper for Unity
//	because the devices cannot access the file system to read the data
//
// Authors:
//	Michael DeRoy <michaelde@unity3d.com>
//	Jonathan Chambers <jonathan@unity3d.com>
//
// Copyright 2018 Unity Technologies, Inc.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if UNITY

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace System {

	public partial class TimeZoneInfo {
		enum TimeZoneData
		{
			DaylightSavingFirstTransitionIdx,
			DaylightSavingSecondTransitionIdx,
			UtcOffsetIdx,
			AdditionalDaylightOffsetIdx
		};

		enum TimeZoneNames
		{
			StandardNameIdx,
			DaylightNameIdx
		};

		private static string GetTimeZoneDirectoryUnity()
		{
			return string.Empty;
		}

		static List<AdjustmentRule> CreateAdjustmentRule (int year, out Int64[] data, out string[] names, string standardNameCurrentYear, string daylightNameCurrentYear)
		{
			List<AdjustmentRule> rulesForYear = new List<AdjustmentRule> ();
			bool dst_inverted;
			if (!System.CurrentSystemTimeZone.GetTimeZoneData(year, out data, out names, out dst_inverted))
				return rulesForYear;
			var firstTransition = new DateTime (data[(int)TimeZoneData.DaylightSavingFirstTransitionIdx]);
			var secondTransition = new DateTime (data[(int)TimeZoneData.DaylightSavingSecondTransitionIdx]);
			var daylightOffset = new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]);

			/* C# TimeZoneInfo does not support timezones the same way as unix. In unix, timezone files are specified by region such as
			 * America/New_York or Asia/Singapore. If a region like Asia/Singapore changes it's timezone from +0730 to +08, the UTC offset
			 * has changed, but there is no support in the C# code to transition to this new UTC offset except for the case of daylight
			 * savings time. As such we'll only generate timezone rules for a region at the times associated with the timezone of the current year.
			 */
			if(standardNameCurrentYear != names[(int)TimeZoneNames.StandardNameIdx])
				return rulesForYear;
			if(daylightNameCurrentYear != names[(int)TimeZoneNames.DaylightNameIdx])
				return rulesForYear;

			// If the first and second transition DateTime objects are the same, ValidateAdjustmentRule will throw
			// an exception. I'm unsure why these would be the same, but we do see that occur for some locales.
			// In that case, just exit early.
			if (firstTransition.Equals(secondTransition))
				return rulesForYear;

			var beginningOfYear = new DateTime (year, 1, 1, 0, 0, 0, 0);
			var endOfYearDay = new DateTime (year, 12, DateTime.DaysInMonth (year, 12));
			var endOfYearMaxTimeout = new DateTime (year, 12, DateTime.DaysInMonth(year, 12), 23, 59, 59, 999);

			if (!dst_inverted) {
				// For daylight savings time that happens between jan and dec, create a rule from jan 1 to dec 31 (the entire year)

				// This rule (for the whole year) specifies the starting and ending months of daylight savings time.
				var startOfDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1).Add (firstTransition.TimeOfDay),
																					firstTransition.Month, firstTransition.Day);
				var endOfDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1).Add (secondTransition.TimeOfDay),
																					secondTransition.Month, secondTransition.Day);

				var fullYearRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (beginningOfYear,
																					endOfYearDay,
																					daylightOffset,
																					startOfDaylightSavingsTime,
																					endOfDaylightSavingsTime);
				rulesForYear.Add (fullYearRule);
			} else {
				// Some timezones (Australia/Sydney) have daylight savings over the new year.
				// Our icall returns the transitions for the current year, so we need two adjustment rules each year for this case

				// The first rule specifies daylight savings starting at jan 1 and ending at the first transition.
				var startOfFirstDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1), 1, 1);
				var endOfFirstDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1).Add (firstTransition.TimeOfDay),
																					firstTransition.Month, firstTransition.Day);

				var transitionOutOfDaylightSavingsRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (
																					new DateTime (year, 1, 1),
																					new DateTime (firstTransition.Year, firstTransition.Month, firstTransition.Day),
																					daylightOffset,
																					startOfFirstDaylightSavingsTime,
																					endOfFirstDaylightSavingsTime);
				rulesForYear.Add (transitionOutOfDaylightSavingsRule);

				// The second rule specifies daylight savings time starting the day after we transition out of daylight savings
				// and ending at the end of the year, with daylight savings starting near the end and ending on the last day of the year
				var startOfSecondDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1).Add (secondTransition.TimeOfDay),
																					secondTransition.Month, secondTransition.Day);
				var endOfSecondDaylightSavingsTime = TransitionTime.CreateFixedDateRule (new DateTime (1,1,1).Add (endOfYearMaxTimeout.TimeOfDay),
																					endOfYearMaxTimeout.Month, endOfYearMaxTimeout.Day);

				var transitionIntoDaylightSavingsRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (
																					new DateTime (firstTransition.Year, firstTransition.Month, firstTransition.Day).AddDays (1),
																					endOfYearDay,
																					daylightOffset,
																					startOfSecondDaylightSavingsTime,
																					endOfSecondDaylightSavingsTime);
				rulesForYear.Add (transitionIntoDaylightSavingsRule);
			}
			return rulesForYear;
		}

		static TimeZoneInfo CreateLocalUnity ()
		{
			Int64[] data;
			string[] names;
			//Some timezones start in DST on january first and disable it during the summer
			bool dst_inverted;
			int currentYear = DateTime.UtcNow.Year;
			if (!System.CurrentSystemTimeZone.GetTimeZoneData (currentYear, out data, out names, out dst_inverted))
				throw new NotSupportedException ("Can't get timezone name.");

			var utcOffsetTS = TimeSpan.FromTicks (data[(int)TimeZoneData.UtcOffsetIdx]);
			char utcOffsetSign = (utcOffsetTS >= TimeSpan.Zero) ? '+' : '-';
			string displayName = "(GMT" + utcOffsetSign + utcOffsetTS.ToString (@"hh\:mm") + ") Local Time";
			string standardDisplayName = names[(int)TimeZoneNames.StandardNameIdx];
			string daylightDisplayName = names[(int)TimeZoneNames.DaylightNameIdx];

			var adjustmentRulesList = new List<AdjustmentRule> ();
			bool disableDaylightSavings = data[(int)TimeZoneData.AdditionalDaylightOffsetIdx] == 0;
			//If the timezone supports daylight savings time, generate adjustment rules for the timezone
			if (!disableDaylightSavings) {
				//the icall only supports years from 1970 through 2037.
				int firstSupportedDate = 1971;
				int lastSupportedDate = 2037;

				//first, generate rules from the current year until the last year mktime is guaranteed to supports
				for (int year = currentYear; year <= lastSupportedDate; year++) {
					var rulesForCurrentYear = CreateAdjustmentRule (year, out data, out names, standardDisplayName, daylightDisplayName);
					//breakout if no more rules
					if (rulesForCurrentYear.Count > 0)
						adjustmentRulesList.AddRange (rulesForCurrentYear);
					else
						break;

				}

				for (int year = currentYear - 1; year >= firstSupportedDate; year--) {
					var rulesForCurrentYear = CreateAdjustmentRule (year, out data, out names, standardDisplayName, daylightDisplayName);
					//breakout if no more rules
					if (rulesForCurrentYear.Count > 0)
						adjustmentRulesList.AddRange (rulesForCurrentYear);
					else
						break;
				}

				adjustmentRulesList.Sort ( (rule1, rule2) => rule1.DateStart.CompareTo (rule2.DateStart) );
			}
			return TimeZoneInfo.CreateCustomTimeZone ("Local",
								utcOffsetTS,
								displayName,
								standardDisplayName,
								daylightDisplayName,
								adjustmentRulesList.ToArray (),
								disableDaylightSavings);
		}
	}

	partial class TimeZone
	{
		// Internal method to get timezone data.
		//    data[0]:  start of daylight saving time (in DateTime ticks).
		//    data[1]:  end of daylight saving time (in DateTime ticks).
		//    data[2]:  utcoffset (in TimeSpan ticks).
		//    data[3]:  additional offset when daylight saving (in TimeSpan ticks).
		//    name[0]:  name of this timezone when not daylight saving.
		//    name[1]:  name of this timezone when daylight saving.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern bool GetTimeZoneData (int year, out Int64[] data, out string[] names, out bool daylight_inverted);
	}
}

#endif
