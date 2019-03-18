// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: TimeZoneInfo
**
**
** Purpose: 
** This class is used to represent a Dynamic TimeZone.  It
** has methods for converting a DateTime between TimeZones,
** and for reading TimeZone data from the Windows Registry
**
**
============================================================*/

namespace System {
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    //
    // DateTime uses TimeZoneInfo under the hood for IsDaylightSavingTime, IsAmbiguousTime, and GetUtcOffset.
    // These TimeZoneInfo APIs can throw ArgumentException when an Invalid-Time is passed in.  To avoid this
    // unwanted behavior in DateTime public APIs, DateTime internally passes the
    // TimeZoneInfoOptions.NoThrowOnInvalidTime flag to internal TimeZoneInfo APIs.
    //
    // In the future we can consider exposing similar options on the public TimeZoneInfo APIs if there is enough
    // demand for this alternate behavior.
    //
    [Flags]
    internal enum TimeZoneInfoOptions {
        None                      = 1,
        NoThrowOnInvalidTime      = 2
    };


    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#if MOBILE
    [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#else
    [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#endif
    sealed public partial class TimeZoneInfo : IEquatable<TimeZoneInfo>, ISerializable, IDeserializationCallback {
#if !MONO
        // ---- SECTION:  members supporting exposed properties -------------*
        private String m_id;
        private String m_displayName;
        private String m_standardDisplayName;
        private String m_daylightDisplayName;
        private TimeSpan m_baseUtcOffset;
        private Boolean m_supportsDaylightSavingTime;
        private AdjustmentRule[] m_adjustmentRules;

        // ---- SECTION:  members for internal support ---------*
        private enum TimeZoneInfoResult {
            Success                   = 0,
            TimeZoneNotFoundException = 1,
            InvalidTimeZoneException  = 2,
            SecurityException         = 3
        };


#if FEATURE_WIN32_REGISTRY
        // registry constants for the 'Time Zones' hive
        //
        private const string c_timeZonesRegistryHive = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones";
        private const string c_timeZonesRegistryHivePermissionList = @"HKEY_LOCAL_MACHINE\" + c_timeZonesRegistryHive;
        private const string c_displayValue = "Display";
        private const string c_daylightValue = "Dlt";
        private const string c_standardValue = "Std";
        private const string c_muiDisplayValue = "MUI_Display";
        private const string c_muiDaylightValue = "MUI_Dlt";
        private const string c_muiStandardValue = "MUI_Std";
        private const string c_timeZoneInfoValue = "TZI";
        private const string c_firstEntryValue = "FirstEntry";
        private const string c_lastEntryValue = "LastEntry";

#endif // FEATURE_WIN32_REGISTRY

        // constants for TimeZoneInfo.Local and TimeZoneInfo.Utc
        private const string c_utcId = "UTC";
        private const string c_localId = "Local";

        private const int    c_maxKeyLength = 255;

        private const int    c_regByteLength = 44;

        // Number of 100ns ticks per time unit
        private const long c_ticksPerMillisecond = 10000;
        private const long c_ticksPerSecond      = c_ticksPerMillisecond * 1000;
        private const long c_ticksPerMinute      = c_ticksPerSecond * 60;
        private const long c_ticksPerHour        = c_ticksPerMinute * 60;
        private const long c_ticksPerDay         = c_ticksPerHour * 24;
        private const long c_ticksPerDayRange    = c_ticksPerDay - c_ticksPerMillisecond;

        //
        // All cached data are encapsulated in a helper class to allow consistent view even when the data are refreshed using ClearCachedData()
        //
        // For example, TimeZoneInfo.Local can be cleared by another thread calling TimeZoneInfo.ClearCachedData. Without the consistent snapshot, 
        // there is a chance that the internal ConvertTime calls will throw since 'source' won't be reference equal to the new TimeZoneInfo.Local.
        //
#pragma warning disable 0420
        class CachedData
        {
            private volatile TimeZoneInfo m_localTimeZone;
            private volatile TimeZoneInfo m_utcTimeZone;

            private TimeZoneInfo CreateLocal()
            {
                lock (this)
                {
                    TimeZoneInfo timeZone = m_localTimeZone;
                    if (timeZone == null) {
                        timeZone = TimeZoneInfo.GetLocalTimeZone(this);

                        // this step is to break the reference equality
                        // between TimeZoneInfo.Local and a second time zone
                        // such as "Pacific Standard Time"
                        timeZone = new TimeZoneInfo(
                                            timeZone.m_id,
                                            timeZone.m_baseUtcOffset,
                                            timeZone.m_displayName,
                                            timeZone.m_standardDisplayName,
                                            timeZone.m_daylightDisplayName,
                                            timeZone.m_adjustmentRules,
                                            false);

                        m_localTimeZone = timeZone;
                    }
                    return timeZone;
                }
            }

            public TimeZoneInfo Local {
                get {
                    TimeZoneInfo timeZone = m_localTimeZone;
                    if (timeZone == null) {
                        timeZone = CreateLocal();
                    }
                    return timeZone;
                }
            }

            private TimeZoneInfo CreateUtc()
            {
                lock (this)
                {
                    TimeZoneInfo timeZone = m_utcTimeZone;
                    if (timeZone == null) {
                        timeZone = CreateCustomTimeZone(c_utcId, TimeSpan.Zero, c_utcId, c_utcId);
                        m_utcTimeZone = timeZone;
                    }
                    return timeZone;
                }
            }

            public TimeZoneInfo Utc {
                get {
                    Contract.Ensures(Contract.Result<TimeZoneInfo>() != null);

                    TimeZoneInfo timeZone = m_utcTimeZone;
                    if (timeZone == null) {
                        timeZone = CreateUtc();
                    }
                    return timeZone;
                }
            }     

            //
            // GetCorrespondingKind-
            //
            // Helper function that returns the corresponding DateTimeKind for this TimeZoneInfo
            //
            public DateTimeKind GetCorrespondingKind(TimeZoneInfo timeZone) {
                DateTimeKind kind;

                //
                // we check reference equality to see if 'this' is the same as
                // TimeZoneInfo.Local or TimeZoneInfo.Utc.  This check is needed to 
                // support setting the DateTime Kind property to 'Local' or
                // 'Utc' on the ConverTime(...) return value.  
                //
                // Using reference equality instead of value equality was a 
                // performance based design compromise.  The reference equality
                // has much greater performance, but it reduces the number of
                // returned DateTime's that can be properly set as 'Local' or 'Utc'.
                //
                // For example, the user could be converting to the TimeZoneInfo returned
                // by FindSystemTimeZoneById("Pacific Standard Time") and their local
                // machine may be in Pacific time.  If we used value equality to determine
                // the corresponding Kind then this conversion would be tagged as 'Local';
                // where as we are currently tagging the returned DateTime as 'Unspecified'
                // in this example.  Only when the user passes in TimeZoneInfo.Local or
                // TimeZoneInfo.Utc to the ConvertTime(...) methods will this check succeed.
                //
                if ((object)timeZone == (object)m_utcTimeZone) {
                    kind = DateTimeKind.Utc;
                }
                else if ((object)timeZone == (object)m_localTimeZone) {
                    kind = DateTimeKind.Local;
                }
                else {
                    kind = DateTimeKind.Unspecified;
                }

                return kind;
            }

#if FEATURE_WIN32_REGISTRY
            public Dictionary<string, TimeZoneInfo> m_systemTimeZones;
            public ReadOnlyCollection<TimeZoneInfo> m_readOnlySystemTimeZones;
            public bool m_allSystemTimeZonesRead;

            [System.Security.SecuritySafeCritical]
            private static TimeZoneInfo GetCurrentOneYearLocal() {
                // load the data from the OS
                TimeZoneInfo match;

                Win32Native.TimeZoneInformation timeZoneInformation = new Win32Native.TimeZoneInformation();
                long result = UnsafeNativeMethods.GetTimeZoneInformation(out timeZoneInformation);
                if (result == Win32Native.TIME_ZONE_ID_INVALID)
                    match = CreateCustomTimeZone(c_localId, TimeSpan.Zero, c_localId, c_localId);
                else
                    match = GetLocalTimeZoneFromWin32Data(timeZoneInformation, false);               
                return match;
            }

            private volatile OffsetAndRule m_oneYearLocalFromUtc;
         
            public OffsetAndRule GetOneYearLocalFromUtc(int year) {
                OffsetAndRule oneYearLocFromUtc = m_oneYearLocalFromUtc;
                if (oneYearLocFromUtc == null || oneYearLocFromUtc.year != year) {
                    TimeZoneInfo currentYear = GetCurrentOneYearLocal();
                    AdjustmentRule rule = currentYear.m_adjustmentRules == null ? null : currentYear.m_adjustmentRules[0];
                    oneYearLocFromUtc = new OffsetAndRule(year, currentYear.BaseUtcOffset, rule);
                    m_oneYearLocalFromUtc = oneYearLocFromUtc;
                }
                return oneYearLocFromUtc;
            }

#endif // FEATURE_WIN32_REGISTRY
        };
#pragma warning restore 0420

        static CachedData s_cachedData = new CachedData();

        private class OffsetAndRule {
            public int year;
            public TimeSpan offset;
            public AdjustmentRule rule;
            public OffsetAndRule(int year, TimeSpan offset, AdjustmentRule rule) {
                this.year = year;
                this.offset = offset;
                this.rule = rule;
            }
        }

        // used by GetUtcOffsetFromUtc (DateTime.Now, DateTime.ToLocalTime) for max/min whole-day range checks
        private static DateTime s_maxDateOnly = new DateTime(9999, 12, 31);
        private static DateTime s_minDateOnly = new DateTime(1, 1, 2);

        // ---- SECTION: public properties --------------*

        public String Id {
            get { 
                return m_id;
            }
        }    

        public String DisplayName {
            get {
                return (m_displayName == null ? String.Empty : m_displayName);
            }
        }

        public String StandardName {
            get { 
                return (m_standardDisplayName == null ? String.Empty : m_standardDisplayName);
            }
        }

        public String DaylightName {
            get { 
                return (m_daylightDisplayName == null? String.Empty : m_daylightDisplayName);
            }
        }

        public TimeSpan BaseUtcOffset {
            get { 
                return m_baseUtcOffset;
            }
        }

        public Boolean SupportsDaylightSavingTime {
            get { 
                return m_supportsDaylightSavingTime;
            }
        }


        // ---- SECTION: public methods --------------*

        //
        // GetAdjustmentRules -
        //
        // returns a cloned array of AdjustmentRule objects
        //
        public AdjustmentRule [] GetAdjustmentRules() {
            if (m_adjustmentRules == null) {
                return new AdjustmentRule[0];
            }
            else {
                return (AdjustmentRule[])m_adjustmentRules.Clone();
            }
        }


        //
        // GetAmbiguousTimeOffsets -
        //
        // returns an array of TimeSpan objects representing all of
        // possible UTC offset values for this ambiguous time
        //
        public TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset) {
            if (!SupportsDaylightSavingTime) {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeOffsetIsNotAmbiguous"), "dateTimeOffset");
            }
            Contract.EndContractBlock();

            DateTime adjustedTime = (TimeZoneInfo.ConvertTime(dateTimeOffset, this)).DateTime;

            Boolean isAmbiguous = false;
            AdjustmentRule rule = GetAdjustmentRuleForTime(adjustedTime);
            if (rule != null && rule.HasDaylightSaving) {
                DaylightTime daylightTime = GetDaylightTime(adjustedTime.Year, rule);
                isAmbiguous = GetIsAmbiguousTime(adjustedTime, rule, daylightTime);
            }

            if (!isAmbiguous) {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeOffsetIsNotAmbiguous"), "dateTimeOffset");
            }

            // the passed in dateTime is ambiguous in this TimeZoneInfo instance
            TimeSpan[] timeSpans = new TimeSpan[2];

            TimeSpan actualUtcOffset = m_baseUtcOffset + rule.BaseUtcOffsetDelta;

            // the TimeSpan array must be sorted from least to greatest
            if (rule.DaylightDelta > TimeSpan.Zero) {
                timeSpans[0] = actualUtcOffset; // FUTURE:  + rule.StandardDelta;
                timeSpans[1] = actualUtcOffset + rule.DaylightDelta;
            } 
            else {
                timeSpans[0] = actualUtcOffset + rule.DaylightDelta;
                timeSpans[1] = actualUtcOffset; // FUTURE: + rule.StandardDelta;
            }
            return timeSpans;
        }


        public TimeSpan[] GetAmbiguousTimeOffsets(DateTime dateTime) {
            if (!SupportsDaylightSavingTime) {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsNotAmbiguous"), "dateTime");
            }
            Contract.EndContractBlock();

            DateTime adjustedTime;
            if (dateTime.Kind == DateTimeKind.Local) {
                CachedData cachedData = s_cachedData;
                adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, this, TimeZoneInfoOptions.None, cachedData);
            }
            else if (dateTime.Kind == DateTimeKind.Utc) {
                CachedData cachedData = s_cachedData;
                adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Utc, this, TimeZoneInfoOptions.None, cachedData);
            }
            else {
                adjustedTime = dateTime;
            }

            Boolean isAmbiguous = false;
            AdjustmentRule rule = GetAdjustmentRuleForTime(adjustedTime);
            if (rule != null && rule.HasDaylightSaving) {
                DaylightTime daylightTime = GetDaylightTime(adjustedTime.Year, rule);
                isAmbiguous = GetIsAmbiguousTime(adjustedTime, rule, daylightTime);
            }

            if (!isAmbiguous) {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsNotAmbiguous"), "dateTime");
            }

            // the passed in dateTime is ambiguous in this TimeZoneInfo instance
            TimeSpan[] timeSpans = new TimeSpan[2];
            TimeSpan actualUtcOffset = m_baseUtcOffset + rule.BaseUtcOffsetDelta;
 
            // the TimeSpan array must be sorted from least to greatest
            if (rule.DaylightDelta > TimeSpan.Zero) {
                timeSpans[0] = actualUtcOffset; // FUTURE:  + rule.StandardDelta;
                timeSpans[1] = actualUtcOffset + rule.DaylightDelta;
            } 
            else {
                timeSpans[0] = actualUtcOffset + rule.DaylightDelta;
                timeSpans[1] = actualUtcOffset; // FUTURE: + rule.StandardDelta;
            }
            return timeSpans;
        }

