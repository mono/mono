// 
// System.EnterpriseServices.AutoCompleteAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class AutoCompleteAttribute : Attribute {

		#region Fields

		bool val;

		#endregion // Fields

		#region Constructors

		public AutoCompleteAttribute ()
		{
			val = true;
		}

		public AutoCompleteAttribute (bool val)
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
