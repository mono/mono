//
// PublicKeyInfo.cs - System.Security.Cryptography.Pkcs.PublicKeyInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class PublicKeyInfo {

		private AlgorithmIdentifier _algorithm;
		private byte[] _key;

		// constructors

		// only used in KeyAgreeRecipientInfo.OriginatorIdentifierOrKey.Value
		// when SubjectIdentifierOrKeyType == PublicKeyInfo
		internal PublicKeyInfo (AlgorithmIdentifier algorithm, byte[] key) 
		{
			_algorithm = algorithm;
			_key = key;
		}

		// properties

		public AlgorithmIdentifier Algorithm {
			get { return _algorithm; }
		}

		public byte[] KeyValue {
			get { return _key; }
		}
	}
}

#endif