// 
// System.Web.HttpApplicationFactory
//
// Author:
// 	Patrik Torstensson (ptorsten@hotmail.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.SessionState;

namespace System.Web {
	/// <summary>
	/// TODO: We need to compile the global.asax into a type inherited from HttpApplication
	/// </summary>
	[MonoTODO]
	class HttpApplicationFactory {
		private string _appFilename;
		private Type _appType;

		private bool _appInitialized;
		private bool _appFiredEnd;

		private Stack			_appFreePublicList;
		private int				_appFreePublicInstances;
		static private int	_appMaxFreePublicInstances = 32;

		private HttpApplicationState _state;
		Hashtable appTypeEventHandlers;
		static Hashtable appEventNames;

		static IHttpHandler custApplication;

		static private HttpApplicationFactory s_Factory = new HttpApplicationFactory();

		static HttpApplicationFactory ()
		{
			appEventNames = new Hashtable ();
			appEventNames.Add ("Application_Start", null);
			appEventNames.Add ("Application_End", null);
			appEventNames.Add ("Application_BeginRequest", null);
			appEventNames.Add ("Application_AuthenticateRequest", null);
			appEventNames.Add ("Application_AuthorizeRequest", null);
			appEventNames.Add ("Application_ResolveRequestCache", null);
			appEventNames.Add ("Application_AcquireRequestState", null);
			appEventNames.Add ("Application_PreRequestHandlerExecute", null);
			appEventNames.Add ("Application_PostRequestHandlerExecute", null);
			appEventNames.Add ("Application_ReleaseRequestState", null);
			appEventNames.Add ("Application_UpdateRequestCache", null);
			appEventNames.Add ("Application_EndRequest", null);
			appEventNames.Add ("Application_PreSendRequestHeaders", null);
			appEventNames.Add ("Application_PreSendRequestContent", null);
			appEventNames.Add ("Application_Disposed", null);
			appEventNames.Add ("Application_Error", null);
			appEventNames.Add ("Session_Start", null);
			appEventNames.Add ("Session_End", null);
		}
		
		public HttpApplicationFactory() {
			_appInitialized = false;
			_appFiredEnd = false;

			_appFreePublicList = new Stack();
			_appFreePublicInstances = 0;
		}

		static private string GetAppFilename(HttpContext context) {
			return Path.Combine(context.Request.PhysicalApplicationPath, "global.asax");
		}

		private void CompileApp(HttpContext context) {
			if (File.Exists(_appFilename)) {
				// Setup filemonitor for all filedepend also. CacheDependency?

				_appType = GlobalAsaxCompiler.CompileApplicationType (_appFilename, context);
				if (_appType == null)
					throw new ApplicationException ("Error compiling application file (global.asax).");
			} else {
				_appType = typeof (System.Web.HttpApplication);
				_state = new HttpApplicationState ();
			}

			GetApplicationTypeEvents ();
		}

		private bool IsEventHandler (MethodInfo m)
		{
			if (m.ReturnType != typeof (void))
				return false;

			ParameterInfo [] pi = m.GetParameters ();
			if (pi.Length != 2)
				return false;

			if (pi [0].ParameterType != typeof (object) ||
			    pi [1].ParameterType != typeof (EventArgs))
				return false;
			
			return appEventNames.ContainsKey (m.Name);
		}

		void AddEvent (MethodInfo method)
		{
			string name = method.Name;
			ArrayList list;
			list = appTypeEventHandlers [name] as ArrayList;
			if (list == null) {
				list = new ArrayList ();
				appTypeEventHandlers [name] = list;
			}
			list.Add (method);
		}
		
		private void GetApplicationTypeEvents ()
		{
			appTypeEventHandlers = new Hashtable ();
			ArrayList evtMethods = new ArrayList ();
			BindingFlags flags = BindingFlags.Public    |
					     BindingFlags.NonPublic | 
					     BindingFlags.DeclaredOnly |
					     BindingFlags.Instance |
					     BindingFlags.Static;

			MethodInfo [] methods = _appType.GetMethods (flags);
			foreach (MethodInfo m in methods) {
				if (IsEventHandler (m))
					AddEvent (m);
			}

			Type baseType = _appType.BaseType;
			if (baseType == typeof (HttpApplication))
				return;

			flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

			methods = _appType.GetMethods (flags);
			foreach (MethodInfo m in methods) {
				if (IsEventHandler (m))
					AddEvent (m);
			}
		}

