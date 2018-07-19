//------------------------------------------------------------------------------
// <copyright file="AuthStoreRoleProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.Security
{
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Security;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Text;
    using System.Configuration.Provider;
    using System.Configuration;
    using System.Data.OleDb;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Threading;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class AuthorizationStoreRoleProvider : RoleProvider
    {
        ////////////////////////////////////////////////////////////
        // Public properties

        public override string ApplicationName
        {
            get { return _AppName; }
            set {
                if (_AppName != value) {
                    if ( value.Length > 256 )
                        throw new ProviderException( SR.GetString(SR.Provider_application_name_too_long)  );
                    _AppName = value;
                    _InitAppDone = false;
                }
            }
        }

        public string ScopeName
        {
            get { return _ScopeName; }
            set {
                if( _ScopeName != value ) {
                    _ScopeName = value;
                    _InitAppDone = false;
                }
            }
        }

        public int CacheRefreshInterval
        {
            get{ return _CacheRefreshInterval; }
        }

        ////////////////////////////////////////////////////////////
        // Public Methods

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void Initialize(string name, NameValueCollection config)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (String.IsNullOrEmpty(name))
                name = "AuthorizationStoreRoleProvider";
            if (config == null)
               throw new ArgumentNullException("config");
           if (string.IsNullOrEmpty(config["description"])) {
               config.Remove("description");
               config.Add("description", SR.GetString(SR.RoleAuthStoreProvider_description));
           }
            base.Initialize(name, config);

            _CacheRefreshInterval = SecUtility.GetIntValue( config, "cacheRefreshInterval", 60, false, 0 );

            _ScopeName = config["scopeName"];
            if (_ScopeName != null && _ScopeName.Length == 0)
                _ScopeName = null;

            _ConnectionString = config["connectionStringName"];
            if (_ConnectionString == null || _ConnectionString.Length < 1)
                throw new ProviderException(SR.GetString(SR.Connection_name_not_specified));
            ConnectionStringsSection sec1 = null;
            sec1 = RuntimeConfig.GetAppConfig().ConnectionStrings;
            ConnectionStringSettings connObj = sec1.ConnectionStrings[_ConnectionString];
            if (connObj == null)
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, _ConnectionString));

            if (string.IsNullOrEmpty(connObj.ConnectionString))
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, _ConnectionString));

            _ConnectionString = connObj.ConnectionString;
            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

            config.Remove("connectionStringName");
            config.Remove("cacheRefreshInterval");
            config.Remove("applicationName");
            config.Remove("scopeName");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
            if (username.Length < 1)
                return false;
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            bool foundRole = IsUserInRoleCore(username, roleName);
            return foundRole;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override string[] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
            if (username.Length < 1)
                return new string[0];
            string[] allRoles = GetRolesForUserCore(username);
            return allRoles;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void CreateRole(string roleName)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.API_not_supported_at_this_level);
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            InitApp();
            object[] args = new object[2];
            args[0] = roleName;
            args[1] = null;
            object role = CallMethod(_ObjAzScope != null ? _ObjAzScope : _ObjAzApplication, "CreateRole", args);
            args[0] = 0;
            args[1] = null;

            try {
                try {
                    CallMethod(role, "Submit", args);
                } finally {
                    //
                    // Release the handle to the underlying object
                    //

                    Marshal.FinalReleaseComObject(role);
                }
            } catch {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.API_not_supported_at_this_level);
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            InitApp();
            if (throwOnPopulatedRole)
            {
                string[] users;
                try
                {
                    users = GetUsersInRole(roleName);
                }
                catch
                {
                    return false;
                }

                if (users.Length != 0)
                    throw new ProviderException(SR.GetString(SR.Role_is_not_empty));
            }

            object[] args = new object[2];

            args[0] = roleName;
            args[1] = null;
            CallMethod(_ObjAzScope != null ? _ObjAzScope : _ObjAzApplication, "DeleteRole", args);

            args[0] = 0;
            args[1] = null;
            CallMethod(_ObjAzScope != null ? _ObjAzScope : _ObjAzApplication, "Submit", args);

            return true;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            bool found = false;
            object role = null;
            try {
                role = GetRole(roleName);
                found = (role != null);
            } catch (TargetInvocationException e) {
                // "Element not found" error is expected
                COMException ce = (e.InnerException as COMException);
                if (ce != null && (uint)ce.ErrorCode == 0x80070490) {
                    return false;
                }
                throw;
            } finally {
                if (role != null)
                    Marshal.FinalReleaseComObject(role);
            }
            return found;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.API_not_supported_at_this_level);
            SecUtility.CheckArrayParameter(ref roleNames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "roleNames");

            SecUtility.CheckArrayParameter( ref usernames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "usernames");

            int      index = 0;
            object[] args  = new object[ 2 ];
            object[] roles = new object[ roleNames.Length ];

            foreach( string rolename in roleNames )
            {
                roles[ index++ ] = GetRole( rolename );
            }

            try {
                try {
                    foreach (object role in roles) {
                        foreach (string username in usernames) {
                            args[0] = username;
                            args[1] = null;
                            CallMethod(role, "AddMemberName", args);
                        }
                    }

                    foreach (object role in roles) {
                        args[0] = 0;
                        args[1] = null;
                        CallMethod(role, "Submit", args);
                    }
                } finally {
                    foreach (object role in roles) {
                        Marshal.FinalReleaseComObject(role);
                    }
                }
            } catch {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.API_not_supported_at_this_level);
            SecUtility.CheckArrayParameter(ref roleNames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "roleNames");

            SecUtility.CheckArrayParameter( ref userNames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "userNames");

            int      index = 0;
            object[] args  = new object[ 2 ];
            object[] roles = new object[ roleNames.Length ];

            foreach( string rolename in roleNames )
            {
                roles[ index++ ] = GetRole( rolename );
            }

            try {
                try {
                    foreach (object role in roles) {
                        foreach (string username in userNames) {
                            args[0] = username;
                            args[1] = null;
                            CallMethod(role, "DeleteMemberName", args);
                        }
                    }

                    foreach (object role in roles) {
                        args[0] = 0;
                        args[1] = null;
                        CallMethod(role, "Submit", args);
                    }
                } finally {
                    foreach (object role in roles) {
                        Marshal.FinalReleaseComObject(role);
                    }
                }
            } catch {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override string[] GetUsersInRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
            object role = GetRole(roleName);

            object memberNames;
            try {
                try {
                    memberNames = CallProperty(role, "MembersName", null);
                } finally {
                    //
                    // Release the handle to the underlying object
                    //

                    Marshal.FinalReleaseComObject(role);
                }
            } catch {
                throw;
            }

            StringCollection userNameCollection = new StringCollection();

            try
            {
                if ( HostingEnvironment.IsHosted && _XmlFileName != null )
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }

                try
                {
                    IEnumerable allUsers = (IEnumerable)memberNames;
                    foreach (object objUserName in allUsers)
                        userNameCollection.Add((string)objUserName);
                }
                finally
                {
                    if( HostingEnvironment.IsHosted && _XmlFileName != null )
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }

            string [] usersArray = new string[userNameCollection.Count];
            userNameCollection.CopyTo(usersArray, 0);
            return usersArray;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override string[] GetAllRoles()
        {
            InitApp();
            object objAllRoles = CallProperty(_ObjAzScope != null ? _ObjAzScope : _ObjAzApplication, "Roles", null);
            StringCollection roleNameCollection = new StringCollection();

            try
            {
                if( HostingEnvironment.IsHosted && _XmlFileName != null )
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }

                try
                {
                    IEnumerable allRoles = (IEnumerable)objAllRoles;
                    foreach (object role in allRoles)
                    {
                        string name = (string)CallProperty(role, "Name", null);
                        roleNameCollection.Add(name);
                    }
                }
                finally
                {
                    if( HostingEnvironment.IsHosted && _XmlFileName != null )
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }

            string[] rolesArray = new string[roleNameCollection.Count];
            roleNameCollection.CopyTo(rolesArray, 0);
            return rolesArray;
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        // Private Methods and data
        private string      _AppName;
        private string      _ConnectionString;
        private int         _CacheRefreshInterval;
        private string      _ScopeName;
        private object      _ObjAzApplication;
        private bool        _InitAppDone;
        private object      _ObjAzScope;
        private DateTime    _LastUpdateCacheDate;
        private object      _ObjAzAuthorizationStoreClass;
        private bool        _NewAuthInterface;
        private string      _XmlFileName;
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object CallMethod(object objectToCallOn, string methodName, object[] args)
        {
            if( HostingEnvironment.IsHosted && _XmlFileName != null) {
                InternalSecurityPermissions.Unrestricted.Assert();
            }

            try {
                using (new ApplicationImpersonationContext()) {
                    return objectToCallOn.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                                                                null, objectToCallOn, args, CultureInfo.InvariantCulture);
                }
            } catch {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object CallProperty(object objectToCallOn, string propName, object[] args)
        {
            if (HostingEnvironment.IsHosted && _XmlFileName != null) {
                InternalSecurityPermissions.Unrestricted.Assert();
            }

            try {
                using (new ApplicationImpersonationContext()) {
                    return objectToCallOn.GetType().InvokeMember(propName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                                                                null, objectToCallOn, args, CultureInfo.InvariantCulture);
                }
            } catch {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private void InitApp()
        {
            try {
                using (new ApplicationImpersonationContext()) {
                    if (_InitAppDone) {
                        if (DateTime.Now > _LastUpdateCacheDate.AddMinutes(CacheRefreshInterval)) {
                            _LastUpdateCacheDate = DateTime.Now;
                            CallMethod(_ObjAzAuthorizationStoreClass, "UpdateCache", null);
                        }
                        return;
                    }
                    lock (this) {
                        if (_InitAppDone)
                            return;
                        if (_ConnectionString.ToLower(CultureInfo.InvariantCulture).StartsWith("msxml://", StringComparison.Ordinal)) {
                            if (_ConnectionString.Contains("/~/")) {
                                string appPath = null;
                                if (HostingEnvironment.IsHosted)
                                    appPath = HttpRuntime.AppDomainAppPath;
                                else {
    #if !FEATURE_PAL // FEATURE_PAL does not enable the ProcessModule class
                                    Process p = Process.GetCurrentProcess();
                                    ProcessModule pm = (p != null ? p.MainModule : null);
                                    string exeName = (pm != null ? pm.FileName : null);
                                    if (exeName != null)
                                        appPath = System.IO.Path.GetDirectoryName(exeName);
    #endif // !FEATURE_PAL
                                    if (appPath == null || appPath.Length < 1)
                                        appPath = Environment.CurrentDirectory;
                                }
                                appPath = appPath.Replace('\\', '/');
                                _ConnectionString = _ConnectionString.Replace("~", appPath);
                            }
                            string fileName = _ConnectionString.Substring("msxml://".Length).Replace('/', '\\');

                            if( HostingEnvironment.IsHosted )
                            {
                                HttpRuntime.CheckFilePermission( fileName, false );
                            }

                            if (!FileUtil.FileExists(fileName)) {
                                throw new FileNotFoundException(SR.GetString(SR.AuthStore_policy_file_not_found,
                                                HttpRuntime.GetSafePath(fileName)));
                            }

                            _XmlFileName = fileName;
                        }

                        Type typeAzAuthorizationStoreClass = null;
                        try {
                            _NewAuthInterface = true;
                            typeAzAuthorizationStoreClass = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", 
                                                                         false /*throwOnError*/);
                            if (typeAzAuthorizationStoreClass == null)
                                typeAzAuthorizationStoreClass = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=1.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", 
                                                                         false /*throwOnError*/);
                            if (typeAzAuthorizationStoreClass == null) {
                                _NewAuthInterface = false;
                                typeAzAuthorizationStoreClass = Type.GetType("Microsoft.Interop.Security.AzRoles.AzAuthorizationStoreClass, Microsoft.Interop.Security.AzRoles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", 
                                                                         true /*throwOnError*/);
                            } 
                        } 
                        catch (FileNotFoundException e) {
                            HttpContext context = HttpContext.Current;
                            if (context == null)
                            {
                                throw new ProviderException(SR.GetString(SR.AuthStoreNotInstalled_Title), e);
                            }

                            context.Response.Clear();
                            context.Response.StatusCode = 500;
                            context.Response.Write(AuthStoreErrorFormatter.GetErrorText());
                            context.Response.End();
                        }

                        if ( HostingEnvironment.IsHosted && _XmlFileName != null )
                        {
                            InternalSecurityPermissions.Unrestricted.Assert();
                        }

                        _ObjAzAuthorizationStoreClass = Activator.CreateInstance(typeAzAuthorizationStoreClass);
                        object[] args = new object[] { 0, _ConnectionString, null };

                        CallMethod(_ObjAzAuthorizationStoreClass, "Initialize", args);

                        args = new object[2];
                        args[0] = _AppName;
                        args[1] = null;
                        if(_NewAuthInterface)
                        {
                            _ObjAzApplication = CallMethod(_ObjAzAuthorizationStoreClass, "OpenApplication2", args);
                        }
                        else
                        {
                            _ObjAzApplication = CallMethod(_ObjAzAuthorizationStoreClass, "OpenApplication", args);
                        }
                        if (_ObjAzApplication == null)
                            throw new ProviderException(SR.GetString(SR.AuthStore_Application_not_found));
                        _ObjAzScope = null;
                        if (!string.IsNullOrEmpty(_ScopeName)) {
                            args[0] = _ScopeName;
                            args[1] = null;
                            _ObjAzScope = CallMethod(_ObjAzApplication, "OpenScope", args);
                            if (_ObjAzScope == null)
                                throw new ProviderException(SR.GetString(SR.AuthStore_Scope_not_found));
                        }
                        _LastUpdateCacheDate = DateTime.Now;
                        _InitAppDone = true;
                    }
                }
            } catch {
                throw;
            }
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private IntPtr GetWindowsTokenWithAssert(string userName)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext context = HttpContext.Current;
                if (context != null && context.User != null && context.User.Identity != null && context.User.Identity is WindowsIdentity &&
                     StringUtil.EqualsIgnoreCase(userName, context.User.Identity.Name))
                {
                    return ((WindowsIdentity)context.User.Identity).Token;
                }
            }
            IPrincipal user = Thread.CurrentPrincipal;
            if (user != null && user.Identity != null && user.Identity is WindowsIdentity &&
                    StringUtil.EqualsIgnoreCase(userName, user.Identity.Name))
            {
                return ((WindowsIdentity)user.Identity).Token;
            }

            return IntPtr.Zero;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object GetClientContext(string userName)
        {
            InitApp();
            IntPtr token = GetWindowsTokenWithAssert(userName);
            if (token != IntPtr.Zero)
                return GetClientContextFromToken(token);
            else
                return GetClientContextFromName(userName);
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object GetClientContextFromToken(IntPtr token)
        {
            if (_NewAuthInterface)
            {
                object [] args = new object[3];
                args[0] = ( UInt32 )token;
                args[1] = 0;
                args[2] = null;
                return CallMethod(_ObjAzApplication, "InitializeClientContextFromToken2", args);
            }
            else
            {
                object [] args = new object[2];
                args[0] = ( UInt64 )token;
                args[1] = null;
                return CallMethod(_ObjAzApplication, "InitializeClientContextFromToken", args);
            }
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object GetClientContextFromName(string userName)
        {
            string[] names = userName.Split(new char[] { '\\' });
            string domain = null;
            if (names.Length > 1) {
                domain = names[0];
                userName = names[1];
            }

            object [] args = new object[3];
            args[0] = userName;
            args[1] = domain;
            args[2] = null;
            return CallMethod(_ObjAzApplication, "InitializeClientContextFromName", args);
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private bool IsUserInRoleCore(string username, string roleName)
        {
            object objClientContext = GetClientContext(username);
            if (objClientContext == null)
                return false;
            object objAllRoles = CallMethod(objClientContext, "GetRoles", new object[] { _ScopeName });
            if (objAllRoles == null || !(objAllRoles is IEnumerable))
                return false;

            try
            {
                if( HostingEnvironment.IsHosted && _XmlFileName != null )
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }

                try
                {
                    IEnumerable allRoles = (IEnumerable)objAllRoles;

                    foreach (object objRoleName in allRoles)
                    {
                        string strRoleName = (string)objRoleName;
                        if (strRoleName != null && StringUtil.EqualsIgnoreCase(strRoleName, roleName))
                            return true;
                    }
                    return false;
                }
                finally
                {
                    if( HostingEnvironment.IsHosted && _XmlFileName != null )
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private string[] GetRolesForUserCore(string username)
        {
            object objClientContext = GetClientContext(username);
            if (objClientContext == null)
                return new string[0];
            object objAllRoles = CallMethod(objClientContext, "GetRoles", new object[] { _ScopeName });
            if (objAllRoles == null || !(objAllRoles is IEnumerable))
                return new string[0];

            StringCollection roleNameCollection = new StringCollection();

            try
            {
                if( HostingEnvironment.IsHosted && _XmlFileName != null )
                {
                    InternalSecurityPermissions.Unrestricted.Assert();
                }

                try
                {
                    IEnumerable allRoles = (IEnumerable)objAllRoles;
                    foreach (object objRoleName in allRoles)
                    {
                        string strRoleName = (string)objRoleName;
                        if (strRoleName != null)
                            roleNameCollection.Add(strRoleName);
                    }
                }
                finally
                {
                if( HostingEnvironment.IsHosted && _XmlFileName != null )
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                throw;
            }

            string[] rolesArray = new string[roleNameCollection.Count];
            roleNameCollection.CopyTo(rolesArray, 0);
            return rolesArray;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private object GetRole(string roleName)
        {
            InitApp();
            object[] args = new object[2];
            args[0] = roleName;
            args[1] = null;
            return CallMethod(_ObjAzScope != null ? _ObjAzScope : _ObjAzApplication, "OpenRole", args);
        }
    }

    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    // ErrorFormatter for generating adaptive error for Authorization
    // Store Role Provider

    internal sealed class AuthStoreErrorFormatter : ErrorFormatter
    {
        private static String s_errMsg = null;
        private static Object s_Lock   = new Object();

        internal AuthStoreErrorFormatter()
        {
        }

        internal static String GetErrorText()
        {
            if( s_errMsg != null )
            {
                return s_errMsg;
            }

            lock( s_Lock )
            {
                if( s_errMsg != null )
                {
                    return s_errMsg;
                }

                AuthStoreErrorFormatter errFormatter = new AuthStoreErrorFormatter();

                s_errMsg = errFormatter.GetErrorMessage();
            }

            return s_errMsg;
        }

        protected override string ErrorTitle
        {
            get
            {
                return SR.GetString(SR.AuthStoreNotInstalled_Title) ;
            }
        }

        protected override string Description
        {
            get
            {
                return SR.GetString(SR.AuthStoreNotInstalled_Description) ;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return null;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return null;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                return null;
            }
        }

        protected override string ColoredSquareContent
        {
            get
            {
                return null;
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return false;
            }
        }
    }
}
