// 
// System.Web.HttpRuntime
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
//	Gaurav Vaish (gvaish@iitk.ac.in)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.IO;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web.Configuration;
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
		private TimeoutManager timeoutManager;
		private QueueManager queueManager;
		private TraceManager traceManager;
		private WaitCallback doRequestCallback;
		private int pendingCallbacks;

		static HttpRuntime ()
		{
			_runtime = new HttpRuntime ();
			_runtime.Init();
		}

		public HttpRuntime ()
		{
			doRequestCallback = new WaitCallback (DoRequest);
		}

		static internal object CreateInternalObject(Type type) {
			return Activator.CreateInstance(type, true);
		}

		private void Init ()
		{
			try {
				_cache = new Cache ();
				timeoutManager = new TimeoutManager ();

				_endOfSendCallback = new HttpWorkerRequest.EndOfSendNotification(OnEndOfSend);
				_handlerCallback = new AsyncCallback(OnHandlerReady);
				_appDomainCallback = new WaitCallback(OnAppDomainUnload);
			} 
			catch (Exception error) {
				_initError = error;
			}
		}

		private void OnFirstRequestStart(HttpContext context) {
			if (_initError != null)
				throw _initError;

			try {
				WebConfigurationSettings.Init (context);
				traceManager = new TraceManager ();
				queueManager = new QueueManager ();
			} catch (Exception e) {
				_initError = e;
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
			if (error == null) {
				try {
					context.Response.FlushAtEndOfRequest();
				} catch (Exception obj) {
					error = obj;
				}
			}

			HttpWorkerRequest request = context.WorkerRequest;
			if (null != error) {
				WebTrace.WriteLine (error.ToString ());

				context.Response.Clear ();
				context.Response.ClearHeaders ();

				if (!(error is HttpException)) {
					error = new HttpException (String.Empty, error);
					context.Response.StatusCode = 500;
				} else {
					context.Response.StatusCode = ((HttpException) error).GetHttpCode ();
				}

				if (!RedirectCustomError (context))
					context.Response.Write (((HttpException) error).GetHtmlErrorMessage ());

				context.Response.FinalFlush ();
			}

			/*
			 * This is not being used. OnFirstRequestEnd is empty.
			if (!_firstRequestExecuted) {
				lock (this) {
					if (!_firstRequestExecuted) {
						_firstRequestExecuted = true;
						OnFirstRequestEnd();
					}
				}
			}
			*/

			Interlocked.Decrement(ref _activeRequests);

			if (null != request)
				request.EndOfRequest();

			TryExecuteQueuedRequests ();
		}

		bool RedirectCustomError (HttpContext context)
		{
			if (!context.IsCustomErrorEnabled)
				return false;

			CustomErrorsConfig config = null;
			try {
				config = (CustomErrorsConfig) context.GetConfig ("system.web/customErrors");
			} catch { }

			if (config == null) {
				if (context.ErrorPage != null)
					return context.Response.RedirectCustomError (context.ErrorPage);

				return false;
			}

			string redirect =  config [context.Response.StatusCode];
			if (redirect == null) {
				redirect = context.ErrorPage;
				if (redirect == null)
					redirect = config.DefaultRedirect;
			}

			if (redirect == null)
				return false;

			return context.Response.RedirectCustomError (redirect);
		}

		internal static void FinishUnavailable (HttpWorkerRequest wr)
		{
			HttpContext context = new HttpContext (wr);
			HttpException exception = new HttpException (503, "Service unavailable");
			Interlocked.Increment (ref _runtime._activeRequests);
			context.Response.InitializeWriter ();
			_runtime.FinishRequest (context, exception);
		}

		private void OnAppDomainUnload(object state) {
			Dispose();
		}

		internal void Dispose() {
			WaitForRequests(5000);
			queueManager.Dispose (); // Send a 503 to all queued requests
			queueManager = null;
			
			_cache = null;
			HttpApplicationFactory.EndApplication();
		}

		internal void WaitForRequests(int ms) {
			DateTime timeout = DateTime.Now.AddMilliseconds(ms);

			do {
				if (Interlocked.CompareExchange (ref _activeRequests, 0, 0) == 0)
					return;

				Thread.Sleep (100);
			} while (timeout > DateTime.Now);
		}

		internal void InternalExecuteRequest (HttpWorkerRequest request)
		{
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
							OnFirstRequestStart(context);
							_firstRequestStarted = true;
						}
					}
				}

				// This *must* be done after the configuration is initialized.
				context.Response.InitializeWriter ();
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
				context.Response.InitializeWriter ();
				FinishRequest(context, error);
			}
		}

		void DoRequest (object o)
		{
			Interlocked.Decrement (ref pendingCallbacks);
			InternalExecuteRequest ((HttpWorkerRequest) o);
		}
		
		void TryExecuteQueuedRequests ()
		{
			// Wait for pending jobs to start
			if (Interlocked.CompareExchange (ref pendingCallbacks, 3, 3) == 3)
				return;

			HttpWorkerRequest wr = queueManager.GetNextRequest (null);
			if (wr == null)
				return;

			Interlocked.Increment (ref pendingCallbacks);
			ThreadPool.QueueUserWorkItem (doRequestCallback, wr);
			TryExecuteQueuedRequests ();
		}

		public static void ProcessRequest (HttpWorkerRequest request)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			QueueManager mgr = _runtime.queueManager;
			if (_runtime._firstRequestStarted && mgr != null) {
				request = mgr.GetNextRequest (request);
				// We're busy, return immediately
				if (request == null)
					return;
			}

			_runtime.InternalExecuteRequest (request);
		}

#if NET_1_1
		[MonoTODO]
		public void UnloadAppDomain ()
		{
			throw new NotImplementedException ();
		}
#endif
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

		public static string AspInstallDirectory {
			get {
				return ICalls.GetMachineInstallDirectory ();
			}
		}

		public static string BinDirectory {
			get {
				return Path.Combine (AppDomainAppPath, "bin");
			}
		}

		public static string ClrInstallDirectory {
			get {
				return ICalls.GetMachineInstallDirectory ();
			}
		}

		public static string CodegenDir {
			get {
				return AppDomain.CurrentDomain.SetupInformation.DynamicBase;
			}
		}

		public static bool IsOnUNCShare {
			get {
				// IsUnc broken under unix?
				return (!((int) Environment.OSVersion.Platform == 128) &&
					new Uri ("file://" + ClrInstallDirectory).IsUnc);
			}
		}

		public static string MachineConfigurationDirectory {
			get {
				return Path.GetDirectoryName (WebConfigurationSettings.MachineConfigPath);
			}
		}

		internal static TimeoutManager TimeoutManager {
			get {
				return HttpRuntime._runtime.timeoutManager;
			}
		}

                internal static TraceManager TraceManager {
                        get {
                                return HttpRuntime._runtime.traceManager;
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
			return key;
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
