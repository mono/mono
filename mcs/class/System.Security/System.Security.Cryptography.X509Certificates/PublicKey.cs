//
// PublicKey.cs - System.Security.Cryptography.PublicKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
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

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class PublicKey {

		private AsymmetricAlgorithm _key;
		private AsnEncodedData _keyValue;
		private AsnEncodedData _params;
		private Oid _oid;

		[MonoTODO]
		public PublicKey (Oid oid, AsnEncodedData parameters, AsnEncodedData keyValue)
		{
			_oid = oid;
			_params = parameters;
			_keyValue = keyValue;
		}

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
