//
// System.Diagnostics.CounterCreationData.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	[Serializable]
	public class CounterCreationData {

		private string help;
		private string name;
		private PerformanceCounterType type;

		public CounterCreationData ()
		{
		}

		public CounterCreationData (string counterName, 
			string counterHelp, 
			PerformanceCounterType counterType)
		{
			name = counterName;
			help = counterHelp;
			type = counterType;
		}

		public string CounterHelp {
			get {return help;}
			set {help = value;}
		}

		public string CounterName {
			get {return name;}
			set {name = value;}
		}

		// may throw InvalidEnumArgumentException
		public PerformanceCounterType CounterType {
			get {return type;}
			set {type = value;}
		}
	}
}

