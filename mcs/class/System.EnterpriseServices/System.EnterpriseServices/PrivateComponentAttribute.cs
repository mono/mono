// 
// System.EnterpriseServices.PrivateComponentAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class PrivateComponentAttribute : Attribute {

		#region Constructors

		public PrivateComponentAttribute () 
		{
		}

		#endregion // Constructors
	}
}
