//---------------------------------------------------------------------
// <copyright file="EntityCommandExecutionException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.IO;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Represents a failure while trying to prepare or execute a CommandExecution
    /// 
    /// This exception is intended to provide a common exception that people can catch to 
    /// hold provider exceptions (SqlException, OracleException) when using the EntityCommand
    /// to execute statements.
    /// </summary>
    [Serializable]
    public sealed class EntityCommandExecutionException : EntityException {

        #region Constructors
        /// <summary>
        /// initializes a new instance of EntityCommandExecutionException, no message, no inner exception.  Probably shouldn't 
        /// exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandExecutionException()
            : base() {
            HResult = HResults.CommandExecution;
        }

        /// <summary>
        /// initializes a new instance of EntityCommandExecutionException, with message, no inner exception.  Probably shouldn't 
        /// exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandExecutionException(string message)
            : base(message) {
            HResult = HResults.CommandExecution;
        }

        /// <summary>
        /// initializes a new instance of EntityCommandExecutionException with message and an inner exception instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public EntityCommandExecutionException(string message, Exception innerException)
            : base(message, innerException) {
            HResult = HResults.CommandExecution;
        }

        /// <summary>
        /// initializes a new instance EntityCommandExecutionException with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        private EntityCommandExecutionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
            HResult = HResults.CommandExecution;
        }
        #endregion
    }
}
