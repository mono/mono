//------------------------------------------------------------------------------
// <copyright file="PerformanceCounterType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {    
    using System;
    using System.ComponentModel;
    using Microsoft.Win32;


    /// <devdoc>
    ///     Enum of friendly names to counter types (maps directory to the native types)
    /// </devdoc>
    [TypeConverterAttribute(typeof(AlphabeticalEnumConverter))]
    public enum PerformanceCounterType {
        NumberOfItems32         = NativeMethods.PERF_COUNTER_RAWCOUNT,
        NumberOfItems64         = NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT,
        NumberOfItemsHEX32      = NativeMethods.PERF_COUNTER_RAWCOUNT_HEX,
        NumberOfItemsHEX64      = NativeMethods.PERF_COUNTER_LARGE_RAWCOUNT_HEX,
        RateOfCountsPerSecond32 = NativeMethods.PERF_COUNTER_COUNTER,
        RateOfCountsPerSecond64 = NativeMethods.PERF_COUNTER_BULK_COUNT,
        CountPerTimeInterval32  = NativeMethods.PERF_COUNTER_QUEUELEN_TYPE,
        CountPerTimeInterval64  = NativeMethods.PERF_COUNTER_LARGE_QUEUELEN_TYPE,
        RawFraction             = NativeMethods.PERF_RAW_FRACTION,
        RawBase                 = NativeMethods.PERF_RAW_BASE,
        
        AverageTimer32          = NativeMethods.PERF_AVERAGE_TIMER,
        AverageBase             = NativeMethods.PERF_AVERAGE_BASE,
        AverageCount64          = NativeMethods.PERF_AVERAGE_BULK,
        
        SampleFraction          = NativeMethods.PERF_SAMPLE_FRACTION,
        SampleCounter           = NativeMethods.PERF_SAMPLE_COUNTER,
        SampleBase              = NativeMethods.PERF_SAMPLE_BASE,
        
        CounterTimer            = NativeMethods.PERF_COUNTER_TIMER,
        CounterTimerInverse     = NativeMethods.PERF_COUNTER_TIMER_INV,
        Timer100Ns              = NativeMethods.PERF_100NSEC_TIMER,
        Timer100NsInverse       = NativeMethods.PERF_100NSEC_TIMER_INV,
        ElapsedTime             = NativeMethods.PERF_ELAPSED_TIME,
        CounterMultiTimer       = NativeMethods.PERF_COUNTER_MULTI_TIMER,
        CounterMultiTimerInverse= NativeMethods.PERF_COUNTER_MULTI_TIMER_INV,
        CounterMultiTimer100Ns  = NativeMethods.PERF_100NSEC_MULTI_TIMER,
        CounterMultiTimer100NsInverse       = NativeMethods.PERF_100NSEC_MULTI_TIMER_INV,
        CounterMultiBase        = NativeMethods.PERF_COUNTER_MULTI_BASE,

        CounterDelta32          = NativeMethods.PERF_COUNTER_DELTA,
        CounterDelta64          = NativeMethods.PERF_COUNTER_LARGE_DELTA

    }
}

