// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Internal.Runtime.Augments
{
    public enum CausalityRelation
    {
        AssignDelegate = 0,
        Join = 1,
        Choice = 2,
        Cancel = 3,
        Error = 4,
    }

    public enum CausalitySource
    {
        Application = 0,
        Library = 1,
        System = 2,
    }

    public enum CausalityTraceLevel
    {
        Required = 0,
        Important = 1,
        Verbose = 2,
    }

    public enum AsyncStatus
    {
        Started = 0,
        Completed = 1,
        Canceled = 2,
        Error = 3,
    }

    public enum CausalitySynchronousWork
    {
        CompletionNotification = 0,
        ProgressNotification = 1,
        Execution = 2,
    }
}
