// System.EnterpriseServices.Internal.IClrObjectFactory.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) Alejandro Sánchez Acosta
//
using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("ecabafd2-7f19-11d2-978e-0000f8757e2a")]
	public interface IClrObjectFactory
	{
		object CreateFromAssembly (string assembly, string type, string mode);

		object CreateFromMailbox (string Mailbox, string Mode);

		object CreateFromVroot (string VrootUrl, string Mode);

		object CreateFromWsdl (string WsdlUrl, string Mode);
	}
#endif
}
