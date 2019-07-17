// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Security.Cryptography
{
    public sealed partial class AesCcm : System.IDisposable
    {
        public AesCcm (byte[] key) => throw new PlatformNotSupportedException ();
        public AesCcm (System.ReadOnlySpan<byte> key) => throw new PlatformNotSupportedException ();
        public static System.Security.Cryptography.KeySizes NonceByteSizes => throw new PlatformNotSupportedException ();
        public static System.Security.Cryptography.KeySizes TagByteSizes => throw new PlatformNotSupportedException ();
        public void Decrypt (byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext, byte[] associatedData = null) => throw new PlatformNotSupportedException ();
        public void Decrypt (System.ReadOnlySpan<byte> nonce, System.ReadOnlySpan<byte> ciphertext, System.ReadOnlySpan<byte> tag, System.Span<byte> plaintext, System.ReadOnlySpan<byte> associatedData = default(System.ReadOnlySpan<byte>)) => throw new PlatformNotSupportedException ();
        public void Dispose () {}
        public void Encrypt (byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag, byte[] associatedData = null) => throw new PlatformNotSupportedException ();
        public void Encrypt (System.ReadOnlySpan<byte> nonce, System.ReadOnlySpan<byte> plaintext, System.Span<byte> ciphertext, System.Span<byte> tag, System.ReadOnlySpan<byte> associatedData = default(System.ReadOnlySpan<byte>)) => throw new PlatformNotSupportedException ();
    }
}