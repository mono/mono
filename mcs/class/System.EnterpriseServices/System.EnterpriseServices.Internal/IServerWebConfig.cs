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
		void AddElement (string FilePath, string AssemblyName, string TypeName, string ProgId, string Mode, out string Error);
		[DispId(2)]
		void Create (string FilePath, string FileRootName, out string Error);
	}
#endif
}
