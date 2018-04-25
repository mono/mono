//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;

    public interface IOutputSessionChannel
        : IOutputChannel, ISessionChannel<IOutputSession>
    {
    }
}
