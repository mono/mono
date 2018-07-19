//------------------------------------------------------------------------------
// <copyright file="HomogenizingCryptoServiceWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Configuration;
    using System.Security.Cryptography;

    // Wraps an ICryptoService instance and homogenizes any exceptions that might occur.

    internal sealed class HomogenizingCryptoServiceWrapper : ICryptoService {

        public HomogenizingCryptoServiceWrapper(ICryptoService wrapped) {
            WrappedCryptoService = wrapped;
        }

        internal ICryptoService WrappedCryptoService {
            get;
            private set;
        }

        private static byte[] HomogenizeErrors(Func<byte[], byte[]> func, byte[] input) {
            // If the underlying method returns null or throws an exception, the
            // error will be homogenized as a single CryptographicException.

            byte[] output = null;
            bool allowExceptionToBubble = false;

            try {
                output = func(input);
                return output;
            }
            catch (ConfigurationException) {
                // ConfigurationException isn't a side channel; it means the application is misconfigured.
                // We need to bubble this up so that the developer can react to it.
                allowExceptionToBubble = true;
                throw;
            }
            finally {
                if (output == null && !allowExceptionToBubble) {
                    throw new CryptographicException();
                }
            }
        }

        public byte[] Protect(byte[] clearData) {
            return HomogenizeErrors(WrappedCryptoService.Protect, clearData);
        }

        public byte[] Unprotect(byte[] protectedData) {
            return HomogenizeErrors(WrappedCryptoService.Unprotect, protectedData);
        }

    }
}
