//------------------------------------------------------------------------------
// <copyright file="CommandBehavior.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    [Flags()]
    public enum CommandBehavior {
        Default          = 0,  // with data, multiple results, may affect database, MDAC 68240
        SingleResult     = 1,  // with data, force single result, may affect database
        SchemaOnly       = 2,  // column info, no data, no effect on database
        KeyInfo          = 4,  // column info + primary key information (if available)
        // 
        SingleRow        = 8, // data, hint single row and single result, may affect database - doesn't apply to child(chapter) results
        SequentialAccess = 0x10,
        CloseConnection  = 0x20,
    }
}
