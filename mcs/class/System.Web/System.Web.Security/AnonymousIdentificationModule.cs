//
// System.Web.Security.AnonymousIdentificationModule
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Web;
using System.Web.Configuration;
using System.Text;

namespace System.Web.Security {

	public sealed class AnonymousIdentificationModule : IHttpModule {
		static readonly object creatingEvent = new object ();
		
		HttpApplication app;
		EventHandlerList events = new EventHandlerList ();
		
		public event AnonymousIdentificationEventHandler Creating  {
			add { events.AddHandler (creatingEvent, value); }
			remove { events.RemoveHandler (creatingEvent, value); }
		}

		public static void ClearAnonymousIdentifier ()
		{
			if (Config == null || !Config.Enabled)
				/* XXX The user for the current request is anonymous */
				throw new NotSupportedException ();
		}

		public void Dispose ()
		{
			app.PostAuthenticateRequest -= OnEnter;
			app = null;
		}
		
		public void Init (HttpApplication app)
		{
			this.app = app;
			app.PostAuthenticateRequest += OnEnter;
		}

		[MonoTODO ("cookieless userid")]
		void OnEnter (object source, EventArgs eventArgs)
		{
			if (!Enabled)
				return;

			string anonymousID = null;

			HttpCookie cookie = app.Request.Cookies [Config.CookieName];
			if (cookie != null && (cookie.Expires == DateTime.MinValue || cookie.Expires > DateTime.Now)) {
				try {
					anonymousID = Encoding.Unicode.GetString (Convert.FromBase64String (cookie.Value));
				}
				catch { }
			}

			if (anonymousID == null) {
				AnonymousIdentificationEventHandler eh = events [creatingEvent] as AnonymousIdentificationEventHandler;
				if (eh != null) {
					AnonymousIdentificationEventArgs e = new AnonymousIdentificationEventArgs (HttpContext.Current);
					eh (this, e);

					anonymousID = e.AnonymousID;
				}

				if (anonymousID == null)
					anonymousID = Guid.NewGuid ().ToString ();

				HttpCookie newCookie = new HttpCookie (Config.CookieName);
				newCookie.Path = app.Request.ApplicationPath;
				newCookie.Expires = DateTime.Now + Config.CookieTimeout;
				newCookie.Value = Convert.ToBase64String (Encoding.Unicode.GetBytes (anonymousID));
				app.Response.AppendCookie (newCookie);
			}
			app.Request.AnonymousID = anonymousID;
		}

		public static bool Enabled {
			get {
				if (Config == null)
					return false;

				return Config.Enabled;
			}
		}

#if TARGET_JVM
		static AnonymousIdentificationSection Config
		{
			get
			{
				AnonymousIdentificationSection config = (AnonymousIdentificationSection) AppDomain.CurrentDomain.GetData ("Anonymous.Config");
				if (config == null) {
					lock (typeof (AnonymousIdentificationModule)) {
						config = (AnonymousIdentificationSection) AppDomain.CurrentDomain.GetData ("Anonymous.Config");
						if (config == null)
							config = (AnonymousIdentificationSection) WebConfigurationManager.GetSection ("system.web/anonymousIdentification");
						AppDomain.CurrentDomain.SetData ("Anonymous.Config", config);
					}
				}
				return config;
			}
		}
#else
		static AnonymousIdentificationSection Config = (AnonymousIdentificationSection) WebConfigurationManager.GetSection ("system.web/anonymousIdentification");
#endif
	}
}
#endif

