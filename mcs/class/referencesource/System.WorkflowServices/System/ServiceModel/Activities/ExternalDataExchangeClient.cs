//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Workflow.Activities;

    class ExternalDataExchangeClient : ClientBase<IExternalDataExchange>
    {
        public ExternalDataExchangeClient(Binding binding, EndpointAddress address)
            : base(binding, address)
        {
            ContractDescription contractDescription = this.Endpoint.Contract;
            foreach (OperationDescription opDesc in contractDescription.Operations)
            {
                NetDataContractSerializerOperationBehavior netDataContractSerializerOperationBehavior = NetDataContractSerializerOperationBehavior.ApplyTo(opDesc);
                Fx.Assert(netDataContractSerializerOperationBehavior != null, "IExternalDataExchange must use NetDataContractSerializer.");
            }
        }

        public void RaiseEvent(ExternalDataEventArgs eventArgs, IComparable queueName, object message)
        {
            base.Channel.RaiseEvent(eventArgs, queueName, message);
        }
    }
}
