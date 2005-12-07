//
// HttpCookieCas.cs - CAS unit tests for System.Web.HttpCookie
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
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpCookieCas : AspNetHostingMinimal {

		private void GetSetProperties (HttpCookie biscuit)
		{
			Assert.IsNull (biscuit.Domain, "Domain");
			biscuit.Domain = String.Empty;

			Assert.AreEqual (DateTime.MinValue, biscuit.Expires, "Domain");
			biscuit.Expires = DateTime.MaxValue;

			Assert.IsFalse (biscuit.HasKeys, "HasKeys");
			biscuit["mono"] = "monkey";
			Assert.AreEqual ("monkey", biscuit["mono"], "this");

			Assert.IsNull (biscuit.Name, "Name");
			biscuit.Name = "my";

			Assert.AreEqual ("/", biscuit.Path, "Path");
			biscuit.Path = String.Empty;

			Assert.IsFalse (biscuit.Secure, "Secure");
			biscuit.Secure = true;

			Assert.IsTrue (biscuit.Value.IndexOf ("mono=monkey") >= 0, "Value");
			biscuit.Value = "monkey=mono&singe=monkey";
#if NET_2_0
			Assert.IsFalse (biscuit.HttpOnly, "HttpOnly");
			biscuit.HttpOnly = true;
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			HttpCookie biscuit = new HttpCookie (null);
			GetSetProperties (biscuit);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			HttpCookie biscuit = new HttpCookie (null, String.Empty);
			GetSetProperties (biscuit);
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (string) });
			Assert.IsNotNull (ci, ".ctor(Type)");
			return ci.Invoke (new object[1] { null });
		}

		public override Type Type {
			get { return typeof (HttpCookie); }
		}
	}
}
