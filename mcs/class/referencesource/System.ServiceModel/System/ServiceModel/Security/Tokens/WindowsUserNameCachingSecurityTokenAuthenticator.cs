//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Text;

    public interface ILogonTokenCacheManager
    {
        bool RemoveCachedLogonToken(string username);
        void FlushLogonTokenCache();
    }

    class LogonTokenCache : TimeBoundedCache
    {
        const int lowWaterMarkFactor = 75;
        const int saltSize = 4;

        TimeSpan cachedLogonTokenLifetime;
        RNGCryptoServiceProvider random;

        public LogonTokenCache(int maxCachedLogonTokens, TimeSpan cachedLogonTokenLifetime)
            : base((maxCachedLogonTokens * lowWaterMarkFactor) / 100, maxCachedLogonTokens, StringComparer.OrdinalIgnoreCase, PurgingMode.TimerBasedPurge, TimeSpan.FromTicks(cachedLogonTokenLifetime.Ticks >> 2), true)
        {
            this.cachedLogonTokenLifetime = cachedLogonTokenLifetime;
            this.random = new RNGCryptoServiceProvider();
        }

        public bool TryGetTokenCache(string userName, out LogonToken token)
        {
            token = (LogonToken)GetItem(userName);
            return token != null;
        }

        public bool TryAddTokenCache(string userName, string password, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            byte[] salt = new byte[saltSize];
            this.random.GetBytes(salt);
            LogonToken token = new LogonToken(userName, password, salt, authorizationPolicies);
            DateTime expirationTime = DateTime.UtcNow.Add(this.cachedLogonTokenLifetime);
            return TryAddItem(userName, token, expirationTime, true);
        }

        // Remove those about to expire
        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            List<IExpirableItem> items = new List<IExpirableItem>(cacheTable.Count);
            foreach (IExpirableItem value in cacheTable.Values)
            {
                items.Add(value);
            }
            // Those expired soon in front
            items.Sort(ExpirableItemComparer.Default);
            int pruningAmount = (items.Count * (100 - lowWaterMarkFactor)) / 100;
            // edge case
            pruningAmount = pruningAmount <= 0 ? items.Count : pruningAmount;
            ArrayList keys = new ArrayList(pruningAmount);
            for (int i = 0; i < pruningAmount; ++i)
            {
                LogonToken token = (LogonToken)ExtractItem(items[i]);
                keys.Add(token.UserName);
                OnRemove(token);
            }
            return keys;
        }

        public bool TryRemoveTokenCache(string userName)
        {
            return this.TryRemoveItem(userName);
        }

        public void Flush()
        {
            this.ClearItems();
        }

        protected override void OnRemove(object item)
        {
            ((LogonToken)item).Dispose();
            base.OnRemove(item);
        }
    }

    class LogonToken : IDisposable
    {
        string userName;
        byte[] passwordHash;
        byte[] salt;
        ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;

        public LogonToken(string userName, string password, byte[] salt, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            this.userName = userName;
            this.passwordHash = ComputeHash(password, salt);
            this.salt = salt;
            this.authorizationPolicies = System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(authorizationPolicies);
        }

        public bool PasswordEquals(string password)
        {
            byte[] passwordHash = ComputeHash(password, this.salt);
            return CryptoHelper.IsEqual(this.passwordHash, passwordHash);
        }

        public string UserName
        {
            get { return this.userName; }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies()
        {
            return System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
        }

        public void Dispose()
        {
            System.IdentityModel.SecurityUtils.DisposeAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
        }

        static byte[] ComputeHash(string password, byte[] salt)
        {
            if (String.IsNullOrEmpty(password))
            {
                return salt;
            }
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            int saltSize = salt.Length;
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] ^= salt[i % saltSize];
            }
            using (HashAlgorithm hashAlgorithm = CryptoHelper.NewSha1HashAlgorithm())
            {
                return hashAlgorithm.ComputeHash(bytes);
            }
        }
    }

    class WindowsUserNameCachingSecurityTokenAuthenticator : WindowsUserNameSecurityTokenAuthenticator, ILogonTokenCacheManager, IDisposable
    {
        LogonTokenCache logonTokenCache;

        public WindowsUserNameCachingSecurityTokenAuthenticator(bool includeWindowsGroups, int maxCachedLogonTokens, TimeSpan cachedLogonTokenLifetime)
            : base(includeWindowsGroups)
        {
            this.logonTokenCache = new LogonTokenCache(maxCachedLogonTokens, cachedLogonTokenLifetime);
        }

        public void Dispose()
        {
            FlushLogonTokenCache();
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            LogonToken token;
            if (this.logonTokenCache.TryGetTokenCache(userName, out token))
            {
                if (token.PasswordEquals(password))
                {
                    return token.GetAuthorizationPolicies();
                }
                else
                {
                    // this prevents logon with old password.
                    this.logonTokenCache.TryRemoveTokenCache(userName);
                }
            }

            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = base.ValidateUserNamePasswordCore(userName, password);
            this.logonTokenCache.TryAddTokenCache(userName, password, authorizationPolicies);
            return authorizationPolicies;
        }

        public bool RemoveCachedLogonToken(string username)
        {
            if (this.logonTokenCache == null)
                return false;

            return this.logonTokenCache.TryRemoveTokenCache(username);
        }

        public void FlushLogonTokenCache()
        {
            if (this.logonTokenCache != null)
                this.logonTokenCache.Flush();
        }
    }
}
