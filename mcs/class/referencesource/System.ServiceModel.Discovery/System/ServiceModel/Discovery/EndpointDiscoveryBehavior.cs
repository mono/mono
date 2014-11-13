//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml.Linq;
    using System.Xml;

    [Fx.Tag.XamlVisible(false)]
    public class EndpointDiscoveryBehavior : IEndpointBehavior
    {
        ScopeCollection scopes;
        ContractTypeNameCollection contractTypeNames;
        NonNullItemCollection<XElement> extensions;
        bool enabled;

        public EndpointDiscoveryBehavior()
        {
            this.enabled = true;
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                this.enabled = value;
            }
        }

        public Collection<XmlQualifiedName> ContractTypeNames
        {
            get
            {
                if (this.contractTypeNames == null)
                {
                    this.contractTypeNames = new ContractTypeNameCollection();
                }

                return this.contractTypeNames;
            }
        }

        public Collection<Uri> Scopes
        {
            get
            {
                if (this.scopes == null)
                {
                    this.scopes = new ScopeCollection();
                }
                return this.scopes;
            }
        }

        public Collection<XElement> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new NonNullItemCollection<XElement>();
                }

                return this.extensions;
            }
        }

        internal Collection<XmlQualifiedName> InternalContractTypeNames
        {
            get
            {
                return this.contractTypeNames;
            }
        }

        internal Collection<Uri> InternalScopes
        {
            get
            {
                return this.scopes;
            }
        }

        internal Collection<XElement> InternalExtensions
        {
            get
            {
                return this.extensions;
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes)]
        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
