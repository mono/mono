// System.EnterpriseServices.Internal.IServerWebConfig.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("6261e4b5-572a-4142-a2f9-1fe1a0c97097")]
	public interface IServerWebConfig {
		[DispId(1)]
		void AddElement (
			[MarshalAs(UnmanagedType.BStr)] string FilePath,
			[MarshalAs(UnmanagedType.BStr)] string AssemblyName,
			[MarshalAs(UnmanagedType.BStr)] string TypeName,
			[MarshalAs(UnmanagedType.BStr)] string ProgId,
			[MarshalAs(UnmanagedType.BStr)] string Mode,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(2)]
		void Create (
			[MarshalAs(UnmanagedType.BStr)] string FilePath,
			[MarshalAs(UnmanagedType.BStr)] string FileRootName,
			[MarshalAs(UnmanagedType.BStr)] out string Error);
	}
#endif
}
