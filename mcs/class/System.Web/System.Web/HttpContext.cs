//
// System.Web.HttpContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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

namespace System.Web
{
	public sealed class HttpContext : IServiceProvider
	{
		private ArrayList _arrExceptions;

		private HttpResponse	_oResponse;
		private HttpRequest _oRequest;
		private HttpServerUtility _Server;
		private HttpApplication _oApplication;
		private HttpWorkerRequest _oWorkerRequest;
		private IHttpHandler _Handler;
		private IHttpAsyncHandler _AsyncHandler;
		private IPrincipal _User;

		private bool _skipauth;
		private Hashtable		_oItems;
		private DateTime		_oTimestamp;

		public HttpContext (HttpRequest Request, HttpResponse Response)
		{
			Context = this;

			_arrExceptions = null;
			_oItems = null;
			_oTimestamp = DateTime.Now;
			_oRequest = Request;
			_oResponse = Response;
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
		}

		internal HttpWorkerRequest WorkerRequest
		{
			get {
				return _oWorkerRequest;
			}
		}

		[MonoTODO("Context - Use System.Remoting.Messaging.CallContext instead of Thread storage")]
		internal static HttpContext Context
		{
			get {
				return (HttpContext) Thread.GetData (Thread.GetNamedDataSlot ("Context"));
			}

			set {
				Thread.SetData (Thread.GetNamedDataSlot ("Context"), value);
			}
		}

		public Exception [] AllErrors
		{
			get {
				if (_arrExceptions == null || _arrExceptions.Count == 0)
					return null;

				return (Exception []) _arrExceptions.ToArray (typeof (Exception));;
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

		public static HttpContext Current
		{
			get {
				return Context;
			}
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

		[MonoTODO("bool IsCustomErrorEnabled")]
		public bool IsCustomErrorEnabled
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("bool IsDebuggingEnabled")]
		public bool IsDebuggingEnabled
		{
			get {
				throw new NotImplementedException ();
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
				return (HttpSessionState) Items ["sessionstate"];
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

		[MonoTODO("TraceContext Trace")]
		public TraceContext Trace
		{
			get {
				throw new NotImplementedException();
			}
		}

		public IPrincipal User
		{
			get {
				return _User;
			}
			set {
				_User = value;
			}
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
			return ConfigurationSettings.GetConfig (name);
		}

		public static object GetAppConfig (string name)
		{
			return ConfigurationSettings.GetConfig (name);
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
			if (!path.StartsWith (Request.BaseVirtualDir))
				throw new HttpException (404, "The virtual path '" + path +
							 "' maps to another application.");

			Request.SetFilePath (path);
			Request.QueryStringRaw = query;
		}
	}
}

