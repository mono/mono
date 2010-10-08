//
// System.Web.SessionState.SesionStateModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Stefan Görling (stefan@gorling.se)
//	Jackson Harper (jackson@ximian.com)
//      Marek Habersack (grendello@gmail.com)
//
// Copyright (C) 2002-2006 Novell, Inc (http://www.novell.com)
// (C) 2003 Stefan Görling (http://www.gorling.se)
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

#if NET_2_0
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Configuration;
using System.Web.Caching;
using System.Web.Util;
using System.Security.Permissions;
using System.Threading;
using System.Configuration;
using System.Diagnostics;

namespace System.Web.SessionState
{	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class SessionStateModule : IHttpModule
	{
		class CallbackState
		{
			public readonly HttpContext Context;
			public readonly AutoResetEvent AutoEvent;
			public readonly string SessionId;
			public readonly bool IsReadOnly;

			public CallbackState (HttpContext context, AutoResetEvent e, string sessionId, bool isReadOnly) {
				this.Context = context;
				this.AutoEvent = e;
				this.SessionId = sessionId;
				this.IsReadOnly = isReadOnly;
			}
		}

		internal const string HeaderName = "AspFilterSessionId";
		internal const string CookielessFlagName = "_SessionIDManager_IsCookieLess";

		static readonly object startEvent = new object ();
		static readonly object endEvent = new object ();
		
		SessionStateSection config;

		SessionStateStoreProviderBase handler;
		ISessionIDManager idManager;
		bool supportsExpiration;

		HttpApplication app;

		// Store state
		bool storeLocked;
		TimeSpan storeLockAge;
		object storeLockId;
		SessionStateActions storeSessionAction;
		bool storeIsNew;
		
		// Session state
		SessionStateStoreData storeData;
		HttpSessionStateContainer container;

		// config
		TimeSpan executionTimeout;
		//int executionTimeoutMS;

		EventHandlerList events = new EventHandlerList ();
		
		public event EventHandler Start {
			add { events.AddHandler (startEvent, value); }
			remove { events.RemoveHandler (startEvent, value); }
		}

		// This event is public, but only Session_[On]End in global.asax will be invoked if present.
		public event EventHandler End {
			add { events.AddHandler (endEvent, value); }
			remove { events.RemoveHandler (endEvent, value); }
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public SessionStateModule () {
		}

		public void Dispose () {
			app.BeginRequest -= new EventHandler (OnBeginRequest);
			app.AcquireRequestState -= new EventHandler (OnAcquireRequestState);
			app.ReleaseRequestState -= new EventHandler (OnReleaseRequestState);
			app.EndRequest -= new EventHandler (OnEndRequest);
			handler.Dispose ();
		}

		[EnvironmentPermission (SecurityAction.Assert, Read = "MONO_XSP_STATIC_SESSION")]
		public void Init (HttpApplication app)
		{
			config = (SessionStateSection) WebConfigurationManager.GetSection ("system.web/sessionState");

			ProviderSettings settings;
			switch (config.Mode) {
				case SessionStateMode.Custom:
					settings = config.Providers [config.CustomProvider];
					if (settings == null)
						throw new HttpException (String.Format ("Cannot find '{0}' provider.", config.CustomProvider));
					break;
				case SessionStateMode.Off:
					return;
#if TARGET_J2EE
				default:
					config = new SessionStateSection ();
					config.Mode = SessionStateMode.Custom;
					config.CustomProvider = "ServletSessionStateStore";
					config.SessionIDManagerType = "Mainsoft.Web.SessionState.ServletSessionIDManager";
					config.Providers.Add (new ProviderSettings ("ServletSessionStateStore", "Mainsoft.Web.SessionState.ServletSessionStateStoreProvider"));
					goto case SessionStateMode.Custom;
#else
				case SessionStateMode.InProc:
					settings = new ProviderSettings (null, typeof (SessionInProcHandler).AssemblyQualifiedName);
					break;

				case SessionStateMode.SQLServer:
					settings = new ProviderSettings (null, typeof (SessionSQLServerHandler).AssemblyQualifiedName);
					break;

				case SessionStateMode.StateServer:
					settings = new ProviderSettings (null, typeof (SessionStateServerHandler).AssemblyQualifiedName);
					break;

				default:
					throw new NotImplementedException (String.Format ("The mode '{0}' is not implemented.", config.Mode));
			
#endif
			}

			handler = (SessionStateStoreProviderBase) ProvidersHelper.InstantiateProvider (settings, typeof (SessionStateStoreProviderBase));

			if (String.IsNullOrEmpty(config.SessionIDManagerType)) {
				idManager = new SessionIDManager ();
			} else {
				Type idManagerType = HttpApplication.LoadType (config.SessionIDManagerType, true);
				idManager = (ISessionIDManager)Activator.CreateInstance (idManagerType);
			}

			try {				
				idManager.Initialize ();
			} catch (Exception ex) {
				throw new HttpException ("Failed to initialize session ID manager.", ex);
			}

			supportsExpiration = handler.SetItemExpireCallback (OnSessionExpired);
			HttpRuntimeSection runtime = WebConfigurationManager.GetSection ("system.web/httpRuntime") as HttpRuntimeSection;
			executionTimeout = runtime.ExecutionTimeout;
			//executionTimeoutMS = executionTimeout.Milliseconds;

			this.app = app;

			app.BeginRequest += new EventHandler (OnBeginRequest);
			app.AcquireRequestState += new EventHandler (OnAcquireRequestState);
			app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
			app.EndRequest += new EventHandler (OnEndRequest);
		}

		internal static bool IsCookieLess (HttpContext context, SessionStateSection config) {
			if (config.Cookieless == HttpCookieMode.UseCookies)
				return false;
			if (config.Cookieless == HttpCookieMode.UseUri)
				return true;
			object cookieless = context.Items [CookielessFlagName];
			if (cookieless == null)
				return false;
			return (bool) cookieless;
		}

		void OnBeginRequest (object o, EventArgs args)
		{
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			string file_path = context.Request.FilePath;
			string base_path = VirtualPathUtility.GetDirectory (file_path);
			string id = UrlUtils.GetSessionId (base_path);

			if (id == null)
				return;

			string new_path = UrlUtils.RemoveSessionId (base_path, file_path);
			context.Request.SetFilePath (new_path);
			context.Request.SetHeader (HeaderName, id);
			context.Response.SetAppPathModifier (id);
		}

		void OnAcquireRequestState (object o, EventArgs args) {
			Trace.WriteLine ("SessionStateModule.OnAcquireRequestState (hash " + this.GetHashCode ().ToString ("x") + ")");
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;

			if (!(context.Handler is IRequiresSessionState)) {
				Trace.WriteLine ("Handler (" + context.Handler + ") does not require session state");
				return;
			}
			bool isReadOnly = (context.Handler is IReadOnlySessionState);

			bool supportSessionIDReissue;
			if (idManager.InitializeRequest (context, false, out supportSessionIDReissue))
				return; // Redirected, will come back here in a while
			string sessionId = idManager.GetSessionID (context);

			handler.InitializeRequest (context);

			GetStoreData (context, sessionId, isReadOnly);

			storeIsNew = false;
			if (storeData == null && !storeLocked) {
				storeIsNew = true;
				sessionId = idManager.CreateSessionID (context);
				Trace.WriteLine ("New session ID allocated: " + sessionId);
				bool redirected;
				bool cookieAdded;
				idManager.SaveSessionID (context, sessionId, out redirected, out cookieAdded);
				if (redirected) {
					if (supportSessionIDReissue)
						handler.CreateUninitializedItem (context, sessionId, (int)config.Timeout.TotalMinutes);
					context.Response.End ();
					return;
				}
				else
					storeData = handler.CreateNewStoreData (context, (int)config.Timeout.TotalMinutes);
			}
			else if (storeData == null && storeLocked) {
				WaitForStoreUnlock (context, sessionId, isReadOnly);
			}
			else if (storeData != null &&
				 !storeLocked &&
				 storeSessionAction == SessionStateActions.InitializeItem &&
				 IsCookieLess (context, config)) {
				storeData = handler.CreateNewStoreData (context, (int)config.Timeout.TotalMinutes);
			}

			container = CreateContainer (sessionId, storeData, storeIsNew, isReadOnly);
			SessionStateUtility.AddHttpSessionStateToContext (app.Context, container);
			if (storeIsNew) {
				OnSessionStart ();
				HttpSessionState hss = app.Session;

				if (hss != null)
					storeData.Timeout = hss.Timeout;
			}
		}

		void OnReleaseRequestState (object o, EventArgs args) {

			Trace.WriteLine ("SessionStateModule.OnReleaseRequestState (hash " + this.GetHashCode ().ToString ("x") + ")");

			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			if (!(context.Handler is IRequiresSessionState))
				return;

			Trace.WriteLine ("\tsessionId == " + container.SessionID);
			Trace.WriteLine ("\trequest path == " + context.Request.FilePath);
			Trace.WriteLine ("\tHandler (" + context.Handler + ") requires session state");
			try {
				if (!container.IsAbandoned) {
					Trace.WriteLine ("\tnot abandoned");
					if (!container.IsReadOnly) {
						Trace.WriteLine ("\tnot read only, storing and releasing");
						handler.SetAndReleaseItemExclusive (context, container.SessionID, storeData, storeLockId, storeIsNew);
					}
					else {
						Trace.WriteLine ("\tread only, releasing");
						handler.ReleaseItemExclusive (context, container.SessionID, storeLockId);
					}
					handler.ResetItemTimeout (context, container.SessionID);
				}
				else {
					handler.ReleaseItemExclusive (context, container.SessionID, storeLockId);
					handler.RemoveItem (context, container.SessionID, storeLockId, storeData);
					if (supportsExpiration)
#if TARGET_J2EE
						;
					else
#else
						// Make sure the expiration handler is not called after we will have raised
						// the session end event.
						handler.SetItemExpireCallback (null);
#endif
					SessionStateUtility.RaiseSessionEnd (container, this, args);
				}
				SessionStateUtility.RemoveHttpSessionStateFromContext (context);
			}
			finally {
				container = null;
				storeData = null;
			}
		}

		void OnEndRequest (object o, EventArgs args) {
			if (handler == null)
				return;

			if (container != null)
				OnReleaseRequestState (o, args);

			HttpApplication application = o as HttpApplication;
			if (application == null)
				return;
			if (handler != null)
				handler.EndRequest (application.Context);
		}

		void GetStoreData (HttpContext context, string sessionId, bool isReadOnly) {
			storeData = (isReadOnly) ?
				handler.GetItem (context,
								 sessionId,
								 out storeLocked,
								 out storeLockAge,
								 out storeLockId,
								 out storeSessionAction)
								 :
				handler.GetItemExclusive (context,
									  sessionId,
									  out storeLocked,
									  out storeLockAge,
									  out storeLockId,
									  out storeSessionAction);
			if (storeLockId == null)
				storeLockId = 0;
		}

		void WaitForStoreUnlock (HttpContext context, string sessionId, bool isReadonly) {
			AutoResetEvent are = new AutoResetEvent (false);
			TimerCallback tc = new TimerCallback (StoreUnlockWaitCallback);
			CallbackState cs = new CallbackState (context, are, sessionId, isReadonly);
			using (Timer timer = new Timer (tc, cs, 500, 500)) {
				try {
					are.WaitOne (executionTimeout, false);
				}
				catch {
					storeData = null;
				}
			}
		}

		void StoreUnlockWaitCallback (object s) {
			CallbackState state = (CallbackState) s;

			GetStoreData (state.Context, state.SessionId, state.IsReadOnly);

			if (storeData == null && storeLocked && (storeLockAge > executionTimeout)) {
				handler.ReleaseItemExclusive (state.Context, state.SessionId, storeLockId);
				state.AutoEvent.Set ();
			}
			else if (storeData != null && !storeLocked)
				state.AutoEvent.Set ();
		}

		HttpSessionStateContainer CreateContainer (string sessionId, SessionStateStoreData data, bool isNew, bool isReadOnly) {
			if (data == null)
				return new HttpSessionStateContainer (
					sessionId, null, null, 0, isNew,
					config.Cookieless, config.Mode, isReadOnly);
			
			return new HttpSessionStateContainer (
				sessionId,
				data.Items,
				data.StaticObjects,
				data.Timeout,
				isNew,
				config.Cookieless,
				config.Mode,
				isReadOnly);
		}

		void OnSessionExpired (string id, SessionStateStoreData item) {
			SessionStateUtility.RaiseSessionEnd (
				CreateContainer (id, item, false, true),
				this, EventArgs.Empty);
		}

		void OnSessionStart () {
			EventHandler eh = events [startEvent] as EventHandler;
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
	}
}
#endif
