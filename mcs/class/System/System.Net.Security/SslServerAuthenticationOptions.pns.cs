// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Net.Security
{
    public partial class SslServerAuthenticationOptions
    {
        public SslServerAuthenticationOptions() => throw new PlatformNotSupportedException();
        public bool AllowRenegotiation { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Collections.Generic.List<System.Net.Security.SslApplicationProtocol> ApplicationProtocols { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Security.Cryptography.X509Certificates.X509RevocationMode CertificateRevocationCheckMode { get { throw new PlatformNotSupportedException(); } set { } }
        public bool ClientCertificateRequired { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Security.Authentication.SslProtocols EnabledSslProtocols { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Net.Security.EncryptionPolicy EncryptionPolicy { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Net.Security.RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Security.Cryptography.X509Certificates.X509Certificate ServerCertificate { get { throw new PlatformNotSupportedException(); } set { } }
        public System.Net.Security.ServerCertificateSelectionCallback ServerCertificateSelectionCallback { get { throw new PlatformNotSupportedException(); } set { } }
    }
}