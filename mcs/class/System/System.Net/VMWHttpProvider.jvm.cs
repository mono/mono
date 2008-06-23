using System;
using System.IO;
using System.Threading;

using mainsoft.apache.commons.httpclient;
using mainsoft.apache.commons.httpclient.methods;
using mainsoft.apache.commons.httpclient.@params;
using mainsoft.apache.commons.httpclient.auth;
using mainsoft.apache.commons.httpclient.auth.negotiate;
using javax.security.auth;
using org.ietf.jgss;
using java.security;
using System.Collections.Specialized;
using System.Collections;
using mainsoft.apache.commons.httpclient.cookie;

namespace System.Net
{
	/// <summary>
	/// Summary description for VMWHttpProvider.
	/// </summary>
	internal class VMWHttpProvider : HttpProvider
	{
		protected static HttpClient _sclient;
		protected static HttpStateCache _stateCache = new HttpStateCache();

		protected static object LOCK_OBJECT = new object();
		
		protected HttpClient _client;
		protected bool _disableHttpConnectionPooling = false;

		protected HttpMethod _method;
		protected HttpState _state;
		protected HostConfiguration _hostConfig;
		
		protected HttpWebResponse _response;
		protected bool _hasResponse;
		protected bool _hasRequest;
		protected Stream _writeStream;
		private GHWebAsyncResult _asyncWrite;		

		private bool _isConnectionOpened;
		
		static VMWHttpProvider()
		{
			if(java.lang.System.getProperty("mainsoft.apache.commons.logging.Log") == null)
				java.lang.System.setProperty("mainsoft.apache.commons.logging.Log",
					"mainsoft.apache.commons.logging.impl.SimpleLog");
			if(java.lang.System.getProperty("mainsoft.apache.commons.logging.simplelog.showdatetime") == null)
				java.lang.System.setProperty("mainsoft.apache.commons.logging.simplelog.showdatetime",
					"true");
			if(java.lang.System.getProperty("mainsoft.apache.commons.logging.simplelog.log.httpclient.wire") == null)
				java.lang.System.setProperty("mainsoft.apache.commons.logging.simplelog.log.httpclient.wire",
					"error");
			if(java.lang.System.getProperty("mainsoft.apache.commons.logging.simplelog.log.mainsoft.apache.commons.httpclient")
				== null)
				java.lang.System.setProperty("mainsoft.apache.commons.logging.simplelog.log.mainsoft.apache.commons.httpclient",
					"error");
			if(java.lang.System.getProperty("mainsoft.apache.commons.logging.simplelog.log.httpclient.wire.header")
				== null)
				java.lang.System.setProperty("mainsoft.apache.commons.logging.simplelog.log.httpclient.wire.header", 
					"error");

		}
		public VMWHttpProvider(Uri uri) : base (uri)
		{
			string s = System.Configuration.ConfigurationSettings.AppSettings["disableHttpConnectionPooling"];
			if (s != null) 
			{
				_disableHttpConnectionPooling = bool.Parse(s);
			}
			InitDefaultCredentialsProvider ();
			InitSPNProviders ();
		}

		internal override ServicePoint ServicePoint
		{
			get {throw new NotImplementedException();}
		}



		public override bool IsRequestStarted()
		{
			if(_method == null)
				return false;
			return _method.isRequestSent();
		}

		public override Uri GetAddress()
		{
			if(_method == null)
				return GetOriginalAddress();
			mainsoft.apache.commons.httpclient.URI javaURI =  _method.getURI();
			return new Uri(javaURI.ToString());
		}

		public override bool IsHaveResponse()
		{
			return _hasResponse;
		}

		private void SetJavaCredential(NetworkCredential nc, string type)
		{
			SetJavaCredential(nc, type, false);
		}

