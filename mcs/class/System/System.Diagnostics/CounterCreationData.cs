//
// System.Diagnostics.CounterCreationData.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Diagnostics {

	[Serializable]
	#if (NET_1_0)
		[TypeConverter ("System.Diagnostics.Design.CounterCreationDataConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	#endif
	#if (NET_1_1)
    		[TypeConverter ("System.Diagnostics.Design.CounterCreationDataConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	#endif
	public class CounterCreationData 
	{

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

		[DefaultValue ("")]
		[MonitoringDescription ("Description of this counter.")]
		public string CounterHelp {
			get {return help;}
			set {help = value;}
		}

		[DefaultValue ("")]
		[MonitoringDescription ("Name of this counter.")]
		#if (NET_1_0)
			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		public string CounterName 
		{
			get {return name;}
			set {name = value;}
		}

		// may throw InvalidEnumArgumentException
		[DefaultValue (typeof (PerformanceCounterType), "NumberOfItems32")]
		[MonitoringDescription ("Type of this counter.")]
		public PerformanceCounterType CounterType {
			get {return type;}
			set {type = value;}
		}
	}
}

