// 
// System.EnterpriseServices.ISecurityCallersColl.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	internal interface ISecurityCallersColl {

		#region Properties

		int Count {
			get;
		}

		#endregion

		#region Methods

		void GetEnumerator (out IEnumerator enumerator);
		ISecurityIdentityColl GetItem (int idx);

		#endregion // Methods
	}
}