		private void SetJavaCredential(NetworkCredential nc, string type, bool proxyCredentials)
		{
			string host = null;
			
			if(!proxyCredentials)
				host = GetOriginalAddress().Host;
			else
				host = ((WebProxy)this.Proxy).Address.Host;

			string domain = (nc.Domain == null) ? host : nc.Domain;

			if(String.Compare (type, "any", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				if(!proxyCredentials)
				{
					_state.setCredentials(AuthScope.ANY,
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
					_state.setCredentials(new AuthScope(AuthScope.ANY_HOST, AuthScope.ANY_PORT, AuthScope.ANY_REALM, "Ntlm"),
						new NTCredentials(nc.UserName, nc.Password, host, domain));
				}
				else
				{
					_state.setProxyCredentials(AuthScope.ANY,
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
					_state.setProxyCredentials(new AuthScope(AuthScope.ANY_HOST, AuthScope.ANY_PORT, AuthScope.ANY_REALM, "Ntlm"),
						new NTCredentials(nc.UserName, nc.Password, host, domain));
				}
			}
			else if(String.Compare (type, "basic", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				if(!proxyCredentials)
				{
					_state.setCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "basic"),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
				else
				{
					_state.setProxyCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "basic"),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
			}
			else if(String.Compare (type, "digest", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				if(!proxyCredentials)
				{
					_state.setCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "digest"),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
				else
				{
					_state.setProxyCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "digest"),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
			}
			else if(String.Compare (type, "ntlm", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				if(!proxyCredentials)
				{
					_state.setCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "ntlm"),
						new NTCredentials(nc.UserName, nc.Password, host, domain));
				}
				else
				{
					_state.setProxyCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, "ntlm"),
						new NTCredentials(nc.UserName, nc.Password, host, domain));
				}
			}
			else if(String.Compare (type, "negotiate", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				SetAuthenticationScheme (AuthPolicy.NEGOTIATE);
			}
			else
			{
				if(!proxyCredentials)
				{
					_state.setCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, type),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
				else
				{
					_state.setProxyCredentials(new AuthScope(AuthScope.ANY_HOST,
						AuthScope.ANY_PORT, AuthScope.ANY_REALM, type),
						new UsernamePasswordCredentials(nc.UserName, nc.Password));
				}
			}						
		}

		private void SetAuthenticationScheme (string type) {			
			_method.getHostAuthState ().setAuthScheme (AuthPolicy.getAuthScheme (type));								
			if (type != null && type.ToLower () == AuthPolicy.NEGOTIATE.ToLower ()) {					
				_method.getParams ().setParameter (CredentialsProvider__Finals.PROVIDER, new HTTPClientCredentialsBridge( DefaultCredentialsProvider));
				_method.getParams ().setParameter (NegotiateScheme.SPN_LIST_PARAM, SPNProviders);
			}			
		}

		private java.util.ArrayList SPNProviders {
			get {
				return (java.util.ArrayList) AppDomain.CurrentDomain.GetData ("GH$SPNProviders");
			}
			set {
				AppDomain.CurrentDomain.SetData ("GH$SPNProviders", value);
			}
		}

		private void InitSPNProviders () {
			if (SPNProviders != null)
				return;
			java.util.ArrayList spnProviders = new java.util.ArrayList ();
			NameValueCollection configAttributes = System.Configuration.ConfigurationSettings.AppSettings;
			string providersList = configAttributes ["SPNProviders"];
			if (providersList == null)
				return;
			string[] tokens = providersList.Split (',');
			foreach (string spnClass in tokens) {
				try {
					spnProviders.add (Activator.CreateInstance (Type.GetType (spnClass)));
				}
				catch (Exception) { }
			}
			SPNProviders = spnProviders;
		}

		private vmw.@internal.auth.CredentialsProvider DefaultCredentialsProvider {
			get {
				return (vmw.@internal.auth.CredentialsProvider) AppDomain.CurrentDomain.GetData ("GH$DefaultCredentialsProvider");
			}
			set {
				AppDomain.CurrentDomain.SetData ("GH$DefaultCredentialsProvider", value);
			}
		}

		private void InitDefaultCredentialsProvider () {
			if (DefaultCredentialsProvider != null)
				return;
			vmw.@internal.auth.CredentialsProvider defaultProvider = null;
			NameValueCollection configAttributes = System.Configuration.ConfigurationSettings.AppSettings;
			
			string defaultProviderClass = configAttributes ["DefaultCredentialsProvider"];
			if (defaultProviderClass != null) {
				try {					
					defaultProvider = (vmw.@internal.auth.CredentialsProvider)
						Activator.CreateInstance (Type.GetType (defaultProviderClass));
				}
				catch (Exception e) {
					Console.WriteLine ("Failed to initialize Credentials Provider: " + defaultProviderClass + " Message: " + e.Message);					
				}
			}			

			if (defaultProvider == null) 
				defaultProvider = new vmw.@internal.auth.SubjectCredentialsPrvider ();

			defaultProvider.init (ConvertToTable (configAttributes));
			DefaultCredentialsProvider = defaultProvider;
		}

		private java.util.Properties ConvertToTable (NameValueCollection col) {
			java.util.Properties table = new java.util.Properties ();
			foreach (String key in col.Keys)
				table.put (key, col [key]);
			return table;
		}

		private void InitProxyCredentials () {
			if (this.Proxy == null)
				return;

			if (!(this.Proxy is WebProxy))
				return;

			WebProxy proxy = (WebProxy) this.Proxy;
			ICredentials creds = proxy.Credentials;

			if(creds == null)
				return;

			if(creds is CredentialCache)
			{
				string type = "basic";
				NetworkCredential nc = ((CredentialCache)creds).GetCredential(proxy.Address, "basic");
				if(nc == null)
				{
					type = "digest";
					nc = ((CredentialCache)creds).GetCredential(proxy.Address, "digest");
					if(nc == null)
					{
						type = "ntlm";
						nc = ((CredentialCache)creds).GetCredential(proxy.Address, "ntlm");
						if (nc == null) {
							nc = ((CredentialCache) _credentials).GetCredential (GetOriginalAddress (), "negotiate");
							type = "negotiate";
						}
					}
				}
				if(nc != null)
				{
					SetJavaCredential(nc, type, true);
				}
			}
			else if (creds is NetworkCredential)
			{
				SetJavaCredential((NetworkCredential)creds, "any", true);
			}

			_method.setDoAuthentication(true);
		}

		private void InitCredentials()
		{
			if(_credentials == null)
				return;
			if (_credentials == CredentialCache.DefaultCredentials) {
				SetAuthenticationScheme (AuthPolicy.NEGOTIATE);
			}
			else if (_credentials is CredentialCache) {
				NetworkCredential nc = ((CredentialCache) _credentials).GetCredential (GetOriginalAddress (), "basic");
				string type = "basic";
				if(nc == null)
				{
					nc = ((CredentialCache)_credentials).GetCredential(GetOriginalAddress(), "digest");
					type = "digest";
					if(nc == null)
					{
						nc = ((CredentialCache)_credentials).GetCredential(GetOriginalAddress(), "ntlm");
						type = "ntlm";
						if (nc == null) {
							nc = ((CredentialCache) _credentials).GetCredential (GetOriginalAddress (), "negotiate");
							type = "negotiate";
						}
					}
				}
				if(nc != null)
				{
					SetJavaCredential(nc, type);
				}
			}
			else if(_credentials is NetworkCredential)
			{
				SetJavaCredential((NetworkCredential)_credentials, "any");
			}

			_method.setDoAuthentication(true);
		}

		private void InitHostConfig()
		{
			if (this.Proxy == null || this.Proxy == WebRequest.DefaultWebProxy)
				return;
			if(this.Proxy.IsBypassed(GetOriginalAddress()))
				return;

			_hostConfig = new HostConfiguration();			
			_hostConfig.setHost(new HttpHost(_method.getURI()));

			
			if(this.Proxy is WebProxy)
			{
				WebProxy wp = (WebProxy) this.Proxy;
				_hostConfig.setProxyHost(new ProxyHost(wp.Address.Host, wp.Address.Port));
			}
			else
				throw new NotImplementedException("Cannot accept Proxy which is not System.Net.WebProxy instance");

			
		}

		private void SetConnectionHeader(string val)
		{
			string connectionHeader = (this.Proxy != null) ? "Proxy-Connection" : "Connection";
			Headers.RemoveInternal ((this.Proxy != null) ? "Proxy-Connection" : "Connection");
			
			if(val != null)
				_method.setRequestHeader(connectionHeader, val);

			if (_keepAlive) 
			{
				_method.addRequestHeader (connectionHeader, "keep-alive");
				Headers.SetInternal(connectionHeader,"keep-alive");
			}
			else if (!_keepAlive && _version == HttpVersion.Version11) 
			{
				_method.addRequestHeader (connectionHeader, "close");
				Headers.SetInternal(connectionHeader,"close");
			}

		}
		private bool OpenConnection()
		{
			lock(this)
			{
				if(_isConnectionOpened)
					return false;
				_isConnectionOpened = true;
			}
			InitClient();
			InitMethod();

			_state = _stateCache.GetHttpState();

			//todo insert needed Authontication, Cookies info to state!
			_method.setDoAuthentication(this.PreAuthenticate);
			
			InitHostConfig();
			InitCredentials();
			InitProxyCredentials();
			
			if(this.ProtocolVersion	== HttpVersion.Version11)
				_method.getParams().setVersion(mainsoft.apache.commons.httpclient.HttpVersion.HTTP_1_1);
			else if(ProtocolVersion == HttpVersion.Version10)
				_method.getParams().setVersion(mainsoft.apache.commons.httpclient.HttpVersion.HTTP_1_0);
			else 
				throw new ProtocolViolationException("Unsupported protocol version: " + ProtocolVersion);

			if(!(_method is mainsoft.apache.commons.httpclient.methods.EntityEnclosingMethod))
			{
				_method.setFollowRedirects(this.AllowAutoRedirect);
			}
			else
			{
				if(!AllowWriteStreamBuffering && _contentLength < 0 && !SendChunked)
					throw new ProtocolViolationException();
				if(SendChunked)
					((EntityEnclosingMethod)_method).setContentChunked(SendChunked);				
			}
			if(MaxAutoRedirections != _defaultMaxRedirectsNum)
			{
				_method.getParams().setParameter(HttpClientParams.MAX_REDIRECTS,
					new java.lang.Integer(MaxAutoRedirections));
			}
			
			
			
			foreach(string k in Headers)
			{	
				if(String.Compare (k, "connection", StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;
				string val = Headers[k];
				val = (val == null) ? "" : val;
				_method.setRequestHeader(k, val);
			}

			if (this.CookieContainer != null) 
			{
				string cookieHeader = this.CookieContainer.GetCookieHeader (this.GetOriginalAddress());
				if (cookieHeader != "")
					_method.setRequestHeader("Cookie", cookieHeader);
			}
			SetConnectionHeader(Headers["Connection"]);
			
			_method.getParams().setSoTimeout(ReadWriteTimeout);

			return true;
			
		}

		private void InitClient()
		{
			lock(LOCK_OBJECT)
			{
				if((!_disableHttpConnectionPooling) && (_client == null))
				{
					_client = _sclient;
				}
				if(_client == null)
				{
					mainsoft.apache.commons.httpclient.MultiThreadedHttpConnectionManager manager =
						new mainsoft.apache.commons.httpclient.MultiThreadedHttpConnectionManager();
					manager.setConnectionStaleCheckingEnabled(false);
					manager.setMaxTotalConnections(200);
					//by some reasons RFC something - the default 
					//value will be 2 , so we need to change it ...
					manager.setMaxConnectionsPerHost(20);
					_client = new HttpClient(manager);
					_client.getParams().setIntParameter(HttpClientParams.MAX_REDIRECTS, _defaultMaxRedirectsNum);
					_client.getParams().setParameter(HttpClientParams.ALLOW_CIRCULAR_REDIRECTS, new java.lang.Boolean(true));
					_client.getParams().setParameter(HttpClientParams.CONNECTION_MANAGER_TIMEOUT, new java.lang.Long(30000));
					_client.getParams().setParameter(HttpClientParams.USER_AGENT, 
							"VMW4J HttpClient (based on Jakarta Commons HttpClient)");
					_client.getParams ().setBooleanParameter (HttpClientParams.SINGLE_COOKIE_HEADER, true);
					java.util.ArrayList schemas = new java.util.ArrayList ();
					schemas.add ("Ntlm");
					schemas.add ("Digest");
					schemas.add ("Basic");
					schemas.add ("Negotiate");
					_client.getParams ().setParameter (AuthPolicy.AUTH_SCHEME_PRIORITY, schemas);
					if (!_disableHttpConnectionPooling) {
						_sclient = _client;
					}
				}
			}
		}

		private void InitMethod()
		{
			lock(this)
			{
				if(_method == null)
				{
					string uriString = this.GetOriginalAddress().AbsoluteUri;

					if(this.MethodName == null || this.MethodName == "")
					{
						this.MethodName = "GET";
					}
			
					string name = this.MethodName.ToUpper().Trim();

					switch(name)
					{
						case "GET" : _method = new GetMethod(uriString); break;
						case "PUT" : _method = new PutMethod(uriString);
							if (ServicePointManager.Expect100Continue)
								_method.getParams ().setBooleanParameter (HttpMethodParams.USE_EXPECT_CONTINUE, true);
							break;
						case "POST": _method = new PostMethod(uriString);
							if (ServicePointManager.Expect100Continue)
								_method.getParams ().setBooleanParameter (HttpMethodParams.USE_EXPECT_CONTINUE, true);
							break;
						case "HEAD": _method = new HeadMethod(uriString); break;
						case "TRACE": _method = new TraceMethod(uriString);break;
						case "DELETE": _method = new DeleteMethod(uriString);break;
						case "OPTIONS": _method = new OptionsMethod(uriString);break;
						default: _method = new GenericMethod(uriString, MethodName); break;
					}
				}
			}
		}

		private void InitHostConfiguration()
		{
			lock(this)
			{
				if(_hostConfig == null)
				{
					_hostConfig = new HostConfiguration();
				}
			}
		}

		

		public override Stream GetRequestStream()
		{
			bool isPutPost = String.Compare("post", MethodName, true) == 0 
				|| String.Compare("put", MethodName, true) == 0;
			if(!isPutPost)
				throw new ProtocolViolationException();
			lock(this)
			{
				if (_isAborted)
					throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);
				if(_writeStream != null)
					return _writeStream;
				this.OpenConnection();

				//java.io.PipedInputStream inJavaStream = new java.io.PipedInputStream();
				//java.io.PipedOutputStream outJavaStream = new java.io.PipedOutputStream(inJavaStream);
							
				long contLen = _contentLength;
				
				OutputStreamRequestEntity reqEntity = new OutputStreamRequestEntity(contLen);

				_writeStream = new VMWRequestStream(reqEntity, contLen);
			
				EntityEnclosingMethod method = (EntityEnclosingMethod)_method;
				if(AllowWriteStreamBuffering )
					method.setRequestEntity(reqEntity);
				else if(!AllowWriteStreamBuffering && contLen < 0 && !SendChunked)
					throw new ProtocolViolationException();
				else
					method.setRequestEntity(reqEntity);
			
				_hasRequest = true;
				return _writeStream;
			}
		}
		private static bool isRedirectNeeded(HttpMethod method)
		{
			switch (method.getStatusCode()) 
			{
				case 302:
				case 301:
				case 303:
				case 307:
					return true;
				default:
					return false;
			} //end of switch
		}

		private void synchHeaders()
		{
			foreach(string k in Headers)
			{
				if (String.Compare (k, "connection", StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;
				string val = Headers[k];
				val = (val == null) ? "" : val;
				_method.setRequestHeader(k, val);
			}
		}

		delegate WebResponse GetResponseDelegate();
		private sealed class AsyncContext
		{
			public readonly AsyncCallback AsyncCallback;
			public readonly Delegate AsyncDelegate;
			public readonly object AsyncState;
			public readonly DelegateAsyncResult DelegateAsyncResult;

			public AsyncContext (Delegate @delegate, DelegateAsyncResult delegateAsyncResult, AsyncCallback asyncCallback, object userState) {
				AsyncDelegate = @delegate;
				AsyncCallback = asyncCallback;
				AsyncState = userState;
				DelegateAsyncResult = delegateAsyncResult;
			}
		}
		private sealed class DelegateAsyncResult : IAsyncResult
		{
			
			IAsyncResult _asyncResult;

			public IAsyncResult AsyncResult {
				get { return _asyncResult; }
				set { _asyncResult = value; }
			}

			AsyncContext AsyncContext {
				get { return (AsyncContext) _asyncResult.AsyncState; }
			}

			public static void Callback (IAsyncResult result) {
				AsyncContext context = (AsyncContext) result.AsyncState;
				context.AsyncCallback.Invoke (context.DelegateAsyncResult);
			}

			public Delegate AsyncDelegate {
				get { return AsyncContext.AsyncDelegate; }
			}

			#region IAsyncResult Members

			public object AsyncState {
				get { return AsyncContext.AsyncState; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return _asyncResult.AsyncWaitHandle; }
			}

			public bool CompletedSynchronously {
				get { return _asyncResult.CompletedSynchronously; }
			}

			public bool IsCompleted {
				get { return _asyncResult.IsCompleted; }
			}

			#endregion
		}
		
		WebResponse GetAsyncResponse()
		{
			try {
				return GetResponse ();
			}
			catch {
				return null;
			}
		}

		public override WebResponse GetResponse()
		{
			lock(this)
			{
				if (_isAborted)
					throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);
				if(!_isConnectionOpened)
					OpenConnection();
				if(_response == null)
				{
					try
					{	
						synchHeaders();
						InternalExecuteMethod ();						
						int numOfRedirects = 0;
						while (isRedirectNeeded (_method) && _allowAutoRedirect && numOfRedirects < MaxAutoRedirections) {
							if (!HandleManualyRedirect ())
								break;
							numOfRedirects++;
						}
						
						//todo right place to re-put all headers again...
						mainsoft.apache.commons.httpclient.Header hostHeader =
							_method.getRequestHeader("Host");
						if(hostHeader != null)
							Headers.SetInternal("Host", hostHeader.getValue());

						_response = new HttpWebResponse(_method, _state, _stateCache, GetAddress(), this.MethodName);
						
						if(_response != null && 
							_response.Cookies != null && 
							_response.Cookies.Count > 0)
						{
							if(CookieContainer != null)
							{
								foreach(Cookie cooky in _response.Cookies)
								{
									CookieContainer.Add(GetAddress(), cooky);
								}
							}
						}

						_hasResponse = true;
						int respCodeAsInt = (int) _response.StatusCode;
						if(respCodeAsInt >= 400)
						{
							// The WebException contains the readable (not closed) response stream.
							// So, in case of WebException, we should read all data from the 
							// network response stream into the memory stream, and after that
							// close the underlying network stream. The following requests to read
							// from the stream will actually read from the memory stream.
							// So, the this.Abort() should not be called in this case.
							_response.ReadAllAndClose();
							//this.Abort();
							throw new WebException("The remote server returned an error: (" + respCodeAsInt +") " +_response.StatusCode, null, WebExceptionStatus.ProtocolError, _response);
						}
						Header location = _method.getResponseHeader ("location");
						if (isRedirectNeeded (_method) && location == null && _method.getFollowRedirects ())
						{
							// See comments above for the error >= 400
							_response.ReadAllAndClose();
							//this.Abort();
							throw new WebException("Got response code "+_response.StatusCode+", but no location provided", null, WebExceptionStatus.ProtocolError, _response);
						}
					}
					catch(ProtocolException e)
					{
						throw new WebException("", e);
					}
					catch(java.net.ConnectException e)
					{
						throw new WebException("Unable to connect to the remote server.", e);
					}
					catch(java.net.SocketTimeoutException e)
					{
						throw new WebException("Timeout exceeded", e);
					}
					catch(java.io.IOException e)
					{
						throw new WebException("", e);
					}
				}
				return _response;
			}

		}

		private void InternalExecuteMethod () {
			_client.executeMethod (_hostConfig, _method, _state);			
		}		

		private bool HandleManualyRedirect () {			
			Header redirectHeader = _method.getResponseHeader ("location");
			if (redirectHeader == null) {
				// See comments above for the error >= 400
				_response.ReadAllAndClose ();
				//this.Abort();
				throw new WebException ("Got response code " + _response.StatusCode + ", but no location provided", null, WebExceptionStatus.ProtocolError, _response);
			}

			mainsoft.apache.commons.httpclient.HttpMethod originalMethod = _method;
			try {
				string location = redirectHeader.getValue ();
				URI currentUri = _method.getURI ();
				URI redirectUri = null;

				redirectUri = new URI (location, true);
				if (redirectUri.isRelativeURI ()) {
					//location is incomplete, use current values for defaults	
					redirectUri = new URI (currentUri, redirectUri);
				}
				
				_method = new GetMethod ();
				foreach(Header h in originalMethod.getRequestHeaders())
					_method.addRequestHeader(h);				
				_method.setURI (redirectUri);				
				InternalExecuteMethod ();
				return true;
			}
			catch (URIException e) {
				_method = originalMethod;
				return false;
			}
		}

		public override void Abort()
		{
			lock (this) {
				if (_isAborted)
					return;
				_isAborted = true;
				try {
					if (_hasResponse) {
						_response.Close ();
					}
				}
				finally {
					if (_method != null)
						_method.releaseConnection ();
					_method = null;
					_hasResponse = false;
					_response = null;
				}
			}
		}

		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			lock(this)
			{
				if(_asyncWrite != null)
				{
					throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
						"method while a previous call is still in progress.");
				}
	
				_asyncWrite = new GHWebAsyncResult (this, callback, state);
				if (_hasRequest) 
				{
					if (_writeStream != null) 
					{
						_asyncWrite.SetCompleted (true, _writeStream);
						_asyncWrite.DoCallback ();
						return _asyncWrite;
					}
				}
				
				
				try
				{
					this.GetRequestStream();					
				}
				catch(Exception e)
				{
					_asyncWrite.SetCompleted(false, e);
					_asyncWrite.DoCallback ();
					return _asyncWrite;
				}

				_asyncWrite.SetCompleted (true, _writeStream);
				_asyncWrite.DoCallback ();
				return _asyncWrite;
				
			}
		}   

		public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			GHWebAsyncResult result = asyncResult as GHWebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			_asyncWrite = result;

			result.WaitUntilComplete ();

			Exception e = result.Exception;
			
			if (e != null)
				throw e;

			return result.WriteStream;
		}

		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			GetResponseDelegate d = new GetResponseDelegate (GetAsyncResponse);
			DelegateAsyncResult result = new DelegateAsyncResult ();
			AsyncContext userContext = new AsyncContext (d, result, callback, state);
			result.AsyncResult = d.BeginInvoke (new AsyncCallback (DelegateAsyncResult.Callback), userContext);
			return result;
		}

		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			if (_isAborted)
				throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			DelegateAsyncResult result = asyncResult as DelegateAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			return ((GetResponseDelegate) result.AsyncDelegate).EndInvoke (result.AsyncResult);
		}







		




