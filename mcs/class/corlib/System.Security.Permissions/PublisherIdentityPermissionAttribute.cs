//
// System.Security.Permissions.PublisherIdentityPermissionAttribute.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Cryptography;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class PublisherIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		private string certFile;
		private string signedFile;
		private string x509data;
		
		public PublisherIdentityPermissionAttribute (SecurityAction action) : base (action) {}

		// If X509Certificate is set, this property is ignored.
		public string CertFile {
			get { return certFile; }
			set { certFile = value; }
		}

		// If either X509Certificate or CertFile is set, this property is ignored.
		public string SignedFile {
			get { return signedFile; }
			set { signedFile = value; }
		}

		public string X509Certificate {
			get { return x509data; }
			set { x509data = value; }
		}

		public override IPermission CreatePermission ()
		{
			if (this.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted");

			X509Certificate x509 = null;
			if (x509data != null) {
				byte[] rawcert = CryptoConvert.FromHex (x509data);
				x509 = new X509Certificate (rawcert);
				return new PublisherIdentityPermission (x509);
			}
			if (certFile != null) {
				x509 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile (certFile);
				return new PublisherIdentityPermission (x509);
			}
			if (signedFile != null) {
				x509 = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile (signedFile);
				return new PublisherIdentityPermission (x509);
			}
			return new PublisherIdentityPermission (PermissionState.None);
		}
	}
}
