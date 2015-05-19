//
// KeyTransRecipientInfo.cs - System.Security.Cryptography.Pkcs.KeyTransRecipientInfo
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

#if SECURITY_DEP

using System.Collections;

namespace System.Security.Cryptography.Pkcs {

	public sealed class KeyTransRecipientInfo : RecipientInfo {

		private byte[] _encryptedKey;
		private AlgorithmIdentifier _keyEncryptionAlgorithm;
		private SubjectIdentifier _recipientIdentifier;
		private int _version;

		// only accessible from EnvelopedCms.RecipientInfos
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
