// System.EnterpriseServices.Internal.ISoapServerTlb.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("1E7BA9F7-21DB-4482-929E-21BDE2DFE51C")]
	public interface ISoapServerTlb {
		[DispId(1)]
		void AddServerTlb (string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string baseUrl, string virtualRoot, string clientActivated, string wellKnown, string discoFile, string operation, out string assemblyName, out string typeName);
		[DispId(2)]
		void DeleteServerTlb (string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string baseUrl, string virtualRoot, string operation, string assemblyName, string typeName);
	}
#endif
}
