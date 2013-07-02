//
// System.Security.Permissions.PublisherIdentityPermissionAttribute.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using SSCX = System.Security.Cryptography.X509Certificates;

using Mono.Security.Cryptography;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class PublisherIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		private string certFile;
		private string signedFile;
		private string x509data;
		
		public PublisherIdentityPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

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
				return new PublisherIdentityPermission (PermissionState.Unrestricted);

			SSCX.X509Certificate x509 = null;
			if (x509data != null) {
				byte[] rawcert = CryptoConvert.FromHex (x509data);
				x509 = new SSCX.X509Certificate (rawcert);
				return new PublisherIdentityPermission (x509);
			}
			if (certFile != null) {
				x509 = SSCX.X509Certificate.CreateFromCertFile (certFile);
				return new PublisherIdentityPermission (x509);
			}
			if (signedFile != null) {
				x509 = SSCX.X509Certificate.CreateFromSignedFile (signedFile);
				return new PublisherIdentityPermission (x509);
			}
			return new PublisherIdentityPermission (PermissionState.None);
		}
	}
}
