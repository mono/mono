// System.EnterpriseServices.Internal.SoapUtility.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("5F9A955F-AA55-4127-A32B-33496AA8A44E")]
	public sealed class SoapUtility : ISoapUtility {

		[MonoTODO]
		public SoapUtility ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetServerBinPath (string rootWebServer, string inBaseUrl, string inVirtualRoot, out string binPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetServerPhysicalPath (string rootWebServer, string inBaseUrl, string inVirtualRoot, out string physicalPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Present ()
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
