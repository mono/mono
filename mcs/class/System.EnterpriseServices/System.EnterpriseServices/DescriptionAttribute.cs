// 
// System.EnterpriseServices.DescriptionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface)]
	[ComVisible(false)]
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
