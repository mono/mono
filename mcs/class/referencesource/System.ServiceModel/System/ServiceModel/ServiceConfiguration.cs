// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Configuration;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    /// <summary>
    /// Facade over ServiceHost to use in programmatic configuration.
    /// </summary>
    public class ServiceConfiguration
    {
        private ServiceHost host;

        // ServiceConfiguration is a facade over a ServiceHost
        internal ServiceConfiguration(ServiceHost host)
        {
            CheckArgument(host, "host");
            this.host = host;
        }

        /// <summary>
        /// Gets a complete, in-memory description of the service being configured, including all the endpoints for the service and specifications for their respective addresses, bindings, contracts and behaviors.
        /// </summary>
        public ServiceDescription Description
        {
            get
            {
                return this.host.Description;
            }
        }

        /// <summary>
        /// Gets the ServiceAuthenticationBehavior currently enabled on the service's Description.
        /// </summary>
        public ServiceAuthenticationBehavior Authentication
        {
            get
            {
                return this.host.Authentication;
            }
        }

        /// <summary>
        /// Gets the ServiceAuthorizationBehavior currently enabled on the service's Description.
        /// </summary>
        public ServiceAuthorizationBehavior Authorization
        {
            get
            {
                return this.host.Authorization;
            }
        }

        /// <summary>
        /// Gets the ServiceCredentials currently enabled on the service's Description.
        /// </summary>
        public ServiceCredentials Credentials
        {
            get
            {
                return this.host.Credentials;
            }
        }

        /// <summary>
        /// Gets the base addresses for the service as specified by the underlying ServiceHost.
        /// </summary>
        public ReadOnlyCollection<Uri> BaseAddresses
        {
            get
            {
                return this.host.BaseAddresses;
            }
        }

        /// <summary>
        /// Gets or sets OpenTimeout for the underlying ServiceHost
        /// </summary>
        public TimeSpan OpenTimeout
        {
            get
            {
                return this.host.OpenTimeout;
            }

            set
            {
                this.host.OpenTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets CloseTimeout for the underlying ServiceHost
        /// </summary>
        public TimeSpan CloseTimeout
        {
            get
            {
                return this.host.CloseTimeout;
            }

            set
            {
                this.host.CloseTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use token handlers
        /// </summary>
        public bool UseIdentityConfiguration
        {
            get
            {
                return this.Credentials.UseIdentityConfiguration;
            }

            set
            {
                this.Credentials.UseIdentityConfiguration = value;
            }
        }

        /// <summary>
        /// Gets or sets IdentityConfiguration for the underlying ServiceHost
        /// </summary>
        public IdentityConfiguration IdentityConfiguration
        {
            get
            {
                return this.Credentials.IdentityConfiguration;
            }
            
            set
            {
                this.Credentials.IdentityConfiguration = value;
            }
        }

        /// <summary>
        /// Validate a service endpoint and add it to Description
        /// </summary>
        /// <param name="endpoint">endpoint to add</param>
        public void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            CheckArgument(endpoint, "endpoint");

            // Do some other checks to match ServiceHostBase.AddServiceEndpoint
            if ((this.host.State != CommunicationState.Created) && (this.host.State != CommunicationState.Opening))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("SFxServiceHostBaseCannotAddEndpointAfterOpen")));
            }

            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("SFxServiceHostBaseCannotAddEndpointWithoutDescription")));
            }

            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("SFxEndpointAddressNotSpecified"));
            }

            if (endpoint.Contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("SFxEndpointContractNotSpecified"));
            }

            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("SFxEndpointBindingNotSpecified"));
            }

            if (!endpoint.IsSystemEndpoint || (endpoint.Contract.ContractType == typeof(IMetadataExchange)))
            {
                // Throw if contract is not valid for this service
                //   i.e. if contract is not implemented by service, unless endpoint is a standard endpoint 
                //   note: (metadata endpoints require metadata behavior to implement IMetadataExchange even though it's a standard endpoint)            
                IContractResolver resolver = this.host.GetContractResolver(this.host.ImplementedContracts);
                ConfigLoader configLoader = new ConfigLoader(resolver);
                configLoader.LookupContract(endpoint.Contract.ConfigurationName, this.Description.Name); // throws on failure
            }

            this.Description.Endpoints.Add(endpoint);
        }

        /// <summary>
        /// Create a new service endpoint and add it to Description
        /// </summary>
        /// <param name="contractType">interface annotated with [ServiceContract]</param>
        /// <param name="binding">protocol to use for communication</param>
        /// <param name="address">absolute address for service, or address relative to base address for supplied binding</param>
        /// <returns>The endpoint which was created</returns>
        public ServiceEndpoint AddServiceEndpoint(Type contractType, Binding binding, string address)
        {
            CheckArgument(address, "address");
            return this.AddServiceEndpoint(contractType, binding, new Uri(address, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Create a new service endpoint and add it to Description
        /// </summary>
        /// <param name="contractType">interface annotated with [ServiceContract]</param>
        /// <param name="binding">protocol to use for communication</param>
        /// <param name="address">absolute address for service, or address relative to base address for supplied binding</param>
        /// <returns>The endpoint which was created</returns>
        public ServiceEndpoint AddServiceEndpoint(Type contractType, Binding binding, Uri address)
        {
            CheckArgument(contractType, "contractType");
            CheckArgument(binding, "binding");
            CheckArgument(address, "address");

            ContractDescription contract = this.host.ImplementedContracts == null
                ? null
                : this.host.ImplementedContracts.Values.FirstOrDefault(implementedContract => implementedContract.ContractType == contractType);
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("contractType", SR.GetString(SR.SFxMethodNotSupportedByType2, this.host.Description.ServiceType, contractType));
            }

            ServiceEndpoint endpoint = new ServiceEndpoint(contract, binding, new EndpointAddress(ServiceHost.MakeAbsoluteUri(address, binding, this.host.InternalBaseAddresses)));
            this.AddServiceEndpoint(endpoint);
            return endpoint;
        }

        /// <summary>
        /// Create a new service endpoint and add it to Description
        /// </summary>
        /// <param name="contractType">interface annotated with [ServiceContract]</param>
        /// <param name="binding">protocol to use for communication</param>
        /// <param name="address">absolute logical address for service, or address relative to base address for supplied binding</param>
        /// <param name="listenUri">absolute physical address for service, or address relative to base address for supplied binding</param>
        /// <returns>The endpoint which was created</returns>
        public ServiceEndpoint AddServiceEndpoint(Type contractType, Binding binding, string address, Uri listenUri)
        {
            CheckArgument(listenUri, "listenUri");

            ServiceEndpoint endpoint = this.AddServiceEndpoint(contractType, binding, address);
            this.SetListenUri(endpoint, binding, listenUri);
            return endpoint;
        }

        /// <summary>
        /// Create a new service endpoint and add it to Description
        /// </summary>
        /// <param name="contractType">interface annotated with [ServiceContract]</param>
        /// <param name="binding">protocol to use for communication</param>
        /// <param name="address">absolute logical address for service, or address relative to base address for supplied binding</param>
        /// <param name="listenUri">absolute physical address for service, or address relative to base address for supplied binding</param>
        /// <returns>The endpoint which was created</returns>
        public ServiceEndpoint AddServiceEndpoint(Type contractType, Binding binding, Uri address, Uri listenUri)
        {
            CheckArgument(listenUri, "listenUri");

            ServiceEndpoint endpoint = this.AddServiceEndpoint(contractType, binding, address);
            this.SetListenUri(endpoint, binding, listenUri);
            return endpoint;
        }

        /// <summary>
        /// Convenience method to compute and set endpoint's address
        /// </summary>
        /// <param name="endpoint">endpoint to set</param>
        /// <param name="relativeAddress">address relative to the ServiceHost's base address, if any, for endpoint's current binding</param>
        public void SetEndpointAddress(ServiceEndpoint endpoint, string relativeAddress)
        {
            CheckArgument(endpoint, "endpoint");
            CheckArgument(relativeAddress, "relativeAddress");

            this.host.SetEndpointAddress(endpoint, relativeAddress);
        }

        /// <summary>
        /// Automatically add endpoints for all of a service's enabled contracts, for all of its enabled base addresses that match the specified binding.
        /// </summary>
        /// <param name="protocol">Binding to add endpoints for</param>
        /// <returns>Endpoints created</returns>
        public Collection<ServiceEndpoint> EnableProtocol(Binding protocol)
        {
            CheckArgument(protocol, "protocol");
            Collection<ServiceEndpoint> generatedEndpoints = new Collection<ServiceEndpoint>();

            if (this.host.ImplementedContracts != null)
            {
                // don't generate endpoints for contracts that serve as the base type for other reflected contracts            
                IEnumerable<ContractDescription> contracts = this.host.ImplementedContracts.Values;
                IEnumerable<ContractDescription> mostSpecificContracts = contracts.Where(contract
                    => contracts.All(otherContract
                        => object.ReferenceEquals(contract, otherContract)
                            || !contract.ContractType.IsAssignableFrom(otherContract.ContractType)));

                foreach (var uri in this.host.BaseAddresses)
                {
                    if (uri.Scheme.Equals(protocol.Scheme))
                    {
                        foreach (ContractDescription contract in mostSpecificContracts)
                        {
                            ServiceEndpoint endpoint = new ServiceEndpoint(contract, protocol, new EndpointAddress(uri));
                            this.AddServiceEndpoint(endpoint);
                            generatedEndpoints.Add(endpoint);
                        }
                    }
                }
            }

            return generatedEndpoints;
        }

        /// <summary>
        /// Load endpoints and behaviors into service Description from current AppDomain's configuration
        /// </summary>
        public void LoadFromConfiguration()
        {
            this.host.LoadFromConfiguration();
        }

        /// <summary>
        /// Load endpoints and behaviors into service Description from supplied configuration
        /// </summary>
        /// <param name="configuration">configuration to load from</param>
        public void LoadFromConfiguration(System.Configuration.Configuration configuration)
        {
            CheckArgument(configuration, "configuration");
            this.host.LoadFromConfiguration(configuration);
        }

        private static void CheckArgument<T>(T argument, string argumentName)
        {
            if (argument == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(argumentName);
            }
        }

        private void SetListenUri(ServiceEndpoint endpoint, Binding binding, Uri listenUri)
        {
            endpoint.UnresolvedListenUri = listenUri;
            endpoint.ListenUri = ServiceHost.MakeAbsoluteUri(listenUri, binding, this.host.InternalBaseAddresses);
        }
    }
}
