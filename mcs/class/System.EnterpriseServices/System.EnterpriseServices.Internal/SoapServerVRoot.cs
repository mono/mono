// System.EnterpriseServices.Internal.SoapServerVRoot.cs
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
	[Guid("CAA817CC-0C04-4d22-A05C-2B7E162F4E8F")]
	public sealed class SoapServerVRoot : ISoapServerVRoot {

		[MonoTODO]
		public SoapServerVRoot ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CreateVirtualRootEx (string rootWebServer, string inBaseUrl, string inVirtualRoot, string homePage, string discoFile, string secureSockets, string authentication, string operation, out string baseUrl, out string virtualRoot, out string physicalPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteVirtualRootEx (string rootWebServer, string inBaseUrl, string inVirtualRoot)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public void GetVirtualRootStatus (string RootWebServer, string inBaseUrl, string inVirtualRoot, out string Exists, out string SSL, out string WindowsAuth, out string Anonymous, out string HomePage, out string DiscoFile, out string PhysicalPath, out string BaseUrl, out string VirtualRoot)
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
