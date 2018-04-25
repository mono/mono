//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    public class WindowsUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        bool includeWindowsGroups;

        public WindowsUserNameSecurityTokenAuthenticator()
            : this(WindowsClaimSet.DefaultIncludeWindowsGroups)
        {
        }

        public WindowsUserNameSecurityTokenAuthenticator(bool includeWindowsGroups)
        {
            this.includeWindowsGroups = includeWindowsGroups;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            string domain = null;
            string[] strings = userName.Split('\\');
            if (strings.Length != 1)
            {
                if (strings.Length != 2 || string.IsNullOrEmpty(strings[0]))
                {
                    // Only support one slash and domain cannot be empty (consistent with windowslogon).
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.IncorrectUserNameFormat));
                }

                // This is the downlevel case - domain\userName
                userName = strings[1];
                domain = strings[0];
            }

            const uint LOGON32_PROVIDER_DEFAULT = 0;
            const uint LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
            SafeCloseHandle tokenHandle = null;
            try
            {
                if (!NativeMethods.LogonUser(userName, domain, password, LOGON32_LOGON_NETWORK_CLEARTEXT, LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.FailLogonUser, userName), new Win32Exception(error)));
                }

                WindowsIdentity windowsIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle(), SecurityUtils.AuthTypeBasic);
                WindowsClaimSet claimSet = new WindowsClaimSet(windowsIdentity, SecurityUtils.AuthTypeBasic, this.includeWindowsGroups, false);
                return SecurityUtils.CreateAuthorizationPolicies(claimSet, claimSet.ExpirationTime);
            }
            finally
            {
                if (tokenHandle != null)
                    tokenHandle.Close();
            }
        }
    }
}
