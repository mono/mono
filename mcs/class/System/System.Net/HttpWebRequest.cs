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
	public class HttpWebRequest : WebRequest, ISerializable, IDisposable
	{
		private Uri requestUri;
		private Uri actualUri = null;
		private bool allowAutoRedirect = true;
		private bool allowBuffering = true;
		private X509CertificateCollection certificate = null;
		private string connectionGroup = null;
		private long contentLength = -1;
		private HttpContinueDelegate continueDelegate = null;
		private CookieContainer cookieContainer = null;
		private ICredentials credentials = null;
		private bool haveResponse = false;		
		private WebHeaderCollection webHeaders;
		private bool keepAlive = true;
		private int maxAutoRedirect = 50;
		private string mediaType = String.Empty;
		private string method;
		private bool pipelined = true;
		private bool preAuthenticate = false;
		private Version version;		
		private IWebProxy proxy;
		private bool sendChunked = false;
		private ServicePoint servicePoint = null;
		private int timeout = System.Threading.Timeout.Infinite;
		
		private HttpWebRequestStream requestStream = null;
		private HttpWebResponse webResponse = null;
		private AutoResetEvent requestEndEvent = null;
		private bool requesting = false;
		private bool asyncResponding = false;
		private Socket socket;
		private bool requestClosed = false;
		private bool responseClosed = false;
		private bool disposed;
		
		// Constructors
		
		internal HttpWebRequest (Uri uri) 
		{ 
			this.requestUri = uri;
			this.actualUri = uri;
			this.webHeaders = new WebHeaderCollection (true);
			this.webHeaders.SetInternal ("Host", uri.Authority);
			this.webHeaders.SetInternal ("Date", DateTime.Now.ToUniversalTime ().ToString ("r", null));
			this.webHeaders.SetInternal ("Expect", "100-continue");
			this.method = "GET";
			this.version = HttpVersion.Version11;
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
			set { this.allowBuffering = value; }
		}
		
		public X509CertificateCollection ClientCertificates {
			get { return certificate; }
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
					throw new ArgumentException ("value");
				if (KeepAlive && val.IndexOf ("keep-alive") == -1)
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
					throw new ArgumentException ("value");
				contentLength = value;
				webHeaders.SetInternal ("Content-Length", Convert.ToString (value));
			}
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
					throw new ArgumentException ("value");
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
				newHeaders.SetInternal ("Host", this.webHeaders["Host"]);
				newHeaders.SetInternal ("Date", this.webHeaders["Date"]);
				newHeaders.SetInternal ("Expect", this.webHeaders["Expect"]);
				newHeaders.SetInternal ("Connection", this.webHeaders["Connection"]);
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
				CheckRequestStarted ();
				return keepAlive;
			}
			set {
				CheckRequestStarted ();
				keepAlive = value;
				if (Connection == null)
					webHeaders.SetInternal ("Connection", value ? "Keep-Alive" : "Close");
			}
		}
		
		public int MaximumAutomaticRedirections {
			get { return maxAutoRedirect; }
			set {
				if (value < 0)
					throw new ArgumentException ("value");
				maxAutoRedirect = value;
			}			
		}
		
		public string MediaType {
			get { return mediaType; }
			set { 
				CheckRequestStarted ();
				mediaType = value;
			}
		}
		
		public override string Method { 
			get { return this.method; }
			set { 
				CheckRequestStarted ();
				
				if (value == null ||
				    (value != "GET" &&
				     value != "HEAD" &&
				     value != "POST" &&
				     value != "PUT" &&
				     value != "DELETE" &&
				     value != "TRACE" &&
				     value != "OPTIONS"))
					throw new ArgumentException ("not a valid method");
				if (contentLength != -1 &&
				    value != "POST" &&
				    value != "PUT")
				    	throw new ArgumentException ("method must be PUT or POST");
				
				method = value;
			}
		}
		
		public bool Pipelined {
			get { return pipelined; }
			set { this.pipelined = value; }
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
				version = (Version) value; 
			}
		}
		
		public override IWebProxy Proxy { 
			get { return proxy; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				proxy = value;
			}
		}
		
		public string Referer {
			get { return webHeaders ["Referer" ]; }
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
			get { return servicePoint; }
		}
		
		public override int Timeout { 
			get { return timeout; }
			set { timeout = value; }
		}
		
		public string TransferEncoding {
			get { return webHeaders ["Transfer-Encoding"]; }
			set {
				CheckRequestStarted ();
				if (!sendChunked)
					throw new InvalidOperationException ("SendChunked must be True");
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower ();
				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Transfer-Encoding");
					return;
				}
				if (val == "chunked")
					throw new ArgumentException ("Cannot set value to Chunked");
				webHeaders.SetInternal ("Transfer-Encoding", value);
			}
		}
		
		public string UserAgent {
			get { return webHeaders ["User-Agent"]; }
			set { webHeaders.SetInternal ("User-Agent", value); }
		}
				
		// Methods
		
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
		
		private delegate Stream GetRequestStreamCallback ();
		private delegate WebResponse GetResponseCallback ();
		
		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (method == null || (!method.Equals ("PUT") && !method.Equals ("POST")))
				throw new ProtocolViolationException ("Cannot send file when method is: " + this.method + ". Method must be PUT.");
			// workaround for bug 24943
			Exception e = null;
			lock (this) {
				if (asyncResponding || webResponse != null)
					e = new InvalidOperationException ("This operation cannot be performed after the request has been submitted.");
				else if (requesting)
					e = new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				else
					requesting = true;
			}
			if (e != null)
				throw e;
			/*
			lock (this) {
				if (asyncResponding || webResponse != null)
					throw new InvalidOperationException ("This operation cannot be performed after the request has been submitted.");
				if (requesting)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				requesting = true;
			}
			*/
			GetRequestStreamCallback c = new GetRequestStreamCallback (this.GetRequestStreamInternal);
			return c.BeginInvoke (callback, state);	
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();				
			AsyncResult async = (AsyncResult) asyncResult;
			GetRequestStreamCallback cb = (GetRequestStreamCallback) async.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}
		
		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			if (!(asyncResult.AsyncWaitHandle.WaitOne (timeout, false))) {
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}
			return EndGetRequestStream (asyncResult);
		}
		
		internal Stream GetRequestStreamInternal ()
		{
		        if (this.requestStream == null) {
				this.requestStream = new HttpWebRequestStream (
					this, GetSocket (), new EventHandler (OnClose));
			}

			return this.requestStream;
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			// workaround for bug 24943
			Exception e = null;
			lock (this) {
				if (asyncResponding)
					e = new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				else 
					asyncResponding = true;
			}
			if (e != null)
				throw e;
			/*
			lock (this) {
				if (asyncResponding)
					throw new InvalidOperationException ("Cannot re-call start of asynchronous method while a previous call is still in progress.");
				asyncResponding = true;
			}
			*/
			GetResponseCallback c = new GetResponseCallback (this.GetResponseInternal);
			return c.BeginInvoke (callback, state);
		}
		
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();			
			AsyncResult async = (AsyncResult) asyncResult;
			GetResponseCallback cb = (GetResponseCallback) async.AsyncDelegate;
			WebResponse webResponse = cb.EndInvoke(asyncResult);
			asyncResponding = false;
			if (webResponse != null) {
				HttpStatusCode code = ((HttpWebResponse) webResponse).StatusCode;
				if (code >= HttpStatusCode.MultipleChoices) {
					string msg = String.Format ("The remote server returned an error: ({0}) {1}",
						(int) code, code);
					
					throw new WebException (
						msg,
						null,
						WebExceptionStatus.ProtocolError,
						webResponse);
				}
			}
			
			return webResponse;
		}
		
		public override WebResponse GetResponse()
		{
			IAsyncResult asyncResult = BeginGetResponse (null, null);
			if (!(asyncResult.AsyncWaitHandle.WaitOne (timeout, false))) {
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}
			return EndGetResponse (asyncResult);
		}
		
		WebResponse GetResponseInternal ()
		{
			if (webResponse != null)
				return webResponse;			

			GetRequestStreamInternal (); // Make sure we do the request

			webResponse = new HttpWebResponse (this.actualUri,
							   method,
							   GetSocket (),
							   timeout,
							   new EventHandler (OnClose));
 			return webResponse;
		}

		[MonoTODO]		
		public override void Abort()
		{
			this.haveResponse = true;
			throw new NotImplementedException ();
		}		
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		~HttpWebRequest ()
		{
			Dispose (false);
		}		
		
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			disposed = true;
			if (disposing) {
				Close ();
			}

			requestStream = null;
			webResponse = null;
			socket = null;
		}
		
		// Private Methods
		
		private void CheckRequestStarted () 
		{
			if (requesting)
				throw new InvalidOperationException ("request started");
		}
		
		internal void Close ()
		{
			if (requestStream != null)
			 	requestStream.Close ();

			if (webResponse != null)
			 	webResponse.Close ();

			lock (this) {			
				requesting = false;
				if (requestEndEvent != null) 
					requestEndEvent.Set ();
				// requestEndEvent = null;
			}
		}
		
		void OnClose (object sender, EventArgs args)
		{
			if (sender == requestStream)
				requestClosed = true;
			else if (sender == webResponse)
				responseClosed = true;

			// TODO: if both have been used, wait until both are Closed. That's what MS does.
			if (requestClosed && responseClosed && socket != null && socket.Connected)
				socket.Close ();
		}
		
		Socket GetSocket ()
		{
			if (socket != null)
				return socket;

			IPAddress hostAddr = Dns.Resolve (actualUri.Host).AddressList[0];
			IPEndPoint endPoint = new IPEndPoint (hostAddr, actualUri.Port);
			socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			socket.Connect (endPoint);
			if (!socket.Poll (timeout, SelectMode.SelectWrite))
				throw new WebException("Connection timed out.", WebExceptionStatus.Timeout);

			return socket;
		}
		
		// Private Classes
		
		// to catch the Close called on the NetworkStream
		internal class HttpWebRequestStream : NetworkStream
		{
			bool disposed;
			EventHandler onClose;
			ArrayList AccumulateOutput;
			HttpWebRequest webRequest;
			
			internal HttpWebRequestStream (HttpWebRequest webRequest,
						       Socket socket,
						       EventHandler onClose)
				: base (socket, FileAccess.Write, false)
			{
				this.onClose = onClose;
				this.webRequest = webRequest;

				bool need_content_length = false;
				if (webRequest.method == "POST" || webRequest.method == "HEAD")
					need_content_length = true;

				long content_length = webRequest.ContentLength;				
				if (need_content_length && content_length == -1){
					//
					// Turn on accumulation mode
					//
					AccumulateOutput = new ArrayList ();
				} else {
					if (!socket.Poll (webRequest.Timeout, SelectMode.SelectWrite))
						throw new WebException("The request timed out", WebExceptionStatus.Timeout);
					WriteHeaders (content_length, false);
				}
				

				// FIXME: write cookie headers (CookieContainer not yet implemented)
			}

			//
			// This can be invoked at two points: at the startup of the stream,
			// or after we accumulated data.
			//
			// At startup we are complete (ContentLength specified), so we use
			// the information from the headers.
			void WriteHeaders (long content_length, bool add_manually_cl)
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append (webRequest.Method + " " + 
					   webRequest.actualUri.PathAndQuery +
					   " HTTP/" +
					   webRequest.version.ToString (2) +
					   "\r\n");
				
				foreach (string header in webRequest.webHeaders) 
					sb.Append (header + ": " + webRequest.webHeaders[header] + "\r\n");

				//
				// we do not use the parent property, because that property tracks
				// the state, and disallows changes after creation
				//
				if (add_manually_cl)
					sb.Append ("Content-Length: " + content_length + "\r\n");
				
				sb.Append ("\r\n");
				byte [] bytes = Encoding.ASCII.GetBytes (sb.ToString ());
				this.Write (bytes, 0, bytes.Length);
			}

			public override void Write (byte [] buffer, int offset, int count)
			{
				if (AccumulateOutput == null){
					base.Write (buffer, offset, count);
				} else {
					if (buffer == null)
						throw new ArgumentNullException ();
					if (offset < 0)
						throw new ArgumentOutOfRangeException ("offset into buffer is negative");
					if (count < 0)
						throw new ArgumentOutOfRangeException ("count is negative");
				        if (disposed)
						throw new ObjectDisposedException ("stream has been disposed");
					if (offset + count > buffer.Length)
						throw new ArgumentException ("Write beyond end of buffer requested");

					byte [] copy = new byte [count];
					Array.Copy (buffer, offset, copy, 0, count);
					AccumulateOutput.Add (copy);
				}
			}

			public override void Flush ()
			{
				if (AccumulateOutput == null)
					base.Flush ();
			}

			protected override void Dispose (bool disposing)
			{
				if (AccumulateOutput != null)
					Flush ();
				
				if (disposed)
					return;

				disposed = true;
				if (disposing) {
					/* This does not work !??
					if (socket.Connected)
						socket.Shutdown (SocketShutdown.Send);
					*/
					if (onClose != null)
						onClose (this, EventArgs.Empty);
				}

				onClose = null;
				base.Dispose (disposing);
			}
			
			public override void Close() 
			{

				if (AccumulateOutput != null){
					ArrayList output = AccumulateOutput;
					AccumulateOutput = null;
					long size = 0;
					foreach (byte [] b in output)
						size += b.Length;
					WriteHeaders (size, true);
					foreach (byte [] b in output){
						base.Write (b, 0, b.Length);
					}
					AccumulateOutput = null;
					base.Flush ();
				}
					       
				GC.SuppressFinalize (this);
				Dispose (true);
			}
		}		
	}
}

