//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

using System.Security.Claims;


namespace System.ServiceModel.Security
{
    internal class SecurityTokenAuthenticatorAdapter : SecurityTokenAuthenticator
    {
        SecurityTokenHandler _securityTokenHandler;
        ExceptionMapper _exceptionMapper;

        public SecurityTokenAuthenticatorAdapter(SecurityTokenHandler securityTokenHandler, ExceptionMapper exceptionMapper)
        {
            if (securityTokenHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenHandler");
            }

            if (exceptionMapper == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exceptionMapper");
            }

            _securityTokenHandler = securityTokenHandler;
            _exceptionMapper = exceptionMapper;
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            return ((token.GetType() == _securityTokenHandler.TokenType) && (_securityTokenHandler.CanValidateToken));
        }

        protected sealed override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            IEnumerable<ClaimsIdentity> subjectCollection = null;

            try
            {
                subjectCollection = _securityTokenHandler.ValidateToken(token);
            }
            catch (Exception ex)
            {
                if (!_exceptionMapper.HandleSecurityTokenProcessingException(ex))
                {
                    throw;
                }
            }

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new AuthorizationPolicy(subjectCollection));
            return policies.AsReadOnly();
        }
    }
}
