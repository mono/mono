//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.ServiceModel;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.ServiceModel.Security.Tokens;
using System.IdentityModel;

namespace System.ServiceModel.Security
{
    /// <summary>
    /// Creates a security token provider that produces a security token as an issued token
    /// for federated bindings.
    /// </summary>
    public class SimpleSecurityTokenProvider : SecurityTokenProvider
    {
        SecurityToken _securityToken;

        /// <summary>
        /// Creates a security token provider that produces a security token as an issued token
        /// for federated bindings.
        /// </summary>
        /// <param name="token">The security token to provide.</param>
        /// <param name="tokenRequirement">
        /// The requirements described by the binding that will use <paramref name="token"/> to secure
        /// messages.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is set to null.</exception>
        public SimpleSecurityTokenProvider(SecurityToken token, SecurityTokenRequirement tokenRequirement)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            GenericXmlSecurityToken xmlIssuedToken = token as GenericXmlSecurityToken;
            if (xmlIssuedToken != null)
            {
                _securityToken = WrapWithAuthPolicy(xmlIssuedToken, tokenRequirement);
            }
            else
            {
                _securityToken = token;
            }
        }

        /// <summary>
        /// Creates a security token according to a gieven timeout.
        /// </summary>
        /// <param name="timeout">The <see cref="TimeSpan"/>.</param>
        /// <returns></returns>
        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return _securityToken;
        }

        /// <summary>
        /// Gets a GenericXmlSecurityToken that wraps the provided issued token
        /// with the authorization policies necessary.
        /// </summary>
        static GenericXmlSecurityToken WrapWithAuthPolicy(GenericXmlSecurityToken issuedToken,
                                                           SecurityTokenRequirement tokenRequirement)
        {
            EndpointIdentity endpointIdentity = null;

            var issuedTokenRequirement = tokenRequirement as InitiatorServiceModelSecurityTokenRequirement;
            if (issuedTokenRequirement != null)
            {
                EndpointAddress targetAddress = issuedTokenRequirement.TargetAddress;
                if (targetAddress.Uri.IsAbsoluteUri)
                {
                    endpointIdentity = EndpointIdentity.CreateDnsIdentity(targetAddress.Uri.DnsSafeHost);
                }
            }

            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies
                = GetServiceAuthorizationPolicies(endpointIdentity);

            return new GenericXmlSecurityToken(issuedToken.TokenXml,
                                                issuedToken.ProofToken,
                                                issuedToken.ValidFrom,
                                                issuedToken.ValidTo,
                                                issuedToken.InternalTokenReference,
                                                issuedToken.ExternalTokenReference,
                                                authorizationPolicies);
        }

        //
        // Modeled after WCF's CoreFederatedTokenProvider.GetServiceAuthorizationPolicies
        //
        static ReadOnlyCollection<IAuthorizationPolicy> GetServiceAuthorizationPolicies(EndpointIdentity endpointIdentity)
        {
            if (endpointIdentity != null)
            {
                List<Claim> claims = new List<Claim>(1);
                claims.Add(endpointIdentity.IdentityClaim);
                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
                policies.Add(new UnconditionalPolicy(SecurityUtils.CreateIdentity(endpointIdentity.IdentityClaim.Resource.ToString()),
                               new DefaultClaimSet(ClaimSet.System, claims)));
                return policies.AsReadOnly();
            }
            else
            {
                return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
        }
    }
}
