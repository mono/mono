// 
// System.EnterpriseServices.IAsyncErrorNotify.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Guid("FE6777FB-A674-4177-8F32-6D707E113484")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAsyncErrorNotify {

		#region Methods

		void OnError (int hresult);

		#endregion
	}
#endif
}
