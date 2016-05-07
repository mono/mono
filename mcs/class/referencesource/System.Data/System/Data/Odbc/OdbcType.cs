//------------------------------------------------------------------------------
// <copyright file="OdbcType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">mithomas</owner>
// <owner current="true" primary="false">markash</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;

namespace System.Data.Odbc
{
    public enum OdbcType {
        BigInt = 1,
        Binary = 2,
        Bit = 3,
        Char = 4,
        DateTime = 5,
        Decimal = 6,
        Numeric = 7,
        Double = 8,
        Image = 9,
        Int = 10,
        NChar = 11,
        NText = 12,
        NVarChar = 13,
        Real = 14,
        UniqueIdentifier = 15,
        SmallDateTime = 16,
        SmallInt = 17,
        Text = 18,
        Timestamp = 19,
        TinyInt = 20,
        VarBinary = 21,
        VarChar = 22,
        Date = 23,
        Time = 24,
    }
}
