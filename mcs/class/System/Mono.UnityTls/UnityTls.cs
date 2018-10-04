using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Unity
{
    // Unitytls uses UInt8 to denote raw buffers and Int8/char for strings.
    // Since we need to funnel all in- and outgoing strings through Encoding.UTF8 it is easier to let the Int8 alias point to Byte instead of SByte.
    // The aliases here are just there to keep the semantic in the interface and make it more similar to the c original.
    using UInt8 = Byte;
    using Int8 = Byte;
    using size_t = IntPtr;

    unsafe internal static partial class UnityTls
    {
        // ------------------------------------
        // Error Handling
        // ------------------------------------
        public enum unitytls_error_code : UInt32
        {
            UNITYTLS_SUCCESS = 0,
            UNITYTLS_INVALID_ARGUMENT,      // One of the arguments has an invalid value (e.g. null where not allowed)
            UNITYTLS_INVALID_FORMAT,        // The passed data does not have a valid format.
            UNITYTLS_INVALID_PASSWORD,      // Invalid password
            UNITYTLS_INVALID_STATE,         // The object operating being operated on is not in a state that allows this function call.
            UNITYTLS_BUFFER_OVERFLOW,       // A passed buffer was not large enough.
            UNITYTLS_OUT_OF_MEMORY,         // Out of memory error
            UNITYTLS_INTERNAL_ERROR,        // Internal implementation error.
            UNITYTLS_NOT_SUPPORTED,         // The requested action is not supported on the current platform/implementation.
            UNITYTLS_ENTROPY_SOURCE_FAILED, // Failed to generate requested amount of entropy data.
            UNITYTLS_STREAM_CLOSED,         // The operation is not possible because the stream between the peers was closed.

            UNITYTLS_USER_CUSTOM_ERROR_START    = 0x100000,
            UNITYTLS_USER_WOULD_BLOCK,      // Can be set by the user to signal that a call (e.g. read/write callback) would block and needs to be called again.
                                            // Some implementations may set this if not all bytes have been read/written.
            UNITYTLS_USER_READ_FAILED,      // Can be set by the user to indicate a failed read operation.
            UNITYTLS_USER_WRITE_FAILED,     // Can be set by the user to indicate a failed write operation.
            UNITYTLS_USER_UNKNOWN_ERROR,    // Can be set by the user to indicate a generic error.
            UNITYTLS_USER_CUSTOM_ERROR_END      = 0x200000,
        }

        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_errorstate
        {
            private UInt32              magic;
            public  unitytls_error_code code;
            private UInt64              reserved;   // Implementation specific error code/handle.
        }

        // ------------------------------------
        // Private Key
        // ------------------------------------

        public struct unitytls_key {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_key_ref { public UInt64 handle; }

        // ------------------------------------
        // X.509 Certificate
        // -----------------------------------
        public struct unitytls_x509 {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_x509_ref { public UInt64 handle; }

        // ------------------------------------
        // X.509 Certificate List
        // ------------------------------------
        public struct unitytls_x509list {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_x509list_ref { public UInt64 handle; }

        // ------------------------------------
        // X.509 Certificate Verification
        // ------------------------------------
        [Flags]
        public enum unitytls_x509verify_result : UInt32
        {
            UNITYTLS_X509VERIFY_SUCCESS            = 0x00000000,
            UNITYTLS_X509VERIFY_NOT_DONE           = 0x80000000,
            UNITYTLS_X509VERIFY_FATAL_ERROR        = 0xFFFFFFFF,

            UNITYTLS_X509VERIFY_FLAG_EXPIRED       = 0x00000001,
            UNITYTLS_X509VERIFY_FLAG_REVOKED       = 0x00000002,
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate unitytls_x509verify_result unitytls_x509verify_callback(void* userData, unitytls_x509_ref cert, unitytls_x509verify_result result, unitytls_errorstate* errorState);

        // ------------------------------------
        // TLS Context
        // ------------------------------------
        public struct unitytls_tlsctx {}
        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_tlsctx_ref { public UInt64 handle; }

        public struct unitytls_x509name {}

        public enum unitytls_ciphersuite : UInt32
        {
            // With exception of the INVALID value, this enum represents an IANA cipher ID.
            UNITYTLS_CIPHERSUITE_INVALID = 0xFFFFFF
        }

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
            public unitytls_protocol min;
            public unitytls_protocol max;
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate size_t unitytls_tlsctx_write_callback(void* userData, UInt8* data, size_t bufferLen, unitytls_errorstate* errorState);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate size_t unitytls_tlsctx_read_callback(void* userData, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void   unitytls_tlsctx_trace_callback(void* userData, unitytls_tlsctx* ctx, Int8* traceMessage, size_t traceMessageLen);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void   unitytls_tlsctx_certificate_callback(void* userData, unitytls_tlsctx* ctx, Int8* cn, size_t cnLen, unitytls_x509name* caList, size_t caListLen, unitytls_x509list_ref* chain, unitytls_key_ref* key, unitytls_errorstate* errorState);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate unitytls_x509verify_result unitytls_tlsctx_x509verify_callback(void* userData, unitytls_x509list_ref chain, unitytls_errorstate* errorState);

        [StructLayout (LayoutKind.Sequential)]
        public struct unitytls_tlsctx_callbacks
        {
            public unitytls_tlsctx_read_callback   read;
            public unitytls_tlsctx_write_callback  write;
            public void*                           data;
        };



        // ------------------------------------------------------------------------
        // unitytls interface defintion
        // ------------------------------------------------------------------------

        // This native struct is used to provide all necessary fields and function calls from unitytls to the mono-unitytls-binding.
        // Native implementation lives in unity:Modules/TLS/InterfaceStruct.cpp and needs to be adapted to every change.
        [StructLayout (LayoutKind.Sequential)]
        public class unitytls_interface_struct
        {
            public readonly UInt64 UNITYTLS_INVALID_HANDLE;
            public readonly unitytls_tlsctx_protocolrange UNITYTLS_TLSCTX_PROTOCOLRANGE_DEFAULT;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_errorstate                           unitytls_errorstate_create_t();
            public unitytls_errorstate_create_t                           unitytls_errorstate_create;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_errorstate_raise_error_t(unitytls_errorstate* errorState, unitytls_error_code errorCode);
            public unitytls_errorstate_raise_error_t                      unitytls_errorstate_raise_error;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_key_ref                              unitytls_key_get_ref_t(unitytls_key* key, unitytls_errorstate* errorState);
            public unitytls_key_get_ref_t                                 unitytls_key_get_ref;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_key*                                 unitytls_key_parse_der_t(UInt8* buffer, size_t bufferLen, Int8* password, size_t passwordLen, unitytls_errorstate* errorState);
            public unitytls_key_parse_der_t                               unitytls_key_parse_der;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_key*                                 unitytls_key_parse_pem_t(Int8* buffer, size_t bufferLen, Int8* password, size_t passwordLen, unitytls_errorstate* errorState);
            public unitytls_key_parse_pem_t                               unitytls_key_parse_pem;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_key_free_t(unitytls_key* key);
            public unitytls_key_free_t                                    unitytls_key_free;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate size_t                                        unitytls_x509_export_der_t(unitytls_x509_ref cert, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
            public unitytls_x509_export_der_t                             unitytls_x509_export_der;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509list_ref                         unitytls_x509list_get_ref_t(unitytls_x509list* list, unitytls_errorstate* errorState);
            public unitytls_x509list_get_ref_t                            unitytls_x509list_get_ref;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509_ref                             unitytls_x509list_get_x509_t(unitytls_x509list_ref list, size_t index, unitytls_errorstate* errorState);
            public unitytls_x509list_get_x509_t                           unitytls_x509list_get_x509;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509list*                            unitytls_x509list_create_t(unitytls_errorstate* errorState);
            public unitytls_x509list_create_t                             unitytls_x509list_create;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_x509list_append_t(unitytls_x509list* list, unitytls_x509_ref cert, unitytls_errorstate* errorState);
            public unitytls_x509list_append_t                             unitytls_x509list_append;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_x509list_append_der_t(unitytls_x509list* list, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
            public unitytls_x509list_append_der_t                         unitytls_x509list_append_der;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_x509list_append_pem_t(unitytls_x509list* list, Int8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
            public unitytls_x509list_append_der_t                         unitytls_x509list_append_pem;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_x509list_free_t(unitytls_x509list* list);
            public unitytls_x509list_free_t                               unitytls_x509list_free;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509verify_result                    unitytls_x509verify_default_ca_t(unitytls_x509list_ref chain, Int8* cn, size_t cnLen, unitytls_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);
            public unitytls_x509verify_default_ca_t                       unitytls_x509verify_default_ca;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509verify_result                    unitytls_x509verify_explicit_ca_t(unitytls_x509list_ref chain, unitytls_x509list_ref trustCA, Int8* cn, size_t cnLen, unitytls_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);
            public unitytls_x509verify_explicit_ca_t                      unitytls_x509verify_explicit_ca;

            // Note that we take UInt64 here instead of handles!
            // This a workaround for an android specific crash, caused by a bug in Mono's implementation of native calls through the arm ABI.
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_tlsctx*                              unitytls_tlsctx_create_server_t(unitytls_tlsctx_protocolrange supportedProtocols, unitytls_tlsctx_callbacks callbacks, UInt64 certChain, UInt64 leafCertificateKey, unitytls_errorstate* errorState);
            public unitytls_tlsctx_create_server_t                        unitytls_tlsctx_create_server;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_tlsctx*                              unitytls_tlsctx_create_client_t(unitytls_tlsctx_protocolrange supportedProtocols, unitytls_tlsctx_callbacks callbacks, Int8* cn, size_t cnLen, unitytls_errorstate* errorState);
            public unitytls_tlsctx_create_client_t                        unitytls_tlsctx_create_client;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_server_require_client_authentication_t(unitytls_tlsctx* ctx, unitytls_x509list_ref clientAuthCAList, unitytls_errorstate* errorState);
            public unitytls_tlsctx_server_require_client_authentication_t unitytls_tlsctx_server_require_client_authentication;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_set_certificate_callback_t(unitytls_tlsctx* ctx, unitytls_tlsctx_certificate_callback cb, void* userData, unitytls_errorstate* errorState);
            public unitytls_tlsctx_set_certificate_callback_t             unitytls_tlsctx_set_certificate_callback;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_set_trace_callback_t(unitytls_tlsctx* ctx, unitytls_tlsctx_trace_callback cb, void* userData, unitytls_errorstate* errorState);
            public unitytls_tlsctx_set_trace_callback_t                   unitytls_tlsctx_set_trace_callback;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_set_x509verify_callback_t(unitytls_tlsctx* ctx, unitytls_tlsctx_x509verify_callback cb, void* userData, unitytls_errorstate* errorState);
            public unitytls_tlsctx_set_x509verify_callback_t              unitytls_tlsctx_set_x509verify_callback;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_set_supported_ciphersuites_t(unitytls_tlsctx* ctx, unitytls_ciphersuite* supportedCiphersuites, size_t supportedCiphersuitesLen, unitytls_errorstate* errorState);
            public unitytls_tlsctx_set_supported_ciphersuites_t           unitytls_tlsctx_set_supported_ciphersuites;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_ciphersuite                          unitytls_tlsctx_get_ciphersuite_t(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
            public unitytls_tlsctx_get_ciphersuite_t                      unitytls_tlsctx_get_ciphersuite;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_protocol                             unitytls_tlsctx_get_protocol_t(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
            public unitytls_tlsctx_get_protocol_t                         unitytls_tlsctx_get_protocol;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate unitytls_x509verify_result                    unitytls_tlsctx_process_handshake_t(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
            public unitytls_tlsctx_process_handshake_t                    unitytls_tlsctx_process_handshake;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate size_t                                        unitytls_tlsctx_read_t(unitytls_tlsctx* ctx, UInt8* buffer, size_t bufferLen, unitytls_errorstate* errorState);
            public unitytls_tlsctx_read_t                                 unitytls_tlsctx_read;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate size_t                                        unitytls_tlsctx_write_t(unitytls_tlsctx* ctx, UInt8* data, size_t bufferLen, unitytls_errorstate* errorState);
            public unitytls_tlsctx_write_t                                unitytls_tlsctx_write;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_notify_close_t(unitytls_tlsctx* ctx, unitytls_errorstate* errorState);
            public unitytls_tlsctx_notify_close_t                         unitytls_tlsctx_notify_close;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_tlsctx_free_t(unitytls_tlsctx* ctx);
            public unitytls_tlsctx_free_t                                 unitytls_tlsctx_free;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void                                          unitytls_random_generate_bytes_t(UInt8 * buffer, size_t bufferLen, unitytls_errorstate * errorState);
            public unitytls_random_generate_bytes_t                       unitytls_random_generate_bytes;
        }

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        private static extern IntPtr GetUnityTlsInterface ();
        
        private static unitytls_interface_struct marshalledInterface = null;

        public static bool IsSupported => NativeInterface != null;

        public static unitytls_interface_struct NativeInterface
        {
            get
            {
                if (marshalledInterface == null) {
                    IntPtr rawInterface = GetUnityTlsInterface ();
                    if (rawInterface == IntPtr.Zero)
                        return null;
                    marshalledInterface = Marshal.PtrToStructure<unitytls_interface_struct> (rawInterface);
                }
                return marshalledInterface;
            }
        }
    }
}