//-----------------------------------------------------------------------
// <copyright file="InvalidRequestException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Throw this exception when the request was invalid or malformed.
    /// </summary>
    [Serializable]
    public class InvalidRequestException : RequestException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public InvalidRequestException()
            : base( SR.GetString( SR.ID2005 ) )
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public InvalidRequestException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public InvalidRequestException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        /// <summary>
        /// Constructor that sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidRequestException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
