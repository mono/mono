//
// System.Diagnostics.PerformanceCounterType.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.ComponentModel;

namespace System.Diagnostics {

	[Serializable]
	[TypeConverter (typeof (AlphabeticalEnumConverter))]
	public enum PerformanceCounterType {
		NumberOfItemsHEX32=0x00000000,
		NumberOfItemsHEX64=0x00000100,
		NumberOfItems32=0x00010000,
		NumberOfItems64=0x00010100,
		CounterDelta32=0x00400400,
		CounterDelta64=0x00400500,
		SampleCounter=0x00410400,
		CountPerTimeInterval32=0x00450400,
		CountPerTimeInterval64=0x00450500,
		RateOfCountsPerSecond32=0x10410400,
		RateOfCountsPerSecond64=0x10410500,
		RawFraction=0x20020400,
		CounterTimer=0x20410500,
		Timer100Ns=0x20510500,
		SampleFraction=0x20C20400,
		CounterTimerInverse=0x21410500,
		Timer100NsInverse=0x21510500,
		CounterMultiTimer=0x22410500,
		CounterMultiTimer100Ns=0x22510500,
		CounterMultiTimerInverse=0x23410500,
		CounterMultiTimer100NsInverse=0x23510500,
		AverageTimer32=0x30020400,
		ElapsedTime=0x30240500,
		AverageCount64=0x40020500,
		SampleBase=0x40030401,
		AverageBase=0x40030402,
		RawBase=0x40030403,
		CounterMultiBase=0x42030500
	}
}

