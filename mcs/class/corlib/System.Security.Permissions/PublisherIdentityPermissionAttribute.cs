//
// System.Security.Permissions.PublisherIdentityPermissionAttribute.cs
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
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

		private byte FromHexChar (char c) 
		{
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			throw new ArgumentException ("invalid hex char");
		}

		public override IPermission CreatePermission ()
		{
			X509Certificate x509 = null;
			if (x509data != null) {
				byte[] rawcert = new byte [x509data.Length >> 1];
				int n = 0;
				int i = 0;
				while (n < rawcert.Length) {
					rawcert [n] = (byte) (FromHexChar (x509data [i++]) << 4);
					rawcert [n++] += FromHexChar (x509data [i++]);
				}
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
