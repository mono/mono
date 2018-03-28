//
// System.Net.HttpListenerRequest
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

using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace System.Net {
	public sealed class HttpListenerRequest
	{
		const string EXCEPTION_MESSAGE = "System.Net.HttpListenerRequest is not supported on the current platform.";

		HttpListenerRequest ()
		{
		}

		public string [] AcceptTypes {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ClientCertificateError {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Encoding ContentEncoding {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public long ContentLength64 {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string ContentType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public CookieCollection Cookies {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool HasEntityBody {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public NameValueCollection Headers {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string HttpMethod {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Stream InputStream {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsLocal {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsSecureConnection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool KeepAlive {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IPEndPoint LocalEndPoint {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Version ProtocolVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public NameValueCollection QueryString {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string RawUrl {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IPEndPoint RemoteEndPoint {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Guid RequestTraceIdentifier {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Uri Url {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Uri UrlReferrer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string UserAgent {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string UserHostAddress {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string UserHostName {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string [] UserLanguages {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IAsyncResult BeginGetClientCertificate (AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public X509Certificate2 EndGetClientCertificate (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public X509Certificate2 GetClientCertificate ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string ServiceName {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public TransportContext TransportContext {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsWebSocketRequest {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Task<X509Certificate2> GetClientCertificateAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
