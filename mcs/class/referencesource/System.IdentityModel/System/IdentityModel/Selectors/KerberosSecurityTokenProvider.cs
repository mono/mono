//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;

    public class KerberosSecurityTokenProvider : SecurityTokenProvider
    {
        string servicePrincipalName;
        TokenImpersonationLevel tokenImpersonationLevel;
        NetworkCredential networkCredential;

        public KerberosSecurityTokenProvider(string servicePrincipalName)
            : this(servicePrincipalName, TokenImpersonationLevel.Identification)
        {
        }

        public KerberosSecurityTokenProvider(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel)
            : this(servicePrincipalName, tokenImpersonationLevel, null)
        {
        }

        public KerberosSecurityTokenProvider(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential)
        {
            if (servicePrincipalName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("servicePrincipalName");
            if (tokenImpersonationLevel != TokenImpersonationLevel.Identification && tokenImpersonationLevel != TokenImpersonationLevel.Impersonation)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenImpersonationLevel",
                    SR.GetString(SR.ImpersonationLevelNotSupported, tokenImpersonationLevel)));
            }

            this.servicePrincipalName = servicePrincipalName;
            this.tokenImpersonationLevel = tokenImpersonationLevel;
            this.networkCredential = networkCredential;
        }

        public string ServicePrincipalName
        {
            get { return this.servicePrincipalName; }
        }

        public TokenImpersonationLevel TokenImpersonationLevel
        {
            get { return this.tokenImpersonationLevel; }
        }

        public NetworkCredential NetworkCredential
        {
            get { return this.networkCredential; }
        }

        internal SecurityToken GetToken(TimeSpan timeout, ChannelBinding channelbinding)
        {
            return new KerberosRequestorSecurityToken(this.ServicePrincipalName,
                this.TokenImpersonationLevel, this.NetworkCredential,
                SecurityUniqueId.Create().Value, channelbinding);
        }
        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.GetToken(timeout, null);
        }

    }
}
