//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Web.Configuration;
    using System.Web.Hosting;
    
    /// <summary>
    /// ServiceHost for registering SecurityTokenService. The ServiceHost will have multiple endpoints
    /// registered based on the number of listeners registered in the config.
    /// </summary>
    public class WSTrustServiceHost : ServiceHost
    {
        WSTrustServiceContract _serviceContract;        

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustServiceHost"/>
        /// </summary>
        /// <param name="securityTokenServiceConfiguration">SecurityTokenServiceConfiguration instance used to initialize this ServiceHost.</param>
        /// <param name="baseAddresses">BaseAddress collection for the service host</param>
        /// <remarks>
        /// A default WSTrustServiceContract is instantiated using the SecurityTokenServiceConfiguration instance.
        /// The SecurityTokenServiceConfiguration instance is used for one-time initialization of the ServiceHost and
        /// setting properties on the configuration instance after the host is initialization may not result in
        /// behavioral changes.
        /// </remarks>
        public WSTrustServiceHost(SecurityTokenServiceConfiguration securityTokenServiceConfiguration, params Uri[] baseAddresses)
            : this(new WSTrustServiceContract(securityTokenServiceConfiguration), baseAddresses)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustServiceHost"/>
        /// </summary>
        /// <param name="serviceContract">ServiceContract implementation to use.</param>
        /// <param name="baseAddresses">BaseAddress collection for the service host</param>
        /// <exception cref="ArgumentNullException">One of the input argument is null.</exception>
        public WSTrustServiceHost(WSTrustServiceContract serviceContract, params Uri[] baseAddresses)
            : base(serviceContract, baseAddresses)
        {
            if (serviceContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceContract");
            }

            if (serviceContract.SecurityTokenServiceConfiguration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceContract.SecurityTokenServiceConfiguration");
            }

            _serviceContract = serviceContract;
        }

        /// <summary>
        /// Gets the WSTrustServiceContract associated with this instance.
        /// </summary>
        public WSTrustServiceContract ServiceContract
        {
            get
            {
                return _serviceContract;
            }
        }

        /// <summary>
        /// Gets the SecurityTokenServiceConfiguration
        /// </summary>
        public SecurityTokenServiceConfiguration SecurityTokenServiceConfiguration
        {
            get
            {
                return _serviceContract.SecurityTokenServiceConfiguration;
            }
        }

        /// <summary>
        /// Configures metadata (WSDL) for the service host. The method loops through the 
        /// base addresses, and adds mex endpoints for http, https, net.tcp and net.pipe
        /// addresses, only when no mex endpoints have been previously added by the user. 
        /// For http and htps addresses, HTTP and HTTPS "Get" mechanism for WSDL retrieval 
        /// is enabled.
        /// </summary>
        protected virtual void ConfigureMetadata()
        {
            if (this.BaseAddresses == null || this.BaseAddresses.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3140));
            }

            // Check if a ServiceMetadataBehavior is added.
            ServiceMetadataBehavior metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                Description.Behaviors.Add(metadataBehavior);
            }

            // Check if an Mex endpoint has alread been added by user. This can be enabled through 
            // configuration.
            bool isMexEndpointAlreadyAdded = (Description.Endpoints.Find(typeof(IMetadataExchange)) != null);

            Binding mexBinding = null;
            foreach (Uri baseAddress in this.BaseAddresses)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(baseAddress.Scheme, Uri.UriSchemeHttp))
                {
                    metadataBehavior.HttpGetEnabled = true;
                    mexBinding = MetadataExchangeBindings.CreateMexHttpBinding();
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(baseAddress.Scheme, Uri.UriSchemeHttps))
                {
                    metadataBehavior.HttpsGetEnabled = true;
                    mexBinding = MetadataExchangeBindings.CreateMexHttpsBinding();
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(baseAddress.Scheme, Uri.UriSchemeNetTcp))
                {
                    mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(baseAddress.Scheme, Uri.UriSchemeNetPipe))
                {
                    mexBinding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
                }

                if (!isMexEndpointAlreadyAdded && (mexBinding != null))
                {
                    AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, mexBinding, "mex");
                }

                mexBinding = null;
            }
        }

        /// <summary>
        ///  Loads the service description information from the configuration file and
        ///  applies it to the runtime being constructed.
        /// </summary>
        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();

            //
            // Configure metadata endpoints
            //
            WSTrustServiceContract serviceContract = (WSTrustServiceContract)base.SingletonInstance;

            if (!serviceContract.SecurityTokenServiceConfiguration.DisableWsdl)
            {
                ConfigureMetadata();
            }
        }

        /// <summary>
        /// Override of the base class method. Configures the <see cref="ServiceConfiguration"/> on the
        /// service host and then invokes the base implementation.
        /// </summary>
        protected override void InitializeRuntime()
        {
            if (Description.Endpoints.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3097)));
            }

            UpdateServiceConfiguration();
            base.InitializeRuntime();
        }

        /// <summary>
        /// Overrides the <see cref="IdentityConfiguration"/> on the ServiceHost Credentials
        /// with the SecurityTokenServiceConfiguration.
        /// </summary>
        protected virtual void UpdateServiceConfiguration()
        {
            Credentials.IdentityConfiguration = _serviceContract.SecurityTokenServiceConfiguration;
            Credentials.UseIdentityConfiguration = true;
        }
    }
}
