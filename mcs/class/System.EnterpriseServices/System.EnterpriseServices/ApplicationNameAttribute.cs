// 
// System.EnterpriseServices.ApplicationNameAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	[ComVisible(false)]
	public sealed class ApplicationNameAttribute : Attribute {

		#region Fields

		string name;

		#endregion // Fields

		#region Constructors

		public ApplicationNameAttribute (string name)
		{
			this.name = name;
		}

		#endregion // Constructors

		#region Properties

		public string Value {	
			get { return name; }
		}

		#endregion // Properties
	}
}
