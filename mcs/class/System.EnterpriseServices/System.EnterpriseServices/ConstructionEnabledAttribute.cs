// 
// System.EnterpriseServices.ConstructionEnabledAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	[ComVisible(false)]
	public sealed class ConstructionEnabledAttribute : Attribute {

		#region Fields

		string def;
		bool enabled;

		#endregion // Fields

		#region Constructors

		public ConstructionEnabledAttribute ()
		{
			def = String.Empty;
			enabled = true;
		}

		public ConstructionEnabledAttribute (bool val)
		{
			def = String.Empty;
			enabled = val;
		}

		#endregion // Constructors

		#region Properties

		public string Default {
			get { return def; }
			set { def = value; }
		}

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		#endregion // Properties
	}
}
