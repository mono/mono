//
// System.Web.UI.WebControls.ProfileModule.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Web.Configuration;
using System.Text;

namespace System.Web.Profile
{
	public sealed class ProfileModule : IHttpModule
	{
		HttpApplication app;
		ProfileBase profile;
		string anonymousCookieName = null;

		public ProfileModule ()
		{
		}

		public void Dispose ()
		{
			app.EndRequest -= OnLeave;
			app.PostMapRequestHandler -= OnEnter;
		}

		public void Init (HttpApplication app)
		{
			this.app = app;
			app.PostMapRequestHandler += OnEnter;
			app.EndRequest += OnLeave;

			AnonymousIdentificationSection anonymousConfig = 
				(AnonymousIdentificationSection) WebConfigurationManager.GetSection ("system.web/anonymousIdentification");

			if (anonymousConfig == null)
				return;

			anonymousCookieName = anonymousConfig.CookieName;
		}

		void OnEnter (object o, EventArgs eventArgs)
		{
			if (!ProfileManager.Enabled)
				return;

			if (HttpContext.Current.Request.IsAuthenticated) {
				HttpCookie cookie = app.Request.Cookies [anonymousCookieName];
				if (cookie != null && (cookie.Expires != DateTime.MinValue && cookie.Expires > DateTime.Now)) {
					if (MigrateAnonymous != null) {
						ProfileMigrateEventArgs e = new ProfileMigrateEventArgs (HttpContext.Current,
							Encoding.Unicode.GetString (Convert.FromBase64String (cookie.Value)));
						MigrateAnonymous (this, e);
					}

					HttpCookie newCookie = new HttpCookie (anonymousCookieName);
					newCookie.Path = app.Request.ApplicationPath;
					newCookie.Expires = new DateTime (1970, 1, 1);
					newCookie.Value = "";
					app.Response.AppendCookie (newCookie);
				}
			}
		}

		void OnLeave (object o, EventArgs eventArgs)
		{
			if (!ProfileManager.Enabled)
				return;

			if (!app.Context.ProfileInitialized)
				return;

			if (ProfileManager.AutomaticSaveEnabled) {
				profile = app.Context.Profile;

				if (profile == null)
					return;

				if (ProfileAutoSaving != null) {
					ProfileAutoSaveEventArgs args = new ProfileAutoSaveEventArgs (app.Context);
					ProfileAutoSaving (this, args);
					if (!args.ContinueWithProfileAutoSave)
						return;
				}
				profile.Save ();
			}
		}

		public event ProfileMigrateEventHandler MigrateAnonymous;
		[MonoTODO ("implement event rising")]
		public event ProfileEventHandler Personalize;
		public event ProfileAutoSaveEventHandler ProfileAutoSaving;
	}
}

#endif
