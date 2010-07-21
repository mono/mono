//
// System.Net.Security.SslStream.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004,2007 Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_0 && SECURITY_DEP

#if !MOONLIGHT
extern alias PrebuiltSystem;
using X509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#endif

using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;
using Mono.Security.Protocol.Tls;

using CipherAlgorithmType = System.Security.Authentication.CipherAlgorithmType;
using HashAlgorithmType = System.Security.Authentication.HashAlgorithmType;
using ExchangeAlgorithmType = System.Security.Authentication.ExchangeAlgorithmType;

using MonoCipherAlgorithmType = Mono.Security.Protocol.Tls.CipherAlgorithmType;
using MonoHashAlgorithmType = Mono.Security.Protocol.Tls.HashAlgorithmType;
using MonoExchangeAlgorithmType = Mono.Security.Protocol.Tls.ExchangeAlgorithmType;
using MonoSecurityProtocolType = Mono.Security.Protocol.Tls.SecurityProtocolType;

namespace System.Net.Security 
{
	[MonoTODO ("Non-X509Certificate2 certificate is not supported")]
	public class SslStream : AuthenticatedStream
	{
		#region Fields

		SslStreamBase ssl_stream;
		RemoteCertificateValidationCallback validation_callback;
		LocalCertificateSelectionCallback selection_callback;

		#endregion // Fields

		#region Constructors

		public SslStream (Stream innerStream)
			: this (innerStream, false)
		{
		}

		public SslStream (Stream innerStream, bool leaveStreamOpen)
			: base (innerStream, leaveStreamOpen)
		{
		}

		[MonoTODO ("certValidationCallback is not passed X509Chain and SslPolicyErrors correctly")]
		public SslStream (Stream innerStream, bool leaveStreamOpen, RemoteCertificateValidationCallback certValidationCallback)
			: this (innerStream, leaveStreamOpen, certValidationCallback, null)
		{
		}

		[MonoTODO ("certValidationCallback is not passed X509Chain and SslPolicyErrors correctly")]
		public SslStream (Stream innerStream, bool leaveStreamOpen, RemoteCertificateValidationCallback certValidationCallback, LocalCertificateSelectionCallback certSelectionCallback)
			: base (innerStream, leaveStreamOpen)
		{
			// they are nullable.
			validation_callback = certValidationCallback;
			selection_callback = certSelectionCallback;
		}
		#endregion // Constructors

		#region Properties

		public override bool CanRead {
			get { return InnerStream.CanRead; }
		}

		public override bool CanSeek {
			get { return InnerStream.CanSeek; }
		}

		public override bool CanTimeout {
			get { return InnerStream.CanTimeout; }
		}

		public override bool CanWrite {
			get { return InnerStream.CanWrite; }
		}

		public override long Length {
			get { return InnerStream.Length; }
		}

		public override long Position {
			get { return InnerStream.Position; }
			set {
				throw new NotSupportedException ("This stream does not support seek operations");
			}
		}

		// AuthenticatedStream overrides

		public override bool IsAuthenticated { 
			get { return ssl_stream != null; }
		}

		public override bool IsEncrypted { 
			get { return IsAuthenticated; }
		}

		public override bool IsMutuallyAuthenticated { 
			get { return IsAuthenticated && (IsServer ? RemoteCertificate != null : LocalCertificate != null); }
		}

		public override bool IsServer { 
#if MOONLIGHT
			get { return false; }
#else
			get { return ssl_stream is SslServerStream; }
#endif
		}

		public override bool IsSigned { 
			get { return IsAuthenticated; }
		}

		public override int ReadTimeout {
			get { return InnerStream.ReadTimeout; }
			set { InnerStream.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return InnerStream.WriteTimeout; }
			set { InnerStream.WriteTimeout = value; }
		}

		// SslStream

		public virtual bool CheckCertRevocationStatus {
			get {
				if (!IsAuthenticated)
					return false;

				return ssl_stream.CheckCertRevocationStatus;
			}
		}

