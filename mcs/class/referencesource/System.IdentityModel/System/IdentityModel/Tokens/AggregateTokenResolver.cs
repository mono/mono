//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;


using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class defines a TokenResolver that can wrap multiple Token Resolvers 
    /// and resolve tokens across all the wrapped token resolvers.
    /// </summary>
    public class AggregateTokenResolver : SecurityTokenResolver
    {
        List<SecurityTokenResolver> _tokenResolvers = new List<SecurityTokenResolver>();

        /// <summary>
        /// Initializes an instance of <see cref="AggregateTokenResolver"/>
        /// </summary>
        /// <param name="tokenResolvers">IEnumerable list of TokenResolvers to be wrapped.</param>
        /// <exception cref="ArgumentNullException">The input argument 'tokenResolvers' is null.</exception>
        /// <exception cref="ArgumentException">The input 'tokenResolver' list does not contain a valid
        /// SecurityTokenResolver. At least one SecurityTokenResolver should be specified.</exception>
        public AggregateTokenResolver( IEnumerable<SecurityTokenResolver> tokenResolvers )
        {
            if ( tokenResolvers == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "tokenResolvers" );
            }
            
            AddNonEmptyResolvers( tokenResolvers );
        }

        /// <summary>
        /// Gets a read-only collection of TokenResolvers.
        /// </summary>
        public ReadOnlyCollection<SecurityTokenResolver> TokenResolvers
        {
            get
            {
                return _tokenResolvers.AsReadOnly();
            }
        }

        /// <summary>
        /// Override of the base class. Resolves the given SecurityKeyIdentifierClause to a 
        /// SecurityKey.
        /// </summary>
        /// <param name="keyIdentifierClause">The Clause to be resolved.</param>
        /// <param name="key">The resolved SecurityKey</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">Input argument 'keyIdentifierClause' is null.</exception>
        protected override bool TryResolveSecurityKeyCore( SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key )
        {
            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            key = null;
            foreach ( SecurityTokenResolver tokenResolver in _tokenResolvers )
            {
                if ( tokenResolver.TryResolveSecurityKey( keyIdentifierClause, out key ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Override of the base class. Resolves the given SecurityKeyIdentifier to a 
        /// SecurityToken.
        /// </summary>
        /// <param name="keyIdentifier">The KeyIdentifier to be resolved.</param>
        /// <param name="token">The resolved SecurityToken</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">Input argument 'keyIdentifier' is null.</exception>
        protected override bool TryResolveTokenCore( SecurityKeyIdentifier keyIdentifier, out SecurityToken token )
        {
            if ( keyIdentifier == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifer" );
            }

            token = null;
            foreach ( SecurityTokenResolver tokenResolver in _tokenResolvers )
            {
                if ( tokenResolver.TryResolveToken( keyIdentifier, out token ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Override of the base class. Resolves the given SecurityKeyIdentifierClause to a 
        /// SecurityToken.
        /// </summary>
        /// <param name="keyIdentifierClause">The KeyIdentifier to be resolved.</param>
        /// <param name="token">The resolved SecurityToken</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">Input argument 'keyIdentifierClause' is null.</exception>
        protected override bool TryResolveTokenCore( SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token )
        {
            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            token = null;
            foreach ( SecurityTokenResolver tokenResolver in _tokenResolvers )
            {
                if ( tokenResolver.TryResolveToken( keyIdentifierClause, out token ) )
                {
                    return true;
                }
            }

            return false;
        }

        private void AddNonEmptyResolvers( IEnumerable<SecurityTokenResolver> resolvers )
        {
            foreach ( SecurityTokenResolver resolver in resolvers )
            {
                if ( resolver != null && resolver != EmptySecurityTokenResolver.Instance )
                {
                    _tokenResolvers.Add( resolver );
                }
            }
        }
    }
}
