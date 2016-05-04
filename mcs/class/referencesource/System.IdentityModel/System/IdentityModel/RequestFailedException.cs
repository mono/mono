//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Throw this exception if the specified request failed due to an external reason that cannot be specifically determined.
    /// </summary>
    [Serializable]
    public class RequestFailedException : RequestException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RequestFailedException()
            : base(SR.GetString(SR.ID2008))
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public RequestFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public RequestFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor that sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected RequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
