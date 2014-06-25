//
// System.Net.HttpWebRequest
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net 
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable {
		Uri requestUri;
		Uri actualUri;
		bool hostChanged;
		bool allowAutoRedirect = true;
		bool allowBuffering = true;
		X509CertificateCollection certificates;
		string connectionGroup;
		bool haveContentLength;
		long contentLength = -1;
		HttpContinueDelegate continueDelegate;
		CookieContainer cookieContainer;
		ICredentials credentials;
		bool haveResponse;		
		bool haveRequest;
		bool requestSent;
		WebHeaderCollection webHeaders;
		bool keepAlive = true;
		int maxAutoRedirect = 50;
		string mediaType = String.Empty;
		string method = "GET";
		string initialMethod = "GET";
		bool pipelined = true;
		bool preAuthenticate;
		bool usedPreAuth;
		Version version = HttpVersion.Version11;
		bool force_version;
		Version actualVersion;
		IWebProxy proxy;
		bool sendChunked;
		ServicePoint servicePoint;
		int timeout = 100000;
		
		WebConnectionStream writeStream;
		HttpWebResponse webResponse;
		WebAsyncResult asyncWrite;
		WebAsyncResult asyncRead;
		EventHandler abortHandler;
		int aborted;
		bool gotRequestStream;
		int redirects;
		bool expectContinue;
		byte[] bodyBuffer;
		int bodyBufferLength;
		bool getResponseCalled;
		Exception saved_exc;
		object locker = new object ();
		bool finished_reading;
		internal WebConnection WebConnection;
		DecompressionMethods auto_decomp;
		int maxResponseHeadersLength;
		static int defaultMaxResponseHeadersLength;
		int readWriteTimeout = 300000; // ms

		enum NtlmAuthState {
			None,
			Challenge,
			Response
		}
		AuthorizationState auth_state, proxy_auth_state;
		string host;

		// Constructors
		static HttpWebRequest ()
		{
			defaultMaxResponseHeadersLength = 64 * 1024;
#if !NET_2_1
			NetConfig config = ConfigurationSettings.GetConfig ("system.net/settings") as NetConfig;
			if (config != null) {
				int x = config.MaxResponseHeadersLength;
				if (x != -1)
					x *= 64;

				defaultMaxResponseHeadersLength = x;
			}
#endif
		}

#if NET_2_1
		public
#else
		internal
#endif
		HttpWebRequest (Uri uri) 
		{
			this.requestUri = uri;
			this.actualUri = uri;
			this.proxy = GlobalProxySelection.Select;
			this.webHeaders = new WebHeaderCollection (WebHeaderCollection.HeaderInfo.Request);
			ThrowOnError = true;
			ResetAuthorization ();
		}
		
		[Obsolete ("Serialization is obsoleted for this type", false)]
		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			SerializationInfo info = serializationInfo;

			requestUri = (Uri) info.GetValue ("requestUri", typeof (Uri));
			actualUri = (Uri) info.GetValue ("actualUri", typeof (Uri));
			allowAutoRedirect = info.GetBoolean ("allowAutoRedirect");
			allowBuffering = info.GetBoolean ("allowBuffering");
			certificates = (X509CertificateCollection) info.GetValue ("certificates", typeof (X509CertificateCollection));
			connectionGroup = info.GetString ("connectionGroup");
			contentLength = info.GetInt64 ("contentLength");
			webHeaders = (WebHeaderCollection) info.GetValue ("webHeaders", typeof (WebHeaderCollection));
			keepAlive = info.GetBoolean ("keepAlive");
			maxAutoRedirect = info.GetInt32 ("maxAutoRedirect");
			mediaType = info.GetString ("mediaType");
			method = info.GetString ("method");
			initialMethod = info.GetString ("initialMethod");
			pipelined = info.GetBoolean ("pipelined");
			version = (Version) info.GetValue ("version", typeof (Version));
			proxy = (IWebProxy) info.GetValue ("proxy", typeof (IWebProxy));
			sendChunked = info.GetBoolean ("sendChunked");
			timeout = info.GetInt32 ("timeout");
			redirects = info.GetInt32 ("redirects");
			host = info.GetString ("host");
			ResetAuthorization ();
		}

		void ResetAuthorization ()
		{
			auth_state = new AuthorizationState (this, false);
			proxy_auth_state = new AuthorizationState (this, true);
		}
		
		// Properties

		public string Accept {
			get { return webHeaders ["Accept"]; }
			set {
				CheckRequestStarted ();
				webHeaders.RemoveAndAdd ("Accept", value);
			}
		}
		
		public Uri Address {
			get { return actualUri; }
			internal set { actualUri = value; } // Used by Ftp+proxy
		}
		
		public bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { this.allowAutoRedirect = value; }
		}
		
		public bool AllowWriteStreamBuffering {
			get { return allowBuffering; }
			set { allowBuffering = value; }
		}
		
#if NET_4_5
		public virtual bool AllowReadStreamBuffering {
			get { return allowBuffering; }
			set { allowBuffering = value; }
		}
