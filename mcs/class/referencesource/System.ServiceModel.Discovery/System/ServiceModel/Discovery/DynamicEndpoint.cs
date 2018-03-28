//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Runtime;
    using System.Xml;        

    [Fx.Tag.XamlVisible(false)]
    public class DynamicEndpoint : ServiceEndpoint
    {
        DiscoveryClientBindingElement discoveryClientBindingElement;

        internal DynamicEndpoint(ContractDescription contract)
            : base(contract, null, DiscoveryClientBindingElement.DiscoveryEndpointAddress)
        {
            this.discoveryClientBindingElement = new DiscoveryClientBindingElement();
        }

        public DynamicEndpoint(ContractDescription contract, Binding binding)
            : base(contract, binding, DiscoveryClientBindingElement.DiscoveryEndpointAddress)
        {
            if (binding == null)
            {
                throw FxTrace.Exception.ArgumentNull("binding");
            }

            this.discoveryClientBindingElement = new DiscoveryClientBindingElement();

            if (this.ValidateAndInsertDiscoveryClientBindingElement(binding))
            {
                this.FindCriteria.ContractTypeNames.Add(
                            new XmlQualifiedName(contract.Name, contract.Namespace));    
            }
            else
            {                
                throw FxTrace.Exception.Argument(
                    "binding",
                    SR.DiscoveryClientBindingElementPresentInDynamicEndpoint);
            }
        }

        public DiscoveryEndpointProvider DiscoveryEndpointProvider
        {
            get
            {
                return this.discoveryClientBindingElement.DiscoveryEndpointProvider;
            }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.discoveryClientBindingElement.DiscoveryEndpointProvider = value;
            }
        }

        public FindCriteria FindCriteria
        {
            get
            {
                return this.discoveryClientBindingElement.FindCriteria;
            }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.discoveryClientBindingElement.FindCriteria = value;
            }
        }

        internal bool ValidateAndInsertDiscoveryClientBindingElement(Binding binding)
        {            
            CustomBinding customBinding = new CustomBinding(binding);            

            if (customBinding.Elements.Find<DiscoveryClientBindingElement>() == null)
            {
                customBinding.Elements.Insert(0, this.discoveryClientBindingElement);
                this.Binding = customBinding;
                return true;
            }
            else
            {
                return false;
            }                      
        }        
    }
}
