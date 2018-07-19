//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Throw this exception when a received Security Token has expiration time in the past.
    /// </summary>
    [Serializable]
    public class SecurityTokenExpiredException : SecurityTokenValidationException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenExpiredException"/>
        /// </summary>
        public SecurityTokenExpiredException()
            : base(SR.GetString(SR.ID4181))
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenExpiredException"/>
        /// </summary>
        public SecurityTokenExpiredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenExpiredException"/>
        /// </summary>
        public SecurityTokenExpiredException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenExpiredException"/>
        /// </summary>
        protected SecurityTokenExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
