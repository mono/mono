//---------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="Microsoft">
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
    /// Thrown to indicate that a command tree is invalid.
    /// </summary>
    [Serializable]
    public sealed class InvalidCommandTreeException : DataException /*InvalidQueryException*/
    {
        /// <summary>
        /// Constructs a new InvalidCommandTreeException with a default message.
        /// </summary>
        public InvalidCommandTreeException()
            : base(System.Data.Entity.Strings.Cqt_Exceptions_InvalidCommandTree) { }

        /// <summary>
        /// Constructs a new InvalidCommandTreeException with the specified message.
        /// </summary>
        /// <param name="message">The exception message</param>
        public InvalidCommandTreeException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a new InvalidCommandTreeException with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of this InvalidCommandTreeException.</param>
        public InvalidCommandTreeException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Constructs a new InvalidCommandTreeException from the specified serialization info and streaming context.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private InvalidCommandTreeException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

     }        
}
