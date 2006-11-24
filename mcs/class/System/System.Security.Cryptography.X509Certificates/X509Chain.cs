//
// System.Security.Cryptography.X509Certificates.X509Chain
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	public class X509Chain {

		// Set to internal to remove a warning
		private StoreLocation location;
		private X509ChainElementCollection elements;
		private X509ChainPolicy policy;
		private X509ChainStatus[] status;

		static X509ChainStatus[] Empty = new X509ChainStatus [0];

		// constructors

		public X509Chain ()
			: this (false)
		{
		}

		public X509Chain (bool useMachineContext) 
		{
			location = useMachineContext ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
			elements = new X509ChainElementCollection ();
			policy = new X509ChainPolicy ();
		}

		[MonoTODO ("Mono's X509Chain is fully managed. All handles are invalid.")]
		public X509Chain (IntPtr chainContext)
		{
			// CryptoAPI compatibility (unmanaged handle)
			throw new NotSupportedException ();
		}

		// properties

		[MonoTODO ("Mono's X509Chain is fully managed. Always returns IntPtr.Zero.")]
		public IntPtr ChainContext {
			get { return IntPtr.Zero; }
		}

		public X509ChainElementCollection ChainElements {
			get { return elements; }
		}

		public X509ChainPolicy ChainPolicy {
			get { return policy; }
			set { policy = value; }
		}

		public X509ChainStatus[] ChainStatus {
			get { 
				if (status == null)
					return Empty;
				return status;
			}
		} 

		// methods

		[MonoTODO ("Work in progress")]
		public bool Build (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentException ("certificate");

			Reset ();

			X509ChainStatusFlags flag;
			try {
				flag = BuildFrom (certificate);
			}
			catch (CryptographicException ce) {
				throw new ArgumentException ("certificate", ce);
			}

			ArrayList list = new ArrayList ();
			// build "global" ChainStatus from the ChainStatus of every ChainElements
			foreach (X509ChainElement ce in elements) {
				foreach (X509ChainStatus cs in ce.ChainElementStatus) {
					// FIXME - avoid duplicates ?
					list.Add (cs);
				}
			}
			// and if required add some
			if (flag != X509ChainStatusFlags.NoError) {
				list.Insert (0, new X509ChainStatus (flag));
			}
			status = (X509ChainStatus[]) list.ToArray (typeof (X509ChainStatus));

			// (fast path) this ignore everything we have checked
			if (ChainPolicy.VerificationFlags == X509VerificationFlags.AllFlags)
				return true;

			bool result = true;
			// now check if exclude some verification for the "end result" (boolean)
			foreach (X509ChainStatus cs in status) {
				switch (cs.Status) {
				case X509ChainStatusFlags.UntrustedRoot:
				case X509ChainStatusFlags.PartialChain:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.AllowUnknownCertificateAuthority) != 0);
					break;
				case X509ChainStatusFlags.NotTimeValid:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreNotTimeValid) != 0);
					break;
				// FIXME - from here we needs new test cases for all cases
				case X509ChainStatusFlags.NotTimeNested:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreNotTimeNested) != 0);
					break;
				case X509ChainStatusFlags.InvalidBasicConstraints:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreInvalidBasicConstraints) != 0);
					break;
				case X509ChainStatusFlags.InvalidPolicyConstraints:
				case X509ChainStatusFlags.NoIssuanceChainPolicy:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreInvalidPolicy) != 0);
					break;
				case X509ChainStatusFlags.InvalidNameConstraints:
				case X509ChainStatusFlags.HasNotSupportedNameConstraint:
				case X509ChainStatusFlags.HasNotPermittedNameConstraint:
				case X509ChainStatusFlags.HasExcludedNameConstraint:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreInvalidName) != 0);
					break;
				case X509ChainStatusFlags.InvalidExtension:
					// not sure ?!?
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreWrongUsage) != 0);
					break;
				//
				//	((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreRootRevocationUnknown) != 0)
				//	((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreEndRevocationUnknown) != 0)
				case X509ChainStatusFlags.CtlNotTimeValid:
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreCtlNotTimeValid) != 0);
					break;
				case X509ChainStatusFlags.CtlNotSignatureValid:
					// ?
					break;
				//	((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreCtlSignerRevocationUnknown) != 0);
				case X509ChainStatusFlags.CtlNotValidForUsage:
					// FIXME - does IgnoreWrongUsage apply to CTL (it doesn't have Ctl in it's name like the others)
					result &= ((ChainPolicy.VerificationFlags & X509VerificationFlags.IgnoreWrongUsage) != 0);
					break;
				default:
					result = false;
					break;
				}
				// once we have one failure there's no need to check further
				if (!result)
					return false;
			}

			// every "problem" was excluded
			return true;
		}

		public void Reset () 
		{
			if ((status != null) && (status.Length != 0))
				status = null;
			if (elements.Count > 0)
				elements.Clear ();
			// note: this call doesn't Reset the X509ChainPolicy
		}

		// static methods

		public static X509Chain Create ()
		{
			return (X509Chain) CryptoConfig.CreateFromName ("X509Chain");
		}

		// private stuff

		private X509ChainStatusFlags BuildFrom (X509Certificate2 certificate)
		{
			X509ChainStatusFlags result = X509ChainStatusFlags.NoError;
			X509ChainStatusFlags flags = X509ChainStatusFlags.NoError;

			// check certificate
			Process (certificate, ref flags);

			// check if certificate is self-signed
			if (IsSelfSigned (certificate)) {
				// FIXME - add support for cross-certificate, bridges
				ProcessRoot (certificate, ref flags);
			} else {
				CheckRevocation (certificate, ref flags);

				X509Certificate2 parent = FindParent (certificate, ref flags);
				if (parent != null) {
					// recurse
					result = BuildFrom (parent);
					if (result != X509ChainStatusFlags.NoError)
						return result;
				} else {
					// we didn't end with a root, nor could we find one (stores)
					result = X509ChainStatusFlags.PartialChain;
				}
			}
			elements.Add (certificate, flags);
			return result;
		}

		private void Process (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			// is it the end-entity ?
			if (elements.Count == 0) {
			}

			if ((ChainPolicy.VerificationTime < certificate.NotBefore) ||
				(ChainPolicy.VerificationTime > certificate.NotAfter)) {
				flags |= X509ChainStatusFlags.NotTimeValid;
			}

			// TODO - for X509ChainStatusFlags.NotTimeNested (needs global structure)

			// TODO - for X509ChainStatusFlags.InvalidExtension

			// TODO - check for X509ChainStatusFlags.InvalidBasicConstraint

			// TODO - for X509ChainStatusFlags.InvalidPolicyConstraints
			//	using X509ChainPolicy.ApplicationPolicy and X509ChainPolicy.CertificatePolicy

			// TODO - check for X509ChainStatusFlags.NoIssuanceChainPolicy

			// TODO - check for X509ChainStatusFlags.InvalidNameConstraint
			// TODO - check for X509ChainStatusFlags.HasNotSupportedNameConstraint
			// TODO - check for X509ChainStatusFlags.HasNotPermittedNameConstraint
			// TODO - check for X509ChainStatusFlags.HasExcludedNameConstraint
		}

		private void ProcessEndEntity (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
		}

		private void ProcessCertificateAuthority (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
		}

		// CTL == Certificate Trust List / not sure how/if they apply here
		private void ProcessCTL (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			// TODO - check for X509ChainStatusFlags.CtlNotTimeValid
			// TODO - check for X509ChainStatusFlags.CtlNotSignatureValid
			// TODO - check for X509ChainStatusFlags.CtlNotValidForUsage
		}

		private void ProcessRoot (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			X509Store trust = new X509Store (StoreName.Root, location);
			trust.Open (OpenFlags.ReadOnly);
			if (!trust.Certificates.Contains (certificate)) {
				flags |= X509ChainStatusFlags.UntrustedRoot;
			}
			trust.Close ();

			if (!IsSignedBy (certificate, certificate)) {
				flags |= X509ChainStatusFlags.NotSignatureValid;
			}
		}

		// we search local user (default) or machine certificate store 
		// and in the extra certificate supplied in ChainPolicy.ExtraStore
		private X509Certificate2 FindParent (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			X509Certificate2 parent = null;

			// TODO - check for X509ChainStatusFlags.Cyclic

			if ((parent != null) && !IsSignedBy (certificate, parent)) {
				flags |= X509ChainStatusFlags.NotSignatureValid;
			}
			return null;
		}

		// check for "self-signed" certificate - without verifying the signature
		private bool IsSelfSigned (X509Certificate2 certificate)
		{
			// FIXME - very incomplete
			return (certificate.Issuer == certificate.Subject);
		}

		// this method verify the signature
		private bool IsSignedBy (X509Certificate2 signed, X509Certificate2 signer)
		{
			// FIXME
			return true;
		}

		private void CheckRevocation (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			switch (ChainPolicy.RevocationMode) {
			case X509RevocationMode.Online:
				// local?/download CRL and OCSP
				CheckOnlineRevocation (certificate, ref flags);
				break;
			case X509RevocationMode.Offline:
				// only local CRL ?
				CheckOfflineRevocation (certificate, ref flags);
				break;
			case X509RevocationMode.NoCheck:
				break;
			default:
				throw new InvalidOperationException ();
			}
		}

		private void CheckOfflineRevocation (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			// TODO - check for X509ChainStatusFlags.Revoked
			// TODO - check for X509ChainStatusFlags.RevocationStatusUnknown
			// TODO - check for X509ChainStatusFlags.OfflineRevocation
			//	 (using X509ChainPolicy.RevocationFlag and X509ChainPolicy.RevocationMode)
		}

		private void CheckOnlineRevocation (X509Certificate2 certificate, ref X509ChainStatusFlags flags)
		{
			// TODO - check for X509ChainStatusFlags.Revoked
			// TODO - check for X509ChainStatusFlags.RevocationStatusUnknown
			// TODO - check for X509ChainStatusFlags.OfflineRevocation
			//	 (using X509ChainPolicy.RevocationFlag and X509ChainPolicy.RevocationMode)
		}
	}
}

#endif
