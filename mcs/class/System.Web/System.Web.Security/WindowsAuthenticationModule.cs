//
// System.Web.Security.WindowsAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Web;

namespace System.Web.Security
{
	public sealed class WindowsAuthenticationModule : IHttpModule
	{
		public event WindowsAuthenticationEventHandler Authenticate;

		public WindowsAuthenticationModule ()
		{
		}

		public void Dispose ()
		{
		}

		[MonoTODO]
		public void Init (HttpApplication app)
		{
			throw new NotImplementedException ();
		}
	}
}

