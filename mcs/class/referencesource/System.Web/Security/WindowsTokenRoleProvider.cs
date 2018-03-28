//------------------------------------------------------------------------------
// <copyright file="WindowsTokenRoleProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Web.Configuration;
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Runtime.Serialization;
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Data;
    using  System.Data.SqlClient;
    using  System.Data.SqlTypes;
    using  System.Text;
    using  System.Configuration.Provider;
    using  System.Web.Hosting;
    using  System.Threading;
    using  System.Web.Util;

    public class WindowsTokenRoleProvider : RoleProvider {

        private static string _MachineName = null;
        private string  _AppName;

        public override string ApplicationName
        {
            get { return _AppName; }
            set {
                _AppName = value;

                if ( _AppName.Length > 256 )
                {
                    throw new ProviderException( SR.GetString(SR.Provider_application_name_too_long)  );
                }
            }
        }

        public override void Initialize(string name, NameValueCollection config){
            if (String.IsNullOrEmpty(name))
                name = "WindowsTokenProvider";
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.RoleWindowsTokenProvider_description));
            }
            base.Initialize(name, config);

            if (config == null)
               throw new ArgumentNullException("config");
            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        public bool IsUserInRole(string username, System.Security.Principal.WindowsBuiltInRole role){
            if (username == null)
                throw new ArgumentNullException("username");
            username = username.Trim();
            WindowsIdentity     wi = GetCurrentWindowsIdentityAndCheckName(username);
            if (username.Length < 1)
                return false;

            WindowsPrincipal    wp = new WindowsPrincipal(wi);
            return wp.IsInRole(role);
        }

        public override bool IsUserInRole(string username, string roleName){
            if (username == null)
                throw new ArgumentNullException("username");
            username = username.Trim();
            if (roleName == null)
                throw new ArgumentNullException("roleName");
            roleName = roleName.Trim();
            if (username.Length < 1)
                return false;
            StringBuilder error = new StringBuilder(1024);
            IntPtr token = GetCurrentTokenAndCheckName(username);
            switch (UnsafeNativeMethods.IsUserInRole(token, roleName, error, 1024)) {
                case 1:
                    return true;
                case 0:
                    return false;
            }
            throw new ProviderException(SR.GetString(SR.API_failed_due_to_error, error.ToString()));
        }

        public override string [] GetRolesForUser(string username){
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.API_not_supported_at_this_level);
            if (username == null)
                throw new ArgumentNullException("username");
            username = username.Trim();
            IntPtr token = GetCurrentTokenAndCheckName(username);
            if (username.Length < 1)
                return new string[0];
            StringBuilder allRoles = new StringBuilder(1024);
            StringBuilder error    = new StringBuilder(1024);

            int status = UnsafeNativeMethods.GetGroupsForUser(token, allRoles, 1024, error, 1024);
            if (status < 0)
            {
                allRoles = new StringBuilder(-status);
                status = UnsafeNativeMethods.GetGroupsForUser(token, allRoles, -status, error, 1024);
            }
            if (status <= 0)
                throw new ProviderException(SR.GetString(SR.API_failed_due_to_error, error.ToString()));
            string [] roles = allRoles.ToString().Split('\t');
            return AddLocalGroupsWithoutDomainNames(roles);
        }

        private static string [] AddLocalGroupsWithoutDomainNames(string [] roles)
        {
            string             computerName    = GetMachineName();
            int                len             = computerName.Length;

            for (int iter = 0; iter < roles.Length; iter++) {
                roles[iter] = roles[iter].Trim();
                if (roles[iter].ToLower(CultureInfo.InvariantCulture).StartsWith(computerName, StringComparison.Ordinal)) // Is it a local group?
                    roles[iter] = roles[iter].Substring(len);
            }
            return roles;
        }

        public override void CreateRole(string roleName)
        {
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole){
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }


        public override bool RoleExists(string roleName){
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        public override void  AddUsersToRoles(string [] usernames, string [] roleNames) {
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        public override void RemoveUsersFromRoles(string [] usernames, string [] roleNames) {
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        public override string [] GetUsersInRole(string roleName){
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        public override string [] GetAllRoles(){
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new ProviderException(SR.GetString(SR.Windows_Token_API_not_supported));
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private IntPtr GetCurrentTokenAndCheckName(string userName)
        {
            return GetCurrentWindowsIdentityAndCheckName(userName).Token;
        }
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static string GetMachineName()
        {
            if (_MachineName == null)
                _MachineName = (System.Environment.MachineName + "\\").ToLower(CultureInfo.InvariantCulture);
            return _MachineName;
        }
        private WindowsIdentity GetCurrentWindowsIdentityAndCheckName(string userName)
        {
            if (HostingEnvironment.IsHosted) {
                HttpContext context = HttpContext.Current;
                if (context == null || context.User == null)
                    throw new ProviderException(SR.GetString(SR.API_supported_for_current_user_only));
                if (!(context.User.Identity is WindowsIdentity))
                    throw new ProviderException(SR.GetString(SR.API_supported_for_current_user_only));
                if (!StringUtil.EqualsIgnoreCase(userName, context.User.Identity.Name))
                    throw new ProviderException(SR.GetString(SR.API_supported_for_current_user_only));
                return (WindowsIdentity)context.User.Identity;
            } else {
                IPrincipal user = Thread.CurrentPrincipal;
                if (user == null || user.Identity == null || !(user.Identity is WindowsIdentity))
                    throw new ProviderException(SR.GetString(SR.API_supported_for_current_user_only));
                if (!StringUtil.EqualsIgnoreCase(userName, user.Identity.Name))
                    throw new ProviderException(SR.GetString(SR.API_supported_for_current_user_only));
                return (WindowsIdentity)user.Identity;
            }
        }
    }
}
