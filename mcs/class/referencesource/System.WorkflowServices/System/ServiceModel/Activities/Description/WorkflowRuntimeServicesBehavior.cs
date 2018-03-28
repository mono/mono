//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Runtime.DurableInstancing;
    using System.Collections.Generic;
    using System.Threading;
    using System.ServiceModel.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    class WorkflowRuntimeServicesBehavior : IEndpointBehavior
    {
        WorkflowRuntimeServicesExtensionProvider extensionProvider;

        public WorkflowRuntimeServicesBehavior()
        {
            this.extensionProvider = new WorkflowRuntimeServicesExtensionProvider();
        }

        public void AddService(object service)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            this.extensionProvider.AddService(service);
        }

        public void RemoveService(object service)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            this.extensionProvider.RemoveService(service);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");
            }
            return this.extensionProvider.GetService(serviceType);
        }

        public T GetService<T>()
        {
            return this.extensionProvider.GetService<T>();
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            WorkflowServiceHost serviceHost = endpointDispatcher.ChannelDispatcher.Host as WorkflowServiceHost;
            if (serviceHost != null)
            {
                foreach (OperationDescription operation in endpoint.Contract.Operations)
                {
                    NetDataContractSerializerOperationBehavior netDataContractSerializerOperationBehavior =
                        NetDataContractSerializerOperationBehavior.ApplyTo(operation);
                }

                this.extensionProvider.PopulateExtensions(serviceHost, endpointDispatcher.EndpointAddress.Uri.AbsoluteUri);
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
