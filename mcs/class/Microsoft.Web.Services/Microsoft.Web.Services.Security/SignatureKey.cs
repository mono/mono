//
// SignatureKey.cs: Handles WS-Security SignatureKey
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
using System.Security.Cryptography.Xml;

namespace Microsoft.Web.Services.Security {

	public class SignatureKey {

		private AsymmetricAlgorithm asymKey;
		private SymmetricAlgorithm symKey;

		public SignatureKey (AsymmetricAlgorithm key) 
		{
			asymKey = key;
		}

		public SignatureKey (SymmetricAlgorithm key) 
		{
			symKey = key;
		}

		public virtual void ComputeSignature (SignedXml signedXml) 
		{
			if (asymKey != null) {
				signedXml.SigningKey = asymKey;
				signedXml.ComputeSignature ();
			}
			else {
				HMACSHA1 hmac = new HMACSHA1 (symKey.Key);
				signedXml.ComputeSignature (hmac);
			}
		}
	}
}
