//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Diagnostics.Application;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime.Diagnostics;

    public abstract class SecurityTokenAuthenticator
    {
        protected SecurityTokenAuthenticator() { }

        public bool CanValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return this.CanValidateTokenCore(token);
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (!CanValidateToken(token))
            {
                // warning 56506: Parameter 'token' to this public method must be validated:  A null-dereference can occur here.
#pragma warning suppress 56506
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.CannotValidateSecurityTokenType, this, token.GetType())));
            }

            EventTraceActivity eventTraceActivity = null;
            string tokenType = null;

            if (TD.TokenValidationStartedIsEnabled())
            {
                eventTraceActivity = eventTraceActivity ?? EventTraceActivity.GetFromThreadOrCreate();
                tokenType = tokenType ?? token.GetType().ToString();
                TD.TokenValidationStarted(eventTraceActivity, tokenType, token.Id);
            }

            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = ValidateTokenCore(token);
            if (authorizationPolicies == null)
            {
                string errorMsg = SR.GetString(SR.CannotValidateSecurityTokenType, this, token.GetType());
                if (TD.TokenValidationFailureIsEnabled())
                {
                    eventTraceActivity = eventTraceActivity ?? EventTraceActivity.GetFromThreadOrCreate();
                    tokenType = tokenType ?? token.GetType().ToString();
                    TD.TokenValidationFailure(eventTraceActivity, tokenType, token.Id, errorMsg);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(errorMsg));
            }

            if (TD.TokenValidationSuccessIsEnabled())
            {
                eventTraceActivity = eventTraceActivity ?? EventTraceActivity.GetFromThreadOrCreate();
                tokenType = tokenType ?? token.GetType().ToString();
                TD.TokenValidationSuccess(eventTraceActivity, tokenType, token.Id);
            }

            return authorizationPolicies;
        }

        protected abstract bool CanValidateTokenCore(SecurityToken token);
        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token);
    }
}
