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
using System.Threading;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	#region Delegates

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	delegate bool CertificateValidationCallback(
		X509Certificate certificate, 
		int[]			certificateErrors);

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class ValidationResult {
		bool trusted;
		bool user_denied;
		int error_code;

		public ValidationResult (bool trusted, bool user_denied, int error_code)
		{
			this.trusted = trusted;
			this.user_denied = user_denied;
			this.error_code = error_code;
		}

		public bool Trusted {
			get { return trusted; }
		}

		public bool UserDenied {
			get { return user_denied; }
		}

		public int ErrorCode {
			get { return error_code; }
		}
	}

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	delegate ValidationResult CertificateValidationCallback2 (Mono.Security.X509.X509CertificateCollection collection);

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	delegate X509Certificate CertificateSelectionCallback(
		X509CertificateCollection	clientCertificates, 
		X509Certificate				serverCertificate, 
		string						targetHost, 
		X509CertificateCollection	serverRequestedCertificates);

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	delegate AsymmetricAlgorithm PrivateKeySelectionCallback(
		X509Certificate	certificate, 
		string			targetHost);

	#endregion

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class SslClientStream : SslStreamBase
	{
		#region Internal Events
		
		internal event CertificateValidationCallback	ServerCertValidation;
		internal event CertificateSelectionCallback		ClientCertSelection;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Properties

		// required by HttpsClientStream for proxy support
		internal Stream InputBuffer 
		{
			get { return base.inputBuffer; }
		}

		public X509CertificateCollection ClientCertificates
		{
			get { return this.context.ClientSettings.Certificates; }
		}

		public X509Certificate SelectedClientCertificate
		{
			get { return this.context.ClientSettings.ClientCertificate; }
		}

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ServerCertValidationDelegate
		{
			get { return this.ServerCertValidation; }
			set { this.ServerCertValidation = value; }			
		}

		public CertificateSelectionCallback ClientCertSelectionDelegate 
		{
			get { return this.ClientCertSelection; }
			set { this.ClientCertSelection = value; }
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate
		{
			get { return this.PrivateKeySelection; }
			set { this.PrivateKeySelection = value; }
		}
		
		#endregion

		public event CertificateValidationCallback2 ServerCertValidation2;

		#region Constructors
		
		public SslClientStream(
			Stream	stream, 
			string	targetHost, 
			bool	ownsStream) 
			: this(
				stream, targetHost, ownsStream, 
				SecurityProtocolType.Default, null)
		{
		}
		
		public SslClientStream(
			Stream				stream, 
			string				targetHost, 
			X509Certificate		clientCertificate) 
			: this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				new X509CertificateCollection(new X509Certificate[]{clientCertificate}))
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost, 
			X509CertificateCollection clientCertificates) : 
			this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				clientCertificates)
		{
		}

		public SslClientStream(
			Stream					stream,
			string					targetHost,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType) 
			: this(
				stream, targetHost, ownsStream, securityProtocolType,
				new X509CertificateCollection())
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost,
			bool						ownsStream,
			SecurityProtocolType		securityProtocolType,
			X509CertificateCollection	clientCertificates):
			base(stream, ownsStream)
		{
			if (targetHost == null || targetHost.Length == 0)
			{
				throw new ArgumentNullException("targetHost is null or an empty string.");
			}

			this.context = new ClientContext(
				this,
				securityProtocolType, 
				targetHost, 
				clientCertificates);

			this.protocol = new ClientRecordProtocol(innerStream, (ClientContext)this.context);
		}

		#endregion

		#region Finalizer

		~SslClientStream()
		{
			base.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				this.ServerCertValidation = null;
				this.ClientCertSelection = null;
				this.PrivateKeySelection = null;
				this.ServerCertValidation2 = null;
			}
		}

		#endregion

		#region Handshake Methods

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

		private void SafeEndReceiveRecord (IAsyncResult ar, bool ignoreEmpty = false)
		{
			byte[] record = this.protocol.EndReceiveRecord (ar);
			if (!ignoreEmpty && ((record == null) || (record.Length == 0))) {
				throw new TlsException (
					AlertDescription.HandshakeFailiure,
					"The server stopped the handshake.");
			}
		}

		private enum NegotiateState
		{
			SentClientHello,
			ReceiveClientHelloResponse,
			SentCipherSpec,
			ReceiveCipherSpecResponse,
			SentKeyExchange,
			ReceiveFinishResponse,
			SentFinished,
		};

		private class NegotiateAsyncResult : IAsyncResult
		{
			private object locker = new object ();
			private AsyncCallback _userCallback;
			private object _userState;
			private Exception _asyncException;
			private ManualResetEvent handle;
			private NegotiateState _state;
			private bool completed;

			public NegotiateAsyncResult(AsyncCallback userCallback, object userState, NegotiateState state)
			{
				_userCallback = userCallback;
				_userState = userState;
				_state = state;
			}

			public NegotiateState State
			{
				get { return _state; }
				set { _state = value; }
			}

			public object AsyncState
			{
				get { return _userState; }
			}

			public Exception AsyncException
			{
				get { return _asyncException; }
			}

			public bool CompletedWithError
			{
				get {
					if (!IsCompleted)
						return false; // Perhaps throw InvalidOperationExcetion?

					return null != _asyncException;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get {
					lock (locker) {
						if (handle == null)
							handle = new ManualResetEvent (completed);
					}
					return handle;
				}

			}

			public bool CompletedSynchronously
			{
				get { return false; }
			}

			public bool IsCompleted
			{
				get {
					lock (locker) {
						return completed;
					}
				}
			}

			public void SetComplete(Exception ex)
			{
				lock (locker) {
					if (completed)
						return;

					completed = true;
					if (handle != null)
						handle.Set ();

					if (_userCallback != null)
						_userCallback.BeginInvoke (this, null, null);

					_asyncException = ex;
				}
			}

			public void SetComplete()
			{
				SetComplete(null);
			}
		}

		internal override IAsyncResult BeginNegotiateHandshake(AsyncCallback callback, object state)
		{
			if (this.context.HandshakeState != HandshakeState.None) {
				this.context.Clear ();
			}

			// Obtain supported cipher suites
			this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (false, context.SecurityProtocol);

			// Set handshake state
			this.context.HandshakeState = HandshakeState.Started;

			NegotiateAsyncResult result = new NegotiateAsyncResult (callback, state, NegotiateState.SentClientHello);

			// Begin sending the client hello
			this.protocol.BeginSendRecord (HandshakeType.ClientHello, NegotiateAsyncWorker, result);

			return result;
		}

		internal override void EndNegotiateHandshake (IAsyncResult result)
		{
			NegotiateAsyncResult negotiate = result as NegotiateAsyncResult;

			if (negotiate == null)
				throw new ArgumentNullException ();
			if (!negotiate.IsCompleted)
				negotiate.AsyncWaitHandle.WaitOne();
			if (negotiate.CompletedWithError)
				throw negotiate.AsyncException;
		}

		private void NegotiateAsyncWorker (IAsyncResult result)
		{
			NegotiateAsyncResult negotiate = result.AsyncState as NegotiateAsyncResult;

			try
			{
				switch (negotiate.State)
				{
				case NegotiateState.SentClientHello:
					this.protocol.EndSendRecord (result);

					// we are now ready to ready the receive the hello response.
					negotiate.State = NegotiateState.ReceiveClientHelloResponse;

					// Start reading the client hello response
					this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);
					break;

				case NegotiateState.ReceiveClientHelloResponse:
					this.SafeEndReceiveRecord (result, true);

					if (this.context.LastHandshakeMsg != HandshakeType.ServerHelloDone &&
						(!this.context.AbbreviatedHandshake || this.context.LastHandshakeMsg != HandshakeType.ServerHello)) {
						// Read next record (skip empty, e.g. warnings alerts)
						this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);
						break;
					}

					// special case for abbreviated handshake where no ServerHelloDone is sent from the server
					if (this.context.AbbreviatedHandshake) {
						ClientSessionCache.SetContextFromCache (this.context);
						this.context.Negotiating.Cipher.ComputeKeys ();
						this.context.Negotiating.Cipher.InitializeCipher ();

						negotiate.State = NegotiateState.SentCipherSpec;

						// Send Change Cipher Spec message with the current cipher
						// or as plain text if this is the initial negotiation
						this.protocol.BeginSendChangeCipherSpec(NegotiateAsyncWorker, negotiate);
					} else {
						// Send client certificate if requested
						// even if the server ask for it it _may_ still be optional
						bool clientCertificate = this.context.ServerSettings.CertificateRequest;

						using (var memstream = new MemoryStream())
						{
							// NOTE: sadly SSL3 and TLS1 differs in how they handle this and
							// the current design doesn't allow a very cute way to handle 
							// SSL3 alert warning for NoCertificate (41).
							if (this.context.SecurityProtocol == SecurityProtocolType.Ssl3)
							{
								clientCertificate = ((this.context.ClientSettings.Certificates != null) &&
									(this.context.ClientSettings.Certificates.Count > 0));
								// this works well with OpenSSL (but only for SSL3)
							}

							byte[] record = null;

							if (clientCertificate)
							{
								record = this.protocol.EncodeHandshakeRecord(HandshakeType.Certificate);
								memstream.Write(record, 0, record.Length);
							}

							// Send Client Key Exchange
							record = this.protocol.EncodeHandshakeRecord(HandshakeType.ClientKeyExchange);
							memstream.Write(record, 0, record.Length);

							// Now initialize session cipher with the generated keys
							this.context.Negotiating.Cipher.InitializeCipher();

							// Send certificate verify if requested (optional)
							if (clientCertificate && (this.context.ClientSettings.ClientCertificate != null))
							{
								record = this.protocol.EncodeHandshakeRecord(HandshakeType.CertificateVerify);
								memstream.Write(record, 0, record.Length);
							}

							// send the chnage cipher spec.
							this.protocol.SendChangeCipherSpec(memstream);

							// Send Finished message
							record = this.protocol.EncodeHandshakeRecord(HandshakeType.Finished);
							memstream.Write(record, 0, record.Length);

							negotiate.State = NegotiateState.SentKeyExchange;

							// send all the records.
							this.innerStream.BeginWrite (memstream.GetBuffer (), 0, (int)memstream.Length, NegotiateAsyncWorker, negotiate);
						}
					}
					break;

				case NegotiateState.SentKeyExchange:
					this.innerStream.EndWrite (result);

					negotiate.State = NegotiateState.ReceiveFinishResponse;

					this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);

					break;

				case NegotiateState.ReceiveFinishResponse:
					this.SafeEndReceiveRecord (result);

					// Read record until server finished is received
					if (this.context.HandshakeState != HandshakeState.Finished) {
						// If all goes well this will process messages:
						// 		Change Cipher Spec
						//		Server finished
						this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);
					}
					else {
						// Reset Handshake messages information
						this.context.HandshakeMessages.Reset ();

						// Clear Key Info
						this.context.ClearKeyInfo();

						negotiate.SetComplete ();
					}
					break;


				case NegotiateState.SentCipherSpec:
					this.protocol.EndSendChangeCipherSpec (result);

					negotiate.State = NegotiateState.ReceiveCipherSpecResponse;

					// Start reading the cipher spec response
					this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);
					break;

				case NegotiateState.ReceiveCipherSpecResponse:
					this.SafeEndReceiveRecord (result, true);

					if (this.context.HandshakeState != HandshakeState.Finished)
					{
						this.protocol.BeginReceiveRecord (this.innerStream, NegotiateAsyncWorker, negotiate);
					}
					else
					{
						negotiate.State = NegotiateState.SentFinished;
						this.protocol.BeginSendRecord(HandshakeType.Finished, NegotiateAsyncWorker, negotiate);
					}
					break;

				case NegotiateState.SentFinished:
					this.protocol.EndSendRecord (result);

					// Reset Handshake messages information
					this.context.HandshakeMessages.Reset ();

					// Clear Key Info
					this.context.ClearKeyInfo();

					negotiate.SetComplete ();

					break;
				}
			}
			catch (TlsException ex)
			{
				// FIXME: should the send alert also be done asynchronously here and below?
				this.protocol.SendAlert(ex.Alert);
				negotiate.SetComplete (new IOException("The authentication or decryption has failed.", ex));
			}
			catch (Exception ex)
			{
				this.protocol.SendAlert(AlertDescription.InternalError);
				negotiate.SetComplete (new IOException("The authentication or decryption has failed.", ex));
			}
		}

		#endregion

		#region Event Methods

		internal override X509Certificate OnLocalCertificateSelection(X509CertificateCollection clientCertificates, X509Certificate serverCertificate, string targetHost, X509CertificateCollection serverRequestedCertificates)
		{
			if (this.ClientCertSelection != null)
			{
				return this.ClientCertSelection(
					clientCertificates,
					serverCertificate,
					targetHost,
					serverRequestedCertificates);
			}

			return null;
		}

		internal override bool HaveRemoteValidation2Callback {
			get { return ServerCertValidation2 != null; }
		}

		internal override ValidationResult OnRemoteCertificateValidation2 (Mono.Security.X509.X509CertificateCollection collection)
		{
			CertificateValidationCallback2 cb = ServerCertValidation2;
			if (cb != null)
				return cb (collection);
			return null;
		}

		internal override bool OnRemoteCertificateValidation(X509Certificate certificate, int[] errors)
		{
			if (this.ServerCertValidation != null)
			{
				return this.ServerCertValidation(certificate, errors);
			}

			return (errors != null && errors.Length == 0);
		}

		internal virtual bool RaiseServerCertificateValidation(
			X509Certificate certificate, 
			int[]			certificateErrors)
		{
			return base.RaiseRemoteCertificateValidation(certificate, certificateErrors);
		}

		internal virtual ValidationResult RaiseServerCertificateValidation2 (Mono.Security.X509.X509CertificateCollection collection)
		{
			return base.RaiseRemoteCertificateValidation2 (collection);
		}

		internal X509Certificate RaiseClientCertificateSelection(
			X509CertificateCollection	clientCertificates, 
			X509Certificate				serverCertificate, 
			string						targetHost, 
			X509CertificateCollection	serverRequestedCertificates)
		{
			return base.RaiseLocalCertificateSelection(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
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
