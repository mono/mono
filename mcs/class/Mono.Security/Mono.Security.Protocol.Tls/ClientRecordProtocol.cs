/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;

using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;

namespace Mono.Security.Protocol.Tls
{
	internal class ClientRecordProtocol : RecordProtocol
	{
		#region Constructors

		public ClientRecordProtocol(
			Stream			innerStream, 
			TlsContext		context) : base(innerStream, context)
		{
		}

		#endregion

		#region Send Messages

		public override void SendRecord(TlsHandshakeType type)
		{
			// Create the record message
			TlsHandshakeMessage msg = this.createClientHandshakeMessage(type);
			
			// Write record
			this.SendRecord(msg.ContentType, msg.EncodeMessage());

			// Update session
			msg.Update();

			// Reset message contents
			msg.Reset();
		}

		#endregion

		#region Handshake Processing Methods

		protected override void ProcessHandshakeMessage(TlsStream handMsg)
		{
			TlsHandshakeType	handshakeType	= (TlsHandshakeType)handMsg.ReadByte();
			TlsHandshakeMessage	message			= null;

			// Read message length
			int length = handMsg.ReadInt24();

			// Read message data
			byte[] data = new byte[length];
			handMsg.Read(data, 0, length);

			// Create and process the server message
			message = this.createServerHandshakeMessage(handshakeType, data);

			// Update session
			if (message != null)
			{
				message.Update();
			}
		}

		#endregion

		#region Client Handshake Message Factories

		private TlsHandshakeMessage createClientHandshakeMessage(TlsHandshakeType type)
		{
			switch (type)
			{
				case TlsHandshakeType.ClientHello:
					return new TlsClientHello(this.context);

				case TlsHandshakeType.Certificate:
					return new TlsClientCertificate(this.context);

				case TlsHandshakeType.ClientKeyExchange:
					return new TlsClientKeyExchange(this.context);

				case TlsHandshakeType.CertificateVerify:
					return new TlsClientCertificateVerify(this.context);

				case TlsHandshakeType.Finished:
					return new TlsClientFinished(this.context);

				default:
					throw new InvalidOperationException("Unknown client handshake message type: " + type.ToString() );
			}
		}

		private TlsHandshakeMessage createServerHandshakeMessage(TlsHandshakeType type, byte[] buffer)
		{
			switch (type)
			{
				case TlsHandshakeType.HelloRequest:
					this.SendRecord(TlsHandshakeType.ClientHello);
					return null;

				case TlsHandshakeType.ServerHello:
					return new TlsServerHello(this.context, buffer);

				case TlsHandshakeType.Certificate:
					return new TlsServerCertificate(this.context, buffer);

				case TlsHandshakeType.ServerKeyExchange:
					return new TlsServerKeyExchange(this.context, buffer);

				case TlsHandshakeType.CertificateRequest:
					return new TlsServerCertificateRequest(this.context, buffer);

				case TlsHandshakeType.ServerHelloDone:
					return new TlsServerHelloDone(this.context, buffer);

				case TlsHandshakeType.Finished:
					return new TlsServerFinished(this.context, buffer);

				default:
					throw this.context.CreateException("Unknown server handshake message received ({0})", type.ToString());
			}
		}

		#endregion
	}
}
