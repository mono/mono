//---------------------------------------------------------------------
// <copyright file="EntityCommandCompilationException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.IO;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Represents a failure while trying to prepare or execute a CommandCompilation
    /// 
    /// This exception is intended to provide a common exception that people can catch to 
    /// hold provider exceptions (SqlException, OracleException) when using the EntityCommand
    /// to execute statements.
    /// </summary>
    [Serializable]
    public sealed class EntityCommandCompilationException : EntityException {

        #region Constructors
        /// <summary>
        /// initializes a new instance of EntityCommandCompilationException, no message, no inner exception.  Probably shouldn't 
        /// exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandCompilationException()
            : base() {
            HResult = HResults.CommandCompilation;
        }

        /// <summary>
        /// initializes a new instance of EntityCommandCompilationException, with message, no inner exception.  Probably shouldn't 
        /// exist, but it makes FxCop happy.
        /// </summary>
        public EntityCommandCompilationException(string message)
            : base(message) {
            HResult = HResults.CommandCompilation;
        }

        /// <summary>
        /// initializes a new instance of EntityCommandCompilationException with message and an inner exception instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public EntityCommandCompilationException(string message, Exception innerException)
            : base(message, innerException) {
            HResult = HResults.CommandCompilation;
        }

        /// <summary>
        /// initializes a new instance EntityCommandCompilationException with a given SerializationInfo and StreamingContext
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        private EntityCommandCompilationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
            HResult = HResults.CommandCompilation;
        }
        #endregion
    }
}
