// <copyright file="ConfigUtil.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Runtime.Caching.Resources;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace System.Runtime.Caching.Configuration {
    internal static class ConfigUtil {
        internal const string CacheMemoryLimitMegabytes = "cacheMemoryLimitMegabytes";
        internal const string PhysicalMemoryLimitPercentage = "physicalMemoryLimitPercentage";
        internal const string PollingInterval = "pollingInterval";
        internal const int DefaultPollingTimeMilliseconds = 120000;

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed) {
            string sValue = config[valueName];

            if (sValue == null) {
                return defaultValue;
            }

            int iValue;
            if (!Int32.TryParse(sValue, out iValue) 
                || iValue < 0 
                || (!zeroAllowed && iValue == 0)) {
                if (zeroAllowed) {
                    throw new ArgumentException(RH.Format(R.Value_must_be_non_negative_integer, valueName, sValue), "config");
                }

                throw new ArgumentException(RH.Format(R.Value_must_be_positive_integer, valueName, sValue), "config");
            }

            if (maxValueAllowed > 0 && iValue > maxValueAllowed) {
                throw new ArgumentException(RH.Format(R.Value_too_big, 
                                                      valueName, 
                                                      sValue, 
                                                      maxValueAllowed.ToString(CultureInfo.InvariantCulture)), "config");
            }

            return iValue;
        }

        internal static int GetIntValueFromTimeSpan(NameValueCollection config, string valueName, int defaultValue) {
            string sValue = config[valueName];

            if (sValue == null) {
                return defaultValue;
            }

            if (sValue == "Infinite") {
                return Int32.MaxValue;
            }

            TimeSpan tValue;
            if (!TimeSpan.TryParse(sValue, out tValue) || tValue <= TimeSpan.Zero) {
                throw new ArgumentException(RH.Format(R.TimeSpan_invalid_format, valueName, sValue), "config");
            }

            double milliseconds = tValue.TotalMilliseconds;
            int iValue = (milliseconds < (double)Int32.MaxValue) ? (int) milliseconds : Int32.MaxValue;
            return iValue;
        }

    }
}
