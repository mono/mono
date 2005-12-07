//
// System.Web.HttpApplication.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
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
// The Application Processing Pipeline.
// 
//     The Http application pipeline implemented in this file is a
//     beautiful thing.  The application pipeline invokes a number of
//     hooks at various stages of the processing of a request.  These
//     hooks can be either synchronous or can be asynchronous.
//     
//     The pipeline must ensure that every step is completed before
//     moving to the next step.  A trivial thing for synchronous
//     hooks, but asynchronous hooks introduce an extra layer of
//     complexity: when the hook is invoked, the thread must
//     relinquish its control so that the thread can be reused in
//     another operation while waiting.
//
//     To implement this functionality we used C# iterators manually;
//     we drive the pipeline by executing the various hooks from the
//     `RunHooks' routine which is an enumerator that will yield the
//     value `false' if execution must proceed or `true' if execution
//     must be stopped.
//
//     By yielding values we can suspend execution of RunHooks.
//
//     Special attention must be given to `in_begin' and `must_yield'
//     variables.  These are used in the case that an async hook
//     completes synchronously as its important to not yield in that
//     case or we would hang.
//    
//     Many of Mono modules used to be declared async, but they would
//     actually be completely synchronous, this might resurface in the
//     future with other modules.
//
// TODO:
//    Events Disposed
//

using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.UI;
	
