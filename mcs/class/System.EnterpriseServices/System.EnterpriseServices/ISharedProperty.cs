// 
// System.EnterpriseServices.SharedProperty.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	internal interface ISharedProperty {

		#region Properties

		object Value {
			get;
			set;
		}

		#endregion
	}
}
