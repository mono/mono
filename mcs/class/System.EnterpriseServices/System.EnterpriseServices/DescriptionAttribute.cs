// 
// System.EnterpriseServices.DescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface)]
	public sealed class DescriptionAttribute : Attribute {

		#region Fields

		string desc;

		#endregion // Fields

		#region Constructors

		public DescriptionAttribute (string desc)
		{
			this.desc = desc;
		}

		#endregion // Constructors
	}
}
