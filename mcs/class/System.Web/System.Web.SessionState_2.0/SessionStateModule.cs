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

namespace System.Web.SessionState
{
	class CallbackState
	{
		public HttpContext Context;
		public AutoResetEvent AutoEvent;

		public CallbackState (HttpContext context, AutoResetEvent e)
		{
			this.Context = context;
			this.AutoEvent = e;
		}
	}
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class SessionStateModule : IHttpModule
	{
		internal const string HeaderName = "AspFilterSessionId";
		internal const string CookielessFlagName = "_SessionIDManager_IsCookieLess";
		
		static object locker = new object ();
		
		internal static string CookieName {
			get {
				config = GetConfig ();
				if (config == null)
					return null;
				return config.CookieName;
			}
		}
				
#if TARGET_J2EE		
		static private SessionStateSection config {
			get {
				return (SessionStateSection) AppDomain.CurrentDomain.GetData ("SessionStateModule.config");
			}
			set {
				AppDomain.CurrentDomain.SetData ("SessionStateModule.config", value);
			}
		}
		
		static private Type handlerType
		{
			get {
				return (Type) AppDomain.CurrentDomain.GetData ("SessionStateModule.handlerType");
			}
			set {
				AppDomain.CurrentDomain.SetData ("SessionStateModule.handlerType", value);
			}
		}
		
		static private Type idManagerType
		{
			get {
				return (Type) AppDomain.CurrentDomain.GetData ("SessionStateModule.idManagerType");
			}
			set {
				AppDomain.CurrentDomain.SetData ("SessionStateModule.idManagerType", value);
			}
		}
#else
		static SessionStateSection config;
		static Type handlerType;
		static Type idManagerType;
#endif		
		SessionStateStoreProviderBase handler;
		ISessionIDManager idManager;
		HttpApplication app;
		
		// Store state
		bool storeLocked;
		TimeSpan storeLockAge;
		object storeLockId = new object();
		SessionStateActions storeSessionAction;
		SessionStateStoreData storeData;

		// Session state
		bool isReadOnly;
		bool isNew;
		bool supportSessionIDReissue;
		bool supportsExpiration;
		string sessionId;
		HttpSessionStateContainer container;
		
		// config
		static TimeSpan executionTimeout;
		static int executionTimeoutMS;

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public SessionStateModule ()
		{
		}

		public void Dispose ()
		{
		    if (handler!=null)
			handler.Dispose();
		}

		static SessionStateSection GetConfig ()
		{
			lock (locker) {
				if (config != null)
					return config;

				config = (SessionStateSection) WebConfigurationManager.GetSection ("system.web/sessionState");
				SessionStateMode handlerMode = config.Mode;
				
#if TARGET_J2EE
				if (handlerMode == SessionStateMode.SQLServer || handlerMode == SessionStateMode.StateServer)
					throw new NotImplementedException("You must use web.xml to specify session state handling");
#endif
				InitTypesFromConfig (config, handlerMode);
				HttpRuntimeSection runtime = WebConfigurationManager.GetSection ("system.web/httpruntime") as HttpRuntimeSection;
				if (runtime != null) {
					executionTimeout = runtime.ExecutionTimeout;
					executionTimeoutMS = executionTimeout.Milliseconds;
				}
				
				return config;
			}
		}

		static void InitTypesFromConfig (SessionStateSection config, SessionStateMode handlerMode)
		{
 			if (handlerMode == SessionStateMode.StateServer)
 				handlerType = typeof (SessionStateServerHandler);

// 			if (handlerMode == SessionStateMode.SQLServer)
// 				handlerType = typeof (SessionSQLServerHandler);
			
			if (handlerMode == SessionStateMode.InProc)
				handlerType = typeof (SessionInProcHandler);

			if (handlerMode == SessionStateMode.Custom)
				handlerType = GetCustomHandlerType (config);
			
			try {
				idManagerType = Type.GetType (config.SessionIDManagerType, true);
			} catch {
				idManagerType = typeof (SessionIDManager);
			} 
		}

		static Type GetCustomHandlerType (SessionStateSection config)
		{
			return null;
		}
		
		[EnvironmentPermission (SecurityAction.Assert, Read = "MONO_XSP_STATIC_SESSION")]
		public void Init (HttpApplication app)
		{
			SessionStateSection cfg = GetConfig ();
			this.app = app;
			if (handlerType == null || idManagerType == null)
				throw new HttpException ("Cannot initialize the session state module. Missing handler or ID manager types.");
			app.BeginRequest += new EventHandler (OnBeginRequest);
			app.AcquireRequestState += new EventHandler (OnAcquireRequestState);
			app.ReleaseRequestState += new EventHandler (OnReleaseRequestState);
			app.EndRequest += new EventHandler (OnEndRequest);

			if (handler == null) {
				try {
					handler = Activator.CreateInstance (handlerType, new object [] {cfg}) as SessionStateStoreProviderBase;
					handler.Initialize (GetHandlerName (), GetHandlerConfig ());
				} catch (Exception ex) {
					throw new HttpException ("Failed to initialize session storage provider.", ex);
				}
			}

			if (idManager == null) {
				try {
					idManager = Activator.CreateInstance (idManagerType) as ISessionIDManager;
					idManager.Initialize ();
				} catch (Exception ex) {
					throw new HttpException ("Failed to initialize session ID manager.", ex);
				}
			}
		}

		string GetHandlerName ()
		{
			switch (config.Mode) {
				case SessionStateMode.InProc:
				case SessionStateMode.StateServer:
				case SessionStateMode.SQLServer:
					return null; // set by the handler

				case SessionStateMode.Custom:
					return "Custom Session State Handler";

				default:
					throw new HttpException ("Unknown session handler mode.");
			}
		}

		NameValueCollection GetHandlerConfig ()
		{
			switch (config.Mode) {
				case SessionStateMode.InProc:
				case SessionStateMode.StateServer:
				case SessionStateMode.SQLServer:
					return new NameValueCollection ();

				// TODO: implement
				case SessionStateMode.Custom:
					return new NameValueCollection ();

				default:
					throw new HttpException ("Unknown session handler mode.");
			}
		}

		internal static bool IsCookieLess (HttpContext context)
                {
                        if (config.Cookieless == HttpCookieMode.UseCookies)
                                return false;
			if (config.Cookieless == HttpCookieMode.UseUri)
				return true;
                        object cookieless = context.Items [CookielessFlagName];
                        if (cookieless == null)
                                return false;
                        return (bool)cookieless;
                }
		
		void OnBeginRequest (object o, EventArgs args)
		{
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

		void OnAcquireRequestState (object o, EventArgs args)
		{
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
			isReadOnly = (context.Handler is IReadOnlySessionState);			
			
			if (idManager != null) {
				if (idManager.InitializeRequest (context, false, out supportSessionIDReissue))
					return; // Redirected, will come back here in a while
				sessionId = idManager.GetSessionID (context);
			}
			
 			if (handler != null) {
				handler.InitializeRequest (context);
				GetStoreData (context);
				if (storeData == null && !storeLocked) {
					isNew = true;
					sessionId = idManager.CreateSessionID (context);
#if TRACE
					Console.WriteLine ("New session ID allocated: {0}", sessionId);
#endif
					bool redirected = false;
					bool cookieAdded = false;
					idManager.SaveSessionID (context, sessionId, out redirected, out cookieAdded);
					if (redirected) {
						if (supportSessionIDReissue)
							handler.CreateUninitializedItem (context, sessionId, config.Timeout.Minutes);
						context.Response.End();
						return;
					} else
						storeData = handler.CreateNewStoreData (context, config.Timeout.Minutes);
				} else if (storeData == null && storeLocked) {
					WaitForStoreUnlock (context);
				} else if (storeData != null &&
					   !storeLocked &&
					   storeSessionAction == SessionStateActions.InitializeItem &&
					   IsCookieLess (context)) {
					storeData = handler.CreateNewStoreData (context, config.Timeout.Minutes);
				}
				
				SessionSetup (context, isNew);
			}
		}
		
		void OnReleaseRequestState (object o, EventArgs args)
		{
#if TRACE
			Console.WriteLine ("SessionStateModule.OnReleaseRequestState (hash {0})", this.GetHashCode ().ToString ("x"));
			Console.WriteLine ("\tsessionId == {0}", sessionId);
#endif
			if (handler == null)
				return;

			HttpApplication application = (HttpApplication) o;
			HttpContext context = application.Context;
			if (!(context.Handler is IRequiresSessionState))
				return;
#if TRACE
			Console.WriteLine ("\trequest path == {0}", context.Request.FilePath);
			Console.WriteLine ("\tHandler ({0}) requires session state", context.Handler);
#endif
			
			if (!container.IsAbandoned) {
#if TRACE
				Console.WriteLine ("\tnot abandoned");
#endif
				if (!isReadOnly) {
#if TRACE
					Console.WriteLine ("\tnot read only, storing and releasing");
#endif
					handler.SetAndReleaseItemExclusive (context, sessionId, storeData, storeLockId, false);
				} else {
#if TRACE
					Console.WriteLine ("\tread only, releasing");
#endif
					handler.ReleaseItemExclusive (context, sessionId, storeLockId);
				}
				handler.ResetItemTimeout (context, sessionId);
			} else {
				handler.ReleaseItemExclusive (context, sessionId, storeLockId);
				handler.RemoveItem (context, sessionId, storeLockId, storeData);
			}
			SessionStateUtility.RemoveHttpSessionStateFromContext (context);
			if (supportsExpiration)
				SessionStateUtility.RaiseSessionEnd (container, o, args);
		}

		void OnEndRequest (object o, EventArgs args)
		{
			if (handler == null)
				return;
			
			HttpApplication application = o as HttpApplication;
			if (application == null)
				return;
			if (handler != null)
				handler.EndRequest (application.Context);
		}

		void GetStoreData (HttpContext context)
		{
			if (sessionId == null)
				return;
			
			if (isReadOnly)
				storeData = handler.GetItem (context,
							     sessionId,
							     out storeLocked,
							     out storeLockAge,
							     out storeLockId,
							     out storeSessionAction);
			else
				storeData = handler.GetItemExclusive (context,
								      sessionId,
								      out storeLocked,
								      out storeLockAge,
								      out storeLockId,
								      out storeSessionAction);
		}
		
		void WaitForStoreUnlock (HttpContext context)
		{
			AutoResetEvent are = new AutoResetEvent (false);
			TimerCallback tc = new TimerCallback (this.StoreUnlockWaitCallback);
			CallbackState cs = new CallbackState (context, are);
			using (Timer timer = new Timer (tc, cs, 500, 500)) {
				try {
					are.WaitOne (executionTimeout, false);
				} catch {
					storeData = null;
				}
			}
		}

		void StoreUnlockWaitCallback (object s)
		{
			CallbackState state = s as CallbackState;
			GetStoreData (state.Context);
			if (storeData == null && storeLocked && (storeLockAge > executionTimeout)) {
				handler.ReleaseItemExclusive (state.Context, sessionId, storeLockId);
				state.AutoEvent.Set ();
			} else if (storeData != null && !storeLocked)
				state.AutoEvent.Set ();
		}
		
		void SessionSetup (HttpContext context, bool isNew)
		{
			if (storeData != null && sessionId != null) {
				container = new HttpSessionStateContainer (
					sessionId,
					storeData.Items,
					storeData.StaticObjects,
					storeData.Timeout,
					isNew,
					config.Cookieless,
					config.Mode,
					isReadOnly);
				SessionStateUtility.AddHttpSessionStateToContext (context, container);
				if (isNew) {
					supportsExpiration = handler.SetItemExpireCallback (OnSessionExpired);
					OnSessionStart ();
				}
			}
		}

		void OnSessionExpired (string id, SessionStateStoreData item)
		{
		}
		
		void OnSessionStart ()
		{
			if (Start != null)
				Start (this, EventArgs.Empty);
		}
		
		public event EventHandler Start;

		// This event is public, but only Session_[On]End in global.asax will be invoked if present.
		public event EventHandler End;
	}
}
#endif
