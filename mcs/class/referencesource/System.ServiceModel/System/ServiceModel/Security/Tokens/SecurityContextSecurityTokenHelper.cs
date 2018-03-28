//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Runtime;

namespace System.ServiceModel.Security.Tokens
{
    static class SecurityContextSecurityTokenHelper
    {
        static public SessionSecurityToken ConvertSctToSessionToken(SecurityContextSecurityToken sct)
        {
            return ConvertSctToSessionToken(sct, SecureConversationVersion.Default);
        }

        static public SessionSecurityToken ConvertSctToSessionToken(SecurityContextSecurityToken sct, SecureConversationVersion version)
        {
            string endpointId = String.Empty;

            for (int i = 0; i < sct.AuthorizationPolicies.Count; ++i)
            {
                EndpointAuthorizationPolicy epAuthPolicy = sct.AuthorizationPolicies[i] as EndpointAuthorizationPolicy;
                if (epAuthPolicy != null)
                {
                    endpointId = epAuthPolicy.EndpointId;
                    break;
                }
            }

            SctAuthorizationPolicy sctAuthPolicy = null;
            for (int i = 0; i < sct.AuthorizationPolicies.Count; i++)
            {
                IAuthorizationPolicy authPolicy = sct.AuthorizationPolicies[i];

                // The WCF SCT will have a SctAuthorizationPolicy that wraps the Primary Identity
                // of the bootstrap token. This is required for SCT renewal scenarios. Write the 
                // SctAuthorizationPolicy if one is available.
                sctAuthPolicy = authPolicy as SctAuthorizationPolicy;
                if (sctAuthPolicy != null)
                {
                    break;
                }
            }

            ClaimsPrincipal claimsPrincipal = null;
            // these can be empty in transport security
            if (sct.AuthorizationPolicies != null && sct.AuthorizationPolicies.Count > 0)
            {
                AuthorizationPolicy ap = null;
                for (int i = 0; i < sct.AuthorizationPolicies.Count; ++i)
                {
                    ap = sct.AuthorizationPolicies[i] as AuthorizationPolicy;
                    if (ap != null)
                    {
                        // We should have exactly one IAuthorizationPolicy of type AuthorizationPolicy.
                        break;
                    }
                }
                if (ap != null)
                {
                    if (ap.IdentityCollection != null)
                    {
                        claimsPrincipal = new ClaimsPrincipal(ap.IdentityCollection);
                    }
                }
            }

            if (claimsPrincipal == null)
            {
                // When _securityContextTokenWrapper is true, this implies WCF.
                // Authpolicies not found occurs when the SCT represents a bootstrap nego that is used obtain a key
                // for the outer or actual SCT {unfortunate but true and we haven't found a way to distinguish this otherwise}.
                // So return an empty ClaimsPrincipal so that when written on wire in cookie mode we DO NOT write an empty identity.
                // If we did, then when the actual bootstrap token, such as a SAML token arrives, we will add the bootstrap AND the SAML identities to the ClaimsPrincipal
                // and end up with multiple, one of them anonymous.
                // 
                claimsPrincipal = new ClaimsPrincipal();
            }

            return new SessionSecurityToken(claimsPrincipal, sct.ContextId, sct.Id, String.Empty, sct.GetKeyBytes(), endpointId, sct.ValidFrom, sct.ValidTo, sct.KeyGeneration, sct.KeyEffectiveTime, sct.KeyExpirationTime, sctAuthPolicy, new Uri(version.Namespace.Value));
        }

        static public SecurityContextSecurityToken ConvertSessionTokenToSecurityContextSecurityToken(SessionSecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            if (token.SctAuthorizationPolicy != null)
            {
                policies.Add(token.SctAuthorizationPolicy);
            }

            if (token.ClaimsPrincipal != null && token.ClaimsPrincipal.Identities != null)
            {
                policies.Add(new AuthorizationPolicy(token.ClaimsPrincipal.Identities));
            }

            byte[] key = null;
            SymmetricSecurityKey symmetricKey = token.SecurityKeys[0] as SymmetricSecurityKey;
            if (symmetricKey != null)
            {
                key = symmetricKey.GetSymmetricKey();
            }

            SecurityContextSecurityToken sct = new SecurityContextSecurityToken(
                token.ContextId,
                 token.Id,
                 key, 
                 token.ValidFrom, 
                 token.ValidTo, 
                 token.KeyGeneration, 
                 token.KeyEffectiveTime, 
                 token.KeyExpirationTime,
                 policies.AsReadOnly());

            return sct;
        }
    }
}
