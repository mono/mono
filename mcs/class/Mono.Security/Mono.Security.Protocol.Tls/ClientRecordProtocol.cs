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

		public override HandshakeMessage GetMessage(HandshakeType type)
		{
			HandshakeMessage msg = this.createClientHandshakeMessage(type);

			return msg;
		}

		#endregion

		#region Handshake Processing Methods

		protected override void ProcessHandshakeMessage(TlsStream handMsg)
		{
			HandshakeType		handshakeType	= (HandshakeType)handMsg.ReadByte();
			HandshakeMessage	message			= null;

			DebugHelper.WriteLine(">>>> Processing Handshake record ({0})", handshakeType);

			// Read message length
			int length = handMsg.ReadInt24();

			// Read message data
			byte[] data = null;
			if (length > 0)
			{
				data = new byte[length];
				handMsg.Read (data, 0, length);
			}

			// Create and process the server message
			message = this.createServerHandshakeMessage(handshakeType, data);
			if (message != null)
			{
				message.Process();
			}

			// Update the last handshake message
			this.Context.LastHandshakeMsg = handshakeType;

			// Update session
			if (message != null)
			{
				message.Update();
				this.Context.HandshakeMessages.WriteByte ((byte) handshakeType);
				this.Context.HandshakeMessages.WriteInt24 (length);
				if (length > 0) 
				{
					this.Context.HandshakeMessages.Write (data, 0, data.Length);
				}
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
			var last = context.LastHandshakeMsg;

			switch (type)
			{
				case HandshakeType.HelloRequest:
					if (context.HandshakeState != HandshakeState.Started)
					{
						context.HandshakeState = HandshakeState.None;
						// re-negotiation will occur at next read/write
						// (i.e. not during an existing encode/decode op)
					}
					else
					{
						this.SendAlert(
							AlertLevel.Warning,
							AlertDescription.NoRenegotiation);
					}
					return null;

				case HandshakeType.ServerHello:
					if (last != HandshakeType.HelloRequest)
						break;
					return new TlsServerHello(this.context, buffer);

					// Optional
				case HandshakeType.Certificate:
					if (last != HandshakeType.ServerHello)
						break;
					return new TlsServerCertificate(this.context, buffer);

					// Optional
				case HandshakeType.CertificateRequest:
					if (last == HandshakeType.ServerKeyExchange || last == HandshakeType.Certificate)
						return new TlsServerCertificateRequest(this.context, buffer);
					break;

				case HandshakeType.ServerHelloDone:
					if (last == HandshakeType.CertificateRequest || last == HandshakeType.Certificate || last == HandshakeType.ServerHello)
						return new TlsServerHelloDone(this.context, buffer);
					break;

				case HandshakeType.Finished:
					// depends if a full (ServerHelloDone) or an abbreviated handshake (ServerHello) is being done
					bool check = context.AbbreviatedHandshake ? (last == HandshakeType.ServerHello) : (last == HandshakeType.ServerHelloDone);
					// ChangeCipherSpecDone is not an handshake message (it's a content type) but still needs to be happens before finished
					if (check && context.ChangeCipherSpecDone) {
						context.ChangeCipherSpecDone = false;
						return new TlsServerFinished (this.context, buffer);
					}
					break;
					
				default:
					throw new TlsException(
						AlertDescription.UnexpectedMessage,
						String.Format(CultureInfo.CurrentUICulture,
							"Unknown server handshake message received ({0})", 
							type.ToString()));
			}
			throw new TlsException (AlertDescription.HandshakeFailiure, String.Format ("Protocol error, unexpected protocol transition from {0} to {1}", last, type));
		}

		#endregion
	}
}
