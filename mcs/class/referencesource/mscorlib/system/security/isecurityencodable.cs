// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// ISecurityEncodable.cs
// 
// <OWNER>ShawnFa</OWNER>
//
// All encodable security classes that support encoding need to
// implement this interface
//

namespace System.Security  {
    
    using System;
    using System.Security.Util;
    
    
[System.Runtime.InteropServices.ComVisible(true)]
    public interface ISecurityEncodable
    {
#if FEATURE_CAS_POLICY
        SecurityElement ToXml();
    
        void FromXml( SecurityElement e );
#endif // FEATURE_CAS_POLICY
    }

}


