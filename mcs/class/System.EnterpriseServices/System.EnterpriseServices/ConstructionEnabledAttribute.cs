// 
// System.EnterpriseServices.ConstructionEnabledAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ConstructionEnabledAttribute : Attribute {

		#region Fields

		string def;
		bool val;

		#endregion // Fields

		#region Constructors

		public ConstructionEnabledAttribute ()
		{
			def = String.Empty;
			this.val = false;
		}

		public ConstructionEnabledAttribute (bool val)
		{
			def = String.Empty;
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public string Default {
			get { return def; }
			set { def = value; }
		}

		public bool Value {
			get { return val; }
			set { val = value; }
		}

		#endregion // Properties
	}
}
