//---------------------------------------------------------------------
// <copyright file="ObjectNotFoundException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft, nkline
//---------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This exception is thrown when a requested object is not found in the store.
    /// </summary>
    [Serializable]
    public sealed class ObjectNotFoundException : DataException
    {
        /// <summary>
        /// Initializes a new instance of ObjectNotFoundException
        /// </summary>
        public ObjectNotFoundException() 
            : base() 
        { }

        /// <summary>
        /// Initializes a new instance of ObjectNotFoundException
        /// </summary>
        /// <param name="message"></param>
        public ObjectNotFoundException(string message) 
            : base(message) 
        { }
        
        /// <summary>
        /// Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ObjectNotFoundException(string message, Exception innerException)
            : base(message, innerException) 
        { }

        /// <summary>
        /// Initializes a new instance of ObjectNotFoundException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        { }
    }
}

