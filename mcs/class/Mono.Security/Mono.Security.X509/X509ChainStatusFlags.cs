//
// X509ChainStatusFlags.cs: X.509 Chain Status
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace Mono.Security.X509 {

	// definitions from Fx 1.2
	// commented flags aren't implemented in X509Chain

	[Serializable]
#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	enum X509ChainStatusFlags {
//		CtlNotSignatureValid = 262144,
//		CtlNotTimeValid = 131072,
//		CtlNotValidForUsage = 524288,
//		Cyclic = 128,
//		HasExcludedNameConstraint = 32768,
//		HasNotDefinedNameConstraint = 8192,
//		HasNotPermittedNameConstraint = 16384,
//		HasNotSupportedNameConstraint = 4096,
		InvalidBasicConstraints = 1024,
//		InvalidExtension = 256,
//		InvalidNameConstraints = 2048,
//		InvalidPolicyConstraints = 512,
		NoError = 0,
//		NoIssuanceChainPolicy = 33554432,
		NotSignatureValid = 8,
		NotTimeNested = 2,
		NotTimeValid = 1,
//		NotValidForUsage = 16,
//		OfflineRevocation = 16777216,
		PartialChain = 65536,
//		RevocationStatusUnknown = 64,
//		Revoked = 4,
		UntrustedRoot = 32
	} 
}
