//---------------------------------------------------------------------
// <copyright file="EdmSchemaErrorSeverity.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       leil
// @backupOwner anpete
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    // if you edit this file be sure you change GeneratorErrorSeverity
    // also, they must stay in sync

    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public enum EdmSchemaErrorSeverity
    {
        /// <summary></summary>
        Warning = 0,
        /// <summary></summary>
        Error = 1,
    }
}
