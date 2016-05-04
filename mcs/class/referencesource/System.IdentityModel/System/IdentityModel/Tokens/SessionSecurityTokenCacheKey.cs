//-----------------------------------------------------------------------
// <copyright file="SessionSecurityTokenCacheKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Text;
    using SystemUniqueId = System.Xml.UniqueId;

    /// <summary>
    /// When caching an <see cref="SessionSecurityToken"/> there are two indexes required. One is the ContextId
    /// that is unique across all <see cref="SessionSecurityToken"/> and the next is KeyGeneration which is 
    /// unique within a session. When an <see cref="SessionSecurityToken"/> is issued it has only a ContextId. When
    /// the <see cref="SessionSecurityToken"/> is renewed the KeyGeneration is added as an second index to the
    /// <see cref="SessionSecurityToken"/>. Now the renewed <see cref="SessionSecurityToken"/> is uniquely identifiable via the ContextId and 
    /// KeyGeneration. 
    /// The class <see cref="SessionSecurityTokenCacheKey"/> is used as the index
    /// to the <see cref="SessionSecurityToken"/> cache. This index will always have a valid ContextId specified 
    /// but the KeyGeneration may be null. There is also an optional EndpointId
    /// which gives the endpoint to which the token is scoped.
    /// </summary>
    public class SessionSecurityTokenCacheKey
    {
        private SystemUniqueId contextId;
        private SystemUniqueId keyGeneration;
        private string endpointId;
        private bool ignoreKeyGeneration;
        
        /// <summary>
        /// Creates an instance of <see cref="SessionSecurityTokenCacheKey"/> which
        /// is used as an index while caching <see cref="SessionSecurityToken"/>.
        /// </summary>
        /// <param name="endpointId">The endpoint Id to which the <see cref="SessionSecurityToken"/> is scoped.</param>
        /// <param name="contextId">UniqueId of the <see cref="SessionSecurityToken"/>.</param>
        /// <param name="keyGeneration">UniqueId which is available when the <see cref="SessionSecurityToken"/> is renewed. Will be
        /// null when caching a new <see cref="SessionSecurityToken"/>.</param>
        public SessionSecurityTokenCacheKey(string endpointId, System.Xml.UniqueId contextId, System.Xml.UniqueId keyGeneration)
        {
            if (endpointId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointId");
            }

            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            
            this.endpointId = endpointId;
            this.contextId = contextId;
            this.keyGeneration = keyGeneration;
        }

        /// <summary>
        /// Gets or sets a value indicating whether KeyGeneration can be ignored
        /// while doing index comparison.
        /// </summary>
        public bool IgnoreKeyGeneration
        {
            get
            {
                return this.ignoreKeyGeneration;
            }
            
            set
            {
                this.ignoreKeyGeneration = value;
            }
        }

        /// <summary>
        /// Gets the ContextId of the <see cref="SessionSecurityToken"/>
        /// </summary>
        public System.Xml.UniqueId ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        /// <summary>
        /// Gets the EndpointId to which this cache entry is scoped.
        /// </summary>
        public string EndpointId
        {
            get
            {
                return this.endpointId;
            }
        }

        /// <summary>
        /// Gets the KeyGeneration of the <see cref="SessionSecurityToken"/>
        /// </summary>
        public System.Xml.UniqueId KeyGeneration
        {
            get
            {
                return this.keyGeneration;
            }
        }

        /// <summary>
        /// Implements the equality operator for <see cref="SessionSecurityTokenCacheKey"/>.
        /// </summary>
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <returns>'true' if both objects are equal.</returns>
        public static bool operator ==(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second)
        {
            if (object.ReferenceEquals(first, null))
            {
                return object.ReferenceEquals(second, null);
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Implements the inequality operator for <see cref="SessionSecurityTokenCacheKey"/>.
        /// </summary>
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <returns>'true' if both the objects are different.</returns>
        public static bool operator !=(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second)
        {
            return !(first == second);
        }
        
        /// <summary>
        /// Checks if the given object is the same as the current object.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns>'true' if both are the same object else false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is SessionSecurityTokenCacheKey)
            {
                SessionSecurityTokenCacheKey key2 = obj as SessionSecurityTokenCacheKey;
                if (key2.ContextId != this.contextId)
                {
                    return false;
                }

                if (!StringComparer.Ordinal.Equals(key2.EndpointId, this.endpointId))
                {
                    return false;
                }
                
                // If KeyGeneration can be ignored on either one of them then we
                // don't do KeyGeneration comparison.
                if (!this.ignoreKeyGeneration && !key2.IgnoreKeyGeneration)
                {
                    return key2.KeyGeneration == this.keyGeneration;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a Hash code for this object.
        /// </summary>
        /// <returns>Hash code for the object as a Integer.</returns>
        public override int GetHashCode()
        {
            if (this.keyGeneration == null)
            {
                return this.contextId.GetHashCode();
            }
            else
            {
                return this.contextId.GetHashCode() ^ this.keyGeneration.GetHashCode();
            }
        }

        /// <summary>
        /// Implements ToString() to provide a unique identifier.
        /// </summary>
        /// <returns>This key, in string form.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.endpointId);
            sb.Append(';');
            sb.Append(this.contextId.ToString());
            sb.Append(';');
            if (!this.ignoreKeyGeneration && this.keyGeneration != null)
            {
                sb.Append(this.keyGeneration.ToString());
            }

            return sb.ToString();
        }       
    }
}
