//
// System.Net.HttpWebRequest
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net 
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		Uri requestUri;
		Uri actualUri;
		bool allowAutoRedirect = true;
		bool allowBuffering = true;
		X509CertificateCollection certificates;
		string connectionGroup;
		long contentLength = -1;
		HttpContinueDelegate continueDelegate;
		CookieContainer cookieContainer;
		ICredentials credentials;
		bool haveResponse;		
		bool haveRequest;
		bool requestSent;
		WebHeaderCollection webHeaders = new WebHeaderCollection (true);
		bool keepAlive = true;
		int maxAutoRedirect = 50;
		string mediaType = String.Empty;
		string method = "GET";
		string initialMethod = "GET";
		bool pipelined = true;
		bool preAuthenticate;
		Version version = HttpVersion.Version11;
		IWebProxy proxy;
		bool sendChunked;
		ServicePoint servicePoint;
		int timeout = 100000;
		
		WebConnectionStream writeStream;
		HttpWebResponse webResponse;
		AutoResetEvent requestEndEvent;
		WebAsyncResult asyncWrite;
		WebAsyncResult asyncRead;
		EventHandler abortHandler;
		bool aborted;
		bool gotRequestStream;
		int redirects;
		bool expectContinue;
		
		// Constructors
		
		internal HttpWebRequest (Uri uri) 
		{
			this.requestUri = uri;
			this.actualUri = uri;
			this.proxy = GlobalProxySelection.Select;
		}		
		
		[MonoTODO]
		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		public string Accept {
			get { return webHeaders ["Accept"]; }
			set {
				CheckRequestStarted ();
				webHeaders.SetInternal ("Accept", value);
			}
		}
		
		public Uri Address {
			get { return actualUri; }
		}
		
		public bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { this.allowAutoRedirect = value; }
		}
		
		public bool AllowWriteStreamBuffering {
			get { return allowBuffering; }
			set { allowBuffering = value; }
		}
		
		internal bool InternalAllowBuffering {
			get {
				return (allowBuffering && (method == "PUT" || method == "POST"));
			}
		}
		
		public X509CertificateCollection ClientCertificates {
			get {
				if (certificates == null)
					certificates = new X509CertificateCollection ();

				return certificates;
			}
		}
		
		public string Connection {
			get { return webHeaders ["Connection"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null) 
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Connection");
					return;
				}

				if (val == "keep-alive" || val == "close") 
					throw new ArgumentException ("Keep-Alive and Close may not be set with this property");

				if (keepAlive && val.IndexOf ("keep-alive") == -1)
					value = value + ", Keep-Alive";
				
				webHeaders.SetInternal ("Connection", value);
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
			}
		}
		
		internal long InternalContentLength {
			set { contentLength = value; }
		}
		
		public override string ContentType { 
			get { return webHeaders ["Content-Type"]; }
			set {
				CheckRequestStarted ();
				if (value == null || value.Trim().Length == 0) {
					webHeaders.RemoveInternal ("Content-Type");
					return;
				}
				webHeaders.SetInternal ("Content-Type", value);
			}
		}
		
		public HttpContinueDelegate ContinueDelegate {
			get { return continueDelegate; }
			set { continueDelegate = value; }
		}
		
		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}
		
		public override ICredentials Credentials { 
			get { return credentials; }
			set { credentials = value; }
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
				webHeaders.SetInternal ("Expect", value);
			}
		}
		
		public bool HaveResponse {
			get { return haveResponse; }
		}
		
		public override WebHeaderCollection Headers { 
			get { return webHeaders; }
			set {
				CheckRequestStarted ();
				WebHeaderCollection newHeaders = new WebHeaderCollection (true);
				int count = value.Count;
				for (int i = 0; i < count; i++) 
					newHeaders.Add (value.GetKey (i), value.Get (i));

				webHeaders = newHeaders;
			}
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

				method = value;
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

				version = value; 
			}
		}
		
		public override IWebProxy Proxy { 
			get { return proxy; }
			set { 
				CheckRequestStarted ();
				if (value == null)
					throw new ArgumentNullException ("value");
				proxy = value;
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

				webHeaders.SetInternal ("Transfer-Encoding", value);
			}
		}
		
		public string UserAgent {
			get { return webHeaders ["User-Agent"]; }
			set { webHeaders.SetInternal ("User-Agent", value); }
		}

		internal bool GotRequestStream {
			get { return gotRequestStream; }
		}

		internal bool ExpectContinue {
			get { return expectContinue; }
			set { expectContinue = value; }
		}
		
		// Methods
		
		internal ServicePoint GetServicePoint ()
		{
			if (servicePoint != null)
				return servicePoint;

			lock (this) {
				if (servicePoint == null)
					servicePoint = ServicePointManager.FindServicePoint (actualUri, proxy);
			}

			return servicePoint;
		}
		
		public void AddRange (int range)
		{
			AddRange ("bytes", range);
		}
		
		public void AddRange (int from, int to)
		{
			AddRange ("bytes", from, to);
		}
		
		public void AddRange (string rangeSpecifier, int range)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			string value = webHeaders ["Range"];
			if (value == null || value.Length == 0) 
				value = rangeSpecifier + "=";
			else if (value.ToLower ().StartsWith (rangeSpecifier.ToLower () + "="))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			webHeaders.SetInternal ("Range", value + range + "-");	
		}
		
		public void AddRange (string rangeSpecifier, int from, int to)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (from < 0 || to < 0 || from > to)
				throw new ArgumentOutOfRangeException ();			
			string value = webHeaders ["Range"];
			if (value == null || value.Length == 0) 
				value = rangeSpecifier + "=";
			else if (value.ToLower ().StartsWith (rangeSpecifier.ToLower () + "="))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			webHeaders.SetInternal ("Range", value + from + "-" + to);	
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		void CommonChecks (bool putpost)
		{
			if (method == null)
				throw new ProtocolViolationException ("Method is null.");

			if (putpost && ((!keepAlive || (contentLength == -1 && !sendChunked)) && !allowBuffering))
				throw new ProtocolViolationException ("Content-Length not set");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new ProtocolViolationException ("SendChunked should be true.");
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (aborted)
				throw new WebException ("The request was previosly aborted.");

			bool send = (method == "PUT" || method == "POST");
			if (method == null || !send)
				throw new ProtocolViolationException ("Cannot send data when method is: " + method);

			CommonChecks (send);
			Monitor.Enter (this);
			if (asyncWrite != null) {
				Monitor.Exit (this);
				throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
							"method while a previous call is still in progress.");
			}

			WebAsyncResult result;
			result = asyncWrite = new WebAsyncResult (this, callback, state);
			initialMethod = method;
			if (haveRequest) {
				if (writeStream != null) {
					Monitor.Exit (this);
					result.SetCompleted (true, writeStream);
					result.DoCallback ();
					return result;
				}
			}
			
			haveRequest = true;
			gotRequestStream = true;
			Monitor.Exit (this);
			servicePoint = GetServicePoint ();
			abortHandler = servicePoint.SendRequest (this, connectionGroup);
			return result;
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			result.WaitUntilComplete ();

			Exception e = result.Exception;
			if (e != null)
				throw e;

			return result.WriteStream;
		}
		
		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			if (!asyncResult.AsyncWaitHandle.WaitOne (timeout, false)) {
				Abort ();
				throw new WebException ("The request timed out", WebExceptionStatus.Timeout);
			}

			return EndGetRequestStream (asyncResult);
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			bool send = (method == "PUT" || method == "POST");
			if (send) {
				if ((!KeepAlive || (ContentLength == -1 && !SendChunked)) && !AllowWriteStreamBuffering)
					throw new ProtocolViolationException ("Content-Length not set");
			}

			CommonChecks (send);
			Monitor.Enter (this);
			if (asyncRead != null && !haveResponse) {
				Monitor.Exit (this);
				throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
							"method while a previous call is still in progress.");
			}

			asyncRead = new WebAsyncResult (this, callback, state);
			initialMethod = method;
			if (haveResponse) {
				if (webResponse != null) {
					Monitor.Exit (this);
					asyncRead.SetCompleted (true, webResponse);
					asyncRead.DoCallback ();
					return asyncRead;
				}
			}
			
			if (!requestSent) {
				servicePoint = GetServicePoint ();
				abortHandler = servicePoint.SendRequest (this, connectionGroup);
			}

			Monitor.Exit (this);
			return asyncRead;
		}
		
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			redirects = 0;
			bool redirected = false;
			asyncRead = result;
			do {
				if (redirected) {
					haveResponse = false;
					result.Reset ();
					servicePoint = GetServicePoint ();
					abortHandler = servicePoint.SendRequest (this, connectionGroup);
				}

				if (!result.WaitUntilComplete (timeout, false)) {
					Abort ();
					throw new WebException("The request timed out", WebExceptionStatus.Timeout);
				}

				redirected = CheckFinalStatus (result);
			} while (redirected);
			
			return result.Response;
		}
		
		public override WebResponse GetResponse()
		{
			if (haveResponse && webResponse != null)
				return webResponse;

			WebAsyncResult result = (WebAsyncResult) BeginGetResponse (null, null);
			return EndGetResponse (result);
		}
		
		public override void Abort ()
		{
			haveResponse = true;
			aborted = true;
			asyncRead = null;
			asyncWrite = null;
			if (abortHandler != null) {
				try {
					abortHandler (this, EventArgs.Empty);
				} catch {}
				abortHandler = null;
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
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		void CheckRequestStarted () 
		{
			if (haveRequest)
				throw new InvalidOperationException ("request started");
		}

		internal void DoContinueDelegate (int statusCode, WebHeaderCollection headers)
		{
			if (continueDelegate != null)
				continueDelegate (statusCode, headers);
		}
		
		bool Redirect (WebAsyncResult result, HttpStatusCode code)
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
			case HttpStatusCode.TemporaryRedirect: // 307
				if (method != "GET" && method != "HEAD") // 10.3
					return false;

				uriString = webResponse.Headers ["Location"];
				break;
			case HttpStatusCode.SeeOther: //303
				method = "GET";
				uriString = webResponse.Headers ["Location"];
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

			if (e != null)
				throw e;

			actualUri = new Uri (uriString);
			return true;
		}

		string GetHeaders ()
		{
			StringBuilder result = new StringBuilder ();
			bool continue100 = false;
			if (gotRequestStream && contentLength != -1) {
				continue100 = true;
				webHeaders.SetInternal ("Content-Length", contentLength.ToString ());
			} else if (sendChunked) {
				continue100 = true;
				webHeaders.SetInternal ("Transfer-Encoding", "chunked");
			}

			if (continue100 && servicePoint.SendContinue) { // RFC2616 8.2.3
				webHeaders.SetInternal ("Expect" , "100-continue");
				expectContinue = true;
			} else {
				expectContinue = false;
			}

			if (keepAlive && version == HttpVersion.Version10)
				webHeaders.SetInternal ("Connection", "keep-alive");
			else if (!keepAlive && version == HttpVersion.Version11)
				webHeaders.SetInternal ("Connection", "close");

			webHeaders.SetInternal ("Host", actualUri.Host);
			if (cookieContainer != null) {
				string cookieHeader = cookieContainer.GetCookieHeader (requestUri);
				if (cookieHeader != "")
					webHeaders.SetInternal ("Cookie", cookieHeader);
			}

			return webHeaders.ToString ();
		}
		
		internal void SetWriteStreamError (WebExceptionStatus status)
		{
			if (aborted) {
				//TODO
			}

			WebAsyncResult r = asyncWrite;
			if (r == null)
				r = asyncRead;

			if (r != null) {
				r.SetCompleted (false, new WebException ("Error: " + status, status));
				r.DoCallback ();
			}
		}

		internal void SendRequestHeaders ()
		{
			StringBuilder req = new StringBuilder ();
			req.AppendFormat ("{0} {1} HTTP/{2}.{3}\r\n", method, actualUri.PathAndQuery,
								      version.Major, version.Minor);
			req.Append (GetHeaders ());
			string reqstr = req.ToString ();
			byte [] bytes = Encoding.UTF8.GetBytes (reqstr);
			writeStream.SetHeaders (bytes, 0, bytes.Length);
		}

		internal void SetWriteStream (WebConnectionStream stream)
		{
			if (aborted) {
				//TODO
			}
			
			writeStream = stream;
			haveRequest = true;
			if (asyncWrite != null) {
				asyncWrite.SetCompleted (false, stream);
				asyncWrite.DoCallback ();
				asyncWrite = null;
			}

			SendRequestHeaders ();
		}

		internal void SetResponseError (WebExceptionStatus status, Exception e)
		{
			WebAsyncResult r = asyncRead;
			if (r == null)
				r = asyncWrite;

			if (r != null) {
				WebException wexc = new WebException ("Error getting response stream", e, status, null); 
				r.SetCompleted (false, wexc);
				r.DoCallback ();
			}
		}
		
		internal void SetResponseData (WebConnectionData data)
		{
			if (aborted) {
				if (data.stream != null)
					data.stream.Close ();
				return;
			}
			
			webResponse = new HttpWebResponse (actualUri, method, data);
			haveResponse = true;

			if (asyncRead != null) {
				asyncRead.SetCompleted (false, webResponse);
				asyncRead.DoCallback ();
			}
		}

		// Returns true if redirected
		bool CheckFinalStatus (WebAsyncResult result)
		{
			Exception throwMe = result.Exception;

			HttpWebResponse resp = result.Response;
			WebExceptionStatus protoError = WebExceptionStatus.ProtocolError;
			HttpStatusCode code = 0;
			if (throwMe == null && webResponse != null) {
				code  = webResponse.StatusCode;
				if ((int) code >= 400 ) {
					string err = String.Format ("The remote server returned an error: ({0}) {1}.",
								    (int) code, webResponse.StatusDescription);
					throwMe = new WebException (err, null, protoError, webResponse);
				} else if ((int) code >= 300 && allowAutoRedirect && redirects > maxAutoRedirect) {
					throwMe = new WebException ("Max. redirections exceeded.", null,
								    protoError, webResponse);
				}
			}

			if (throwMe == null) {
				bool b = false;
				if (allowAutoRedirect && (int) code >= 300)
					b = Redirect (result, code);

				return b;
			}

			if (writeStream != null) {
				writeStream.InternalClose ();
				writeStream = null;
			}

			if (webResponse != null)
				webResponse = null;

			throw throwMe;
		}
	}
}

