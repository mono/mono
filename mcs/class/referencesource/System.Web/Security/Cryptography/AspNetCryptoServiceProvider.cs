//------------------------------------------------------------------------------
// <copyright file="AspNetCryptoServiceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Web.Configuration;

    // The central ASP.NET class for providing ICryptoService instances.
    // Get an instance of this class via the static Instance property.

    internal sealed class AspNetCryptoServiceProvider : ICryptoServiceProvider {

        private static readonly Lazy<AspNetCryptoServiceProvider> _singleton = new Lazy<AspNetCryptoServiceProvider>(GetSingletonCryptoServiceProvider);

        private readonly ICryptoAlgorithmFactory _cryptoAlgorithmFactory;
        private readonly IDataProtectorFactory _dataProtectorFactory;
        private readonly bool _isDataProtectorEnabled;
        private KeyDerivationFunction _keyDerivationFunction;
        private readonly MachineKeySection _machineKeySection;
        private readonly IMasterKeyProvider _masterKeyProvider;

        // This constructor is used only for testing purposes and by the singleton provider
        // and should not otherwise be called during ASP.NET request processing.
        internal AspNetCryptoServiceProvider(MachineKeySection machineKeySection = null, ICryptoAlgorithmFactory cryptoAlgorithmFactory = null, IMasterKeyProvider masterKeyProvider = null, IDataProtectorFactory dataProtectorFactory = null, KeyDerivationFunction keyDerivationFunction = null) {
            _machineKeySection = machineKeySection;
            _cryptoAlgorithmFactory = cryptoAlgorithmFactory;
            _masterKeyProvider = masterKeyProvider;
            _dataProtectorFactory = dataProtectorFactory;
            _keyDerivationFunction = keyDerivationFunction;

            // This CryptoServiceProvider is active if specified as such in the <system.web/machineKey> section
            IsDefaultProvider = (machineKeySection != null && machineKeySection.CompatibilityMode >= MachineKeyCompatibilityMode.Framework45);

            // The DataProtectorCryptoService is active if specified as such in config
            _isDataProtectorEnabled = (machineKeySection != null && !String.IsNullOrWhiteSpace(machineKeySection.DataProtectorType));
        }

        internal static AspNetCryptoServiceProvider Instance {
            get {
                return _singleton.Value;
            }
        }

        // Returns a value indicating whether this crypto service provider is the default
        // provider for the current application.
        internal bool IsDefaultProvider {
            get;
            private set;
        }

        public ICryptoService GetCryptoService(Purpose purpose, CryptoServiceOptions options = CryptoServiceOptions.None) {
            ICryptoService cryptoService;
            if (_isDataProtectorEnabled && options == CryptoServiceOptions.None) {
                // We can only use DataProtector if it's configured and the caller didn't ask for any special behavior like cacheability
                cryptoService = GetDataProtectorCryptoService(purpose);
            }
            else {
                // Otherwise we fall back to using the <machineKey> algorithms for cryptography
                cryptoService = GetNetFXCryptoService(purpose, options);
            }

            // always homogenize errors returned from the crypto service
            return new HomogenizingCryptoServiceWrapper(cryptoService);
        }

        private DataProtectorCryptoService GetDataProtectorCryptoService(Purpose purpose) {
            // just return the ICryptoService directly
            return new DataProtectorCryptoService(_dataProtectorFactory, purpose);
        }

        private NetFXCryptoService GetNetFXCryptoService(Purpose purpose, CryptoServiceOptions options) {
            // Extract the encryption and validation keys from the provided Purpose object
            CryptographicKey encryptionKey = purpose.GetDerivedEncryptionKey(_masterKeyProvider, _keyDerivationFunction);
            CryptographicKey validationKey = purpose.GetDerivedValidationKey(_masterKeyProvider, _keyDerivationFunction);

            // and return the ICryptoService
            // (predictable IV turned on if the caller requested cacheable output)
            return new NetFXCryptoService(_cryptoAlgorithmFactory, encryptionKey, validationKey, predictableIV: (options == CryptoServiceOptions.CacheableOutput));
        }

        private static AspNetCryptoServiceProvider GetSingletonCryptoServiceProvider() {
            // Provides all of the necessary dependencies for an application-level
            // AspNetCryptoServiceProvider.

            MachineKeySection machineKeySection = MachineKeySection.GetApplicationConfig();

            return new AspNetCryptoServiceProvider(
                machineKeySection: machineKeySection,
                cryptoAlgorithmFactory: new MachineKeyCryptoAlgorithmFactory(machineKeySection),
                masterKeyProvider: new MachineKeyMasterKeyProvider(machineKeySection),
                dataProtectorFactory: new MachineKeyDataProtectorFactory(machineKeySection),
                keyDerivationFunction: SP800_108.DeriveKey);
        }

    }
}