#endif

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		public DecompressionMethods AutomaticDecompression
		{
			get {
				return auto_decomp;
			}
			set {
				CheckRequestStarted ();
				auto_decomp = value;
			}
		}
		
		internal bool InternalAllowBuffering {
			get {
				return (allowBuffering && (method != "HEAD" && method != "GET" &&
							method != "MKCOL" && method != "CONNECT" &&
							method != "TRACE"));
			}
		}
		
		public X509CertificateCollection ClientCertificates {
			get {
				if (certificates == null)
					certificates = new X509CertificateCollection ();

				return certificates;
			}
			[MonoTODO]
			set {
				throw GetMustImplement ();
			}
		}
		
		public string Connection {
			get { return webHeaders ["Connection"]; }
			set {
				CheckRequestStarted ();

				if (string.IsNullOrEmpty (value)) {
					webHeaders.RemoveInternal ("Connection");
					return;
				}

				string val = value.ToLowerInvariant ();
				if (val.Contains ("keep-alive") || val.Contains ("close"))
					throw new ArgumentException ("Keep-Alive and Close may not be set with this property");

				if (keepAlive)
					value = value + ", Keep-Alive";
				
				webHeaders.RemoveAndAdd ("Connection", value);
			}
		}		
		
		public override string ConnectionGroupName { 
			get { return connectionGroup; }
			set { connectionGroup = value; }
		}
		
		public override long ContentLength { 
			get { return contentLength; }
			set { 
				CheckRequestStarted ();
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "Content-Length must be >= 0");
					
				contentLength = value;
				haveContentLength = true;
			}
		}
		
		internal long InternalContentLength {
			set { contentLength = value; }
		}
			
		internal bool ThrowOnError { get; set; }
		
		public override string ContentType { 
			get { return webHeaders ["Content-Type"]; }
			set {
				if (value == null || value.Trim().Length == 0) {
					webHeaders.RemoveInternal ("Content-Type");
					return;
				}
				webHeaders.RemoveAndAdd ("Content-Type", value);
			}
		}
		
		public HttpContinueDelegate ContinueDelegate {
			get { return continueDelegate; }
			set { continueDelegate = value; }
		}
		
#if NET_4_5
		virtual
#endif
		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}
		
		public override ICredentials Credentials { 
			get { return credentials; }
			set { credentials = value; }
		}
#if NET_4_0
		public DateTime Date {
			get {
				string date = webHeaders ["Date"];
				if (date == null)
					return DateTime.MinValue;
				return DateTime.ParseExact (date, "r", CultureInfo.InvariantCulture).ToLocalTime ();
			}
			set {
				if (value.Equals (DateTime.MinValue))
					webHeaders.RemoveInternal ("Date");
				else
					webHeaders.RemoveAndAdd ("Date", value.ToUniversalTime ().ToString ("r", CultureInfo.InvariantCulture));
			}
		}
#endif

#if !NET_2_1
		[MonoTODO]
		public static new RequestCachePolicy DefaultCachePolicy
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
#endif
		
		[MonoTODO]
		public static int DefaultMaximumErrorResponseLength
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
		
		public string Expect {
			get { return webHeaders ["Expect"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Expect");
					return;
				}

				if (val == "100-continue")
					throw new ArgumentException ("100-Continue cannot be set with this property.",
								     "value");
				webHeaders.RemoveAndAdd ("Expect", value);
			}
		}
		
#if NET_4_5
		virtual
#endif
		public bool HaveResponse {
			get { return haveResponse; }
		}
		
		public override WebHeaderCollection Headers { 
			get { return webHeaders; }
			set {
				CheckRequestStarted ();
				WebHeaderCollection newHeaders = new WebHeaderCollection (WebHeaderCollection.HeaderInfo.Request);
				int count = value.Count;
				for (int i = 0; i < count; i++) 
					newHeaders.Add (value.GetKey (i), value.Get (i));

				webHeaders = newHeaders;
			}
		}
		
#if NET_4_0
		public
#else
		internal
#endif
		string Host {
			get {
				if (host == null)
					return actualUri.Authority;
				return host;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (!CheckValidHost (actualUri.Scheme, value))
					throw new ArgumentException ("Invalid host: " + value);

				host = value;
			}
		}

		static bool CheckValidHost (string scheme, string val)
		{
			if (val.Length == 0)
				return false;

			if (val [0] == '.')
				return false;

			int idx = val.IndexOf ('/');
			if (idx >= 0)
				return false;

			IPAddress ipaddr;
			if (IPAddress.TryParse (val, out ipaddr))
				return true;

			string u = scheme + "://" + val + "/";
			return Uri.IsWellFormedUriString (u, UriKind.Absolute);
		}

		public DateTime IfModifiedSince {
			get { 
				string str = webHeaders ["If-Modified-Since"];
				if (str == null)
					return DateTime.Now;
				try {
					return MonoHttpDate.Parse (str);
				} catch (Exception) {
					return DateTime.Now;
				}
			}
			set {
				CheckRequestStarted ();
				// rfc-1123 pattern
				webHeaders.SetInternal ("If-Modified-Since", 
					value.ToUniversalTime ().ToString ("r", null));
				// TODO: check last param when using different locale
			}
		}

		public bool KeepAlive {		
			get {
				return keepAlive;
			}
			set {
				keepAlive = value;
			}
		}
		
		public int MaximumAutomaticRedirections {
			get { return maxAutoRedirect; }
			set {
				if (value <= 0)
					throw new ArgumentException ("Must be > 0", "value");

				maxAutoRedirect = value;
			}			
		}

		[MonoTODO ("Use this")]
		public int MaximumResponseHeadersLength {
			get { return maxResponseHeadersLength; }
			set { maxResponseHeadersLength = value; }
		}

		[MonoTODO ("Use this")]
		public static int DefaultMaximumResponseHeadersLength {
			get { return defaultMaxResponseHeadersLength; }
			set { defaultMaxResponseHeadersLength = value; }
		}

		public	int ReadWriteTimeout {
			get { return readWriteTimeout; }
			set {
				if (requestSent)
					throw new InvalidOperationException ("The request has already been sent.");

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "Must be >= -1");

				readWriteTimeout = value;
			}
		}
		
