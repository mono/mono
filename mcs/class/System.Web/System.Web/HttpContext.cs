//
// System.Web.HttpContext
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003, 2004 Novell, Inc. (http://www.novell.com)
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
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Util;
using System.Web.SessionState;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace System.Web
{
	public sealed class HttpContext : IServiceProvider
	{
		private ArrayList _arrExceptions;

		private HttpResponse	_oResponse;
		private HttpRequest _oRequest;
		private HttpServerUtility _Server;
		private HttpApplication _oApplication;
		private HttpSessionState _oSession;
		private HttpWorkerRequest _oWorkerRequest;
		private TraceContext _oTrace;
		private IHttpHandler _Handler;
		private IHttpAsyncHandler _AsyncHandler;

		private bool _skipauth;
		private Hashtable		_oItems;
		private DateTime		_oTimestamp;
		int timeoutPossible;
		long timeoutBegin;
		object configTimeout;
		string errorPage;
		IPrincipal user;

		public HttpContext (HttpRequest Request, HttpResponse Response)
		{
			Context = this;

			_arrExceptions = null;
			_oItems = null;
			_oTimestamp = DateTime.Now;
			_oRequest = Request;
			_oResponse = Response;
			_oTrace = new TraceContext (this);
		}

		public HttpContext (HttpWorkerRequest WorkerRequest)
		{
			Context = this;

			_arrExceptions = null;
			_oItems = null;
			_oTimestamp = DateTime.Now;
			_oRequest = new HttpRequest (WorkerRequest, this);
			_oResponse = new HttpResponse (WorkerRequest, this);
			_oWorkerRequest = WorkerRequest;
			_oTrace = new TraceContext (this);
		}

		internal HttpWorkerRequest WorkerRequest
		{
			get {
				return _oWorkerRequest;
			}
		}

		internal static HttpContext Context
		{
			get {
				return (HttpContext) CallContext.GetData ("Context");
			}

			set {
				CallContext.SetData ("Context", value);
			}
		}

		public Exception [] AllErrors
		{
			get {
				if (_arrExceptions == null || _arrExceptions.Count == 0)
					return null;

				return (Exception []) _arrExceptions.ToArray (typeof (Exception));
			}
		}

		public HttpApplicationState Application
		{
			get {
				return HttpApplicationFactory.ApplicationState;
			}
		}

		public HttpApplication ApplicationInstance
		{
			get {
				return _oApplication;
			}
			set {
				_oApplication = value;
			}
		}

		public Cache Cache
		{
			get {
				return HttpRuntime.Cache;
			}
		}

		public static HttpContext Current {
			get { return Context; }
#if NET_1_1
			set { Context = value; }
#endif
		}

		public Exception Error
		{
			get {
				if (_arrExceptions == null || _arrExceptions.Count == 0)
					return null;

				return (Exception) _arrExceptions [0];
			}
		}

		public IHttpHandler Handler
		{
			get {    
				return _Handler;
			}

			set {
				_Handler = value;
			}
		}

		internal IHttpAsyncHandler AsyncHandler
		{
			get {    
				return _AsyncHandler;
			}

			set {
				_AsyncHandler = value;
			}
		}

		public bool IsCustomErrorEnabled {
			get {
				CustomErrorsConfig cfg;
				try {
					cfg = (CustomErrorsConfig) GetConfig ("system.web/customErrors");
				} catch (Exception) {
					return false;
				}
				
				if (cfg == null)
					return false;

				CustomErrorMode mode = cfg.Mode;
				if (mode == CustomErrorMode.On)
					return true;

				return (mode == CustomErrorMode.RemoteOnly &&
					_oRequest.ServerVariables ["LOCAL_ADDR"] == _oRequest.UserHostAddress);
			}
		}

		public bool IsDebuggingEnabled
		{
			get {
				return CompilationConfiguration.GetInstance (this).Debug;
			}
		}

		public IDictionary Items
		{
			get {
				if (_oItems == null)
					_oItems = new Hashtable ();

				return _oItems;
			}
		}

		public HttpRequest Request
		{
			get {
				return _oRequest;
			}
		}

		public HttpResponse Response
		{
			get {
				return _oResponse;
			}
		}

		public HttpServerUtility Server
		{
			get {
				if (null == _Server)
					_Server = new HttpServerUtility (this);

				return _Server; 
			}
		}

		public HttpSessionState Session
		{
			get {
				return (HttpSessionState) _oSession;
			}
		}

		public bool SkipAuthorization
		{
			get {
				return _skipauth;
			}

			set {
				_skipauth = value;
			}
		}

		public DateTime Timestamp
		{
			get {
				return _oTimestamp;
			}
		}

		public TraceContext Trace
		{
			get {
				return _oTrace;
			}
		}

		public IPrincipal User {
			get { return user; }
			set { user = value; }
		}

		internal bool TimeoutPossible {
			get { return (Interlocked.CompareExchange (ref timeoutPossible, 1, 1) == 1); }
		}
		
		internal void BeginTimeoutPossible ()
		{
			timeoutPossible = 1;
			timeoutBegin = DateTime.UtcNow.Ticks;
		}

		internal void EndTimeoutPossible ()
		{
			Interlocked.CompareExchange (ref timeoutPossible, 0, 1);
		}
		
		internal void TryWaitForTimeout () 
		{
			while (Interlocked.CompareExchange (ref timeoutPossible, 1, 1) == 1) {
				Thread.Sleep (500);
			}
		}

		internal bool CheckIfTimeout (DateTime dt)
		{
			TimeSpan ts = new TimeSpan (dt.Ticks - timeoutBegin);
			return (ts > ConfigTimeout);
		}

		internal TimeSpan ConfigTimeout {
			get {
				if (configTimeout == null) {
					HttpRuntimeConfig config = (HttpRuntimeConfig)
								GetConfig ("system.web/httpRuntime");
					configTimeout = new TimeSpan (0, 0, config.ExecutionTimeout);
				}

				return (TimeSpan) configTimeout;
			}
			set {
				configTimeout = value;
			}
		}
		
		internal string ErrorPage {
			get { return errorPage; }
			set { errorPage = value; }
		}
		
		internal void SetSession (HttpSessionState session)
		{
			_oSession = session;
		}
		
		public void AddError (Exception errorInfo)
		{
			if (_arrExceptions == null)
				_arrExceptions = new ArrayList ();

			_arrExceptions.Add (errorInfo);
		}

		public void ClearError ()
		{
			_arrExceptions = null;
		}

		public object GetConfig (string name)
		{
			return WebConfigurationSettings.GetConfig (name, this);
		}

		public static object GetAppConfig (string name)
		{
			return WebConfigurationSettings.GetConfig (name);
		}

		object IServiceProvider.GetService (Type service)
		{
			if (service == typeof (HttpWorkerRequest)) 
				return _oWorkerRequest;

			if (service == typeof (HttpRequest))
				return Request;

			if (service == typeof (HttpResponse))
				return Response;

			if (service == typeof (HttpApplication))
				return ApplicationInstance;

			if (service == typeof (HttpApplicationState))
				return Application;

			if (service == typeof (HttpSessionState))
				return Session;

			if (service == typeof (HttpServerUtility))
				return Server;

			return null;
		}

		public void RewritePath (string path)
		{
			//LAMESPEC: they say that they throw a ArgumentNullException, however,
			// i get a NullReferenceException...
			if (path == null)
				throw new ArgumentNullException ("path");

			string query;
			int qmark = path.IndexOf ('?');
			if (qmark == -1 || qmark + 1 >= path.Length) {
				query = null;
			} else {
				query = path.Substring (qmark + 1);
				path = path.Substring (0, qmark);
			}

			path = UrlUtils.Combine (Request.BaseVirtualDir, path);
			if (!path.StartsWith (HttpRuntime.AppDomainAppVirtualPath))
				throw new HttpException (404, "The virtual path '" + path +
							 "' maps to another application.");

			Request.SetFilePath (path);
			Request.QueryStringRaw = query;
		}

#if NET_1_1
		public void RewritePath (string filePath, string pathInfo, string queryString)
		{
			RewritePath (filePath + "?" + queryString);
			Request.SetPathInfo (pathInfo);
		}
#endif
	}
}

