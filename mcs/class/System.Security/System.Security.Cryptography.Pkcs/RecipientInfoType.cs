//
// RecipientInfoType.cs - System.Security.Cryptography.Pkcs.RecipientInfoType
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public enum RecipientInfoType {
		Unknown,
		KeyTransport,
		KeyAgreement
	}
}

#endif