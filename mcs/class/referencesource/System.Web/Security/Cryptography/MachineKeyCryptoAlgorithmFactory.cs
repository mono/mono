//------------------------------------------------------------------------------
// <copyright file="MachineKeyCryptoAlgorithmFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;
    using System.Web.Configuration;

    // Can create cryptographic algorithms from a given <machineKey> element

    internal sealed class MachineKeyCryptoAlgorithmFactory : ICryptoAlgorithmFactory {

        private Func<SymmetricAlgorithm> _encryptionAlgorithmFactory;
        private readonly MachineKeySection _machineKeySection;
        private Func<KeyedHashAlgorithm> _validationAlgorithmFactory;

        public MachineKeyCryptoAlgorithmFactory(MachineKeySection machineKeySection) {
            _machineKeySection = machineKeySection;
        }

        public SymmetricAlgorithm GetEncryptionAlgorithm() {
            if (_encryptionAlgorithmFactory == null) {
                _encryptionAlgorithmFactory = GetEncryptionAlgorithmFactory();
            }
            return _encryptionAlgorithmFactory();
        }

        private Func<SymmetricAlgorithm> GetEncryptionAlgorithmFactory() {
            return GetGenericAlgorithmFactory<SymmetricAlgorithm>(
                configAttributeName: "decryption",
                configAttributeValue: _machineKeySection.GetDecryptionAttributeSkipValidation(),
                switchStatement: algorithmName => {
                    // We suppress CS0618 since some of the algorithms we support are marked with [Obsolete].
                    // These deprecated algorithms are *not* enabled by default. Developers must opt-in to
                    // them, so we're secure by default.
#pragma warning disable 618
                    switch (algorithmName) {
                        case "AES":
                        case "Auto": // currently "Auto" defaults to AES
                            return CryptoAlgorithms.CreateAes;

                        case "DES":
                            return CryptoAlgorithms.CreateDES;

                        case "3DES":
                            return CryptoAlgorithms.CreateTripleDES;

                        default:
                            return null; // unknown
#pragma warning restore 618
                    }
                },
                errorResourceString: SR.Wrong_decryption_enum);
        }

        public KeyedHashAlgorithm GetValidationAlgorithm() {
            if (_validationAlgorithmFactory == null) {
                _validationAlgorithmFactory = GetValidationAlgorithmFactory();
            }
            return _validationAlgorithmFactory();
        }

        private Func<KeyedHashAlgorithm> GetValidationAlgorithmFactory() {
            return GetGenericAlgorithmFactory<KeyedHashAlgorithm>(
                configAttributeName: "validation",
                configAttributeValue: _machineKeySection.GetValidationAttributeSkipValidation(),
                switchStatement: algorithmName => {
                    switch (algorithmName) {
                        case "SHA1":
                            return CryptoAlgorithms.CreateHMACSHA1;

                        case "HMACSHA256":
                            return CryptoAlgorithms.CreateHMACSHA256;

                        case "HMACSHA384":
                            return CryptoAlgorithms.CreateHMACSHA384;

                        case "HMACSHA512":
                            return CryptoAlgorithms.CreateHMACSHA512;

                        default:
                            return null; // unknown
                    }
                },
                errorResourceString: SR.Wrong_validation_enum_FX45);
        }

        // Contains common logic for creating encryption / validation factories, including
        // custom algorithm lookup and exception handling.
        private Func<TResult> GetGenericAlgorithmFactory<TResult>(string configAttributeName, string configAttributeValue, Func<string, Func<TResult>> switchStatement, string errorResourceString) where TResult : class, IDisposable {
            Func<TResult> factory;

            if (configAttributeValue != null && configAttributeValue.StartsWith("alg:", StringComparison.Ordinal)) {
                string algorithmName = configAttributeValue.Substring("alg:".Length);
                factory = () => {
                    // Since the custom algorithm might depend on the impersonated
                    // identity, we must instantiate it under app-level impersonation.
                    using (new ApplicationImpersonationContext()) {
                        return (TResult)CryptoConfig.CreateFromName(algorithmName);
                    }
                };
            }
            else {
                // If using a built-in algorithm, consult the switch statement to get the factory.
                factory = switchStatement(configAttributeValue);
            }

            // Invoke the factory once to make sure there aren't any configuration errors.
            Exception factoryCreationException = null;
            try {
                if (factory != null) {
                    TResult algorithm = factory();
                    if (algorithm != null) {
                        algorithm.Dispose();
                        return factory; // we know at this point the factory is good
                    }
                }
            }
            catch (Exception ex) {
                factoryCreationException = ex;
            }

            // If we reached this point, there was a failure:
            // the factory returned null, threw, or did something else unexpected.
            throw ConfigUtil.MakeConfigurationErrorsException(
                message: SR.GetString(errorResourceString),
                innerException: factoryCreationException, // can be null
                configProperty: _machineKeySection.ElementInformation.Properties[configAttributeName]);
        }

    }
}
