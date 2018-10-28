//
// System.Net.Security.SslStream.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004,2007 Novell, Inc. (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
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

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoCipherAlgorithmType = MonoSecurity::Mono.Security.Protocol.Tls.CipherAlgorithmType;
using MonoHashAlgorithmType = MonoSecurity::Mono.Security.Protocol.Tls.HashAlgorithmType;
using MonoExchangeAlgorithmType = MonoSecurity::Mono.Security.Protocol.Tls.ExchangeAlgorithmType;
using MonoSecurityProtocolType = MonoSecurity::Mono.Security.Protocol.Tls.SecurityProtocolType;
using MonoSecurity::Mono.Security.Protocol.Tls;
using MonoSecurity::Mono.Security.Interface;
#else
using MonoCipherAlgorithmType = Mono.Security.Protocol.Tls.CipherAlgorithmType;
using MonoHashAlgorithmType = Mono.Security.Protocol.Tls.HashAlgorithmType;
using MonoExchangeAlgorithmType = Mono.Security.Protocol.Tls.ExchangeAlgorithmType;
using MonoSecurityProtocolType = Mono.Security.Protocol.Tls.SecurityProtocolType;
using Mono.Security.Protocol.Tls;
using Mono.Security.Interface;
#endif

using CipherAlgorithmType = System.Security.Authentication.CipherAlgorithmType;
using HashAlgorithmType = System.Security.Authentication.HashAlgorithmType;
using ExchangeAlgorithmType = System.Security.Authentication.ExchangeAlgorithmType;

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;

namespace Mono.Net.Security.Private
{
	/*
	 * Strictly private - do not use outside the Mono.Net.Security directory.
	 */
	[MonoTODO ("Non-X509Certificate2 certificate is not supported")]
	internal class LegacySslStream : AuthenticatedStream, IMonoSslStream
	{
		#region Fields

		SslStreamBase ssl_stream;
		ICertificateValidator certificateValidator;

		#endregion // Fields

		#region Constructors

