// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Net.Security
{
    public delegate System.Security.Cryptography.X509Certificates.X509Certificate ServerCertificateSelectionCallback(object sender, string hostName);
    
    public readonly partial struct SslApplicationProtocol : System.IEquatable<System.Net.Security.SslApplicationProtocol>
    {
        private readonly object _dummy;
        public static readonly System.Net.Security.SslApplicationProtocol Http11;
        public static readonly System.Net.Security.SslApplicationProtocol Http2;
        public SslApplicationProtocol(byte[] protocol) { throw new PlatformNotSupportedException(); }
        public SslApplicationProtocol(string protocol) { throw new PlatformNotSupportedException(); }
        public System.ReadOnlyMemory<byte> Protocol { get { throw new PlatformNotSupportedException(); } }
        public bool Equals(System.Net.Security.SslApplicationProtocol other) { throw new PlatformNotSupportedException(); }
        public override bool Equals(object obj) { throw new PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new PlatformNotSupportedException(); }
        public static bool operator ==(System.Net.Security.SslApplicationProtocol left, System.Net.Security.SslApplicationProtocol right) { throw new PlatformNotSupportedException(); }
        public static bool operator !=(System.Net.Security.SslApplicationProtocol left, System.Net.Security.SslApplicationProtocol right) { throw new PlatformNotSupportedException(); }
        public override string ToString() { throw new PlatformNotSupportedException(); }
    }
}