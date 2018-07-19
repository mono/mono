//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal class WbemNative
    {
        internal enum WbemStatus
        {
            WBEM_NO_ERROR = unchecked((int)0x00000000),
            WBEM_S_NO_ERROR = unchecked((int)0x00000000),
            WBEM_S_SAME = unchecked((int)0x00000000),
            WBEM_S_FALSE = unchecked((int)0x00000001),
            WBEM_S_ALREADY_EXISTS = unchecked((int)0x00040001),
            WBEM_S_RESET_TO_DEFAULT = unchecked((int)0x00040002),
            WBEM_S_DIFFERENT = unchecked((int)0x00040003),
            WBEM_S_TIMEDOUT = unchecked((int)0x00040004),
            WBEM_S_NO_MORE_DATA = unchecked((int)0x00040005),
            WBEM_S_OPERATION_CANCELLED = unchecked((int)0x00040006),
            WBEM_S_PENDING = unchecked((int)0x00040007),
            WBEM_S_DUPLICATE_OBJECTS = unchecked((int)0x00040008),
            WBEM_S_ACCESS_DENIED = unchecked((int)0x00040009),
            WBEM_S_PARTIAL_RESULTS = unchecked((int)0x00040010),
            WBEM_S_NO_POSTHOOK = unchecked((int)0x00040011),
            WBEM_S_POSTHOOK_WITH_BOTH = unchecked((int)0x00040012),
            WBEM_S_POSTHOOK_WITH_NEW = unchecked((int)0x00040013),
            WBEM_S_POSTHOOK_WITH_STATUS = unchecked((int)0x00040014),
            WBEM_S_POSTHOOK_WITH_OLD = unchecked((int)0x00040015),
            WBEM_S_REDO_PREHOOK_WITH_ORIGINAL_OBJECT = unchecked((int)0x00040016),
            WBEM_S_SOURCE_NOT_AVAILABLE = unchecked((int)0x00040017),
            WBEM_E_FAILED = unchecked((int)0x80041001),
            WBEM_E_NOT_FOUND = unchecked((int)0x80041002),
            WBEM_E_ACCESS_DENIED = unchecked((int)0x80041003),
            WBEM_E_PROVIDER_FAILURE = unchecked((int)0x80041004),
            WBEM_E_TYPE_MISMATCH = unchecked((int)0x80041005),
            WBEM_E_OUT_OF_MEMORY = unchecked((int)0x80041006),
            WBEM_E_INVALID_CONTEXT = unchecked((int)0x80041007),
            WBEM_E_INVALID_PARAMETER = unchecked((int)0x80041008),
            WBEM_E_NOT_AVAILABLE = unchecked((int)0x80041009),
            WBEM_E_CRITICAL_ERROR = unchecked((int)0x8004100A),
            WBEM_E_INVALID_STREAM = unchecked((int)0x8004100B),
            WBEM_E_NOT_SUPPORTED = unchecked((int)0x8004100C),
            WBEM_E_INVALID_SUPERCLASS = unchecked((int)0x8004100D),
            WBEM_E_INVALID_NAMESPACE = unchecked((int)0x8004100E),
            WBEM_E_INVALID_OBJECT = unchecked((int)0x8004100F),
            WBEM_E_INVALID_CLASS = unchecked((int)0x80041010),
            WBEM_E_PROVIDER_NOT_FOUND = unchecked((int)0x80041011),
            WBEM_E_INVALID_PROVIDER_REGISTRATION = unchecked((int)0x80041012),
            WBEM_E_PROVIDER_LOAD_FAILURE = unchecked((int)0x80041013),
            WBEM_E_INITIALIZATION_FAILURE = unchecked((int)0x80041014),
            WBEM_E_TRANSPORT_FAILURE = unchecked((int)0x80041015),
            WBEM_E_INVALID_OPERATION = unchecked((int)0x80041016),
            WBEM_E_INVALID_QUERY = unchecked((int)0x80041017),
            WBEM_E_INVALID_QUERY_TYPE = unchecked((int)0x80041018),
            WBEM_E_ALREADY_EXISTS = unchecked((int)0x80041019),
            WBEM_E_OVERRIDE_NOT_ALLOWED = unchecked((int)0x8004101A),
            WBEM_E_PROPAGATED_QUALIFIER = unchecked((int)0x8004101B),
            WBEM_E_PROPAGATED_PROPERTY = unchecked((int)0x8004101C),
            WBEM_E_UNEXPECTED = unchecked((int)0x8004101D),
            WBEM_E_ILLEGAL_OPERATION = unchecked((int)0x8004101E),
            WBEM_E_CANNOT_BE_KEY = unchecked((int)0x8004101F),
            WBEM_E_INCOMPLETE_CLASS = unchecked((int)0x80041020),
            WBEM_E_INVALID_SYNTAX = unchecked((int)0x80041021),
            WBEM_E_NONDECORATED_OBJECT = unchecked((int)0x80041022),
            WBEM_E_READ_ONLY = unchecked((int)0x80041023),
            WBEM_E_PROVIDER_NOT_CAPABLE = unchecked((int)0x80041024),
            WBEM_E_CLASS_HAS_CHILDREN = unchecked((int)0x80041025),
            WBEM_E_CLASS_HAS_INSTANCES = unchecked((int)0x80041026),
            WBEM_E_QUERY_NOT_IMPLEMENTED = unchecked((int)0x80041027),
            WBEM_E_ILLEGAL_NULL = unchecked((int)0x80041028),
            WBEM_E_INVALID_QUALIFIER_TYPE = unchecked((int)0x80041029),
            WBEM_E_INVALID_PROPERTY_TYPE = unchecked((int)0x8004102A),
            WBEM_E_VALUE_OUT_OF_RANGE = unchecked((int)0x8004102B),
            WBEM_E_CANNOT_BE_SINGLETON = unchecked((int)0x8004102C),
            WBEM_E_INVALID_CIM_TYPE = unchecked((int)0x8004102D),
            WBEM_E_INVALID_METHOD = unchecked((int)0x8004102E),
            WBEM_E_INVALID_METHOD_PARAMETERS = unchecked((int)0x8004102F),
            WBEM_E_SYSTEM_PROPERTY = unchecked((int)0x80041030),
            WBEM_E_INVALID_PROPERTY = unchecked((int)0x80041031),
            WBEM_E_CALL_CANCELLED = unchecked((int)0x80041032),
            WBEM_E_SHUTTING_DOWN = unchecked((int)0x80041033),
            WBEM_E_PROPAGATED_METHOD = unchecked((int)0x80041034),
            WBEM_E_UNSUPPORTED_PARAMETER = unchecked((int)0x80041035),
            WBEM_E_MISSING_PARAMETER_ID = unchecked((int)0x80041036),
            WBEM_E_INVALID_PARAMETER_ID = unchecked((int)0x80041037),
            WBEM_E_NONCONSECUTIVE_PARAMETER_IDS = unchecked((int)0x80041038),
            WBEM_E_PARAMETER_ID_ON_RETVAL = unchecked((int)0x80041039),
            WBEM_E_INVALID_OBJECT_PATH = unchecked((int)0x8004103A),
            WBEM_E_OUT_OF_DISK_SPACE = unchecked((int)0x8004103B),
            WBEM_E_BUFFER_TOO_SMALL = unchecked((int)0x8004103C),
            WBEM_E_UNSUPPORTED_PUT_EXTENSION = unchecked((int)0x8004103D),
            WBEM_E_UNKNOWN_OBJECT_TYPE = unchecked((int)0x8004103E),
            WBEM_E_UNKNOWN_PACKET_TYPE = unchecked((int)0x8004103F),
            WBEM_E_MARSHAL_VERSION_MISMATCH = unchecked((int)0x80041040),
            WBEM_E_MARSHAL_INVALID_SIGNATURE = unchecked((int)0x80041041),
            WBEM_E_INVALID_QUALIFIER = unchecked((int)0x80041042),
            WBEM_E_INVALID_DUPLICATE_PARAMETER = unchecked((int)0x80041043),
            WBEM_E_TOO_MUCH_DATA = unchecked((int)0x80041044),
            WBEM_E_SERVER_TOO_BUSY = unchecked((int)0x80041045),
            WBEM_E_INVALID_FLAVOR = unchecked((int)0x80041046),
            WBEM_E_CIRCULAR_REFERENCE = unchecked((int)0x80041047),
            WBEM_E_UNSUPPORTED_CLASS_UPDATE = unchecked((int)0x80041048),
            WBEM_E_CANNOT_CHANGE_KEY_INHERITANCE = unchecked((int)0x80041049),
            WBEM_E_CANNOT_CHANGE_INDEX_INHERITANCE = unchecked((int)0x80041050),
            WBEM_E_TOO_MANY_PROPERTIES = unchecked((int)0x80041051),
            WBEM_E_UPDATE_TYPE_MISMATCH = unchecked((int)0x80041052),
            WBEM_E_UPDATE_OVERRIDE_NOT_ALLOWED = unchecked((int)0x80041053),
            WBEM_E_UPDATE_PROPAGATED_METHOD = unchecked((int)0x80041054),
            WBEM_E_METHOD_NOT_IMPLEMENTED = unchecked((int)0x80041055),
            WBEM_E_METHOD_DISABLED = unchecked((int)0x80041056),
            WBEM_E_REFRESHER_BUSY = unchecked((int)0x80041057),
            WBEM_E_UNPARSABLE_QUERY = unchecked((int)0x80041058),
            WBEM_E_NOT_EVENT_CLASS = unchecked((int)0x80041059),
            WBEM_E_MISSING_GROUP_WITHIN = unchecked((int)0x8004105A),
            WBEM_E_MISSING_AGGREGATION_LIST = unchecked((int)0x8004105B),
            WBEM_E_PROPERTY_NOT_AN_OBJECT = unchecked((int)0x8004105C),
            WBEM_E_AGGREGATING_BY_OBJECT = unchecked((int)0x8004105D),
            WBEM_E_UNINTERPRETABLE_PROVIDER_QUERY = unchecked((int)0x8004105F),
            WBEM_E_BACKUP_RESTORE_WINMGMT_RUNNING = unchecked((int)0x80041060),
            WBEM_E_QUEUE_OVERFLOW = unchecked((int)0x80041061),
            WBEM_E_PRIVILEGE_NOT_HELD = unchecked((int)0x80041062),
            WBEM_E_INVALID_OPERATOR = unchecked((int)0x80041063),
            WBEM_E_LOCAL_CREDENTIALS = unchecked((int)0x80041064),
            WBEM_E_CANNOT_BE_ABSTRACT = unchecked((int)0x80041065),
            WBEM_E_AMENDED_OBJECT = unchecked((int)0x80041066),
            WBEM_E_CLIENT_TOO_SLOW = unchecked((int)0x80041067),
            WBEM_E_NULL_SECURITY_DESCRIPTOR = unchecked((int)0x80041068),
            WBEM_E_TIMED_OUT = unchecked((int)0x80041069),
            WBEM_E_INVALID_ASSOCIATION = unchecked((int)0x8004106A),
            WBEM_E_AMBIGUOUS_OPERATION = unchecked((int)0x8004106B),
            WBEM_E_QUOTA_VIOLATION = unchecked((int)0x8004106C),
            WBEM_E_RESERVED_001 = unchecked((int)0x8004106D),
            WBEM_E_RESERVED_002 = unchecked((int)0x8004106E),
            WBEM_E_UNSUPPORTED_LOCALE = unchecked((int)0x8004106F),
            WBEM_E_HANDLE_OUT_OF_DATE = unchecked((int)0x80041070),
            WBEM_E_CONNECTION_FAILED = unchecked((int)0x80041071),
            WBEM_E_INVALID_HANDLE_REQUEST = unchecked((int)0x80041072),
            WBEM_E_PROPERTY_NAME_TOO_WIDE = unchecked((int)0x80041073),
            WBEM_E_CLASS_NAME_TOO_WIDE = unchecked((int)0x80041074),
            WBEM_E_METHOD_NAME_TOO_WIDE = unchecked((int)0x80041075),
            WBEM_E_QUALIFIER_NAME_TOO_WIDE = unchecked((int)0x80041076),
            WBEM_E_RERUN_COMMAND = unchecked((int)0x80041077),
            WBEM_E_DATABASE_VER_MISMATCH = unchecked((int)0x80041078),
            WBEM_E_VETO_DELETE = unchecked((int)0x80041079),
            WBEM_E_VETO_PUT = unchecked((int)0x8004107A),
            WBEM_E_INVALID_LOCALE = unchecked((int)0x80041080),
            WBEM_E_PROVIDER_SUSPENDED = unchecked((int)0x80041081),
            WBEM_E_SYNCHRONIZATION_REQUIRED = unchecked((int)0x80041082),
            WBEM_E_NO_SCHEMA = unchecked((int)0x80041083),
            WBEM_E_PROVIDER_ALREADY_REGISTERED = unchecked((int)0x80041084),
            WBEM_E_PROVIDER_NOT_REGISTERED = unchecked((int)0x80041085),
            WBEM_E_FATAL_TRANSPORT_ERROR = unchecked((int)0x80041086),
            WBEM_E_ENCRYPTED_CONNECTION_REQUIRED = unchecked((int)0x80041087),
            WBEM_E_PROVIDER_TIMED_OUT = unchecked((int)0x80041088),
            WBEM_E_NO_KEY = unchecked((int)0x80041089),
            WBEMESS_E_REGISTRATION_TOO_BROAD = unchecked((int)0x80042001),
            WBEMESS_E_REGISTRATION_TOO_PRECISE = unchecked((int)0x80042002),
            WBEMMOF_E_EXPECTED_QUALIFIER_NAME = unchecked((int)0x80044001),
            WBEMMOF_E_EXPECTED_SEMI = unchecked((int)0x80044002),
            WBEMMOF_E_EXPECTED_OPEN_BRACE = unchecked((int)0x80044003),
            WBEMMOF_E_EXPECTED_CLOSE_BRACE = unchecked((int)0x80044004),
            WBEMMOF_E_EXPECTED_CLOSE_BRACKET = unchecked((int)0x80044005),
            WBEMMOF_E_EXPECTED_CLOSE_PAREN = unchecked((int)0x80044006),
            WBEMMOF_E_ILLEGAL_CONSTANT_VALUE = unchecked((int)0x80044007),
            WBEMMOF_E_EXPECTED_TYPE_IDENTIFIER = unchecked((int)0x80044008),
            WBEMMOF_E_EXPECTED_OPEN_PAREN = unchecked((int)0x80044009),
            WBEMMOF_E_UNRECOGNIZED_TOKEN = unchecked((int)0x8004400A),
            WBEMMOF_E_UNRECOGNIZED_TYPE = unchecked((int)0x8004400B),
            WBEMMOF_E_EXPECTED_PROPERTY_NAME = unchecked((int)0x8004400C),
            WBEMMOF_E_TYPEDEF_NOT_SUPPORTED = unchecked((int)0x8004400D),
            WBEMMOF_E_UNEXPECTED_ALIAS = unchecked((int)0x8004400E),
            WBEMMOF_E_UNEXPECTED_ARRAY_INIT = unchecked((int)0x8004400F),
            WBEMMOF_E_INVALID_AMENDMENT_SYNTAX = unchecked((int)0x80044010),
            WBEMMOF_E_INVALID_DUPLICATE_AMENDMENT = unchecked((int)0x80044011),
            WBEMMOF_E_INVALID_PRAGMA = unchecked((int)0x80044012),
            WBEMMOF_E_INVALID_NAMESPACE_SYNTAX = unchecked((int)0x80044013),
            WBEMMOF_E_EXPECTED_CLASS_NAME = unchecked((int)0x80044014),
            WBEMMOF_E_TYPE_MISMATCH = unchecked((int)0x80044015),
            WBEMMOF_E_EXPECTED_ALIAS_NAME = unchecked((int)0x80044016),
            WBEMMOF_E_INVALID_CLASS_DECLARATION = unchecked((int)0x80044017),
            WBEMMOF_E_INVALID_INSTANCE_DECLARATION = unchecked((int)0x80044018),
            WBEMMOF_E_EXPECTED_DOLLAR = unchecked((int)0x80044019),
            WBEMMOF_E_CIMTYPE_QUALIFIER = unchecked((int)0x8004401A),
            WBEMMOF_E_DUPLICATE_PROPERTY = unchecked((int)0x8004401B),
            WBEMMOF_E_INVALID_NAMESPACE_SPECIFICATION = unchecked((int)0x8004401C),
            WBEMMOF_E_OUT_OF_RANGE = unchecked((int)0x8004401D),
            WBEMMOF_E_INVALID_FILE = unchecked((int)0x8004401E),
            WBEMMOF_E_ALIASES_IN_EMBEDDED = unchecked((int)0x8004401F),
            WBEMMOF_E_NULL_ARRAY_ELEM = unchecked((int)0x80044020),
            WBEMMOF_E_DUPLICATE_QUALIFIER = unchecked((int)0x80044021),
            WBEMMOF_E_EXPECTED_FLAVOR_TYPE = unchecked((int)0x80044022),
            WBEMMOF_E_INCOMPATIBLE_FLAVOR_TYPES = unchecked((int)0x80044023),
            WBEMMOF_E_MULTIPLE_ALIASES = unchecked((int)0x80044024),
            WBEMMOF_E_INCOMPATIBLE_FLAVOR_TYPES2 = unchecked((int)0x80044025),
            WBEMMOF_E_NO_ARRAYS_RETURNED = unchecked((int)0x80044026),
            WBEMMOF_E_MUST_BE_IN_OR_OUT = unchecked((int)0x80044027),
            WBEMMOF_E_INVALID_FLAGS_SYNTAX = unchecked((int)0x80044028),
            WBEMMOF_E_EXPECTED_BRACE_OR_BAD_TYPE = unchecked((int)0x80044029),
            WBEMMOF_E_UNSUPPORTED_CIMV22_QUAL_VALUE = unchecked((int)0x8004402A),
            WBEMMOF_E_UNSUPPORTED_CIMV22_DATA_TYPE = unchecked((int)0x8004402B),
            WBEMMOF_E_INVALID_DELETEINSTANCE_SYNTAX = unchecked((int)0x8004402C),
            WBEMMOF_E_INVALID_QUALIFIER_SYNTAX = unchecked((int)0x8004402D),
            WBEMMOF_E_QUALIFIER_USED_OUTSIDE_SCOPE = unchecked((int)0x8004402E),
            WBEMMOF_E_ERROR_CREATING_TEMP_FILE = unchecked((int)0x8004402F),
            WBEMMOF_E_ERROR_INVALID_INCLUDE_FILE = unchecked((int)0x80044030),
            WBEMMOF_E_INVALID_DELETECLASS_SYNTAX = unchecked((int)0x80044031),
        }

        public enum CIMTYPE : int
        {
            CIM_ILLEGAL = 4095,    // 0xFFF
            CIM_EMPTY = 0,    // 0x0
            CIM_SINT8 = 16,    // 0x10
            CIM_UINT8 = 17,    // 0x11
            CIM_SINT16 = 2,    // 0x2
            CIM_UINT16 = 18,    // 0x12
            CIM_SINT32 = 3,    // 0x3
            CIM_UINT32 = 19,    // 0x13
            CIM_SINT64 = 20,    // 0x14
            CIM_UINT64 = 21,    // 0x15
            CIM_REAL32 = 4,    // 0x4
            CIM_REAL64 = 5,    // 0x5
            CIM_BOOLEAN = 11,    // 0xB
            CIM_STRING = 8,    // 0x8
            CIM_DATETIME = 101,    // 0x65
            CIM_REFERENCE = 102,    // 0x66
            CIM_CHAR16 = 103,    // 0x67
            CIM_OBJECT = 13,    // 0xD
            CIM_FLAG_ARRAY = 8192    // 0x2000
        }
        internal enum tag_WBEM_STATUS_TYPE
        {
            WBEM_STATUS_COMPLETE = unchecked((int)0x00000000),
            WBEM_STATUS_REQUIREMENTS = unchecked((int)0x00000001),
            WBEM_STATUS_PROGRESS = unchecked((int)0x00000002),
        }

        internal enum tag_WBEM_EXTRA_RETURN_CODES
        {
            WBEM_S_INITIALIZED = unchecked((int)0x00000000),
            WBEM_S_LIMITED_SERVICE = unchecked((int)0x00043001),
            WBEM_S_INDIRECTLY_UPDATED = unchecked((int)0x00043002),
            WBEM_S_SUBJECT_TO_SDS = unchecked((int)0x00043003),
            WBEM_E_RETRY_LATER = unchecked((int)0x80043001),
            WBEM_E_RESOURCE_CONTENTION = unchecked((int)0x80043002),
        }

        [ComImport, Guid("4CFC7932-0F9D-4BEF-9C32-8EA2A6B56FCB")]
        internal class WbemDecoupledRegistrar
        {
        }

        [ComImport,
         GuidAttribute("1BE41572-91DD-11D1-AEB2-00C04FB68820"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemProviderInit
        {
            [PreserveSig]
            int Initialize(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszUser,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszNamespace,
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszLocale,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemServices pNamespace,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemProviderInitSink pInitSink
                );
        }

        [ComImport,
         GuidAttribute("1005CBCF-E64F-4646-BCD3-3A089D8A84B4"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemDecoupledRegistrar
        {
            [PreserveSig]
            int Register(
                [In] Int32 flags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext context,
                [In][MarshalAs(UnmanagedType.LPWStr)] string user,
                [In][MarshalAs(UnmanagedType.LPWStr)] string locale,
                [In][MarshalAs(UnmanagedType.LPWStr)] string scope,
                [In][MarshalAs(UnmanagedType.LPWStr)] string registration,
                [In][MarshalAs(UnmanagedType.IUnknown)] object unknown
                );

            [PreserveSig]
            int UnRegister();
        }

        [ComImport,
         GuidAttribute("9556DC99-828C-11CF-A37E-00AA003240C7"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemServices
        {
            [PreserveSig]
            int OpenNamespace(
                [In][MarshalAs(UnmanagedType.BStr)] string strNamespace,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][Out][MarshalAs(UnmanagedType.Interface)] ref IWbemServices ppWorkingNamespace,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int CancelAsyncCall(
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink
                );

            [PreserveSig]
            int QueryObjectSink(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemObjectSink ppResponseHandler
                );

            [PreserveSig]
            int GetObject(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][Out][MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject ppObject,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int GetObjectAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int PutClass(
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pObject,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int PutClassAsync(
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pObject,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int DeleteClass(
                [In][MarshalAs(UnmanagedType.BStr)] string strClass,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int DeleteClassAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strClass,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int CreateClassEnum(
                [In][MarshalAs(UnmanagedType.BStr)] string strSuperclass,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [Out][MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum
                );

            [PreserveSig]
            int CreateClassEnumAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strSuperclass,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int PutInstance(
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pInst,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int PutInstanceAsync(
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pInst,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int DeleteInstance(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int DeleteInstanceAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int CreateInstanceEnum(
                [In][MarshalAs(UnmanagedType.BStr)] string strFilter,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [Out][MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum
                );

            [PreserveSig]
            int CreateInstanceEnumAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strFilter,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int ExecQuery(
                [In][MarshalAs(UnmanagedType.BStr)] string strQueryLanguage,
                [In][MarshalAs(UnmanagedType.BStr)] string strQuery,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [Out][MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum
                );

            [PreserveSig]
            int ExecQueryAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strQueryLanguage,
                [In][MarshalAs(UnmanagedType.BStr)] string strQuery,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int ExecNotificationQuery(
                [In][MarshalAs(UnmanagedType.BStr)] string strQueryLanguage,
                [In][MarshalAs(UnmanagedType.BStr)] string strQuery,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [Out][MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum
                );

            [PreserveSig]
            int ExecNotificationQueryAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strQueryLanguage,
                [In][MarshalAs(UnmanagedType.BStr)] string strQuery,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );

            [PreserveSig]
            int ExecMethod(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In][MarshalAs(UnmanagedType.BStr)] string strMethodName,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pInParams,
                [In][Out][MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject ppOutParams,
                [In] IntPtr ppCallResult
                );

            [PreserveSig]
            int ExecMethodAsync(
                [In][MarshalAs(UnmanagedType.BStr)] string strObjectPath,
                [In][MarshalAs(UnmanagedType.BStr)] string strMethodName,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pInParams,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler
                );
        }

        [ComImport,
         GuidAttribute("DC12A681-737F-11CF-884D-00AA004B2E24"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemClassObject
        {
            [PreserveSig]
            int GetQualifierSet(
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet ppQualSet
                );

            [PreserveSig]
            int Get(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In][Out] ref object pVal,
                [In][Out] ref Int32 pType,
                [In][Out] ref Int32 plFlavor
                );

            [PreserveSig]
            int Put(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In] ref object pVal,
                [In] Int32 Type
                );

            [PreserveSig]
            int Delete(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName
                );

            [PreserveSig]
            int GetNames(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszQualifierName,
                [In] Int32 lFlags,
                [In] ref object pQualifierVal,
                [Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pNames
                );

            [PreserveSig]
            int BeginEnumeration(
                [In] Int32 lEnumFlags
                );

            [PreserveSig]
            int Next(
                [In] Int32 lFlags,
                [In][Out][MarshalAs(UnmanagedType.BStr)] ref string strName,
                [In][Out] ref object pVal,
                [In][Out] ref Int32 pType,
                [In][Out] ref Int32 plFlavor
                );

            [PreserveSig]
            int EndEnumeration();

            [PreserveSig]
            int GetPropertyQualifierSet(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszProperty,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet ppQualSet
                );

            [PreserveSig]
            int Clone(
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemClassObject ppCopy
                );

            [PreserveSig]
            int GetObjectText(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.BStr)] out string pstrObjectText
                );

            [PreserveSig]
            int SpawnDerivedClass(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemClassObject ppNewClass
                );

            [PreserveSig]
            int SpawnInstance(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemClassObject ppNewInstance
                );

            [PreserveSig]
            int CompareTo([In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pCompareTo
                );

            [PreserveSig]
            int GetPropertyOrigin(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [Out][MarshalAs(UnmanagedType.BStr)] out string pstrClassName
                );

            [PreserveSig]
            int InheritsFrom(
                [In][MarshalAs(UnmanagedType.LPWStr)] string strAncestor
                );

            [PreserveSig]
            int GetMethod(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In] IntPtr ppInSignature,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemClassObject ppOutSignature
                );

            [PreserveSig]
            int PutMethod(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pInSignature,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pOutSignature
                );

            [PreserveSig]
            int DeleteMethod(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName
                );

            [PreserveSig]
            int BeginMethodEnumeration(
                [In] Int32 lEnumFlags
                );

            [PreserveSig]
            int NextMethod(
                [In] Int32 lFlags,
                [In][Out][MarshalAs(UnmanagedType.BStr)] ref string pstrName,
                [In][Out][MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject ppInSignature,
                [In][Out][MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject ppOutSignature
                );

            [PreserveSig]
            int EndMethodEnumeration();

            [PreserveSig]
            int GetMethodQualifierSet(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszMethod,
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemQualifierSet ppQualSet
                );

            [PreserveSig]
            int GetMethodOrigin(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszMethodName,
                [Out][MarshalAs(UnmanagedType.BStr)] out string pstrClassName
                );
        }

        [ComImport,
         GuidAttribute("44ACA674-E8FC-11D0-A07C-00C04FB68820"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemContext
        {
            [PreserveSig]
            int Clone(
                [Out][MarshalAs(UnmanagedType.Interface)] out IWbemContext ppNewCopy
                );

            [PreserveSig]
            int GetNames(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pNames
                );

            [PreserveSig]
            int BeginEnumeration(
                [In] Int32 lFlags
                );

            [PreserveSig]
            int Next(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.BStr)] out string pstrName,
                [Out] out object pValue
                );

            [PreserveSig]
            int EndEnumeration();

            [PreserveSig]
            int SetValue(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In] ref object pValue
                );

            [PreserveSig]
            int GetValue(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [Out] out object pValue
                );

            [PreserveSig]
            int DeleteValue(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags
                );

            [PreserveSig]
            int DeleteAll();
        }

        [ComImport,
         GuidAttribute("1BE41571-91DD-11D1-AEB2-00C04FB68820"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemProviderInitSink
        {
            [PreserveSig]
            int SetStatus(
                [In] Int32 lStatus,
                [In] Int32 lFlags
                );
        }

        [ComImport,
         GuidAttribute("7C857801-7381-11CF-884D-00AA004B2E24"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemObjectSink
        {
            [PreserveSig]
            int Indicate(
                [In] Int32 lObjectCount,
                //[In][MarshalAs(UnmanagedType.Interface)]  ref 
                [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IWbemClassObject[] apObjArray
                );

            [PreserveSig]
            int SetStatus(
                [In] Int32 lFlags,
                [In][MarshalAs(UnmanagedType.Error)] Int32 hResult,
                [In][MarshalAs(UnmanagedType.BStr)] string strParam,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemClassObject pObjParam
                );
        }

        [ComImport,
         GuidAttribute("027947E1-D731-11CE-A357-000000000001"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IEnumWbemClassObject
        {
            [PreserveSig]
            int Reset();

            [PreserveSig]
            int Next(
                [In] Int32 lTimeout,
                [In] UInt32 uCount,
                [In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IWbemClassObject[] apObjects,
                [Out] out UInt32 puReturned
                );

            [PreserveSig]
            int NextAsync(
                [In] UInt32 uCount,
                [In][MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink
                );

            [PreserveSig]
            int Clone(
                [Out][MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum
                );

            [PreserveSig]
            int Skip(
                [In] Int32 lTimeout,
                [In] UInt32 nCount
                );
        }

        [ComImport,
         GuidAttribute("DC12A680-737F-11CF-884D-00AA004B2E24"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWbemQualifierSet
        {
            [PreserveSig]
            int Get(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] Int32 lFlags,
                [In][Out] ref object pVal,
                [In][Out] ref Int32 plFlavor
                );

            [PreserveSig]
            int Put(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName,
                [In] ref object pVal,
                [In] Int32 lFlavor
                );

            [PreserveSig]
            int Delete(
                [In][MarshalAs(UnmanagedType.LPWStr)] string wszName
                );

            [PreserveSig]
            int GetNames(
                [In] Int32 lFlags,
                [Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pNames
                );

            [PreserveSig]
            int BeginEnumeration(
                [In] Int32 lFlags
                );

            [PreserveSig]
            int Next(
                [In] Int32 lFlags,
                [In][Out][MarshalAs(UnmanagedType.BStr)] ref string pstrName,
                [In][Out] ref object pVal, [In][Out] ref Int32 plFlavor
                );

            [PreserveSig]
            int EndEnumeration();
        }
    }
}
