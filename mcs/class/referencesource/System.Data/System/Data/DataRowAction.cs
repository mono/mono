//------------------------------------------------------------------------------
// <copyright file="DataRowAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    [Flags] 
    public enum DataRowAction { 
        Nothing                  = 0,        //  0 0x00
        Delete                   = (1 << 0), //  1 0x01
        Change                   = (1 << 1), //  2 0x02
        Rollback                 = (1 << 2), //  4 0x04
        Commit                   = (1 << 3), //  8 0x08
        Add                      = (1 << 4), // 16 0x10
        ChangeOriginal           = (1 << 5), // 32 0x20
        ChangeCurrentAndOriginal = (1 << 6), // 64 0x40
    }
}
