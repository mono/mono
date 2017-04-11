//---------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class holding utility functions for metadata
    /// </summary>
    internal static class Util
    {
        #region Methods
        /// <summary>
        /// Throws an appropriate exception if the given item is a readonly, used when an attempt is made to change
        /// a property
        /// </summary>
        /// <param name="item">The item whose readonly is being tested</param>
        internal static void ThrowIfReadOnly(MetadataItem item)
        {
            Debug.Assert(item != null, "The given item is null");
            if (item.IsReadOnly)
            {
                throw EntityUtil.OperationOnReadOnlyItem();
            }
        }

        /// <summary>
        /// Check to make sure the given item do have identity
        /// </summary>
        /// <param name="item">The item to check for valid identity</param>
        /// <param name="argumentName">The name of the argument</param>
        [Conditional("DEBUG")]
        internal static void AssertItemHasIdentity(MetadataItem item, string argumentName)
        {
            Debug.Assert(!string.IsNullOrEmpty(item.Identity), "Item has empty identity.");
            EntityUtil.GenericCheckArgumentNull(item, argumentName);
        }
        #endregion
    }
}
