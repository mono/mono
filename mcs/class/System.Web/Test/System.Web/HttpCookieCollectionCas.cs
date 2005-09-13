//
// HttpCookieCollectionCas.cs 
//	- CAS unit tests for System.Web.HttpCookieCollection
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
using System.Collections;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpCookieCollectionCas : AspNetHostingMinimal {

		private HttpCookie biscuit;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			biscuit = new HttpCookie (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			HttpCookieCollection jar = new HttpCookieCollection ();
			jar.Add (biscuit);
			jar.CopyTo (new object[1], 0);
			Assert.IsNull (jar.GetKey (0), "GetKey");
			jar.Remove ("chocolat");
			jar.Set (biscuit);
			Assert.IsNotNull (jar.Get (0), "Get(int)");
			Assert.IsNull (jar.Get ("chocolat"), "Get(string)");
			Assert.IsNotNull (jar[0], "this[int]");
			Assert.IsNull (jar["chocolat"], "this[string]");
			Assert.AreEqual (1, jar.AllKeys.Length, "AllKeys");
			jar.Clear ();
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (HttpCookieCollection); }
		}
	}
}
