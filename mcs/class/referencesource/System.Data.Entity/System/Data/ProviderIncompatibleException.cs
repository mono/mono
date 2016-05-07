//---------------------------------------------------------------------
// <copyright file="ProviderIncompatibleException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// This exception is thrown when the store provider exhibits a behavior incompatible with the entity client provider
    /// </summary>
    [Serializable]
    public sealed class ProviderIncompatibleException : EntityException
    {
        /// <summary>
        /// Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        public ProviderIncompatibleException() 
            : base() 
        { }

        /// <summary>
        /// Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        /// <param name="message"></param>
        public ProviderIncompatibleException(string message) 
            : base(message) 
        { }
        
        /// <summary>
        /// Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ProviderIncompatibleException(string message, Exception innerException)
            : base(message, innerException) 
        { }

        /// <summary>
        /// Initializes a new instance of ProviderIncompatibleException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private ProviderIncompatibleException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        { }
    }
}

