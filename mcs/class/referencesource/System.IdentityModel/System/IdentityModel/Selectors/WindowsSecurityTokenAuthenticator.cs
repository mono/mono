//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;

    public class WindowsSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        bool includeWindowsGroups;

        public WindowsSecurityTokenAuthenticator()
            : this(WindowsClaimSet.DefaultIncludeWindowsGroups)
        {
        }

        public WindowsSecurityTokenAuthenticator(bool includeWindowsGroups)
        {
            this.includeWindowsGroups = includeWindowsGroups;
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is WindowsSecurityToken;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            WindowsSecurityToken windowsToken = (WindowsSecurityToken)token;
            WindowsClaimSet claimSet = new WindowsClaimSet(windowsToken.WindowsIdentity, windowsToken.AuthenticationType, this.includeWindowsGroups, windowsToken.ValidTo);
            return SecurityUtils.CreateAuthorizationPolicies(claimSet, windowsToken.ValidTo);
        }
    }
}
