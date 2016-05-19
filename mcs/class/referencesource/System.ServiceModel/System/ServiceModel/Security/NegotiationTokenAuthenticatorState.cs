//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Security.Tokens;

    class NegotiationTokenAuthenticatorState : IDisposable
    {
        bool isNegotiationCompleted;
        SecurityContextSecurityToken serviceToken;
        Object thisLock;

        public NegotiationTokenAuthenticatorState() 
        {
            thisLock = new Object();
        }

        public Object ThisLock
        {
            get
            {
                return thisLock; 
            }
        }

        public bool IsNegotiationCompleted 
        {
            get
            {
                return this.isNegotiationCompleted;
            }
        }

        public SecurityContextSecurityToken ServiceToken 
        {
            get
            {
                CheckCompleted();
                return this.serviceToken;
            }
        }

        public virtual void Dispose() { }

        public void SetServiceToken(SecurityContextSecurityToken token) 
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.serviceToken = token;
            this.isNegotiationCompleted = true;
        }

        public virtual string GetRemoteIdentityName()
        {
            if (this.isNegotiationCompleted)
            {
                return SecurityUtils.GetIdentityNamesFromPolicies(this.serviceToken.AuthorizationPolicies);
            }
            return String.Empty;
        }

        void CheckCompleted()
        {
            if (!this.isNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NegotiationIsNotCompleted)));
            }
        }
    }
}
