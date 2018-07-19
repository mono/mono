//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    using System;

    /// <summary>
    /// Defines the encryption method.
    /// </summary>
    public class EncryptionMethod
    {
        Uri _algorithm;

        /// <summary>
        /// Constructs an encryption method with the algorithm.
        /// </summary>
        /// <param name="algorithm">The encryption algorithm.</param>
        /// <exception cref="ArgumentNullException">If the algorithm is null.</exception>
        public EncryptionMethod(Uri algorithm)
        {
            if (algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
            }

            _algorithm = algorithm;
        }

        /// <summary>
        /// Gets or sets the encryption method algorithm attribute.
        /// </summary>
        /// <exception cref="ArgumentNullException">If the new value is null.</exception>
        public Uri Algorithm
        {
            get { return _algorithm; }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _algorithm = value;
            }
        }
    }
}
