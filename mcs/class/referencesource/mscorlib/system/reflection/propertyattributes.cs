// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// PropertyAttributes is an enum which defines the attributes that may be associated
// 
// <OWNER>WESU</OWNER>
//    with a property.  The values here are defined in Corhdr.h.
//
// <TODO>Author: darylo</TODO>
// Date: Aug 99
//
namespace System.Reflection {
    
    using System;
    // This Enum matchs the CorPropertyAttr defined in CorHdr.h
[Serializable]
[Flags]  
[System.Runtime.InteropServices.ComVisible(true)]
    public enum PropertyAttributes
    {
        None            =   0x0000,
        SpecialName     =   0x0200,     // property is special.  Name describes how.

        // Reserved flags for Runtime use only.
        ReservedMask          =   0xf400,
        RTSpecialName         =   0x0400,     // Runtime(metadata internal APIs) should check name encoding.
        HasDefault            =   0x1000,     // Property has default 
        Reserved2             =   0x2000,     // reserved bit
        Reserved3             =   0x4000,     // reserved bit 
        Reserved4             =   0x8000      // reserved bit 
    }
}
