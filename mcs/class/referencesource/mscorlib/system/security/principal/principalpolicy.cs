// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>ShawnFa</OWNER>
// 

//
//  PrincipalPolicy.cs
//
//  Enum describing what type of principal to create by default (assuming no
//  principal has been set on the AppDomain).
//

namespace System.Security.Principal
{
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum PrincipalPolicy {
        // Note: it's important that the default policy has the value 0.
        UnauthenticatedPrincipal = 0,
        NoPrincipal = 1,
#if !FEATURE_PAL
        WindowsPrincipal = 2,
#endif // !FEATURE_PAL
    }
}
