//---------------------------------------------------------------------
// <copyright file="EdmError.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Data;

namespace System.Data.Metadata.Edm
{

    /// <summary>
    /// This class encapsulates the error information for a generic EDM error.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [Serializable]
    public abstract class EdmError
    {
        #region Instance Fields
        private string _message = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message">The explanation of the error.</param>
        /// <param name="errorCode">The code associated with this error.</param>
        /// <param name="severity">The severity of the error.</param>
        internal EdmError(string message)
        {
            EntityUtil.CheckStringArgument(message, "message");
            _message = message;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }
        #endregion

    }
}
