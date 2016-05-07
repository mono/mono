//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    public delegate void FaultCallback(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom);
}
