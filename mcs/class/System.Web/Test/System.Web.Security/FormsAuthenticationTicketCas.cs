//
// FormsAuthenticationTicketCasCas.cs
//	- CAS unit tests for System.Web.Security.FormsAuthenticationTicketCas
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Security;

namespace MonoCasTests.System.Web.Security {

	[TestFixture]
	[Category ("CAS")]
	public class FormsAuthenticationTicketCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3 ()
		{
			FormsAuthenticationTicket ticket = null;
			try {
				// this ctor got a problem on MS 1.x
				ticket = new FormsAuthenticationTicket ("mine", false, Int32.MaxValue);
			}
			catch (NullReferenceException) {
#if NET_2_0
				Assert.Fail ("this should work on 2.0");
#else
				Assert.Ignore ("fails with NullReferenceException on MS 1.x");
#endif
			}
			Assert.AreEqual ("/", ticket.CookiePath, "CookiePath");
			Assert.IsTrue (ticket.Expiration.Year >= 6088, "Expiration");
			Assert.IsFalse (ticket.Expired, "Expired");
			Assert.IsFalse (ticket.IsPersistent, "IsPersistent");
			Assert.IsTrue (ticket.IssueDate <= DateTime.Now, "IssueDate");
			Assert.AreEqual ("mine", ticket.Name, "Name");
			Assert.AreEqual (String.Empty, ticket.UserData, "UserData");
			Assert.IsTrue (ticket.Version > 0, "Version");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor6 ()
		{
			FormsAuthenticationTicket ticket = null;
			try {
				// this ctor got a problem on MS 1.x
				ticket = new FormsAuthenticationTicket (1, "mine", DateTime.MinValue, DateTime.MaxValue, true, "data");
			}
			catch (NullReferenceException) {
#if NET_2_0
				Assert.Fail ("this should work on 2.0");
#else
				Assert.Ignore ("fails with NullReferenceException on MS 1.x");
#endif
			}
			Assert.AreEqual ("/", ticket.CookiePath, "CookiePath");
			Assert.AreEqual (DateTime.MaxValue, ticket.Expiration, "Expiration");
			Assert.IsFalse (ticket.Expired, "Expired");
			Assert.IsTrue (ticket.IsPersistent, "IsPersistent");
			Assert.AreEqual (DateTime.MinValue, ticket.IssueDate, "IssueDate");
			Assert.AreEqual ("mine", ticket.Name, "Name");
			Assert.AreEqual ("data", ticket.UserData, "UserData");
			Assert.AreEqual (1, ticket.Version, "Version");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor7 ()
		{
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket (3, "mine", DateTime.MinValue, DateTime.Now.AddSeconds (-1), false, "data", "path");
			Assert.AreEqual ("path", ticket.CookiePath, "CookiePath");
			Assert.IsTrue (ticket.Expiration <= DateTime.Now, "Expiration");
			Assert.IsTrue (ticket.Expired, "Expired");
			Assert.IsFalse (ticket.IsPersistent, "IsPersistent");
			Assert.AreEqual (DateTime.MinValue, ticket.IssueDate, "IssueDate");
			Assert.AreEqual ("mine", ticket.Name, "Name");
			Assert.AreEqual ("data", ticket.UserData, "UserData");
			Assert.AreEqual (3, ticket.Version, "Version");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// we can't take the simpler (3 params) ctor as it fails under 1.x (NRE)
			ConstructorInfo ci = this.Type.GetConstructor (new Type[7] { typeof (int), typeof (string), typeof (DateTime), typeof (DateTime), typeof (bool), typeof (string), typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string,bool,int)");
			return ci.Invoke (new object[7] { 3, "mine", DateTime.MinValue, DateTime.Now.AddSeconds (-1), false, "data", "path" });
		}

		public override Type Type {
			get { return typeof (FormsAuthenticationTicket); }
		}
	}
}
