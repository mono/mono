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

    static class ServiceOperationFormatterProvider
    {
        static DispatchRuntime dummyDispatchRuntime;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "The GetDispatcherFormatterFromRuntime uses this.")]
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
            Justification = "The GetDispatcherFormatterFromRuntime is used by Receive")]
        internal static IDispatchMessageFormatter GetDispatcherFormatterFromRuntime(OperationDescription operationDescription)
        {
            System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation = new System.ServiceModel.Dispatcher.DispatchOperation(DummyDispatchRuntime, operationDescription.Name, operationDescription.Messages[0].Action);
            IOperationBehavior operationBehavior = new DataContractSerializerOperationBehavior(operationDescription);
            operationBehavior.ApplyDispatchBehavior(operationDescription, dispatchOperation);

            return dispatchOperation.Formatter;
        }
    }
}
