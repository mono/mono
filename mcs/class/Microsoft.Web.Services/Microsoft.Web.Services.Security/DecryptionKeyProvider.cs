//
// DecryptionKeyProvider.cs: Decryption Key Provider
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
using System.Security.Cryptography.Xml;

namespace Microsoft.Web.Services.Security {

	public class DecryptionKeyProvider : IDecryptionKeyProvider {

		public DecryptionKeyProvider () {}

		[MonoTODO]
		public virtual DecryptionKey GetDecryptionKey (string algorithmUri, KeyInfo keyInfo) 
		{
			return null;
		}
	}
}
