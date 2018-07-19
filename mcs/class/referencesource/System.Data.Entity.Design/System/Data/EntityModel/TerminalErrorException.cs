//---------------------------------------------------------------------
// <copyright file="TerminalErrorException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner srimand
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// This class is used to interupt the normal flow of CodeGenerator.Start 
    /// when errors are found during schema parsing, or too many errors have
    /// been found.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    internal class TerminalErrorException : Exception
    {
    }
}
