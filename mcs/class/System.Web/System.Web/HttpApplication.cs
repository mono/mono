// 
// System.Web.HttpApplication
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Tim Coleman (tim@timcoleman.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2002-2003 Ximian, Inc. (http://www.ximian.com)
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Runtime.Remoting.Messaging;
using System.Web.UI;
using System.Web.Configuration;
using System.Web.SessionState;

namespace System.Web
{

	[ToolboxItem(true)]
	public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable
	{

#region Event Handlers

		// Async event holders
		AsyncEvents _acquireRequestStateAsync;
		AsyncEvents _authenticateRequestAsync;
		AsyncEvents _endRequestAsync;
		AsyncEvents _beginRequestAsync;
		AsyncEvents _authorizeRequestAsync;
		AsyncEvents _updateRequestCacheAsync;
		AsyncEvents _resolveRequestCacheAsync;
		AsyncEvents _releaseRequestStateAsync;
		AsyncEvents _preRequestHandlerExecuteAsync;
		AsyncEvents _postRequestHandlerExecuteAsync;

		// ID objects used to indentify the event
		static object AcquireRequestStateId = new Object ();
		static object AuthenticateRequestId = new Object ();
		static object DefaultAuthenticationId = new Object ();
		static object EndRequestId = new Object ();
		static object DisposedId = new Object ();
		static object BeginRequestId = new Object ();
		static object AuthorizeRequestId = new Object ();
		static object UpdateRequestCacheId = new Object ();
		static object ResolveRequestCacheId = new Object ();
		static object ReleaseRequestStateId = new Object ();
		static object PreSendRequestContentId = new Object ();
		static object PreSendRequestHeadersId = new Object ();
		static object PreRequestHandlerExecuteId = new Object ();
		static object PostRequestHandlerExecuteId = new Object ();
		static object ErrorId = new Object ();

		// List of events
		private EventHandlerList _Events;

		public event EventHandler AcquireRequestState {
			add { Events.AddHandler (AcquireRequestStateId, value); }
			remove { Events.RemoveHandler (AcquireRequestStateId, value); }
		}

		public event EventHandler AuthenticateRequest {
			add { Events.AddHandler (AuthenticateRequestId, value); }
			remove { Events.RemoveHandler (AuthenticateRequestId, value); }
		}

		public event EventHandler AuthorizeRequest {
			add { Events.AddHandler (AuthorizeRequestId, value); }
			remove { Events.RemoveHandler (AuthorizeRequestId, value); }
		}

		public event EventHandler BeginRequest {
			add { Events.AddHandler (BeginRequestId, value); }
			remove { Events.RemoveHandler (BeginRequestId, value); }
		}

		public event EventHandler Disposed {
			add { Events.AddHandler (DisposedId, value); }
			remove { Events.RemoveHandler (DisposedId, value); }
		}

		public event EventHandler EndRequest {
			add { Events.AddHandler (EndRequestId, value); }
			remove { Events.RemoveHandler (EndRequestId, value); }
		}

		public event EventHandler Error {
			add { Events.AddHandler (ErrorId, value); }
			remove { Events.RemoveHandler (ErrorId, value); }
		}

		public event EventHandler PostRequestHandlerExecute {
			add { Events.AddHandler (PostRequestHandlerExecuteId, value); }
			remove { Events.RemoveHandler (PostRequestHandlerExecuteId, value); }
		}

		public event EventHandler PreRequestHandlerExecute {
			add { Events.AddHandler (PreRequestHandlerExecuteId, value); }
			remove { Events.RemoveHandler (PreRequestHandlerExecuteId, value); }
		}

		public event EventHandler PreSendRequestContent {
			add { Events.AddHandler (PreSendRequestContentId, value); }
			remove { Events.RemoveHandler (PreSendRequestContentId, value); }
		}

		public event EventHandler ReleaseRequestState {
			add { Events.AddHandler (ReleaseRequestStateId, value); }
			remove { Events.RemoveHandler (ReleaseRequestStateId, value); }
		}

		public event EventHandler ResolveRequestCache
		{
			add { Events.AddHandler (ResolveRequestCacheId, value); }
			remove { Events.RemoveHandler (ResolveRequestCacheId, value); }
		}

		public event EventHandler UpdateRequestCache {
			add { Events.AddHandler (UpdateRequestCacheId, value); }
			remove { Events.RemoveHandler (UpdateRequestCacheId, value); }
		}

		public event EventHandler PreSendRequestHeaders {
			add { Events.AddHandler (PreSendRequestHeadersId, value); }
			remove { Events.RemoveHandler (PreSendRequestHeadersId, value); }
		}

		internal event EventHandler DefaultAuthentication {
			add { Events.AddHandler (DefaultAuthenticationId, value); }
			remove { Events.RemoveHandler (DefaultAuthenticationId, value); }
		}

		public void AddOnAcquireRequestStateAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _acquireRequestStateAsync)
				_acquireRequestStateAsync = new AsyncEvents ();

			_acquireRequestStateAsync.Add (beg, end);
		}

		public void AddOnAuthenticateRequestAsync(BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _authenticateRequestAsync)
				_authenticateRequestAsync = new AsyncEvents ();

			_authenticateRequestAsync.Add (beg, end);
		}

		public void AddOnAuthorizeRequestAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _authorizeRequestAsync)
				_authorizeRequestAsync = new AsyncEvents ();

			_authorizeRequestAsync.Add (beg, end);
		}

		public void AddOnBeginRequestAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _beginRequestAsync)
				_beginRequestAsync = new AsyncEvents ();

			_beginRequestAsync.Add (beg, end);
		}

		public void AddOnEndRequestAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _endRequestAsync)
				_endRequestAsync = new AsyncEvents ();

			_endRequestAsync.Add (beg, end);
		}

		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _postRequestHandlerExecuteAsync)
				_postRequestHandlerExecuteAsync = new AsyncEvents ();

			_postRequestHandlerExecuteAsync.Add (beg, end);
		}

		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _preRequestHandlerExecuteAsync)
				_preRequestHandlerExecuteAsync = new AsyncEvents ();

			_preRequestHandlerExecuteAsync.Add (beg, end);
		}

		public void AddOnReleaseRequestStateAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _releaseRequestStateAsync)
				_releaseRequestStateAsync = new AsyncEvents ();

			_releaseRequestStateAsync.Add (beg, end);
		}

		public void AddOnResolveRequestCacheAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _resolveRequestCacheAsync)
				_resolveRequestCacheAsync = new AsyncEvents ();

			_resolveRequestCacheAsync.Add (beg, end);
		}

		public void AddOnUpdateRequestCacheAsync (BeginEventHandler beg, EndEventHandler end)
		{
			if (null == _updateRequestCacheAsync)
				_updateRequestCacheAsync = new AsyncEvents ();

			_updateRequestCacheAsync.Add (beg, end);
		}

