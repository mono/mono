//
// System.Diagnostics.PerformanceCounterType.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

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

using System;
using System.ComponentModel;

namespace System.Diagnostics {
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

