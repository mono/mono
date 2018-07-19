//
// System.Net.HttpWebRequest
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

using System.IO;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace System.Net
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		internal const string EXCEPTION_MESSAGE = "System.Net.HttpWebRequest is not supported on the current platform.";

#if MOBILE
		public
#else
		internal
#endif
		HttpWebRequest (Uri uri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal HttpWebRequest (Uri uri, object /* MonoTlsProvider */ tlsProvider, object /* MonoTlsSettings */ settings = null)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string Accept {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Uri Address {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool AllowAutoRedirect {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool AllowWriteStreamBuffering {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool AllowReadStreamBuffering {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DecompressionMethods AutomaticDecompression {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal bool InternalAllowBuffering {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public X509CertificateCollection ClientCertificates {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Connection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string ConnectionGroupName {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long ContentLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal long InternalContentLength {
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string ContentType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public HttpContinueDelegate ContinueDelegate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual CookieContainer CookieContainer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override ICredentials Credentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DateTime Date {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

#if !MOBILE
		public static new RequestCachePolicy DefaultCachePolicy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
#endif

		public static int DefaultMaximumErrorResponseLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Expect {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool HaveResponse {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override WebHeaderCollection Headers {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Host {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DateTime IfModifiedSince {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool KeepAlive {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaximumAutomaticRedirections {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaximumResponseHeadersLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static int DefaultMaximumResponseHeadersLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ReadWriteTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ContinueTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string MediaType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string Method {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool Pipelined {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool PreAuthenticate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Version ProtocolVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override IWebProxy Proxy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Referer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override Uri RequestUri {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool SendChunked {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ServicePoint ServicePoint {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal ServicePoint ServicePointNoLock {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool SupportsCookieContainer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int Timeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string TransferEncoding {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool UseDefaultCredentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string UserAgent {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UnsafeAuthenticatedConnectionSharing {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal bool ExpectContinue {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal ServerCertValidationCallback ServerCertValidationCallback {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal Uri AuthUri {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void AddRange (int range)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (int from, int to)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (string rangeSpecifier, int range)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (string rangeSpecifier, int from, int to)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (long range)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (long from, long to)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (string rangeSpecifier, long range)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddRange (string rangeSpecifier, long from, long to)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Stream GetRequestStream()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public System.IO.Stream GetRequestStream (out TransportContext context)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Stream EndGetRequestStream (IAsyncResult asyncResult, out TransportContext transportContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override WebResponse GetResponse()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal bool FinishedReading {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal bool Aborted {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void Abort ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void DoContinueDelegate (int statusCode, WebHeaderCollection headers)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void SetWriteStreamError (WebExceptionStatus status, Exception exc)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal byte[] GetRequestHeaders ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
