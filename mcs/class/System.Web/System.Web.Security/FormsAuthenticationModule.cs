//
// System.Web.Security.FormsAuthenticationModule
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
	public sealed class FormsAuthenticationModule : IHttpModule
	{
		public event FormsAuthenticationEventHandler Authenticate;

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

