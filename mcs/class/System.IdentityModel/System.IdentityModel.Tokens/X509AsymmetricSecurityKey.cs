//
// X509AsymmetricSecurityKey.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace System.IdentityModel.Tokens
{
	public class X509AsymmetricSecurityKey : AsymmetricSecurityKey
	{
		public X509AsymmetricSecurityKey (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			cert = certificate;
		}

		X509Certificate2 cert;

		// AsymmetricSecurityKey implementation

		public override AsymmetricAlgorithm GetAsymmetricAlgorithm (
			string algorithm, bool privateKey)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (privateKey && !cert.HasPrivateKey)
				throw new NotSupportedException ("The certificate does not contain a private key.");

			AsymmetricAlgorithm alg = privateKey ?
				cert.PrivateKey : cert.PublicKey.Key;

			switch (algorithm) {
//			case SignedXml.XmlDsigDSAUrl:
//				if (alg is DSA)
//					return alg;
//				throw new NotSupportedException (String.Format ("The certificate does not contain DSA private key while '{0}' requires it.", algorithm));
			case EncryptedXml.XmlEncRSA15Url:
			case EncryptedXml.XmlEncRSAOAEPUrl:
			case SignedXml.XmlDsigRSASHA1Url:
			case SecurityAlgorithms.RsaSha256Signature:
				if (alg is RSA)
					return alg;
				throw new NotSupportedException (String.Format ("The certificate does not contain RSA private key while '{0}' requires it.", algorithm));
			}

			throw new NotSupportedException (String.Format ("The asymmetric algorithm '{0}' is not supported.", algorithm));
		}

		public override HashAlgorithm GetHashAlgorithmForSignature (
			string algorithm)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			switch (algorithm) {
			//case SignedXml.XmlDsigDSAUrl: // it is documented as supported, but it isn't in reality and it wouldn't be possible.
			case SignedXml.XmlDsigRSASHA1Url:
				return new HMACSHA1 ();
			case SecurityAlgorithms.RsaSha256Signature:
				return new HMACSHA256 ();
			default:
				throw new NotSupportedException (String.Format ("'{0}' Hash algorithm is not supported in this security key.", algorithm));
			}
		}

		[MonoTODO]
		public override AsymmetricSignatureDeformatter GetSignatureDeformatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override AsymmetricSignatureFormatter GetSignatureFormatter (string algorithm)
		{
			throw new NotImplementedException ();
		}

		public override bool HasPrivateKey ()
		{
			return cert.HasPrivateKey;
		}

		// SecurityKey implementation

		public override int KeySize {
			get { return cert.PublicKey.Key.KeySize; }
		}

		public override byte [] DecryptKey (string algorithm, byte [] keyData)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (keyData == null)
				throw new ArgumentNullException ("keyData");

			if (!HasPrivateKey ())
				throw new NotSupportedException ("This X509 certificate does not contain private key.");

			if (cert.PrivateKey.KeyExchangeAlgorithm == null)
				throw new NotSupportedException ("The exchange algorithm of the X509 certificate private key is null");

			switch (algorithm) {
			case EncryptedXml.XmlEncRSA15Url:
			case EncryptedXml.XmlEncRSAOAEPUrl:
				break;
			default:
				throw new NotSupportedException (String.Format ("This X509 security key does not support specified algorithm '{0}'", algorithm));
			}

			bool useOAEP =
				algorithm == EncryptedXml.XmlEncRSAOAEPUrl;
			return EncryptedXml.DecryptKey (keyData, cert.PrivateKey as RSA, useOAEP);
		}

		public override byte [] EncryptKey (string algorithm, byte [] keyData)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (keyData == null)
				throw new ArgumentNullException ("keyData");

			switch (algorithm) {
			case EncryptedXml.XmlEncRSA15Url:
			case EncryptedXml.XmlEncRSAOAEPUrl:
				break;
			default:
				throw new NotSupportedException (String.Format ("This X509 security key does not support specified algorithm '{0}'", algorithm));
			}

			bool useOAEP =
				algorithm == EncryptedXml.XmlEncRSAOAEPUrl;

			return EncryptedXml.EncryptKey (keyData, cert.PublicKey.Key as RSA, useOAEP);
		}

		public override bool IsAsymmetricAlgorithm (string algorithm)
		{
			return GetAlgorithmSupportType (algorithm) == AlgorithmSupportType.Asymmetric;
		}

		public override bool IsSupportedAlgorithm (string algorithm)
		{
			switch (algorithm) {
			case SecurityAlgorithms.RsaV15KeyWrap:
			case SecurityAlgorithms.RsaOaepKeyWrap:
			case SecurityAlgorithms.RsaSha1Signature:
			case SecurityAlgorithms.RsaSha256Signature:
				return true;
			default:
				return false;
			}
		}

		public override bool IsSymmetricAlgorithm (string algorithm)
		{
			return GetAlgorithmSupportType (algorithm) == AlgorithmSupportType.Symmetric;
		}
	}
}
