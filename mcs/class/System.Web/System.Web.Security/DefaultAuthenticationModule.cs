//
// System.Web.Security.DefaultAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;
using System.Security.Principal;

namespace System.Web.Security
{
	public sealed class DefaultAuthenticationModule : IHttpModule
	{
		static GenericIdentity defaultIdentity = new GenericIdentity ("", "");

		public event DefaultAuthenticationEventHandler Authenticate;

		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.DefaultAuthentication += new EventHandler (OnDefaultAuthentication);
		}

		void OnDefaultAuthentication (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;

			if (context.User == null && Authenticate != null)
				Authenticate (this, new DefaultAuthenticationEventArgs (context));

			if (context.User == null)
				context.User = new GenericPrincipal (defaultIdentity, new string [0]);
		}
	}
}

