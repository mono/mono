//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Throw this exception when a received Security Token has been replayed.
    /// </summary>
    [Serializable]
    public class SecurityTokenReplayDetectedException : SecurityTokenValidationException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenReplayDetectedException"/>
        /// </summary>
        public SecurityTokenReplayDetectedException()
            : base(SR.GetString(SR.ID1070))
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenReplayDetectedException"/>
        /// </summary>
        public SecurityTokenReplayDetectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenReplayDetectedException"/>
        /// </summary>
        public SecurityTokenReplayDetectedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SecurityTokenReplayDetectedException"/>
        /// </summary>
        protected SecurityTokenReplayDetectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
