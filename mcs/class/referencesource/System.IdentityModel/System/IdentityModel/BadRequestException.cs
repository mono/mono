//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The specified RequestSecurityToken is not understood.
    /// </summary>
    [Serializable]
    public class BadRequestException : RequestException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BadRequestException()
            : base(SR.GetString(SR.ID2009))
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public BadRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor that sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/>. that contains contextual information about the source or destination.</param>
        protected BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
