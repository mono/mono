// 
// System.Web.HttpRuntime
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//   Gaurav Vaish (gvaish@iitk.ac.in)
//
using System;
using System.Collections;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web.UI;
using System.Web.Util;
using System.Web.Caching;

namespace System.Web {

	public sealed class HttpRuntime {

		// Security permission helper objects
		private static IStackWalk appPathDiscoveryStackWalk;
		private static IStackWalk ctrlPrincipalStackWalk;
		private static IStackWalk sensitiveInfoStackWalk;
		private static IStackWalk unmgdCodeStackWalk;
		private static IStackWalk unrestrictedStackWalk;
		private static IStackWalk reflectionStackWalk;

		private static HttpRuntime _runtime;
		private static string appDomainAppId;
		private static string appDomainId;
		private static string appDomainAppPath;
		private static string appDomainAppVirtualPath;
		private Cache _cache;

		private int _activeRequests;
		private HttpWorkerRequest.EndOfSendNotification _endOfSendCallback;
		private AsyncCallback _handlerCallback;
		private WaitCallback _appDomainCallback;

		private bool _firstRequestStarted;
		private bool _firstRequestExecuted;
		private DateTime _firstRequestStartTime;

		private Exception _initError;

		static HttpRuntime ()
		{
			appPathDiscoveryStackWalk = null;
			ctrlPrincipalStackWalk    = null;
			sensitiveInfoStackWalk    = null;
			unmgdCodeStackWalk        = null;
			unrestrictedStackWalk     = null;
         
			_runtime = new HttpRuntime ();
			_runtime.Init();
		}

		public HttpRuntime ()
		{	
		}

		static internal object CreateInternalObject(Type type) {
			return Activator.CreateInstance(type, true);
		}

		[MonoTODO()]
		private void Init ()
		{
			try {
				_cache = new Cache ();

				// TODO: timeout manager
				// TODO: Load all app domain data
				// TODO: Trace manager
				_endOfSendCallback = new HttpWorkerRequest.EndOfSendNotification(OnEndOfSend);
				_handlerCallback = new AsyncCallback(OnHandlerReady);
				_appDomainCallback = new WaitCallback(OnAppDomainUnload);
			} 
			catch (Exception error) {
				_initError = error;
			}
		}

		private void OnFirstRequestStart() {
			if (null == _initError) {
				// TODO: First request initialize (config, trace, request queue)
			}

			// If we got an error during init, throw to client now..
			if (null != _initError)
				throw _initError;
		}

		private void OnFirstRequestEnd() {
		}

		private void OnHandlerReady(IAsyncResult ar) {
			HttpContext context = (HttpContext) ar.AsyncState;
			try {
				IHttpAsyncHandler handler = context.AsyncHandler;

				try {
					handler.EndProcessRequest(ar);
				}
				catch (Exception error) {
					context.AddError(error);
				}
			}
			finally {
				context.AsyncHandler = null;
			}

			FinishRequest(context, context.Error);
		}

		private void OnEndOfSend(HttpWorkerRequest request, object data) {
			HttpContext context = (HttpContext) data;

			context.Request.Dispose();
			context.Response.Dispose();
		}

		internal void FinishRequest(HttpContext context, Exception error) {
			HttpWorkerRequest request = context.WorkerRequest;

			if (error == null) {
				try {
					context.Response.FlushAtEndOfRequest();
				} catch (Exception obj) {
					error = obj;
				}
			}

			if (null != error) {
				WebTrace.WriteLine (error.ToString ());
				context.Response.Clear ();
				if (!(error is HttpException)) {
					error = new HttpException (String.Empty, error);
				}
				context.Response.Write (((HttpException) error).GetHtmlErrorMessage ());
				context.Response.FinalFlush ();
			}

			if (!_firstRequestExecuted) {
				lock (this) {
					if (!_firstRequestExecuted) {
						OnFirstRequestEnd();
						_firstRequestExecuted = true;
					}
				}
			}

			Interlocked.Decrement(ref _activeRequests);

			if (null != request)
				request.EndOfRequest();

			// TODO: Schedule more work in request queue
		}

		private void OnAppDomainUnload(object state) {
			Dispose();
		}

		[MonoTODO]
		internal void Dispose() {
			// TODO: Drain Request queue
			// TODO: Stop request queue
			// TODO: Move timeout value to config
			WaitForRequests(5000);
			
			_cache.Dispose();
			HttpApplicationFactory.EndApplication();
		}

		[MonoTODO]
		internal void WaitForRequests(int ms) {
			DateTime timeout = DateTime.Now.AddMilliseconds(ms);

			do {
				// TODO: We should check the request queue here also
				if (_activeRequests == 0)
					return;

				Thread.Sleep(100);
			} while (timeout > DateTime.Now);
		}

		internal void InternalExecuteRequest(HttpWorkerRequest request)
		{
			if (request == null)
				throw new ArgumentNullException ("request");
			
			IHttpHandler handler;
			IHttpAsyncHandler async_handler;

			HttpContext context = new HttpContext(request);

			request.SetEndOfSendNotification(_endOfSendCallback, context);
			
			Interlocked.Increment(ref _activeRequests);

			try {
				if (!_firstRequestStarted) {
					lock (this) {
						if (!_firstRequestStarted) {
							_firstRequestStartTime = DateTime.Now;

							OnFirstRequestStart();
							_firstRequestStarted = true;
						}						
					}
				}

				handler = HttpApplicationFactory.GetInstance(context);
				if (null == handler)
					throw new HttpException(FormatResourceString("unable_to_create_app"));

				if (handler is IHttpAsyncHandler) {
					async_handler = (IHttpAsyncHandler) handler;

					context.AsyncHandler = async_handler;
					async_handler.BeginProcessRequest(context, _handlerCallback, context);
				} else {
					handler.ProcessRequest(context);
					FinishRequest(context, null);
				}
			}
			catch (Exception error) {
				FinishRequest(context, error);
			}
		}

