//------------------------------------------------------------------------------
// <copyright file="FileAuthorizationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * FileAclAuthorizationModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Runtime.Serialization;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Web.Management;
    using System.Web.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;



    /// <devdoc>
    ///    <para>
    ///       Verifies that the remote user has NT permissions to access the
    ///       file requested.
    ///    </para>
    /// </devdoc>
    public sealed class FileAuthorizationModule : IHttpModule {


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.FileAuthorizationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public FileAuthorizationModule() {
        }

        private static bool s_EnabledDetermined;
        private static bool s_Enabled;


        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public static bool CheckFileAccessForUser(String virtualPath, IntPtr token, string verb) {
            if (virtualPath == null)
                throw new ArgumentNullException("virtualPath");
            if (token == IntPtr.Zero)
                throw new ArgumentNullException("token");
            if (verb == null)
                throw new ArgumentNullException("verb");
            VirtualPath vPath = VirtualPath.Create(virtualPath);

            if (!vPath.IsWithinAppRoot)
                throw new ArgumentException(SR.GetString(SR.Virtual_path_outside_application_not_supported), "virtualPath");

            if (!s_EnabledDetermined) {
                if (HttpRuntime.UseIntegratedPipeline) {
                    s_Enabled = true; // always enabled in Integrated Mode
                }
                else {
                    HttpModulesSection modulesSection = RuntimeConfig.GetConfig().HttpModules;
                    int len = modulesSection.Modules.Count;
                    for (int iter = 0; iter < len; iter++) {
                        HttpModuleAction module = modulesSection.Modules[iter];
                        if (Type.GetType(module.Type, false) == typeof(FileAuthorizationModule)) {
                            s_Enabled = true;
                            break;
                        }
                    }
                }
                s_EnabledDetermined = true;
            }
            if (!s_Enabled)
                return true;
            ////////////////////////////////////////////////////////////
            // Step 3: Check the cache for the file-security-descriptor
            //        for the requested file
            bool freeDescriptor;
            FileSecurityDescriptorWrapper oSecDesc = GetFileSecurityDescriptorWrapper(vPath.MapPath(), out freeDescriptor);

            ////////////////////////////////////////////////////////////
            // Step 4: Check if access is allowed
            int iAccess = 3;
            if (verb == "GET" || verb == "POST" || verb == "HEAD" || verb == "OPTIONS")
                iAccess = 1;
            bool fAllowed = oSecDesc.IsAccessAllowed(token, iAccess);

            ////////////////////////////////////////////////////////////
            // Step 5: Free the security descriptor if adding to cache failed
            if (freeDescriptor)
                oSecDesc.FreeSecurityDescriptor();
            return fAllowed;
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Init(HttpApplication app) {
            app.AuthorizeRequest += new EventHandler(this.OnEnter);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose() {
        }

        void OnEnter(Object source, EventArgs eventArgs) {
            if (HttpRuntime.IsOnUNCShareInternal)
                return; // don't check on UNC shares -- the user token is bogus anyway
            HttpApplication app;
            HttpContext context;

            app = (HttpApplication)source;
            context = app.Context;

            if (!IsUserAllowedToFile(context, null)) {
                context.Response.SetStatusCode(401, subStatus: 3);
                WriteErrorMessage(context);
                app.CompleteRequest();
            }
        }

        internal static bool IsWindowsIdentity(HttpContext context) {
            return context.User != null &&
                 context.User.Identity != null &&
                 context.User.Identity is WindowsIdentity;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method is not dangerous.")]
        private static bool IsUserAllowedToFile(HttpContext context, string fileName) {
            ////////////////////////////////////////////////////////////
            // Step 1: Check if this is WindowsLogin
            // It's not a windows authenticated user: allow access
            if (!IsWindowsIdentity(context)) {
                return true;
            }

            if (fileName == null) {
                fileName = context.Request.PhysicalPathInternal;
            }

            bool           isAnonymousUser = (context.User == null || !context.User.Identity.IsAuthenticated);
            CachedPathData pathData        = null;
            int            iAccess         = 3;
            HttpVerb       verb            = context.Request.HttpVerb;

            if (verb == HttpVerb.GET
                || verb == HttpVerb.POST
                || verb == HttpVerb.HEAD
                || context.Request.HttpMethod  == "OPTIONS")
            {
                iAccess = 1;

                ////////////////////////////////////////////////////////////
                // iff it's a GET or POST or HEAD or OPTIONs verb, we can use the cached result
                if (!CachedPathData.DoNotCacheUrlMetadata) {
                    pathData = context.GetConfigurationPathData();
                    // as a perf optimization, we cache results for annoymous access
                    //  to CachedPathData.PhysicalPath, and avoid doing the full check
                    if (!StringUtil.EqualsIgnoreCase(fileName, pathData.PhysicalPath)) {
                        // set to null so we don't attempt to update it after the full check below
                        pathData = null;
                    }
                    else {
                        if (pathData.AnonymousAccessAllowed) { // fast path when everyone has access
                            Debug.Trace("FAM", "IsUserAllowedToFile: pathData.AnonymousAccessAllowed");
                            return true;
                        }
                        if (pathData.AnonymousAccessChecked && isAnonymousUser) { // fast path for anonymous user
                            // another thread could be modifying CachedPathData, so return the 
                            // value of AnonymousAccessAllowed instead of assuming it is false
                            Debug.Trace("FAM", "IsUserAllowedToFile: pathData.AnonymousAccessChecked && isAnonymousUser");
                            return pathData.AnonymousAccessAllowed;
                        }
                    }
                }
            }


            // Step 3: Check the cache for the file-security-descriptor
            //        for the requested file
            bool freeDescriptor;
            FileSecurityDescriptorWrapper oSecDesc = GetFileSecurityDescriptorWrapper(fileName, out freeDescriptor);

            ////////////////////////////////////////////////////////////
            // Step 4: Check if access is allowed
            bool fAllowed;
            if (iAccess == 1) { // iff it's a GET or POST or HEAD or OPTIONs verb, we can cache the result
                if (oSecDesc._AnonymousAccessChecked && isAnonymousUser) {
                    Debug.Trace("FAM", "IsUserAllowedToFile: oSecDesc._AnonymousAccessChecked && isAnonymousUser");
                    fAllowed = oSecDesc._AnonymousAccess;
                }
                else {
                    Debug.Trace("FAM", "IsUserAllowedToFile: calling oSecDesc.IsAccessAllowed with iAccess == 1");
                    fAllowed = oSecDesc.IsAccessAllowed(context.WorkerRequest.GetUserToken(), iAccess);
                }

                if (!oSecDesc._AnonymousAccessChecked && isAnonymousUser) {
                    oSecDesc._AnonymousAccess = fAllowed;
                    oSecDesc._AnonymousAccessChecked = true;
                }

                // Cache results in CachedPathData if the file exists and annonymous access has been checked.
                // Note that if CachedPathData.Exists is false, then it does not have a dependency on the file path,
                // and won't be expunged if the file changes.
                if (pathData != null && pathData.Exists && oSecDesc._AnonymousAccessChecked) {
                    Debug.Trace("FAM", "IsUserAllowedToFile: updating pathData");
                    pathData.AnonymousAccessAllowed = oSecDesc._AnonymousAccess;
                    pathData.AnonymousAccessChecked = true;
                }
            } 
            else {
                Debug.Trace("FAM", "IsUserAllowedToFile: calling oSecDesc.IsAccessAllowed with iAccess != 1");
                fAllowed = oSecDesc.IsAccessAllowed(context.WorkerRequest.GetUserToken(), iAccess); // don't cache this anywhere
            }

            ////////////////////////////////////////////////////////////
            // Step 5: Free the security descriptor if adding to cache failed
            if (freeDescriptor)
                oSecDesc.FreeSecurityDescriptor();

            if (fAllowed) {
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFileAuthorizationSuccess);
            }
            else {
                if (!isAnonymousUser)
                    WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFileAuthorizationFailure);
            }

            return fAllowed;
        }
        private static FileSecurityDescriptorWrapper GetFileSecurityDescriptorWrapper(string fileName, out bool freeDescriptor) {
            if (CachedPathData.DoNotCacheUrlMetadata) {
                freeDescriptor = true;
                return new FileSecurityDescriptorWrapper(fileName);
            }

            freeDescriptor = false;
            string                          oCacheKey   = CacheInternal.PrefixFileSecurity + fileName;
            FileSecurityDescriptorWrapper   oSecDesc    = HttpRuntime.Cache.InternalCache.Get(oCacheKey) as FileSecurityDescriptorWrapper;

            // If it's not present in the cache, then create it and add to the cache
            if (oSecDesc == null) {
                Debug.Trace("FAM", "GetFileSecurityDescriptorWrapper: cache miss for " + fileName);
                oSecDesc = new FileSecurityDescriptorWrapper(fileName);
                string cacheDependencyPath = oSecDesc.GetCacheDependencyPath();
                if (cacheDependencyPath != null) {
                    // Add it to the cache: ignore failures, since a different thread may have added it or the file doesn't exist
                    try {
                        Debug.Trace("FAM", "GetFileSecurityDescriptorWrapper: inserting into cache with dependency on " + cacheDependencyPath);
                        CacheDependency dependency = new CacheDependency(0, cacheDependencyPath);
                        TimeSpan slidingExp = CachedPathData.UrlMetadataSlidingExpiration;
                        HttpRuntime.Cache.InternalCache.Insert(oCacheKey, oSecDesc, new CacheInsertOptions() {
                                                                                    Dependencies = dependency,
                                                                                    SlidingExpiration = slidingExp,
                                                                                    OnRemovedCallback = new CacheItemRemovedCallback(oSecDesc.OnCacheItemRemoved)
                                                                                });
                    } catch (Exception e){
                        Debug.Trace("internal", e.ToString());
                        freeDescriptor = true;
                    }
                }
            }
            return oSecDesc;
        }


        private void WriteErrorMessage(HttpContext context) {
            if (!context.IsCustomErrorEnabled) {
                context.Response.Write((new FileAccessFailedErrorFormatter(context.Request.PhysicalPathInternal)).GetErrorMessage(context, false));
            } else {
                context.Response.Write((new FileAccessFailedErrorFormatter(null)).GetErrorMessage(context, true));
            }
            // In Integrated pipeline, ask for handler headers to be generated.  This would be unnecessary
            // if we just threw an access denied exception, and used the standard error mechanism
            context.Response.GenerateResponseHeadersForHandler();
        }


        static internal bool RequestRequiresAuthorization(HttpContext context) {

            Object                        sec;
            FileSecurityDescriptorWrapper oSecDesc;
            string                        oCacheKey;

            if (!IsWindowsIdentity(context)) {
                return false;
            }

            oCacheKey = CacheInternal.PrefixFileSecurity + context.Request.PhysicalPathInternal;

            sec = HttpRuntime.Cache.InternalCache.Get(oCacheKey);

            // If it's not present in the cache, then return true
            if (sec == null || !(sec is FileSecurityDescriptorWrapper))
                return true;

            oSecDesc = (FileSecurityDescriptorWrapper) sec;
            if (oSecDesc._AnonymousAccessChecked && oSecDesc._AnonymousAccess)
                return false;

            return true;
        }
        internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath)
        {
            return IsUserAllowedToFile(context, virtualPath.MapPath());
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    internal class FileSecurityDescriptorWrapper : IDisposable {
        ~FileSecurityDescriptorWrapper() {
            FreeSecurityDescriptor();
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal FileSecurityDescriptorWrapper(String strFile) {
            _FileName = FileUtil.RemoveTrailingDirectoryBackSlash(strFile);
            _securityDescriptor = UnsafeNativeMethods.GetFileSecurityDescriptor(_FileName);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal bool IsAccessAllowed(IntPtr iToken, int iAccess) {
            if (iToken == IntPtr.Zero)
                return true;

            if (_SecurityDescriptorBeingFreed)
                return IsAccessAllowedUsingNewSecurityDescriptor(iToken, iAccess);

            _Lock.AcquireReaderLock();
            try {
                try {
                    if (!_SecurityDescriptorBeingFreed) {
                        if (_securityDescriptor == IntPtr.Zero)
                            return true;
                        if (_securityDescriptor == UnsafeNativeMethods.INVALID_HANDLE_VALUE)
                            return false;
                        else
                            return (UnsafeNativeMethods.IsAccessToFileAllowed(_securityDescriptor, iToken, iAccess) != 0);
                    }
                } finally {
                    _Lock.ReleaseReaderLock();
                }
            } catch {
                throw;
            }

            return IsAccessAllowedUsingNewSecurityDescriptor(iToken, iAccess);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private bool IsAccessAllowedUsingNewSecurityDescriptor(IntPtr iToken, int iAccess) {
            if (iToken == IntPtr.Zero)
                return true;

            IntPtr secDes = UnsafeNativeMethods.GetFileSecurityDescriptor(_FileName);
            if (secDes == IntPtr.Zero)
                return true;
            if (secDes == UnsafeNativeMethods.INVALID_HANDLE_VALUE)
                return false;

            try {
                try {
                    return (UnsafeNativeMethods.IsAccessToFileAllowed(secDes, iToken, iAccess) != 0);
                } finally {
                    UnsafeNativeMethods.FreeFileSecurityDescriptor(secDes);
                }
            } catch {
                throw;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal void OnCacheItemRemoved(String key, Object value, CacheItemRemovedReason reason) {
            FreeSecurityDescriptor();
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal void FreeSecurityDescriptor() {
            if (!IsSecurityDescriptorValid())
                return;
            _SecurityDescriptorBeingFreed = true;

            _Lock.AcquireWriterLock();
            try {
                try {
                    if (!IsSecurityDescriptorValid())
                        return;
                    // VSWHIDBEY 493667: double free in webengine!FreeFileSecurityDescriptor()
                    IntPtr temp = _securityDescriptor;
                    _securityDescriptor = UnsafeNativeMethods.INVALID_HANDLE_VALUE;
                    UnsafeNativeMethods.FreeFileSecurityDescriptor(temp);
                } finally {
                    _Lock.ReleaseWriterLock();
                }
            } catch {
                throw;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal bool IsSecurityDescriptorValid() {
            return
                _securityDescriptor != UnsafeNativeMethods.INVALID_HANDLE_VALUE &&
                _securityDescriptor != IntPtr.Zero;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal string GetCacheDependencyPath() {
            // if security descriptor is invalid, we cannot cache it
            if (_securityDescriptor == UnsafeNativeMethods.INVALID_HANDLE_VALUE) {
                Debug.Trace("FAM", "GetCacheDependencyPath: invalid security descriptor");
                return null;
            }
            // if security descriptor is valid (file exists), cache it with a dependency on the file name
            if (_securityDescriptor != IntPtr.Zero) {
                Debug.Trace("FAM", "GetCacheDependencyPath: valid security descriptor");
                return _FileName;
            }
            // file does not exist, but if it's path is beneath the app root, we will cache it and
            // use the first existing directory as the cache depenedency path
            string existingDir = FileUtil.GetFirstExistingDirectory(AppRoot, _FileName);

#if DBG
            if (existingDir != null) {
                Debug.Trace("FAM", "GetCacheDependencyPath: beneath app root");
            }
            else {
                Debug.Trace("FAM", "GetCacheDependencyPath: not beneath app root");
            }
#endif
            return existingDir;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static string AppRoot {
            get {
                string appRoot = _AppRoot;
                if (appRoot == null) {
                    InternalSecurityPermissions.AppPathDiscovery.Assert();
                    appRoot = Path.GetFullPath(HttpRuntime.AppDomainAppPathInternal);
                    appRoot = FileUtil.RemoveTrailingDirectoryBackSlash(appRoot);
                }
                return appRoot;
            }
        }

        void IDisposable.Dispose()
        {
            FreeSecurityDescriptor();
            GC.SuppressFinalize(this);
        }
        private  IntPtr               _securityDescriptor;
        internal bool                 _AnonymousAccessChecked  = false;
        internal bool                 _AnonymousAccess         = false;
        private  bool                 _SecurityDescriptorBeingFreed        = false;
        private  string               _FileName                = null;
        private  ReadWriteSpinLock    _Lock                    = new ReadWriteSpinLock();
        private static string         _AppRoot                 = null;
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    internal class FileAccessFailedErrorFormatter : ErrorFormatter {
        private String _strFile;

        internal FileAccessFailedErrorFormatter(string strFile) {
            _strFile = strFile;
            if (_strFile == null)
                _strFile = String.Empty;
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Assess_Denied_Title);}
            //get { return "Access Denied Error";}
        }

        protected override string Description {
            get {
                return SR.GetString(SR.Assess_Denied_Description3);
                //return "An error occurred while accessing the resources required to serve this request. &nbsp; This typically happens if you do not have permissions to view the file you are trying to access.";
            }
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.Assess_Denied_Section_Title3); }
            //get { return "Error message 401.3";}
        }

        protected override string MiscSectionContent {
            get {
                string miscContent;
                if (_strFile.Length > 0)
                    miscContent = SR.GetString(SR.Assess_Denied_Misc_Content3, HttpRuntime.GetSafePath(_strFile));
                //return "Access is denied due to NT ACLs on the requested file. Ask the web server's administrator to give you access to "+ _strFile + ".";
                else
                    miscContent = SR.GetString(SR.Assess_Denied_Misc_Content3_2);

                AdaptiveMiscContent.Add(miscContent);
                return miscContent;
            }
        }

        protected override string ColoredSquareTitle {
            get { return null;}
        }

        protected override string ColoredSquareContent {
            get { return null;}
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }
    }
}