#endregion

#region Recycle Helper 
		class HandlerFactory 
		{
			public IHttpHandler Handler;
			public IHttpHandlerFactory Factory;

			public HandlerFactory (IHttpHandler handler, IHttpHandlerFactory factory) 
			{
				this.Handler = handler;
				this.Factory = factory;
			}
		}
#endregion

#region State Machine 

		interface IStateHandler
		{
			void Execute();
			bool CompletedSynchronously { get; }
			bool PossibleToTimeout { get; }
		}

		class EventState : IStateHandler
		{
			HttpApplication _app;
			EventHandler _event;

			public EventState (HttpApplication app, EventHandler evt)
			{
				_app = app;
				_event = evt;
			}

			public void Execute ()
			{
				if (null != _event)
					_event (_app, EventArgs.Empty);	
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool PossibleToTimeout {
				get { return true; }
			}
		}

		class AsyncEventState : IStateHandler
		{
			HttpApplication	 _app;
			BeginEventHandler _begin;
			EndEventHandler _end;
			AsyncCallback _callback;
			bool _async;

			public AsyncEventState (HttpApplication app,
						BeginEventHandler begin,
						EndEventHandler end)
			{
				_async = false;
				_app = app;
				_begin = begin;
				_end = end;
				_callback = new AsyncCallback (OnAsyncReady);
			}

			public void Execute ()
			{
				_async = true;
				IAsyncResult ar = _begin (_app, EventArgs.Empty, _callback, null);
				if (ar.CompletedSynchronously) {
					_async = false;
					_end (ar);
				}
			}

			public bool CompletedSynchronously {
				get { return !_async; }
			}

			public bool PossibleToTimeout {
				get {
					// We can't cancel a async event
					return false;
				}
			}

			private void OnAsyncReady (IAsyncResult ar)
			{
				if (ar.CompletedSynchronously)
					return;

				Exception error = null;

				try {
					// Invoke end handler
					_end(ar);
				} catch (Exception exc) {
					// Flow this error to the next state (handle during state execution)
					error = exc;
				}

				_app._state.ExecuteNextAsync (error);
			}
		}

		class AsyncEvents
		{
			ArrayList _events;
			class EventRecord {
				public EventRecord(BeginEventHandler beg, EndEventHandler end)
				{
					Begin = beg;
					End = end;
				}

				public BeginEventHandler	Begin;
				public EndEventHandler		End;
			}

			public AsyncEvents ()
			{
				_events = new ArrayList ();
			}

			public void Add (BeginEventHandler begin, EndEventHandler end)
			{
				_events.Add (new EventRecord (begin, end));
			}

			public void GetAsStates (HttpApplication app, ArrayList states)
			{
				foreach (object obj in _events)
					states.Add (new AsyncEventState (app,
									((EventRecord) obj).Begin,
									((EventRecord) obj).End));
			}
		}


		class ExecuteHandlerState : IStateHandler
		{
			HttpApplication _app;
			AsyncCallback _callback;
			IHttpAsyncHandler _handler;
			bool _async;

			public ExecuteHandlerState (HttpApplication app)
			{
				_app = app;
				_callback = new AsyncCallback (OnAsyncReady);
			}

			private void OnAsyncReady (IAsyncResult ar)
			{
				if (ar.CompletedSynchronously)
					return;

				Exception error = null;

				try {
					// Invoke end handler
					_handler.EndProcessRequest(ar);
				} catch (Exception exc) {
					// Flow this error to the next state (handle during state execution)
					error = exc;
				}

				_handler = null;
				_app._state.ExecuteNextAsync (error);
			}

			public void Execute ()
			{
				IHttpHandler handler = _app.Context.Handler;
				if (handler == null)
					return;

				// Check if we can execute async
				if (handler is IHttpAsyncHandler) {
					_async = true;
					_handler = (IHttpAsyncHandler) handler;

					IAsyncResult ar = _handler.BeginProcessRequest (_app.Context,
											_callback,
											null);

					if (ar.CompletedSynchronously) {
						_async = false;
						_handler = null;
						((IHttpAsyncHandler) handler).EndProcessRequest (ar);
					}
				} else {
					_async = false;

					// Sync handler
					handler.ProcessRequest (_app.Context);
				}
			}

			public bool CompletedSynchronously {
				get { return !_async; }
			}

			public bool PossibleToTimeout {
				get {
					if (_app.Context.Handler is IHttpAsyncHandler)
						return false;

					return true;
				}
			}
		}

		class CreateHandlerState : IStateHandler
		{
			HttpApplication _app;

			public CreateHandlerState (HttpApplication app)
			{
				_app = app;
			}

			public void Execute ()
			{
			_app.Context.Handler = _app.CreateHttpHandler ( _app.Context,
									_app.Request.RequestType,
									_app.Request.FilePath,
									_app.Request.PhysicalPath);
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool PossibleToTimeout {
				get { return false; }
			}
		}

		class FilterHandlerState : IStateHandler
		{
			HttpApplication _app;

			public FilterHandlerState (HttpApplication app)
			{
				_app = app;
			}

			public void Execute ()
			{
				_app.Context.Response.DoFilter (true);
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool PossibleToTimeout {
				get { return true; }
			}
		}

		class StateMachine
		{
			HttpApplication _app;
			WaitCallback _asynchandler;

			IStateHandler [] _handlers;
			int _currentStateIdx;
			int _endStateIdx;
			int _endRequestStateIdx;
			int _countSteps;
			int _countSyncSteps;

			// Helper to create the states for a normal event
			private void GetAsStates (object Event, ArrayList states)
			{
				// Get list of clients for the sync events
				Delegate evnt = _app.Events [Event];
				if (evnt == null)
					return;

				System.Delegate [] clients = evnt.GetInvocationList();
				foreach (Delegate client in clients)
					states.Add (new EventState (_app, (EventHandler) client));
			}

			internal StateMachine (HttpApplication app)
			{
				_app = app;
			}

			internal void Init ()
			{
				_asynchandler = new WaitCallback (OnAsyncCallback);

				// Create a arraylist of states to execute
				ArrayList states = new ArrayList ();

				// BeginRequest
				if (null != _app._beginRequestAsync)
					_app._beginRequestAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.BeginRequestId, states);

				// AuthenticateRequest
				if (null != _app._authenticateRequestAsync)
					_app._authenticateRequestAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.AuthenticateRequestId, states);

				// DefaultAuthentication
				EventHandler defaultAuthHandler = (EventHandler) _app.Events [HttpApplication.DefaultAuthenticationId];
				states.Add (new EventState (_app, defaultAuthHandler));

				// AuthorizeRequest
				if (null != _app._authorizeRequestAsync)
					_app._authorizeRequestAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.AuthorizeRequestId, states);

				// ResolveRequestCache
				if (null != _app._resolveRequestCacheAsync)
					_app._resolveRequestCacheAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.ResolveRequestCacheId, states);

				// [A handler (a page corresponding to the request URL) is created at this point.]
				states.Add (new CreateHandlerState (_app));

				// AcquireRequestState
				if (null != _app._acquireRequestStateAsync)
					_app._acquireRequestStateAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.AcquireRequestStateId, states);

				// PreRequestHandlerExecute
				if (null != _app._preRequestHandlerExecuteAsync)
					_app._preRequestHandlerExecuteAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.PreRequestHandlerExecuteId, states);

				// [The handler is executed.]
				states.Add (new ExecuteHandlerState (_app));

				// PostRequestHandlerExecute
				if (null != _app._postRequestHandlerExecuteAsync)
					_app._postRequestHandlerExecuteAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.PostRequestHandlerExecuteId, states);

				// ReleaseRequestState
				if (null != _app._releaseRequestStateAsync)
					_app._releaseRequestStateAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.ReleaseRequestStateId, states);

				// [Response filters, if any, filter the output.]
				states.Add (new FilterHandlerState (_app));

				// UpdateRequestCache
				if (null != _app._updateRequestCacheAsync)
					_app._updateRequestCacheAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.UpdateRequestCacheId, states);

				// EndRequest
				_endRequestStateIdx = states.Count;
				if (null != _app._endRequestAsync)
					_app._endRequestAsync.GetAsStates (_app, states);
				GetAsStates (HttpApplication.EndRequestId, states);

				// Make list ready to execute
				_handlers = new IStateHandler [states.Count];
				states.CopyTo (_handlers);
			}


			internal void Reset ()
			{
				_countSyncSteps = 0;
				_countSteps = 0;
				_currentStateIdx = -1;
				_endStateIdx = _handlers.Length - 1;
			}

			internal void Start ()
			{
				Reset ();
				ExecuteNextAsync (null);
			}

			internal void ExecuteNextAsync (Exception lasterror)
			{
				if (!Thread.CurrentThread.IsThreadPoolThread)
					ThreadPool.QueueUserWorkItem (_asynchandler, lasterror);
				else
					ExecuteNext (lasterror);
			}

			internal void ExecuteNext (Exception lasterror)
			{
				bool ready_sync = false;
				IStateHandler handler;

				lock (_app) {
					_app.OnStateExecuteEnter ();
					try {
						do {
							if (null != lasterror) {
								_app.HandleError (lasterror);
								lasterror = null;
							}

							// Check if request flow is to be stopped
							if ((_app.GetLastError () != null || _app._CompleteRequest) &&
							    _currentStateIdx < _endRequestStateIdx) {
								_currentStateIdx = _endRequestStateIdx;
								// MS does not filter on error
								_app._Context.Response.DoFilter (false);
							} else if (_currentStateIdx < _endStateIdx) {
								// Get next state handler
								_currentStateIdx++;
							}

							handler = _handlers [_currentStateIdx];
							_countSteps++;
							lasterror = ExecuteState (handler, ref ready_sync);
							if (ready_sync) 
								_countSyncSteps++;
						} while (_currentStateIdx < _endStateIdx);

						if (null != lasterror)
							_app.HandleError (lasterror);
					} finally {
						_app.OnStateExecuteLeave ();
					}
				}
				

				// Finish the request off..
				if (lasterror != null || _currentStateIdx == _endStateIdx) {
					_app._asyncWebResult.Complete ((_countSyncSteps == _countSteps),
								       null,
								       null);

					_app.Context.Handler = null;
					_app.Context.ApplicationInstance = null;
					_app.RecycleHandlers ();
					_app._asyncWebResult = null;

					HttpApplicationFactory.RecycleInstance (_app);
				}
			}

			private void OnAsyncCallback (object obj)
			{
				ExecuteNext ((Exception) obj);
			}

			private Exception ExecuteState (IStateHandler state, ref bool readysync)
			{
				Exception lasterror = null;
				try {
					if (state.PossibleToTimeout) {
						_app.Context.BeginTimeoutPossible ();
					}

					try {
						state.Execute ();	
					} finally {
						if (state.PossibleToTimeout) {
							_app.Context.EndTimeoutPossible ();
						}
					}

					if (state.PossibleToTimeout) {
						// Async Execute
						_app.Context.TryWaitForTimeout ();
					}

					readysync = state.CompletedSynchronously;
				} catch (ThreadAbortException obj) {
					object o = obj.ExceptionState;
					Type type = (o != null) ? o.GetType () : null;
					if (type == typeof (StepTimeout)) {
						Thread.ResetAbort ();
						lasterror = new HttpException ("The request timed out.");
						_app.CompleteRequest ();
					} else if (type == typeof (StepCompleteRequest)) {
						Thread.ResetAbort ();
						_app.CompleteRequest ();
					}
				} catch (Exception obj) {
					lasterror = obj;
				}

				return lasterror;
			}
		}

