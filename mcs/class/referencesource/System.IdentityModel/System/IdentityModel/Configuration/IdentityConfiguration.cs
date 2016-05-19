//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Diagnostics;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Xml;

namespace System.IdentityModel.Configuration
{
    /// <summary>
    /// Defines the collection of configurable properties controlling the behavior of the Windows Identity Foundation.
    /// </summary>
    public class IdentityConfiguration
    {
#pragma warning disable 1591
        public const string DefaultServiceName = ConfigurationStrings.DefaultServiceName;
        public static readonly TimeSpan DefaultMaxClockSkew = new TimeSpan(0, 5, 0);
        internal const string DefaultMaxClockSkewString = "00:05:00";
        public static readonly X509CertificateValidationMode DefaultCertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
        public static readonly Type DefaultIssuerNameRegistryType = typeof(ConfigurationBasedIssuerNameRegistry);
        public static readonly X509RevocationMode DefaultRevocationMode = X509RevocationMode.Online;
        public static readonly StoreLocation DefaultTrustedStoreLocation = StoreLocation.LocalMachine;
#pragma warning restore 1591

        ClaimsAuthenticationManager _claimsAuthenticationManager = new ClaimsAuthenticationManager();
        ClaimsAuthorizationManager _claimsAuthorizationManager = new ClaimsAuthorizationManager();
        bool _isInitialized;
        SecurityTokenHandlerCollectionManager _securityTokenHandlerCollectionManager;
        string _identityConfigurationName = DefaultServiceName;
        TimeSpan _serviceMaxClockSkew = DefaultMaxClockSkew;
        SecurityTokenHandlerConfiguration _serviceHandlerConfiguration;
        X509Certificate2 _serviceCertificate;
        List<X509Certificate2> knownCertificates;

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        public IdentityConfiguration()
        {
            SystemIdentityModelSection section = SystemIdentityModelSection.Current;
            IdentityConfigurationElement element = (section != null) ? section.IdentityConfigurationElements.GetElement(DefaultServiceName) : null;
            LoadConfiguration(element);
        }

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        /// <param name="serviceCertificate">The service certificate to be used in ServiceTokenResolver and SessionSecurityTokenHandler.</param>
        public IdentityConfiguration(X509Certificate2 serviceCertificate)
            : this()
        {
            this.ServiceCertificate = serviceCertificate;
        }

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        /// <param name="loadConfig">Whether or not config should be loaded.</param>
        /// <exception cref="InvalidOperationException">Thrown if loadConfig is set to true but there is no 
        /// &lt;System.IdentityModel> configuration element</exception>
        public IdentityConfiguration(bool loadConfig)
        {
            if (loadConfig)
            {
                SystemIdentityModelSection section = SystemIdentityModelSection.Current;

                if (section == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7027));
                }

                IdentityConfigurationElement element = section.IdentityConfigurationElements.GetElement(DefaultServiceName);
                LoadConfiguration(element);
            }
            else
            {
                LoadConfiguration(null);
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        /// <param name="loadConfig">Whether or not config should be loaded.</param>
        /// <param name="serviceCertificate">The service certificate to be used in ServiceTokenResolver and SessionSecurityTokenHandler.</param>
        /// <exception cref="InvalidOperationException">Thrown if loadConfig is set to true but there is no 
        /// &lt;System.IdentityModel> configuration element</exception>
        public IdentityConfiguration(bool loadConfig, X509Certificate2 serviceCertificate)
            : this(loadConfig)
        {
            this.ServiceCertificate = serviceCertificate;
        }

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        /// <param name="identityConfigurationName">The name of the &lt;service> element from which configuration is to be loaded.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is no &lt;System.IdentityModel> configuration element</exception>
        /// <remarks>If this constructor is called then a System.IdentityModel config section with the provided name must exist.</remarks>        
        public IdentityConfiguration(string identityConfigurationName)
        {
            if (identityConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identityConfigurationName");
            }

            SystemIdentityModelSection section = SystemIdentityModelSection.Current;

            if (section == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7027));
            }

