// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System {

    // This enum is used to indentify DateTime instances in cases when they are known to be in local time, 
    // UTC time or if this information has not been specified or is not applicable.

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum DateTimeKind
    {
        Unspecified = 0,
        Utc = 1,
        Local = 2,
    }
}
