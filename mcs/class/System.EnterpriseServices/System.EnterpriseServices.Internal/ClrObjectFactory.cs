// System.EnterpriseServices.Internal.ClrObjectFactory.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("ecabafd1-7f19-11d2-978e-0000f8757e2a")]
	public class ClrObjectFactory : IClrObjectFactory {

		[MonoTODO]
		public ClrObjectFactory ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object CreateFromAssembly (string AssemblyName, string TypeName, string Mode)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual object CreateFromMailbox (string Mailbox, string Mode)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual object CreateFromVroot (string VrootUrl, string Mode)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual object CreateFromWsdl (string WsdlUrl, string Mode)
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
