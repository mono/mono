//
// X509ChainStatusFlags.cs: X.509 Chain Status
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Mono.Security.X509 {

	// definitions from Fx 1.2
	// commented flags aren't implemented in X509Chain

	[Serializable]
	[Flags]
#if INSIDE_CORLIB || INSIDE_SYSTEM
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
