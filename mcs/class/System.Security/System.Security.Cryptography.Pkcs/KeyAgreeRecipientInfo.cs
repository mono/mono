//
// KeyAgreeRecipientInfo.cs - System.Security.Cryptography.Pkcs.KeyAgreeRecipientInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	[MonoTODO]
	public sealed class KeyAgreeRecipientInfo : RecipientInfo {

		// only accessible from EnvelopedPkcs7.RecipientInfos
		internal KeyAgreeRecipientInfo () {}

		public DateTime Date {
			get { return DateTime.MinValue; }
		}

		public override byte[] EncryptedKey {
			get { return null; }
		}

		public override AlgorithmIdentifier KeyEncryptionAlgorithm {
			get { return null; }
		}

		public SubjectIdentifierOrKey OriginatorIdentifierOrKey {
			get { return null; }
		}

		public CryptographicAttribute OtherKeyAttribute {
			get { return null; }
		}

		public override SubjectIdentifier RecipientIdentifier {
			get { return null; }
		}

		public override int Version {
			get { return 0; }
		}
	}
}

#endif