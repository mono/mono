//
// AsymmetricDecryptionKey.cs: Handles WS-Security AsymmetricDecryptionKey
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

	public sealed class AsymmetricDecryptionKey : DecryptionKey {

		private AsymmetricAlgorithm key;

		public AsymmetricDecryptionKey (AsymmetricAlgorithm key) 
		{
			if (key is System.Security.Cryptography.RSACryptoServiceProvider)
				this.key = key;
			else
				throw new SecurityFault ("not RSACryptoServiceProvider", null);
		}
	}
}
