//
// SymmetricEncryptionKey.cs: Handles WS-Security SymmetricEncryptionKey
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

	public class SymmetricEncryptionKey : EncryptionKey {

		private SymmetricAlgorithm algo;
		private byte[] key;

		public SymmetricEncryptionKey() 
		{
			// uses TripleDESCryptoServiceProvider - not default (Rjindael)
			algo = SymmetricAlgorithm.Create ("TripleDES");
		}

		public SymmetricEncryptionKey (SymmetricAlgorithm key) 
		{
			if (key == null)
				throw new NullReferenceException ("algo");
			algo = key;
		}

		public SymmetricEncryptionKey (SymmetricAlgorithm key, byte[] keyValue) 
		{
			algo = key;
			this.key = keyValue;
		}
	}
}