		public virtual CipherAlgorithmType CipherAlgorithm  {
			get {
				CheckConnectionAuthenticated ();

				switch (ssl_stream.CipherAlgorithm) {
				case MonoCipherAlgorithmType.Des:
					return CipherAlgorithmType.Des;
				case MonoCipherAlgorithmType.None:
					return CipherAlgorithmType.None;
				case MonoCipherAlgorithmType.Rc2:
					return CipherAlgorithmType.Rc2;
				case MonoCipherAlgorithmType.Rc4:
					return CipherAlgorithmType.Rc4;
				case MonoCipherAlgorithmType.SkipJack:
					break;
				case MonoCipherAlgorithmType.TripleDes:
					return CipherAlgorithmType.TripleDes;
				case MonoCipherAlgorithmType.Rijndael:
					switch (ssl_stream.CipherStrength) {
					case 128:
						return CipherAlgorithmType.Aes128;
					case 192:
						return CipherAlgorithmType.Aes192;
					case 256:
						return CipherAlgorithmType.Aes256;
					}
					break;
				}

				throw new InvalidOperationException ("Not supported cipher algorithm is in use. It is likely a bug in SslStream.");
			}
		}

		public virtual int CipherStrength  {
			get {
				CheckConnectionAuthenticated ();

				return ssl_stream.CipherStrength;
			}
		}

		public virtual HashAlgorithmType HashAlgorithm  {
			get {
				CheckConnectionAuthenticated ();

				switch (ssl_stream.HashAlgorithm) {
				case MonoHashAlgorithmType.Md5:
					return HashAlgorithmType.Md5;
				case MonoHashAlgorithmType.None:
					return HashAlgorithmType.None;
				case MonoHashAlgorithmType.Sha1:
					return HashAlgorithmType.Sha1;
				}

				throw new InvalidOperationException ("Not supported hash algorithm is in use. It is likely a bug in SslStream.");
			}
		}

		public virtual int HashStrength  {
			get {
				CheckConnectionAuthenticated ();

				return ssl_stream.HashStrength;
			}
		}

		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm { 
			get {
				CheckConnectionAuthenticated ();

				switch (ssl_stream.KeyExchangeAlgorithm) {
				case MonoExchangeAlgorithmType.DiffieHellman:
					return ExchangeAlgorithmType.DiffieHellman;
				case MonoExchangeAlgorithmType.Fortezza:
					break;
				case MonoExchangeAlgorithmType.None:
					return ExchangeAlgorithmType.None;
				case MonoExchangeAlgorithmType.RsaKeyX:
					return ExchangeAlgorithmType.RsaKeyX;
				case MonoExchangeAlgorithmType.RsaSign:
					return ExchangeAlgorithmType.RsaSign;
				}

				throw new InvalidOperationException ("Not supported exchange algorithm is in use. It is likely a bug in SslStream.");
			}
		}

		public virtual int KeyExchangeStrength { 
			get {
				CheckConnectionAuthenticated ();

				return ssl_stream.KeyExchangeStrength;
			}
		}

		public virtual X509Certificate LocalCertificate {
			get {
				CheckConnectionAuthenticated ();

				return IsServer ? ssl_stream.ServerCertificate : ((SslClientStream) ssl_stream).SelectedClientCertificate;
			}
		}

		public virtual X509Certificate RemoteCertificate {
			get {
				CheckConnectionAuthenticated ();
#if MOONLIGHT
				return ssl_stream.ServerCertificate;
#else
				return !IsServer ? ssl_stream.ServerCertificate : ((SslServerStream) ssl_stream).ClientCertificate;
#endif
			}
		}

		public virtual SslProtocols SslProtocol {
			get {
				CheckConnectionAuthenticated ();

				switch (ssl_stream.SecurityProtocol) {
				case MonoSecurityProtocolType.Default:
					return SslProtocols.Default;
				case MonoSecurityProtocolType.Ssl2:
					return SslProtocols.Ssl2;
				case MonoSecurityProtocolType.Ssl3:
					return SslProtocols.Ssl3;
				case MonoSecurityProtocolType.Tls:
					return SslProtocols.Tls;
				}

				throw new InvalidOperationException ("Not supported SSL/TLS protocol is in use. It is likely a bug in SslStream.");
			}
		}

		#endregion // Properties

