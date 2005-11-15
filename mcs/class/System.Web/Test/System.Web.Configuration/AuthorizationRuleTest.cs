//
// AuthorizationRuleTest.cs 
//	- unit tests for System.Web.Configuration.AuthorizationRule
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class AuthorizationRuleTest  {

		[Test]
		public void Defaults()
		{
			AuthorizationRule a = new AuthorizationRule(AuthorizationRuleAction.Deny);

			Assert.AreEqual (AuthorizationRuleAction.Deny, a.Action, "A1");
			Assert.IsNotNull (a.Roles, "A2");
			Assert.IsNotNull (a.Users, "A3");
			Assert.IsNotNull (a.Verbs, "A4");
		}

		[Test]
		public void Test_EqualsAndHashCode ()
		{
			AuthorizationRule a = new AuthorizationRule (AuthorizationRuleAction.Deny);
			AuthorizationRule b = new AuthorizationRule (AuthorizationRuleAction.Deny);

			a.Users.Add ("toshok");
			a.Roles.Add ("Admin");
			a.Verbs.Add ("reboot");

			b.Users.Add ("toshok");
			b.Roles.Add ("Admin");
			b.Verbs.Add ("reboot");

			Assert.AreEqual (a, b, "A1");
			Assert.AreEqual (a.GetHashCode (), b.GetHashCode (), "A2");
		}
	}

}

#endif
