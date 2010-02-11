//
// RolesTest.cs - Unit tests for System.Web.Security.Roles
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

#if NET_2_0

using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

using MonoTests.SystemWeb.Framework;
using NUnit.Framework;
using System.Configuration.Provider;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class RolesTest {

		[Test]
        [Category ("NotWorking")]
		public void Enabled ()
		{
			Assert.IsFalse (Roles.Enabled, "Enabled");

			Assert.IsFalse (Roles.CacheRolesInCookie, "CacheRolesInCookie");
			Assert.AreEqual (".ASPXROLES", Roles.CookieName, "CookieName");
			Assert.AreEqual ("/", Roles.CookiePath, "CookiePath");
			Assert.AreEqual (CookieProtection.All, Roles.CookieProtectionValue, "CookieProtectionValue");
			Assert.IsFalse (Roles.CookieRequireSSL, "CookieRequireSSL");
			Assert.IsTrue (Roles.CookieSlidingExpiration, "CookieSlidingExpiration");
			Assert.AreEqual (30, Roles.CookieTimeout, "CookieTimeout");
			Assert.IsFalse (Roles.CreatePersistentCookie, "CreatePersistentCookie");
			Assert.IsNull (Roles.Domain, "Domain");
			Assert.AreEqual (25, Roles.MaxCachedResults, "MaxCachedResults");

			// ProviderException (missing web.config) for
			// - ApplicationName
			// - Provider
			// - Providers
		}

		[Test]
		[Category ("NunitWeb")]
		public void IsUserInRole ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
						Assert.IsTrue (Roles.Enabled, "Enabled");
						Assert.IsTrue (Roles.IsUserInRole ("true", "rolename"), "#1");
						Assert.IsFalse (Roles.IsUserInRole ("false", "rolename"), "#2");

						// NOTE: The next two tests do NOT throw an exception on MS 
						//       .NET (even if the underlying membership-provider may, 
						//       despite being documented differently on MSDN), but 
						//       this convenient behaviour allows ASP.NET pages to run 
						//       when roles are queried before the user is logged on
						Assert.IsFalse (Roles.IsUserInRole (string.Empty, "rolename"), "#3a");
						Assert.IsFalse (Roles.IsUserInRole ("rolename"), "#3b");
					}));
			t.Run ();
			global::System.Diagnostics.Trace.WriteLineIf ((t.Response.StatusCode != global::System.Net.HttpStatusCode.OK), t.Response.Body);
			Assert.AreEqual (global::System.Net.HttpStatusCode.OK, t.Response.StatusCode, "HttpStatusCode");
		}
	}
}

#endif
