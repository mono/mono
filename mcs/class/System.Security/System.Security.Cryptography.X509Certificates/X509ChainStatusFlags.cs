//
// X509ChainStatusFlags.cs - System.Security.Cryptography.X509Certificates.X509ChainStatusFlags
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

	public enum X509ChainStatusFlags {
		NoError,
		NotTimeValid,
		NotTimeNested,
		Revoked,
		NotSignatureValid,
		NotValidForUsage,
		UntrustedRoot,
		RevocationStatusUnknown,
		Cyclic,
		InvalidExtension,
		InvalidPolicyConstraints,
		InvalidBasicConstraints,
		InvalidNameConstraints,
		HasNotSupportedNameConstraint,
		HasNotDefinedNameConstraint,
		HasNotPermittedNameConstraint,
		HasExcludedNameConstraint,
		PartialChain,
		CtlNotTimeValid,
		CtlNotSignatureValid,
		CtlNotValidForUsage,
		OfflineRevocation,
		NoIssuanceChainPolicy
	}
}

#endif