		#region VMWRequestStream class
		internal class VMWRequestStream : Stream, IDisposable
		{

			private java.io.OutputStream _javaOutput;
			private long _len;
			private long _contentLength;

			internal VMWRequestStream (java.io.OutputStream stream) :
				this(stream , -1L)
			{
			}

			internal VMWRequestStream (java.io.OutputStream stream, long contentLength)
			{
				_javaOutput = stream;
				_contentLength = contentLength;
				_len = 0;
			}
			public override bool CanRead
			{
				get    {return false;}
			}

			public override bool CanWrite
			{
				get{return true;}
			}

			public override bool CanSeek
			{
				get { return false;}
			}

			public override long Length
			{
				get{ return _len;}
			}

			public override long Position
			{
				get
				{
					return _len;
				}

				set
				{
					throw new NotSupportedException();
				}
			}

			private volatile bool _closed = false;

			public override void Close()
			{
				if(!_closed)
				{
					lock(this)
					{
						if(!_closed)
						{
							try {
								_closed = true;
								_javaOutput.close ();
							}
							catch (Exception e) {
								throw new WebException ("The request was aborted: The request was canceled.",
								e, WebExceptionStatus.RequestCanceled, null);
							}
						}
					}
				}
			}

			public override void Flush()
			{
				_javaOutput.flush();
			}