#endregion

#region Fields

		StateMachine _state;

		bool _CompleteRequest;
		HttpContext _Context;
		Exception _lastError;

		HttpContext _savedContext;
		IPrincipal _savedUser;
		HttpAsyncResult _asyncWebResult;

		ISite _Site;
		HttpModuleCollection _ModuleCollection;
		HttpSessionState _Session;
		HttpApplicationState _appState;
		string assemblyLocation;
		ArrayList _recycleHandlers;

		bool _InPreRequestResponseMode;

		CultureInfo appCulture;
		CultureInfo appUICulture;
		CultureInfo prevAppCulture;
		CultureInfo prevAppUICulture;
#endregion

#region Constructor

		public HttpApplication ()
		{
			assemblyLocation = GetType ().Assembly.Location;
		}

#endregion

#region Methods
		internal IHttpHandler CreateHttpHandler (HttpContext context,
							string type,
							string file,
							string path)
		{
			HandlerFactoryConfiguration handler =
				HttpContext.GetAppConfig ("system.web/httpHandlers")
				as HandlerFactoryConfiguration;

			if (handler == null)
				throw new HttpException ("Cannot get system.web/httpHandlers handler.");

			object result = handler.FindHandler (type, file).Create ();
			if (result is IHttpHandler)
				return (IHttpHandler) result;

			if (result is IHttpHandlerFactory) {
				IHttpHandlerFactory factory = (IHttpHandlerFactory) result;
				try {
					IHttpHandler ret = factory.GetHandler (context, type, file, path);
					if (null != ret) {
						if (null == _recycleHandlers)
							_recycleHandlers = new ArrayList();
						
						_recycleHandlers.Add (new HandlerFactory (ret, factory));
					}
					
					return ret;
				} catch (DirectoryNotFoundException) {
					throw new HttpException (404, "Cannot find '" + file + "'.");
				} catch (FileNotFoundException fnf) {
					string fname = fnf.FileName;
					if (fname != null && fname != "") {
						file = Path.GetFileName (fname);
					}

					throw new HttpException (404, "Cannot find '" + file + "'.");
				}
			}

			throw new HttpException ("Handler not found");
		}

		internal void RecycleHandlers ()
		{
			if (null == _recycleHandlers || _recycleHandlers.Count == 0)
				return;

			foreach (HandlerFactory item in _recycleHandlers) 
				item.Factory.ReleaseHandler (item.Handler);
				
			_recycleHandlers.Clear ();
		}

		internal void InitModules ()
		{
			ModulesConfiguration modules;

			modules = (ModulesConfiguration) HttpContext.GetAppConfig ("system.web/httpModules");
			if (null == modules)
				throw new HttpException (
						HttpRuntime.FormatResourceString ("missing_modules_config"));

			_ModuleCollection = modules.CreateCollection ();
			if (_ModuleCollection == null)
				return;

			int pos, count;

			count = _ModuleCollection.Count;
			for (pos = 0; pos != count; pos++)
				((IHttpModule) _ModuleCollection.Get (pos)).Init (this);
		}

		internal void InitCulture ()
		{
			GlobalizationConfiguration cfg = GlobalizationConfiguration.GetInstance (null);
			if (cfg != null) {
				appCulture = cfg.Culture;
				appUICulture = cfg.UICulture;
			}
		}

		void SaveThreadCulture ()
		{
			Thread th = Thread.CurrentThread;

			if (appCulture != null) {
				prevAppCulture = th.CurrentCulture;
				th.CurrentCulture = appCulture;
			}

			if (appUICulture != null) {
				prevAppUICulture = th.CurrentUICulture;
				th.CurrentUICulture = appUICulture;
			}
		}

		void RestoreThreadCulture ()
		{
			Thread th = Thread.CurrentThread;

			if (prevAppCulture != null) {
				th.CurrentCulture = prevAppCulture;
				prevAppCulture = null;
			}

			if (prevAppUICulture != null) {
				th.CurrentUICulture = prevAppUICulture;
				prevAppUICulture = null;
			}
		}

		internal void OnStateExecuteEnter ()
		{
			SaveThreadCulture ();
			_savedContext = HttpContext.Context;
			HttpContext.Context = _Context;
			HttpRuntime.TimeoutManager.Add (_Context);
			SetPrincipal (Context.User);
		}

		internal void OnStateExecuteLeave ()
		{
			RestoreThreadCulture ();
			HttpRuntime.TimeoutManager.Remove (_Context);
			HttpContext.Context = _savedContext;
			RestorePrincipal ();
		}

		internal void SetPrincipal (IPrincipal principal)
		{
			// Don't overwrite _savedUser if called from inside a step
			if (_savedUser == null)
				_savedUser = Thread.CurrentPrincipal;

			Thread.CurrentPrincipal = principal;
		}

		internal void RestorePrincipal ()
		{
			if (_savedUser == null)
				return;

			Thread.CurrentPrincipal = _savedUser;
			_savedUser = null;
		}

		internal void ClearError ()
		{
			_lastError = null;
		}

		internal Exception GetLastError ()
		{
			if (_Context == null)
				return _lastError;

			return _Context.Error;
		}
		
		internal void HandleError (Exception obj)
		{
			EventHandler handler;			
			bool fire = true;

			if (null != _Context) {
				if (null != _Context.Error) 
					fire = false;

				_Context.AddError (obj);
			} else {
				if (null != _lastError)
					fire = false;

				_lastError = obj;
			}

			if (!fire)
				return;

			// Fire OnError event here
			handler = (EventHandler) Events [HttpApplication.ErrorId];
			if (null != handler) {
				try {
					handler (this, EventArgs.Empty);
				} catch (Exception excp) {
					if (null != _Context)
						_Context.AddError (excp);
				}
			}
		}

		internal void Startup (HttpContext context, HttpApplicationState state)
		{
			_Context = context;

			_appState = state;
			_state = new StateMachine (this);

			// Initialize all IHttpModule(s)
			InitModules ();
			HttpApplicationFactory.AttachEvents (this);
			InitCulture ();

			// Initialize custom application
			_InPreRequestResponseMode = true;
			try {
				Init ();
			} catch (Exception obj) {
				HandleError (obj);
			}

			_InPreRequestResponseMode = false;

			_state.Init ();
		}

		internal void Cleanup ()
		{
			try {
				Dispose ();
			} catch (Exception obj) {
				HandleError (obj);
			}

			if (null != _ModuleCollection) {
				int pos;
				int count = _ModuleCollection.Count;

				for (pos = 0; pos != count; pos++)
					((IHttpModule) _ModuleCollection.Get (pos)).Dispose ();

				_ModuleCollection = null;
			}

			_state = null;
		}

		public void CompleteRequest ()
		{
			_CompleteRequest = true;
		}

		public virtual void Dispose ()
		{
			_Site = null;
			EventHandler disposed = (EventHandler) Events [HttpApplication.DisposedId];
			if (null != disposed) 
				disposed (this, EventArgs.Empty);
		}

		public virtual void Init ()
		{
		}

		public virtual string GetVaryByCustomString (HttpContext context, string custom)
		{
			if (custom.ToLower () == "browser")
				return context.Request.Browser.Type;

			return null;
		}

		IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context,
								    AsyncCallback cb,
								    object extraData)
		{
			_Context = context;
			_Context.ApplicationInstance = this;
			_CompleteRequest = false;

			_asyncWebResult = new HttpAsyncResult (cb, extraData);

			_state.Start ();

			return _asyncWebResult;
		}

		void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result)
		{
			HttpAsyncResult ar = (HttpAsyncResult) result;

			if (null != ar.Error) 
				throw ar.Error;
		}

		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			throw new NotSupportedException (HttpRuntime.FormatResourceString("sync_not_supported"));
		}

		bool IHttpHandler.IsReusable {
			get { return true; }
		}
