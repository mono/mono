//
// System.Web.Security.FormsAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Security
{
	public sealed class FormsAuthenticationModule : IHttpModule
	{
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
				return;
			}

			string cookieName = config.CookieName;
			string cookiePath = config.CookiePath;
			string loginPage = config.LoginUrl;

			string reqPath = context.Request.PhysicalPath;
			string loginPath = context.Request.MapPath (loginPage);
			context.SkipAuthorization = (reqPath == loginPath);
			
			FormsAuthenticationEventArgs formArgs = new FormsAuthenticationEventArgs (context);
			if (Authenticate != null)
				Authenticate (this, formArgs);

			bool contextUserNull = (context.User == null);
			if (formArgs.User != null || !contextUserNull) {
				if (contextUserNull)
					context.User = formArgs.User;
				return;
			}
				
			HttpCookie cookie = context.Request.Cookies [cookieName];
			if (cookie == null || (cookie.Expires != DateTime.MinValue && cookie.Expires < DateTime.Now))
				return;

			FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt (cookie.Value);
			if (ticket == null || ticket.Expired)
				return;

			if (config.SlidingExpiration)
				ticket = FormsAuthentication.RenewTicketIfOld (ticket);

			context.User = new GenericPrincipal (new FormsIdentity (ticket), new string [0]);

			cookie.Value = FormsAuthentication.Encrypt (ticket);
			cookie.Path = cookiePath;
			if (ticket.IsPersistent)
				cookie.Expires = ticket.Expiration;

			context.Response.Cookies.Add (cookie);
		}

		void OnEndRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.Response.StatusCode != 401 || context.Request.QueryString ["ReturnUrl"] != null)
				return;

			AuthConfig config = (AuthConfig) context.GetConfig ("system.web/authentication");
			if (config.Mode != AuthenticationMode.Forms)
				return;

			StringBuilder login = new StringBuilder ();
			login.Append (UrlUtils.Combine (context.Request.ApplicationPath, config.LoginUrl));
			login.AppendFormat ("?ReturnUrl={0}", HttpUtility.UrlEncode (context.Request.RawUrl));
			context.Response.Redirect (login.ToString ());
		}

		public event FormsAuthenticationEventHandler Authenticate;
	}
}

