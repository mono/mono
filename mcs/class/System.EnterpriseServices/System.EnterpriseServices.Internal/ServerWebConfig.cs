// System.EnterpriseServices.Internal.ServerWebConfig.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	public class ServerWebConfig : IServerWebConfig {

		[MonoTODO]
		public ServerWebConfig ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddElement (string FilePath, string AssemblyName, string TypeName, string ProgId, string WkoMode, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Create (string FilePath, string FilePrefix, out string Error)
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
