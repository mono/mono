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

using System;

namespace System.Diagnostics {

	public sealed class CounterSampleCalculator {

		private CounterSampleCalculator ()
		{
		}

		public static float ComputeCounterValue (CounterSample newSample)
		{
			return ComputeCounterValue (CounterSample.Empty, newSample);
		}

		[MonoTODO("What's the algorithm?")]
		public static float ComputeCounterValue (CounterSample oldSample,
			CounterSample newSample)
		{
			throw new NotImplementedException ();
		}
	}
}

