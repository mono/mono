//
// MembershipTest.cs - Unit tests for System.Web.Security.Membership
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


using System;
using System.Text;
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class MembershipTest {

		[Test]
		public void Provider ()
		{
			Assert.IsNotNull (Membership.Provider, "Membership.Provider");
		}

		[Test]
		public void GeneratePassword ()
		{
			string pwd;
			int count;
			int i;

			pwd = Membership.GeneratePassword (5, 0);
			Assert.AreEqual (5, pwd.Length, "A1");

			pwd = Membership.GeneratePassword (5, 1);
			Assert.AreEqual (5, pwd.Length, "A2");
			/* count up the non-alphanumeric characters in the string */
			count = 0;
			for (i = 0; i < pwd.Length; i ++)
				if (!Char.IsLetterOrDigit (pwd, i))
					count++;
			Assert.IsTrue (count >= 1, "A2");
		}

		[Test (Description = "Bug #647631")]
		public void CreatePassword_InvalidInput ()
		{
			MembershipUser user;

			Assert.Throws<MembershipCreateUserException> (() => {
				user = Membership.CreateUser (null, "password");
			}, "#A1");

			Assert.Throws<MembershipCreateUserException> (() => {
				user = Membership.CreateUser (String.Empty, "password");
			}, "#A2");

			Assert.Throws<MembershipCreateUserException> (() => {
				user = Membership.CreateUser ("user", null);
			}, "#B1");

			Assert.Throws<MembershipCreateUserException> (() => {
				user = Membership.CreateUser ("user", String.Empty);
			}, "#B2");
		}
	}
}

