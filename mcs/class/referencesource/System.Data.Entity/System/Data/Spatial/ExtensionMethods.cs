//------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Spatial.Internal;

namespace System.Data.Spatial
{
    internal static class ExtensionMethods
    {
        internal static void CheckNull<T>(this T value, string argumentName) where T : class
        {
            if (value == null)
            {
                throw SpatialExceptions.ArgumentNull(argumentName);
            }
        }
    }
}
