// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Enums
**
**
** Purpose: Enums for pipe streams.
**
**
===========================================================*/

using System;
using System.Text;

namespace System.IO.Pipes {

    [Serializable]
    public enum PipeDirection {
        In = 1,
        Out = 2,
        InOut = In | Out,
    }

    [Serializable]
    public enum PipeTransmissionMode {
        Byte = 0,
        Message = 1,
    }

    [Serializable]
    [Flags]
    public enum PipeOptions {
        None = 0x0,
        WriteThrough = unchecked((int)0x80000000),
        Asynchronous = unchecked((int)0x40000000),  // corresponds to FILE_FLAG_OVERLAPPED
    }

}


