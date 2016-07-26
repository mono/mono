//
// X509ChainStatusFlags.cs - System.Security.Cryptography.X509Certificates.X509ChainStatusFlags
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

namespace System.Security.Cryptography.X509Certificates {

	[Flags]
	public enum X509ChainStatusFlags {
		NoError = 0,
		NotTimeValid = 1,
		NotTimeNested = 2,
		Revoked = 4,
		NotSignatureValid = 8,
		NotValidForUsage = 16,
		UntrustedRoot = 32,
		RevocationStatusUnknown = 64,
		Cyclic = 128,
		InvalidExtension = 256,
		InvalidPolicyConstraints = 512,
		InvalidBasicConstraints = 1024,
		InvalidNameConstraints = 2048,
		HasNotSupportedNameConstraint = 4096,
		HasNotDefinedNameConstraint = 8192,
		HasNotPermittedNameConstraint = 16384,
		HasExcludedNameConstraint = 32768,
		PartialChain = 65536,
		CtlNotTimeValid = 131072,
		CtlNotSignatureValid = 262144,
		CtlNotValidForUsage = 524288,
		OfflineRevocation = 16777216,
		NoIssuanceChainPolicy = 33554432,
		ExplicitDistrust = 67108864,
		HasNotSupportedCriticalExtension = 134217728,
		HasWeakSignature = 1048576,
	}
}

