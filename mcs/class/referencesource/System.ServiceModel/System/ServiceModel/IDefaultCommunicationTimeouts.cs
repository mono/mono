//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public interface IDefaultCommunicationTimeouts
    {
        TimeSpan CloseTimeout { get; }
        TimeSpan OpenTimeout { get; }
        TimeSpan ReceiveTimeout { get; }
        TimeSpan SendTimeout { get; }
    }
}
