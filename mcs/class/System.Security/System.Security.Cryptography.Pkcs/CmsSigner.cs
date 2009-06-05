//
// System.Security.Cryptography.Pkcs.CmsSigner class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public sealed class CmsSigner {

		private SubjectIdentifierType _signer;
		private X509Certificate2 _certificate;
		private X509Certificate2Collection _coll;
		private Oid _digest;
		private X509IncludeOption _options;
		private CryptographicAttributeObjectCollection _signed;
		private CryptographicAttributeObjectCollection _unsigned;

		// constructors

		public CmsSigner () 
		{
			_signer = SubjectIdentifierType.IssuerAndSerialNumber;
			_digest = new Oid ("1.3.14.3.2.26");
			_options = X509IncludeOption.ExcludeRoot;
			_signed = new CryptographicAttributeObjectCollection ();
			_unsigned = new CryptographicAttributeObjectCollection ();
			_coll = new X509Certificate2Collection ();
		}

		public CmsSigner (SubjectIdentifierType signerIdentifierType) : this ()
		{
			if (signerIdentifierType == SubjectIdentifierType.Unknown)
				_signer = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				_signer = signerIdentifierType;
		}

		public CmsSigner (SubjectIdentifierType signerIdentifierType, X509Certificate2 certificate) 
			: this (signerIdentifierType)
		{
			_certificate = certificate;
		}

		public CmsSigner (X509Certificate2 certificate) : this ()
		{
			_certificate = certificate;
		}

		[MonoTODO]
		public CmsSigner (CspParameters parameters) : this ()
		{
		}		

		// properties

		public CryptographicAttributeObjectCollection SignedAttributes {
			get { return _signed; }
		}

		public X509Certificate2 Certificate {
			get { return _certificate; }
			set { _certificate = value; }
		}

		public X509Certificate2Collection Certificates {
			get { return _coll; }
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

		public CryptographicAttributeObjectCollection UnsignedAttributes {
			get { return _unsigned; }
		}
	}
}

#endif
