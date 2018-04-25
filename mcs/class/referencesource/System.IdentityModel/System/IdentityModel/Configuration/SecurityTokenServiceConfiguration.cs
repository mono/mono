//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using SecurityTokenTypes = System.IdentityModel.Tokens.SecurityTokenTypes;
using STS = System.IdentityModel.SecurityTokenService;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Protocols.WSTrust;

namespace System.IdentityModel.Configuration
{
    /// <summary>
    /// Defines the configuration specific to a SecurityTokenService.
    /// </summary>
    public class SecurityTokenServiceConfiguration : IdentityConfiguration
    {
        string _tokenIssuerName;
        SigningCredentials _signingCredentials;

        TimeSpan _defaultTokenLifetime = TimeSpan.FromHours(1.0);
        TimeSpan _maximumTokenLifetime = TimeSpan.FromDays(1);

        string _defaultTokenType = SecurityTokenTypes.SamlTokenProfile11;
        internal const int DefaultKeySizeInBitsConstant = 256;

        int _defaultSymmetricKeySizeInBits = DefaultKeySizeInBitsConstant;
        int _defaultMaxSymmetricKeySizeInBits = 1024;
        bool _disableWsdl;
        
        Type _securityTokenServiceType;

        // 
        // Trust Serializers.
        //
        WSTrust13RequestSerializer _wsTrust13RequestSerializer = new WSTrust13RequestSerializer();
        WSTrust13ResponseSerializer _wsTrust13ResponseSerializer = new WSTrust13ResponseSerializer();
        WSTrustFeb2005RequestSerializer _wsTrustFeb2005RequestSerializer = new WSTrustFeb2005RequestSerializer();
        WSTrustFeb2005ResponseSerializer _wsTrustFeb2005ResponseSerializer = new WSTrustFeb2005ResponseSerializer();

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <remarks>
        /// IssuerName must be set before the <see cref="SecurityTokenService"/> is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="loadConfig">Whether or not config should be loaded.</param>
        /// <remarks>
        /// IssuerName must be set before the <see cref="SecurityTokenService"/> is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(bool loadConfig)
            : this(null, null, loadConfig)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="issuerName">The issuer name.</param>
        /// <remarks>
        /// If issuerName is null, IssuerName must be set before the <see cref="SecurityTokenService"/>
        /// is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(string issuerName)
            : this(issuerName, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="loadConfig">Whether or not config should be loaded.</param>
        /// <remarks>
        /// If issuerName is null, IssuerName must be set before the <see cref="SecurityTokenService"/>
        /// is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(string issuerName, bool loadConfig)
            : this(issuerName, null, loadConfig)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="signingCredentials">The signing credential for the STS.</param>
        /// <remarks>
        /// If issuerName is null, IssuerName must be set before the <see cref="SecurityTokenService"/>
        /// is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(string issuerName, SigningCredentials signingCredentials)
            : base()
        {
            _tokenIssuerName = issuerName;
            _signingCredentials = signingCredentials;
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="signingCredentials">The signing credential for the STS.</param>
        /// <param name="loadConfig">Whether or not config should be loaded.</param>
        /// <remarks>
        /// If issuerName is null, IssuerName must be set before the <see cref="SecurityTokenService"/>
        /// is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(string issuerName, SigningCredentials signingCredentials, bool loadConfig)
            : base(loadConfig)
        {
            _tokenIssuerName = issuerName;
            _signingCredentials = signingCredentials;
        }

        /// <summary>
        /// Initializes an instance of <see cref="SecurityTokenServiceConfiguration"/>
        /// </summary>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="signingCredentials">The signing credential for the STS.</param>
        /// <param name="serviceName">The name of the &lt;service> element from which configuration is to be loaded.</param>
        /// <remarks>
        /// If issuerName is null, IssuerName must be set before the <see cref="SecurityTokenService"/>
        /// is used to create a token.
        /// </remarks>
        public SecurityTokenServiceConfiguration(string issuerName, SigningCredentials signingCredentials, string serviceName)
            : base(serviceName)
        {
            _tokenIssuerName = issuerName;
            _signingCredentials = signingCredentials;
        }

        /// <summary>
        /// Gets or sets the type of the SecurityTokenService.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null.</exception>
        public Type SecurityTokenService
        {
            get
            {
                return _securityTokenServiceType;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (!typeof(System.IdentityModel.SecurityTokenService).IsAssignableFrom(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID2069));
                }

                _securityTokenServiceType = value;
            }
        }

        /// <summary>
        /// Creates an instance of SecurityTokenService from the type specified in
        /// SecurityTokenServiceConfiguration.SecurityTokenService. The method
        /// expects the type to implement a constructor that takes in the SecurityTokenServiceConfiguration.
        /// </summary>
        /// <returns>Instance of SecurityTokenService.</returns>
        /// <exception cref="InvalidOperationException">Unable to create a SecurityTokenService instance from the configuration.</exception>
        public virtual STS CreateSecurityTokenService()
        {
            Type stsType = this.SecurityTokenService;

            if (stsType == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2073));
            }

