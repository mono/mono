//
// System.Net.HttpWebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace System.Net 
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
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
		
		private bool requestStarted = false;
		
		// Constructors
		
		internal HttpWebRequest (Uri uri) 
		{ 
			this.requestUri = uri;
			this.actualUri = uri;
			this.webHeaders = new WebHeaderCollection (true);
			// this.webHeaders.SetInternal ("Host", uri.Authority);
			// this.webHeaders.SetInternal ("Date", DateTime.Now.ToUniversalTime ().ToString ("r", null));
			// this.webHeaders.SetInternal ("Expect", "100-continue");
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
				webHeaders = newHeaders;
			}
		}
		
		public DateTime IfModifiedSince {
			get { 
				string str = webHeaders ["If-Modified-Since"];
				if (str == null)
					return DateTime.Now;
					
				// TODO:
				// HTTP-date    = rfc1123-date | rfc850-date | asctime-date
				// for now, assume rfc1123 only
				try {
					DateTime dt = DateTime.ParseExact (str, "r", null);
					return dt;
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
				
				value = value.ToUpper ();
				if (value == null ||
				    (value != "GET" &&
				     value != "HEADER" &&
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

		[MonoTODO]		
		public override void Abort()
		{
			throw new NotImplementedException ();
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
			webHeaders.SetInternal ("Range", value + range);	
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
		
		[MonoTODO]
		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (!method.Equals ("PUT") && !method.Equals ("POST"))
				throw new ProtocolViolationException ("Invalid method");
//			if (putting)
//				throw new WebException ("Stream already open");
/*				
                        HttpWebRequestAsyncResult requestResult = new HttpWebRequestAsyncResult (stateObject);				
                        Worker worker = new Worker (hostName, requestCallback, requestResult);
                        Thread child = new Thread (new ThreadStart (worker.GetRequestStream));
                        child.Start();
                        return requestResult;				
*/
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		[MonoTODO]
		public override Stream GetRequestStream()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override WebResponse GetResponse()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotImplementedException ();
		}
		
		// Private Methods
		
		private void CheckRequestStarted () 
		{
			if (requestStarted)
				throw new InvalidOperationException ("request started");
		}


		// Private Classes
	}
}