//
// Pkcs7Signer.cs - System.Security.Cryptography.Pkcs.Pkcs7Signer
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

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

#if NET_2_0

using System;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs7Signer {

		private SubjectIdentifierType _signer;
		private X509CertificateEx _certificate;
		private Oid _digest;
		private X509IncludeOption _options;
		private Pkcs9AttributeCollection _auth;
		private Pkcs9AttributeCollection _unauth;

		// constructors

		public Pkcs7Signer () 
		{
			_signer = SubjectIdentifierType.IssuerAndSerialNumber;
			_digest = new Oid ("1.3.14.3.2.26");
			_options = X509IncludeOption.ExcludeRoot;
			_auth = new Pkcs9AttributeCollection ();
			_unauth = new Pkcs9AttributeCollection ();
		}

		public Pkcs7Signer (SubjectIdentifierType signerIdentifierType) : this ()
		{
			if (signerIdentifierType == SubjectIdentifierType.Unknown)
				_signer = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				_signer = signerIdentifierType;
		}

		public Pkcs7Signer (SubjectIdentifierType signerIdentifierType, X509CertificateEx certificate) 
			: this (signerIdentifierType)
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (certificate == null)
//				throw new ArgumentNullException ("certificate");
			_certificate = certificate;
		}

		public Pkcs7Signer (X509CertificateEx certificate) : this ()
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (certificate == null)
//				throw new ArgumentNullException ("certificate");
			_certificate = certificate;
		}

		// properties

		public Pkcs9AttributeCollection AuthenticatedAttributes {
			get { return _auth; }
		}

		public X509CertificateEx Certificate {
			get { return _certificate; }
			set { _certificate = value; }
		}

		public Oid DigestAlgorithm {
			get { return _digest; }
			set { _digest = value; }
		} 

		public X509IncludeOption IncludeOption {
			get { return _options; }
			set { _options = value; }
		} 

		public SubjectIdentifierType SignerIdentifierType {
			get { return _signer; }
			set { 
				if (value == SubjectIdentifierType.Unknown)
					throw new ArgumentException ("value");

				_signer = value;
			}
		}

		public Pkcs9AttributeCollection UnauthenticatedAttributes {
			get { return _unauth; }
		}
	}
}

#endif