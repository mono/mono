//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System.Xml;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    public class SecurityContextSecurityTokenResolver : SecurityTokenResolver, ISecurityContextSecurityTokenCache
    {
        SecurityContextTokenCache tokenCache;
        bool removeOldestTokensOnCacheFull;
        int capacity;
        TimeSpan clockSkew = SecurityProtocolFactory.defaultMaxClockSkew;

        public SecurityContextSecurityTokenResolver( int securityContextCacheCapacity, bool removeOldestTokensOnCacheFull )
            : this( securityContextCacheCapacity, removeOldestTokensOnCacheFull, SecurityProtocolFactory.defaultMaxClockSkew )
        {
        }

        public SecurityContextSecurityTokenResolver(int securityContextCacheCapacity, bool removeOldestTokensOnCacheFull, TimeSpan clockSkew)
        {
            if (securityContextCacheCapacity <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("securityContextCacheCapacity", SR.GetString(SR.ValueMustBeGreaterThanZero)));
            }

            if ( clockSkew < TimeSpan.Zero )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentOutOfRangeException( "clockSkew", SR.GetString( SR.TimeSpanCannotBeLessThanTimeSpanZero ) ) );
            }

            this.capacity = securityContextCacheCapacity;
            this.removeOldestTokensOnCacheFull = removeOldestTokensOnCacheFull;
            this.clockSkew = clockSkew;
            this.tokenCache = new SecurityContextTokenCache(this.capacity, this.removeOldestTokensOnCacheFull, clockSkew);
        }

        public int SecurityContextTokenCacheCapacity
        {
            get
            {
                return this.capacity;
            }
        }

        public TimeSpan ClockSkew
        {
            get
            {
                return this.clockSkew;
            }
        }

        public bool RemoveOldestTokensOnCacheFull
        {
            get
            {
                return this.removeOldestTokensOnCacheFull;
            }
        }

        public void AddContext(SecurityContextSecurityToken token)
        {
            this.tokenCache.AddContext(token);
        }
        
        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            return this.tokenCache.TryAddContext(token);
        }


        public void ClearContexts()
        {
            this.tokenCache.ClearContexts();
        }

        public void RemoveContext(UniqueId contextId, UniqueId generation)
        {
            this.tokenCache.RemoveContext(contextId, generation, false);
        }

        public void RemoveAllContexts(UniqueId contextId)
        {
            this.tokenCache.RemoveAllContexts(contextId);
        }

        public SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation)
        {
            return this.tokenCache.GetContext(contextId, generation);
        }

        public Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId)
        {
            return this.tokenCache.GetAllContexts(contextId);
        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken context, DateTime expirationTime)
        {
            this.tokenCache.UpdateContextCachingTime(context, expirationTime);
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause sctSkiClause = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            if (sctSkiClause != null)
            {
                token = this.tokenCache.GetContext(sctSkiClause.ContextId, sctSkiClause.Generation);
            }
            else
            {
                token = null;
            }
            return (token != null);
        }

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

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause sctSkiClause;
            if (keyIdentifier.TryFind<SecurityContextKeyIdentifierClause>(out sctSkiClause))
            {
                return TryResolveToken(sctSkiClause, out token);
            }
            else
            {
                token = null;
                return false;
            }
        }
    }
}
