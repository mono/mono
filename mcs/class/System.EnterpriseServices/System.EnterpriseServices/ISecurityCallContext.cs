// 
// System.EnterpriseServices.ISecurityCallContext.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

namespace System.EnterpriseServices {
	internal interface ISecurityCallContext {

		#region Properties

		int Count {
			get;
		}

		#endregion

		#region Methods

		void GetEnumerator (ref IEnumerator enumerator);
		object GetItem (string user);
		bool IsCallerInRole (string role);
		bool IsSecurityEnabled ();
		bool IsUserInRole (ref object user, string role);

		#endregion
	}
}
