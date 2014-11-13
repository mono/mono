//---------------------------------------------------------------------
// <copyright file="UpdateException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Data.Objects;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    /// <summary>
    /// Exception during save changes to store
    /// </summary>
    [Serializable]
    public class UpdateException : DataException
    {
        [NonSerialized]
        private ReadOnlyCollection<ObjectStateEntry> _stateEntries;
            
    
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateException() 
            : base() 
        {
        }

        /// <summary>
        /// Constructor that takes a message
        /// </summary>
        /// <param name="message"></param>
        public UpdateException(string message) 
            : base(message) 
        {
        }

        /// <summary>
        /// Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public UpdateException(string message, Exception innerException) 
            : base(message, innerException) 
        {
        }

        /// <summary>
        /// Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="stateEntries"></param>
        public UpdateException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
            : base(message, innerException)
        {
            List<ObjectStateEntry> list = new List<ObjectStateEntry>(stateEntries); 
            _stateEntries = list.AsReadOnly();
        }

        /// <summary>
        /// Gets state entries implicated in the error.
        /// </summary>
        public ReadOnlyCollection<ObjectStateEntry> StateEntries { get { return _stateEntries; } }

        /// <summary>
        /// The protected constructor for serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected UpdateException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        {
        }

        #endregion
     }
}
