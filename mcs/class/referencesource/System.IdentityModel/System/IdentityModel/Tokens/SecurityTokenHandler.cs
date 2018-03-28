//-----------------------------------------------------------------------
// <copyright file="SecurityTokenHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics.Application;
    using System.IdentityModel.Selectors;
    using System.Runtime.Diagnostics;
    using System.Security.Claims;
    using System.Xml;

    /// <summary>
    /// Defines the interface for a Security Token Handler.
    /// </summary>
    public abstract class SecurityTokenHandler : ICustomIdentityConfiguration
    {
        private SecurityTokenHandlerCollection collection;
        private SecurityTokenHandlerConfiguration configuration;
        private EventTraceActivity eventTraceActivity;

        /// <summary>
        /// Creates an instance of <see cref="SecurityTokenHandler"/>
        /// </summary>
        protected SecurityTokenHandler()
        {
        }

        private EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                }

                return this.eventTraceActivity;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this handler supports validation of tokens 
        /// handled by this instance.
        /// </summary>v
        /// <returns>'True' if the instance is capable of SecurityToken
        /// validation.</returns>
        public virtual bool CanValidateToken
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the class provides serialization functionality to serialize token handled 
        /// by this instance.
        /// </summary>
        /// <returns>true if the WriteToken method can serialize this token.</returns>
        public virtual bool CanWriteToken
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenHandlerConfiguration" />
        /// </summary>
        public SecurityTokenHandlerConfiguration Configuration
        {
            get { return this.configuration; }
            set { this.configuration = value; }
        }

        /// <summary>
        /// Gets or sets the SecurityTokenHandlerCollection that this SecurityTokenHandler
        /// is part of. This property should never be set directly. When the SecurityTokenHandler
        /// is added to a collection this property is automatically set.
        /// </summary>
        public SecurityTokenHandlerCollection ContainingCollection
        {
            get 
            { 
                return this.collection;
            }

            internal set 
            { 
                this.collection = value;
            }
        }

        /// <summary>
        /// Gets the System.Type of the SecurityToken this instance handles.
        /// </summary>
        public abstract Type TokenType
        {
            get;
        }

        /// <summary>
        /// Indicates whether the current XML element can be read as a token 
        /// of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at a start 
        /// element. The reader should not be advanced.</param>
        /// <returns>'True' if the ReadToken method can the element.</returns>
        public virtual bool CanReadToken(XmlReader reader)
        {
            return false;
        }

        /// <summary>
        /// Indicates whether the current token string can be read as a token 
        /// of the type handled by this instance.
        /// </summary>
        /// <param name="tokenString">The token string thats needs to be read.</param>
        /// <returns>'True' if the ReadToken method can parse the token string.</returns>
        public virtual bool CanReadToken(string tokenString)
        {
            return false;
        }
        
        /// <summary>
        /// Deserializes from XML a token of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the token's start 
        /// element.</param>
        /// <returns>SecurityToken instance.</returns>
        public virtual SecurityToken ReadToken(XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "ReadToken")));
        }

        /// <summary>
        /// Deserializes from XML a token of the type handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the token's start 
        /// element.</param>
        /// <param name="tokenResolver">The SecrityTokenResolver that contains out-of-band and cached tokens.</param>
        /// <returns>SecurityToken instance.</returns>
        public virtual SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            // The default implementation ignores the SecurityTokenResolver and delegates the call to the 
            // ReadToken method that takes a XmlReader.
            return this.ReadToken(reader);
        }

        /// <summary>
        /// Deserializes from string a token of the type handled by this instance.
        /// </summary>
        /// <param name="tokenString">The string to be deserialized.</param>
        /// <returns>SecurityToken instance which represents the serialized token.</returns>
        public virtual SecurityToken ReadToken(string tokenString)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "ReadToken")));
        }

        /// <summary>
        /// Serializes to XML a token of the type handled by this instance.
        /// </summary>
        /// <param name="writer">The XML writer.</param>
        /// <param name="token">A token of type TokenType.</param>
        public virtual void WriteToken(XmlWriter writer, SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "WriteToken")));
        }

        /// <summary>
        /// Serializes to string a token of the type handled by this instance.
        /// </summary>
        /// <param name="token">A token of type TokenType.</param>
        /// <returns>The serialized token.</returns>
        public virtual string WriteToken(SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "WriteToken")));
        }

        /// <summary>
        /// Indicates if the current XML element is pointing to a KeyIdentifierClause that
        /// can be serialized by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the start element. 
        /// The reader should not be advanced.</param>
        /// <returns>true if the ReadKeyIdentifierClause can read the element.</returns>
        public virtual bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            return false;
        }

        /// <summary>
        /// Deserializes the XML to a KeyIdentifierClause that references a token 
        /// handled by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the KeyIdentifierClause start element.</param>
        /// <returns>SecurityKeyIdentifierClause instance.</returns>
        public virtual SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "ReadKeyIdentifierClause")));
        }

        /// <summary>
        /// Indicates if the given SecurityKeyIdentifierClause can be serialized by this
        /// instance.
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause to be serialized.</param>
        /// <returns>true if the given SecurityKeyIdentifierClause can be serialized.</returns>
        public virtual bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            return false;
        }

        /// <summary>
        /// Serializes to XML a SecurityKeyIdentifierClause that this instance supports.
        /// </summary>
        /// <param name="writer">The XML writer.</param>
        /// <param name="securityKeyIdentifierClause">The SecurityKeyIdentifierClause to be used to serialize the token.</param>
        public virtual void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "WriteKeyIdentifierClause")));
        }

        /// <summary>
        /// Called by the STS to create a token given a token descriptor. 
        /// </summary>
        /// <param name="tokenDescriptor">Describes the token; properties such 
        /// as ValidFrom, AppliesTo, EncryptingCredentials, Claims, etc., are filled in 
        /// before the call to create token. </param>
        /// <returns>A SecurityToken that matches the properties of the token descriptor.</returns>
        public virtual SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "CreateToken")));
        }

        /// <summary>
        /// Creates the security token reference for tokens handled by this instance.
        /// </summary>
        /// <param name="token">The SecurityToken instance for which the references needs to be
        /// created.</param>
        /// <param name="attached">Boolean that indicates if a attached or unattached
        /// reference needs to be created.</param>
        /// <returns>A SecurityKeyIdentifierClause that identifies the given token.</returns>
        public virtual SecurityKeyIdentifierClause CreateSecurityTokenReference(SecurityToken token, bool attached)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "CreateSecurityTokenReference")));
        }

        /// <summary>
        /// The URI used in requests to identify a token of the type handled
        /// by this instance. 
        /// </summary>
        /// <remarks>
        /// For example, this should be the URI value used 
        /// in the RequestSecurityToken's TokenType element to request this
        /// sort of token.
        /// </remarks>
        /// <returns>The set of URIs that identify the token this handler supports.</returns>
        public abstract string[] GetTokenTypeIdentifiers();

        /// <summary>
        /// Validates a <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SecurityToken"/> to validate.</param>
        /// <returns>The <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <remarks>Derived types will validate specific tokens.</remarks>
        public virtual ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4008, "SecurityTokenHandler", "ValidateToken")));
        }

        /// <summary>
        /// Throws if a token is detected as being replayed.
        /// Override this method in your derived class to detect replays.
        /// </summary>
        /// <param name="token">The token to check for replay.</param>
        protected virtual void DetectReplayedToken(SecurityToken token)
        {
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="nodelist">Custom configuration elements</param>
        public virtual void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID0023, this.GetType().AssemblyQualifiedName)));
        }

        protected void TraceTokenValidationSuccess(SecurityToken token)
        {
            if (TD.TokenValidationSuccessIsEnabled())
            {
                TD.TokenValidationSuccess(this.EventTraceActivity, token.GetType().ToString(), token.Id);
            }
        }

        protected void TraceTokenValidationFailure(SecurityToken token, string errorMessage)
        {
            if (TD.TokenValidationFailureIsEnabled())
            {
                TD.TokenValidationFailure(this.EventTraceActivity, token.GetType().ToString(), token.Id, errorMessage);
            }
        }
    }
}
