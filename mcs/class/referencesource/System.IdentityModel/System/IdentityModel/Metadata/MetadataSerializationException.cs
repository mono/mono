//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// This indicates an error has occured while serializing/deserializing Saml
    /// metadata.
    /// </summary>
    [Serializable]
    public class MetadataSerializationException : Exception
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public MetadataSerializationException()
            : this(SR.GetString(SR.ID3198))
        {
        }

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        public MetadataSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message describes what was causing the exception.</param>
        /// <param name="innerException">The inner exception indicates the real reason the exception was thrown.</param>
        public MetadataSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor that sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/>. that contains contextual information about the source or destination.</param>
        protected MetadataSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
