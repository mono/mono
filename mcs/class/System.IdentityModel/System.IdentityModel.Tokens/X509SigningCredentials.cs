//
// X509SigningCredentials.cs
//
// Author:
//   Noesis Labs (Ryan.Melena@noesislabs.com)
//
// Copyright (C) 2014 Noesis Labs, LLC  https://noesislabs.com
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
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Tokens
{
	public class X509SigningCredentials : SigningCredentials
	{
		public X509Certificate2 Certificate { get; private set; }

		public X509SigningCredentials (X509Certificate2 certificate)
			: this (certificate, X509SigningCredentials.GetSecurityKeyIdentifier (certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, SecurityKeyIdentifier ski)
			: this (certificate, ski, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, string signatureAlgorithm, string digestAlgorithm)
			: this (certificate, X509SigningCredentials.GetSecurityKeyIdentifier (certificate), signatureAlgorithm, digestAlgorithm)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, SecurityKeyIdentifier ski, string signatureAlgorithm, string digestAlgorithm)
			: base (new X509SecurityToken (certificate).SecurityKeys[0], signatureAlgorithm, digestAlgorithm, ski)
		{
			Certificate = certificate;
		}

		private static SecurityKeyIdentifier GetSecurityKeyIdentifier (X509Certificate2 certificate) {
			return new SecurityKeyIdentifier (new X509SecurityToken (certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause> ());
		}
	}
}