			public override int ReadByte()
			{
				throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if(_contentLength >= 0)
				{
					_len += count;
					if(_len > _contentLength)
					{
						throw new  System.Net.ProtocolViolationException(
							"Bytes to be written to the stream exceed Content-Length bytes size specified.");
					}
				}
				_javaOutput.write(vmw.common.TypeUtils.ToSByteArray(buffer), offset, count);

				if(_contentLength == _len)
				{
					_javaOutput.flush();
					_javaOutput.close();
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long length)
			{
				throw new NotSupportedException();
			}

			void IDisposable.Dispose()
			{
				Close ();
			}
		}
		
		#endregion

		#region GHWebAsyncResult
		internal class GHWebAsyncResult  : IAsyncResult
		{
			private object _state;
			private AsyncCallback _callback;
			private ManualResetEvent _handle;
			private bool _isCompleted = false;
			private bool _callbackDone;
			private Stream _writeStream;
			private HttpProvider _provider;

			private Exception _exception;

			#region	  Constructors

			public GHWebAsyncResult(HttpProvider provider, 
				AsyncCallback callback, object state) : 
				this(state, callback)
			{
				_provider = provider;
			}

			public GHWebAsyncResult(object state, AsyncCallback callback)
			{
				_state = state;
				_callback = callback;
			}
			#endregion