        //
        // GetUtcOffset -
        //
        // returns the Universal Coordinated Time (UTC) Offset
        // for the current TimeZoneInfo instance.
        //
        public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset) {
            return GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this);
        }


        public TimeSpan GetUtcOffset(DateTime dateTime) {
            return GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime, s_cachedData);
        }

        // Shortcut for TimeZoneInfo.Local.GetUtcOffset
        internal static TimeSpan GetLocalUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags) {
            CachedData cachedData = s_cachedData;
            return cachedData.Local.GetUtcOffset(dateTime, flags, cachedData);
        }

        internal TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags) {
            return GetUtcOffset(dateTime, flags, s_cachedData);
        }

        private TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags, CachedData cachedData) {
            if (dateTime.Kind == DateTimeKind.Local) {
                if (cachedData.GetCorrespondingKind(this) != DateTimeKind.Local) {
                    //
                    // normal case of converting from Local to Utc and then getting the offset from the UTC DateTime
                    //
                    DateTime adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, cachedData.Utc, flags);
                    return GetUtcOffsetFromUtc(adjustedTime, this);
                }

                //
                // Fall through for TimeZoneInfo.Local.GetUtcOffset(date)
                // to handle an edge case with Invalid-Times for DateTime formatting:
                //
                // Consider the invalid PST time "2007-03-11T02:00:00.0000000-08:00"
                //
                // By directly calling GetUtcOffset instead of converting to UTC and then calling GetUtcOffsetFromUtc
                // the correct invalid offset of "-08:00" is returned.  In the normal case of converting to UTC as an 
                // interim-step, the invalid time is adjusted into a *valid* UTC time which causes a change in output:
                //
                // 1) invalid PST time "2007-03-11T02:00:00.0000000-08:00"
                // 2) converted to UTC "2007-03-11T10:00:00.0000000Z"
                // 3) offset returned  "2007-03-11T03:00:00.0000000-07:00"
                //
            }
            else if (dateTime.Kind == DateTimeKind.Utc) {
                if (cachedData.GetCorrespondingKind(this) == DateTimeKind.Utc) {
                    return m_baseUtcOffset;
                }
                else {
                    //
                    // passing in a UTC dateTime to a non-UTC TimeZoneInfo instance is a
                    // special Loss-Less case.
                    //
                    return GetUtcOffsetFromUtc(dateTime, this);
                }
            }

            return GetUtcOffset(dateTime, this, flags);
        }

        //
        // IsAmbiguousTime -
        //
        // returns true if the time is during the ambiguous time period
        // for the current TimeZoneInfo instance.
        //
        public Boolean IsAmbiguousTime(DateTimeOffset dateTimeOffset) {
            if (!m_supportsDaylightSavingTime) {
                return false;
            }

            DateTimeOffset adjustedTime = TimeZoneInfo.ConvertTime(dateTimeOffset, this);
            return IsAmbiguousTime(adjustedTime.DateTime);
        }


        public Boolean IsAmbiguousTime(DateTime dateTime) {
            return IsAmbiguousTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        internal Boolean IsAmbiguousTime(DateTime dateTime, TimeZoneInfoOptions flags) {
            if (!m_supportsDaylightSavingTime) {
                return false;
            }

            DateTime adjustedTime;
            if (dateTime.Kind == DateTimeKind.Local) {
                CachedData cachedData = s_cachedData;
                adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, this, flags, cachedData); 
            }
            else if (dateTime.Kind == DateTimeKind.Utc) {
                CachedData cachedData = s_cachedData;
                adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Utc, this, flags, cachedData);
            }
            else {
                adjustedTime = dateTime;
            }

            AdjustmentRule rule = GetAdjustmentRuleForTime(adjustedTime);
            if (rule != null && rule.HasDaylightSaving) {
                DaylightTime daylightTime = GetDaylightTime(adjustedTime.Year, rule);
                return GetIsAmbiguousTime(adjustedTime, rule, daylightTime);
            }
            return false;
        }



        //
        // IsDaylightSavingTime -
        //
        // Returns true if the time is during Daylight Saving time
        // for the current TimeZoneInfo instance.
        //
        public Boolean IsDaylightSavingTime(DateTimeOffset dateTimeOffset) {
            Boolean isDaylightSavingTime;
            GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this, out isDaylightSavingTime);
            return isDaylightSavingTime;
        }


        public Boolean IsDaylightSavingTime(DateTime dateTime) {
            return IsDaylightSavingTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime, s_cachedData);
        }

        internal Boolean IsDaylightSavingTime(DateTime dateTime, TimeZoneInfoOptions flags) {
            return IsDaylightSavingTime(dateTime, flags, s_cachedData);
        }

        private Boolean IsDaylightSavingTime(DateTime dateTime, TimeZoneInfoOptions flags, CachedData cachedData) {
            //
            //    dateTime.Kind is UTC, then time will be converted from UTC
            //        into current instance's timezone
            //    dateTime.Kind is Local, then time will be converted from Local 
            //        into current instance's timezone
            //    dateTime.Kind is UnSpecified, then time is already in
            //        current instance's timezone
            //
            // Our DateTime handles ambiguous times, (one is in the daylight and
            // one is in standard.) If a new DateTime is constructed during ambiguous 
            // time, it is defaulted to "Standard" (i.e. this will return false).
            // For Invalid times, we will return false

            if (!m_supportsDaylightSavingTime || m_adjustmentRules == null) {
                return false;
            }

            DateTime adjustedTime;
            //
            // handle any Local/Utc special cases...
            //
            if (dateTime.Kind == DateTimeKind.Local) {
                adjustedTime = TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, this, flags, cachedData); 
           }
            else if (dateTime.Kind == DateTimeKind.Utc) {
                if (cachedData.GetCorrespondingKind(this) == DateTimeKind.Utc) {
                    // simple always false case: TimeZoneInfo.Utc.IsDaylightSavingTime(dateTime, flags);
                    return false;
                }
                else {
                    //
                    // passing in a UTC dateTime to a non-UTC TimeZoneInfo instance is a
                    // special Loss-Less case.
                    //
                    Boolean isDaylightSavings;
                    GetUtcOffsetFromUtc(dateTime, this, out isDaylightSavings);
                    return isDaylightSavings;
                }
            }
            else {
                adjustedTime = dateTime;
            }

            //
            // handle the normal cases...
            //
            AdjustmentRule rule = GetAdjustmentRuleForTime(adjustedTime);
            if (rule != null && rule.HasDaylightSaving) {
                DaylightTime daylightTime = GetDaylightTime(adjustedTime.Year, rule);
                return GetIsDaylightSavings(adjustedTime, rule, daylightTime, flags);
            }
            else {
                return false;
            }
        }


        //
        // IsInvalidTime -
        //
        // returns true when dateTime falls into a "hole in time".
        //
        public Boolean IsInvalidTime(DateTime dateTime) {
            Boolean isInvalid = false;
          
            if ( (dateTime.Kind == DateTimeKind.Unspecified)
            ||   (dateTime.Kind == DateTimeKind.Local && s_cachedData.GetCorrespondingKind(this) == DateTimeKind.Local) ) {

                // only check Unspecified and (Local when this TimeZoneInfo instance is Local)
                AdjustmentRule rule = GetAdjustmentRuleForTime(dateTime);

                if (rule != null && rule.HasDaylightSaving) {
                    DaylightTime daylightTime = GetDaylightTime(dateTime.Year, rule);
                    isInvalid = GetIsInvalidTime(dateTime, rule, daylightTime);
                }
                else {
                    isInvalid = false;
                }
            }

            return isInvalid;
        }


        //
        // ClearCachedData -
        //
        // Clears data from static members
        //
        static public void ClearCachedData() {
            // Clear a fresh instance of cached data
            s_cachedData = new CachedData();
        }

#if FEATURE_WIN32_REGISTRY
        //
        // ConvertTimeBySystemTimeZoneId -
        //
        // Converts the value of a DateTime object from sourceTimeZone to destinationTimeZone
        //
        static public DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, String destinationTimeZoneId) {
            return ConvertTime(dateTimeOffset, FindSystemTimeZoneById(destinationTimeZoneId));
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        static public DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, String destinationTimeZoneId) {
            return ConvertTime(dateTime, FindSystemTimeZoneById(destinationTimeZoneId));
        }
#endif // FEATURE_WIN32_REGISTRY

#if FEATURE_WIN32_REGISTRY
        static public DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, String sourceTimeZoneId, String destinationTimeZoneId) {
            if (dateTime.Kind == DateTimeKind.Local && String.Compare(sourceTimeZoneId, TimeZoneInfo.Local.Id, StringComparison.OrdinalIgnoreCase) == 0) {
                // TimeZoneInfo.Local can be cleared by another thread calling TimeZoneInfo.ClearCachedData.
                // Take snapshot of cached data to guarantee this method will not be impacted by the ClearCachedData call.
                // Without the snapshot, there is a chance that ConvertTime will throw since 'source' won't
                // be reference equal to the new TimeZoneInfo.Local
                //
                CachedData cachedData = s_cachedData;
                return ConvertTime(dateTime, cachedData.Local, FindSystemTimeZoneById(destinationTimeZoneId), TimeZoneInfoOptions.None, cachedData);
            }    
            else if (dateTime.Kind == DateTimeKind.Utc && String.Compare(sourceTimeZoneId, TimeZoneInfo.Utc.Id, StringComparison.OrdinalIgnoreCase) == 0) {
                // TimeZoneInfo.Utc can be cleared by another thread calling TimeZoneInfo.ClearCachedData.
                // Take snapshot of cached data to guarantee this method will not be impacted by the ClearCachedData call.
                // Without the snapshot, there is a chance that ConvertTime will throw since 'source' won't
                // be reference equal to the new TimeZoneInfo.Utc
                //
                CachedData cachedData = s_cachedData;
                return ConvertTime(dateTime, cachedData.Utc, FindSystemTimeZoneById(destinationTimeZoneId), TimeZoneInfoOptions.None, cachedData);
            }
            else {
                return ConvertTime(dateTime, FindSystemTimeZoneById(sourceTimeZoneId), FindSystemTimeZoneById(destinationTimeZoneId));
            }
        }
#endif // FEATURE_WIN32_REGISTRY


        //
        // ConvertTime -
        //
        // Converts the value of the dateTime object from sourceTimeZone to destinationTimeZone
        //

        static public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone) {
            if (destinationTimeZone == null) {
                throw new ArgumentNullException("destinationTimeZone");
            }

            Contract.EndContractBlock();
            // calculate the destination time zone offset
            DateTime utcDateTime = dateTimeOffset.UtcDateTime;
            TimeSpan destinationOffset = GetUtcOffsetFromUtc(utcDateTime, destinationTimeZone);

            // check for overflow
            Int64 ticks = utcDateTime.Ticks + destinationOffset.Ticks;

            if (ticks > DateTimeOffset.MaxValue.Ticks) {
                return DateTimeOffset.MaxValue;
            }
            else if (ticks < DateTimeOffset.MinValue.Ticks) {
                return DateTimeOffset.MinValue;
            }
            else {
                return new DateTimeOffset(ticks, destinationOffset);
            }
        }

        static public DateTime ConvertTime(DateTime dateTime, TimeZoneInfo destinationTimeZone) {
            if (destinationTimeZone == null) {
                throw new ArgumentNullException("destinationTimeZone");
            }
            Contract.EndContractBlock();

            // Special case to give a way clearing the cache without exposing ClearCachedData()
            if (dateTime.Ticks == 0) {
                ClearCachedData();
            }
            CachedData cachedData = s_cachedData;
            if (dateTime.Kind == DateTimeKind.Utc) {
                return ConvertTime(dateTime, cachedData.Utc, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
            }
            else {
                return ConvertTime(dateTime, cachedData.Local, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
            }
        }

        static public DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone) {
            return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, TimeZoneInfoOptions.None, s_cachedData);
        }


        static internal DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags) {
            return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, flags, s_cachedData);
        }

        static private DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags, CachedData cachedData) {
            if (sourceTimeZone == null) {
                throw new ArgumentNullException("sourceTimeZone");
            }

            if (destinationTimeZone == null) {
                throw new ArgumentNullException("destinationTimeZone");
            }
            Contract.EndContractBlock();

            DateTimeKind sourceKind = cachedData.GetCorrespondingKind(sourceTimeZone);
            if ( ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0) && (dateTime.Kind != DateTimeKind.Unspecified) && (dateTime.Kind != sourceKind) ) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ConvertMismatch"), "sourceTimeZone");
            }

            //
            // check to see if the DateTime is in an invalid time range.  This check
            // requires the current AdjustmentRule and DaylightTime - which are also
            // needed to calculate 'sourceOffset' in the normal conversion case.
            // By calculating the 'sourceOffset' here we improve the
            // performance for the normal case at the expense of the 'ArgumentException'
            // case and Loss-less Local special cases.
            //
            AdjustmentRule sourceRule = sourceTimeZone.GetAdjustmentRuleForTime(dateTime);
            TimeSpan sourceOffset = sourceTimeZone.BaseUtcOffset;

            if (sourceRule != null) {
                sourceOffset = sourceOffset + sourceRule.BaseUtcOffsetDelta;
                if (sourceRule.HasDaylightSaving) {
                    Boolean sourceIsDaylightSavings = false;
                    DaylightTime sourceDaylightTime = GetDaylightTime(dateTime.Year, sourceRule);

                    // 'dateTime' might be in an invalid time range since it is in an AdjustmentRule
                    // period that supports DST 
                    if (((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0) && GetIsInvalidTime(dateTime, sourceRule, sourceDaylightTime)) {
                        throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsInvalid"), "dateTime");
                    }
                    sourceIsDaylightSavings = GetIsDaylightSavings(dateTime, sourceRule, sourceDaylightTime, flags);

                    // adjust the sourceOffset according to the Adjustment Rule / Daylight Saving Rule
                    sourceOffset += (sourceIsDaylightSavings ? sourceRule.DaylightDelta : TimeSpan.Zero /*FUTURE: sourceRule.StandardDelta*/);
                }
            }

            DateTimeKind targetKind = cachedData.GetCorrespondingKind(destinationTimeZone);

            // handle the special case of Loss-less Local->Local and UTC->UTC)
            if (dateTime.Kind != DateTimeKind.Unspecified && sourceKind != DateTimeKind.Unspecified
                && sourceKind == targetKind) {
                return dateTime;
            }

            Int64 utcTicks = dateTime.Ticks - sourceOffset.Ticks;

            // handle the normal case by converting from 'source' to UTC and then to 'target'
            Boolean isAmbiguousLocalDst = false;
            DateTime targetConverted = ConvertUtcToTimeZone(utcTicks, destinationTimeZone, out isAmbiguousLocalDst);
            
            if (targetKind == DateTimeKind.Local) {
                // Because the ticks conversion between UTC and local is lossy, we need to capture whether the 
                // time is in a repeated hour so that it can be passed to the DateTime constructor.
                return new DateTime(targetConverted.Ticks, DateTimeKind.Local, isAmbiguousLocalDst); 
            }
            else {
                return new DateTime(targetConverted.Ticks, targetKind);
            }
        }



        //
        // ConvertTimeFromUtc -
        //
        // Converts the value of a DateTime object from Coordinated Universal Time (UTC) to
        // the destinationTimeZone.
        //
        static public DateTime ConvertTimeFromUtc(DateTime dateTime, TimeZoneInfo destinationTimeZone) {
            CachedData cachedData = s_cachedData;
            return ConvertTime(dateTime, cachedData.Utc, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
        }


        //
        // ConvertTimeToUtc -
        //
        // Converts the value of a DateTime object to Coordinated Universal Time (UTC).
        //
        static public DateTime ConvertTimeToUtc(DateTime dateTime) {
            if (dateTime.Kind == DateTimeKind.Utc) {
                return dateTime;
            }
            CachedData cachedData = s_cachedData;
            return ConvertTime(dateTime, cachedData.Local, cachedData.Utc, TimeZoneInfoOptions.None, cachedData);
        }


        static internal DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfoOptions flags) {
            if (dateTime.Kind == DateTimeKind.Utc) {
                return dateTime;
            }
            CachedData cachedData = s_cachedData;
            return ConvertTime(dateTime, cachedData.Local, cachedData.Utc, flags, cachedData);
        }

        static public DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfo sourceTimeZone) {
            CachedData cachedData = s_cachedData;
            return ConvertTime(dateTime, sourceTimeZone, cachedData.Utc, TimeZoneInfoOptions.None, cachedData);
        }


        //
        // IEquatable.Equals -
        //
        // returns value equality.  Equals does not compare any localizable
        // String objects (DisplayName, StandardName, DaylightName).
        //
        public bool Equals(TimeZoneInfo other) {
            return (other != null && String.Compare(this.m_id, other.m_id, StringComparison.OrdinalIgnoreCase) == 0 && HasSameRules(other));
        }

        public override bool Equals(object obj) {
            TimeZoneInfo tzi = obj as TimeZoneInfo;            
            if (null == tzi) {
                return false;
            }            
            return Equals(tzi);
        }

        //    
        // FromSerializedString -
        //
        static public TimeZoneInfo FromSerializedString(string source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (source.Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSerializedString", source), "source");
            }
            Contract.EndContractBlock();

            return StringSerializer.GetDeserializedTimeZoneInfo(source);
        }


        //
        // GetHashCode -
        //
        public override int GetHashCode() {
            return m_id.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
        }


#if FEATURE_WIN32_REGISTRY
        //
        // GetSystemTimeZones -
        //
        // returns a ReadOnlyCollection<TimeZoneInfo> containing all valid TimeZone's
        // from the local machine.  The entries in the collection are sorted by
        // 'DisplayName'.
        //
        // This method does *not* throw TimeZoneNotFoundException or
        // InvalidTimeZoneException.
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <Asserts Name="Imperative: System.Security.PermissionSet" />
        // </SecurityKernel>
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        static public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones() {

            CachedData cachedData = s_cachedData;

            lock (cachedData) {
                if (cachedData.m_readOnlySystemTimeZones == null) {
                    PermissionSet permSet = new PermissionSet(PermissionState.None);
                    permSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, c_timeZonesRegistryHivePermissionList));
                    permSet.Assert();
               
                    using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(
                                        c_timeZonesRegistryHive,
#if FEATURE_MACL
                                        RegistryKeyPermissionCheck.Default,
                                        System.Security.AccessControl.RegistryRights.ReadKey
#else
                                        false
#endif
                                        )) {

                        if (reg != null) {
                            foreach (string keyName in reg.GetSubKeyNames()) {
                                TimeZoneInfo value;
                                Exception ex;
                                TryGetTimeZone(keyName, false, out value, out ex, cachedData);  // populate the cache
                            }
                        }
                        cachedData.m_allSystemTimeZonesRead = true;
                    }

                    List<TimeZoneInfo> list;
                    if (cachedData.m_systemTimeZones != null) {
                        // return a collection of the cached system time zones
                        list = new List<TimeZoneInfo>(cachedData.m_systemTimeZones.Values);
                    }
                    else {
                        // return an empty collection
                        list = new List<TimeZoneInfo>();
                    }

                    // sort and copy the TimeZoneInfo's into a ReadOnlyCollection for the user
                    list.Sort(new TimeZoneInfoComparer());

                    cachedData.m_readOnlySystemTimeZones = new ReadOnlyCollection<TimeZoneInfo>(list);
                }          
            }
            return cachedData.m_readOnlySystemTimeZones;
        }
