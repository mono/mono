//
// System.Diagnostics.MonitoringDescriptionAttribute.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Diagnostics {

	[AttributeUsage (AttributeTargets.All)]
	public class MonitoringDescriptionAttribute : DescriptionAttribute {

		public MonitoringDescriptionAttribute (string description)
			: base (description)
		{
		}

		public override string Description {
			get {return base.Description;}
		}
	}
}

