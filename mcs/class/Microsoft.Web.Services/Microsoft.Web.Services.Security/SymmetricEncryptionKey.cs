//
// SymmetricEncryptionKey.cs: Handles WS-Security SymmetricEncryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services.Security {

	public class SymmetricEncryptionKey : EncryptionKey {

		private SymmetricAlgorithm algo;

		public SymmetricEncryptionKey () 
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
			algo.Key = keyValue;
		}

		internal SymmetricAlgorithm Algorithm {
			get { return algo; }
		}
	}
}
