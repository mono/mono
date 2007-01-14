//
// Mainsoft.Web.Security.J2EEAuthenticationModule
//
// Authors:
//	Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

namespace Mainsoft.Web.Security
{
	public sealed class J2EEAuthenticationModule : IHttpModule
	{
		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.AuthenticateRequest += new EventHandler (OnAuthenticateRequest);
		}

		void OnAuthenticateRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpServletRequest req = app.Context.Request.ServletWorkerRequest.ServletRequest;
			if (req.getRemoteUser() != null)
				app.Context.User = new J2EEPrincipal(req);
		}
	}

	internal class J2EEPrincipal : IPrincipal
	{
		HttpServletRequest _request;
		IIdentity _identity;

		public J2EEPrincipal(HttpServletRequest req)
		{
			_request = req;
			string authType = req.getAuthType();
			if (authType == null)
				authType = "";
			_identity = new GenericIdentity(req.getRemoteUser(), authType);
		}

		public bool IsInRole(string role)
		{
			return _request.isUserInRole(role);
		}

		public IIdentity Identity { get { return _identity; } }
	}
}

