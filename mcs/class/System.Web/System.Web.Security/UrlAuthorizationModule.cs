//
// System.Web.Security.UrlAuthorizationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Web;
using System.Security.Principal;

namespace System.Web.Security
{
	public sealed class UrlAuthorizationModule : IHttpModule
	{
		public UrlAuthorizationModule ()
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

