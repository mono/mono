//------------------------------------------------------------------------------
// <copyright file="ICertificatePolicy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Security.Cryptography.X509Certificates;

    // CertificatePolicy
    /// <devdoc>
    ///    <para>
    ///       Validates
    ///       a server certificate.
    ///    </para>
    /// </devdoc>
    public interface ICertificatePolicy {
        /// <devdoc>
        ///    <para>
        ///       Validates a server certificate.
        ///    </para>
        /// </devdoc>
        bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem);

    } // interface ICertificatePolicy

} // namespace System.Net