#if NET_4_5
		[MonoTODO]
		public int ContinueTimeout {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif
		
		public string MediaType {
			get { return mediaType; }
			set { 
				mediaType = value;
			}
		}
		
		public override string Method { 
			get { return this.method; }
			set { 
				if (value == null || value.Trim () == "")
					throw new ArgumentException ("not a valid method");

				method = value.ToUpperInvariant ();
				if (method != "HEAD" && method != "GET" && method != "POST" && method != "PUT" &&
					method != "DELETE" && method != "CONNECT" && method != "TRACE" &&
					method != "MKCOL") {
					method = value;
				}
			}
		}
		
		public bool Pipelined {
			get { return pipelined; }
			set { pipelined = value; }
		}		
		
		public override bool PreAuthenticate { 
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}
		
		public Version ProtocolVersion {
			get { return version; }
			set { 
				if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
					throw new ArgumentException ("value");

				force_version = true;
				version = value; 
			}
		}
		
		public override IWebProxy Proxy { 
			get { return proxy; }
			set { 
				CheckRequestStarted ();
				proxy = value;
				servicePoint = null; // we may need a new one
			}
		}
		
		public string Referer {
			get { return webHeaders ["Referer"]; }
			set {
				CheckRequestStarted ();
				if (value == null || value.Trim().Length == 0) {
					webHeaders.RemoveInternal ("Referer");
					return;
				}
				webHeaders.SetInternal ("Referer", value);
			}
		}

		public override Uri RequestUri { 
			get { return requestUri; }
		}
		
		public bool SendChunked {
			get { return sendChunked; }
			set {
				CheckRequestStarted ();
				sendChunked = value;
			}
		}
		
		public ServicePoint ServicePoint {
			get { return GetServicePoint (); }
		}

		internal ServicePoint ServicePointNoLock {
			get { return servicePoint; }
		}
#if NET_4_0
		public virtual bool SupportsCookieContainer { 
			get {
				// The managed implementation supports the cookie container
				// it is only Silverlight that returns false here
				return true;
			}
		}
#endif
		public override int Timeout { 
			get { return timeout; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				timeout = value;
			}
		}
		
		public string TransferEncoding {
			get { return webHeaders ["Transfer-Encoding"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Transfer-Encoding");
					return;
				}

				if (val == "chunked")
					throw new ArgumentException ("Chunked encoding must be set with the SendChunked property");

				if (!sendChunked)
					throw new ArgumentException ("SendChunked must be True", "value");

				webHeaders.RemoveAndAdd ("Transfer-Encoding", value);
			}
		}

		public override bool UseDefaultCredentials
		{
			get { return CredentialCache.DefaultCredentials == Credentials; }
			set { Credentials = value ? CredentialCache.DefaultCredentials : null; }
		}
		
		public string UserAgent {
			get { return webHeaders ["User-Agent"]; }
			set { webHeaders.SetInternal ("User-Agent", value); }
		}

		bool unsafe_auth_blah;
		public bool UnsafeAuthenticatedConnectionSharing
		{
			get { return unsafe_auth_blah; }
			set { unsafe_auth_blah = value; }
		}

		internal bool GotRequestStream {
			get { return gotRequestStream; }
		}

		internal bool ExpectContinue {
			get { return expectContinue; }
			set { expectContinue = value; }
		}
		
		internal Uri AuthUri {
			get { return actualUri; }
		}
		
		internal bool ProxyQuery {
			get { return servicePoint.UsesProxy && !servicePoint.UseConnect; }
		}
		
		// Methods
		
		internal ServicePoint GetServicePoint ()
		{
			lock (locker) {
				if (hostChanged || servicePoint == null) {
					servicePoint = ServicePointManager.FindServicePoint (actualUri, proxy);
					hostChanged = false;
				}
			}

			return servicePoint;
		}
		
		public void AddRange (int range)
		{
			AddRange ("bytes", (long) range);
		}
		
		public void AddRange (int from, int to)
		{
			AddRange ("bytes", (long) from, (long) to);
		}
		
		public void AddRange (string rangeSpecifier, int range)
		{
			AddRange (rangeSpecifier, (long) range);
		}
		
		public void AddRange (string rangeSpecifier, int from, int to)
		{
			AddRange (rangeSpecifier, (long) from, (long) to);
		}
#if NET_4_0
		public
#else
		internal
#endif
		void AddRange (long range)
		{
			AddRange ("bytes", (long) range);
		}

#if NET_4_0
		public
#else
		internal
#endif
		void AddRange (long from, long to)
		{
			AddRange ("bytes", from, to);
		}

#if NET_4_0
		public
#else
		internal
#endif
		void AddRange (string rangeSpecifier, long range)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (!WebHeaderCollection.IsHeaderValue (rangeSpecifier))
				throw new ArgumentException ("Invalid range specifier", "rangeSpecifier");

			string r = webHeaders ["Range"];
			if (r == null)
				r = rangeSpecifier + "=";
			else {
				string old_specifier = r.Substring (0, r.IndexOf ('='));
				if (String.Compare (old_specifier, rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
					throw new InvalidOperationException ("A different range specifier is already in use");
				r += ",";
			}

			string n = range.ToString (CultureInfo.InvariantCulture);
			if (range < 0)
				r = r + "0" + n;
			else
				r = r + n + "-";
			webHeaders.RemoveAndAdd ("Range", r);
		}

#if NET_4_0
		public
#else
		internal
#endif
		void AddRange (string rangeSpecifier, long from, long to)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (!WebHeaderCollection.IsHeaderValue (rangeSpecifier))
				throw new ArgumentException ("Invalid range specifier", "rangeSpecifier");
			if (from > to || from < 0)
				throw new ArgumentOutOfRangeException ("from");
			if (to < 0)
				throw new ArgumentOutOfRangeException ("to");

			string r = webHeaders ["Range"];
			if (r == null)
				r = rangeSpecifier + "=";
			else
				r += ",";

			r = String.Format ("{0}{1}-{2}", r, from, to);
			webHeaders.RemoveAndAdd ("Range", r);
		}

		
		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (Aborted)
				throw new WebException ("The request was canceled.", WebExceptionStatus.RequestCanceled);

			bool send = !(method == "GET" || method == "CONNECT" || method == "HEAD" ||
					method == "TRACE");
			if (method == null || !send)
				throw new ProtocolViolationException ("Cannot send data when method is: " + method);

			if (contentLength == -1 && !sendChunked && !allowBuffering && KeepAlive)
				throw new ProtocolViolationException ("Content-Length not set");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new ProtocolViolationException ("SendChunked should be true.");

			lock (locker)
			{
				if (getResponseCalled)
					throw new InvalidOperationException ("The operation cannot be performed once the request has been submitted.");

				if (asyncWrite != null) {
					throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
								"method while a previous call is still in progress.");
				}
	
				asyncWrite = new WebAsyncResult (this, callback, state);
				initialMethod = method;
				if (haveRequest) {
					if (writeStream != null) {
						asyncWrite.SetCompleted (true, writeStream);
						asyncWrite.DoCallback ();
						return asyncWrite;
					}
				}
				
				gotRequestStream = true;
				WebAsyncResult result = asyncWrite;
				if (!requestSent) {
					requestSent = true;
					redirects = 0;
					servicePoint = GetServicePoint ();
					abortHandler = servicePoint.SendRequest (this, connectionGroup);
				}
				return result;
			}
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			asyncWrite = result;
			result.WaitUntilComplete ();

			Exception e = result.Exception;
			if (e != null)
				throw e;

			return result.WriteStream;
		}
		
		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult = asyncWrite;
			if (asyncResult == null) {
				asyncResult = BeginGetRequestStream (null, null);
				asyncWrite = (WebAsyncResult) asyncResult;
			}

			if (!asyncResult.IsCompleted && !asyncResult.AsyncWaitHandle.WaitOne (timeout, false)) {
				Abort ();
				throw new WebException ("The request timed out", WebExceptionStatus.Timeout);
			}

			return EndGetRequestStream (asyncResult);
		}

		bool CheckIfForceWrite (SimpleAsyncResult result)
		{
			if (writeStream == null || writeStream.RequestWritten || !InternalAllowBuffering)
				return false;
			#if NET_4_0
			if (contentLength < 0 && writeStream.CanWrite == true && writeStream.WriteBufferLength < 0)
				return false;

			if (contentLength < 0 && writeStream.WriteBufferLength >= 0)
				InternalContentLength = writeStream.WriteBufferLength;
			#else
			if (contentLength < 0 && writeStream.CanWrite == true)
				return false;
			#endif

			// This will write the POST/PUT if the write stream already has the expected
			// amount of bytes in it (ContentLength) (bug #77753) or if the write stream
			// contains data and it has been closed already (xamarin bug #1512).

			if (writeStream.WriteBufferLength == contentLength || (contentLength == -1 && writeStream.CanWrite == false))
				return writeStream.WriteRequestAsync (result);

			return false;
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			if (Aborted)
				throw new WebException ("The request was canceled.", WebExceptionStatus.RequestCanceled);

			if (method == null)
				throw new ProtocolViolationException ("Method is null.");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new ProtocolViolationException ("SendChunked should be true.");

			Monitor.Enter (locker);
			getResponseCalled = true;
			if (asyncRead != null && !haveResponse) {
				Monitor.Exit (locker);
				throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
							"method while a previous call is still in progress.");
			}

			asyncRead = new WebAsyncResult (this, callback, state);
			WebAsyncResult aread = asyncRead;
			initialMethod = method;

			SimpleAsyncResult.RunWithLock (locker, CheckIfForceWrite, inner => {
				var synch = inner.CompletedSynchronously;

				if (inner.GotException) {
					aread.SetCompleted (synch, inner.Exception);
					aread.DoCallback ();
					return;
				}

				if (haveResponse) {
					Exception saved = saved_exc;
					if (webResponse != null) {
						if (saved == null) {
							aread.SetCompleted (synch, webResponse);
						} else {
							aread.SetCompleted (synch, saved);
						}
						aread.DoCallback ();
						return;
					} else if (saved != null) {
						aread.SetCompleted (synch, saved);
						aread.DoCallback ();
						return;
					}
				}

				if (!requestSent) {
					requestSent = true;
					redirects = 0;
					servicePoint = GetServicePoint ();
					abortHandler = servicePoint.SendRequest (this, connectionGroup);
				}
			});

			return aread;
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (!result.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}

			if (result.GotException)
				throw result.Exception;

			return result.Response;
		}
		