namespace System.Web {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem(false)]
	public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable {
		HttpContext context;
		HttpSessionState session;
		ISite isite;

		// The source, and the exposed API (cache).
		HttpModuleCollection modcoll;

		string assemblyLocation;

		//
		// The factory for the handler currently running.
		//
		IHttpHandlerFactory factory;
		
		//
		// Whether the pipeline should be stopped
		//
		bool stop_processing;

		//
		// The Pipeline
		//
		IEnumerator pipeline;

		// To flag when we are done processing a request from BeginProcessRequest.
		ManualResetEvent done;

		// The current IAsyncResult for the running async request handler in the pipeline
		AsyncRequestState begin_iar;

		// Tracks the current AsyncInvocation being dispatched
		AsyncInvoker current_ai;

		// We don't use the EventHandlerList here, but derived classes might do
		EventHandlerList events;

		// Culture and IPrincipal
		CultureInfo app_culture;
		CultureInfo appui_culture;
		CultureInfo prev_app_culture;
		CultureInfo prev_appui_culture;
		IPrincipal prev_user;

		//
		// These are used to detect the case where the EndXXX method is invoked
		// from within the BeginXXXX delegate, so we detect whether we kick the
		// pipeline from here, or from the the RunHook routine
		//
		bool must_yield;
		bool in_begin;

		public HttpApplication ()
		{
			done = new ManualResetEvent (false);
		}

		internal void InitOnce (bool full_init)
		{
			lock (this) {
				if (modcoll != null)
					return;

				ModulesConfiguration modules;
				modules = (ModulesConfiguration) HttpContext.GetAppConfig ("system.web/httpModules");

				modcoll = modules.LoadModules (this);

				if (full_init)
					HttpApplicationFactory.AttachEvents (this);

				GlobalizationConfiguration cfg = GlobalizationConfiguration.GetInstance (null);
				if (cfg != null) {
					app_culture = cfg.Culture;
					appui_culture = cfg.UICulture;
				}
			}
		}

		internal string AssemblyLocation {
			get {
				if (assemblyLocation == null)
					assemblyLocation = GetType ().Assembly.Location;
				return assemblyLocation;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpApplicationState Application {
			get {
				return HttpApplicationFactory.ApplicationState;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpContext Context {
			get {
				return context;
			}
		}
					 
		protected EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList ();

				return events;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpModuleCollection Modules {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get {
				if (modcoll == null)
					modcoll = new HttpModuleCollection ();
				
				return modcoll;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpRequest Request {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				return context.Request;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpResponse Response {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				return context.Response;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpServerUtility Server {
			get {
				if (context != null)
					return context.Server;

				//
				// This is so we can get the Server and call a few methods
				// which are not context sensitive, see HttpServerUtilityTest
				//
				return new HttpServerUtility ((HttpContext) null);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpSessionState Session {
			get {
				// Only used for Session_End
				if (session != null)
					return session;

				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				return context.Session;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual ISite Site {
			get {
				return isite;
			}

			set {
				isite = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IPrincipal User {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				if (context.User == null)
					throw new HttpException (Locale.GetText ("No currently authenticated user."));
				
				return context.User;
			}
		}
		
		public virtual event EventHandler Disposed;
		public virtual event EventHandler Error;

		public event EventHandler PreSendRequestHeaders;
		internal void TriggerPreSendRequestHeaders ()
		{
			if (PreSendRequestHeaders != null)
				PreSendRequestHeaders (this, EventArgs.Empty);
		}

		public event EventHandler PreSendRequestContent;
		internal void TriggerPreSendRequestContent ()
		{
			if (PreSendRequestContent != null)
				PreSendRequestContent (this, EventArgs.Empty);
		}
		
		public event EventHandler AcquireRequestState;
		public void AddOnAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AcquireRequestState += new EventHandler (invoker.Invoke);
		}

		public event EventHandler AuthenticateRequest;
		public void AddOnAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AuthenticateRequest += new EventHandler (invoker.Invoke);
		}

		public event EventHandler AuthorizeRequest;
		public void AddOnAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AuthorizeRequest += new EventHandler (invoker.Invoke);
		}

		public event EventHandler BeginRequest;
		public void AddOnBeginRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			BeginRequest += new EventHandler (invoker.Invoke);
		}

		public event EventHandler EndRequest;
		public void AddOnEndRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			EndRequest += new EventHandler (invoker.Invoke);
		}
		
		public event EventHandler PostRequestHandlerExecute;
		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			PostRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public event EventHandler PreRequestHandlerExecute;
		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			PreRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public event EventHandler ReleaseRequestState;
		public void AddOnReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			ReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		public event EventHandler ResolveRequestCache;
		public void AddOnResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			ResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		public event EventHandler UpdateRequestCache;
		public void AddOnUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			UpdateRequestCache += new EventHandler (invoker.Invoke);
		}

#if NET_2_0
		public event EventHandler PostAuthenticateRequest;
		public void AddOnPostAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAuthenticateRequestAsync (bh, eh, null);
		}
			
		public void AddOnPostAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAuthenticateRequest += new EventHandler (invoker.Invoke);
		}
		
		public event EventHandler PostAuthorizeRequest;
		public void AddOnPostAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAuthorizeRequestAsync (bh, eh, null);
		}
		
		public void AddOnPostAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAuthorizeRequest += new EventHandler (invoker.Invoke);
		}

		public event EventHandler PostResolveRequestCache;
		public void AddOnPostResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostResolveRequestCacheAsync (bh, eh, null);
		}
		
		public void AddOnPostResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		public event EventHandler PostMapRequestHandler;
		public void AddOnPostMapRequestHandlerAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostMapRequestHandlerAsync (bh, eh, null);
		}
		
		public void AddOnPostMapRequestHandlerAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostMapRequestHandler += new EventHandler (invoker.Invoke);
		}
		
		public event EventHandler PostAcquireRequestState;
		public void AddOnPostAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAcquireRequestStateAsync (bh, eh, null);
		}
		
		public void AddOnPostAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAcquireRequestState += new EventHandler (invoker.Invoke);
		}
		
		public event EventHandler PostReleaseRequestState;
		public void AddOnPostReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostReleaseRequestStateAsync (bh, eh, null);
		}
		
		public void AddOnPostReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		public event EventHandler PostUpdateRequestCache;
		public void AddOnPostUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostUpdateRequestCacheAsync (bh, eh, null);
		}
		
		public void AddOnPostUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostUpdateRequestCache += new EventHandler (invoker.Invoke);
		}

		//
		// The new overloads that take a data parameter
		//
		public void AddOnAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AcquireRequestState += new EventHandler (invoker.Invoke);
		}

		public void AddOnAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AuthenticateRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AuthorizeRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnBeginRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			BeginRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnEndRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			EndRequest += new EventHandler (invoker.Invoke);
		}
		
		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PreRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public void AddOnReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			ReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		public void AddOnResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			ResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		public void AddOnUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			UpdateRequestCache += new EventHandler (invoker.Invoke);
		}
