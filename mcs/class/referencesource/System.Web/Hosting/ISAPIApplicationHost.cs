//------------------------------------------------------------------------------
// <copyright file="ISAPIApplicationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Application host for IIS 5.0 and 6.0
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using Microsoft.Win32;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Management;
    using System.Diagnostics.CodeAnalysis;


    // helper class to implement AppHost based on ISAPI
    internal class ISAPIApplicationHost : MarshalByRefObject, IApplicationHost {
        private String _appId;
        private String _siteID;
        private String _siteName;
        private VirtualPath _virtualPath;
        private String _physicalPath;
        private IProcessHostSupportFunctions _functions;
        private String _iisVersion;

        private const int MAX_PATH = 260;
        private const string LMW3SVC_PREFIX = "/LM/W3SVC/";
        private const string DEFAULT_SITEID = "1";
        private const string DEFAULT_APPID_PREFIX = "/LM/W3SVC/1/ROOT";

        internal ISAPIApplicationHost(string appIdOrVirtualPath, string physicalPath, bool validatePhysicalPath, IProcessHostSupportFunctions functions, string iisVersion = null) {
            _iisVersion = iisVersion;
            // appIdOrVirtualPath is either a full metabase path, or just a virtual path
            // e.g. /LM/W3SVC/1/Root/MyApp ot /MyApp
            // Figure out which one we have, and get the other one from it
            _functions = functions;

            // make sure the functions are set in the default domain
            if (null == _functions) {
                ProcessHost h = ProcessHost.DefaultHost;

                if (null != h) {
                    _functions = h.SupportFunctions;

                    if (null != _functions) {
                        HostingEnvironment.SupportFunctions = _functions;
                    }
                }
            }

            IServerConfig serverConfig = ServerConfig.GetDefaultDomainInstance(_iisVersion);

            if (StringUtil.StringStartsWithIgnoreCase(appIdOrVirtualPath, LMW3SVC_PREFIX)) {
                _appId = appIdOrVirtualPath;
                _virtualPath = VirtualPath.Create(ExtractVPathFromAppId(_appId));
                _siteID = ExtractSiteIdFromAppId(_appId);
                _siteName = serverConfig.GetSiteNameFromSiteID(_siteID);
            }
            else {
                _virtualPath = VirtualPath.Create(appIdOrVirtualPath);
                _appId = GetDefaultAppIdFromVPath(_virtualPath.VirtualPathString);
                _siteID = DEFAULT_SITEID;
                _siteName = serverConfig.GetSiteNameFromSiteID(_siteID);
            }

            // Get the physical path from the virtual path if it wasn't passed in
            if (physicalPath == null) {
                _physicalPath = serverConfig.MapPath(this, _virtualPath);
            }
            else {
                _physicalPath = physicalPath;
            }

            if (validatePhysicalPath) {
                if (!Directory.Exists(_physicalPath)) {
                    throw new HttpException(SR.GetString(SR.Invalid_IIS_app, appIdOrVirtualPath));
                }
            }
        }

        internal ISAPIApplicationHost(string appIdOrVirtualPath, string physicalPath, bool validatePhysicalPath)
            :this(appIdOrVirtualPath, physicalPath, validatePhysicalPath, null)
        {}

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        // IApplicationHost implementation
        string IApplicationHost.GetVirtualPath() {
            return _virtualPath.VirtualPathString;
        }

        String IApplicationHost.GetPhysicalPath() {
            return _physicalPath;
        }

        IConfigMapPathFactory IApplicationHost.GetConfigMapPathFactory() {
            return new ISAPIConfigMapPathFactory();
        }

        IntPtr IApplicationHost.GetConfigToken() {
            if (null != _functions) {
                return _functions.GetConfigToken(_appId);
            }
            IntPtr token = IntPtr.Zero;

            String username;
            String password;
            IServerConfig serverConfig = ServerConfig.GetDefaultDomainInstance(_iisVersion);
            bool hasUncUser = serverConfig.GetUncUser(this, _virtualPath, out username, out password);
            if (hasUncUser) {
                try {
                    String error;
                    token = IdentitySection.CreateUserToken(username, password, out error);
                }
                catch {
                }
            }

            return token;
        }

        String IApplicationHost.GetSiteName() {
            return _siteName;
        }

        String IApplicationHost.GetSiteID() {
            return _siteID;
        }

        void IApplicationHost.MessageReceived() {
        // make this method call a no-op 
        // it will be removed soon altogether
        }

        internal string AppId {
            get { return _appId; }
        }

        private static String ExtractVPathFromAppId(string id) {
            // app id is /LM/W3SVC/1/ROOT for root or /LM/W3SVC/1/ROOT/VDIR

            // find fifth / (assuming it starts with /)
            int si = 0;
            for (int i = 1; i < 5; i++) {
                si = id.IndexOf('/', si+1);
                if (si < 0)
                    break;
            }

            if (si < 0) // root?
                return "/";
            else
                return id.Substring(si);
        }

        private static String GetDefaultAppIdFromVPath(string virtualPath) {
            if (virtualPath.Length == 1 && virtualPath[0] == '/') {
                return DEFAULT_APPID_PREFIX;
            }
            else {
                return DEFAULT_APPID_PREFIX + virtualPath;
            }
        }

        private static String ExtractSiteIdFromAppId(string id) {
            // app id is /LM/W3SVC/1/ROOT for root or /LM/W3SVC/1/ROOT/VDIR
            // the site id is right after prefix
            int offset = LMW3SVC_PREFIX.Length;
            int si = id.IndexOf('/', offset);
            return (si > 0) ? id.Substring(offset, si - offset) : DEFAULT_SITEID;
        }

        internal IProcessHostSupportFunctions SupportFunctions {
            get {
                return _functions;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method's caller is trusted.")]
        internal string ResolveRootWebConfigPath() {
            string rootWebConfigPath = null;

            if (null != _functions) {
                rootWebConfigPath = _functions.GetRootWebConfigFilename();
            }

            return rootWebConfigPath;
        }


    }

    //
    // Create an instance of IConfigMapPath in the worker appdomain.
    // By making the class Serializable, the call to IConfigMapPathFactory.Create()
    // will execute in the worker appdomain.
    // 
    [Serializable()]
    internal class ISAPIConfigMapPathFactory : IConfigMapPathFactory {
        IConfigMapPath IConfigMapPathFactory.Create(string virtualPath, string physicalPath) {
            return IISMapPath.GetInstance();
        }
    }
}
