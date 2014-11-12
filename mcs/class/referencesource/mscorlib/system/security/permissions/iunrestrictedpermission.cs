// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// IUnrestrictedPermission.cs
// 
// <OWNER>[....]</OWNER>
//

namespace System.Security.Permissions {
    
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IUnrestrictedPermission
    {
        bool IsUnrestricted();
    }
}
