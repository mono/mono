//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;

    public class KerberosSecurityTokenAuthenticator : WindowsSecurityTokenAuthenticator
    {
        public KerberosSecurityTokenAuthenticator()
        {
        }

        public KerberosSecurityTokenAuthenticator(bool includeWindowsGroups)
            : base(includeWindowsGroups)
        {
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is KerberosReceiverSecurityToken;
        }
    }
}
