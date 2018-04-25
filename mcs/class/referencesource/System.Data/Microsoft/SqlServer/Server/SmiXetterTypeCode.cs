//------------------------------------------------------------------------------
// <copyright file="SmiXetterTypeCode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">alazela</owner>
// <owner current="true" primary="false">billin</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;

    // Types should match Getter/Setter names
    internal enum SmiXetterTypeCode {
        XetBoolean,
        XetByte,
        XetBytes,
        XetChars,
        XetString,
        XetInt16,
        XetInt32,
        XetInt64,
        XetSingle,
        XetDouble,
        XetSqlDecimal,
        XetDateTime,
        XetGuid,
        GetVariantMetaData,     // no set call, just get
        GetXet,
        XetTime,                // XetTime mistakenly named, does not match getter/setter method name
        XetTimeSpan = XetTime,  // prefer using XetTimeSpan instead of XetTime.  Both mean the same thing for now.
        XetDateTimeOffset,
    }
}
