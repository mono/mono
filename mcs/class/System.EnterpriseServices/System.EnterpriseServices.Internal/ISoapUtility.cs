// System.EnterpriseServices.Internal.ISoapUtility.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("5AC4CB7E-F89F-429b-926B-C7F940936BF4")]
	public interface ISoapUtility {
		[DispId(2)]
		void GetServerBinPath (string rootWebServer, string inBaseUrl, string inVirtualRoot, out string binPath);
		[DispId(1)]
		void GetServerPhysicalPath (string rootWebServer, string inBaseUrl, string inVirtualRoot, out string physicalPath);
		[DispId(3)]
		void Present ();
	}
#endif
}
