// System.EnterpriseServices.Internal.IISVirtualRoot.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("d8013ef1-730b-45e2-ba24-874b7242c425")]
	public class IISVirtualRoot : IComSoapIISVRoot {

		[MonoTODO]
		public IISVirtualRoot ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Create (string RootWeb, string inPhysicalDirectory, string VirtualDirectory, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete (string RootWeb, string PhysicalDirectory, string VirtualDirectory, out string Error)
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
