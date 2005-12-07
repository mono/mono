//
// HttpCacheVaryByParamsCas.cs 
//	- CAS unit tests for System.Web.HttpCacheVaryByParams
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
	public class HttpCacheVaryByParamsCas : AspNetHostingMinimal {

		private HttpResponse response;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			response = new HttpResponse (Console.Out);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			HttpCacheVaryByParams cache = response.Cache.VaryByParams;
			Assert.IsFalse (cache.IgnoreParams, "IgnoreParams");
			cache.IgnoreParams = true;
			Assert.IsFalse (cache["mono"], "this[string]");
			cache["mono"] = true;
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// no public ctor is available but we know that it's properties don't have any restrictions
			MethodInfo mi = this.Type.GetProperty ("IgnoreParams").GetGetMethod ();
			Assert.IsNotNull (mi, "get_IgnoreParams");
			return mi.Invoke (response.Cache.VaryByParams, null);
		}

		public override Type Type {
			get { return typeof (HttpCacheVaryByParams); }
		}
	}
}
