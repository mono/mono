// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
namespace System.Reflection.Emit {
    
    using System;
    // This Enum matchs the CorFieldAttr defined in CorHdr.h
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum PEFileKinds
    {
        Dll                = 0x0001,
        ConsoleApplication = 0x0002,
        WindowApplication = 0x0003,
    }
}
