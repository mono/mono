//
// SymmetricDecryptionKey.cs: Handles WS-Security SymmetricDecryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Security.Cryptography;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services.Security {

	public sealed class SymmetricDecryptionKey : DecryptionKey {

		private SymmetricAlgorithm algo;
		private byte[] key;

		private bool IsSupported (SymmetricAlgorithm algo) 
		{
			return ((algo is Rijndael) || (algo is TripleDES));
		}

		public SymmetricDecryptionKey (SymmetricAlgorithm key) 
		{
			if (!IsSupported (key))
				throw new SecurityFault ("Unsupported algorithm", null);
			algo = key;
		}

		public SymmetricDecryptionKey (SymmetricAlgorithm key, byte[] keyValue) 
		{
			if (!IsSupported (key))
				throw new SecurityFault ("Unsupported algorithm", null);
			if (keyValue == null)
				throw new ArgumentNullException ("KeyValue");

			algo = key;
			this.key = keyValue;
		}
	}
}
