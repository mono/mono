//
// System.Web.Security.FormsAuthenticationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class FormsAuthenticationModule : IHttpModule
	{
		static readonly object authenticateEvent = new object ();
		
#if NET_2_0
		AuthenticationSection _config = null;
#else
		AuthConfig _config = null;
#endif
		bool isConfigInitialized = false;
		EventHandlerList events = new EventHandlerList ();
		
		public event FormsAuthenticationEventHandler Authenticate {
			add { events.AddHandler (authenticateEvent, value); }
			remove { events.RemoveHandler (authenticateEvent, value); }
		}
		
		void InitConfig (HttpContext context)
		{
			if(isConfigInitialized)
				return;
#if NET_2_0
			_config = (AuthenticationSection) WebConfigurationManager.GetSection ("system.web/authentication");
#else
			_config = (AuthConfig) context.GetConfig ("system.web/authentication");
#endif
			isConfigInitialized = true;
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public FormsAuthenticationModule ()
		{
		}

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

			string cookieName;
			string cookiePath;
			string loginPage;
			bool slidingExpiration;

			InitConfig (context);
			if (_config == null || _config.Mode != AuthenticationMode.Forms) {
				return;
			}

#if NET_2_0
			cookieName = _config.Forms.Name;
			cookiePath = _config.Forms.Path;
			loginPage = _config.Forms.LoginUrl;
			slidingExpiration = _config.Forms.SlidingExpiration;
#else
			cookieName = _config.CookieName;
			cookiePath = _config.CookiePath;
			loginPage = _config.LoginUrl;
			slidingExpiration = _config.SlidingExpiration;
#endif

			if (!VirtualPathUtility.IsRooted (loginPage))
				loginPage = "~/" + loginPage;

			string reqPath = String.Empty;
			string loginPath = null;
			try {
				reqPath = context.Request.PhysicalPath;
				loginPath = context.Request.MapPath (loginPage);
			} catch {} // ignore

			context.SkipAuthorization = String.Compare (reqPath, loginPath, RuntimeHelpers.CaseInsensitive, Helpers.InvariantCulture) == 0;
			
#if NET_2_0
			//TODO: need to check that the handler is System.Web.Handlers.AssemblyResourceLoader type
			string filePath = context.Request.FilePath;
			if (filePath.Length > 15 && String.CompareOrdinal ("WebResource.axd", 0, filePath, filePath.Length - 15, 15) == 0)
				context.SkipAuthorization = true;
#endif

			FormsAuthenticationEventArgs formArgs = new FormsAuthenticationEventArgs (context);
			FormsAuthenticationEventHandler eh = events [authenticateEvent] as FormsAuthenticationEventHandler;
			if (eh != null)
				eh (this, formArgs);

			bool contextUserNull = (context.User == null);
			if (formArgs.User != null || !contextUserNull) {
				if (contextUserNull)
					context.User = formArgs.User;
				return;
			}
				
			HttpCookie cookie = context.Request.Cookies [cookieName];
			if (cookie == null || (cookie.Expires != DateTime.MinValue && cookie.Expires < DateTime.Now))
				return;

			FormsAuthenticationTicket ticket = null;
			try {
				ticket = FormsAuthentication.Decrypt (cookie.Value);
			}
			catch (ArgumentException) {
				// incorrect cookie value, suppress the exception
				return;
			}
			if (ticket == null || (!ticket.IsPersistent && ticket.Expired))
				return;

			FormsAuthenticationTicket oldticket = ticket;
			if (slidingExpiration)
				ticket = FormsAuthentication.RenewTicketIfOld (ticket);

			context.User = new GenericPrincipal (new FormsIdentity (ticket), new string [0]);

			if (cookie.Expires == DateTime.MinValue && oldticket == ticket) 
				return;

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
#if NET_4_5
			if (context.Response.SuppressFormsAuthenticationRedirect)
				return;
#endif
			string loginPage;
			InitConfig (context);
#if NET_2_0
			loginPage = _config.Forms.LoginUrl;
#else
			loginPage = _config.LoginUrl;
#endif
			if (_config == null || _config.Mode != AuthenticationMode.Forms)
				return;

			StringBuilder login = new StringBuilder ();
			login.Append (UrlUtils.Combine (context.Request.ApplicationPath, loginPage));
			login.AppendFormat ("?ReturnUrl={0}", HttpUtility.UrlEncode (context.Request.RawUrl));
			context.Response.Redirect (login.ToString (), false);
		}
	}
}

