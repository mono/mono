//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Collections.Generic;

    class ComPlusContractBehavior : IContractBehavior
    {
        ServiceInfo info;

        public ComPlusContractBehavior(ServiceInfo info)
        {
            this.info = info;
        }

        public void Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ContractDescription description,
                              ServiceEndpoint endpoint,
                              BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription description,
                                 ServiceEndpoint endpoint,
                                 DispatchRuntime dispatch)
        {
            dispatch.InstanceProvider = new ComPlusInstanceProvider(info);
            dispatch.InstanceContextInitializers.Add(new ComPlusInstanceContextInitializer(info));

            foreach (DispatchOperation operation in dispatch.Operations)
            {
                operation.CallContextInitializers.Add(
                    new ComPlusThreadInitializer(
                        description,
                        operation,
                        info));
            }
        }

        public void ApplyClientBehavior(ContractDescription description,
                              ServiceEndpoint endpoint,
                              ClientRuntime proxy)
        {
            return;
        }
    }
}
