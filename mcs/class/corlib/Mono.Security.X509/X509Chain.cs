//
// X509Chain.cs: X.509 Certificate Path
//	This is a VERY simplified and minimal version (for Authenticode support)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class X509Chain {

		private X509CertificateCollection roots;
		private X509CertificateCollection certs;
		private X509Certificate root;

		public X509Chain ()
		{
			certs = new X509CertificateCollection ();
		}

		public void LoadCertificate (X509Certificate x509) 
		{
			certs.Add (x509);
		}

		public void LoadCertificates (X509CertificateCollection coll) 
		{
			certs.AddRange (coll);
		}

		public X509Certificate FindByIssuerName (string issuerName) 
		{
			foreach (X509Certificate x in certs) {
				if (x.IssuerName == issuerName)
					return x;
			}
			return null;
		}

		public X509CertificateCollection GetChain (X509Certificate x509) 
		{
			X509CertificateCollection path = new X509CertificateCollection ();
			X509Certificate x = FindCertificateParent (x509);
			if (x != null) {
				while (x != null) {
					x509 = x;
					path.Add (x509);
					x = FindCertificateParent (x509);
					if ((x != null) && (x.IsSelfSigned))
						x = null;
				}
			}
			// find a trusted root
			x = FindCertificateRoot (x509);
			if (x == null)
				return null;
			root = x;
			return path;
		}

		private X509CertificateCollection GetTrustAnchors () 
		{
			// TODO - Load from machine.config
			ITrustAnchors trust = (ITrustAnchors) new TestAnchors ();
			return trust.Anchors;
		}

		public X509CertificateCollection TrustAnchors {
			get { return ((roots == null) ? GetTrustAnchors () : roots); }
			set { roots = value; }
		}

		public X509Certificate Root {
			get { return root; }
		}

		public void Reset () 
		{
			// this force a reload
			roots = null;
			certs.Clear ();
		}

		private X509Certificate FindCertificateParent (X509Certificate child) 
		{
			foreach (X509Certificate potentialParent in certs) {
				if (IsParent (child, potentialParent))
					return potentialParent;
			}
			return null;
		}

		private X509Certificate FindCertificateRoot (X509Certificate x509) 
		{
			// if the trusted root is in the path
			if (TrustAnchors.Contains (x509))
				return x509;

			foreach (X509Certificate root in TrustAnchors) {
				if (IsParent (x509, root))
					return root;
			}

			return null;
		}

		private bool IsParent (X509Certificate child, X509Certificate parent) 
		{
			if (child.IssuerName != parent.SubjectName)
				return false;
			return (child.VerifySignature (parent.RSA));
		}
	}
}