            if (!typeof(STS).IsAssignableFrom(stsType))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2074, stsType, typeof(STS)));
            }

            return Activator.CreateInstance(stsType, this) as STS;
        }

        /// <summary>
        /// Gets or sets the default key size in bits used in the issued token.
        /// </summary>
        /// <remarks>
        /// This only applies to the symmetric key case.
        /// </remarks>
        public int DefaultSymmetricKeySizeInBits
        {
            get
            {
                return _defaultSymmetricKeySizeInBits;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", SR.GetString(SR.ID0002));
                }

                _defaultSymmetricKeySizeInBits = value;
            }
        }

        /// <summary>
        /// Gets or sets the default key size limit in bits used check if the KeySize specified in the request
        /// is within this limit.
        /// </summary>
        /// <remarks>
        /// This only applies to the symmetric key case.
        /// </remarks>
        public int DefaultMaxSymmetricKeySizeInBits
        {
            get
            {
                return _defaultMaxSymmetricKeySizeInBits;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", SR.GetString(SR.ID0002));
                }

                _defaultMaxSymmetricKeySizeInBits = value;
            }
        }

        /// <summary>
        /// Gets or sets the default lifetime used in the issued tokens.
        /// </summary>
        public TimeSpan DefaultTokenLifetime
        {
            get
            {
                return _defaultTokenLifetime;
            }
            set
            {

                _defaultTokenLifetime = value;
            }
        }

        /// <summary>
        /// Gets or sets the default token type used in token issuance.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null or empty.</exception>
        /// <exception cref="ArgumentException">The provided value is not defined in the token handlers.</exception>
        public string DefaultTokenType
        {
            get
            {
                return _defaultTokenType;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("value");
                }

                if (SecurityTokenHandlers[value] == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID2015, value));
                }

                _defaultTokenType = value;
            }
        }

        /// <summary>
        /// Gets or Sets a boolean that specifies if WSDL generation for the
        /// Service should be enabled. Default is false.
        /// </summary>
        public bool DisableWsdl
        {
            get
            {
                return _disableWsdl;
            }
            set
            {
                _disableWsdl = value;
            }
        }

        
        /// <summary>
        /// Gets or sets the maximum token lifetime for issued tokens.
        /// </summary>
        public TimeSpan MaximumTokenLifetime
        {
            get
            {
                return _maximumTokenLifetime;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", SR.GetString(SR.ID0016));
                }
                _maximumTokenLifetime = value;
            }
        }

        /// <summary>
        /// Gets or sets the signing credentials.
        /// </summary>
        public SigningCredentials SigningCredentials
        {
            get
            {
                return _signingCredentials;
            }
            set
            {
                _signingCredentials = value;
            }

        }

        /// <summary>
        /// Gets the issuer name so that it can be reflected in the issued token.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        public string TokenIssuerName
        {
            get
            {
                return _tokenIssuerName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _tokenIssuerName = value;
            }
        }

        /// <summary>
        /// Gets or sets the WS-Trust 1.3 Request (RST) serializer.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null.</exception>
        public WSTrust13RequestSerializer WSTrust13RequestSerializer
        {
            get
            {
                return _wsTrust13RequestSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _wsTrust13RequestSerializer = value;
            }
        }

        /// <summary>
        /// Gets or sets the WS-Trust 1.3 Response (RSTR) serializer.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null.</exception>
        public WSTrust13ResponseSerializer WSTrust13ResponseSerializer
        {
            get
            {
                return _wsTrust13ResponseSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _wsTrust13ResponseSerializer = value;
            }
        }

        /// <summary>
        /// Gets or sets the WS-Trust Feb 2005 Request (RST) serializer.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null.</exception>
        public WSTrustFeb2005RequestSerializer WSTrustFeb2005RequestSerializer
        {
            get
            {
                return _wsTrustFeb2005RequestSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _wsTrustFeb2005RequestSerializer = value;
            }
        }

        /// <summary>
        /// Gets or sets the WS-Trust Feb 2005 Response (RSTR) serializer.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided value is null.</exception>
        public WSTrustFeb2005ResponseSerializer WSTrustFeb2005ResponseSerializer
        {
            get
            {
                return _wsTrustFeb2005ResponseSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _wsTrustFeb2005ResponseSerializer = value;
            }
        }
    }
}
