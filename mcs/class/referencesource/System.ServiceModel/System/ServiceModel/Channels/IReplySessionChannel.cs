//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;

    public interface IReplySessionChannel : IReplyChannel, ISessionChannel<IInputSession>
    {
    }
}
