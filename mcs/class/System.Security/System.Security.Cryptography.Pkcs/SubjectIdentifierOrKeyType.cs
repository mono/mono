//
// SubjectIdentifierOrKeyType.cs - System.Security.Cryptography.Pkcs.SubjectIdentifierOrKeyType
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public enum SubjectIdentifierOrKeyType {
		Unknown,
		IssuerAndSerialNumber,
		SubjectKeyIdentifier,
		PublicKeyInfo
	}
}

#endif