		#region Methods
/*
		AsymmetricAlgorithm GetPrivateKey (X509Certificate cert, string targetHost)
		{
			// FIXME: what can I do for non-X509Certificate2 ?
			X509Certificate2 cert2 = cert as X509Certificate2;
			return cert2 != null ? cert2.PrivateKey : null;
		}
*/
		X509Certificate OnCertificateSelection (X509CertificateCollection clientCerts, X509Certificate serverCert, string targetHost, X509CertificateCollection serverRequestedCerts)
		{
			string [] acceptableIssuers = new string [serverRequestedCerts != null ? serverRequestedCerts.Count : 0];
			for (int i = 0; i < acceptableIssuers.Length; i++)
				acceptableIssuers [i] = serverRequestedCerts [i].GetIssuerName ();
			return selection_callback (this, targetHost, clientCerts, serverCert, acceptableIssuers);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient (targetHost, new X509CertificateCollection (), SslProtocols.Tls, false, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols sslProtocolType, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			if (IsAuthenticated)
				throw new InvalidOperationException ("This SslStream is already authenticated");

			SslClientStream s = new SslClientStream (InnerStream,  targetHost, !LeaveInnerStreamOpen, GetMonoSslProtocol (sslProtocolType), clientCertificates);
			s.CheckCertRevocationStatus = checkCertificateRevocation;

			// Due to the Mono.Security internal, it cannot reuse
			// the delegated argument, as Mono.Security creates 
			// another instance of X509Certificate which lacks 
			// private key but is filled the private key via this
			// delegate.
			s.PrivateKeyCertSelectionDelegate = delegate (X509Certificate cert, string host) {
				string hash = cert.GetCertHashString ();
				// ... so, we cannot use the delegate argument.
				foreach (X509Certificate cc in clientCertificates) {
					if (cc.GetCertHashString () != hash)
						continue;
					X509Certificate2 cert2 = cc as X509Certificate2;
					cert2 = cert2 ?? new X509Certificate2 (cc);
					return cert2.PrivateKey;
				}
				return null;
			};

			if (validation_callback != null)
				s.ServerCertValidationDelegate = delegate (X509Certificate cert, int [] certErrors) {
					X509Chain chain = new X509Chain ();
					X509Certificate2 x2 = (cert as X509Certificate2);
					if (x2 == null)
						x2 = new X509Certificate2 (cert);

					if (!ServicePointManager.CheckCertificateRevocationList)
						chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

					// SSL specific checks (done by Mono.Security.dll SSL/TLS implementation) 
					SslPolicyErrors errors = SslPolicyErrors.None;
					foreach (int i in certErrors) {
						switch (i) {
						case -2146762490: // CERT_E_PURPOSE
							errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
							break;
						case -2146762481: // CERT_E_CN_NO_MATCH
							errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
							break;
						default:
							errors |= SslPolicyErrors.RemoteCertificateChainErrors;
							break;
						}
					}

					chain.Build (x2);

					// non-SSL specific X509 checks (i.e. RFC3280 related checks)
					foreach (X509ChainStatus status in chain.ChainStatus) {
						if (status.Status == X509ChainStatusFlags.NoError)
							continue;
						if ((status.Status & X509ChainStatusFlags.PartialChain) != 0)
							errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
						else
							errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					}

					return validation_callback (this, cert, chain, errors);
				};
			if (selection_callback != null)
				s.ClientCertSelectionDelegate = OnCertificateSelection;

			ssl_stream = s;

			return BeginWrite (new byte [0], 0, 0, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			CheckConnectionAuthenticated ();

			return ssl_stream.BeginRead (buffer, offset, count, asyncCallback, asyncState);
		}
#if !MOONLIGHT
		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback callback, object asyncState)
		{
			return BeginAuthenticateAsServer (serverCertificate, false, SslProtocols.Tls, false, callback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols sslProtocolType, bool checkCertificateRevocation, AsyncCallback callback, object asyncState)
		{
			if (IsAuthenticated)
				throw new InvalidOperationException ("This SslStream is already authenticated");

			SslServerStream s = new SslServerStream (InnerStream, serverCertificate, clientCertificateRequired, !LeaveInnerStreamOpen, GetMonoSslProtocol (sslProtocolType));
			s.CheckCertRevocationStatus = checkCertificateRevocation;
			// Due to the Mono.Security internal, it cannot reuse
			// the delegated argument, as Mono.Security creates 
			// another instance of X509Certificate which lacks 
			// private key but is filled the private key via this
			// delegate.
			s.PrivateKeyCertSelectionDelegate = delegate (X509Certificate cert, string targetHost) {
				// ... so, we cannot use the delegate argument.
				X509Certificate2 cert2 = serverCertificate as X509Certificate2 ?? new X509Certificate2 (serverCertificate);
				return cert2 != null ? cert2.PrivateKey : null;
			};

			if (validation_callback != null)
				s.ClientCertValidationDelegate = delegate (X509Certificate cert, int [] certErrors) {
					X509Chain chain = null;
					if (cert is X509Certificate2) {
						chain = new X509Chain ();
						chain.Build ((X509Certificate2) cert);
					}
					// FIXME: SslPolicyErrors is incomplete
					SslPolicyErrors errors = certErrors.Length > 0 ? SslPolicyErrors.RemoteCertificateChainErrors : SslPolicyErrors.None;
					return validation_callback (this, cert, chain, errors);
				};

			ssl_stream = s;

			return BeginRead (new byte [0], 0, 0, callback, asyncState);
		}
#endif
		MonoSecurityProtocolType GetMonoSslProtocol (SslProtocols ms)
		{
			switch (ms) {
			case SslProtocols.Ssl2:
				return MonoSecurityProtocolType.Ssl2;
			case SslProtocols.Ssl3:
				return MonoSecurityProtocolType.Ssl3;
			case SslProtocols.Tls:
				return MonoSecurityProtocolType.Tls;
			default:
				return MonoSecurityProtocolType.Default;
			}
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			CheckConnectionAuthenticated ();

			return ssl_stream.BeginWrite (buffer, offset, count, asyncCallback, asyncState);
		}

		public virtual void AuthenticateAsClient (string targetHost)
		{
			AuthenticateAsClient (targetHost, new X509CertificateCollection (), SslProtocols.Tls, false);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols sslProtocolType, bool checkCertificateRevocation)
		{
			EndAuthenticateAsClient (BeginAuthenticateAsClient (
				targetHost, clientCertificates, sslProtocolType, checkCertificateRevocation, null, null));
		}
#if !MOONLIGHT
		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			AuthenticateAsServer (serverCertificate, false, SslProtocols.Tls, false);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols sslProtocolType, bool checkCertificateRevocation)
		{
			EndAuthenticateAsServer (BeginAuthenticateAsServer (
				serverCertificate, clientCertificateRequired, sslProtocolType, checkCertificateRevocation, null, null));
		}
#endif
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (ssl_stream != null)
					ssl_stream.Dispose ();
				ssl_stream = null;
			}
			base.Dispose (disposing);
		}

		public virtual void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			CheckConnectionAuthenticated ();

			if (CanRead)
				ssl_stream.EndRead (asyncResult);
			else
				ssl_stream.EndWrite (asyncResult);
		}
#if !MOONLIGHT
		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			CheckConnectionAuthenticated ();

			if (CanRead)
				ssl_stream.EndRead (asyncResult);
			else
				ssl_stream.EndWrite (asyncResult);
		}
#endif
		public override int EndRead (IAsyncResult asyncResult)
		{
			CheckConnectionAuthenticated ();

			return ssl_stream.EndRead (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			CheckConnectionAuthenticated ();

			ssl_stream.EndWrite (asyncResult);
		}

		public override void Flush ()
		{
			CheckConnectionAuthenticated ();

			InnerStream.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return EndRead (BeginRead (buffer, offset, count, null, null));
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ("This stream does not support seek operations");
		}

		public override void SetLength (long value)
		{
			InnerStream.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			EndWrite (BeginWrite (buffer, offset, count, null, null));
		}

		public void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		void CheckConnectionAuthenticated ()
		{
			if (!IsAuthenticated)
				throw new InvalidOperationException ("This operation is invalid until it is successfully authenticated");
		}

		#endregion // Methods
	}
}

#endif
