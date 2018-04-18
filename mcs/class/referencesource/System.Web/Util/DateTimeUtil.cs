//------------------------------------------------------------------------------
// <copyright file="FileChangesMonitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {

    using System;

    internal enum TimeUnit {
        Unknown         = 0,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    };

    internal sealed class DateTimeUtil {
        private DateTimeUtil() {}
        const long FileTimeOffset = 504911232000000000;
        static readonly DateTime    MinValuePlusOneDay = DateTime.MinValue.AddDays(1);
        static readonly DateTime    MaxValueMinusOneDay = DateTime.MaxValue.AddDays(-1);

        static internal DateTime FromFileTimeToUtc(long filetime) {
            long universalTicks = filetime + FileTimeOffset;
            // Dev10 733288: Caching: behavior change for CacheDependency when using UseMemoryCache=1
            // ObjectCacheHost converts DateTime to a DateTimeOffset, and the conversion requires
            // that DateTimeKind be set correctly
            return new DateTime(universalTicks, DateTimeKind.Utc);
        }

        static internal DateTime ConvertToUniversalTime(DateTime localTime) {
            if (localTime < MinValuePlusOneDay) {
                return DateTime.MinValue;
            }

            if (localTime > MaxValueMinusOneDay) {
                return DateTime.MaxValue;
            }

            return localTime.ToUniversalTime();
        }

        static internal DateTime ConvertToLocalTime(DateTime utcTime) {
            if (utcTime < MinValuePlusOneDay) {
                return DateTime.MinValue;
            }

            if (utcTime > MaxValueMinusOneDay) {
                return DateTime.MaxValue;
            }

            return utcTime.ToLocalTime();
        }

        static internal TimeSpan GetTimeoutFromTimeUnit(int timeoutValue, TimeUnit timeoutUnit) {
            switch (timeoutUnit) {
                case TimeUnit.Days:
                    return new TimeSpan(timeoutValue, 0, 0, 0);
                case TimeUnit.Hours:
                    return new TimeSpan(timeoutValue, 0, 0);
                case TimeUnit.Seconds:
                    return new TimeSpan(0, 0, timeoutValue);
                case TimeUnit.Milliseconds:
                    return new TimeSpan(0, 0, 0, 0, timeoutValue);
                case TimeUnit.Minutes:
                    return new TimeSpan(0, timeoutValue, 0);
                case TimeUnit.Unknown:
                default:
                    break;
            }

            throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "timeoutUnit"));
        }
    }
}


