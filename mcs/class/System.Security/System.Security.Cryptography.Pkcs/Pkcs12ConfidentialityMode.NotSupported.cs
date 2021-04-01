namespace System.Security.Cryptography.Pkcs {
    /// <summary>Represents the kind of encryption associated with a PKCS#12 SafeContents value.</summary>
    public enum Pkcs12ConfidentialityMode {
        /// <summary>The SafeContents value is not encrypted.</summary>
        None = 1,
        /// <summary>The SafeContents value is encrypted with a password.</summary>
        Password = 2,
        /// <summary>The SafeContents value is encrypted using public key cryptography.</summary>
        PublicKey = 3,
        /// <summary>The kind of encryption applied to the SafeContents is unknown or could not be determined.</summary>
        Unknown = 0
    }
}
