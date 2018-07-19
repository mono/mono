//------------------------------------------------------------------------------
// <copyright file="SqlException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">balnee</owner>
// <owner current="true" primary="false">krishnib</owner>
//------------------------------------------------------------------------------
namespace System.Data.SqlClient
{
    /// <summary>
    /// Encryption types supported in TCE
    /// </summary>
    internal enum SqlClientEncryptionType
    {
        PlainText = 0,
        Deterministic,
        Randomized
    }
}
