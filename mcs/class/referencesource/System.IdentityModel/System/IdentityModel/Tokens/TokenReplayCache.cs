//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.IdentityModel.Configuration;
using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class defines the API for a cache that stores tokens for and purges them 
    /// on a schedule time interval.
    /// </summary>
    public abstract class TokenReplayCache : ICustomIdentityConfiguration
    {
        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="nodelist">Custom configuration elements</param>
        public virtual void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID0023, this.GetType().AssemblyQualifiedName)));
        }

        /// <summary>
        /// Attempt to add a new entry or update an existing entry.
        /// </summary>
        /// <param name="key">Key to use when adding item</param>
        /// <param name="securityToken">SecurityToken to add to cache, can be null</param>
        /// <param name="expirationTime">The expiration time of the entry.</param>
        public abstract void AddOrUpdate(string key, SecurityToken securityToken, DateTime expirationTime);

        /// <summary>
        /// Attempt to find if a matching entry exists in the cache.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>true if a matching entry is ifound in the cache, false otherwise</returns>
        public abstract bool Contains(string key);

        /// <summary>
        /// Attempt to get a SecurityToken
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The <see cref="SecurityToken"/> found, if any, null otherwise.</returns>
        public abstract SecurityToken Get(string key);

        /// <summary>
        /// Attempt to remove an entry from the cache
        /// </summary>
        /// <param name="key">The key to the entry to remove</param>
        public abstract void Remove(string key);
    }
}

