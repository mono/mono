// 
// System.EnterpriseServices.IProcessInitControl.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Guid("72380d55-8d2b-43a3-8513-2b6ef31434e9")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IProcessInitControl {

		#region Methods

		void ResetInitializerTimeout (int dwSecondsRemaining);

		#endregion
	}
#endif
}
