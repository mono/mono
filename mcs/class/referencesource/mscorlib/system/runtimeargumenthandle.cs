// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {
    
    using System;
    //  This value type is used for constructing System.ArgIterator. 
    // 
    //  SECURITY : m_ptr cannot be set to anything other than null by untrusted
    //  code.  
    // 
    //  This corresponds to EE VARARGS cookie.

    // Cannot be serialized
    [System.Runtime.InteropServices.ComVisible(true)]
    public struct RuntimeArgumentHandle
    {
        private IntPtr m_ptr;

        internal IntPtr Value { get { return m_ptr; } }
    }

}
