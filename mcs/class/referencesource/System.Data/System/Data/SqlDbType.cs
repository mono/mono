//------------------------------------------------------------------------------
// <copyright file="SqlDbType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">blained</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    /* enumeration used to identify data types specific to SQL Server
     * 
     * note that these are a subset of the types exposed by OLEDB so keep the enum values in ssync with
     * OleDbType values
     */

    using System;

    // Specifies the SQL Server data type.
    public enum SqlDbType {
        // A 64-bit signed integer.
        BigInt           = 0,
        Binary           = 1,
        Bit              = 2,
        Char             = 3,
        DateTime         = 4,
        Decimal          = 5,
        Float            = 6,
        Image            = 7,
        Int              = 8,
        Money            = 9,
        NChar            = 10, 
        NText            = 11, 
        NVarChar         = 12, 
        Real             = 13,
        UniqueIdentifier = 14,
        SmallDateTime    = 15,
        SmallInt         = 16,
        SmallMoney       = 17,
        Text             = 18,
        Timestamp        = 19,
        TinyInt          = 20,
        VarBinary        = 21,
        VarChar          = 22,
        Variant          = 23,
        Xml              = 25, 
        Udt              = 29,
        Structured       = 30,
        Date             = 31,
        Time             = 32,
        DateTime2        = 33,
        DateTimeOffset   = 34,
    }
}
