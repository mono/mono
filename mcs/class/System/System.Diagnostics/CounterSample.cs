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

namespace System.Diagnostics {

	public struct CounterSample {
		
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
	}
}

