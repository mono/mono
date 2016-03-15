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
namespace System.Security.Cryptography.X509Certificates
{
	internal abstract class X509Certificate2Impl : X509CertificateImpl
	{
#if SECURITY_DEP

		public abstract bool Archived {
			get; set;
		}

		public abstract X509ExtensionCollection Extensions {
			get;
		}

		public abstract bool HasPrivateKey {
			get;
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

		public abstract Oid SignatureAlgorithm {
			get;
		}

		public abstract X500DistinguishedName SubjectName {
			get;
		}

		public abstract int Version {
			get;
		}

		public abstract string GetNameInfo (X509NameType nameType, bool forIssuer);

		public abstract void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags);

		public abstract byte[] Export (X509ContentType contentType, string password);

		public abstract bool Verify (X509Certificate2 thisCertificate);

		public abstract void Reset ();

#endif
	}
}
