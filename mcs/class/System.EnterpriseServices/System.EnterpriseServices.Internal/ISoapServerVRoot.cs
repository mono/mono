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
		void CreateVirtualRootEx (string rootWebServer, string inBaseUrl, string inVirtualRoot, string homePage, string discoFile, string secureSockets, string authentication, string operation, out string baseUrl, out string virtualRoot, out string physicalPath);
		[DispId(2)]
		void DeleteVirtualRootEx (string rootWebServer, string baseUrl, string virtualRoot);
		[DispId(3)]
		void GetVirtualRootStatus (string rootWebServer, string inBaseUrl, string inVirtualRoot, out string exists, out string secureSockets, out string windowsAuth, out string anonymous, out string homePage, out string discoFile, out string physicalPath, out string baseUrl, out string virtualRoot);
	}
#endif
}
