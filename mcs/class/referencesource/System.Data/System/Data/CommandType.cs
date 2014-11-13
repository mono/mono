//------------------------------------------------------------------------------
// <copyright file="CommandType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum CommandType {
        Text            = 0x1,
      //Table           = 0x2,
        StoredProcedure = 0x4,
      //File            = 0x100,
        TableDirect     = 0x200,
    }
}