		void FireEvents (string method_name, object state_context, object [] args)
		{
			//TODO: anything to do with the context or state?
			ArrayList methods = appTypeEventHandlers [method_name] as ArrayList;
			if (methods == null || methods.Count == 0)
				return;

			foreach (MethodInfo method in methods)
				method.Invoke (this, args);
		}
		
		void FireOnAppStart (HttpContext context)
		{
			FireEvents ("Application_Start", context, new object [] {this, EventArgs.Empty});
		}

		void FireOnAppEnd ()
		{
			FireEvents ("Application_End", null, new object [] {this, EventArgs.Empty});
		}

		void FireOnSessionStart (HttpSessionState state, object source, EventArgs args)
		{
			FireEvents ("Session_Start", state, new object [] {source, args});
		}
		
		void FireOnSessionEnd (HttpSessionState state, object source, EventArgs args)
		{
			FireEvents ("Session_End", state, new object [] {source, args});
		}
	
		private void InitializeFactory(HttpContext context) {
			// TODO: Should we be impersonating here? We are reading a app file.. security issue?

			_appFilename = GetAppFilename(context);

			CompileApp(context);
			FireOnAppStart(context);
		}

		private void Dispose() {
			ArrayList torelease = new ArrayList();
			lock (_appFreePublicList) {
				do {
					torelease.Add(_appFreePublicList.Pop());
				} while (_appFreePublicList.Count > 0);
			}

			if (torelease.Count > 0) {
				foreach (Object obj in torelease) {
					((HttpApplication) obj).Cleanup();
				}
			}

			if (!_appFiredEnd) {
				lock (this) {
					if (!_appFiredEnd) {
						FireOnAppEnd();
						_appFiredEnd = true;
					}
				}
			}
		}

		internal static IHttpHandler GetInstance(HttpContext context)
		{
			if (custApplication != null)
				return custApplication;

			if (!s_Factory._appInitialized) {
				lock (s_Factory) {
					if (!s_Factory._appInitialized) {
						s_Factory.InitializeFactory(context);
						s_Factory._appInitialized = true;
					}
				}
			}

			return s_Factory.GetPublicInstance(context);
		}

		internal static void RecycleInstance(HttpApplication app) {
			if (!s_Factory._appInitialized)
				throw new InvalidOperationException("Factory not intialized");

			s_Factory.RecyclePublicInstance(app);
		}

		void AttachEvents (HttpApplication app)
		{
			foreach (string key in appTypeEventHandlers.Keys) {
				if (key == "Application_Start" || key == "Application_End" ||
				    key == "Sesssion_Start"    || key == "Session_End")
				    continue;

				int pos = key.IndexOf ('_');
				if (pos == -1 || key.Length <= pos + 1)
					continue;

				EventInfo evt = _appType.GetEvent (key.Substring (pos + 1));
				if (evt == null)
					continue;
			
				ArrayList list = appTypeEventHandlers [key] as ArrayList;
				if (list == null || list.Count == 0)
					continue;

				foreach (MethodInfo method in list)
					evt.AddEventHandler (app, Delegate.CreateDelegate (typeof (EventHandler), method));
			}
		}

		private IHttpHandler GetPublicInstance(HttpContext context) {
			HttpApplication app = null;

			lock (_appFreePublicList) {
				if (_appFreePublicInstances > 0) {
					app = (HttpApplication) _appFreePublicList.Pop();
					_appFreePublicInstances--;
				}
			}

			if (app == null) {
				// Create non-public object
				app = (HttpApplication) HttpRuntime.CreateInternalObject(_appType);

				AttachEvents (app);
				app.Startup(context, HttpApplicationFactory.ApplicationState);
			}

			return (IHttpHandler) app;
		}

		internal void RecyclePublicInstance(HttpApplication app) {
			lock (_appFreePublicList) {
				if (_appFreePublicInstances < _appMaxFreePublicInstances) {
					_appFreePublicList.Push(app);
					_appFreePublicInstances++;

					app = null;
				}
			}
			
			if  (app != null) {
				app.Cleanup();
			}
		}

		static internal HttpApplicationState ApplicationState {
			get {
				if (null == s_Factory._state) {
					s_Factory._state = new HttpApplicationState();
				}

				return s_Factory._state;
			}
		}

		internal static void EndApplication() {
			s_Factory.Dispose();
		}

		internal static void StartSession(HttpSessionState state, object source, EventArgs args) {
			s_Factory.FireOnSessionStart(state, source, args);
		}

		static void EndSession(HttpSessionState state, object source, EventArgs args) {
			s_Factory.FireOnSessionEnd(state, source, args);
		}

		public static void SetCustomApplication (IHttpHandler customApplication)
		{
			custApplication = customApplication;
		}
	}
}
