// 
// System.EnterpriseServices.InterfaceQueuingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class InterfaceQueuingAttribute : Attribute {

		#region Fields

		bool enabled;
		string interfaceName;

		#endregion // Fields

		#region Constructors

		public InterfaceQueuingAttribute () 
			: this (true)
		{
		}

		public InterfaceQueuingAttribute (bool enabled)
		{
			this.enabled = enabled;
			interfaceName = null;
		}

		#endregion // Constructors

		#region Properties

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public string Interface {
			get { return interfaceName; }
			set { interfaceName = value; }
		}

		#endregion // Properties
	}
}
