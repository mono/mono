// 
// System.EnterpriseServices.SynchronizationAttribute.cs
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
	public sealed class SynchronizationAttribute : Attribute {

		#region Fields

		SynchronizationOption val;

		#endregion // Fields

		#region Constructors

		public SynchronizationAttribute ()
			: this (SynchronizationOption.Required)
		{
		}

		public SynchronizationAttribute (SynchronizationOption val)
		{
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public SynchronizationOption Value {
			get { return val; }
		}

		#endregion // Properties
	}
}
