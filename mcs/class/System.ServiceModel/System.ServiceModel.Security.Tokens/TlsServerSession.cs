//
// TlsServerSession.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Server;

namespace System.ServiceModel.Security.Tokens
{
	internal class TlsServerSession : TlsSession
	{
		SslServerStream ssl;
		MemoryStream stream;
		bool mutual;

		public TlsServerSession (X509Certificate2 cert, bool mutual)
		{
			this.mutual = mutual;
			stream = new MemoryStream ();
			ssl = new SslServerStream (stream, cert, mutual, true, SecurityProtocolType.Tls);
			ssl.PrivateKeyCertSelectionDelegate = delegate (X509Certificate c, string host) {
				if (c.GetCertHashString () == cert.GetCertHashString ())
					return cert.PrivateKey;
				return null;
			};
			ssl.ClientCertValidationDelegate = delegate (X509Certificate certificate, int[] certificateErrors) {
				// FIXME: use X509CertificateValidator
				return true;
			};
		}

		protected override Context Context {
			get { return ssl.context; }
		}

		protected override RecordProtocol Protocol {
			get { return ssl.protocol; }
		}

		public void ProcessClientHello (byte [] raw)
		{
			Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (Context.SecurityProtocol);
			Context.HandshakeState = HandshakeState.Started;

			stream.Write (raw, 0, raw.Length);
			stream.Seek (0, SeekOrigin.Begin);

			Protocol.ReceiveRecord (stream);
		}

		// ServerHello, ServerCertificate and ServerHelloDone
		public byte [] ProcessServerHello ()
		{
			Context.SessionId = Context.GetSecureRandomBytes (32);

#if false
			// so, can I send handshake batch with RecordProtocol?
			stream.SetLength (0);
			Protocol.SendRecord (HandshakeType.ServerHello);
			Protocol.SendRecord (HandshakeType.Certificate);
			Protocol.SendRecord (HandshakeType.ServerHelloDone);
			stream.Flush ();
			return stream.ToArray ();

#else

			MemoryStream ms = new MemoryStream ();

			WriteHandshake (ms);

			if (mutual)
				WriteOperations (ms,
					new TlsServerHello (ssl.context),
					new TlsServerCertificate (ssl.context),
					new TlsServerCertificateRequest (ssl.context),
					new TlsServerHelloDone (ssl.context));
			else
				WriteOperations (ms,
					new TlsServerHello (ssl.context),
					new TlsServerCertificate (ssl.context),
					new TlsServerHelloDone (ssl.context));

			return ms.ToArray ();
#endif
		}

		public void ProcessClientKeyExchange (byte [] raw)
		{
			stream.SetLength (0);
			stream.Write (raw, 0, raw.Length);
			stream.Seek (0, SeekOrigin.Begin);

			if (mutual)
				Protocol.ReceiveRecord (stream); // Certificate
			Protocol.ReceiveRecord (stream); // ClientKeyExchange
			Protocol.ReceiveRecord (stream); // ChangeCipherSpec
			Protocol.ReceiveRecord (stream); // ClientFinished

			if (stream.Position != stream.Length)
				throw new SecurityNegotiationException (String.Format ("Unexpected SSL negotiation binary: {0} bytes of excess in {1} bytes of the octets", stream.Length - stream.Position, stream.Length));
		}

		public byte [] ProcessServerFinished ()
		{
			stream.SetLength (0);
			Protocol.SendChangeCipherSpec ();
#if false
			Protocol.SendRecord (HandshakeType.Finished);
			stream.Flush ();
			return stream.ToArray ();
#else
			MemoryStream ms = new MemoryStream ();
			WriteOperations (ms, new TlsServerFinished (ssl.context));
			ms.Flush ();
			return ms.ToArray ();
#endif
		}

		public byte [] ProcessApplicationData (byte [] raw)
		{
			stream.SetLength (0);
			Protocol.SendRecord (ContentType.ApplicationData, raw);
			stream.Flush ();
			return stream.ToArray ();
		}
	}
}
