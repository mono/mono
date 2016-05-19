//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class NetDispatcherFaultException : FaultException
    {
        public NetDispatcherFaultException(string reason, FaultCode code, Exception innerException)
            : base(reason, code, FaultCodeConstants.Actions.NetDispatcher, innerException)
        {
        }
        public NetDispatcherFaultException(FaultReason reason, FaultCode code, Exception innerException)
            : base(reason, code, FaultCodeConstants.Actions.NetDispatcher, innerException)
        {
        }
    }
}