#endregion 		

#region Properties

		internal string AssemblyLocation {
			get { return assemblyLocation; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpApplicationState Application {
			get { return _appState; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpContext Context {
			get { return _Context; }
		}

		protected EventHandlerList Events {
			get {
				if (null == _Events)
					_Events = new EventHandlerList ();

				return _Events;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpModuleCollection Modules {
			get {
				if (null == _ModuleCollection)
					_ModuleCollection = new HttpModuleCollection ();

				return _ModuleCollection;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpRequest Request {
			get {
				if (null != _Context && (!_InPreRequestResponseMode))
					return _Context.Request;

				throw new HttpException ("Can't get request object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpResponse Response {
			get {
				if (null != _Context && (!_InPreRequestResponseMode))
					return _Context.Response;

				throw new HttpException ("Can't get response object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpServerUtility Server {
			get {
				if (null != _Context)
					return _Context.Server;

				return new HttpServerUtility (this);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpSessionState Session {
			get {
				if (null != _Session)
					return _Session;

				if (null != _Context && null != _Context.Session)
					return _Context.Session;

				throw new HttpException ("Failed to get session object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IPrincipal User {
			get { return _Context.User; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ISite Site {
			get { return _Site; }
			set { _Site = value; }
		}
#endregion Properties
	}

	// Used in HttpResponse.End ()
	class StepCompleteRequest
	{
	}
}

