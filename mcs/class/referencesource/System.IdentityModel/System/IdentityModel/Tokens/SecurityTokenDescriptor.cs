//-----------------------------------------------------------------------
// <copyright file="SecurityTokenDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Protocols.WSTrust;
    using System.Security.Claims;
    using System.Xml;

    using RST = System.IdentityModel.Protocols.WSTrust.RequestSecurityToken;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;

    /// <summary>
    /// This is a place holder for all the attributes related to the issued token.
    /// </summary>
    public class SecurityTokenDescriptor
    {
        private SecurityKeyIdentifierClause attachedReference;
        private AuthenticationInformation authenticationInfo;
        private string tokenIssuerName;
        private ProofDescriptor proofDescriptor;
        private ClaimsIdentity subject;
        private SecurityToken token;
        private string tokenType;
        private SecurityKeyIdentifierClause unattachedReference;
        private Lifetime lifetime;
        private string appliesToAddress;
        private string replyToAddress;
        private EncryptingCredentials encryptingCredentials;
        private SigningCredentials signingCredentials;

        private Dictionary<string, object> properties = new Dictionary<string, object>(); // for any custom data

        /// <summary>
        /// Gets or sets the address for the <see cref="RequestSecurityTokenResponse"/> AppliesTo property.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not an absolute URI.</exception>
        public string AppliesToAddress
        {
            get 
            { 
                return this.appliesToAddress; 
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!UriUtil.CanCreateValidUri(value, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2002)));
                    }
                }

                this.appliesToAddress = value;
            }
        }

        /// <summary>
        /// Sets the appropriate things, such as requested security token, inside the RSTR 
        /// based on what is inside this token descriptor instance.  
        /// </summary>
        /// <param name="response">The RSTR object that this security token descriptor needs to modify.</param>
        /// <exception cref="ArgumentNullException">When response is null.</exception>
        public virtual void ApplyTo(RSTR response)
        {
            if (response == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (tokenType != null)
            {
                response.TokenType = tokenType;
            }

            if (token != null)
            {
                response.RequestedSecurityToken = new RequestedSecurityToken(token);
            }

            if (attachedReference != null)
            {
                response.RequestedAttachedReference = attachedReference;
            }

            if (unattachedReference != null)
            {
                response.RequestedUnattachedReference = unattachedReference;
            }

            if (lifetime != null)
            {
                response.Lifetime = lifetime;
            }

            if (proofDescriptor != null)
            {
                proofDescriptor.ApplyTo(response);
            }
        }

        /// <summary>
        /// Gets or sets the address for the <see cref="RequestSecurityTokenResponse"/> ReplyToAddress property.
        /// </summary>
        public string ReplyToAddress
        {
            get { return this.replyToAddress; }
            set { this.replyToAddress = value; }
        }

        /// <summary>
        /// Gets or sets the credentials used to encrypt the token.
        /// </summary>
        public EncryptingCredentials EncryptingCredentials
        {
            get { return this.encryptingCredentials; }
            set { this.encryptingCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the credentials used to sign the token.
        /// </summary>
        public SigningCredentials SigningCredentials
        {
            get { return this.signingCredentials; }
            set { this.signingCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the SecurityKeyIdentifierClause when the token is attached 
        /// to the message.
        /// </summary>
        public SecurityKeyIdentifierClause AttachedReference
        {
            get { return this.attachedReference; }
            set { this.attachedReference = value; }
        }

        /// <summary>
        /// Gets or sets the issuer name, which may be used inside the issued token as well.
        /// </summary>
        public string TokenIssuerName
        {
            get { return this.tokenIssuerName; }
            set { this.tokenIssuerName = value; }
        }

        /// <summary>
        /// Gets or sets the proof descriptor, which can be used to modify some fields inside
        /// the RSTR, such as requested proof token.
        /// </summary>
        public ProofDescriptor Proof
        {
            get { return this.proofDescriptor; }
            set { this.proofDescriptor = value; }
        }

        /// <summary>
        /// Gets the properties bag to extend the object.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return this.properties; }
        }

        /// <summary>
        /// Gets or sets the issued security token.
        /// </summary>
        public SecurityToken Token
        {
            get { return this.token; }
            set { this.token = value; }
        }

        /// <summary>
        /// Gets or sets the token type of the issued token.
        /// </summary>
        public string TokenType
        {
            get { return this.tokenType; }
            set { this.tokenType = value; }
        }

        /// <summary>
        /// Gets or sets the unattached token reference to refer to the issued token when it is not 
        /// attached to the message.
        /// </summary>
        public SecurityKeyIdentifierClause UnattachedReference
        {
            get { return this.unattachedReference; }
            set { this.unattachedReference = value; }
        }

        /// <summary>
        /// Gets or sets the lifetime information for the issued token.
        /// </summary>
        public Lifetime Lifetime
        {
            get { return this.lifetime; }
            set { this.lifetime = value; }
        }

        /// <summary>
        /// Gets or sets the OutputClaims to be included in the issued token.
        /// </summary>
        public ClaimsIdentity Subject
        {
            get { return this.subject; }
            set { this.subject = value; }
        }

        /// <summary>
        /// Gets or sets the AuthenticationInformation.
        /// </summary>
        public AuthenticationInformation AuthenticationInfo
        {
            get { return this.authenticationInfo; }
            set { this.authenticationInfo = value; }
        }

        /// <summary>
        /// Adds a <see cref="Claim"/> for the authentication type to the claim collection of 
        /// the <see cref="SecurityTokenDescriptor"/>
        /// </summary>
        /// <param name="authType">The authentication type.</param>
        public void AddAuthenticationClaims(string authType)
        {
            this.AddAuthenticationClaims(authType, DateTime.UtcNow);
        }

        /// <summary>
        /// Adds <see cref="Claim"/>s for the authentication type and the authentication instant 
        /// to the claim collection of the <see cref="SecurityTokenDescriptor"/>
        /// </summary>
        /// <param name="authType">Specifies the authentication type</param>
        /// <param name="time">Specifies the authentication instant in UTC. If the input is not in UTC, it is converted to UTC.</param> 
        public void AddAuthenticationClaims(string authType, DateTime time)
        {
            this.Subject.AddClaim(
                new Claim(ClaimTypes.AuthenticationMethod, authType, ClaimValueTypes.String));

            this.Subject.AddClaim(
                new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(time.ToUniversalTime(), DateTimeFormats.Generated), ClaimValueTypes.DateTime));
        }
    }
}