            _identityConfigurationName = identityConfigurationName;
            IdentityConfigurationElement element = section.IdentityConfigurationElements.GetElement(identityConfigurationName);
            LoadConfiguration(element);
        }

        /// <summary>
        /// Initializes an instance of <see cref="IdentityConfiguration"/>
        /// </summary>
        /// <param name="identityConfigurationName">The name of the &lt;service> element from which configuration is to be loaded.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is no &lt;System.IdentityModel> configuration element</exception>
        /// <param name="serviceCertificate">Thrown if there is no &lt;System.IdentityModel> configuration element</param>
        /// <remarks>If this constructor is called then a System.IdentityModel config section with the provided name must exist.</remarks>
        public IdentityConfiguration(string identityConfigurationName, X509Certificate2 serviceCertificate)
            : this(identityConfigurationName)
        {
            this.ServiceCertificate = serviceCertificate;
        }

        /// <summary>
        /// Gets or sets the AudienceRestriction.
        /// </summary>
        public AudienceRestriction AudienceRestriction
        {
            get { return _serviceHandlerConfiguration.AudienceRestriction; }
            set { _serviceHandlerConfiguration.AudienceRestriction = value; }
        }

        /// <summary>
        /// Gets the Caches configured.
        /// </summary>
        public IdentityModelCaches Caches
        {
            get { return _serviceHandlerConfiguration.Caches; }
            set { _serviceHandlerConfiguration.Caches = value; }
        }

        /// <summary>
        /// Gets or sets the certificate validation mode used by handlers to validate issuer certificates
        /// </summary>
        public X509CertificateValidationMode CertificateValidationMode
        {
            get { return _serviceHandlerConfiguration.CertificateValidationMode; }
            set { _serviceHandlerConfiguration.CertificateValidationMode = value; }
        }

        /// <summary>
        /// Gets or sets the certificate validator used by handlers to validate issuer certificates
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get { return _serviceHandlerConfiguration.CertificateValidator; }
            set { _serviceHandlerConfiguration.CertificateValidator = value; }
        }

        /// <summary>
        /// Gets or Sets the ClaimsAuthenticationManager.
        /// </summary>
        public ClaimsAuthenticationManager ClaimsAuthenticationManager
        {
            get { return _claimsAuthenticationManager; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _claimsAuthenticationManager = value;
            }
        }

        /// <summary>
        /// Gets or Sets the ClaimsAuthorizationManager.
        /// </summary>
        public ClaimsAuthorizationManager ClaimsAuthorizationManager
        {
            get { return _claimsAuthorizationManager; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _claimsAuthorizationManager = value;
            }
        }

        /// <summary>
        /// Gets or Sets detection of replaying of tokens by handlers in the default handler configuration.
        /// </summary>
        public bool DetectReplayedTokens
        {
            get { return _serviceHandlerConfiguration.DetectReplayedTokens; }
            set { _serviceHandlerConfiguration.DetectReplayedTokens = value; }
        }

        /// <summary>
        /// Determines if <see cref="IdentityConfiguration.Initialize"/> has been called.
        /// </summary>
        public virtual bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            protected set
            {
                _isInitialized = value;
            }
        }

        private static SecurityTokenResolver GetServiceTokenResolver(IdentityConfigurationElement element)
        {
            try
            {
                return CustomTypeElement.Resolve<SecurityTokenResolver>(element.ServiceTokenResolver);
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(
                    element, ConfigurationStrings.ServiceTokenResolver, inner);
            }
        }

        private static SecurityTokenResolver GetIssuerTokenResolver(IdentityConfigurationElement element)
        {
            try
            {
                return CustomTypeElement.Resolve<SecurityTokenResolver>(element.IssuerTokenResolver);
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(
                    element, ConfigurationStrings.IssuerTokenResolver, inner);
            }
        }

        private static ClaimsAuthenticationManager GetClaimsAuthenticationManager(IdentityConfigurationElement element)
        {
            try
            {
                return CustomTypeElement.Resolve<ClaimsAuthenticationManager>(element.ClaimsAuthenticationManager);
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(
                    element, ConfigurationStrings.ClaimsAuthenticationManager, inner);
            }
        }

        private static IssuerNameRegistry GetIssuerNameRegistry(IssuerNameRegistryElement element)
        {

            try
            {
                Type type = string.IsNullOrEmpty(element.Type) ? DefaultIssuerNameRegistryType : Type.GetType(element.Type);
                return TypeResolveHelper.Resolve<IssuerNameRegistry>(element, type);
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(
                    element, ConfigurationStrings.IssuerNameRegistry, inner);
            }
        }

        /// <summary>
        /// Updates properties in the <see cref="SecurityTokenHandlerConfiguration"/> objects for the 
        /// <see cref="SecurityTokenHandlerCollection"/> objects contained in 
        /// <see cref="IdentityConfiguration.SecurityTokenHandlerCollectionManager"/> to be consistent with the property
        /// values on this <see cref="IdentityConfiguration"/> instance.
        /// </summary>
        /// <remarks>
        /// This method should be invoked prior to using these token handlers
        /// for token processing.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this method is invoked more than once.</exception>
        public virtual void Initialize()
        {
            if (this.IsInitialized)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7009));
            }

            SecurityTokenHandlerCollection defaultCollection = this.SecurityTokenHandlers;

            if (!object.ReferenceEquals(_serviceHandlerConfiguration, defaultCollection.Configuration))
            {
                //
                // If someone has created their own new STHConfig and set it as default, leave that config alone.
                //
                TraceUtility.TraceString(TraceEventType.Information, SR.GetString(SR.ID4283));
                this.IsInitialized = true;
                return;
            }

            // Update the ServiceTokenResolver of the default TokenHandlerCollection's configuration, if serviceCertificate is set.
            if (this.ServiceCertificate != null)
            {
                SecurityTokenResolver serviceCertificateResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(new ReadOnlyCollection<SecurityToken>(
                                                      new SecurityToken[] { new X509SecurityToken(this.ServiceCertificate) }), false);

                SecurityTokenResolver tokenResolver = this.SecurityTokenHandlers.Configuration.ServiceTokenResolver;

                if ((tokenResolver != null) && (tokenResolver != EmptySecurityTokenResolver.Instance))
                {
                    this.SecurityTokenHandlers.Configuration.ServiceTokenResolver = new AggregateTokenResolver(new SecurityTokenResolver[] { serviceCertificateResolver, tokenResolver });
                }
                else
                {
                    this.SecurityTokenHandlers.Configuration.ServiceTokenResolver = serviceCertificateResolver;
                }
            }

            SecurityTokenResolver configuredIssuerTokenResolver = this.IssuerTokenResolver;

            if (this.IssuerTokenResolver == SecurityTokenHandlerConfiguration.DefaultIssuerTokenResolver)
            {
                //
                // Add the known certificates from WCF's ServiceCredentials in front of 
                // the default issuer token resolver.
                //
                if (this.KnownIssuerCertificates != null)
                {
                    int count = this.KnownIssuerCertificates.Count;
                    if (count > 0)
                    {
                        SecurityToken[] tokens = new SecurityToken[count];
                        for (int i = 0; i < count; i++)
                        {
                            tokens[i] = new X509SecurityToken(this.KnownIssuerCertificates[i]);
                        }

                        SecurityTokenResolver knownCertificateTokenResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(new ReadOnlyCollection<SecurityToken>(tokens), false);
                        
                        this.IssuerTokenResolver = new AggregateTokenResolver(new SecurityTokenResolver[] { knownCertificateTokenResolver, configuredIssuerTokenResolver });                       
                    }
                }
            }
            
            if (this.CertificateValidationMode != X509CertificateValidationMode.Custom)
            {
                defaultCollection.Configuration.CertificateValidator = X509Util.CreateCertificateValidator(defaultCollection.Configuration.CertificateValidationMode,
                                                                                                            defaultCollection.Configuration.RevocationMode,
                                                                                                            defaultCollection.Configuration.TrustedStoreLocation);
            }
            else if (object.ReferenceEquals(defaultCollection.Configuration.CertificateValidator, SecurityTokenHandlerConfiguration.DefaultCertificateValidator))
            {
                //
                // If the mode is custom but the validator or still default, something has gone wrong.
                //
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4280)));
            }

            this.IsInitialized = true;
        }

        /// <summary>
        /// Loads the settings for the IdentityConfiguration from the application or web configuration file.
        /// </summary>
        /// <remarks>
        /// If there is no configuration file, or the named section does not exist, then no exception is thrown,
        /// instead the class is loaded with a set of default values.
        /// </remarks>
        protected void LoadConfiguration(IdentityConfigurationElement element)
        {

            if (element != null)
            {
                //
                // Load the claims authentication manager
                //
                if (element.ClaimsAuthenticationManager.IsConfigured)
                {
                    _claimsAuthenticationManager = GetClaimsAuthenticationManager(element);
                }

                //
                // Load the claims authorization manager.
                //
                if (element.ClaimsAuthorizationManager.IsConfigured)
                {
                    _claimsAuthorizationManager = CustomTypeElement.Resolve<ClaimsAuthorizationManager>(element.ClaimsAuthorizationManager);
                }

                //
                // Load the service level Security Token Handler configuration
                //
                _serviceHandlerConfiguration = LoadHandlerConfiguration(element);
            }

            //
            // Reads handler configuration via LoadConfiguredHandlers. Do this last.
            //
            _securityTokenHandlerCollectionManager = LoadHandlers(element);
        }

        /// <summary>
        /// Loads the <see cref="SecurityTokenHandlerCollectionManager"/> defined for a given service.
        /// </summary>
        /// <param name="serviceElement">The <see cref="IdentityConfigurationElement"/> used to configure this instance.</param>
        /// <returns></returns>
        protected SecurityTokenHandlerCollectionManager LoadHandlers(IdentityConfigurationElement serviceElement)
        {
            //
            // We start with a token handler collection manager that contains a single collection that includes the default
            // handlers for the system.
            //
            SecurityTokenHandlerCollectionManager manager = SecurityTokenHandlerCollectionManager.CreateEmptySecurityTokenHandlerCollectionManager();

            if (serviceElement != null)
            {
                //
                // Load any token handler collections that appear as part of this service element
                //
                if (serviceElement.SecurityTokenHandlerSets.Count > 0)
                {
                    foreach (SecurityTokenHandlerElementCollection handlerElementCollection in serviceElement.SecurityTokenHandlerSets)
                    {
                        try
                        {
                            SecurityTokenHandlerConfiguration handlerConfiguration;
                            SecurityTokenHandlerCollection handlerCollection;

                            if (string.IsNullOrEmpty(handlerElementCollection.Name) ||
                                 StringComparer.Ordinal.Equals(handlerElementCollection.Name, ConfigurationStrings.DefaultConfigurationElementName))
                            {
                                //
                                // For the default collection, merge the IdentityConfiguration with the underlying config, if it exists.
                                //
                                if (handlerElementCollection.SecurityTokenHandlerConfiguration.IsConfigured)
                                {
                                    //
                                    // Configuration from a nested configuration object. We start with Service level configuration for 
                                    // handlers and then override the collection specific configuration. The result is a new configuration
                                    // object that can only be modified by accessing the collection or handlers configuration properties.
                                    //
                                    _serviceHandlerConfiguration = LoadHandlerConfiguration(serviceElement);
                                    handlerConfiguration = LoadHandlerConfiguration(_serviceHandlerConfiguration, handlerElementCollection.SecurityTokenHandlerConfiguration);
                                }
                                else
                                {
                                    //
                                    // No nested configuration object. We use the values from the ServiceElement for this case.
                                    //
                                    handlerConfiguration = LoadHandlerConfiguration(serviceElement);
                                }

                                _serviceHandlerConfiguration = handlerConfiguration;
                            }
                            else
                            {
                                //
                                // This is a non-default collection. There should be no settings inherited from IdentityConfiguration.
                                //
                                if (handlerElementCollection.SecurityTokenHandlerConfiguration.IsConfigured)
                                {
                                    handlerConfiguration = LoadHandlerConfiguration(null, handlerElementCollection.SecurityTokenHandlerConfiguration);
                                }
                                else
                                {
                                    //
                                    // If there is no underlying config, set everything as default.
                                    //
                                    handlerConfiguration = new SecurityTokenHandlerConfiguration();
                                }
                            }

                            handlerCollection = new SecurityTokenHandlerCollection(handlerConfiguration);
                            manager[handlerElementCollection.Name] = handlerCollection;

                            foreach (CustomTypeElement handlerElement in handlerElementCollection)
                            {
                                handlerCollection.Add(CustomTypeElement.Resolve<SecurityTokenHandler>(handlerElement));
                            }
                        }
                        catch (ArgumentException inner)
                        {
                            throw DiagnosticUtility.ThrowHelperConfigurationError(serviceElement, handlerElementCollection.Name, inner);
                        }
                    }
                }
                //
                // Ensure that the default usage collection always exists
                //
                if (!manager.ContainsKey(SecurityTokenHandlerCollectionManager.Usage.Default))
                {
                    manager[SecurityTokenHandlerCollectionManager.Usage.Default] = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection(_serviceHandlerConfiguration);
                }
            }
            else
            {
                //
                // Ensure that the default usage collection always exists
                //
                _serviceHandlerConfiguration = new SecurityTokenHandlerConfiguration();

                _serviceHandlerConfiguration.MaxClockSkew = _serviceMaxClockSkew;

                if (!manager.ContainsKey(SecurityTokenHandlerCollectionManager.Usage.Default))
                {
                    manager[SecurityTokenHandlerCollectionManager.Usage.Default] = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection(_serviceHandlerConfiguration);
                }
            }

            return manager;
        }

        /// <summary>
        /// Loads a SecurityTokenHandlerConfiguration using the elements directly under the ServiceElement.
        /// </summary>
        protected SecurityTokenHandlerConfiguration LoadHandlerConfiguration(IdentityConfigurationElement element)
        {

            SecurityTokenHandlerConfiguration handlerConfiguration = new SecurityTokenHandlerConfiguration();

            try
            {
                if (element.ElementInformation.Properties[ConfigurationStrings.MaximumClockSkew].ValueOrigin != PropertyValueOrigin.Default)
                {
                    handlerConfiguration.MaxClockSkew = element.MaximumClockSkew;
                }
                else
                {
                    handlerConfiguration.MaxClockSkew = _serviceMaxClockSkew;
                }
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(element, ConfigurationStrings.MaximumClockSkew, inner);
            }

            if (element.AudienceUris.IsConfigured)
            {
                handlerConfiguration.AudienceRestriction.AudienceMode = element.AudienceUris.Mode;

                foreach (AudienceUriElement audienceUriElement in element.AudienceUris)
                {
                    handlerConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audienceUriElement.Value, UriKind.RelativeOrAbsolute));
                }
            }
            if (element.Caches.IsConfigured)
            {
                if (element.Caches.TokenReplayCache.IsConfigured)
                {
                    handlerConfiguration.Caches.TokenReplayCache = CustomTypeElement.Resolve<TokenReplayCache>(element.Caches.TokenReplayCache);
                }

                if (element.Caches.SessionSecurityTokenCache.IsConfigured)
                {
                    handlerConfiguration.Caches.SessionSecurityTokenCache = CustomTypeElement.Resolve<SessionSecurityTokenCache>(element.Caches.SessionSecurityTokenCache);
                }
            }

            if (element.CertificateValidation.IsConfigured)
            {
                handlerConfiguration.RevocationMode = element.CertificateValidation.RevocationMode;
                handlerConfiguration.CertificateValidationMode = element.CertificateValidation.CertificateValidationMode;
                handlerConfiguration.TrustedStoreLocation = element.CertificateValidation.TrustedStoreLocation;

                if (element.CertificateValidation.CertificateValidator.IsConfigured)
                {
                    handlerConfiguration.CertificateValidator = CustomTypeElement.Resolve<X509CertificateValidator>(element.CertificateValidation.CertificateValidator);
                }
            }

            //
            // Load the issuer name registry
            //
            if (element.IssuerNameRegistry.IsConfigured)
            {
                handlerConfiguration.IssuerNameRegistry = GetIssuerNameRegistry(element.IssuerNameRegistry);
            }

            //
            // Load the issuer token resolver
            //
            if (element.IssuerTokenResolver.IsConfigured)
            {
                handlerConfiguration.IssuerTokenResolver = GetIssuerTokenResolver(element);
            }

            //
            // SaveBootstrapContext
            //
            handlerConfiguration.SaveBootstrapContext = element.SaveBootstrapContext;

            //
            // Load the service token resolver
            //
            if (element.ServiceTokenResolver.IsConfigured)
            {
                handlerConfiguration.ServiceTokenResolver = GetServiceTokenResolver(element);
            }

            //
            // TokenReplayCache related items
            //
            if (element.TokenReplayDetection.IsConfigured)
            {
                //
                // Set on SecurityTokenHandlerConfiguration
                //

                // DetectReplayedTokens set - { true | false }
                //
                handlerConfiguration.DetectReplayedTokens = element.TokenReplayDetection.Enabled;

                // ExpirationPeriod { TimeSpan }
                //
                handlerConfiguration.TokenReplayCacheExpirationPeriod = element.TokenReplayDetection.ExpirationPeriod;
            }

            return handlerConfiguration;
        }

        /// <summary>
        /// Loads configuration elements pertaining to the <see cref="SecurityTokenHandlerCollection"/>
        /// </summary>
        /// <param name="baseConfiguration">Base <see cref="SecurityTokenHandlerConfiguration"/> from which to inherit default values.</param>
        /// <param name="element">The <see cref="SecurityTokenHandlerConfigurationElement"/> from the configuration file.</param>
        /// <returns></returns>
        protected SecurityTokenHandlerConfiguration LoadHandlerConfiguration(SecurityTokenHandlerConfiguration baseConfiguration, SecurityTokenHandlerConfigurationElement element)
        {
            SecurityTokenHandlerConfiguration handlerConfiguration = (baseConfiguration == null) ? new SecurityTokenHandlerConfiguration() : baseConfiguration;

            if (element.AudienceUris.IsConfigured)
            {
                //
                // There is no inheritance of the content of the element from base to child, only the whole element. If the
                // user specifies any part, they must specify it all.
                //
                handlerConfiguration.AudienceRestriction.AudienceMode = AudienceUriMode.Always;
                handlerConfiguration.AudienceRestriction.AllowedAudienceUris.Clear();

                handlerConfiguration.AudienceRestriction.AudienceMode = element.AudienceUris.Mode;

                foreach (AudienceUriElement audienceUriElement in element.AudienceUris)
                {
                    handlerConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audienceUriElement.Value, UriKind.RelativeOrAbsolute));
                }
            }

            if (element.Caches.IsConfigured)
            {
                if (element.Caches.TokenReplayCache.IsConfigured)
                {
                    handlerConfiguration.Caches.TokenReplayCache = CustomTypeElement.Resolve<TokenReplayCache>(element.Caches.TokenReplayCache);
                }

                if (element.Caches.SessionSecurityTokenCache.IsConfigured)
                {
                    handlerConfiguration.Caches.SessionSecurityTokenCache = CustomTypeElement.Resolve<SessionSecurityTokenCache>(element.Caches.SessionSecurityTokenCache);
                }
            }

            if (element.CertificateValidation.IsConfigured)
            {
                handlerConfiguration.RevocationMode = element.CertificateValidation.RevocationMode;
                handlerConfiguration.CertificateValidationMode = element.CertificateValidation.CertificateValidationMode;
                handlerConfiguration.TrustedStoreLocation = element.CertificateValidation.TrustedStoreLocation;

                if (element.CertificateValidation.CertificateValidator.IsConfigured)
                {
                    handlerConfiguration.CertificateValidator = CustomTypeElement.Resolve<X509CertificateValidator>(element.CertificateValidation.CertificateValidator);
                }
            }

            //
            // Load the issuer name registry
            //
            if (element.IssuerNameRegistry.IsConfigured)
            {
                handlerConfiguration.IssuerNameRegistry = GetIssuerNameRegistry(element.IssuerNameRegistry);
            }

            //
            // Load the issuer token resolver
            //
            if (element.IssuerTokenResolver.IsConfigured)
            {
                handlerConfiguration.IssuerTokenResolver = CustomTypeElement.Resolve<SecurityTokenResolver>(element.IssuerTokenResolver);
            }

            //
            // Load MaxClockSkew
            //
            try
            {
                if (element.ElementInformation.Properties[ConfigurationStrings.MaximumClockSkew].ValueOrigin != PropertyValueOrigin.Default)
                {
                    handlerConfiguration.MaxClockSkew = element.MaximumClockSkew;
                }
            }
            catch (ArgumentException inner)
            {
                throw DiagnosticUtility.ThrowHelperConfigurationError(element, ConfigurationStrings.MaximumClockSkew, inner);
            }

            //
            // SaveBootstrapTokens
            //
            if (element.ElementInformation.Properties[ConfigurationStrings.SaveBootstrapContext].ValueOrigin != PropertyValueOrigin.Default)
            {
                handlerConfiguration.SaveBootstrapContext = element.SaveBootstrapContext;
            }

            //
            // Load the service token resolver
            //
            if (element.ServiceTokenResolver.IsConfigured)
            {
                handlerConfiguration.ServiceTokenResolver = CustomTypeElement.Resolve<SecurityTokenResolver>(element.ServiceTokenResolver);
            }

            //
            // TokenReplayCache related items
            //
            if (element.TokenReplayDetection.IsConfigured)
            {
                //
                // Set on SecurityTokenHandlerConfiguration
                //

                //
                // DetectReplayedTokens set - { true | false }
                //
                handlerConfiguration.DetectReplayedTokens = element.TokenReplayDetection.Enabled;

                //
                // ExpirationPeriod { TimeSpan }
                //
                handlerConfiguration.TokenReplayCacheExpirationPeriod = element.TokenReplayDetection.ExpirationPeriod;

            }

            return handlerConfiguration;
        }

        /// <summary>
        /// Gets or sets the maximum allowable time difference between the 
        /// system clocks of the two parties that are communicating.
        /// </summary>
        public TimeSpan MaxClockSkew
        {
            get { return _serviceHandlerConfiguration.MaxClockSkew; }
            set { _serviceHandlerConfiguration.MaxClockSkew = value; }
        }

        /// <summary>
        /// Gets or sets the service name of this configuration.
        /// </summary>
        public string Name
        {
            get
            {
                return _identityConfigurationName;
            }
        }

        /// <summary>
        /// Gets or sets the IssuerNameRegistry used to resolve issuer names.
        /// </summary>
        public IssuerNameRegistry IssuerNameRegistry
        {
            get
            {
                return _serviceHandlerConfiguration.IssuerNameRegistry;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _serviceHandlerConfiguration.IssuerNameRegistry = value;
            }
        }

        /// <summary>
        /// The service certificate to initialize the ServiceTokenResolver and the SessionSecurityTokenHandler.
        /// </summary>
        public X509Certificate2 ServiceCertificate
        {
            get { return _serviceCertificate; }
            set { this._serviceCertificate = value; }
        }

        internal List<X509Certificate2> KnownIssuerCertificates
        {
            get
            {
                return this.knownCertificates;
            }
            set
            {
                this.knownCertificates = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Issuer token resolver.
        /// </summary>
        public SecurityTokenResolver IssuerTokenResolver
        {
            get
            {
                return _serviceHandlerConfiguration.IssuerTokenResolver;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _serviceHandlerConfiguration.IssuerTokenResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the revocation mode used by handlers to validate issuer certificates
        /// </summary>
        public X509RevocationMode RevocationMode
        {
            get { return _serviceHandlerConfiguration.RevocationMode; }
            set { _serviceHandlerConfiguration.RevocationMode = value; }
        }

        /// <summary>
        /// Gets or Sets the Service token resolver.
        /// </summary>
        public SecurityTokenResolver ServiceTokenResolver
        {
            get
            {
                return _serviceHandlerConfiguration.ServiceTokenResolver;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _serviceHandlerConfiguration.ServiceTokenResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets if BootstrapContext is saved in the ClaimsIdentity and Sessions after token validation.
        /// </summary>
        public bool SaveBootstrapContext
        {
            get { return _serviceHandlerConfiguration.SaveBootstrapContext; }
            set { _serviceHandlerConfiguration.SaveBootstrapContext = value; }
        }

        /// <summary>
        /// The <see cref="SecurityTokenHandlerCollectionManager" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </summary>
        public SecurityTokenHandlerCollectionManager SecurityTokenHandlerCollectionManager
        {
            get
            {
                return _securityTokenHandlerCollectionManager;
            }
        }

        /// <summary>
        /// The <see cref="SecurityTokenHandlerCollection" /> collection of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// If user wants to register their own token handler, they
        /// can simply add their own handler to this collection.
        /// </summary>
        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            get
            {
                return _securityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.Default];
            }
        }

        /// <summary>
        /// Gets or Sets the expiration period for items placed in the TokenReplayCache.
        /// </summary>
        public TimeSpan TokenReplayCacheExpirationPeriod
        {
            get { return _serviceHandlerConfiguration.TokenReplayCacheExpirationPeriod; }
            set { _serviceHandlerConfiguration.TokenReplayCacheExpirationPeriod = value; }
        }

        /// <summary>
        /// Gets or sets the trusted store location used by handlers to validate issuer certificates
        /// </summary>
        public StoreLocation TrustedStoreLocation
        {
            get { return _serviceHandlerConfiguration.TrustedStoreLocation; }
            set { _serviceHandlerConfiguration.TrustedStoreLocation = value; }
        }
    }
}
