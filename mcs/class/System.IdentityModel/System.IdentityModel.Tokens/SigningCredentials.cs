//
// SigningCredentials.cs
//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
	public class SigningCredentials
	{
		SecurityKey key;
		string sig_alg;
		string dig_alg;
		SecurityKeyIdentifier identifier;

		public SigningCredentials (SecurityKey signingKey, string signatureAlgorithm, string digestAlgorithm)
		{
			if (signingKey == null)
				throw new ArgumentNullException ("signingKey");
			if (signatureAlgorithm == null)
				throw new ArgumentNullException ("signatureAlgorithm");
			if (digestAlgorithm == null)
				throw new ArgumentNullException ("digestAlgorithm");
			this.key = signingKey;
			this.sig_alg = signatureAlgorithm;
			this.dig_alg = digestAlgorithm;
		}

		public SigningCredentials (SecurityKey signingKey, string signatureAlgorithm, string digestAlgorithm, SecurityKeyIdentifier signingKeyIdentifier)
			: this (signingKey, signatureAlgorithm, digestAlgorithm)
		{
			if (signingKeyIdentifier == null)
				throw new ArgumentNullException ("signingKeyIdentifier");
			this.identifier = signingKeyIdentifier;
		}

		public string DigestAlgorithm {
			get { return dig_alg; }
		}

		public string SignatureAlgorithm {
			get { return sig_alg; }
		}

		public SecurityKey SigningKey {
			get { return key; }
		}

		public SecurityKeyIdentifier SigningKeyIdentifier {
			get { return identifier; }
		}
	}
}
