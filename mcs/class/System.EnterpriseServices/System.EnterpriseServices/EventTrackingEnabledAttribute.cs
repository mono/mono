// 
// System.EnterpriseServices.EventTrackingEnabledAttribute.cs
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
	public sealed class EventTrackingEnabledAttribute : Attribute {

		#region Fields

		bool val;

		#endregion // Fields

		#region Constructors

		public EventTrackingEnabledAttribute ()
		{
			val = true;
		}

		public EventTrackingEnabledAttribute (bool val)
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
