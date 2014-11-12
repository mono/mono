// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X500Name.cs
//
// 07/10/2003
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    [Flags]
    public enum X500DistinguishedNameFlags {
        None                = 0x0000,
        Reversed            = 0x0001,

        UseSemicolons       = 0x0010,
        DoNotUsePlusSign    = 0x0020,
        DoNotUseQuotes      = 0x0040,
        UseCommas           = 0x0080,
        UseNewLines         = 0x0100,

        UseUTF8Encoding     = 0x1000,
        UseT61Encoding      = 0x2000,
        ForceUTF8Encoding   = 0x4000,
    }

    public sealed class X500DistinguishedName : AsnEncodedData {
        private string m_distinguishedName = null;

        //
        // Constructors.
        //

        internal X500DistinguishedName (CAPI.CRYPTOAPI_BLOB encodedDistinguishedNameBlob) : base (new Oid(), encodedDistinguishedNameBlob) {}

        public X500DistinguishedName (byte[] encodedDistinguishedName) : base(new Oid(), encodedDistinguishedName) {}

        public X500DistinguishedName (AsnEncodedData encodedDistinguishedName) : base(encodedDistinguishedName) {}

        public X500DistinguishedName (X500DistinguishedName distinguishedName) : base((AsnEncodedData) distinguishedName) {
            m_distinguishedName = distinguishedName.Name;
        }

        public X500DistinguishedName (string distinguishedName) : this(distinguishedName, X500DistinguishedNameFlags.Reversed) {}

        public X500DistinguishedName (string distinguishedName, X500DistinguishedNameFlags flag) : base(new Oid(), Encode(distinguishedName, flag)) {
            m_distinguishedName = distinguishedName;
        }

        //
        // Public properties.
        //

        public string Name {
            get {
                if (m_distinguishedName == null)
                    m_distinguishedName = Decode(X500DistinguishedNameFlags.Reversed);
                return m_distinguishedName;
            }
        }

        //
        // Public methods.
        //

        public string Decode (X500DistinguishedNameFlags flag) {
            uint dwStrType = CAPI.CERT_X500_NAME_STR | MapNameToStrFlag(flag);
            unsafe {
                byte[] encodedDistinguishedName = this.m_rawData;
                fixed (byte * pbEncoded = encodedDistinguishedName) {
                    CAPI.CRYPTOAPI_BLOB nameBlob;
                    IntPtr pNameBlob = new IntPtr(&nameBlob);
                    nameBlob.cbData = (uint) encodedDistinguishedName.Length;
                    nameBlob.pbData = new IntPtr(pbEncoded);

                    uint cchDecoded = CAPI.CertNameToStrW(CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                          pNameBlob,
                                                          dwStrType,
                                                          SafeLocalAllocHandle.InvalidHandle,
                                                          0);
                    if (cchDecoded == 0)
                        throw new CryptographicException(CAPI.CERT_E_INVALID_NAME);

                    using (SafeLocalAllocHandle pwszDecodeName = CAPI.LocalAlloc(CAPI.LPTR, new IntPtr(2 * cchDecoded))) {
                        if (CAPI.CertNameToStrW(CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                pNameBlob,
                                                dwStrType,
                                                pwszDecodeName,
                                                cchDecoded) == 0)
                            throw new CryptographicException(CAPI.CERT_E_INVALID_NAME);
                        return Marshal.PtrToStringUni(pwszDecodeName.DangerousGetHandle());
                    }
                }
            }
        }

        public override string Format (bool multiLine) {
            //
            // We must override to use the "numeric" pointer version of
            // CryptFormatObject, since X509 DN does not have an official OID.
            //

            // Return empty string if no data to format.
            if (m_rawData == null || m_rawData.Length == 0)
                return String.Empty;

            return CAPI.CryptFormatObject(CAPI.X509_ASN_ENCODING, 
                                          multiLine ? CAPI.CRYPT_FORMAT_STR_MULTI_LINE : 0,
                                          new IntPtr(CAPI.X509_NAME),
                                          m_rawData);
        }

        //
        // Private methods.
        //

        private unsafe static byte[] Encode (string distinguishedName, X500DistinguishedNameFlags flag) {
            if (distinguishedName == null)
                throw new ArgumentNullException("distinguishedName");

            uint cbEncoded = 0;
            uint dwStrType = CAPI.CERT_X500_NAME_STR | MapNameToStrFlag(flag);

            if (!CAPI.CertStrToNameW(CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                     distinguishedName,
                                     dwStrType,
                                     IntPtr.Zero,
                                     IntPtr.Zero,
                                     ref cbEncoded,
                                     IntPtr.Zero))
                throw new CryptographicException(Marshal.GetLastWin32Error());

            byte[] encodedName = new byte[cbEncoded];
            fixed (byte * pbEncoded = encodedName) {
                if (!CAPI.CertStrToNameW(CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                        distinguishedName,
                                        dwStrType,
                                        IntPtr.Zero,
                                        new IntPtr(pbEncoded),
                                        ref cbEncoded,
                                        IntPtr.Zero))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            return encodedName;
        }

        private static uint MapNameToStrFlag (X500DistinguishedNameFlags flag) {
            // All values or'ed together. Change this if you add values to the enumeration.
            uint allFlags = 0x71F1;
            uint dwFlags = (uint) flag;
            if ((dwFlags & ~allFlags) != 0)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Arg_EnumIllegalVal), "flag"));

            uint dwStrType = 0;
            if (dwFlags != 0) {
                if ((flag & X500DistinguishedNameFlags.Reversed) == X500DistinguishedNameFlags.Reversed)
                    dwStrType |= CAPI.CERT_NAME_STR_REVERSE_FLAG;

                if ((flag & X500DistinguishedNameFlags.UseSemicolons) == X500DistinguishedNameFlags.UseSemicolons)
                    dwStrType |= CAPI.CERT_NAME_STR_SEMICOLON_FLAG;
                else if ((flag & X500DistinguishedNameFlags.UseCommas) == X500DistinguishedNameFlags.UseCommas)
                    dwStrType |= CAPI.CERT_NAME_STR_COMMA_FLAG;
                else if ((flag & X500DistinguishedNameFlags.UseNewLines) == X500DistinguishedNameFlags.UseNewLines)
                    dwStrType |= CAPI.CERT_NAME_STR_CRLF_FLAG;

                if ((flag & X500DistinguishedNameFlags.DoNotUsePlusSign) == X500DistinguishedNameFlags.DoNotUsePlusSign)
                    dwStrType |= CAPI.CERT_NAME_STR_NO_PLUS_FLAG;
                if ((flag & X500DistinguishedNameFlags.DoNotUseQuotes) == X500DistinguishedNameFlags.DoNotUseQuotes)
                    dwStrType |= CAPI.CERT_NAME_STR_NO_QUOTING_FLAG;

                if ((flag & X500DistinguishedNameFlags.ForceUTF8Encoding) == X500DistinguishedNameFlags.ForceUTF8Encoding)
                    dwStrType |= CAPI.CERT_NAME_STR_FORCE_UTF8_DIR_STR_FLAG;

                if ((flag & X500DistinguishedNameFlags.UseUTF8Encoding) == X500DistinguishedNameFlags.UseUTF8Encoding)
                    dwStrType |= CAPI.CERT_NAME_STR_ENABLE_UTF8_UNICODE_FLAG;
                else if ((flag & X500DistinguishedNameFlags.UseT61Encoding) == X500DistinguishedNameFlags.UseT61Encoding)
                    dwStrType |= CAPI.CERT_NAME_STR_ENABLE_T61_UNICODE_FLAG;
            }
            return dwStrType;
        }
    }
}
