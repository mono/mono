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

namespace System {

	public partial class TimeZoneInfo {
		enum TimeZoneData
		{
			DaylightSavingStartIdx,
			DaylightSavingEndIdx,
			UtcOffsetIdx,
			AdditionalDaylightOffsetIdx
		};
		
		enum TimeZoneNames
		{
			StandardNameIdx,
			DaylightNameIdx
		};

		static AdjustmentRule CreateAdjustmentRule(int year, out Int64[] data, out string[] names, string standardNameCurrentYear, string daylightNameCurrentYear)
		{
			if(!System.CurrentSystemTimeZone.GetTimeZoneData(year, out data, out names))
				return null;
			var startTime = new DateTime (data[(int)TimeZoneData.DaylightSavingStartIdx]);
			var endTime = new DateTime (data[(int)TimeZoneData.DaylightSavingEndIdx]);
			var daylightOffset = new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]);

			/* C# TimeZoneInfo does not support timezones the same way as unix. In unix, timezone files are specified by region such as
			 * America/New_York or Asia/Singapore. If a region like Asia/Singapore changes it's timezone from +0730 to +08, the UTC offset
			 * has changed, but there is no support in the C# code to transition to this new UTC offset except for the case of daylight
			 * savings time. As such we'll only generate timezone rules for a region at the times associated with the timezone of the current year.
			 */
			if(standardNameCurrentYear != names[(int)TimeZoneNames.StandardNameIdx])
				return null;
			if(daylightNameCurrentYear != names[(int)TimeZoneNames.DaylightNameIdx])
				return null;

			var dlsTransitionStart = TransitionTime.CreateFixedDateRule(new DateTime(1,1,1).Add(startTime.TimeOfDay),
																				startTime.Month, startTime.Day);
			var dlsTransitionEnd = TransitionTime.CreateFixedDateRule(new DateTime(1,1,1).Add(endTime.TimeOfDay),
																				endTime.Month, endTime.Day);

			var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(year, 1, 1),
																				new DateTime(year, 12, DateTime.DaysInMonth(year, 12)),
																				daylightOffset,
																				dlsTransitionStart,
																				dlsTransitionEnd);
			return rule;
		}

		static TimeZoneInfo CreateLocalUnity ()
		{
			Int64[] data;
			string[] names;
			int currentYear = DateTime.UtcNow.Year;
			if (!System.CurrentSystemTimeZone.GetTimeZoneData (currentYear, out data, out names))
				throw new NotSupportedException ("Can't get timezone name.");

			var utcOffsetTS = TimeSpan.FromTicks(data[(int)TimeZoneData.UtcOffsetIdx]);
			char utcOffsetSign = (utcOffsetTS >= TimeSpan.Zero) ? '+' : '-';
			string displayName = "(GMT" + utcOffsetSign + utcOffsetTS.ToString(@"hh\:mm") + ") Local Time";
			string standardDisplayName = names[(int)TimeZoneNames.StandardNameIdx];
			string daylightDisplayName = names[(int)TimeZoneNames.DaylightNameIdx];

			var adjustmentList = new List<AdjustmentRule>();
			bool disableDaylightSavings = data[(int)TimeZoneData.AdditionalDaylightOffsetIdx] <= 0;
			//If the timezone supports daylight savings time, generate adjustment rules for the timezone
			if(!disableDaylightSavings)
			{
				//the icall only supports years from 1970 through 2037.
				int firstSupportedDate = 1971;
				int lastSupportedDate = 2037;

				//first, generate rules from the current year until the last year mktime is guaranteed to supports
				for(int year = currentYear; year <= lastSupportedDate; year++)
				{
					var rule = CreateAdjustmentRule(year, out data, out names, standardDisplayName, daylightDisplayName);
					//breakout if timezone changes, or fails
					if(rule == null)
						break;
					adjustmentList.Add(rule);
				}

				for(int year = currentYear - 1; year >= firstSupportedDate; year--)
				{
					var rule = CreateAdjustmentRule(year, out data, out names, standardDisplayName, daylightDisplayName);
					//breakout if timezone changes, or fails
					if(rule == null)
						break;
					adjustmentList.Add(rule);
				}

				adjustmentList.Sort( (rule1, rule2) => rule1.DateStart.CompareTo(rule2.DateStart) );
			}
			return TimeZoneInfo.CreateCustomTimeZone("Local",
								utcOffsetTS,
								displayName,
								standardDisplayName,
								daylightDisplayName,
								adjustmentList.ToArray(),
								disableDaylightSavings);
		}
	}
}

#endif
