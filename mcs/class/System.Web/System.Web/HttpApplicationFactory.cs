// 
// System.Web.HttpApplicationFactory
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System;
using System.Collections;
using System.IO;
using System.Web;
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
				// Compile the global application file here
				// Setup filemonitor for all filedepend also.

				// tmp
				_appType = Type.GetType("System.Web.HttpApplication");
			} else {
				_appType = Type.GetType("System.Web.HttpApplication");
			}

			// TODO: Use reflection to find the Session/Application/Custom Module events
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

		static public IHttpHandler GetInstance(HttpContext context) {
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

		static public void RecycleInstance(HttpApplication app) {
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

		public void RecyclePublicInstance(HttpApplication app) {
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

		static public void EndApplication() {
			s_Factory.Dispose();
		}

		static public void StartSession(HttpSessionState state, object source, EventArgs args) {
			s_Factory.FireOnSessionStart(state, source, args);
		}

		static void EndSession(HttpSessionState state, object source, EventArgs args) {
			s_Factory.FireOnSessionEnd(state, source, args);
		}
	}
}
