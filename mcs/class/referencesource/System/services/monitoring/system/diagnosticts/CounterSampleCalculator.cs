//------------------------------------------------------------------------------
// <copyright file="CounterSampleCalculator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Threading;    
    using System;    
    using System.ComponentModel;
    using Microsoft.Win32;
    using System.Text;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///     Set of utility functions for interpreting the counter data
    ///     NOTE: most of this code was taken and ported from counters.c (PerfMon source code)
    /// </devdoc>
    public static class CounterSampleCalculator {
        static volatile bool perfCounterDllLoaded = false;
        
        /// <devdoc>
        ///    Converts 100NS elapsed time to fractional seconds
        /// </devdoc>
        /// <internalonly/>
        private static float GetElapsedTime(CounterSample oldSample, CounterSample newSample) {
            float eSeconds;
            float eDifference;

            if (newSample.RawValue == 0) {
                // no data [start time = 0] so return 0
                return 0.0f;
            } 
            else {
                float eFreq;
                eFreq = (float)(ulong)oldSample.CounterFrequency;

                if (oldSample.UnsignedRawValue >= (ulong)newSample.CounterTimeStamp || eFreq <= 0.0f)
                    return 0.0f;
                    
                // otherwise compute difference between current time and start time
                eDifference = (float)((ulong)newSample.CounterTimeStamp - oldSample.UnsignedRawValue);
            
                // convert to fractional seconds using object counter
                eSeconds = eDifference / eFreq;

                return eSeconds;
            }           
        }

        /// <devdoc>
        ///    Computes the calculated value given a raw counter sample.
        /// </devdoc>
        public static float ComputeCounterValue(CounterSample newSample) {
            return ComputeCounterValue(CounterSample.Empty, newSample);
        }

        /// <devdoc>
        ///    Computes the calculated value given a raw counter sample.
        /// </devdoc>
        public static float ComputeCounterValue(CounterSample oldSample, CounterSample newSample) {
            int newCounterType = (int) newSample.CounterType;
            if (oldSample.SystemFrequency == 0) {
                if ((newCounterType != NativeMethods.PERF_RAW_FRACTION) &&
                    (newCounterType != NativeMethods.PERF_COUNTER_RAWCOUNT) &&
                    (newCounterType != NativeMethods.PERF_COUNTER_RAWCOUNT_HEX) &&
                    (newCounterType != NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT) &&
                    (newCounterType != NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT_HEX) &&
                    (newCounterType != NativeMethods.PERF_COUNTER_MULTI_BASE)) {

                    // Since oldSample has a system frequency of 0, this means the newSample is the first sample
                    // on a two sample calculation.  Since we can't do anything with it, return 0.
                    return 0.0f;
                }
            }
            else if (oldSample.CounterType != newSample.CounterType) {
                throw new InvalidOperationException(SR.GetString(SR.MismatchedCounterTypes));
            }

            if (newCounterType == NativeMethods.PERF_ELAPSED_TIME) 
                return (float)GetElapsedTime(oldSample, newSample);
            
            NativeMethods.PDH_RAW_COUNTER newPdhValue = new NativeMethods.PDH_RAW_COUNTER();
            NativeMethods.PDH_RAW_COUNTER oldPdhValue = new NativeMethods.PDH_RAW_COUNTER();

            FillInValues(oldSample, newSample, oldPdhValue, newPdhValue);

            LoadPerfCounterDll();

            NativeMethods.PDH_FMT_COUNTERVALUE pdhFormattedValue= new NativeMethods.PDH_FMT_COUNTERVALUE();
            long timeBase = newSample.SystemFrequency;
            int result = SafeNativeMethods.FormatFromRawValue((uint) newCounterType, NativeMethods.PDH_FMT_DOUBLE | NativeMethods.PDH_FMT_NOSCALE | NativeMethods.PDH_FMT_NOCAP100, 
                                                          ref timeBase, newPdhValue, oldPdhValue, pdhFormattedValue);
            
            if (result != NativeMethods.ERROR_SUCCESS) {
                // If the numbers go negative, just return 0.  This better matches the old behavior. 
                if (result == NativeMethods.PDH_CALC_NEGATIVE_VALUE || result == NativeMethods.PDH_CALC_NEGATIVE_DENOMINATOR || result == NativeMethods.PDH_NO_DATA)
                    return 0;
                else
                    throw new Win32Exception(result, SR.GetString(SR.PerfCounterPdhError, result.ToString("x", CultureInfo.InvariantCulture)));
            }
            
            return (float) pdhFormattedValue.data;
            
        }


        // This method figures out which values are supposed to go into which structures so that PDH can do the 
        // calculation for us.  This was ported from Window's cutils.c
        private static void FillInValues(CounterSample oldSample, CounterSample newSample, NativeMethods.PDH_RAW_COUNTER oldPdhValue, NativeMethods.PDH_RAW_COUNTER newPdhValue) {
            int newCounterType = (int) newSample.CounterType;

            switch (newCounterType) {
                case NativeMethods.PERF_COUNTER_COUNTER:
                case NativeMethods.PERF_COUNTER_QUEUELEN_TYPE:
                case NativeMethods.PERF_SAMPLE_COUNTER:
                case NativeMethods.PERF_OBJ_TIME_TIMER:
                case NativeMethods.PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE:
                    newPdhValue.FirstValue  = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp;

                    oldPdhValue.FirstValue  = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp;
                    break;
                
                case NativeMethods.PERF_COUNTER_100NS_QUEUELEN_TYPE:
                    newPdhValue.FirstValue  = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp100nSec;

                    oldPdhValue.FirstValue  = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
                    break;
                
                case NativeMethods.PERF_COUNTER_TIMER:
                case NativeMethods.PERF_COUNTER_TIMER_INV:
                case NativeMethods.PERF_COUNTER_BULK_COUNT:
                case NativeMethods.PERF_COUNTER_LARGE_QUEUELEN_TYPE:
                case NativeMethods.PERF_COUNTER_MULTI_TIMER:
                case NativeMethods.PERF_COUNTER_MULTI_TIMER_INV:
                    newPdhValue.FirstValue  = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp;

                    oldPdhValue.FirstValue  = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp;
                    if (newCounterType == NativeMethods.PERF_COUNTER_MULTI_TIMER || newCounterType == NativeMethods.PERF_COUNTER_MULTI_TIMER_INV) {
                        //  this is to make PDH work like PERFMON for
                        //  this counter type
                        newPdhValue.FirstValue *= (uint) newSample.CounterFrequency;
                        if (oldSample.CounterFrequency != 0) {
                            oldPdhValue.FirstValue *= (uint) oldSample.CounterFrequency;
                        }
                    }

                    if ((newCounterType & NativeMethods.PERF_MULTI_COUNTER) == NativeMethods.PERF_MULTI_COUNTER) {
                            newPdhValue.MultiCount = (int) newSample.BaseValue;
                            oldPdhValue.MultiCount = (int) oldSample.BaseValue;
                    }
                    
                        
                    break;
                //
                //  These counters do not use any time reference
                //
                case NativeMethods.PERF_COUNTER_RAWCOUNT:
                case NativeMethods.PERF_COUNTER_RAWCOUNT_HEX:
                case NativeMethods.PERF_COUNTER_DELTA:
                case NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT:
                case NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT_HEX:
                case NativeMethods.PERF_COUNTER_LARGE_DELTA:
                    newPdhValue.FirstValue  = newSample.RawValue;
                    newPdhValue.SecondValue = 0;

                    oldPdhValue.FirstValue  = oldSample.RawValue;
                    oldPdhValue.SecondValue = 0;
                    break;
                //
                //  These counters use the 100 Ns time base in thier calculation
                //
                case NativeMethods.PERF_100NSEC_TIMER:
                case NativeMethods.PERF_100NSEC_TIMER_INV:
                case NativeMethods.PERF_100NSEC_MULTI_TIMER:
                case NativeMethods.PERF_100NSEC_MULTI_TIMER_INV:
                    newPdhValue.FirstValue  = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.TimeStamp100nSec;

                    oldPdhValue.FirstValue  = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
                    if ((newCounterType & NativeMethods.PERF_MULTI_COUNTER) == NativeMethods.PERF_MULTI_COUNTER) {
                        newPdhValue.MultiCount = (int) newSample.BaseValue;
                        oldPdhValue.MultiCount = (int) oldSample.BaseValue;
                    }
                    break;
                //
                //  These counters use two data points
                //
                case NativeMethods.PERF_SAMPLE_FRACTION:
                case NativeMethods.PERF_RAW_FRACTION:
                case NativeMethods.PERF_LARGE_RAW_FRACTION:
                case NativeMethods.PERF_PRECISION_SYSTEM_TIMER:
                case NativeMethods.PERF_PRECISION_100NS_TIMER:
                case NativeMethods.PERF_PRECISION_OBJECT_TIMER:
                case NativeMethods.PERF_AVERAGE_TIMER:
                case NativeMethods.PERF_AVERAGE_BULK:
                    newPdhValue.FirstValue = newSample.RawValue;
                    newPdhValue.SecondValue = newSample.BaseValue;

                    oldPdhValue.FirstValue = oldSample.RawValue;
                    oldPdhValue.SecondValue = oldSample.BaseValue;
                    break;
                
                default:
                    // an unidentified counter was returned so
                    newPdhValue.FirstValue  = 0;
                    newPdhValue.SecondValue = 0;

                    oldPdhValue.FirstValue  = 0;
                    oldPdhValue.SecondValue = 0;
                    break;
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void LoadPerfCounterDll() {
            if (perfCounterDllLoaded)
                return;

            new FileIOPermission(PermissionState.Unrestricted).Assert();

            string installPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string perfcounterPath = Path.Combine(installPath, "perfcounter.dll");
            if (SafeNativeMethods.LoadLibrary(perfcounterPath) == IntPtr.Zero) {
                throw new Win32Exception( Marshal.GetLastWin32Error() );
            }

            perfCounterDllLoaded = true;
        }           
    }
}

