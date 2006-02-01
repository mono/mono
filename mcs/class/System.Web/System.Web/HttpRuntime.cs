//
// System.Web.HttpRuntime.cs 
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

//
// TODO: Call HttpRequest.CloseInputStream when we finish a request, as we are using the IntPtr stream.
//
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.UI;
using System.Threading;

namespace System.Web {
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpRuntime {
#if TARGET_J2EE
		static QueueManager queue_manager { get { return _runtime._queue_manager; } }
		static TraceManager trace_manager { get { return _runtime._trace_manager; } }
		static Cache cache { get { return _runtime._cache; } }
		static WaitCallback do_RealProcessRequest;

		QueueManager _queue_manager;
		TraceManager _trace_manager;
		Cache _cache;

		static HttpRuntime ()
		{
			do_RealProcessRequest = new WaitCallback (RealProcessRequest);
		}

		public HttpRuntime ()
		{
			_queue_manager = new QueueManager ();
			_trace_manager = new TraceManager ();
			_cache = new Cache ();
		}

		static private HttpRuntime _runtime {
			get {
				HttpRuntime runtime = (HttpRuntime)AppDomain.CurrentDomain.GetData("HttpRuntime");
				if (runtime == null)
					lock (typeof(HttpRuntime)) {
						runtime = (HttpRuntime)AppDomain.CurrentDomain.GetData("HttpRuntime");
						if (runtime == null) {
							runtime = new HttpRuntime();
							AppDomain.CurrentDomain.SetData("HttpRuntime", runtime);
						}
					}
				return runtime;
			}
		}
#else
		static QueueManager queue_manager;
		static TraceManager trace_manager;
		static TimeoutManager timeout_manager;
		static Cache cache;
		static WaitCallback do_RealProcessRequest;

		static HttpRuntime ()
		{
			queue_manager = new QueueManager ();
			trace_manager = new TraceManager ();
			timeout_manager = new TimeoutManager ();
			cache = new Cache ();
			do_RealProcessRequest = new WaitCallback (RealProcessRequest);
		}

#if ONLY_1_1
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
#endif
		public HttpRuntime ()
		{
		}
#endif

#region AppDomain handling
		//
		// http://radio.weblogs.com/0105476/stories/2002/07/12/executingAspxPagesWithoutAWebServer.html
		//
		public static string AppDomainAppId {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get {
				//
				// This value should not change across invocations
				//
				string dirname = (string) AppDomain.CurrentDomain.GetData (".appId");
				if ((dirname != null) && (dirname.Length > 0) && SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		// Physical directory for the application
		public static string AppDomainAppPath {
			get {
				string dirname = (string) AppDomain.CurrentDomain.GetData (".appPath");
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		public static string AppDomainAppVirtualPath {
			get {
				return (string) AppDomain.CurrentDomain.GetData (".appVPath");
			}
		}

		public static string AppDomainId {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get {
				return (string) AppDomain.CurrentDomain.GetData (".domainId");
			}
		}

		public static string AspInstallDirectory {
			get {
				string dirname = (string) AppDomain.CurrentDomain.GetData (".hostingInstallDir");
				if ((dirname != null) && (dirname.Length > 0) && SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}
#endregion
		
		public static string BinDirectory {
			get {
				string dirname = Path.Combine (AppDomainAppPath, "bin");
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		public static Cache Cache {
			get {
				return cache;
			}
		}

		public static string ClrInstallDirectory {
			get {
				string dirname = Path.GetDirectoryName (typeof (Object).Assembly.Location);
				if ((dirname != null) && (dirname.Length > 0) && SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		public static string CodegenDir {
			get {
				string dirname = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		[MonoTODO]
		public static bool IsOnUNCShare {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				throw new NotImplementedException ();
			}
		}

		public static string MachineConfigurationDirectory {
			get {
#if NET_2_0
				string dirname = Path.GetDirectoryName (WebConfigurationManager.OpenMachineConfiguration().FilePath);
#else
				string dirname = Path.GetDirectoryName (WebConfigurationSettings.MachineConfigPath);
#endif
				if ((dirname != null) && (dirname.Length > 0) && SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, dirname).Demand ();
				}
				return dirname;
			}
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void Close ()
		{
			// Remove all items from cache.
		}

		static void QueuePendingRequests ()
		{
			HttpWorkerRequest request = queue_manager.GetNextRequest (null);
			if (request == null)
				return;
			ThreadPool.QueueUserWorkItem (do_RealProcessRequest, request);
		}

		static void RealProcessRequest (object o)
		{
			HttpContext context = new HttpContext ((HttpWorkerRequest) o);
			HttpContext.Current = context;

			//
			// Get application instance (create or reuse an instance of the correct class)
			//
			HttpApplication app = null;
			bool error = false;
			try {
				app = HttpApplicationFactory.GetApplication (context);
			} catch (Exception e) {
				FinishWithException ((HttpWorkerRequest) o, new HttpException ("", e));
				error = true;
			}

			if (error) {
				context.Request.ReleaseResources ();
				context.Response.ReleaseResources ();
				HttpContext.Current = null;
			} else {
				context.ApplicationInstance = app;
			
				//
				// Ask application to service the request
				//
				IHttpAsyncHandler ihah = app;

				IAsyncResult appiar = ihah.BeginProcessRequest (context, new AsyncCallback (request_processed), context);
				ihah.EndProcessRequest (appiar);

				HttpApplicationFactory.Recycle (app);
			}
			
			QueuePendingRequests ();
		}
		
		//
		// ProcessRequest method is executed in the AppDomain of the application
		//
		// Observations:
		//    ProcessRequest does not guarantee that `wr' will be processed synchronously,
		//    the request can be queued and processed later.
		//
		[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
		public static void ProcessRequest (HttpWorkerRequest wr)
		{
			if (wr == null)
				throw new ArgumentNullException ("wr");
			//
			// Queue our request, fetch the next available one from the queue
			//
			HttpWorkerRequest request = queue_manager.GetNextRequest (wr);
			if (request == null)
				return;

			RealProcessRequest (request);
		}

		//
		// Callback to be invoked by IHttpAsyncHandler.BeginProcessRequest
		//
		static void request_processed (IAsyncResult iar)
		{
			HttpContext context = (HttpContext) iar.AsyncState;

			context.Request.ReleaseResources ();
			context.Response.ReleaseResources ();
		}

		//
		// Called when we are shutting down or we need to reload an application
		// that has been modified (touch global.asax) 
		//
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void UnloadAppDomain ()
		{
			//
			// TODO: call ReleaseResources
			//
			ThreadPool.QueueUserWorkItem (new WaitCallback (ShutdownAppDomain), null);
		}

		//
		// Shuts down the AppDomain
		//
		static void ShutdownAppDomain (object args)
		{
			// This will call Session_End if needed.
			Cache.InvokePrivateCallbacks ();
			// Kill our application.
			HttpApplicationFactory.Dispose ();
			ThreadPool.QueueUserWorkItem (new WaitCallback (DoUnload), null);
		}

#if TARGET_J2EE // No unload support for appdomains under Grasshopper
		static void DoUnload (object state)
		{
		}
#else
		static void DoUnload (object state)
		{
			AppDomain.Unload (AppDomain.CurrentDomain);
		}
#endif

                static string content503 = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
			"<html><head>\n<title>503 Server Unavailable</title>\n</head><body>\n" +
			"<h1>Server Unavailable</h1>\n" +
			"</body></html>\n";

		static void FinishWithException (HttpWorkerRequest wr, HttpException e)
		{
			int code = e.GetHttpCode ();
			wr.SendStatus (code, HttpWorkerRequest.GetStatusDescription (code));
			wr.SendUnknownResponseHeader ("Connection", "close");
			wr.SendUnknownResponseHeader ("Date", DateTime.Now.ToUniversalTime ().ToString ("r"));
			Encoding enc = Encoding.ASCII;
			wr.SendUnknownResponseHeader ("Content-Type", "text/html; charset=" + enc.WebName);
			string msg = e.GetHtmlErrorMessage ();
			byte [] contentBytes = enc.GetBytes (msg);
			wr.SendUnknownResponseHeader ("Content-Length", contentBytes.Length.ToString ());
			wr.SendResponseFromMemory (contentBytes, contentBytes.Length);
			wr.FlushResponse (true);
			wr.CloseConnection ();
		}

		//
		// This is called from the QueueManager if a request
		// can not be processed (load, no resources, or
		// appdomain unload).
		//
		static internal void FinishUnavailable (HttpWorkerRequest wr)
		{
			wr.SendStatus (503, "Service unavailable");
			wr.SendUnknownResponseHeader ("Connection", "close");
			wr.SendUnknownResponseHeader ("Date", DateTime.Now.ToUniversalTime ().ToString ("r"));
			Encoding enc = Encoding.ASCII;
			wr.SendUnknownResponseHeader ("Content-Type", "text/html; charset=" + enc.WebName);
			byte [] contentBytes = enc.GetBytes (content503);
			wr.SendUnknownResponseHeader ("Content-Length", contentBytes.Length.ToString ());
			wr.SendResponseFromMemory (contentBytes, contentBytes.Length);
			wr.FlushResponse (true);
			wr.CloseConnection ();
		}

		internal static TraceManager TraceManager {
			get {
				return trace_manager;
			}
		}

#if !TARGET_JVM
		internal static TimeoutManager TimeoutManager {
			get {
				return timeout_manager;
			}
		}
#endif
	}
}
