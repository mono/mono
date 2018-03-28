//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Runtime;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace System.ServiceModel.Security
{
    /// <summary>
    /// This class derives from System.ServiceModel.Security.WSSecurityTokenSerializer and wraps a collection of SecurityTokenHandlers. 
    /// Any call to this serilaizer is delegated to the token handler and delegated to the base class if no token handler
    /// is registered to handle this particular token or KeyIdentifier.
    /// </summary>
    class WsSecurityTokenSerializerAdapter : WSSecurityTokenSerializer
    {
        SecureConversationVersion _scVersion;
        SecurityTokenHandlerCollection _securityTokenHandlers;
        bool _mapExceptionsToSoapFaults;
        ExceptionMapper _exceptionMapper = new ExceptionMapper();

        /// <summary>
        /// Initializes an instance of <see cref="WsSecurityTokenSerializerAdapter"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollection">
        /// The <see cref="SecurityTokenHandlerCollection" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </param>
        public WsSecurityTokenSerializerAdapter( SecurityTokenHandlerCollection securityTokenHandlerCollection )
            : this( securityTokenHandlerCollection, MessageSecurityVersion.Default.SecurityVersion )
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WsSecurityTokenSerializerAdapter"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollection">
        /// The <see cref="SecurityTokenHandlerCollection" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </param>
        /// <param name="securityVersion">The SecurityTokenVersion of the base WSSecurityTokenSerializer.</param>
        public WsSecurityTokenSerializerAdapter( SecurityTokenHandlerCollection securityTokenHandlerCollection, SecurityVersion securityVersion )
            : this( securityTokenHandlerCollection, securityVersion, true, new SamlSerializer(), null, null )
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WsSecurityTokenSerializerAdapter"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollection">
        /// The <see cref="SecurityTokenHandlerCollection" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </param>
        /// <param name="securityVersion">The SecurityVersion of the base WSSecurityTokenSerializer.</param>
        /// <param name="emitBspAttributes">Flag that determines if the serailization shoudl be BSP compliant.</param>
        /// <param name="samlSerializer">Serializer for SAML 1.1 tokens.</param>
        /// <param name="stateEncoder">SecurityStateEncoder used for resolving SCT.</param>
        /// <param name="knownTypes">The collection of known claim types.</param>
        public WsSecurityTokenSerializerAdapter( SecurityTokenHandlerCollection securityTokenHandlerCollection, SecurityVersion securityVersion, bool emitBspAttributes, SamlSerializer samlSerializer, SecurityStateEncoder stateEncoder, IEnumerable<Type> knownTypes )
            : this( securityTokenHandlerCollection, securityVersion, TrustVersion.WSTrust13, SecureConversationVersion.WSSecureConversation13, emitBspAttributes, samlSerializer, stateEncoder, knownTypes )
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WsSecurityTokenSerializerAdapter"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollection">
        /// The <see cref="SecurityTokenHandlerCollection" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </param>
        /// <param name="securityVersion">The SecurityVersion of the base WSSecurityTokenSerializer.</param>
        /// <param name="trustVersion">The TrustVersion of the serializer uses.</param>
        /// <param name="secureConversationVersion">The SecureConversationVersion of the serializer.</param>
        /// <param name="emitBspAttributes">Flag that determines if the serailization shoudl be BSP compliant.</param>
        /// <param name="samlSerializer">Serializer for SAML 1.1 tokens.</param>
        /// <param name="stateEncoder">SecurityStateEncoder used for resolving SCT.</param>
        /// <param name="knownTypes">The collection of known claim types.</param>
        public WsSecurityTokenSerializerAdapter( SecurityTokenHandlerCollection securityTokenHandlerCollection, SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, bool emitBspAttributes, SamlSerializer samlSerializer, SecurityStateEncoder stateEncoder, IEnumerable<Type> knownTypes )
            : base( securityVersion, trustVersion, secureConversationVersion, emitBspAttributes, samlSerializer, stateEncoder, knownTypes )
        {
            if ( securityTokenHandlerCollection == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "securityTokenHandlerCollection" );
            }

            _scVersion = secureConversationVersion;
            _securityTokenHandlers = securityTokenHandlerCollection;
        }

        /// <summary>
        /// Gets and Sets the property that describes if exceptions
        /// should be mapped to SOAP Fault exceptions. Default is false.
        /// </summary>
        public bool MapExceptionsToSoapFaults
        {
            get
            {
                return _mapExceptionsToSoapFaults;
            }
            set
            {
                _mapExceptionsToSoapFaults = value;
            }
        }

        /// <summary>
        /// Gets the SecurityTokenHandlerCollection.
        /// </summary>
        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            get
            {
                return _securityTokenHandlers;
            }
        }

        /// <summary>
        /// Gets or sets the ExceptionMapper to be used when throwing exceptions.
        /// </summary>
        public ExceptionMapper ExceptionMapper
        {
            get
            {
                return _exceptionMapper;
            }
            set
            {
                if ( value == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
                }
                _exceptionMapper = value;
            }
        }

        /// <summary>
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can read the security token.
        /// </summary>
        /// <param name="reader">Reader to a Security token.</param>
        /// <returns>'True' if the serializer can read the given Security Token.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected override bool CanReadTokenCore( XmlReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            if ( _securityTokenHandlers.CanReadToken( reader ) )
            {
                return true;
            }

            return base.CanReadTokenCore( reader );
        }

        /// <summary>
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can write the given security token.
        /// </summary>
        /// <param name="token">SecurityToken instance.</param>
        /// <returns>'True' if the serializer can write the given security token.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'token' is null.</exception>
        protected override bool CanWriteTokenCore( SecurityToken token )
        {
            if ( token == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "token" );
            }

            if ( _securityTokenHandlers.CanWriteToken( token ) )
            {
                return true;
            }

            return base.CanWriteTokenCore( token );
        }

        /// <summary>
        /// Deserializes the SecurityToken from the given XmlReader.
        /// </summary>
        /// <param name="reader">Reader to a Security token.</param>
        /// <param name="tokenResolver">Instance of SecurityTokenResolver.</param>
        /// <returns>'True' if the serializer can read the given Security Token.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected override SecurityToken ReadTokenCore( XmlReader reader, SecurityTokenResolver tokenResolver )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            try
            {
                foreach ( SecurityTokenHandler securityTokenHandler in _securityTokenHandlers )
                {
                    if ( securityTokenHandler.CanReadToken( reader ) )
                    {
                        SecurityToken token = securityTokenHandler.ReadToken( reader, tokenResolver );
                        SessionSecurityToken sessionToken = token as SessionSecurityToken;

                        if ( sessionToken != null )
                        {
                            if ( sessionToken.SecureConversationVersion.AbsoluteUri != _scVersion.Namespace.Value )
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4053, sessionToken.SecureConversationVersion, _scVersion ) );
                            }

                            return SecurityContextSecurityTokenHelper.ConvertSessionTokenToSecurityContextSecurityToken(sessionToken);
                        }
                        else
                        {
                            return token;
                        }
                    }
                }

                return base.ReadTokenCore( reader, tokenResolver );
            }
            catch ( Exception ex )
            {
                if ( !( MapExceptionsToSoapFaults && _exceptionMapper.HandleSecurityTokenProcessingException( ex ) ) )
                {
                    throw;
                }
                Fx.Assert( false, "ExceptionMapper did not handle an exception correctly." );
                // This should never happen. ExceptionMapper will handle the exception, in which case,
                // a fault exception is thrown or the original exception gets thrown.
            }

            return null;
        }
        
        /// <summary>
        /// Serializes the SecurityToken to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write to.</param>
        /// <param name="token">The SecurityToken to serializer.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'token' is null.</exception>
        protected override void WriteTokenCore( XmlWriter writer, SecurityToken token )
        {
            if ( writer == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "writer" );
            }

            if ( token == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "token" );
            }

            try
            {
                //
                // Wire the session handler for SCT
                //
                SecurityContextSecurityToken sct = token as SecurityContextSecurityToken;
                if ( sct != null )
                {
                    //
                    // Bare SCT tokens are wrapped with a SessionSecurityToken.
                    // The property SessionSecurityToken.IsSecurityContextSecurityTokenWrapper will be true.
                    //
                    token = SecurityContextSecurityTokenHelper.ConvertSctToSessionToken( sct, _scVersion );
                }

                SecurityTokenHandler securityTokenHandler = _securityTokenHandlers[token];

                if ( ( securityTokenHandler != null ) && ( securityTokenHandler.CanWriteToken ) )
                {
                    securityTokenHandler.WriteToken( writer, token );

                    return;
                }

                base.WriteTokenCore( writer, token );
            }
            catch ( Exception ex )
            {
                if ( !( MapExceptionsToSoapFaults && _exceptionMapper.HandleSecurityTokenProcessingException( ex ) ) )
                {
                    throw;
                }
                Fx.Assert( false, "ExceptionMapper did not handle an exception correctly." );
                // This should never happen. ExceptionMapper will handle the exception, in which case,
                // a fault exception is thrown or the original exception gets thrown.
            }
        }

        /// <summary>
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can read the security key identifier.
        /// </summary>
        /// <param name="reader">Reader pointing at a Security Key Identifier {ds:Keyinfo}.</param>
        /// <returns>'True' if the serializer can read the given Security Key Identifier.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        protected override bool CanReadKeyIdentifierCore( XmlReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            if ( reader.IsStartElement( XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace ) )
            {
                return true;
            }
            else
            {
                return base.CanReadKeyIdentifierCore( reader );
            }
        }

        /// <summary>
        /// Reads an SecurityKeyIdentifier from a XML stream.
        /// </summary>
        /// <param name="reader">An XML reader positioned at an SecurityKeyIdentifier (ds: KeyInfo) as defined in 'http://www.w3.org/TR/xmldsig-core'.</param>
        /// <returns>SecurityKeyIdentifier.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="reader"/> is not positioned at KeyInfo element.</exception>
        protected override SecurityKeyIdentifier ReadKeyIdentifierCore( XmlReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            if ( reader.IsStartElement( XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace ) )
            {
                KeyInfo keyInfo = new KeyInfo( this );
                keyInfo.ReadXml( XmlDictionaryReader.CreateDictionaryReader( reader ) );
                return keyInfo.KeyIdentifier;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4192 ) );
            }
        }

        /// <summary>
        /// Checks if the wrapped SecurityTokenHandler or the base WSSecurityTokenSerializer can read the 
        /// SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="reader">Reader to a SecurityKeyIdentifierClause.</param>
        /// <returns>'True' if the SecurityKeyIdentifierCause can be read.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected override bool CanReadKeyIdentifierClauseCore( XmlReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            foreach ( SecurityTokenHandler securityTokenHandler in _securityTokenHandlers )
            {
                if ( securityTokenHandler.CanReadKeyIdentifierClause( reader ) )
                {
                    return true;
                }
            }

            return base.CanReadKeyIdentifierClauseCore( reader );
        }

        /// <summary>
        /// Checks if the wrapped SecurityTokenHandler or the base WSSecurityTokenSerializer can write the
        /// given SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to be checked.</param>
        /// <returns>'True' if the SecurityTokenKeyIdentifierClause can be written.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'keyIdentifierClause' is null.</exception>
        protected override bool CanWriteKeyIdentifierClauseCore( SecurityKeyIdentifierClause keyIdentifierClause )
        {
            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            foreach ( SecurityTokenHandler securityTokenHandler in _securityTokenHandlers )
            {
                if ( securityTokenHandler.CanWriteKeyIdentifierClause( keyIdentifierClause ) )
                {
                    return true;
                }
            }

            return base.CanWriteKeyIdentifierClauseCore( keyIdentifierClause );
        }

        /// <summary>
        /// Deserializes a SecurityKeyIdentifierClause from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader to a SecurityKeyIdentifierClause.</param>
        /// <returns>The deserialized SecurityKeyIdentifierClause.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore( XmlReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            try
            {
                foreach ( SecurityTokenHandler securityTokenHandler in _securityTokenHandlers )
                {
                    if ( securityTokenHandler.CanReadKeyIdentifierClause( reader ) )
                    {
                        return securityTokenHandler.ReadKeyIdentifierClause( reader );
                    }
                }

                return base.ReadKeyIdentifierClauseCore( reader );
            }
            catch ( Exception ex )
            {
                if ( !( MapExceptionsToSoapFaults && _exceptionMapper.HandleSecurityTokenProcessingException( ex ) ) )
                {
                    throw;
                }
                Fx.Assert( false, "ExceptionMapper did not handle an exception correctly." );
                // This should never happen. ExceptionMapper will handle the exception, in which case,
                // a fault exception is thrown or the original exception gets thrown.
            }

            return null;
        }

        /// <summary>
        /// Serializes the given SecurityKeyIdentifierClause in a XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write into.</param>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to be written.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'keyIdentifierClause' is null.</exception>
        protected override void WriteKeyIdentifierClauseCore( XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause )
        {
            if ( writer == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "writer" );
            }

            if ( keyIdentifierClause == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "keyIdentifierClause" );
            }

            try
            {
                foreach ( SecurityTokenHandler securityTokenHandler in _securityTokenHandlers )
                {
                    if ( securityTokenHandler.CanWriteKeyIdentifierClause( keyIdentifierClause ) )
                    {
                        securityTokenHandler.WriteKeyIdentifierClause( writer, keyIdentifierClause );
                        return;
                    }
                }

                base.WriteKeyIdentifierClauseCore( writer, keyIdentifierClause );
            }
            catch ( Exception ex )
            {
                if ( !( MapExceptionsToSoapFaults && _exceptionMapper.HandleSecurityTokenProcessingException( ex ) ) )
                {
                    throw;
                }
                Fx.Assert( false, "ExceptionMapper did not handle an exception correctly." );
                // This should never happen. ExceptionMapper will handle the exception, in which case,
                // a fault exception is thrown or the original exception gets thrown.
            }

        }


    }
}
