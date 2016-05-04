//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [Fx.Tag.XamlVisible(false)]
    public sealed class RoutingBehavior : IServiceBehavior
    {
        RoutingConfiguration configuration;

        public RoutingBehavior(RoutingConfiguration routingConfiguration)
        {
            if (routingConfiguration == null)
            {
                throw FxTrace.Exception.ArgumentNull("routingConfiguration");
            }

            this.configuration = routingConfiguration;
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            RoutingExtension routingExtension = new RoutingExtension(this.configuration);
            serviceHostBase.Extensions.Add(routingExtension);
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (!endpointDispatcher.IsSystemEndpoint &&
                            RoutingUtilities.IsRoutingServiceNamespace(endpointDispatcher.ContractNamespace))
                        {
                            DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                            //Since we use PerSession instancing this concurrency only applies to messages
                            //in the same session, also needed to maintain order.
                            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Single;
                            dispatchRuntime.EnsureOrderedDispatch = this.configuration.EnsureOrderedDispatch;
                        }
                    }
                }
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            HashSet<string> endpoints = new HashSet<string>();
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (!endpoint.IsSystemEndpoint && 
                    RoutingUtilities.IsRoutingServiceNamespace(endpoint.Contract.Namespace))
                {
                    endpoint.Behaviors.Add(new RoutingEndpointBehavior(endpoint));
                    endpoints.Add(endpoint.Name);
                }
            }
            EndpointNameMessageFilter.Validate(this.configuration.InternalFilterTable.Keys, endpoints);
        }

        public static Type GetContractForDescription(ContractDescription description)
        {
            if (description == null)
            {
                throw FxTrace.Exception.ArgumentNull("description");
            }

            if (description.CallbackContractType != null)
            {
                return typeof(IDuplexSessionRouter);
            }

            bool allOneWay = true;
            foreach (OperationDescription operation in description.Operations)
            {
                if (!operation.IsOneWay)
                {
                    allOneWay = false;
                    break;
                }
            }

            if (allOneWay)
            {
                if (description.SessionMode == SessionMode.Required)
                {
                    return typeof(ISimplexSessionRouter);
                }
                else
                {
                    return typeof(ISimplexDatagramRouter);
                }
            }
            else
            {
                return typeof(IRequestReplyRouter);
            }
        }

        internal class RoutingEndpointBehavior : IEndpointBehavior, IChannelInitializer, IInputSessionShutdown
        {
            public RoutingEndpointBehavior(ServiceEndpoint endpoint)
            {
                this.Endpoint = endpoint;
                this.EndpointName = endpoint.Name;
            }

            internal ChannelDispatcher ChannelDispatcher
            {
                get;
                private set;
            }

            internal ServiceEndpoint Endpoint
            {
                get;
                private set;
            }

            internal string EndpointName
            {
                get;
                private set;
            }

            internal bool ImpersonationRequired
            {
                get;
                private set;
            }

            internal bool ReceiveContextEnabled
            {
                get;
                private set;
            }

            internal bool TransactedReceiveEnabled
            {
                get;
                private set;
            }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
                //Turn on ReceiveContext here if supported
                IReceiveContextSettings receiveContextSettings = endpoint.Binding.GetProperty<IReceiveContextSettings>(bindingParameters);
                if (receiveContextSettings != null)
                {
                    receiveContextSettings.Enabled = true;
                }
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                this.ChannelDispatcher = endpointDispatcher.ChannelDispatcher;
                this.ChannelDispatcher.ChannelInitializers.Add(this);
                endpointDispatcher.DispatchRuntime.InputSessionShutdownHandlers.Add(this);
                endpointDispatcher.DispatchRuntime.AutomaticInputSessionShutdown = false;

                if (endpointDispatcher.DispatchRuntime.ImpersonateCallerForAllOperations)
                {
                    this.ImpersonationRequired = true;
                }
                else if (AspNetEnvironment.Current.AspNetCompatibilityEnabled)
                {
                    this.ImpersonationRequired = true;
                }

                BindingParameterCollection bindingParams = new BindingParameterCollection();
                if (RoutingUtilities.IsTransactedReceive(endpoint.Binding, bindingParams))
                {
                    foreach (OperationDescription operation in endpoint.Contract.Operations)
                    {
                        if (operation.Behaviors.Find<TransactedReceiveOperationBehavior>() == null)
                        {
                            operation.Behaviors.Add(new TransactedReceiveOperationBehavior());
                        }
                    }
                    this.ChannelDispatcher.IsTransactedReceive = true;
                    endpointDispatcher.DispatchRuntime.TransactionAutoCompleteOnSessionClose = true;
                    this.TransactedReceiveEnabled = true;
                }

                IReceiveContextSettings rcSettings = endpoint.Binding.GetProperty<IReceiveContextSettings>(bindingParams);
                if (rcSettings != null && rcSettings.Enabled)
                {                   
                    foreach (OperationDescription operation in endpoint.Contract.Operations)
                    {
                        ReceiveContextEnabledAttribute rcEnabled = new ReceiveContextEnabledAttribute();
                        rcEnabled.ManualControl = true;
                        operation.Behaviors.Add(rcEnabled);
                    }
                    this.ReceiveContextEnabled = true;

                    //Switch TransactedReceive off, because we don't want the Dispatcher creating any Transaction
                    endpointDispatcher.ChannelDispatcher.IsTransactedReceive = false;
                    endpointDispatcher.DispatchRuntime.TransactionAutoCompleteOnSessionClose = false;
                }
            }

            public void Validate(ServiceEndpoint endpoint)
            {
            }

            void IChannelInitializer.Initialize(IClientChannel channel)
            {
                RoutingChannelExtension channelState = RoutingChannelExtension.Create(this);
                channel.Extensions.Add(channelState);
            }

            void IInputSessionShutdown.ChannelFaulted(IDuplexContextChannel channel)
            {
                RoutingChannelExtension channelExtension = channel.Extensions.Find<RoutingChannelExtension>();
                if (channelExtension != null)
                {
                    channelExtension.Fault(new CommunicationObjectFaultedException());
                }
                else
                {
                    RoutingUtilities.Abort(channel, channel.LocalAddress);
                }
            }

            void IInputSessionShutdown.DoneReceiving(IDuplexContextChannel channel)
            {
                RoutingChannelExtension channelExtension = channel.Extensions.Find<RoutingChannelExtension>();
                channelExtension.DoneReceiving(this.Endpoint.Binding.CloseTimeout);
            }
        }

        class TransactedReceiveOperationBehavior : IOperationBehavior
        {
            public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
            {
            }

            public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
            {
                if (operationDescription.Behaviors.Find<ReceiveContextEnabledAttribute>() == null)
                {
                    dispatchOperation.TransactionRequired = true;
                }

                ContractDescription contract = operationDescription.DeclaringContract;
                if (dispatchOperation.IsOneWay && contract.SessionMode == SessionMode.Required)
                {
                    dispatchOperation.Parent.ConcurrencyMode = ConcurrencyMode.Single;
                    dispatchOperation.Parent.ReleaseServiceInstanceOnTransactionComplete = false;
                    dispatchOperation.TransactionAutoComplete = false;
                }
            }

            public void Validate(OperationDescription operationDescription)
            {
            }
        }
    }
}
