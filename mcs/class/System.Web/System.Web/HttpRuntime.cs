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
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.UI;
using System.Threading;

namespace System.Web {
	
	public sealed class HttpRuntime {
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
		
		public HttpRuntime ()
		{
		}

#region AppDomain handling
		//
		// http://radio.weblogs.com/0105476/stories/2002/07/12/executingAspxPagesWithoutAWebServer.html
		//
		public static string AppDomainAppId {
			get {
				//
				// This value should not change across invocations
				//
			       
				return (string) AppDomain.CurrentDomain.GetData (".appId");
			}
		}

		// Physical directory for the application
		public static string AppDomainAppPath {
			get {
				return (string) AppDomain.CurrentDomain.GetData (".appPath");
			}
		}

		public static string AppDomainAppVirtualPath {
			get {
				return (string) AppDomain.CurrentDomain.GetData (".appVPath");
			}
		}

		public static string AppDomainId {
			get {
				return (string) AppDomain.CurrentDomain.GetData (".domainId");
			}
		}

		public static string AspInstallDirectory {
			get {
				return (string) AppDomain.CurrentDomain.GetData (".hostingInstallDir");
			}
		}
#endregion
		
		public static string BinDirectory {
			get {
				return Path.Combine (AppDomainAppPath, "bin");
			}
		}

		public static Cache Cache {
			get {
				return cache;
			}
		}

		public static string ClrInstallDirectory {
			get {
				return Path.GetDirectoryName (typeof (Object).Assembly.CodeBase);
			}
		}

		public static string CodegenDir {
			get {
				return AppDomain.CurrentDomain.SetupInformation.DynamicBase;
			}
		}

		public static bool IsOnUNCShare {
			get {
				throw new NotImplementedException ();
			}
		}

		public static string MachineConfigurationDirectory {
			get {
				return Path.GetDirectoryName (WebConfigurationSettings.MachineConfigPath);
			}
		}

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
			HttpApplication app = HttpApplicationFactory.GetApplication (context);

			context.ApplicationInstance = app;
			
			//
			// Initialize, load modules specific on the config file.
			//

			//
			// Ask application to service the request
			//
			IHttpAsyncHandler ihah = app;

			IAsyncResult appiar = ihah.BeginProcessRequest (context, new AsyncCallback (request_processed), context);
			ihah.EndProcessRequest (appiar);

			HttpApplicationFactory.Recycle (app);
			
			QueuePendingRequests ();
		}
		
		//
		// ProcessRequest method is executed in the AppDomain of the application
		//
		// Observations:
		//    ProcessRequest does not guarantee that `wr' will be processed synchronously,
		//    the request can be queued and processed later.
		//
		public static void ProcessRequest (HttpWorkerRequest wr)
		{
			HttpWorkerRequest request;

			//
			// Queue our request, fetch the next available one from the queue
			//
			request = queue_manager.GetNextRequest (wr);
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
			// Kill our application.
			HttpApplicationFactory.Dispose ();
		}

                static string content503 = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
			"<html><head>\n<title>503 Server Unavailable</title>\n</head><body>\n" +
			"<h1>Server Unavailable</h1>\n" +
			"</body></html>\n";

		//
		// This is called from the QueueManager if a request
		// can not be processed (load, no resources, or
		// appdomain unload).
		//
		static internal void FinishUnavailable (HttpWorkerRequest wr)
		{
			string host = "FIXME";
			string location = "FIXME";
				
			//
			// From Mono.WebServer
			//
			wr.SendStatus (503, "Service unavailable");
                        wr.SendUnknownResponseHeader ("Connection", "close");
                        wr.SendUnknownResponseHeader ("Date", DateTime.Now.ToUniversalTime ().ToString ("r"));
                        wr.SendUnknownResponseHeader ("Location", String.Format ("http://{0}{1}", host, location));
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

		internal static TimeoutManager TimeoutManager {
			get {
				return timeout_manager;
			}
		}

#if NET_2_0
		static ApplicationShutdownReason shutdown_reason = ApplicationShutdownReason.None;

		[MonoTODO]
		public static ApplicationShutdownReason ShutdownReason {
			get {
				//
				// Unlike previously believed by Gonzalo and
				// myself HttpRuntime.UnloadAppDomain is not
				// something that happens right away, UnloadAppDomain
				// mereley "queues" the domain for destruction, but
				// the application continues to execute until it
				// is time to terminate.  Only at that point is
				// the domain unloaded.
				//
				// This means that we should probably not use the
				// QueueUserWorkItem above to shutdown the appdomain
				// in a separate thread, but rather just flag this
				// app for termination, and then unload the domain
				//
				// The user can continue executing for a long time
				//
				// See the sample in the docs for ShutdownReason.
				//
				
				return shutdown_reason;
			}
		}
#endif

	}
}
