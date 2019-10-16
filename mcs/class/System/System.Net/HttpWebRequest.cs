//
// System.Net.HttpWebRequest
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Martin Baulig <mabaul@microsoft.com>
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
#endif

using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Net.Security;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Net.Security;

namespace System.Net
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		Uri requestUri;
		Uri actualUri;
		bool hostChanged;
		bool allowAutoRedirect = true;
		bool allowBuffering = true;
		bool allowReadStreamBuffering;
		X509CertificateCollection certificates;
		string connectionGroup;
		bool haveContentLength;
		long contentLength = -1;
		HttpContinueDelegate continueDelegate;
		CookieContainer cookieContainer;
		ICredentials credentials;
		bool haveResponse;
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
		int continueTimeout = 350;

		WebRequestStream writeStream;
		HttpWebResponse webResponse;
		WebCompletionSource responseTask;
		WebOperation currentOperation;
		int aborted;
		bool gotRequestStream;
		int redirects;
		bool expectContinue;
		bool getResponseCalled;
		object locker = new object ();
		bool finished_reading;
		DecompressionMethods auto_decomp;
		int maxResponseHeadersLength;
		static int defaultMaxResponseHeadersLength;
		static int defaultMaximumErrorResponseLength;
		static RequestCachePolicy defaultCachePolicy;
		int readWriteTimeout = 300000; // ms
#if SECURITY_DEP
		MobileTlsProvider tlsProvider;
		MonoTlsSettings tlsSettings;
#endif
		ServerCertValidationCallback certValidationCallback;

		// stores the user provided Host header as Uri. If the user specified a default port explicitly we'll lose
		// that information when converting the host string to a Uri. _HostHasPort will store that information.
		bool hostHasPort;
		Uri hostUri;

		enum NtlmAuthState
		{
			None,
			Challenge,
			Response
		}
		AuthorizationState auth_state, proxy_auth_state;

		[NonSerialized]
		internal Func<Stream, Task> ResendContentFactory;

		// Constructors
		static HttpWebRequest ()
		{
			defaultMaxResponseHeadersLength = 64;
			defaultMaximumErrorResponseLength = 64;
			defaultCachePolicy = new RequestCachePolicy (RequestCacheLevel.BypassCache);
#if !MOBILE
#pragma warning disable 618
			NetConfig config = ConfigurationSettings.GetConfig ("system.net/settings") as NetConfig;
#pragma warning restore 618
			if (config != null)
				defaultMaxResponseHeadersLength = config.MaxResponseHeadersLength;
#endif
		}

#if MOBILE
		public
#else
		internal
#endif
		HttpWebRequest (Uri uri)
		{
			this.requestUri = uri;
			this.actualUri = uri;
			this.proxy = InternalDefaultWebProxy;
			this.webHeaders = new WebHeaderCollection (WebHeaderCollectionType.HttpWebRequest);
			ThrowOnError = true;
			ResetAuthorization ();
		}

#if SECURITY_DEP
		internal HttpWebRequest (Uri uri, MobileTlsProvider tlsProvider, MonoTlsSettings settings = null)
			: this (uri)
		{
			this.tlsProvider = tlsProvider;
			this.tlsSettings = settings;
		}
#endif

		[Obsolete ("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			// In CoreFX, attempting to serialize this class fails due to
			// non-serializable fields, so this constructor never gets called.
			// They're throwing PlatformNotSupportedException() in here.
			throw new SerializationException ();
		}

#if MONO_WEB_DEBUG
		static int nextId;
		internal readonly int ID = ++nextId;
#else
		internal readonly int ID;
#endif

		void ResetAuthorization ()
		{
			auth_state = new AuthorizationState (this, false);
			proxy_auth_state = new AuthorizationState (this, true);
		}

		// Properties

		void SetSpecialHeaders (string HeaderName, string value)
		{
			value = WebHeaderCollection.CheckBadChars (value, true);
			webHeaders.RemoveInternal (HeaderName);
			if (value.Length != 0) {
				webHeaders.AddInternal (HeaderName, value);
			}
		}

		public string Accept {
			get { return webHeaders["Accept"]; }
			set {
				CheckRequestStarted ();
				SetSpecialHeaders ("Accept", value);
			}
		}

		public Uri Address {
			get { return actualUri; }
			internal set { actualUri = value; } // Used by Ftp+proxy
		}

		public virtual bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { this.allowAutoRedirect = value; }
		}

		public virtual bool AllowWriteStreamBuffering {
			get { return allowBuffering; }
			set { allowBuffering = value; }
		}

		public virtual bool AllowReadStreamBuffering {
			get { return allowReadStreamBuffering; }
			set { allowReadStreamBuffering = value; }
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}

		public DecompressionMethods AutomaticDecompression {
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
				return allowBuffering && MethodWithBuffer;
			}
		}

		bool MethodWithBuffer {
			get {
				return method != "HEAD" && method != "GET" &&
				method != "MKCOL" && method != "CONNECT" &&
				method != "TRACE";
			}
		}

