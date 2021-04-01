using System;
using System.Security.Cryptography.Pkcs;

namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the SafeContentsBag from PKCS#12, a container whose contents are a PKCS#12 SafeContents value. This class cannot be inherited.</summary>
    public sealed class Pkcs12SafeContentsBag : Pkcs12SafeBag {
        /// <summary>Gets the SafeContents value contained within this bag.</summary>
        /// <returns>The SafeContents value contained within this bag.</returns>
        public Pkcs12SafeContents SafeContents {
            get {
                throw new PlatformNotSupportedException ();
            }
        }

        internal Pkcs12SafeContentsBag ()
            : base (null, default(ReadOnlyMemory<byte>)) {
            throw new PlatformNotSupportedException ();
        }
    }
}
