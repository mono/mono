//-----------------------------------------------------------------------
// <copyright file="RequestSecurityToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Configuration;

    /// <summary>
    /// The class defines the wst:RequestSecurityToken element which 
    /// is used to request a security token.
    /// </summary>
    public class RequestSecurityToken : WSTrustMessage
    {
        AdditionalContext _additionalContext;
        RequestClaimCollection _claims;
        string _computedKeyAlgorithm;
        Renewing _renewing;
        SecurityTokenElement _renewTarget;
        SecurityTokenElement _proofEncryption;
        RequestSecurityToken _secondaryParameters;
        SecurityTokenElement _onBehalfOf;
        EndpointReference _onBehalfOfIssuer;
        SecurityTokenElement _actAs;
        SecurityTokenElement _delegateTo;
        bool? _forwardable;
        bool? _delegatable;
        SecurityTokenElement _cancelTarget;
        SecurityTokenElement _validateTarget;
        Participants _participants;
        SecurityTokenElement _encryption;

        /// <summary>
        /// This constructor is usually used on the receiving end.
        /// </summary>
        public RequestSecurityToken()
            : this(null, null)
        {
        }

        /// <summary>
        /// This constructor is usually used on the sending side to instantiate a
        /// instance of RST based on the request type and its string value.
        /// </summary>
        public RequestSecurityToken(string requestType)
            : this(requestType, null)
        {
        }

        /// <summary>
        /// This constructor is usually used on the sending side to instantiate a
        /// instance of RST based on the request type and its string value.
        /// </summary>
        public RequestSecurityToken(string requestType, string keyType)
            : base()
        {
            RequestType = requestType;

            if (keyType == KeyTypes.Symmetric)
            {
                Entropy = new Entropy(SecurityTokenServiceConfiguration.DefaultKeySizeInBitsConstant);
                KeySizeInBits = SecurityTokenServiceConfiguration.DefaultKeySizeInBitsConstant;
            }
            else if (keyType == KeyTypes.Bearer)
            {
                KeySizeInBits = 0;
            }
            else if (keyType == KeyTypes.Asymmetric)
            {
                KeySizeInBits = 1024;
            }

            KeyType = keyType;
        }

        /// <summary>
        /// The optional element requests a specific set of claim types requested by the client.
        /// </summary>
        public RequestClaimCollection Claims
        {
            get
            {
                if (_claims == null)
                {
                    _claims = new RequestClaimCollection();
                }

                return _claims;
            }
        }

        /// <summary>
        /// The optional element provides that provides information on the token/key to use when encrypting
        /// </summary>
        public SecurityTokenElement Encryption
        {
            get
            {
                return _encryption;
            }

            set
            {
                _encryption = value;
            }
        }

        /// <summary>
        /// This optional URI element indicates the desired algorithm to use when computed
        /// key are used for issued tokens.
        /// </summary>
        /// <remarks>
        /// This is an extension to the RequestSecurityToken element.
        /// </remarks>
        /// <devdocs>
        ///  It is defined in the section 11.2 in the WS-Trust spec.
        /// </devdocs>
        public string ComputedKeyAlgorithm
        {
            get
            {
                return _computedKeyAlgorithm;
            }

            set
            {
                _computedKeyAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or Sets a boolean that specifies if the returned token should
        /// be delegatable.
        /// </summary>
        public bool? Delegatable
        {
            get
            {
                return _delegatable;
            }

            set
            {
                _delegatable = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Identity to which the Issued Token is delegated to.
        /// </summary>
        public SecurityTokenElement DelegateTo
        {
            get
            {
                return _delegateTo;
            }

            set
            {
                _delegateTo = value;
            }
        }

        /// <summary>
        /// Gets or Sets a boolean that specifies if the Issued Token should
        /// be marked forwardable.
        /// </summary>
        public bool? Forwardable
        {
            get
            {
                return _forwardable;
            }

            set
            {
                _forwardable = value;
            }
        }

        /// <summary>
        /// This optional element indicates that the requestor is making the request 
        /// on behalf of another.
        /// </summary>
        public SecurityTokenElement OnBehalfOf
        {
            get
            {
                return _onBehalfOf;
            }

            set
            {
                _onBehalfOf = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Participants who are authorized to use
        /// the issued token.
        /// </summary>
        public Participants Participants
        {
            get
            {
                return _participants;
            }

            set
            {
                _participants = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Issuer of the OnBehalfOf token.
        /// </summary>
        public EndpointReference Issuer
        {
            get
            {
                return _onBehalfOfIssuer;
            }

            set
            {
                _onBehalfOfIssuer = value;
            }
        }

        /// <summary>
        /// This is a optional element that provides additional information
        /// for authorization decision.
        /// </summary>
        public AdditionalContext AdditionalContext
        {
            get
            {
                return _additionalContext;
            }

            set
            {
                _additionalContext = value;
            }
        }

        /// <summary>
        /// This optional element indicates that the requestor is making the request 
        /// on to act as another.
        /// </summary>
        public SecurityTokenElement ActAs
        {
            get
            {
                return _actAs;
            }

            set
            {
                _actAs = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Token that is requested to be cancelled.
        /// </summary>
        public SecurityTokenElement CancelTarget
        {
            get
            {
                return _cancelTarget;
            }

            set
            {
                _cancelTarget = value;
            }
        }

        /// <summary>
        /// Gets or sets the SecurityToken to be used to encrypt the proof token.
        /// </summary>
        public SecurityTokenElement ProofEncryption
        {
            get
            {
                return _proofEncryption;
            }

            set
            {
                _proofEncryption = value;
            }
        }

        /// <summary>
        /// Gets or sets the Renewing element inside the RequestSecurityToken message.
        /// </summary>
        public Renewing Renewing
        {
            get
            {
                return _renewing;
            }

            set
            {
                _renewing = value;
            }
        }

        /// <summary>
        /// Gets or sets the RenewTarget element inside the RequestSecurityToken message.
        /// </summary>
        public SecurityTokenElement RenewTarget
        {
            get
            {
                return _renewTarget;
            }

            set
            {
                _renewTarget = value;
            }
        }

        /// <summary>
        /// Gets or sets the SecondaryParameters inside the RequestSecurityToken message.
        /// This represents the information for which the requestor is not the orginator. The STS MAY choose to use values found here.
        /// </summary>
        public RequestSecurityToken SecondaryParameters
        {
            get
            {
                return _secondaryParameters;
            }

            set
            {
                _secondaryParameters = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Security Token to be Validated.
        /// </summary>
        public SecurityTokenElement ValidateTarget
        {
            get
            {
                return _validateTarget;
            }

            set
            {
                _validateTarget = value;
            }
        }
    }
}