			#region IAsyncResult Members

			public object AsyncState
			{
				get
				{
					return _state;
				}
			}

			public bool CompletedSynchronously
			{
				get
				{
					// TODO:  Add HWebAsyncResult.CompletedSynchronously getter implementation
					return false;
				}
			}

			public WaitHandle AsyncWaitHandle 
			{
				get 
				{
					if (_handle == null) 
					{
						lock (this) 
						{
							if (_handle == null)
								_handle = new ManualResetEvent (_isCompleted);
						}
					}
				
					return _handle;
				}
			}

			public bool IsCompleted
			{
				get
				{
					return _isCompleted;	
				}
			}

			#endregion

			#region Internal Properties

			internal Stream WriteStream
			{
				get
				{
					return _writeStream;
				}
			}

			internal Exception Exception
			{
				get
				{
					return _exception;
				}
			}

			internal HttpWebResponse Response
			{
				get
				{
					return ((VMWHttpProvider)_provider)._response;
				}
			}

			#endregion

			#region Internal Methods

			internal void SetCompleted(bool res, Stream writeStream)
			{
				_isCompleted = res;
				_writeStream = writeStream;
				((ManualResetEvent) AsyncWaitHandle).Set ();
			}

			internal void SetCompleted(bool res, Exception exc)
			{
				_isCompleted = res;
				_exception = exc;
				((ManualResetEvent) AsyncWaitHandle).Set ();
			}

