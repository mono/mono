//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Policy;
    using System.Xml;

    public class SecurityContextSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        public SecurityContextSecurityTokenAuthenticator()
            : base()
        { }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SecurityContextSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            SecurityContextSecurityToken sct = (SecurityContextSecurityToken)token;
            if (!IsTimeValid(sct))
            {
                this.ThrowExpiredContextFaultException(sct.ContextId, sct);
            }

            return sct.AuthorizationPolicies;
        }

        void ThrowExpiredContextFaultException(UniqueId contextId, SecurityContextSecurityToken sct)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityContextTokenValidationException(SR.GetString(SR.SecurityContextExpired, contextId, sct.KeyGeneration == null ? "none" : sct.KeyGeneration.ToString())));
        }

        bool IsTimeValid(SecurityContextSecurityToken sct)
        {
            DateTime utcNow = DateTime.UtcNow;
            return (sct.ValidFrom <= utcNow && sct.ValidTo >= utcNow && sct.KeyEffectiveTime <= utcNow);
        }
   }
}
