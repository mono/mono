//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;

    public interface IInputSessionChannel : IInputChannel, ISessionChannel<IInputSession>
    {
    }
}
