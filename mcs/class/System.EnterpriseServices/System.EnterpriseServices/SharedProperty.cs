// 
// System.EnterpriseServices.SharedProperty.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[ComVisible (false)]
	public sealed class SharedProperty {

		#region Fields

		ISharedProperty property;

		#endregion

		#region Constructors

		internal SharedProperty (ISharedProperty property)
		{
			this.property = property;
		}

		#endregion // Constructors

		#region Properties

		public object Value {
			get { return property.Value; }
			set { property.Value = value; }
		}

		#endregion
	}
}
