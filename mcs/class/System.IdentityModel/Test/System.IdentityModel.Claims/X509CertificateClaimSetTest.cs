//
// X509CertificateClaimSetTest.cs
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
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Net.Mail;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.IdentityModel.Claims
{
	[TestFixture]
	public class X509CertificateClaimSetTest
	{
		static X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");

		[Test]
		[Ignore ("not up to date")] // X509Chain
		public void DefaultValues ()
		{
			X509Chain chain = new X509Chain ();
			chain.Build (cert);
			Assert.IsTrue (chain.ChainElements.Count > 1, "#0");
			ClaimSet cs = new X509CertificateClaimSet (cert);
			ClaimSet ident = cs.Issuer;
			X509CertificateClaimSet x509is = ident as X509CertificateClaimSet;
			Assert.IsNotNull (x509is, "#0-2");
			Assert.AreEqual (chain.ChainElements [1].Certificate, x509is.X509Certificate, "#0-3");
			Assert.AreEqual (6, cs.Count, "#1");
			Assert.AreEqual (6, ident.Issuer.Count, "#2");
			Assert.IsFalse (cs.ContainsClaim (Claim.System), "#3");
			List<string> d = new List<string> ();
			foreach (Claim c in cs) {
				if (c.ClaimType != ClaimTypes.Thumbprint)
					Assert.AreEqual (Rights.PossessProperty, c.Right, "#4");
				d.Add (c.ClaimType);
			}
			Assert.IsTrue (d.Contains (ClaimTypes.X500DistinguishedName), "#5");
			Assert.IsTrue (d.Contains (ClaimTypes.Thumbprint), "#6");
			Assert.IsTrue (d.Contains (ClaimTypes.Dns), "#7");
			Assert.IsTrue (d.Contains (ClaimTypes.Rsa), "#8");
			Assert.IsTrue (d.Contains (ClaimTypes.Name), "#9");
		}
	}
}
#endif
