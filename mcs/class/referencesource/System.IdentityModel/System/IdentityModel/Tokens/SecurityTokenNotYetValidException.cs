//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Throw this exception when a received Security token has an effective time 
    /// in the future.
    /// </summary>
    [Serializable]
    public class SecurityTokenNotYetValidException : SecurityTokenValidationException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenNotYetValidException"/>
        /// </summary>
        public SecurityTokenNotYetValidException()
            : base(SR.GetString(SR.ID4182))
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenNotYetValidException"/>
        /// </summary>
        public SecurityTokenNotYetValidException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenNotYetValidException"/>
        /// </summary>
        public SecurityTokenNotYetValidException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenNotYetValidException"/>
        /// </summary>
        protected SecurityTokenNotYetValidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
