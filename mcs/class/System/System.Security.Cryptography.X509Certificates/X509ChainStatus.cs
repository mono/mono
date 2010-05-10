//
// X509ChainStatus.cs - System.Security.Cryptography.X509Certificates.X509ChainStatus
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP || MOONLIGHT

namespace System.Security.Cryptography.X509Certificates {

	public struct X509ChainStatus {

		private X509ChainStatusFlags status;
		private string info;

		internal X509ChainStatus (X509ChainStatusFlags flag)
		{
			status = flag;
			info = GetInformation (flag);
		}

		// properties

		public X509ChainStatusFlags Status {
			get { return status; }
			set { status = value; }
		}

		public string StatusInformation {
			get { return info; }
			set { info = value; }
		}

		// private stuff

		// note: flags isn't a flag (i.e. multiple values) when used here
		static internal string GetInformation (X509ChainStatusFlags flags)
		{
			switch (flags) {
			case X509ChainStatusFlags.NotTimeValid:
			case X509ChainStatusFlags.NotTimeNested:
			case X509ChainStatusFlags.Revoked:
			case X509ChainStatusFlags.NotSignatureValid:
			case X509ChainStatusFlags.NotValidForUsage:
			case X509ChainStatusFlags.UntrustedRoot:
			case X509ChainStatusFlags.RevocationStatusUnknown:
			case X509ChainStatusFlags.Cyclic:
			case X509ChainStatusFlags.InvalidExtension:
			case X509ChainStatusFlags.InvalidPolicyConstraints:
			case X509ChainStatusFlags.InvalidBasicConstraints:
			case X509ChainStatusFlags.InvalidNameConstraints:
			case X509ChainStatusFlags.HasNotSupportedNameConstraint:
			case X509ChainStatusFlags.HasNotDefinedNameConstraint:
			case X509ChainStatusFlags.HasNotPermittedNameConstraint:
			case X509ChainStatusFlags.HasExcludedNameConstraint:
			case X509ChainStatusFlags.PartialChain:
			case X509ChainStatusFlags.CtlNotTimeValid:
			case X509ChainStatusFlags.CtlNotSignatureValid:
			case X509ChainStatusFlags.CtlNotValidForUsage:
			case X509ChainStatusFlags.OfflineRevocation:
			case X509ChainStatusFlags.NoIssuanceChainPolicy:
				return Locale.GetText (flags.ToString ()); // FIXME - add a better description
			case X509ChainStatusFlags.NoError:
			default:
				// should never happen
				return String.Empty;
			}
		}
	}
}

#endif
