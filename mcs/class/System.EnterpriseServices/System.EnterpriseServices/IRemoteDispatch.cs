// 
// System.EnterpriseServices.IRemoteDispatch.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

	[Guid("6619a740-8154-43be-a186-0319578e02db")]
	public interface IRemoteDispatch {

		#region Methods

		[AutoComplete]
		string RemoteDispatchAutoDone (string s);

		[AutoComplete]
		string RemoteDispatchNotAutoDone (string s);

		#endregion
	}
}
