//------------------------------------------------------------------------------
// <copyright file="httpserverutility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Server intrinsic used to match ASP's object model
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

// Don't entity encode high chars (160 to 256), to fix bugs VSWhidbey 85857/111927
// 
#define ENTITY_ENCODE_HIGH_ASCII_CHARS

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class ErrorFormatterGenerator {
        internal abstract ErrorFormatter GetErrorFormatter(Exception e);
    }


    /// <devdoc>
    ///    <para>
    ///       Provides several
    ///       helper methods that can be used in the processing of Web requests.
    ///    </para>
    /// </devdoc>
    public sealed class HttpServerUtility {
        private HttpContext _context;
        private HttpApplication _application;

        private static IDictionary _cultureCache = Hashtable.Synchronized(new Hashtable());

        internal HttpServerUtility(HttpContext context) {
            _context = context;
        }

        internal HttpServerUtility(HttpApplication application) {
            _application = application;
        }

        //
        // Misc ASP compatibility methods
        //


        /// <devdoc>
        ///    <para>
        ///       Instantiates a COM object identified via a progid.
        ///    </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public object CreateObject(string progID) {
            EnsureHasNotTransitionedToWebSocket();

            Type type = null;
            object obj = null;

            try {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                type = Type.GetTypeFromProgID(progID);
#else // !FEATURE_PAL
                throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL
            }
            catch {
            }

            if (type == null) {
                throw new HttpException(SR.GetString(SR.Could_not_create_object_of_type, progID));
            }

            // Disallow Apartment components in non-compat mode
            AspCompatApplicationStep.CheckThreadingModel(progID, type.GUID);

            // Instantiate the object
            obj = Activator.CreateInstance(type);

            // For ASP compat: take care of OnPageStart/OnPageEnd
            AspCompatApplicationStep.OnPageStart(obj);

            return obj;
        }


        /// <devdoc>
        ///    <para>
        ///       Instantiates a COM object identified via a Type.
        ///    </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public object CreateObject(Type type) {
            EnsureHasNotTransitionedToWebSocket();

            // Disallow Apartment components in non-compat mode
            AspCompatApplicationStep.CheckThreadingModel(type.FullName, type.GUID);

            // Instantiate the object
            Object obj = Activator.CreateInstance(type);

            // For ASP compat: take care of OnPageStart/OnPageEnd
            AspCompatApplicationStep.OnPageStart(obj);

            return obj;
        }



        /// <devdoc>
        ///    <para>
        ///       Instantiates a COM object identified via a clsid.
        ///    </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public object CreateObjectFromClsid(string clsid) {
            EnsureHasNotTransitionedToWebSocket();

            Type type = null;
            object obj = null;

            // Create a Guid out of it
            Guid guid = new Guid(clsid);

            // Disallow Apartment components in non-compat mode
            AspCompatApplicationStep.CheckThreadingModel(clsid, guid);

            try {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                type = Type.GetTypeFromCLSID(guid, null, true /*throwOnError*/);
#else // !FEATURE_PAL
                throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL

                // Instantiate the object
                obj = Activator.CreateInstance(type);
            }
            catch {
            }

            if (obj == null) {
                throw new HttpException(
                    SR.GetString(SR.Could_not_create_object_from_clsid, clsid));
            }

            // For ASP compat: take care of OnPageStart/OnPageEnd
            AspCompatApplicationStep.OnPageStart(obj);

            return obj;
        }

        // Internal static method that returns a read-only, non-user override accounted, CultureInfo object
        internal static CultureInfo CreateReadOnlyCultureInfo(string name) {
            if (!_cultureCache.Contains(name)) {
                // To be threadsafe, get the lock before creating
                lock (_cultureCache) {
                    if (_cultureCache[name] == null) {
                        _cultureCache[name] = CultureInfo.ReadOnly(new CultureInfo(name));
                    }
                }
            }
            return (CultureInfo)_cultureCache[name];
        }

        // Internal static method that returns a read-only, non-user override accounted, culture specific CultureInfo object
        internal static CultureInfo CreateReadOnlySpecificCultureInfo(string name) {
            if(name.IndexOf('-') > 0) {
                return CreateReadOnlyCultureInfo(name);
            }
            CultureInfo ci = CultureInfo.CreateSpecificCulture(name);
            if (!_cultureCache.Contains(ci.Name)) {
                //To be threadsafe, get the lock before creating
                lock (_cultureCache) {
                    if (_cultureCache[ci.Name] == null) {
                        _cultureCache[ci.Name] = CultureInfo.ReadOnly(ci);
                    }
                }
            }
            return (CultureInfo)_cultureCache[ci.Name];
        }

        // Internal static method that returns a read-only, non-user override accounted, CultureInfo object
        internal static CultureInfo CreateReadOnlyCultureInfo(int culture) {
            if (!_cultureCache.Contains(culture)) {
                // To be threadsafe, get the lock before creating
                lock (_cultureCache) {
                    if (_cultureCache[culture] == null) {
                        _cultureCache[culture] = CultureInfo.ReadOnly(new CultureInfo(culture));
                    }
                }
            }
            return (CultureInfo)_cultureCache[culture];
        }

        /// <devdoc>
        ///    <para>
        ///       Maps a virtual path to a physical path.
        ///    </para>
        /// </devdoc>
        public string MapPath(string path) {
            if (_context == null)
                throw new HttpException(SR.GetString(SR.Server_not_available));
            // Disable hiding the request so that Server.MapPath works when called from
            // Application_Start in integrated mode
            bool unhideRequest = _context.HideRequestResponse;
            string realPath;
            try {
                if (unhideRequest) {
                    _context.HideRequestResponse = false;
                }
                realPath = _context.Request.MapPath(path);
            }
            finally {
                if (unhideRequest) {
                    _context.HideRequestResponse = true;
                }
            }
            return realPath;
        }


        /// <devdoc>
        ///    <para>Returns the last recorded exception.</para>
        /// </devdoc>
        public Exception GetLastError() {
            if (_context != null)
                return _context.Error;
            else if (_application != null)
                return _application.LastError;
            else
                return null;
        }


        /// <devdoc>
        ///    <para>Clears the last error.</para>
        /// </devdoc>
        public void ClearError() {
            if (_context != null)
                _context.ClearError();
            else if (_application != null)
                _application.ClearError();
        }

        //
        // Server.Transfer/Server.Execute -- child requests
        //


        /// <devdoc>
        ///    <para>
        ///       Executes a new request (using the specified URL path as the target). Unlike
        ///       the Transfer method, execution of the original page continues after the executed
        ///       page completes.
        ///    </para>
        /// </devdoc>
        public void Execute(string path) {
            Execute(path, null, true /*preserveForm*/);
        }


        /// <devdoc>
        ///    <para>
        ///       Executes a new request (using the specified URL path as the target). Unlike
        ///       the Transfer method, execution of the original page continues after the executed
        ///       page completes.
        ///    </para>
        /// </devdoc>
        public void Execute(string path, TextWriter writer) {
            Execute(path, writer, true /*preserveForm*/);
        }


        /// <devdoc>
        ///    <para>
        ///       Executes a new request (using the specified URL path as the target). Unlike
        ///       the Transfer method, execution of the original page continues after the executed
        ///       page completes.
        ///       If preserveForm is false, the QueryString and Form collections are cleared.
        ///    </para>
        /// </devdoc>
        public void Execute(string path, bool preserveForm) {
            Execute(path, null, preserveForm);
        }


        /// <devdoc>
        ///    <para>
        ///       Executes a new request (using the specified URL path as the target). Unlike
        ///       the Transfer method, execution of the original page continues after the executed
        ///       page completes.
        ///       If preserveForm is false, the QueryString and Form collections are cleared.
        ///    </para>
        /// </devdoc>
        public void Execute(string path, TextWriter writer, bool preserveForm) {
            EnsureHasNotTransitionedToWebSocket();

            if (_context == null)
                throw new HttpException(SR.GetString(SR.Server_not_available));

            if (path == null)
                throw new ArgumentNullException("path");

            string queryStringOverride = null;
            HttpRequest request = _context.Request;
            HttpResponse response = _context.Response;

            // Remove potential cookie-less session id (ASURT 100558)
            path = response.RemoveAppPathModifier(path);

            // Allow query string override
            int iqs = path.IndexOf('?');
            if (iqs >= 0) {
                queryStringOverride = path.Substring(iqs+1);
                path = path.Substring(0, iqs);
            }

            if (!UrlPath.IsValidVirtualPathWithoutProtocol(path)) {
                throw new ArgumentException(SR.GetString(SR.Invalid_path_for_child_request, path));
            }

            VirtualPath virtualPath = VirtualPath.Create(path);

            // Find the handler for the path

            IHttpHandler handler = null;

            string physPath = request.MapPath(virtualPath);        // get physical path
            VirtualPath filePath = request.FilePathObject.Combine(virtualPath);    // vpath

            // Demand read access to the physical path of the target handler
            InternalSecurityPermissions.FileReadAccess(physPath).Demand();

            // We need to Assert since there typically is user code on the stack (VSWhidbey 270965)
            if (HttpRuntime.IsLegacyCas) {
                InternalSecurityPermissions.Unrestricted.Assert();
            }

            try {
                // paths that ends with . are disallowed as they are used to get around
                // extension mappings and server source as static file
                if (StringUtil.StringEndsWith(virtualPath.VirtualPathString, '.'))
                    throw new HttpException(404, String.Empty);

                bool useAppConfig = !filePath.IsWithinAppRoot;

                using (new DisposableHttpContextWrapper(_context)) {

                    try {
                        // We need to increase the depth when calling MapHttpHandler,
                        // since PageHandlerFactory relies on it
                        _context.ServerExecuteDepth++;
                        
                        if (_context.WorkerRequest is IIS7WorkerRequest) {
                            handler = _context.ApplicationInstance.MapIntegratedHttpHandler(
                                _context,
                                request.RequestType,
                                filePath,
                                physPath,
                                useAppConfig,
                                true /*convertNativeStaticFileModule*/);
                        }
                        else {
                            handler = _context.ApplicationInstance.MapHttpHandler(
                                _context,
                                request.RequestType,
                                filePath,
                                physPath,
                                useAppConfig);
                        }
                    }
                    finally {
                        _context.ServerExecuteDepth--;
                    }
                }
            }
            catch (Exception e) {
                // 500 errors (compilation errors) get preserved
                if (e is HttpException) {
                    int code = ((HttpException)e).GetHttpCode();

                    if (code != 500 && code != 404) {
                        e = null;
                    }
                }

                throw new HttpException(SR.GetString(SR.Error_executing_child_request_for_path, path), e);
            }

            ExecuteInternal(handler, writer, preserveForm, true /*setPreviousPage*/,
                virtualPath, filePath, physPath, null, queryStringOverride);
        }


        public void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm) {
            if (_context == null)
                throw new HttpException(SR.GetString(SR.Server_not_available));

            Execute(handler, writer, preserveForm, true /*setPreviousPage*/);
        }

        internal void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm, bool setPreviousPage) {
            HttpRequest request = _context.Request;
            VirtualPath filePath = request.CurrentExecutionFilePathObject;
            string physicalPath = request.MapPath(filePath);

            ExecuteInternal(handler, writer, preserveForm, setPreviousPage,
                null, filePath, physicalPath, null, null);
        }

        private void ExecuteInternal(IHttpHandler handler, TextWriter writer, bool preserveForm, bool setPreviousPage,
            VirtualPath path, VirtualPath filePath, string physPath, Exception error, string queryStringOverride) {

            EnsureHasNotTransitionedToWebSocket();

            if (handler == null)
                throw new ArgumentNullException("handler");

            HttpRequest request = _context.Request;
            HttpResponse response = _context.Response;
            HttpApplication app = _context.ApplicationInstance;

            HttpValueCollection savedForm = null;
            VirtualPath savedCurrentExecutionFilePath = null;
            string savedQueryString = null;
            TextWriter savedOutputWriter = null;
            AspNetSynchronizationContextBase savedSyncContext = null;

            // Transaction wouldn't flow into ASPCOMPAT mode -- need to report an error
            VerifyTransactionFlow(handler);

            // create new trace context
            _context.PushTraceContext();

            // set the new handler as the current handler
            _context.SetCurrentHandler(handler);

            // because we call this synchrnously async operations must be disabled
            bool originalSyncContextWasEnabled = _context.SyncContext.Enabled;
            _context.SyncContext.Disable();

            // Execute the handler
            try {
                try {
                    _context.ServerExecuteDepth++;

                    savedCurrentExecutionFilePath = request.SwitchCurrentExecutionFilePath(filePath);

                    if (!preserveForm) {
                        savedForm = request.SwitchForm(new HttpValueCollection());

                        // Clear out the query string, but honor overrides
                        if (queryStringOverride == null)
                            queryStringOverride = String.Empty;
                    }

                    // override query string if requested
                    if (queryStringOverride != null) {
                        savedQueryString = request.QueryStringText;
                        request.QueryStringText = queryStringOverride;
                    }

                    // capture output if requested
                    if (writer != null)
                        savedOutputWriter = response.SwitchWriter(writer);

                    Page targetPage = handler as Page;
                    if (targetPage != null) {
                        if (setPreviousPage) {
                            // Set the previousPage of the new Page as the previous Page
                            targetPage.SetPreviousPage(_context.PreviousHandler as Page);
                        }

                        Page sourcePage = _context.Handler as Page;

#pragma warning disable 0618    // To avoid deprecation warning
                        // If the source page of the transfer has smart nav on,
                        // always do as if the destination has it too (ASURT 97732)
                        if (sourcePage != null && sourcePage.SmartNavigation)
                            targetPage.SmartNavigation = true;
#pragma warning restore 0618

                        // If the target page is async need to save/restore sync context
                        if (targetPage is IHttpAsyncHandler) {
                            savedSyncContext = _context.InstallNewAspNetSynchronizationContext();
                        }
                    }

                    if ((handler is StaticFileHandler || handler is DefaultHttpHandler) &&
                       !DefaultHttpHandler.IsClassicAspRequest(filePath.VirtualPathString)) {
                        // cannot apply static files handler directly
                        // -- it would dump the source of the current page
                        // instead just dump the file content into response
                        try {
                            response.WriteFile(physPath);
                        }
                        catch {
                            // hide the real error as it could be misleading
                            // in case of mismapped requests like /foo.asmx/bar
                            error = new HttpException(404, String.Empty);
                        }
                    }
                    else if (!(handler is Page)) {
                        // disallow anything but pages
                        error = new HttpException(404, String.Empty);
                    }
                    else if (handler is IHttpAsyncHandler) {
                        // Asynchronous handler

                        // suspend cancellable period (don't abort this thread while
                        // we wait for another to finish)
                        bool isCancellable =  _context.IsInCancellablePeriod;
                        if (isCancellable)
                            _context.EndCancellablePeriod();

                        try {
                            IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler)handler;

                            if (!AppSettings.UseTaskFriendlySynchronizationContext) {
                                // Legacy code path: behavior ASP.NET <= 4.0

                                IAsyncResult ar = asyncHandler.BeginProcessRequest(_context, null, null);

                                // wait for completion
                                if (!ar.IsCompleted) {
                                    // suspend app lock while waiting
                                    bool needToRelock = false;

                                    try {
                                        try { }
                                        finally {
                                            _context.SyncContext.DisassociateFromCurrentThread();
                                            needToRelock = true;
                                        }

                                        WaitHandle h = ar.AsyncWaitHandle;

                                        if (h != null) {
                                            h.WaitOne();
                                        }
                                        else {
                                            while (!ar.IsCompleted)
                                                Thread.Sleep(1);
                                        }
                                    }
                                    finally {
                                        if (needToRelock) {
                                            _context.SyncContext.AssociateWithCurrentThread();
                                        }
                                    }
                                }

                                // end the async operation (get error if any)

                                try {
                                    asyncHandler.EndProcessRequest(ar);
                                }
                                catch (Exception e) {
                                    error = e;
                                }
                            }
                            else {
                                // New code path: behavior ASP.NET >= 4.5
                                IAsyncResult ar;
                                bool blockedThread;

                                using (CountdownEvent countdownEvent = new CountdownEvent(1)) {
                                    using (_context.SyncContext.AcquireThreadLock()) {
                                        // Kick off the asynchronous operation
                                        ar = asyncHandler.BeginProcessRequest(_context,
                                           cb: _ => { countdownEvent.Signal(); },
                                           extraData: null);
                                    }

                                    // The callback passed to BeginProcessRequest will signal the CountdownEvent.
                                    // The Wait() method blocks until the callback executes; no-ops if the operation completed synchronously.
                                    blockedThread = !countdownEvent.IsSet;
                                    countdownEvent.Wait();
                                }

                                // end the async operation (get error if any)

                                try {
                                    using (_context.SyncContext.AcquireThreadLock()) {
                                        asyncHandler.EndProcessRequest(ar);
                                    }

                                    // If we blocked the thread, YSOD the request to display a diagnostic message.
                                    if (blockedThread && !_context.SyncContext.AllowAsyncDuringSyncStages) {
                                        throw new InvalidOperationException(SR.GetString(SR.Server_execute_blocked_on_async_handler));
                                    }
                                }
                                catch (Exception e) {
                                    error = e;
                                }
                            }
                        }
                        finally {
                            // resume cancelleable period
                            if (isCancellable)
                                _context.BeginCancellablePeriod();
                        }
                    }
                    else {
                        // Synchronous handler

                        using (new DisposableHttpContextWrapper(_context)) {
                            try {
                                handler.ProcessRequest(_context);
                            }
                            catch (Exception e) {
                                error = e;
                            }
                        }
                    }
                }
                finally {
                    _context.ServerExecuteDepth--;

                    // Restore the handlers;
                    _context.RestoreCurrentHandler();

                    // restore output writer
                    if (savedOutputWriter != null)
                        response.SwitchWriter(savedOutputWriter);

                    // restore overriden query string
                    if (queryStringOverride != null && savedQueryString != null)
                        request.QueryStringText = savedQueryString;

                    if (savedForm != null)
                        request.SwitchForm(savedForm);

                    request.SwitchCurrentExecutionFilePath(savedCurrentExecutionFilePath);

                    if (savedSyncContext != null) {
                        _context.RestoreSavedAspNetSynchronizationContext(savedSyncContext);
                    }

                    if (originalSyncContextWasEnabled) {
                        _context.SyncContext.Enable();
                    }

                    // restore trace context
                    _context.PopTraceContext();
                }
            }
            catch { // Protect against exception filters
                throw;
            }

            // Report any error
            if (error != null) {
                // suppress errors with HTTP codes (for child requests they mislead more than help)
                if (error is HttpException && ((HttpException)error).GetHttpCode() != 500)
                    error = null;

                if (path != null)
                    throw new HttpException(SR.GetString(SR.Error_executing_child_request_for_path, path), error);

                throw new HttpException(SR.GetString(SR.Error_executing_child_request_for_handler, handler.GetType().ToString()), error);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Terminates execution of the current page and begins execution of a new
        ///       request using the supplied URL path.
        ///       If preserveForm is false, the QueryString and Form collections are cleared.
        ///    </para>
        /// </devdoc>
        public void Transfer(string path, bool preserveForm) {
            Page page = _context.Handler as Page;
            if ((page != null) && page.IsCallback) {
                throw new ApplicationException(SR.GetString(SR.Transfer_not_allowed_in_callback));
            }
            
            // execute child request

            Execute(path, null, preserveForm);

            // suppress the remainder of the current one

            _context.Response.End();
        }


        /// <devdoc>
        ///    <para>
        ///       Terminates execution of the current page and begins execution of a new
        ///       request using the supplied URL path.
        ///    </para>
        /// </devdoc>
        public void Transfer(string path) {
            // Make sure the transfer is not treated as a postback, which could cause a stack
            // overflow if the user doesn't expect it (VSWhidbey 181013).
            // If the use *does* want it treated as a postback, they can call Transfer(path, true).
            bool savedPreventPostback = _context.PreventPostback;
            _context.PreventPostback = true;

            Transfer(path, true /*preserveForm*/);

            _context.PreventPostback = savedPreventPostback;
        }


        public void Transfer(IHttpHandler handler, bool preserveForm) {
            Page page = handler as Page;
            if ((page != null) && page.IsCallback) {
                throw new ApplicationException(SR.GetString(SR.Transfer_not_allowed_in_callback));
            }
            
            Execute(handler, null, preserveForm);

            // suppress the remainder of the current one

            _context.Response.End();
        }

        public void TransferRequest(string path)
        {
            TransferRequest(path, false, null, null, preserveUser: true);
        }

        public void TransferRequest(string path, bool preserveForm)
        {
            TransferRequest(path, preserveForm, null, null, preserveUser: true);
        }

        public void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers) {
            TransferRequest(path, preserveForm, method, headers, preserveUser: true);
        }

        public void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers, bool preserveUser) {
            EnsureHasNotTransitionedToWebSocket();

            if (!HttpRuntime.UseIntegratedPipeline) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }

            if (_context == null) {
                throw new HttpException(SR.GetString(SR.Server_not_available));
            }

            if (path == null) {
                throw new ArgumentNullException("path");
            }

            IIS7WorkerRequest wr = _context.WorkerRequest as IIS7WorkerRequest;
            HttpRequest request = _context.Request;
            HttpResponse response = _context.Response;

            if (wr == null) {
                throw new HttpException(SR.GetString(SR.Server_not_available));            
            }
                
            // Remove potential cookie-less session id (ASURT 100558)
            path = response.RemoveAppPathModifier(path);

            // Extract query string if specified
            String qs = null;
            int iqs = path.IndexOf('?');
            if (iqs >= 0) {
                qs = (iqs < path.Length-1) ? path.Substring(iqs+1) : String.Empty;
                path = path.Substring(0, iqs);   
            }

            if (!UrlPath.IsValidVirtualPathWithoutProtocol(path)) {
                throw new ArgumentException(SR.GetString(SR.Invalid_path_for_child_request, path));
            }

            VirtualPath virtualPath = request.FilePathObject.Combine(VirtualPath.Create(path));

            //  Schedule the child execution
            wr.ScheduleExecuteUrl( virtualPath.VirtualPathString,
                                   qs,
                                   method,
                                   preserveForm,
                                   preserveForm ? request.EntityBody : null,
                                   headers,
                                   preserveUser);
            
            // force the completion of the current request so that the 
            // child execution can be performed immediately after unwind
            _context.ApplicationInstance.EnsureReleaseState();

            // DevDiv Bugs 162750: IIS7 Integrated Mode:  TransferRequest performance issue
            // Instead of calling Response.End we call HttpApplication.CompleteRequest()
            _context.ApplicationInstance.CompleteRequest();
        }

        private void VerifyTransactionFlow(IHttpHandler handler) {
            Page topPage = _context.Handler as Page;
            Page childPage = handler as Page;

            if (childPage != null && childPage.IsInAspCompatMode && // child page aspcompat
                topPage != null && !topPage.IsInAspCompatMode &&    // top page is not aspcompat
                Transactions.Utils.IsInTransaction) {               // we are in transaction

                throw new HttpException(SR.GetString(SR.Transacted_page_calls_aspcompat));
            }
        }

        //
        // Static method to execute a request outside of HttpContext and capture the response
        //

        internal static void ExecuteLocalRequestAndCaptureResponse(String path, TextWriter writer,
                                                                ErrorFormatterGenerator errorFormatterGenerator) {
            HttpRequest request = new HttpRequest(
                                          VirtualPath.CreateAbsolute(path),
                                          String.Empty);

            HttpResponse response = new HttpResponse(writer);

            HttpContext context = new HttpContext(request, response);

            HttpApplication app = HttpApplicationFactory.GetApplicationInstance(context) as HttpApplication;
            context.ApplicationInstance = app;

            try {
                context.Server.Execute(path);
            }
            catch (HttpException e) {
                if (errorFormatterGenerator != null) {
                    context.Response.SetOverrideErrorFormatter(errorFormatterGenerator.GetErrorFormatter(e));
                }

                context.Response.ReportRuntimeError(e, false, true);
            }
            finally {
                if (app != null) {
                    context.ApplicationInstance = null;
                    HttpApplicationFactory.RecycleApplicationInstance(app);
                }
            }
        }

        //
        // Computer name
        //

        private static object _machineNameLock = new object();
        private static string _machineName;
        private const int _maxMachineNameLength = 256;


        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the server machine name.
        ///    </para>
        /// </devdoc>
        public string MachineName {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
            get {
                return GetMachineNameInternal();
            }
        }

        internal static string GetMachineNameInternal()
        {
            if (_machineName != null)
                return _machineName;
            lock (_machineNameLock)
            {
                if (_machineName != null)
                    return _machineName;

                StringBuilder   buf = new StringBuilder (_maxMachineNameLength);
                int             len = _maxMachineNameLength;

                if (UnsafeNativeMethods.GetComputerName (buf, ref len) == 0)
                    throw new HttpException (SR.GetString(SR.Get_computer_name_failed));

                _machineName = buf.ToString();
            }
            return _machineName;
        }

        //
        // Request Timeout
        //


        /// <devdoc>
        ///    <para>
        ///       Request timeout in seconds
        ///    </para>
        /// </devdoc>
        public int ScriptTimeout {
            get {
                if (_context != null) {
                    return Convert.ToInt32(_context.Timeout.TotalSeconds);
                }
                else {
                    return HttpRuntimeSection.DefaultExecutionTimeout;
                }
            }

            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
            set {
                if (_context == null)
                    throw new HttpException(SR.GetString(SR.Server_not_available));
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _context.Timeout = new TimeSpan(0, 0, value);
            }
        }

        //
        // Encoding / Decoding -- wrappers for HttpUtility
        //


        /// <devdoc>
        ///    <para>
        ///       HTML
        ///       decodes a given string and
        ///       returns the decoded string.
        ///    </para>
        /// </devdoc>
        public string HtmlDecode(string s) {
            return HttpUtility.HtmlDecode(s);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML
        ///       decode a string and send the result to a TextWriter output
        ///       stream.
        ///    </para>
        /// </devdoc>
        public void HtmlDecode(string s, TextWriter output) {
            HttpUtility.HtmlDecode(s, output);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML
        ///       encodes a given string and
        ///       returns the encoded string.
        ///    </para>
        /// </devdoc>
        public string HtmlEncode(string s) {
            return HttpUtility.HtmlEncode(s);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML
        ///       encodes
        ///       a string and returns the output to a TextWriter stream of output.
        ///    </para>
        /// </devdoc>
        public void HtmlEncode(string s, TextWriter output) {
            HttpUtility.HtmlEncode(s, output);
        }


        /// <devdoc>
        ///    <para>
        ///       URL
        ///       encodes a given
        ///       string and returns the encoded string.
        ///    </para>
        /// </devdoc>
        public string UrlEncode(string s) {
            Encoding e = (_context != null) ? _context.Response.ContentEncoding : Encoding.UTF8;
            return HttpUtility.UrlEncode(s, e);
        }


        /// <devdoc>
        ///    <para>
        ///       URL encodes a path portion of a URL string and returns the encoded string.
        ///    </para>
        /// </devdoc>
        public string UrlPathEncode(string s) {
            return HttpUtility.UrlPathEncode(s);
        }


        /// <devdoc>
        ///    <para>
        ///       URL
        ///       encodes
        ///       a string and returns the output to a TextWriter output stream.
        ///    </para>
        /// </devdoc>
        public void UrlEncode(string s, TextWriter output) {
            if (s != null)
                output.Write(UrlEncode(s));
        }


        /// <devdoc>
        ///    <para>
        ///       URL decodes a string and returns the output in a string.
        ///    </para>
        /// </devdoc>
        public string UrlDecode(string s) {
            Encoding e = (_context != null) ? _context.Request.ContentEncoding : Encoding.UTF8;
            return HttpUtility.UrlDecode(s, e);
        }


        /// <devdoc>
        ///    <para>
        ///       URL decodes a string and returns the output as a TextWriter output
        ///       stream.
        ///    </para>
        /// </devdoc>
        public void UrlDecode(string s, TextWriter output) {
            if (s != null)
                output.Write(UrlDecode(s));
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        static public string UrlTokenEncode(byte [] input)
        {
            return HttpEncoder.Current.UrlTokenEncode(input);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        static public byte [] UrlTokenDecode(string input) {
            return HttpEncoder.Current.UrlTokenDecode(input);
        }

        // helper that throws an exception if we have transitioned the current request to a WebSocket request
        internal void EnsureHasNotTransitionedToWebSocket() {
            if (_context != null) {
                _context.EnsureHasNotTransitionedToWebSocket();
            }
        }
    }


    /// <devdoc>
    /// </devdoc>

    // VSWhidbey 473228 - removed link demand from HttpUtility for ClickOnce scenario
    public sealed class HttpUtility {

        public HttpUtility () {}
		
		//////////////////////////////////////////////////////////////////////////
        //
        //  HTML Encoding / Decoding
        //


        /// <devdoc>
        ///    <para>
        ///       HTML decodes a string and returns the decoded string.
        ///    </para>
        /// </devdoc>
        public static string HtmlDecode(string s) {
            return HttpEncoder.Current.HtmlDecode(s);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML decode a string and send the result to a TextWriter output stream.
        ///    </para>
        /// </devdoc>
        public static void HtmlDecode(string s, TextWriter output) {
            HttpEncoder.Current.HtmlDecode(s, output);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML encodes a string and returns the encoded string.
        ///    </para>
        /// </devdoc>
        public static String HtmlEncode(String s) {
            return HttpEncoder.Current.HtmlEncode(s);
        }


        /// <devdoc>
        ///    <para>
        ///       HTML encodes an object's string representation and returns the encoded string.
        ///       If the object implements IHtmlString, don't encode it
        ///    </para>
        /// </devdoc>
        public static String HtmlEncode(object value) {
            if (value == null) {
                // Return null to be consistent with HtmlEncode(string)
                return null;
            }

            var htmlString = value as IHtmlString;
            if (htmlString != null) {
                return htmlString.ToHtmlString();
            }

            return HtmlEncode(Convert.ToString(value, CultureInfo.CurrentCulture));
        }


        /// <devdoc>
        ///    <para>
        ///       HTML encodes a string and returns the output to a TextWriter stream of
        ///       output.
        ///    </para>
        /// </devdoc>
        public static void HtmlEncode(String s, TextWriter output) {
            HttpEncoder.Current.HtmlEncode(s, output);
        }


        /// <devdoc>
        ///    <para>
        ///       Encodes a string to make it a valid HTML attribute and returns the encoded string.
        ///    </para>
        /// </devdoc>
        public static String HtmlAttributeEncode(String s) {
            return HttpEncoder.Current.HtmlAttributeEncode(s);
        }
                

        /// <devdoc>
        ///    <para>
        ///       Encodes a string to make it a valid HTML attribute and returns the output
        ///       to a TextWriter stream of
        ///       output.
        ///    </para>
        /// </devdoc>
        public static void HtmlAttributeEncode(String s, TextWriter output) {
            HttpEncoder.Current.HtmlAttributeEncode(s, output);
        }

        
        internal static string FormatPlainTextSpacesAsHtml(string s) {
            if (s == null) {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);

            int cb = s.Length;

            for (int i = 0; i < cb; i++) {
                char ch = s[i];
                if(ch == ' ') {
                    writer.Write("&nbsp;");
                }
                else {
                    writer.Write(ch);
                }
            }
            return builder.ToString();
        }

        internal static String FormatPlainTextAsHtml(String s) {
            if (s == null)
                return null;

            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);

            FormatPlainTextAsHtml(s, writer);

            return builder.ToString();
        }

        internal static void FormatPlainTextAsHtml(String s, TextWriter output) {
            if (s == null)
                return;

            int cb = s.Length;

            char prevCh = '\0';

            for (int i=0; i<cb; i++) {
                char ch = s[i];
                switch (ch) {
                    case '<':
                        output.Write("&lt;");
                        break;
                    case '>':
                        output.Write("&gt;");
                        break;
                    case '"':
                        output.Write("&quot;");
                        break;
                    case '&':
                        output.Write("&amp;");
                        break;
                    case ' ':
                        if (prevCh == ' ')
                            output.Write("&nbsp;");
                        else
                            output.Write(ch);
                        break;
                    case '\r':
                        // Ignore \r, only handle \n
                        break;
                    case '\n':
                        output.Write("<br>");
                        break;

                    // 
                    default:
#if ENTITY_ENCODE_HIGH_ASCII_CHARS
                        // The seemingly arbitrary 160 comes from RFC
                        if (ch >= 160 && ch < 256) {
                            output.Write("&#");
                            output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                            output.Write(';');
                            break;
                        }
#endif // ENTITY_ENCODE_HIGH_ASCII_CHARS

                        output.Write(ch);
                        break;
                }

                prevCh = ch;
            }
        }


        //////////////////////////////////////////////////////////////////////////
        //
        //  ASII encode - everything all non-7-bit to '?'
        //

        /*internal static String AsciiEncode(String s) {
            if (s == null)
                return null;

            StringBuilder sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++) {
                char ch = s[i];
                if (((ch & 0xff80) != 0) || (ch < ' ' && ch != '\r' && ch != '\n' && ch != '\t'))
                    ch = '?';
                sb.Append(ch);
            }

            return sb.ToString();
        }*/


        //
        //  Query string parsing support
        //

        public static NameValueCollection ParseQueryString(string query) {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding) {
            if (query == null) {
                throw new ArgumentNullException("query");
            }

            if (encoding == null) {
                throw new ArgumentNullException("encoding");
            }

            if (query.Length > 0 && query[0] == '?') {
                query = query.Substring(1);
            }

            return new HttpValueCollection(query, false, true, encoding);
        }

        //////////////////////////////////////////////////////////////////////////
        //
        //  URL decoding / encoding
        //
        //////////////////////////////////////////////////////////////////////////

        //
        //  Public static methods
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlEncode(string str) {
            if (str == null)
                return null;
            return UrlEncode(str, Encoding.UTF8);
        }


        /// <devdoc>
        ///    <para>
        ///       URL encodes a path portion of a URL string and returns the encoded string.
        ///    </para>
        /// </devdoc>
        public static string UrlPathEncode(string str) {
            return HttpEncoder.Current.UrlPathEncode(str);
        }

        internal static string AspCompatUrlEncode(string s) {
            s = UrlEncode(s);
            s = s.Replace("!", "%21");
            s = s.Replace("*", "%2A");
            s = s.Replace("(", "%28");
            s = s.Replace(")", "%29");
            s = s.Replace("-", "%2D");
            s = s.Replace(".", "%2E");
            s = s.Replace("_", "%5F");
            s = s.Replace("\\", "%5C");
            return s;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlEncode(string str, Encoding e) {
            if (str == null)
                return null;
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        //  Helper to encode the non-ASCII url characters only
        internal static String UrlEncodeNonAscii(string str, Encoding e) {
            return HttpEncoder.Current.UrlEncodeNonAscii(str, e);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlEncode(byte[] bytes) {
            if (bytes == null)
                return null;
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlEncode(byte[] bytes, int offset, int count) {
            if (bytes == null)
                return null;
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlEncodeToBytes(string str) {
            if (str == null)
                return null;
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlEncodeToBytes(string str, Encoding e) {
            if (str == null)
                return null;
            byte[] bytes = e.GetBytes(str);
            return HttpEncoder.Current.UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlEncodeToBytes(byte[] bytes) {
            if (bytes == null)
                return null;
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count) {
            return HttpEncoder.Current.UrlEncode(bytes, offset, count, true /* alwaysCreateNewReturnValue */);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("This method produces non-standards-compliant output and has interoperability issues. The preferred alternative is UrlEncode(String).")]
        public static string UrlEncodeUnicode(string str) {
            return HttpEncoder.Current.UrlEncodeUnicode(str, false /* ignoreAscii */);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("This method produces non-standards-compliant output and has interoperability issues. The preferred alternative is UrlEncodeToBytes(String).")]
        public static byte[] UrlEncodeUnicodeToBytes(string str) {
            if (str == null)
                return null;
            return Encoding.ASCII.GetBytes(UrlEncodeUnicode(str));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlDecode(string str) {
            if (str == null)
                return null;
            return UrlDecode(str, Encoding.UTF8);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlDecode(string str, Encoding e) {
            return HttpEncoder.Current.UrlDecode(str, e);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlDecode(byte[] bytes, Encoding e) {
            if (bytes == null)
                return null;
            return UrlDecode(bytes, 0, bytes.Length, e);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e) {
            return HttpEncoder.Current.UrlDecode(bytes, offset, count, e);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlDecodeToBytes(string str) {
            if (str == null)
                return null;
            return UrlDecodeToBytes(str, Encoding.UTF8);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlDecodeToBytes(string str, Encoding e) {
            if (str == null)
                return null;
            return UrlDecodeToBytes(e.GetBytes(str));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlDecodeToBytes(byte[] bytes) {
            if (bytes == null)
                return null;
            return UrlDecodeToBytes(bytes, 0, (bytes != null) ? bytes.Length : 0);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count) {
            return HttpEncoder.Current.UrlDecode(bytes, offset, count);
        }
                

        //////////////////////////////////////////////////////////////////////////
        //
        //  Misc helpers
        //
        //////////////////////////////////////////////////////////////////////////

        internal static String FormatHttpDateTime(DateTime dt) {
            if (dt < DateTime.MaxValue.AddDays(-1) && dt > DateTime.MinValue.AddDays(1))
                dt = dt.ToUniversalTime();
            return dt.ToString("R", DateTimeFormatInfo.InvariantInfo);
        }

        internal static String FormatHttpDateTimeUtc(DateTime dt) {
            return dt.ToString("R", DateTimeFormatInfo.InvariantInfo);
        }

        internal static String FormatHttpCookieDateTime(DateTime dt) {
            if (dt < DateTime.MaxValue.AddDays(-1) && dt > DateTime.MinValue.AddDays(1))
                dt = dt.ToUniversalTime();
            return dt.ToString("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'", DateTimeFormatInfo.InvariantInfo);
        }

        //
        // JavaScriptStringEncode
        //

        public static String JavaScriptStringEncode(string value) {
            return JavaScriptStringEncode(value, false);
        }

        public static String JavaScriptStringEncode(string value, bool addDoubleQuotes) {
            string encoded = HttpEncoder.Current.JavaScriptStringEncode(value);
            return (addDoubleQuotes) ? "\"" + encoded + "\"" : encoded;
        }

        /// <summary>
        /// Attempts to parse a co-ordinate as a double precision floating point value. 
        /// This essentially does a Double.TryParse while disallowing specific floating point constructs such as the exponent.
        /// </summary>
        internal static bool TryParseCoordinates(string value, out double doubleValue) {
            var flags = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            return Double.TryParse(value, flags, CultureInfo.InvariantCulture, out doubleValue);
        }
    }
}
