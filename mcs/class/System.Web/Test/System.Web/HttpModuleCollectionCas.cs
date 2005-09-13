//
// HttpModuleCollectionCas.cs 
//	- CAS unit tests for System.Web.HttpModuleCollection
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
	public class HttpModuleCollectionCas : AspNetHostingMinimal {

		private HttpModuleCollection coll;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			coll = new HttpApplication ().Modules;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			Assert.IsNotNull (coll.AllKeys, "AllKeys");
			coll.CopyTo (new object[0], 0);
			Assert.IsNull (coll.Get ("mono"), "Get(string)");
			Assert.IsNull (coll["mono"], "this[string]");
			try {
				Assert.IsNull (coll[0], "this[int]");
			}
			catch (ArgumentOutOfRangeException) {
				// normal (can't avoid it)
			}
			try {
				Assert.IsNull (coll.GetKey (0), "GetKey(int)");
			}
			catch (ArgumentOutOfRangeException) {
				// normal (can't avoid it)
			}
			try {
				Assert.IsNull (coll.Get (0), "Get(int)");
			}
			catch (ArgumentOutOfRangeException) {
				// normal (can't avoid it)
			}
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// no public ctor is available but we know that it's properties don't have any restrictions
			MethodInfo mi = this.Type.GetProperty ("AllKeys").GetGetMethod ();
			Assert.IsNotNull (mi, "get_AllKeys");
			return mi.Invoke (coll, null);
		}

		public override Type Type {
			get { return typeof (HttpModuleCollection); }
		}
	}
}
