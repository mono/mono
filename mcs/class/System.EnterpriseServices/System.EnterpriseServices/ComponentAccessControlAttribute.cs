// 
// System.EnterpriseServices.ComponentAccessControlAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ComponentAccessControlAttribute : Attribute {

		#region Fields

		bool val;

		#endregion // Fields

		#region Constructors

		public ComponentAccessControlAttribute ()
		{
			this.val = false;
		}

		public ComponentAccessControlAttribute (bool val)
		{
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public bool Value {
			get { return val; }
			set { val = value; }
		}

		#endregion // Properties
	}
}
