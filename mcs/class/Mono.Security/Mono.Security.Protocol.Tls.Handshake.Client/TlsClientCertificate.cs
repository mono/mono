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
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientCertificate : HandshakeMessage
	{
		#region Constructors

		public TlsClientCertificate(Context context) 
			: base(context, HandshakeType.Certificate)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Reset();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
#warning "Client certificate selection is unfinished"
			ClientContext context = (ClientContext)this.Context;
			string msg = "Client certificate requested by the server and no client certificate specified.";

			if (context.ClientSettings.Certificates == null ||
				context.ClientSettings.Certificates.Count == 0)
			{
				throw new TlsException(AlertDescription.UserCancelled, msg);
			}
			
			// Select a valid certificate
			X509Certificate clientCert = this.Context.ClientSettings.Certificates[0];

			clientCert = context.SslStream.RaiseClientCertificateSelection(
				this.Context.ClientSettings.Certificates,
				new X509Certificate(this.Context.ServerSettings.Certificates[0].RawData),
				this.Context.ClientSettings.TargetHost,
				null);

			if (clientCert == null)
			{
				throw new TlsException(AlertDescription.UserCancelled, msg);
			}

			// Update the selected client certificate
			context.ClientSettings.ClientCertificate = clientCert;

			// Write client certificates information to a stream
			TlsStream stream = new TlsStream();

			stream.WriteInt24(clientCert.GetRawCertData().Length);
			stream.Write(clientCert.GetRawCertData());

			// Compose the message
			this.WriteInt24((int)stream.Length);
			this.Write(stream.ToArray());
		}

		#endregion
	}
}
