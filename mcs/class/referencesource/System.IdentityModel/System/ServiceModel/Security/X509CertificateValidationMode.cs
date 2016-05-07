//-----------------------------------------------------------------------
// <copyright file="X509CertificateValidationMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// An enumeration that lists the ways of validating a certificate.
    /// </summary>
    [TypeForwardedFrom( "System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" )]
    public enum X509CertificateValidationMode
    {
        /// <summary>
        /// No validation of the certificate is performed. 
        /// </summary>
        None,

        /// <summary>
        /// The certificate is valid if it is in the trusted people store.
        /// </summary>
        PeerTrust,

        /// <summary>
        /// The certificate is valid if the chain builds to a certification authority in the trusted root store.
        /// </summary>
        ChainTrust,

        /// <summary>
        /// The certificate is valid if it is in the trusted people store, or if the chain builds to a certification authority in the trusted root store.
        /// </summary>
        PeerOrChainTrust,

        /// <summary>
        /// The user must plug in a custom <c>X509CertificateValidator</c> to validate the certificate.
        /// </summary>
        Custom
    }    
}
