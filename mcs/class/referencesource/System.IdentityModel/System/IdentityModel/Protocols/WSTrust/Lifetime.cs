//-----------------------------------------------------------------------
// <copyright file="Lifetime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Used in the RequestSecurityToken or RequestSecurityTokenResponse to indicated the desired or 
    /// required lifetime of a token. Everything here is stored in Utc format.
    /// </summary>
    public class Lifetime
    {
        DateTime? _created;
        DateTime? _expires;

        /// <summary>
        /// Instantiates a LifeTime object with token creation and expiration time in Utc.
        /// </summary>
        /// <param name="created">Token creation time in Utc.</param>
        /// <param name="expires">Token expiration time in Utc.</param>
        /// <exception cref="ArgumentException">When the given expiration time is 
        /// before the given creation time.</exception>
        public Lifetime( DateTime created, DateTime expires )
            : this( (DateTime?)created, (DateTime?)expires )
        {
        }

        /// <summary>
        /// Instantiates a LifeTime object with token creation and expiration time in Utc.
        /// </summary>
        /// <param name="created">Token creation time in Utc.</param>
        /// <param name="expires">Token expiration time in Utc.</param>
        /// <exception cref="ArgumentException">When the given expiration time is 
        /// before the given creation time.</exception>
        public Lifetime( DateTime? created, DateTime? expires )
        {
            if ( created != null && expires != null && expires.Value <= created.Value )
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentException( SR.GetString( SR.ID2000 ) ) );

            _created = DateTimeUtil.ToUniversalTime( created );
            _expires = DateTimeUtil.ToUniversalTime( expires );
        }

        /// <summary>
        /// Gets the token creation time in UTC time.
        /// </summary>
        public DateTime? Created
        {
            get 
            { 
                return _created; 
            }
            set
            {
                _created = value;
            }
        }

        /// <summary>
        /// Gets the token expiration time in UTC time.
        /// </summary>
        public DateTime? Expires
        {
            get 
            { 
                return _expires; 
            }
            set
            {
                _expires = value;
            }
        }
    }
}
