//
// AuthenticationService.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.Web.Services;
using System.Web.Configuration;
using System.Web.Security;

namespace System.Web.Script.Services
{
	sealed class AuthenticationService
	{
		public const string DefaultWebServicePath = "/Authentication_JSON_AppService.axd";

		readonly ScriptingAuthenticationServiceSection _section;

		public AuthenticationService () {
			_section = (ScriptingAuthenticationServiceSection) WebConfigurationManager.GetSection ("system.web.extensions/scripting/webServices/authenticationService");
		}

		void EnsureEnabled() {
			if (_section == null || !_section.Enabled)
				throw new InvalidOperationException ("Authentication service is disabled.");

			if (_section.RequireSSL && !HttpContext.Current.Request.IsSecureConnection)
				throw new HttpException ("SSL is required for this operation.");
		}

		[WebMethod ()]
		public bool Login (string userName, string password, bool createPersistentCookie) {
			EnsureEnabled ();

			if (!Membership.Provider.ValidateUser (userName, password))
				return false;

			FormsAuthentication.SetAuthCookie (userName, createPersistentCookie);

			return true;
		}

		[WebMethod ()]
		public void Logout () {
			EnsureEnabled ();

			FormsAuthentication.SignOut ();
		}
	}
}