#endif // FEATURE_WIN32_REGISTRY


        //
        // HasSameRules -
        //
        // Value equality on the "adjustmentRules" array
        //
        public Boolean HasSameRules(TimeZoneInfo other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }

            // check the utcOffset and supportsDaylightSavingTime members
            Contract.EndContractBlock();

            if (this.m_baseUtcOffset != other.m_baseUtcOffset 
            || this.m_supportsDaylightSavingTime != other.m_supportsDaylightSavingTime) {
                return false;
            }

            bool sameRules;
            AdjustmentRule[] currentRules = this.m_adjustmentRules;
            AdjustmentRule[] otherRules = other.m_adjustmentRules;

            sameRules = (currentRules == null && otherRules == null)
                      ||(currentRules != null && otherRules != null);

            if (!sameRules) {
                // AdjustmentRule array mismatch
                return false;
            }

            if (currentRules != null) {
                if (currentRules.Length != otherRules.Length) {
                    // AdjustmentRule array length mismatch
                    return false;
                }

                for(int i = 0; i < currentRules.Length; i++) {
                    if (!(currentRules[i]).Equals(otherRules[i])) {
                        // AdjustmentRule value-equality mismatch
                        return false;
                    }
                }

            }
            return sameRules;          
        }

        //
        // Local -
        //
        // returns a TimeZoneInfo instance that represents the local time on the machine.
        // Accessing this property may throw InvalidTimeZoneException or COMException
        // if the machine is in an unstable or corrupt state.
        //
        static public TimeZoneInfo Local {
            get {
                Contract.Ensures(Contract.Result<TimeZoneInfo>() != null);
                return s_cachedData.Local;
            }
        }


        //
        // ToSerializedString -
        //
        // "TimeZoneInfo"           := TimeZoneInfo Data;[AdjustmentRule Data 1];...;[AdjustmentRule Data N]
        //
        // "TimeZoneInfo Data"      := <m_id>;<m_baseUtcOffset>;<m_displayName>;
        //                          <m_standardDisplayName>;<m_daylightDispayName>;
        //
        // "AdjustmentRule Data" := <DateStart>;<DateEnd>;<DaylightDelta>;
        //                          [TransitionTime Data DST Start]
        //                          [TransitionTime Data DST End]
        //
        // "TransitionTime Data" += <DaylightStartTimeOfDat>;<Month>;<Week>;<DayOfWeek>;<Day>
        //
        public String ToSerializedString() {
            return StringSerializer.GetSerializedString(this);
        }


        //
        // ToString -
        //
        // returns the DisplayName: 
        // "(GMT-08:00) Pacific Time (US & Canada); Tijuana"
        //
        public override string ToString() {
            return this.DisplayName;
        }


        //
        // Utc -
        //
        // returns a TimeZoneInfo instance that represents Universal Coordinated Time (UTC)
        //
        static public TimeZoneInfo Utc {
            get {
                Contract.Ensures(Contract.Result<TimeZoneInfo>() != null);
                return s_cachedData.Utc;
            }
        }     


        // -------- SECTION: constructors -----------------*
        // 
        // TimeZoneInfo -
        //
        // private ctor
        //
        [System.Security.SecurityCritical]  // auto-generated
        private TimeZoneInfo(Win32Native.TimeZoneInformation zone, Boolean dstDisabled) {
            
            if (String.IsNullOrEmpty(zone.StandardName)) {
                m_id = c_localId;  // the ID must contain at least 1 character - initialize m_id to "Local"
            }
            else {
                m_id = zone.StandardName;
            }
            m_baseUtcOffset = new TimeSpan(0, -(zone.Bias), 0);

            if (!dstDisabled) {
                // only create the adjustment rule if DST is enabled
                Win32Native.RegistryTimeZoneInformation regZone = new Win32Native.RegistryTimeZoneInformation(zone);
                AdjustmentRule rule = CreateAdjustmentRuleFromTimeZoneInformation(regZone, DateTime.MinValue.Date, DateTime.MaxValue.Date, zone.Bias);
                if (rule != null) {
                    m_adjustmentRules = new AdjustmentRule[1];
                    m_adjustmentRules[0] = rule;
                }
            }

            ValidateTimeZoneInfo(m_id, m_baseUtcOffset, m_adjustmentRules, out m_supportsDaylightSavingTime);
            m_displayName = zone.StandardName;
            m_standardDisplayName = zone.StandardName;
            m_daylightDisplayName = zone.DaylightName;
        }
        
        private TimeZoneInfo(
                String id,
                TimeSpan baseUtcOffset,
                String displayName,
                String standardDisplayName,
                String daylightDisplayName,
                AdjustmentRule [] adjustmentRules,
                Boolean disableDaylightSavingTime) {

            Boolean adjustmentRulesSupportDst;
            ValidateTimeZoneInfo(id, baseUtcOffset, adjustmentRules, out adjustmentRulesSupportDst);

            if (!disableDaylightSavingTime && adjustmentRules != null && adjustmentRules.Length > 0) {
                m_adjustmentRules = (AdjustmentRule[])adjustmentRules.Clone();
            }

            m_id = id;
            m_baseUtcOffset = baseUtcOffset;
            m_displayName = displayName;
            m_standardDisplayName = standardDisplayName;
            m_daylightDisplayName = (disableDaylightSavingTime ? null : daylightDisplayName);
            m_supportsDaylightSavingTime = adjustmentRulesSupportDst && !disableDaylightSavingTime;
        }

        // -------- SECTION: factory methods -----------------*
 
        //
        // CreateCustomTimeZone -
        // 
        // returns a simple TimeZoneInfo instance that does
        // not support Daylight Saving Time
        //
        static public TimeZoneInfo CreateCustomTimeZone(
                String id,
                TimeSpan baseUtcOffset,
                String displayName,
                  String standardDisplayName) {

            return new TimeZoneInfo(
                           id,
                           baseUtcOffset,
                           displayName,
                           standardDisplayName,
                           standardDisplayName,
                           null,
                           false);
        }

        //
        // CreateCustomTimeZone -
        // 
        // returns a TimeZoneInfo instance that may
        // support Daylight Saving Time
        //
        static public TimeZoneInfo CreateCustomTimeZone(
                String id,
                TimeSpan baseUtcOffset,
                String displayName,
                String standardDisplayName,
                String daylightDisplayName,
                AdjustmentRule [] adjustmentRules) {

            return new TimeZoneInfo(
                           id,
                           baseUtcOffset,
                           displayName,
                           standardDisplayName,
                           daylightDisplayName,
                           adjustmentRules,
                           false);
        }


        //
        // CreateCustomTimeZone -
        // 
        // returns a TimeZoneInfo instance that may
        // support Daylight Saving Time
        //
        // This class factory method is identical to the
        // TimeZoneInfo private constructor
        //
        static public TimeZoneInfo CreateCustomTimeZone(
                String id,
                TimeSpan baseUtcOffset,
                String displayName,
                String standardDisplayName,
                String daylightDisplayName,
                AdjustmentRule [] adjustmentRules,
                Boolean disableDaylightSavingTime) {

           return new TimeZoneInfo(
                           id,
                           baseUtcOffset,
                           displayName,
                           standardDisplayName,
                           daylightDisplayName,
                           adjustmentRules,
                           disableDaylightSavingTime);
        }



        // ----- SECTION: private serialization instance methods  ----------------*

#if FEATURE_SERIALIZATION
        void IDeserializationCallback.OnDeserialization(Object sender) {
            try {
                Boolean adjustmentRulesSupportDst;
                ValidateTimeZoneInfo(m_id, m_baseUtcOffset, m_adjustmentRules, out adjustmentRulesSupportDst);

                if (adjustmentRulesSupportDst != m_supportsDaylightSavingTime) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_CorruptField", "SupportsDaylightSavingTime"));
                }
            }
            catch (ArgumentException e) {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
            }
            catch (InvalidTimeZoneException e) {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
            }
        }


        [System.Security.SecurityCritical]  // auto-generated_required
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();

            info.AddValue("Id", m_id);
            info.AddValue("DisplayName", m_displayName);
            info.AddValue("StandardName", m_standardDisplayName);
            info.AddValue("DaylightName", m_daylightDisplayName);
            info.AddValue("BaseUtcOffset", m_baseUtcOffset);
            info.AddValue("AdjustmentRules", m_adjustmentRules);
            info.AddValue("SupportsDaylightSavingTime", m_supportsDaylightSavingTime);
        }

        
        TimeZoneInfo(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            m_id                  = (String)info.GetValue("Id", typeof(String));
            m_displayName         = (String)info.GetValue("DisplayName", typeof(String));
            m_standardDisplayName = (String)info.GetValue("StandardName", typeof(String));
            m_daylightDisplayName = (String)info.GetValue("DaylightName", typeof(String));
            m_baseUtcOffset       = (TimeSpan)info.GetValue("BaseUtcOffset", typeof(TimeSpan));
            m_adjustmentRules     = (AdjustmentRule[])info.GetValue("AdjustmentRules", typeof(AdjustmentRule[]));
            m_supportsDaylightSavingTime = (Boolean)info.GetValue("SupportsDaylightSavingTime", typeof(Boolean));
        }
#endif



        // ----- SECTION: internal instance utility methods ----------------*


        // assumes dateTime is in the current time zone's time
        private AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime) {
            if (m_adjustmentRules == null || m_adjustmentRules.Length == 0) {
                return null;
            }

            // Only check the whole-date portion of the dateTime -
            // This is because the AdjustmentRule DateStart & DateEnd are stored as
            // Date-only values {4/2/2006 - 10/28/2006} but actually represent the
            // time span {4/2/2006@00:00:00.00000 - 10/28/2006@23:59:59.99999}
            DateTime date = dateTime.Date;

            for (int i = 0; i < m_adjustmentRules.Length; i++) {
                if (m_adjustmentRules[i].DateStart <= date && m_adjustmentRules[i].DateEnd >= date) {
                    return m_adjustmentRules[i];
                }
            }

            return null;
        }



        // ----- SECTION: internal static utility methods ----------------*

        //
        // CheckDaylightSavingTimeNotSupported -
        //
        // Helper function to check if the current TimeZoneInformation struct does not support DST.  This
        // check returns true when the DaylightDate == StandardDate
        //
        // This check is only meant to be used for "Local".
        //
        [System.Security.SecurityCritical]  // auto-generated
        static private Boolean CheckDaylightSavingTimeNotSupported(Win32Native.TimeZoneInformation timeZone) {
            return (   timeZone.DaylightDate.Year         == timeZone.StandardDate.Year
                    && timeZone.DaylightDate.Month        == timeZone.StandardDate.Month
                    && timeZone.DaylightDate.DayOfWeek    == timeZone.StandardDate.DayOfWeek
                    && timeZone.DaylightDate.Day          == timeZone.StandardDate.Day
                    && timeZone.DaylightDate.Hour         == timeZone.StandardDate.Hour
                    && timeZone.DaylightDate.Minute       == timeZone.StandardDate.Minute
                    && timeZone.DaylightDate.Second       == timeZone.StandardDate.Second
                    && timeZone.DaylightDate.Milliseconds == timeZone.StandardDate.Milliseconds);
        }


        //
        // ConvertUtcToTimeZone -
        //
        // Helper function that converts a dateTime from UTC into the destinationTimeZone
        //
        // * returns DateTime.MaxValue when the converted value is too large
        // * returns DateTime.MinValue when the converted value is too small
        //
        static private DateTime ConvertUtcToTimeZone(Int64 ticks, TimeZoneInfo destinationTimeZone, out Boolean isAmbiguousLocalDst) {
            DateTime utcConverted;
            DateTime localConverted;

            // utcConverted is used to calculate the UTC offset in the destinationTimeZone
            if (ticks > DateTime.MaxValue.Ticks) {
                utcConverted = DateTime.MaxValue;
            }
            else if (ticks < DateTime.MinValue.Ticks) {
                utcConverted = DateTime.MinValue;
            }
            else {
                utcConverted = new DateTime(ticks);
            }

            // verify the time is between MinValue and MaxValue in the new time zone
            TimeSpan offset = GetUtcOffsetFromUtc(utcConverted, destinationTimeZone, out isAmbiguousLocalDst);
            ticks += offset.Ticks;

            if (ticks > DateTime.MaxValue.Ticks) {
                localConverted = DateTime.MaxValue;
            }
            else if (ticks < DateTime.MinValue.Ticks) {
                localConverted = DateTime.MinValue;
            }
            else {
                localConverted = new DateTime(ticks);
            }
            return localConverted;           
        }


        //
        // CreateAdjustmentRuleFromTimeZoneInformation-
        //
        // Converts a Win32Native.RegistryTimeZoneInformation (REG_TZI_FORMAT struct) to an AdjustmentRule
        //
        [System.Security.SecurityCritical]  // auto-generated
        static private AdjustmentRule CreateAdjustmentRuleFromTimeZoneInformation(Win32Native.RegistryTimeZoneInformation timeZoneInformation, DateTime startDate, DateTime endDate, int defaultBaseUtcOffset) {
            AdjustmentRule rule;
            bool supportsDst = (timeZoneInformation.StandardDate.Month != 0);

            if (!supportsDst) {
                if (timeZoneInformation.Bias == defaultBaseUtcOffset)
                {
                    // this rule will not contain any information to be used to adjust dates. just ignore it
                    return null;
                }

                return rule = AdjustmentRule.CreateAdjustmentRule(
                    startDate,
                    endDate,
                    TimeSpan.Zero, // no daylight saving transition
                    TransitionTime.CreateFixedDateRule(DateTime.MinValue, 1, 1),
                    TransitionTime.CreateFixedDateRule(DateTime.MinValue.AddMilliseconds(1), 1, 1),
                    new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.Bias, 0));  // Bias delta is all what we need from this rule
            }

            //
            // Create an AdjustmentRule with TransitionTime objects
            //
            TransitionTime daylightTransitionStart;
            if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out daylightTransitionStart, true /* start date */)) {
                return null;
            }

            TransitionTime daylightTransitionEnd;
            if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out daylightTransitionEnd, false /* end date */)) {
                return null;
            }

            if (daylightTransitionStart.Equals(daylightTransitionEnd)) {
                // this happens when the time zone does support DST but the OS has DST disabled
                return null;
            }

            rule = AdjustmentRule.CreateAdjustmentRule(
                startDate,
                endDate,
                new TimeSpan(0, -timeZoneInformation.DaylightBias, 0),
                (TransitionTime)daylightTransitionStart,
                (TransitionTime)daylightTransitionEnd,
                new TimeSpan(0, defaultBaseUtcOffset - timeZoneInformation.Bias, 0));

            return rule;
        }


#if FEATURE_WIN32_REGISTRY
        //
        // FindIdFromTimeZoneInformation -
        //
        // Helper function that searches the registry for a time zone entry
        // that matches the TimeZoneInformation struct
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        static private String FindIdFromTimeZoneInformation(Win32Native.TimeZoneInformation timeZone, out Boolean dstDisabled) {
            dstDisabled = false;

            try {
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, c_timeZonesRegistryHivePermissionList));
                permSet.Assert();

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                                  c_timeZonesRegistryHive,
#if FEATURE_MACL
                                  RegistryKeyPermissionCheck.Default,
                                  System.Security.AccessControl.RegistryRights.ReadKey
#else
                                  false
#endif
                                  )) {

                    if (key == null) {
                        return null;
                    }
                    foreach (string keyName in key.GetSubKeyNames()) {
                        if (TryCompareTimeZoneInformationToRegistry(timeZone, keyName, out dstDisabled)) {
                            return keyName;
                        }
                    }
                }
            }
            finally {
                PermissionSet.RevertAssert();
            }
            return null;
        }
