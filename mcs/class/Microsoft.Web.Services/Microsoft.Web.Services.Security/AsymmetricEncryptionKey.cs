//
// AsymmetricEncryptionKey.cs: Handles WS-Security AsymmetricEncryptionKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Microsoft.Web.Services.Security {

	public sealed class AsymmetricEncryptionKey : EncryptionKey {

		private AsymmetricAlgorithm key;

		public AsymmetricEncryptionKey (AsymmetricAlgorithm key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			this.key = key;
			// TODO ? impact on KeyInfo ? not seen in tests !!!
		}

		internal AsymmetricAlgorithm Algorithm {
			get { return key; }
		}
	}
}
