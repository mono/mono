//
// X509FindType.cs - System.Security.Cryptography.X509Certificates.X509FindType
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public enum X509FindType {
		FindByThumbprint,
		FindBySubjectName,
		FindBySubjectDistinguishedName,
		FindByIssuerName,
		FindByIssuerDistinguishedName,
		FindBySerialNumber,
		FindByTimeValid,
		FindByTimeNotYetValid,
		FindByTimeExpired,
		FindByTemplateName,
		FindByApplicationPolicy,
		FindByExtension,
		FindByKeyUsage,
		FindBySubjectKeyIdentifier
	}
}

#endif