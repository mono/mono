// 
// System.EnterpriseServices.IProcessInitializer.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Guid("1113f52d-dc7f-4943-aed6-88d04027e32a")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IProcessInitializer {

		#region Methods

		void Shutdown ();
		void Startup ([In, MarshalAs(UnmanagedType.IUnknown)] object punkProcessControl);

		#endregion
	}
#endif
}
