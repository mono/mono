//
// SslStream.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace System.Net.Security
{
	/*
	 * These two are defined by the referencesource; add them here to make
	 * it easy to switch between the two implementations.
	 */

	internal delegate bool RemoteCertValidationCallback (
		string host,
		X509Certificate certificate,
		X509Chain chain,
		SslPolicyErrors sslPolicyErrors);

	internal delegate X509Certificate LocalCertSelectionCallback (
		string targetHost,
		X509CertificateCollection localCertificates,
		X509Certificate remoteCertificate,
		string[] acceptableIssuers);

	public partial class SslStream : System.Net.Security.AuthenticatedStream
	{
		public SslStream(System.IO.Stream innerStream) : base (default(System.IO.Stream), default(bool)) => throw new PlatformNotSupportedException ();
		public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen) : base (default(System.IO.Stream), default(bool)) => throw new PlatformNotSupportedException ();
		public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen, System.Net.Security.RemoteCertificateValidationCallback userCertificateValidationCallback) : base (default(System.IO.Stream), default(bool)) => throw new PlatformNotSupportedException ();
		public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen, System.Net.Security.RemoteCertificateValidationCallback userCertificateValidationCallback, System.Net.Security.LocalCertificateSelectionCallback userCertificateSelectionCallback) : base (default(System.IO.Stream), default(bool)) => throw new PlatformNotSupportedException ();
		public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen, System.Net.Security.RemoteCertificateValidationCallback userCertificateValidationCallback, System.Net.Security.LocalCertificateSelectionCallback userCertificateSelectionCallback, System.Net.Security.EncryptionPolicy encryptionPolicy) : base (default(System.IO.Stream), default(bool)) => throw new PlatformNotSupportedException ();
		public override bool CanRead { get { throw new PlatformNotSupportedException (); } }
		public override bool CanSeek { get { throw new PlatformNotSupportedException (); } }
		public override bool CanTimeout { get { throw new PlatformNotSupportedException (); } }
		public override bool CanWrite { get { throw new PlatformNotSupportedException (); } }
		public virtual bool CheckCertRevocationStatus { get { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Authentication.CipherAlgorithmType CipherAlgorithm { get { throw new PlatformNotSupportedException (); } }
		public virtual int CipherStrength { get { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Authentication.HashAlgorithmType HashAlgorithm { get { throw new PlatformNotSupportedException (); } }
		public virtual int HashStrength { get { throw new PlatformNotSupportedException (); } }
		public override bool IsAuthenticated { get { throw new PlatformNotSupportedException (); } }
		public override bool IsEncrypted { get { throw new PlatformNotSupportedException (); } }
		public override bool IsMutuallyAuthenticated { get { throw new PlatformNotSupportedException (); } }
		public override bool IsServer { get { throw new PlatformNotSupportedException (); } }
		public override bool IsSigned { get { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Authentication.ExchangeAlgorithmType KeyExchangeAlgorithm { get { throw new PlatformNotSupportedException (); } }
		public virtual int KeyExchangeStrength { get { throw new PlatformNotSupportedException (); } }
		public override long Length { get { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Cryptography.X509Certificates.X509Certificate LocalCertificate { get { throw new PlatformNotSupportedException (); } }
		public System.Net.Security.SslApplicationProtocol NegotiatedApplicationProtocol { get { throw new PlatformNotSupportedException (); } }
		public override long Position { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }
		public override int ReadTimeout { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Cryptography.X509Certificates.X509Certificate RemoteCertificate { get { throw new PlatformNotSupportedException (); } }
		public virtual System.Security.Authentication.SslProtocols SslProtocol { get { throw new PlatformNotSupportedException (); } }
		public System.Net.TransportContext TransportContext { get { throw new PlatformNotSupportedException (); } }
		public override int WriteTimeout { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }
		public virtual void AuthenticateAsClient(string targetHost) { throw new PlatformNotSupportedException (); }
		public virtual void AuthenticateAsClient(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual void AuthenticateAsClient(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public System.Threading.Tasks.Task AuthenticateAsClientAsync(System.Net.Security.SslClientAuthenticationOptions sslClientAuthenticationOptions, System.Threading.CancellationToken cancellationToken) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsClientAsync(string targetHost) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsClientAsync(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsClientAsync(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual void AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate) { throw new PlatformNotSupportedException (); }
		public virtual void AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual void AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Net.Security.SslServerAuthenticationOptions sslServerAuthenticationOptions, System.Threading.CancellationToken cancellationToken) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsClient(string targetHost, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsClient(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, bool checkCertificateRevocation, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsClient(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public virtual System.IAsyncResult BeginAuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public override System.IAsyncResult BeginRead(byte[] buffer, int offset, int count, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		public override System.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback asyncCallback, object asyncState) { throw new PlatformNotSupportedException (); }
		protected override void Dispose(bool disposing) { throw new PlatformNotSupportedException (); }
		public override System.Threading.Tasks.ValueTask DisposeAsync() { throw new PlatformNotSupportedException (); }
		public virtual void EndAuthenticateAsClient(System.IAsyncResult asyncResult) { throw new PlatformNotSupportedException (); }
		public virtual void EndAuthenticateAsServer(System.IAsyncResult asyncResult) { throw new PlatformNotSupportedException (); }
		public override int EndRead(System.IAsyncResult asyncResult) { throw new PlatformNotSupportedException (); }
		public override void EndWrite(System.IAsyncResult asyncResult) { throw new PlatformNotSupportedException (); }
		public override void Flush() { throw new PlatformNotSupportedException (); }
		public override System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken) { throw new PlatformNotSupportedException (); }
		public override int Read(byte[] buffer, int offset, int count) { throw new PlatformNotSupportedException (); }
		public override long Seek(long offset, System.IO.SeekOrigin origin) { throw new PlatformNotSupportedException (); }
		public override void SetLength(long value) { throw new PlatformNotSupportedException (); }
		public virtual System.Threading.Tasks.Task ShutdownAsync() { throw new PlatformNotSupportedException (); }
		public void Write(byte[] buffer) { throw new PlatformNotSupportedException (); }
		public override void Write(byte[] buffer, int offset, int count) { throw new PlatformNotSupportedException (); }
	}
}
