// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// CryptoApi.cs
//

namespace System.Security.Cryptography {
    using Microsoft.Win32.SafeHandles;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;

    using _FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    internal abstract class CAPIBase {

        //
        // PInvoke dll's.
        //

        internal const String ADVAPI32 = "advapi32.dll";
        internal const String CRYPT32  = "crypt32.dll";
        internal const String KERNEL32 = "kernel32.dll";

        //
        // Constants
        //

        internal const uint LMEM_FIXED    = 0x0000;
        internal const uint LMEM_ZEROINIT = 0x0040;
        internal const uint LPTR          = (LMEM_FIXED | LMEM_ZEROINIT);

        internal const int S_OK        = 0;
        internal const int S_FALSE     = 1;

        internal const uint FORMAT_MESSAGE_FROM_SYSTEM    = 0x00001000;
        internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        internal const uint VER_PLATFORM_WIN32s        = 0;
        internal const uint VER_PLATFORM_WIN32_WINDOWS = 1;
        internal const uint VER_PLATFORM_WIN32_NT      = 2;
        internal const uint VER_PLATFORM_WINCE         = 3;

        // ASN.
        internal const uint ASN_TAG_NULL    = 0x05;
        internal const uint ASN_TAG_OBJID   = 0x06;

        // cert query object types.
        internal const uint CERT_QUERY_OBJECT_FILE = 1;
        internal const uint CERT_QUERY_OBJECT_BLOB = 2;

        // cert query content types.
        internal const uint CERT_QUERY_CONTENT_CERT               = 1;
        internal const uint CERT_QUERY_CONTENT_CTL                = 2;
        internal const uint CERT_QUERY_CONTENT_CRL                = 3;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_STORE   = 4;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CERT    = 5;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CTL     = 6;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CRL     = 7;
        internal const uint CERT_QUERY_CONTENT_PKCS7_SIGNED       = 8;
        internal const uint CERT_QUERY_CONTENT_PKCS7_UNSIGNED     = 9;
        internal const uint CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10;
        internal const uint CERT_QUERY_CONTENT_PKCS10             = 11;
        internal const uint CERT_QUERY_CONTENT_PFX                = 12;
        internal const uint CERT_QUERY_CONTENT_CERT_PAIR          = 13;

        // cert query content flags.
        internal const uint CERT_QUERY_CONTENT_FLAG_CERT                    = (1 << (int) CERT_QUERY_CONTENT_CERT);
        internal const uint CERT_QUERY_CONTENT_FLAG_CTL                     = (1 << (int) CERT_QUERY_CONTENT_CTL);
        internal const uint CERT_QUERY_CONTENT_FLAG_CRL                     = (1 << (int) CERT_QUERY_CONTENT_CRL);
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE        = (1 << (int) CERT_QUERY_CONTENT_SERIALIZED_STORE);
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT         = (1 << (int) CERT_QUERY_CONTENT_SERIALIZED_CERT);
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL          = (1 << (int) CERT_QUERY_CONTENT_SERIALIZED_CTL);
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL          = (1 << (int) CERT_QUERY_CONTENT_SERIALIZED_CRL);
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED            = (1 << (int) CERT_QUERY_CONTENT_PKCS7_SIGNED);
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED          = (1 << (int) CERT_QUERY_CONTENT_PKCS7_UNSIGNED);
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED      = (1 << (int) CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED);
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS10                  = (1 << (int) CERT_QUERY_CONTENT_PKCS10);
        internal const uint CERT_QUERY_CONTENT_FLAG_PFX                     = (1 << (int) CERT_QUERY_CONTENT_PFX);
        internal const uint CERT_QUERY_CONTENT_FLAG_CERT_PAIR               = (1 << (int) CERT_QUERY_CONTENT_CERT_PAIR);
        internal const uint CERT_QUERY_CONTENT_FLAG_ALL                     =
                                       (CERT_QUERY_CONTENT_FLAG_CERT |
                                        CERT_QUERY_CONTENT_FLAG_CTL |
                                        CERT_QUERY_CONTENT_FLAG_CRL |
                                        CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE |
                                        CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT |
                                        CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL |
                                        CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL |
                                        CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED |
                                        CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED |
                                        CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED |
                                        CERT_QUERY_CONTENT_FLAG_PKCS10 |
                                        CERT_QUERY_CONTENT_FLAG_PFX |
                                        CERT_QUERY_CONTENT_FLAG_CERT_PAIR);

        internal const uint CERT_QUERY_FORMAT_BINARY                = 1;
        internal const uint CERT_QUERY_FORMAT_BASE64_ENCODED        = 2;
        internal const uint CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;

        internal const uint CERT_QUERY_FORMAT_FLAG_BINARY                   = (1 << (int) CERT_QUERY_FORMAT_BINARY);
        internal const uint CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED           = (1 << (int) CERT_QUERY_FORMAT_BASE64_ENCODED);
        internal const uint CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED    = (1 << (int) CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED);
        internal const uint CERT_QUERY_FORMAT_FLAG_ALL                      =
                                       (CERT_QUERY_FORMAT_FLAG_BINARY |
                                        CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED |
                                        CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED);

        // OID key type.
        internal const uint CRYPT_OID_INFO_OID_KEY   = 1;
        internal const uint CRYPT_OID_INFO_NAME_KEY  = 2;
        internal const uint CRYPT_OID_INFO_ALGID_KEY = 3;
        internal const uint CRYPT_OID_INFO_SIGN_KEY  = 4;

        // OID group Id's.
        internal const uint CRYPT_HASH_ALG_OID_GROUP_ID     = 1;
        internal const uint CRYPT_ENCRYPT_ALG_OID_GROUP_ID  = 2;
        internal const uint CRYPT_PUBKEY_ALG_OID_GROUP_ID   = 3;
        internal const uint CRYPT_SIGN_ALG_OID_GROUP_ID     = 4;
        internal const uint CRYPT_RDN_ATTR_OID_GROUP_ID     = 5;
        internal const uint CRYPT_EXT_OR_ATTR_OID_GROUP_ID  = 6;
        internal const uint CRYPT_ENHKEY_USAGE_OID_GROUP_ID = 7;
        internal const uint CRYPT_POLICY_OID_GROUP_ID       = 8;
        internal const uint CRYPT_TEMPLATE_OID_GROUP_ID     = 9;
        internal const uint CRYPT_LAST_OID_GROUP_ID         = 9;

        internal const uint CRYPT_FIRST_ALG_OID_GROUP_ID    = CRYPT_HASH_ALG_OID_GROUP_ID;
        internal const uint CRYPT_LAST_ALG_OID_GROUP_ID     = CRYPT_SIGN_ALG_OID_GROUP_ID;

        // cert encoding flags.
        internal const uint CRYPT_ASN_ENCODING          = 0x00000001;
        internal const uint CRYPT_NDR_ENCODING          = 0x00000002;
        internal const uint X509_ASN_ENCODING           = 0x00000001;
        internal const uint X509_NDR_ENCODING           = 0x00000002;
        internal const uint PKCS_7_ASN_ENCODING         = 0x00010000;
        internal const uint PKCS_7_NDR_ENCODING         = 0x00020000;
        internal const uint PKCS_7_OR_X509_ASN_ENCODING = (PKCS_7_ASN_ENCODING | X509_ASN_ENCODING);

        // cert store provider
        internal const uint CERT_STORE_PROV_MSG                = 1;
        internal const uint CERT_STORE_PROV_MEMORY             = 2;
        internal const uint CERT_STORE_PROV_FILE               = 3;
        internal const uint CERT_STORE_PROV_REG                = 4;
        internal const uint CERT_STORE_PROV_PKCS7              = 5;
        internal const uint CERT_STORE_PROV_SERIALIZED         = 6;
        internal const uint CERT_STORE_PROV_FILENAME_A         = 7;
        internal const uint CERT_STORE_PROV_FILENAME_W         = 8;
        internal const uint CERT_STORE_PROV_FILENAME           = CERT_STORE_PROV_FILENAME_W;
        internal const uint CERT_STORE_PROV_SYSTEM_A           = 9;
        internal const uint CERT_STORE_PROV_SYSTEM_W           = 10;
        internal const uint CERT_STORE_PROV_SYSTEM             = CERT_STORE_PROV_SYSTEM_W;
        internal const uint CERT_STORE_PROV_COLLECTION         = 11;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY_A  = 12;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY_W  = 13;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY    = CERT_STORE_PROV_SYSTEM_REGISTRY_W;
        internal const uint CERT_STORE_PROV_PHYSICAL_W         = 14;
        internal const uint CERT_STORE_PROV_PHYSICAL           = CERT_STORE_PROV_PHYSICAL_W;
        internal const uint CERT_STORE_PROV_SMART_CARD_W       = 15;
        internal const uint CERT_STORE_PROV_SMART_CARD         = CERT_STORE_PROV_SMART_CARD_W;
        internal const uint CERT_STORE_PROV_LDAP_W             = 16;
        internal const uint CERT_STORE_PROV_LDAP               = CERT_STORE_PROV_LDAP_W;

        // cert store flags
        internal const uint CERT_STORE_NO_CRYPT_RELEASE_FLAG            = 0x00000001;
        internal const uint CERT_STORE_SET_LOCALIZED_NAME_FLAG          = 0x00000002;
        internal const uint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004;
        internal const uint CERT_STORE_DELETE_FLAG                      = 0x00000010;
        internal const uint CERT_STORE_SHARE_STORE_FLAG                 = 0x00000040;
        internal const uint CERT_STORE_SHARE_CONTEXT_FLAG               = 0x00000080;
        internal const uint CERT_STORE_MANIFOLD_FLAG                    = 0x00000100;
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG               = 0x00000200;
        internal const uint CERT_STORE_UPDATE_KEYID_FLAG                = 0x00000400;
        internal const uint CERT_STORE_BACKUP_RESTORE_FLAG              = 0x00000800;
        internal const uint CERT_STORE_READONLY_FLAG                    = 0x00008000;
        internal const uint CERT_STORE_OPEN_EXISTING_FLAG               = 0x00004000;
        internal const uint CERT_STORE_CREATE_NEW_FLAG                  = 0x00002000;
        internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG             = 0x00001000;

        // cert store location
        internal const uint CERT_SYSTEM_STORE_UNPROTECTED_FLAG              = 0x40000000;
        internal const uint CERT_SYSTEM_STORE_LOCATION_MASK                 = 0x00FF0000;
        internal const uint CERT_SYSTEM_STORE_LOCATION_SHIFT                = 16;

        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_ID               = 1;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ID              = 2;
        internal const uint CERT_SYSTEM_STORE_CURRENT_SERVICE_ID            = 4;
        internal const uint CERT_SYSTEM_STORE_SERVICES_ID                   = 5;
        internal const uint CERT_SYSTEM_STORE_USERS_ID                      = 6;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID  = 7;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID = 8;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID   = 9;

        internal const uint CERT_SYSTEM_STORE_CURRENT_USER                  = ((int) CERT_SYSTEM_STORE_CURRENT_USER_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE                 = ((int) CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_CURRENT_SERVICE               = ((int) CERT_SYSTEM_STORE_CURRENT_SERVICE_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_SERVICES                      = ((int) CERT_SYSTEM_STORE_SERVICES_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_USERS                         = ((int) CERT_SYSTEM_STORE_USERS_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY     = ((int) CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY    = ((int) CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE      = ((int) CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);

        // cert name types.
        internal const uint CERT_NAME_EMAIL_TYPE            = 1;
        internal const uint CERT_NAME_RDN_TYPE              = 2;
        internal const uint CERT_NAME_ATTR_TYPE             = 3;
        internal const uint CERT_NAME_SIMPLE_DISPLAY_TYPE   = 4;
        internal const uint CERT_NAME_FRIENDLY_DISPLAY_TYPE = 5;
        internal const uint CERT_NAME_DNS_TYPE              = 6;
        internal const uint CERT_NAME_URL_TYPE              = 7;
        internal const uint CERT_NAME_UPN_TYPE              = 8;

        // cert name flags.
        internal const uint CERT_SIMPLE_NAME_STR                   = 1;
        internal const uint CERT_OID_NAME_STR                      = 2;
        internal const uint CERT_X500_NAME_STR                     = 3;

        internal const uint CERT_NAME_STR_SEMICOLON_FLAG           = 0x40000000;
        internal const uint CERT_NAME_STR_NO_PLUS_FLAG             = 0x20000000;
        internal const uint CERT_NAME_STR_NO_QUOTING_FLAG          = 0x10000000;
        internal const uint CERT_NAME_STR_CRLF_FLAG                = 0x08000000;
        internal const uint CERT_NAME_STR_COMMA_FLAG               = 0x04000000;
        internal const uint CERT_NAME_STR_REVERSE_FLAG             = 0x02000000;

        internal const uint CERT_NAME_ISSUER_FLAG                  = 0x1;
        internal const uint CERT_NAME_STR_DISABLE_IE4_UTF8_FLAG    = 0x00010000;
        internal const uint CERT_NAME_STR_ENABLE_T61_UNICODE_FLAG  = 0x00020000;
        internal const uint CERT_NAME_STR_ENABLE_UTF8_UNICODE_FLAG = 0x00040000;
        internal const uint CERT_NAME_STR_FORCE_UTF8_DIR_STR_FLAG  = 0x00080000;

        // cert context property Id's.
        internal const uint CERT_KEY_PROV_HANDLE_PROP_ID                = 1;
        internal const uint CERT_KEY_PROV_INFO_PROP_ID                  = 2;
        internal const uint CERT_SHA1_HASH_PROP_ID                      = 3;
        internal const uint CERT_MD5_HASH_PROP_ID                       = 4;
        internal const uint CERT_HASH_PROP_ID                           = CERT_SHA1_HASH_PROP_ID;
        internal const uint CERT_KEY_CONTEXT_PROP_ID                    = 5;
        internal const uint CERT_KEY_SPEC_PROP_ID                       = 6;
        internal const uint CERT_IE30_RESERVED_PROP_ID                  = 7;
        internal const uint CERT_PUBKEY_HASH_RESERVED_PROP_ID           = 8;
        internal const uint CERT_ENHKEY_USAGE_PROP_ID                   = 9;
        internal const uint CERT_CTL_USAGE_PROP_ID                      = CERT_ENHKEY_USAGE_PROP_ID;
        internal const uint CERT_NEXT_UPDATE_LOCATION_PROP_ID           = 10;
        internal const uint CERT_FRIENDLY_NAME_PROP_ID                  = 11;
        internal const uint CERT_PVK_FILE_PROP_ID                       = 12;
        internal const uint CERT_DESCRIPTION_PROP_ID                    = 13;
        internal const uint CERT_ACCESS_STATE_PROP_ID                   = 14;
        internal const uint CERT_SIGNATURE_HASH_PROP_ID                 = 15;
        internal const uint CERT_SMART_CARD_DATA_PROP_ID                = 16;
        internal const uint CERT_EFS_PROP_ID                            = 17;
        internal const uint CERT_FORTEZZA_DATA_PROP_ID                  = 18;
        internal const uint CERT_ARCHIVED_PROP_ID                       = 19;
        internal const uint CERT_KEY_IDENTIFIER_PROP_ID                 = 20;
        internal const uint CERT_AUTO_ENROLL_PROP_ID                    = 21;
        internal const uint CERT_PUBKEY_ALG_PARA_PROP_ID                = 22;
        internal const uint CERT_CROSS_CERT_DIST_POINTS_PROP_ID         = 23;
        internal const uint CERT_ISSUER_PUBLIC_KEY_MD5_HASH_PROP_ID     = 24;
        internal const uint CERT_SUBJECT_PUBLIC_KEY_MD5_HASH_PROP_ID    = 25;
        internal const uint CERT_ENROLLMENT_PROP_ID                     = 26;
        internal const uint CERT_DATE_STAMP_PROP_ID                     = 27;
        internal const uint CERT_ISSUER_SERIAL_NUMBER_MD5_HASH_PROP_ID  = 28;
        internal const uint CERT_SUBJECT_NAME_MD5_HASH_PROP_ID          = 29;
        internal const uint CERT_EXTENDED_ERROR_INFO_PROP_ID            = 30;
        internal const uint CERT_RENEWAL_PROP_ID                        = 64;
        internal const uint CERT_ARCHIVED_KEY_HASH_PROP_ID              = 65;
        internal const uint CERT_FIRST_RESERVED_PROP_ID                 = 66;

        // This value shall be defined in wincrypt.h so we avoid conflicts
        internal const uint CERT_DELETE_KEYSET_PROP_ID                 = 101;

        // CertSetCertificateContextProperty flags.
        internal const uint CERT_SET_PROPERTY_IGNORE_PERSIST_ERROR_FLAG = 0x80000000;
        internal const uint CERT_SET_PROPERTY_INHIBIT_PERSIST_FLAG      = 0x40000000;

        // cert info flags.
        internal const uint CERT_INFO_VERSION_FLAG                 = 1;
        internal const uint CERT_INFO_SERIAL_NUMBER_FLAG           = 2;
        internal const uint CERT_INFO_SIGNATURE_ALGORITHM_FLAG     = 3;
        internal const uint CERT_INFO_ISSUER_FLAG                  = 4;
        internal const uint CERT_INFO_NOT_BEFORE_FLAG              = 5;
        internal const uint CERT_INFO_NOT_AFTER_FLAG               = 6;
        internal const uint CERT_INFO_SUBJECT_FLAG                 = 7;
        internal const uint CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG = 8;
        internal const uint CERT_INFO_ISSUER_UNIQUE_ID_FLAG        = 9;
        internal const uint CERT_INFO_SUBJECT_UNIQUE_ID_FLAG       = 10;
        internal const uint CERT_INFO_EXTENSION_FLAG               = 11;

        // cert compare flags.
        internal const uint CERT_COMPARE_MASK                   = 0xFFFF;
        internal const uint CERT_COMPARE_SHIFT                  = 16;
        internal const uint CERT_COMPARE_ANY                    = 0;
        internal const uint CERT_COMPARE_SHA1_HASH              = 1;
        internal const uint CERT_COMPARE_NAME                   = 2;
        internal const uint CERT_COMPARE_ATTR                   = 3;
        internal const uint CERT_COMPARE_MD5_HASH               = 4;
        internal const uint CERT_COMPARE_PROPERTY               = 5;
        internal const uint CERT_COMPARE_PUBLIC_KEY             = 6;
        internal const uint CERT_COMPARE_HASH                   = CERT_COMPARE_SHA1_HASH;
        internal const uint CERT_COMPARE_NAME_STR_A             = 7;
        internal const uint CERT_COMPARE_NAME_STR_W             = 8;
        internal const uint CERT_COMPARE_KEY_SPEC               = 9;
        internal const uint CERT_COMPARE_ENHKEY_USAGE           = 10;
        internal const uint CERT_COMPARE_CTL_USAGE              = CERT_COMPARE_ENHKEY_USAGE;
        internal const uint CERT_COMPARE_SUBJECT_CERT           = 11;
        internal const uint CERT_COMPARE_ISSUER_OF              = 12;
        internal const uint CERT_COMPARE_EXISTING               = 13;
        internal const uint CERT_COMPARE_SIGNATURE_HASH         = 14;
        internal const uint CERT_COMPARE_KEY_IDENTIFIER         = 15;
        internal const uint CERT_COMPARE_CERT_ID                = 16;
        internal const uint CERT_COMPARE_CROSS_CERT_DIST_POINTS = 17;
        internal const uint CERT_COMPARE_PUBKEY_MD5_HASH        = 18;

        // cert find flags.
        internal const uint CERT_FIND_ANY                    = ((int) CERT_COMPARE_ANY << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_SHA1_HASH              = ((int) CERT_COMPARE_SHA1_HASH << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_MD5_HASH               = ((int) CERT_COMPARE_MD5_HASH << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_SIGNATURE_HASH         = ((int) CERT_COMPARE_SIGNATURE_HASH << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_KEY_IDENTIFIER         = ((int) CERT_COMPARE_KEY_IDENTIFIER << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_HASH                   = CERT_FIND_SHA1_HASH;
        internal const uint CERT_FIND_PROPERTY               = ((int) CERT_COMPARE_PROPERTY << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_PUBLIC_KEY             = ((int) CERT_COMPARE_PUBLIC_KEY << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_SUBJECT_NAME           = ((int) CERT_COMPARE_NAME << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_SUBJECT_ATTR           = ((int) CERT_COMPARE_ATTR << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_ISSUER_NAME            = ((int) CERT_COMPARE_NAME << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_ISSUER_ATTR            = ((int) CERT_COMPARE_ATTR << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR_A          = ((int) CERT_COMPARE_NAME_STR_A << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR_W          = ((int) CERT_COMPARE_NAME_STR_W << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_SUBJECT_FLAG);
        internal const uint CERT_FIND_SUBJECT_STR            = CERT_FIND_SUBJECT_STR_W;
        internal const uint CERT_FIND_ISSUER_STR_A           = ((int) CERT_COMPARE_NAME_STR_A << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_ISSUER_STR_W           = ((int) CERT_COMPARE_NAME_STR_W << (int) CERT_COMPARE_SHIFT | (int) CERT_INFO_ISSUER_FLAG);
        internal const uint CERT_FIND_ISSUER_STR             = CERT_FIND_ISSUER_STR_W;
        internal const uint CERT_FIND_KEY_SPEC               = ((int) CERT_COMPARE_KEY_SPEC << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_ENHKEY_USAGE           = ((int) CERT_COMPARE_ENHKEY_USAGE << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_CTL_USAGE              = CERT_FIND_ENHKEY_USAGE;
        internal const uint CERT_FIND_SUBJECT_CERT           = ((int) CERT_COMPARE_SUBJECT_CERT << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_ISSUER_OF              = ((int) CERT_COMPARE_ISSUER_OF << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_EXISTING               = ((int) CERT_COMPARE_EXISTING << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_CERT_ID                = ((int) CERT_COMPARE_CERT_ID << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_CROSS_CERT_DIST_POINTS = ((int) CERT_COMPARE_CROSS_CERT_DIST_POINTS << (int) CERT_COMPARE_SHIFT);
        internal const uint CERT_FIND_PUBKEY_MD5_HASH        = ((int) CERT_COMPARE_PUBKEY_MD5_HASH << (int) CERT_COMPARE_SHIFT);

        // cert key usage flags.
        internal const uint CERT_ENCIPHER_ONLY_KEY_USAGE     = 0x0001;
        internal const uint CERT_CRL_SIGN_KEY_USAGE          = 0x0002;
        internal const uint CERT_KEY_CERT_SIGN_KEY_USAGE     = 0x0004;
        internal const uint CERT_KEY_AGREEMENT_KEY_USAGE     = 0x0008;
        internal const uint CERT_DATA_ENCIPHERMENT_KEY_USAGE = 0x0010;
        internal const uint CERT_KEY_ENCIPHERMENT_KEY_USAGE  = 0x0020;
        internal const uint CERT_NON_REPUDIATION_KEY_USAGE   = 0x0040;
        internal const uint CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x0080;
        internal const uint CERT_DECIPHER_ONLY_KEY_USAGE     = 0x8000;

        // Add certificate/CRL, encoded, context or element disposition values.
        internal const uint CERT_STORE_ADD_NEW                                 = 1;
        internal const uint CERT_STORE_ADD_USE_EXISTING                        = 2;
        internal const uint CERT_STORE_ADD_REPLACE_EXISTING                    = 3;
        internal const uint CERT_STORE_ADD_ALWAYS                              = 4;
        internal const uint CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES = 5;
        internal const uint CERT_STORE_ADD_NEWER                               = 6;
        internal const uint CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES            = 7;

        // constants for dwFormatStrType of function CryptFormatObject
        internal const uint CRYPT_FORMAT_STR_MULTI_LINE = 0x0001;
        internal const uint CRYPT_FORMAT_STR_NO_HEX     = 0x0010;

        // store save as type.
        internal const uint CERT_STORE_SAVE_AS_STORE = 1;
        internal const uint CERT_STORE_SAVE_AS_PKCS7 = 2;

        // store save to type.
        internal const uint CERT_STORE_SAVE_TO_FILE       = 1;
        internal const uint CERT_STORE_SAVE_TO_MEMORY     = 2;
        internal const uint CERT_STORE_SAVE_TO_FILENAME_A = 3;
        internal const uint CERT_STORE_SAVE_TO_FILENAME_W = 4;
        internal const uint CERT_STORE_SAVE_TO_FILENAME   = CERT_STORE_SAVE_TO_FILENAME_W;

        // flags for CERT_BASIC_CONSTRAINTS_INFO.SubjectType
        internal const uint CERT_CA_SUBJECT_FLAG         = 0x80;
        internal const uint CERT_END_ENTITY_SUBJECT_FLAG = 0x40;

        // dwFlags definitions for PFXExportCertStoreEx.
        internal const uint REPORT_NO_PRIVATE_KEY                 = 0x0001;
        internal const uint REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY = 0x0002;
        internal const uint EXPORT_PRIVATE_KEYS                   = 0x0004;
        internal const uint PKCS12_EXPORT_RESERVED_MASK           = 0xffff0000;

        // Predefined primitive data structures that can be encoded / decoded.
        internal const uint RSA_CSP_PUBLICKEYBLOB = 19;
        internal const uint X509_MULTI_BYTE_UINT  = 38;
        internal const uint X509_DSS_PUBLICKEY    = X509_MULTI_BYTE_UINT;
        internal const uint X509_DSS_PARAMETERS   = 39;
        internal const uint X509_DSS_SIGNATURE    = 40;

        // Object Identifiers short hand.
        internal const uint X509_EXTENSIONS            = 5;
        internal const uint X509_NAME_VALUE            = 6;
        internal const uint X509_NAME                  = 7;
        internal const uint X509_AUTHORITY_KEY_ID      = 9;
        internal const uint X509_KEY_USAGE_RESTRICTION = 11;
        internal const uint X509_BASIC_CONSTRAINTS     = 13;
        internal const uint X509_KEY_USAGE             = 14;
        internal const uint X509_BASIC_CONSTRAINTS2    = 15;
        internal const uint X509_CERT_POLICIES         = 16;
        internal const uint PKCS_UTC_TIME              = 17;
        internal const uint PKCS_ATTRIBUTE             = 22;
        internal const uint X509_UNICODE_NAME_VALUE    = 24;
        internal const uint X509_OCTET_STRING          = 25;
        internal const uint X509_BITS                  = 26;
        internal const uint X509_ANY_STRING            = X509_NAME_VALUE;
        internal const uint X509_UNICODE_ANY_STRING    = X509_UNICODE_NAME_VALUE;
        internal const uint X509_ENHANCED_KEY_USAGE    = 36;
        internal const uint PKCS_RC2_CBC_PARAMETERS    = 41;
        internal const uint X509_CERTIFICATE_TEMPLATE  = 64;
        internal const uint PKCS7_SIGNER_INFO          = 500;
        internal const uint CMS_SIGNER_INFO            = 501;

        internal const string szOID_COMMON_NAME               = "2.5.4.3";
        internal const string szOID_AUTHORITY_KEY_IDENTIFIER  = "2.5.29.1";
        internal const string szOID_KEY_USAGE_RESTRICTION     = "2.5.29.4";
        internal const string szOID_SUBJECT_ALT_NAME          = "2.5.29.7";
        internal const string szOID_ISSUER_ALT_NAME           = "2.5.29.8";
        internal const string szOID_BASIC_CONSTRAINTS         = "2.5.29.10";
        internal const string szOID_SUBJECT_KEY_IDENTIFIER    = "2.5.29.14";
        internal const string szOID_KEY_USAGE                 = "2.5.29.15";
        internal const string szOID_SUBJECT_ALT_NAME2         = "2.5.29.17";
        internal const string szOID_ISSUER_ALT_NAME2          = "2.5.29.18";
        internal const string szOID_BASIC_CONSTRAINTS2        = "2.5.29.19";
        internal const string szOID_CRL_DIST_POINTS           = "2.5.29.31";
        internal const string szOID_CERT_POLICIES             = "2.5.29.32";
        internal const string szOID_ENHANCED_KEY_USAGE        = "2.5.29.37";
        internal const string szOID_KEYID_RDN                 = "1.3.6.1.4.1.311.10.7.1";
        internal const string szOID_ENROLL_CERTTYPE_EXTENSION = "1.3.6.1.4.1.311.20.2";
        internal const string szOID_NT_PRINCIPAL_NAME         = "1.3.6.1.4.1.311.20.2.3";
        internal const string szOID_CERTIFICATE_TEMPLATE      = "1.3.6.1.4.1.311.21.7";
        internal const string szOID_RDN_DUMMY_SIGNER          = "1.3.6.1.4.1.311.21.9";
        internal const string szOID_AUTHORITY_INFO_ACCESS     = "1.3.6.1.5.5.7.1.1";

        // Predefined verify chain policies
        internal const uint CERT_CHAIN_POLICY_BASE              = 1;
        internal const uint CERT_CHAIN_POLICY_AUTHENTICODE      = 2;
        internal const uint CERT_CHAIN_POLICY_AUTHENTICODE_TS   = 3;
        internal const uint CERT_CHAIN_POLICY_SSL               = 4;
        internal const uint CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;
        internal const uint CERT_CHAIN_POLICY_NT_AUTH           = 6;
        internal const uint CERT_CHAIN_POLICY_MICROSOFT_ROOT    = 7;

        // Default usage match type is AND with value zero
        internal const uint USAGE_MATCH_TYPE_AND = 0x00000000;
        internal const uint USAGE_MATCH_TYPE_OR  = 0x00000001;

        // Common chain policy flags.
        internal const uint CERT_CHAIN_REVOCATION_CHECK_END_CERT           = 0x10000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN              = 0x20000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x40000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY         = 0x80000000;
        internal const uint CERT_CHAIN_REVOCATION_ACCUMULATIVE_TIMEOUT     = 0x08000000;

        // These can be applied to certificates and chains
        internal const uint CERT_TRUST_NO_ERROR                            = 0x00000000;
        internal const uint CERT_TRUST_IS_NOT_TIME_VALID                   = 0x00000001;
        internal const uint CERT_TRUST_IS_NOT_TIME_NESTED                  = 0x00000002;
        internal const uint CERT_TRUST_IS_REVOKED                          = 0x00000004;
        internal const uint CERT_TRUST_IS_NOT_SIGNATURE_VALID              = 0x00000008;
        internal const uint CERT_TRUST_IS_NOT_VALID_FOR_USAGE              = 0x00000010;
        internal const uint CERT_TRUST_IS_UNTRUSTED_ROOT                   = 0x00000020;
        internal const uint CERT_TRUST_REVOCATION_STATUS_UNKNOWN           = 0x00000040;
        internal const uint CERT_TRUST_IS_CYCLIC                           = 0x00000080;

        internal const uint CERT_TRUST_INVALID_EXTENSION                   = 0x00000100;
        internal const uint CERT_TRUST_INVALID_POLICY_CONSTRAINTS          = 0x00000200;
        internal const uint CERT_TRUST_INVALID_BASIC_CONSTRAINTS           = 0x00000400;
        internal const uint CERT_TRUST_INVALID_NAME_CONSTRAINTS            = 0x00000800;
        internal const uint CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT   = 0x00001000;
        internal const uint CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT     = 0x00002000;
        internal const uint CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT   = 0x00004000;
        internal const uint CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT        = 0x00008000;

        internal const uint CERT_TRUST_IS_OFFLINE_REVOCATION               = 0x01000000;
        internal const uint CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY            = 0x02000000;

        // These can be applied to chains only
        internal const uint CERT_TRUST_IS_PARTIAL_CHAIN                    = 0x00010000;
        internal const uint CERT_TRUST_CTL_IS_NOT_TIME_VALID               = 0x00020000;
        internal const uint CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID          = 0x00040000;
        internal const uint CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE          = 0x00080000;

        // Common chain policy flags
        internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG            = 0x00000001;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG        = 0x00000002;
        internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG           = 0x00000004;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_BASIC_CONSTRAINTS_FLAG = 0x00000008;

        internal const uint CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG                 = 0x00000010;
        internal const uint CERT_CHAIN_POLICY_IGNORE_WRONG_USAGE_FLAG               = 0x00000020;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_NAME_FLAG              = 0x00000040;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_POLICY_FLAG            = 0x00000080;

        internal const uint CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG           = 0x00000100;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG    = 0x00000200;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG            = 0x00000400;
        internal const uint CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG          = 0x00000800;

        internal const uint CERT_CHAIN_POLICY_IGNORE_ALL_REV_UNKNOWN_FLAGS = (
                                                CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG        | 
                                                CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG | 
                                                CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG         | 
                                                CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG);

        // The following are info status bits

        // These can be applied to certificates only
        internal const uint CERT_TRUST_HAS_EXACT_MATCH_ISSUER      = 0x00000001;
        internal const uint CERT_TRUST_HAS_KEY_MATCH_ISSUER        = 0x00000002;
        internal const uint CERT_TRUST_HAS_NAME_MATCH_ISSUER       = 0x00000004;
        internal const uint CERT_TRUST_IS_SELF_SIGNED              = 0x00000008;

        // These can be applied to certificates and chains
        internal const uint CERT_TRUST_HAS_PREFERRED_ISSUER        = 0x00000100;
        internal const uint CERT_TRUST_HAS_ISSUANCE_CHAIN_POLICY   = 0x00000200;
        internal const uint CERT_TRUST_HAS_VALID_NAME_CONSTRAINTS  = 0x00000400;

        // These can be applied to chains only
        internal const uint CERT_TRUST_IS_COMPLEX_CHAIN            = 0x00010000;

        // Signature value that only contains the hash octets. The parameters for
        // this algorithm must be present and must be encoded as NULL.
        internal const string szOID_PKIX_NO_SIGNATURE        = "1.3.6.1.5.5.7.6.2";

        // Consistent key usage bits: DIGITAL_SIGNATURE, KEY_ENCIPHERMENT or KEY_AGREEMENT
        internal const string szOID_PKIX_KP_SERVER_AUTH      = "1.3.6.1.5.5.7.3.1";
        // Consistent key usage bits: DIGITAL_SIGNATURE
        internal const string szOID_PKIX_KP_CLIENT_AUTH      = "1.3.6.1.5.5.7.3.2";
        // Consistent key usage bits: DIGITAL_SIGNATURE
        internal const string szOID_PKIX_KP_CODE_SIGNING     = "1.3.6.1.5.5.7.3.3";
        // Consistent key usage bits: DIGITAL_SIGNATURE, NON_REPUDIATION and/or (KEY_ENCIPHERMENT or KEY_AGREEMENT)
        internal const string szOID_PKIX_KP_EMAIL_PROTECTION = "1.3.6.1.5.5.7.3.4";

        internal const string SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.21";
        internal const string SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.22";

        // CertGetCertificateChain chain engine handles.
        internal const uint HCCE_CURRENT_USER  = 0x0;
        internal const uint HCCE_LOCAL_MACHINE = 0x1;

        // PKCS.
        internal const string szOID_PKCS_1            = "1.2.840.113549.1.1";
        internal const string szOID_PKCS_2            = "1.2.840.113549.1.2";
        internal const string szOID_PKCS_3            = "1.2.840.113549.1.3";
        internal const string szOID_PKCS_4            = "1.2.840.113549.1.4";
        internal const string szOID_PKCS_5            = "1.2.840.113549.1.5";
        internal const string szOID_PKCS_6            = "1.2.840.113549.1.6";
        internal const string szOID_PKCS_7            = "1.2.840.113549.1.7";
        internal const string szOID_PKCS_8            = "1.2.840.113549.1.8";
        internal const string szOID_PKCS_9            = "1.2.840.113549.1.9";
        internal const string szOID_PKCS_10           = "1.2.840.113549.1.10";
        internal const string szOID_PKCS_12           = "1.2.840.113549.1.12";

        // PKCS7 Content Types.
        internal const string szOID_RSA_data          = "1.2.840.113549.1.7.1";
        internal const string szOID_RSA_signedData    = "1.2.840.113549.1.7.2";
        internal const string szOID_RSA_envelopedData = "1.2.840.113549.1.7.3";
        internal const string szOID_RSA_signEnvData   = "1.2.840.113549.1.7.4";
        internal const string szOID_RSA_digestedData  = "1.2.840.113549.1.7.5";
        internal const string szOID_RSA_hashedData    = "1.2.840.113549.1.7.5";
        internal const string szOID_RSA_encryptedData = "1.2.840.113549.1.7.6";

        // PKCS9 Attributes.
        internal const string szOID_RSA_emailAddr     = "1.2.840.113549.1.9.1";
        internal const string szOID_RSA_unstructName  = "1.2.840.113549.1.9.2";
        internal const string szOID_RSA_contentType   = "1.2.840.113549.1.9.3";
        internal const string szOID_RSA_messageDigest = "1.2.840.113549.1.9.4";
        internal const string szOID_RSA_signingTime   = "1.2.840.113549.1.9.5";
        internal const string szOID_RSA_counterSign   = "1.2.840.113549.1.9.6";
        internal const string szOID_RSA_challengePwd  = "1.2.840.113549.1.9.7";
        internal const string szOID_RSA_unstructAddr  = "1.2.840.113549.1.9.8";
        internal const string szOID_RSA_extCertAttrs  = "1.2.840.113549.1.9.9";
        internal const string szOID_RSA_SMIMECapabilities       = "1.2.840.113549.1.9.15";

        internal const string szOID_CAPICOM                      = "1.3.6.1.4.1.311.88";     // Reserved for CAPICOM.
        internal const string szOID_CAPICOM_version              = "1.3.6.1.4.1.311.88.1";   // CAPICOM version
        internal const string szOID_CAPICOM_attribute            = "1.3.6.1.4.1.311.88.2";   // CAPICOM attribute
        internal const string szOID_CAPICOM_documentName         = "1.3.6.1.4.1.311.88.2.1"; // Document type attribute
        internal const string szOID_CAPICOM_documentDescription  = "1.3.6.1.4.1.311.88.2.2"; // Document description attribute
        internal const string szOID_CAPICOM_encryptedData        = "1.3.6.1.4.1.311.88.3";   // CAPICOM encrypted data message.
        internal const string szOID_CAPICOM_encryptedContent     = "1.3.6.1.4.1.311.88.3.1"; // CAPICOM content of encrypted data.
 
        // Digest Algorithms
        internal const string szOID_OIWSEC_sha1              = "1.3.14.3.2.26";
        internal const string szOID_RSA_MD5                  = "1.2.840.113549.2.5";
        internal const string szOID_OIWSEC_SHA256            = "2.16.840.1.101.3.4.1";
        internal const string szOID_OIWSEC_SHA384            = "2.16.840.1.101.3.4.2";
        internal const string szOID_OIWSEC_SHA512            = "2.16.840.1.101.3.4.3";

        // Encryption Algorithms
        internal const string szOID_RSA_RC2CBC         = "1.2.840.113549.3.2";
        internal const string szOID_RSA_RC4            = "1.2.840.113549.3.4";
        internal const string szOID_RSA_DES_EDE3_CBC   = "1.2.840.113549.3.7";
        internal const string szOID_OIWSEC_desCBC      = "1.3.14.3.2.7";

        // Key encryption algorithms
        internal const string szOID_RSA_SMIMEalg             = "1.2.840.113549.1.9.16.3";
        internal const string szOID_RSA_SMIMEalgESDH         = "1.2.840.113549.1.9.16.3.5";
        internal const string szOID_RSA_SMIMEalgCMS3DESwrap  = "1.2.840.113549.1.9.16.3.6";
        internal const string szOID_RSA_SMIMEalgCMSRC2wrap   = "1.2.840.113549.1.9.16.3.7";

        // DSA signing algorithms
        internal const string szOID_X957_DSA                 = "1.2.840.10040.4.1";
        internal const string szOID_X957_sha1DSA             = "1.2.840.10040.4.3";

        // RSA signing algorithms
        internal const string szOID_OIWSEC_sha1RSASign       = "1.3.14.3.2.29";

        // Alt Name Types.
        internal const uint CERT_ALT_NAME_OTHER_NAME     = 1;
        internal const uint CERT_ALT_NAME_RFC822_NAME    = 2;
        internal const uint CERT_ALT_NAME_DNS_NAME       = 3;
        internal const uint CERT_ALT_NAME_X400_ADDRESS   = 4;
        internal const uint CERT_ALT_NAME_DIRECTORY_NAME = 5;
        internal const uint CERT_ALT_NAME_EDI_PARTY_NAME = 6;
        internal const uint CERT_ALT_NAME_URL            = 7;
        internal const uint CERT_ALT_NAME_IP_ADDRESS     = 8;
        internal const uint CERT_ALT_NAME_REGISTERED_ID  = 9;

        // CERT_RDN Attribute Value Types
        internal const uint CERT_RDN_ANY_TYPE         = 0;
        internal const uint CERT_RDN_ENCODED_BLOB     = 1;
        internal const uint CERT_RDN_OCTET_STRING     = 2;
        internal const uint CERT_RDN_NUMERIC_STRING   = 3;
        internal const uint CERT_RDN_PRINTABLE_STRING = 4;
        internal const uint CERT_RDN_TELETEX_STRING   = 5;
        internal const uint CERT_RDN_T61_STRING       = 5;
        internal const uint CERT_RDN_VIDEOTEX_STRING  = 6;
        internal const uint CERT_RDN_IA5_STRING       = 7;
        internal const uint CERT_RDN_GRAPHIC_STRING   = 8;
        internal const uint CERT_RDN_VISIBLE_STRING   = 9;
        internal const uint CERT_RDN_ISO646_STRING    = 9;
        internal const uint CERT_RDN_GENERAL_STRING   = 10;
        internal const uint CERT_RDN_UNIVERSAL_STRING = 11;
        internal const uint CERT_RDN_INT4_STRING      = 11;
        internal const uint CERT_RDN_BMP_STRING       = 12;
        internal const uint CERT_RDN_UNICODE_STRING   = 12;
        internal const uint CERT_RDN_UTF8_STRING      = 13;
        internal const uint CERT_RDN_TYPE_MASK        = 0x000000FF;
        internal const uint CERT_RDN_FLAGS_MASK       = 0xFF000000;

        // Certificate Store control types
        internal const uint CERT_STORE_CTRL_RESYNC        = 1;
        internal const uint CERT_STORE_CTRL_NOTIFY_CHANGE = 2;
        internal const uint CERT_STORE_CTRL_COMMIT        = 3;
        internal const uint CERT_STORE_CTRL_AUTO_RESYNC   = 4;
        internal const uint CERT_STORE_CTRL_CANCEL_NOTIFY = 5;

        // Certificate Identifier
        internal const uint CERT_ID_ISSUER_SERIAL_NUMBER = 1;
        internal const uint CERT_ID_KEY_IDENTIFIER       = 2;
        internal const uint CERT_ID_SHA1_HASH            = 3;

        // MS provider names.
        internal const string MS_ENHANCED_PROV     = "Microsoft Enhanced Cryptographic Provider v1.0";
        internal const string MS_STRONG_PROV       = "Microsoft Strong Cryptographic Provider";
        internal const string MS_DEF_PROV          = "Microsoft Base Cryptographic Provider v1.0";
        internal const string MS_DEF_DSS_DH_PROV   = "Microsoft Base DSS and Diffie-Hellman Cryptographic Provider";
        internal const string MS_ENH_DSS_DH_PROV   = "Microsoft Enhanced DSS and Diffie-Hellman Cryptographic Provider";

        // HashOnly Signature
        internal const string DummySignerCommonName = "CN=Dummy Signer";

        // CSP types.
        internal const uint PROV_RSA_FULL          = 1;
        internal const uint PROV_DSS_DH            = 13;

        // Algorithm types
        internal const uint ALG_TYPE_ANY           = (0);
        internal const uint ALG_TYPE_DSS           = (1 << 9);
        internal const uint ALG_TYPE_RSA           = (2 << 9);
        internal const uint ALG_TYPE_BLOCK         = (3 << 9);
        internal const uint ALG_TYPE_STREAM        = (4 << 9);
        internal const uint ALG_TYPE_DH            = (5 << 9);
        internal const uint ALG_TYPE_SECURECHANNEL = (6 << 9);

        // Algorithm classes
        internal const uint ALG_CLASS_ANY          = (0);
        internal const uint ALG_CLASS_SIGNATURE    = (1 << 13);
        internal const uint ALG_CLASS_MSG_ENCRYPT  = (2 << 13);
        internal const uint ALG_CLASS_DATA_ENCRYPT = (3 << 13);
        internal const uint ALG_CLASS_HASH         = (4 << 13);
        internal const uint ALG_CLASS_KEY_EXCHANGE = (5 << 13);
        internal const uint ALG_CLASS_ALL          = (7 << 13);

        internal const uint ALG_SID_ANY            = (0);
        // Some RSA sub-ids
        internal const uint ALG_SID_RSA_ANY        = 0;
        internal const uint ALG_SID_RSA_PKCS       = 1;
        internal const uint ALG_SID_RSA_MSATWORK   = 2;
        internal const uint ALG_SID_RSA_ENTRUST    = 3;
        internal const uint ALG_SID_RSA_PGP        = 4;

        // Some DSS sub-ids
        internal const uint ALG_SID_DSS_ANY        = 0;
        internal const uint ALG_SID_DSS_PKCS       = 1;
        internal const uint ALG_SID_DSS_DMS        = 2;

        // Block cipher sub ids
        // DES sub_ids
        internal const uint ALG_SID_DES            = 1;
        internal const uint ALG_SID_3DES           = 3;
        internal const uint ALG_SID_DESX           = 4;
        internal const uint ALG_SID_IDEA           = 5;
        internal const uint ALG_SID_CAST           = 6;
        internal const uint ALG_SID_SAFERSK64      = 7;
        internal const uint ALG_SID_SAFERSK128     = 8;
        internal const uint ALG_SID_3DES_112       = 9;
        internal const uint ALG_SID_CYLINK_MEK     = 12;
        internal const uint ALG_SID_RC5            = 13;
        internal const uint ALG_SID_AES_128        = 14;
        internal const uint ALG_SID_AES_192        = 15;
        internal const uint ALG_SID_AES_256        = 16;
        internal const uint ALG_SID_AES            = 17;

        // Fortezza sub-ids
        internal const uint ALG_SID_SKIPJACK       = 10;
        internal const uint ALG_SID_TEK            = 11;

        // RC2 sub-ids
        internal const uint ALG_SID_RC2            = 2;

        // Stream cipher sub-ids
        internal const uint ALG_SID_RC4            = 1;
        internal const uint ALG_SID_SEAL           = 2;

        // Diffie-Hellman sub-ids
        internal const uint ALG_SID_DH_SANDF       = 1;
        internal const uint ALG_SID_DH_EPHEM       = 2;
        internal const uint ALG_SID_AGREED_KEY_ANY = 3;
        internal const uint ALG_SID_KEA            = 4;

        // Hash sub ids
        internal const uint ALG_SID_MD2              = 1;
        internal const uint ALG_SID_MD4              = 2;
        internal const uint ALG_SID_MD5              = 3;
        internal const uint ALG_SID_SHA              = 4;
        internal const uint ALG_SID_SHA1             = 4;
        internal const uint ALG_SID_MAC              = 5;
        internal const uint ALG_SID_RIPEMD           = 6;
        internal const uint ALG_SID_RIPEMD160        = 7;
        internal const uint ALG_SID_SSL3SHAMD5       = 8;
        internal const uint ALG_SID_HMAC             = 9;
        internal const uint ALG_SID_TLS1PRF          = 10;
        internal const uint ALG_SID_HASH_REPLACE_OWF = 11;

        // secure channel sub ids
        internal const uint ALG_SID_SSL3_MASTER          = 1;
        internal const uint ALG_SID_SCHANNEL_MASTER_HASH = 2;
        internal const uint ALG_SID_SCHANNEL_MAC_KEY     = 3;
        internal const uint ALG_SID_PCT1_MASTER          = 4;
        internal const uint ALG_SID_SSL2_MASTER          = 5;
        internal const uint ALG_SID_TLS1_MASTER          = 6;
        internal const uint ALG_SID_SCHANNEL_ENC_KEY     = 7;

        // algorithm identifier definitions
        internal const uint CALG_MD2                = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD2);
        internal const uint CALG_MD4                = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD4);
        internal const uint CALG_MD5                = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD5);
        internal const uint CALG_SHA                = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA);
        internal const uint CALG_SHA1               = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA1);
        internal const uint CALG_MAC                = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MAC);
        internal const uint CALG_RSA_SIGN           = (ALG_CLASS_SIGNATURE | ALG_TYPE_RSA | ALG_SID_RSA_ANY);
        internal const uint CALG_DSS_SIGN           = (ALG_CLASS_SIGNATURE | ALG_TYPE_DSS | ALG_SID_DSS_ANY);
        internal const uint CALG_NO_SIGN            = (ALG_CLASS_SIGNATURE | ALG_TYPE_ANY | ALG_SID_ANY);
        internal const uint CALG_RSA_KEYX           = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_RSA|ALG_SID_RSA_ANY);
        internal const uint CALG_DES                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_DES);
        internal const uint CALG_3DES_112           = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_3DES_112);
        internal const uint CALG_3DES               = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_3DES);
        internal const uint CALG_DESX               = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_DESX);
        internal const uint CALG_RC2                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_RC2);
        internal const uint CALG_RC4                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_STREAM|ALG_SID_RC4);
        internal const uint CALG_SEAL               = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_STREAM|ALG_SID_SEAL);
        internal const uint CALG_DH_SF              = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_DH|ALG_SID_DH_SANDF);
        internal const uint CALG_DH_EPHEM           = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_DH|ALG_SID_DH_EPHEM);
        internal const uint CALG_AGREEDKEY_ANY      = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_DH|ALG_SID_AGREED_KEY_ANY);
        internal const uint CALG_KEA_KEYX           = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_DH|ALG_SID_KEA);
        internal const uint CALG_HUGHES_MD5         = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_ANY|ALG_SID_MD5);
        internal const uint CALG_SKIPJACK           = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_SKIPJACK);
        internal const uint CALG_TEK                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_TEK);
        internal const uint CALG_CYLINK_MEK         = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_CYLINK_MEK);
        internal const uint CALG_SSL3_SHAMD5        = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SSL3SHAMD5);
        internal const uint CALG_SSL3_MASTER        = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_SSL3_MASTER);
        internal const uint CALG_SCHANNEL_MASTER_HASH = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_SCHANNEL_MASTER_HASH);
        internal const uint CALG_SCHANNEL_MAC_KEY   = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_SCHANNEL_MAC_KEY);
        internal const uint CALG_SCHANNEL_ENC_KEY   = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_SCHANNEL_ENC_KEY);
        internal const uint CALG_PCT1_MASTER        = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_PCT1_MASTER);
        internal const uint CALG_SSL2_MASTER        = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_SSL2_MASTER);
        internal const uint CALG_TLS1_MASTER        = (ALG_CLASS_MSG_ENCRYPT|ALG_TYPE_SECURECHANNEL|ALG_SID_TLS1_MASTER);
        internal const uint CALG_RC5                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_RC5);
        internal const uint CALG_HMAC               = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_HMAC);
        internal const uint CALG_TLS1PRF            = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_TLS1PRF);
        internal const uint CALG_HASH_REPLACE_OWF   = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_HASH_REPLACE_OWF);
        internal const uint CALG_AES_128            = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_128);
        internal const uint CALG_AES_192            = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_192);
        internal const uint CALG_AES_256            = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES_256);
        internal const uint CALG_AES                = (ALG_CLASS_DATA_ENCRYPT|ALG_TYPE_BLOCK|ALG_SID_AES);

        // CryptGetProvParam flags
        internal const uint CRYPT_FIRST             = 1;
        internal const uint CRYPT_NEXT              = 2;
        internal const uint PP_ENUMALGS_EX          = 22;

        // dwFlags definitions for CryptAcquireContext
        internal const uint CRYPT_VERIFYCONTEXT     = 0xF0000000;
        internal const uint CRYPT_NEWKEYSET         = 0x00000008;
        internal const uint CRYPT_DELETEKEYSET      = 0x00000010;
        internal const uint CRYPT_MACHINE_KEYSET    = 0x00000020;
        internal const uint CRYPT_SILENT            = 0x00000040;
        internal const uint CRYPT_USER_KEYSET       = 0x00001000;

        // dwFlag definitions for CryptGenKey
        internal const uint CRYPT_EXPORTABLE        = 0x00000001;
        internal const uint CRYPT_USER_PROTECTED    = 0x00000002;
        internal const uint CRYPT_CREATE_SALT       = 0x00000004;
        internal const uint CRYPT_UPDATE_KEY        = 0x00000008;
        internal const uint CRYPT_NO_SALT           = 0x00000010;
        internal const uint CRYPT_PREGEN            = 0x00000040;
        internal const uint CRYPT_RECIPIENT         = 0x00000010;
        internal const uint CRYPT_INITIATOR         = 0x00000040;
        internal const uint CRYPT_ONLINE            = 0x00000080;
        internal const uint CRYPT_SF                = 0x00000100;
        internal const uint CRYPT_CREATE_IV         = 0x00000200;
        internal const uint CRYPT_KEK               = 0x00000400;
        internal const uint CRYPT_DATA_KEY          = 0x00000800;
        internal const uint CRYPT_VOLATILE          = 0x00001000;
        internal const uint CRYPT_SGCKEY            = 0x00002000;
        internal const uint CRYPT_ARCHIVABLE        = 0x00004000;

        internal const byte CUR_BLOB_VERSION        = 2;

        // Exported key blob definitions
        internal const byte SIMPLEBLOB              = 0x1;
        internal const byte PUBLICKEYBLOB           = 0x6;
        internal const byte PRIVATEKEYBLOB          = 0x7;
        internal const byte PLAINTEXTKEYBLOB        = 0x8;
        internal const byte OPAQUEKEYBLOB           = 0x9;
        internal const byte PUBLICKEYBLOBEX         = 0xA;
        internal const byte SYMMETRICWRAPKEYBLOB    = 0xB;

        // Magic constants
        internal const uint DSS_MAGIC               = 0x31535344;
        internal const uint DSS_PRIVATE_MAGIC       = 0x32535344;
        internal const uint DSS_PUB_MAGIC_VER3      = 0x33535344;
        internal const uint DSS_PRIV_MAGIC_VER3     = 0x34535344;
        internal const uint RSA_PUB_MAGIC           = 0x31415352;
        internal const uint RSA_PRIV_MAGIC          = 0x32415352;

        // CryptAcquireCertificatePrivateKey dwFlags
        internal const uint CRYPT_ACQUIRE_CACHE_FLAG         = 0x00000001; 
        internal const uint CRYPT_ACQUIRE_USE_PROV_INFO_FLAG = 0x00000002; 
        internal const uint CRYPT_ACQUIRE_COMPARE_KEY_FLAG   = 0x00000004; 
        internal const uint CRYPT_ACQUIRE_SILENT_FLAG        = 0x00000040; 

        // CryptMsgOpenToDecode dwFlags
        internal const uint CMSG_BARE_CONTENT_FLAG             = 0x00000001;
        internal const uint CMSG_LENGTH_ONLY_FLAG              = 0x00000002;
        internal const uint CMSG_DETACHED_FLAG                 = 0x00000004;
        internal const uint CMSG_AUTHENTICATED_ATTRIBUTES_FLAG = 0x00000008;
        internal const uint CMSG_CONTENTS_OCTETS_FLAG          = 0x00000010;
        internal const uint CMSG_MAX_LENGTH_FLAG               = 0x00000020;

        // Get parameter types and their corresponding data structure definitions.
        internal const uint CMSG_TYPE_PARAM                              = 1;
        internal const uint CMSG_CONTENT_PARAM                           = 2;
        internal const uint CMSG_BARE_CONTENT_PARAM                      = 3;
        internal const uint CMSG_INNER_CONTENT_TYPE_PARAM                = 4;
        internal const uint CMSG_SIGNER_COUNT_PARAM                      = 5;
        internal const uint CMSG_SIGNER_INFO_PARAM                       = 6;
        internal const uint CMSG_SIGNER_CERT_INFO_PARAM                  = 7;
        internal const uint CMSG_SIGNER_HASH_ALGORITHM_PARAM             = 8;
        internal const uint CMSG_SIGNER_AUTH_ATTR_PARAM                  = 9;
        internal const uint CMSG_SIGNER_UNAUTH_ATTR_PARAM                = 10;
        internal const uint CMSG_CERT_COUNT_PARAM                        = 11;
        internal const uint CMSG_CERT_PARAM                              = 12;
        internal const uint CMSG_CRL_COUNT_PARAM                         = 13;
        internal const uint CMSG_CRL_PARAM                               = 14;
        internal const uint CMSG_ENVELOPE_ALGORITHM_PARAM                = 15;
        internal const uint CMSG_RECIPIENT_COUNT_PARAM                   = 17;
        internal const uint CMSG_RECIPIENT_INDEX_PARAM                   = 18;
        internal const uint CMSG_RECIPIENT_INFO_PARAM                    = 19;
        internal const uint CMSG_HASH_ALGORITHM_PARAM                    = 20;
        internal const uint CMSG_HASH_DATA_PARAM                         = 21;
        internal const uint CMSG_COMPUTED_HASH_PARAM                     = 22;
        internal const uint CMSG_ENCRYPT_PARAM                           = 26;
        internal const uint CMSG_ENCRYPTED_DIGEST                        = 27;
        internal const uint CMSG_ENCODED_SIGNER                          = 28;
        internal const uint CMSG_ENCODED_MESSAGE                         = 29;
        internal const uint CMSG_VERSION_PARAM                           = 30;
        internal const uint CMSG_ATTR_CERT_COUNT_PARAM                   = 31;
        internal const uint CMSG_ATTR_CERT_PARAM                         = 32;
        internal const uint CMSG_CMS_RECIPIENT_COUNT_PARAM               = 33;
        internal const uint CMSG_CMS_RECIPIENT_INDEX_PARAM               = 34;
        internal const uint CMSG_CMS_RECIPIENT_ENCRYPTED_KEY_INDEX_PARAM = 35;
        internal const uint CMSG_CMS_RECIPIENT_INFO_PARAM                = 36;
        internal const uint CMSG_UNPROTECTED_ATTR_PARAM                  = 37;
        internal const uint CMSG_SIGNER_CERT_ID_PARAM                    = 38;
        internal const uint CMSG_CMS_SIGNER_INFO_PARAM                   = 39;

        // Message control types.
        internal const uint CMSG_CTRL_VERIFY_SIGNATURE       = 1;
        internal const uint CMSG_CTRL_DECRYPT                = 2;
        internal const uint CMSG_CTRL_VERIFY_HASH            = 5;
        internal const uint CMSG_CTRL_ADD_SIGNER             = 6;
        internal const uint CMSG_CTRL_DEL_SIGNER             = 7;
        internal const uint CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR = 8;
        internal const uint CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR = 9;
        internal const uint CMSG_CTRL_ADD_CERT               = 10;
        internal const uint CMSG_CTRL_DEL_CERT               = 11;
        internal const uint CMSG_CTRL_ADD_CRL                = 12;
        internal const uint CMSG_CTRL_DEL_CRL                = 13;
        internal const uint CMSG_CTRL_ADD_ATTR_CERT          = 14;
        internal const uint CMSG_CTRL_DEL_ATTR_CERT          = 15;
        internal const uint CMSG_CTRL_KEY_TRANS_DECRYPT      = 16;
        internal const uint CMSG_CTRL_KEY_AGREE_DECRYPT      = 17;
        internal const uint CMSG_CTRL_MAIL_LIST_DECRYPT      = 18;
        internal const uint CMSG_CTRL_VERIFY_SIGNATURE_EX    = 19;
        internal const uint CMSG_CTRL_ADD_CMS_SIGNER_INFO    = 20;

        // Signer Types
        internal const uint CMSG_VERIFY_SIGNER_PUBKEY        = 1; // pvSigner: PCERT_PUBLIC_KEY_INFO
        internal const uint CMSG_VERIFY_SIGNER_CERT          = 2; // pvSigner: PCCERT_CONTEXT
        internal const uint CMSG_VERIFY_SIGNER_CHAIN         = 3; // pvSigner: PCCERT_CHAIN_CONTEXT
        internal const uint CMSG_VERIFY_SIGNER_NULL          = 4; // pvSigner: NULL

        // Message types.
        internal const uint CMSG_DATA                 = 1;
        internal const uint CMSG_SIGNED               = 2;
        internal const uint CMSG_ENVELOPED            = 3;
        internal const uint CMSG_SIGNED_AND_ENVELOPED = 4;
        internal const uint CMSG_HASHED               = 5;
        internal const uint CMSG_ENCRYPTED            = 6;

        // Recipient types
        internal const uint CMSG_KEY_TRANS_RECIPIENT  = 1;
        internal const uint CMSG_KEY_AGREE_RECIPIENT  = 2;
        internal const uint CMSG_MAIL_LIST_RECIPIENT  = 3;
        
        // Key agree type
        internal const uint CMSG_KEY_AGREE_ORIGINATOR_CERT       = 1;
        internal const uint CMSG_KEY_AGREE_ORIGINATOR_PUBLIC_KEY = 2;

        // Key agree choices
        internal const uint CMSG_KEY_AGREE_EPHEMERAL_KEY_CHOICE  = 1;
        internal const uint CMSG_KEY_AGREE_STATIC_KEY_CHOICE     = 2;

        // dwVersion numbers for the KeyTrans, KeyAgree and MailList recipients
        internal const uint CMSG_ENVELOPED_RECIPIENT_V0      = 0;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V2      = 2;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V3      = 3;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V4      = 4;
        internal const uint CMSG_KEY_TRANS_PKCS_1_5_VERSION  = CMSG_ENVELOPED_RECIPIENT_V0;
        internal const uint CMSG_KEY_TRANS_CMS_VERSION       = CMSG_ENVELOPED_RECIPIENT_V2;
        internal const uint CMSG_KEY_AGREE_VERSION           = CMSG_ENVELOPED_RECIPIENT_V3;
        internal const uint CMSG_MAIL_LIST_VERSION           = CMSG_ENVELOPED_RECIPIENT_V4;

        // RC2 encryption algorithm version (key length).
        internal const uint CRYPT_RC2_40BIT_VERSION   = 160;
        internal const uint CRYPT_RC2_56BIT_VERSION   = 52;
        internal const uint CRYPT_RC2_64BIT_VERSION   = 120;
        internal const uint CRYPT_RC2_128BIT_VERSION  = 58;

        // Error codes.
        internal const int E_NOTIMPL                    = unchecked((int) 0x80000001); // Not implemented.
        internal const int E_OUTOFMEMORY                = unchecked((int) 0x8007000E); // Ran out of memory.
        internal const int NTE_NO_KEY                   = unchecked((int) 0x8009000D); // Key does not exist.
        internal const int NTE_BAD_PUBLIC_KEY           = unchecked((int) 0x80090015); // Provider's public key is invalid.
        internal const int NTE_BAD_KEYSET               = unchecked((int) 0x80090016); // Keyset does not exist
        internal const int CRYPT_E_MSG_ERROR            = unchecked((int) 0x80091001); // An error occurred while performing an operation on a cryptographic message.
        internal const int CRYPT_E_UNKNOWN_ALGO         = unchecked((int) 0x80091002); // Unknown cryptographic algorithm.
        internal const int CRYPT_E_INVALID_MSG_TYPE     = unchecked((int) 0x80091004); // Invalid cryptographic message type.
        internal const int CRYPT_E_RECIPIENT_NOT_FOUND  = unchecked((int) 0x8009100B); // The enveloped-data message does not contain the specified recipient.
        internal const int CRYPT_E_ISSUER_SERIALNUMBER  = unchecked((int) 0x8009100D); // Invalid issuer and/or serial number.
        internal const int CRYPT_E_SIGNER_NOT_FOUND     = unchecked((int) 0x8009100E); // Cannot find the original signer.
        internal const int CRYPT_E_ATTRIBUTES_MISSING   = unchecked((int) 0x8009100F); // The cryptographic message does not contain all of the requested attributes.
        internal const int CRYPT_E_BAD_ENCODE           = unchecked((int) 0x80092002); // An error occurred during encode or decode operation.
        internal const int CRYPT_E_NOT_FOUND            = unchecked((int) 0x80092004); // Cannot find object or property.
        internal const int CRYPT_E_NO_MATCH             = unchecked((int) 0x80092009); // Cannot find the requested object.
        internal const int CRYPT_E_NO_SIGNER            = unchecked((int) 0x8009200E); // The signed cryptographic message does not have a signer for the specified signer index.
        internal const int CRYPT_E_REVOKED              = unchecked((int) 0x80092010); // The certificate is revoked.
        internal const int CRYPT_E_NO_REVOCATION_CHECK  = unchecked((int) 0x80092012); // The revocation function was unable to check revocation for the certificate.        
        internal const int CRYPT_E_REVOCATION_OFFLINE   = unchecked((int) 0x80092013); // The revocation function was unable to check revocation 
                                                                                       // because the revocation server was offline.        
        internal const int CRYPT_E_ASN1_BADTAG          = unchecked((int) 0x8009310B); // ASN1 bad tag value met.

        internal const int TRUST_E_CERT_SIGNATURE       = unchecked((int) 0x80096004); // The signature of the certificate can not be verified.
        internal const int TRUST_E_BASIC_CONSTRAINTS    = unchecked((int) 0x80096019); // A certificate's basic constraint extension has not been observed.        
        internal const int CERT_E_EXPIRED               = unchecked((int) 0x800B0101); // A required certificate is not within its validity period when verifying against 
                                                                                       // the current system clock or the timestamp in the signed file.        
        internal const int CERT_E_VALIDITYPERIODNESTING = unchecked((int) 0x800B0102); // The validity periods of the certification chain do not nest correctly.        
        internal const int CERT_E_UNTRUSTEDROOT         = unchecked((int) 0x800B0109); // A certificate chain processed, but terminated in a root 
                                                                                       // certificate which is not trusted by the trust provider.
        internal const int CERT_E_CHAINING              = unchecked((int) 0x800B010A); // An internal certificate chaining error has occurred.        
        internal const int TRUST_E_FAIL                 = unchecked((int) 0x800B010B); // Generic trust failure.        
        internal const int CERT_E_REVOKED               = unchecked((int) 0x800B010C); // A certificate was explicitly revoked by its issuer.        
        internal const int CERT_E_UNTRUSTEDTESTROOT     = unchecked((int) 0x800B010D); // The certification path terminates with the test root which 
                                                                                       // is not trusted with the current policy settings.        
        internal const int CERT_E_REVOCATION_FAILURE    = unchecked((int) 0x800B010E); // The revocation process could not continue - the certificate(s) could not be checked.        
        internal const int CERT_E_WRONG_USAGE           = unchecked((int) 0x800B0110); // The certificate is not valid for the requested usage.        
        internal const int CERT_E_INVALID_POLICY        = unchecked((int) 0x800B0113); // The certificate has invalid policy.        
        internal const int CERT_E_INVALID_NAME          = unchecked((int) 0x800B0114); // The certificate has an invalid name. The name is not included 
                                                                                       // in the permitted list or is explicitly excluded.

        internal const int ERROR_SUCCESS                = 0;                           // The operation completed successfully.
        internal const int ERROR_CALL_NOT_IMPLEMENTED   = 120;                         // This function is not supported on this system.
        internal const int ERROR_CANCELLED              = 1223;                        // The operation was canceled by the user.

        //
        // Structures
        //

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct BLOBHEADER {
            internal byte  bType;
            internal byte  bVersion;
            internal short reserved;
            internal uint  aiKeyAlg;
        };

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_INFO {
            internal uint   cAltEntry;
            internal IntPtr rgAltEntry; // PCERT_ALT_NAME_ENTRY
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_ENTRY {
            internal uint           dwAltNameChoice;
            internal CERT_ALT_NAME_ENTRY_UNION Value;
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_ENTRY_UNION {
            [FieldOffset(0)] 
            internal IntPtr         pOtherName;
            [FieldOffset(0)] 
            internal IntPtr         pwszRfc822Name;
            [FieldOffset(0)] 
            internal IntPtr         pwszDNSName;
            [FieldOffset(0)] 
            internal CRYPTOAPI_BLOB DirectoryName;
            [FieldOffset(0)] 
            internal IntPtr         pwszURL;
            [FieldOffset(0)] 
            internal CRYPTOAPI_BLOB IPAddress;
            [FieldOffset(0)] 
            internal IntPtr         pszRegisteredID;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_BASIC_CONSTRAINTS_INFO {
            internal CRYPT_BIT_BLOB SubjectType;
            internal bool           fPathLenConstraint;
            internal uint           dwPathLenConstraint;
            internal uint           cSubtreesConstraint;
            internal IntPtr         rgSubtreesConstraint; // PCERT_NAME_BLOB
        };

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_BASIC_CONSTRAINTS2_INFO {
            internal int  fCA;
            internal int  fPathLenConstraint;
            internal uint dwPathLenConstraint;
        };

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_CONTEXT {
            internal CERT_CHAIN_CONTEXT(int size) {
                cbSize = (uint) size;
                dwErrorStatus = 0;
                dwInfoStatus = 0;
                cChain = 0;
                rgpChain = IntPtr.Zero;
                cLowerQualityChainContext = 0;
                rgpLowerQualityChainContext = IntPtr.Zero;
                fHasRevocationFreshnessTime = 0;
                dwRevocationFreshnessTime = 0;
            }
            internal uint              cbSize;
            internal uint              dwErrorStatus;   // serialized CERT_TRUST_STATUS
            internal uint              dwInfoStatus;    // serialized CERT_TRUST_STATUS
            internal uint              cChain;
            internal IntPtr            rgpChain;                    // PCERT_SIMPLE_CHAIN*
            internal uint              cLowerQualityChainContext;
            internal IntPtr            rgpLowerQualityChainContext; // PCCERT_CHAIN_CONTEXT*
            internal uint              fHasRevocationFreshnessTime; // Note that we declare the field as a uint here since we are manipulating 
                                                                    // the structure manually and a bool is only 1 byte in the managed world.
            internal uint              dwRevocationFreshnessTime;   // seconds
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_ELEMENT {
            internal CERT_CHAIN_ELEMENT(int size) {
                cbSize = (uint) size;
                pCertContext = IntPtr.Zero;
                dwErrorStatus = 0;
                dwInfoStatus = 0;
                pRevocationInfo = IntPtr.Zero;
                pIssuanceUsage = IntPtr.Zero;
                pApplicationUsage = IntPtr.Zero; 
                pwszExtendedErrorInfo = IntPtr.Zero;
            }
            internal uint              cbSize;
            internal IntPtr            pCertContext;
            internal uint              dwErrorStatus;   // serialized CERT_TRUST_STATUS
            internal uint              dwInfoStatus;    // serialized CERT_TRUST_STATUS
            internal IntPtr            pRevocationInfo;
            internal IntPtr            pIssuanceUsage;
            internal IntPtr            pApplicationUsage;
            internal IntPtr            pwszExtendedErrorInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_PARA {
            internal uint             cbSize;
            internal CERT_USAGE_MATCH RequestedUsage;
            internal CERT_USAGE_MATCH RequestedIssuancePolicy;
            internal uint             dwUrlRetrievalTimeout; // milliseconds
            internal bool             fCheckRevocationFreshnessTime;
            internal uint             dwRevocationFreshnessTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_PARA {
            internal CERT_CHAIN_POLICY_PARA(int size) {
                cbSize = (uint) size;
                dwFlags = 0;
                pvExtraPolicyPara = IntPtr.Zero;
            }
            internal uint   cbSize;
            internal uint   dwFlags;
            internal IntPtr pvExtraPolicyPara; 
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_STATUS {
            internal CERT_CHAIN_POLICY_STATUS(int size) {
                cbSize = (uint) size;
                dwError = 0;
                lChainIndex = IntPtr.Zero;
                lElementIndex = IntPtr.Zero;
                pvExtraPolicyStatus = IntPtr.Zero;
            }
            internal uint   cbSize;
            internal uint   dwError;
            internal IntPtr lChainIndex;
            internal IntPtr lElementIndex;
            internal IntPtr pvExtraPolicyStatus; 
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CONTEXT {
            internal uint   dwCertEncodingType;
            internal IntPtr pbCertEncoded;
            internal uint   cbCertEncoded;
            internal IntPtr pCertInfo;
            internal IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_DSS_PARAMETERS {
            internal CRYPTOAPI_BLOB p;
            internal CRYPTOAPI_BLOB q;
            internal CRYPTOAPI_BLOB g;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ENHKEY_USAGE {
            internal uint   cUsageIdentifier;
            internal IntPtr rgpszUsageIdentifier; // LPSTR*
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_EXTENSION {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string         pszObjId;
            internal bool           fCritical;
            internal CRYPTOAPI_BLOB Value;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ID {
            internal uint          dwIdChoice;
            internal CERT_ID_UNION Value;
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        internal struct CERT_ID_UNION {
            [FieldOffset(0)] 
            internal CERT_ISSUER_SERIAL_NUMBER IssuerSerialNumber; // CERT_ID_ISSUER_SERIAL_NUMBER
            [FieldOffset(0)] 
            internal CRYPTOAPI_BLOB KeyId;                         // CERT_ID_KEY_IDENTIFIER
            [FieldOffset(0)] 
            internal CRYPTOAPI_BLOB HashId;                        // CERT_ID_SHA1_HASH
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ISSUER_SERIAL_NUMBER {
            internal CRYPTOAPI_BLOB Issuer;
            internal CRYPTOAPI_BLOB SerialNumber;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_INFO {
            internal uint                       dwVersion;
            internal CRYPTOAPI_BLOB             SerialNumber;
            internal CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            internal CRYPTOAPI_BLOB             Issuer;
            internal _FILETIME                   NotBefore;
            internal _FILETIME                   NotAfter;
            internal CRYPTOAPI_BLOB             Subject;
            internal CERT_PUBLIC_KEY_INFO       SubjectPublicKeyInfo;
            internal CRYPT_BIT_BLOB             IssuerUniqueId;
            internal CRYPT_BIT_BLOB             SubjectUniqueId;
            internal uint                       cExtension;
            internal IntPtr                     rgExtension; // PCERT_EXTENSION
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_KEY_USAGE_RESTRICTION_INFO {
            internal uint           cCertPolicyId;
            internal IntPtr         rgCertPolicyId;     // LPSTR*
            internal CRYPT_BIT_BLOB RestrictedKeyUsage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_NAME_INFO {
            internal uint   cRDN;
            internal IntPtr rgRDN; // PCERT_RDN
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_NAME_VALUE {
            internal uint           dwValueType;
            internal CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_OTHER_NAME {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string         pszObjId;
            internal CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_ID {
            internal uint   cCertPolicyElementId;
            internal IntPtr rgpszCertPolicyElementId; // LPSTR*
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICIES_INFO {
            internal uint   cPolicyInfo;
            internal IntPtr rgPolicyInfo;      // PCERT_POLICY_INFO
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_INFO {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string pszPolicyIdentifier;
            internal uint   cPolicyQualifier;
            internal IntPtr rgPolicyQualifier; // PCERT_POLICY_QUALIFIER_INFO
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_QUALIFIER_INFO {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string pszPolicyQualifierId;
            CRYPTOAPI_BLOB  Qualifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO {
            internal CRYPT_ALGORITHM_IDENTIFIER Algorithm;
            internal CRYPT_BIT_BLOB             PublicKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO2 {
            internal CRYPT_ALGORITHM_IDENTIFIER2 Algorithm;
            internal CRYPT_BIT_BLOB              PublicKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_RDN {
            internal uint   cRDNAttr;
            internal IntPtr rgRDNAttr; // PCERT_RDN_ATTR
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_RDN_ATTR {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string         pszObjId;
            internal uint           dwValueType;
            internal CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_SIMPLE_CHAIN {
            internal CERT_SIMPLE_CHAIN(int size) {
                cbSize = (uint) size;
                dwErrorStatus = 0;
                dwInfoStatus = 0;
                cElement = 0;
                rgpElement = IntPtr.Zero;
                pTrustListInfo = IntPtr.Zero;
                fHasRevocationFreshnessTime = 0;
                dwRevocationFreshnessTime = 0;
            }
            internal uint              cbSize;
            internal uint              dwErrorStatus;   // serialized CERT_TRUST_STATUS
            internal uint              dwInfoStatus;    // serialized CERT_TRUST_STATUS
            internal uint              cElement;        
            internal IntPtr            rgpElement;      // PCERT_CHAIN_ELEMENT*
            internal IntPtr            pTrustListInfo;
            internal uint              fHasRevocationFreshnessTime; // Note that we declare the field as a uint here since we are manipulating 
                                                                    // the structure manually and a bool is only 1 byte in the managed world.
            internal uint              dwRevocationFreshnessTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_TEMPLATE_EXT {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string pszObjId;
            internal uint   dwMajorVersion;
            bool            fMinorVersion;
            uint            dwMinorVersion;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_TRUST_STATUS {
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_USAGE_MATCH {
            internal uint              dwType;
            internal CERT_ENHKEY_USAGE Usage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CMS_RECIPIENT_INFO {
            internal uint                       dwRecipientChoice;
            // union {
            //  PCMSG_KEY_TRANS_RECIPIENT_INFO pKeyTrans;   // CMSG_KEY_TRANS_RECIPIENT
            //  PCMSG_KEY_AGREE_RECIPIENT_INFO pKeyAgree;   // CMSG_KEY_AGREE_RECIPIENT
            //  PCMSG_MAIL_LIST_RECIPIENT_INFO pMailList;   // CMSG_MAIL_LIST_RECIPIENT
            // }
            //
            internal IntPtr                     pRecipientInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CMS_SIGNER_INFO {
            internal uint                       dwVersion;
            internal CERT_ID                    SignerId;
            internal CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal CRYPTOAPI_BLOB             EncryptedHash;
            internal CRYPT_ATTRIBUTES           AuthAttrs;
            internal CRYPT_ATTRIBUTES           UnauthAttrs;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA {
            internal CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA(int size) {
                cbSize = (uint) size;
                dwSignerIndex = 0;
                blob = new CRYPTOAPI_BLOB();
            }

            internal uint                       cbSize;
            internal uint                       dwSignerIndex;
            internal CRYPTOAPI_BLOB             blob;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_DECRYPT_PARA {
            internal CMSG_CTRL_DECRYPT_PARA(int size) {
                cbSize = (uint) size;
                hCryptProv = IntPtr.Zero;
                dwKeySpec = 0;
                dwRecipientIndex = 0;
            }
            
            internal uint                       cbSize;
            internal IntPtr                     hCryptProv;
            internal uint                       dwKeySpec;
            internal uint                       dwRecipientIndex;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA {
            internal CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA(int size) {
                cbSize = (uint) size;
                dwSignerIndex = 0;
                dwUnauthAttrIndex = 0;
            }

            internal uint                       cbSize;
            internal uint                       dwSignerIndex;
            internal uint                       dwUnauthAttrIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_KEY_TRANS_DECRYPT_PARA {
            internal uint                       cbSize;
            internal SafeCryptProvHandle        hCryptProv;
            internal uint                       dwKeySpec;
            internal IntPtr                     pKeyTrans; // PCMSG_KEY_TRANS_RECIPIENT_INFO
            internal uint                       dwRecipientIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO {
            internal uint                       cbSize;
            internal CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal IntPtr                     pvKeyEncryptionAuxInfo;
            internal CRYPT_ALGORITHM_IDENTIFIER KeyWrapAlgorithm;
            internal IntPtr                     pvKeyWrapAuxInfo;
            // The following hCryptProv and dwKeySpec must be specified for the
            // CMSG_KEY_AGREE_STATIC_KEY_CHOICE.
            //
            // For CMSG_KEY_AGREE_EPHEMERAL_KEY_CHOICE, dwKeySpec isn't applicable
            // and hCryptProv is optional.
            internal IntPtr                     hCryptProv; // HCRYPTPROV
            internal uint                       dwKeySpec;
            internal uint                       dwKeyChoice;
            // union {
            //    PCRYPT_ALGORITHM_IDENTIFIER // for CMSG_KEY_AGREE_EPHEMERAL_KEY_CHOICE
            //    PCERT_ID                    // for CMSG_KEY_AGREE_STATIC_KEY_CHOICE
            // }
            //
            internal IntPtr                     pEphemeralAlgorithmOrSenderId;  // PCRYPT_ALGORITHM_IDENTIFIER or PCERT_ID 
            internal CRYPTOAPI_BLOB             UserKeyingMaterial; // OPTIONAL
            internal uint                       cRecipientEncryptedKeys;
            internal IntPtr                     rgpRecipientEncryptedKeys;      // PCMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO *
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO {
            internal uint                       cbSize;
            internal CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal IntPtr                     pvKeyEncryptionAuxInfo;
            internal IntPtr                     hCryptProv; // HCRYPTPROV
            internal CRYPT_BIT_BLOB             RecipientPublicKey;
            internal CERT_ID                    RecipientId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RC2_AUX_INFO {
            internal CMSG_RC2_AUX_INFO(int size) {
                cbSize    = (uint) size;
                dwBitLen  = 0;
            }

            internal uint cbSize;
            internal uint dwBitLen;
        }
        
        internal struct CMSG_RECIPIENT_ENCODE_INFO {
            internal uint                       dwRecipientChoice;
            // union {
            //  PCMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO pKeyTrans; // CMSG_KEY_TRANS_RECIPIENT
            //  PCMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO pKeyAgree; // CMSG_KEY_AGREE_RECIPIENT
            //  PCMSG_MAIL_LIST_RECIPIENT_ENCODE_INFO pMailList; // CMSG_MAIL_LIST_RECIPIENT
            // }
            //
            internal IntPtr                     pRecipientInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO {
            internal uint                       cbSize;
            internal CRYPT_BIT_BLOB             RecipientPublicKey;
            internal CERT_ID                    RecipientId;
            // Following fields are optional and only applicable to KEY_IDENTIFIER CERT_IDs.
            internal _FILETIME                   Date;
            internal IntPtr                     pOtherAttr; // PCRYPT_ATTRIBUTE_TYPE_VALUE
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_ENVELOPED_ENCODE_INFO {
            internal CMSG_ENVELOPED_ENCODE_INFO(int size) {
                cbSize = (uint) size;
                hCryptProv = IntPtr.Zero;
                ContentEncryptionAlgorithm = new CRYPT_ALGORITHM_IDENTIFIER();
                pvEncryptionAuxInfo = IntPtr.Zero;
                cRecipients = 0;
                rgpRecipients = IntPtr.Zero;
                rgCmsRecipients = IntPtr.Zero;
                cCertEncoded = 0;
                rgCertEncoded = IntPtr.Zero;
                cCrlEncoded = 0;
                rgCrlEncoded = IntPtr.Zero;
                cAttrCertEncoded = 0;
                rgAttrCertEncoded = IntPtr.Zero;
                cUnprotectedAttr = 0;
                rgUnprotectedAttr = IntPtr.Zero;
            }
            
            internal uint                       cbSize;
            internal IntPtr                     hCryptProv;
            internal CRYPT_ALGORITHM_IDENTIFIER ContentEncryptionAlgorithm;
            internal IntPtr                     pvEncryptionAuxInfo;
            internal uint                       cRecipients;
            internal IntPtr                     rgpRecipients; // PCERT_INFO
            // If rgCmsRecipients != NULL, then, the above rgpRecipients must be NULL.
            internal IntPtr                     rgCmsRecipients; // PCMSG_RECIPIENT_ENCODE_INFO
            internal uint                       cCertEncoded;
            internal IntPtr                     rgCertEncoded; // PCERT_BLOB
            internal uint                       cCrlEncoded;
            internal IntPtr                     rgCrlEncoded; // PCRL_BLOB
            internal uint                       cAttrCertEncoded;
            internal IntPtr                     rgAttrCertEncoded; // PCERT_BLOB
            internal uint                       cUnprotectedAttr;
            internal IntPtr                     rgUnprotectedAttr; // PCRYPT_ATTRIBUTE
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_KEY_AGREE_DECRYPT_PARA {
            internal CMSG_CTRL_KEY_AGREE_DECRYPT_PARA(int size) {
                cbSize = (uint) size;
                hCryptProv = IntPtr.Zero;
                dwKeySpec = 0;
                pKeyAgree = IntPtr.Zero;
                dwRecipientIndex = 0;
                dwRecipientEncryptedKeyIndex = 0;
                OriginatorPublicKey = new CRYPT_BIT_BLOB();
            }

            internal uint                       cbSize;
            internal IntPtr                     hCryptProv;
            internal uint                       dwKeySpec;
            internal IntPtr                     pKeyAgree; // PCMSG_KEY_AGREE_RECIPIENT_INFO
            internal uint                       dwRecipientIndex;
            internal uint                       dwRecipientEncryptedKeyIndex;
            internal CRYPT_BIT_BLOB             OriginatorPublicKey;
        }

        // CLR marshaller can't handle this kind of union (with managed type in structures within 
        // the union (OriginatorPublicKeyInfo.Algorithm.pszObjId is declared as managed string type).
        // So break it up into different structures ourselves, and marshal accordingly.
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_RECIPIENT_INFO {
            internal uint                        dwVersion;
            internal uint                        dwOriginatorChoice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO {
            internal uint                               dwVersion;
            internal uint                               dwOriginatorChoice;         // CMSG_KEY_AGREE_ORIGINATOR_CERT
            internal CERT_ID                            OriginatorCertId;           // CERT_ID
            internal IntPtr                             Padding;                    // Padded to the size of CERT_PUBLIC_KEY_INFO
            internal CRYPTOAPI_BLOB                     UserKeyingMaterial;
            internal CRYPT_ALGORITHM_IDENTIFIER         KeyEncryptionAlgorithm;
            internal uint                               cRecipientEncryptedKeys;
            internal IntPtr                             rgpRecipientEncryptedKeys;  // PCMSG_RECIPIENT_ENCRYPTED_KEY_INFO*
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO {
            internal uint                               dwVersion;
            internal uint                               dwOriginatorChoice;         // CMSG_KEY_AGREE_ORIGINATOR_PUBLIC_KEY
            internal CERT_PUBLIC_KEY_INFO               OriginatorPublicKeyInfo;    // CERT_PUBLIC_KEY_INFO
            internal CRYPTOAPI_BLOB              UserKeyingMaterial;
            internal CRYPT_ALGORITHM_IDENTIFIER  KeyEncryptionAlgorithm;
            internal uint                        cRecipientEncryptedKeys;
            internal IntPtr                      rgpRecipientEncryptedKeys; // PCMSG_RECIPIENT_ENCRYPTED_KEY_INFO *
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RECIPIENT_ENCRYPTED_KEY_INFO {
            // Currently, only ISSUER_SERIAL_NUMBER or KEYID choices
            internal CERT_ID                     RecipientId;
            internal CRYPTOAPI_BLOB              EncryptedKey;
            // The following optional fields are only applicable to KEYID choice
            internal _FILETIME                   Date;
            internal IntPtr                      pOtherAttr; //PCRYPT_ATTRIBUTE_TYPE_VALUE 
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA {
            internal CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA(int size) {
                cbSize = (uint) size;
                hCryptProv = IntPtr.Zero;
                dwSignerIndex = 0;
                dwSignerType = 0;
                pvSigner = IntPtr.Zero;
            }
            internal uint                       cbSize;
            internal IntPtr                     hCryptProv; // HCRYPTPROV
            internal uint                       dwSignerIndex;
            internal uint                       dwSignerType;
            internal IntPtr                     pvSigner;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_TRANS_RECIPIENT_INFO {
            internal uint                       dwVersion;
            // Currently, only ISSUER_SERIAL_NUMBER or KEYID choices
            internal CERT_ID                    RecipientId;
            internal CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal CRYPTOAPI_BLOB             EncryptedKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNED_ENCODE_INFO {
            internal CMSG_SIGNED_ENCODE_INFO(int size) {
                cbSize = (uint) size;
                cSigners = 0;
                rgSigners = IntPtr.Zero;
                cCertEncoded = 0;
                rgCertEncoded = IntPtr.Zero;
                cCrlEncoded = 0;
                rgCrlEncoded = IntPtr.Zero;
                cAttrCertEncoded = 0;
                rgAttrCertEncoded = IntPtr.Zero;
            }
            internal uint                       cbSize;
            internal uint                       cSigners;
            internal IntPtr                     rgSigners;
            internal uint                       cCertEncoded;
            internal IntPtr                     rgCertEncoded;
            internal uint                       cCrlEncoded;
            internal IntPtr                     rgCrlEncoded;
            internal uint                       cAttrCertEncoded;
            internal IntPtr                     rgAttrCertEncoded;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNER_ENCODE_INFO {
            [DllImport(KERNEL32, SetLastError=true)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern 
            IntPtr LocalFree(IntPtr hMem);

            [DllImport(ADVAPI32, SetLastError=true)]
            [ResourceExposure(ResourceScope.None)]
            internal extern static 
            bool CryptReleaseContext (
                [In]     IntPtr hProv, 
                [In]     uint   dwFlags);

            internal CMSG_SIGNER_ENCODE_INFO(int size) {
                cbSize = (uint) size;
                pCertInfo = IntPtr.Zero;
                hCryptProv = IntPtr.Zero;
                dwKeySpec = 0;
                HashAlgorithm = new CRYPT_ALGORITHM_IDENTIFIER();
                pvHashAuxInfo = IntPtr.Zero;
                cAuthAttr = 0;
                rgAuthAttr = IntPtr.Zero;
                cUnauthAttr = 0;
                rgUnauthAttr = IntPtr.Zero;
                SignerId = new CERT_ID();
                HashEncryptionAlgorithm = new CRYPT_ALGORITHM_IDENTIFIER();
                pvHashEncryptionAuxInfo = IntPtr.Zero;
            }

            internal void Dispose() {
                // Free hCryptProv
                if (hCryptProv != IntPtr.Zero) {
                    CryptReleaseContext(hCryptProv, 0);
                }
                // Free SignerId.KeyId.pbData
                if (SignerId.Value.KeyId.pbData != IntPtr.Zero) {
                    LocalFree(SignerId.Value.KeyId.pbData);
                }
                // Free rgAuthAttr and rgUnauthAttr
                if (rgAuthAttr != IntPtr.Zero) {
                    LocalFree(rgAuthAttr);
                }
                if (rgUnauthAttr != IntPtr.Zero) {
                    LocalFree(rgUnauthAttr);
                }
            }

            internal uint                       cbSize;
            internal IntPtr                     pCertInfo;
            internal IntPtr                     hCryptProv;
            internal uint                       dwKeySpec;
            internal CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal IntPtr                     pvHashAuxInfo;
            internal uint                       cAuthAttr;
            internal IntPtr                     rgAuthAttr;
            internal uint                       cUnauthAttr;
            internal IntPtr                     rgUnauthAttr;
            internal CERT_ID                    SignerId;
            internal CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal IntPtr                     pvHashEncryptionAuxInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNER_INFO {
            internal uint                       dwVersion;
            internal CRYPTOAPI_BLOB             Issuer;
            internal CRYPTOAPI_BLOB             SerialNumber;
            internal CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal CRYPTOAPI_BLOB             EncryptedHash;
            internal CRYPT_ATTRIBUTES           AuthAttrs;
            internal CRYPT_ATTRIBUTES           UnauthAttrs;
        }

        internal delegate bool PFN_CMSG_STREAM_OUTPUT(IntPtr pvArg, IntPtr pbData, uint cbData, bool fFinal);
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal class CMSG_STREAM_INFO {
            internal CMSG_STREAM_INFO (uint cbContent, PFN_CMSG_STREAM_OUTPUT pfnStreamOutput, IntPtr pvArg) {
                this.cbContent = cbContent;
                this.pfnStreamOutput = pfnStreamOutput;
                this.pvArg = pvArg;
            }

            internal uint                   cbContent;
            internal PFN_CMSG_STREAM_OUTPUT pfnStreamOutput;
            internal IntPtr                 pvArg;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string         pszObjId;
            internal CRYPTOAPI_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER2 {
            internal IntPtr         pszObjId;
            internal CRYPTOAPI_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTE {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string pszObjId;
            internal uint   cValue;
            internal IntPtr rgValue;    // PCRYPT_ATTR_BLOB
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTES {
            internal uint   cAttr;
            internal IntPtr rgAttr;     // PCRYPT_ATTRIBUTE
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTE_TYPE_VALUE {
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string                pszObjId;
            internal CRYPTOAPI_BLOB        Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_BIT_BLOB {
            internal uint   cbData;
            internal IntPtr pbData;
            internal uint   cUnusedBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_KEY_PROV_INFO {
            internal string pwszContainerName;
            internal string pwszProvName;
            internal uint   dwProvType;
            internal uint   dwFlags;
            internal uint   cProvParam;
            internal IntPtr rgProvParam;
            internal uint   dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_OID_INFO {
            internal CRYPT_OID_INFO(int size) {
                cbSize = (uint) size;
                pszOID = null;
                pwszName = null;
                dwGroupId = 0;
                Algid = 0;
                ExtraInfo = new CRYPTOAPI_BLOB();
            }
            internal uint           cbSize;
            [MarshalAs(UnmanagedType.LPStr)] 
            internal string         pszOID;
            internal string         pwszName;
            internal uint           dwGroupId;
            internal uint           Algid;
            internal CRYPTOAPI_BLOB ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_RC2_CBC_PARAMETERS {
            internal uint   dwVersion;
            internal bool   fIV;            // set if has following IV
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=8)]
            internal byte[] rgbIV;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPTOAPI_BLOB {
            internal uint   cbData;
            internal IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct DSSPUBKEY {
            internal uint magic;
            internal uint bitlen;
        }

        // Mapping from key usage friendly name to actual key usage values.
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct KEY_USAGE_STRUCT {
            internal KEY_USAGE_STRUCT(string pwszKeyUsage, uint dwKeyUsageBit) {
                this.pwszKeyUsage = pwszKeyUsage;
                this.dwKeyUsageBit = dwKeyUsageBit;
            }
            internal string pwszKeyUsage;
            internal uint   dwKeyUsageBit;
        };

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct PROV_ENUMALGS_EX {
            internal uint       aiAlgid; // ALG_ID
            internal uint       dwDefaultLen;
            internal uint       dwMinLen;
            internal uint       dwMaxLen;
            internal uint       dwProtocols;
            internal uint       dwNameLen;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=20)]
            internal byte[]     szName;
            internal uint       dwLongNameLen;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=40)]
            internal byte[]     szLongName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct RSAPUBKEY {
            internal uint magic;
            internal uint bitlen;
            internal uint pubexp;
        };
    }

    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    ///
    /// <summary>
    /// 
    /// Class   : CAPINative.
    /// 
    /// Synopsis: CAPI wrapper class containing only static methods to wrap
    ///           native CAPI through P/Invoke.
    /// 
    ///           All methods within this group require caller to have 
    ///           unmanaged code permission. So, it is fine to put any method in
    ///           here without worry of security breach.
    ///
    /// </summary>
    /// 
    ///-------------------------------------------------------------------------

    internal abstract class CAPINative : CAPIBase {
    }

    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    ///
    /// <summary>
    /// 
    /// Class   : CAPISafe.
    /// 
    /// Synopsis: CAPI wrapper class containing only static methods to wrap
    ///           safe CAPI through P/Invoke.
    /// 
    ///           All methods within this class will suppress unmanaged code
    ///           permission and demand NO other permission, which means it is
    ///           OK to be called by anyone.
    ///       
    ///           !!! DANGER !!!! 
    ///
    ///           Only in very specific situations you should place your method in
    ///           this class. You MUST be absolutely sure that the method is 
    ///           safe to be called by anyone. 
    /// 
    ///           For example, CryptDecodeObject() would be fine to be in this
    ///           class. On the other hand, CertOpenStore() definitely should
    ///           NOT, especially when called with CERT_STORE_DELETE_FLAG.
    ///
    /// </summary>
    /// 
    ///-------------------------------------------------------------------------

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal abstract class CAPISafe : CAPINative {
        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        uint FormatMessage(
            [In]     uint                 dwFlags, 
            [In]     IntPtr               lpSource,
            [In]     uint                 dwMessageId,
            [In]     uint                 dwLanguageId,
            [In,Out] StringBuilder        lpBuffer,
            [In]     uint                 nSize,
            [In]     IntPtr               Arguments);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern 
        bool FreeLibrary(
            [In] IntPtr hModule);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        IntPtr GetProcAddress(
            [In] IntPtr hModule,
            [In] [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        SafeLocalAllocHandle LocalAlloc(
            [In] uint uFlags, 
            [In] IntPtr sizetdwBytes);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false, EntryPoint="LoadLibraryA")]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern 
        IntPtr LoadLibrary(
            [In] [MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]  // This is a way of recovering from process SxS problems.
        internal static extern
        bool CertControlStore (
            [In]     SafeCertStoreHandle hCertStore,
            [In]     uint                dwFlags,
            [In]     uint                dwCtrlType,
            [In]     IntPtr              pvCtrlPara);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertContextHandle CertCreateCertificateContext (
            [In]     uint                 dwCertEncodingType,
            [In]     SafeLocalAllocHandle pbCertEncoded,
            [In]     uint                 cbCertEncoded);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertContextHandle CertDuplicateCertificateContext(
            [In]     IntPtr pCertContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertContextHandle CertDuplicateCertificateContext(
            [In]     SafeCertContextHandle pCertContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertChainHandle CertDuplicateCertificateChain(
            [In]     IntPtr pChainContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertChainHandle CertDuplicateCertificateChain(
            [In]     SafeCertChainHandle pChainContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        SafeCertStoreHandle CertDuplicateStore(
            [In]     IntPtr hCertStore);

        [DllImport(CRYPT32, CharSet=CharSet.Auto,SetLastError=true,BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        IntPtr CertFindExtension(
            [In]     [MarshalAs(UnmanagedType.LPStr)] string pszObjId,
            [In]     uint cExtensions,
            [In]     IntPtr rgExtensions);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        bool CertFreeCertificateContext (
            [In]     IntPtr pCertContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CertGetCertificateChain(
            [In]     IntPtr                  hChainEngine,
            [In]     SafeCertContextHandle   pCertContext,
            [In]     ref _FILETIME            pTime,
            [In]     SafeCertStoreHandle     hAdditionalStore,
            [In]     ref CERT_CHAIN_PARA     pChainPara,
            [In]     uint                    dwFlags,
            [In]     IntPtr                  pvReserved,
            [In,Out] ref SafeCertChainHandle ppChainContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CertGetCertificateContextProperty(
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwPropId,
            [In,Out] SafeLocalAllocHandle  pvData,
            [In,Out] ref uint              pcbData);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CertGetIntendedKeyUsage(
            [In]     uint                  dwCertEncodingType,
            [In]     IntPtr                pCertInfo,
            [In]     IntPtr                pbKeyUsage,
            [In,Out] uint                  cbKeyUsage);

        [DllImport(CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        uint CertGetNameStringW(
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwType,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pvTypePara,
            [In,Out] SafeLocalAllocHandle  pszNameString,
            [In]     uint                  cchNameString);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        uint CertGetPublicKeyLength(
            [In]    uint                  dwCertEncodingType,
            [In]    IntPtr                pPublicKey); // PCERT_PUBLIC_KEY_INFO 

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CertGetValidUsages(
            [In]     uint                 cCerts,
            [In]     IntPtr               rghCerts,
            [Out]    IntPtr               cNumOIDs,
            [In,Out] SafeLocalAllocHandle rghOIDs,
            [In,Out] IntPtr               pcbOIDs);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        uint CertNameToStrW(
            [In]     uint                 dwCertEncodingType,
            [In]     IntPtr               pName,
            [In]     uint                 dwStrType,
            [In,Out] SafeLocalAllocHandle psz,
            [In]     uint                 csz);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CertSerializeCertificateStoreElement(
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwFlags,
            [In,Out] SafeLocalAllocHandle  pbElement,
            [In,Out] IntPtr                pcbElement);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CertStrToNameW(
            [In]     uint                 dwCertEncodingType,
            [In]     string               pszX500,
            [In]     uint                 dwStrType,
            [In]     IntPtr               pvReserved,
            [In,Out] IntPtr               pbEncoded,
            [In,Out] ref uint             pcbEncoded,
            [In,Out] IntPtr               ppszError);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        int CertVerifyTimeValidity (
            [In,Out] ref _FILETIME pTimeToVerify,
            [In]     IntPtr       pCertInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CertVerifyCertificateChainPolicy(
            [In]     IntPtr                       pszPolicyOID,
            [In]     SafeCertChainHandle          pChainContext,
            [In]     ref CERT_CHAIN_POLICY_PARA   pPolicyPara,
            [In,Out] ref CERT_CHAIN_POLICY_STATUS pPolicyStatus);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptAcquireCertificatePrivateKey(
            [In]     SafeCertContextHandle      pCert,
            [In]     uint                       dwFlags,
            [In]     IntPtr                     pvReserved,
            [In,Out] ref SafeCryptProvHandle    phCryptProv,
            [In,Out] ref uint                   pdwKeySpec,
            [In,Out] ref bool                   pfCallerFreeProv);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CryptDecodeObject(
            [In]     uint                 dwCertEncodingType,
            [In]     IntPtr               lpszStructType,
            [In]     IntPtr               pbEncoded,
            [In]     uint                 cbEncoded,
            [In]     uint                 dwFlags,
            [In,Out] SafeLocalAllocHandle pvStructInfo,
            [In,Out] IntPtr               pcbStructInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CryptDecodeObject(
            [In]     uint                 dwCertEncodingType,
            [In]     IntPtr               lpszStructType,
            [In]     byte[]               pbEncoded,
            [In]     uint                 cbEncoded,
            [In]     uint                 dwFlags,
            [In,Out] SafeLocalAllocHandle pvStructInfo,
            [In,Out] IntPtr               pcbStructInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CryptEncodeObject(
            [In]     uint                 dwCertEncodingType,
            [In]     IntPtr               lpszStructType,
            [In]     IntPtr               pvStructInfo,
            [In,Out] SafeLocalAllocHandle pbEncoded,
            [In,Out] IntPtr               pcbEncoded);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern 
        bool CryptEncodeObject(
            [In]     uint                 dwCertEncodingType,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string lpszStructType,
            [In]     IntPtr               pvStructInfo,
            [In,Out] SafeLocalAllocHandle pbEncoded,
            [In,Out] IntPtr               pcbEncoded);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        IntPtr CryptFindOIDInfo(
            [In]     uint                 dwKeyType,
            [In]     IntPtr               pvKey,
            [In]     OidGroup             dwGroupId);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        IntPtr CryptFindOIDInfo(
            [In]     uint                 dwKeyType,
            [In]     SafeLocalAllocHandle pvKey,
            [In]     OidGroup             dwGroupId);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptFormatObject(
            [In]     uint   dwCertEncodingType,
            [In]     uint   dwFormatType,
            [In]     uint   dwFormatStrType,
            [In]     IntPtr pFormatStruct,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string lpszStructType,
            [In]     byte[] pbEncoded,
            [In]     uint   cbEncoded,
            [In,Out] SafeLocalAllocHandle pbFormat,
            [In,Out] IntPtr pcbFormat);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptFormatObject(
            [In]     uint   dwCertEncodingType,
            [In]     uint   dwFormatType,
            [In]     uint   dwFormatStrType,
            [In]     IntPtr pFormatStruct,
            [In]     IntPtr lpszStructType,
            [In]     byte[] pbEncoded,
            [In]     uint   cbEncoded,
            [In,Out] SafeLocalAllocHandle pbFormat,
            [In,Out] IntPtr pcbFormat);

        [DllImport(ADVAPI32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptGetProvParam(
            [In] SafeCryptProvHandle        hProv,
            [In] uint                       dwParam,
            [In] IntPtr                     pbData,
            [In] IntPtr                     pdwDataLen,
            [In] uint                       dwFlags);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptHashCertificate(
            [In]      IntPtr hCryptProv, // HCRYPTPROV
            [In]      uint   Algid,      // ALG_ID
            [In]      uint   dwFlags,
            [In]      IntPtr pbEncoded,
            [In]      uint   cbEncoded,
            [Out]     IntPtr pbComputedHash,
            [In, Out] IntPtr pcbComputedHash);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptHashPublicKeyInfo(
            [In]      IntPtr hCryptProv, // HCRYPTPROV
            [In]      uint   Algid,      // ALG_ID
            [In]      uint   dwFlags,
            [In]      uint   dwCertEncodingType,
            [In]      IntPtr pInfo,
            [Out]     IntPtr pbComputedHash,
            [In, Out] IntPtr pcbComputedHash);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptMsgGetParam(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwParamType,
            [In]      uint                  dwIndex,
            [In, Out] IntPtr                pvData,
            [In, Out] IntPtr                pcbData);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptMsgGetParam(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwParamType,
            [In]      uint                  dwIndex,
            [In, Out] SafeLocalAllocHandle  pvData,
            [In, Out] IntPtr                pcbData);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        SafeCryptMsgHandle CryptMsgOpenToDecode(
            [In]      uint               dwMsgEncodingType,
            [In]      uint               dwFlags,
            [In]      uint               dwMsgType,
            [In]      IntPtr             hCryptProv,
            [In]      IntPtr             pRecipientInfo,
            [In]      IntPtr             pStreamInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptMsgUpdate(
            [In]      SafeCryptMsgHandle hCryptMsg,
            [In]      byte[]             pbData,
            [In]      uint               cbData,
            [In]      bool               fFinal);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptMsgUpdate(
            [In]      SafeCryptMsgHandle hCryptMsg,
            [In]      IntPtr             pbData,
            [In]      uint               cbData,
            [In]      bool               fFinal);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static 
        bool CryptMsgVerifyCountersignatureEncoded(
            [In]      IntPtr              hCryptProv,
            [In]      uint                dwEncodingType,
            [In]      IntPtr              pbSignerInfo,
            [In]      uint                cbSignerInfo,
            [In]      IntPtr              pbSignerInfoCountersignature,
            [In]      uint                cbSignerInfoCountersignature,
            [In]      IntPtr              pciCountersigner);

        [DllImport(KERNEL32, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal extern static
        void SetLastError(uint dwErrorCode);

        [DllImport(KERNEL32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        IntPtr LocalFree(IntPtr handle);

        [DllImport(KERNEL32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        void ZeroMemory(IntPtr handle, uint length);

        [DllImport(ADVAPI32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        int LsaNtStatusToWinError (
            [In]    int         status);
    }

    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    ///
    /// <summary>
    /// 
    /// Class   : CAPIUnsafe.
    /// 
    /// Synopsis: CAPI wrapper class containing only static methods to wrap
    ///           unsafe CAPI through P/Invoke.
    /// 
    ///           All methods within this class will suppress unmanaged code
    ///           permission but should in turn demand for other appropriate 
    ///           permission(s).
    ///       
    ///           !!! WARNING !!!! 
    /// 
    ///           You should first consider whether the method should go into
    ///           the Native class above.
    /// 
    ///           If you are convinced that the method does belong in this
    ///           class, you MUST:
    ///           
    ///           (1) declare the method as "protected internal" here.
    ///           
    ///           (2) override the method in CAPI class below.
    ///           
    ///           (3) demand appropriate permission(s) in your override.
    ///           
    ///           (4) properly validate all parameters. 
    /// 
    ///           For example, if you decide to put any method that will create
    ///           a file on disk, such as CertSaveStore() with the
    ///           CERT_STORE_SAVE_TO_FILENAME flag, you then better demand
    ///           FileIOPermission for Write, and make sure to also validate
    ///           the filename (think about what will happen if the filename is
    ///           C:\boot.ini?).
    ///
    /// </summary>
    /// 
    ///-------------------------------------------------------------------------

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal abstract class CAPIUnsafe : CAPISafe {
        [DllImport(ADVAPI32, CharSet=CharSet.Auto, BestFitMapping=false, EntryPoint="CryptAcquireContextA")]
        [ResourceExposure(ResourceScope.Machine)]
        internal protected extern static 
        bool CryptAcquireContext(
            [In,Out] ref SafeCryptProvHandle                  hCryptProv,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string  pszContainer,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string  pszProvider,
            [In]     uint                                     dwProvType,
            [In]     uint                                     dwFlags);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertAddCertificateContextToStore (
            [In]     SafeCertStoreHandle   hCertStore,
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwAddDisposition,
            [In,Out] SafeCertContextHandle ppStoreContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertAddCertificateLinkToStore (
            [In]     SafeCertStoreHandle   hCertStore,
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwAddDisposition,
            [In,Out] SafeCertContextHandle ppStoreContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertDeleteCertificateFromStore (
            [In]     SafeCertContextHandle pCertContext);

        [DllImport (CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        IntPtr CertEnumCertificatesInStore(
            [In]     SafeCertStoreHandle hCertStore, 
            [In]     IntPtr              pPrevCertContext);

        [DllImport (CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        SafeCertContextHandle CertEnumCertificatesInStore (
            [In]     SafeCertStoreHandle   hCertStore, 
            [In]     SafeCertContextHandle pPrevCertContext);


        [DllImport (CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        SafeCertContextHandle CertFindCertificateInStore(
            [In]     SafeCertStoreHandle   hCertStore,
            [In]     uint                  dwCertEncodingType,
            [In]     uint                  dwFindFlags,
            [In]     uint                  dwFindType,
            [In]     IntPtr                pvFindPara,
            [In]     SafeCertContextHandle pPrevCertContext);

        [DllImport(CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern 
        SafeCertStoreHandle CertOpenStore(
            [In]     IntPtr lpszStoreProvider,
            [In]     uint   dwMsgAndCertEncodingType,
            [In]     IntPtr hCryptProv,
            [In]     uint   dwFlags,
            [In]     string pvPara); // we want this always as a Unicode string.

        [DllImport (CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        bool CertSaveStore(
            [In]     SafeCertStoreHandle hCertStore,
            [In]     uint                dwMsgAndCertEncodingType,
            [In]     uint                dwSaveAs,
            [In]     uint                dwSaveTo,
            [In,Out] IntPtr              pvSaveToPara,
            [In]     uint                dwFlags);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertSetCertificateContextProperty (
            [In]     IntPtr                pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pvData);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertSetCertificateContextProperty (
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pvData);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CertSetCertificateContextProperty (
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     SafeLocalAllocHandle  safeLocalAllocHandle);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        SafeCertContextHandle CertCreateSelfSignCertificate(
            [In]     SafeCryptProvHandle   hProv,
            [In]     IntPtr                pSubjectIssuerBlob,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pKeyProvInfo,
            [In]     IntPtr                pSignatureAlgorithm,
            [In]     IntPtr                pStartTime,
            [In]     IntPtr                pEndTime,
            [In]     IntPtr                pExtensions);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CryptMsgControl(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwFlags,
            [In]      uint                  dwCtrlType,
            [In]      IntPtr                pvCtrlPara);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        bool CryptMsgCountersign(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwIndex,
            [In]      uint                  cCountersigners,
            [In]      IntPtr                rgCountersigners);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        SafeCryptMsgHandle CryptMsgOpenToEncode(
            [In]      uint                        dwMsgEncodingType,
            [In]      uint                        dwFlags,
            [In]      uint                        dwMsgType,
            [In]      IntPtr                      pvMsgEncodeInfo,
            [In]      IntPtr                      pszInnerContentObjID,
            [In]      IntPtr                      pStreamInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        internal protected extern static 
        SafeCryptMsgHandle CryptMsgOpenToEncode(
            [In]      uint                                    dwMsgEncodingType,
            [In]      uint                                    dwFlags,
            [In]      uint                                    dwMsgType,
            [In]      IntPtr                                  pvMsgEncodeInfo,
            [In]      [MarshalAs(UnmanagedType.LPStr)] string pszInnerContentObjID,
            [In]      IntPtr                                  pStreamInfo);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        bool CryptQueryObject (
            [In]     uint   dwObjectType,
            [In]     IntPtr pvObject,
            [In]     uint   dwExpectedContentTypeFlags,
            [In]     uint   dwExpectedFormatTypeFlags,
            [In]     uint   dwFlags,
            [Out]    IntPtr pdwMsgAndCertEncodingType,
            [Out]    IntPtr pdwContentType,
            [Out]    IntPtr pdwFormatType,
            [In,Out] IntPtr phCertStore,
            [In,Out] IntPtr phMsg,
            [In,Out] IntPtr ppvContext);

        [DllImport(CRYPT32, CharSet=CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        bool CryptQueryObject (
            [In]     uint                    dwObjectType,
            [In]     IntPtr                  pvObject,
            [In]     uint                    dwExpectedContentTypeFlags,
            [In]     uint                    dwExpectedFormatTypeFlags,
            [In]     uint                    dwFlags,
            [Out]    IntPtr                  pdwMsgAndCertEncodingType,
            [Out]    IntPtr                  pdwContentType,
            [Out]    IntPtr                  pdwFormatType,
            [In,Out] ref SafeCertStoreHandle phCertStore,
            [In,Out] IntPtr                  phMsg,
            [In,Out] IntPtr                  ppvContext);

        [DllImport(CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        bool CryptProtectData (
            [In]     IntPtr pDataIn,
            [In]     string szDataDescr,
            [In]     IntPtr pOptionalEntropy,
            [In]     IntPtr pvReserved,
            [In]     IntPtr pPromptStruct,
            [In]     uint   dwFlags,
            [In,Out] IntPtr pDataBlob);

        [DllImport(CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern
        bool CryptUnprotectData (
            [In]     IntPtr pDataIn,
            [In]     IntPtr ppszDataDescr,
            [In]     IntPtr pOptionalEntropy,
            [In]     IntPtr pvReserved,
            [In]     IntPtr pPromptStruct,
            [In]     uint   dwFlags,
            [In,Out] IntPtr pDataBlob);

        [DllImport (CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        bool PFXExportCertStore(
            [In]     SafeCertStoreHandle hStore,
            [In,Out] IntPtr              pPFX,
            [In]     string              szPassword,
            [In]     uint                dwFlags);

        [DllImport(CRYPT32, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        internal protected static extern
        SafeCertStoreHandle PFXImportCertStore (
            [In]     IntPtr pPFX,
            [In]     string szPassword,
            [In]     uint   dwFlags);
    }

    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    ///
    /// <summary>
    /// 
    /// Class   : CAPIMethods.
    /// 
    /// Synopsis: Dummy layer.
    ///
    /// </summary>
    /// 
    ///-------------------------------------------------------------------------

    internal abstract class CAPIMethods : CAPIUnsafe {
    }

    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    ///
    /// <summary>
    /// 
    /// Class   : CAPI.
    /// 
    /// Synopsis: This class provides overrides for above classes.
    ///
    /// </summary>
    /// 
    ///-------------------------------------------------------------------------

    [SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses", Justification="by design; fixing requires changing class hierarchy")]
    internal sealed class CAPI : CAPIMethods {
        internal static
        byte[] BlobToByteArray(IntPtr pBlob) {
            CAPI.CRYPTOAPI_BLOB blob = (CAPI.CRYPTOAPI_BLOB) Marshal.PtrToStructure(pBlob, typeof(CAPI.CRYPTOAPI_BLOB));
            if (blob.cbData == 0)
                return new byte[0];
            return BlobToByteArray(blob);
        }

        internal static
        byte[] BlobToByteArray(CAPI.CRYPTOAPI_BLOB blob) {
            if (blob.cbData == 0)
                return new byte[0];
            byte[] data = new byte[blob.cbData];
            Marshal.Copy(blob.pbData, data, 0, data.Length);
            return data;
        }

        internal static unsafe
        bool DecodeObject(IntPtr pszStructType, 
                          IntPtr pbEncoded,
                          uint   cbEncoded,
                          out SafeLocalAllocHandle decodedValue,
                          out uint cbDecodedValue) {
            // Initialize out parameters
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;

            // Decode
            uint cbDecoded = 0;
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            bool result = CAPIMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                        pszStructType,
                                                        pbEncoded,
                                                        cbEncoded,
                                                        0,
                                                        ptr,
                                                        new IntPtr(&cbDecoded));
            if (result == false)
                return false;

            ptr = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbDecoded));
            result = CAPIMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                   pszStructType,
                                                   pbEncoded,
                                                   cbEncoded,
                                                   0,
                                                   ptr,
                                                   new IntPtr(&cbDecoded));
            if (result == false)
                return false;

            // Return decoded values
            decodedValue = ptr;
            cbDecodedValue = cbDecoded;

            return true;
        }

        internal static unsafe
        bool DecodeObject(IntPtr pszStructType, 
                          byte[] pbEncoded,
                          out SafeLocalAllocHandle decodedValue,
                          out uint cbDecodedValue) {
            // Initialize out parameters
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;

            // Decode
            uint cbDecoded = 0;
            SafeLocalAllocHandle pbDecoded = SafeLocalAllocHandle.InvalidHandle;
            
            if (!CAPIMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               pszStructType,
                                               pbEncoded,
                                               (uint) pbEncoded.Length,
                                               0,
                                               pbDecoded,
                                               new IntPtr(&cbDecoded)))
                return false;

            pbDecoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbDecoded));
            if (!CAPIMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               pszStructType,
                                               pbEncoded,
                                               (uint) pbEncoded.Length,
                                               0,
                                               pbDecoded,
                                               new IntPtr(&cbDecoded)))
                return false;

            // Return decoded values
            decodedValue = pbDecoded;
            cbDecodedValue = cbDecoded;

            return true;
        }

        internal static unsafe
        bool EncodeObject(IntPtr lpszStructType,
                          IntPtr pvStructInfo,
                          out byte[] encodedData) {
            // Initialize out parameter
            encodedData = new byte[0];

            // Encode
            uint cbEncoded = 0;
            SafeLocalAllocHandle pbEncoded = SafeLocalAllocHandle.InvalidHandle;

            if (!CAPIMethods.CryptEncodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               lpszStructType,
                                               pvStructInfo,
                                               pbEncoded,
                                               new IntPtr(&cbEncoded)))
                return false;

            pbEncoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbEncoded));
            if (!CAPIMethods.CryptEncodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               lpszStructType,
                                               pvStructInfo,
                                               pbEncoded,
                                               new IntPtr(&cbEncoded)))
                return false;

            // Return encoded data
            encodedData = new byte[cbEncoded];
            Marshal.Copy(pbEncoded.DangerousGetHandle(), encodedData, 0, (int) cbEncoded);

            pbEncoded.Dispose();
            return true;
        }

        internal static unsafe
        bool EncodeObject(string lpszStructType,
                          IntPtr pvStructInfo,
                          out byte[] encodedData) {
            // Initialize out parameter
            encodedData = new byte[0];

            // Encode
            uint cbEncoded = 0;
            SafeLocalAllocHandle pbEncoded = SafeLocalAllocHandle.InvalidHandle;

            if (!CAPIMethods.CryptEncodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               lpszStructType,
                                               pvStructInfo,
                                               pbEncoded,
                                               new IntPtr(&cbEncoded)))
                return false;

            pbEncoded = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbEncoded));
            if (!CAPIMethods.CryptEncodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               lpszStructType,
                                               pvStructInfo,
                                               pbEncoded,
                                               new IntPtr(&cbEncoded)))
                return false;

            // Return encoded data
            encodedData = new byte[cbEncoded];
            Marshal.Copy(pbEncoded.DangerousGetHandle(), encodedData, 0, (int) cbEncoded);

            pbEncoded.Dispose();
            return true;
        }

        internal static unsafe
        string GetCertNameInfo(
            [In]    SafeCertContextHandle safeCertContext,
            [In]    uint                  dwFlags,
            [In]    uint                  dwDisplayType) {

            if (safeCertContext == null)
                throw new ArgumentNullException("pCertContext");
            if (safeCertContext.IsInvalid) 
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "safeCertContext");

            uint dwStrType = CERT_X500_NAME_STR | CERT_NAME_STR_REVERSE_FLAG;
            SafeLocalAllocHandle pszCommonName = SafeLocalAllocHandle.InvalidHandle;
            if (dwDisplayType == CAPI.CERT_NAME_ATTR_TYPE)
                pszCommonName = X509Utils.StringToAnsiPtr(CAPI.szOID_COMMON_NAME);

            uint cchCount = 0;
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            cchCount = CertGetNameStringW(safeCertContext,
                                          dwDisplayType,
                                          dwFlags,
                                          (dwDisplayType == CAPI.CERT_NAME_ATTR_TYPE ? pszCommonName.DangerousGetHandle() : new IntPtr(&dwStrType)),
                                          ptr,
                                          0);
            if (cchCount == 0) 
                throw new CryptographicException(Marshal.GetLastWin32Error());

            ptr = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(2 * cchCount));
            if (CertGetNameStringW(safeCertContext,
                                   dwDisplayType,
                                   dwFlags,
                                   (dwDisplayType == CAPI.CERT_NAME_ATTR_TYPE ? pszCommonName.DangerousGetHandle() : new IntPtr(&dwStrType)),
                                   ptr,
                                   cchCount) == 0)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            string name = Marshal.PtrToStringUni(ptr.DangerousGetHandle());
            ptr.Dispose();
            pszCommonName.Dispose();
            return name;
        }

        new internal static 
        SafeLocalAllocHandle LocalAlloc(uint uFlags, IntPtr sizetdwBytes) {
            SafeLocalAllocHandle safeLocalAllocHandle = CAPIMethods.LocalAlloc(uFlags, sizetdwBytes);
            if (safeLocalAllocHandle == null || safeLocalAllocHandle.IsInvalid) 
                throw new OutOfMemoryException();

            return safeLocalAllocHandle;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        new internal static 
        bool CryptAcquireContext(
            [In,Out] ref SafeCryptProvHandle                  hCryptProv,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string  pwszContainer,
            [In]     [MarshalAs(UnmanagedType.LPStr)] string  pwszProvider,
            [In]     uint                                     dwProvType,
            [In]     uint                                     dwFlags) {

            CspParameters parameters = new CspParameters();
            parameters.ProviderName = pwszProvider;
            parameters.KeyContainerName = pwszContainer;
            parameters.ProviderType = (int) dwProvType;
            parameters.KeyNumber = -1;
            parameters.Flags = (CspProviderFlags) ((dwFlags & CAPI.CRYPT_MACHINE_KEYSET) == CAPI.CRYPT_MACHINE_KEYSET ? CspProviderFlags.UseMachineKeyStore : 0);

            if (!CompatibilitySwitches.IsAppEarlierThanWindowsPhone8) {
                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open);
                kp.AccessEntries.Add(entry);
                kp.Demand();
            }

            bool rc = CAPIMethods.CryptAcquireContext(ref hCryptProv,
                                                      pwszContainer,
                                                      pwszProvider,
                                                      dwProvType,
                                                      dwFlags);

            if (!rc && Marshal.GetLastWin32Error() == CAPI.NTE_BAD_KEYSET) {
                rc = CAPIMethods.CryptAcquireContext(ref hCryptProv,
                                                     pwszContainer,
                                                     pwszProvider,
                                                     dwProvType,
                                                     dwFlags | CAPI.CRYPT_NEWKEYSET);
            }

            return rc;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static 
        bool CryptAcquireContext(
            ref SafeCryptProvHandle  hCryptProv,
            IntPtr                   pwszContainer,
            IntPtr                   pwszProvider,
            uint                     dwProvType,
            uint                     dwFlags) {

            string container = null;
            if (pwszContainer != IntPtr.Zero) { 
                container = Marshal.PtrToStringUni(pwszContainer);
            }

            string provider = null;
            if (pwszProvider != IntPtr.Zero) { 
                provider = Marshal.PtrToStringUni(pwszProvider);
            }

            return CryptAcquireContext(ref hCryptProv,
                                       container,
                                       provider,
                                       dwProvType,
                                       dwFlags);
        }

        new internal static
        CRYPT_OID_INFO CryptFindOIDInfo(
            [In]    uint     dwKeyType,
            [In]    IntPtr   pvKey,
            [In]    OidGroup dwGroupId) {

            if (pvKey == IntPtr.Zero)
                throw new ArgumentNullException("pvKey");

            CRYPT_OID_INFO pOIDInfo = new CRYPT_OID_INFO(Marshal.SizeOf(typeof(CRYPT_OID_INFO)));
            IntPtr pv = CAPIMethods.CryptFindOIDInfo(dwKeyType, 
                                                     pvKey,
                                                     dwGroupId);

            if (pv != IntPtr.Zero)
                pOIDInfo = (CRYPT_OID_INFO) Marshal.PtrToStructure(pv, typeof(CAPI.CRYPT_OID_INFO));

            return pOIDInfo;
        }

        new internal static
        CRYPT_OID_INFO CryptFindOIDInfo(
            [In]    uint                 dwKeyType,
            [In]    SafeLocalAllocHandle pvKey,
            [In]    OidGroup             dwGroupId) {

            if (pvKey == null)
                throw new ArgumentNullException("pvKey");
            if (pvKey.IsInvalid) 
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "pvKey");

            CRYPT_OID_INFO pOIDInfo = new CRYPT_OID_INFO(Marshal.SizeOf(typeof(CRYPT_OID_INFO)));
            IntPtr pv = CAPIMethods.CryptFindOIDInfo(dwKeyType, 
                                                     pvKey,
                                                     dwGroupId);

            if (pv != IntPtr.Zero)
                pOIDInfo = (CRYPT_OID_INFO) Marshal.PtrToStructure(pv, typeof(CAPI.CRYPT_OID_INFO));

            return pOIDInfo;
        }

        internal static unsafe
        string CryptFormatObject(
            [In]    uint    dwCertEncodingType,
            [In]    uint    dwFormatStrType,
            [In]    string  lpszStructType,
            [In]    byte[]  rawData) {

            if (rawData == null)
                throw new ArgumentNullException("rawData");

            uint cbFormat = 0;
            SafeLocalAllocHandle pbFormat = SafeLocalAllocHandle.InvalidHandle;

            //
            // This method will never throw an exception. If the call to CryptFormatObject fails
            // we'll just return a hex representation of the ASN data.
            //

            if (!CAPIMethods.CryptFormatObject(dwCertEncodingType, 
                                               0, 
                                               dwFormatStrType,
                                               IntPtr.Zero,
                                               lpszStructType,
                                               rawData,
                                               (uint) rawData.Length,
                                               pbFormat,
                                               new IntPtr(&cbFormat)))
                return X509Utils.EncodeHexString(rawData);

            pbFormat = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbFormat));
            if (!CAPIMethods.CryptFormatObject(dwCertEncodingType, 
                                               0, 
                                               dwFormatStrType,
                                               IntPtr.Zero,
                                               lpszStructType,
                                               rawData,
                                               (uint) rawData.Length,
                                               pbFormat,
                                               new IntPtr(&cbFormat)))
                return X509Utils.EncodeHexString(rawData);

            string s = Marshal.PtrToStringUni(pbFormat.DangerousGetHandle());
            pbFormat.Dispose();

            return s;
        }

        internal static unsafe
        string CryptFormatObject(
            [In]    uint    dwCertEncodingType,
            [In]    uint    dwFormatStrType,
            [In]    IntPtr  lpszStructType,
            [In]    byte[]  rawData) {

            if (rawData == null)
                throw new ArgumentNullException("rawData");

            uint cbFormat = 0;
            SafeLocalAllocHandle pbFormat = SafeLocalAllocHandle.InvalidHandle;

            //
            // This method will never throw an exception. If the call to CryptFormatObject fails
            // we'll just return a hex representation of the ASN data.
            //

            if (!CAPIMethods.CryptFormatObject(dwCertEncodingType, 
                                               0, 
                                               dwFormatStrType,
                                               IntPtr.Zero,
                                               lpszStructType,
                                               rawData,
                                               (uint) rawData.Length,
                                               pbFormat,
                                               new IntPtr(&cbFormat)))
                return X509Utils.EncodeHexString(rawData);

            pbFormat = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(cbFormat));
            if (!CAPIMethods.CryptFormatObject(dwCertEncodingType, 
                                               0, 
                                               dwFormatStrType,
                                               IntPtr.Zero,
                                               lpszStructType,
                                               rawData,
                                               (uint) rawData.Length,
                                               pbFormat,
                                               new IntPtr(&cbFormat)))
                return X509Utils.EncodeHexString(rawData);

            string s = Marshal.PtrToStringUni(pbFormat.DangerousGetHandle());
            pbFormat.Dispose();

            return s;
        }

        new internal static 
        bool CryptMsgControl(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwFlags,
            [In]      uint                  dwCtrlType,
            [In]      IntPtr                pvCtrlPara) {

            // We are supposed to demand Open and Sign permissions for the key container here
            // for the CMSG_CTRL_DECRYPT class of parameters. For now, the caller should demand,
            // as to do the demand here will be more complicated and expensive since the CSP
            // parameters are not readily available here.
            //

            return CAPIMethods.CryptMsgControl(hCryptMsg,
                                               dwFlags,
                                               dwCtrlType,
                                               pvCtrlPara);
        }

        new internal static 
        bool CryptMsgCountersign(
            [In]      SafeCryptMsgHandle    hCryptMsg,
            [In]      uint                  dwIndex,
            [In]      uint                  cCountersigners,
            [In]      IntPtr                rgCountersigners) {

            //
            // We are supposed to demand Open and Sign permissions for the key container here.
            // For now, the caller should demand, as to do the demand here will be more
            // complicated since the CSP parameters are not readily available here.
            //

            return CAPIMethods.CryptMsgCountersign(hCryptMsg,
                                                   dwIndex,
                                                   cCountersigners,
                                                   rgCountersigners);
        }

        new internal static 
        SafeCryptMsgHandle CryptMsgOpenToEncode(
            [In]      uint                        dwMsgEncodingType,
            [In]      uint                        dwFlags,
            [In]      uint                        dwMsgType,
            [In]      IntPtr                      pvMsgEncodeInfo,
            [In]      IntPtr                      pszInnerContentObjID,
            [In]      IntPtr                      pStreamInfo) {

            //
            // We are supposed to demand Open and Sign permissions for the key container here.
            // For now, the caller should demand, as to do the demand here will be more
            // complicated since the CSP parameters are not readily available here.
            //

            return CAPIMethods.CryptMsgOpenToEncode(dwMsgEncodingType,
                                                    dwFlags,
                                                    dwMsgType,
                                                    pvMsgEncodeInfo,
                                                    pszInnerContentObjID,
                                                    pStreamInfo);
        }

        new internal static 
        SafeCryptMsgHandle CryptMsgOpenToEncode(
            [In]      uint                        dwMsgEncodingType,
            [In]      uint                        dwFlags,
            [In]      uint                        dwMsgType,
            [In]      IntPtr                      pvMsgEncodeInfo,
            [In]      string                      pszInnerContentObjID,
            [In]      IntPtr                      pStreamInfo) {
           
            //
            // We are supposed to demand Open and Sign permissions for the key container here.
            // For now, the caller should demand, as to do the demand here will be more
            // complicated and expensive since the CSP parameters are not readily available
            // here.
            //

            return CAPIMethods.CryptMsgOpenToEncode(dwMsgEncodingType,
                                                    dwFlags,
                                                    dwMsgType,
                                                    pvMsgEncodeInfo,
                                                    pszInnerContentObjID,
                                                    pStreamInfo);
        }

        new internal static
        bool CertSetCertificateContextProperty (
            [In]     IntPtr                pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pvData) {

            if (pvData == IntPtr.Zero)
                throw new ArgumentNullException("pvData");

            if (dwPropId != CERT_ARCHIVED_PROP_ID && dwPropId != CERT_FRIENDLY_NAME_PROP_ID &&
                dwPropId != CERT_DELETE_KEYSET_PROP_ID && dwPropId != CERT_KEY_PROV_INFO_PROP_ID)
                throw new ArgumentException(SR.GetString(SR.Security_InvalidValue), "dwFlags");

            // We do not want semi-trusted code to archive or set the friendly name property of a cert.
            if (dwPropId == CERT_ARCHIVED_PROP_ID || dwPropId == CERT_FRIENDLY_NAME_PROP_ID || dwPropId == CERT_KEY_PROV_INFO_PROP_ID)
                new PermissionSet(PermissionState.Unrestricted).Demand();

            return CAPIMethods.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, pvData);
        }

        new internal static
        bool CertSetCertificateContextProperty (
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     IntPtr                pvData) {

            if (pvData == IntPtr.Zero)
                throw new ArgumentNullException("pvData");

            if (dwPropId != CERT_ARCHIVED_PROP_ID && dwPropId != CERT_FRIENDLY_NAME_PROP_ID &&
                dwPropId != CERT_DELETE_KEYSET_PROP_ID && dwPropId != CERT_KEY_PROV_INFO_PROP_ID)
                throw new ArgumentException(SR.GetString(SR.Security_InvalidValue), "dwFlags");

            // We do not want semi-trusted code to archive or set the friendly name property of a cert.
            if (dwPropId == CERT_ARCHIVED_PROP_ID || dwPropId == CERT_FRIENDLY_NAME_PROP_ID || dwPropId == CERT_KEY_PROV_INFO_PROP_ID)
                new PermissionSet(PermissionState.Unrestricted).Demand();

            return CAPIMethods.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, pvData);
        }

        new internal static
        bool CertSetCertificateContextProperty (
            [In]     SafeCertContextHandle pCertContext,
            [In]     uint                  dwPropId,
            [In]     uint                  dwFlags,
            [In]     SafeLocalAllocHandle  safeLocalAllocHandle) {

            if (pCertContext == null)
                throw new ArgumentNullException("pCertContext");
            if (pCertContext.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "pCertContext");

            if (dwPropId != CERT_ARCHIVED_PROP_ID && dwPropId != CERT_FRIENDLY_NAME_PROP_ID &&
                dwPropId != CERT_DELETE_KEYSET_PROP_ID && dwPropId != CERT_KEY_PROV_INFO_PROP_ID)
                throw new ArgumentException(SR.GetString(SR.Security_InvalidValue), "dwFlags");

            // We do not want semi-trusted code to archive or set the friendly name property of a cert.
            if (dwPropId == CERT_ARCHIVED_PROP_ID || dwPropId == CERT_FRIENDLY_NAME_PROP_ID || dwPropId == CERT_KEY_PROV_INFO_PROP_ID)
                new PermissionSet(PermissionState.Unrestricted).Demand();

            return CAPIMethods.CertSetCertificateContextProperty(pCertContext, dwPropId, dwFlags, safeLocalAllocHandle);
        }

        new internal static
        SafeCertContextHandle CertDuplicateCertificateContext (
            [In]     IntPtr     pCertContext) {
            if (pCertContext == IntPtr.Zero)
                return SafeCertContextHandle.InvalidHandle;

            return CAPIMethods.CertDuplicateCertificateContext(pCertContext);
        }

        new internal static
        SafeCertContextHandle CertDuplicateCertificateContext (
            [In]     SafeCertContextHandle      pCertContext) {
            if (pCertContext == null || pCertContext.IsInvalid)
                return SafeCertContextHandle.InvalidHandle;

            return CAPIMethods.CertDuplicateCertificateContext(pCertContext);
        }

        new internal static
        IntPtr CertEnumCertificatesInStore (
            [In]     SafeCertStoreHandle     hCertStore, 
            [In]     IntPtr                  pPrevCertContext) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            if (pPrevCertContext == IntPtr.Zero) {
                StorePermission sp = new StorePermission(StorePermissionFlags.EnumerateCertificates);
                sp.Demand();
            }

            IntPtr handle = CAPIMethods.CertEnumCertificatesInStore(hCertStore, pPrevCertContext);
            if (handle == IntPtr.Zero) {
                int dwErrorCode = Marshal.GetLastWin32Error();
                if (dwErrorCode != CRYPT_E_NOT_FOUND) {
                    CAPIMethods.CertFreeCertificateContext(handle);
                    throw new CryptographicException(dwErrorCode);
                }
            }
            return handle;
        }

        new internal static
        SafeCertContextHandle CertEnumCertificatesInStore (
            [In]     SafeCertStoreHandle     hCertStore, 
            [In]     SafeCertContextHandle   pPrevCertContext) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            if (pPrevCertContext.IsInvalid) {
                StorePermission sp = new StorePermission(StorePermissionFlags.EnumerateCertificates);
                sp.Demand();
            }

            SafeCertContextHandle safeCertContextHandle = CAPIMethods.CertEnumCertificatesInStore(hCertStore, pPrevCertContext);
            if (safeCertContextHandle == null || safeCertContextHandle.IsInvalid) {
                int dwErrorCode = Marshal.GetLastWin32Error();
                if (dwErrorCode != CRYPT_E_NOT_FOUND)
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return safeCertContextHandle;
        }

        //
        // This override demands FileIOPermission.Read if dwObjectType is CERT_QUERY_OBJECT_FILE.
        //

        [ResourceExposure(ResourceScope.Machine)]  // If you pass in a file name
        [ResourceConsumption(ResourceScope.Machine)]
        internal static unsafe
        bool CryptQueryObject (
            [In]     uint                     dwObjectType,
            [In]     object                   pvObject,
            [In]     uint                     dwExpectedContentTypeFlags,
            [In]     uint                     dwExpectedFormatTypeFlags,
            [In]     uint                     dwFlags,
            [Out]    IntPtr                   pdwMsgAndCertEncodingType,
            [Out]    IntPtr                   pdwContentType,
            [Out]    IntPtr                   pdwFormatType,
            [In,Out] IntPtr                   phCertStore,
            [In,Out] IntPtr                   phMsg,
            [In,Out] IntPtr                   ppvContext) {

            bool result = false;
            GCHandle handle = GCHandle.Alloc(pvObject, GCHandleType.Pinned);
            IntPtr pbData = handle.AddrOfPinnedObject();
            CRYPTOAPI_BLOB certBlob;

            try {
                if (pvObject == null)
                    throw new ArgumentNullException("pvObject");

                if (dwObjectType == CERT_QUERY_OBJECT_FILE) {
                    string fullPath = Path.GetFullPath((string) pvObject);
                    new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                } else {
                    certBlob.cbData = (uint) ((byte[]) pvObject).Length;
                    certBlob.pbData = pbData;
                    pbData = new IntPtr(&certBlob);
                }

                result = CAPIMethods.CryptQueryObject(dwObjectType,
                                                      pbData,
                                                      dwExpectedContentTypeFlags,
                                                      dwExpectedFormatTypeFlags,
                                                      dwFlags,
                                                      pdwMsgAndCertEncodingType,
                                                      pdwContentType,
                                                      pdwFormatType,
                                                      phCertStore,
                                                      phMsg,
                                                      ppvContext);
            }
            finally {
                if (handle.IsAllocated)
                   handle.Free();
            }

            return result;
        }

        //
        // This override demands FileIOPermission.Read if dwObjectType is CERT_QUERY_OBJECT_FILE.
        //

        [ResourceExposure(ResourceScope.Machine)]  // If you passed in a file name
        [ResourceConsumption(ResourceScope.Machine)]
        internal static unsafe
        bool CryptQueryObject (
            [In]     uint                     dwObjectType,
            [In]     object                   pvObject,
            [In]     uint                     dwExpectedContentTypeFlags,
            [In]     uint                     dwExpectedFormatTypeFlags,
            [In]     uint                     dwFlags,
            [Out]    IntPtr                   pdwMsgAndCertEncodingType,
            [Out]    IntPtr                   pdwContentType,
            [Out]    IntPtr                   pdwFormatType,
            [In,Out] ref SafeCertStoreHandle  phCertStore,
            [In,Out] IntPtr                   phMsg,
            [In,Out] IntPtr                   ppvContext) {

            bool result = false;
            GCHandle handle = GCHandle.Alloc(pvObject, GCHandleType.Pinned);
            IntPtr pbData = handle.AddrOfPinnedObject();
            CRYPTOAPI_BLOB certBlob;

            try {
                if (pvObject == null)
                    throw new ArgumentNullException("pvObject");

                if (dwObjectType == CERT_QUERY_OBJECT_FILE) {
                    string fullPath = Path.GetFullPath((string) pvObject);
                    new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                } else {
                    certBlob.cbData = (uint) ((byte[]) pvObject).Length;
                    certBlob.pbData = pbData;
                    pbData = new IntPtr(&certBlob);
                }

                result = CAPIMethods.CryptQueryObject(dwObjectType,
                                                      pbData,
                                                      dwExpectedContentTypeFlags,
                                                      dwExpectedFormatTypeFlags,
                                                      dwFlags,
                                                      pdwMsgAndCertEncodingType,
                                                      pdwContentType,
                                                      pdwFormatType,
                                                      ref phCertStore,
                                                      phMsg,
                                                      ppvContext);
            }
            finally {
                if (handle.IsAllocated)
                   handle.Free();
            }

            return result;
        }

        //
        // This override demands FileIOPermission.Read if dwObjectType is CERT_QUERY_OBJECT_FILE.
        //

        [ResourceExposure(ResourceScope.Machine)]  // If you pass in a file name
        [ResourceConsumption(ResourceScope.Machine)]
        internal static unsafe
        SafeCertStoreHandle PFXImportCertStore(
            [In]     uint      dwObjectType,
            [In]     object    pvObject,
            [In]     string    szPassword,
            [In]     uint      dwFlags,
            [In]     bool      persistKeyContainers) {

            if (pvObject == null)
                throw new ArgumentNullException("pvObject");

            byte[] pbData = null;
            if (dwObjectType == CERT_QUERY_OBJECT_FILE) {
                pbData = File.ReadAllBytes((string)pvObject);
            } else {
                pbData = (byte[]) pvObject;
            }

            if (persistKeyContainers) {
                //
                // Right now, we always demand KeyContainerPermission regardless of whether the PFX contains a private key or not.
                // We could avoid that by looping through the certs in the store and find out whether there are actually private keys.
                //
                if (!CompatibilitySwitches.IsAppEarlierThanWindowsPhone8) {
                    KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.Create);
                    kp.Demand();
                }
            }

            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;
            GCHandle handle = GCHandle.Alloc(pbData, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            try {
                CRYPTOAPI_BLOB certBlob;
                certBlob.cbData = (uint) pbData.Length;
                certBlob.pbData = ptr;

                safeCertStoreHandle = CAPIMethods.PFXImportCertStore(new IntPtr(&certBlob),
                                                                     szPassword,
                                                                     dwFlags);
            }

            finally {
                if (handle.IsAllocated)
                   handle.Free();
            }

            if (!safeCertStoreHandle.IsInvalid) {
                //
                // If the user did not want us to persist private keys, then we should loop through all
                // the certificates in the collection and set our custom CERT_DELETE_KEYSET_PROP_ID property
                // so the key container will be deleted when the cert contexts will go away.
                //

                if (persistKeyContainers == false) {
                    IntPtr pEnumContext = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, IntPtr.Zero);
                    while (pEnumContext != IntPtr.Zero) {
                        CAPI.CRYPTOAPI_BLOB blob = new CAPI.CRYPTOAPI_BLOB();
                        if (!CAPI.CertSetCertificateContextProperty(pEnumContext,
                                                                    CERT_DELETE_KEYSET_PROP_ID,
                                                                    CERT_SET_PROPERTY_INHIBIT_PERSIST_FLAG, // we don't want this property to be persisted.
                                                                    new IntPtr(&blob)))
                            throw new CryptographicException(Marshal.GetLastWin32Error());
                        pEnumContext = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, pEnumContext);
                    }
                }
            }

            return safeCertStoreHandle;
        }

        //
        // This override demands StorePermission.AddToStore.
        //

        new internal static 
        bool CertAddCertificateContextToStore (
            [In]     SafeCertStoreHandle        hCertStore,
            [In]     SafeCertContextHandle      pCertContext,
            [In]     uint                       dwAddDisposition,
            [In,Out] SafeCertContextHandle      ppStoreContext) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            if (pCertContext == null)
                throw new ArgumentNullException("pCertContext");
            if (pCertContext.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "pCertContext");

            StorePermission sp = new StorePermission(StorePermissionFlags.AddToStore);
            sp.Demand();

            return CAPIMethods.CertAddCertificateContextToStore(hCertStore, 
                                                                pCertContext, 
                                                                dwAddDisposition, 
                                                                ppStoreContext);
        }

        //
        // This override demands StorePermission.AddToStore.
        //

        new internal static 
        bool CertAddCertificateLinkToStore (
            [In]     SafeCertStoreHandle        hCertStore,
            [In]     SafeCertContextHandle      pCertContext,
            [In]     uint                       dwAddDisposition,
            [In,Out] SafeCertContextHandle      ppStoreContext) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            if (pCertContext == null)
                throw new ArgumentNullException("pCertContext");
            if (pCertContext.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "pCertContext");

            StorePermission sp = new StorePermission(StorePermissionFlags.AddToStore);
            sp.Demand();

            return CAPIMethods.CertAddCertificateLinkToStore(hCertStore, 
                                                             pCertContext, 
                                                             dwAddDisposition, 
                                                             ppStoreContext);
        }

        //
        // This override demands StorePermission.RemoveFromStore.
        //

        new internal static 
        bool CertDeleteCertificateFromStore (
            [In]     SafeCertContextHandle  pCertContext) {

            if (pCertContext == null)
                throw new ArgumentNullException("pCertContext");
            if (pCertContext.IsInvalid) 
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "pCertContext");

            StorePermission sp = new StorePermission(StorePermissionFlags.RemoveFromStore);
            sp.Demand();

            return CAPIMethods.CertDeleteCertificateFromStore(pCertContext); 
        }

        //
        // This override demands StorePermission.OpenStore.
        //

        new internal static 
        SafeCertStoreHandle CertOpenStore(
            [In]    IntPtr  lpszStoreProvider,
            [In]    uint    dwMsgAndCertEncodingType,
            [In]    IntPtr  hCryptProv,
            [In]    uint    dwFlags,
            [In]    string  pvPara) {

            if (lpszStoreProvider != new IntPtr(CERT_STORE_PROV_MEMORY) && lpszStoreProvider != new IntPtr(CERT_STORE_PROV_SYSTEM))
                throw new ArgumentException(SR.GetString(SR.Security_InvalidValue), "lpszStoreProvider");

            if ((dwFlags & CERT_SYSTEM_STORE_LOCAL_MACHINE) == CERT_SYSTEM_STORE_LOCAL_MACHINE ||
                (dwFlags & CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY) == CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY ||
                (dwFlags & CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE) == CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE) {
                // We do not allow opening remote local machine stores if in semi-trusted environment.
                if (pvPara != null && pvPara.StartsWith(@"\\", StringComparison.Ordinal))
                    new PermissionSet(PermissionState.Unrestricted).Demand();
            }

            if ((dwFlags & CERT_STORE_DELETE_FLAG) == CERT_STORE_DELETE_FLAG) {
                StorePermission sp = new StorePermission(StorePermissionFlags.DeleteStore);
                sp.Demand();
            } else {
                StorePermission sp = new StorePermission(StorePermissionFlags.OpenStore);
                sp.Demand();
            }

            if ((dwFlags & CERT_STORE_CREATE_NEW_FLAG) == CERT_STORE_CREATE_NEW_FLAG) {
                StorePermission sp = new StorePermission(StorePermissionFlags.CreateStore);
                sp.Demand();
            }

            if ((dwFlags & CERT_STORE_OPEN_EXISTING_FLAG) == 0) {
                StorePermission sp = new StorePermission(StorePermissionFlags.CreateStore);
                sp.Demand();
            }

            return CAPIMethods.CertOpenStore(lpszStoreProvider, 
                                             dwMsgAndCertEncodingType, 
                                             hCryptProv, 
                                             dwFlags | CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 
                                             pvPara);
        }

        new internal static
        SafeCertContextHandle CertFindCertificateInStore(
            [In]    SafeCertStoreHandle     hCertStore,
            [In]    uint                    dwCertEncodingType,
            [In]    uint                    dwFindFlags,
            [In]    uint                    dwFindType,
            [In]    IntPtr                  pvFindPara,
            [In]    SafeCertContextHandle   pPrevCertContext) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            // Note we do not demand EnumerateCertificates flag of StorePermission since we only call this method
            // for a memory store and we don't want to incur the cost of a demand. Ideally, we should be doing that.

            return CAPIMethods.CertFindCertificateInStore(hCertStore, 
                                                          dwCertEncodingType,
                                                          dwFindFlags,
                                                          dwFindType,
                                                          pvFindPara,
                                                          pPrevCertContext);
        }

        new internal static
        bool PFXExportCertStore(
            [In]     SafeCertStoreHandle    hCertStore,
            [In,Out] IntPtr                 pPFX,
            [In]     string                 szPassword,
            [In]     uint                   dwFlags) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            //
            // Right now, we always demand KeyContainerPermission regardless of whether the PFX contains a private key or not.
            // We could avoid that by looping through the certs in the store and find out whether there are actually private keys.
            //
            if (!CompatibilitySwitches.IsAppEarlierThanWindowsPhone8) {
                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.Open | KeyContainerPermissionFlags.Export);
                kp.Demand();
            }

            return CAPIMethods.PFXExportCertStore(hCertStore,
                                                  pPFX,
                                                  szPassword,
                                                  dwFlags);
        }

        new internal static
        bool CertSaveStore(
            [In]     SafeCertStoreHandle    hCertStore,
            [In]     uint                   dwMsgAndCertEncodingType,
            [In]     uint                   dwSaveAs,
            [In]     uint                   dwSaveTo,
            [In,Out] IntPtr                 pvSaveToPara,
            [In]     uint                   dwFlags) {

            if (hCertStore == null)
                throw new ArgumentNullException("hCertStore");
            if (hCertStore.IsInvalid)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidHandle), "hCertStore");

            StorePermission sp = new StorePermission(StorePermissionFlags.EnumerateCertificates);
            sp.Demand();

            if (dwSaveTo == CERT_STORE_SAVE_TO_FILENAME_A || dwSaveTo == CERT_STORE_SAVE_TO_FILENAME_W)
                throw new ArgumentException(SR.GetString(SR.Security_InvalidValue), "pvSaveToPara");

            return CAPIMethods.CertSaveStore(hCertStore,
                                             dwMsgAndCertEncodingType,
                                             dwSaveAs,
                                             dwSaveTo,
                                             pvSaveToPara,
                                             dwFlags);
        }
    }

    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLibraryHandle() : base(true) { }

        [DllImport(CAPI.KERNEL32, SetLastError = true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary([In] IntPtr hModule);

        protected override bool ReleaseHandle()
        {
            return FreeLibrary(handle);
        }
    }

    internal sealed class SafeLocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeLocalAllocHandle () : base(true) {}

        // 0 is an Invalid Handle
        internal SafeLocalAllocHandle (IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeLocalAllocHandle InvalidHandle {
            get { return new SafeLocalAllocHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.KERNEL32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr LocalFree(IntPtr handle);

        override protected bool ReleaseHandle()
        {
            return LocalFree(handle) == IntPtr.Zero;
        }
    }

    internal sealed class SafeCryptProvHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeCryptProvHandle() : base (true) {}

        // 0 is an Invalid Handle
        internal SafeCryptProvHandle(IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeCryptProvHandle InvalidHandle {
            get { return new SafeCryptProvHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.ADVAPI32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CryptReleaseContext(IntPtr hCryptProv, uint dwFlags); 

        override protected bool ReleaseHandle()
        {
            return CryptReleaseContext(handle, 0);
        }
    }

    internal sealed class SafeCertContextHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeCertContextHandle() : base (true) {}

        // 0 is an Invalid Handle
        internal SafeCertContextHandle(IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeCertContextHandle InvalidHandle {
            get { return new SafeCertContextHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.CRYPT32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CertFreeCertificateContext (IntPtr pCertContext);

        override protected bool ReleaseHandle()
        {
            return CertFreeCertificateContext(handle);
        }
    }

    internal sealed class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeCertStoreHandle() : base (true) {}

        // 0 is an Invalid Handle
        internal SafeCertStoreHandle(IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeCertStoreHandle InvalidHandle {
            get { return new SafeCertStoreHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.CRYPT32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CertCloseStore (IntPtr hCertStore, uint dwFlags);

        override protected bool ReleaseHandle()
        {
            return CertCloseStore(handle, 0);
        }
    }

    internal sealed class SafeCryptMsgHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeCryptMsgHandle () : base(true) {}

        internal SafeCryptMsgHandle (IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeCryptMsgHandle InvalidHandle {
            get { return new SafeCryptMsgHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.CRYPT32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CryptMsgClose(IntPtr handle);

        override protected bool ReleaseHandle()
        {
            return CryptMsgClose(handle);
        }
    }

    internal sealed class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeCertChainHandle () : base(true) {}

        internal SafeCertChainHandle (IntPtr handle) : base (true) {
            SetHandle(handle);
        }

        internal static SafeCertChainHandle InvalidHandle {
            get { return new SafeCertChainHandle(IntPtr.Zero); }
        }

        [DllImport(CAPI.CRYPT32, SetLastError=true),
         SuppressUnmanagedCodeSecurity,
         ResourceExposure(ResourceScope.None),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern void CertFreeCertificateChain(IntPtr handle);

        override protected bool ReleaseHandle()
        {
            CertFreeCertificateChain(handle);
            return true;
        }
    }
}
