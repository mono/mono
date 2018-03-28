//---------------------------------------------------------------------
// <copyright file="EntityException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Provider exception - Used by the entity client.
    /// </summary>
    /// 
    [Serializable]
    public class EntityException : DataException
    {
        /// <summary>
        /// Constructor with default message
        /// </summary>
        public EntityException() // required ctor
            : base(System.Data.Entity.Strings.EntityClient_ProviderGeneralError)
        {
        }

        /// <summary>
        /// Constructor that accepts a pre-formatted message
        /// </summary>
        /// <param name="message">localized error message</param>
        public EntityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor that accepts a pre-formatted message and an inner exception
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        public EntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor for deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