#endif // FEATURE_WIN32_REGISTRY


        //
        // GetDaylightTime -
        //
        // Helper function that returns a DaylightTime from a year and AdjustmentRule
        //
        static private DaylightTime GetDaylightTime(Int32 year, AdjustmentRule rule) {
            TimeSpan delta = rule.DaylightDelta;
            DateTime startTime = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
            DateTime endTime = TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
            return new DaylightTime(startTime, endTime, delta);
        }

        //
        // GetIsDaylightSavings -
        //
        // Helper function that checks if a given dateTime is in Daylight Saving Time (DST)
        // This function assumes the dateTime and AdjustmentRule are both in the same time zone
        //
        static private Boolean GetIsDaylightSavings(DateTime time, AdjustmentRule rule, DaylightTime daylightTime, TimeZoneInfoOptions flags) {
            if (rule == null) {
                return false;
            }

            DateTime startTime;
            DateTime endTime;
            
            if (time.Kind == DateTimeKind.Local) {
                // startTime and endTime represent the period from either the start of DST to the end and ***includes*** the 
                // potentially overlapped times
                startTime = rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : daylightTime.Start + daylightTime.Delta;
                endTime = rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1) : daylightTime.End;
            }
            else {
                // startTime and endTime represent the period from either the start of DST to the end and 
                // ***does not include*** the potentially overlapped times
                //
                //         -=-=-=-=-=- Pacific Standard Time -=-=-=-=-=-=-
                //    April 2, 2006                            October 29, 2006
                // 2AM            3AM                        1AM              2AM
                // |      +1 hr     |                        |       -1 hr      |
                // | <invalid time> |                        | <ambiguous time> |
                //                  [========== DST ========>)
                //
                //        -=-=-=-=-=- Some Weird Time Zone -=-=-=-=-=-=-
                //    April 2, 2006                          October 29, 2006
                // 1AM              2AM                    2AM              3AM
                // |      -1 hr       |                      |       +1 hr      |
                // | <ambiguous time> |                      |  <invalid time>  |
                //                    [======== DST ========>)
                //
                Boolean invalidAtStart = rule.DaylightDelta > TimeSpan.Zero;
                startTime = rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : daylightTime.Start + (invalidAtStart ? rule.DaylightDelta : TimeSpan.Zero); /* FUTURE: - rule.StandardDelta; */
                endTime = rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1) : daylightTime.End + (invalidAtStart ? -rule.DaylightDelta : TimeSpan.Zero);
            }

            Boolean isDst = CheckIsDst(startTime, time, endTime, false);

            // If this date was previously converted from a UTC date and we were able to detect that the local
            // DateTime would be ambiguous, this data is stored in the DateTime to resolve this ambiguity. 
            if (isDst && time.Kind == DateTimeKind.Local) {
                // For normal time zones, the ambiguous hour is the last hour of daylight saving when you wind the 
                // clock back. It is theoretically possible to have a positive delta, (which would really be daylight
                // reduction time), where you would have to wind the clock back in the begnning.
                if (GetIsAmbiguousTime(time, rule, daylightTime)) {
                    isDst = time.IsAmbiguousDaylightSavingTime();
                }
            }

            return isDst;
        }


        //
        // GetIsDaylightSavingsFromUtc -
        //
        // Helper function that checks if a given dateTime is in Daylight Saving Time (DST)
        // This function assumes the dateTime is in UTC and AdjustmentRule is in a different time zone
        //
        static private Boolean GetIsDaylightSavingsFromUtc(DateTime time, Int32 Year, TimeSpan utc, AdjustmentRule rule, out Boolean isAmbiguousLocalDst, TimeZoneInfo zone) {
            isAmbiguousLocalDst = false;

            if (rule == null) {
                return false;
            }

            // Get the daylight changes for the year of the specified time.
            TimeSpan offset = utc + rule.BaseUtcOffsetDelta; /* FUTURE: + rule.StandardDelta; */
            DaylightTime daylightTime = GetDaylightTime(Year, rule);

            // The start and end times represent the range of universal times that are in DST for that year.                
            // Within that there is an ambiguous hour, usually right at the end, but at the beginning in
            // the unusual case of a negative daylight savings delta.
            // We need to handle the case if the current rule has daylight saving end by the end of year. If so, we need to check if next year starts with daylight saving on  
            // and get the actual daylight saving end time. Here is example for such case:
            //      Converting the UTC datetime "12/31/2011 8:00:00 PM" to "(UTC+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2)" zone. 
            //      In 2011 the daylight saving will go through the end of the year. If we use the end of 2011 as the daylight saving end, 
            //      that will fail the conversion because the UTC time +4 hours (3 hours for the zone UTC offset and 1 hour for daylight saving) will move us to the next year "1/1/2012 12:00 AM", 
            //      checking against the end of 2011 will tell we are not in daylight saving which is wrong and the conversion will be off by one hour.
            // Note we handle the similar case when rule year start with daylight saving and previous year end with daylight saving.

            bool ignoreYearAdjustment = false;
            DateTime startTime;
            if (rule.IsStartDateMarkerForBeginningOfYear() && daylightTime.Start.Year > DateTime.MinValue.Year) {
                AdjustmentRule previousYearRule = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.Start.Year - 1, 12, 31));
                if (previousYearRule != null && previousYearRule.IsEndDateMarkerForEndOfYear()) {
                    DaylightTime previousDaylightTime = GetDaylightTime(daylightTime.Start.Year - 1, previousYearRule);
                    startTime = previousDaylightTime.Start - utc - previousYearRule.BaseUtcOffsetDelta;
                    ignoreYearAdjustment = true;
                } else {
                    startTime = new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) - offset;
                }
            } else {
                startTime = daylightTime.Start - offset;
            }

            DateTime endTime;
            if (rule.IsEndDateMarkerForEndOfYear() && daylightTime.End.Year < DateTime.MaxValue.Year) {
                AdjustmentRule nextYearRule = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.End.Year + 1, 1, 1));
                if (nextYearRule != null && nextYearRule.IsStartDateMarkerForBeginningOfYear()) {
                    if (nextYearRule.IsEndDateMarkerForEndOfYear()) {// next year end with daylight saving on too
                        endTime = new DateTime(daylightTime.End.Year + 1, 12, 31) - utc - nextYearRule.BaseUtcOffsetDelta - nextYearRule.DaylightDelta;
                    } else {
                        DaylightTime nextdaylightTime = GetDaylightTime(daylightTime.End.Year + 1, nextYearRule);
                        endTime = nextdaylightTime.End - utc - nextYearRule.BaseUtcOffsetDelta - nextYearRule.DaylightDelta;
                    }
                    ignoreYearAdjustment = true;
                } else {
                    endTime = new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1) - offset - rule.DaylightDelta; ;
                }
            } else {
                endTime = daylightTime.End - offset - rule.DaylightDelta;
            }

            DateTime ambiguousStart;
            DateTime ambiguousEnd;
            if (daylightTime.Delta.Ticks > 0) {
                ambiguousStart = endTime - daylightTime.Delta;
                ambiguousEnd = endTime;
            } else {
                ambiguousStart = startTime;
                ambiguousEnd = startTime - daylightTime.Delta;
            }

            Boolean isDst = CheckIsDst(startTime, time, endTime, ignoreYearAdjustment);

            // See if the resulting local time becomes ambiguous. This must be captured here or the
            // DateTime will not be able to round-trip back to UTC accurately.
            if (isDst) {
                isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd);

                if (!isAmbiguousLocalDst && ambiguousStart.Year != ambiguousEnd.Year) {
                    // there exists an extreme corner case where the start or end period is on a year boundary and
                    // because of this the comparison above might have been performed for a year-early or a year-later
                    // than it should have been.
                    DateTime ambiguousStartModified;
                    DateTime ambiguousEndModified;
                    try {
                        ambiguousStartModified = ambiguousStart.AddYears(1);
                        ambiguousEndModified   = ambiguousEnd.AddYears(1);
                        isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd); 
                    }
                    catch (ArgumentOutOfRangeException) {}

                    if (!isAmbiguousLocalDst) {
                        try {
                            ambiguousStartModified = ambiguousStart.AddYears(-1);
                            ambiguousEndModified   = ambiguousEnd.AddYears(-1);
                            isAmbiguousLocalDst = (time >= ambiguousStart && time < ambiguousEnd);
                        }
                        catch (ArgumentOutOfRangeException) {}
                    }

                }
            }

            return isDst;
        }


        static private Boolean CheckIsDst(DateTime startTime, DateTime time, DateTime endTime,bool ignoreYearAdjustment) {
            Boolean isDst;

            if (!ignoreYearAdjustment) {
                int startTimeYear = startTime.Year;
                int endTimeYear = endTime.Year;

                if (startTimeYear != endTimeYear) {
                    endTime = endTime.AddYears(startTimeYear - endTimeYear);
                }

                int timeYear = time.Year;

                if (startTimeYear != timeYear) {
                    time = time.AddYears(startTimeYear - timeYear);
                }
            }

            if (startTime > endTime) {
                // In southern hemisphere, the daylight saving time starts later in the year, and ends in the beginning of next year.
                // Note, the summer in the southern hemisphere begins late in the year.
                isDst = (time < endTime || time >= startTime);
            }
            else {
                // In northern hemisphere, the daylight saving time starts in the middle of the year.
                isDst = (time >= startTime && time < endTime);
            }
            return isDst;
        }


        //
        // GetIsAmbiguousTime(DateTime dateTime, AdjustmentRule rule, DaylightTime daylightTime) -
        //
        // returns true when the dateTime falls into an ambiguous time range.
        // For example, in Pacific Standard Time on Sunday, October 29, 2006 time jumps from
        // 2AM to 1AM.  This means the timeline on Sunday proceeds as follows:
        // 12AM ... [1AM ... 1:59:59AM -> 1AM ... 1:59:59AM] 2AM ... 3AM ...
        //
        // In this example, any DateTime values that fall into the [1AM - 1:59:59AM] range
        // are ambiguous; as it is unclear if these times are in Daylight Saving Time.
        //
        static private Boolean GetIsAmbiguousTime(DateTime time, AdjustmentRule rule, DaylightTime daylightTime) {
            Boolean isAmbiguous = false;
            if (rule == null || rule.DaylightDelta == TimeSpan.Zero) {
                return isAmbiguous;
            }

            DateTime startAmbiguousTime;
            DateTime endAmbiguousTime;

            // if at DST start we transition forward in time then there is an ambiguous time range at the DST end
            if (rule.DaylightDelta > TimeSpan.Zero) {
                if (rule.IsEndDateMarkerForEndOfYear()) { // year end with daylight on so there is no ambiguous time
                    return false;
                }
                startAmbiguousTime = daylightTime.End;
                endAmbiguousTime = daylightTime.End - rule.DaylightDelta; /* FUTURE: + rule.StandardDelta; */
            }
            else {
                if (rule.IsStartDateMarkerForBeginningOfYear()) { // year start with daylight on so there is no ambiguous time
                    return false;
                }
                startAmbiguousTime = daylightTime.Start;
                endAmbiguousTime = daylightTime.Start + rule.DaylightDelta; /* FUTURE: - rule.StandardDelta; */
            }

            isAmbiguous = (time >= endAmbiguousTime && time < startAmbiguousTime);

            if (!isAmbiguous && startAmbiguousTime.Year != endAmbiguousTime.Year) {
                // there exists an extreme corner case where the start or end period is on a year boundary and
                // because of this the comparison above might have been performed for a year-early or a year-later
                // than it should have been.
                DateTime startModifiedAmbiguousTime;
                DateTime endModifiedAmbiguousTime;
                try {
                    startModifiedAmbiguousTime = startAmbiguousTime.AddYears(1);
                    endModifiedAmbiguousTime   = endAmbiguousTime.AddYears(1);
                    isAmbiguous = (time >= endModifiedAmbiguousTime && time < startModifiedAmbiguousTime);
                }
                catch (ArgumentOutOfRangeException) {}

                if (!isAmbiguous) {
                    try {
                        startModifiedAmbiguousTime = startAmbiguousTime.AddYears(-1);
                        endModifiedAmbiguousTime  = endAmbiguousTime.AddYears(-1);
                        isAmbiguous = (time >= endModifiedAmbiguousTime && time < startModifiedAmbiguousTime);
                    }
                    catch (ArgumentOutOfRangeException) {}
                }
            }
            return isAmbiguous;
        }



        //
        // GetIsInvalidTime -
        //
        // Helper function that checks if a given DateTime is in an invalid time ("time hole")
        // A "time hole" occurs at a DST transition point when time jumps forward;
        // For example, in Pacific Standard Time on Sunday, April 2, 2006 time jumps from
        // 1:59:59.9999999 to 3AM.  The time range 2AM to 2:59:59.9999999AM is the "time hole".
        // A "time hole" is not limited to only occurring at the start of DST, and may occur at
        // the end of DST as well.
        //
        static private Boolean GetIsInvalidTime(DateTime time, AdjustmentRule rule, DaylightTime daylightTime) {
            Boolean isInvalid = false;
            if (rule == null || rule.DaylightDelta == TimeSpan.Zero) {
                return isInvalid;
            }

            DateTime startInvalidTime;
            DateTime endInvalidTime;

            // if at DST start we transition forward in time then there is an ambiguous time range at the DST end
            if (rule.DaylightDelta < TimeSpan.Zero) {
                // if the year ends with daylight saving on then there cannot be any time-hole's in that year.
                if (rule.IsEndDateMarkerForEndOfYear())
                    return false;

                startInvalidTime = daylightTime.End;
                endInvalidTime = daylightTime.End - rule.DaylightDelta; /* FUTURE: + rule.StandardDelta; */
            }
            else {
                // if the year starts with daylight saving on then there cannot be any time-hole's in that year.
                if (rule.IsStartDateMarkerForBeginningOfYear())
                    return false;

                startInvalidTime = daylightTime.Start;
                endInvalidTime = daylightTime.Start + rule.DaylightDelta; /* FUTURE: - rule.StandardDelta; */
            }

            isInvalid = (time >= startInvalidTime && time < endInvalidTime);

            if (!isInvalid && startInvalidTime.Year != endInvalidTime.Year) {
                // there exists an extreme corner case where the start or end period is on a year boundary and
                // because of this the comparison above might have been performed for a year-early or a year-later
                // than it should have been.
                DateTime startModifiedInvalidTime;
                DateTime endModifiedInvalidTime;
                try {
                    startModifiedInvalidTime = startInvalidTime.AddYears(1);
                    endModifiedInvalidTime   = endInvalidTime.AddYears(1);
                    isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
                }
                catch (ArgumentOutOfRangeException) {}

                if (!isInvalid) {
                    try {
                        startModifiedInvalidTime = startInvalidTime.AddYears(-1);
                        endModifiedInvalidTime  = endInvalidTime.AddYears(-1);
                        isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
                    }
                    catch (ArgumentOutOfRangeException) {}
                }
            }
            return isInvalid;
        }



        //
        // GetLocalTimeZone -
        //
        // Helper function for retrieving the local system time zone.
        //
        // returns a new TimeZoneInfo instance
        //
        // may throw COMException, TimeZoneNotFoundException, InvalidTimeZoneException
        //
        // assumes cachedData lock is taken
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        static private TimeZoneInfo GetLocalTimeZone(CachedData cachedData) {


#if FEATURE_WIN32_REGISTRY
            String id = null;

            //
            // Try using the "kernel32!GetDynamicTimeZoneInformation" API to get the "id"
            //
            Win32Native.DynamicTimeZoneInformation dynamicTimeZoneInformation =
                new Win32Native.DynamicTimeZoneInformation();

            // call kernel32!GetDynamicTimeZoneInformation...
            long result = UnsafeNativeMethods.GetDynamicTimeZoneInformation(out dynamicTimeZoneInformation);
            if (result == Win32Native.TIME_ZONE_ID_INVALID) {
                // return a dummy entry
                return CreateCustomTimeZone(c_localId, TimeSpan.Zero, c_localId, c_localId);
            }

            Win32Native.TimeZoneInformation timeZoneInformation = 
                new Win32Native.TimeZoneInformation(dynamicTimeZoneInformation);

            Boolean dstDisabled = dynamicTimeZoneInformation.DynamicDaylightTimeDisabled;

            // check to see if we can use the key name returned from the API call
            if (!String.IsNullOrEmpty(dynamicTimeZoneInformation.TimeZoneKeyName)) {
                TimeZoneInfo zone;
                Exception ex;
                    
                if (TryGetTimeZone(dynamicTimeZoneInformation.TimeZoneKeyName, dstDisabled, out zone, out ex, cachedData) == TimeZoneInfoResult.Success) {
                    // successfully loaded the time zone from the registry
                    return zone;
                }
            }

            // the key name was not returned or it pointed to a bogus entry - search for the entry ourselves                
            id = FindIdFromTimeZoneInformation(timeZoneInformation, out dstDisabled);

            if (id != null) {
                TimeZoneInfo zone;
                Exception ex;
                if (TryGetTimeZone(id, dstDisabled, out zone, out ex, cachedData) == TimeZoneInfoResult.Success) {
                    // successfully loaded the time zone from the registry
                    return zone;
                }
            }

            // We could not find the data in the registry.  Fall back to using
            // the data from the Win32 API
            return GetLocalTimeZoneFromWin32Data(timeZoneInformation, dstDisabled);
            
#else // FEATURE_WIN32_REGISTRY
            // Without Registry support, just create a dummy TZ for now
            return GetLocalTimeZoneFromTzFile();
#endif // FEATURE_WIN32_REGISTRY
        }


#if !FEATURE_WIN32_REGISTRY
        //
        // GetLocalTimeZoneFromTzFile -
        //
        // Helper function used by 'GetLocalTimeZone()' - this function wraps the call
        // for loading time zone data from computers without Registry support.
        //
        // The GetLocalTzFile() call returns a Byte[] containing the compiled tzfile.
        // 
        [System.Security.SecurityCritical]
        static private TimeZoneInfo GetLocalTimeZoneFromTzFile() {
            Byte[] rawData = GetLocalTzFile();

            if (rawData != null) {
                try {
                    return new TimeZoneInfo(rawData, false); // create a TimeZoneInfo instance from the TZif data w/ DST support
                }
                catch (ArgumentException) {}
                catch (InvalidTimeZoneException) {}
                try {
                    return new TimeZoneInfo(rawData, true); // create a TimeZoneInfo instance from the TZif data w/o DST support
                }
                catch (ArgumentException) {}
                catch (InvalidTimeZoneException) {}
             }
            // the data returned from the PAL is completely bogus; return a dummy entry
            return CreateCustomTimeZone(c_localId, TimeSpan.Zero, c_localId, c_localId);
        }
#endif // !FEATURE_WIN32_REGISTRY


        //
        // GetLocalTimeZoneFromWin32Data -
        //
        // Helper function used by 'GetLocalTimeZone()' - this function wraps a bunch of
        // try/catch logic for handling the TimeZoneInfo private constructor that takes
        // a Win32Native.TimeZoneInformation structure.
        //
        [System.Security.SecurityCritical]  // auto-generated
        static private TimeZoneInfo GetLocalTimeZoneFromWin32Data(Win32Native.TimeZoneInformation timeZoneInformation, Boolean dstDisabled) {
            // first try to create the TimeZoneInfo with the original 'dstDisabled' flag
            try {
                return new TimeZoneInfo(timeZoneInformation, dstDisabled);
            }
            catch (ArgumentException) {}
            catch (InvalidTimeZoneException) {}

            // if 'dstDisabled' was false then try passing in 'true' as a last ditch effort
            if (!dstDisabled) {
                try {
                    return new TimeZoneInfo(timeZoneInformation, true);
                }
                catch (ArgumentException) {}
                catch (InvalidTimeZoneException) {}
            }

            // the data returned from Windows is completely bogus; return a dummy entry
            return CreateCustomTimeZone(c_localId, TimeSpan.Zero, c_localId, c_localId);
        }


#if FEATURE_WIN32_REGISTRY
        //
        // FindSystemTimeZoneById -
        //
        // Helper function for retrieving a TimeZoneInfo object by <time_zone_name>.
        // This function wraps the logic necessary to keep the private 
        // SystemTimeZones cache in working order
        //
        // This function will either return a valid TimeZoneInfo instance or 
        // it will throw 'InvalidTimeZoneException' / 'TimeZoneNotFoundException'.
        //
        static public TimeZoneInfo FindSystemTimeZoneById(string id) {

            // Special case for Utc as it will not exist in the dictionary with the rest
            // of the system time zones.  There is no need to do this check for Local.Id
            // since Local is a real time zone that exists in the dictionary cache
            if (String.Compare(id, c_utcId, StringComparison.OrdinalIgnoreCase) == 0) {
                return TimeZoneInfo.Utc;
            }

            if (id == null) {
                throw new ArgumentNullException("id");
            }
            else if (id.Length == 0 || id.Length > c_maxKeyLength || id.Contains("\0")) {
                throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", id));
            }

            TimeZoneInfo value;
            Exception e;

            TimeZoneInfoResult result;

            CachedData cachedData = s_cachedData;

            lock (cachedData) {
                result = TryGetTimeZone(id, false, out value, out e, cachedData);
            }

            if (result == TimeZoneInfoResult.Success) {
                return value;
            }
            else if (result == TimeZoneInfoResult.InvalidTimeZoneException) {
                throw new InvalidTimeZoneException(Environment.GetResourceString("InvalidTimeZone_InvalidRegistryData", id), e);
            }
            else if (result == TimeZoneInfoResult.SecurityException) {
                throw new SecurityException(Environment.GetResourceString("Security_CannotReadRegistryData", id), e);
            }
            else {
                throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", id), e);
            }
        }
