using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.UnityTLS
{
    using size_t = UIntPtr;
    using UInt8 = Byte;

    unsafe internal static partial class UnityTLS
    {
        private const string DLLNAME = "MacStandalonePlayer_TLSModule_Dynamic.dylib";
        private const CallingConvention CALLCONV = CallingConvention.Cdecl;

        // TODO
        //extern const UInt64 UNITYTLS_INVALID_HANDLE;

        // ------------------------------------
        // Error Handling
        // ------------------------------------
        public enum unitytls_error_code : UInt32
        {
            UNITYTLS_SUCCESS = 0,
            UNITYTLS_INVALID_ARGUMENT,   // One of the arguments has an invalid value (e.g. null where not allowed)
            UNITYTLS_INVALID_FORMAT,     // The passed data does not have a valid format.
            UNITYTLS_INVALID_STATE,      // The object operating being operated on is not in a state that allows this function call.
            UNITYTLS_BUFFER_OVERFLOW,    // A passed buffer was not large enough.
            UNITYTLS_OUT_OF_MEMORY,      // Out of memory error
            UNITYTLS_INTERNAL_ERROR,     // public implementation error.
            UNITYTLS_NOT_SUPPORTED,      // The requested action is not supported on the current platform/implementation.
            UNITYTLS_ENTROPY_SOURCE_FAILED, // Failed to generate requested amount of entropy data.

            UNITYTLS_USER_WOULD_BLOCK,   // Can be set by the user to signal that a call (e.g. read/write callback) would block and needs to be called again.
                                         // Some implementations may set this if not all bytes have been read/written.
            UNITYTLS_USER_STREAM_CLOSED, // Can be set by the user to cancel a read/write operation.
            UNITYTLS_USER_READ_FAILED,   // Can be set by the user to indicate a failed read operation.
            UNITYTLS_USER_WRITE_FAILED,  // Can be set by the user to indicate a failed write operation.
            UNITYTLS_USER_UNKNOWN_ERROR, // Can be set by the user to indicate a generic error.
        }

        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_errorstate
        {
            UInt32              magic;
            unitytls_error_code code;
            UInt64              reserved;   // Implementation specific error code/handle.
        }

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_errorstate           unitytls_errorstate_create();
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                          unitytls_errorstate_raise_error(unitytls_errorstate* errorState, unitytls_error_code errorCode);


        public struct unitytls_pubkey {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_pubkey_ref { UInt64 handle; }

        // ------------------------------------
        // public Key
        // ------------------------------------
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_pubkey_ref            unitytls_pubkey_get_ref(unitytls_pubkey* key, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_pubkey*               unitytls_pubkey_parse_der(UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_pubkey*               unitytls_pubkey_parse_pem(char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_pubkey_export_der(unitytls_pubkey_ref key, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_pubkey_export_pem(unitytls_pubkey_ref key, char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_pubkey_free(unitytls_pubkey* key);

        // ------------------------------------
        // Private Key
        // ------------------------------------
        public enum unitytls_key_type : UInt32
        {
            UNITYTLS_KEY_TYPE_INVALID,
            UNITYTLS_KEY_TYPE_RSA,
            // UNITYTLS_KEY_TYPE_EC, // Not supported yet.
        }

        public struct unitytls_key {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_key_ref { UInt64 handle; }

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_key_ref               unitytls_key_get_ref(unitytls_key* key, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_key*                  unitytls_key_parse_der(UInt8* buffer, size_t bufferLen, UInt8* password, size_t passwordLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_key*                  unitytls_key_parse_pem(char* buffer, size_t bufferLen, UInt8* password, size_t passwordLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_key_export_der(unitytls_key_ref key, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_key_export_pem(unitytls_key_ref key, char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_pubkey_ref            unitytls_key_get_pubkey(unitytls_key_ref key, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_key_type              unitytls_key_get_type(unitytls_key_ref key, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_key_free(unitytls_key* key);

        // ------------------------------------
        // X.509 Certificate
        // -----------------------------------
        public struct unitytls_x509 {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_x509_ref { UInt64 handle; }

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509_ref              unitytls_x509_get_ref(unitytls_x509* cert, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509*                 unitytls_x509_parse_der(UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509*                 unitytls_x509_parse_pem(char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_x509_export_der(unitytls_x509_ref cert, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_x509_export_pem(unitytls_x509_ref cert, char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_pubkey_ref            unitytls_x509_get_pubkey(unitytls_x509_ref cert, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_x509_free(unitytls_x509* cert);

        // ------------------------------------
        // X.509 Certificate List
        // ------------------------------------
        public struct unitytls_x509list {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_x509list_ref { UInt64 handle; }

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509list_ref    unitytls_x509list_get_ref(unitytls_x509list* list, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509list*             unitytls_x509list_parse_pem(char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_x509list_export_pem(unitytls_x509list_ref list, char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_x509list_get_size(unitytls_x509list_ref list, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509_ref              unitytls_x509list_get_x509(unitytls_x509list_ref list, size_t index, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509list*             unitytls_x509list_create(unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_x509list_append(unitytls_x509list* list, unitytls_x509_ref cert, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_x509list_append_der(unitytls_x509list* list, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_x509list_append_pem(unitytls_x509list* list, char* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_x509list_free(unitytls_x509list* list);

        // ------------------------------------
        // X.509 Certificate Verification
        // ------------------------------------
        public enum unitytls_x509verify_result : UInt32
        {
            UNITYTLS_X509VERIFY_SUCCESS            = 0x00000000,
            UNITYTLS_X509VERIFY_NOT_DONE           = 0x80000000,
            UNITYTLS_X509VERIFY_FATAL_ERROR        = 0xFFFFFFFF,

            UNITYTLS_X509VERIFY_FLAG_EXPIRED       = 0x00000001,
            UNITYTLS_X509VERIFY_FLAG_REVOKED       = 0x00000002, // requires CRL backend
            UNITYTLS_X509VERIFY_FLAG_CN_MISMATCH   = 0x00000004,
            UNITYTLS_X509VERIFY_FLAG_NOT_TRUSTED   = 0x00000008,

            UNITYTLS_X509VERIFY_FLAG_USER_ERROR1   = 0x00010000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR2   = 0x00020000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR3   = 0x00040000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR4   = 0x00080000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR5   = 0x00100000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR6   = 0x00200000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR7   = 0x00400000,
            UNITYTLS_X509VERIFY_FLAG_USER_ERROR8   = 0x00800000,

            UNITYTLS_X509VERIFY_FLAG_UNKNOWN_ERROR = 0x08000000,
        }

        public delegate unitytls_x509verify_result unitytls_x509verify_callback(void* userData, unitytls_x509_ref cert, unitytls_x509verify_result result, unitytls_errorstate* errorState);

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509verify_result     unitytls_x509verify_default_ca(unitytls_x509list_ref chain, char* cn, size_t cnLen, unitytls_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509verify_result     unitytls_x509verify_explicit_ca(unitytls_x509list_ref chain, unitytls_x509list_ref trustCA, char* cn, size_t cnLen, unitytls_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);


        // ------------------------------------
        // TLS Context
        // ------------------------------------
        public struct unitytls_tlsctx {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_tlsctx_ref { UInt64 handle; }

        public enum unitytls_protocol : UInt32
        {
            UNITYTLS_PROTOCOL_TLS_1_0,
            UNITYTLS_PROTOCOL_TLS_1_1,
            UNITYTLS_PROTOCOL_TLS_1_2,

            UNITYTLS_PROTOCOL_INVALID,
        }
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_tlsctx_protocolrange
        {
            unitytls_protocol min;
            unitytls_protocol max;
        };
        // TODO
        //[DllImport (DLLNAME, CallingConvention=CALLCONV)]
        //extern public static unitytls_tlsctx_protocolrange UNITYTLS_TLSCTX_PROTOCOLRANGE_DEFAULT;

        public enum unitytls_tlsctx_handshakestate : UInt32
        {
            UNITYTLS_HANDSHAKESTATE_BEGIN,                   // Called right before a handshake is performed.
            UNITYTLS_HANDSHAKESTATE_DONE,                    // Called after a handshake was successfully performed.

            UNITYTLS_HANDSHAKESTATE_PEER_X509_CERT_VERIFY,   // Server certificate needs to be verified. Set error state if verification failed.
            UNITYTLS_HANDSHAKESTATE_PEER_X509_CERT_REQUEST,  // A certificate is requested.
        }

        public delegate size_t unitytls_tlsctx_callback_write(void* userData, UInt8* data, size_t bufferLen, unitytls_errorstate* errorState);
        public delegate size_t unitytls_tlsctx_callback_read(void* userData, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        public delegate void   unitytls_tlsctx_callback_handshake(void* userData, unitytls_tlsctx* ctx, unitytls_tlsctx_handshakestate currentState, unitytls_errorstate* errorState);
        public delegate unitytls_x509verify_result unitytls_tlsctx_x509verify_callback(void* userData, unitytls_x509list_ref chain, unitytls_errorstate* errorState);

        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_tlsctx_callbacks
        {
            unitytls_tlsctx_callback_read   read;
            unitytls_tlsctx_callback_write  write;
            void*                           data;
        };

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_tlsctx*               unitytls_tlsctx_create_server(unitytls_tlsctx_protocolrange supportedProtocols, unitytls_tlsctx_callbacks callbacks, unitytls_x509list_ref certChain, unitytls_key_ref leafCertificateKey, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_tlsctx*               unitytls_tlsctx_create_client(unitytls_tlsctx_protocolrange supportedProtocols, unitytls_tlsctx_callbacks callbacks, char* cn, size_t cnLen, unitytls_errorstate* errorState);

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_tlsctx_set_x509verify_callback(unitytls_tlsctx* ctx, unitytls_tlsctx_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_tlsctx_set_handshake_callback(unitytls_tlsctx* ctx, unitytls_tlsctx_callback_handshake cb, void* userData, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_tlsctx_set_supported_ciphersuites(unitytls_tlsctx* ctx, unitytls_ciphersuite* supportedCiphersuites, size_t supportedCiphersuitesLen, unitytls_errorstate* errorState);

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_ciphersuite           unitytls_tlsctx_get_ciphersuite(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_protocol              unitytls_tlsctx_get_protocol(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509list_ref          unitytls_tlsctx_get_peer_x509list(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509verify_result     unitytls_tlsctx_get_verify_result(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);

        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static unitytls_x509verify_result     unitytls_tlsctx_process_handshake(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_tlsctx_read(unitytls_tlsctx* ctx, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_tlsctx_write(unitytls_tlsctx* ctx, UInt8* data, size_t bufferLen, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static void                           unitytls_tlsctx_free(unitytls_tlsctx* ctx);


        // ------------------------------------
        // Encoding / Decoding
        // ------------------------------------
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_base64_encode(char* outputBuffer, size_t outputBufferLen, UInt8* inputBuffer, size_t inputBufferLen, size_t lineMaxLength, unitytls_errorstate* errorState);
        [DllImport (DLLNAME, CallingConvention=CALLCONV)]
        extern public static size_t                         unitytls_base64_decode(UInt8* outputBuffer, size_t outputBufferLen, char* inputBuffer, size_t inputBufferLen, unitytls_errorstate* errorState);
    }
}