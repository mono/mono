//
// PublicKey.cs - System.Security.Cryptography.PublicKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class PublicKey {

		private AsymmetricAlgorithm _key;
		private AsnEncodedData _keyValue;
		private AsnEncodedData _params;
		private Oid _oid;

		internal PublicKey (Mono.Security.X509.X509Certificate certificate)
		{
			if (certificate.KeyAlgorithm == "1.2.840.113549.1.1.1") {
				_key = certificate.RSA;
			}
			else {
				_key = certificate.DSA;
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
			get { return _key; }
		}

		public Oid Oid {
			get { return _oid; }
		}
	}
}

#endif