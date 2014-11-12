// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

namespace System.Security.Principal
{
#if !FEATURE_NETCORE
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
#endif
    public enum TokenImpersonationLevel {
        None            = 0,
        Anonymous       = 1,
        Identification  = 2,
        Impersonation   = 3,
        Delegation      = 4
    }
}
