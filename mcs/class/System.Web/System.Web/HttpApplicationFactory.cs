// 
// System.Web.HttpApplicationFactory
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
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
		MethodInfo [] appTypeEventHandlers;
		static IHttpHandler custApplication;

		static private HttpApplicationFactory s_Factory = new HttpApplicationFactory();

		public HttpApplicationFactory() {
			_appInitialized = false;
			_appFiredEnd = false;

			_appFreePublicList = new Stack();
			_appFreePublicInstances = 0;
		}

		static private string GetAppFilename(HttpContext context) {
			return Path.Combine(context.Request.PhysicalApplicationPath, "global.asax");
		}

		[MonoTODO("CompileApp(HttpContext context)")]
		private void CompileApp(HttpContext context) {
			if (File.Exists(_appFilename)) {
				// Setup filemonitor for all filedepend also.

				_appType = GlobalAsaxCompiler.CompileApplicationType (_appFilename);
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
			//Anything else?
			ParameterInfo [] pi = m.GetParameters ();
			if (pi.Length != 2)
				return false;

			if (pi [0].ParameterType != typeof (object) ||
			    pi [1].ParameterType != typeof (EventArgs))
				return false;
				
			if (m.Name != "Application_OnStart" &&
			    m.Name != "Application_OnEnd" &&
			    m.Name != "Session_OnStart" &&
			    m.Name != "Session_OnEnd")
				return false;

			return true;

		}

		private void GetApplicationTypeEvents ()
		{
			ArrayList evtMethods = new ArrayList ();
			BindingFlags flags = BindingFlags.Public    |
					     BindingFlags.NonPublic | 
					     BindingFlags.DeclaredOnly;

			MethodInfo [] methods = _appType.GetMethods (flags);
			foreach (MethodInfo m in methods) {
				if (IsEventHandler (m))
					evtMethods.Add (m);
			}

			Type baseType = _appType.BaseType;
			if (baseType != typeof (HttpApplication)) {
				flags = BindingFlags.NonPublic |
				        BindingFlags.Static    | 
				        BindingFlags.Instance;

				methods = _appType.GetMethods (flags);
				foreach (MethodInfo m in methods) {
					if (IsEventHandler (m))
						evtMethods.Add (m);
				}
			}

			appTypeEventHandlers = (MethodInfo []) evtMethods.ToArray (typeof (MethodInfo));
		}

		[MonoTODO("FireOnAppStart(HttpContext context)")]
		private void FireOnAppStart(HttpContext context) {
			// TODO: Fire ProcessRequest (need to have the compile in place first of the app object)
		}

		[MonoTODO("FireOnAppEnd(HttpContext context)")]
		private void FireOnAppEnd() {
			// TODO: Fire ProcessRequest (need to have the compile in place first of the app object)
		}

		[MonoTODO("FireOnSessionStart")]
		private void FireOnSessionStart(HttpSessionState state, object source, EventArgs args) {
		}
		
		[MonoTODO("FireOnSessionEnd")]
		private void FireOnSessionEnd(HttpSessionState state, object source, EventArgs args) {
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

				// TODO: Attach functions in global.asax to correct modules/app
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
