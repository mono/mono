// 
// System.EnterpriseServices.PrivateComponentAttribute.cs
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
	public sealed class PrivateComponentAttribute : Attribute {

		#region Constructors

		public PrivateComponentAttribute () 
		{
		}

		#endregion // Constructors
	}
}
