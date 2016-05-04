//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Claims;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Security.Principal;
    using System.Net;
   
    public class SspiSecurityToken : SecurityToken
    {
        string id;
        TokenImpersonationLevel impersonationLevel;
        bool allowNtlm;
        NetworkCredential networkCredential;
        bool extractGroupsForWindowsAccounts;
        bool allowUnauthenticatedCallers = SspiSecurityTokenProvider.DefaultAllowUnauthenticatedCallers;
        DateTime effectiveTime;
        DateTime expirationTime;

        public SspiSecurityToken(TokenImpersonationLevel impersonationLevel, bool allowNtlm, NetworkCredential networkCredential)
        {
            this.impersonationLevel = impersonationLevel;
            this.allowNtlm = allowNtlm;
            this.networkCredential = SecurityUtils.GetNetworkCredentialsCopy(networkCredential);
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = this.effectiveTime.AddHours(10);
        }

        public SspiSecurityToken(NetworkCredential networkCredential, bool extractGroupsForWindowsAccounts, bool allowUnauthenticatedCallers)
        {
            this.networkCredential = SecurityUtils.GetNetworkCredentialsCopy(networkCredential);
            this.extractGroupsForWindowsAccounts = extractGroupsForWindowsAccounts;
            this.allowUnauthenticatedCallers = allowUnauthenticatedCallers;
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = this.effectiveTime.AddHours(10);
        }

        public override string Id
        {
            get
            {
                if (this.id == null)
                    this.id = SecurityUniqueId.Create().Value;
                return this.id; 
            }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.expirationTime; }
        }

        public bool AllowUnauthenticatedCallers
        {
            get
            {
                return this.allowUnauthenticatedCallers;
            }
        }

        public TokenImpersonationLevel ImpersonationLevel
        {
            get
            {
                return this.impersonationLevel;
            }
        }

        public bool AllowNtlm
        {
            get
            {
                return this.allowNtlm;
            }
        }

        public NetworkCredential NetworkCredential
        {
            get
            {
                return this.networkCredential;
            }
        }

        public bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get 
            {
                return EmptyReadOnlyCollection<SecurityKey>.Instance; 
            }
        }
    }
}
