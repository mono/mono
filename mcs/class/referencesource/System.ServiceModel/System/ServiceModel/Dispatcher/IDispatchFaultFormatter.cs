//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;

namespace System.ServiceModel.Dispatcher
{
    internal interface IDispatchFaultFormatter
    {
        MessageFault Serialize(FaultException faultException, out string action);
    }

    internal interface IDispatchFaultFormatterWrapper
    {
        IDispatchFaultFormatter InnerFaultFormatter
        {
            get;
            set;
        }
    }
}