#if NET_3_5
		public Stream EndGetRequestStream (IAsyncResult asyncResult, out TransportContext transportContext)
		{
			transportContext = null;
			return EndGetRequestStream (asyncResult);
		}
#endif

		public override WebResponse GetResponse()
		{
			WebAsyncResult result = (WebAsyncResult) BeginGetResponse (null, null);
			return EndGetResponse (result);
		}
		
		internal bool FinishedReading {
			get { return finished_reading; }
			set { finished_reading = value; }
		}

		internal bool Aborted {
			get { return Interlocked.CompareExchange (ref aborted, 0, 0) == 1; }
		}

		public override void Abort ()
		{
			if (Interlocked.CompareExchange (ref aborted, 1, 0) == 1)
				return;

			if (haveResponse && finished_reading)
				return;

			haveResponse = true;
			if (abortHandler != null) {
				try {
					abortHandler (this, EventArgs.Empty);
				} catch (Exception) {}
				abortHandler = null;
			}

			if (asyncWrite != null) {
				WebAsyncResult r = asyncWrite;
				if (!r.IsCompleted) {
					try {
						WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled); 
						r.SetCompleted (false, wexc);
						r.DoCallback ();
					} catch {}
				}
				asyncWrite = null;
			}			

			if (asyncRead != null) {
				WebAsyncResult r = asyncRead;
				if (!r.IsCompleted) {
					try {
						WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled); 
						r.SetCompleted (false, wexc);
						r.DoCallback ();
					} catch {}
				}
				asyncRead = null;
			}			

			if (writeStream != null) {
				try {
					writeStream.Close ();
					writeStream = null;
				} catch {}
			}

			if (webResponse != null) {
				try {
					webResponse.Close ();
					webResponse = null;
				} catch {}
			}
		}		
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			GetObjectData (serializationInfo, streamingContext);
		}

		protected override void GetObjectData (SerializationInfo serializationInfo,
			StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			info.AddValue ("requestUri", requestUri, typeof (Uri));
			info.AddValue ("actualUri", actualUri, typeof (Uri));
			info.AddValue ("allowAutoRedirect", allowAutoRedirect);
			info.AddValue ("allowBuffering", allowBuffering);
			info.AddValue ("certificates", certificates, typeof (X509CertificateCollection));
			info.AddValue ("connectionGroup", connectionGroup);
			info.AddValue ("contentLength", contentLength);
			info.AddValue ("webHeaders", webHeaders, typeof (WebHeaderCollection));
			info.AddValue ("keepAlive", keepAlive);
			info.AddValue ("maxAutoRedirect", maxAutoRedirect);
			info.AddValue ("mediaType", mediaType);
			info.AddValue ("method", method);
			info.AddValue ("initialMethod", initialMethod);
			info.AddValue ("pipelined", pipelined);
			info.AddValue ("version", version, typeof (Version));
			info.AddValue ("proxy", proxy, typeof (IWebProxy));
			info.AddValue ("sendChunked", sendChunked);
			info.AddValue ("timeout", timeout);
			info.AddValue ("redirects", redirects);
			info.AddValue ("host", host);
		}
		
		void CheckRequestStarted () 
		{
			if (requestSent)
				throw new InvalidOperationException ("request started");
		}

		internal void DoContinueDelegate (int statusCode, WebHeaderCollection headers)
		{
			if (continueDelegate != null)
				continueDelegate (statusCode, headers);
		}

		void RewriteRedirectToGet ()
		{
			method = "GET";
			webHeaders.RemoveInternal ("Transfer-Encoding");
			sendChunked = false;
		}
		
		bool Redirect (WebAsyncResult result, HttpStatusCode code, WebResponse response)
		{
			redirects++;
			Exception e = null;
			string uriString = null;
			switch (code) {
			case HttpStatusCode.Ambiguous: // 300
				e = new WebException ("Ambiguous redirect.");
				break;
			case HttpStatusCode.MovedPermanently: // 301
			case HttpStatusCode.Redirect: // 302
				if (method == "POST")
					RewriteRedirectToGet ();
				break;
			case HttpStatusCode.TemporaryRedirect: // 307
				break;
			case HttpStatusCode.SeeOther: //303
				RewriteRedirectToGet ();
				break;
			case HttpStatusCode.NotModified: // 304
				return false;
			case HttpStatusCode.UseProxy: // 305
				e = new NotImplementedException ("Proxy support not available.");
				break;
			case HttpStatusCode.Unused: // 306
			default:
				e = new ProtocolViolationException ("Invalid status code: " + (int) code);
				break;
			}

			if (method != "GET" && !InternalAllowBuffering)
				e = new WebException ("The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, webResponse);

			if (e != null)
				throw e;

			contentLength = -1;
			uriString = webResponse.Headers ["Location"];

			if (uriString == null)
				throw new WebException ("No Location header found for " + (int) code,
							WebExceptionStatus.ProtocolError);

			Uri prev = actualUri;
			try {
				actualUri = new Uri (actualUri, uriString);
			} catch (Exception) {
				throw new WebException (String.Format ("Invalid URL ({0}) for {1}",
									uriString, (int) code),
									WebExceptionStatus.ProtocolError);
			}

			hostChanged = (actualUri.Scheme != prev.Scheme || Host != prev.Authority);
			return true;
		}

		string GetHeaders ()
		{
			bool continue100 = false;
			if (sendChunked) {
				continue100 = true;
				webHeaders.RemoveAndAdd ("Transfer-Encoding", "chunked");
				webHeaders.RemoveInternal ("Content-Length");
			} else if (contentLength != -1) {
				if (auth_state.NtlmAuthState == NtlmAuthState.Challenge || proxy_auth_state.NtlmAuthState == NtlmAuthState.Challenge) {
					// We don't send any body with the NTLM Challenge request.
					if (haveContentLength || gotRequestStream || contentLength > 0)
						webHeaders.SetInternal ("Content-Length", "0");
					else
						webHeaders.RemoveInternal ("Content-Length");
				} else {
					if (contentLength > 0)
						continue100 = true;

					if (haveContentLength || gotRequestStream || contentLength > 0)
						webHeaders.SetInternal ("Content-Length", contentLength.ToString ());
				}
				webHeaders.RemoveInternal ("Transfer-Encoding");
			} else {
				webHeaders.RemoveInternal ("Content-Length");
			}

			if (actualVersion == HttpVersion.Version11 && continue100 &&
			    servicePoint.SendContinue) { // RFC2616 8.2.3
				webHeaders.RemoveAndAdd ("Expect" , "100-continue");
				expectContinue = true;
			} else {
				webHeaders.RemoveInternal ("Expect");
				expectContinue = false;
			}

			bool proxy_query = ProxyQuery;
			string connectionHeader = (proxy_query) ? "Proxy-Connection" : "Connection";
			webHeaders.RemoveInternal ((!proxy_query) ? "Proxy-Connection" : "Connection");
			Version proto_version = servicePoint.ProtocolVersion;
			bool spoint10 = (proto_version == null || proto_version == HttpVersion.Version10);

			if (keepAlive && (version == HttpVersion.Version10 || spoint10)) {
				if (webHeaders[connectionHeader] == null
				    || webHeaders[connectionHeader].IndexOf ("keep-alive", StringComparison.OrdinalIgnoreCase) == -1)
					webHeaders.RemoveAndAdd (connectionHeader, "keep-alive");
			} else if (!keepAlive && version == HttpVersion.Version11) {
				webHeaders.RemoveAndAdd (connectionHeader, "close");
			}

			webHeaders.SetInternal ("Host", Host);
			if (cookieContainer != null) {
				string cookieHeader = cookieContainer.GetCookieHeader (actualUri);
				if (cookieHeader != "")
					webHeaders.RemoveAndAdd ("Cookie", cookieHeader);
				else
					webHeaders.RemoveInternal ("Cookie");
			}

			string accept_encoding = null;
			if ((auto_decomp & DecompressionMethods.GZip) != 0)
				accept_encoding = "gzip";
			if ((auto_decomp & DecompressionMethods.Deflate) != 0)
				accept_encoding = accept_encoding != null ? "gzip, deflate" : "deflate";
			if (accept_encoding != null)
				webHeaders.RemoveAndAdd ("Accept-Encoding", accept_encoding);

			if (!usedPreAuth && preAuthenticate)
				DoPreAuthenticate ();

			return webHeaders.ToString ();
		}

		void DoPreAuthenticate ()
		{
			bool isProxy = (proxy != null && !proxy.IsBypassed (actualUri));
			ICredentials creds = (!isProxy || credentials != null) ? credentials : proxy.Credentials;
			Authorization auth = AuthenticationManager.PreAuthenticate (this, creds);
			if (auth == null)
				return;

			webHeaders.RemoveInternal ("Proxy-Authorization");
			webHeaders.RemoveInternal ("Authorization");
			string authHeader = (isProxy && credentials == null) ? "Proxy-Authorization" : "Authorization";
			webHeaders [authHeader] = auth.Message;
			usedPreAuth = true;
		}
		
		internal void SetWriteStreamError (WebExceptionStatus status, Exception exc)
		{
			if (Aborted)
				return;

			WebAsyncResult r = asyncWrite;
			if (r == null)
				r = asyncRead;

			if (r != null) {
				string msg;
				WebException wex;
				if (exc == null) {
					msg = "Error: " + status;
					wex = new WebException (msg, status);
				} else {
					msg = String.Format ("Error: {0} ({1})", status, exc.Message);
					wex = new WebException (msg, exc, status);
				}
				r.SetCompleted (false, wex);
				r.DoCallback ();
			}
		}

		internal byte[] GetRequestHeaders ()
		{
			StringBuilder req = new StringBuilder ();
			string query;
			if (!ProxyQuery) {
				query = actualUri.PathAndQuery;
			} else {
				query = String.Format ("{0}://{1}{2}",  actualUri.Scheme,
									Host,
									actualUri.PathAndQuery);
			}
			
			if (!force_version && servicePoint.ProtocolVersion != null && servicePoint.ProtocolVersion < version) {
				actualVersion = servicePoint.ProtocolVersion;
			} else {
				actualVersion = version;
			}

			req.AppendFormat ("{0} {1} HTTP/{2}.{3}\r\n", method, query,
								actualVersion.Major, actualVersion.Minor);
			req.Append (GetHeaders ());
			string reqstr = req.ToString ();
			return Encoding.UTF8.GetBytes (reqstr);
		}

		internal void SetWriteStream (WebConnectionStream stream)
		{
			if (Aborted)
				return;
			
			writeStream = stream;
			if (bodyBuffer != null) {
				webHeaders.RemoveInternal ("Transfer-Encoding");
				contentLength = bodyBufferLength;
				writeStream.SendChunked = false;
			}

			writeStream.SetHeadersAsync (false, result => {
				if (result.GotException) {
					SetWriteStreamError (result.Exception);
					return;
				}

				haveRequest = true;

				SetWriteStreamInner (inner => {
					if (inner.GotException) {
						SetWriteStreamError (inner.Exception);
						return;
					}

					if (asyncWrite != null) {
						asyncWrite.SetCompleted (inner.CompletedSynchronously, writeStream);
						asyncWrite.DoCallback ();
						asyncWrite = null;
					}
				});
			});
		}

		void SetWriteStreamInner (SimpleAsyncCallback callback)
		{
			SimpleAsyncResult.Run (result => {
				if (bodyBuffer != null) {
					// The body has been written and buffered. The request "user"
					// won't write it again, so we must do it.
					if (auth_state.NtlmAuthState != NtlmAuthState.Challenge && proxy_auth_state.NtlmAuthState != NtlmAuthState.Challenge) {
						// FIXME: this is a blocking call on the thread pool that could lead to thread pool exhaustion
						writeStream.Write (bodyBuffer, 0, bodyBufferLength);
						bodyBuffer = null;
						writeStream.Close ();
					}
				} else if (method != "HEAD" && method != "GET" && method != "MKCOL" && method != "CONNECT" &&
				          method != "TRACE") {
					if (getResponseCalled && !writeStream.RequestWritten)
						return writeStream.WriteRequestAsync (result);
				}

				return false;
			}, callback);
		}

		void SetWriteStreamError (Exception exc)
		{
			WebException wexc = exc as WebException;
			if (wexc != null)
				SetWriteStreamError (wexc.Status, wexc);
			else
				SetWriteStreamError (WebExceptionStatus.SendFailure, exc);
		}

		internal void SetResponseError (WebExceptionStatus status, Exception e, string where)
		{
			if (Aborted)
				return;
			lock (locker) {
			string msg = String.Format ("Error getting response stream ({0}): {1}", where, status);
			WebAsyncResult r = asyncRead;
			if (r == null)
				r = asyncWrite;

			WebException wexc;
			if (e is WebException) {
				wexc = (WebException) e;
			} else {
				wexc = new WebException (msg, e, status, null); 
			}
			if (r != null) {
				if (!r.IsCompleted) {
					r.SetCompleted (false, wexc);
					r.DoCallback ();
				} else if (r == asyncWrite) {
					saved_exc = wexc;
				}
				haveResponse = true;
				asyncRead = null;
				asyncWrite = null;
			} else {
				haveResponse = true;
				saved_exc = wexc;
			}
			}
		}

		void CheckSendError (WebConnectionData data)
		{
			// Got here, but no one called GetResponse
			int status = data.StatusCode;
			if (status < 400 || status == 401 || status == 407)
				return;

			if (writeStream != null && asyncRead == null && !writeStream.CompleteRequestWritten) {
				// The request has not been completely sent and we got here!
				// We should probably just close and cause an error in any case,
				saved_exc = new WebException (data.StatusDescription, null, WebExceptionStatus.ProtocolError, webResponse); 
				if (allowBuffering || sendChunked || writeStream.totalWritten >= contentLength) {
					webResponse.ReadAll ();
				} else {
					writeStream.IgnoreIOErrors = true;
				}
			}
		}

		bool HandleNtlmAuth (WebAsyncResult r)
		{
			bool isProxy = webResponse.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
			if ((isProxy ? proxy_auth_state.NtlmAuthState : auth_state.NtlmAuthState) == NtlmAuthState.None)
				return false;

			WebConnectionStream wce = webResponse.GetResponseStream () as WebConnectionStream;
			if (wce != null) {
				WebConnection cnc = wce.Connection;
				cnc.PriorityRequest = this;
				ICredentials creds = !isProxy ? credentials : proxy.Credentials;
				if (creds != null) {
					cnc.NtlmCredential = creds.GetCredential (requestUri, "NTLM");
					cnc.UnsafeAuthenticatedConnectionSharing = unsafe_auth_blah;
				}
			}
			r.Reset ();
			finished_reading = false;
			haveResponse = false;
			webResponse.ReadAll ();
			webResponse = null;
			return true;
		}

		internal void SetResponseData (WebConnectionData data)
		{
			lock (locker) {
			if (Aborted) {
				if (data.stream != null)
					data.stream.Close ();
				return;
			}

			WebException wexc = null;
			try {
				webResponse = new HttpWebResponse (actualUri, method, data, cookieContainer);
			} catch (Exception e) {
				wexc = new WebException (e.Message, e, WebExceptionStatus.ProtocolError, null); 
				if (data.stream != null)
					data.stream.Close ();
			}

			if (wexc == null && (method == "POST" || method == "PUT")) {
				CheckSendError (data);
				if (saved_exc != null)
					wexc = (WebException) saved_exc;
			}

			WebAsyncResult r = asyncRead;

			bool forced = false;
			if (r == null && webResponse != null) {
				// This is a forced completion (302, 204)...
				forced = true;
				r = new WebAsyncResult (null, null);
				r.SetCompleted (false, webResponse);
			}

			if (r != null) {
				if (wexc != null) {
					haveResponse = true;
					if (!r.IsCompleted)
						r.SetCompleted (false, wexc);
					r.DoCallback ();
					return;
				}

				bool isProxy = ProxyQuery && !proxy.IsBypassed (actualUri);

				bool redirected;
				try {
					redirected = CheckFinalStatus (r);
					if (!redirected) {
						if ((isProxy ? proxy_auth_state.IsNtlmAuthenticated : auth_state.IsNtlmAuthenticated) &&
								webResponse != null && (int)webResponse.StatusCode < 400) {
							WebConnectionStream wce = webResponse.GetResponseStream () as WebConnectionStream;
							if (wce != null) {
								WebConnection cnc = wce.Connection;
								cnc.NtlmAuthenticated = true;
							}
						}

						// clear internal buffer so that it does not
						// hold possible big buffer (bug #397627)
						if (writeStream != null)
							writeStream.KillBuffer ();

						haveResponse = true;
						r.SetCompleted (false, webResponse);
						r.DoCallback ();
					} else {
						if (sendChunked) {
							sendChunked = false;
							webHeaders.RemoveInternal ("Transfer-Encoding");
						}

						if (webResponse != null) {
							if (HandleNtlmAuth (r))
								return;
							webResponse.Close ();
						}
						finished_reading = false;
						haveResponse = false;
						webResponse = null;
						r.Reset ();
						servicePoint = GetServicePoint ();
						abortHandler = servicePoint.SendRequest (this, connectionGroup);
					}
				} catch (WebException wexc2) {
					if (forced) {
						saved_exc = wexc2;
						haveResponse = true;
					}
					r.SetCompleted (false, wexc2);
					r.DoCallback ();
					return;
				} catch (Exception ex) {
					wexc = new WebException (ex.Message, ex, WebExceptionStatus.ProtocolError, null); 
					if (forced) {
						saved_exc = wexc;
						haveResponse = true;
					}
					r.SetCompleted (false, wexc);
					r.DoCallback ();
					return;
				}
			}
			}
		}

		struct AuthorizationState
		{
			readonly HttpWebRequest request;
			readonly bool isProxy;
			bool isCompleted;
			NtlmAuthState ntlm_auth_state;

			public bool IsCompleted {
				get { return isCompleted; }
			}

			public NtlmAuthState NtlmAuthState {
				get { return ntlm_auth_state; }
			}

			public bool IsNtlmAuthenticated {
				get { return isCompleted && ntlm_auth_state != NtlmAuthState.None; }
			}

			public AuthorizationState (HttpWebRequest request, bool isProxy)
			{
				this.request = request;
				this.isProxy = isProxy;
				isCompleted = false;
				ntlm_auth_state = NtlmAuthState.None;
			}

			public bool CheckAuthorization (WebResponse response, HttpStatusCode code)
			{
				isCompleted = false;
				if (code == HttpStatusCode.Unauthorized && request.credentials == null)
					return false;

				// FIXME: This should never happen!
				if (isProxy != (code == HttpStatusCode.ProxyAuthenticationRequired))
					return false;

				if (isProxy && (request.proxy == null || request.proxy.Credentials == null))
					return false;

				string [] authHeaders = response.Headers.GetValues_internal (isProxy ? "Proxy-Authenticate" : "WWW-Authenticate", false);
				if (authHeaders == null || authHeaders.Length == 0)
					return false;

				ICredentials creds = (!isProxy) ? request.credentials : request.proxy.Credentials;
				Authorization auth = null;
				foreach (string authHeader in authHeaders) {
					auth = AuthenticationManager.Authenticate (authHeader, request, creds);
					if (auth != null)
						break;
				}
				if (auth == null)
					return false;
				request.webHeaders [isProxy ? "Proxy-Authorization" : "Authorization"] = auth.Message;
				isCompleted = auth.Complete;
				bool is_ntlm = (auth.Module.AuthenticationType == "NTLM");
				if (is_ntlm)
					ntlm_auth_state = (NtlmAuthState)((int) ntlm_auth_state + 1);
				return true;
			}

			public void Reset ()
			{
				isCompleted = false;
				ntlm_auth_state = NtlmAuthState.None;
				request.webHeaders.RemoveInternal (isProxy ? "Proxy-Authorization" : "Authorization");
			}

			public override string ToString ()
			{
				return string.Format ("{0}AuthState [{1}:{2}]", isProxy ? "Proxy" : "", isCompleted, ntlm_auth_state);
			}
		}

		bool CheckAuthorization (WebResponse response, HttpStatusCode code)
		{
			bool isProxy = code == HttpStatusCode.ProxyAuthenticationRequired;
			return isProxy ? proxy_auth_state.CheckAuthorization (response, code) : auth_state.CheckAuthorization (response, code);
		}

		// Returns true if redirected
		bool CheckFinalStatus (WebAsyncResult result)
		{
			if (result.GotException) {
				bodyBuffer = null;
				throw result.Exception;
			}

			Exception throwMe = result.Exception;

			HttpWebResponse resp = result.Response;
			WebExceptionStatus protoError = WebExceptionStatus.ProtocolError;
			HttpStatusCode code = 0;
			if (throwMe == null && webResponse != null) {
				code = webResponse.StatusCode;
				if ((!auth_state.IsCompleted && code == HttpStatusCode.Unauthorized && credentials != null) ||
					(ProxyQuery && !proxy_auth_state.IsCompleted && code == HttpStatusCode.ProxyAuthenticationRequired)) {
					if (!usedPreAuth && CheckAuthorization (webResponse, code)) {
						// Keep the written body, so it can be rewritten in the retry
						if (InternalAllowBuffering) {
							if (writeStream.WriteBufferLength > 0) {
								bodyBuffer = writeStream.WriteBuffer;
								bodyBufferLength = writeStream.WriteBufferLength;
							}
							return true;
						} else if (method != "PUT" && method != "POST") {
							bodyBuffer = null;
							return true;
						}

						if (!ThrowOnError)
							return false;
							
						writeStream.InternalClose ();
						writeStream = null;
						webResponse.Close ();
						webResponse = null;
						bodyBuffer = null;
							
						throw new WebException ("This request requires buffering " +
									"of data for authentication or " +
									"redirection to be sucessful.");
					}
				}

				bodyBuffer = null;
				if ((int) code >= 400) {
					string err = String.Format ("The remote server returned an error: ({0}) {1}.",
								    (int) code, webResponse.StatusDescription);
					throwMe = new WebException (err, null, protoError, webResponse);
					webResponse.ReadAll ();
				} else if ((int) code == 304 && allowAutoRedirect) {
					string err = String.Format ("The remote server returned an error: ({0}) {1}.",
								    (int) code, webResponse.StatusDescription);
					throwMe = new WebException (err, null, protoError, webResponse);
				} else if ((int) code >= 300 && allowAutoRedirect && redirects >= maxAutoRedirect) {
					throwMe = new WebException ("Max. redirections exceeded.", null,
								    protoError, webResponse);
					webResponse.ReadAll ();
				}
			}

			bodyBuffer = null;
			if (throwMe == null) {
				bool b = false;
				int c = (int) code;
				if (allowAutoRedirect && c >= 300) {
					b = Redirect (result, code, webResponse);
					if (InternalAllowBuffering && writeStream.WriteBufferLength > 0) {
						bodyBuffer = writeStream.WriteBuffer;
						bodyBufferLength = writeStream.WriteBufferLength;
					}
					if (b && !unsafe_auth_blah) {
						auth_state.Reset ();
						proxy_auth_state.Reset ();
					}
				}

				if (resp != null && c >= 300 && c != 304)
					resp.ReadAll ();

				return b;
			}
				
			if (!ThrowOnError)
				return false;

			if (writeStream != null) {
				writeStream.InternalClose ();
				writeStream = null;
			}

			webResponse = null;

			throw throwMe;
		}

		internal bool ReuseConnection {
			get;
			set;
		}

		internal WebConnection StoredConnection;
	}
}