#endif // FEATURE_WIN32_REGISTRY


        //
        // GetUtcOffset -
        //
        // Helper function that calculates the UTC offset for a dateTime in a timeZone.
        // This function assumes that the dateTime is already converted into the timeZone.
        //
        static private TimeSpan GetUtcOffset(DateTime time, TimeZoneInfo zone, TimeZoneInfoOptions flags) {
            TimeSpan baseOffset = zone.BaseUtcOffset;
            AdjustmentRule rule = zone.GetAdjustmentRuleForTime(time);
 
            if (rule != null) {
                baseOffset = baseOffset + rule.BaseUtcOffsetDelta;
                if (rule.HasDaylightSaving) {
                    DaylightTime daylightTime = GetDaylightTime(time.Year, rule);
                    Boolean isDaylightSavings = GetIsDaylightSavings(time, rule, daylightTime, flags);
                    baseOffset += (isDaylightSavings ? rule.DaylightDelta : TimeSpan.Zero /* FUTURE: rule.StandardDelta */);
                }
            }

            return baseOffset;
        }


        //
        // GetUtcOffsetFromUtc -
        //
        // Helper function that calculates the UTC offset for a UTC-dateTime in a timeZone.
        // This function assumes that the dateTime is represented in UTC and has *not*
        // already been converted into the timeZone.
        //
        static private TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone) {
            Boolean isDaylightSavings;
            return GetUtcOffsetFromUtc(time, zone, out isDaylightSavings);
        }

        static private TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out Boolean isDaylightSavings) {
            Boolean isAmbiguousLocalDst;
            return GetUtcOffsetFromUtc(time, zone, out isDaylightSavings, out isAmbiguousLocalDst);
        }

        // DateTime.Now fast path that avoids allocating an historically accurate TimeZoneInfo.Local and just creates a 1-year (current year) accurate time zone
        static internal TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out Boolean isAmbiguousLocalDst) {
            Boolean isDaylightSavings = false;
#if FEATURE_WIN32_REGISTRY
            isAmbiguousLocalDst = false;
            TimeSpan baseOffset;
            int timeYear = time.Year;

            OffsetAndRule match = s_cachedData.GetOneYearLocalFromUtc(timeYear);
            baseOffset = match.offset;

            if (match.rule != null) {
                baseOffset = baseOffset + match.rule.BaseUtcOffsetDelta;
                if (match.rule.HasDaylightSaving) {
                    isDaylightSavings = GetIsDaylightSavingsFromUtc(time, timeYear, match.offset, match.rule, out isAmbiguousLocalDst, TimeZoneInfo.Local);
                    baseOffset += (isDaylightSavings ? match.rule.DaylightDelta : TimeSpan.Zero /* FUTURE: rule.StandardDelta */);
                }
            }                
            return baseOffset;          
#else
            // Use the standard code path for the Macintosh since there isn't a faster way of handling current-year-only time zones
            return GetUtcOffsetFromUtc(time, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst);
#endif // FEATURE_WIN32_REGISTRY
        }

        static internal TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out Boolean isDaylightSavings, out Boolean isAmbiguousLocalDst) {
            isDaylightSavings = false;
            isAmbiguousLocalDst = false;
            TimeSpan baseOffset = zone.BaseUtcOffset;
            Int32 year;
            AdjustmentRule rule;

            if (time > s_maxDateOnly) {
                rule = zone.GetAdjustmentRuleForTime(DateTime.MaxValue);
                year = 9999;
            }
            else if (time < s_minDateOnly) {
                rule = zone.GetAdjustmentRuleForTime(DateTime.MinValue);
                year = 1;
            }
            else {
                DateTime targetTime = time + baseOffset;

                // As we get the associated rule using the adjusted targetTime, we should use the adjusted year (targetTime.Year) too as after adding the baseOffset, 
                // sometimes the year value can change if the input datetime was very close to the beginning or the end of the year. Examples of such cases:
                //      “Libya Standard Time” when used with the date 2011-12-31T23:59:59.9999999Z
                //      "W. Australia Standard Time" used with date 2005-12-31T23:59:00.0000000Z
                year = targetTime.Year;

                rule = zone.GetAdjustmentRuleForTime(targetTime);
            }

            if (rule != null) {
                baseOffset = baseOffset + rule.BaseUtcOffsetDelta;
                if (rule.HasDaylightSaving) {
                    isDaylightSavings = GetIsDaylightSavingsFromUtc(time, year, zone.m_baseUtcOffset, rule, out isAmbiguousLocalDst, zone);
                    baseOffset += (isDaylightSavings ? rule.DaylightDelta : TimeSpan.Zero /* FUTURE: rule.StandardDelta */);
                }
            }

            return baseOffset;
        }


        //
        // TransitionTimeFromTimeZoneInformation -
        //
        // Converts a Win32Native.RegistryTimeZoneInformation (REG_TZI_FORMAT struct) to a TransitionTime
        //
        // * when the argument 'readStart' is true the corresponding daylightTransitionTimeStart field is read
        // * when the argument 'readStart' is false the corresponding dayightTransitionTimeEnd field is read
        //
        [System.Security.SecurityCritical]  // auto-generated
        static private bool TransitionTimeFromTimeZoneInformation(Win32Native.RegistryTimeZoneInformation timeZoneInformation, out TransitionTime transitionTime, bool readStartDate) {
            //
            // SYSTEMTIME - 
            //
            // If the time zone does not support daylight saving time or if the caller needs
            // to disable daylight saving time, the wMonth member in the SYSTEMTIME structure
            // must be zero. If this date is specified, the DaylightDate value in the 
            // TIME_ZONE_INFORMATION structure must also be specified. Otherwise, the system 
            // assumes the time zone data is invalid and no changes will be applied.
            //
            bool supportsDst = (timeZoneInformation.StandardDate.Month != 0);

            if (!supportsDst) {
                transitionTime = default(TransitionTime);
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
            //   Hour      = 2, 
            //   Month     = 4,
            //   DayOfWeek = 0,
            //   Day       = 1.
            //
            //   Specify 2:00a.m. on the last Thursday in October as follows:
            //   Hour      = 2,
            //   Month     = 10,
            //   DayOfWeek = 4,
            //   Day       = 5.
            //
            if (readStartDate) {
                 //
                 // read the "daylightTransitionStart"
                 //
                 if (timeZoneInformation.DaylightDate.Year == 0) {
                    transitionTime = TransitionTime.CreateFloatingDateRule(
                                     new DateTime(1,    /* year  */           
                                                  1,    /* month */
                                                  1,    /* day   */
                                                  timeZoneInformation.DaylightDate.Hour,
                                                  timeZoneInformation.DaylightDate.Minute,
                                                  timeZoneInformation.DaylightDate.Second,
                                                  timeZoneInformation.DaylightDate.Milliseconds),
                                     timeZoneInformation.DaylightDate.Month,
                                     timeZoneInformation.DaylightDate.Day,   /* Week 1-5 */
                                     (DayOfWeek) timeZoneInformation.DaylightDate.DayOfWeek);
                }
                else {
                    transitionTime = TransitionTime.CreateFixedDateRule(
                                     new DateTime(1,    /* year  */           
                                                  1,    /* month */
                                                  1,    /* day   */
                                                  timeZoneInformation.DaylightDate.Hour,
                                                  timeZoneInformation.DaylightDate.Minute,
                                                  timeZoneInformation.DaylightDate.Second,
                                                  timeZoneInformation.DaylightDate.Milliseconds),
                                     timeZoneInformation.DaylightDate.Month,
                                     timeZoneInformation.DaylightDate.Day);
                }
            }
            else {
                //
                // read the "daylightTransitionEnd"
                //
                if (timeZoneInformation.StandardDate.Year == 0) {
                    transitionTime = TransitionTime.CreateFloatingDateRule(
                                     new DateTime(1,    /* year  */           
                                                  1,    /* month */
                                                  1,    /* day   */
                                                  timeZoneInformation.StandardDate.Hour,
                                                  timeZoneInformation.StandardDate.Minute,
                                                  timeZoneInformation.StandardDate.Second,
                                                  timeZoneInformation.StandardDate.Milliseconds),
                                     timeZoneInformation.StandardDate.Month,
                                     timeZoneInformation.StandardDate.Day,   /* Week 1-5 */
                                     (DayOfWeek) timeZoneInformation.StandardDate.DayOfWeek);
                }
                else {
                    transitionTime = TransitionTime.CreateFixedDateRule(
                                     new DateTime(1,    /* year  */           
                                                  1,    /* month */
                                                  1,    /* day   */
                                                  timeZoneInformation.StandardDate.Hour,
                                                  timeZoneInformation.StandardDate.Minute,
                                                  timeZoneInformation.StandardDate.Second,
                                                  timeZoneInformation.StandardDate.Milliseconds),
                                     timeZoneInformation.StandardDate.Month,
                                     timeZoneInformation.StandardDate.Day);
                }
            }

            return true;
        }      

        //
        // TransitionTimeToDateTime -
        //
        // Helper function that converts a year and TransitionTime into a DateTime
        //
        static private DateTime TransitionTimeToDateTime(Int32 year, TransitionTime transitionTime) {
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

#if FEATURE_WIN32_REGISTRY
        //
        // TryCreateAdjustmentRules -
        //
        // Helper function that takes 
        //  1. a string representing a <time_zone_name> registry key name
        //  2. a RegistryTimeZoneInformation struct containing the default rule
        //  3. an AdjustmentRule[] out-parameter
        // 
        // returns 
        //     TimeZoneInfoResult.InvalidTimeZoneException,
        //     TimeZoneInfoResult.TimeZoneNotFoundException,
        //     TimeZoneInfoResult.Success
        //                             
        // Optional, Dynamic Time Zone Registry Data
        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
        //
        // HKLM 
        //     Software 
        //         Microsoft 
        //             Windows NT 
        //                 CurrentVersion 
        //                     Time Zones 
        //                         <time_zone_name>
        //                             Dynamic DST
        // * "FirstEntry" REG_DWORD "1980"
        //                           First year in the table. If the current year is less than this value,
        //                           this entry will be used for DST boundaries
        // * "LastEntry"  REG_DWORD "2038"
        //                           Last year in the table. If the current year is greater than this value,
        //                           this entry will be used for DST boundaries"
        // * "<year1>"    REG_BINARY REG_TZI_FORMAT
        //                       See Win32Native.RegistryTimeZoneInformation
        // * "<year2>"    REG_BINARY REG_TZI_FORMAT 
        //                       See Win32Native.RegistryTimeZoneInformation
        // * "<year3>"    REG_BINARY REG_TZI_FORMAT
        //                       See Win32Native.RegistryTimeZoneInformation
        //
        // This method expects that its caller has already Asserted RegistryPermission.Read
        //
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        static private bool TryCreateAdjustmentRules(string id, Win32Native.RegistryTimeZoneInformation defaultTimeZoneInformation, out AdjustmentRule[] rules, out Exception e, int defaultBaseUtcOffset) {
            e = null;

            try {
                using (RegistryKey dynamicKey = Registry.LocalMachine.OpenSubKey(
                                   String.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\Dynamic DST",
                                       c_timeZonesRegistryHive, id),
#if FEATURE_MACL
                                   RegistryKeyPermissionCheck.Default,
                                   System.Security.AccessControl.RegistryRights.ReadKey
#else
                                   false
#endif
                                   )) {
                    if (dynamicKey == null) {
                        AdjustmentRule rule = CreateAdjustmentRuleFromTimeZoneInformation(
                                              defaultTimeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date, defaultBaseUtcOffset);

                        if (rule == null) {
                            rules = null;
                        }
                        else {
                            rules = new AdjustmentRule[1];
                            rules[0] = rule;
                        }
                        
                        return true;
                    }

                    //
                    // loop over all of the "<time_zone_name>\Dynamic DST" hive entries
                    //
                    // read FirstEntry  {MinValue      - (year1, 12, 31)}
                    // read MiddleEntry {(yearN, 1, 1) - (yearN, 12, 31)}
                    // read LastEntry   {(yearN, 1, 1) - MaxValue       }

                    // read the FirstEntry and LastEntry key values (ex: "1980", "2038")
                    Int32 first = (Int32)dynamicKey.GetValue(c_firstEntryValue, -1, RegistryValueOptions.None);
                    Int32 last = (Int32)dynamicKey.GetValue(c_lastEntryValue, -1, RegistryValueOptions.None);

                    if (first == -1 || last == -1 || first > last) {
                        rules = null;
                        return false;
                    }

                    // read the first year entry
                    Win32Native.RegistryTimeZoneInformation dtzi;
                    Byte[] regValue = dynamicKey.GetValue(first.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as Byte[];
                    if (regValue == null || regValue.Length != c_regByteLength) {
                        rules = null;
                        return false;
                    }
                    dtzi = new Win32Native.RegistryTimeZoneInformation(regValue);

                    if (first == last) {
                        // there is just 1 dynamic rule for this time zone.
                        AdjustmentRule rule =  CreateAdjustmentRuleFromTimeZoneInformation(dtzi, DateTime.MinValue.Date, DateTime.MaxValue.Date, defaultBaseUtcOffset);

                        if (rule == null) {
                            rules = null;
                        }
                        else {
                            rules = new AdjustmentRule[1];
                            rules[0] = rule;
                        }

                        return true;
                    }

                    List<AdjustmentRule> rulesList = new List<AdjustmentRule>(1);

                     // there are more than 1 dynamic rules for this time zone.
                    AdjustmentRule firstRule = CreateAdjustmentRuleFromTimeZoneInformation(
                                              dtzi,
                                              DateTime.MinValue.Date,        // MinValue
                                              new DateTime(first, 12, 31),   // December 31, <FirstYear>
                                              defaultBaseUtcOffset); 
                    if (firstRule != null) {
                        rulesList.Add(firstRule);
                    }

                    // read the middle year entries
                    for (Int32 i = first + 1; i < last; i++) {
                        regValue = dynamicKey.GetValue(i.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as Byte[];
                        if (regValue == null || regValue.Length != c_regByteLength) {
                            rules = null;
                            return false;
                        }
                        dtzi = new Win32Native.RegistryTimeZoneInformation(regValue);
                        AdjustmentRule middleRule = CreateAdjustmentRuleFromTimeZoneInformation(
                                                  dtzi,
                                                  new DateTime(i, 1, 1),    // January  01, <Year>
                                                  new DateTime(i, 12, 31),  // December 31, <Year>
                                                  defaultBaseUtcOffset);
                        if (middleRule != null) {
                            rulesList.Add(middleRule);
                        }
                    }
                    // read the last year entry
                    regValue = dynamicKey.GetValue(last.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as Byte[];
                    dtzi = new Win32Native.RegistryTimeZoneInformation(regValue);
                    if (regValue == null || regValue.Length != c_regByteLength) {
                        rules = null;
                        return false;
                    }
                    AdjustmentRule lastRule = CreateAdjustmentRuleFromTimeZoneInformation(
                                              dtzi,
                                              new DateTime(last, 1, 1),    // January  01, <LastYear>
                                              DateTime.MaxValue.Date,      // MaxValue
                                              defaultBaseUtcOffset);
                    if (lastRule != null) {
                        rulesList.Add(lastRule);
                    }

                    // convert the ArrayList to an AdjustmentRule array
                    rules = rulesList.ToArray();
                    if (rules != null && rules.Length == 0) {
                        rules = null;
                    }
                } // end of: using (RegistryKey dynamicKey...
            }
            catch (InvalidCastException ex) {
                // one of the RegistryKey.GetValue calls could not be cast to an expected value type
                rules = null;
                e = ex;
                return false;
            }
            catch (ArgumentOutOfRangeException ex) {
                rules = null;
                e = ex;
                return false;
            }
            catch (ArgumentException ex) {
                rules = null;
                e = ex;
                return false;
            }
            return true;
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        //
        // TryCompareStandardDate -
        //
        // Helper function that compares the StandardBias and StandardDate portion a
        // TimeZoneInformation struct to a time zone registry entry
        //
        [System.Security.SecurityCritical]  // auto-generated
        static private Boolean TryCompareStandardDate(Win32Native.TimeZoneInformation timeZone, Win32Native.RegistryTimeZoneInformation registryTimeZoneInfo) {
            return timeZone.Bias                         == registryTimeZoneInfo.Bias
                   && timeZone.StandardBias              == registryTimeZoneInfo.StandardBias
                   && timeZone.StandardDate.Year         == registryTimeZoneInfo.StandardDate.Year
                   && timeZone.StandardDate.Month        == registryTimeZoneInfo.StandardDate.Month
                   && timeZone.StandardDate.DayOfWeek    == registryTimeZoneInfo.StandardDate.DayOfWeek
                   && timeZone.StandardDate.Day          == registryTimeZoneInfo.StandardDate.Day
                   && timeZone.StandardDate.Hour         == registryTimeZoneInfo.StandardDate.Hour
                   && timeZone.StandardDate.Minute       == registryTimeZoneInfo.StandardDate.Minute
                   && timeZone.StandardDate.Second       == registryTimeZoneInfo.StandardDate.Second
                   && timeZone.StandardDate.Milliseconds == registryTimeZoneInfo.StandardDate.Milliseconds;
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        //
        // TryCompareTimeZoneInformationToRegistry -
        //
        // Helper function that compares a TimeZoneInformation struct to a time zone registry entry
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        static private Boolean TryCompareTimeZoneInformationToRegistry(Win32Native.TimeZoneInformation timeZone, string id, out Boolean dstDisabled) {

            dstDisabled = false;
            try {
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, c_timeZonesRegistryHivePermissionList));
                permSet.Assert();

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                                  String.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                                      c_timeZonesRegistryHive, id),
#if FEATURE_MACL
                                  RegistryKeyPermissionCheck.Default,
                                  System.Security.AccessControl.RegistryRights.ReadKey
#else
                                  false
#endif
                                  )) {

                    if (key == null) {
                        return false;
                    }

                    Win32Native.RegistryTimeZoneInformation registryTimeZoneInfo;
                    Byte[] regValue = (Byte[])key.GetValue(c_timeZoneInfoValue, null, RegistryValueOptions.None) as Byte[];
                    if (regValue == null || regValue.Length != c_regByteLength) return false;
                    registryTimeZoneInfo = new Win32Native.RegistryTimeZoneInformation(regValue);

                    //
                    // first compare the bias and standard date information between the data from the Win32 API
                    // and the data from the registry...
                    //
                    Boolean result = TryCompareStandardDate(timeZone, registryTimeZoneInfo);

                    if (!result) {
                        return false;
                    }

                    result = dstDisabled || CheckDaylightSavingTimeNotSupported(timeZone)
                             //
                             // since Daylight Saving Time is not "disabled", do a straight comparision between
                             // the Win32 API data and the registry data ...
                             //
                             ||(   timeZone.DaylightBias              == registryTimeZoneInfo.DaylightBias
                                && timeZone.DaylightDate.Year         == registryTimeZoneInfo.DaylightDate.Year
                                && timeZone.DaylightDate.Month        == registryTimeZoneInfo.DaylightDate.Month
                                && timeZone.DaylightDate.DayOfWeek    == registryTimeZoneInfo.DaylightDate.DayOfWeek
                                && timeZone.DaylightDate.Day          == registryTimeZoneInfo.DaylightDate.Day
                                && timeZone.DaylightDate.Hour         == registryTimeZoneInfo.DaylightDate.Hour
                                && timeZone.DaylightDate.Minute       == registryTimeZoneInfo.DaylightDate.Minute
                                && timeZone.DaylightDate.Second       == registryTimeZoneInfo.DaylightDate.Second
                                && timeZone.DaylightDate.Milliseconds == registryTimeZoneInfo.DaylightDate.Milliseconds);

                    // Finally compare the "StandardName" string value...
                    //
                    // we do not compare "DaylightName" as this TimeZoneInformation field may contain
                    // either "StandardName" or "DaylightName" depending on the time of year and current machine settings
                    //
                    if (result) {
                        String registryStandardName = key.GetValue(c_standardValue, String.Empty, RegistryValueOptions.None) as String;
                        result = String.Compare(registryStandardName, timeZone.StandardName, StringComparison.Ordinal) == 0;
                    }
                    return result;  
                }  
            }
            finally {
                PermissionSet.RevertAssert();
            }
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        //
        // TryGetLocalizedNameByMuiNativeResource -
        //
        // Helper function for retrieving a localized string resource via MUI.
        // The function expects a string in the form: "@resource.dll, -123"
        //
        // "resource.dll" is a language-neutral portable executable (LNPE) file in
        // the %windir%\system32 directory.  The OS is queried to find the best-fit
        // localized resource file for this LNPE (ex: %windir%\system32\en-us\resource.dll.mui).
        // If a localized resource file exists, we LoadString resource ID "123" and
        // return it to our caller.
        //
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeNativeMethods.GetFileMUIPath(System.Int32,System.String,System.Text.StringBuilder,System.Int32&,System.Text.StringBuilder,System.Int32&,System.Int64&):System.Boolean" />
        // <ReferencesCritical Name="Method: TryGetLocalizedNameByNativeResource(String, Int32):String" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [FileIOPermissionAttribute(SecurityAction.Assert, AllLocalFiles = FileIOPermissionAccess.PathDiscovery)]
#endif
        static private string TryGetLocalizedNameByMuiNativeResource(string resource) {
            if (String.IsNullOrEmpty(resource)) {
                return String.Empty;
            }

            // parse "@tzres.dll, -100"
            // 
            // filePath   = "C:\Windows\System32\tzres.dll"
            // resourceId = -100
            //
            string[] resources = resource.Split(new char[] {','}, StringSplitOptions.None);
            if (resources.Length != 2) {
                return String.Empty;
            }

            string filePath;
            int resourceId;

            // get the path to Windows\System32
            string system32 = Environment.UnsafeGetFolderPath(Environment.SpecialFolder.System);

            // trim the string "@tzres.dll" => "tzres.dll"
            string tzresDll = resources[0].TrimStart(new char[] {'@'});

            try {
                filePath = Path.Combine(system32, tzresDll);
            }
            catch (ArgumentException) {
                //  there were probably illegal characters in the path
                return String.Empty;
            }

            if (!Int32.TryParse(resources[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out resourceId)) {
                return String.Empty;
            }
            resourceId = -resourceId;


            try {
                StringBuilder fileMuiPath = StringBuilderCache.Acquire(Win32Native.MAX_PATH);
                fileMuiPath.Length = Win32Native.MAX_PATH;
                int fileMuiPathLength = Win32Native.MAX_PATH;
                int languageLength = 0;
                Int64 enumerator = 0;

                Boolean succeeded = UnsafeNativeMethods.GetFileMUIPath(
                                        Win32Native.MUI_PREFERRED_UI_LANGUAGES,
                                        filePath, null /* language */, ref languageLength,
                                        fileMuiPath, ref fileMuiPathLength, ref enumerator);
                if (!succeeded) {
                    StringBuilderCache.Release(fileMuiPath);
                    return String.Empty;
                }
                return TryGetLocalizedNameByNativeResource(StringBuilderCache.GetStringAndRelease(fileMuiPath), resourceId);
            }
            catch (EntryPointNotFoundException) {
                return String.Empty;
            }
        }
#endif // FEATURE_WIN32_REGISTRY



#if FEATURE_WIN32_REGISTRY
        //
        // TryGetLocalizedNameByNativeResource -
        //
        // Helper function for retrieving a localized string resource via a native resource DLL.
        // The function expects a string in the form: "C:\Windows\System32\en-us\resource.dll"
        //
        // "resource.dll" is a language-specific resource DLL.
        // If the localized resource DLL exists, LoadString(resource) is returned.
        //
        [SecurityCritical]
        static private string TryGetLocalizedNameByNativeResource(string filePath, int resource) {
            using (SafeLibraryHandle handle = 
                       UnsafeNativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, Win32Native.LOAD_LIBRARY_AS_DATAFILE)) {

                if (!handle.IsInvalid) {
                    StringBuilder localizedResource = StringBuilderCache.Acquire(Win32Native.LOAD_STRING_MAX_LENGTH);
                    localizedResource.Length = Win32Native.LOAD_STRING_MAX_LENGTH;

                    int result = UnsafeNativeMethods.LoadString(handle, resource, 
                                     localizedResource, localizedResource.Length);

                    if (result != 0) {
                        return StringBuilderCache.GetStringAndRelease(localizedResource);
                    }
                }
            }
            return String.Empty;
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        //
        // TryGetLocalizedNamesByRegistryKey -
        //
        // Helper function for retrieving the DisplayName, StandardName, and DaylightName from the registry
        //
        // The function first checks the MUI_ key-values, and if they exist, it loads the strings from the MUI
        // resource dll(s).  When the keys do not exist, the function falls back to reading from the standard
        // key-values
        //
        // This method expects that its caller has already Asserted RegistryPermission.Read
        //
        #if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
        #endif
        static private Boolean TryGetLocalizedNamesByRegistryKey(RegistryKey key, out String displayName, out String standardName, out String daylightName) {
            displayName  = String.Empty;
            standardName = String.Empty;
            daylightName = String.Empty;

            // read the MUI_ registry keys
            String displayNameMuiResource  = key.GetValue(c_muiDisplayValue,  String.Empty, RegistryValueOptions.None) as String;
            String standardNameMuiResource = key.GetValue(c_muiStandardValue, String.Empty, RegistryValueOptions.None) as String;
            String daylightNameMuiResource = key.GetValue(c_muiDaylightValue, String.Empty, RegistryValueOptions.None) as String;

            // try to load the strings from the native resource DLL(s)
            if (!String.IsNullOrEmpty(displayNameMuiResource)) {
                displayName  = TryGetLocalizedNameByMuiNativeResource(displayNameMuiResource);
            }

            if (!String.IsNullOrEmpty(standardNameMuiResource)) {
                standardName = TryGetLocalizedNameByMuiNativeResource(standardNameMuiResource);
            }

            if (!String.IsNullOrEmpty(daylightNameMuiResource)) {
                daylightName = TryGetLocalizedNameByMuiNativeResource(daylightNameMuiResource);
            }

            // fallback to using the standard registry keys
            if (String.IsNullOrEmpty(displayName)) {
                displayName  = key.GetValue(c_displayValue,  String.Empty, RegistryValueOptions.None) as String;
            }
            if (String.IsNullOrEmpty(standardName)) {
                standardName = key.GetValue(c_standardValue, String.Empty, RegistryValueOptions.None) as String;
            }
            if (String.IsNullOrEmpty(daylightName)) {
                daylightName = key.GetValue(c_daylightValue, String.Empty, RegistryValueOptions.None) as String;
            }

            return true;
        }
#endif // FEATURE_WIN32_REGISTRY



#if FEATURE_WIN32_REGISTRY
        //
        // TryGetTimeZoneByRegistryKey -
        //
        // Helper function that takes a string representing a <time_zone_name> registry key name
        // and returns a TimeZoneInfo instance.
        // 
        // returns 
        //     TimeZoneInfoResult.InvalidTimeZoneException,
        //     TimeZoneInfoResult.TimeZoneNotFoundException,
        //     TimeZoneInfoResult.SecurityException,
        //     TimeZoneInfoResult.Success
        // 
        //
        // Standard Time Zone Registry Data
        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        // HKLM 
        //     Software 
        //         Microsoft 
        //             Windows NT 
        //                 CurrentVersion 
        //                     Time Zones 
        //                         <time_zone_name>
        // * STD,         REG_SZ "Standard Time Name" 
        //                       (For OS installed zones, this will always be English)
        // * MUI_STD,     REG_SZ "@tzres.dll,-1234" 
        //                       Indirect string to localized resource for Standard Time,
        //                       add "%windir%\system32\" after "@"
        // * DLT,         REG_SZ "Daylight Time Name"
        //                       (For OS installed zones, this will always be English)
        // * MUI_DLT,     REG_SZ "@tzres.dll,-1234"
        //                       Indirect string to localized resource for Daylight Time,
        //                       add "%windir%\system32\" after "@"
        // * Display,     REG_SZ "Display Name like (GMT-8:00) Pacific Time..."
        // * MUI_Display, REG_SZ "@tzres.dll,-1234"
        //                       Indirect string to localized resource for the Display,
        //                       add "%windir%\system32\" after "@"
        // * TZI,         REG_BINARY REG_TZI_FORMAT
        //                       See Win32Native.RegistryTimeZoneInformation
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        static private TimeZoneInfoResult TryGetTimeZoneByRegistryKey(string id, out TimeZoneInfo value, out Exception e) {
            e = null;

            try {
                PermissionSet permSet = new PermissionSet(PermissionState.None);
                permSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, c_timeZonesRegistryHivePermissionList));
                permSet.Assert();

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                                  String.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                                      c_timeZonesRegistryHive, id),
#if FEATURE_MACL
                                  RegistryKeyPermissionCheck.Default,
                                  System.Security.AccessControl.RegistryRights.ReadKey
#else
                                  false
#endif
                                  )) {

                    if (key == null) {
                        value = null;
                        return TimeZoneInfoResult.TimeZoneNotFoundException;
                    }

                    Win32Native.RegistryTimeZoneInformation defaultTimeZoneInformation;
                    Byte[] regValue = key.GetValue(c_timeZoneInfoValue, null, RegistryValueOptions.None) as Byte[];
                    if (regValue == null || regValue.Length != c_regByteLength) {
                        // the registry value could not be cast to a byte array
                        value = null;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }
                    defaultTimeZoneInformation = new Win32Native.RegistryTimeZoneInformation(regValue);

                    AdjustmentRule[] adjustmentRules;  
                    if (!TryCreateAdjustmentRules(id, defaultTimeZoneInformation, out adjustmentRules, out e, defaultTimeZoneInformation.Bias)) {
                        value = null;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }

                    string displayName;
                    string standardName;
                    string daylightName;

                    if (!TryGetLocalizedNamesByRegistryKey(key, out displayName, out standardName, out daylightName)) {
                        value = null;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }

                    try {
                        value = new TimeZoneInfo(
                            id,
                            new TimeSpan(0, -(defaultTimeZoneInformation.Bias), 0),
                            displayName,
                            standardName,
                            daylightName,
                            adjustmentRules,
                            false);

                        return TimeZoneInfoResult.Success;
                    }
                    catch (ArgumentException ex) {
                        // TimeZoneInfo constructor can throw ArgumentException and InvalidTimeZoneException
                        value = null;
                        e = ex;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }
                    catch (InvalidTimeZoneException ex) {
                        // TimeZoneInfo constructor can throw ArgumentException and InvalidTimeZoneException
                        value = null;
                        e = ex;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }

                }
            } 
            finally {
                PermissionSet.RevertAssert();
            }
        }
