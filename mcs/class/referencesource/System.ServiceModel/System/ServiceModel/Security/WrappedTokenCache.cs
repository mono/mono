//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;

using SystemUniqueId = System.Xml.UniqueId;
using SR = System.ServiceModel.SR;

namespace System.ServiceModel.Security
{

    /// <summary>
    /// The purpose of this class is to provide an ISecurityContextSecurityTokenCache contract over a SecurityTokenCache.  
    /// This allows for a consistent interface for the SecurityContextSecurityTokenHandler and a SessionSecurityTokenHandler.
    /// The SecurityTokenCache can be passed to the SecurityContextSecurityTokenHandler and wrapped to expose an ISecurityContextSecurityTokenCache
    /// that can be set to the be the token cache for WCF context tokens
    /// </summary>
    class WrappedTokenCache : SecurityTokenResolver, ISecurityContextSecurityTokenCache
    {
        SessionSecurityTokenCache _tokenCache;
        SctClaimsHandler _claimsHandler;

        public WrappedTokenCache(SessionSecurityTokenCache tokenCache, SctClaimsHandler sctClaimsHandler)
        {
            if (tokenCache == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenCache");
            }

            if (sctClaimsHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sctClaimsHandler");
            }

            _tokenCache = tokenCache;
            _claimsHandler = sctClaimsHandler;
        }

        #region ISecurityContextSecurityTokenCache Members

        public void AddContext(SecurityContextSecurityToken token)
        {
            //
            // WCF will cache the token first before calling the WrappedSessionSecurityTokenHandler.OnTokenIssued.
            // We need to map the claims here so we will be caching the correct token with Geneva Claims substitued
            // in place of the WCF claims.
            //
            _claimsHandler.SetPrincipalBootstrapTokensAndBindIdfxAuthPolicy(token);

            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(_claimsHandler.EndpointId, token.ContextId, token.KeyGeneration);
            SessionSecurityToken sessionToken = SecurityContextSecurityTokenHelper.ConvertSctToSessionToken(token, SecureConversationVersion.Default);
            DateTime expiryTime = DateTimeUtil.Add(sessionToken.ValidTo, _claimsHandler.SecurityTokenHandlerCollection.Configuration.MaxClockSkew);
            _tokenCache.AddOrUpdate(key, sessionToken, expiryTime);
        }

        public void ClearContexts()
        {
            _tokenCache.RemoveAll(_claimsHandler.EndpointId);
        }

        /// <summary>
        /// Called to retrieve all tokens that match a particular contextId. WCF will call this
        /// </summary>
        /// <param name="contextId"></param>
        /// <returns></returns>
        public Collection<SecurityContextSecurityToken> GetAllContexts(System.Xml.UniqueId contextId)
        {
            Collection<SecurityContextSecurityToken> tokens = new Collection<SecurityContextSecurityToken>();

            IEnumerable<SessionSecurityToken> cachedTokens = _tokenCache.GetAll(_claimsHandler.EndpointId, contextId);
            if (cachedTokens != null)
            {
                foreach (SessionSecurityToken sessionSct in cachedTokens)
                {
                    if (sessionSct != null && sessionSct.IsSecurityContextSecurityTokenWrapper)
                    {
                        SecurityContextSecurityToken sctToken = SecurityContextSecurityTokenHelper.ConvertSessionTokenToSecurityContextSecurityToken(sessionSct);
                        tokens.Add(sctToken);
                    }
                }
            }

            return tokens;
        }

        public SecurityContextSecurityToken GetContext(System.Xml.UniqueId contextId, System.Xml.UniqueId generation)
        {
            SessionSecurityToken token = null;
            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(_claimsHandler.EndpointId, contextId, generation);
            token = _tokenCache.Get(key);

            SecurityContextSecurityToken sctToken = null;

            if (token != null && token.IsSecurityContextSecurityTokenWrapper)
            {
                sctToken = SecurityContextSecurityTokenHelper.ConvertSessionTokenToSecurityContextSecurityToken(token);
            }

            return sctToken;
        }

        /// <summary>
        /// Removes all the tokens that match the contextId.
        /// </summary>
        /// <param name="contextId">The context id.</param>
        /// <remarks>
        /// When WCF renews a token, its context id is the same as the issuedToken. The only
        /// difference is in the generationId. When WCF closes the session channel, all the tokens that 
        /// were issued need to be removed that match the contextId.
        /// </remarks>
        public void RemoveAllContexts(System.Xml.UniqueId contextId)
        {
            _tokenCache.RemoveAll(_claimsHandler.EndpointId, contextId);
        }

        public void RemoveContext(System.Xml.UniqueId contextId, System.Xml.UniqueId generation)
        {
            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(_claimsHandler.EndpointId, contextId, generation);
            _tokenCache.Remove(key);
        }

        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            //
            // WCF will cache the token first before calling the WrappedSessionSecurityTokenHandler.OnTokenIssued.
            // We need to map the claims here so we will be caching the correct token with Geneva Claims substitued
            // in place of the WCF claims.
            //
            _claimsHandler.SetPrincipalBootstrapTokensAndBindIdfxAuthPolicy(token);

            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(_claimsHandler.EndpointId, token.ContextId, token.KeyGeneration);
            SessionSecurityToken sessionToken = SecurityContextSecurityTokenHelper.ConvertSctToSessionToken(token, SecureConversationVersion.Default);
            DateTime expiryTime = DateTimeUtil.Add(token.ValidTo, _claimsHandler.SecurityTokenHandlerCollection.Configuration.MaxClockSkew);
            _tokenCache.AddOrUpdate(key, sessionToken, expiryTime);
            return true;
        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken token, DateTime expirationTime)
        {
            if (token.ValidTo <= expirationTime.ToUniversalTime())
            {
                return;
            }

            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(_claimsHandler.EndpointId, token.ContextId, token.KeyGeneration);
            SessionSecurityToken sessionToken = SecurityContextSecurityTokenHelper.ConvertSctToSessionToken(token, SecureConversationVersion.Default);
            DateTime expiryTime = DateTimeUtil.Add(sessionToken.ValidTo, _claimsHandler.SecurityTokenHandlerCollection.Configuration.MaxClockSkew);
            if (_tokenCache.Get(key) == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4285, sessionToken.ContextId.ToString()));
            }
            _tokenCache.AddOrUpdate(key, sessionToken, expiryTime);
        }

        #endregion

        // these are not needed as this will never be used as an SecurityTokenResolver.
        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            SecurityToken sct;
            if (TryResolveTokenCore(keyIdentifierClause, out sct))
            {
                key = ((SecurityContextSecurityToken)sct).SecurityKeys[0];
                return true;
            }
            else
            {
                key = null;
                return false;
            }
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause sctSkiClause = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            if (sctSkiClause != null)
            {
                token = GetContext(sctSkiClause.ContextId, sctSkiClause.Generation) as SecurityToken;
            }
            else
            {
                token = null;
            }
            return (token != null);
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause sctSkiClause;
            if (keyIdentifier.TryFind<SecurityContextKeyIdentifierClause>(out sctSkiClause))
            {
                return TryResolveTokenCore(sctSkiClause, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }
    }
}
