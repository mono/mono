//-----------------------------------------------------------------------
// <copyright file="SecurityTokenHandlerConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;

    /// <summary>
    /// Configuration common to all SecurityTokenHandlers.
    /// </summary>
    public class SecurityTokenHandlerConfiguration
    {
        // 

#pragma warning disable 1591
        /// <summary>
        /// Gets a value indicating whether or not to detect replay tokens by default.
        /// </summary>
        public static readonly bool DefaultDetectReplayedTokens; // false

        /// <summary>
        /// Gets the default issuer name registry.
        /// </summary>
        public static readonly IssuerNameRegistry DefaultIssuerNameRegistry = new ConfigurationBasedIssuerNameRegistry();

        /// <summary>
        /// Gets the default issuer token resolver.
        /// </summary>
        public static readonly SecurityTokenResolver DefaultIssuerTokenResolver = System.IdentityModel.Tokens.IssuerTokenResolver.DefaultInstance;

        /// <summary>
        /// Gets the default maximum clock skew.
        /// </summary>
        public static readonly TimeSpan DefaultMaxClockSkew = new TimeSpan(0, 5, 0); // 5 minutes

        /// <summary>
        /// Gets a value indicating whether or not to save bootstrap tokens by default.
        /// </summary>
        public static readonly bool DefaultSaveBootstrapContext; // false;

        /// <summary>
        /// Gets the default token replay cache expiration period.
        /// </summary>
        public static readonly TimeSpan DefaultTokenReplayCacheExpirationPeriod = TimeSpan.MaxValue;

        // The below 3 defaults were moved from  IdentityConfiguration class as we can not have service configuration in IdentityModel.

        /// <summary>
        /// Gets the default X.509 certificate validation mode.
        /// </summary>
        public static readonly X509CertificateValidationMode DefaultCertificateValidationMode = IdentityConfiguration.DefaultCertificateValidationMode;

        /// <summary>
        /// Gets the default X.509 certificate revocation validation mode.
        /// </summary>
        public static readonly X509RevocationMode DefaultRevocationMode = IdentityConfiguration.DefaultRevocationMode;

        /// <summary>
        /// Gets the default X.509 certificate trusted store location.
        /// </summary>
        public static readonly StoreLocation DefaultTrustedStoreLocation = IdentityConfiguration.DefaultTrustedStoreLocation;

        StoreLocation trustedStoreLocation = DefaultTrustedStoreLocation;
        X509RevocationMode revocationMode = DefaultRevocationMode;
        X509CertificateValidationMode certificateValidationMode = DefaultCertificateValidationMode;

        /// <summary>
        /// Gets the default X.509 certificate validator instance.
        /// </summary>
        public static readonly X509CertificateValidator DefaultCertificateValidator = X509Util.CreateCertificateValidator(DefaultCertificateValidationMode, DefaultRevocationMode, DefaultTrustedStoreLocation);
#pragma warning restore 1591

        private AudienceRestriction audienceRestriction = new AudienceRestriction();
        private X509CertificateValidator certificateValidator = DefaultCertificateValidator;
        private bool detectReplayedTokens = DefaultDetectReplayedTokens;
        private IssuerNameRegistry issuerNameRegistry = DefaultIssuerNameRegistry;
        private SecurityTokenResolver issuerTokenResolver = DefaultIssuerTokenResolver;
        private TimeSpan maxClockSkew = DefaultMaxClockSkew;
        private bool saveBootstrapContext = DefaultSaveBootstrapContext;
        private SecurityTokenResolver serviceTokenResolver = EmptySecurityTokenResolver.Instance;
        private TimeSpan tokenReplayCacheExpirationPeriod = DefaultTokenReplayCacheExpirationPeriod;
        private IdentityModelCaches caches = new IdentityModelCaches();
                
        /// <summary>
        /// Creates an instance of <see cref="SecurityTokenHandlerConfiguration"/>
        /// </summary>
        public SecurityTokenHandlerConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the AudienceRestriction.
        /// </summary>
        public AudienceRestriction AudienceRestriction
        {
            get
            {
                return this.audienceRestriction;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.audienceRestriction = value;
            }
        }

        /// <summary>
        /// Gets or sets the certificate validator used by handlers to validate issuer certificates
        /// </summary>
        public X509CertificateValidator CertificateValidator
        {
            get
            {
                return this.certificateValidator;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.certificateValidator = value;
            }
        }

        public X509RevocationMode RevocationMode
        {
            get { return revocationMode; }
            set { revocationMode = value; }
        }

        /// <summary>
        /// Gets or sets the trusted store location used by handlers to validate issuer certificates
        /// </summary>
        public StoreLocation TrustedStoreLocation
        {
            get { return trustedStoreLocation; }
            set { trustedStoreLocation = value; }
        }

        /// <summary>
        /// Gets or sets the certificate validation mode used by handlers to validate issuer certificates
        /// </summary>
        public X509CertificateValidationMode CertificateValidationMode
        {
            get { return certificateValidationMode; }
            set { certificateValidationMode = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to detect replaying of tokens by handlers in this configuration.
        /// </summary>
        public bool DetectReplayedTokens
        {
            get { return this.detectReplayedTokens; }
            set { this.detectReplayedTokens = value; }
        }

        /// <summary>
        /// Gets or sets the IssuerNameRegistry.
        /// </summary>
        public IssuerNameRegistry IssuerNameRegistry
        {
            get 
            {
                return this.issuerNameRegistry; 
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.issuerNameRegistry = value;
            }
        }

        /// <summary>
        /// Gets or sets the IssuerTokenResolver.
        /// </summary>
        public SecurityTokenResolver IssuerTokenResolver
        {
            get 
            {
                return this.issuerTokenResolver; 
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.issuerTokenResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum clock skew for handlers using this config.
        /// </summary>
        public TimeSpan MaxClockSkew
        {
            get 
            {
                return this.maxClockSkew; 
            }

            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID2070));
                }

                this.maxClockSkew = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether BootstrapContext is saved in the ClaimsIdentity and Sessions after token validation.
        /// </summary>
        public bool SaveBootstrapContext
        {
            get { return this.saveBootstrapContext; }
            set { this.saveBootstrapContext = value; }
        }

        /// <summary>
        /// Gets or sets the TokenResolver that resolves Service tokens.
        /// </summary>
        public SecurityTokenResolver ServiceTokenResolver
        {
            get 
            {
                return this.serviceTokenResolver; 
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.serviceTokenResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the Caches that are used.
        /// </summary>
        public IdentityModelCaches Caches
        {
            get 
            {
                return this.caches; 
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.caches = value;
            }
        }

        /// <summary>
        /// Gets or sets the expiration period for items placed in the TokenReplayCache.
        /// </summary>
        public TimeSpan TokenReplayCacheExpirationPeriod
        {
            get 
            {
                return this.tokenReplayCacheExpirationPeriod; 
            }

            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID0016));
                }

                this.tokenReplayCacheExpirationPeriod = value;
            }
        }
    }
}