#endif // FEATURE_WIN32_REGISTRY


#if FEATURE_WIN32_REGISTRY
        //
        // TryGetTimeZone -
        //
        // Helper function for retrieving a TimeZoneInfo object by <time_zone_name>.
        //
        // This function may return null.
        //
        // assumes cachedData lock is taken
        //
        static private TimeZoneInfoResult TryGetTimeZone(string id, Boolean dstDisabled, out TimeZoneInfo value, out Exception e, CachedData cachedData) {
            TimeZoneInfoResult result = TimeZoneInfoResult.Success;
            e = null;
            TimeZoneInfo match = null;

            // check the cache
            if (cachedData.m_systemTimeZones != null) {
                if (cachedData.m_systemTimeZones.TryGetValue(id, out match)) {
                    if (dstDisabled && match.m_supportsDaylightSavingTime) {
                        // we found a cache hit but we want a time zone without DST and this one has DST data
                        value = CreateCustomTimeZone(match.m_id, match.m_baseUtcOffset, match.m_displayName, match.m_standardDisplayName);
                    }
                    else {
                        value = new TimeZoneInfo(match.m_id, match.m_baseUtcOffset, match.m_displayName, match.m_standardDisplayName,
                                              match.m_daylightDisplayName, match.m_adjustmentRules, false);
                    }
                    return result;
                }
            }

            // fall back to reading from the local machine 
            // when the cache is not fully populated               
            if (!cachedData.m_allSystemTimeZonesRead) {
                result = TryGetTimeZoneByRegistryKey(id, out match, out e);
                if (result == TimeZoneInfoResult.Success) {
                    if (cachedData.m_systemTimeZones == null)
                        cachedData.m_systemTimeZones = new Dictionary<string, TimeZoneInfo>();

                    cachedData.m_systemTimeZones.Add(id, match);

                    if (dstDisabled && match.m_supportsDaylightSavingTime) {
                        // we found a cache hit but we want a time zone without DST and this one has DST data
                        value = CreateCustomTimeZone(match.m_id, match.m_baseUtcOffset, match.m_displayName, match.m_standardDisplayName);
                    }
                    else {
                        value = new TimeZoneInfo(match.m_id, match.m_baseUtcOffset, match.m_displayName, match.m_standardDisplayName,
                                              match.m_daylightDisplayName, match.m_adjustmentRules, false);
                    }
                }
                else {
                    value = null;
                }
            }
            else {
                result = TimeZoneInfoResult.TimeZoneNotFoundException;
                value = null;
            }

            return result;
        }
#endif // FEATURE_WIN32_REGISTRY

#endif
        //
        // UtcOffsetOutOfRange -
        //
        // Helper function that validates the TimeSpan is within +/- 14.0 hours
        //
        [Pure]
        static internal Boolean UtcOffsetOutOfRange(TimeSpan offset) {
            return (offset.TotalHours < -14.0 || offset.TotalHours > 14.0);
        }

#if MONO
        //
        // ValidateTimeZoneInfo -
        //
        // Helper function that performs all of the validation checks for the 
        // factory methods and deserialization callback
        //
        // returns a Boolean indicating whether the AdjustmentRule[] supports DST
        //
        static private void ValidateTimeZoneInfo(
                String id,
                TimeSpan baseUtcOffset,
                AdjustmentRule [] adjustmentRules,
                out Boolean adjustmentRulesSupportDst) {

            if (id == null) {
                throw new ArgumentNullException("id");
            }

            if (id.Length == 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidId", id), "id");
            }

            if (UtcOffsetOutOfRange(baseUtcOffset)) {

                throw new ArgumentOutOfRangeException("baseUtcOffset", Environment.GetResourceString("ArgumentOutOfRange_UtcOffset"));
            }

            if (baseUtcOffset.Ticks % TimeSpan.TicksPerMinute != 0) {
                throw new ArgumentException(Environment.GetResourceString("Argument_TimeSpanHasSeconds"), "baseUtcOffset");
            }
            Contract.EndContractBlock();

            adjustmentRulesSupportDst = false;

            //
            // "adjustmentRules" can either be null or a valid array of AdjustmentRule objects.
            // A valid array is one that does not contain any null elements and all elements
            // are sorted in chronological order
            //

            if (adjustmentRules != null && adjustmentRules.Length != 0) {
                adjustmentRulesSupportDst = true;
                AdjustmentRule prev = null;
                AdjustmentRule current = null;
                for (int i = 0; i < adjustmentRules.Length; i++) {
                    prev = current;
                    current = adjustmentRules[i];

                    if (current == null) {
                        throw new InvalidTimeZoneException(Environment.GetResourceString("Argument_AdjustmentRulesNoNulls"));
                    }

                    // 



                    if (UtcOffsetOutOfRange(baseUtcOffset + current.DaylightDelta)) {
                        throw new InvalidTimeZoneException(Environment.GetResourceString("ArgumentOutOfRange_UtcOffsetAndDaylightDelta"));
                    }                       


                    if (prev != null && current.DateStart <= prev.DateEnd) {
                        // verify the rules are in chronological order and the DateStart/DateEnd do not overlap
                        throw new InvalidTimeZoneException(Environment.GetResourceString("Argument_AdjustmentRulesOutOfOrder"));
                    }
                }
            }
        }
