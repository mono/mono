//
// SignerInfo.cs - System.Security.Cryptography.Pkcs.SignerInfo
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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

	public sealed class SignerInfo {

		private SubjectIdentifier _signer;
		private X509CertificateEx _certificate;
		private Oid _digest;
		private SignerInfoCollection _counter;
		private CryptographicAttributeCollection _signed;
		private CryptographicAttributeCollection _unsigned;
		private int _version;

		// only accessible from SignedPkcs7.SignerInfos
		internal SignerInfo (string hashOid, X509CertificateEx certificate, SubjectIdentifierType type, object o, int version)
		{
			_digest = new Oid (hashOid);
			_certificate = certificate;
			_counter = new SignerInfoCollection ();
			_signed = new CryptographicAttributeCollection ();
			_unsigned = new CryptographicAttributeCollection ();
			_signer = new SubjectIdentifier (type, o);
			_version = version;
		}

		// properties

		public CryptographicAttributeCollection SignedAttributes {
			get { return _signed; }
		} 

		public X509CertificateEx Certificate {
			get { return _certificate; }
		}

		public SignerInfoCollection CounterSignerInfos {
			get { return _counter; }
		}

		public Oid DigestAlgorithm {
			get { return _digest; }
		}

		public SubjectIdentifier SignerIdentifier {
			get { return _signer; }
		}

		public CryptographicAttributeCollection UnsignedAttributes {
			get { return _unsigned; }
		}

		public int Version {
			get { return _version; }
		}

		// methods

		[MonoTODO]
		public void CheckHash ()
		{
		}

		[MonoTODO]
		public void CheckSignature (bool verifySignatureOnly)
		{
		}

		[MonoTODO]
		public void CheckSignature (X509CertificateExCollection extraStore, bool verifySignatureOnly)
		{
		}

		[MonoTODO]
		public void ComputeCounterSignature ()
		{
		}

		[MonoTODO]
		public void ComputeCounterSignature (CmsSigner signer)
		{
		}

		[MonoTODO]
		public void RemoveCounterSignature (SignerInfo counterSignerInfo)
		{
		}

		[MonoTODO]
		public void RemoveCounterSignature (int index)
		{
		}
	}
}

#endif
