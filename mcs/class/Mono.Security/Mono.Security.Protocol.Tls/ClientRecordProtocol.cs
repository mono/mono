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
using System.Globalization;
using System.IO;

using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;

namespace Mono.Security.Protocol.Tls
{
	internal class ClientRecordProtocol : RecordProtocol
	{
		#region Constructors

		public ClientRecordProtocol(
			Stream			innerStream, 
			ClientContext	context) : base(innerStream, context)
		{
		}

		#endregion

		#region Send Messages

		public override void SendRecord(HandshakeType type)
		{
			// Create and process the record message
			HandshakeMessage msg = this.createClientHandshakeMessage(type);
			msg.Process();

			DebugHelper.WriteLine(">>>> Write handshake record ({0}|{1})", context.Protocol, msg.ContentType);

			// Write record
			this.SendRecord(msg.ContentType, msg.EncodeMessage());

			// Update session
			msg.Update();

			// Reset message contents
			msg.Reset();
		}

		#endregion

		#region Handshake Processing Methods

		protected override void ProcessChangeCipherSpec()
		{
			// Reset sequence numbers
			this.context.ReadSequenceNumber = 0;
		}

		protected override void ProcessHandshakeMessage(TlsStream handMsg)
		{
			HandshakeType		handshakeType	= (HandshakeType)handMsg.ReadByte();
			HandshakeMessage	message			= null;

			DebugHelper.WriteLine(">>>> Processing Handshake record ({0})", handshakeType);

			// Read message length
			int length = handMsg.ReadInt24();

			// Read message data
			byte[] data = new byte[length];
			handMsg.Read(data, 0, length);

			// Create and process the server message
			message = this.createServerHandshakeMessage(handshakeType, data);
			message.Process();

			// Update the last handshake message
			this.Context.LastHandshakeMsg = handshakeType;

			// Update session
			if (message != null)
			{
				message.Update();
			}
		}

		#endregion

		#region Client Handshake Message Factories

		private HandshakeMessage createClientHandshakeMessage(HandshakeType type)
		{
			switch (type)
			{
				case HandshakeType.ClientHello:
					return new TlsClientHello(this.context);

				case HandshakeType.Certificate:
					return new TlsClientCertificate(this.context);

				case HandshakeType.ClientKeyExchange:
					return new TlsClientKeyExchange(this.context);

				case HandshakeType.CertificateVerify:
					return new TlsClientCertificateVerify(this.context);

				case HandshakeType.Finished:
					return new TlsClientFinished(this.context);

				default:
					throw new InvalidOperationException("Unknown client handshake message type: " + type.ToString() );
			}
		}

		private HandshakeMessage createServerHandshakeMessage(
			HandshakeType type, byte[] buffer)
		{
			ClientContext context = (ClientContext)this.context;

			switch (type)
			{
				case HandshakeType.HelloRequest:
					if (context.HandshakeState != HandshakeState.Started)
					{
						context.SslStream.NegotiateHandshake();
					}
					else
					{
						this.SendAlert(
							AlertLevel.Warning,
							AlertDescription.NoRenegotiation);
					}
					return null;

				case HandshakeType.ServerHello:
					return new TlsServerHello(this.context, buffer);

				case HandshakeType.Certificate:
					return new TlsServerCertificate(this.context, buffer);

				case HandshakeType.ServerKeyExchange:
					return new TlsServerKeyExchange(this.context, buffer);

				case HandshakeType.CertificateRequest:
					return new TlsServerCertificateRequest(this.context, buffer);

				case HandshakeType.ServerHelloDone:
					return new TlsServerHelloDone(this.context, buffer);

				case HandshakeType.Finished:
					return new TlsServerFinished(this.context, buffer);

				default:
					throw new TlsException(
						AlertDescription.UnexpectedMessage,
						String.Format(CultureInfo.CurrentUICulture,
							"Unknown server handshake message received ({0})", 
							type.ToString()));
			}
		}

		#endregion
	}
}
