//
// X509Chain.cs: X.509 Certificate Path
//	This is a VERY simplified and minimal version
//	used for
//		Authenticode support
//		TLS/SSL support
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

#if !INSIDE_CORLIB
using System.Net;
#endif

using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class X509Chain {

		private X509CertificateCollection roots;
		private X509CertificateCollection certs;
		private X509Certificate _root;

		private X509CertificateCollection _chain;
		private X509ChainStatusFlags _status;

		// constructors

		public X509Chain ()
		{
			certs = new X509CertificateCollection ();
		}

		// get a pre-builded chain
		public X509Chain (X509CertificateCollection chain) : this ()
		{
			certs.AddRange (chain);
		}

		// properties

		public X509CertificateCollection Chain {
			get { return _chain; }
		}

		// the root of the specified certificate (may not be trusted!)
		public X509Certificate Root {
			get { return _root; }
		}

		public X509ChainStatusFlags Status {
			get { return _status; }
		}

		public X509CertificateCollection TrustAnchors {
			get { 
				if (roots == null) {
					roots = new X509CertificateCollection ();
					roots.AddRange (X509StoreManager.TrustedRootCertificates);
					return roots;
				}
				return roots;
			}
			[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
			set { roots = value; }
		}

		// methods

		public void LoadCertificate (X509Certificate x509) 
		{
			certs.Add (x509);
		}

		public void LoadCertificates (X509CertificateCollection collection) 
		{
			certs.AddRange (collection);
		}

		public X509Certificate FindByIssuerName (string issuerName) 
		{
			foreach (X509Certificate x in certs) {
				if (x.IssuerName == issuerName)
					return x;
			}
			return null;
		}

		public bool Build (X509Certificate leaf) 
		{
			// When finished, _chain should contain the leaf, and optional intermediates, excluding the root.

			_status = X509ChainStatusFlags.NoError;

			// Even when chain is supplied, it's really just a suggestion, how to build the chain. It may contain
			// unnecessary certs, or be in the wrong order, etc.  So build a newChain unconditionally.
			_chain = new X509CertificateCollection ();
			if (leaf.IsSelfSigned) {
				_chain.Add (leaf);	// chain should always contain leaf, even if it happens to be a root
			}
			X509Certificate x = leaf;
			X509Certificate tmp = x;
			while ((x != null) && (!x.IsSelfSigned)) {
				tmp = x; // last valid
				_chain.Add (x);
				x = FindCertificateParent (x);
			}
			// find a trusted root
			_root = FindCertificateRoot (tmp);

			// validate the chain
			if ((_chain != null) && (_status == X509ChainStatusFlags.NoError)) {
				foreach (X509Certificate c in _chain) {
					// validate dates for each certificate in the chain
					// note: we DO NOT check for nested date/time
					if (!IsValid (c)) {
						return false;
					}
				}
				// check leaf
				if (!IsValid (leaf)) {
					// switch status code if the failure is expiration
					if (_status == X509ChainStatusFlags.NotTimeNested)
						_status = X509ChainStatusFlags.NotTimeValid;
					return false;
				}
				// check root
				if ((_root != null) && !IsValid (_root)) {
					return false;
				}
			}

			if (_status == X509ChainStatusFlags.NoError) {
				// I am assuming the leaf cert is present in _chain, so if _chain.Count > 1 it means
				// we have some intermediates in there too
				if (_chain.Count > 1) {
					X509CertificateCollection intermediates = X509StoreManager.IntermediateCACertificates;
					foreach (X509Certificate c in _chain) {
						if ( (c!=leaf) && (!intermediates.Contains(c)) ) {
							X509StoreManager.CurrentUser.IntermediateCA.Import(c);
						}
					}
				}
				return true;
			}
			else {
				return false;
			}
		}

		//

		public void Reset () 
		{
			_status = X509ChainStatusFlags.NoError;
			roots = null; // this force a reload
			certs.Clear ();
			if (_chain != null)
				_chain.Clear ();
		}

		// private stuff

		private bool IsValid (X509Certificate cert) 
		{
			if (!cert.IsCurrent) {
				// FIXME: nesting isn't very well implemented
				_status = X509ChainStatusFlags.NotTimeNested;
				return false;
			}

			// TODO - we should check for CRITICAL but unknown extensions
			// X509ChainStatusFlags.InvalidExtension
#if !INSIDE_CORLIB
			if (ServicePointManager.CheckCertificateRevocationList) {
				// TODO - check revocation (CRL, OCSP ...)
				// X509ChainStatusFlags.RevocationStatusUnknown
				// X509ChainStatusFlags.Revoked
			}
#endif
			return true;
		}

		private X509Certificate FindCertificateParent (X509Certificate child) 
		{
			foreach (X509Certificate potentialParent in certs) {
				if (IsParent (child, potentialParent))
					return potentialParent;
			}
			return null;
		}

		private X509Certificate FindCertificateRoot (X509Certificate potentialRoot) 
		{
			if (potentialRoot == null) {
				_status = X509ChainStatusFlags.PartialChain;
				return null;
			}

			// if the trusted root is in the chain
			if (IsTrusted (potentialRoot)) {
				return potentialRoot;
			}

			// if the root isn't in the chain
			foreach (X509Certificate root in TrustAnchors) {
				if (IsParent (potentialRoot, root)) {
					return root;
				}
			}

			// is it a (untrusted) root ?
			if (potentialRoot.IsSelfSigned) {
				_status = X509ChainStatusFlags.UntrustedRoot;
				return potentialRoot;
			}

			_status = X509ChainStatusFlags.PartialChain;
			return null;
		}

		private bool IsTrusted (X509Certificate potentialTrusted) 
		{
			return TrustAnchors.Contains (potentialTrusted);
		}

		private bool IsParent (X509Certificate child, X509Certificate parent) 
		{
			if (child.IssuerName != parent.SubjectName)
				return false;

			// parent MUST have the Basic Constraint CA=true (except for trusted roots)
			// see why at http://www.microsoft.com/technet/security/bulletin/MS02-050.asp
			if ((parent.Version > 2) && (!IsTrusted (parent))) {
				// TODO: we do not support pathLenConstraint
				X509Extension ext = parent.Extensions ["2.5.29.19"];
				if (ext != null) {
					BasicConstraintsExtension bc = new BasicConstraintsExtension (ext);
					if (!bc.CertificateAuthority)
						_status = X509ChainStatusFlags.InvalidBasicConstraints;
				}
				else
					_status = X509ChainStatusFlags.InvalidBasicConstraints;
			}

			if (!child.VerifySignature (parent.RSA)) {
				_status = X509ChainStatusFlags.NotSignatureValid;
				return false;
			}
			return true;
		}
	}
}
