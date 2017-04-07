// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics.Contracts;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Utility class to strongly type the format of key blobs used with CNG. Since all CNG APIs which
    ///     require or return a key blob format take the name as a string, we use this string wrapper class to
    ///     specifically mark which parameters and return values are expected to be key blob formats.  We also
    ///     provide a list of well known blob formats, which helps Intellisense users find a set of good blob
    ///     formats to use.
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class CngKeyBlobFormat  : IEquatable<CngKeyBlobFormat> {
        private static volatile CngKeyBlobFormat s_eccPrivate;
        private static volatile CngKeyBlobFormat s_eccPublic;
        private static volatile CngKeyBlobFormat s_eccFullPrivate;
        private static volatile CngKeyBlobFormat s_eccFullPublic;
        private static volatile CngKeyBlobFormat s_genericPrivate;
        private static volatile CngKeyBlobFormat s_genericPublic;
        private static volatile CngKeyBlobFormat s_opaqueTransport;
        private static volatile CngKeyBlobFormat s_pkcs8Private;

        private string m_format;

        public CngKeyBlobFormat(string format) {
            Contract.Ensures(!String.IsNullOrEmpty(m_format));

            if (format == null) {
                throw new ArgumentNullException("format");
            }
            if (format.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidKeyBlobFormat, format), "format");
            }

            m_format = format;
        }

        /// <summary>
        ///     Name of the blob format
        /// </summary>
        public string Format {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return m_format;
            }
        }

        public static bool operator ==(CngKeyBlobFormat left, CngKeyBlobFormat right) {
            if (Object.ReferenceEquals(left, null)) {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        [Pure]
        public static bool operator !=(CngKeyBlobFormat left, CngKeyBlobFormat right) {
            if (Object.ReferenceEquals(left, null)) {
                return !Object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        public override bool Equals(object obj) {
            Contract.Assert(m_format != null);

            return Equals(obj as CngKeyBlobFormat);
        }

        public bool Equals(CngKeyBlobFormat other) {
            if (Object.ReferenceEquals(other, null)) {
                return false;
            }

            return m_format.Equals(other.Format);
        }

        public override int GetHashCode() {
            Contract.Assert(m_format != null);
            return m_format.GetHashCode();
        }

        public override string ToString() {
            Contract.Assert(m_format != null);
            return m_format;
        }

        //
        // Well known key blob formats
        //

        public static CngKeyBlobFormat EccPrivateBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_eccPrivate == null) {
                    s_eccPrivate = new CngKeyBlobFormat("ECCPRIVATEBLOB");      // BCRYPT_ECCPRIVATE_BLOB
                }

                return s_eccPrivate;
            }
        }

        public static CngKeyBlobFormat EccPublicBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_eccPublic == null) {
                    s_eccPublic = new CngKeyBlobFormat("ECCPUBLICBLOB");        // BCRYPT_ECCPUBLIC_BLOB
                }

                return s_eccPublic;
            }
        }

        public static CngKeyBlobFormat EccFullPrivateBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_eccFullPrivate == null) {
                    s_eccFullPrivate = new CngKeyBlobFormat("ECCFULLPRIVATEBLOB"); // BCRYPT_ECCFULLPRIVATE_BLOB
                }

                return s_eccFullPrivate;
            }
        }

        public static CngKeyBlobFormat EccFullPublicBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_eccFullPublic == null) {
                    s_eccFullPublic = new CngKeyBlobFormat("ECCFULLPUBLICBLOB"); // BCRYPT_ECCFULLPUBLIC_BLOB
                }

                return s_eccFullPublic;
            }
        }

        public static CngKeyBlobFormat GenericPrivateBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_genericPrivate == null) {
                    s_genericPrivate = new CngKeyBlobFormat("PRIVATEBLOB");     // BCRYPT_PRIVATE_KEY_BLOB
                }

                return s_genericPrivate;
            }
        }

        public static CngKeyBlobFormat GenericPublicBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_genericPublic == null) {
                    s_genericPublic = new CngKeyBlobFormat("PUBLICBLOB");       // BCRYPT_PUBLIC_KEY_BLOB
                }

                return s_genericPublic;
            }
        }

        public static CngKeyBlobFormat OpaqueTransportBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_opaqueTransport == null) {
                    s_opaqueTransport = new CngKeyBlobFormat("OpaqueTransport");    // NCRYPT_OPAQUETRANSPORT_BLOB
                }

                return s_opaqueTransport;
            }
        }

        public static CngKeyBlobFormat Pkcs8PrivateBlob {
            get {
                Contract.Ensures(Contract.Result<CngKeyBlobFormat>() != null);

                if (s_pkcs8Private == null) {
                    s_pkcs8Private = new CngKeyBlobFormat("PKCS8_PRIVATEKEY");      // NCRYPT_PKCS8_PRIVATE_KEY_BLOB
                }

                return s_pkcs8Private;
            }
        }
    }
}
