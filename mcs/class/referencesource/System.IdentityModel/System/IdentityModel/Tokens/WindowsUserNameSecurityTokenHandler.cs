//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Xml;
using System.Security.Claims;
using System.IdentityModel.Configuration;
using System.Collections.Generic;
using System.Runtime;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// The token handler will validated the Windows Username token.
    /// </summary>
    public class WindowsUserNameSecurityTokenHandler : UserNameSecurityTokenHandler
    {
        /// <summary>
        /// Initializes an instance of <see cref="WindowsUserNameSecurityTokenHandler"/>
        /// </summary>
        public WindowsUserNameSecurityTokenHandler()
        {
        }

        /// <summary>
        /// Returns true to indicate that the token handler can Validate
        /// UserNameSecurityToken.
        /// </summary>
        public override bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Validates a <see cref="UserNameSecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="UserNameSecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from<see cref="UserNameSecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">Configuration <see cref="SecurityTokenHandlerConfiguration"/>is null.</exception>
        /// <exception cref="ArgumentException">If username is not if the form 'user\domain'.</exception>
        /// <exception cref="SecurityTokenValidationException">LogonUser using the given token failed.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            UserNameSecurityToken usernameToken = token as UserNameSecurityToken;
            if (usernameToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID0018, typeof(UserNameSecurityToken)));
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            try
            {
                string userName = usernameToken.UserName;
                string password = usernameToken.Password;
                string domain = null;
                string[] strings = usernameToken.UserName.Split('\\');
                if (strings.Length != 1)
                {
                    if (strings.Length != 2 || string.IsNullOrEmpty(strings[0]))
                    {
                        // Only support one slash and domain cannot be empty (consistent with windowslogon).
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID4062));
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
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4063, userName), new Win32Exception(error)));
                    }

                    WindowsIdentity windowsIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle(), AuthenticationTypes.Password, WindowsAccountType.Normal, true);

                    // PARTIAL TRUST: will fail when adding claims, AddClaim is SecurityCritical.
                    windowsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated), ClaimValueTypes.DateTime));
                    windowsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password));

                    if (this.Configuration.SaveBootstrapContext)
                    {
                        if (RetainPassword)
                        {
                            windowsIdentity.BootstrapContext = new BootstrapContext(usernameToken, this);
                        }
                        else
                        {
                            windowsIdentity.BootstrapContext = new BootstrapContext(new UserNameSecurityToken(usernameToken.UserName, null), this);
                        }
                    }

                    this.TraceTokenValidationSuccess(token);

                    List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                    identities.Add(windowsIdentity);
                    return identities.AsReadOnly();
                }
                finally
                {
                    if (tokenHandle != null)
                    {
                        tokenHandle.Close();
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }
    }
}
