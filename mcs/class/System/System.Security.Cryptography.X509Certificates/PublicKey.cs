//
// PublicKey.cs - System.Security.Cryptography.PublicKey
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if SECURITY_DEP || MOONLIGHT

using Mono.Security;
using Mono.Security.Cryptography;
using MSX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class PublicKey {

		private const string rsaOid = "1.2.840.113549.1.1.1";
		private const string dsaOid = "1.2.840.10040.4.1";

		private AsymmetricAlgorithm _key;
		private AsnEncodedData _keyValue;
		private AsnEncodedData _params;
		private Oid _oid;

		public PublicKey (Oid oid, AsnEncodedData parameters, AsnEncodedData keyValue)
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			if (keyValue == null)
				throw new ArgumentNullException ("keyValue");

			_oid = new Oid (oid);
			_params = new AsnEncodedData (parameters);
			_keyValue = new AsnEncodedData (keyValue);
		}

		internal PublicKey (MSX.X509Certificate certificate)
		{
			// note: _key MUSTonly contains the public part of the key
			bool export_required = true;

			if (certificate.KeyAlgorithm == rsaOid) {
#if !MOONLIGHT
				// shortcut export/import in the case the private key isn't available
				RSACryptoServiceProvider rcsp = (certificate.RSA as RSACryptoServiceProvider);
				if ((rcsp != null) && rcsp.PublicOnly) {
					_key = certificate.RSA;
					export_required = false;
				} else 
#endif
				{
					RSAManaged rsam = (certificate.RSA as RSAManaged);
					if ((rsam != null) && rsam.PublicOnly) {
						_key = certificate.RSA;
						export_required = false;
					}
				}

				if (export_required) {
					RSAParameters rsap = certificate.RSA.ExportParameters (false);
					_key = RSA.Create ();
					(_key as RSA).ImportParameters (rsap);
				}
			} else {
#if !MOONLIGHT
				// shortcut export/import in the case the private key isn't available
				DSACryptoServiceProvider dcsp = (certificate.DSA as DSACryptoServiceProvider);
				if ((dcsp != null) && dcsp.PublicOnly) {
					_key = certificate.DSA;
					export_required = false;
				}
				// note: DSAManaged isn't available in Mono.Security due to a bug in Fx 1.x

				if (export_required) {
					DSAParameters rsap = certificate.DSA.ExportParameters (false);
					_key = DSA.Create ();
					(_key as DSA).ImportParameters (rsap);
				}
#endif
			}

			_oid = new Oid (certificate.KeyAlgorithm);
			_keyValue = new AsnEncodedData (_oid, certificate.PublicKey);
			_params = new AsnEncodedData (_oid, certificate.KeyAlgorithmParameters);
		}

		// properties

		public AsnEncodedData EncodedKeyValue {
			get { return _keyValue; }
		}

		public AsnEncodedData EncodedParameters {
			get { return _params; }
		}

		public AsymmetricAlgorithm Key {
			get {
				if (_key == null) {
					switch (_oid.Value) {
					case rsaOid:
						_key = DecodeRSA (_keyValue.RawData);
						break;
					case dsaOid:
						_key = DecodeDSA (_keyValue.RawData, _params.RawData);
						break;
					default:
						string msg = Locale.GetText ("Cannot decode public key from unknown OID '{0}'.", _oid.Value);
						throw new NotSupportedException (msg);
					}
				}
				return _key;
			}
		}

		public Oid Oid {
			get { return _oid; }
		}

		// private stuff

		static private byte[] GetUnsignedBigInteger (byte[] integer) 
		{
			if (integer [0] != 0x00)
				return integer;

			// this first byte is added so we're sure it's an unsigned integer
			// however we can't feed it into RSAParameters or DSAParameters
			int length = integer.Length - 1;
			byte[] uinteger = new byte [length];
			Buffer.BlockCopy (integer, 1, uinteger, 0, length);
			return uinteger;
		}

		static internal DSA DecodeDSA (byte[] rawPublicKey, byte[] rawParameters)
		{
			DSAParameters dsaParams = new DSAParameters ();
			try {
				// for DSA rawPublicKey contains 1 ASN.1 integer - Y
				ASN1 pubkey = new ASN1 (rawPublicKey);
				if (pubkey.Tag != 0x02)
					throw new CryptographicException (Locale.GetText ("Missing DSA Y integer."));
				dsaParams.Y = GetUnsignedBigInteger (pubkey.Value);

				ASN1 param = new ASN1 (rawParameters);
				if ((param == null) || (param.Tag != 0x30) || (param.Count < 3))
					throw new CryptographicException (Locale.GetText ("Missing DSA parameters."));
				if ((param [0].Tag != 0x02) || (param [1].Tag != 0x02) || (param [2].Tag != 0x02))
					throw new CryptographicException (Locale.GetText ("Invalid DSA parameters."));

				dsaParams.P = GetUnsignedBigInteger (param [0].Value);
				dsaParams.Q = GetUnsignedBigInteger (param [1].Value);
				dsaParams.G = GetUnsignedBigInteger (param [2].Value);
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Error decoding the ASN.1 structure.");
				throw new CryptographicException (msg, e);
			}

#if MOONLIGHT
			DSA dsa = (DSA) new DSAManaged (dsaParams.Y.Length << 3);
#else
			DSA dsa = (DSA) new DSACryptoServiceProvider (dsaParams.Y.Length << 3);
#endif
			dsa.ImportParameters (dsaParams);
			return dsa;
		}

		static internal RSA DecodeRSA (byte[] rawPublicKey)
		{
			RSAParameters rsaParams = new RSAParameters ();
			try {
				// for RSA rawPublicKey contains 2 ASN.1 integers
				// the modulus and the public exponent
				ASN1 pubkey = new ASN1 (rawPublicKey);
				if (pubkey.Count == 0)
					throw new CryptographicException (Locale.GetText ("Missing RSA modulus and exponent."));
				ASN1 modulus = pubkey [0];
				if ((modulus == null) || (modulus.Tag != 0x02))
					throw new CryptographicException (Locale.GetText ("Missing RSA modulus."));
				ASN1 exponent = pubkey [1];
				if (exponent.Tag != 0x02)
					throw new CryptographicException (Locale.GetText ("Missing RSA public exponent."));

				rsaParams.Modulus = GetUnsignedBigInteger (modulus.Value);
				rsaParams.Exponent = exponent.Value;
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Error decoding the ASN.1 structure.");
				throw new CryptographicException (msg, e);
			}

			int keySize = (rsaParams.Modulus.Length << 3);
#if MOONLIGHT
			RSA rsa = (RSA) new RSAManaged (keySize);
#else
			RSA rsa = (RSA) new RSACryptoServiceProvider (keySize);
#endif
			rsa.ImportParameters (rsaParams);
			return rsa;
		}
	}
}

#endif
