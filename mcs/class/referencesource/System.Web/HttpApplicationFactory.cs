//------------------------------------------------------------------------------
// <copyright file="HttpApplicationFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * The HttpApplicationFactory class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;

    /*
     * Application Factory only has and public static methods to get / recycle
     * application instances.  The information cached per application
     * config file is encapsulated by ApplicationData class.
     * Only one static instance of application factory is created.
     */
    internal class HttpApplicationFactory {

        internal const string applicationFileName = "global.asax";

        // the only instance of application factory
        private static HttpApplicationFactory _theApplicationFactory = new HttpApplicationFactory();

        // flag to indicate that initialization was done
        private bool _inited;

        // filename for the global.asax
        private String _appFilename;
        private ICollection _fileDependencies;

        // call application on_start only once
        private bool _appOnStartCalled = false;

        // call application on_end only once
        private bool _appOnEndCalled = false;

        // dictionary of application state
        private HttpApplicationState _state;

        // class of the application object
        private Type _theApplicationType;

        // free list of app objects
        private Stack _freeList = new Stack();
        private int _numFreeAppInstances = 0;
        private int _minFreeAppInstances = 0;

        // free list of special (context-less) app objects 
        // to be used for global events (App_OnEnd, Session_OnEnd, etc.)
        private Stack _specialFreeList = new Stack();
        private int _numFreeSpecialAppInstances = 0;
        private const int _maxFreeSpecialAppInstances = 20;

        // results of the reflection on the app class
        private MethodInfo   _onStartMethod;        // Application_OnStart
        private int          _onStartParamCount;
        private MethodInfo   _onEndMethod;          // Application_OnEnd
        private int          _onEndParamCount;
        private MethodInfo   _sessionOnEndMethod;   // Session_OnEnd
        private int          _sessionOnEndParamCount;
        private EventHandler _sessionOnEndEventHandlerAspCompatHelper; // helper for AspCompat
        // list of methods suspected as event handlers
        private MethodInfo[] _eventHandlerMethods;

        internal HttpApplicationFactory() {
            _sessionOnEndEventHandlerAspCompatHelper = new EventHandler(this.SessionOnEndEventHandlerAspCompatHelper);
        }

        internal static void ThrowIfApplicationOnStartCalled() {
            if (_theApplicationFactory._appOnStartCalled) {
                throw new InvalidOperationException(SR.GetString(SR.MethodCannotBeCalledAfterAppStart));
            }
        }

        //
        // Initialization on first request
        //

        private void Init() {
            if (_customApplication != null)
                return;

            try {
                try {
                    _appFilename = GetApplicationFile();

                    CompileApplication();
                }
                finally {
                    // Always set up global.asax file change notification, even if compilation
                    // failed.  This way, if the problem is fixed, the appdomain will be restarted.
                    SetupChangesMonitor();
                }
            }
            catch { // Protect against exception filters
                throw;
            }
        }

        internal static void SetupFileChangeNotifications() {
            // Just call EnsureInited() to make sure global.asax FCN are set up.
            // But don't if we never even got to initialize Fusion
            if (HttpRuntime.CodegenDirInternal != null)
                _theApplicationFactory.EnsureInited();
        }

        private void EnsureInited() {
            if (!_inited) {
                lock (this) {
                    if (!_inited) {
                        Init();
                        _inited = true;
                    }
                }
            }
        }

        internal static void EnsureAppStartCalledForIntegratedMode(HttpContext context, HttpApplication app) {
            if (!_theApplicationFactory._appOnStartCalled) {
                Exception error = null;
                lock (_theApplicationFactory) {
                    if (!_theApplicationFactory._appOnStartCalled) {
                        using (new DisposableHttpContextWrapper(context)) {
                            // impersonation could be required (UNC share or app credentials)
                            
                            WebBaseEvent.RaiseSystemEvent(_theApplicationFactory, WebEventCodes.ApplicationStart);
                            
                            if (_theApplicationFactory._onStartMethod != null) {
                                app.ProcessSpecialRequest(context,
                                                          _theApplicationFactory._onStartMethod,
                                                          _theApplicationFactory._onStartParamCount,
                                                          _theApplicationFactory,
                                                          EventArgs.Empty, 
                                                          null);
                            }
                        }
                    }
                    
                    _theApplicationFactory._appOnStartCalled = true;
                    error = context.Error;
                }
                if (error != null) {
                    throw new HttpException(error.Message, error);
                }
            }
        }

        private void EnsureAppStartCalled(HttpContext context) {
            if (!_appOnStartCalled) {
                lock (this) {
                    if (!_appOnStartCalled) {
                        using (new DisposableHttpContextWrapper(context)) {
                            // impersonation could be required (UNC share or app credentials)

                            WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.ApplicationStart);

                            // fire outside of impersonation as HttpApplication logic takes
                            // care of impersonation by itself
                            FireApplicationOnStart(context);
                        }

                        _appOnStartCalled = true;
                    }
                }
            }
        }

        internal static String GetApplicationFile() {
            return Path.Combine(HttpRuntime.AppDomainAppPathInternal, applicationFileName);
        }

        private void CompileApplication() {
            // Get the Application Type and AppState from the global file

            _theApplicationType = BuildManager.GetGlobalAsaxType();

            BuildResultCompiledGlobalAsaxType result = BuildManager.GetGlobalAsaxBuildResult();

            if (result != null) {

                // Even if global.asax was already compiled, we need to get the collections
                // of application and session objects, since they are not persisted when
                // global.asax is compiled.  Ideally, they would be, but since <object> tags
                // are only there for ASP compat, it's not worth the trouble.
                // Note that we only do this is the rare case where we know global.asax contains
                // <object> tags, to avoid always paying the price (VSWhidbey 453101)
                if (result.HasAppOrSessionObjects) {
                    GetAppStateByParsingGlobalAsax();
                }

                // Remember file dependencies
                _fileDependencies = result.VirtualPathDependencies;
            }

            if (_state == null) {
                _state = new HttpApplicationState();
            }


            // Prepare to hookup event handlers via reflection

            ReflectOnApplicationType();
        }

        private void GetAppStateByParsingGlobalAsax() {
            using (new ApplicationImpersonationContext()) {
                // It may not exist if the app is precompiled
                if (FileUtil.FileExists(_appFilename)) {
                    ApplicationFileParser parser;

                    parser = new ApplicationFileParser();
                    AssemblySet referencedAssemblies = System.Web.UI.Util.GetReferencedAssemblies(
                        _theApplicationType.Assembly);
                    referencedAssemblies.Add(typeof(string).Assembly);
                    VirtualPath virtualPath = HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine(
                        applicationFileName);
                    parser.Parse(referencedAssemblies, virtualPath);

                    // Create app state
                    _state = new HttpApplicationState(parser.ApplicationObjects, parser.SessionObjects);
                }
            }
        }

        private bool ReflectOnMethodInfoIfItLooksLikeEventHandler(MethodInfo m) {
            if (m.ReturnType != typeof(void))
                return false;

            // has to have either no args or two args (object, eventargs)
            ParameterInfo[] parameters = m.GetParameters();

            switch (parameters.Length) {
                case 0:
                    // ok
                    break;
                case 2:
                    // param 0 must be object
                    if (parameters[0].ParameterType != typeof(System.Object))
                        return false;
                    // param 1 must be eventargs
                    if (parameters[1].ParameterType != typeof(System.EventArgs) &&
                        !parameters[1].ParameterType.IsSubclassOf(typeof(System.EventArgs)))
                        return false;
                    // ok
                    break;

                default:
                    return false;
            }

            // check the name (has to have _ not as first or last char)
            String name = m.Name;
            int j = name.IndexOf('_');
            if (j <= 0 || j > name.Length-1)
                return false;

            // special pseudo-events
            if (StringUtil.EqualsIgnoreCase(name, "Application_OnStart") ||
                StringUtil.EqualsIgnoreCase(name, "Application_Start")) {
                _onStartMethod = m;
                _onStartParamCount = parameters.Length;
            }
            else if (StringUtil.EqualsIgnoreCase(name, "Application_OnEnd") ||
                     StringUtil.EqualsIgnoreCase(name, "Application_End")) {
                _onEndMethod = m;
                _onEndParamCount = parameters.Length;
            }
            else if (StringUtil.EqualsIgnoreCase(name, "Session_OnEnd") ||
                     StringUtil.EqualsIgnoreCase(name, "Session_End")) {
                _sessionOnEndMethod = m;
                _sessionOnEndParamCount = parameters.Length;
            }

            return true;
        }

        private void ReflectOnApplicationType() {
            ArrayList handlers = new ArrayList();
            MethodInfo[] methods;

            Debug.Trace("PipelineRuntime", "ReflectOnApplicationType");

            // get this class methods
            methods = _theApplicationType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo m in methods) {
                if (ReflectOnMethodInfoIfItLooksLikeEventHandler(m))
                    handlers.Add(m);
            }
            
            // get base class private methods (GetMethods would not return those)
            Type baseType = _theApplicationType.BaseType;
            if (baseType != null && baseType != typeof(HttpApplication)) {
                methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (MethodInfo m in methods) {
                    if (m.IsPrivate && ReflectOnMethodInfoIfItLooksLikeEventHandler(m))
                        handlers.Add(m);
                }
            }

            // remember as an array
            _eventHandlerMethods = new MethodInfo[handlers.Count];
            for (int i = 0; i < _eventHandlerMethods.Length; i++)
                _eventHandlerMethods[i] = (MethodInfo)handlers[i];
        }

        private void SetupChangesMonitor() {
            FileChangeEventHandler handler = new FileChangeEventHandler(this.OnAppFileChange);

            HttpRuntime.FileChangesMonitor.StartMonitoringFile(_appFilename, handler);

            if (_fileDependencies != null) {
                foreach (string fileName in _fileDependencies) {
                    HttpRuntime.FileChangesMonitor.StartMonitoringFile(
                        HostingEnvironment.MapPathInternal(fileName), handler);
                }
            }
        }

        private void OnAppFileChange(Object sender, FileChangeEvent e) {
            // shutdown the app domain if app file changed
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because of application file change");
            string message = FileChangesMonitor.GenerateErrorMessage(e.Action, e.FileName);
            if (message == null) {
                message = "Change in GLOBAL.ASAX";
            }
            HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.ChangeInGlobalAsax, message);
        }

        //
        //  Application instance management
        //

        private HttpApplication GetNormalApplicationInstance(HttpContext context) {
            HttpApplication app = null;

            lock (_freeList) {
                if (_numFreeAppInstances > 0) {
                    app = (HttpApplication)_freeList.Pop();
                    _numFreeAppInstances--;

                    if (_numFreeAppInstances < _minFreeAppInstances) {
                        _minFreeAppInstances = _numFreeAppInstances;
                    }
                }
            }

            if (app == null) {
                // If ran out of instances, create a new one
                app = (HttpApplication)HttpRuntime.CreateNonPublicInstance(_theApplicationType);

                using (new ApplicationImpersonationContext()) {
                    app.InitInternal(context, _state, _eventHandlerMethods);
                }
            }

            if (AppSettings.UseTaskFriendlySynchronizationContext) {
                // When this HttpApplication instance is no longer in use, recycle it.
                app.ApplicationInstanceConsumersCounter = new CountdownTask(1); // representing required call to HttpApplication.ReleaseAppInstance
                app.ApplicationInstanceConsumersCounter.Task.ContinueWith((_, o) => RecycleApplicationInstance((HttpApplication)o), app, TaskContinuationOptions.ExecuteSynchronously);
            }
            return app;
        }

        private void RecycleNormalApplicationInstance(HttpApplication app) {
            lock (_freeList) {
                _freeList.Push(app);
                _numFreeAppInstances++;
            }
        }

        private void TrimApplicationInstanceFreeList(bool trimAll = false) {
            // reset last min length
            int minFreeAppInstances = _minFreeAppInstances;
            _minFreeAppInstances = _numFreeAppInstances;

            // if free list is empty or was empty since last trim, don't trim now
            if (minFreeAppInstances <= 1) {
                return;
            }

            ArrayList apps = null;

            lock (_freeList) {
                if (_numFreeAppInstances > 1) {
                    apps = new ArrayList();

                    // trim a percentage at a time or 1 item
                    int trimCount = (_numFreeAppInstances * 3)/100 + 1;  // 3% at the time

                    while (trimCount > 0) {
                        apps.Add(_freeList.Pop());
                        _numFreeAppInstances--;
                        trimCount--;
                    }
                    _minFreeAppInstances = _numFreeAppInstances;
                }
            }

            // dispose the applications that were removed (if any)
            if (apps != null) {
                foreach (HttpApplication app in apps) {
                    app.DisposeInternal();
                }
            }
        }

        internal static HttpApplication GetPipelineApplicationInstance(IntPtr appContext, HttpContext context) {
            _theApplicationFactory.EnsureInited();
            return _theApplicationFactory.GetSpecialApplicationInstance(appContext, context);
        }

        internal static void RecyclePipelineApplicationInstance(HttpApplication app) {
            _theApplicationFactory.RecycleSpecialApplicationInstance(app);
        }

        private HttpApplication GetSpecialApplicationInstance(IntPtr appContext, HttpContext context) {
            HttpApplication app = null;

            lock (_specialFreeList) {
                if (_numFreeSpecialAppInstances > 0) {
                    app = (HttpApplication)_specialFreeList.Pop();
                    _numFreeSpecialAppInstances--;
                }
            }
            
            if (app == null) {
                //
                //  Put the context on the thread, to make it available to anyone calling
                //  HttpContext.Current from the HttpApplication constructor or module Init
                //
                using (new DisposableHttpContextWrapper(context)) {
                    // If ran out of instances, create a new one
                    app = (HttpApplication)HttpRuntime.CreateNonPublicInstance(_theApplicationType);

                    using (new ApplicationImpersonationContext()) {
                        app.InitSpecial(_state, _eventHandlerMethods, appContext, context);
                    }
                }
            }

            return app;
        }

        private HttpApplication GetSpecialApplicationInstance() {
            return GetSpecialApplicationInstance(IntPtr.Zero, null);
        }

        private void RecycleSpecialApplicationInstance(HttpApplication app) {
            if (_numFreeSpecialAppInstances < _maxFreeSpecialAppInstances) {
                lock (_specialFreeList) {
                    _specialFreeList.Push(app);
                    _numFreeSpecialAppInstances++;
                }
            }
            // else: don't dispose these
        }

        //
        //  Application on_start / on_end
        //

        private void FireApplicationOnStart(HttpContext context) {
            if (_onStartMethod != null) {
                HttpApplication app = GetSpecialApplicationInstance();

                app.ProcessSpecialRequest(
                                         context,
                                         _onStartMethod,
                                         _onStartParamCount,
                                         this, 
                                         EventArgs.Empty, 
                                         null);

                RecycleSpecialApplicationInstance(app);
            }
        }

        private void FireApplicationOnEnd() {
            if (_onEndMethod != null) {
                HttpApplication app = GetSpecialApplicationInstance();

                app.ProcessSpecialRequest(
                                         null,
                                         _onEndMethod, 
                                         _onEndParamCount,
                                         this, 
                                         EventArgs.Empty, 
                                         null);

                RecycleSpecialApplicationInstance(app);
            }
        }

        //
        //  Session on_start / on_end
        //

        class AspCompatSessionOnEndHelper {
            private HttpApplication _app;
            private HttpSessionState _session;
            private Object _eventSource;
            private EventArgs _eventArgs;

            internal AspCompatSessionOnEndHelper(HttpApplication app, HttpSessionState session, Object eventSource, EventArgs eventArgs) {
                _app = app;
                _session = session;
                _eventSource = eventSource;
                _eventArgs = eventArgs;
            }

            internal HttpApplication Application { get { return _app; } }
            internal HttpSessionState Session { get { return _session; } }
            internal Object Source { get { return _eventSource; } }
            internal EventArgs Args { get { return _eventArgs; } }
        }

        private void SessionOnEndEventHandlerAspCompatHelper(Object eventSource, EventArgs eventArgs) {
            AspCompatSessionOnEndHelper helper = (AspCompatSessionOnEndHelper)eventSource;

            helper.Application.ProcessSpecialRequest(
                                                     null,
                                                     _sessionOnEndMethod,
                                                     _sessionOnEndParamCount,
                                                     helper.Source, 
                                                     helper.Args, 
                                                     helper.Session);
        }

        private void FireSessionOnEnd(HttpSessionState session, Object eventSource, EventArgs eventArgs) {
            if (_sessionOnEndMethod != null) {
                HttpApplication app = GetSpecialApplicationInstance();
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                if (AspCompatApplicationStep.AnyStaObjectsInSessionState(session) || HttpRuntime.ApartmentThreading) {
                    AspCompatSessionOnEndHelper helper = new AspCompatSessionOnEndHelper(app, session, eventSource, eventArgs);

                    AspCompatApplicationStep.RaiseAspCompatEvent(
                                            null, 
                                            app,
                                            session.SessionID,
                                            _sessionOnEndEventHandlerAspCompatHelper, 
                                            helper, 
                                            EventArgs.Empty);
                }
                else {
#endif // !FEATURE_PAL
                    app.ProcessSpecialRequest(
                                            null,
                                            _sessionOnEndMethod,
                                            _sessionOnEndParamCount,
                                            eventSource, 
                                            eventArgs, 
                                            session);
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                }
#endif // !FEATURE_PAL

                RecycleSpecialApplicationInstance(app);
            }
        }

        private void FireApplicationOnError(Exception error) {
            HttpApplication app = GetSpecialApplicationInstance();
            app.RaiseErrorWithoutContext(error);
            RecycleSpecialApplicationInstance(app);
        }

        //
        //  Dispose resources associated with the app factory
        //

        private void Dispose() {
            // dispose all 'normal' application instances
            DisposeHttpApplicationInstances(_freeList, ref _numFreeAppInstances);

            // call application_onEnd (only if application_onStart was called before)
            if (_appOnStartCalled && !_appOnEndCalled) {
                lock (this) {
                    if (!_appOnEndCalled) {
                        FireApplicationOnEnd();
                        _appOnEndCalled = true;
                    }
                }
            }

            // dispose all 'special' application instances (DevDiv #109006)
            if (!AppSettings.DoNotDisposeSpecialHttpApplicationInstances) {
                DisposeHttpApplicationInstances(_specialFreeList, ref _numFreeSpecialAppInstances);
            }
        }

        private static void DisposeHttpApplicationInstances(Stack freeList, ref int numFreeInstances) {
            // freeList is the stack of HttpApplication instances, and numFreeInstances is the number of elements we're allowed to pop from the stack

            List<HttpApplication> instances;
            lock (freeList) {
                instances = new List<HttpApplication>(numFreeInstances); // access 'numFreeInstances' only from within the lock
                for (; numFreeInstances > 0; numFreeInstances--) {
                    instances.Add((HttpApplication)freeList.Pop());
                }
            }

            // dispose of each instance outside the lock
            foreach (HttpApplication instance in instances) {
                instance.DisposeInternal();
            }
        }

        //
        // Static methods for outside use
        //

        // custom application -- every request goes directly to the same handler
        private static IHttpHandler _customApplication;

        internal static void SetCustomApplication(IHttpHandler customApplication) {
            // ignore this in app domains where we execute requests (ASURT 128047)
            if (HttpRuntime.AppDomainAppId == null) // only if 'clean' app domain
                _customApplication = customApplication;
        }

        internal static IHttpHandler GetApplicationInstance(HttpContext context) {
            if (_customApplication != null)
                return _customApplication;

            // Check to see if it's a debug auto-attach request
            if (context.Request.IsDebuggingRequest)
                return new HttpDebugHandler();

            _theApplicationFactory.EnsureInited();

            _theApplicationFactory.EnsureAppStartCalled(context);

            return _theApplicationFactory.GetNormalApplicationInstance(context);
        }

        internal static void RecycleApplicationInstance(HttpApplication app) {
            _theApplicationFactory.RecycleNormalApplicationInstance(app);
        }

        internal static void TrimApplicationInstances(bool removeAll = false) {
            if (_theApplicationFactory != null) {
                if (removeAll) {
                    // Remove all pooled HttpApplication instances (potentially reclaiming memory eagerly)
                    DisposeHttpApplicationInstances(_theApplicationFactory._freeList, ref _theApplicationFactory._numFreeAppInstances);
                }
                else {
                    // Remove only some pooled HttpApplication instances
                    _theApplicationFactory.TrimApplicationInstanceFreeList();
                }
            }
        }

        internal static void EndApplication() {
            _theApplicationFactory.Dispose();
        }

        internal static void EndSession(HttpSessionState session, Object eventSource, EventArgs eventArgs) {
            _theApplicationFactory.FireSessionOnEnd(session, eventSource, eventArgs);
        }

        internal static void RaiseError(Exception error) {
            _theApplicationFactory.EnsureInited(); // VSWhidbey 482346
            _theApplicationFactory.FireApplicationOnError(error);
        }

        internal static HttpApplicationState ApplicationState {
            get {
                HttpApplicationState state = _theApplicationFactory._state;
                if (state == null)
                    state = new HttpApplicationState();
                return state;
            }
        }
    }

}