		public static void ProcessRequest (HttpWorkerRequest Request)
		{
			// TODO: Request queue
			_runtime.InternalExecuteRequest(Request);
		}

		public static Cache Cache {
			get {
				return _runtime._cache;
			}
		}      

		public static string AppDomainAppId {
			get {
				if (appDomainAppId == null)
					appDomainAppId = (string) AppDomain.CurrentDomain.GetData (".appId");

				return appDomainAppId;
			}
		}

		public static string AppDomainAppPath {
			get {
				if (appDomainAppPath == null)
					appDomainAppPath = (string) AppDomain.CurrentDomain.GetData (".appPath");

				return appDomainAppPath;
			}
		}

		public static string AppDomainAppVirtualPath {
			get {
				if (appDomainAppVirtualPath == null)
					appDomainAppVirtualPath = (string) AppDomain.CurrentDomain.GetData (".appVPath");

				return appDomainAppVirtualPath;
			}
		}

		public static string AppDomainId {
			get {
				if (appDomainId == null)
					appDomainId = (string) AppDomain.CurrentDomain.GetData (".domainId");

				return appDomainId;
			}
		}

		[MonoTODO]
		public static string AspInstallDirectory {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static string BinDirectory {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static string ClrInstallDirectory {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static string CodegenDir {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static bool IsOnUNCShare {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static string MachineConfigurationDirectory {
			get {
				throw new NotImplementedException ();
			}
		}

		public static void Close ()
		{
			_runtime.Dispose();
		}

		internal static string FormatResourceString (string key)
		{
			return GetResourceString (key);
		}

		internal static string FormatResourceString (string key, string arg0)
		{
			/*string format = GetResourceString (key);

			if (format == null)
				return null;
			
			return String.Format (format, arg0);
			*/
			return String.Format ("{0}: {1}", key, arg0);
		}

		[MonoTODO ("FormatResourceString (string, string, string)")]
		internal static string FormatResourceString (string key, string arg0, string type) {
			return String.Format ("{0}: {1} {2}", key, arg0, type);
		}

		[MonoTODO ("FormatResourceString (string, string, string, string)")]
		internal static string FormatResourceString (string key, string arg0,
							     string arg1, string arg2)
		{
			return String.Format ("{0}: {1} {2} {3}", key, arg0, arg1, arg2);
		}

		[MonoTODO ("FormatResourceString (string, string[]")]
		internal static string FormatResourceString (string key, string[] args)
		{
			//StringBuilder sb = new StringBuilder ();
			/*sb.AppendFormat ("{0}: ", key);
			foreach (string s in args)
				sb.AppendFormat ("{0} ", s);

			if (sb.Length > 0)
				sb.Length--;
			return sb.ToString ();*/
			string s = key + ": ";
			if (args != null)
				foreach (string k in args)
					s += k + " ";
			return s;
		}

		private static string GetResourceString (string key) {
			return _runtime.GetResourceStringFromResourceManager (key);
		}

		[MonoTODO ("GetResourceStringFromResourceManager (string)")]
		private string GetResourceStringFromResourceManager (string key) {
			return "String returned by HttpRuntime.GetResourceStringFromResourceManager";
		}

		#region Security Internal Methods (not impl)
		[MonoTODO ("Get Application path from the appdomain object")]
		internal static IStackWalk AppPathDiscovery {
			get {
				if (appPathDiscoveryStackWalk == null) {
					appPathDiscoveryStackWalk = new FileIOPermission (
						FileIOPermissionAccess.PathDiscovery, "<apppath>");
				}
				return appPathDiscoveryStackWalk;
			}
		}

		internal static IStackWalk ControlPrincipal {
			get {
				if (ctrlPrincipalStackWalk == null) {
					ctrlPrincipalStackWalk = new SecurityPermission (
						SecurityPermissionFlag.ControlPrincipal);
				}
				return ctrlPrincipalStackWalk;
			}
		}

		internal static IStackWalk Reflection {
			get {
				if (reflectionStackWalk == null) {
					reflectionStackWalk = new ReflectionPermission (
						ReflectionPermissionFlag.TypeInformation |
						ReflectionPermissionFlag.MemberAccess);
				}
				return reflectionStackWalk;
			}
		}

		internal static IStackWalk SensitiveInformation {
			get {
				if (sensitiveInfoStackWalk == null) {
					sensitiveInfoStackWalk = new EnvironmentPermission (
						PermissionState.Unrestricted);
				}
				return sensitiveInfoStackWalk;
			}
		}

		internal static IStackWalk UnmanagedCode {
			get {
				if (unmgdCodeStackWalk == null) {
					unmgdCodeStackWalk = new SecurityPermission (
						SecurityPermissionFlag.UnmanagedCode);
				}
				return unmgdCodeStackWalk;
			}
		}

		internal static IStackWalk Unrestricted {
			get {
				if (unrestrictedStackWalk == null) {
					unrestrictedStackWalk = new PermissionSet (
						PermissionState.Unrestricted);
				}
				return unrestrictedStackWalk;
			}
		}

		internal static IStackWalk FileReadAccess (string file)
		{
			return new FileIOPermission (FileIOPermissionAccess.Read, file);
		}

		internal static IStackWalk PathDiscoveryAccess (string path)
		{
			return new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path);
		}
		#endregion
	}
}
