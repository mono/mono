// 
// System.EnterpriseServices.IISIntrinsicsAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class IISIntrinsicsAttribute : Attribute {

		#region Fields

		bool val;

		#endregion // Fields

		#region Constructors

		public IISIntrinsicsAttribute ()
		{
			val = true;
		}

		public IISIntrinsicsAttribute (bool val)
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
