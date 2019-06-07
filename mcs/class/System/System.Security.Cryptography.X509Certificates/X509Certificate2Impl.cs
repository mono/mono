//
// X509Certificate2Impl.cs
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// Copyright (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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
using System.Text;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates
{
	internal abstract class X509Certificate2Impl : X509CertificateImpl
	{
		public abstract bool Archived {
			get; set;
		}

		public abstract IEnumerable<X509Extension> Extensions {
			get;
		}

		public abstract string FriendlyName {
			get; set;
		}

		public abstract X500DistinguishedName IssuerName {
			get;
		}

		public abstract AsymmetricAlgorithm PrivateKey {
			get; set;
		}

		public abstract PublicKey PublicKey {
			get;
		}

		public abstract string SignatureAlgorithm {
			get;
		}

		public abstract X500DistinguishedName SubjectName {
			get;
		}

		public abstract int Version {
			get;
		}

		internal abstract X509CertificateImplCollection IntermediateCertificates {
			get;
		}

		internal abstract X509Certificate2Impl FallbackImpl {
			get;
		}

		public abstract string GetNameInfo (X509NameType nameType, bool forIssuer);

		public abstract bool Verify (X509Certificate2 thisCertificate);

		public abstract void AppendPrivateKeyInfo (StringBuilder sb);

		public sealed override X509CertificateImpl CopyWithPrivateKey (RSA privateKey)
		{
			var impl = (X509Certificate2Impl)Clone ();
			impl.PrivateKey = privateKey;
			return impl;
		}

		public sealed override X509Certificate CreateCertificate ()
		{
			return new X509Certificate2 (this);
		}

		public abstract void Reset ();
	}
}
