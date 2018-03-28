//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Threading;

    static class WaitCallbackActionItem
    {
        internal static bool ShouldUseActivity { get; set; }
    }
}
