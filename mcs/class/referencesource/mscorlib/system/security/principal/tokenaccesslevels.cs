// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>ShawnFa</OWNER>
// 

namespace System.Security.Principal
{

    [Serializable]
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum TokenAccessLevels {
        AssignPrimary       = 0x00000001,
        Duplicate           = 0x00000002,
        Impersonate         = 0x00000004,
        Query               = 0x00000008,
        QuerySource         = 0x00000010,
        AdjustPrivileges    = 0x00000020,
        AdjustGroups        = 0x00000040,
        AdjustDefault       = 0x00000080,
        AdjustSessionId     = 0x00000100,

        Read                = 0x00020000 | Query,

        Write               = 0x00020000 | AdjustPrivileges | AdjustGroups | AdjustDefault,

        AllAccess           = 0x000F0000       |
                              AssignPrimary    |
                              Duplicate        |
                              Impersonate      |
                              Query            |
                              QuerySource      |
                              AdjustPrivileges |
                              AdjustGroups     |
                              AdjustDefault    |
                              AdjustSessionId,

        MaximumAllowed      = 0x02000000
    }
}
