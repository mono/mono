// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// CallingConventions is a set of Bits representing the calling conventions
// 
// <OWNER>WESU</OWNER>
//    in the system.
//
// <EMAIL>Author: meichint</EMAIL>
// Date: Aug 99
//
namespace System.Reflection {
    using System.Runtime.InteropServices;
    using System;
    [Serializable]
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum CallingConventions
    {
        //NOTE: If you change this please update COMMember.cpp.  These
        //    are defined there.
        Standard        = 0x0001,
        VarArgs         = 0x0002,
        Any             = Standard | VarArgs,
        HasThis         = 0x0020,
        ExplicitThis    = 0x0040,
    }
}
