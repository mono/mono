//
// KeyTransRecipientInfo.cs - System.Security.Cryptography.Pkcs.KeyTransRecipientInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.Pkcs {

	public sealed class KeyTransRecipientInfo : RecipientInfo {

		private byte[] _encryptedKey;
		private AlgorithmIdentifier _keyEncryptionAlgorithm;
		private SubjectIdentifier _recipientIdentifier;
		private int _version;

		// only accessible from EnvelopedPkcs7.RecipientInfos
		internal KeyTransRecipientInfo (byte[] encryptedKey, AlgorithmIdentifier keyEncryptionAlgorithm, SubjectIdentifier recipientIdentifier, int version)
			: base (RecipientInfoType.KeyTransport)
		{
			_encryptedKey = encryptedKey;
			_keyEncryptionAlgorithm = keyEncryptionAlgorithm;
			_recipientIdentifier = recipientIdentifier;
			_version = version;
		}

		public override byte[] EncryptedKey {
			get { return _encryptedKey; }
		}

		public override AlgorithmIdentifier KeyEncryptionAlgorithm {
			get { return _keyEncryptionAlgorithm; }
		} 

		public override SubjectIdentifier RecipientIdentifier {
			get { return _recipientIdentifier; }
		} 

		public override int Version {
			get { return _version; }
		}
	}
}

#endif