#endif
/*============================================================
**
** Class: TimeZoneInfo.AdjustmentRule
**
**
** Purpose: 
** This class is used to represent a Dynamic TimeZone.  It
** has methods for converting a DateTime to UTC from local time
** and to local time from UTC and methods for getting the 
** standard name and daylight name of the time zone.  
**
**
============================================================*/
        [Serializable]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#if MOBILE
        [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#else
        [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#endif
        sealed public class AdjustmentRule : IEquatable<AdjustmentRule>, ISerializable, IDeserializationCallback {

            // ---- SECTION:  members supporting exposed properties -------------*
            private DateTime m_dateStart;
            private DateTime m_dateEnd;
            private TimeSpan m_daylightDelta;
            private TransitionTime m_daylightTransitionStart;
            private TransitionTime m_daylightTransitionEnd;
            private TimeSpan m_baseUtcOffsetDelta;   // delta from the default Utc offset (utcOffset = defaultUtcOffset + m_baseUtcOffsetDelta)


            // ---- SECTION: public properties --------------*
            public DateTime  DateStart {
                get { 
                    return this.m_dateStart;
                }
            }

            public DateTime  DateEnd {
                get { 
                    return this.m_dateEnd;
                }
            } 

            public TimeSpan DaylightDelta {
                get { 
                    return this.m_daylightDelta;
                }
            }        


            public TransitionTime DaylightTransitionStart {
                get {
                    return this.m_daylightTransitionStart;
                }
            }


            public TransitionTime DaylightTransitionEnd {
                get {
                    return this.m_daylightTransitionEnd;
                }
            }

            internal TimeSpan BaseUtcOffsetDelta {
                get {
                    return this.m_baseUtcOffsetDelta;
                }
            }

            internal bool HasDaylightSaving {
                get {
                    return this.DaylightDelta != TimeSpan.Zero ||
                            this.DaylightTransitionStart.TimeOfDay != DateTime.MinValue ||
                            this.DaylightTransitionEnd.TimeOfDay != DateTime.MinValue.AddMilliseconds(1);
                }
            }

            // ---- SECTION: public methods --------------*

            // IEquatable<AdjustmentRule>
            public bool Equals(AdjustmentRule other) {
                bool equals = (other != null
                     && this.m_dateStart == other.m_dateStart
                     && this.m_dateEnd  == other.m_dateEnd
                     && this.m_daylightDelta == other.m_daylightDelta
                     && this.m_baseUtcOffsetDelta == other.m_baseUtcOffsetDelta);

                equals = equals && this.m_daylightTransitionEnd.Equals(other.m_daylightTransitionEnd)
                         && this.m_daylightTransitionStart.Equals(other.m_daylightTransitionStart);

                return equals;
            }


            public override int GetHashCode() {
                return m_dateStart.GetHashCode();
            }



            // -------- SECTION: constructors -----------------*

            private AdjustmentRule() { }


            // -------- SECTION: factory methods -----------------*

            static public AdjustmentRule CreateAdjustmentRule( 
                             DateTime dateStart,
                             DateTime dateEnd,
                             TimeSpan daylightDelta,
                             TransitionTime daylightTransitionStart,
                             TransitionTime daylightTransitionEnd) {

                ValidateAdjustmentRule(dateStart, dateEnd, daylightDelta,
                                       daylightTransitionStart, daylightTransitionEnd);

                AdjustmentRule rule = new AdjustmentRule();

                rule.m_dateStart = dateStart;
                rule.m_dateEnd   = dateEnd;
                rule.m_daylightDelta = daylightDelta;
                rule.m_daylightTransitionStart = daylightTransitionStart;
                rule.m_daylightTransitionEnd = daylightTransitionEnd;
                rule.m_baseUtcOffsetDelta = TimeSpan.Zero;

                return rule;
            }

            static internal AdjustmentRule CreateAdjustmentRule(
                             DateTime dateStart,
                             DateTime dateEnd,
                             TimeSpan daylightDelta,
                             TransitionTime daylightTransitionStart,
                             TransitionTime daylightTransitionEnd,
                             TimeSpan baseUtcOffsetDelta) {
                AdjustmentRule rule = CreateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
                rule.m_baseUtcOffsetDelta = baseUtcOffsetDelta;
                return rule;
            }
 
            // ----- SECTION: internal utility methods ----------------*

            //
            // When Windows sets the daylight transition start Jan 1st at 12:00 AM, it means the year starts with the daylight saving on. 
            // We have to special case this value and not adjust it when checking if any date is in the daylight saving period. 
            //
            internal bool IsStartDateMarkerForBeginningOfYear() {
                return DaylightTransitionStart.Month == 1 && DaylightTransitionStart.Day == 1 && DaylightTransitionStart.TimeOfDay.Hour == 0 && 
                       DaylightTransitionStart.TimeOfDay.Minute == 0 && DaylightTransitionStart.TimeOfDay.Second == 0 &&
                       m_dateStart.Year == m_dateEnd.Year;
            }

            //
            // When Windows sets the daylight transition end Jan 1st at 12:00 AM, it means the year ends with the daylight saving on. 
            // We have to special case this value and not adjust it when checking if any date is in the daylight saving period. 
            //
            internal bool IsEndDateMarkerForEndOfYear() {
                return DaylightTransitionEnd.Month == 1 && DaylightTransitionEnd.Day == 1 && DaylightTransitionEnd.TimeOfDay.Hour == 0 &&
                       DaylightTransitionEnd.TimeOfDay.Minute == 0 && DaylightTransitionEnd.TimeOfDay.Second == 0 &&
                       m_dateStart.Year == m_dateEnd.Year;
            }

            //
            // ValidateAdjustmentRule -
            //
            // Helper function that performs all of the validation checks for the 
            // factory methods and deserialization callback
            //
            static private void ValidateAdjustmentRule( 
                             DateTime dateStart,
                             DateTime dateEnd,
                             TimeSpan daylightDelta,
                             TransitionTime daylightTransitionStart,
                             TransitionTime daylightTransitionEnd) {


                if (dateStart.Kind != DateTimeKind.Unspecified) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "dateStart");
                }

                if (dateEnd.Kind != DateTimeKind.Unspecified) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "dateEnd");
                }

                if (daylightTransitionStart.Equals(daylightTransitionEnd)) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_TransitionTimesAreIdentical"),
                                                "daylightTransitionEnd");
                }


                if (dateStart > dateEnd) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_OutOfOrderDateTimes"), "dateStart");
                }

                if (TimeZoneInfo.UtcOffsetOutOfRange(daylightDelta)) {
                    throw new ArgumentOutOfRangeException("daylightDelta", daylightDelta,
                        Environment.GetResourceString("ArgumentOutOfRange_UtcOffset"));
                }

                if (daylightDelta.Ticks % TimeSpan.TicksPerMinute != 0) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_TimeSpanHasSeconds"),
                        "daylightDelta");
                }

                if (dateStart.TimeOfDay != TimeSpan.Zero) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTimeOfDay"),
                        "dateStart");
                }

                if (dateEnd.TimeOfDay != TimeSpan.Zero) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTimeOfDay"),
                        "dateEnd");
                }
                Contract.EndContractBlock();
            }



            // ----- SECTION: private serialization instance methods  ----------------*

#if FEATURE_SERIALIZATION
            void IDeserializationCallback.OnDeserialization(Object sender) {
                // OnDeserialization is called after each instance of this class is deserialized.
                // This callback method performs AdjustmentRule validation after being deserialized.

                try {
                    ValidateAdjustmentRule(m_dateStart, m_dateEnd, m_daylightDelta,
                                           m_daylightTransitionStart, m_daylightTransitionEnd);
                }
                catch (ArgumentException e) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                }
            }

            [System.Security.SecurityCritical]  // auto-generated_required
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
                if (info == null) {
                    throw new ArgumentNullException("info");
                }
                Contract.EndContractBlock();
 
                info.AddValue("DateStart",                  m_dateStart);
                info.AddValue("DateEnd",                    m_dateEnd);
                info.AddValue("DaylightDelta",              m_daylightDelta);
                info.AddValue("DaylightTransitionStart",    m_daylightTransitionStart);
                info.AddValue("DaylightTransitionEnd",      m_daylightTransitionEnd);
                info.AddValue("BaseUtcOffsetDelta",         m_baseUtcOffsetDelta);
            }

            AdjustmentRule(SerializationInfo info, StreamingContext context) {
                if (info == null) {
                    throw new ArgumentNullException("info");
                }

                m_dateStart           = (DateTime)info.GetValue("DateStart", typeof(DateTime));
                m_dateEnd             = (DateTime)info.GetValue("DateEnd", typeof(DateTime));
                m_daylightDelta       = (TimeSpan)info.GetValue("DaylightDelta", typeof(TimeSpan));
                m_daylightTransitionStart = (TransitionTime)info.GetValue("DaylightTransitionStart", typeof(TransitionTime));
                m_daylightTransitionEnd   = (TransitionTime)info.GetValue("DaylightTransitionEnd", typeof(TransitionTime));

                object o = info.GetValueNoThrow("BaseUtcOffsetDelta", typeof(TimeSpan));
                if (o != null) {
                    m_baseUtcOffsetDelta = (TimeSpan) o;
                }
            }
#endif
        }
       

/*============================================================
**
** Class: TimeZoneInfo.TransitionTime
**
**
** Purpose: 
** This class is used to represent a Dynamic TimeZone.  It
** has methods for converting a DateTime to UTC from local time
** and to local time from UTC and methods for getting the 
** standard name and daylight name of the time zone.  
**
**
============================================================*/
        [Serializable]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#if MOBILE
        [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#else
        [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#endif
        public readonly struct TransitionTime : IEquatable<TransitionTime>, ISerializable, IDeserializationCallback {

            // ---- SECTION:  members supporting exposed properties -------------*
            private readonly DateTime m_timeOfDay;
            private readonly byte m_month;
            private readonly byte m_week;
            private readonly byte m_day;
            private readonly DayOfWeek m_dayOfWeek;
            private readonly Boolean m_isFixedDateRule;

            internal TransitionTime(bool isFixedDateRule, DateTime timeOfDay, DayOfWeek dayOfWeek, byte day, byte week, byte month)
            {
                m_isFixedDateRule = isFixedDateRule;
                m_timeOfDay = timeOfDay;
                m_dayOfWeek = dayOfWeek;
                m_day = (byte)day;
                m_week = (byte)week;
                m_month = (byte)month;
            }

            // ---- SECTION: public properties --------------*
            public DateTime TimeOfDay {
                get {
                    return m_timeOfDay;
                }
            }

            public Int32 Month {
                get {
                    return (int)m_month;
                }
            }


            public Int32 Week {
                get {
                    return (int)m_week;
                }
            }

            public Int32 Day {
                get {
                    return (int)m_day;
                }
            }

            public DayOfWeek DayOfWeek {
                get {
                    return m_dayOfWeek;
                }
            }

            public Boolean IsFixedDateRule {
                get {
                    return m_isFixedDateRule;
                }
            }

            // ---- SECTION: public methods --------------*
            [Pure]
            public override bool Equals(Object obj) {
                if (obj is TransitionTime) {
                    return Equals((TransitionTime)obj);
                }
                return false;
            }

            public static bool operator ==(TransitionTime t1, TransitionTime t2) {
                return t1.Equals(t2);
            }

            public static bool operator !=(TransitionTime t1, TransitionTime t2) {
                return (!t1.Equals(t2));
            }

            [Pure]
            public bool Equals(TransitionTime other) {

                bool equal = (this.m_isFixedDateRule == other.m_isFixedDateRule
                             && this.m_timeOfDay == other.m_timeOfDay
                             && this.m_month == other.m_month);

                if (equal) {
                    if (other.m_isFixedDateRule) {
                        equal = (this.m_day == other.m_day);
                    }
                    else {
                        equal = (this.m_week == other.m_week
                            && this.m_dayOfWeek == other.m_dayOfWeek);
                    }
                }
                return equal;
            }


            public override int GetHashCode() {
                return ((int)m_month ^ (int)m_week << 8);
            }


            // -------- SECTION: constructors -----------------*
/*
            private TransitionTime() {           
                m_timeOfDay = new DateTime();
                m_month = 0;
                m_week  = 0;
                m_day   = 0;
                m_dayOfWeek = DayOfWeek.Sunday;
                m_isFixedDateRule = false;
            }
*/


            // -------- SECTION: factory methods -----------------*


            static public TransitionTime CreateFixedDateRule(
                    DateTime timeOfDay,
                    Int32 month,
                    Int32 day) {

                return CreateTransitionTime(timeOfDay, month, 1, day, DayOfWeek.Sunday, true);
            }


            static public TransitionTime CreateFloatingDateRule(
                    DateTime timeOfDay,
                    Int32 month,
                    Int32 week,
                    DayOfWeek dayOfWeek) {

                return CreateTransitionTime(timeOfDay, month, week, 1, dayOfWeek, false);
            }


            static private TransitionTime CreateTransitionTime(
                    DateTime timeOfDay,
                    Int32 month,
                    Int32 week,
                    Int32 day,
                    DayOfWeek dayOfWeek,
                    Boolean isFixedDateRule) {

                ValidateTransitionTime(timeOfDay, month, week, day, dayOfWeek);
                
                TransitionTime t = new TransitionTime(
                    isFixedDateRule,
                    timeOfDay,
                    dayOfWeek,
                    (byte)day,
                    (byte)week,
                    (byte)month);

                return t;
            }


            // ----- SECTION: internal utility methods ----------------*

            //
            // ValidateTransitionTime -
            //
            // Helper function that validates a TransitionTime instance
            //
            static private void ValidateTransitionTime(
                    DateTime timeOfDay,
                    Int32 month,
                    Int32 week,
                    Int32 day,
                    DayOfWeek dayOfWeek) { 

                if (timeOfDay.Kind != DateTimeKind.Unspecified) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "timeOfDay");
                }

                // Month range 1-12
                if (month < 1 || month > 12) {
                    throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_MonthParam"));
                }

                // Day range 1-31
                if (day < 1 || day > 31) {
                    throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("ArgumentOutOfRange_DayParam"));
                }

                // Week range 1-5
                if (week < 1 || week > 5) {
                    throw new ArgumentOutOfRangeException("week", Environment.GetResourceString("ArgumentOutOfRange_Week"));
                }

                // DayOfWeek range 0-6
                if ((int)dayOfWeek < 0 || (int)dayOfWeek > 6) {
                    throw new ArgumentOutOfRangeException("dayOfWeek", Environment.GetResourceString("ArgumentOutOfRange_DayOfWeek"));
                }
                Contract.EndContractBlock();

                if (timeOfDay.Year != 1 || timeOfDay.Month != 1 
                || timeOfDay.Day != 1 || (timeOfDay.Ticks % TimeSpan.TicksPerMillisecond != 0)) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTicks"), "timeOfDay");
                }
            }

#if FEATURE_SERIALIZATION
            void IDeserializationCallback.OnDeserialization(Object sender) {
                // OnDeserialization is called after each instance of this class is deserialized.
                // This callback method performs TransitionTime validation after being deserialized.

                try {
                    ValidateTransitionTime(m_timeOfDay, (Int32)m_month, (Int32)m_week, (Int32)m_day, m_dayOfWeek);
                }
                catch (ArgumentException e) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                }
            }


            [System.Security.SecurityCritical]  // auto-generated_required
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
                if (info == null) {
                    throw new ArgumentNullException("info");
                }
                Contract.EndContractBlock();
 
                info.AddValue("TimeOfDay",       m_timeOfDay);
                info.AddValue("Month",           m_month);
                info.AddValue("Week",            m_week);
                info.AddValue("Day",             m_day);
                info.AddValue("DayOfWeek",       m_dayOfWeek);
                info.AddValue("IsFixedDateRule", m_isFixedDateRule);
            }

            TransitionTime(SerializationInfo info, StreamingContext context) {
                if (info == null) {
                    throw new ArgumentNullException("info");
                }

                m_timeOfDay       = (DateTime)info.GetValue("TimeOfDay", typeof(DateTime));
                m_month           = (byte)info.GetValue("Month", typeof(byte));
                m_week            = (byte)info.GetValue("Week", typeof(byte));
                m_day             = (byte)info.GetValue("Day", typeof(byte));
                m_dayOfWeek       = (DayOfWeek)info.GetValue("DayOfWeek", typeof(DayOfWeek));
                m_isFixedDateRule = (Boolean)info.GetValue("IsFixedDateRule", typeof(Boolean));
            }
#endif
        }


