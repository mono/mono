//
// System.Diagnostics.CounterSampleCalculator.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
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

namespace System.Diagnostics {

	public static class CounterSampleCalculator {

		public static float ComputeCounterValue (CounterSample newSample)
		{
			switch (newSample.CounterType) {
			case PerformanceCounterType.RawFraction:
			case PerformanceCounterType.NumberOfItems32:
			case PerformanceCounterType.NumberOfItemsHEX32:
			case PerformanceCounterType.NumberOfItems64:
			case PerformanceCounterType.NumberOfItemsHEX64:
				return (float)newSample.RawValue;
			default:
				return 0;
			}
		}

		[MonoTODO("What's the algorithm?")]
		public static float ComputeCounterValue (CounterSample oldSample,
			CounterSample newSample)
		{
			if (newSample.CounterType != oldSample.CounterType)
				throw new Exception ("The counter samples must be of the same type");
			switch (newSample.CounterType) {
			case PerformanceCounterType.RawFraction:
			case PerformanceCounterType.NumberOfItems32:
			case PerformanceCounterType.NumberOfItemsHEX32:
			case PerformanceCounterType.NumberOfItems64:
			case PerformanceCounterType.NumberOfItemsHEX64:
				return (float)newSample.RawValue;
			case PerformanceCounterType.AverageCount64:
				return (float)(newSample.RawValue - oldSample.RawValue)/(float)(newSample.BaseValue - oldSample.BaseValue);
			case PerformanceCounterType.AverageTimer32:
				return (((float)(newSample.RawValue - oldSample.RawValue))/newSample.SystemFrequency)/(float)(newSample.BaseValue - oldSample.BaseValue);
			case PerformanceCounterType.CounterDelta32:
			case PerformanceCounterType.CounterDelta64:
				return (float)(newSample.RawValue - oldSample.RawValue);
			case PerformanceCounterType.CounterMultiTimer:
				return ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp) * 100.0f/newSample.BaseValue;
			case PerformanceCounterType.CounterMultiTimer100Ns:
				return ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec) * 100.0f/newSample.BaseValue;
			case PerformanceCounterType.CounterMultiTimerInverse:
				return (newSample.BaseValue - ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100.0f;
			case PerformanceCounterType.CounterMultiTimer100NsInverse:
				return (newSample.BaseValue - ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100.0f;
			case PerformanceCounterType.CounterTimer:
			case PerformanceCounterType.CountPerTimeInterval32:
			case PerformanceCounterType.CountPerTimeInterval64:
				return ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp);
			case PerformanceCounterType.CounterTimerInverse:
				return (1.0f - ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100.0f;
			case PerformanceCounterType.ElapsedTime:
				// FIXME
				return 0;
			case PerformanceCounterType.Timer100Ns:
				return ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp) * 100.0f;
			case PerformanceCounterType.Timer100NsInverse:
				return (1f - ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100.0f;
			case PerformanceCounterType.RateOfCountsPerSecond32:
			case PerformanceCounterType.RateOfCountsPerSecond64:
				return ((float)(newSample.RawValue - oldSample.RawValue))/(float)(newSample.TimeStamp - oldSample.TimeStamp) * 10000000;
			default:
				Console.WriteLine ("Counter type {0} not handled", newSample.CounterType);
				return 0;
			}
		}
	}
}

