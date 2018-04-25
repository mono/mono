//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using WsdlNS = System.Web.Services.Description;

    public class WsdlEndpointConversionContext
    {

        readonly ServiceEndpoint endpoint;
        readonly WsdlNS.Binding wsdlBinding;
        readonly WsdlNS.Port wsdlPort;
        readonly WsdlContractConversionContext contractContext;

        readonly Dictionary<OperationDescription, WsdlNS.OperationBinding> wsdlOperationBindings;
        readonly Dictionary<WsdlNS.OperationBinding, OperationDescription> operationDescriptionBindings;
        readonly Dictionary<MessageDescription, WsdlNS.MessageBinding> wsdlMessageBindings;
        readonly Dictionary<FaultDescription, WsdlNS.FaultBinding> wsdlFaultBindings;
        readonly Dictionary<WsdlNS.MessageBinding, MessageDescription> messageDescriptionBindings;
        readonly Dictionary<WsdlNS.FaultBinding, FaultDescription> faultDescriptionBindings;
        
        internal WsdlEndpointConversionContext(WsdlContractConversionContext contractContext, ServiceEndpoint endpoint, WsdlNS.Binding wsdlBinding, WsdlNS.Port wsdlport)
        {

            this.endpoint = endpoint;
            this.wsdlBinding = wsdlBinding;
            this.wsdlPort = wsdlport;
            this.contractContext = contractContext;

            this.wsdlOperationBindings = new Dictionary<OperationDescription, WsdlNS.OperationBinding>();
            this.operationDescriptionBindings = new Dictionary<WsdlNS.OperationBinding, OperationDescription>();
            this.wsdlMessageBindings = new Dictionary<MessageDescription, WsdlNS.MessageBinding>();
            this.messageDescriptionBindings = new Dictionary<WsdlNS.MessageBinding, MessageDescription>();
            this.wsdlFaultBindings = new Dictionary<FaultDescription, WsdlNS.FaultBinding>();
            this.faultDescriptionBindings = new Dictionary<WsdlNS.FaultBinding, FaultDescription>();
        }

        internal WsdlEndpointConversionContext(WsdlEndpointConversionContext bindingContext, ServiceEndpoint endpoint, WsdlNS.Port wsdlport)
        {

            this.endpoint = endpoint;
            this.wsdlBinding = bindingContext.WsdlBinding;
            this.wsdlPort = wsdlport;
            this.contractContext = bindingContext.contractContext;

            this.wsdlOperationBindings = bindingContext.wsdlOperationBindings;
            this.operationDescriptionBindings = bindingContext.operationDescriptionBindings;
            this.wsdlMessageBindings = bindingContext.wsdlMessageBindings;
            this.messageDescriptionBindings = bindingContext.messageDescriptionBindings;
            this.wsdlFaultBindings = bindingContext.wsdlFaultBindings;
            this.faultDescriptionBindings = bindingContext.faultDescriptionBindings;
        }

        internal IEnumerable<IWsdlExportExtension> ExportExtensions
        {
            get
            {
                foreach (IWsdlExportExtension extension in endpoint.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (IWsdlExportExtension extension in endpoint.Binding.CreateBindingElements().FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (IWsdlExportExtension extension in endpoint.Contract.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return extension;
                }

                foreach (OperationDescription operation in endpoint.Contract.Operations)
                {
                    if (!WsdlExporter.OperationIsExportable(operation))
                    {
                        continue;
                    }

                    // In 3.0SP1, the DCSOB and XSOB were moved from before to after the custom behaviors.  For
                    // IWsdlExportExtension compat, run them in the pre-SP1 order.
                    // TEF QFE 367607
                    Collection<IWsdlExportExtension> extensions = operation.Behaviors.FindAll<IWsdlExportExtension>();
                    for (int i = 0; i < extensions.Count;)
                    {
                        if (WsdlExporter.IsBuiltInOperationBehavior(extensions[i]))
                        {
                            yield return extensions[i];
                            extensions.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    foreach (IWsdlExportExtension extension in extensions)
                    {
                        yield return extension;
                    }
                }
            }
        }

        public ServiceEndpoint Endpoint { get { return endpoint; } }
        public WsdlNS.Binding WsdlBinding { get { return wsdlBinding; } }
        public WsdlNS.Port WsdlPort { get { return wsdlPort; } }
        public WsdlContractConversionContext ContractConversionContext { get { return contractContext; } }

        public WsdlNS.OperationBinding GetOperationBinding(OperationDescription operation)
        {
            return this.wsdlOperationBindings[operation];
        }

        public WsdlNS.MessageBinding GetMessageBinding(MessageDescription message)
        {
            return this.wsdlMessageBindings[message];
        }

        public WsdlNS.FaultBinding GetFaultBinding(FaultDescription fault)
        {
            return this.wsdlFaultBindings[fault];
        }

        public OperationDescription GetOperationDescription(WsdlNS.OperationBinding operationBinding)
        {
            return this.operationDescriptionBindings[operationBinding];
        }

        public MessageDescription GetMessageDescription(WsdlNS.MessageBinding messageBinding)
        {
            return this.messageDescriptionBindings[messageBinding];
        }

        public FaultDescription GetFaultDescription(WsdlNS.FaultBinding faultBinding)
        {
            return this.faultDescriptionBindings[faultBinding];
        }

        // --------------------------------------------------------------------------------------------------

        internal void AddOperationBinding(OperationDescription operationDescription, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            this.wsdlOperationBindings.Add(operationDescription, wsdlOperationBinding);
            this.operationDescriptionBindings.Add(wsdlOperationBinding, operationDescription);
        }

        internal void AddMessageBinding(MessageDescription messageDescription, WsdlNS.MessageBinding wsdlMessageBinding)
        {
            this.wsdlMessageBindings.Add(messageDescription, wsdlMessageBinding);
            this.messageDescriptionBindings.Add(wsdlMessageBinding, messageDescription);
        }

        internal void AddFaultBinding(FaultDescription faultDescription, WsdlNS.FaultBinding wsdlFaultBinding)
        {
            this.wsdlFaultBindings.Add(faultDescription, wsdlFaultBinding);
            this.faultDescriptionBindings.Add(wsdlFaultBinding, faultDescription);
        }
    }

}
