// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// EventAttributes are an enum defining the attributes associated with
// 
// <OWNER>WESU</OWNER>
//    and Event.  These are defined in CorHdr.h and are a combination of
//    bits and enums.
//
//
namespace System.Reflection {
    
    using System;
    [Serializable]
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum EventAttributes {
        None              =   0x0000,
            
        // This Enum matchs the CorEventAttr defined in CorHdr.h
        SpecialName       =   0x0200,     // event is special.  Name describes how.

        // Reserved flags for Runtime use only.
        ReservedMask      =   0x0400,
        RTSpecialName     =   0x0400,     // Runtime(metadata internal APIs) should check name encoding.
    }
}
