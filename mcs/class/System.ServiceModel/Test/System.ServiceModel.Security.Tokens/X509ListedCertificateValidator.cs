//
// X509ListedCertificateValidator.cs
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
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Security.Tokens
{
	public class X509ListedCertificateValidator : X509CertificateValidator
	{
		Collection<X509Certificate2> certs =
			new Collection<X509Certificate2> ();

		public X509ListedCertificateValidator (params X509Certificate2 [] certificates)
			: this ((IEnumerable<X509Certificate2>) certificates)
		{
		}

		public X509ListedCertificateValidator (IEnumerable<X509Certificate2> certificates)
		{
			foreach (X509Certificate2 cert in certificates)
				certs.Add (cert);
		}

		public override void Validate (X509Certificate2 certificate)
		{
			foreach (X509Certificate2 c in certs)
				if (c.Thumbprint == certificate.Thumbprint)
					return;
			throw new ArgumentException ("The argument certificate is not listed as a trusted certificate in this validator.");
		}
	}
}
#endif
