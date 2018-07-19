//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IdentityModel.Tokens;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// This indicates an error has occured while processing a signature
    /// </summary>
    [Serializable]
    public class SignatureVerificationFailedException : SecurityTokenException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="SignatureVerificationFailedException"/>
        /// </summary>
        public SignatureVerificationFailedException()
            : base(SR.GetString(SR.ID4038))
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SignatureVerificationFailedException"/>
        /// </summary>
        public SignatureVerificationFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SignatureVerificationFailedException"/>
        /// </summary>
        public SignatureVerificationFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="SignatureVerificationFailedException"/>
        /// </summary>
        protected SignatureVerificationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
