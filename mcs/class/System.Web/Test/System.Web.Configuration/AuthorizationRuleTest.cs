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
using System.IO;
using System.Xml;
using System.Reflection;

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

		[Test]
		public void SerializeElement ()
		{
			StringWriter sw;
			XmlWriter writer;
			AuthorizationRule rule;
			MethodInfo mi = typeof (AuthorizationRule).GetMethod ("SerializeElement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object[] parms = new object[2];
			bool failed;

			/* 1 */
			failed = true;
			try {
				sw = new StringWriter ();
				writer = new XmlTextWriter (sw);

				rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
				parms[0] = writer;
				parms[1] = false;
				mi.Invoke (rule, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A1");
				failed = false;
			}
			Assert.IsFalse (failed, "A1");

			/* 2 */
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
			rule.Users.Add ("toshok");
			parms[0] = writer;
			parms[1] = false;
			mi.Invoke (rule, parms);

			Assert.AreEqual ("<allow users=\"toshok\" />", sw.ToString(), "A2");

			/* 2 */
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
			rule.Users.Add ("toshok");
			parms[0] = writer;
			parms[1] = true;
			mi.Invoke (rule, parms);

			Assert.AreEqual ("<allow users=\"toshok\" />", sw.ToString(), "A2");

			/* 3-4 */
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			rule = new AuthorizationRule (AuthorizationRuleAction.Deny);
			rule.Users.Add ("toshok");
			rule.Users.Add ("chris");
			rule.Roles.Add ("admin");
			rule.Roles.Add ("wheel");
			rule.Verbs.Add ("GET");
			rule.Verbs.Add ("PUT");
			parms[0] = writer;
			parms[1] = true;
			bool b = (bool)mi.Invoke (rule, parms);

			Assert.AreEqual ("<deny roles=\"admin,wheel\" users=\"toshok,chris\" verbs=\"GET,PUT\" />", sw.ToString(), "A3");
			Assert.IsTrue (b, "A4");
		}

		[Test]
		public void PostDeserialize ()
		{
			AuthorizationRule rule;
			MethodInfo mi = typeof (AuthorizationRule).GetMethod ("PostDeserialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object[] parms = new object[0];
			bool failed;

			/* 1 */
			failed = true;
			try {
				rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
				mi.Invoke (rule, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A1");
				failed = false;
			}
			Assert.IsFalse (failed, "A1");

			/* 2 */
			rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
			rule.Users.Add ("toshok");
			mi.Invoke (rule, parms);

			/* 2 */
			rule = new AuthorizationRule (AuthorizationRuleAction.Allow);
			rule.Users.Add ("toshok");
			mi.Invoke (rule, parms);

			/* 3-4 */
			rule = new AuthorizationRule (AuthorizationRuleAction.Deny);
			rule.Users.Add ("toshok");
			rule.Users.Add ("chris");
			rule.Roles.Add ("admin");
			rule.Roles.Add ("wheel");
			rule.Verbs.Add ("GET");
			rule.Verbs.Add ("PUT");

			mi.Invoke (rule, parms);
		}

	}

}

#endif
