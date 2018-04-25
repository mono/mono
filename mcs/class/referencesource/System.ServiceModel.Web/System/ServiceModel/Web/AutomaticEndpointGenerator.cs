// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    internal abstract class AutomaticEndpointGenerator
    {
        private IDictionary<string, ContractDescription> implementedContracts;
        private string multipleContractsErrorMessage;
        private string noContractErrorMessage;
        private string standardEndpointKind;
        private Type singleImplementedContract;

        protected AutomaticEndpointGenerator(IDictionary<string, ContractDescription> implementedContracts, string multipleContractsErrorMessage, string noContractErrorMessage, string standardEndpointKind)
        {
            Fx.Assert(implementedContracts != null, "The 'implementedContracts' parameter should not be null.");
            Fx.Assert(multipleContractsErrorMessage != null, "The 'multipleContractsErrorMessage' parameter should not be null.");
            Fx.Assert(noContractErrorMessage != null, "The 'noContractErrorMessage' parameter should not be null.");
            Fx.Assert(standardEndpointKind != null, "The 'standardEndpointKind' parameter should not be null.");

            this.implementedContracts = implementedContracts;
            this.multipleContractsErrorMessage = multipleContractsErrorMessage;
            this.noContractErrorMessage = noContractErrorMessage;
            this.standardEndpointKind = standardEndpointKind;
        }

        protected abstract string BindingCollectionElementName { get; }

        public ServiceEndpoint GenerateServiceEndpoint(ServiceHostBase serviceHost, Uri baseAddress)
        {
            Fx.Assert(serviceHost != null, "The 'serviceHost' parameter should not be null.");
            Fx.Assert(baseAddress != null, "The 'baseAddress' parameter should not be null.");

            AuthenticationSchemes supportedSchemes = GetAuthenticationSchemes(baseAddress);
            Type contractType = this.GetSingleImplementedContract();
            ConfigLoader configLoader = new ConfigLoader(serviceHost.GetContractResolver(this.implementedContracts));
            ServiceEndpointElement serviceEndpointElement = new ServiceEndpointElement();
            
            serviceEndpointElement.Contract = contractType.FullName;
            this.SetBindingConfiguration(baseAddress.Scheme, serviceEndpointElement);
            serviceEndpointElement.Kind = this.standardEndpointKind;

            ServiceEndpoint serviceEndpoint = configLoader.LookupEndpoint(serviceEndpointElement, null, serviceHost, serviceHost.Description, true);
            this.ConfigureBinding(serviceEndpoint.Binding, baseAddress.Scheme, supportedSchemes, AspNetEnvironment.Enabled);

            // Setting the Endpoint address and listenUri now that we've set the binding security
            ConfigLoader.ConfigureEndpointAddress(serviceEndpointElement, serviceHost, serviceEndpoint);
            ConfigLoader.ConfigureEndpointListenUri(serviceEndpointElement, serviceHost, serviceEndpoint);

            return serviceEndpoint;
        }

        protected abstract void ConfigureBinding(Binding binding, string uriScheme, AuthenticationSchemes supportedAuthenticationSchemes, bool hostedEnvironment);

        private static AuthenticationSchemes GetAuthenticationSchemes(Uri baseAddress)
        {
            AuthenticationSchemes supportedSchemes = AspNetEnvironment.Current.GetAuthenticationSchemes(baseAddress);

            if (AspNetEnvironment.Current.IsSimpleApplicationHost)
            {
                // Cassini always reports the auth scheme as anonymous or Ntlm. Map this to Ntlm, except when forms auth
                // is requested
                if (supportedSchemes == (AuthenticationSchemes.Anonymous | AuthenticationSchemes.Ntlm))
                {
                    if (AspNetEnvironment.Current.IsWindowsAuthenticationConfigured())
                    {
                        supportedSchemes = AuthenticationSchemes.Ntlm;
                    }
                    else
                    {
                        supportedSchemes = AuthenticationSchemes.Anonymous;
                    }
                }
            }

            return supportedSchemes;
        }

        private Type GetSingleImplementedContract()
        {
            if (this.singleImplementedContract == null)
            {
                Fx.Assert(this.implementedContracts != null, "The 'implementedContracts' field should not be null.");
                Fx.Assert(this.multipleContractsErrorMessage != null, "The 'multipleContractsErrorMessage' field should not be null.");
                Fx.Assert(this.noContractErrorMessage != null, "The 'noContractErrorMessage' field should not be null.");

                if (this.implementedContracts.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(this.multipleContractsErrorMessage));
                }
                else if (this.implementedContracts.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(this.noContractErrorMessage));
                }

                foreach (ContractDescription contract in this.implementedContracts.Values)
                {
                    this.singleImplementedContract = contract.ContractType;
                    break;
                }
            }

            return this.singleImplementedContract;
        }

        private void SetBindingConfiguration(string uriScheme, ServiceEndpointElement serviceEndpointElement)
        {
            Fx.Assert(uriScheme != null, "The 'uriScheme' parameter should not be null.");
            Fx.Assert(serviceEndpointElement != null, "The 'serviceEndpointElement' parameter should not be null.");
            Fx.Assert(this.BindingCollectionElementName != null, "The 'this.BindingCollectionElementName' property should not be null.");

            ProtocolMappingItem protocolMappingItem = ConfigLoader.LookupProtocolMapping(uriScheme);
            if (protocolMappingItem != null &&
                string.Equals(protocolMappingItem.Binding, this.BindingCollectionElementName, StringComparison.Ordinal))
            {
                serviceEndpointElement.BindingConfiguration = protocolMappingItem.BindingConfiguration;
            }
        }
    }
}
