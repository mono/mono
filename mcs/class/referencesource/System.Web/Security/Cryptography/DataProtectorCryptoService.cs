//------------------------------------------------------------------------------
// <copyright file="DataProtectorCryptoService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;

    // Uses the DataProtector class to protect sensitive information

    internal sealed class DataProtectorCryptoService : ICryptoService {

        private readonly IDataProtectorFactory _dataProtectorFactory;
        private readonly Purpose _purpose;

        public DataProtectorCryptoService(IDataProtectorFactory dataProtectorFactory, Purpose purpose) {
            _dataProtectorFactory = dataProtectorFactory;
            _purpose = purpose;
        }

        // Wraps the common logic of working with a DataProtector instance.
        // 'protect' is TRUE if we're calling Protect, FALSE if we're calling Unprotect.
        private byte[] PerformOperation(byte[] data, bool protect) {
            // Since the DataProtector might depend on the impersonated context, we must
            // work with it only under app-level impersonation. The idea behind this is
            // that if the cryptographic routine is provided by an OS-level implementation
            // (like DPAPI), any keys will be locked to the account of the web application
            // itself.
            using (new ApplicationImpersonationContext()) {
                DataProtector dataProtector = null;
                try {
                    dataProtector = _dataProtectorFactory.GetDataProtector(_purpose);
                    return (protect) ? dataProtector.Protect(data) : dataProtector.Unprotect(data);
                }
                finally {
                    // These instances are transient
                    IDisposable disposable = dataProtector as IDisposable;
                    if (disposable != null) {
                        disposable.Dispose();
                    }
                }
            }
        }

        public byte[] Protect(byte[] clearData) {
            return PerformOperation(clearData, protect: true);
        }

        public byte[] Unprotect(byte[] protectedData) {
            return PerformOperation(protectedData, protect: false);
        }

    }
}
