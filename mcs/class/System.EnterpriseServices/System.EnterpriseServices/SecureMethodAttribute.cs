// 
// System.EnterpriseServices.SecureMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class SecureMethodAttribute : Attribute {

		#region Constructors

		public SecureMethodAttribute ()
		{
		}

		#endregion // Constructors
	}
}
