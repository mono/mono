// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// IStackWalk.cs
// 
// <OWNER>[....]</OWNER>
//

namespace System.Security
{

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IStackWalk
    {
        [DynamicSecurityMethodAttribute()]
        void Assert();
        
        [DynamicSecurityMethodAttribute()]
        void Demand();
        
        [DynamicSecurityMethodAttribute()]
        void Deny();
        
        [DynamicSecurityMethodAttribute()]
        void PermitOnly();
    }
}
