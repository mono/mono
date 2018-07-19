// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// IUnrestrictedPermission.cs
// 
// <OWNER>ShawnFa</OWNER>
//

namespace System.Security.Permissions {
    
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IUnrestrictedPermission
    {
        bool IsUnrestricted();
    }
}
