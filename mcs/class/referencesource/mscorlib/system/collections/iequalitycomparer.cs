// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface:  IEqualityComparer
**
** <OWNER>[....]</OWNER>
** 
**
** Purpose: A mechanism to expose a simplified infrastructure for 
**          Comparing objects in collections.
**
** 
===========================================================*/
namespace System.Collections {
    
    using System;
    // An IEqualityComparer is a mechanism to consume custom performant comparison infrastructure
    // that can be consumed by some of the common collections.
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IEqualityComparer {
        bool Equals(Object x, Object y);
        int GetHashCode(Object obj);        
    }
}
