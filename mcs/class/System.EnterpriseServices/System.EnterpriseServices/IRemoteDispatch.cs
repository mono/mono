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
	[Guid ("")]
	public interface IRemoteDispatch {

		#region Methods

		string RemoteDispatchAutoDone (string s);
		string RemoteDispatchNotAutoDone (string s);

		#endregion // Methods
	}
}
