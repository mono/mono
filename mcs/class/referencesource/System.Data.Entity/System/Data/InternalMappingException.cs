//---------------------------------------------------------------------
// <copyright file="InternalMappingException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Runtime.Serialization;

namespace System.Data {

    /// <summary>
    /// Mapping exception class. Note that this class has state - so if you change even
    /// its internals, it can be a breaking change
    /// </summary>
    [Serializable]
    internal class InternalMappingException : EntityException {
        // effects: constructor with default message
        #region Constructors
        /// <summary>
        /// default constructor
        /// </summary>
        internal InternalMappingException() // required ctor
            : base()
        {
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // required CTOR for exceptions.
        internal InternalMappingException(string message) // required ctor
            : base(message)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // required CTOR for exceptions.
        internal InternalMappingException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InternalMappingException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        // effects: constructor that allows a log
        internal InternalMappingException(string message, ErrorLog errorLog) : base(message) {
            EntityUtil.CheckArgumentNull(errorLog, "errorLog");
            m_errorLog =  errorLog;
        }

        // effects:  constructor that allows single mapping error
        internal InternalMappingException(string message, ErrorLog.Record record)
            : base(message) {
                EntityUtil.CheckArgumentNull(record, "record");
            m_errorLog = new ErrorLog();
            m_errorLog.AddEntry(record);
        }
        #endregion

        #region Fields
        // Keep track of mapping errors that we want to give to the
        // user in one shot
        private ErrorLog m_errorLog;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the inner exceptions stored in this
        /// </summary>
        internal ErrorLog ErrorLog {
            get {
                return m_errorLog;
            }
        }
        #endregion
    }
}
