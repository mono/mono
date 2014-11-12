// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface: IAsyncResult
**
** Purpose: Interface to encapsulate the results of an async
**          operation
**
===========================================================*/
namespace System {
    
    using System;
    using System.Threading;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IAsyncResult
    {
        bool IsCompleted { get; }

        WaitHandle AsyncWaitHandle { get; }


        Object     AsyncState      { get; }

        bool       CompletedSynchronously { get; }
   
    
    }

}
