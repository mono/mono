//
// X509CertificateClaimSet.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Claims
{
	public class X509CertificateClaimSet : ClaimSet, IDisposable
	{
		X509Certificate2 cert;
		ClaimSet issuer;
		List<Claim> claims = new List<Claim> ();

		// Constructors

		class X509IdentityClaimSet : DefaultClaimSet
		{
			public X509IdentityClaimSet (Claim c)
			{
				Initialize (this, new Claim [] {c});
			}
		}

		public X509CertificateClaimSet (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			this.cert = certificate;
			Claim ident = new Claim (ClaimTypes.Thumbprint, cert.Thumbprint, Rights.Identity);
//			issuer = new X509IdentityClaimSet (ident);
			claims.Add (ident);
			//claims.Add (Claim.CreateX500DistinguishedNameClaim (cert.SubjectName));
			//claims.Add (Claim.CreateNameClaim (cert.SubjectName.Name));
			RSA rsa = cert.PublicKey.Key as RSA;
			if (rsa != null)
				claims.Add (Claim.CreateRsaClaim (rsa));
			claims.Add (Claim.CreateThumbprintClaim (cert.GetCertHash ()));
			// FIXME: where is DNS info for X509 cert?
			claims.Add (Claim.CreateDnsClaim (null));
		}

		// Properties

		public override int Count {
			get { return claims.Count; }
		}

		public override ClaimSet Issuer {
			get {
				if (issuer == null) {
					X509Chain chain = new X509Chain ();
					chain.Build (cert);
					if (chain.ChainElements.Count <= 1)
						throw new InvalidOperationException ("hmm, the certificate chain does not contain enough information");
					issuer = new X509CertificateClaimSet (chain.ChainElements [1].Certificate);
				}
				return issuer;
			}
		}

		public override Claim this [int index] {
			get { return claims [index]; }
		}

		[MonoTODO ("use ParseExact")]
		public DateTime ExpirationTime {
			get { return DateTime.Parse (cert.GetExpirationDateString ()); }
		}

		public X509Certificate2 X509Certificate {
			get { return cert; }
		}

		// Methods

		[MonoTODO]
		public void Dispose ()
		{
		}

		[MonoTODO]
		public override IEnumerable<Claim> FindClaims (
			string claimType, string right)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerator<Claim> GetEnumerator ()
		{
			return claims.GetEnumerator ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