#endif
		
		internal event EventHandler DefaultAuthentication;
		
		//
		// Bypass all the event on the Http pipeline and go directly to EndRequest
		//
		public void CompleteRequest ()
		{
			stop_processing = true;
		}

		public virtual void Dispose ()
		{
			if (modcoll != null) {
				for (int i = modcoll.Count; i >= 0; i--) {
					modcoll.Get (i).Dispose ();
				}
				modcoll = null;
			}

			if (Disposed != null)
				Disposed (this, EventArgs.Empty);
			
			done.Close ();
			done = null;
		}

		public virtual string GetVaryByCustomString (HttpContext context, string custom)
		{
			if (custom == null) // Sigh
				throw new NullReferenceException ();

			if (0 == String.Compare (custom, "browser", true, CultureInfo.InvariantCulture))
				return context.Request.Browser.Type;

			return null;
		}

		//
		// If we catch an error, queue this error
		//
		void ProcessError (Exception e)
		{
			bool first = context.Error == null;
			
			context.AddError (e);
			if (first){
				if (Error != null){
					try {
						Error (this, EventArgs.Empty);
					} catch (Exception ee){
						context.AddError (ee);
					}
				}
			}
			stop_processing = true;
		}
		
		//
		// Ticks the clock: next step on the pipeline.
		//
		void Tick ()
		{
			try {
				// FIXME: We should use 'if' instead of 'while'!!
				while (pipeline.MoveNext ()){
					if ((bool)pipeline.Current) {
						PipelineDone ();
						break;
					}
				}
			} catch (Exception e) {
				Console.WriteLine ("Tick caught an exception that has not been propagated:\n" + e.GetType().FullName + e.Message + e.StackTrace);
			}
		}

		void Resume ()
		{
			if (in_begin)
				must_yield = false;
			else
				Tick ();
		}
		
		//
		// Invoked when our async callback called from RunHooks completes,
		// we restart the pipeline here.
		//
		void async_callback_completed_cb (IAsyncResult ar)
		{
			if (current_ai.end != null){
				try {
					current_ai.end (ar);
				} catch (Exception e) {
					ProcessError (e);
				}
			}

			Resume ();
		}

		void async_handler_complete_cb (IAsyncResult ar)
		{
			IHttpAsyncHandler async_handler = ((IHttpAsyncHandler) ar.AsyncState);

			try {
				async_handler.EndProcessRequest (ar);
			} catch (Exception e){
				ProcessError (e);
			}
			
			Resume ();
		}

		//
		// This enumerator yields whether processing must be stopped:
		//    true:  processing of the pipeline must be stopped
		//    false: processing of the pipeline must not be stopped
		//
		internal class RunHooksEnumerator : IEnumerable, IEnumerator
		{
			Delegate [] delegates;
			int currentStep = 0;
			HttpApplication app;

			internal RunHooksEnumerator(HttpApplication app, Delegate list)
			{
				this.app = app;
				delegates = list.GetInvocationList ();
			}

			public virtual IEnumerator GetEnumerator() { return this; }
			public virtual object Current { get{ return app.stop_processing; } }
			public virtual void Reset()
			{
				throw new NotImplementedException("HttpApplication.RunHooksEnumerator.Reset called.");
			}
			public virtual bool MoveNext ()
			{
				while (currentStep < delegates.Length) {
					if (ProcessDelegate((EventHandler)delegates[currentStep++]))
						return true;
				}
				return false;
			}

			bool ProcessDelegate(EventHandler d)
			{
				if (d.Target != null && (d.Target is AsyncInvoker)){
					app.current_ai = (AsyncInvoker) d.Target;

					try {
						app.must_yield = true;
						app.in_begin = true;
						app.context.BeginTimeoutPossible ();
						app.current_ai.begin (app, EventArgs.Empty, new AsyncCallback(app.async_callback_completed_cb), app.current_ai.data);
					}
					catch (ThreadAbortException taex){
						object obj = taex.ExceptionState;
						Thread.ResetAbort ();
						app.stop_processing = true;
						if (obj is StepTimeout)
							app.ProcessError (new HttpException ("The request timed out."));
					}
					catch (Exception e){
						app.ProcessError (e);
					}
					finally {
						app.in_begin = false;
						app.context.EndTimeoutPossible ();
					}

					//
					// If things are still moving forward, yield this
					// thread now
					//
					if (app.must_yield)
						return true;
					else if (app.stop_processing)
						return true;
				}
				else {
					try {
						app.context.BeginTimeoutPossible ();
						d (app, EventArgs.Empty);
					} catch (ThreadAbortException taex){
						object obj = taex.ExceptionState;
						Thread.ResetAbort ();
						app.stop_processing = true;
						if (obj is StepTimeout)
							app.ProcessError (new HttpException ("The request timed out."));
					}
					catch (Exception e){
						app.ProcessError (e);
					}
					finally {
						app.context.EndTimeoutPossible ();
					}
					if (app.stop_processing)
						return true;
				}
				return false;
			}
		}

		IEnumerable RunHooks (Delegate list)
		{
			return new RunHooksEnumerator(this, list);
		}

		static void FinalErrorWrite (HttpResponse response, string error)
		{
			try {
				response.Write (error);
				response.Flush (true);
			} catch {
				response.Close ();
			}
		}

		void OutputPage ()
		{
			if (context.Error == null){
				try {
					context.Response.Flush (true);
				} catch (Exception e){
					context.AddError (e);
				}
			}

			Exception error = context.Error;
			if (error != null){
				HttpResponse response = context.Response;

				if (!response.HeadersSent){
					response.ClearHeaders ();
					response.ClearContent ();

					if (error is HttpException){
						response.StatusCode = ((HttpException)error).GetHttpCode ();
					} else {
						error = new HttpException ("", error);
						response.StatusCode = 500;
					}
					if (!RedirectCustomError ())
						FinalErrorWrite (response, ((HttpException) error).GetHtmlErrorMessage ());
					else
						response.Flush (true);
				} else {
					if (!(error is HttpException))
						error = new HttpException ("", error);
					FinalErrorWrite (response, ((HttpException) error).GetHtmlErrorMessage ());
				}
			}
			
		}
		
		//
		// Invoked at the end of the pipeline execution
		//
		void PipelineDone ()
		{
			try {
				if (EndRequest != null)
					EndRequest (this, EventArgs.Empty);
			} catch (Exception e){
				ProcessError (e);
			}

			try {

				OutputPage ();
			} catch (Exception e) {
				Console.WriteLine ("Internal error: OutputPage threw an exception " + e);
			} finally {
				context.WorkerRequest.EndOfRequest();
				if (begin_iar != null){
					try {
						begin_iar.Complete ();
					} catch {
						//
						// TODO: if this throws an error, we have no way of reporting it
						// Not really too bad, since the only failure might be
						// `HttpRuntime.request_processed'
						//
					}
				}
				
				done.Set ();

				if (factory != null && context.Handler != null){
					factory.ReleaseHandler (context.Handler);
					factory = null;
				}
				
				context.Handler = null;
				// context = null; -> moved to PostDone
				pipeline = null;
				current_ai = null;
			}
			PostDone ();
		}

		class PipeLineEnumerator : IEnumerator
		{
			HttpApplication app;
			IEnumerator currentEnumerator = null;
			int currentStep = 0;
			bool pipelineFinished = false;
			IHttpHandler handler = null;
			bool currentVal;
			InternalStepDelegate AllocateHandlerDel;
			InternalStepDelegate ProcessHandlerDel;
			InternalStepDelegate ReleaseHandlerDel;

			// true means that we need to yield and return the current value;
			// false means that we need to go on to the next delegate and return
			// values from there.
			delegate bool InternalStepDelegate();

			internal PipeLineEnumerator(HttpApplication app)
			{
				this.app = app;
				AllocateHandlerDel = new InternalStepDelegate(AllocateHandler);
				ProcessHandlerDel = new InternalStepDelegate(ProcessHandler);
				ReleaseHandlerDel = new InternalStepDelegate(ReleaseHandler);
			}

			public virtual object Current
			{
				get
				{
					if (currentEnumerator != null)
						return currentEnumerator.Current;
					return currentVal;
				}
			}

			// See InternalStepDelegate for meaning of true/false return value
			bool AllocateHandler()
			{
				// Obtain the handler for the request.
				try {
					handler = app.GetHandler (app.context);
				}
				catch (FileNotFoundException fnf){
					if (app.context.Request.IsLocal)
						app.ProcessError (new HttpException (404, String.Format ("File not found {0}", fnf.FileName), fnf));
					else
						app.ProcessError (new HttpException (404, "File not found", fnf));
				} catch (DirectoryNotFoundException dnf){
					app.ProcessError (new HttpException (404, "Directory not found", dnf));
				} catch (Exception e) {
					app.ProcessError (e);
				}

				if (app.stop_processing) {
					currentVal = false;
					return true;
				}
				return false;
			}

			// See InternalStepDelegate for meaning of true/false return value
			bool ProcessHandler()
			{
				//
				// From this point on, we need to ensure that we call
				// ReleaseRequestState, so the code below jumps to
				// `release:' to guarantee it rather than yielding.
				//
				if (app.PreRequestHandlerExecute != null)
					foreach (bool stop in app.RunHooks (app.PreRequestHandlerExecute))
						if (stop)
							return false;

				try {
					app.context.BeginTimeoutPossible ();
					if (handler != null){
						IHttpAsyncHandler async_handler = handler as IHttpAsyncHandler;
					
						if (async_handler != null){
							app.must_yield = true;
							app.in_begin = true;
							async_handler.BeginProcessRequest (app.context, new AsyncCallback(app.async_handler_complete_cb), handler);
						} else {
							app.must_yield = false;
							handler.ProcessRequest (app.context);
						}
					}
				}
				catch (ThreadAbortException taex){
					object obj = taex.ExceptionState;
					Thread.ResetAbort ();
					app.stop_processing = true;
					if (obj is StepTimeout)
						app.ProcessError (new HttpException ("The request timed out."));
				}
				catch (Exception e){
					app.ProcessError (e);
				}
				finally {
					app.in_begin = false;
					app.context.EndTimeoutPossible ();
				}
				if (app.must_yield) {
					currentVal = app.stop_processing;
					return true;
				}
				else if (app.stop_processing)
					return false;
			
				// These are executed after the application has returned
				if (app.PostRequestHandlerExecute != null)
					foreach (bool stop in app.RunHooks (app.PostRequestHandlerExecute))
						if (stop)
							return false;

				return false;
			}

			// See InternalStepDelegate for meaning of true/false return value
			bool ReleaseHandler()
			{
				if (app.ReleaseRequestState != null){
					foreach (bool stop in app.RunHooks (app.ReleaseRequestState)){
						//
						// Ignore the stop signal while release the state
						//
					}
				}

				if (app.stop_processing) {
					currentVal = true;
					return true;
				}
				return false;
			}

			Delegate FindNextDelegate ()
			{
				switch(currentStep++) {
					case  1: return app.BeginRequest;
					case  2: return app.AuthenticateRequest;
					case  3: return app.DefaultAuthentication;
#if NET_2_0
					case  4: return app.PostAuthenticateRequest;
#endif
					case  5: return app.AuthorizeRequest;
#if NET_2_0
					case  6: return app.PostAuthorizeRequest;
#endif
					case  7: return app.ResolveRequestCache;
					case  8: return AllocateHandlerDel;
#if NET_2_0
					case  9: return app.PostResolveRequestCache;
#endif
#if NET_2_0
					case 10: return app.PostMapRequestHandler;
#endif
					case 11: return app.AcquireRequestState;
#if NET_2_0
					case 12: return app.PostAcquireRequestState;
#endif
					case 13: return app.ResolveRequestCache;
					case 14: return ProcessHandlerDel;
					case 15: return ReleaseHandlerDel;
#if NET_2_0
					case 16: return app.PostReleaseRequestState;
#endif
					case 17: return app.UpdateRequestCache;
#if NET_2_0
					case 18: return app.PostUpdateRequestCache;
#endif
					case 19: pipelineFinished = true; return null;
				}
				return null;
			}

			public virtual bool MoveNext ()
			{
				while (!pipelineFinished) {
					if (currentEnumerator != null && currentEnumerator.MoveNext())
						return true;
					currentEnumerator = null;

					Delegate d = FindNextDelegate();
					InternalStepDelegate d1 = d as InternalStepDelegate;
					if (d1 != null) {
						if (d1())
							return true;
					}
					else if (d != null)
						currentEnumerator = app.RunHooks(d).GetEnumerator();
				}

				app.PipelineDone ();
				return false;
			}

			public virtual void Reset()
			{
				throw new NotImplementedException("HttpApplication.PipelineEnumerator.Reset called.");
			}
		}

		IEnumerator Pipeline ()
		{
			return new PipeLineEnumerator(this);
		}

		void PreStart ()
		{
#if !TARGET_J2EE
			HttpRuntime.TimeoutManager.Add (context);
#endif
			Thread th = Thread.CurrentThread;
			if (app_culture != null) {
				prev_app_culture = th.CurrentCulture;
				th.CurrentCulture = app_culture;
			}

			if (appui_culture != null) {
				prev_appui_culture = th.CurrentUICulture;
				th.CurrentUICulture = appui_culture;
			}

#if !TARGET_JVM
			prev_user = Thread.CurrentPrincipal;
#endif
		}

		void PostDone ()
		{
			Thread th = Thread.CurrentThread;
#if !TARGET_JVM
			if (Thread.CurrentPrincipal != prev_user)
				Thread.CurrentPrincipal = prev_user;
#endif
			if (prev_appui_culture != null && prev_appui_culture != th.CurrentUICulture)
				th.CurrentUICulture = prev_appui_culture;
			if (prev_app_culture != null && prev_app_culture != th.CurrentCulture)
				th.CurrentCulture = prev_app_culture;

#if !TARGET_J2EE
			HttpRuntime.TimeoutManager.Remove (context);
#endif
			context = null;
			session = null;
			HttpContext.Current = null;
		}

		void Start (object x)
		{
			InitOnce (true);
			PreStart ();
			stop_processing = false;
			pipeline = Pipeline ();
			Tick ();
		}
	
		// Used by HttpServerUtility.Execute
		internal IHttpHandler GetHandler (HttpContext context)
		{
			HttpRequest request = context.Request;
			string verb = request.RequestType;
			string url = request.FilePath;
			
			HandlerFactoryConfiguration factory_config = (HandlerFactoryConfiguration) HttpContext.GetAppConfig ("system.web/httpHandlers");
			object o = factory_config.LocateHandler (verb, url);
			factory = o as IHttpHandlerFactory;
			IHttpHandler handler;
			
			if (factory == null) {
				handler = (IHttpHandler) o;
			} else {
				handler = factory.GetHandler (context, verb, url, request.PhysicalPath);
			}
			context.Handler = handler;

			return handler;
		}
		
		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			begin_iar = null;
			this.context = context;
			done.Reset ();

			Start (null);
			done.WaitOne ();
		}

		//
		// This is used by FireOnAppStart, when we init the application
		// as the context is required to be set at that point (the user
		// might call methods that require it on that hook).
		//
		internal void SetContext (HttpContext context)
		{
			this.context = context;
		}

		internal void SetSession (HttpSessionState session)
		{
			this.session = session;
		}

		IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData)
		{
			this.context = context;
			done.Reset ();
			
			begin_iar = new AsyncRequestState (done, cb, extraData);
			Start (null);
			return begin_iar;
		}

		void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result)
		{
			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne ();
			begin_iar = null;
		}

		public virtual void Init ()
		{
		}

		bool IHttpHandler.IsReusable {
			get {
				return true;
			}
		}
		
