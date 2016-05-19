//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IChannel : ICommunicationObject
    {
        T GetProperty<T>() where T : class;
    }
}
