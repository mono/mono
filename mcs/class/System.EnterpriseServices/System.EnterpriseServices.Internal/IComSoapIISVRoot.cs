// System.EnterpriseServices.Internal.IComSoapIISVRoot.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("d8013ef0-730b-45e2-ba24-874b7242c425")]
	public interface IComSoapIISVRoot {
		[DispId(1)]
		void Create (string RootWeb, string PhysicalDirectory, string VirtualDirectory, out string Error);
		[DispId(2)]
		void Delete (string RootWeb, string PhysicalDirectory, string VirtualDirectory, out string Error);
	}
#endif
}
