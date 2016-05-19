//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    static class ClientOperationFormatterProvider
    {
        static DispatchRuntime dummyDispatchRuntime;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "The GetFormatterFromRuntime uses this.")]
        static ClientRuntime DummyClientRuntime
        {
            get
            {
                return DummyDispatchRuntime.CallbackClientRuntime;
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "The GetFormatterFromRuntime uses this.")]
        static DispatchRuntime DummyDispatchRuntime
        {
            get
            {
                if (dummyDispatchRuntime == null)
                {
                    EndpointDispatcher dispatcher = new EndpointDispatcher(new EndpointAddress("http://dummyuri/"), "dummyContract", "urn:dummyContractNs");
                    dummyDispatchRuntime = dispatcher.DispatchRuntime;
                }
                return dummyDispatchRuntime;
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "The GetFormatterFromRuntime is used by ClientOperationFormatterHelper")]
        internal static IClientMessageFormatter GetFormatterFromRuntime(OperationDescription operationDescription)
        {
            System.ServiceModel.Dispatcher.ClientOperation clientOperation = new System.ServiceModel.Dispatcher.ClientOperation(DummyClientRuntime, operationDescription.Name, operationDescription.Messages[0].Action);

            // Default to DataContractSerializerOperationBehavior
            if (operationDescription.Behaviors.Count == 0)
            {
                IOperationBehavior operationBehavior = new DataContractSerializerOperationBehavior(operationDescription);
                operationBehavior.ApplyClientBehavior(operationDescription, clientOperation);
            }
            else
            {
                foreach (IOperationBehavior operationBehavior in operationDescription.Behaviors)
                {
                    operationBehavior.ApplyClientBehavior(operationDescription, clientOperation);
                }
            }

            return clientOperation.Formatter;
        }
    }
}
