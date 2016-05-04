//------------------------------------------------------------------------------
// <copyright file="MachineKeyDataProtectorFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;
    using System.Web.Configuration;

    // Can create DataProtector instances from a given <machineKey> element

    internal sealed class MachineKeyDataProtectorFactory : IDataProtectorFactory {

        private static readonly Purpose _creationTestingPurpose = new Purpose("test-1", "test-2", "test-3");

        private Func<Purpose, DataProtector> _dataProtectorFactory;
        private readonly MachineKeySection _machineKeySection;

        public MachineKeyDataProtectorFactory(MachineKeySection machineKeySection) {
            _machineKeySection = machineKeySection;
        }

        public DataProtector GetDataProtector(Purpose purpose) {
            if (_dataProtectorFactory == null) {
                _dataProtectorFactory = GetDataProtectorFactory();
            }
            return _dataProtectorFactory(purpose);
        }

        private Func<Purpose, DataProtector> GetDataProtectorFactory() {
            string applicationName = _machineKeySection.ApplicationName;
            string dataProtectorTypeName = _machineKeySection.DataProtectorType;

            Func<Purpose, DataProtector> factory = purpose => {
                // Since the custom implementation might depend on the impersonated
                // identity, we must instantiate it under app-level impersonation.
                using (new ApplicationImpersonationContext()) {
                    return DataProtector.Create(dataProtectorTypeName, applicationName, purpose.PrimaryPurpose, purpose.SpecificPurposes);
                }
            };

            // Invoke the factory once to make sure there aren't any configuration errors.
            Exception factoryCreationException = null;
            try {
                DataProtector dataProtector = factory(_creationTestingPurpose);
                if (dataProtector != null) {
                    IDisposable disposable = dataProtector as IDisposable;
                    if (disposable != null) {
                        disposable.Dispose();
                    }
                    return factory; // we know at this point the factory is good
                }
            }
            catch (Exception ex) {
                factoryCreationException = ex;
            }

            // If we reached this point, there was a failure:
            // the factory returned null, threw, or did something else unexpected.
            throw ConfigUtil.MakeConfigurationErrorsException(
                message: SR.GetString(SR.MachineKeyDataProtectorFactory_FactoryCreationFailed),
                innerException: factoryCreationException, // can be null
                configProperty: _machineKeySection.ElementInformation.Properties["dataProtectorType"]);
        }

    }
}
