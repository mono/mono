// 
// System.EnterpriseServices.ISecurityIdentityColl.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	internal interface ISecurityIdentityColl {

		#region Properties

		int Count {
			get;
		}

		#endregion // Properties

		#region Methods

		void GetEnumerator (out IEnumerator enumerator);
		SecurityIdentity GetItem (int idx);

		#endregion // Methods
	}
}