#region internals
		internal void ClearError ()
		{
			context.ClearError ();
		}

		bool RedirectErrorPage (string error_page)
		{
			if (context.Request.QueryString ["aspxerrorpath"] != null)
				return false;

			Response.Redirect (error_page + "?aspxerrorpath=" + Request.Path, false);
			return true;
		}
							
		bool RedirectCustomError ()
		{
			if (!context.IsCustomErrorEnabled)
				return false;
			
			CustomErrorsConfig config = null;
			try {
				config = (CustomErrorsConfig) context.GetConfig ("system.web/customErrors");
			} catch { }
			
			if (config == null) {
				if (context.ErrorPage != null)
					return RedirectErrorPage (context.ErrorPage);
				
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
			
			return RedirectErrorPage (redirect);
		}
#endregion
	}

	//
	// Based on Fritz' Onion's AsyncRequestState class for asynchronous IHttpAsyncHandlers
	// 
	class AsyncRequestState : IAsyncResult {
		AsyncCallback cb;
		object cb_data;
		bool completed;
		ManualResetEvent complete_event = null;
		
		internal AsyncRequestState (ManualResetEvent complete_event, AsyncCallback cb, object cb_data)
		{
			this.cb = cb;
			this.cb_data = cb_data;
			this.complete_event = complete_event;
		}

		internal void Complete ()
		{
			completed = true;
			if (cb != null)
				cb (this);
			
			complete_event.Set ();
		}

		public object AsyncState {
			get {
				return cb_data;
			}
		}

		public bool CompletedSynchronously {
			get {
				return false;
			}
		}

		public bool IsCompleted {
			get {
				return completed;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return complete_event;
			}
		}
	}

#region Helper classes
	
	//
	// A wrapper to keep track of begin/end pairs
	//
	class AsyncInvoker {
		public BeginEventHandler begin;
		public EndEventHandler end;
		public object data;
		
		public AsyncInvoker (BeginEventHandler bh, EndEventHandler eh, object d)
		{
			begin = bh;
			end = eh;
			data = d;
		}

		public AsyncInvoker (BeginEventHandler bh, EndEventHandler eh)
		{
			begin = bh;
			end = eh;
		}
		
		public void Invoke (object sender, EventArgs e)
		{
			throw new Exception ("This is just a dummy");
		}
	}
#endregion
}

