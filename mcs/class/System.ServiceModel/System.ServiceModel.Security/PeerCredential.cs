//
// PeerCredential.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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

using System.Security.Cryptography.X509Certificates;

namespace System.ServiceModel.Security
{
	public class PeerCredential
	{
		internal PeerCredential ()
		{
		}

		X509Certificate2 cert;
		string mesh_pwd;
		X509PeerCertificateAuthentication cert_auth =
			new X509PeerCertificateAuthentication ();
		X509PeerCertificateAuthentication peer_auth =
			new X509PeerCertificateAuthentication ();

		internal PeerCredential Clone ()
		{
			return new PeerCredential () { cert = this.cert, cert_auth = this.cert_auth.Clone (), peer_auth = this.peer_auth.Clone () };
		}

		public X509Certificate2 Certificate {
			get { return cert; }
			set { cert = value; }
		}

		public string MeshPassword {
			get { return mesh_pwd; }
			set { mesh_pwd = value; }
		}

		public X509PeerCertificateAuthentication MessageSenderAuthentication {
			get { return cert_auth; }
			// huh, should there be a setter?
			set { cert_auth = value; }
		}

		public X509PeerCertificateAuthentication PeerAuthentication {
			get { return peer_auth; }
			set { peer_auth = value; }
		}

		[MonoTODO]
		public void SetCertificate (string subjectName, StoreLocation storeLocation, StoreName storeName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetCertificate (StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
		{
			throw new NotImplementedException ();
		}
	}
}
