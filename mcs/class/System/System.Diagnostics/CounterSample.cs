//
// System.Diagnostics.CounterSample.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Collections;
using System.Diagnostics;

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

		CounterSample (long rawValue, 
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

		CounterSample (long rawValue, 
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

//		[MonoTODO("What's the algorithm?")]
//		public static float Calculate (CounterSample counterSample)
//		{
//			throw new NotSupportedException ();
//		}
//
//		[MonoTODO("What's the algorithm?")]
//		public static float Calculate (CounterSample counterSample,
//			CounterSample nextCounterSample)
//		{
//			throw new NotSupportedException ();
//		}
	}
}

