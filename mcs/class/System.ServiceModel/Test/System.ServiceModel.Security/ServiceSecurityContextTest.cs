//
// ServiceSecurityContextTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

using PolicyList = System.Collections.ObjectModel.ReadOnlyCollection<System.IdentityModel.Policy.IAuthorizationPolicy>;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceSecurityContextTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullArgs1 ()
		{
			new ServiceSecurityContext (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullArgs2 ()
		{
			new ServiceSecurityContext ((AuthorizationContext) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullArgs3 ()
		{
			new ServiceSecurityContext ((PolicyList) null);
		}

		[Test]
		public void Constructor ()
		{
			ServiceSecurityContext c = new ServiceSecurityContext (new PolicyList (new IAuthorizationPolicy [0]));
			Assert.IsNotNull (c.AuthorizationContext, "#1");
			Assert.AreEqual (0, c.AuthorizationPolicies.Count, "#2");
			// it is somehow treated as anonymous ...
			Assert.IsTrue (c.IsAnonymous, "#3");
			// FIXME: test PrimaryIdentity
		}

		[Test]
		public void Anonymous ()
		{
			ServiceSecurityContext c = ServiceSecurityContext.Anonymous;
			Assert.IsNotNull (c.AuthorizationContext, "#1");
			Assert.AreEqual (0, c.AuthorizationPolicies.Count, "#2");
			Assert.IsTrue (c.IsAnonymous, "#3");
			// FIXME: test PrimaryIdentity
		}
	}
}
#endif