#if SECURITY_DEP
		internal MobileTlsProvider TlsProvider {
			get { return tlsProvider; }
		}

		internal MonoTlsSettings TlsSettings {
			get { return tlsSettings; }
		}
#endif

		public X509CertificateCollection ClientCertificates {
			get {
				if (certificates == null)
					certificates = new X509CertificateCollection ();
				return certificates;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				certificates = value;
			}
		}

		public string Connection {
			get { return webHeaders["Connection"]; }
			set {
				CheckRequestStarted ();

				if (string.IsNullOrWhiteSpace (value)) {
					webHeaders.RemoveInternal ("Connection");
					return;
				}

				string val = value.ToLowerInvariant ();
				if (val.Contains ("keep-alive") || val.Contains ("close"))
					throw new ArgumentException (SR.net_connarg, nameof (value));

				string checkedValue = HttpValidationHelpers.CheckBadHeaderValueChars (value);
				webHeaders.CheckUpdate ("Connection", checkedValue);
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
			get { return webHeaders["Content-Type"]; }
			set {
				SetSpecialHeaders ("Content-Type", value);
			}
		}

		public HttpContinueDelegate ContinueDelegate {
			get { return continueDelegate; }
			set { continueDelegate = value; }
		}

		virtual
		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}

		public override ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}
		public DateTime Date {
			get {
				string date = webHeaders["Date"];
				if (date == null)
					return DateTime.MinValue;
				return DateTime.ParseExact (date, "r", CultureInfo.InvariantCulture).ToLocalTime ();
			}
			set {
				SetDateHeaderHelper ("Date", value);
			}
		}

		void SetDateHeaderHelper (string headerName, DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
				SetSpecialHeaders (headerName, null); // remove header
			else
				SetSpecialHeaders (headerName, HttpProtocolUtils.date2string (dateTime));
		}

#if !MOBILE
		[MonoTODO]
		public static new RequestCachePolicy DefaultCachePolicy {
			get { return defaultCachePolicy; }
			set { defaultCachePolicy = value; }
		}
