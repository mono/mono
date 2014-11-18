//-----------------------------------------------------------------------
// <copyright file="RequestSecurityTokenResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// The class defines the wst:RequestSecurityTokenResponse element which 
    /// is used to return a security token.
    /// </summary>
    public class RequestSecurityTokenResponse : WSTrustMessage
    {
        SecurityKeyIdentifierClause _requestedAttachedReference;
        RequestedProofToken _requestedProofToken;
        RequestedSecurityToken _requestedSecurityToken;
        SecurityKeyIdentifierClause _requestedUnattachedReference;
        bool _requestedTokenCancelled;
        Status _status;
        bool _isFinal = true;

        /// <summary>
        /// This constructor is usually used on the RSTR receiving end.
        /// </summary>
        public RequestSecurityTokenResponse()
            : base()
        {
        }

        /// <summary>
        /// This constructor is usually used on the RSTR sending side.
        /// </summary>
        /// <remarks>
        /// This constructor will copy some information, such as Context, KeyType, 
        /// KeySize and RequestType from the request message. Note here the RequestType
        /// is not a sub element under RSTR, need it just for token request processing.
        /// </remarks>
        public RequestSecurityTokenResponse(WSTrustMessage message)
            : base()
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            RequestType = message.RequestType;  // note this is NOT a sub element under RSTR
            Context = message.Context;
            KeyType = message.KeyType;

            if (message.KeySizeInBits > 0 && StringComparer.Ordinal.Equals(message.KeyType, KeyTypes.Symmetric))
            {
                KeySizeInBits = message.KeySizeInBits;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines if the RSTR is the final message
        /// and should be serialized as such.
        /// </summary>
        /// <remarks>
        /// This flag is only useful if the version of WS-Trust provides rules for serializing
        /// the final RSTR in a message flow. For instance, WS-Trust 1.3 requires the final RSTR
        /// to be enclosed within a RequestSecurityTokenResponseCollection element.
        /// </remarks>
        public bool IsFinal
        {
            get
            {
                return _isFinal;
            }
            set
            {
                _isFinal = value;
            }
        }

        /// <summary>
        /// Gets or sets the security token reference when the requested token is attached 
        /// to the message.
        /// </summary>
        /// <remarks>
        /// This optional element is specified to indicate how to reference the returned token when 
        /// that token doesn't support references using URI fragments.
        /// </remarks>
        public SecurityKeyIdentifierClause RequestedAttachedReference
        {
            get
            {
                return _requestedAttachedReference;
            }
            set
            {
                _requestedAttachedReference = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional elemnet used to return the requested security token.
        /// </summary>
        public RequestedSecurityToken RequestedSecurityToken
        {
            get
            {
                return _requestedSecurityToken;
            }
            set
            {
                _requestedSecurityToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional elemnet used to return the proof of possession token.
        /// </summary>
        public RequestedProofToken RequestedProofToken
        {
            get
            {
                return _requestedProofToken;
            }
            set
            {
                _requestedProofToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the security token reference when the requested token is not attached 
        /// to the message.
        /// </summary>
        /// <remarks>
        /// This optional element is specified to indicate how to reference the returned token when 
        /// that token is not placed in the message.
        /// </remarks>
        public SecurityKeyIdentifierClause RequestedUnattachedReference
        {
            get
            {
                return _requestedUnattachedReference;
            }
            set
            {
                _requestedUnattachedReference = value;
            }
        }

        /// <summary>
        /// Gets or sets the RequestedTokenCancelled element.
        /// </summary>
        public bool RequestedTokenCancelled
        {
            get
            {
                return _requestedTokenCancelled;
            }
            set
            {
                _requestedTokenCancelled = value;
            }
        }

        /// <summary>
        /// Gets or sets the Status element in the RSTR.
        /// </summary>
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }
    }
}
