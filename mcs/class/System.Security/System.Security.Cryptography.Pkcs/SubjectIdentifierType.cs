//
// SubjectIdentifierType.cs - System.Security.Cryptography.Pkcs.SubjectIdentifierType
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public enum SubjectIdentifierType {
		Unknown,
		IssuerAndSerialNumber,
		SubjectKeyIdentifier
	}
}

#endif