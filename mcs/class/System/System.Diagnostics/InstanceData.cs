//
// System.Diagnostics.InstanceData.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	public class InstanceData {

		private string instanceName;
		private CounterSample sample;

		public InstanceData (string instanceName, CounterSample sample)
		{
			this.instanceName = instanceName;
			this.sample = sample;
		}

		public string InstanceName {
			get {return instanceName;}
		}

		public long RawValue {
			get {return sample.RawValue;}
		}

		public CounterSample Sample {
			get {return sample;}
		}
	}
}

