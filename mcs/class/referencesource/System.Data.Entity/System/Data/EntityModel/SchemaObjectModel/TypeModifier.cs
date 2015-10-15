//---------------------------------------------------------------------
// <copyright file="TypeModifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner anpete
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Return value from StructuredProperty RemoveTypeModifier
    /// </summary>
    internal enum TypeModifier
    {
        /// <summary>Type string has no modifier</summary>
        None,
        /// <summary>Type string was of form Array(...)</summary>
        Array,
        /// <summary>Type string was of form Set(...)</summary>
        Set,
        /// <summary>Type string was of form Table(...)</summary>
        Table,
    }
}
