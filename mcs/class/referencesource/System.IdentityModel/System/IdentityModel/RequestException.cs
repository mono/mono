//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel
{
    /// <summary>
    /// Base class for exceptions thrown on request failures.
    /// </summary>
    [Serializable]
    public abstract class RequestException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected RequestException()
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        protected RequestException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        protected RequestException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        /// <summary>
        /// Constructor that sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected RequestException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