/*============================================================
**
** Class: TimeZoneInfo.StringSerializer
**
**
** Purpose: 
** This class is used to serialize and deserialize TimeZoneInfo
** objects based on the custom string serialization format
**
**
============================================================*/
        sealed private class StringSerializer {

            // ---- SECTION: private members  -------------*
            private enum State {
                Escaped      = 0,
                NotEscaped   = 1,
                StartOfToken = 2,
                EndOfLine    = 3
            }

            private String m_serializedText;
            private int m_currentTokenStartIndex;
            private State m_state;

            // the majority of the strings contained in the OS time zones fit in 64 chars
            private const int initialCapacityForString = 64;
            private const char esc = '\\';
            private const char sep = ';';
            private const char lhs = '[';
            private const char rhs = ']';
            private const string escString = "\\";
            private const string sepString = ";";
            private const string lhsString = "[";
            private const string rhsString = "]";
            private const string escapedEsc = "\\\\";
            private const string escapedSep = "\\;";
            private const string escapedLhs = "\\[";
            private const string escapedRhs = "\\]";
            private const string dateTimeFormat  = "MM:dd:yyyy";
            private const string timeOfDayFormat = "HH:mm:ss.FFF";


            // ---- SECTION: public static methods --------------*

            //
            // GetSerializedString -
            //
            // static method that creates the custom serialized string
            // representation of a TimeZoneInfo instance
            //
            static public String GetSerializedString(TimeZoneInfo zone) {
                StringBuilder serializedText = StringBuilderCache.Acquire();

                //
                // <m_id>;<m_baseUtcOffset>;<m_displayName>;<m_standardDisplayName>;<m_daylightDispayName>
                //
                serializedText.Append(SerializeSubstitute(zone.Id));
                serializedText.Append(sep);
                serializedText.Append(SerializeSubstitute(
                           zone.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture)));
                serializedText.Append(sep);
                serializedText.Append(SerializeSubstitute(zone.DisplayName));
                serializedText.Append(sep);
                serializedText.Append(SerializeSubstitute(zone.StandardName));
                serializedText.Append(sep);
                serializedText.Append(SerializeSubstitute(zone.DaylightName));
                serializedText.Append(sep);

                AdjustmentRule[] rules = zone.GetAdjustmentRules();

                if (rules != null && rules.Length > 0) {  
                    for (int i = 0; i < rules.Length; i++) {
                        AdjustmentRule rule = rules[i];

                        serializedText.Append(lhs);
                        serializedText.Append(SerializeSubstitute(rule.DateStart.ToString(
                                                dateTimeFormat, DateTimeFormatInfo.InvariantInfo)));
                        serializedText.Append(sep);
                        serializedText.Append(SerializeSubstitute(rule.DateEnd.ToString(
                                                dateTimeFormat, DateTimeFormatInfo.InvariantInfo)));
                        serializedText.Append(sep);
                        serializedText.Append(SerializeSubstitute(rule.DaylightDelta.TotalMinutes.ToString(CultureInfo.InvariantCulture)));
                        serializedText.Append(sep);
                        // serialize the TransitionTime's
                        SerializeTransitionTime(rule.DaylightTransitionStart, serializedText);
                        serializedText.Append(sep);
                        SerializeTransitionTime(rule.DaylightTransitionEnd, serializedText);
                        serializedText.Append(sep);
                        if (rule.BaseUtcOffsetDelta != TimeSpan.Zero) { // Serialize it only when BaseUtcOffsetDelta has a value to reduce the impact of adding rule.BaseUtcOffsetDelta
                            serializedText.Append(SerializeSubstitute(rule.BaseUtcOffsetDelta.TotalMinutes.ToString(CultureInfo.InvariantCulture)));
                            serializedText.Append(sep);
                        }
                        serializedText.Append(rhs);
                    }
                }
                serializedText.Append(sep);
                return StringBuilderCache.GetStringAndRelease(serializedText);
            }


            //
            // GetDeserializedTimeZoneInfo -
            //
            // static method that instantiates a TimeZoneInfo from a custom serialized
            // string
            //
            static public TimeZoneInfo GetDeserializedTimeZoneInfo(String source) {
                StringSerializer s = new StringSerializer(source);

                String id              = s.GetNextStringValue(false);
                TimeSpan baseUtcOffset = s.GetNextTimeSpanValue(false);
                String displayName     = s.GetNextStringValue(false);
                String standardName    = s.GetNextStringValue(false);
                String daylightName    = s.GetNextStringValue(false);
                AdjustmentRule[] rules = s.GetNextAdjustmentRuleArrayValue(false);

                try { 
                    return TimeZoneInfo.CreateCustomTimeZone(id, baseUtcOffset, displayName, standardName, daylightName, rules);
                }
                catch (ArgumentException ex) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), ex);
                }
                catch (InvalidTimeZoneException ex) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), ex);
                }
            }

            // ---- SECTION: public instance methods --------------*


            // -------- SECTION: constructors -----------------*

            //
            // StringSerializer -
            //
            // private constructor - used by GetDeserializedTimeZoneInfo()
            //
            private StringSerializer(String str) {
                m_serializedText = str;
                m_state          = State.StartOfToken;
            }



            // ----- SECTION: internal static utility methods ----------------*

            //
            // SerializeSubstitute -
            //
            // returns a new string with all of the reserved sub-strings escaped
            //
            // ";" -> "\;"
            // "[" -> "\["
            // "]" -> "\]"
            // "\" -> "\\"
            //
            static private String SerializeSubstitute(String text) {
                text = text.Replace(escString, escapedEsc);
                text = text.Replace(lhsString, escapedLhs);
                text = text.Replace(rhsString, escapedRhs);
                return text.Replace(sepString, escapedSep);
            }


            //
            // SerializeTransitionTime -
            //
            // Helper method to serialize a TimeZoneInfo.TransitionTime object
            //
            static private void SerializeTransitionTime(TransitionTime time, StringBuilder serializedText) {
                serializedText.Append(lhs);
                Int32 fixedDate = (time.IsFixedDateRule ? 1 : 0);
                serializedText.Append(fixedDate.ToString(CultureInfo.InvariantCulture));
                serializedText.Append(sep);

                if (time.IsFixedDateRule) {
                    serializedText.Append(SerializeSubstitute(time.TimeOfDay.ToString(timeOfDayFormat, DateTimeFormatInfo.InvariantInfo)));
                    serializedText.Append(sep);
                    serializedText.Append(SerializeSubstitute(time.Month.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(sep);
                    serializedText.Append(SerializeSubstitute(time.Day.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(sep);
                }
                else {
                    serializedText.Append(SerializeSubstitute(time.TimeOfDay.ToString(timeOfDayFormat, DateTimeFormatInfo.InvariantInfo)));
                    serializedText.Append(sep);
                    serializedText.Append(SerializeSubstitute(time.Month.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(sep);
                    serializedText.Append(SerializeSubstitute(time.Week.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(sep);
                    serializedText.Append(SerializeSubstitute(((int)time.DayOfWeek).ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(sep);
                }
                serializedText.Append(rhs);
            }

            //
            // VerifyIsEscapableCharacter -
            //
            // Helper function to determine if the passed in string token is allowed to be preceeded by an escape sequence token
            //
            static private void VerifyIsEscapableCharacter(char c) {
               if (c != esc && c != sep && c != lhs && c != rhs) {
                   throw new SerializationException(Environment.GetResourceString("Serialization_InvalidEscapeSequence", c));
               }
            }

            // ----- SECTION: internal instance utility methods ----------------*

            //
            // SkipVersionNextDataFields -
            //
            // Helper function that reads past "v.Next" data fields.  Receives a "depth" parameter indicating the
            // current relative nested bracket depth that m_currentTokenStartIndex is at.  The function ends
            // successfully when "depth" returns to zero (0).
            //
            //
            private void SkipVersionNextDataFields(Int32 depth /* starting depth in the nested brackets ('[', ']')*/) {
                if (m_currentTokenStartIndex < 0 || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                State tokenState = State.NotEscaped;

                // walk the serialized text, building up the token as we go...
                for (int i = m_currentTokenStartIndex; i < m_serializedText.Length; i++) {
                    if (tokenState == State.Escaped) {
                        VerifyIsEscapableCharacter(m_serializedText[i]);
                        tokenState = State.NotEscaped;
                    }
                    else if (tokenState == State.NotEscaped) {
                        switch (m_serializedText[i]) {
                            case esc:
                                tokenState = State.Escaped;
                                break;

                            case lhs:
                                depth++;
                                break;
                            case rhs:
                                depth--;
                                if (depth == 0) {
                                    m_currentTokenStartIndex = i + 1;
                                    if (m_currentTokenStartIndex >= m_serializedText.Length) {
                                        m_state = State.EndOfLine;
                                    }
                                    else {
                                        m_state = State.StartOfToken;
                                    }
                                    return;
                                }
                                break;

                            case '\0':
                                // invalid character
                                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));

                            default:
                                break;
                        }
                    }
                }

                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
            }


            //
            // GetNextStringValue -
            //
            // Helper function that reads a string token from the serialized text.  The function
            // updates the m_currentTokenStartIndex to point to the next token on exit.  Also m_state
            // is set to either State.StartOfToken or State.EndOfLine on exit.
            //
            // The function takes a parameter "canEndWithoutSeparator".  
            //
            // * When set to 'false' the function requires the string token end with a ";".
            // * When set to 'true' the function requires that the string token end with either
            //   ";", State.EndOfLine, or "]".  In the case that "]" is the terminal case the
            //   m_currentTokenStartIndex is left pointing at index "]" to allow the caller to update
            //   its depth logic.
            //
            private String GetNextStringValue(Boolean canEndWithoutSeparator) {

                // first verify the internal state of the object
                if (m_state == State.EndOfLine) {
                    if (canEndWithoutSeparator) {
                        return null;
                    }
                    else {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                }
                if (m_currentTokenStartIndex < 0 || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                State tokenState = State.NotEscaped;
                StringBuilder token = StringBuilderCache.Acquire(initialCapacityForString);

                // walk the serialized text, building up the token as we go...
                for (int i = m_currentTokenStartIndex; i < m_serializedText.Length; i++) {
                    if (tokenState == State.Escaped) {
                        VerifyIsEscapableCharacter(m_serializedText[i]);
                        token.Append(m_serializedText[i]);
                        tokenState = State.NotEscaped;
                    }
                    else if (tokenState == State.NotEscaped) {
                        switch (m_serializedText[i]) {
                            case esc:
                                tokenState = State.Escaped;
                                break;

                            case lhs:
                                // '[' is an unexpected character
                                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));

                            case rhs:
                                if (canEndWithoutSeparator) {
                                    // if ';' is not a required terminal then treat ']' as a terminal
                                    // leave m_currentTokenStartIndex pointing to ']' so our callers can handle
                                    // this special case
                                    m_currentTokenStartIndex = i;
                                    m_state = State.StartOfToken;
                                    return token.ToString();
                                }
                                else {
                                    // ']' is an unexpected character
                                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                                }

                            case sep:
                                m_currentTokenStartIndex = i + 1;
                                if (m_currentTokenStartIndex >= m_serializedText.Length) {
                                    m_state = State.EndOfLine;
                                }
                                else {
                                    m_state = State.StartOfToken;
                                }
                                return StringBuilderCache.GetStringAndRelease(token);
                            
                            case '\0':
                                // invalid character
                                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));

                            default:
                                token.Append(m_serializedText[i]);
                                break;
                        }
                    }
                }
                //
                // we are at the end of the line
                //
                if (tokenState == State.Escaped) {
                   // we are at the end of the serialized text but we are in an escaped state
                   throw new SerializationException(Environment.GetResourceString("Serialization_InvalidEscapeSequence", String.Empty));
                }
                
                if (!canEndWithoutSeparator) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                m_currentTokenStartIndex = m_serializedText.Length;
                m_state = State.EndOfLine;
                return StringBuilderCache.GetStringAndRelease(token);
            }

            //
            // GetNextDateTimeValue -
            //
            // Helper function to read a DateTime token.  Takes a boolean "canEndWithoutSeparator"
            // and a "format" string.
            //
            private DateTime GetNextDateTimeValue(Boolean canEndWithoutSeparator, string format) {
                String token = GetNextStringValue(canEndWithoutSeparator);
                DateTime time;
                if (!DateTime.TryParseExact(token, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out time)) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                return time;
            }

            //
            // GetNextTimeSpanValue -
            //
            // Helper function to read a DateTime token.  Takes a boolean "canEndWithoutSeparator".
            //
            private TimeSpan GetNextTimeSpanValue(Boolean canEndWithoutSeparator) {
                Int32 token = GetNextInt32Value(canEndWithoutSeparator);
                
                try {
                    return new TimeSpan(0 /* hours */, token /* minutes */, 0 /* seconds */);
                }
                catch (ArgumentOutOfRangeException e) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                }
            }


            //
            // GetNextInt32Value -
            //
            // Helper function to read an Int32 token.  Takes a boolean "canEndWithoutSeparator".
            //
            private Int32 GetNextInt32Value(Boolean canEndWithoutSeparator) {
                String token = GetNextStringValue(canEndWithoutSeparator);
                Int32 value;
                if (!Int32.TryParse(token, NumberStyles.AllowLeadingSign /* "[sign]digits" */, CultureInfo.InvariantCulture, out value)) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                return value;
            }


            //
            // GetNextAdjustmentRuleArrayValue -
            //
            // Helper function to read an AdjustmentRule[] token.  Takes a boolean "canEndWithoutSeparator".
            //
            private AdjustmentRule[] GetNextAdjustmentRuleArrayValue(Boolean canEndWithoutSeparator) {
                List<AdjustmentRule> rules = new List<AdjustmentRule>(1);
                int count = 0;

                // individual AdjustmentRule array elements do not require semicolons
                AdjustmentRule rule = GetNextAdjustmentRuleValue(true);
                while (rule != null) {
                    rules.Add(rule);
                    count++;

                    rule = GetNextAdjustmentRuleValue(true);
                }

                if (!canEndWithoutSeparator) {
                    // the AdjustmentRule array must end with a separator
                    if (m_state == State.EndOfLine) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                    if (m_currentTokenStartIndex < 0 || m_currentTokenStartIndex >= m_serializedText.Length) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                }

                return (count != 0 ? rules.ToArray() : null);
            }

            //
            // GetNextAdjustmentRuleValue -
            //
            // Helper function to read an AdjustmentRule token.  Takes a boolean "canEndWithoutSeparator".
            //
            private AdjustmentRule GetNextAdjustmentRuleValue(Boolean canEndWithoutSeparator) {            
                // first verify the internal state of the object
                if (m_state == State.EndOfLine) {
                    if (canEndWithoutSeparator) {
                        return null;
                    }
                    else {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                }

                if (m_currentTokenStartIndex < 0 || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                // check to see if the very first token we see is the separator
                if (m_serializedText[m_currentTokenStartIndex] == sep) {
                    return null;
                }

                // verify the current token is a left-hand-side marker ("[")
                if (m_serializedText[m_currentTokenStartIndex] != lhs) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                m_currentTokenStartIndex++;

                DateTime dateStart           = GetNextDateTimeValue(false, dateTimeFormat);
                DateTime dateEnd             = GetNextDateTimeValue(false, dateTimeFormat);
                TimeSpan daylightDelta       = GetNextTimeSpanValue(false);
                TransitionTime daylightStart = GetNextTransitionTimeValue(false);
                TransitionTime daylightEnd   = GetNextTransitionTimeValue(false);
                TimeSpan baseUtcOffsetDelta  = TimeSpan.Zero;
                // verify that the string is now at the right-hand-side marker ("]") ...

                if (m_state == State.EndOfLine || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                // Check if we have baseUtcOffsetDelta in the serialized string and then deserialize it
                if ((m_serializedText[m_currentTokenStartIndex] >= '0' && m_serializedText[m_currentTokenStartIndex] <= '9') ||
                    m_serializedText[m_currentTokenStartIndex] == '-' || m_serializedText[m_currentTokenStartIndex] == '+') {
                    baseUtcOffsetDelta = GetNextTimeSpanValue(false);
                }

                if (m_state == State.EndOfLine || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                if (m_serializedText[m_currentTokenStartIndex] != rhs) {
                    // skip ahead of any "v.Next" data at the end of the AdjustmentRule
                    //
                    // 


                    SkipVersionNextDataFields(1);
                }
                else {
                    m_currentTokenStartIndex++;
                }

                // create the AdjustmentRule from the deserialized fields ...

                AdjustmentRule rule;
                try {
                    rule = AdjustmentRule.CreateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightStart, daylightEnd, baseUtcOffsetDelta);
                }
                catch (ArgumentException e) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                }

                // finally set the state to either EndOfLine or StartOfToken for the next caller
                if (m_currentTokenStartIndex >= m_serializedText.Length) {
                    m_state = State.EndOfLine;
                }
                else {
                    m_state = State.StartOfToken;
                }
                return rule;
            }       


            //
            // GetNextTransitionTimeValue -
            //
            // Helper function to read a TransitionTime token.  Takes a boolean "canEndWithoutSeparator".
            //
            private TransitionTime GetNextTransitionTimeValue(Boolean canEndWithoutSeparator) {

                // first verify the internal state of the object

                if (m_state == State.EndOfLine
                    || (m_currentTokenStartIndex < m_serializedText.Length
                        && m_serializedText[m_currentTokenStartIndex] == rhs)) {
                    //
                    // we are at the end of the line or we are starting at a "]" character
                    //
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                if (m_currentTokenStartIndex < 0 || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                // verify the current token is a left-hand-side marker ("[")

                if (m_serializedText[m_currentTokenStartIndex] != lhs) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                m_currentTokenStartIndex++;

                Int32 isFixedDate   = GetNextInt32Value(false);

                if (isFixedDate != 0 && isFixedDate != 1) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                TransitionTime transition;

                DateTime timeOfDay  = GetNextDateTimeValue(false, timeOfDayFormat);
                timeOfDay = new DateTime(1, 1, 1, timeOfDay.Hour,timeOfDay.Minute,timeOfDay.Second, timeOfDay.Millisecond);

                Int32 month         = GetNextInt32Value(false);

                if (isFixedDate == 1) {
                    Int32 day       = GetNextInt32Value(false);

                    try {
                        transition = TransitionTime.CreateFixedDateRule(timeOfDay, month, day);
                    }
                    catch (ArgumentException e) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                    }
                }
                else {
                    Int32 week      = GetNextInt32Value(false);
                    Int32 dayOfWeek = GetNextInt32Value(false);

                    try {
                        transition = TransitionTime.CreateFloatingDateRule(timeOfDay, month, week, (DayOfWeek)dayOfWeek);
                    }
                    catch (ArgumentException e) {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), e);
                    }

                }

                // verify that the string is now at the right-hand-side marker ("]") ...

                if (m_state == State.EndOfLine || m_currentTokenStartIndex >= m_serializedText.Length) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }

                if (m_serializedText[m_currentTokenStartIndex] != rhs) {
                    // skip ahead of any "v.Next" data at the end of the AdjustmentRule
                    //
                    // 


                    SkipVersionNextDataFields(1);
                }
                else {
                    m_currentTokenStartIndex++;
                }

                // check to see if the string is now at the separator (";") ...
                Boolean sepFound = false;
                if (m_currentTokenStartIndex < m_serializedText.Length
                    && m_serializedText[m_currentTokenStartIndex] == sep) {
                    // handle the case where we ended on a ";"
                    m_currentTokenStartIndex++;
                    sepFound = true;
                }

                if (!sepFound && !canEndWithoutSeparator) {
                    // we MUST end on a separator
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }


                // finally set the state to either EndOfLine or StartOfToken for the next caller
                if (m_currentTokenStartIndex >= m_serializedText.Length) {
                    m_state = State.EndOfLine;
                }
                else {
                    m_state = State.StartOfToken;
                }
                return transition;
            }
        }

        private class TimeZoneInfoComparer : System.Collections.Generic.IComparer<TimeZoneInfo> {
            int System.Collections.Generic.IComparer<TimeZoneInfo>.Compare(TimeZoneInfo x, TimeZoneInfo y)  {
                // sort by BaseUtcOffset first and by DisplayName second - this is similar to the Windows Date/Time control panel
                int comparison = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
                return comparison == 0 ? String.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal) : comparison;
            }
        }
    } // TimezoneInfo
} // namespace System
