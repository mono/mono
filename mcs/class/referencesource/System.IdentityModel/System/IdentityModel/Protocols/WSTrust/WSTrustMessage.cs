//-----------------------------------------------------------------------
// <copyright file="WSTrustMessage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Base class for RST and RSTR.
    /// </summary>
    public abstract class WSTrustMessage : OpenObject
    {
        private bool allowPostdating;
        private EndpointReference appliesTo;
        private string replyTo;
        private string authenticationType;
        private string canonicalizationAlgorithm;
        private string context;
        private string encryptionAlgorithm;
        private Entropy entropy;
        private string issuedTokenEncryptionAlgorithm;
        private string keyWrapAlgorithm;
        private string issuedTokenSignatureAlgorithm;
        private int? keySizeInBits;
        private string keyType;
        private Lifetime lifetime;
        private string requestType;
        private string signatureAlgorithm;
        private string tokenType;
        private UseKey useKey;
        private BinaryExchange binaryExchange;

        /// <summary>
        /// Default constructor for extensibility.
        /// </summary>
        protected WSTrustMessage()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the returned tokens should allow requsts for postdated
        /// tokens.
        /// </summary>
        /// <remarks>
        /// This property is usually used in the token renewal scenario.
        /// </remarks>
        /// <devdocs>
        /// Please refer to section 7 in the WS-Trust spec for more details.
        /// </devdocs>
        public bool AllowPostdating
        {
            get
            {
                return this.allowPostdating;
            }
            
            set
            {
                this.allowPostdating = value;
            }
        }

        /// <summary>
        /// Gets or sets this optional element that specifies the endpoint address for which this security token is desired.
        /// For example, the service to which this token applies.
        /// </summary>
        /// <remarks>
        /// Either TokenType or AppliesTo SHOULD be defined in the token request message. If both 
        /// are specified, the AppliesTo field takes precedence.
        /// </remarks>
        public EndpointReference AppliesTo
        {
            get
            {
                return this.appliesTo;
            }
            
            set
            {
                this.appliesTo = value;
            }
        }

        /// <summary>
        /// Gets or sets the binary data that is exchanged.
        /// </summary>
        public BinaryExchange BinaryExchange
        {
            get
            {
                return this.binaryExchange;
            }
            
            set
            {
                this.binaryExchange = value;
            }
        }

        /// <summary>
        /// Gets or sets the address to be used for replying to the Relying Party.
        /// </summary>
        /// <remarks>
        /// This is a local extension for WS-Fed Passive scenario.
        /// </remarks>
        public string ReplyTo
        {
            get
            {
                return this.replyTo;
            }
           
            set
            {
                this.replyTo = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional element indicates the type of authencation desired,
        /// specified as a URI.
        /// </summary>
        public string AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
            
            set
            {
                this.authenticationType = value;
            }
        }

        /// <summary>
        /// Gets or sets the CanonicalizationAlgorithm.
        /// </summary>
        public string CanonicalizationAlgorithm
        {
            get
            {
                return this.canonicalizationAlgorithm;
            }
            
            set
            {
                this.canonicalizationAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional context element specifies an identifier/context for this request.
        /// </summary>
        /// <remarks>
        /// All subsequent RSTR elements relating to this request MUST carry this attribute.
        /// </remarks>
        public string Context
        {
            get
            {
                return this.context;
            }
            
            set
            {
                this.context = value;
            }
        }

        /// <summary>
        /// Gets or sets the EncryptionAlgorithm.
        /// </summary>
        public string EncryptionAlgorithm
        {
            get
            {
                return this.encryptionAlgorithm;
            }
            
            set
            {
                this.encryptionAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets this optional element that allows a requestor to specify entropy that is to 
        /// be used in creating the key.
        /// </summary>
        /// <remarks>
        /// This is commonly used in a token issuance request message. The value of this element
        /// SHOULD be either an EncryptedKey or BinarySecret depending on whether or not the key 
        /// is encrypted. 
        /// </remarks>
        /// <devdocs>
        /// It is defined in the section 6.1 in the WS-Trust spec.
        /// </devdocs>
        public Entropy Entropy
        {
            get
            {
                return this.entropy;
            }
            
            set
            {
                this.entropy = value;
            }
        }

        /// <summary>
        /// Gets or sets this optional URI element that indicates the desired encryption algorithm to be used
        /// with the issued security token.
        /// </summary>
        /// <remarks>
        /// This is an extension to the RequestSecurityToken element. 
        /// </remarks>
        /// <deodocs>
        /// It is defined in the section 11.2 in the WS-Trust spec.
        /// </deodocs>
        /// <exception cref="ArgumentNullException">When the given value is null or an empty string.</exception>
        public string EncryptWith
        {
            get
            {
                return this.issuedTokenEncryptionAlgorithm;
            }
            
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("IssuedTokenEncryptionAlgorithm");
                }

                this.issuedTokenEncryptionAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets this optional URI element that indicates the desired signature algorithm to be used
        /// with the issued security token.
        /// </summary>
        /// <remarks>
        /// This is an extension to the RequestSecurityToken element. 
        /// </remarks>
        /// <deodocs>
        /// It is defined in the section 11.2 in the WS-Trust spec.
        /// </deodocs>
        /// <exception cref="ArgumentNullException">When the given IssuedTokenSignatureAlgorithm algorithm value is null or string.empty.</exception>
        public string SignWith
        {
            get
            {
                return this.issuedTokenSignatureAlgorithm;
            }
            
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.issuedTokenSignatureAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets this element that defines the KeySize element inside the RequestSecurityToken message
        /// It is specified in bits.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When the given KeySizeInBits value is less than or equal to zero.</exception>
        public int? KeySizeInBits
        {
            get
            {
                return this.keySizeInBits;
            }
            
            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.keySizeInBits = value;
            }
        }

        /// <summary>
        /// Gets or sets the KeyTypeOption element inside the RequestSecurityToken message.
        /// </summary>
        /// <remarks>
        /// This optional URI element indicates the type of key desired in the security
        /// token. 
        /// </remarks>
        /// <devdocs>
        /// Please refer to the section 11.2 in the WS-Trust spec for further details.
        /// </devdocs>
        public string KeyType
        {
            get
            {
                return this.keyType;
            }
            
            set
            {
                this.keyType = value;
            }
        }

        /// <summary>
        /// Gets or sets optional URI indicates the desired algorithm to use for key 
        /// wrapping when STS encrypts the issued token for the relying party 
        /// using an asymmetric key. 
        /// </summary>
        public string KeyWrapAlgorithm
        {
            get
            {
                return this.keyWrapAlgorithm;
            }
            
            set
            {
                this.keyWrapAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the Lifetime element inside the RequestSecurityToken message.
        /// </summary>
        public Lifetime Lifetime
        {
            get
            {
                return this.lifetime;
            }
            
            set
            {
                this.lifetime = value;
            }
        }

        /// <summary>
        /// Gets or sets the required element that indicates the request type.
        /// </summary>
        public string RequestType
        {
            get
            {
                return this.requestType;
            }
            
            set
            {
                this.requestType = value;
            }
        }

        /// <summary>
        /// Gets or sets the SignatureAlgorithm.
        /// </summary>
        public string SignatureAlgorithm
        {
            get
            {
                return this.signatureAlgorithm;
            }
            
            set
            {
                this.signatureAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the desired token type.
        /// </summary>
        public string TokenType
        {
            get
            {
                return this.tokenType;
            }
           
            set
            {
                this.tokenType = value;
            }
        }

        /// <summary>
        /// Gets or sets the element, which tf the requestor wishes to use an existing key rather than create a new one, 
        /// then this property can be used to reference a security token containing 
        /// the desired key. 
        /// </summary>
        /// <remarks>
        /// This is commonly used in the asymetric key issurance case. On the wire, it will be 
        /// serialized out as a subelement in UseKey element.
        /// </remarks>
        public UseKey UseKey
        {
            get
            {
                return this.useKey;
            }
            
            set
            {
                this.useKey = value;
            }
        }
    }
}
