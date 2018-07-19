//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace System.IdentityModel
{
    static class DateTimeUtil
    {
        /// <summary>
        /// Add a DateTime and a TimeSpan.
        /// The maximum time is DateTime.MaxTime.  It is not an error if time + timespan > MaxTime.
        /// Just return MaxTime.
        /// </summary>
        /// <param name="time">Initial <see cref="DateTime"/> value.</param>
        /// <param name="timespan"><see cref="TimeSpan"/> to add.</param>
        /// <returns></returns>
        public static DateTime Add(DateTime time, TimeSpan timespan)
        {
            if (timespan >= TimeSpan.Zero && DateTime.MaxValue - time <= timespan)
            {
                return GetMaxValue(time.Kind);
            }

            if (timespan <= TimeSpan.Zero && DateTime.MinValue - time >= timespan)
            {
                return GetMinValue(time.Kind);
            }

            return time + timespan;
        }

        /// <summary>
        /// Add a DateTime and a non-negative TimeSpan.
        /// The maximum time is DateTime.MaxTime.  It is not an error if time + timespan > MaxTime.
        /// Just return MaxTime. If TimeSpan is &lt; TimeSpan.Zero, throw exception.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        public static DateTime AddNonNegative(DateTime time, TimeSpan timespan)
        {
            if (timespan <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2082)));
            }
            return Add(time, timespan);
        }

        public static DateTime GetMaxValue(DateTimeKind kind)
        {
            return new DateTime(DateTime.MaxValue.Ticks, kind);
        }

        public static DateTime GetMinValue(DateTimeKind kind)
        {
            return new DateTime(DateTime.MinValue.Ticks, kind);
        }

        public static DateTime? ToUniversalTime(DateTime? value)
        {
            if (null == value || value.Value.Kind == DateTimeKind.Utc)
            {
                return value;
            }
            return ToUniversalTime(value.Value);
        }

        public static DateTime ToUniversalTime(DateTime value)
        {

            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }

            return value.ToUniversalTime();
        }
    }
}
