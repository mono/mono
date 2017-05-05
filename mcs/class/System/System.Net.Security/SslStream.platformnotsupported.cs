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
	 * These two are defined by the referencesource; add them heere to make
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

	public class SslStream : AuthenticatedStream
	{
		const string EXCEPTION_MESSAGE = "System.Net.Security.SslStream is not supported on the current platform.";

		public SslStream (Stream innerStream)
			: this (innerStream, false)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen)
			: base (innerStream, leaveInnerStreamOpen)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}

#if SECURITY_DEP
		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}
#endif

		public virtual void AuthenticateAsClient (string targetHost)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TransportContext TransportContext {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool IsAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsMutuallyAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsEncrypted {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsSigned {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsServer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual SslProtocols SslProtocol {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool CheckCertRevocationStatus {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual X509Certificate LocalCertificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual X509Certificate RemoteCertificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual CipherAlgorithmType CipherAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int CipherStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual HashAlgorithmType HashAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int HashStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int KeyExchangeStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanSeek {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanRead {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanWrite {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int ReadTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int WriteTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long Length {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long Position {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void SetLength (long value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Flush ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void Dispose (bool disposing)
		{
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Write (byte[] buffer)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
