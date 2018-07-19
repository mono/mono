// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  PermissionState.cs
// 
// <OWNER>ShawnFa</OWNER>
//
//  The Runtime policy manager.  Maintains a set of IdentityMapper objects that map 
//  inbound evidence to groups.  Resolves an identity into a set of permissions
//

namespace System.Security.Permissions {
    
    using System;
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum PermissionState
    {
        Unrestricted = 1,
        None = 0,
    } 
    
}
