//
// System.Diagnostics.CounterSample.cs
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

namespace System.Diagnostics {

	public struct CounterSample {
		
		// do not reorder and keep in sync with the runtime
		// in metadata/mono-perfcounters.c
		private long rawValue;
		private long baseValue;
		private long counterFrequency;
		private long systemFrequency;
		private long timeStamp;
		private long timeStamp100nSec; 
		private long counterTimeStamp;
		private PerformanceCounterType counterType;

		public CounterSample (long rawValue, 
			long baseValue, 
			long counterFrequency, 
			long systemFrequency, 
			long timeStamp, 
			long timeStamp100nSec, 
			PerformanceCounterType counterType)
			: this (rawValue, baseValue, counterFrequency, 
				systemFrequency, timeStamp, timeStamp100nSec, 
				counterType, 0)
		{
		}

		public CounterSample (long rawValue, 
			long baseValue, 
			long counterFrequency, 
			long systemFrequency, 
			long timeStamp, 
			long timeStamp100nSec, 
			PerformanceCounterType counterType, 
			long counterTimeStamp)
		{
			this.rawValue = rawValue;
			this.baseValue = baseValue;
			this.counterFrequency = counterFrequency;
			this.systemFrequency = systemFrequency;
			this.timeStamp = timeStamp;
			this.timeStamp100nSec = timeStamp100nSec;
			this.counterType = counterType;
			this.counterTimeStamp = counterTimeStamp;
		}

		public static CounterSample Empty = new CounterSample (
			0, 0, 0, 0, 0, 0, 
			PerformanceCounterType.NumberOfItems32, 
			0);

		public long BaseValue {
			get {return baseValue;}
		}

		public long CounterFrequency {
			get {return counterFrequency;}
		}

		public long CounterTimeStamp {
			get {return counterTimeStamp;}
		}

		public PerformanceCounterType CounterType {
			get {return counterType;}
		}

		public long RawValue {
			get {return rawValue;}
		}

		public long SystemFrequency {
			get {return systemFrequency;}
		}

		public long TimeStamp {
			get {return timeStamp;}
		}

		public long TimeStamp100nSec {
			get {return timeStamp100nSec;}
		}

		public static float Calculate (CounterSample counterSample)
		{
			return CounterSampleCalculator.ComputeCounterValue (counterSample);
		}

		public static float Calculate (CounterSample counterSample,
			CounterSample nextCounterSample)
		{
			return CounterSampleCalculator.ComputeCounterValue (counterSample, nextCounterSample);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is CounterSample))
				return false;
			return Equals ((CounterSample) obj);
		}

		public bool Equals (CounterSample other)
		{
			return
				rawValue == other.rawValue &&
				baseValue == other.counterFrequency &&
				counterFrequency == other.counterFrequency &&
				systemFrequency == other.systemFrequency &&
				timeStamp == other.timeStamp &&
				timeStamp100nSec == other.timeStamp100nSec &&
				counterTimeStamp == other.counterTimeStamp &&
				counterType == other.counterType;
		}

		public static bool operator == (CounterSample obj1, CounterSample obj2)
		{
			return obj1.Equals (obj2);
		}

		public static bool operator != (CounterSample obj1, CounterSample obj2)
		{
			return !obj1.Equals (obj2);
		}

		public override int GetHashCode ()
		{
			return (int) (rawValue << 28 ^
				(baseValue << 24 ^
				(counterFrequency << 20 ^
				(systemFrequency << 16 ^
				(timeStamp << 8 ^
				(timeStamp100nSec << 4 ^
				(counterTimeStamp ^
				(int) counterType)))))));
		}
	}
}

