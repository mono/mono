// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  IIdentityPermissionFactory.cs
// 
// <OWNER>ShawnFa</OWNER>
//
//  All Identities will implement this interface.
//

namespace System.Security.Policy {
    using System.Runtime.Remoting;
    using System;
    using System.Security.Util;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IIdentityPermissionFactory
    {
        IPermission CreateIdentityPermission( Evidence evidence );
    }

}
