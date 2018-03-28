//-----------------------------------------------------------------------
// <copyright file="LimitExceededException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class defines the exception thrown when a configured limit or quota is exceeded.
    /// </summary>
    [Serializable]
    public class LimitExceededException : SystemException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LimitExceededException"/>.
        /// </summary>
        public LimitExceededException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LimitExceededException"/>.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public LimitExceededException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LimitExceededException"/>.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public LimitExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LimitExceededException"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/>. that contains contextual information about the source or destination.</param>
        protected LimitExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
