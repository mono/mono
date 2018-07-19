//---------------------------------------------------------------------
// <copyright file="EdmItemError.cs" company="Microsoft">
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
    /// Class representing Edm error for an inmemory EdmItem
    /// </summary>
    internal class EdmItemError : EdmError
    {
        #region Constructors

        /// <summary>
        /// Construct the EdmItemError with an error message
        /// </summary>
        /// <param name="message">The error message for this validation error</param>
        /// <param name="item">The item that causes the validation error</param>
        public EdmItemError(string message, MetadataItem item)
            : base(message)
        {
            _item = item;
        }

        #endregion

        #region Fields
        private MetadataItem _item; //Metadata item for which the error is being reported
        #endregion
    }
}