		public LegacySslStream (Stream innerStream, bool leaveInnerStreamOpen, SslStream owner, MonoTlsProvider provider, MonoTlsSettings settings)
			: base (innerStream, leaveInnerStreamOpen)
		{
			SslStream = owner;
			Provider = provider;
			certificateValidator = ChainValidationHelper.GetInternalValidator (owner, provider, settings);
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
			get { return ssl_stream is SslServerStream; }
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

		X509Certificate IMonoSslStream.InternalLocalCertificate {
			get {
				return IsServer ? ssl_stream.ServerCertificate : ((SslClientStream) ssl_stream).SelectedClientCertificate;
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
				return !IsServer ? ssl_stream.ServerCertificate : ((SslServerStream) ssl_stream).ClientCertificate;
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
#pragma warning disable 618
			string [] acceptableIssuers = new string [serverRequestedCerts != null ? serverRequestedCerts.Count : 0];
			for (int i = 0; i < acceptableIssuers.Length; i++)
				acceptableIssuers [i] = serverRequestedCerts [i].GetIssuerName ();
			X509Certificate clientCertificate;
			certificateValidator.SelectClientCertificate (targetHost, clientCerts, serverCert, acceptableIssuers, out clientCertificate);
			return clientCertificate;
#pragma warning restore 618
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient (targetHost, new X509CertificateCollection (), SslProtocols.Tls, false, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			if (IsAuthenticated)
				throw new InvalidOperationException ("This SslStream is already authenticated");

			SslClientStream s = new SslClientStream (InnerStream, targetHost, !LeaveInnerStreamOpen, GetMonoSslProtocol (enabledSslProtocols), clientCertificates);
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

			// Even if validation_callback is null this allows us to verify requests where the user
			// does not provide a verification callback but attempts to authenticate with the website
			// as a client (see https://bugzilla.xamarin.com/show_bug.cgi?id=18962 for an example)
			s.ServerCertValidation2 += (mcerts) => {
				X509CertificateCollection certs = null;
				if (mcerts != null) {
					certs = new X509CertificateCollection ();
					for (int i = 0; i < mcerts.Count; i++)
						certs.Add (new X509Certificate2 (mcerts [i].RawData));
				}
				return ((ChainValidationHelper)certificateValidator).ValidateCertificate (targetHost, false, certs);
			};
			s.ClientCertSelectionDelegate = OnCertificateSelection;

			ssl_stream = s;

			return BeginWrite (new byte [0], 0, 0, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			CheckConnectionAuthenticated ();

			return ssl_stream.BeginRead (buffer, offset, count, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsServer (serverCertificate, false, SslProtocols.Tls, false, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			if (IsAuthenticated)
				throw new InvalidOperationException ("This SslStream is already authenticated");

			SslServerStream s = new SslServerStream (InnerStream, serverCertificate, false, clientCertificateRequired, !LeaveInnerStreamOpen, GetMonoSslProtocol (enabledSslProtocols));
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

			s.ClientCertValidationDelegate = delegate (X509Certificate cert, int[] certErrors) {
				var errors = certErrors.Length > 0 ? MonoSslPolicyErrors.RemoteCertificateChainErrors : MonoSslPolicyErrors.None;
				return ((ChainValidationHelper)certificateValidator).ValidateClientCertificate (cert, errors);
			};

			ssl_stream = s;

			return BeginWrite (new byte[0], 0, 0, asyncCallback, asyncState);
		}

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

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
		{
			AuthenticateAsClient (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			EndAuthenticateAsClient (BeginAuthenticateAsClient (
				targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, null, null));
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			AuthenticateAsServer (serverCertificate, false, SslProtocols.Tls, false);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
		{
			AuthenticateAsServer (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			EndAuthenticateAsServer (BeginAuthenticateAsServer (
				serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, null, null));
		}

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

		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			CheckConnectionAuthenticated ();

			if (CanRead)
				ssl_stream.EndRead (asyncResult);
			else
				ssl_stream.EndWrite (asyncResult);
		}

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

		public virtual Task AuthenticateAsClientAsync (string targetHost)
		{
			return Task.Factory.FromAsync (BeginAuthenticateAsClient, EndAuthenticateAsClient, targetHost, null);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
		{
			return AuthenticateAsClientAsync (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			var t = Tuple.Create (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, this);

			return Task.Factory.FromAsync ((callback, state) => {
				var d = (Tuple<string, X509CertificateCollection, SslProtocols, bool, LegacySslStream>) state;
				return d.Item5.BeginAuthenticateAsClient (d.Item1, d.Item2, d.Item3, d.Item4, callback, null);
			}, EndAuthenticateAsClient, t);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Task.Factory.FromAsync (BeginAuthenticateAsServer, EndAuthenticateAsServer, serverCertificate, null);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
		{
			return AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			var t = Tuple.Create (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, this);

			return Task.Factory.FromAsync ((callback, state) => {
				var d = (Tuple<X509Certificate, bool, SslProtocols, bool, LegacySslStream>) state;
				return d.Item5.BeginAuthenticateAsServer (d.Item1, d.Item2, d.Item3, d.Item4, callback, null);
			}, EndAuthenticateAsServer, t);
		}

		#endregion // Methods

		#region IMonoSslStream

		Task IMonoSslStream.ShutdownAsync ()
		{
			return Task.CompletedTask;
		}

		AuthenticatedStream IMonoSslStream.AuthenticatedStream {
			get { return this; }
		}

		TransportContext IMonoSslStream.TransportContext {
			get { throw new NotSupportedException (); }
		}

		public SslStream SslStream {
			get;
		}

		public MonoTlsProvider Provider {
			get;
		}

		public MonoTlsConnectionInfo GetConnectionInfo ()
		{
			return null;
		}

		public bool CanRenegotiate => false;

		public Task RenegotiateAsync (CancellationToken cancellationToken)
		{
			throw new NotSupportedException ();
		}

		#endregion
	}
}

#endif
