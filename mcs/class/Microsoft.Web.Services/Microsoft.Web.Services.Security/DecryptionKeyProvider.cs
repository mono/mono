//
// DecryptionKeyProvider.cs: Decryption Key Provider
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.Xml;

namespace Microsoft.Web.Services.Security {

#if !WSE1
	[ObsoleteAttribute ("Use SecurityTokenManager instead", true)]
#endif
	public class DecryptionKeyProvider : IDecryptionKeyProvider {

		public DecryptionKeyProvider () {}

		public virtual DecryptionKey GetDecryptionKey (string algorithmUri, KeyInfo keyInfo) 
		{
			if (keyInfo == null)
				throw new ArgumentNullException ("keyInfo");

			switch (algorithmUri) {
				case XmlEncryption.AlgorithmURI.RSA15:
					// permission to continue further
					break;
				default: // including null
					return null;
			}

			foreach (KeyInfoClause kic in keyInfo) {
				if (kic is KeyInfoNode) {
					KeyInfoNode kin = (kic as KeyInfoNode);
					if ((kin != null) && (kin.Value.LocalName == WSSecurity.ElementNames.SecurityTokenReference)) {
						SecurityTokenReference str = new SecurityTokenReference (kin.Value);
						if (str.KeyIdentifier != null)
							return str.KeyIdentifier.DecryptionKey;
					}
				}
			}
			return null;
		}
	}
}