#endif

		[MonoTODO]
		public static int DefaultMaximumErrorResponseLength {
			get { return defaultMaximumErrorResponseLength; }
			set { defaultMaximumErrorResponseLength = value; }
		}

		public string Expect {
			get { return webHeaders["Expect"]; }
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

				webHeaders.CheckUpdate ("Expect", value);
			}
		}

		virtual
		public bool HaveResponse {
			get { return haveResponse; }
		}

		public override WebHeaderCollection Headers {
			get { return webHeaders; }
			set {
				CheckRequestStarted ();

				WebHeaderCollection webHeaders = value;
				WebHeaderCollection newWebHeaders = new WebHeaderCollection (WebHeaderCollectionType.HttpWebRequest);

				// Copy And Validate -
				// Handle the case where their object tries to change
				//  name, value pairs after they call set, so therefore,
				//  we need to clone their headers.
				//

				foreach (String headerName in webHeaders.AllKeys) {
					newWebHeaders.Add (headerName, webHeaders[headerName]);
				}

				this.webHeaders = newWebHeaders;
			}
		}

		public string Host {
			get {
				Uri uri = hostUri ?? Address;
				return (hostUri == null || !hostHasPort) && Address.IsDefaultPort ?
				    uri.Host : uri.Host + ":" + uri.Port;
			}
			set {
				CheckRequestStarted ();

				if (value == null)
					throw new ArgumentNullException (nameof (value));

				Uri uri;
				if ((value.IndexOf ('/') != -1) || (!TryGetHostUri (value, out uri)))
					throw new ArgumentException (SR.net_invalid_host, nameof (value));

				hostUri = uri;

				// Determine if the user provided string contains a port
				if (!hostUri.IsDefaultPort) {
					hostHasPort = true;
				} else if (value.IndexOf (':') == -1) {
					hostHasPort = false;
				} else {
					int endOfIPv6Address = value.IndexOf (']');
					hostHasPort = endOfIPv6Address == -1 || value.LastIndexOf (':') > endOfIPv6Address;
				}
			}
		}

		bool TryGetHostUri (string hostName, out Uri hostUri)
		{
			string s = Address.Scheme + "://" + hostName + Address.PathAndQuery;
			return Uri.TryCreate (s, UriKind.Absolute, out hostUri);
		}

		public DateTime IfModifiedSince {
			get {
				string str = webHeaders["If-Modified-Since"];
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
			set {
				CheckRequestStarted ();
				if (value < 0 && value != System.Threading.Timeout.Infinite)
					throw new ArgumentOutOfRangeException (nameof (value), SR.net_toosmall);

				maxResponseHeadersLength = value;
			}
		}

		[MonoTODO ("Use this")]
		public static int DefaultMaximumResponseHeadersLength {
			get { return defaultMaxResponseHeadersLength; }
			set { defaultMaxResponseHeadersLength = value; }
		}

		public int ReadWriteTimeout {
			get { return readWriteTimeout; }
			set {
				CheckRequestStarted ();

				if (value <= 0 && value != System.Threading.Timeout.Infinite)
					throw new ArgumentOutOfRangeException (nameof (value), SR.net_io_timeout_use_gt_zero);

				readWriteTimeout = value;
			}
		}

		[MonoTODO]
		public int ContinueTimeout {
			get {
				return continueTimeout;
			}
			set {
				CheckRequestStarted ();
				if ((value < 0) && (value != System.Threading.Timeout.Infinite))
					throw new ArgumentOutOfRangeException (nameof (value), SR.net_io_timeout_use_ge_zero);
				continueTimeout = value;
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
				if (string.IsNullOrEmpty (value))
					throw new ArgumentException (SR.net_badmethod, nameof (value));
				if (HttpValidationHelpers.IsInvalidMethodOrHeaderString (value))
					throw new ArgumentException (SR.net_badmethod, nameof (value));

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
					throw new ArgumentException (SR.net_wrongversion, nameof (value));

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
				GetServicePoint ();
			}
		}

		public string Referer {
			get { return webHeaders["Referer"]; }
			set {
				CheckRequestStarted ();
				if (value == null || value.Trim ().Length == 0) {
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
		public virtual bool SupportsCookieContainer {
			get {
				// The managed implementation supports the cookie container
				// it is only Silverlight that returns false here
				return true;
			}
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
			get { return webHeaders["Transfer-Encoding"]; }
			set {
				CheckRequestStarted ();

				if (string.IsNullOrWhiteSpace (value)) {
					webHeaders.RemoveInternal ("Transfer-Encoding");
					return;
				}

				string val = value.ToLower ();
				//
				// prevent them from adding chunked, or from adding an Encoding without
				// turning on chunked, the reason is due to the HTTP Spec which prevents
				// additional encoding types from being used without chunked
				//
				if (val.Contains ("chunked"))
					throw new ArgumentException (SR.net_nochunked, nameof (value));
				else if (!SendChunked)
					throw new InvalidOperationException (SR.net_needchunked);

				string checkedValue = HttpValidationHelpers.CheckBadHeaderValueChars (value);
				webHeaders.CheckUpdate ("Transfer-Encoding", checkedValue);
			}
		}

		public override bool UseDefaultCredentials {
			get { return CredentialCache.DefaultCredentials == Credentials; }
			set { Credentials = value ? CredentialCache.DefaultCredentials : null; }
		}

		public string UserAgent {
			get { return webHeaders["User-Agent"]; }
			set { webHeaders.SetInternal ("User-Agent", value); }
		}

		bool unsafe_auth_blah;
		public bool UnsafeAuthenticatedConnectionSharing {
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

		internal ServerCertValidationCallback ServerCertValidationCallback {
			get { return certValidationCallback; }
		}

		public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get {
				if (certValidationCallback == null)
					return null;
				return certValidationCallback.ValidationCallback;
			}
			set {
				if (value == null)
					certValidationCallback = null;
				else
					certValidationCallback = new ServerCertValidationCallback (value);
			}
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
			AddRange ("bytes", (long)range);
		}

		public void AddRange (int from, int to)
		{
			AddRange ("bytes", (long)from, (long)to);
		}

		public void AddRange (string rangeSpecifier, int range)
		{
			AddRange (rangeSpecifier, (long)range);
		}

		public void AddRange (string rangeSpecifier, int from, int to)
		{
			AddRange (rangeSpecifier, (long)from, (long)to);
		}
		public
		void AddRange (long range)
		{
			AddRange ("bytes", (long)range);
		}

		public
		void AddRange (long from, long to)
		{
			AddRange ("bytes", from, to);
		}

		public
		void AddRange (string rangeSpecifier, long range)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (!WebHeaderCollection.IsValidToken (rangeSpecifier))
				throw new ArgumentException ("Invalid range specifier", "rangeSpecifier");

			string r = webHeaders["Range"];
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
			webHeaders.ChangeInternal ("Range", r);
		}

		public
		void AddRange (string rangeSpecifier, long from, long to)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (!WebHeaderCollection.IsValidToken (rangeSpecifier))
				throw new ArgumentException ("Invalid range specifier", "rangeSpecifier");
			if (from > to || from < 0)
				throw new ArgumentOutOfRangeException ("from");
			if (to < 0)
				throw new ArgumentOutOfRangeException ("to");

			string r = webHeaders["Range"];
			if (r == null)
				r = rangeSpecifier + "=";
			else
				r += ",";

			r = String.Format ("{0}{1}-{2}", r, from, to);
			webHeaders.ChangeInternal ("Range", r);
		}

		WebOperation SendRequest (bool redirecting, BufferOffsetSize writeBuffer, CancellationToken cancellationToken)
		{
			lock (locker) {
				WebConnection.Debug ($"HWR SEND REQUEST: Req={ID} requestSent={requestSent} actualUri={actualUri} redirecting={redirecting}");

				WebOperation operation;
				if (!redirecting) {
					if (requestSent) {
						operation = currentOperation;
						if (operation == null)
							throw new InvalidOperationException ("Should never happen!");
						return operation;
					}
				}

				operation = new WebOperation (this, writeBuffer, false, cancellationToken);
				if (Interlocked.CompareExchange (ref currentOperation, operation, null) != null)
					throw new InvalidOperationException ("Invalid nested call.");

				requestSent = true;
				if (!redirecting)
					redirects = 0;
				servicePoint = GetServicePoint ();
				servicePoint.SendRequest (operation, connectionGroup);
				return operation;
			}
		}

		Task<Stream> MyGetRequestStreamAsync (CancellationToken cancellationToken)
		{
			if (Aborted)
				throw CreateRequestAbortedException ();

			bool send = !(method == "GET" || method == "CONNECT" || method == "HEAD" || method == "TRACE");
			if (method == null || !send)
				throw new ProtocolViolationException (SR.net_nouploadonget);

			if (contentLength == -1 && !sendChunked && !allowBuffering && KeepAlive)
				throw new ProtocolViolationException ("Content-Length not set");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new InvalidOperationException (SR.net_needchunked);

			WebOperation operation;
			lock (locker) {
				if (getResponseCalled)
					throw new InvalidOperationException (SR.net_reqsubmitted);

				operation = currentOperation;
				if (operation == null) {
					initialMethod = method;

					gotRequestStream = true;
					operation = SendRequest (false, null, cancellationToken);
				}
			}

			return operation.GetRequestStream ();
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
		{
			return TaskToApm.Begin (RunWithTimeout (MyGetRequestStreamAsync), callback, state);
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			try {
				return TaskToApm.End<Stream> (asyncResult);
			} catch (Exception e) {
				throw GetWebException (e);
			}
		}

		public override Stream GetRequestStream ()
		{
			try {
				return GetRequestStreamAsync ().Result;
			} catch (Exception e) {
				throw GetWebException (e);
			}
		}

		[MonoTODO]
		public Stream GetRequestStream (out TransportContext context)
		{
			throw new NotImplementedException ();
		}

		public override Task<Stream> GetRequestStreamAsync ()
		{
			return RunWithTimeout (MyGetRequestStreamAsync);
		}

		internal static Task<T> RunWithTimeout<T> (
			Func<CancellationToken, Task<T>> func, int timeout, Action abort,
			Func<bool> aborted, CancellationToken cancellationToken)
		{
			var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			// Call `func` here to propagate any potential exception that it
			// might throw to our caller rather than returning a faulted task.
			var workerTask = func (cts.Token);
			return RunWithTimeoutWorker (workerTask, timeout, abort, aborted, cts);
		}

		static async Task<T> RunWithTimeoutWorker<T> (
			Task<T> workerTask, int timeout, Action abort, Func<bool> aborted,
			CancellationTokenSource cts)
		{
			try {
				if (await ServicePointScheduler.WaitAsync (workerTask, timeout).ConfigureAwait (false))
					return workerTask.Result;
				try {
					cts.Cancel ();
					abort ();
				} catch {
					// Ignore; we report the timeout.
				}
#pragma warning disable 4014
				// Make sure the workerTask's Exception is actually observed.
				// Fixes https://github.com/mono/mono/issues/10488.
				workerTask.ContinueWith (t => t.Exception?.GetHashCode (), TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore 4014
				throw new WebException (SR.net_timeout, WebExceptionStatus.Timeout);
			} catch (Exception ex) {
				throw GetWebException (ex, aborted ());
			} finally {
				cts.Dispose ();
			}
		}

		Task<T> RunWithTimeout<T> (Func<CancellationToken, Task<T>> func)
		{
			// Call `func` here to propagate any potential exception that it
			// might throw to our caller rather than returning a faulted task.
			var cts = new CancellationTokenSource ();
			var workerTask = func (cts.Token);
			return RunWithTimeoutWorker (workerTask, timeout, Abort, () => Aborted, cts);
		}

		async Task<HttpWebResponse> MyGetResponseAsync (CancellationToken cancellationToken)
		{
			if (Aborted)
				throw CreateRequestAbortedException ();

			var completion = new WebCompletionSource ();
			WebOperation operation;
			lock (locker) {
				getResponseCalled = true;
				var oldCompletion = Interlocked.CompareExchange (ref responseTask, completion, null);
				WebConnection.Debug ($"HWR GET RESPONSE: Req={ID} {oldCompletion != null}");
				if (oldCompletion != null) {
					oldCompletion.ThrowOnError ();
					if (haveResponse && oldCompletion.Task.IsCompleted)
						return webResponse;
					throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
								"method while a previous call is still in progress.");
				}

				operation = currentOperation;
				if (currentOperation != null)
					writeStream = currentOperation.WriteStream;

				initialMethod = method;

				operation = SendRequest (false, null, cancellationToken);
			}

			while (true) {
				WebException throwMe = null;
				HttpWebResponse response = null;
				WebResponseStream stream = null;
				bool redirect = false;
				bool mustReadAll = false;
				WebOperation ntlm = null;
				BufferOffsetSize writeBuffer = null;

				try {
					cancellationToken.ThrowIfCancellationRequested ();

					WebConnection.Debug ($"HWR GET RESPONSE LOOP: Req={ID} Op={operation?.ID} {auth_state.NtlmAuthState}");

					writeStream = await operation.GetRequestStreamInternal ();
					await writeStream.WriteRequestAsync (cancellationToken).ConfigureAwait (false);

					stream = await operation.GetResponseStream ();

					WebConnection.Debug ($"HWR RESPONSE LOOP #0: Req={ID} Op={operation?.ID} - {stream?.Headers != null}");

					(response, redirect, mustReadAll, writeBuffer, ntlm) = await GetResponseFromData (
						stream, cancellationToken).ConfigureAwait (false);
				} catch (Exception e) {
					throwMe = GetWebException (e);
				}

				WebConnection.Debug ($"HWR GET RESPONSE LOOP #1: Req={ID} Op={operation?.ID} - redirect={redirect} mustReadAll={mustReadAll} writeBuffer={writeBuffer != null} ntlm={ntlm != null} - {throwMe != null}");

				lock (locker) {
					if (throwMe != null) {
						WebConnection.Debug ($"HWR GET RESPONSE LOOP #1 EX: Req={ID} {throwMe.Status} {throwMe.InnerException?.GetType ()}");
						haveResponse = true;
						completion.TrySetException (throwMe);
						throw throwMe;
					}

					if (!redirect) {
						haveResponse = true;
						webResponse = response;
						completion.TrySetCompleted ();
						return response;
					}

					finished_reading = false;
					haveResponse = false;
					webResponse = null;
					currentOperation = ntlm;
					WebConnection.Debug ($"HWR GET RESPONSE LOOP #2: Req={ID} {mustReadAll} {ntlm}");
				}

				try {
					if (mustReadAll)
						await stream.ReadAllAsync (redirect || ntlm != null, cancellationToken).ConfigureAwait (false);
					operation.Finish (true);
					response.Close ();
				} catch (Exception e) {
					throwMe = GetWebException (e);
				}

				lock (locker) {
					WebConnection.Debug ($"HWR GET RESPONSE LOOP #3: Req={ID} {writeBuffer != null} {ntlm != null}");
					if (throwMe != null) {
						WebConnection.Debug ($"HWR GET RESPONSE LOOP #3 EX: Req={ID} {throwMe.Status} {throwMe.InnerException?.GetType ()}");
						haveResponse = true;
						stream?.Close ();
						completion.TrySetException (throwMe);
						throw throwMe;
					}

					if (ntlm == null) {
						operation = SendRequest (true, writeBuffer, cancellationToken);
					} else {
						operation = ntlm;
					}
				}
			}
		}

		async Task<(HttpWebResponse response, bool redirect, bool mustReadAll, BufferOffsetSize writeBuffer, WebOperation ntlm)>
			GetResponseFromData (WebResponseStream stream, CancellationToken cancellationToken)
		{
			/*
			 * WebConnection has either called SetResponseData() or SetResponseError().
		 	*/

			var response = new HttpWebResponse (actualUri, method, stream, cookieContainer);

			WebException throwMe = null;
			bool redirect = false;
			bool mustReadAll = false;
			WebOperation ntlm = null;
			Task<BufferOffsetSize> rewriteHandler = null;
			BufferOffsetSize writeBuffer = null;

			lock (locker) {
				(redirect, mustReadAll, rewriteHandler, throwMe) = CheckFinalStatus (response);
			}

			if (throwMe != null) {
				if (mustReadAll)
					await stream.ReadAllAsync (false, cancellationToken).ConfigureAwait (false);
				throw throwMe;
			}

			if (rewriteHandler != null) {
				writeBuffer = await rewriteHandler.ConfigureAwait (false);
			}

			lock (locker) {
				bool isProxy = ProxyQuery && proxy != null && !proxy.IsBypassed (actualUri);

				if (!redirect) {
					if ((isProxy ? proxy_auth_state : auth_state).IsNtlmAuthenticated && (int)response.StatusCode < 400) {
						stream.Connection.NtlmAuthenticated = true;
					}

					// clear internal buffer so that it does not
					// hold possible big buffer (bug #397627)
					if (writeStream != null)
						writeStream.KillBuffer ();

					return (response, false, false, writeBuffer, null);
				}

				if (sendChunked) {
					sendChunked = false;
					webHeaders.RemoveInternal ("Transfer-Encoding");
				}

				bool isChallenge;
				(ntlm, isChallenge) = HandleNtlmAuth (stream, response, writeBuffer, cancellationToken);
				WebConnection.Debug ($"HWR REDIRECT: {ntlm} {isChallenge} {mustReadAll}");
			}

			return (response, true, mustReadAll, writeBuffer, ntlm);
		}

		internal static Exception FlattenException (Exception e)
		{
			if (e is AggregateException ae) {
				ae = ae.Flatten ();
				if (ae.InnerExceptions.Count == 1)
					return ae.InnerException;
			}

			return e;
		}

		WebException GetWebException (Exception e)
		{
			return GetWebException (e, Aborted);
		}

		static WebException GetWebException (Exception e, bool aborted)
		{
			e = FlattenException (e);
			if (e is WebException wexc) {
				if (!aborted || wexc.Status == WebExceptionStatus.RequestCanceled || wexc.Status == WebExceptionStatus.Timeout)
					return wexc;
			}
			if (aborted || e is OperationCanceledException || e is ObjectDisposedException)
				return CreateRequestAbortedException ();
			return new WebException (e.Message, e, WebExceptionStatus.UnknownError, null);
		}

		internal static WebException CreateRequestAbortedException ()
		{
			return new WebException (SR.Format (SR.net_reqaborted, WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
		}

		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			if (Aborted)
				throw CreateRequestAbortedException ();

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "") {
				/*
				 * The only way we could get here without already catching this in the
				 * `TransferEncoding` property settor is via HttpClient, which does not
				 * do strict checking on all headers.
				 *
				 * We can remove this check again after switching to the CoreFX version
				 * of HttpClient.
				 *
				 */
				throw new InvalidOperationException (SR.net_needchunked);
			}

			return TaskToApm.Begin (RunWithTimeout (MyGetResponseAsync), callback, state);
		}

		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException (nameof (asyncResult));

			try {
				return TaskToApm.End<HttpWebResponse> (asyncResult);
			} catch (Exception e) {
				throw GetWebException (e);
			}
		}

		public Stream EndGetRequestStream (IAsyncResult asyncResult, out TransportContext context)
		{
			if (asyncResult == null)
				throw new ArgumentNullException (nameof (asyncResult));

			context = null;
			return EndGetRequestStream (asyncResult);
		}

		public override WebResponse GetResponse ()
		{
			try {
				return GetResponseAsync ().Result;
			} catch (Exception e) {
				throw GetWebException (e);
			}
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

			WebConnection.Debug ($"HWR ABORT: Req={ID}");

			haveResponse = true;
			var operation = currentOperation;
			if (operation != null)
				operation.Abort ();

			responseTask?.TrySetCanceled ();

			if (webResponse != null) {
				try {
					webResponse.Close ();
					webResponse = null;
				} catch { }
			}
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new SerializationException ();
		}

		protected override void GetObjectData (SerializationInfo serializationInfo,
			StreamingContext streamingContext)
		{
			throw new SerializationException ();
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

		bool Redirect (HttpStatusCode code, WebResponse response)
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
				e = new ProtocolViolationException ("Invalid status code: " + (int)code);
				break;
			}

			if (method != "GET" && !InternalAllowBuffering && ResendContentFactory == null &&
			    (writeStream.WriteBufferLength > 0 || contentLength > 0))
				e = new WebException ("The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, response);

			if (e != null)
				throw e;

			if (AllowWriteStreamBuffering || method == "GET")
				contentLength = -1;

			uriString = response.Headers["Location"];

			if (uriString == null)
				throw new WebException ($"No Location header found for {(int)code}", null,
				                        WebExceptionStatus.ProtocolError, response);

			Uri prev = actualUri;
			try {
				actualUri = new Uri (actualUri, uriString);
			} catch (Exception) {
				throw new WebException ($"Invalid URL ({uriString}) for {(int)code}",
				                        null, WebExceptionStatus.ProtocolError, response);
			}

			hostChanged = (actualUri.Scheme != prev.Scheme || Host != prev.Authority);
			return true;
		}

		string GetHeaders ()
		{
			bool continue100 = false;
			if (sendChunked) {
				continue100 = true;
				webHeaders.ChangeInternal ("Transfer-Encoding", "chunked");
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
				webHeaders.ChangeInternal ("Expect", "100-continue");
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
					webHeaders.ChangeInternal (connectionHeader, "keep-alive");
			} else if (!keepAlive && version == HttpVersion.Version11) {
				webHeaders.ChangeInternal (connectionHeader, "close");
			}

			string host;
			if (hostUri != null) {
				if (hostHasPort)
					host = hostUri.GetComponents (UriComponents.HostAndPort, UriFormat.Unescaped);
				else
					host = hostUri.GetComponents (UriComponents.Host, UriFormat.Unescaped);
			} else if (Address.IsDefaultPort) {
				host = Address.GetComponents (UriComponents.Host, UriFormat.Unescaped);
			} else {
				host = Address.GetComponents (UriComponents.HostAndPort, UriFormat.Unescaped);
			}
			webHeaders.SetInternal ("Host", host);

			if (cookieContainer != null) {
				string cookieHeader = cookieContainer.GetCookieHeader (actualUri);
				if (cookieHeader != "")
					webHeaders.ChangeInternal ("Cookie", cookieHeader);
				else
					webHeaders.RemoveInternal ("Cookie");
			}

			string accept_encoding = null;
			if ((auto_decomp & DecompressionMethods.GZip) != 0)
				accept_encoding = "gzip";
			if ((auto_decomp & DecompressionMethods.Deflate) != 0)
				accept_encoding = accept_encoding != null ? "gzip, deflate" : "deflate";
			if (accept_encoding != null)
				webHeaders.ChangeInternal ("Accept-Encoding", accept_encoding);

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
			webHeaders[authHeader] = auth.Message;
			usedPreAuth = true;
		}

		internal byte[] GetRequestHeaders ()
		{
			StringBuilder req = new StringBuilder ();
			string query;
			if (!ProxyQuery) {
				query = actualUri.PathAndQuery;
			} else {
				query = String.Format ("{0}://{1}{2}", actualUri.Scheme,
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

		(WebOperation, bool) HandleNtlmAuth (WebResponseStream stream, HttpWebResponse response,
						     BufferOffsetSize writeBuffer, CancellationToken cancellationToken)
		{
			bool isProxy = response.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
			if ((isProxy ? proxy_auth_state : auth_state).NtlmAuthState == NtlmAuthState.None)
				return (null, false);

			var isChallenge = auth_state.NtlmAuthState == NtlmAuthState.Challenge || proxy_auth_state.NtlmAuthState == NtlmAuthState.Challenge;

			var operation = new WebOperation (this, writeBuffer, isChallenge, cancellationToken);
			stream.Operation.SetPriorityRequest (operation);
			var creds = (!isProxy || proxy == null) ? credentials : proxy.Credentials;
			if (creds != null) {
				stream.Connection.NtlmCredential = creds.GetCredential (requestUri, "NTLM");
				stream.Connection.UnsafeAuthenticatedConnectionSharing = unsafe_auth_blah;
			}
			return (operation, isChallenge);
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

				string[] authHeaders = response.Headers.GetValues (isProxy ? "Proxy-Authenticate" : "WWW-Authenticate");
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
				request.webHeaders[isProxy ? "Proxy-Authorization" : "Authorization"] = auth.Message;
				isCompleted = auth.Complete;
				bool is_ntlm = (auth.ModuleAuthenticationType == "NTLM");
				if (is_ntlm)
					ntlm_auth_state = (NtlmAuthState)((int)ntlm_auth_state + 1);
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

		(Task<BufferOffsetSize> task, WebException throwMe) GetRewriteHandler (HttpWebResponse response, bool redirect)
		{
			if (redirect) {
				if (!MethodWithBuffer)
					return (null, null);

				if (writeStream.WriteBufferLength == 0 || contentLength == 0)
					return (null, null);
			}

			// Keep the written body, so it can be rewritten in the retry
			if (AllowWriteStreamBuffering)
				return (Task.FromResult (writeStream.GetWriteBuffer ()), null);

			if (ResendContentFactory == null)
				return (null, new WebException (
					"The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, response));

			Func<Task<BufferOffsetSize>> handleResendContentFactory = async () => {
				using (var ms = new MemoryStream ()) {
					await ResendContentFactory (ms).ConfigureAwait (false);
					var buffer = ms.ToArray ();
					return new BufferOffsetSize (buffer, 0, buffer.Length, false);
				}
			};

			//
			// Buffering is not allowed but we have alternative way to get same content (we
			// need to resent it due to NTLM Authentication).
			//
			return (handleResendContentFactory (), null);
		}

		// Returns true if redirected
		(bool redirect, bool mustReadAll, Task<BufferOffsetSize> writeBuffer, WebException throwMe) CheckFinalStatus (HttpWebResponse response)
		{
			WebException throwMe = null;

			bool mustReadAll = false;
			HttpStatusCode code = 0;
			Task<BufferOffsetSize> rewriteHandler = null;

			code = response.StatusCode;
			if ((!auth_state.IsCompleted && code == HttpStatusCode.Unauthorized && credentials != null) ||
				(ProxyQuery && !proxy_auth_state.IsCompleted && code == HttpStatusCode.ProxyAuthenticationRequired)) {
				if (!usedPreAuth && CheckAuthorization (response, code)) {
					mustReadAll = true;

					// HEAD, GET, MKCOL, CONNECT, TRACE
					if (!MethodWithBuffer)
						return (true, mustReadAll, null, null);

					(rewriteHandler, throwMe) = GetRewriteHandler (response, false);
					if (throwMe == null)
						return (true, mustReadAll, rewriteHandler, null);

					if (!ThrowOnError)
						return (false, mustReadAll, null, null);

					writeStream.InternalClose ();
					writeStream = null;
					response.Close ();

					return (false, mustReadAll, null, throwMe);
				}
			}

			if ((int)code >= 400) {
				string err = String.Format ("The remote server returned an error: ({0}) {1}.",
				                            (int)code, response.StatusDescription);
				throwMe = new WebException (err, null, WebExceptionStatus.ProtocolError, response);
				mustReadAll = true;
			} else if ((int)code == 304 && allowAutoRedirect) {
				string err = String.Format ("The remote server returned an error: ({0}) {1}.",
				                            (int)code, response.StatusDescription);
				throwMe = new WebException (err, null, WebExceptionStatus.ProtocolError, response);
			} else if ((int)code >= 300 && allowAutoRedirect && redirects >= maxAutoRedirect) {
				throwMe = new WebException ("Max. redirections exceeded.", null,
				                            WebExceptionStatus.ProtocolError, response);
				mustReadAll = true;
			}

			if (throwMe == null) {
				int c = (int)code;
				bool b = false;
				if (allowAutoRedirect && c >= 300) {
					b = Redirect (code, response);
					(rewriteHandler, throwMe) = GetRewriteHandler (response, true);
					if (b && !unsafe_auth_blah) {
						auth_state.Reset ();
						proxy_auth_state.Reset ();
					}
				}

				if (c >= 300 && c != 304)
					mustReadAll = true;

				if (throwMe == null)
					return (b, mustReadAll, rewriteHandler, null);
			}

			if (!ThrowOnError)
				return (false, mustReadAll, null, null);

			if (writeStream != null) {
				writeStream.InternalClose ();
				writeStream = null;
			}

			return (false, mustReadAll, null, throwMe);
		}

		internal bool ReuseConnection {
			get;
			set;
		}

#region referencesource
        internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
        {
            StringBuilder connectionLine = new StringBuilder(connectionGroupName);

            connectionLine.Append(unsafeConnectionGroup ? "U>" : "S>");

            if (isInternalGroup)
            {
                connectionLine.Append("I>");
            }

            return connectionLine;
        }
#endregion
	}
}

