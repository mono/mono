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
using System.Web.Configuration;
using System.Web.Caching;
using System.Web.Util;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;
using System.Configuration;

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

		// Session state
		SessionStateStoreData storeData;
		HttpSessionStateContainer container;

		// config
		TimeSpan executionTimeout;
		//int executionTimeoutMS;

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
		public void Init (HttpApplication app) {

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
				throw new NotSupportedException (String.Format ("The mode '{0}' is not supported. Only Custom mode is supported and maps to J2EE session.", config.Mode));
#else
			case SessionStateMode.InProc:
				settings = new ProviderSettings (null, typeof (SessionInProcHandler).AssemblyQualifiedName);
				break;
			case SessionStateMode.SQLServer:
			//settings = new ProviderSettings (null, typeof (SessionInProcHandler).AssemblyQualifiedName);
			//break;
			default:
				throw new NotImplementedException (String.Format ("The mode '{0}' is not implemented.", config.Mode));
			case SessionStateMode.StateServer:
				settings = new ProviderSettings (null, typeof (SessionStateServerHandler).AssemblyQualifiedName);
				break;
#endif
			}

			handler = (SessionStateStoreProviderBase) ProvidersHelper.InstantiateProvider (settings, typeof (SessionStateStoreProviderBase));

			try {
				Type idManagerType;
				try {
					idManagerType = Type.GetType (config.SessionIDManagerType, true);
				}
				catch {
					idManagerType = typeof (SessionIDManager);
				}
				idManager = Activator.CreateInstance (idManagerType) as ISessionIDManager;
				idManager.Initialize ();
			}
			catch (Exception ex) {
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

		void OnBeginRequest (object o, EventArgs args) {
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			string base_path = context.Request.BaseVirtualDir;
			string id = UrlUtils.GetSessionId (base_path);

			if (id == null)
				return;

			string new_path = UrlUtils.RemoveSessionId (base_path, context.Request.FilePath);
			context.Request.SetFilePath (new_path);
			context.Request.SetHeader (HeaderName, id);
			context.Response.SetAppPathModifier (String.Concat ("(", id, ")"));
		}

		void OnAcquireRequestState (object o, EventArgs args) {
#if TRACE
			Console.WriteLine ("SessionStateModule.OnAcquireRequestState (hash {0})", this.GetHashCode ().ToString ("x"));
#endif
			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;

			if (!(context.Handler is IRequiresSessionState)) {
#if TRACE
				Console.WriteLine ("Handler ({0}) does not require session state", context.Handler);
#endif
				return;
			}
			bool isReadOnly = (context.Handler is IReadOnlySessionState);

			bool supportSessionIDReissue;
			if (idManager.InitializeRequest (context, false, out supportSessionIDReissue))
				return; // Redirected, will come back here in a while
			string sessionId = idManager.GetSessionID (context);


			handler.InitializeRequest (context);

			GetStoreData (context, sessionId, isReadOnly);

			bool isNew = false;
			if (storeData == null && !storeLocked) {
				isNew = true;
				sessionId = idManager.CreateSessionID (context);
#if TRACE
				Console.WriteLine ("New session ID allocated: {0}", sessionId);
#endif
				bool redirected;
				bool cookieAdded;
				idManager.SaveSessionID (context, sessionId, out redirected, out cookieAdded);
				if (redirected) {
					if (supportSessionIDReissue)
						handler.CreateUninitializedItem (context, sessionId, config.Timeout.Minutes);
					context.Response.End ();
					return;
				}
				else
					storeData = handler.CreateNewStoreData (context, config.Timeout.Minutes);
			}
			else if (storeData == null && storeLocked) {
				WaitForStoreUnlock (context, sessionId, isReadOnly);
			}
			else if (storeData != null &&
				 !storeLocked &&
				 storeSessionAction == SessionStateActions.InitializeItem &&
				 IsCookieLess (context, config)) {
				storeData = handler.CreateNewStoreData (context, config.Timeout.Minutes);
			}

			container = CreateContainer (sessionId, storeData, isNew, isReadOnly);
			SessionStateUtility.AddHttpSessionStateToContext (app.Context, container);
			if (isNew)
				OnSessionStart ();
		}

		void OnReleaseRequestState (object o, EventArgs args) {

#if TRACE
			Console.WriteLine ("SessionStateModule.OnReleaseRequestState (hash {0})", this.GetHashCode ().ToString ("x"));
#endif

			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			if (!(context.Handler is IRequiresSessionState))
				return;

#if TRACE
			Console.WriteLine ("\tsessionId == {0}", container.SessionID);
			Console.WriteLine ("\trequest path == {0}", context.Request.FilePath);
			Console.WriteLine ("\tHandler ({0}) requires session state", context.Handler);
#endif
			try {
				if (!container.IsAbandoned) {
#if TRACE
					Console.WriteLine ("\tnot abandoned");
#endif
					if (!container.IsReadOnly) {
#if TRACE
						Console.WriteLine ("\tnot read only, storing and releasing");
#endif
						handler.SetAndReleaseItemExclusive (context, container.SessionID, storeData, storeLockId, false);
					}
					else {
#if TRACE
						Console.WriteLine ("\tread only, releasing");
#endif
						handler.ReleaseItemExclusive (context, container.SessionID, storeLockId);
					}
					handler.ResetItemTimeout (context, container.SessionID);
				}
				else {
					handler.ReleaseItemExclusive (context, container.SessionID, storeLockId);
					handler.RemoveItem (context, container.SessionID, storeLockId, storeData);
					if (!supportsExpiration)
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
			if (Start != null)
				Start (this, EventArgs.Empty);
		}

		public event EventHandler Start;

		// This event is public, but only Session_[On]End in global.asax will be invoked if present.
		public event EventHandler End;
	}
}
#endif
