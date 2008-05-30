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
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Configuration;

namespace System.Web.Security {
	public sealed class RoleManagerModule : IHttpModule
	{
		static readonly object getRolesEvent = new object ();
		
		RoleManagerSection _config = null;
		EventHandlerList events = new EventHandlerList ();
		
		public event RoleManagerEventHandler GetRoles {
			add { events.AddHandler (getRolesEvent, value); }
			remove { events.RemoveHandler (getRolesEvent, value); }
		}

		public void Dispose ()
		{
		}

		void ClearCookie (HttpApplication app, string cookieName)
		{
			HttpCookie clearCookie = new HttpCookie (_config.CookieName, "");

			clearCookie.Path = _config.CookiePath;
			clearCookie.Expires = DateTime.MinValue;
			clearCookie.Domain = _config.Domain;
			clearCookie.Secure = _config.CookieRequireSSL;
			app.Response.SetCookie (clearCookie);
		}

		void OnPostAuthenticateRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication)sender;

			/* if we're disabled, bail out early */
			if (_config == null || !_config.Enabled)
				return;

			/* allow the user to populate the Role */
			RoleManagerEventHandler eh = events [getRolesEvent] as RoleManagerEventHandler;
			if (eh != null) {
				RoleManagerEventArgs role_args = new RoleManagerEventArgs (app.Context);

				eh (this, role_args);

				if (role_args.RolesPopulated)
					return;
			}

			RolePrincipal principal;

			HttpCookie cookie = app.Request.Cookies [_config.CookieName];

			IIdentity currentIdentity = app.Context.User.Identity;
			if (app.Request.IsAuthenticated) {
				if (cookie != null) {
					if (!_config.CacheRolesInCookie)
						cookie = null;
					else if (_config.CookieRequireSSL && !app.Request.IsSecureConnection) {
						cookie = null;
						ClearCookie (app, _config.CookieName);
					}
						
				}

				if (cookie == null || String.IsNullOrEmpty (cookie.Value))
					principal = new RolePrincipal (currentIdentity);
				else
					principal = new RolePrincipal (currentIdentity, cookie.Value);
			}
			else {
				/* anonymous request */

				if (cookie != null) {
					ClearCookie (app, _config.CookieName);
				}

				principal = new RolePrincipal (currentIdentity);
			}

			app.Context.User = principal;
			Thread.CurrentPrincipal = principal;
		}

		void OnEndRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication)sender;

			/* if we're not enabled or configured to cache
			 * cookies, bail out */
			if (_config == null || !_config.Enabled || !_config.CacheRolesInCookie)
				return;

			/* if the user isn't authenticated, bail
			 * out */
			if (!app.Request.IsAuthenticated)
				return;

			/* if the configuration requires ssl for
			 * cookies and we're not on an ssl connection,
			 * bail out */
			if (_config.CookieRequireSSL && !app.Request.IsSecureConnection)
				return;

			RolePrincipal principal = app.Context.User as RolePrincipal;
			if (principal == null) /* just for my sanity */
				return;

			if (!principal.CachedListChanged)
				return;

			string ticket = principal.ToEncryptedTicket ();
			if (ticket == null || ticket.Length > 4096) {
				ClearCookie (app, _config.CookieName);
				return;
			}

			HttpCookie cookie = new HttpCookie (_config.CookieName, ticket);

			cookie.HttpOnly = true;
			if (!string.IsNullOrEmpty (_config.Domain))
				cookie.Domain = _config.Domain;
			if (_config.CookieRequireSSL)
				cookie.Secure = true;
			if (_config.CookiePath.Length > 1) // more than '/'
				cookie.Path = _config.CookiePath;
			app.Response.SetCookie (cookie);
		}

		public void Init (HttpApplication app)
		{
			_config = (RoleManagerSection) WebConfigurationManager.GetSection ("system.web/roleManager");

			app.PostAuthenticateRequest += OnPostAuthenticateRequest;
			app.EndRequest += OnEndRequest;
		}
	}
}
#endif

