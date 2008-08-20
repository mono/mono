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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
	internal class ClientContext : Context
	{
		#region Fields

		private SslClientStream	sslStream;
		private short			clientHelloProtocol;

		#endregion

		#region Properties

		public SslClientStream SslStream
		{
			get { return this.sslStream; }
		}

		public short ClientHelloProtocol
		{
			get { return this.clientHelloProtocol; }
			set { this.clientHelloProtocol = value; }
		}

		#endregion

		#region Constructors

		public ClientContext(
			SslClientStream				stream,
			SecurityProtocolType		securityProtocolType,
			string						targetHost,
			X509CertificateCollection	clientCertificates) 
			: base(securityProtocolType)
		{
			this.sslStream						= stream;
			this.ClientSettings.Certificates	= clientCertificates;
			this.ClientSettings.TargetHost		= targetHost;
		}

		#endregion

		#region Methods

		public override void Clear()
		{
			this.clientHelloProtocol = 0;
			base.Clear();
		}

		#endregion
	}
}
