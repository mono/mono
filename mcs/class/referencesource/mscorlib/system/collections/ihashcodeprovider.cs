// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: IHashCodeProvider
** 
** <OWNER>kimhamil</OWNER>
**
**
** Purpose: A bunch of strings.
**
** 
===========================================================*/
namespace System.Collections {
    
    using System;
    // Provides a mechanism for a hash table user to override the default
    // GetHashCode() function on Objects, providing their own hash function.
    [Obsolete("Please use IEqualityComparer instead.")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IHashCodeProvider 
    {
        // Interfaces are not serializable
        // Returns a hash code for the given object.  
        // 
        int GetHashCode (Object obj);
    }
}
