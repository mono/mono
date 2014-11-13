//---------------------------------------------------------------------
// <copyright file="ObjectMslConstructs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Mapping
{
    /// <summary>
    /// Defines all the string constrcuts defined in OC MSL specification
    /// </summary>
    internal static class ObjectMslConstructs {
        #region Fields
        internal const string MappingElement = "Mapping";
        internal const string AliasElement = "Alias";
        internal const string AliasKeyAttribute = "Key";
        internal const string AliasValueAttribute = "Value";
        internal const char IdentitySeperator = ':';
        #endregion
    }
}
