// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using X509Cert = System.Security.Cryptography.X509Certificates;

using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsServerCertificate : HandshakeMessage
	{
		#region Constructors

		public TlsServerCertificate(Context context) 
			: base(context, HandshakeType.Certificate)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			TlsStream certs = new TlsStream();

			foreach (X509Certificate certificate in this.Context.ServerSettings.Certificates)
			{
				// Write certificate length
				certs.WriteInt24(certificate.RawData.Length);

				// Write certificate data
				certs.Write(certificate.RawData);
			}

			this.WriteInt24(Convert.ToInt32(certs.Length));
			this.Write(certs.ToArray());

			certs.Close();
		}

		#endregion
	}
}
