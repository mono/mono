//
// System.TimeZoneInfo helper for MonoTouch
// 	because the devices cannot access the file system to read the data
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2013 Xamarin Inc.
//
// The class can be either constructed from a string (from user code)
// or from a handle (from iphone-sharp.dll internal calls).  This
// delays the creation of the actual managed string until actually
// required
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
		static TimeZoneInfo CreateLocalUnity ()
		{
			Int64[] data;
		    string[] names;
		    if (!System.CurrentSystemTimeZone.GetTimeZoneData (1973, out data, out names))
				throw new NotSupportedException ("Can't get timezone name.");

		    TimeSpan utcOffsetTS = TimeSpan.FromTicks(data[(int)TimeZoneData.UtcOffsetIdx]);
		    char utcOffsetSign = (utcOffsetTS >= TimeSpan.Zero) ? '+' : '-';
		    string displayName = "(GMT" + utcOffsetSign + utcOffsetTS.ToString(@"hh\:mm") + ") Local Time";
		    string standardDisplayName = names[(int)TimeZoneNames.StandardNameIdx];
		    string daylightDisplayName = names[(int)TimeZoneNames.DaylightNameIdx];
		    
		    //Create The Adjustment Rules For This TimeZoneInfo.
		    var adjustmentList = new List<TimeZoneInfo.AdjustmentRule>();
		    for(int year = 1973; year <= 2037; year++)
		    {	    
				if (!System.CurrentSystemTimeZone.GetTimeZoneData (year, out data, out names))
					continue;
				
				DaylightTime dlt = new DaylightTime (new DateTime (data[(int)TimeZoneData.DaylightSavingStartIdx]),
								     new DateTime (data[(int)TimeZoneData.DaylightSavingEndIdx]),
								     new TimeSpan (data[(int)TimeZoneData.AdditionalDaylightOffsetIdx]));
				
				DateTime dltStartTime = new DateTime(1, 1, 1).Add(dlt.Start.TimeOfDay);
				DateTime dltEndTime = new DateTime(1, 1, 1).Add(dlt.End.TimeOfDay);

				if (dlt.Start == dlt.End)
					continue;

				TimeZoneInfo.TransitionTime startTime = TimeZoneInfo.TransitionTime.CreateFixedDateRule(dltStartTime, dlt.Start.Month, dlt.Start.Day);
				TimeZoneInfo.TransitionTime endTime = TimeZoneInfo.TransitionTime.CreateFixedDateRule(dltEndTime, dlt.End.Month, dlt.End.Day);
				

				//mktime only supports dates starting in 1973, so create an adjustment rule for years before 1973 following 1973s rules 
				if (year == 1973)
				{
				    TimeZoneInfo.AdjustmentRule firstRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue,
															     new DateTime(1969, 12, 31),
															     dlt.Delta,
															     startTime,
															     endTime);
				    adjustmentList.Add(firstRule);
				}
				
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(year, 1, 1),
														    new DateTime(year, 12, 31),
														    dlt.Delta,
														    startTime,
														    endTime);
				adjustmentList.Add(rule);
				
				//mktime only supports dates up to 2037, so create an adjustment rule for years after 2037 following 2037s rules 
				if (year == 2037)
				{
					// create a max date that does not include any time of day offset to make CreateAdjustmentRule happy
					var maxDate = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day);
				    TimeZoneInfo.AdjustmentRule lastRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2038, 1, 1),
				    											maxDate,
															    dlt.Delta,
															    startTime,
															    endTime);
				    adjustmentList.Add(lastRule);
				}
		    }
		    
		    return TimeZoneInfo.CreateCustomTimeZone("local",
									   utcOffsetTS,
									   displayName,
									   standardDisplayName,
									   daylightDisplayName,
									   adjustmentList.ToArray(),
									   false);
		}
	}
}

#endif
