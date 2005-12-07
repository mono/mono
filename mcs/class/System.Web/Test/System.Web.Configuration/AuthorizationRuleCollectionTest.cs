//
// AuthorizationRuleCollectionTest.cs 
//	- unit tests for System.Web.Configuration.AuthorizationRuleCollection
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
	public class AuthorizationRuleCollectionTest  {

		[Test]
		public void AddDuplicate ()
		{
			AuthorizationRuleCollection col = new AuthorizationRuleCollection ();
			AuthorizationRule rule = new AuthorizationRule (AuthorizationRuleAction.Deny);

			rule.Users.Add ("toshok");
			rule.Verbs.Add ("GET");

			col.Add (rule);
			col.Add (rule);

			Assert.AreEqual (2, col.Count, "A1");
			Assert.AreEqual ("toshok", col[0].Users.ToString(), "A2");
		}

		[Test]
		public void AddDuplicate2 ()
		{
			AuthorizationRuleCollection col = new AuthorizationRuleCollection ();
			AuthorizationRule rule1 = new AuthorizationRule (AuthorizationRuleAction.Deny);
			AuthorizationRule rule2 = new AuthorizationRule (AuthorizationRuleAction.Allow);

			rule1.Users.Add ("toshok");
			rule1.Verbs.Add ("GET");

			rule2.Users.Add ("toshok");
			rule2.Verbs.Add ("GET");

			col.Add (rule1);
			col.Add (rule2);

			Assert.AreEqual (2, col.Count, "A1");
			Assert.AreEqual ("toshok", col[0].Users.ToString(), "A2");
			Assert.AreEqual (AuthorizationRuleAction.Deny, col[0].Action, "A3");
			Assert.AreEqual (AuthorizationRuleAction.Allow, col[1].Action, "A4");
		}

		[Test]
		public void GetElementKey ()
		{
			MethodInfo minfo = typeof (AuthorizationRuleCollection).GetMethod ("GetElementKey", BindingFlags.Instance | BindingFlags.NonPublic);
			AuthorizationRuleCollection col = new AuthorizationRuleCollection ();

			AuthorizationRule rule = new AuthorizationRule (AuthorizationRuleAction.Deny);

			rule.Users.Add ("toshok");
			rule.Verbs.Add ("GET");

			col.Add (rule);

			object[] args = new object[1];
			args[0] = rule;
			string key = (string)minfo.Invoke (col, args);

			Assert.AreEqual ("Deny", key, "A1");
		}
	}

}

#endif
