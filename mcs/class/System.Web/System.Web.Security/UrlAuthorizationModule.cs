//
// System.Web.Security.UrlAuthorizationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Web;
using System.Web.Configuration;
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

		public void Init (HttpApplication app)
		{
			app.AuthorizeRequest += new EventHandler (OnAuthorizeRequest);
		}

		void OnAuthorizeRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.SkipAuthorization)
				return;

			AuthorizationConfig config = (AuthorizationConfig) context.GetConfig ("system.web/authorization");
			if (config == null)
				return;

			if (!config.IsValidUser (context.User, context.Request.HttpMethod)) {
				HttpException e =  new HttpException (401, "Forbidden");
				
				context.Response.StatusCode = 401;
				context.Response.Write (e.GetHtmlErrorMessage ());
			}
		}
	}
}

