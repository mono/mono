//
// System.Web.Security.FormsAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Security
{
	public sealed class FormsAuthenticationModule : IHttpModule
	{
		bool noForms;

		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.AuthenticateRequest += new EventHandler (OnAuthenticateRequest);
			app.EndRequest += new EventHandler (OnEndRequest);
		}

		void OnAuthenticateRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			AuthConfig config = (AuthConfig) context.GetConfig ("system.web/authentication");
			if (config.Mode != AuthenticationMode.Forms) {
				noForms = true;
				return;
			}
				
			string cookieName = config.CookieName;
			string cookiePath = config.CookiePath;
			string loginPage = config.LoginUrl;
			
			FormsAuthenticationEventArgs formArgs = new FormsAuthenticationEventArgs (context);
			if (Authenticate != null)
				Authenticate (this, formArgs);

			bool contextUserNull = (context.User == null);
			if (formArgs.User != null || !contextUserNull) {
				Console.WriteLine ("Aquiiiiiiiii");
				if (contextUserNull) {
					context.User = formArgs.User;
					Console.WriteLine ("Allii {0}", formArgs.User);
				}
				return;
			}
				
			HttpCookie cookie = context.Request.Cookies [cookieName];
			if (cookie == null || cookie.Expires < DateTime.Now) {
				Console.WriteLine ("Allá {0} {1}", (cookie == null), (cookie == null) ? "" : cookie.Expires.ToString ());
				return;
			}

			FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt (cookie.Value);
			FormsAuthentication.RenewTicketIfOld (ticket);
			context.User = new GenericPrincipal (new FormsIdentity (ticket), new string [0]);
			Console.WriteLine ("name: " + ticket.Name);

			cookie.Value = FormsAuthentication.Encrypt (ticket);
			cookie.Path = cookiePath;
			if (ticket.IsPersistent)
				cookie.Expires = ticket.Expiration;

			context.Response.Cookies.Add (cookie);
			context.SkipAuthorization = (context.Request.Path == loginPage);
		}

		void OnEndRequest (object sender, EventArgs args)
		{
			if (noForms)
				return;

			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.Response.StatusCode != 401)
				return;

			AuthConfig config = (AuthConfig) context.GetConfig ("system.web/authentication");
			Console.WriteLine ("Redirecting to login page: '{0}'", config.LoginUrl);
			context.Response.Redirect (config.LoginUrl);
		}

		public event FormsAuthenticationEventHandler Authenticate;
	}
}

