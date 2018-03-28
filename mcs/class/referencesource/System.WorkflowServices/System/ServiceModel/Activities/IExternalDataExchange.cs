//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.ServiceModel;

    [ServiceContract(Namespace = Description.WorkflowRuntimeEndpoint.ExternalDataExchangeNamespace)]
    interface IExternalDataExchange
    {
        [OperationContract(IsOneWay = true, Action = Description.WorkflowRuntimeEndpoint.RaiseEventAction)]
        void RaiseEvent(EventArgs eventArgs, IComparable queueName, object message);
    }
}
