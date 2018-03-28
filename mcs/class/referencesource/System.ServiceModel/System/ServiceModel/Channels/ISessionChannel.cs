//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;

    public interface ISessionChannel<TSession> where TSession : ISession
    {
        TSession Session { get; }
    }
}
