// System.EnterpriseServices.Internal.IClrObjectFactory.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//
using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
	//[Guid("")]
	public interface IClrObjectFactory
	{
		object CreateFromAssembly(string assembly, string type, string mode);

		object CreateFromMailbox(string Mailbox, string Mode);

		object CreateFromVroot(string VrootUrl, string Mode);

		object CreateFromWsdl(string WsdlUrl, string Mode);
	}
}
