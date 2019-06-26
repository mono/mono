//
// ClaimTest.cs
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
using System.Net.Mail;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.IdentityModel.Claims
{
	[TestFixture]
	public class ClaimTest
	{
		[Test]
		public void CreateClaims ()
		{
			Claim c;

			// premises
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/right/identity", Rights.Identity, "#1");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/right/possessproperty", Rights.PossessProperty, "#2");

			c = Claim.CreateDnsClaim ("123.45.6.7");
			AssertClaim ("Dns", c, ClaimTypes.Dns, "123.45.6.7", Rights.PossessProperty);

			Uri uri = new Uri ("http://www.example.com");
			c = Claim.CreateUriClaim (uri);
			AssertClaim ("Uri", c, ClaimTypes.Uri, uri, Rights.PossessProperty);

			MailAddress mail = new MailAddress ("rupert@ximian.com");
			c = Claim.CreateMailAddressClaim (mail);
			AssertClaim ("Mail", c, ClaimTypes.Email, mail, Rights.PossessProperty);

			c = Claim.CreateNameClaim ("Rupert");
			AssertClaim ("Name", c, ClaimTypes.Name, "Rupert", Rights.PossessProperty);

			c = Claim.CreateSpnClaim ("foo");
			AssertClaim ("Spn", c, ClaimTypes.Spn, "foo", Rights.PossessProperty);

			c = Claim.CreateUpnClaim ("foo");
			AssertClaim ("Upn", c, ClaimTypes.Upn, "foo", Rights.PossessProperty);

			//SecurityIdentifier sid = new SecurityIdentifier (blah);
			//c = Claim.CreateWindowsSidClaim (sid);
			//AssertClaim ("Sid", c, ClaimTypes.Sid, blah, Rights.PossessProperty);

			byte [] hash = new byte [] {1, 2, 3, 4, 5, 6, 7, 8, 9};
			c = Claim.CreateHashClaim (hash);
			AssertClaim ("Hash", c, ClaimTypes.Hash, hash, Rights.PossessProperty);

			RSA rsa = RSA.Create ();
			c = Claim.CreateRsaClaim (rsa);
			AssertClaim ("Rsa", c, ClaimTypes.Rsa, rsa, Rights.PossessProperty);

			X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			byte [] chash = cert.GetCertHash ();
			c = Claim.CreateThumbprintClaim (chash);
			AssertClaim ("Thumbprint", c, ClaimTypes.Thumbprint, chash, Rights.PossessProperty);

			c = Claim.CreateX500DistinguishedNameClaim (cert.SubjectName);
			AssertClaim ("X500Name", c, ClaimTypes.X500DistinguishedName, cert.SubjectName, Rights.PossessProperty);
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual (
				String.Concat (Rights.PossessProperty, ": ", ClaimTypes.Name),
				Claim.CreateNameClaim ("mono").ToString (),
				"#1");
		}

		[Test]
		public void SystemClaim ()
		{
			Assert.AreEqual (
				String.Concat (Rights.Identity, ": ", ClaimTypes.System),
				Claim.System.ToString (),
				"#1");
			Assert.AreEqual ("System", Claim.System.Resource, "#2");
		}

		public static void AssertClaim (string label, Claim c, string type, object resource, string right)
		{
			Assert.AreEqual (type, c.ClaimType, label + ".ClaimType");
			if (resource != null)
				Assert.AreEqual (resource, c.Resource, label + ".Resource");
			Assert.AreEqual (right, c.Right, label + ".Right");
		}
	}
}
#endif
