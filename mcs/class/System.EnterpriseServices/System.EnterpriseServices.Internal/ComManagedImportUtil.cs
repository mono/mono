// System.EnterpriseServices.Internal.ComManagedImportUtil.cs
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
	[Guid("3b0398c9-7812-4007-85cb-18c771f2206f")]
	public class ComManagedImportUtil : IComManagedImportUtil {

		[MonoTODO]
		public ComManagedImportUtil ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GetComponentInfo (string assemblyPath, out string numComponents, out string componentInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void InstallAssembly (string asmpath, string parname, string appname)
		{
			throw new NotImplementedException ();
		}
	}
#endif
}
