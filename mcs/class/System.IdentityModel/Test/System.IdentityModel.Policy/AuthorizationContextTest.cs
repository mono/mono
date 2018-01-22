//
// AuthorizationContextTest.cs
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
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Claims
{
	[TestFixture]
	public class AuthorizationContextTest
	{
		class MyAuthorizationPolicy : IAuthorizationPolicy
		{
			string id = "uuid:" + Guid.NewGuid ();

			public string Id {
				get { return id; }
			}

			public ClaimSet Issuer {
				get { return ClaimSet.System; }
			}

			public bool Evaluate (EvaluationContext ctx, ref object state)
			{
				return true;
			}
		}

		[Test]
		public void CreateDefaultAuthorizationContext ()
		{
			AuthorizationContext a =
				AuthorizationContext.CreateDefaultAuthorizationContext (new IAuthorizationPolicy [0]);
			Assert.AreEqual (DateTime.MaxValue.AddDays (-1), a.ExpirationTime, "#1-1");
			Assert.AreEqual (0, a.Properties.Count, "#1-2");
			Assert.AreEqual (0, a.ClaimSets.Count, "#1-3");

			a = AuthorizationContext.CreateDefaultAuthorizationContext (new IAuthorizationPolicy [] { new MyAuthorizationPolicy ()});
			Assert.AreEqual (DateTime.MaxValue.AddDays (-1), a.ExpirationTime, "#2-1");
			Assert.AreEqual (0, a.Properties.Count, "#2-2");
			Assert.AreEqual (0, a.ClaimSets.Count, "#2-3");
		}
	}
}
#endif