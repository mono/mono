// System.EnterpriseServices.Internal.SoapClientImport.cs
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
	[Guid("346D5B9F-45E1-45c0-AADF-1B7D221E9063")]
	public sealed class SoapClientImport : ISoapClientImport {

		[MonoTODO]
		public SoapClientImport ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ProcessClientTlbEx (string progId, string virtualRoot, string baseUrl, string authentication, string assemblyName, string typeName)
		{
			throw new NotImplementedException ();
		}


	}
#endif
}
