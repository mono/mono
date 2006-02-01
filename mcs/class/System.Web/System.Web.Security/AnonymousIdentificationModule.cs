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

using System.Web;
using System.Web.Configuration;

namespace System.Web.Security {

	public sealed class AnonymousIdentificationModule : IHttpModule {

		bool use_cookie;

		public event AnonymousIdentificationEventHandler Creating;
		
		public void ClearAnonymousIdentifier ()
		{
			HttpContext c = HttpContext.Current;
			SystemWebSectionGroup g = (SystemWebSectionGroup)WebConfigurationManager.GetSection ("system.web");

			if (!g.AnonymousIdentification.Enabled
			    || false /* XXX The user for the current request is anonymous */)
				throw new NotSupportedException ();

			if (use_cookie) {
			}
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

		void OnEnter (object source, EventArgs eventArgs)
		{
			if (!Enabled)
				return;

			string anonymousID = null;

			if (Creating != null) {
				AnonymousIdentificationEventArgs e = new AnonymousIdentificationEventArgs (HttpContext.Current);
				Creating (this, e);

				anonymousID = e.AnonymousID;
			}

			if (anonymousID == null)
				anonymousID = Guid.NewGuid().ToString();

			app.Request.AnonymousID = anonymousID;
		}

		public static bool Enabled {
			get {
				SystemWebSectionGroup g = (SystemWebSectionGroup)WebConfigurationManager.GetSection ("system.web");
				return g.AnonymousIdentification.Enabled;
			}
		}

		HttpApplication app;
	}
}
#endif

