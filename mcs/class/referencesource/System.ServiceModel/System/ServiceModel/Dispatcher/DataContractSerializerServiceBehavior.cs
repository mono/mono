//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    internal class DataContractSerializerServiceBehavior : IServiceBehavior, IEndpointBehavior
    {
        bool ignoreExtensionDataObject;
        int maxItemsInObjectGraph;

        internal DataContractSerializerServiceBehavior(bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
        }

        public bool IgnoreExtensionDataObject
        {
            get { return this.ignoreExtensionDataObject; }
            set { this.ignoreExtensionDataObject = value; }
        }

        public int MaxItemsInObjectGraph
        {
            get { return this.maxItemsInObjectGraph; }
            set { this.maxItemsInObjectGraph = value; }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            ApplySerializationSettings(description, ignoreExtensionDataObject, maxItemsInObjectGraph);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            ApplySerializationSettings(serviceEndpoint, ignoreExtensionDataObject, maxItemsInObjectGraph);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            ApplySerializationSettings(serviceEndpoint, ignoreExtensionDataObject, maxItemsInObjectGraph);
        }

        internal static void ApplySerializationSettings(ServiceDescription description, bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (!endpoint.InternalIsSystemEndpoint(description))
                {
                    ApplySerializationSettings(endpoint, ignoreExtensionDataObject, maxItemsInObjectGraph);
                }
            }
        }

        internal static void ApplySerializationSettings(ServiceEndpoint endpoint, bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                foreach (IOperationBehavior ob in operation.Behaviors)
                {
                    if (ob is DataContractSerializerOperationBehavior)
                    {
                        DataContractSerializerOperationBehavior behavior = (DataContractSerializerOperationBehavior)ob;
                        if (behavior != null)
                        {
                            if (!behavior.IgnoreExtensionDataObjectSetExplicit)
                            {
                                behavior.ignoreExtensionDataObject = ignoreExtensionDataObject;
                            }
                            if (!behavior.MaxItemsInObjectGraphSetExplicit)
                            {
                                behavior.maxItemsInObjectGraph = maxItemsInObjectGraph;
                            }
                        }
                    }
                }
            }
        }

    }
}
