//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class defines the exception thrown during an asynchrous process.
    /// </summary>
    [Serializable]
    public class AsynchronousOperationException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AsynchronousOperationException()
            : base(SR.GetString(SR.ID4004))
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public AsynchronousOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public AsynchronousOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor with inner exception.
        /// </summary>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public AsynchronousOperationException(Exception innerException)
            : base(SR.GetString(SR.ID4004), innerException)
        {
        }

        /// <summary>
        /// Constructor that sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/>. that contains contextual information about the source or destination.</param>
        protected AsynchronousOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

