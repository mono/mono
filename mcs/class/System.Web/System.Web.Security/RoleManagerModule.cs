//
// System.Web.Security.RoleManagerModule
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Configuration;

namespace System.Web.Security {
	public sealed class RoleManagerModule : IHttpModule {
		public event RoleManagerEventHandler GetRoles;

		public void Dispose ()
		{
		}

		void ClearCookie (HttpApplication app, string cookieName)
		{
			RoleManagerSection config = (RoleManagerSection) WebConfigurationManager.GetSection ("system.web/roleManager");
			HttpCookie clearCookie = new HttpCookie (config.CookieName, "");

			clearCookie.Path = config.CookiePath;
			clearCookie.Expires = DateTime.MinValue;
			clearCookie.Domain = config.Domain;
			clearCookie.Secure = config.CookieRequireSSL;
			app.Response.SetCookie (clearCookie);
		}

		void OnPostAuthenticateRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication)sender;
			RoleManagerSection config = (RoleManagerSection)WebConfigurationManager.GetSection ("system.web/roleManager");

			/* if we're disabled, bail out early */
			if (!config.Enabled)
				return;

			/* allow the user to populate the Role */
			if (GetRoles != null) {
				RoleManagerEventArgs role_args = new RoleManagerEventArgs (app.Context);

				GetRoles (this, role_args);

				if (role_args.RolesPopulated)
					return;
			}

			RolePrincipal principal;

			HttpCookie cookie = app.Request.Cookies[config.CookieName];

			IIdentity currentIdentity = app.Context.User.Identity;
			if (app.Request.IsAuthenticated) {
				if (cookie != null) {
					if (!config.CacheRolesInCookie)
						cookie = null;
					else if (config.CookieRequireSSL && !app.Request.IsSecureConnection) {
						cookie = null;
						ClearCookie (app, config.CookieName);
					}
						
				}

				if (cookie == null)
					principal = new RolePrincipal (currentIdentity);
				else
					principal = new RolePrincipal (currentIdentity, cookie.Value);
			}
			else {
				/* anonymous request */

				if (cookie != null) {
					ClearCookie (app, config.CookieName);
				}

				principal = new RolePrincipal (currentIdentity);
			}

			app.Context.User = principal;
			Thread.CurrentPrincipal = principal;
		}

		void OnEndRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication)sender;
			RoleManagerSection config = (RoleManagerSection)WebConfigurationManager.GetSection ("system.web/roleManager");

			/* if we're not enabled or configured to cache
			 * cookies, bail out */
			if (!config.Enabled || !config.CacheRolesInCookie)
				return;

			/* if the user isn't authenticated, bail
			 * out */
			if (!app.Request.IsAuthenticated)
				return;

			/* if the configuration requires ssl for
			 * cookies and we're not on an ssl connection,
			 * bail out */
			if (config.CookieRequireSSL && !app.Request.IsSecureConnection)
				return;

			RolePrincipal principal = app.Context.User as RolePrincipal;
			if (principal == null) /* just for my sanity */
				return;

			if (!principal.CachedListChanged)
				return;

			string ticket = principal.ToEncryptedTicket ();
			if (ticket == null || ticket.Length > 4096) {
				ClearCookie (app, config.CookieName);
				return;
			}

			HttpCookie cookie = new HttpCookie (config.CookieName, ticket);

			cookie.HttpOnly = true;
			if (!string.IsNullOrEmpty (config.Domain))
				cookie.Domain = config.Domain;
			if (config.CookieRequireSSL)
				cookie.Secure = true;
			if (config.CookiePath.Length > 1) // more than '/'
				cookie.Path = config.CookiePath;
			app.Response.SetCookie (cookie);
		}

		public void Init (HttpApplication app)
		{
			app.PostAuthenticateRequest += OnPostAuthenticateRequest;
			app.EndRequest += OnEndRequest;
		}
	}
}
#endif

