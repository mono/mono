namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the type of anti-tampering applied to a PKCS#12 PFX value.</summary>
    public enum Pkcs12IntegrityMode {
        /// <summary>The PKCS#12 PFX value is not protected from tampering.</summary>
        None = 1,
        /// <summary>The PKCS#12 PFX value is protected from tampering with a Message Authentication Code (MAC) keyed with a password.</summary>
        Password = 2,
        /// <summary>The PKCS#12 PFX value is protected from tampering with a digital signature using public key cryptography.</summary>
        PublicKey = 3,
        /// <summary>The type of anti-tampering applied to the PKCS#12 PFX is unknown or could not be determined.</summary>
        Unknown = 0
    }
}
