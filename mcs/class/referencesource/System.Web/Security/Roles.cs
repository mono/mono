//------------------------------------------------------------------------------
// <copyright file="Roles.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Web.Configuration;
    using  System.Web.Management;
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Runtime.Serialization;
    using  System.Collections;
    using  System.Configuration.Provider;
    using  System.Configuration;
    using  System.Web.Hosting;
    using  System.Threading;
    using  System.Web.Util;
    using  System.Collections.Specialized;
    using System.Web.Compilation;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    // This has no hosting permission demands because of DevDiv Bugs 31461: ClientAppSvcs: ASP.net Provider support
    static public class Roles
    {

        static public RoleProvider Provider {
            get {
                EnsureEnabled();
                if (s_Provider == null) {
                    throw new InvalidOperationException(SR.GetString(SR.Def_role_provider_not_found));
                }
                return s_Provider;
            }
        }

        static public RoleProviderCollection Providers               { get { EnsureEnabled(); return s_Providers;} }

        static public string                 CookieName              { get { Initialize(); return s_CookieName; }}

        static public bool                   CacheRolesInCookie      { get { Initialize(); return s_CacheRolesInCookie; }}

        static public int                    CookieTimeout           { get { Initialize(); return s_CookieTimeout; }}

        static public string                 CookiePath              { get { Initialize(); return s_CookiePath; }}

        static public bool                   CookieRequireSSL        { get { Initialize(); return s_CookieRequireSSL; }}

        static public bool                   CookieSlidingExpiration { get { Initialize(); return s_CookieSlidingExpiration; }}

        static public CookieProtection       CookieProtectionValue   { get { Initialize(); return s_CookieProtection; }}

        static public bool                   CreatePersistentCookie  { get { Initialize(); return s_CreatePersistentCookie; } }

        static public string                 Domain                  { get { Initialize(); return s_Domain; } }

        static public int                    MaxCachedResults        { get { Initialize(); return s_MaxCachedResults; } }


        static public bool Enabled
        {
            get {
                if (HostingEnvironment.IsHosted && !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low))
                    return false;

                if (!s_Initialized && !s_EnabledSet) {
                    RoleManagerSection config = RuntimeConfig.GetAppConfig().RoleManager;
                    s_Enabled = config.Enabled;
                    s_EnabledSet = true;
                }

                return s_Enabled;
            }
            set {
                BuildManager.ThrowIfPreAppStartNotRunning();
                s_Enabled = value;
                s_EnabledSet = true;
            }
        }


        static public string ApplicationName
        {
            get { return Provider.ApplicationName; }
            set { Provider.ApplicationName = value; }
        }

        // authorization

        static public bool IsUserInRole(string username, string roleName) {
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null)
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);

            EnsureEnabled();
            bool isUserInRole = false;
            bool isRolePrincipal = false;
            try {
                SecUtility.CheckParameter(ref roleName, true, true, true, 0, "roleName");
                SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
                if (username.Length < 1)
                    return false;
                IPrincipal user = GetCurrentUser();
                if (user != null && user is RolePrincipal && ((RolePrincipal)user).ProviderName == Provider.Name && StringUtil.EqualsIgnoreCase(username, user.Identity.Name))
                    isUserInRole = user.IsInRole(roleName);
                else
                    isUserInRole = Provider.IsUserInRole(username, roleName);
                return isUserInRole;
            } finally {
                if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null) {
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.AppSvc)) {
                        string status = SR.Resources.GetString(isUserInRole ? SR.Etw_Success : SR.Etw_Failure, CultureInfo.InstalledUICulture);
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_IS_USER_IN_ROLE, HttpContext.Current.WorkerRequest, isRolePrincipal ? "RolePrincipal" : Provider.GetType().FullName, username, roleName, status);
                    }

                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, isRolePrincipal ? "RolePrincipal" : Provider.GetType().FullName, username);
                }
            }
        }


        static public bool IsUserInRole(string roleName) {
            return IsUserInRole(GetCurrentUserName(), roleName);
        }


        static public string[] GetRolesForUser (string username){
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null)
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);

            EnsureEnabled();
            string[] roles = null;
            bool isRolePrincipal = false;
            try {
                SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
                if (username.Length < 1) {
                    roles = new string[0];
                    return roles;
                }
                IPrincipal user = GetCurrentUser();
                if (user != null && user is RolePrincipal && ((RolePrincipal)user).ProviderName == Provider.Name && StringUtil.EqualsIgnoreCase(username, user.Identity.Name)) {
                    roles = ((RolePrincipal)user).GetRoles();
                    isRolePrincipal = true;
                } else {
                    roles = Provider.GetRolesForUser(username);
                }
                return roles;
            } finally {
                if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc) && HttpContext.Current != null) {
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.AppSvc)) {
                        string roleNames = null;
                        if (roles != null && roles.Length > 0)
                            roleNames = roles[0];
                        for (int i = 1; i < roles.Length; i++)
                            roleNames += "," + roles[i];

                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_GET_USER_ROLES, HttpContext.Current.WorkerRequest, isRolePrincipal ? "RolePrincipal" : Provider.GetType().FullName, username, roleNames, null);
                    }
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, isRolePrincipal ? "RolePrincipal" : Provider.GetType().FullName, username);
                }
            }
        }

        static public string[] GetRolesForUser (){
            return GetRolesForUser(GetCurrentUserName());
        }


        // role administration
        //

        static public string[] GetUsersInRole(string roleName){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");
            return Provider.GetUsersInRole(roleName);
        }


        static public void CreateRole(string roleName){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");
            Provider.CreateRole(roleName);
        }


        static public bool DeleteRole(string roleName, bool throwOnPopulatedRole){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");

            bool roleDeleted = Provider.DeleteRole(roleName, throwOnPopulatedRole);

            try {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached && user.IsInRole(roleName))
                    user.SetDirty();
            }
            catch { }

            return roleDeleted;
        }

        static public bool DeleteRole(string roleName)
        {
            return DeleteRole(roleName, true);
        }


        static public bool RoleExists(string roleName){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");
            return Provider.RoleExists(roleName);
        }



        static public void AddUserToRole(string username, string roleName){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");
            SecUtility.CheckParameter(ref username,  true, true, true, 0,  "username");
            Provider.AddUsersToRoles(new string [] {username}, new string [] {roleName});
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached && StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                    user.SetDirty();
            }
            catch { }
        }


        static public void AddUserToRoles(string username, string[] roleNames){
            EnsureEnabled();

            SecUtility.CheckParameter(ref username,  true, true, true,  0,  "username");

            SecUtility.CheckArrayParameter( ref roleNames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "roleNames");

            Provider.AddUsersToRoles(new string [] {username}, roleNames);
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached && StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                    user.SetDirty();
            }
            catch { }
        }


        static public void AddUsersToRole(string[] usernames, string roleName){
            EnsureEnabled();

            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");

            SecUtility.CheckArrayParameter( ref usernames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "usernames");

            Provider.AddUsersToRoles(usernames, new string [] {roleName});
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached)
                    foreach(string username in usernames)
                        if (StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                        {
                            user.SetDirty();
                            break;
                        }
            }
            catch { }
        }


        static public void AddUsersToRoles(string[] usernames, string [] roleNames){
            EnsureEnabled();

            SecUtility.CheckArrayParameter( ref roleNames,
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

            Provider.AddUsersToRoles(usernames, roleNames);
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached)
                    foreach (string username in usernames)
                        if (StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                        {
                            user.SetDirty();
                            break;
                        }
            }
            catch { }
        }


        static public void RemoveUserFromRole(string username, string roleName){
            EnsureEnabled();
            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");
            SecUtility.CheckParameter(ref username,  true, true, true,  0,  "username");
            Provider.RemoveUsersFromRoles(new string [] {username}, new string [] {roleName});
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached && StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                    user.SetDirty();
            }
            catch { }
        }


        static public void RemoveUserFromRoles(string username, string[] roleNames){
            EnsureEnabled();

            SecUtility.CheckParameter(ref username,  true, true, true,  0,  "username");

            SecUtility.CheckArrayParameter( ref roleNames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "roleNames");

            Provider.RemoveUsersFromRoles(new string [] {username}, roleNames);
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached && StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                    user.SetDirty();
            }
            catch { }
        }


        static public void RemoveUsersFromRole(string[] usernames, string roleName){
            EnsureEnabled();

            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");

            SecUtility.CheckArrayParameter( ref usernames,
                                            true,
                                            true,
                                            true,
                                            0,
                                            "usernames");

            Provider.RemoveUsersFromRoles(usernames, new string[] { roleName });
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached)
                    foreach (string username in usernames)
                        if (StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                        {
                            user.SetDirty();
                            break;
                        }
            }
            catch { }
        }


        static public void RemoveUsersFromRoles(string[] usernames, string [] roleNames){
            EnsureEnabled();

            SecUtility.CheckArrayParameter( ref roleNames,
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

            Provider.RemoveUsersFromRoles(usernames, roleNames);
            try
            {
                RolePrincipal user = GetCurrentUser() as RolePrincipal;
                if (user != null && user.ProviderName == Provider.Name && user.IsRoleListCached)
                    foreach (string username in usernames)
                        if (StringUtil.EqualsIgnoreCase(user.Identity.Name, username))
                        {
                            user.SetDirty();
                            break;
                        }
            }
            catch { }
        }


        public static string[] GetAllRoles() {
            EnsureEnabled();
            return Provider.GetAllRoles();
        }


        public static void DeleteCookie() {
            EnsureEnabled();
            if (CookieName == null || CookieName.Length < 1)
                return;

            HttpContext context = HttpContext.Current;
            if (context == null || !context.Request.Browser.Cookies)
                return;
            string cookieValue = String.Empty;
            if (context.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                cookieValue = "NoCookie";
            HttpCookie cookie = new HttpCookie(CookieName, cookieValue);
            cookie.HttpOnly = true;
            cookie.Path = CookiePath;
            cookie.Domain = Domain;
            cookie.Expires = new System.DateTime(1999, 10, 12);
            cookie.Secure = CookieRequireSSL;
            context.Response.Cookies.RemoveCookie(CookieName);
            context.Response.Cookies.Add(cookie);
        }

        static public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            EnsureEnabled();

            SecUtility.CheckParameter(ref roleName, true, true, true, 0,  "roleName");

            SecUtility.CheckParameter( ref usernameToMatch,
                                       true,
                                       true,
                                       false,
                                       0,
                                       "usernameToMatch");

            return Provider.FindUsersInRole(roleName, usernameToMatch);
        }

        static private void EnsureEnabled()
        {
            Initialize();
            if (!s_Enabled)
                throw new ProviderException(SR.GetString(SR.Roles_feature_not_enabled));
        }

        static private void Initialize()
        {
            if (s_Initialized) {
                if (s_InitializeException != null) {
                    throw s_InitializeException;
                }
                if (s_InitializedDefaultProvider) {
                    return;
                }
            }

            lock (s_lock) {
                if (s_Initialized) {
                    if (s_InitializeException != null) {
                        throw s_InitializeException;
                    }
                    if (s_InitializedDefaultProvider) {
                        return;
                    }
                }

                try
                {
                    if (HostingEnvironment.IsHosted)
                        HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

                    RoleManagerSection settings = RuntimeConfig.GetAppConfig().RoleManager;
                        //s_InitializeException = new ProviderException(SR.GetString(SR.Roles_feature_not_enabled));
                    if (!s_EnabledSet) {
                        s_Enabled = settings.Enabled;
                    }
                    s_CookieName = settings.CookieName;
                    s_CacheRolesInCookie = settings.CacheRolesInCookie;
                    s_CookieTimeout = (int)settings.CookieTimeout.TotalMinutes;
                    s_CookiePath = settings.CookiePath;
                    s_CookieRequireSSL = settings.CookieRequireSSL;
                    s_CookieSlidingExpiration = settings.CookieSlidingExpiration;
                    s_CookieProtection = settings.CookieProtection;
                    s_Domain = settings.Domain;
                    s_CreatePersistentCookie = settings.CreatePersistentCookie;
                    s_MaxCachedResults = settings.MaxCachedResults;
                    if (s_Enabled) { // Instantiate providers only if feature is enabled
                        if (s_MaxCachedResults < 0)
                        {
                            throw new ProviderException(SR.GetString(SR.Value_must_be_non_negative_integer, "maxCachedResults"));
                        }
                        InitializeSettings(settings);
                        InitializeDefaultProvider(settings);
                    }
                } catch (Exception e) {
                    s_InitializeException = e;
                }
                s_Initialized = true;
            }

            if (s_InitializeException != null)
                throw s_InitializeException;
        }

        private static void InitializeSettings(RoleManagerSection settings) {
            if (!s_Initialized) {
                s_Providers = new RoleProviderCollection();

                if (HostingEnvironment.IsHosted) {
                    ProvidersHelper.InstantiateProviders(settings.Providers, s_Providers, typeof(RoleProvider));
                }
                else {
                    foreach (ProviderSettings ps in settings.Providers) {
                        Type t = Type.GetType(ps.Type, true, true);
                        if (!typeof(RoleProvider).IsAssignableFrom(t))
                            throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, typeof(RoleProvider).ToString()));
                        RoleProvider provider = (RoleProvider)Activator.CreateInstance(t);
                        NameValueCollection pars = ps.Parameters;
                        NameValueCollection cloneParams = new NameValueCollection(pars.Count, StringComparer.Ordinal);
                        foreach (string key in pars)
                            cloneParams[key] = pars[key];
                        provider.Initialize(ps.Name, cloneParams);
                        s_Providers.Add(provider);
                    }
                }
            }
        }

        private static void InitializeDefaultProvider(RoleManagerSection settings) {
            bool canInitializeDefaultProvider = (!HostingEnvironment.IsHosted || BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit);
            if (!s_InitializedDefaultProvider && canInitializeDefaultProvider) {
                Debug.Assert(s_Providers != null);
                s_Providers.SetReadOnly();

                if (settings.DefaultProvider == null) {
                    s_InitializeException = new ProviderException(SR.GetString(SR.Def_role_provider_not_specified));
                }
                else {
                    try {
                        s_Provider = s_Providers[settings.DefaultProvider];
                    }
                    catch { }
                }

                if (s_Provider == null) {
                    s_InitializeException = new ConfigurationErrorsException(SR.GetString(SR.Def_role_provider_not_found), settings.ElementInformation.Properties["defaultProvider"].Source, settings.ElementInformation.Properties["defaultProvider"].LineNumber);
                }

                s_InitializedDefaultProvider = true;
            }
        }

        static private RoleProvider           s_Provider;
        static private bool                   s_Enabled;
        static private string                 s_CookieName;
        static private bool                   s_CacheRolesInCookie;
        static private int                    s_CookieTimeout;
        static private string                 s_CookiePath;
        static private bool                   s_CookieRequireSSL;
        static private bool                   s_CookieSlidingExpiration;
        static private CookieProtection       s_CookieProtection;
        static private string                 s_Domain;
        static private bool                   s_Initialized;
        static private bool                   s_InitializedDefaultProvider;
        static private bool                   s_EnabledSet;
        static private RoleProviderCollection s_Providers;
        private static Exception              s_InitializeException = null;
        private static bool                   s_CreatePersistentCookie;
        private static object                 s_lock = new object();
        private static int                    s_MaxCachedResults = 25;

        private static string GetCurrentUserName()
        {
            IPrincipal user = GetCurrentUser();
            if (user == null || user.Identity == null)
                return String.Empty;
            else
                return user.Identity.Name;
        }

        private static IPrincipal GetCurrentUser()
        {
            if (HostingEnvironment.IsHosted) {
                HttpContext cur = HttpContext.Current;
                if (cur != null)
                    return cur.User;
            }
            return Thread.CurrentPrincipal;
        }

    }

    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////

   // This has no hosting permission demands because of DevDiv Bugs 31461: ClientAppSvcs: ASP.net Provider support
   public sealed class RoleProviderCollection : ProviderCollection
   {

        public override void Add(ProviderBase provider) {
            if( provider == null )
            {
                throw new ArgumentNullException( "provider" );
            }

            if( !( provider is RoleProvider ) )
            {
                throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, typeof(RoleProvider).ToString()), "provider");
            }

            base.Add(provider);
        }

       new public RoleProvider this[string name] {
           get {
               return (RoleProvider) base[name];
           }
       }

       public void CopyTo(RoleProvider [] array, int index)
       {
           base.CopyTo(array, index);
       }
   }
}
