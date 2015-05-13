namespace System.Diagnostics.PerformanceData
{
    using System;

    public enum CounterType
    {
        AverageBase = 0x40030402,
        AverageCount64 = 0x40020500,
        AverageTimer32 = 0x30020400,
        Delta32 = 0x400400,
        Delta64 = 0x400500,
        ElapsedTime = 0x30240500,
        LargeQueueLength = 0x450500,
        MultiTimerBase = 0x42030500,
        MultiTimerPercentageActive = 0x22410500,
        MultiTimerPercentageActive100Ns = 0x22510500,
        MultiTimerPercentageNotActive = 0x23410500,
        MultiTimerPercentageNotActive100Ns = 0x23510500,
        ObjectSpecificTimer = 0x20610500,
        PercentageActive = 0x20410500,
        PercentageActive100Ns = 0x20510500,
        PercentageNotActive = 0x21410500,
        PercentageNotActive100Ns = 0x21510500,
        PrecisionObjectSpecificTimer = 0x20670500,
        PrecisionSystemTimer = 0x20470500,
        PrecisionTimer100Ns = 0x20570500,
        QueueLength = 0x450400,
        QueueLength100Ns = 0x550500,
        QueueLengthObjectTime = 0x650500,
        RateOfCountPerSecond32 = 0x10410400,
        RateOfCountPerSecond64 = 0x10410500,
        RawBase32 = 0x40030403,
        RawBase64 = 0x40030500,
        RawData32 = 0x10000,
        RawData64 = 0x10100,
        RawDataHex32 = 0,
        RawDataHex64 = 0x100,
        RawFraction32 = 0x20020400,
        RawFraction64 = 0x20020500,
        SampleBase = 0x40030401,
        SampleCounter = 0x410400,
        SampleFraction = 0x20c20400
    }
}

