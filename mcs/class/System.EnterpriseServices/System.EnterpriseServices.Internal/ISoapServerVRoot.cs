// System.EnterpriseServices.Internal.ISoapServerVRoot.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("A31B6577-71D2-4344-AEDF-ADC1B0DC5347")]
	public interface ISoapServerVRoot {
		[DispId(1)]
		void CreateVirtualRootEx (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] string homePage,
			[MarshalAs(UnmanagedType.BStr)] string discoFile,
			[MarshalAs(UnmanagedType.BStr)] string secureSockets,
			[MarshalAs(UnmanagedType.BStr)] string authentication,
			[MarshalAs(UnmanagedType.BStr)] string operation,
			[MarshalAs(UnmanagedType.BStr)] out string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] out string virtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string physicalPath);

		[DispId(2)]
		void DeleteVirtualRootEx (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] string virtualRoot);

		[DispId(3)]
		void GetVirtualRootStatus (
			[MarshalAs(UnmanagedType.BStr)] string rootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string inBaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string inVirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string exists,
			[MarshalAs(UnmanagedType.BStr)] out string secureSockets,
			[MarshalAs(UnmanagedType.BStr)] out string windowsAuth,
			[MarshalAs(UnmanagedType.BStr)] out string anonymous,
			[MarshalAs(UnmanagedType.BStr)] out string homePage,
			[MarshalAs(UnmanagedType.BStr)] out string discoFile,
			[MarshalAs(UnmanagedType.BStr)] out string physicalPath,
			[MarshalAs(UnmanagedType.BStr)] out string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] out string virtualRoot);
	}
#endif
}
