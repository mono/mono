//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.IdentityModel
{
    /// <summary>
    /// Throw this exception when the specified RequestSecurityToken is not understood because of the unknown token type.
    /// </summary>
    [Serializable]
    public class UnsupportedTokenTypeBadRequestException : BadRequestException
    {
        const string TokenTypeProperty = "TokenType";

        string _tokenType;

        /// <summary>
        /// Initialize an instance of UnsupportedTokenTypeBadRequestException.
        /// </summary>
        public UnsupportedTokenTypeBadRequestException()
            : base()
        {
            _tokenType = String.Empty;
        }

        /// <summary>
        /// Initialize an instance of UnsupportedTokenTypeBadRequestException with a given token type.
        /// </summary>
        /// <param name="tokenType"></param>
        public UnsupportedTokenTypeBadRequestException(string tokenType)
            : base( SR.GetString( SR.ID2014, tokenType ) )
        {
            _tokenType = tokenType;
        }

        /// <summary>
        /// Constructor with error message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public UnsupportedTokenTypeBadRequestException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Reconstructs the object after deserialization. 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected UnsupportedTokenTypeBadRequestException(SerializationInfo info, StreamingContext context)
            : base( info, context )
        {
            if (info == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("info");
            _tokenType = info.GetValue(TokenTypeProperty, typeof(string)) as string;
        }

        /// <summary>
        /// Serializes the value out.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("info");
            info.AddValue(TokenTypeProperty, TokenType);

            base.GetObjectData(info, context);
        }
        
        /// <summary>
        /// Gets and sets the token type which is not supported.
        /// </summary>
        public string TokenType
        {
            get 
            { 
                return _tokenType; 
            }
            set 
            { 
                _tokenType = value; 
            }
        }
    }
}
