//---------------------------------------------------------------------
// <copyright file="MetadataException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// metadata exception class
    /// </summary>
    /// 
    [Serializable]
    public sealed class MetadataException : EntityException
    {
        #region Constructors
        /// <summary>
        /// constructor with default message
        /// </summary>
        public MetadataException() // required ctor
            : base(System.Data.Entity.Strings.Metadata_General_Error)
        {
            HResult = HResults.Metadata;
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        public MetadataException(string message) // required ctor
            : base(message)
        {
            HResult = HResults.Metadata;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="message">localized error message</param>
        /// <param name="innerException">inner exception</param>
        public MetadataException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
            HResult = HResults.Metadata;
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private MetadataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
    }
}
