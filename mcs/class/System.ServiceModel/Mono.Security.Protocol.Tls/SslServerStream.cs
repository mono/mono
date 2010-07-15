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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	public class SslServerStream : SslStreamBase
	{
		#region Internal Events
		
		internal event CertificateValidationCallback	ClientCertValidation;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Properties

		public X509Certificate ClientCertificate
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.ClientSettings.ClientCertificate;
				}

				return null;
			}
		}

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ClientCertValidationDelegate 
		{
			get { return this.ClientCertValidation; }
			set { this.ClientCertValidation = value; }
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate
		{
			get { return this.PrivateKeySelection; }
			set { this.PrivateKeySelection = value; }
		}

		#endregion

		public event CertificateValidationCallback2 ClientCertValidation2;
		#region Constructors

		public SslServerStream(
			Stream			stream, 
			X509Certificate serverCertificate) : this(
			stream, 
			serverCertificate, 
			false, 
			false, 
			SecurityProtocolType.Default)
		{
		}

		public SslServerStream(
			Stream			stream,
			X509Certificate serverCertificate,
			bool			clientCertificateRequired,
			bool			ownsStream): this(
			stream, 
			serverCertificate, 
			clientCertificateRequired, 
			ownsStream, 
			SecurityProtocolType.Default)
		{
		}

		public SslServerStream(
			Stream			stream,
			X509Certificate serverCertificate,
			bool			clientCertificateRequired,
			bool			requestClientCertificate,
			bool			ownsStream)
				: this (stream, serverCertificate, clientCertificateRequired, requestClientCertificate, ownsStream, SecurityProtocolType.Default)
		{
		}

		public SslServerStream(
			Stream					stream,
			X509Certificate			serverCertificate,
			bool					clientCertificateRequired,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType)
			: this (stream, serverCertificate, clientCertificateRequired, false, ownsStream, securityProtocolType)
		{
		}

		public SslServerStream(
			Stream					stream,
			X509Certificate			serverCertificate,
			bool					clientCertificateRequired,
			bool					requestClientCertificate,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType)
			: base(stream, ownsStream)
		{
			this.context = new ServerContext(
				this,
				securityProtocolType,
				serverCertificate,
				clientCertificateRequired,
				requestClientCertificate);

			this.protocol = new ServerRecordProtocol(innerStream, (ServerContext)this.context);
		}

		#endregion

		#region Finalizer

		~SslServerStream()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				this.ClientCertValidation = null;
				this.PrivateKeySelection = null;
			}
		}

		#endregion

		#region Handsake Methods

		/*
			Client											Server

			ClientHello                 -------->
															ServerHello
															Certificate*
															ServerKeyExchange*
															CertificateRequest*
										<--------			ServerHelloDone
			Certificate*
			ClientKeyExchange
			CertificateVerify*
			[ChangeCipherSpec]
			Finished                    -------->
															[ChangeCipherSpec]
										<--------           Finished
			Application Data            <------->			Application Data

					Fig. 1 - Message flow for a full handshake		
		*/

		internal override IAsyncResult OnBeginNegotiateHandshake(AsyncCallback callback, object state)
		{
			// Reset the context if needed
			if (this.context.HandshakeState != HandshakeState.None)
			{
				this.context.Clear();
			}

			// Obtain supported cipher suites
			this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);

			// Set handshake state
			this.context.HandshakeState = HandshakeState.Started;

			// Receive Client Hello message
			return this.protocol.BeginReceiveRecord(this.innerStream, callback, state);

		}

		internal override void OnNegotiateHandshakeCallback(IAsyncResult asyncResult)
		{
			// Receive Client Hello message and ignore it
			this.protocol.EndReceiveRecord(asyncResult);

			// If received message is not an ClientHello send a
			// Fatal Alert
			if (this.context.LastHandshakeMsg != HandshakeType.ClientHello)
			{
				this.protocol.SendAlert(AlertDescription.UnexpectedMessage);
			}

			// Send ServerHello message
			this.protocol.SendRecord(HandshakeType.ServerHello);

			// Send ServerCertificate message
			this.protocol.SendRecord(HandshakeType.Certificate);

			// If the negotiated cipher is a KeyEx cipher send ServerKeyExchange
			if (this.context.Negotiating.Cipher.IsExportable)
			{
				this.protocol.SendRecord(HandshakeType.ServerKeyExchange);
			}

			bool certRequested = false;

			// If the negotiated cipher is a KeyEx cipher or
			// the client certificate is required send the CertificateRequest message
			if (this.context.Negotiating.Cipher.IsExportable ||
				((ServerContext)this.context).ClientCertificateRequired ||
				((ServerContext)this.context).RequestClientCertificate)
			{
				this.protocol.SendRecord(HandshakeType.CertificateRequest);
				certRequested = true;
			}

			// Send ServerHelloDone message
			this.protocol.SendRecord(HandshakeType.ServerHelloDone);

			// Receive client response, until the Client Finished message
			// is received. IE can be interrupted at this stage and never
			// complete the handshake
			while (this.context.LastHandshakeMsg != HandshakeType.Finished)
			{
				byte[] record = this.protocol.ReceiveRecord(this.innerStream);
				if ((record == null) || (record.Length == 0))
				{
					throw new TlsException(
						AlertDescription.HandshakeFailiure,
						"The client stopped the handshake.");
				}
			}

			if (certRequested) {
				X509Certificate client_cert = this.context.ClientSettings.ClientCertificate;
				if (client_cert == null && ((ServerContext)this.context).ClientCertificateRequired)
					throw new TlsException (AlertDescription.BadCertificate, "No certificate received from client.");

				if (!RaiseClientCertificateValidation (client_cert, new int[0]))
					throw new TlsException (AlertDescription.BadCertificate, "Client certificate not accepted.");
			}

			// Send ChangeCipherSpec and ServerFinished messages
			this.protocol.SendChangeCipherSpec();
			this.protocol.SendRecord (HandshakeType.Finished);

			// The handshake is finished
			this.context.HandshakeState = HandshakeState.Finished;

			// Reset Handshake messages information
			this.context.HandshakeMessages.Reset ();

			// Clear Key Info
			this.context.ClearKeyInfo();
		}

		#endregion

		#region Event Methods

		internal override X509Certificate OnLocalCertificateSelection(X509CertificateCollection clientCertificates, X509Certificate serverCertificate, string targetHost, X509CertificateCollection serverRequestedCertificates)
		{
			throw new NotSupportedException();
		}

		internal override bool OnRemoteCertificateValidation(X509Certificate certificate, int[] errors)
		{
			if (this.ClientCertValidation != null)
			{
				return this.ClientCertValidation(certificate, errors);
			}

			return (errors != null && errors.Length == 0);
		}

		internal override bool HaveRemoteValidation2Callback {
			get { return ClientCertValidation2 != null; }
		}

		internal override ValidationResult OnRemoteCertificateValidation2 (Mono.Security.X509.X509CertificateCollection collection)
		{
			CertificateValidationCallback2 cb = ClientCertValidation2;
			if (cb != null)
				return cb (collection);
			return null;
		}

		internal bool RaiseClientCertificateValidation(
			X509Certificate certificate, 
			int[]			certificateErrors)
		{
			return base.RaiseRemoteCertificateValidation(certificate, certificateErrors);
		}

		internal override AsymmetricAlgorithm OnLocalPrivateKeySelection(X509Certificate certificate, string targetHost)
		{
			if (this.PrivateKeySelection != null)
			{
				return this.PrivateKeySelection(certificate, targetHost);
			}

			return null;
		}

		internal AsymmetricAlgorithm RaisePrivateKeySelection(
			X509Certificate certificate,
			string targetHost)
		{
			return base.RaiseLocalPrivateKeySelection(certificate, targetHost);
		}

		#endregion
	}
}
