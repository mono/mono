//
// Pkcs7Signer.cs - System.Security.Cryptography.Pkcs.Pkcs7Signer
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

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