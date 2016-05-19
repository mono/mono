// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Diagnostics {
    
    /*
     * FailDebug indicates the debugger should be invoked
     * FailIgnore indicates the failure should be ignored & the 
     *            program continued
     * FailTerminate indicates that the program should be terminated
     * FailContinue indicates that no decision is made - 
     *        the previous Filter should be invoked
     */
    using System;
    [Serializable]
    internal enum AssertFilters
    {
        FailDebug           = 0,
        FailIgnore          = 1,
        FailTerminate       = 2,
        FailContinueFilter  = 3,
    }
}
