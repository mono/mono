//
// System.Web.Security.DefaultAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;

namespace System.Web.Security
{
	public sealed class DefaultAuthenticationModule : IHttpModule
	{
		public event DefaultAuthenticationEventHandler Authenticate;

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

