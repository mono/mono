//
// EndpointIdentity.cs
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
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;


namespace System.ServiceModel
{
	public abstract class EndpointIdentity
	{
		Claim claim;
		IEqualityComparer<Claim> comparer;

		protected EndpointIdentity ()
		{
		}

		public Claim IdentityClaim {
			get { return claim; }
		}

		public static EndpointIdentity CreateDnsIdentity (string dnsName)
		{
			return new DnsEndpointIdentity (dnsName);
		}

		public static EndpointIdentity CreateIdentity (Claim identity)
		{
			if (identity == null)
				throw new ArgumentNullException ();

			if (identity.ClaimType == ClaimTypes.Dns)
				return CreateDnsIdentity ((string) identity.Resource);
			else if (identity.ClaimType == ClaimTypes.Rsa) {
				if (identity.Resource is string)
					return CreateRsaIdentity ((string) identity.Resource);
				else if (identity.Resource is X509Certificate2)
					return CreateRsaIdentity ((X509Certificate2) identity.Resource);
			}
			else if (identity.ClaimType == ClaimTypes.Thumbprint)
				return CreateX509CertificateIdentity ((X509Certificate2) identity.Resource);
			else if (identity.ClaimType == ClaimTypes.Spn)
				return CreateSpnIdentity ((string) identity.Resource);
			else if (identity.ClaimType == ClaimTypes.Upn)
				return CreateSpnIdentity ((string) identity.Resource);

			throw new NotSupportedException (String.Format ("Claim type '{0}' cannot be used to create an endpoint identity.", identity.ClaimType));
		}

		public static EndpointIdentity CreateRsaIdentity (string publicKey)
		{
			return new RsaEndpointIdentity (publicKey);
		}

		public static EndpointIdentity CreateRsaIdentity (
			X509Certificate2 certificate)
		{
			return new RsaEndpointIdentity (certificate);
		}

		public static EndpointIdentity CreateSpnIdentity (string spnName)
		{
			return new SpnEndpointIdentity (spnName);
		}

		public static EndpointIdentity CreateUpnIdentity (string upnName)
		{
			return new UpnEndpointIdentity (upnName);
		}

		public static EndpointIdentity CreateX509CertificateIdentity (
			X509Certificate2 certificate)
		{
			return new X509CertificateEndpointIdentity (certificate);
		}

		public static EndpointIdentity CreateX509CertificateIdentity (
			X509Certificate2 primaryCertificate,
			X509Certificate2Collection supportingCertificates)
		{
			return new X509CertificateEndpointIdentity (primaryCertificate, supportingCertificates);
		}

		public override bool Equals (object obj)
		{
			EndpointIdentity e = obj as EndpointIdentity;
			return e != null && comparer.Equals (claim, e.claim);
		}

		public override int GetHashCode ()
		{
			return comparer.GetHashCode (claim);
		}

		public override string ToString ()
		{
			return String.Concat ("identity(", claim, ")");
		}

		protected void Initialize (Claim identityClaim)
		{
			Initialize (identityClaim, Claim.DefaultComparer);
		}

		protected void Initialize (Claim identityClaim, IEqualityComparer<Claim> claimComparer)
		{
			if (identityClaim == null)
				throw new ArgumentNullException ("identityClaim");
			if (claimComparer == null)
				throw new ArgumentNullException ("claimComparer");
			this.claim = identityClaim;
			this.comparer = claimComparer;
		}
	}
}
