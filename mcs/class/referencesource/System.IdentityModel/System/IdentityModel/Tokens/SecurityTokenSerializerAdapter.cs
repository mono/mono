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
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class derives from System.IdentityModel.Selectors.SecurityTokenSerializer and wraps a collection of SecurityTokenHandlers. 
    /// Any call to this serilaizer is delegated to the token handler and delegated to the base class if no token handler
    /// is registered to handle this particular token or KeyIdentifier.
    /// </summary>
    class SecurityTokenSerializerAdapter : SecurityTokenSerializer
    {
        SecurityTokenHandlerCollection _securityTokenHandlers;

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenSerializerAdapter"/>
        /// </summary>
        /// <param name="securityTokenHandlerCollection">
        /// The <see cref="SecurityTokenHandlerCollection" /> containing the set of <see cref="SecurityTokenHandler" />
        /// </param>
        public SecurityTokenSerializerAdapter(SecurityTokenHandlerCollection securityTokenHandlerCollection)
        {
            if (securityTokenHandlerCollection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenHandlerCollection");
            }
            _securityTokenHandlers = securityTokenHandlerCollection;

            KeyInfoSerializer serializer = securityTokenHandlerCollection.KeyInfoSerializer as KeyInfoSerializer;
            if (serializer != null)
            {
                serializer.InnerSecurityTokenSerializer = this;
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
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can read the security token.
        /// </summary>
        /// <param name="reader">Reader to a Security token.</param>
        /// <returns>'True' if the serializer can read the given Security Token.</returns>
        protected override bool CanReadTokenCore(XmlReader reader)
        {
            return _securityTokenHandlers.CanReadToken(reader);
        }

        /// <summary>
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can write the given security token.
        /// </summary>
        /// <param name="token">SecurityToken instance.</param>
        /// <returns>'True' if the serializer can write the given security token.</returns>
        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            return _securityTokenHandlers.CanWriteToken(token);
        }

        /// <summary>
        /// Deserializes the SecurityToken from the given XmlReader.
        /// </summary>
        /// <param name="reader">Reader to a Security token.</param>
        /// <param name="tokenResolver">Instance of SecurityTokenResolver.</param>
        /// <returns>'True' if the serializer can read the given Security Token.</returns>
        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            return _securityTokenHandlers.ReadToken(reader);
        }

        /// <summary>
        /// Serializes the SecurityToken to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write to.</param>
        /// <param name="token">The SecurityToken to serializer.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'writer' or 'token' is null.</exception>
        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            _securityTokenHandlers.WriteToken(writer, token);
        }

        /// <summary>
        /// Checks if one of the wrapped SecurityTokenHandlers or the base WSSecurityTokenSerializer
        /// can read the security key identifier.
        /// </summary>
        /// <param name="reader">Reader pointing at a Security Key Identifier {ds:Keyinfo}.</param>
        /// <returns>'True' if the serializer can read the given Security Key Identifier.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            return (reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace));
        }

        /// <summary>
        /// Reads an SecurityKeyIdentifier from a XML stream.
        /// </summary>
        /// <param name="reader">An XML reader positioned at an SecurityKeyIdentifier (ds: KeyInfo) as defined in 'http://www.w3.org/TR/xmldsig-core'.</param>
        /// <returns>SecurityKeyIdentifier.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="reader"/> is not positioned at KeyInfo element.</exception>
        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (reader.IsStartElement(XmlSignatureConstants.Elements.KeyInfo, XmlSignatureConstants.Namespace))
            {
                KeyInfo keyInfo = new KeyInfo(this);
                keyInfo.ReadXml(XmlDictionaryReader.CreateDictionaryReader(reader));
                return keyInfo.KeyIdentifier;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperXml(reader, SR.GetString(SR.ID4192));
            }
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            return _securityTokenHandlers.KeyInfoSerializer == null ? false : _securityTokenHandlers.KeyInfoSerializer.CanWriteKeyIdentifier(keyIdentifier);
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            _securityTokenHandlers.KeyInfoSerializer.WriteKeyIdentifier(writer, keyIdentifier);
        }

        /// <summary>
        /// Checks if the wrapped SecurityTokenHandler can read the 
        /// SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="reader">Reader to a SecurityKeyIdentifierClause.</param>
        /// <returns>'True' if the SecurityKeyIdentifierCause can be read.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'reader' is null.</exception>
        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            foreach (SecurityTokenHandler securityTokenHandler in _securityTokenHandlers)
            {
                if (securityTokenHandler.CanReadKeyIdentifierClause(reader))
                {
                    return true;
                }
            }

            return (_securityTokenHandlers.KeyInfoSerializer == null) ? false : _securityTokenHandlers.KeyInfoSerializer.CanReadKeyIdentifierClause(reader);
        }

        /// <summary>
        /// Checks if the wrapped SecurityTokenHandler or the base WSSecurityTokenSerializer can write the
        /// given SecurityKeyIdentifierClause.
        /// </summary>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to be checked.</param>
        /// <returns>'True' if the SecurityTokenKeyIdentifierClause can be written.</returns>
        /// <exception cref="ArgumentNullException">The input parameter 'keyIdentifierClause' is null.</exception>
        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            foreach (SecurityTokenHandler securityTokenHandler in _securityTokenHandlers)
            {
                if (securityTokenHandler.CanWriteKeyIdentifierClause(keyIdentifierClause))
                {
                    return true;
                }
            }

            return (_securityTokenHandlers.KeyInfoSerializer == null) ? false : _securityTokenHandlers.KeyInfoSerializer.CanWriteKeyIdentifierClause(keyIdentifierClause);
        }

        /// <summary>
        /// Deserializes a SecurityKeyIdentifierClause from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader to a SecurityKeyIdentifierClause.</param>
        /// <returns>The deserialized SecurityKeyIdentifierClause.</returns>
        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            return _securityTokenHandlers.ReadKeyIdentifierClause(reader);
        }

        /// <summary>
        /// Serializes the given SecurityKeyIdentifierClause in a XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to write into.</param>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to be written.</param>
        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            _securityTokenHandlers.WriteKeyIdentifierClause(writer, keyIdentifierClause);
        }
    }
}
