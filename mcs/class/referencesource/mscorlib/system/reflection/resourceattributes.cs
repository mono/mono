// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// ResourceAttributes is an enum which defines the attributes that may be associated
// 
// <OWNER>[....]</OWNER>
//  with a manifest resource.  The values here are defined in Corhdr.h.
//
// <EMAIL>Author: [....]</EMAIL>
// Date: April 2000
//
namespace System.Reflection {
    
    using System;
[Serializable]
[Flags]  
[System.Runtime.InteropServices.ComVisible(true)]
    public enum ResourceAttributes
    {
        Public          =   0x0001,
        Private         =   0x0002,
    }
}
