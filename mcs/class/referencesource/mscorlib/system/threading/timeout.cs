// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>Microsoft</OWNER>

namespace System.Threading {
    using System.Threading;
    using System;
    // A constant used by methods that take a timeout (Object.Wait, Thread.Sleep
    // etc) to indicate that no timeout should occur.
    //
    // <

    [System.Runtime.InteropServices.ComVisible(true)]
    public static class Timeout
    {
        [System.Runtime.InteropServices.ComVisible(false)]
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);

        public const int Infinite = -1;
        internal const uint UnsignedInfinite = unchecked((uint)-1);
    }

}
