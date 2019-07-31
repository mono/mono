// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System 
{
    partial class TimeZoneInfo
    {
        //
        // TransitionTimeToDateTime -
        //
        // Helper function that converts a year and TransitionTime into a DateTime
        //
        static internal DateTime TransitionTimeToDateTime(Int32 year, TransitionTime transitionTime) {
            DateTime value;
            DateTime timeOfDay = transitionTime.TimeOfDay;

            if (transitionTime.IsFixedDateRule) {
                // create a DateTime from the passed in year and the properties on the transitionTime

                // if the day is out of range for the month then use the last day of the month
                Int32 day = DateTime.DaysInMonth(year, transitionTime.Month);

                value = new DateTime(year, transitionTime.Month, (day < transitionTime.Day) ? day : transitionTime.Day, 
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
            }
            else {
                if (transitionTime.Week <= 4) {
                    //
                    // Get the (transitionTime.Week)th Sunday.
                    //
                    value = new DateTime(year, transitionTime.Month, 1,
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);

                    int dayOfWeek = (int)value.DayOfWeek;
                    int delta = (int)transitionTime.DayOfWeek - dayOfWeek;
                    if (delta < 0) {
                        delta += 7;
                    }
                    delta += 7 * (transitionTime.Week - 1);

                    if (delta > 0) {
                        value = value.AddDays(delta);
                    }
                }
                else {
                    //
                    // If TransitionWeek is greater than 4, we will get the last week.
                    //
                    Int32 daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                    value = new DateTime(year, transitionTime.Month, daysInMonth,
                            timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);

                    // This is the day of week for the last day of the month.
                    int dayOfWeek = (int)value.DayOfWeek;
                    int delta = dayOfWeek - (int)transitionTime.DayOfWeek;
                    if (delta < 0) {
                        delta += 7;
                    }

                    if (delta > 0) {
                        value = value.AddDays(-delta);
                    }
                }
            }
            return value;
        }
    } // TimezoneInfo
} // namespace System
