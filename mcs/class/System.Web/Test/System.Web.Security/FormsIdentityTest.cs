//
// FormsIdentityTest.cs - Unit tests for System.Web.Security.FormsIdentity
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
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class FormsIdentityTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Null ()
		{
			FormsIdentity identity = new FormsIdentity (null);
		}

		[Test]
		public void Ticket ()
		{
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket (3, "mine", DateTime.MinValue, DateTime.Now.AddSeconds (-1), false, "data", "path");
			FormsIdentity identity = new FormsIdentity (ticket);
			Assert.AreEqual ("Forms", identity.AuthenticationType, "AuthenticationType");
			Assert.IsTrue (identity.IsAuthenticated, "IsAuthenticated");
			Assert.AreEqual ("mine", identity.Name, "Name");
			Assert.IsNotNull (identity.Ticket, "Ticket");
		}
	}
}
