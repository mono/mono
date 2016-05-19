/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCacheManager.cs

Abstract:
    The file contains two streams used in conjunction with caching.
    The first class will combine two streams for reading into just one continues stream.
    The second class will forward (as writes) to external stream all reads issued on a "this" stream.

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

--*/
namespace System.Net {

    [Flags]
    internal enum CloseExState {
        Normal  = 0x0,          // just a close
        Abort   = 0x1,          // unconditionaly release resources
        Silent  = 0x2           // do not throw on close if possible
    }
    //
    // This is advanced closing mechanism required by ConnectStream to work properly
    // Consider: Either revise ConnectStream class or push Stream owners to get this in.
    //
    internal interface ICloseEx {
        void CloseEx(CloseExState closeState);
    }
}
