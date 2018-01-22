//
// CustomUserNameSecurityTokenAuthenticatorTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

using Authenticator = System.IdentityModel.Selectors.CustomUserNameSecurityTokenAuthenticator;
using PolicyCollection = System.Collections.ObjectModel.ReadOnlyCollection<System.IdentityModel.Policy.IAuthorizationPolicy>;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class CustomUserNameSecurityTokenAuthenticatorTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorCertNull ()
		{
			new Authenticator (null);
		}

		[Test]
		public void Validation ()
		{
			Authenticator a = new Authenticator (
				UserNamePasswordValidator.None);
			PolicyCollection pl = a.ValidateToken (new UserNameSecurityToken ("mono", "mono"));
			Assert.AreEqual (1, pl.Count, "#1");
			IAuthorizationPolicy p = pl [0];
			Assert.AreEqual (ClaimSet.System, p.Issuer, "#2");
			TestEvaluationContext ec = new TestEvaluationContext ();
			object o = null;
			Assert.IsTrue (p.Evaluate (ec, ref o), "#3");
			Assert.AreEqual (DateTime.MaxValue.AddDays (-1), ec.ExpirationTime, "#4");
			IList<IIdentity> identities = ec.Properties ["Identities"] as IList<IIdentity>;
			Assert.IsNotNull (identities, "#5");
			Assert.AreEqual (1, identities.Count, "#6");
			IIdentity ident = identities [0];
			Assert.AreEqual (true, ident.IsAuthenticated, "#6-2");
			// it's implementation details.
			//Assert.AreEqual ("NoneUserNamePasswordValidator", ident.AuthenticationType, "#6-3");
			Assert.AreEqual ("mono", ident.Name, "#6-4");
			Assert.AreEqual (1, ec.ClaimSets.Count, "#7");

			Assert.IsTrue (p.Evaluate (ec, ref o), "#8");
			identities = ec.Properties ["Identities"] as IList<IIdentity>;
			Assert.AreEqual (2, identities.Count, "#9");
			Assert.AreEqual (2, ec.ClaimSets.Count, "#10");
		}
	}
}
#endif