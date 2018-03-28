//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides cookie integrity and confidentiality using <see cref="ProtectedData"/>.
    /// </summary>
    /// <remarks>
    /// Due to the nature of <see cref="ProtectedData"/>, cookies
    /// which use this tranform can only be read by the same machine 
    /// which wrote them. As such, this transform is not appropriate
    /// for use in applications that run on a web server farm.
    /// </remarks>
    public sealed class ProtectedDataCookieTransform : CookieTransform
    {
        const string entropyString = "System.IdentityModel.ProtectedDataCookieTransform";
        byte[] entropy;

        /// <summary>
        /// Creates a new instance of <see cref="ProtectedDataCookieTransform"/>.
        /// </summary>
        public ProtectedDataCookieTransform()
        {
            this.entropy = Encoding.UTF8.GetBytes( entropyString );
        }

        /// <summary>
        /// Verifies data protection.
        /// </summary>
        /// <param name="encoded">Data previously returned from <see cref="Encode"/></param>
        /// <returns>The originally protected data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'encoded' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'encoded' contains zero bytes.</exception>
        public override byte[] Decode( byte[] encoded )
        {
            if ( null == encoded )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "encoded" );
            }

            if ( 0 == encoded.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "encoded", SR.GetString( SR.ID6045 ) );
            }

            // CurrentUser is used here, and this has been tested as 
            // NetworkService. Using CurrentMachine allows anyone on 
            // the machine to decrypt the data, which isn't what we 
            // want.
            byte[] decoded;
            try
            {
                decoded = ProtectedData.Unprotect( encoded, this.entropy, DataProtectionScope.CurrentUser );
            }
            catch ( CryptographicException e )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString( SR.ID1073 ), e) );
            }

            return decoded;
        }

        /// <summary>
        /// Protects data.
        /// </summary>
        /// <param name="value">Data to be protected.</param>
        /// <returns>Protected data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'value' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'value' contains zero bytes.</exception>
        public override byte[] Encode( byte[] value )
        {
            if ( null == value )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
            }

            if ( 0 == value.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "value", SR.GetString( SR.ID6044 ) );
            }

            // See note in Decode about the DataProtectionScope.
            byte[] encoded;
            try
            {
                encoded = ProtectedData.Protect( value, this.entropy, DataProtectionScope.CurrentUser );
            }
            catch ( CryptographicException e )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString( SR.ID1074 ), e ) );
            }

            return encoded;
        }
    }
}
