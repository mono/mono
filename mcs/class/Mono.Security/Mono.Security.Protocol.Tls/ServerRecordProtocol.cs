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
using Mono.Security.Protocol.Tls.Handshake.Server;

namespace Mono.Security.Protocol.Tls
{
	internal class ServerRecordProtocol : RecordProtocol
	{
		TlsClientCertificate cert;
		
		#region Constructors

		public ServerRecordProtocol(
			Stream			innerStream, 
			ServerContext	context) : base(innerStream, context)
		{
		}

		#endregion

		#region Send Messages

		public override HandshakeMessage GetMessage(HandshakeType type)
		{
			// Create and process the record message
			HandshakeMessage msg = this.createServerHandshakeMessage(type);

			return msg;
		}

		#endregion

		#region Handshake Processing Methods

		protected override void ProcessHandshakeMessage(TlsStream handMsg)
		{
			HandshakeType		handshakeType	= (HandshakeType)handMsg.ReadByte();
			HandshakeMessage	message			= null;

			// Read message length
			int length = handMsg.ReadInt24();

			// Read message data
			byte[] data = new byte[length];
			handMsg.Read(data, 0, length);

			// Create and process the server message
			message = this.createClientHandshakeMessage(handshakeType, data);
			message.Process();

			// Update the last handshake message
			this.Context.LastHandshakeMsg = handshakeType;

			// Update session
			if (message != null)
			{
				message.Update();
				this.Context.HandshakeMessages.WriteByte ((byte) handshakeType);
				this.Context.HandshakeMessages.WriteInt24 (length);
				this.Context.HandshakeMessages.Write (data, 0, data.Length);
			}
		}

		#endregion

		#region Server Handshake Message Factories

		private HandshakeMessage createClientHandshakeMessage(
			HandshakeType type, byte[] buffer)
		{
			var last = context.LastHandshakeMsg;
			switch (type)
			{
				case HandshakeType.ClientHello:
					return new TlsClientHello(this.context, buffer);

				case HandshakeType.Certificate:
					if (last != HandshakeType.ClientHello)
						break;
					cert = new TlsClientCertificate(this.context, buffer);
					return cert;

				case HandshakeType.ClientKeyExchange:
					if (last == HandshakeType.ClientHello || last == HandshakeType.Certificate)
						return new TlsClientKeyExchange(this.context, buffer);
					break;

				case HandshakeType.CertificateVerify:
					if (last == HandshakeType.ClientKeyExchange && cert != null)
						return new TlsClientCertificateVerify(this.context, buffer);
					break;

				case HandshakeType.Finished:
					// Certificates are optional, but if provided, they should send a CertificateVerify
					bool check = (cert == null) ? (last == HandshakeType.ClientKeyExchange) : (last == HandshakeType.CertificateVerify);
					// ChangeCipherSpecDone is not an handshake message (it's a content type) but still needs to be happens before finished
					if (check && context.ChangeCipherSpecDone) {
						context.ChangeCipherSpecDone = false;
						return new TlsClientFinished(this.context, buffer);
					}
					break;
					
				default:
					throw new TlsException(AlertDescription.UnexpectedMessage, String.Format(CultureInfo.CurrentUICulture,
														 "Unknown server handshake message received ({0})", 
														 type.ToString()));
					break;
			}
			throw new TlsException (AlertDescription.HandshakeFailiure, String.Format ("Protocol error, unexpected protocol transition from {0} to {1}", last, type));
		}

		private HandshakeMessage createServerHandshakeMessage(
			HandshakeType type)
		{
			switch (type)
			{
				case HandshakeType.HelloRequest:
					this.SendRecord(HandshakeType.ClientHello);
					return null;

				case HandshakeType.ServerHello:
					return new TlsServerHello(this.context);

				case HandshakeType.Certificate:
					return new TlsServerCertificate(this.context);

				case HandshakeType.ServerKeyExchange:
					return new TlsServerKeyExchange(this.context);

				case HandshakeType.CertificateRequest:
					return new TlsServerCertificateRequest(this.context);

				case HandshakeType.ServerHelloDone:
					return new TlsServerHelloDone(this.context);

				case HandshakeType.Finished:
					return new TlsServerFinished(this.context);

				default:
					throw new InvalidOperationException("Unknown server handshake message type: " + type.ToString() );					
			}
		}

		#endregion
	}
}