			internal void DoCallback()
			{
				if (!_callbackDone && _callback != null) 
				{
					_callbackDone = true;
					_callback (this);
				}
			}

			internal void WaitUntilComplete()
			{
				if(_isCompleted)
					return;
				AsyncWaitHandle.WaitOne ();
			}

			internal bool WaitUntilComplete (int timeout, bool exitContext)
			{
				if (_isCompleted)
					return true;

				return AsyncWaitHandle.WaitOne (timeout, exitContext);
			}
			#endregion

		}

		#endregion

		#region OutputStreamRequestEntity

		internal class OutputStreamRequestEntity : java.io.OutputStream, RequestEntity
		{
			private long _contentLength;
			private java.io.ByteArrayOutputStream _out;
			private sbyte[] _buffer;

			internal OutputStreamRequestEntity(): this(-1)
			{
			}

			internal OutputStreamRequestEntity(long length)
			{
				_contentLength = length;
				int tmp = (int) _contentLength;

				if(tmp <=0)
					tmp = 4096;
				_out = new java.io.ByteArrayOutputStream(tmp);
			}

			#region RequestEntity Members

			public bool isRepeatable()
			{
				return ((_out != null) || (_buffer != null));
			}

			public long getContentLength()
			{
				if(_out != null)
				{
					_buffer = _out.toByteArray();
				}
				if(_buffer != null)
				{
					_contentLength = _buffer.Length;
					_out = null;
				}
				return _contentLength;
			}

			public void writeRequest(java.io.OutputStream output)
			{
				if(_out != null)
					_buffer = _out.toByteArray();
				if(_buffer != null)
				{
					output.write(_buffer, 0, _buffer.Length);
					_out = null;
				}
				else throw new ApplicationException();
			}

			public string getContentType()
			{
				return null;
			}

			#endregion

			public override void write(int i)
			{
				_out.write(i);
			}

			public override void close () 
			{
				int size = _out.size ();
				_out.close ();

				if (size < _contentLength) {
					throw new IOException ("Cannot close stream until all bytes are written.");
				}
			}
		}

		#endregion







	}

	class HTTPClientCredentialsBridge : CredentialsProvider
	{
		private vmw.@internal.auth.CredentialsProvider m_internalProvider;

		public HTTPClientCredentialsBridge (vmw.@internal.auth.CredentialsProvider internalProvider) {
			m_internalProvider = internalProvider;
		}

		public Credentials getCredentials (AuthScheme scheme, string __p2, int __p3, bool __p4) {
			if (scheme.isComplete ())
				return null;			
			GSSCredential creds = m_internalProvider.getCredentials ();			
			return new DelegatedCredentials (creds);
		}
	}
}
