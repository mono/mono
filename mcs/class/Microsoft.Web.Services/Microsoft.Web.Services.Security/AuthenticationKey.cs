//
// AuthenticationKey.cs: Handles WS-Security AuthenticationKey
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

using System.Security.Cryptography;
// temp
//using System.Security.Cryptography.Xml;

namespace Microsoft.Web.Services.Security {

	public class AuthenticationKey {

		private AsymmetricAlgorithm asymKey;
		private SymmetricAlgorithm symKey;

		public AuthenticationKey (AsymmetricAlgorithm key) 
		{
			asymKey = key;
		}

		public AuthenticationKey (SymmetricAlgorithm key) 
		{
			symKey = key;
		}

		public bool CheckSignature (SignedXml signedXml) 
		{
			if (asymKey != null) {
				return signedXml.CheckSignature (asymKey);
			}
			else {
				HMACSHA1 hmac = new HMACSHA1 (symKey.Key);
				return signedXml.CheckSignature (hmac);
			}
		}
	}
}
