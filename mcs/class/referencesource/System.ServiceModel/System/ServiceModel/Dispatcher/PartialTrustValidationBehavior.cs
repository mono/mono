//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Xml;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.MsmqIntegration;
    using System.Runtime;

    class PartialTrustValidationBehavior : IServiceBehavior, IEndpointBehavior
    {
        static PartialTrustValidationBehavior instance = null;

        internal static PartialTrustValidationBehavior Instance
        {
            get
            {
                // no need to synchronize -- it's ok if two are created
                if (instance == null)
                {
                    instance = new PartialTrustValidationBehavior();
                }
                return instance;
            }
        }

        void ValidateEndpoint(ServiceEndpoint endpoint)
        {
            Binding binding = endpoint.Binding;
            if (binding != null)
            {
                new BindingValidator(endpoint.Binding).Validate();
            }
        }

        #region IEndpointBehavior Members

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            ValidateEndpoint(endpoint);
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

        #endregion

        #region IServiceBehavior Members

        public void Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                if (endpoint != null)
                {
                    ValidateEndpoint(endpoint);
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

        #endregion

        struct BindingValidator
        {
            static Type[] unsupportedBindings = new Type[]
            {
                typeof(NetNamedPipeBinding),
                typeof(WSDualHttpBinding),
                typeof(WS2007FederationHttpBinding),
                typeof(WSFederationHttpBinding),
                typeof(NetMsmqBinding),
#pragma warning disable 0618
                typeof(NetPeerTcpBinding),
#pragma warning restore 0618
                typeof(MsmqIntegrationBinding),
            };

            static Type[] unsupportedBindingElements = new Type[]
            {
                typeof(AsymmetricSecurityBindingElement),
                typeof(CompositeDuplexBindingElement),
                typeof(MsmqTransportBindingElement),
                typeof(NamedPipeTransportBindingElement),
                typeof(OneWayBindingElement),
#pragma warning disable 0618                
                typeof(PeerCustomResolverBindingElement),
                typeof(PeerTransportBindingElement),
                typeof(PnrpPeerResolverBindingElement),
#pragma warning restore 0618                
                typeof(ReliableSessionBindingElement),
                typeof(SymmetricSecurityBindingElement),
                typeof(TransportSecurityBindingElement),
                typeof(MtomMessageEncodingBindingElement),
            }; 
            
            Binding binding;
            internal BindingValidator(Binding binding)
            {
                this.binding = binding;
            }

            internal void Validate()
            {
                Fx.Assert(binding != null, "BindingValidator was not constructed with a valid Binding instance");

                Type bindingType = binding.GetType();
                if (IsUnsupportedBindingType(bindingType))
                {
                    UnsupportedSecurityCheck(SR.FullTrustOnlyBindingSecurityCheck1, bindingType);
                }

                // special-case error message for WSHttpBindings
                bool isWSHttpBinding = typeof(WSHttpBinding).IsAssignableFrom(bindingType);
                string sr = isWSHttpBinding ? SR.FullTrustOnlyBindingElementSecurityCheckWSHttpBinding1 : SR.FullTrustOnlyBindingElementSecurityCheck1;

                BindingElementCollection elements = binding.CreateBindingElements();
                foreach (BindingElement element in elements)
                {
                    Type bindingElementType = element.GetType();
                    if (element != null && IsUnsupportedBindingElementType(bindingElementType))
                    {
                        UnsupportedSecurityCheck(sr, bindingElementType);
                    }
                }
            }

            bool IsUnsupportedBindingType(Type bindingType)
            {
                for (int i = 0; i < unsupportedBindings.Length; i++)
                {
                    if (unsupportedBindings[i] == bindingType)
                        return true;
                }
                return false;
            }

            bool IsUnsupportedBindingElementType(Type bindingElementType)
            {
                for (int i = 0; i < unsupportedBindingElements.Length; i++)
                {
                    if (unsupportedBindingElements[i] == bindingElementType)
                        return true;
                }
                return false;
            }
            
            static readonly PermissionSet fullTrust = new PermissionSet(PermissionState.Unrestricted);
            void UnsupportedSecurityCheck(string resource, Type type)
            {
                try
                {
                    fullTrust.Demand();
                }
                catch (SecurityException)
                {
                    throw new InvalidOperationException(SR.GetString(resource, binding.Name, type));
                }
            }

        }
    }

}
