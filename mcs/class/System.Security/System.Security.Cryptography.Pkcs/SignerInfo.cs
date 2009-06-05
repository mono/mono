//
// System.Security.Cryptography.Pkcs.SignerInfo class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

	public sealed class SignerInfo {

		private SubjectIdentifier _signer;
		private X509Certificate2 _certificate;
		private Oid _digest;
		private SignerInfoCollection _counter;
		private CryptographicAttributeObjectCollection _signed;
		private CryptographicAttributeObjectCollection _unsigned;
		private int _version;

		// only accessible from SignedPkcs7.SignerInfos
		internal SignerInfo (string hashName, X509Certificate2 certificate, SubjectIdentifierType type, object o, int version)
		{
			_digest = new Oid (CryptoConfig.MapNameToOID (hashName));
			_certificate = certificate;
			_counter = new SignerInfoCollection ();
			_signed = new CryptographicAttributeObjectCollection ();
			_unsigned = new CryptographicAttributeObjectCollection ();
			_signer = new SubjectIdentifier (type, o);
			_version = version;
		}

		// properties

		public CryptographicAttributeObjectCollection SignedAttributes {
			get { return _signed; }
		} 

		public X509Certificate2 Certificate {
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

		public CryptographicAttributeObjectCollection UnsignedAttributes {
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
		public void CheckSignature (X509Certificate2Collection extraStore, bool verifySignatureOnly)
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
