//-----------------------------------------------------------------------
// <copyright file="WSTrustSerializationException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Runtime.Serialization;

    /// <summary>
    /// This indicates an error has occured while serializing/deserializing
    /// a WS-Trust message.
    /// </summary>
    [Serializable]
    public class WSTrustSerializationException : Exception, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="WSTrustSerializationException"/>
        /// </summary>
        public WSTrustSerializationException()
            : this(SR.GetString(SR.ID3063))
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="WSTrustSerializationException"/>
        /// </summary>
        /// <param name="message">The exception message.</param>
        public WSTrustSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="WSTrustSerializationException"/>
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public WSTrustSerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="WSTrustSerializationException"/>
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected WSTrustSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
