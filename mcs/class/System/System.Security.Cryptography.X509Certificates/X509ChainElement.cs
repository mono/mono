//
// X509ChainElement.cs - System.Security.Cryptography.X509Certificates.X509ChainElement
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005-2006 Novell Inc. (http://www.novell.com)
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

	public class X509ChainElement {

		private X509Certificate2 certificate;
		private X509ChainStatus[] status;
		private string info;
		private X509ChainStatusFlags compressed_status_flags;

		// constructors

		// only accessible from X509Chain.ChainElements
		internal X509ChainElement (X509Certificate2 certificate)
		{
			this.certificate = certificate;
			// so far String.Empty is the only thing I've seen. 
			// The interesting stuff is inside X509ChainStatus.Information
			info = String.Empty;
		}

		// properties

		public X509Certificate2 Certificate {
			get { return certificate; }
		}

		public X509ChainStatus[] ChainElementStatus {
			get { return status; }
		}

		public string Information {
			get { return info; }
		}

		// private stuff

		internal X509ChainStatusFlags StatusFlags {
			get { return compressed_status_flags; }
			set { compressed_status_flags = value; }
		}

		private int Count (X509ChainStatusFlags flags)
		{
			int size = 0;
			int n = 0;
			int f = (int) flags;
			int m = 0x1;
			while (n++ < 32) {
				if ((f & m) == m)
					size++;
				m <<= 1;
			}
			return size;
		}

		private void Set (X509ChainStatus[] status, ref int position, X509ChainStatusFlags flags, X509ChainStatusFlags mask)
		{
			if ((flags & mask) != 0) {
				status [position].Status = mask;
				status [position].StatusInformation = X509ChainStatus.GetInformation (mask);
				position++;
			}
		}

		internal void UncompressFlags ()
		{
			if (compressed_status_flags == X509ChainStatusFlags.NoError) {
				status = new X509ChainStatus [0];
			} else {
				int size = Count (compressed_status_flags);
				status = new X509ChainStatus [size];

				int n = 0;
				// process every possible error
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.UntrustedRoot);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.NotTimeValid);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.NotTimeNested);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.Revoked);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.NotSignatureValid);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.NotValidForUsage);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.RevocationStatusUnknown);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.Cyclic);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.InvalidExtension);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.InvalidPolicyConstraints);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.InvalidBasicConstraints);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.InvalidNameConstraints);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.HasNotSupportedNameConstraint);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.HasNotDefinedNameConstraint);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.HasNotPermittedNameConstraint);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.HasExcludedNameConstraint);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.PartialChain);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.CtlNotTimeValid);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.CtlNotSignatureValid);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.CtlNotValidForUsage);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.OfflineRevocation);
				Set (status, ref n, compressed_status_flags, X509ChainStatusFlags.NoIssuanceChainPolicy);
			}
		}
	}
}

#endif
