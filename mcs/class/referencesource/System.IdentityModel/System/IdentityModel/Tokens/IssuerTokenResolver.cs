//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Resolves issuer tokens received from service partners.
    /// </summary>
    public class IssuerTokenResolver : SecurityTokenResolver
    {
        /// <summary>
        /// Default store for resolving X509 certificates.
        /// </summary>
        public static readonly StoreName DefaultStoreName = StoreName.TrustedPeople;
        /// <summary>
        /// Default store location for resolving X509 certificates.
        /// </summary>
        public static readonly StoreLocation DefaultStoreLocation = StoreLocation.LocalMachine;

        //
        // By default, the wrapped resolver is an X509CertificateStoreResolver using LM.TrustedPeople.
        // This can be overridden by the caller.
        //
        SecurityTokenResolver _wrappedTokenResolver = null;

        internal static IssuerTokenResolver DefaultInstance = new IssuerTokenResolver();

        /// <summary>
        /// Creates an instance of IssuerTokenResolver.
        /// </summary>
        public IssuerTokenResolver()
            : this( new X509CertificateStoreTokenResolver( DefaultStoreName, DefaultStoreLocation ) )
        {
        }

        /// <summary>
        /// Creates an instance of IssuerTokenResolver using a given <see cref="SecurityTokenResolver"/>.
        /// </summary>
        /// <param name="wrappedTokenResolver">The <see cref="SecurityTokenResolver"/> to use.</param>
        public IssuerTokenResolver( SecurityTokenResolver wrappedTokenResolver )
        {
            if ( wrappedTokenResolver == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappedTokenResolver" );
            }

            _wrappedTokenResolver = wrappedTokenResolver;
        }

        /// <summary>
        /// Gets the <see cref="SecurityTokenResolver"/> wrapped by this class.
        /// </summary>
        public SecurityTokenResolver WrappedTokenResolver
        {
            get
            {
                return _wrappedTokenResolver;
            }
        }

        /// <summary>
        /// Inherited from <see cref="SecurityTokenResolver"/>.
        /// </summary>
        protected override bool TryResolveSecurityKeyCore( SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key )
        {
            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            key = null;

            X509RawDataKeyIdentifierClause rawDataClause = keyIdentifierClause as X509RawDataKeyIdentifierClause;
            if ( rawDataClause != null )
            {
                key = rawDataClause.CreateKey();
                return true;
            }

            RsaKeyIdentifierClause rsaClause = keyIdentifierClause as RsaKeyIdentifierClause;
            if ( rsaClause != null )
            {
                key = rsaClause.CreateKey();
                return true;
            }

            if ( _wrappedTokenResolver.TryResolveSecurityKey( keyIdentifierClause, out key ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Inherited from <see cref="SecurityTokenResolver"/>.
        /// </summary>
        protected override bool TryResolveTokenCore( SecurityKeyIdentifier keyIdentifier, out SecurityToken token )
        {
            if ( keyIdentifier == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifier" );
            }

            token = null;
            foreach ( SecurityKeyIdentifierClause clause in keyIdentifier )
            {
                if ( TryResolveTokenCore( clause, out token ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inherited from <see cref="SecurityTokenResolver"/>.
        /// </summary>
        protected override bool TryResolveTokenCore( SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token )
        {
            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            token = null;

            //
            // Try raw X509
            //
            X509RawDataKeyIdentifierClause rawDataClause = keyIdentifierClause as X509RawDataKeyIdentifierClause;
            if ( rawDataClause != null )
            {
                token = new X509SecurityToken( new X509Certificate2( rawDataClause.GetX509RawData() ) );
                return true;
            }

            //
            // Try RSA
            //
            RsaKeyIdentifierClause rsaClause = keyIdentifierClause as RsaKeyIdentifierClause;
            if ( rsaClause != null )
            {
                token = new RsaSecurityToken( rsaClause.Rsa );
                return true;
            }

            if ( _wrappedTokenResolver.TryResolveToken( keyIdentifierClause, out token ) )
            {
                return true;
            }
            
            return false;
        }
    }
}
