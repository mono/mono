//------------------------------------------------------------------------------
// <copyright file="SimpleWorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {

    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Util;

    //
    // Simple Worker Request provides a concrete implementation 
    // of HttpWorkerRequest that writes the respone to the user
    // supplied writer.
    //

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [ComVisible(false)]
    public class SimpleWorkerRequest : HttpWorkerRequest {

        private bool        _hasRuntimeInfo;
        private String      _appVirtPath;       // "/foo"
        private String      _appPhysPath;       // "c:\foo\"
        private String      _page;
        private String      _pathInfo;
        private String      _queryString;
        private TextWriter  _output;
        private String      _installDir;

        private void ExtractPagePathInfo() {
            int i = _page.IndexOf('/');

            if (i >= 0) {
                _pathInfo = _page.Substring(i);
                _page = _page.Substring(0, i);
            }
        }

        private String GetPathInternal(bool includePathInfo) {
            String s = _appVirtPath.Equals("/") ? ("/" + _page) : (_appVirtPath + "/" + _page);

            if (includePathInfo && _pathInfo != null)
                return s + _pathInfo;
            else
                return s;
        }

        //
        //  HttpWorkerRequest implementation
        //

        // "/foo/page.aspx/tail"

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetUriPath() {
            return GetPathInternal(true);
        }

        // "param=bar"

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetQueryString() {
            return _queryString;
        }

        // "/foo/page.aspx/tail?param=bar"

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetRawUrl() {
            String qs = GetQueryString();
            if (!String.IsNullOrEmpty(qs))
                return GetPathInternal(true) + "?" + qs;
            else
                return GetPathInternal(true);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetHttpVerbName() {
            return "GET";
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetHttpVersion() {
            return "HTTP/1.0";
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetRemoteAddress() {
            return "127.0.0.1";
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetRemotePort() {
            return 0;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetLocalAddress() {
            return "127.0.0.1";
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetLocalPort() {
            return 80;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override IntPtr GetUserToken() {
            return IntPtr.Zero;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetFilePath() {
            return GetPathInternal(false);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetFilePathTranslated() {
            String path =  _appPhysPath + _page.Replace('/', '\\');
            InternalSecurityPermissions.PathDiscovery(path).Demand();
            return path;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetPathInfo() {
            return (_pathInfo != null) ? _pathInfo : String.Empty;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetAppPath() {
            return _appVirtPath;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetAppPathTranslated() {
            InternalSecurityPermissions.PathDiscovery(_appPhysPath).Demand();
            return _appPhysPath;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String GetServerVariable(String name) {
            return String.Empty;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String MapPath(String path) {
            if (!_hasRuntimeInfo)
                return null;

            String mappedPath = null;
            String appPath = _appPhysPath.Substring(0, _appPhysPath.Length-1); // without trailing "\"

            if (String.IsNullOrEmpty(path) || path.Equals("/")) {
                mappedPath = appPath;
            }
            if (StringUtil.StringStartsWith(path, _appVirtPath)) {
                mappedPath = appPath + path.Substring(_appVirtPath.Length).Replace('/', '\\');
            }

            InternalSecurityPermissions.PathDiscovery(mappedPath).Demand();
            return mappedPath;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string MachineConfigPath {
            get {
                if (_hasRuntimeInfo) {
                    string path = HttpConfigurationSystem.MachineConfigurationFilePath;
                    InternalSecurityPermissions.PathDiscovery(path).Demand();
                    return path;
                }
                else 
                    return null;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string RootWebConfigPath {
            get {
                if (_hasRuntimeInfo) {
                    string path = HttpConfigurationSystem.RootWebConfigurationFilePath;
                    InternalSecurityPermissions.PathDiscovery(path).Demand();
                    return path;
                }
                else 
                    return null;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String MachineInstallDirectory {
            get {
                if (_hasRuntimeInfo) {
                    InternalSecurityPermissions.PathDiscovery(_installDir).Demand();
                    return _installDir;
                }
                return null;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendStatus(int statusCode, String statusDescription) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendKnownResponseHeader(int index, String value) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendUnknownResponseHeader(String name, String value) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendResponseFromMemory(byte[] data, int length) {
            _output.Write(System.Text.Encoding.Default.GetChars(data, 0, length));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendResponseFromFile(String filename, long offset, long length) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void FlushResponse(bool finalFlush) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void EndOfRequest() {
        }

        internal override void UpdateInitialCounters() {
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);
        }

        //
        // Internal support
        //
        internal override void UpdateResponseCounters(bool finalFlush, int bytesOut) {
            // Integrated mode uses a fake simple worker request to initialize (Dev10 Bugs 466973)
            if (HttpRuntime.UseIntegratedPipeline) {
                return;
            }

            if (finalFlush) {
                PerfCounters.DecrementGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
                PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
            }
            if (bytesOut > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, bytesOut);
            }
        }

        internal override void UpdateRequestCounters(int bytesIn) {
            // Integrated mode uses a fake simple worker request to initialize (Dev10 Bugs 466973)
            if (HttpRuntime.UseIntegratedPipeline) {
                return;
            }

            if (bytesIn > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, bytesIn);
            }
        }

        //
        // Ctors
        //

        private SimpleWorkerRequest() {
        }

        /*
         *  Ctor that gets application data from HttpRuntime, assuming
         *  HttpRuntime has been set up (app domain specially created, etc.)
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SimpleWorkerRequest(String page, String query, TextWriter output): this() {
            _queryString = query;
            _output = output;
            _page = page;

            ExtractPagePathInfo();

            _appPhysPath = Thread.GetDomain().GetData(".appPath").ToString();
            _appVirtPath = Thread.GetDomain().GetData(".appVPath").ToString();
            _installDir  = HttpRuntime.AspInstallDirectoryInternal;

            _hasRuntimeInfo = true;
        }

        /*
         *  Ctor that gets application data as arguments,assuming HttpRuntime
         *  has not been set up.
         *
         *  This allows for limited functionality to execute handlers.
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SimpleWorkerRequest(String appVirtualDir, String appPhysicalDir, String page, String query, TextWriter output): this() {
            if (Thread.GetDomain().GetData(".appPath") != null) {
                throw new HttpException(SR.GetString(SR.Wrong_SimpleWorkerRequest));
            }

            _appVirtPath = appVirtualDir;
            _appPhysPath = appPhysicalDir;
            _queryString = query;
            _output = output;
            _page = page;

            ExtractPagePathInfo();

            if (!StringUtil.StringEndsWith(_appPhysPath, '\\'))
                _appPhysPath += "\\";
                
            _hasRuntimeInfo = false;
        }
    
    }
}
