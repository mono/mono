// 
// System.EnterpriseServices.COMTIIntrinsicsAttribute.cs
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
	public sealed class COMTIIntrinsicsAttribute : Attribute {

		#region Fields

		bool val;

		#endregion // Fields

		#region Constructors

		public COMTIIntrinsicsAttribute ()
		{
			this.val = false;
		}

		public COMTIIntrinsicsAttribute (bool val)
		{
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public bool Value {
			get { return val; }
		}

		#endregion // Properties
	}
}
