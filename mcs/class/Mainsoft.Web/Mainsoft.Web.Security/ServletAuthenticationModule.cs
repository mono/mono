//
// System.Web.Security.ServletAuthenticationModule
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
using javax.servlet;
using javax.servlet.http;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.Security
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// <para>Sets the identity of the user for an ASP.NET Java EE application.</para>
	/// </summary>
	public sealed class SevletAuthenticationModule : IHttpModule
	{
		public void Dispose () {
		}

		public void Init (HttpApplication app) {
			app.AuthenticateRequest += new EventHandler (OnAuthenticateRequest);
		}

		void OnAuthenticateRequest (object sender, EventArgs args) {
			HttpApplication app = (HttpApplication) sender;
			BaseWorkerRequest req = J2EEUtils.GetWorkerRequest (app.Context);
			if (req.GetRemoteUser () != null)
				app.Context.User = new ServletPrincipal (req);
		}
	}

	public sealed class ServletPrincipal : IPrincipal
	{
		readonly BaseWorkerRequest _request;
		readonly IIdentity _identity;

		internal ServletPrincipal (BaseWorkerRequest req) {
			_request = req;
			string authType = req.GetAuthType ();
			if (authType == null)
				authType = String.Empty;
			_identity = new GenericIdentity (req.GetRemoteUser (), authType);
		}

		public bool IsInRole (string role) {
			return _request.IsUserInRole (role);
		}

		public IIdentity Identity { get { return _identity; } }

		public java.security.Principal Principal { get { return _request.GetUserPrincipal (); } }
	}
}

